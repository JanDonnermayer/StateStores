using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{

    [TestFixture]
    public class InMemoryStateStoreTests : StateStoreTestsBase<InMemoryStateStore>
    {
        protected override InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public override Task BasicFunctionality() => base.BasicFunctionality(); 

        [Test]
        public override Task ParallelFunctionality() => base.ParallelFunctionality(); 

    }
}