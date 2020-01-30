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
            Assert.IsInstanceOf(typeof(ErrorResult), result);

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
            this IStateStore store, int activeChannelCount = 10,
            int stepBlockcount = 100, int passiveChannelCount = 1000)
        {
            await Task.WhenAll(Enumerable
                .Range(0, passiveChannelCount)
                .Select(i => store.AddAsync($"passive_{i}", "state0")))
                .ConfigureAwait(false);

            await Task.WhenAll(Enumerable
                .Range(0, activeChannelCount)
                .Select(async i =>
                {
                    for (int j = 0; j < stepBlockcount; j++)
                        await TestBasicFunctionalityAsync(store, i.ToString()).ConfigureAwait(false);
                }))
                .ConfigureAwait(false);

            await Task.WhenAll(Enumerable
                .Range(0, passiveChannelCount)
                .Select(i => store.RemoveAsync($"passive_{i}", "state0")))
                .ConfigureAwait(false);
        }
    }

}