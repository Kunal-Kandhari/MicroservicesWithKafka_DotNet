using MicroservicesWithKafka.Kafka;
using MicroservicesWithKafka.Middleware;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Repository;
using MicroservicesWithKafka.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<IServiceFactory, ServiceFactory>();
builder.Services.AddSingleton<IFundRepository, FundRepository>();
builder.Services.AddSingleton<IBaseService<Fund>, FundService>();

builder.Services.AddSingleton<KafkaProducer>(provider =>
                                    new KafkaProducer(builder.Configuration.GetSection("Kafka:BootstrapServers").Value));
builder.Services.AddSingleton<BaseService>();

builder.Services.Configure<FundsDatabaseSettings>(builder.Configuration.GetSection("FundsDatabase"));


builder.Services.AddSingleton<IEncryptionService>(provider =>
{
    string encryptionKey = builder.Configuration.GetSection("Middleware:encryptionKey").Value; // 256-bit key
    string encryptionIv = builder.Configuration.GetSection("Middleware:encryptionIv").Value; // 128-bit IV
    return new EncryptionService(encryptionKey, encryptionIv);
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.UseQueryDecryption();

app.Run();


