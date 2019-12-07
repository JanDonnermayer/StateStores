using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;
using StateStores.Test;

namespace StateStores.Redis.Test
{
    [TestFixture]
    public class RedisStateStoreTests
    {

        [SetUp]
        public void Setup() => RedisStateStoreFactory.FlushAllDatabases();


        [Test]
        public async Task TestParallelFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store.TestParallelFunctionalityAsync(
                parallelWorkersCount: 30,
                transactionsBlockCount: 100
            );
        }

        [Test]
        public async Task TestBasicFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store.TestBasicFunctionalityAsync(Guid.NewGuid().ToString());
        }
    }
}