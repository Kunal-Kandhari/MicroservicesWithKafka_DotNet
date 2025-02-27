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
        private readonly string _topic = "fund-events"; 
        private readonly string _bootstrapServers;
        private readonly BaseService _baseService;

        public KafkaConsumerService(string bootstrapServers, BaseService baseService)
        {
            _bootstrapServers = bootstrapServers;
            _baseService = baseService;
        }

        // Run the consumer in the background
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "fund-events-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest 
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();
            consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        //var consumeResult = consumer.Consume(stoppingToken);

                        //Log.Information($"Consumed message: {consumeResult.Message.Value}");

                        //var fund = JsonConvert.DeserializeObject<Fund>(consumeResult.Message.Value);

                        //var fundEvent = new FundEvent();

                        //fundEvent.Fund = fund;

                        //fundEvent.EventType = consumeResult.Message.Key;

                        //Log.Information($"Consumed message with Key: {consumeResult.Message.Key}, Event Type: {fundEvent.EventType}");

                        //await ProcessMessage(fundEvent);

                        //// TODO check await
                        //consumer.Commit(consumeResult);


                        var consumeResult = consumer.Consume(stoppingToken);

                        Log.Information($"Consumed message: {consumeResult.Message.Value}");

                        var eventDTO = JsonConvert.DeserializeObject<GenericEventDTO<object>>(consumeResult.Message.Value);

                        // Log the consumed message
                        Log.Information($"Consumed message with EventType: {eventDTO.EventType}");

                        // Call the generic service with the appropriate entity
                        await _baseService.HandleEvent(eventDTO);

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

        //private async Task ProcessMessage(FundEvent fundEvent)
        //{
        //    switch (fundEvent.EventType)
        //    {
        //        case "CREATE":
        //            await _fundService.AddFund(fundEvent.Fund);
        //            Log.Information($"Created fund with ID: {fundEvent.Fund.FundId}");
        //            break;
        //        case "UPDATE":
        //            await _fundService.UpdateFund(fundEvent.Fund);
        //            Log.Information($"Updated fund with ID: {fundEvent.Fund.FundId}");
        //            break;
        //        case "DELETE":
        //            await _fundService.DeleteFund(fundEvent.Fund.FundId);
        //            Log.Information($"Deleted fund with ID: {fundEvent.Fund.FundId}");
        //            break;
        //        default:
        //            Log.Warning("Unknown event type received.");
        //            break;
        //    }
        //}
    }

    public class FundEvent
    {
        public string EventType { get; set; }
        public Fund Fund { get; set; }
    }
}
