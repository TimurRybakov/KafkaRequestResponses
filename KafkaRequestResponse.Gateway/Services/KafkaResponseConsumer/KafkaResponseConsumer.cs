using Confluent.Kafka;
using KafkaRequestResponse.Gateway.Services.KafkaResponseRouter;

namespace KafkaRequestResponse.Gateway.Services.KafkaResponseConsumer;

internal sealed class KafkaResponseConsumer : BackgroundService
{
    private readonly ILogger<KafkaResponseConsumer> _logger;
    private readonly IConsumer<Guid, string> _consumer;

    private readonly IKafkaResponseRouter _router;

    public KafkaResponseConsumer(ILogger<KafkaResponseConsumer> logger, IConsumer<Guid, string> consumer, IKafkaResponseRouter router)
    {
        _logger = logger;
        _consumer = consumer;
        _router = router;
    }

    private void ConsumeLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var cr = _consumer.Consume(token);
                if (cr?.Message == null) continue;
                //if (Guid.TryParse(cr.Message.Key, out var id))
                {
                    _router.Complete(cr.Message.Key, cr.Message.Value);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError("Exception consuming response: {Message}", e.Message);
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("gateway.response");
        return Task.Factory.StartNew(
            () => ConsumeLoop(stoppingToken),
            stoppingToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Current);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        _consumer.Close();
    }
}
