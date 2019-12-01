using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace StateStores
{
    public interface IStateStore
    {
        Task<StateStoreResult> AddAsync<T>(string key, T next);

        Task<StateStoreResult> UpdateAsync<T>(string key, T current, T next);

        Task<StateStoreResult> RemoveAsync<T>(string key, T current);

        IObservable<IEnumerable<ImmutableDictionary<string, T>>> GetObservable<T>();

    }


}
