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
                    parallelHandlersCount: 5,
                    stateBlockCount: 100); 

        [Test]
        public Task TestReactiveFunctionalityAsync() => 
            GetStateStore()
                .CreateChannel<int>("key1")
                .TestReactiveFunctionalityAsync(
                    stateCount: 10,
                    parallelHandlersCount: 5); 

        [Test]
        public Task TestReplayFunctionalityAsync() => 
            GetStateStore()
                .CreateChannel<string>("key2")
                .TestReplayFunctionalityAsync();

    }
    
}