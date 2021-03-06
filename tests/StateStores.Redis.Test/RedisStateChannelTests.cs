﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;
using StateStores.Redis;
using StateStores.Test;
using static StateStores.Test.StateChannelTests;

namespace StateStores.Redis.Test
{
    [TestFixture]
    public class RedisStateChannelTests
    {
        [SetUp]
        public void Setup() => RedisStateStoreFactory.FlushAllDatabases();


        [Test]
        public async Task TestBasicFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store
                .ToChannel<string>(key: Guid.NewGuid().ToString())
                .TestBasicFunctionalityAsync();
        }

        [Test]
        public async Task TestReactiveFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store
                .ToChannel<int>(key: Guid.NewGuid().ToString())
                .TestReactiveFunctionalityAsync(stepCount: 1000);
        }

        [Test]
        public async Task TestReplayFunctionalityAsync()
        {
            using var store = RedisStateStoreFactory.GetStateStore();
            await store
                .ToChannel<string>(key: Guid.NewGuid().ToString())
                .TestReplayFunctionalityAsync();
        }
    }
}