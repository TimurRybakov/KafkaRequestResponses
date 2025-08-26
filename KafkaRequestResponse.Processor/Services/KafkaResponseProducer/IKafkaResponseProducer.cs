namespace KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

internal interface IKafkaResponseProducer
{
    Task ProduceResponseAsync(Guid correlationId, string payload);
}
