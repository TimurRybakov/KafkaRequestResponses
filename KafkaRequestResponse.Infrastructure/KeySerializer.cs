using Confluent.Kafka;

namespace KafkaRequestResponse.Infrastructure;

public sealed class KeySerializer : ISerializer<Guid>
{
    public byte[] Serialize(Guid data, SerializationContext context)
    {
        return data.ToByteArray();
    }
}
