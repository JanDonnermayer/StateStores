using System;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores
{
    public static class StateStoreProxy
    {
        public static IStateStoreProxy<TState> CreateProxy<TState>(this IStateStore store, string key, string token) =>
            new StateStoreProxyInstance<TState>(store, key, token);

        #region  Private Types

        private struct StateStoreProxyInstance<TState> : IStateStoreProxy<TState>
        {
            private readonly IStateStore store;
            private readonly string key;
            private readonly string token;

            public StateStoreProxyInstance(IStateStore store, string key, string token)
            {
                this.store = store ?? throw new ArgumentNullException(nameof(store));
                this.key = key ?? throw new ArgumentNullException(nameof(key));
                this.token = token ?? throw new ArgumentNullException(nameof(token));
            }

            async Task<StateStoreResult> IStateStoreProxy<TState>.EnterAsync(TState state) => 
                await store.EnterAsync(key, token, state).ConfigureAwait(false);

            async Task<StateStoreResult> IStateStoreProxy<TState>.ExitAsync() => 
                await store.ExitAsync<TState>(key, token).ConfigureAwait(false);

            async Task<StateStoreResult> IStateStoreProxy<TState>.TransferAsync(TState state1, TState state2) => 
                await store.TransferAsync(key, token, state1, state2).ConfigureAwait(false);
        }

        #endregion
    }

}
