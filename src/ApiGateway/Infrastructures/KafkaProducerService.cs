using ApiGateway.Abstractions;
using ApiGateway.ConfigurationSettings;
using Confluent.Kafka;
using SharedContracts;
using System.Text.Json;

namespace ApiGateway.Infrastructures
{
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _settings;

        public KafkaProducerService(IProducer<Null, string> producer, KafkaSettings settings)
        {
            _producer = producer;
            _settings = settings;
        }

        public async Task SendAsync(ClaimRequest request, CancellationToken token)
        {
            var json = JsonSerializer.Serialize(request);
            await _producer.ProduceAsync(
                _settings.RequestTopic,
                new Message<Null, string> { Value = json },
                token);
        }
    }

}