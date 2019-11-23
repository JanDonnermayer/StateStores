using System;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateStoreProxy<TState>
    {
        Task<bool> TrySetAsync(TState state);

        Task<bool> TryRemoveAsync();
    }

}
