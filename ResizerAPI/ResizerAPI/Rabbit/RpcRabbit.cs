using ResizerAPI.ImageResize;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ResizerAPI.Rabbit
{
    public class RpcRabbit : IHostedService , IRpcClient
    {
        private readonly IResizer _resizer;
        private const string QueueName = "UserRpcQueue";
        private IConnection _connection;
        private IModel _channel;
        private bool _isDisposed;

        public RpcRabbit(IResizer resizer)
        {
            _resizer = resizer;
        }

        public void InitializeAndRun()
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

            _channel.QueueDeclare(queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _channel.BasicQos(0, 1, false);

            var consumer = new EventingBasicConsumer(_channel);

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            consumer.Received += Consumer_Received;
        }

        private void Consumer_Received(object? sender, BasicDeliverEventArgs ea)
        {

            LogMessage?.Invoke("Recieved Request from client..");
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
            replyProps.ReplyTo = ea.BasicProperties.ReplyTo;

            var imageInfo = TryDeserialize(message);

            var resizeStatusInfo = _resizer.Resize(imageInfo.Height, imageInfo.Width, imageInfo.Filename);
            string json = JsonConvert.SerializeObject(resizeStatusInfo);
            var responseBody = Encoding.UTF8.GetBytes(json);

            _channel.BasicPublish(exchange: "",
                                routingKey: replyProps.ReplyTo,
                                basicProperties: replyProps,
                                body: responseBody);
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);


        }

        private ImageInfo? TryDeserialize(string text)
        {
            try
            {
                return JsonConvert.DeserializeObject<ImageInfo>(text);
            }
            catch { }
            return null;
        }

        public IEnumerable<string> GenerateUserNames(int count)
        {
            return Enumerable.Range(1, count).Select(x => $"UserName {x}");
        }



        public delegate void LogMessageHandler(string message);

        public event LogMessageHandler LogMessage;

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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                InitializeAndRun();
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(100);
                }
            }, cancellationToken);
        }

        ~RpcRabbit()
        {
            Dispose(false);
        }
    }
}