﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateChannel<TState>
    {        
        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits the current state,
        /// when the state of the channel is set from no state to some state.
        /// </summary>
        IObservable<TState> OnAdd { get; }

        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits the previous state and the current state,
        /// when the state of the channel is set from some state to some other state.
        /// </summary>
        IObservable<(TState previousState, TState currentState)> OnUpdate { get; }

        /// <summary>
        /// Returns an <see cref="IObservable<TState>"/>,
        /// that emits the previous state,
        /// when the state of the channel is set from some state to no state.
        /// </summary>
        IObservable<TState> OnRemove { get; }

        /// <summary>
        /// If the channel is set to no state,
        /// sets it to the specified <paramref name="nextState"/>,
        /// else: returns <see cref="StateError"/>
        /// </summary>
        /// <param name="nextState">The state to set for the channel.</param>
        Task<StateStoreResult> AddAsync(TState nextState);

        /// <summary>
        /// If the channel is set to the specified <paramref name="currentState"/>,
        /// sets it to the specified <paramref name="nextState"/>, 
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="currentState">The expected state of the channel.</param>
        /// <param name="nextState">The state to set for the channel.</param>
        Task<StateStoreResult> UpdateAsync(TState currentState, TState nextState);

        /// <summary>
        /// If the channel is set to the specified <paramref name="currentState"/>,
        /// sets it to no state,
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="currentState">The expected state of the channel.</param>
        /// <param name="nextState">The state to set for the channel.</param>
        Task<StateStoreResult> RemoveAsync(TState currentState);
    }
}
