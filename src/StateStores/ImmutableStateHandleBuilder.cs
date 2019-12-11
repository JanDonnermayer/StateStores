using System;
using System.Reactive;
using System.Reactive.Linq;
using static StateStores.StateStoreResult;

namespace StateStores
{

    public static class ImmutableStateHandleBuilder
    {
        public static IObservable<IImmutableStateHandle<TState>> WithHandle<TState>(
            this IObservable<TState> stateObservable, IStateChannel<TState> channel) =>
                stateObservable
                    .Select(state => ImmutableStateHandle<TState>.Create(channel, state));

        public static IObservable<IImmutableStateHandle<TState>> AddWithHandle<TState>(
            this IStateChannel<TState> channel, TState nextState) =>
                Observable
                    .FromAsync(() => channel.AddAsync(nextState))
                    .Where(r => r is Ok)
                    .Select(_ => ImmutableStateHandle<TState>.Create(channel, nextState));

        public static IObservable<IImmutableStateHandle<TState>> UpdateWithHandle<TState>(
            this IStateChannel<TState> channel, TState currentState, TState nextState) =>
                Observable
                    .FromAsync(() => channel.UpdateAsync(currentState, nextState))
                    .Where(r => r is Ok)
                    .Select(_ => ImmutableStateHandle<TState>.Create(channel, nextState));

        public static IObservable<IImmutableStateHandle<TState>> Update<TState>(
            this IObservable<IImmutableStateHandle<TState>> handleObservable, TState nextState) =>
                (handleObservable ?? throw new ArgumentNullException(nameof(handleObservable)))
                    .Select(handle => handle.Update(nextState))
                    .Concat();

        public static IObservable<IImmutableStateHandle<TState>> Update<TState>(
            this IObservable<IImmutableStateHandle<TState>> handleObservable, 
            Func<TState, TState> nextStateProvider) =>
                (handleObservable ?? throw new ArgumentNullException(nameof(handleObservable)))
                    .Select(handle => handle.Update(nextStateProvider))
                    .Concat();

        public static IObservable<Unit> Remove<TState>(
            this IObservable<IImmutableStateHandle<TState>> handleObservable) =>
                (handleObservable ?? throw new ArgumentNullException(nameof(handleObservable)))
                    .Select(handle => handle.Remove())
                    .Concat();

        public static IObservable<IImmutableStateHandle<TState>> OnNextWithHandle<TState>(
            this IStateChannel<TState> channel) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .OnNext()
                    .WithHandle(channel);

        public static IObservable<IImmutableStateHandle<TState>> OnNextWithHandle<TState>(
            this IStateChannel<TState> channel, Func<TState, bool> triggerCondition) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .OnNext(triggerCondition)
                    .WithHandle(channel);

        public static IObservable<IImmutableStateHandle<TState>> OnNextWithHandle<TState>(
            this IStateChannel<TState> channel, TState triggerValue) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .OnNext(triggerValue)
                    .WithHandle(channel);


        #region  Private Types

        private class ImmutableStateHandle<TState> : IImmutableStateHandle<TState>
        {
            private readonly IStateChannel<TState> channel;

            private ImmutableStateHandle(IStateChannel<TState> channel, TState state)
            {
                this.channel = channel;
                this.State = state;
            }

            public TState State { get; }

            public static ImmutableStateHandle<TState> Create(IStateChannel<TState> channel, TState state) =>
                new ImmutableStateHandle<TState>(channel, state);

            public IObservable<IImmutableStateHandle<TState>> Update(Func<TState, TState> nextStateProvider) =>
                Update(nextStateProvider(State));

            public IObservable<IImmutableStateHandle<TState>> Update(TState nextState) =>
                Observable
                    .FromAsync(() => channel.UpdateAsync(State, nextState))
                    .Where(r => r is Ok)
                    .Select(_ => Create(channel, nextState));

            public IObservable<Unit> Remove() =>
                Observable
                    .FromAsync(() => channel.RemoveAsync(State))
                    .Where(r => r is Ok)
                    .Select(_ => Unit.Default);

        }

        #endregion
    }

}
