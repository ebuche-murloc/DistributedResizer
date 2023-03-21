using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace ResizerASPNET.Rabbit
{
    public class RpcRabbit : IRpcClient
    {
        private IConnection _connection;
        private IModel _channel;
        private string _responseQueueName;
        private string QueueName = "UserRpcResponse";
        private bool _isDisposed;
        private ConcurrentDictionary<string, TaskCompletionSource<string>> _activeTaskQueue = new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public RpcRabbit()
        {
            Initialiaze();
        }

        private void Initialiaze()
        {
            var factory = new ConnectionFactory
            {
                UserName = Environment.GetEnvironmentVariable("RABBIT_USER"),
                Password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD"),
                VirtualHost = "/",
                HostName = Environment.GetEnvironmentVariable("RABBIT_HOST"),
                Port = AmqpTcpEndpoint.UseDefaultPort
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _responseQueueName = _channel.QueueDeclare().QueueName;

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue: _responseQueueName, consumer: consumer, autoAck: true);
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            if (_activeTaskQueue.TryRemove(args.BasicProperties.CorrelationId, out var taskCompletionSource))
            {
                taskCompletionSource.SetResult(message);
            }
        }

        public Task<string> SendAsync(string message)
        {
            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.ReplyTo = _responseQueueName;
            var messageId = Guid.NewGuid().ToString();
            basicProperties.CorrelationId = messageId;
            var taskCompletionSource = new TaskCompletionSource<string>();

            var messageToSend = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "UserRpcQueue", basicProperties: basicProperties, body: messageToSend);
            _activeTaskQueue.TryAdd(messageId, taskCompletionSource);
            return taskCompletionSource.Task;
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _channel.Close();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RpcRabbit()
        {
            Dispose(false);
        }
    }
}