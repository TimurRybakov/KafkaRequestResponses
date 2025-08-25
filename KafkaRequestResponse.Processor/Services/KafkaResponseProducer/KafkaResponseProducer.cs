using Confluent.Kafka;

namespace KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

internal sealed class KafkaResponseProducer : IKafkaResponseProducer
{
    private readonly IProducer<string, string> _producer;

    public KafkaResponseProducer(IProducer<string, string> producer) => _producer = producer;

    public Task ProduceResponseAsync(string correlationId, string payload)
    {
        var msg = new Message<string, string> { Key = correlationId, Value = payload };
        return _producer.ProduceAsync("gateway.response", msg);
    }
}
