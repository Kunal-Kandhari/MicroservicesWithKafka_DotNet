using MicroservicesWithKafka.Kafka;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Repository;
using MicroservicesWithKafka.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
                                    new KafkaProducer("localhost:9092"));
builder.Services.AddSingleton<BaseService>();


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();


