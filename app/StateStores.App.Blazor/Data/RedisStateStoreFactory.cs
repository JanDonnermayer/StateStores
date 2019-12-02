using System;
using StateStores;
using StateStores.Redis;

namespace StateStores.App.Blazor.Data
{
    internal static class RedisStateStoreFactory
    {
        private const string PZS = "pzs";
        private const string HOME_LOCAL = "desktop-qvvvipl";

        static string GetServer() =>
            Environment.UserDomainName.ToLower() switch
            {
                PZS => @"linux-genet01:7001",
                HOME_LOCAL => @"localhost:32769",
                _ => throw new Exception("Unknown environment!")
            };

        public static RedisStateStore GetStateStore() =>
            new RedisStateStore(GetServer());
    }
}
