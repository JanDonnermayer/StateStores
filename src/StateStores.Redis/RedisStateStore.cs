using System;
using StateStores;
using System.Reactive;
using StackExchange.Redis;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Newtonsoft.Json;

namespace StateStores.Redis
{
    public class RedisStateStore : IStateStore
    {

        private readonly string server;
        private readonly Lazy<ConnectionMultiplexer> redis;
        private ImmutableDictionary<string, IObservable<RedisValue>> mut_ObserverDict;

        private IObservable<RedisValue> GetObservable(string channel)
        {
            IObservable<RedisValue> _GetObservable(string channel) =>
                Observable.Create<RedisValue>(o =>
                {
                    var subscriber = redis.Value.GetSubscriber();
                    Action<RedisChannel, RedisValue> handler = (c, m) => o.OnNext(m);
                    subscriber.Subscribe(channel, handler);
                    return Disposable.Create(() => subscriber.Unsubscribe(channel, handler));
                });

            return ImmutableInterlocked.GetOrAdd(
                location: ref mut_ObserverDict,
                key: channel,
                valueFactory: _GetObservable
            );
        }



        public RedisStateStore(string server)
        {
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            this.redis = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this.server));
        }

        public async Task<StateStoreResult> AddAsync<T>(string key, string token, T next)
        {

            throw new NotImplementedException();

        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, string token, T current, T next)
        {
            var db = redis.Value.GetDatabase();
            var transaction = db.CreateTransaction();

            var val = await transaction.StringGetAsync(key);
            if (!val.HasValue) return new StateStoreResult.StateError();
            var tsp = TokenStatePair<T>.FromString(val);

            if (tsp.Token != token) return new StateStoreResult.TokenError();
            if (!tsp.State.Equals(current)) return new StateStoreResult.StateError();

            var tspNew = new TokenStatePair<T>(token, next);
            await transaction.StringSetAsync(key, tspNew.ToString());

            await transaction.ExecuteAsync();
            return new StateStoreResult.Ok();
        }

        public Task<StateStoreResult> RemoveAsync<T>(string key, string token, T current) => throw new NotImplementedException();
        public IObservable<IImmutableDictionary<string, T>> GetObservable<T>() => throw new NotImplementedException();



        #region  Private Types

        private class TokenStatePair<TState>
        {
            public TokenStatePair(string token, TState state)
            {
                Token = token;
                State = state;
            }
            public string Token { get; }
            public TState State { get; }

            /// <summary>
            /// Returns a string representation of this instance of <see cref="TokenStatePair{TState}"/>
            /// </summary>
            /// <returns></returns>
            public override string ToString() =>
                JsonConvert.SerializeObject(this);

            public static TokenStatePair<TState> FromString(string str) =>
                JsonConvert.DeserializeObject<TokenStatePair<TState>>(str);

        }

        #endregion


    }
}