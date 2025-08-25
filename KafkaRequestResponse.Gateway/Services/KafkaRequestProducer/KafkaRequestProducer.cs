using Confluent.Kafka;

namespace KafkaRequestResponse.Gateway.Services.KafkaRequestProducer;

internal sealed class KafkaRequestProducer : IKafkaRequestProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaRequestProducer(IProducer<string, string> producer) => _producer = producer;

    public Task ProduceRequestAsync(Guid correlationId, string payload)
    {
        var msg = new Message<string, string> { Key = correlationId.ToString(), Value = payload };
        return _producer.ProduceAsync("gateway.request", msg);
    }
}
