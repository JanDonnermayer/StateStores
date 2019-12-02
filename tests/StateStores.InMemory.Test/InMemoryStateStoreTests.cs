using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;
using StateStores.Test;
using static StateStores.Test.StateStoreProxyTests;

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
                .TestParallelFunctionalityAsync(5, 10000); 

        [Test]
        public Task TestReactiveFunctionalityAsync() => 
            GetStateStore()
                .CreateProxy<int>("key1")
                .TestReactiveFunctionalityAsync(10, 5); 

        [Test]
        public Task TestReplayFunctionalityAsync() => 
            GetStateStore()
                .CreateProxy<SampleStates>("key2")
                .TestReplayFunctionalityAsync();

    }
}