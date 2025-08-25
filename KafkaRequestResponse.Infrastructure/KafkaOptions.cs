using Confluent.Kafka.Admin;

namespace KafkaRequestResponse.Infrastructure;

public class KafkaOptions
{
    public string? BootstrapServers { get; set; }

    public List<TopicSpecification> Topics { get; set; } = [];
}
