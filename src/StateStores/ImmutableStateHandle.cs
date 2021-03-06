﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using static StateStores.StateStoreResult;

namespace StateStores
{
    public static class ImmutableStateHandle
    {
        public static IObservable<IImmutableStateHandle<TState>> ToHandle<TState>(
            this IStateChannel<TState> channel,
            Func<IStateChannel<TState>, IObservable<TState>> filter) =>
                (filter ?? throw new ArgumentNullException(nameof(filter)))
                    .Invoke(channel ?? throw new ArgumentNullException(nameof(channel)))
                    .Select(state => Instance<TState>.Create(channel, state));

        public static IObservable<IImmutableStateHandle<TState>> AddToHandle<TState>(
            this IStateChannel<TState> channel, TState nextState) =>
                Observable
                    .FromAsync(() => channel.AddAsync(nextState))
                    .Where(r => r is Ok)
                    .Select(_ => Instance<TState>.Create(channel, nextState));

        public static IObservable<IImmutableStateHandle<TState>> UpdateToHandle<TState>(
            this IStateChannel<TState> channel, TState currentState, TState nextState) =>
                Observable
                    .FromAsync(() => channel.UpdateAsync(currentState, nextState))
                    .Where(r => r is Ok)
                    .Select(_ => Instance<TState>.Create(channel, nextState));

        public static IObservable<IImmutableStateHandle<TState>> OnNextToHandle<TState>(
            this IStateChannel<TState> channel) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .ToHandle(cnl => cnl.OnNext());

        public static IObservable<IImmutableStateHandle<TState>> OnNextToHandle<TState>(
            this IStateChannel<TState> channel, Func<TState, bool> filter) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .ToHandle(cnl => cnl.OnNext(filter));

        public static IObservable<IImmutableStateHandle<TState>> OnNextToHandle<TState>(
            this IStateChannel<TState> channel, TState filter) =>
                (channel ?? throw new ArgumentNullException(nameof(channel)))
                    .ToHandle(cnl => cnl.OnNext(filter));

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

        #region  Private Types

        private class Instance<TState> : IImmutableStateHandle<TState>
        {
            private readonly IStateChannel<TState> channel;

            private Instance(IStateChannel<TState> channel, TState state)
            {
                this.channel = channel;
                this.State = state;
            }

            public TState State { get; }

            public static Instance<TState> Create(IStateChannel<TState> channel, TState state) =>
                new Instance<TState>(channel, state);

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
