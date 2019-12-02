using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

        private ImmutableQueue<ImmutableDictionary<string, TValue>> CreateQueue<TValue>() =>
            ImmutableQueue.Create(
                ImmutableDictionary<string, TValue>.Empty,
                ImmutableDictionary<string, TValue>.Empty
            );

        private static ImmutableQueue<ImmutableDictionary<string, TValue>> CastQueue<TValue>(object queue) =>
            (ImmutableQueue<ImmutableDictionary<string, TValue>>)queue;

        private ImmutableQueue<ImmutableDictionary<string, TValue>> GetStateQueue<TValue>() =>
            CastQueue<TValue>(ImmutableInterlocked
                .GetOrAdd(
                    location: ref mut_stateMapMap,
                    key: typeof(TValue),
                    valueFactory: _ => CreateQueue<TValue>()));
        private void PushStateMap<TValue>(
            Func<ImmutableDictionary<string, TValue>, ImmutableDictionary<string, TValue>> updateValue) =>
                ImmutableInterlocked.AddOrUpdate(
                    location: ref mut_stateMapMap,
                    key: typeof(TValue),
                    addValueFactory: _ => CreateQueue<TValue>()
                        .Dequeue()
                        .Enqueue(updateValue(ImmutableDictionary<string, TValue>.Empty)),
                    updateValueFactory: (_, _queue) =>
                    {
                        var queue = CastQueue<TValue>(_queue);
                        return queue
                            .Dequeue()
                            .Enqueue(updateValue(queue.Peek()));
                    });


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

            var map = GetStateQueue<T>().Last();

            if (map.ContainsKey(key)) return new StateStoreResult.Error();

            PushStateMap<T>(m => m.SetItem(key, nextState));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }

        public async Task<StateStoreResult> UpdateAsync<T>(string key, T currentState, T nextState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateQueue<T>().Last();

            if (!map.TryGetValue(key, out var val)) return new StateStoreResult.Error();
            if (!val.Equals(currentState)) return new StateStoreResult.Error();

            PushStateMap<T>(m => m.SetItem(key, nextState));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }


        public async Task<StateStoreResult> RemoveAsync<T>(string key, T currentState)
        {
            using var _ = await GetLockAsync<T>();

            var map = GetStateQueue<T>().Last();

            if (!map.TryGetValue(key, out var val)) return new StateStoreResult.Error();
            if (!val.Equals(currentState)) return new StateStoreResult.Error();

            PushStateMap<T>(m => m.Remove(key));

            NotifyObservers<T>();

            return new StateStoreResult.Ok();
        }

        public IObservable<IEnumerable<ImmutableDictionary<string, T>>> GetObservable<T>() =>
            GetSubject<T>().Select(_ => GetStateQueue<T>().AsEnumerable());

        #endregion

    }

}
