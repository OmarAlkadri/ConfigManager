using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace ConfigManager.Infrastructure.Messaging
{
    public static class RabbitMqConnectionManager
    {
        private static IConnection? _connection;

        public static async Task<IConnection> GetConnectionAsync(string rabbitMqUrl)
        {
            Console.WriteLine($"rabbitMqUrl: {rabbitMqUrl}");

            if (string.IsNullOrEmpty(rabbitMqUrl))
            {
                throw new ArgumentNullException(nameof(rabbitMqUrl), "RabbitMQ URL cannot be null or empty.");
            }

            Console.WriteLine($"[RabbitMqConnectionManager] Trying to connect to: {rabbitMqUrl}");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqUrl),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = new Uri(rabbitMqUrl).Host,
                    AcceptablePolicyErrors = System.Net.Security.SslPolicyErrors.None
                }
            };
            return _connection ??= await factory.CreateConnectionAsync();
        }
    }
}
