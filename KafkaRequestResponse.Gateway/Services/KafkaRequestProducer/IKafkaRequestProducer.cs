namespace KafkaRequestResponse.Gateway.Services.KafkaRequestProducer;

internal interface IKafkaRequestProducer
{
    Task ProduceRequestAsync(Guid correlationId, string payload);
}
