using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using NUnit.Framework;

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

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Error), result);

        [Test]
        public async Task BasicFunctionality()
        {
            const string KEY = "key";
            const string TOKEN = "token";
            const int SAMPLE_STATE = 0;
            const int EXPECTED_UPDATE_NOTIFICATION_COUNT = 1;
            const int EXPECTED_ADD_NOTIFICATION_COUNT = 1;
            const int EXPECTED_REMOVE_NOTIFICATION_COUNT = 1;
            const int OBSERVER_DELAY_MS = 300;

            int mut_ActualAddNotificationCount = 0;
            int mut_ActualUpdateNotificationCount = 0;
            int mut_ActualRemoveNotificationCount = 0;

            var store = GetStateStore();
            var proxy = store.CreateProxy<int>(KEY, TOKEN);

            proxy.OnAdd().Subscribe(_ => mut_ActualAddNotificationCount += 1);
            proxy.OnUpdate().Subscribe(_ => mut_ActualUpdateNotificationCount += 1);
            proxy.OnRemove().Subscribe(_ => mut_ActualRemoveNotificationCount += 1);

            // Can set 
            AssertOk(await proxy.AddAsync(SAMPLE_STATE));

            // Can update 
            AssertOk(await proxy.UpdateAsync(SAMPLE_STATE, SAMPLE_STATE));

            // Can remove 
            AssertOk(await proxy.RemoveAsync(SAMPLE_STATE));

            await Task.Delay(OBSERVER_DELAY_MS);

            Assert.AreEqual(EXPECTED_ADD_NOTIFICATION_COUNT, mut_ActualAddNotificationCount);
            Assert.AreEqual(EXPECTED_UPDATE_NOTIFICATION_COUNT, mut_ActualUpdateNotificationCount);
            Assert.AreEqual(EXPECTED_REMOVE_NOTIFICATION_COUNT, mut_ActualRemoveNotificationCount);
        }
    }
}