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

        static async Task<StateStoreResult> AddInternalAsync<T>(IDatabaseAsync database, string key, string token, T next)
        {
            if (await database.KeyExistsAsync(key)) return new StateStoreResult.StateError();

            await database.StringSetAsync(key, TokenStatePair.Create(token, next).ToRedisValue());
            await AddKeyToSetAsync<T>(database, key);

            return new StateStoreResult.Ok();
        }

        static async Task<StateStoreResult> UpdateInternalAsync<T>(IDatabaseAsync database, string key, string token, T current, T next)
        {

            var val = await database.StringGetAsync(key);
            if (!val.HasValue) return new StateStoreResult.StateError();
            var tsp = TokenStatePair<T>.FromRedisValue(val);

            if (!tsp.State.Equals(current)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            await database.StringSetAsync(key, TokenStatePair.Create(token, next).ToRedisValue());

            return new StateStoreResult.Ok();
        }

        static async Task<StateStoreResult> RemoveInternalAsync<T>(IDatabaseAsync database, string key, string token, T current)
        {
            var val = await database.StringGetAsync(key);
            if (!val.HasValue) return new StateStoreResult.StateError();
            var tsp = TokenStatePair<T>.FromRedisValue(val);

            if (!tsp.State.Equals(current)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            await database.KeyDeleteAsync(key);
            await RemoveKeyFromSetAsync<T>(database, key);

            return new StateStoreResult.Ok();
        }

        private async Task<StateStoreResult> UseLockAsync(string key, Func<IDatabaseAsync, Task<StateStoreResult>> @function)
        {
            var database = GetDatabase();

            var lockToken = Guid.NewGuid().ToString();

            if (!await database.LockTakeAsync(key, lockToken, TimeSpan.FromSeconds(3)))
                return new StateStoreResult.LockError();
            try
            {
                return await @function(database);
            }
            finally
            {
                await database.LockReleaseAsync(key, lockToken);
            }
        }


        #endregion


        #region  Implementation of IStateStore

        public async Task<StateStoreResult> AddAsync<T>(string key, string token, T next)
        {
            var res = await UseLockAsync(key, db => AddInternalAsync(db, key, token, next));

            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, string token, T current, T next)
        {
            var res = await UseLockAsync(key, db => UpdateInternalAsync(db, key, token, current, next));
            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public async Task<StateStoreResult> RemoveAsync<T>(string key, string token, T current)
        {
            var res = await UseLockAsync(key, db => RemoveInternalAsync(db, key, token, current));
            if (res is StateStoreResult.Ok) await NotifyObserversAsync<T>();
            return res;
        }

        public IObservable<IImmutableDictionary<string, T>> GetObservable<T>()
        {
            var db = GetDatabase();

            return GetObservable(GetChannelName<T>()).Select(_ =>
                db.SetMembers(GetKeysSetName<T>()).ToImmutableDictionary(m =>
                    m.ToString(), m => TokenStatePair<T>.FromRedisValue(db.StringGet(m.ToString())).State));
        }

        #endregion


        #region  Private Types

        private static class TokenStatePair
        {
            public static TokenStatePair<T> Create<T>(string token, T state) =>
                new TokenStatePair<T>(token, state);
        }

        private class TokenStatePair<TState>
        {
            public TokenStatePair(string token, TState state)
            {
                Token = token;
                State = state;
            }
            public string Token { get; }
            public TState State { get; }

            public RedisValue ToRedisValue() =>
                JsonConvert.SerializeObject(this);

            public static TokenStatePair<TState> FromRedisValue(RedisValue value)
            {
                var val = value.ToString();
                return JsonConvert.DeserializeObject<TokenStatePair<TState>>(val);
            }
        }

        #endregion


    }
}