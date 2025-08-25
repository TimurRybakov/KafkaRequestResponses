using Confluent.Kafka;
using KafkaRequestResponse.Infrastructure;
using KafkaRequestResponse.Processor.Services.KafkaRequestConsumer;
using KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

var builder = WebApplication.CreateBuilder(args);

builder.AddKafkaProducer<string, string>("kafka");
builder.AddKafkaConsumer<string, string>("kafka", settings =>
{
    settings.Config.GroupId = "gateway";
    settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
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
