using Confluent.Kafka;

namespace KafkaRequestResponse.Infrastructure;

public sealed class KeyDeserializer : IDeserializer<Guid>
{
    public Guid Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return new Guid(data);
    }
}

