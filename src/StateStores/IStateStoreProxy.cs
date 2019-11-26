using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateStoreProxy<TState>
    {
        Task<StateStoreResult> AddAsync(TState nextState);

        Task<StateStoreResult> UpdateAsync(TState currentState, TState nextState);

        Task<StateStoreResult> RemoveAsync(TState currentState);

        IObservable<TState> OnAdd { get; }

        IObservable<(TState previousState, TState currentState)> OnUpdate { get; }

        IObservable<TState> OnRemove { get; }
    }

}
