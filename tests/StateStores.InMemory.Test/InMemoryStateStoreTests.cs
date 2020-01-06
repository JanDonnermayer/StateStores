﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StateStores.InMemory;
using StateStores.Test;
using static StateStores.Test.StateChannelTests;

namespace StateStores.InMemory.Test
{

    [TestFixture]
    class InMemoryStateStoreTests
    {
        private static InMemoryStateStore GetStateStore() =>
            new InMemoryStateStore();

        [Test]
        public Task TestBasicFunctionalityAsync() => 
            GetStateStore()
                .TestBasicFunctionalityAsync(key: Guid.NewGuid().ToString()); 

        [Test]
        public Task TestParallelFunctionalityAsync() => 
            GetStateStore()
                .TestParallelFunctionalityAsync(
                    activeChannelCount: 5,
                    stepBlockcount: 100); 

        [Test]
        public Task TestReactiveFunctionalityAsync() => 
            GetStateStore()
                .CreateChannel<int>("key1")
                .TestReactiveFunctionalityAsync(
                    stepCount: 10,
                    activeChannelCount: 5); 

        [Test]
        public Task TestReplayFunctionalityAsync() => 
            GetStateStore()
                .CreateChannel<string>("key2")
                .TestReplayFunctionalityAsync();

    }
}