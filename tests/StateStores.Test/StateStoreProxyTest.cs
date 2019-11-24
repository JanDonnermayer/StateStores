using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{
    [TestFixture]
    public class StateStoreProxyTest
    {

        private IStateStore GetStateStore()
        {
            var store = new Moq.Mock<IStateStore>();
            store.SetReturnsDefault(Task.FromResult((StateStoreResult)new StateStoreResult.Ok()));
            return store.Object;
        }

        static void AssertOk(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Ok), result);

        static void AssertError(StateStoreResult result) =>
            Assert.IsInstanceOf(typeof(StateStoreResult.Error), result);

        [Test]
        public async Task BasicFunctionality()
        {
            const string KEY = "key";
            const string TOKEN = "token";
            const int SAMPLE_STATE = 0;

            var store = GetStateStore();
            var proxy = store.CreateProxy<int>(KEY, TOKEN);

            // Can set 
            AssertOk(await proxy.EnterAsync(SAMPLE_STATE));

            // Can update 
            AssertOk(await proxy.TransferAsync(SAMPLE_STATE, SAMPLE_STATE));

            // Can remove 
            AssertOk(await proxy.ExitAsync());
        }
    }
}