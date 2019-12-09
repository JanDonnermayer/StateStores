using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores
{
    public static class StateChannel
    {
        public static IStateChannel<TState> CreateChannel<TState>(this IStateStore store, string key) =>
            new Instance<TState>(store, key);


        public static IObservable<TState> OnNext<TState>(this IStateChannel<TState> channel) =>
            channel.OnAdd.Merge(channel.OnUpdate.Select(_ => _.currentState));

        public static IObservable<TState> OnNext<TState>(this IStateChannel<TState> channel,
            Func<TState, bool> condition) =>
                channel.OnNext().Where(condition);

        public static IObservable<TState> OnNext<TState>(this IStateChannel<TState> channel,
            TState value) =>
                channel.OnNext().Where(state => EqualityComparer<TState>.Default.Equals(state, value));


        public static IObservable<TState> OnPrevious<TState>(this IStateChannel<TState> channel) =>
            channel.OnRemove
                .Merge(channel.OnUpdate
                .Select(_ => _.previousState));
                
        public static IObservable<TState> OnPrevious<TState>(this IStateChannel<TState> channel,
            Func<TState, bool> condition) =>
                channel.OnPrevious().Where(condition);

        public static IObservable<TState> OnPrevious<TState>(this IStateChannel<TState> channel,
            TState value) =>
                channel.OnPrevious().Where(state => EqualityComparer<TState>.Default.Equals(state, value));


        #region  Private Types

        private class Instance<TState> : IStateChannel<TState>
        {
            private readonly IStateStore store;
            private readonly string key;
            private readonly Lazy<IObservable<IEnumerable<ImmutableDictionary<string, TState>>>> lazyStateObervable;

            private IObservable<(ImmutableDictionary<string, TState> previous, ImmutableDictionary<string, TState> current)> GetObservable() =>
                lazyStateObervable.Value
                    .Select(_ => _.Reverse())
                    .Select(_ => (_.Skip(1).First(), _.First()));

            public Instance(IStateStore store, string key)
            {
                this.store = store ?? throw new ArgumentNullException(nameof(store));
                this.key = key ?? throw new ArgumentNullException(nameof(key));
                this.lazyStateObervable = new Lazy<IObservable<IEnumerable<ImmutableDictionary<string, TState>>>>(
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
                    .Select(_ => (previousState: _.previous[key], currentState: _.current[key]))
                    .Where(_ => !EqualityComparer<TState>.Default.Equals(_.previousState, _.currentState));
        }

        #endregion
    }

}
