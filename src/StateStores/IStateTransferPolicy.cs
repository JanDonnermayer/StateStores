using System.Collections.Generic;

#nullable enable

namespace StateStores
{
    interface IStateTransferPolicy
    {
        bool PermitsTransfer<TState>(TState state1, TState state2);

        bool PermitsEntry<TState>(TState state);
        
        bool PermitsExit<TState>(TState state);
    }
}
