using Confluent.Kafka.Admin;

namespace KafkaRequestResponse.Gateway;

public class KafkaOptions
{
    public string? BootstrapServers { get; set; }

    public List<TopicSpecification> Topics { get; set; } = [];
}
