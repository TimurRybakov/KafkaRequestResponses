namespace KafkaRequestResponse.Processor.Services.KafkaResponseProducer;

internal interface IKafkaResponseProducer
{
    Task ProduceResponseAsync(string correlationId, string payload);
}
