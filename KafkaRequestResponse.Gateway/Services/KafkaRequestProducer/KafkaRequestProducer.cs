using Confluent.Kafka;

namespace KafkaRequestResponse.Gateway.Services.KafkaRequestProducer;

internal sealed class KafkaRequestProducer : IKafkaRequestProducer
{
    private readonly IProducer<Guid, string> _producer;

    public KafkaRequestProducer(IProducer<Guid, string> producer) => _producer = producer;

    public Task ProduceRequestAsync(Guid correlationId, string payload)
    {
        var msg = new Message<Guid, string> { Key = correlationId, Value = payload };
        return _producer.ProduceAsync("gateway.request", msg);
    }
}
