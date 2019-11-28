using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;

namespace StateStores.Test
{
    [TestFixture]
    public class RedisStateStoreTests 
    {
        //const string SERVER = "linux-genet01:7001";
        const string SERVER = @"localhost:32769";

        private static RedisStateStore GetStateStore() =>
            new RedisStateStore(SERVER);

        private static void FlushAllDatabases(string server)
        {
            var _redis = ConnectionMultiplexer.Connect(server + ",allowAdmin=true");
            _redis.GetServer(server).FlushAllDatabases();
            _redis.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            try
            {
                FlushAllDatabases(SERVER);
            }
            catch (Exception)
            {
                Assert.Inconclusive($"Cannot access redis-server at: {SERVER}");
            }
        }

        [Test]
        public async Task TestBasicFunctionalityAsync()
        {
            using var store = GetStateStore();
            await store.TestBasicFunctionalityAsync();
        }

        [Test]
        public async Task TestParallelFunctionalityAsync()
        {
            using var store = GetStateStore();
            await store.TestParallelFunctionalityAsync();
        }
    }
}