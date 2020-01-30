using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateStores
{
    /// <summary>
    /// Providing functionality to add, modify or remove a state.
    /// </summary>
    public interface IStateChannel<TState>
    {
        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits the current state,
        /// everytime the state of the channel is set from no state to some state,
        /// replaying the latest notification (if any) to new subscribers.
        /// </summary>
        IObservable<TState> OnAdd { get; }

        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits a pair of the previous and the current state,
        /// everytime the state of the channel is set from some state to some other state,
        /// replaying the latest notification (if any) to new subscribers.
        /// </summary>
        IObservable<(TState previousState, TState currentState)> OnUpdate { get; }

        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits the previous state,
        /// everytime the state of the channel is set from some state to no state,
        /// replaying the latest notification (if any) to new subscribers.
        /// </summary>
        IObservable<TState> OnRemove { get; }

        /// <summary>
        /// If the channel is currently set to no state,
        /// sets it to the specified <paramref name="nextState"/>,
        /// else: returns <see cref="StateError"/>
        /// </summary>
        /// <param name="nextState">The state to set for the channel.</param>
        Task<StateStoreResult> AddAsync(TState nextState);

        /// <summary>
        /// If the channel is currently set to the specified <paramref name="currentState"/>,
        /// sets it to the specified <paramref name="nextState"/>, 
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="currentState">The expected state of the channel.</param>
        /// <param name="nextState">The state to set for the channel.</param>
        Task<StateStoreResult> UpdateAsync(TState currentState, TState nextState);

        /// <summary>
        /// If the channel is currently set to the specified <paramref name="currentState"/>,
        /// sets it to no state,
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="currentState">The expected state of the channel.</param>
        Task<StateStoreResult> RemoveAsync(TState currentState);
    }
}
