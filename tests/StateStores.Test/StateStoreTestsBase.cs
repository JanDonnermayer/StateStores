using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    public abstract class StateStoreTestsBase<TStateStore>
        where TStateStore : IStateStore
    {
        protected abstract TStateStore GetStateStore();

        public virtual async Task BasicFunctionality()
        {
            const string KEY = "key";
            const string TOKEN_1 = "token1";
            const string TOKEN_2 = "token2";
            const int SAMPLE_STATE = 0;

            var store = GetStateStore();

            // Can set using Token1
            Assert.IsTrue(await store.TrySetAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Can update using Token1
            Assert.IsTrue(await store.TrySetAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Cannot update using Token2
            Assert.IsFalse(await store.TrySetAsync(KEY, TOKEN_2, SAMPLE_STATE));

            // Cannot remove using Token2
            Assert.IsFalse(await store.TryRemoveAsync<int>(KEY, TOKEN_2));

            // Can remove using Token1
            Assert.IsTrue(await store.TryRemoveAsync<int>(KEY, TOKEN_1));

            // Cannot remove second time using Token1
            Assert.IsFalse(await store.TryRemoveAsync<int>(KEY, TOKEN_1));
        }


        public virtual async Task ParallelFunctionality()
        {
            const int STATES_COUNT = 100;

            static IEnumerable<string> GetStates() =>
                Enumerable.Range(0, STATES_COUNT).Select(index => $"value_{index}");

            var store = GetStateStore();

            async Task<bool> TryUseStoreAsync(int index)
            {
                var key = $"key_{index}";
                var token = $"token_{index}";

                var results = (await Task.WhenAll(GetStates().Select(async state =>
                            await store.TrySetAsync(key, token, state))))
                    .Append(await store.TryRemoveAsync<string>(key, token));

                return results.All(_ => _);
            }

            const int PARALLEL_WORKERS_COUNT = 1000;

            var results = await Task.WhenAll(Enumerable.Range(1, PARALLEL_WORKERS_COUNT)
                .Select(TryUseStoreAsync));

            Assert.IsTrue(results.All(_ => _));
        }

    }
}