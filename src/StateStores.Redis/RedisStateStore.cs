using System;
using StateStores;
using System.Reactive;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

using static StateStores.StateStoreResult;

namespace StateStores.Redis
{
    public sealed class RedisStateStore : IStateStore, IDisposable
    {

        #region Private Members

        private readonly string server;

        private readonly Lazy<ConnectionMultiplexer> lazy_redis;

        private readonly Lazy<IDatabase> lazy_database;

        private readonly Lazy<ISubscriber> lazy_subscriber;

        private ImmutableDictionary<string, IObservable<RedisValue>> mut_ObserverDict =
            ImmutableDictionary<string, IObservable<RedisValue>>.Empty;

        private IDatabase GetDatabase() =>
            lazy_database.Value;

        private ISubscriber GetSubscriber() =>
            lazy_subscriber.Value;

        private static string GetChannelName<TState>() =>
           "udpdate_channel_" + typeof(TState).FullName;

        private static string GetHashName<TState>() =>
           $"set_{typeof(TState).FullName}";

        private async Task NotifyObserversAsync<TState>() =>
            await GetSubscriber()
                .PublishAsync(
                    channel: GetChannelName<TState>(),
                    message: RedisValue.EmptyString
                )
                .ConfigureAwait(false);

        private IObservable<RedisValue> GetObservable(string channel)
        {
            IObservable<RedisValue> _GetObservable(string channel) =>
                Observable.Create<RedisValue>(o =>
                {
                    var subscriber = lazy_redis.Value.GetSubscriber();
                    void handler(RedisChannel c, RedisValue m) => o.OnNext(m);
                    subscriber.Subscribe(channel, handler);
                    return Disposable.Create(() => subscriber.Unsubscribe(channel, handler));
                });

            return ImmutableInterlocked.GetOrAdd(
                location: ref mut_ObserverDict,
                key: channel,
                valueFactory: _GetObservable
            );
        }

        static ImmutableDictionary<string, T> DictionaryFromValues<T>(HashEntry[] entries) =>
            entries
                .Select(_ => new KeyValuePair<string, T>(_.Name, FromRedisValue<T>(_.Value)))
                .ToImmutableDictionary();

        private static RedisValue ToRedisValue<T>(T value) =>
            JsonConvert.SerializeObject(value);

        private static T FromRedisValue<T>(RedisValue value) =>
            value.IsNullOrEmpty switch
            {
                false => JsonConvert.DeserializeObject<T>(value),
                true => default
            };

        #endregion

        #region  Constructor

        public RedisStateStore(string server)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.lazy_redis = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this.server));
            this.lazy_subscriber = new Lazy<ISubscriber>(() => lazy_redis.Value.GetSubscriber());
            this.lazy_database = new Lazy<IDatabase>(() => lazy_redis.Value.GetDatabase());
        }

        #endregion

        #region  Internal

        private static async Task<StateStoreResult> AddInternalAsync<T>(IDatabase database, string key, T next)
        {
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.HashNotExists(GetHashName<T>(), key));

            _ = transaction.HashSetAsync(GetHashName<T>(),
                new HashEntry[] { new KeyValuePair<RedisValue, RedisValue>(key, ToRedisValue(next)) });

            if (!await transaction.ExecuteAsync().ConfigureAwait(false)) return new StateError();

            return new Ok();
        }

        private static async Task<StateStoreResult> UpdateInternalAsync<T>(IDatabase database, string key, T current, T next)
        {
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.HashEqual(GetHashName<T>(), key, ToRedisValue(current)));

            _ = transaction.HashSetAsync(GetHashName<T>(),
                new HashEntry[] { new KeyValuePair<RedisValue, RedisValue>(key, ToRedisValue(next)) });

            if (!await transaction.ExecuteAsync().ConfigureAwait(false)) return new StateError();

            return new Ok();
        }

        private static async Task<StateStoreResult> RemoveInternalAsync<T>(IDatabase database, string key, T current)
        {
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.HashEqual(GetHashName<T>(), key, ToRedisValue(current)));

            _ = transaction.HashDeleteAsync(GetHashName<T>(), key);

            if (!await transaction.ExecuteAsync().ConfigureAwait(false)) return new StateError();

            return new Ok();
        }

        #endregion

        #region  Implementation of IStateStore

        public async Task<StateStoreResult> AddAsync<T>(string key, T next)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var method = new Func<Task<StateStoreResult>>(() =>
                AddInternalAsync(GetDatabase(), key, next));
            var res = await method
                .Catch<Exception>(_ => new ConnectionError())
                .RetryIncrementallyOn<ConnectionError>(
                    baseDelayMs: 100,
                    retryCount: 5)
                .Invoke()
                .ConfigureAwait(false);

            if (res is Ok) _ = NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, T current, T next)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var method = new Func<Task<StateStoreResult>>(() =>
                UpdateInternalAsync(GetDatabase(), key, current, next));
            var res = await method
                .Catch<Exception>(_ => new ConnectionError())
                .RetryIncrementallyOn<ConnectionError>(
                    baseDelayMs: 100,
                    retryCount: 5)
                .Invoke()
                .ConfigureAwait(false);

            if (res is Ok) _ = NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> RemoveAsync<T>(string key, T current)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            var method = new Func<Task<StateStoreResult>>(() =>
                RemoveInternalAsync(GetDatabase(), key, current));
            var res = await method
                .Catch<Exception>(_ => new ConnectionError())
                .RetryIncrementallyOn<ConnectionError>(
                    baseDelayMs: 100,
                    retryCount: 5)
                .Invoke()
                .ConfigureAwait(false);

            if (res is Ok) _ = NotifyObserversAsync<T>();
            return res;
        }

        public IObservable<IEnumerable<ImmutableDictionary<string, T>>> GetObservable<T>() =>
            GetObservable(GetChannelName<T>())
                .Select(_ => Observable.FromAsync(
                        async () => DictionaryFromValues<T>(
                            await GetDatabase()
                                .HashGetAllAsync(GetHashName<T>())
                                .ConfigureAwait(false)
                        )
                    )
                )
                .Concat()
                // Start with empty set so states appear added for new subscribers
                .Merge(Observable.Return(ImmutableDictionary<string, T>.Empty))
                // Pass initial set
                .Merge(Observable.FromAsync(
                        async () => DictionaryFromValues<T>(
                            await GetDatabase()
                                .HashGetAllAsync(GetHashName<T>())
                                .ConfigureAwait(false)
                        )
                    )
                )
                .Buffer(2, 1)
                .Replay(1)
                .RefCount();

        #endregion

        #region  IDisposable

        public void Dispose()
        {
            if (lazy_redis.IsValueCreated)
                lazy_redis.Value.Dispose();
        }

        #endregion
    }
}