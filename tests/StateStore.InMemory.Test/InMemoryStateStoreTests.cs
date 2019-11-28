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
            GetStateStore().TestBasicFunctionalityAsync("test_key_1"); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            GetStateStore().TestParallelFunctionalityAsync(); 

    }
}