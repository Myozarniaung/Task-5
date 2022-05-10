using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using orderconsumerapi.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace orderconsumerapi.Consumer
{
    public class orderconsumer: BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private readonly IConfiguration _env;
        private IModel _channel;
        private readonly orderconsumerDBContext _context;

        public orderconsumer(orderconsumerDBContext context, IConfiguration env, ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<orderconsumer>();
            this._env = env;
            _context = context;
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory
            {

                // HostName = "localhost" , 
                // Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))

            };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare("orders", false, false, false, null);
            // _channel.QueueBind("demo.queue.log", "demo.exchange", "demo.queue.*", null);
            // _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());

                // handle the received message  
                HandleMessageAsync(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume("orders", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string content)
        {
            // we just print this message   
            _logger.LogInformation($"consumer received {content}");
            try
            {
                var order = JsonConvert.DeserializeObject<orderconsumer>(content);

                if (order != null)
                {
                    
                    var result = await insertorder(order);

                   
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error in DeserializeObject:" + e.Message);
                if (e.InnerException != null) Console.WriteLine(e.InnerException.Message);
            }

        }

        


        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private async Task<orderconsumer> insertorder(orderconsumer order)
        {
            //order.Status =EOrderStatus.INITIATED.ToString();

            // try
            // {
            //     _context.orderconsumers.Add(order);
            //     await _context.SaveChangesAsync();
            // }
            // catch (Exception e)
            // {
            //     _logger.LogError(e.Message);
            //     if (e.InnerException != null)
            //         _logger.LogError(e.InnerException.Message);

            //     throw;
            // }

            //order.Status = "Success";

            return order;
        }
    }
}
