using System;
using System.Reactive;

namespace StateStores
{
    /// <summary>
    /// An immutable view on a single state,
    /// providing functionality to modify it,
    /// thereby creating a new immutable view.
    /// </summary>
    public interface IImmutableStateHandle<TState>
    {
        TState State { get; }

        /// <summary>
        /// Sets the underyling state to no state,
        /// returning an Observable, that on success, emits <see cref="Unit"/>.
        /// If the update is not successfull, no notification is emitted.
        /// </summary>
        IObservable<Unit> Remove();

        /// <summary>
        /// Sets the underyling state to the specified <paramref see="nextState"/>,
        /// returning an Observable, that on success, emits a new handle.
        /// If the update is not successfull, no notification is emitted.
        /// </summary>
        /// <param name="nextState">The state to set.</param>
        IObservable<IImmutableStateHandle<TState>> Update(TState nextState);

        /// <summary>
        /// Uses the specified <paramref name="nextStateProvider"/> to update
        /// the underyling state to a new state,
        /// returning an Observable, that on success, emits a new handle.
        /// If the update is not successfull, no notification is emitted.
        /// </summary>
        /// <param name="nextStateProvider">
        /// The function to apply on the current state in order to obtain then next state.
        /// </param>
        IObservable<IImmutableStateHandle<TState>> Update(Func<TState, TState> nextStateProvider);
    }

}
