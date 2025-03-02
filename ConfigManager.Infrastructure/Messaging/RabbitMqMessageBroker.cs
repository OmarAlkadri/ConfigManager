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
        private IConnection? _connection = null;
        private IChannel? _channel = null;
        private readonly string _exchangeName = "config_updates";

        public async Task InitializeAsync()
        {
            try
            {
                _connection = await RabbitMqConnectionManager.GetConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);
            }
            catch (BrokerUnreachableException ex)
            {
                throw new Exception("Failed to connect to RabbitMQ. Ensure RabbitMQ is running and accessible.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while initializing the message broker.", ex);
            }
        }

        public async Task PublishConfigurationUpdateAsync(string settingName)
        {
            if (_channel == null || _channel.IsClosed)
            {
                await InitializeAsync();
            }

            if (_channel == null)
            {
                throw new InvalidOperationException("Message broker not initialized.");
            }

            var message = Encoding.UTF8.GetBytes(settingName);
            await _channel.BasicPublishAsync(exchange: _exchangeName, routingKey: string.Empty, body: message);
            await Task.CompletedTask;
        }
    }
}
