using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;

namespace StateStores.Test
{

    [TestFixture]
    public class InMemoryStateStoreTests : StateStoreTests
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task BasicFunctionality() => 
            TestBasicFunctionality(GetStateStore()); 

        [Test]
        public Task ParallelFunctionality() => 
            TestParallelFunctionality(GetStateStore()); 

    }
}