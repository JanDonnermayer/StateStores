using NUnit.Framework;


namespace StateStores.Test
{
    [TestFixture]
    public class StateStoreResultTests
    {
        [Test]
        public void Test_Ok()
        {
            Assert.IsInstanceOf<Ok>( StateStoreResult.Ok);
        }

        [Test]
        public void Test_StateError()
        {
            Assert.IsInstanceOf<StateError>(StateStoreResult.StateError);
        }

        [Test]
        public void Test_ConnectionError()
        {
            Assert.IsInstanceOf<ConnectionError>( StateStoreResult.ConnectionError);
        }
    }

}