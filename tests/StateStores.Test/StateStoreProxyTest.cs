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
            store.SetReturnsDefault(Task.FromResult(true));
            return store.Object;
        }
        

        [Test]
        public async Task BasicFunctionality()
        {
            const int STATES_COUNT = 100000;

            static IEnumerable<string> GetStates() =>
                Enumerable.Range(0, STATES_COUNT).Select(index => $"value_{index}");

            var store = GetStateStore();

            const string KEY = "lel";

            // Can create
            var proxy = store.CreateProxy<string>(KEY);

            // Can set
            var results = await Task.WhenAll(GetStates().Select(proxy.TrySetAsync));

            Assert.IsTrue(results.All(_ => _));

            await proxy.DisposeAsync();
        }
    }
}