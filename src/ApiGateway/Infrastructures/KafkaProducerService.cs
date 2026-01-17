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
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IProducer<Null, string> producer, KafkaSettings settings, ILogger<KafkaProducerService> logger)
        {
            _producer = producer;
            _settings = settings;
            _logger = logger;
        }

        public async Task SendAsync(ClaimRequest request, CancellationToken token)
        {
            try
            {
                _logger.LogInformation($"Queuing claim request: {request.TransactionId}.");

                var json = JsonSerializer.Serialize(request);
                await _producer.ProduceAsync(
                    _settings.RequestTopic,
                    new Message<Null, string> { Value = json },
                    token);
                _logger.LogInformation($"Queued claim request: {request.TransactionId}.");

            }
            catch(Exception ex)
            {
                _logger.LogError($"Queue claim request failed: {request.TransactionId}.");
                throw;
            }
        }
    }

}