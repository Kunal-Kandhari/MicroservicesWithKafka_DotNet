using Confluent.Kafka;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Services;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using MicroservicesWithKafka.DTO;

namespace ConsumerService.Kafka
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IEnumerable<string> _topics;
        private readonly string _bootstrapServers;
        private readonly BaseService _baseService;

        public KafkaConsumerService(string bootstrapServers, IEnumerable<string> topics, BaseService baseService)
        {
            _bootstrapServers = bootstrapServers;
            _topics = topics;
            _baseService = baseService;
        }

        // Run the consumer in the background
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "multi-topic-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest 
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topics);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);

                        Log.Information($"Consumed message from topic {consumeResult.Topic}: {consumeResult.Message.Value}");

                        await _baseService.HandleEvent<object>(consumeResult.Message.Value);

                        // TODO check await
                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException e)
                    {
                        Log.Information($"Error: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Log.Information("Consumer operation was canceled.");
            }
            finally
            {
                consumer.Close(); 
            }
        }
    }

    public class FundEvent
    {
        public string EventType { get; set; }
        public Fund Fund { get; set; }
    }
}
