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
        public void Setup() => 
            RedisStateStoreFactory.FlushAllDatabases();

        private IStateStore GetStateStore() => 
            RedisStateStoreFactory.GetStateStore();


        [Test]
        public Task TestBasicFunctionalityAsync() => 
            GetStateStore()
                .TestBasicFunctionalityAsync(key: Guid.NewGuid().ToString()); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            GetStateStore()
                .TestParallelFunctionalityAsync(
                    activeChannelCount: 10,
                    stepBlockcount: 100,
                    passiveChannelCount: 1000); 

        [Test]
        public Task TestReactiveFunctionalityAsync() => 
            GetStateStore()
                .ToChannel<int>("key1")
                .TestReactiveFunctionalityAsync(
                    stepCount: 10,
                    activeChannelCount: 5); 

        [Test]
        public Task TestReplayFunctionalityAsync() => 
            GetStateStore()
                .ToChannel<string>("key2")
                .TestReplayFunctionalityAsync();

    }
    
}