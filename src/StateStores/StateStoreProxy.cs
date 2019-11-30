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
    public static class StateStoreProxy
    {
        public static IStateStoreProxy<TState> CreateProxy<TState>(this IStateStore store, string key) =>
            new StateStoreProxyInstance<TState>(store, key);

        #region  Private Types

        private class StateStoreProxyInstance<TState> : IStateStoreProxy<TState>
        {
            private readonly IStateStore store;

            private readonly string key;

            private readonly Lazy<IObservable<IImmutableDictionary<string, TState>>> lazyStateObervable;

            private IObservable<(IImmutableDictionary<string, TState> previous, IImmutableDictionary<string, TState> current)> GetObservable() =>
                lazyStateObervable.Value.Buffer(2, 1).Select(_ => (_[0], _[1]));

            public StateStoreProxyInstance(IStateStore store, string key)
            {
                this.store = store ?? throw new ArgumentNullException(nameof(store));
                this.key = key ?? throw new ArgumentNullException(nameof(key));

                this.lazyStateObervable = new Lazy<IObservable<IImmutableDictionary<string, TState>>>(
                    store.GetObservable<TState>
                );
            }

            public async Task<StateStoreResult> AddAsync(TState nextState) =>
                  await store.AddAsync(key, nextState).ConfigureAwait(false);

            public async Task<StateStoreResult> RemoveAsync(TState currentState) =>
                  await store.RemoveAsync(key, currentState).ConfigureAwait(false);

            public async Task<StateStoreResult> UpdateAsync(TState currentState, TState nextState) =>
                 await store.UpdateAsync(key, currentState, nextState).ConfigureAwait(false);

            public IObservable<TState> OnAdd =>
                GetObservable()
                    .Where(_ => _.current.ContainsKey(key) && !_.previous.ContainsKey(key))
                    .Select(_ => _.current[key]);

            public IObservable<TState> OnRemove =>
                GetObservable()
                    .Where(_ => !_.current.ContainsKey(key) && _.previous.ContainsKey(key))
                    .Select(_ => _.previous[key]);

            public IObservable<(TState previousState, TState currentState)> OnUpdate =>
                GetObservable()
                    .Where(_ => _.current.ContainsKey(key) && _.previous.ContainsKey(key))
                    .Select(_ => (_.previous[key], _.current[key]));
        }

        #endregion
    }

}
