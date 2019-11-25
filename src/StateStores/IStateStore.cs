using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace StateStores
{
    public interface IStateStore
    {
        Task<StateStoreResult> AddAsync<T>(string key, string token, T nexT);

        Task<StateStoreResult> UpdateAsync<T>(string key, string token, T currenT, T nexT);

        Task<StateStoreResult> RemoveAsync<T>(string key, string token, T currenT);

        IObservable<IImmutableDictionary<string, T>> GetObservable<T>();
    }


}
