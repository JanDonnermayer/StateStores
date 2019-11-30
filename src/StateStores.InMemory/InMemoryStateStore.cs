using System;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores.InMemory
{
    public sealed class InMemoryStateStore : IStateStore
    {

        #region Private Members

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
            await sem.WaitAsync().ConfigureAwait(false);
            return Disposable.Create(() => sem.Release());
        }

        private void NotifyObservers<T>() =>
            GetSubject<T>().OnNext(Unit.Default);

        #endregion


        #region  Implementation of IStateStore

        public async Task<StateStoreResult> AddAsync<T>(string key, T nextState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<T>();

            if (map.ContainsKey(key)) return new StateStoreResult.Error();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(
                    key: typeof(T),
                    value: map.SetItem(key, nextState)));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, T currentState, T nextState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<T>();

            if (!map.TryGetValue(key, out var val)) return new StateStoreResult.Error();
            if (!val.Equals(currentState)) return new StateStoreResult.Error();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(
                    key: typeof(T),
                    value: map.SetItem(key, nextState)));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }


        public async Task<StateStoreResult> RemoveAsync<T>(string key, T currentState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateMap<T>();

            if (!map.TryGetValue(key, out var val)) return new StateStoreResult.Error();
            if (!val.Equals(currentState)) return new StateStoreResult.Error();

            ImmutableInterlocked.Update(
                location: ref mut_stateMapMap,
                transformer: m => m.SetItem(
                    key: typeof(T),
                    value: map.Remove(key)));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }

        public IObservable<IImmutableDictionary<string, T>> GetObservable<T>() =>
            GetSubject<T>().Select(_ => GetStateMap<T>());

        #endregion

    }

}
