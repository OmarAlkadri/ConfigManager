using System;
using System.Text;
using System.Threading.Tasks;
using ConfigManager.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace ConfigManager.Infrastructure.Messaging
{
    public class RabbitMqMessageBroker : IMessageBroker
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly string _exchangeName = "config_updates";

        public async Task InitializeAsync()
        {
            // Initialize the connection asynchronously
            _connection = await RabbitMqConnectionManager.GetConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
        }

        public async Task PublishConfigurationUpdateAsync(string settingName)
        {
            var message = Encoding.UTF8.GetBytes(settingName);
            await _channel.BasicPublishAsync(exchange: _exchangeName, routingKey: string.Empty, body: message);
        }

    }
}
