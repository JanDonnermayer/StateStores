using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using static StateStores.StateStoreResult;

namespace StateStores
{

    public static class ImmutableStateHandle
    {
        private static IImmutableStateHandle<TState> Create<TState>(               
               this IStateChannel<TState> channel,
               TState currentState) =>
                   new Instance<TState>(
                       state: currentState,
                       removeAsync: () => Observable
                              .FromAsync(() => channel.RemoveAsync(currentState))
                              .Where(r => r is Ok)
                              .Select(_ => Unit.Default),
                       updateAsync: nextState => Observable
                              .FromAsync(() => channel.UpdateAsync(currentState, nextState))
                              .Where(r => r is Ok)
                              .Select(_ => Create(channel, nextState)));


        public static IObservable<IImmutableStateHandle<TState>> CreateHandle<TState>(this IStateChannel<TState> channel) =>
            channel.OnNext().Select(channel.Create);

        public static IObservable<IImmutableStateHandle<TState>> CreateHandle<TState>(this IStateChannel<TState> channel,
            Func<TState, bool> triggerCondition) =>
                channel.OnNext(triggerCondition).Select(channel.Create);

        public static IObservable<IImmutableStateHandle<TState>> CreateHandle<TState>(this IStateChannel<TState> channel,
            TState triggerValue) =>
                channel.OnNext(triggerValue).Select(channel.Create);


        public static IObservable<IImmutableStateHandle<TState>> Update<TState>(
            this IObservable<IImmutableStateHandle<TState>> observable,
            TState nextState) =>
                observable
                    .Select(h => h.UpdateAsync(nextState))
                    .Concat();
        public static IObservable<IImmutableStateHandle<TState>> Update<TState>(
            this IObservable<IImmutableStateHandle<TState>> observable,
            Func<TState, TState> nextStateFactory) =>
                observable
                    .Select(h => h.UpdateAsync(nextStateFactory(h.Value)))
                    .Concat();
        public static IObservable<Unit> Remove<TState>(
            this IObservable<IImmutableStateHandle<TState>> observable) =>
                observable
                    .Select(h => h.RemoveAsync())
                    .Concat();


        #region  Private Types

        class Instance<TState> : IImmutableStateHandle<TState>
        {
            private readonly Func<IObservable<Unit>> removeAsync;
            private readonly Func<TState, IObservable<IImmutableStateHandle<TState>>> updateAsync;

            public Instance(
                TState state,
                Func<IObservable<Unit>> removeAsync,
                Func<TState, IObservable<IImmutableStateHandle<TState>>> updateAsync)
            {
                this.Value = state;
                this.removeAsync = removeAsync;
                this.updateAsync = updateAsync;
            }

            public TState Value { get; }

            public IObservable<IImmutableStateHandle<TState>> UpdateAsync(TState nextState) =>
                updateAsync(nextState);

            public IObservable<Unit> RemoveAsync() =>
                removeAsync();

        }

        #endregion
    }

}
