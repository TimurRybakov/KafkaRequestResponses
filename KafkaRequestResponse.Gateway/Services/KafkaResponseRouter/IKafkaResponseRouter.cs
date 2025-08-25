namespace KafkaRequestResponse.Gateway.Services.KafkaResponseRouter;

internal interface IKafkaResponseRouter
{
    Task<string> RegisterWaiter(Guid id, TimeSpan timeout);

    void Complete(Guid id, string response);
}
