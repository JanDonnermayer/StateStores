using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    public abstract class StateStoreTestsBase<TStateStore>
        where TStateStore : IStateStore
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Error), result);

        protected abstract TStateStore GetStateStore();

        public virtual async Task BasicFunctionality()
        {
            const string KEY = "key";
            const string TOKEN_1 = "token1";
            const string TOKEN_2 = "token2";
            const int SAMPLE_STATE = 0;
            const int EXPECTED_NOTIFICATION_COUNT = 3 + 1; //+1 for BehaviourSubject
            const int OBSERVER_DELAY_MS = 400;

            var store = GetStateStore();

            int mut_ActualNotificationCount = 0;
            store.GetObservable<int>().Subscribe(_ => mut_ActualNotificationCount += 1);

            // Can enter using Token1
            AssertOk(await store.AddAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Cannot enter using Token2
            AssertError(await store.AddAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Cannot enter second time using Token1
            AssertError(await store.AddAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Can transfer using Token1
            AssertOk(await store.UpdateAsync(KEY, TOKEN_1, SAMPLE_STATE, SAMPLE_STATE));

            // Cannot transfer using Token2
            AssertError(await store.UpdateAsync(KEY, TOKEN_2, SAMPLE_STATE, SAMPLE_STATE));

            // Cannot exit using Token2
            AssertError(await store.RemoveAsync(KEY, TOKEN_2, SAMPLE_STATE));

            // Can exit using Token1
            AssertOk(await store.RemoveAsync(KEY, TOKEN_1, SAMPLE_STATE));

            // Cannot exit second time using Token1
            AssertError(await store.RemoveAsync(KEY, TOKEN_1, SAMPLE_STATE));

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_NOTIFICATION_COUNT, mut_ActualNotificationCount);
        }

    }
}