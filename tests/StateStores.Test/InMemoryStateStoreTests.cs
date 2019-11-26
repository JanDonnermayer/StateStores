using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    [TestFixture]
    public class InMemoryStateStoreTests 
    {
        private InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task BasicFunctionality() => 
            GetStateStore().TestBasicFunctionality(); 

        [Test]
        public Task ParallelFunctionality() => 
            GetStateStore().TestParallelFunctionality(); 

    }
}