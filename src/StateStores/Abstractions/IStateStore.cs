using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace StateStores
{
    /// <summary>
    /// Providing functionality to interact with states.
    /// </summary>
    public interface IStateStore
    {
        /// <summary>
        /// If currently no state is present for the specified <paramref name="key"/>
        /// adds the specified <paramref name="nextState"/>, 
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="key">The key whose state to update.</param>
        /// <param name="nextState">The state to set for the specified <paramref name="key"/>.</param>
        Task<StateStoreResult> AddAsync<TState>(string key, TState nextState);

        /// <summary>
        /// If the state for the specified <paramref name="key"/>
        /// currently equals the specified <paramref name="currentState"/>,
        /// sets it to the specified <paramref name="nextState"/>, 
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="key">The key whose state to update.</param>
        /// <param name="currentState">The expected state for the specified <paramref name="key"/>.</param>
        /// <param name="nextState">The state to set for the specified <paramref name="key"/>.</param>
        Task<StateStoreResult> UpdateAsync<TState>(string key, TState currentState, TState nextState);

        /// <summary>
        /// If the state for the specified <paramref name="key"/>
        /// currently equals the specified <paramref name="currentState"/>,
        /// removes it,
        /// else: returns <see cref="StateError"/>.    
        /// </summary>
        /// <param name="key">The key whose state to update.</param>
        /// <param name="currentState">The expected state for the specified <paramref name="key"/>.</param>
        Task<StateStoreResult> RemoveAsync<TState>(string key, TState currentState);

        /// <summary>
        /// Returns an IObservable that emits the latest states
        /// among their keys when the states change,
        /// replaying the latest notification (if any) to new subscribers.
        /// </summary>
        IObservable<IEnumerable<ImmutableDictionary<string, TState>>> GetObservable<TState>();
    }
}
