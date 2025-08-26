using Confluent.Kafka;

namespace KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

internal sealed class KafkaResponseProducer : IKafkaResponseProducer
{
    private readonly IProducer<Guid, string> _producer;

    public KafkaResponseProducer(IProducer<Guid, string> producer) => _producer = producer;

    public Task ProduceResponseAsync(Guid correlationId, string payload)
    {
        var msg = new Message<Guid, string> { Key = correlationId, Value = payload };
        return _producer.ProduceAsync("gateway.response", msg);
    }
}
