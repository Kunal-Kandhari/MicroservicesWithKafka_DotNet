using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicroservicesWithKafka.Services;
using ConsumerService.Kafka;
using Serilog;
using MicroservicesWithKafka.Repository;
using MicroservicesWithKafka.Kafka;
using MicroservicesWithKafka.Models;
using Microsoft.Extensions.Configuration;

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

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services.Configure<FundsDatabaseSettings>(configuration.GetSection("FundsDatabase"));

                    services.AddSingleton<IServiceFactory, ServiceFactory>();
                    services.AddSingleton<IFundRepository, FundRepository>();
                    services.AddSingleton<IBaseService<Fund>, FundService>();

                    services.AddSingleton<BaseService>();

                    services.AddSingleton<KafkaProducer>(provider =>
                                    new KafkaProducer(configuration.GetSection("Kafka:BootstrapServers").Value));

                    // Register Kafka Consumer as a background service
                    services.AddHostedService<KafkaConsumerService>(provider =>
                        new KafkaConsumerService(configuration.GetSection("Kafka:BootstrapServers").Value,
                                                    configuration.GetSection("Kafka:Topics").Get<string[]>() ?? new[] { "fund-events" },
                                                    provider.GetRequiredService<BaseService>()));
                });
    }

}
