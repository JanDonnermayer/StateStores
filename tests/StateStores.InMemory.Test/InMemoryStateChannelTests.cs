using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.Test;
using static StateStores.Test.StateChannelTests;

namespace StateStores.InMemory.Test
{
    [TestFixture]
    public class InMemoryStateChannelTests
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();


        [Test]
        public async Task TestBasicFunctionalityAsync()
        {
            var store = GetStateStore();
            await store
                .CreateChannel<string>(key: Guid.NewGuid().ToString())
                .TestBasicFunctionalityAsync();
        }

        [Test]
        public async Task TestReactiveFunctionalityAsync()
        {
            var store = GetStateStore();
            await store
                .CreateChannel<int>(key: Guid.NewGuid().ToString())
                .TestReactiveFunctionalityAsync(stepCount: 1000);
        }

        [Test]
        public async Task TestReplayFunctionalityAsync()
        {
            var store = GetStateStore();
            await store
                .CreateChannel<string>(key: Guid.NewGuid().ToString())
                .TestReplayFunctionalityAsync();
        }
    }
}