using Confluent.Kafka;
using Confluent.Kafka.Admin;
using KafkaRequestResponse.Gateway;
using KafkaRequestResponse.Gateway.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddKafkaProducer<string, string>("kafka");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaSetupService, KafkaSetupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

var kafkaSetupService = app.Services.GetRequiredService<IKafkaSetupService>();
await kafkaSetupService.Configure();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/request", async (string message, IProducer<string, string> producer) =>
{
    var corellationId = Guid.NewGuid();
    await producer.ProduceAsync("request", new Message<string, string>()
    {
        Key = corellationId.ToString(),
        Value = message
    });


    return 1;
})
.WithName("SendRequest")
.WithOpenApi();

app.Run();