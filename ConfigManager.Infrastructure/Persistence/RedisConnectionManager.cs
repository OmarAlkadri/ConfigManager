using StackExchange.Redis;
using System;

namespace ConfigManager.Infrastructure.Persistence
{
    public sealed class RedisConnectionManager
    {
        private static readonly Lazy<ConnectionMultiplexer> _lazyConnection = 
            new(() => ConnectionMultiplexer.Connect("localhost:6379"));

        public static ConnectionMultiplexer Connection => _lazyConnection.Value;

        private RedisConnectionManager() { }
    }
}
