using Confluent.Kafka;
using KafkaRequestResponse.Gateway.Services.KafkaRequestProducer;
using KafkaRequestResponse.Gateway.Services.KafkaResponseConsumer;
using KafkaRequestResponse.Gateway.Services.KafkaResponseRouter;
using KafkaRequestResponse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
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
        CancellationToken cancellationToken) =>
    {
        var corellationId = Guid.NewGuid();

        var waitTask = router.RegisterWaiter(corellationId, TimeSpan.FromSeconds(10));
        await producer.ProduceRequestAsync(corellationId, message);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completed = await Task.WhenAny(waitTask, Task.Delay(Timeout.Infinite, linked.Token));
        if (completed == waitTask)
            return Results.Ok(waitTask.Result);

        return Results.StatusCode(504); // таймаут
    })
.WithName("SendRequest")
.WithOpenApi();

app.Run();
