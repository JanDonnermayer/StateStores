using System.Threading.Tasks;
using NUnit.Framework;

namespace StateStores.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var store = new StateStores.InMemoryStateStore();
            
            var key = "lel";
            var token1 = "lol";
            var token2 = "saaas";

            Assert.IsTrue(await store.TrySetAsync(key, token1, 5));

            Assert.IsTrue(await store.TrySetAsync(key, token1, 6));

            Assert.IsFalse(await store.TrySetAsync(key, token2, 6));
            
            Assert.IsFalse(await store.TryRemoveAsync<int>(key, token2));

            Assert.IsTrue(await store.TryRemoveAsync<int>(key, token1));

            Assert.IsFalse(await store.TryRemoveAsync<int>(key, token1));
        }
    }
}