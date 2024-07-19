using ItemService.EventProcessor;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ItemService.RabbitMqClient
{
    public class RabbitMqSubscrive : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly string _queueName;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private IProcessaEvento _processaEvento;

        // fica escutando a fila do rabbitmq
        public RabbitMqSubscrive(IConfiguration configuration, IProcessaEvento processaEvento)
        {
            _configuration = configuration;
            _connection = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMqHost"],
                Port = int.Parse(_configuration["RabbitMqPort"])
            }.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("trigger", ExchangeType.Fanout); // amqp exchange type
            _queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(_queueName, "trigger", "");
            _processaEvento = processaEvento;
        }

        // roda em segundo plano
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumidor = new EventingBasicConsumer(_channel);

            consumidor.Received += (ModuleHandle, args) =>
            {
                var body = args.Body;
                var mensagem = Encoding.UTF8.GetString(body.ToArray());
                _processaEvento.Processa(mensagem);
            };

            // estou avisando o rabbitmq que eu consumi a mensagem
            _channel.BasicConsume(_queueName, true, consumidor);

            return Task.CompletedTask;
        }
    }
}
