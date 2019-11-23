using System;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateStoreProxy<TState> : IAsyncDisposable
    {
        Task<bool> TrySetAsync(TState state);
    }

}
