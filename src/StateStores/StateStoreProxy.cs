using System;
using System.Threading;
using System.Threading.Tasks;

namespace StateStores
{
    public static class StateStoreProxy
    {
        public static IStateStoreProxy<TState> CreateProxy<TState>(this IStateStore store, string key)
        {
            var token = Guid.NewGuid().ToString();
            return new StateStoreProxyInstance<TState>(store, key, token);
        }

        #region  Private Types

        private struct StateStoreProxyInstance<TState> : IStateStoreProxy<TState>
        {
            private readonly IStateStore store;
            private readonly string key;
            private readonly string token;
            private int mut_disposed;

            public StateStoreProxyInstance(IStateStore store, string key, string token)
            {
                this.store = store ?? throw new ArgumentNullException(nameof(store));
                this.key = key ?? throw new ArgumentNullException(nameof(key));
                this.token = token ?? throw new ArgumentNullException(nameof(token));
                this.mut_disposed = 0;
            }

            async ValueTask IAsyncDisposable.DisposeAsync()
            {
                if (Interlocked.Exchange(ref mut_disposed, 1) == 1) throw new ObjectDisposedException(this.GetType().Name);
                await store.TryRemoveAsync<TState>(key, token).ConfigureAwait(false);
            }

            async Task<bool> IStateStoreProxy<TState>.TrySetAsync(TState state)
            {
                if (mut_disposed == 1) throw new ObjectDisposedException(this.GetType().Name);
                return await store.TrySetAsync(key, token, state).ConfigureAwait(false);
            }
        }

        #endregion
    }

}
