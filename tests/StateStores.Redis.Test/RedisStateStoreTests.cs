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
                parallelWorkersCount: 20,
                transactionsBlockCount: 100
            );
        }
    }
}