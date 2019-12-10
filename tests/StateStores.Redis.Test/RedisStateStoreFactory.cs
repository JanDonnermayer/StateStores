using System;
using NUnit.Framework;
using StateStores.Redis;
using StackExchange.Redis;

namespace StateStores.Redis.Test
{
    internal static class RedisStateStoreFactory
    {
        private const string PZS = "pzs";
        private const string HOME_LOCAL = "desktop-qvvvipl";

        static string GetServer() =>
            Environment.UserDomainName.ToLower() switch
            {
                PZS => @"linux-genet01:7001",
                HOME_LOCAL => @"localhost:6379",
                _ => throw new InconclusiveException("Unknown environment!")
            };

        public static RedisStateStore GetStateStore() =>
            new RedisStateStore(GetServer());

        public static void FlushAllDatabases()
        {
            var server = GetServer();
            using var _redis = ConnectionMultiplexer.Connect(server + ",allowAdmin=true");
            _redis.GetServer(server).FlushAllDatabases();
        }
    }
}