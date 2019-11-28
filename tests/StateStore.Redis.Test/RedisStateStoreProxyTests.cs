using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;
using StateStores.Test;

namespace StateStores.Redis.Test
{
    [TestFixture]
    public class RedisStateStoreProxyTests
    {
        [SetUp]
        public void Setup() => RedisStateStoreFactory.FlushAllDatabases();

        [Test]
        public async Task TestReactiveFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store
                .CreateProxy<int>(key: Guid.NewGuid().ToString())
                .TestReactiveFunctionalityAsync(stateCount: 1000);
        }
    }
}