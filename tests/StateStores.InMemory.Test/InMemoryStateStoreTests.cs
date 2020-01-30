using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;
using StateStores.Test;
using static StateStores.Test.StateChannelTests;

namespace StateStores.InMemory.Test
{

    [TestFixture]
    class InMemoryStateStoreTests
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task TestBasicFunctionalityAsync() =>
            GetStateStore()
                .TestBasicFunctionalityAsync(key: Guid.NewGuid().ToString());

        [Test]
        public Task TestParallelFunctionalityAsync() =>
            GetStateStore()
                .TestParallelFunctionalityAsync(
                    activeChannelCount: 10,
                    stepBlockcount: 100,
                    passiveChannelCount: 1000);

        [Test]
        public Task TestReactiveFunctionalityAsync() =>
            GetStateStore()
                .ToChannel<int>("key1")
                .TestReactiveFunctionalityAsync(
                    stepCount: 10,
                    activeChannelCount: 5);

        [Test]
        public Task TestReplayFunctionalityAsync() =>
            GetStateStore()
                .ToChannel<string>("key2")
                .TestReplayFunctionalityAsync();
    }
}