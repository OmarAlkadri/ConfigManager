using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace ConfigManager.Infrastructure.Messaging
{
    public static class RabbitMqConnectionManager
    {
        private static readonly ConnectionFactory _factory = new ConnectionFactory
        {
            Uri = new Uri("amqp://guest:guest@localhost:5672/"),
        };

        private static IConnection? _connection;

        public static async Task<IConnection> GetConnectionAsync()
        {
            return _connection ??= await _factory.CreateConnectionAsync();
        }
    }
}
