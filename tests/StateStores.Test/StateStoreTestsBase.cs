using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    public abstract class StateStoreTestsBase
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Error), result);

        protected virtual async Task TestBasicFunctionality(IStateStore store,
            string key = "keylel")
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

        protected virtual async Task TestParallelFunctionality(IStateStore store)
        {
            const int PARALLEL_WORKERS_COUNT = 3;
            const int COUNT = 100;

            await Task.WhenAll(Enumerable
                .Range(0, PARALLEL_WORKERS_COUNT)
                .Select(async i =>
                {
                    for (int j = 0; j < COUNT; j++)
                        await TestBasicFunctionality(store, i.ToString());
                }));
        }

    }
}