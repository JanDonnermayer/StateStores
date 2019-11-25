using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
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

        private ImmutableDictionary<Type, object> mut_stateMapMap =
            ImmutableDictionary<Type, object>.Empty;

        private ImmutableDictionary<Type, BehaviorSubject<Unit>> mut_subjectMap =
            ImmutableDictionary<Type, BehaviorSubject<Unit>>.Empty;

        private BehaviorSubject<Unit> GetSubject<TValue>() =>
            ImmutableInterlocked.GetOrAdd(
                location: ref mut_subjectMap,
                key: typeof(TValue),
                valueFactory: _ => new BehaviorSubject<Unit>(Unit.Default));

        private SemaphoreSlim GetSemaphore<TValue>() =>
            ImmutableInterlocked.GetOrAdd(
                location: ref mut_semaphoreMap,
                key: typeof(TValue),
                valueFactory: _ => new SemaphoreSlim(1, 1));

        private ImmutableDictionary<string, TValue> GetStateMap<TValue>() =>
            (ImmutableDictionary<string, TValue>)ImmutableInterlocked.GetOrAdd(
                location: ref mut_stateMapMap,
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

        public async Task<StateStoreResult> AddAsync<T>(string key, string token, T nextState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<TokenStatePair<T>>();

            if (map.ContainsKey(key)) return new StateStoreResult.StateError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.SetItem(key, new TokenStatePair<T>(token, nextState))));

            GetSubject<T>().OnNext(Unit.Default);

            return new StateStoreResult.Ok();
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, string token, T currentState, T nextState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<TokenStatePair<T>>();

            if (!map.TryGetValue(key, out var tsp)) return new StateStoreResult.StateError();
            if (!tsp.State.Equals(currentState)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.SetItem(key, new TokenStatePair<T>(token, nextState))));

            GetSubject<T>().OnNext(Unit.Default);

            return new StateStoreResult.Ok();
        }


        public async Task<StateStoreResult> RemoveAsync<T>(string key, string token, T currentState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<TokenStatePair<T>>();

            if (!map.TryGetValue(key, out var tsp)) return new StateStoreResult.StateError();
            if (!tsp.State.Equals(currentState)) return new StateStoreResult.StateError();
            if (tsp.Token != token) return new StateStoreResult.TokenError();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(typeof(TokenStatePair<T>), map.Remove(key)));

            GetSubject<T>().OnNext(Unit.Default);

            return new StateStoreResult.Ok();
        }

        public IObservable<IImmutableDictionary<string, T>> GetObservable<T>() =>
            GetSubject<T>()
                .Select(_ =>
                    GetStateMap<TokenStatePair<T>>()
                    .ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.State));


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
