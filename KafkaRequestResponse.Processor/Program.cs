using Confluent.Kafka;
using KafkaRequestResponse.Infrastructure;
using KafkaRequestResponse.Processor.Services.KafkaRequestConsumer;
using KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKafkaProducer<Guid, string>(
    "kafka",
    configureBuilder: static configureBuilder =>
    {
        configureBuilder.SetKeySerializer(new KeySerializer());
    });
builder.AddKafkaConsumer<Guid, string>(
    "kafka",
    static configureSettings =>
    {
        configureSettings.Config.GroupId = "gateway";
        configureSettings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
    },
    static configureBuilder =>
    {
        configureBuilder.SetKeyDeserializer(new KeyDeserializer());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaSetupService, KafkaSetupService>();

builder.Services.AddSingleton<IKafkaResponseProducer, KafkaResponseProducer>();
builder.Services.AddHostedService<KafkaRequestConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
