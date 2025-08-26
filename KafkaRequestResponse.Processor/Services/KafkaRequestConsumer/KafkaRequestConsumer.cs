using Confluent.Kafka;
using KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

namespace KafkaRequestResponse.Processor.Services.KafkaRequestConsumer;

internal sealed class KafkaRequestConsumer : BackgroundService
{
    private readonly ILogger<KafkaRequestConsumer> _logger;
    private readonly IConsumer<Guid, string> _consumer;
    private readonly IKafkaResponseProducer _producer;

    public KafkaRequestConsumer(ILogger<KafkaRequestConsumer> logger, IConsumer<Guid, string> consumer, IKafkaResponseProducer producer)
    {
        _logger = logger;
        _consumer = consumer;
        _producer = producer;
    }

    private async Task ConsumeLoop(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var cr = _consumer.Consume(token);
                if (cr?.Message == null) continue;
                await _producer.ProduceResponseAsync(
                    cr.Message.Key, $"Message {cr.Message.Key} was processed by {Environment.GetEnvironmentVariable("service.name")}: {cr.Message.Value}");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError("Exception consuming request: {Message}", e.Message);
        }
        finally
        {
            _consumer.Close();
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("gateway.request");
        return Task.Factory.StartNew(
            async () => await ConsumeLoop(stoppingToken),
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
