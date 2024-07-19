using RabbitMQ.Client;
using RestauranteService.Dtos;
using System.Text;
using System.Text.Json;

namespace RestauranteService.RabbitMqClient
{
    public class RabbitMqClient : IRabbitMqClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _connection = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMqHost"],
                Port = int.Parse(_configuration["RabbitMqPort"])
            }.CreateConnection();

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("trigger", ExchangeType.Fanout); // amqp exchange type
        }

        public void PublicaRestaurante(RestauranteReadDto restaurante)
        {
            string mensagem = JsonSerializer.Serialize(restaurante);
            var body = Encoding.UTF8.GetBytes(mensagem);

            // publicando mensagem na fila do rabbitmq
            _channel.BasicPublish("trigger", "", null, body);
        }
    }
}
