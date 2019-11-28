using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;

namespace StateStores.Test
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