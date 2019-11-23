using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StateStores
{
    public interface IStateStore
    {
        Task<bool> TrySetAsync<T>(string key, string token, T state);

        Task<bool> TryRemoveAsync<T>(string key, string token);
        
        IObservable<IEnumerable<KeyValuePair<string, T>>> GetObservable<T>();
    }


}
