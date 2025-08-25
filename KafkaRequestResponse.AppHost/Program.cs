using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
var kafka = builder
    .AddKafka("kafka", port: 9092)
    .WithKafkaUI()
    .WithDataVolume(isReadOnly: false);

builder.AddProject<Projects.KafkaRequestResponse_Gateway>("gateway")
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithArgs(kafka.Resource.Name);

builder.Build().Run();
