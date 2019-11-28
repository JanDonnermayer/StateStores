using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;
using StateStores.Test;

namespace StateStores.InMemory.Test
{

    [TestFixture]
    public class InMemoryStateStoreTests
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task TestBasicFunctionalityAsync() => 
            GetStateStore().TestBasicFunctionalityAsync(key: Guid.NewGuid().ToString()); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            GetStateStore().TestParallelFunctionalityAsync(); 

    }
}