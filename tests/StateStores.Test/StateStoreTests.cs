using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    internal static class StateStoreTests
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        static void AssertStateError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.StateError), result);

        static void AssertTokenError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.TokenError), result);

        public static async Task TestBasicFunctionality(this IStateStore store, string key = "key")
        {
            const string TOKEN_1 = "token1";
            const string TOKEN_2 = "token2";
            const string STATE_1 = "state1";
            const string STATE_2 = "state2";

            AssertOk(await store.AddAsync(key, TOKEN_1, STATE_1));

            AssertStateError(await store.AddAsync(key, TOKEN_1, STATE_1));

            AssertStateError(await store.AddAsync(key, TOKEN_1, STATE_2));

            AssertTokenError(await store.UpdateAsync(key, TOKEN_2, STATE_1, STATE_2));

            AssertStateError(await store.UpdateAsync(key, TOKEN_1, STATE_2, STATE_2));

            AssertOk(await store.UpdateAsync(key, TOKEN_1, STATE_1, STATE_2));

            AssertTokenError(await store.RemoveAsync(key, TOKEN_2, STATE_2));

            AssertStateError(await store.RemoveAsync(key, TOKEN_1, STATE_1));

            AssertOk(await store.RemoveAsync(key, TOKEN_1, STATE_2));

            AssertStateError(await store.RemoveAsync(key, TOKEN_1, STATE_1));
        }

        public static async Task TestParallelFunctionality(this IStateStore store)
        {
            const int PARALLEL_WORKERS_COUNT = 100000;

            await Task.WhenAll(
                Enumerable
                    .Range(0, PARALLEL_WORKERS_COUNT)
                    .Select(i => store.TestBasicFunctionality(i.ToString())));
        }

    }
}