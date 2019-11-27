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
        public Task BasicFunctionality() => 
            GetStateStore().TestBasicFunctionality(); 

        [Test]
        public Task ParallelFunctionality() => 
            GetStateStore().TestParallelFunctionality(); 

    }
}