namespace ApiGateWayTests
{
    using ApiGateway;
    using ApiGateway.ConfigurationSettings;
    using Microsoft.Extensions.Logging;
    using Moq;
    using SharedContracts;
    using StackExchange.Redis;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using Xunit;

    public class EgressBridgeServiceTests
    {
        private readonly Mock<IConnectionMultiplexer> _mockRedis = new();
        private readonly Mock<ISubscriber> _mockSub = new();
        private readonly Mock<ILogger<EgressBridgeService>> _mockLogger = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<ClaimResponse>> _map = new();
        private readonly RedisSettings _settings = new() { ResponseChannel = "test-channel" };

        public EgressBridgeServiceTests()
        {
            _mockRedis.Setup(x => x.GetSubscriber(null)).Returns(_mockSub.Object);
        }

        [Fact]
        public async Task MessageReceived_ResolvesAndRemovesFromMap()
        {
            // 1. Arrange
            var transactionId = "TX-123";
            var tcs = new TaskCompletionSource<ClaimResponse>();
            _map.TryAdd(transactionId, tcs);

            var service = new EgressBridgeService(_mockRedis.Object, _map, _settings, _mockLogger.Object);

            // Capture the callback passed to SubscribeAsync so we can trigger it manually
            Action<RedisChannel, RedisValue> messageHandler = null!;
            _mockSub.Setup(x => x.SubscribeAsync(It.IsAny<RedisChannel>(), It.IsAny<Action<RedisChannel, RedisValue>>(), It.IsAny<CommandFlags>()))
                    .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((ch, handler, flags) => messageHandler = handler)
                    .Returns(Task.CompletedTask);

            // 2. Act
            using var cts = new CancellationTokenSource();
            var executeTask = service.StartAsync(cts.Token);

            // Simulate a Redis message arriving
            var response = new ClaimResponse { TransactionId = transactionId, Status = "Success" };
            var json = JsonSerializer.Serialize(response);

            messageHandler(RedisChannel.Literal(_settings.ResponseChannel), json);

            // 3. Assert
            var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
            Assert.Equal("Success", result.Status);
            Assert.Empty(_map); // Verify it was removed from the dictionary

            await service.StopAsync(cts.Token);
        }
    }
}
