using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateStoreProxy<TState>
    {
        Task<StateStoreResult> EnterAsync(TState state);

        Task<StateStoreResult> TransferAsync(TState state1, TState state2);

        Task<StateStoreResult> ExitAsync();

    }

}
