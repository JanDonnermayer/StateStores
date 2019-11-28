using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    public static class StateStoreTests
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Error), result);

        public static async Task TestBasicFunctionalityAsync(this IStateStore store, string key)
        {

            const string STATE_1 = "state1";
            const string STATE_2 = "state2";


            AssertOk(await store.AddAsync(key, STATE_1));

            AssertError(await store.AddAsync(key, STATE_1));

            AssertError(await store.AddAsync(key, STATE_2));

            AssertError(await store.UpdateAsync(key, STATE_2, STATE_2));

            AssertOk(await store.UpdateAsync(key, STATE_1, STATE_2));

            AssertError(await store.RemoveAsync(key, STATE_1));

            AssertOk(await store.RemoveAsync(key, STATE_2));

            AssertError(await store.RemoveAsync(key, STATE_1));
        }

        public static async Task TestParallelFunctionalityAsync(
            this IStateStore store, int parallelWorkersCount = 1, int transactionsBlockCount = 1000)
        {

            await Task.WhenAll(Enumerable
                .Range(0, parallelWorkersCount)
                .Select(async i =>
                {
                    for (int j = 0; j < transactionsBlockCount; j++)
                        await TestBasicFunctionalityAsync(store, i.ToString());
                }));
        }

    }
}