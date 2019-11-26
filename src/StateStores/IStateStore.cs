using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace StateStores
{
    public interface IStateStore
    {
        Task<StateStoreResult> AddAsync<T>(string key, string token, T next);

        Task<StateStoreResult> UpdateAsync<T>(string key, string token, T current, T next);

        Task<StateStoreResult> RemoveAsync<T>(string key, string token, T current);

        IObservable<IImmutableDictionary<string, T>> GetObservable<T>();
    }


}
