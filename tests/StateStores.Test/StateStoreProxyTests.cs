using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using System.Reactive.Linq;

namespace StateStores.Test
{
    public static class StateStoreProxyTests
    {
        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        public enum SampleStates { state1, state2 }

        public static async Task TestBasicFunctionalityAsync(this IStateStoreProxy<SampleStates> proxy)
        {


            const SampleStates SAMPLE_STATE_1 = SampleStates.state1;
            const SampleStates SAMPLE_STATE_2 = SampleStates.state2;

            const int EXPECTED_UPDATE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_ADD_NOTIFICATION_COUNT = 1;
            const int EXPECTED_REMOVE_NOTIFICATION_COUNT = 1;

            const int OBSERVER_DELAY_MS = 200;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualUpdateNotificationCount = 0;
            int mut_ActualRemoveNotificationCount = 0;

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
        public static async Task TestReactiveFunctionalityAsync(this IStateStoreProxy<int> proxy, int stateCount)
        {

            proxy.OnAdd
                .Subscribe(i =>
                {
                    proxy.UpdateAsync(i, i++);
                });


            proxy.OnUpdate
                .Select(_ => _.currentState)
                .Subscribe(i =>
                {
                    if (i < stateCount)
                    {
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

            Assert.AreEqual(stateCount, finalStateCountActual);
        }


    }
}