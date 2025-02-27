using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicroservicesWithKafka.Services;
using ConsumerService.Kafka;
using Serilog;
using MicroservicesWithKafka.Repository;
using MicroservicesWithKafka.Kafka;

namespace ConsumerService
{
    class Program
    {
        static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
                            .CreateLogger();

            var builder = CreateHostBuilder(args);

            builder.UseSerilog();

            var host = builder.Build();

            //host.UseSerilogRequestLogging();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IFundRepository, FundRepository>();
                    //services.AddSingleton<IFundService, FundService>();

                    services.AddSingleton<FundService>();
                    services.AddSingleton<BaseService>();

                    services.AddSingleton<KafkaProducer>(provider =>
                                    new KafkaProducer("localhost:9092"));

                    // Register Kafka Consumer as a background service
                    services.AddHostedService<KafkaConsumerService>(provider =>
                        new KafkaConsumerService("localhost:9092", provider.GetRequiredService<BaseService>()));
                });
    }

}
