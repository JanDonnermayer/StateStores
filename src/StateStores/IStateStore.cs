using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace StateStores
{
    public interface IStateStore
    {
        Task<StateStoreResult> EnterAsync<T>(string key, string token, T state);

        Task<StateStoreResult> TransferAsync<T>(string key, string token, T state1, T state2);

        Task<StateStoreResult> ExitAsync<T>(string key, string token);
        
        IObservable<IImmutableDictionary<string, T>> GetObservable<T>();
    }


}
