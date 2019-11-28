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
            GetStateStore().TestBasicFunctionalityAsync(); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            GetStateStore().TestParallelFunctionalityAsync(); 

    }
}