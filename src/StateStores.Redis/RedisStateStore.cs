using System;
using StateStores;
using System.Reactive;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Reactive.Concurrency;
using System.Collections.Generic;

namespace StateStores.Redis
{
    public class RedisStateStore : IStateStore
    {

        #region Private Members

        private readonly string server;

        private readonly Lazy<ConnectionMultiplexer> redis;

        private ImmutableDictionary<string, IObservable<RedisValue>> mut_ObserverDict;


        private IDatabase GetDatabase() =>
            redis.Value.GetDatabase();

        private ISubscriber GetSubscriber() =>
            redis.Value.GetSubscriber();

        private static string GetChannelName<TState>() =>
            typeof(TState).FullName;

        private static string GetKeysSetName<TState>() =>
            typeof(TState).FullName;

        private static Task AddKeyToSetAsync<TState>(IDatabaseAsync database, string key) =>
            database.SetAddAsync(GetKeysSetName<TState>(), key);

        private static Task RemoveKeyFromSetAsync<TState>(IDatabaseAsync database, string key) =>
            database.SetRemoveAsync(GetKeysSetName<TState>(), key);

        private Task NotifyObserversAsync<TState>() =>
            GetSubscriber().PublishAsync(GetChannelName<TState>(), RedisValue.EmptyString);


        private IObservable<RedisValue> GetObservable(string channel)
        {
            IObservable<RedisValue> _GetObservable(string channel) =>
                Observable.Create<RedisValue>(o =>
                {
                    var subscriber = redis.Value.GetSubscriber();
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


        #endregion


        #region  Constructor

        public RedisStateStore(string server)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.redis = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this.server));
        }

        #endregion


        #region  Internal

        static async Task<StateStoreResult> AddInternalAsync<T>(IDatabase database, string key, T next)
        {
            var transaction = database.CreateTransaction();

            if (!await transaction.StringSetAsync(key, StateToRedisValue(next))) return new StateStoreResult.Error();
            await AddKeyToSetAsync<T>(database, key);

            return new StateStoreResult.Ok();
        }

        static async Task<StateStoreResult> UpdateInternalAsync<T>(IDatabase database, string key, T current, T next)
        {
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(key, StateToRedisValue(current)));
            // ...

            if (!await transaction.StringSetAsync(key, StateToRedisValue(next))) return new StateStoreResult.Error();
            
            return new StateStoreResult.Ok();
        }

        static async Task<StateStoreResult> RemoveInternalAsync<T>(IDatabase database, string key, T current)
        {
            var transaction = database.CreateTransaction();
            transaction.AddCondition(Condition.StringEqual(key, StateToRedisValue(current)));

            if (!await transaction.KeyDeleteAsync(key)) return new StateStoreResult.Error();
            await RemoveKeyFromSetAsync<T>(database, key);

            return new StateStoreResult.Ok();
        }


        #endregion


        #region  Implementation of IStateStore

        public async Task<StateStoreResult> AddAsync<T>(string key, T next)
        {
            var res = await AddInternalAsync(GetDatabase(), key, next);
            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, T current, T next)
        {
            var res = await UpdateInternalAsync(GetDatabase(), key, current, next);
            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> RemoveAsync<T>(string key, T current)
        {
            var res = await RemoveInternalAsync(GetDatabase(), key, current);
            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public IObservable<IImmutableDictionary<string, T>> GetObservable<T>()
        {
            var db = GetDatabase();

            return GetObservable(GetChannelName<T>()).Select(_ =>
                db.SetMembers(GetKeysSetName<T>()).ToImmutableDictionary(m =>
                    m.ToString(), m => RedisValueToState<T>(db.StringGet(m.ToString()))));
        }

        #endregion


        private static RedisValue StateToRedisValue<T>(T value) =>
            JsonConvert.SerializeObject(value);

        private static T RedisValueToState<T>(RedisValue value) =>
            JsonConvert.DeserializeObject<T>(value);
    }
}