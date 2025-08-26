using System.Diagnostics;
using Confluent.Kafka;
using KafkaRequestResponse.Gateway.Services.KafkaRequestProducer;
using KafkaRequestResponse.Gateway.Services.KafkaResponseConsumer;
using KafkaRequestResponse.Gateway.Services.KafkaResponseRouter;
using KafkaRequestResponse.Infrastructure;

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

builder.Services.AddSingleton<IKafkaRequestProducer, KafkaRequestProducer>();
builder.Services.AddSingleton<IKafkaResponseRouter, KafkaResponseRouter>();
builder.Services.AddHostedService<KafkaResponseConsumer>();

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

app.MapGet(
    "/request",
    static async (
        string message,
        IKafkaRequestProducer producer,
        IKafkaResponseRouter router,
        ActivitySource activitySource,
        CancellationToken cancellationToken) =>
    {
        Guid corellationId = Guid.NewGuid();
        Task<string> resultWaitingTask;
        using (var outer = activitySource.StartActivity("Registering waiter"))
        {
            resultWaitingTask = router.RegisterWaiter(corellationId, TimeSpan.FromSeconds(10));
        }
        using (var outer = activitySource.StartActivity("Posting request"))
        {
            await producer.ProduceRequestAsync(corellationId, message);
        }

        using (var outer = activitySource.StartActivity("Waiting response"))
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var completed = await Task.WhenAny(resultWaitingTask, Task.Delay(Timeout.Infinite, linked.Token));
            if (completed == resultWaitingTask)
                return Results.Ok(resultWaitingTask.Result);
        }

        return Results.StatusCode(504); // таймаут
    })
.WithName("SendRequest")
.WithOpenApi();

app.Run();
