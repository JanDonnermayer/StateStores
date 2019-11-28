using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;

namespace StateStores.Test
{

    [TestFixture]
    public class InMemoryStateStoreTests : StateStoreTestsBase
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task TestBasicFunctionalityAsync() => 
            TestBasicFunctionalityAsync(GetStateStore()); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            TestParallelFunctionalityAsync(GetStateStore()); 

    }
}