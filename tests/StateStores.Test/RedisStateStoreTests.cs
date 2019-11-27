﻿using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;

namespace StateStores.Test
{
    [TestFixture]
    public class RedisStateStoreTests : StateStoreTests
    {
        const string SERVER = "linux-genet01:7001";

        private static RedisStateStore GetStateStore() =>
            new RedisStateStore(SERVER);

        private static void FlushAllDatabases(string server)
        {
            var _redis = ConnectionMultiplexer.Connect(server + ",allowAdmin=true");
            _redis.GetServer(server).FlushAllDatabases();            
        }

        [SetUp]
        public void Setup() =>
            FlushAllDatabases(SERVER);

        [Test]
        public Task BasicFunctionality() =>
            TestBasicFunctionality(GetStateStore());

        [Test]
        public Task ParallelFunctionality() =>
            TestParallelFunctionality(GetStateStore());
    }
}