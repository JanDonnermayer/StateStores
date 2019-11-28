using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;

namespace StateStores.Test
{
    [TestFixture]
    public class RedisStateStoreProxyTests : StateStoreProxyTestBase
    {
        //const string SERVER = "linux-genet01:7001";
        const string SERVER = @"localhost:32768";

        private static RedisStateStore GetStateStore() =>
            new RedisStateStore(SERVER);

        private static void FlushAllDatabases(string server)
        {
            var _redis = ConnectionMultiplexer.Connect(server + ",allowAdmin=true");
            _redis.GetServer(server).FlushAllDatabases();
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
        public Task BasicFunctionality() =>
            BasicFunctionality(GetStateStore());

        [Test]
        public Task ReactiveFunctionality() =>
            ReactiveFunctionality(GetStateStore());
    }
}