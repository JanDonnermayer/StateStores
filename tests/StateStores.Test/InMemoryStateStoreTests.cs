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
        public Task BasicFunctionality() => 
            TestBasicFunctionalityAsync(GetStateStore()); 

        [Test]
        public Task ParallelFunctionality() => 
            TestParallelFunctionalityAsync(GetStateStore()); 

    }
}