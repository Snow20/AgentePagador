using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Agent.Core.Interfaces;

namespace Agent.Infrastructure.Repositories
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        public RabbitMqPublisher(string hostname, string username, string password)
        {
            _hostname = hostname;
            _username = username;
            _password = password;
        }

        public async Task PublishAsync(string routingKey, object message)
        {
            var factory = new ConnectionFactory()
            {   
                HostName = _host,
                UserName = _user,
                Password = _pass,
                VirtualHost = Environment.GetEnvironmentVariable("MQ_VHOST") ?? _user
            };
            
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: routingKey, 
                durable: true, 
                exclusive: false, 
                autoDelete: false, 
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(
                exchange: string.Empty, 
                routingKey: routingKey, 
                body: body);
        }
    }
}