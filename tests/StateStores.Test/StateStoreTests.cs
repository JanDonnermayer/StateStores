using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using NUnit.Framework;
using static StateStores.StateStoreResult;


namespace StateStores.Test
{

    public static class StateStoreTests
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(Ok), result);

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(Error), result);

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
            this IStateStore store, int parallelHandlersCount = 30, int stateBlockCount = 100)
        {

            await Task.WhenAll(Enumerable
                .Range(0, parallelHandlersCount)
                .Select(async i =>
                {
                    for (int j = 0; j < stateBlockCount; j++)
                        await TestBasicFunctionalityAsync(store, i.ToString());
                }));
        }
    }
}