using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores
{
    public sealed class InMemoryStateStore : IStateStore
    {

        #region  Private Members

        private ImmutableDictionary<Type, SemaphoreSlim> mut_semaphoreMap =
            ImmutableDictionary<Type, SemaphoreSlim>.Empty;

        private ImmutableDictionary<Type, object> mut_stateMap =
            ImmutableDictionary<Type, object>.Empty;

        private readonly ReplaySubject<Type> keySubject =
            new ReplaySubject<Type>(TaskPoolScheduler.Default);

        private SemaphoreSlim GetSemaphore<TValue>() =>
            ImmutableInterlocked.GetOrAdd(
                location: ref mut_semaphoreMap,
                key: typeof(TValue),
                valueFactory: _ => new SemaphoreSlim(1, 1));

        private ImmutableDictionary<string, TValue> GetTypedStateMap<TValue>() =>
            (ImmutableDictionary<string, TValue>)ImmutableInterlocked.GetOrAdd(
                location: ref mut_stateMap,
                key: typeof(TValue),
                valueFactory: _ => ImmutableDictionary<string, TValue>.Empty);

        private async Task<IDisposable> GetLockAsync<T>()
        {
            var sem = GetSemaphore<T>();
            await sem.WaitAsync();
            return Disposable.Create(() => sem.Release());
        }

        #endregion


        #region  Implementation of IStateStore

        public IObservable<IEnumerable<KeyValuePair<string, T>>> GetObservable<T>() =>
            keySubject
                .Where(k => k is T)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .Select(_ =>
                    GetTypedStateMap<TokenStatePair<T>>()
                    .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.State));

        public async Task<StateStoreResult> EnterAsync<T>(string key, string token, T state)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetTypedStateMap<TokenStatePair<T>>();

            if (map.ContainsKey(key)) return new StateStoreResult.StateError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.SetItem(key, new TokenStatePair<T>(token, state))));

            keySubject.OnNext(typeof(T));

            return new StateStoreResult.Ok();
        }

        public async Task<StateStoreResult> TransferAsync<T>(string key, string token, T state1, T state2)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetTypedStateMap<TokenStatePair<T>>();

            if (!map.TryGetValue(key, out var tsp)) return new StateStoreResult.StateError();
            if (!tsp.State.Equals(state1)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.SetItem(key, new TokenStatePair<T>(token, state2))));

            keySubject.OnNext(typeof(T));

            return new StateStoreResult.Ok();
        }


        public async Task<StateStoreResult> ExitAsync<T>(string key, string token)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetTypedStateMap<TokenStatePair<T>>();

            if (!map.TryGetValue(key, out var tsp)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.Remove(key)));

            keySubject.OnNext(typeof(T));

            return new StateStoreResult.Ok();
        }


        #endregion


        #region  Private Types

        private readonly struct TokenStatePair<TState>
        {
            public TokenStatePair(string token, TState state)
            {
                Token = token;
                State = state;
            }
            public string Token { get; }
            public TState State { get; }
        }

        #endregion
    }

}
