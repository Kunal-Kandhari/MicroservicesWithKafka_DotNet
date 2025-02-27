using MicroservicesWithKafka.Kafka;
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

// Add services to the container.

builder.Services.AddControllers();


builder.Services.AddSingleton<IFundRepository, FundRepository>();
builder.Services.AddSingleton<KafkaProducer>(provider =>
                                    new KafkaProducer("localhost:9092"));
//builder.Services.AddScoped<IFundService, FundService>();


builder.Services.AddSingleton<FundService>();
builder.Services.AddSingleton<BaseService>();


// Register Kafka Consumer as a background service
//builder.Services.AddHostedService<KafkaConsumerService>(provider =>
//    new KafkaConsumerService("localhost:9092", provider.GetRequiredService<IFundRepository>()));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

//builder.Services.AddHostedService<KafkaConsumer>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();



//var serviceProvider = new ServiceCollection()
//            .AddSingleton<IFundRepository, FundRepository>()
//            .BuildServiceProvider();

//// Get the fund repository from DI container
//var fundRepository = serviceProvider.GetService<IFundRepository>();

//var kafkaConsumer = new KafkaConsumer("localhost:9092", fundRepository);

//// Start consuming messages
//var cts = new CancellationTokenSource();
//Console.CancelKeyPress += (_, e) =>
//{
//    e.Cancel = true;
//    cts.Cancel();
//};

//await kafkaConsumer.StartConsuming(cts.Token);


app.Run();


