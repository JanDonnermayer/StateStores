﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using System.Reactive.Linq;
using StateStores.InMemory;

namespace StateStores.Test
{

    public abstract class StateStoreProxyTestBase
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        protected virtual async Task BasicFunctionality(IStateStore store)
        {
            const string KEY = "key";

            const int SAMPLE_STATE_1 = 0;
            const int SAMPLE_STATE_2 = 1;

            const int EXPECTED_UPDATE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_ADD_NOTIFICATION_COUNT = 1;
            const int EXPECTED_REMOVE_NOTIFICATION_COUNT = 1;

            const int OBSERVER_DELAY_MS = 200;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualUpdateNotificationCount = 0;
            int mut_ActualRemoveNotificationCount = 0;

            var proxy = store.CreateProxy<int>(KEY);

            proxy.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount += 1);
            proxy.OnUpdate.Subscribe(_ => mut_ActualUpdateNotificationCount += 1);
            proxy.OnRemove.Subscribe(_ => mut_ActualRemoveNotificationCount += 1);

            // Can set 
            AssertOk(await proxy.AddAsync(SAMPLE_STATE_1));
            await Task.Delay(OBSERVER_DELAY_MS);

            // Can update 
            AssertOk(await proxy.UpdateAsync(SAMPLE_STATE_1, SAMPLE_STATE_2));
            await Task.Delay(OBSERVER_DELAY_MS);

            // Can remove 
            AssertOk(await proxy.RemoveAsync(SAMPLE_STATE_2));
            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(
                EXPECTED_ADD_NOTIFICATION_COUNT,
                mut_ActualAddNotificationCount);

            Assert.AreEqual(
                EXPECTED_UPDATE_NOTIFICATION_COUNT,
                mut_ActualUpdateNotificationCount);

            Assert.AreEqual(
                EXPECTED_REMOVE_NOTIFICATION_COUNT,
                mut_ActualRemoveNotificationCount);
        }

        // This is a state-transition-chain where observers invoke transitions.
        protected virtual async Task ReactiveFunctionality(IStateStore store)
        {
            const string KEY = "key";
            const int STATE_COUNT = 100;

            var proxy = store.CreateProxy<int>(KEY);

            proxy.OnAdd
                .Subscribe(i =>
                {
                    proxy.UpdateAsync(i, i++);
                });


            proxy.OnUpdate
                .Select(_ => _.currentState)
                .Subscribe(i =>
                {
                    if (i < STATE_COUNT)
                    {   // redundant state updates are rejected
                        proxy.UpdateAsync(i, i + 1);
                    }
                    else
                    {
                        proxy.RemoveAsync(i);
                    }
                });



            var tcsFinal = new TaskCompletionSource<int>();
            proxy.OnRemove.Subscribe(tcsFinal.SetResult);

            AssertOk(await proxy.AddAsync(0));

            var finalStateCountActual = await tcsFinal.Task;

            Assert.AreEqual(STATE_COUNT, finalStateCountActual);
        }


    }
}