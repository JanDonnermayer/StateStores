using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using System.Reactive.Linq;

namespace StateStores.Test
{
    [TestFixture]
    public class StateStoreProxyTest
    {

        private IStateStore GetStateStore()
        {
            var store = new InMemoryStateStore();
            return store;
        }

        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);


        [Test]
        public async Task BasicFunctionality()
        {
            const string KEY = "key";
            const string TOKEN = "token";
            
            const int SAMPLE_STATE_1 = 0;
            const int SAMPLE_STATE_2 = 1;

            const int EXPECTED_UPDATE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_ADD_NOTIFICATION_COUNT = 1;
            const int EXPECTED_REMOVE_NOTIFICATION_COUNT = 1;

            const int OBSERVER_DELAY_MS = 100;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualUpdateNotificationCount = 0;
            int mut_ActualRemoveNotificationCount = 0;

            var store = GetStateStore();
            var proxy = store.CreateProxy<int>(KEY, TOKEN);

            proxy.OnAdd.Subscribe(_ => mut_ActualAddNotificationCount += 1);
            proxy.OnUpdate.Subscribe(_ => mut_ActualUpdateNotificationCount += 1);
            proxy.OnRemove.Subscribe(_ => mut_ActualRemoveNotificationCount += 1);

            // Can set 
            AssertOk(await proxy.AddAsync(SAMPLE_STATE_1));

            // Can update 
            AssertOk(await proxy.UpdateAsync(SAMPLE_STATE_1, SAMPLE_STATE_2));

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
        [Test]
        public async Task ReactiveFunctionality()
        {
            const string KEY = "key";
            const string TOKEN = "token";
            const int STATE_COUNT = 10000;

            // Concurrent handlers are redundant.
            // That is, only one handler is able to change the state.
            const int CONCURRENT_HANDLER_COUNT = 10;

            var tcsFinal = new TaskCompletionSource<int>();

            var store = GetStateStore();
            var proxy = store.CreateProxy<int>(KEY, TOKEN);

            proxy.OnAdd
                .Subscribe(i =>
                {
                    proxy.UpdateAsync(i, i++);
                });

            // Register concurrent handlers
            for (int i = 0; i < CONCURRENT_HANDLER_COUNT; i++)
            {
                proxy.OnUpdate
                    .Select(_ => _.currentState)
                    .Subscribe(i =>
                    {
                        Task.Run(() =>
                        {
                            if (i < STATE_COUNT)
                            {
                                proxy.UpdateAsync(i, i + 1);
                            }
                            else
                            {
                                proxy.RemoveAsync(i);
                            }
                        });
                    });
            }

            proxy.OnRemove.Subscribe(tcsFinal.SetResult);

            AssertOk(await proxy.AddAsync(0));

            var finalStateCountActual = await tcsFinal.Task;

            Assert.AreEqual(STATE_COUNT, finalStateCountActual);
        }


    }
}