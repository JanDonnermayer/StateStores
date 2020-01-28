using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using static StateStores.StateStoreResult;


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
            (ImmutableDictionary<string, TValue>)ImmutableInterlocked
                .GetOrAdd(
                    location: ref mut_stateMapMap,
                    key: typeof(TValue),
                    valueFactory: _ => ImmutableDictionary<string, TValue>.Empty
                );

        private void PushStateMap<TValue>(
            Func<ImmutableDictionary<string, TValue>, ImmutableDictionary<string, TValue>> updateValue) =>
                ImmutableInterlocked.AddOrUpdate(
                    location: ref mut_stateMapMap,
                    key: typeof(TValue),
                    addValueFactory: _ => updateValue(ImmutableDictionary<string, TValue>.Empty),
                    updateValueFactory: (_, map) => updateValue((ImmutableDictionary<string, TValue>)map)
            );

        private async Task<IDisposable> GetLockAsync<T>()
        {
            var sem = GetSemaphore<T>();
            await sem.WaitAsync().ConfigureAwait(false);
            return Disposable.Create(() => sem.Release());
        }

        private Task NotifyObserversAsync<T>() =>
           Task.Run(() => GetSubject<T>().OnNext(Unit.Default));

        #endregion

        #region  Implementation of IStateStore

        public async Task<StateStoreResult> AddAsync<T>(string key, T nextState)
        {
            using var @lock = await GetLockAsync<T>().ConfigureAwait(false);

            var map = GetStateMap<T>();

            if (map.ContainsKey(key)) return new Error();

            PushStateMap<T>(m => m.SetItem(key, nextState));

            _ = NotifyObserversAsync<T>();

            return new Ok();
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, T currentState, T nextState)
        {
            using var @lock = await GetLockAsync<T>().ConfigureAwait(false);

            var map = GetStateMap<T>();

            if (!map.TryGetValue(key, out var val)) return new Error();
            if (!val.Equals(currentState)) return new Error();

            PushStateMap<T>(m => m.SetItem(key, nextState));

            _ = NotifyObserversAsync<T>();

            return new Ok();
        }

        public async Task<StateStoreResult> RemoveAsync<T>(string key, T currentState)
        {
            using var @lock = await GetLockAsync<T>().ConfigureAwait(false);

            var map = GetStateMap<T>();

            if (!map.TryGetValue(key, out var val)) return new Error();
            if (!val.Equals(currentState)) return new Error();

            PushStateMap<T>(m => m.Remove(key));

            _ = NotifyObserversAsync<T>();

            return new Ok();
        }

        public IObservable<IEnumerable<ImmutableDictionary<string, T>>> GetObservable<T>() =>
            GetSubject<T>().Select(_ => GetStateMap<T>())
                 // Start with empty set so states appear added for new subscribers
                .Merge(Observable.Return(ImmutableDictionary<string, T>.Empty))
                .Merge(Observable.Return(GetStateMap<T>()))
                .Buffer(2, 1)
                .Replay(1)
                .RefCount();

        #endregion

    }

}
