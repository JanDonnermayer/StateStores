using NUnit.Framework;

namespace StateStores.Test
{
    [TestFixture]
    public class StateStoreResultTests
    {
        [Test]
        public void Test_Ok()
        {
            Assert.IsNotNull(StateStoreResult.Ok());
        }

        [Test]
        public void Test_StateError()
        {
            Assert.IsNotNull(StateStoreResult.StateError());
        }

        [Test]
        public void Test_ConnectionError()
        {
            Assert.IsNotNull(StateStoreResult.ConnectionError());
        }
    }

}