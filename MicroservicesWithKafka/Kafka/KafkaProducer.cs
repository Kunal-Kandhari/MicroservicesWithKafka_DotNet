using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace MicroservicesWithKafka.Kafka
{
    public class KafkaProducer
    {
        //private readonly string _topic = "fund-events";
        //private readonly string _bootstrapServers;

        //public KafkaProducer(IConfiguration config)
        //{
        //    _bootstrapServers = config["Kafka:BootstrapServers"];
        //}

        //public async Task PublishMessage(string key, string message)
        //{
        //    var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

        //    using var producer = new ProducerBuilder<string, string>(config).Build();
        //    await producer.ProduceAsync(_topic, new Message<string, string> { Key = key, Value = message });
        //}

        private readonly IProducer<string, string> _producer;

        public KafkaProducer(string bootstrapServers)
        {
            var config = new ProducerConfig { BootstrapServers = bootstrapServers };
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishMessage(string topic, string key, object value)
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = JsonConvert.SerializeObject(value)
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, message);
                Console.WriteLine($"Message delivered to {deliveryResult.TopicPartitionOffset}");
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"Error producing message: {e.Error.Reason}");
            }
        }
    }
}
