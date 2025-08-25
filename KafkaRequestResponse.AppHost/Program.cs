var builder = DistributedApplication.CreateBuilder(args);
var kafka = builder
    .AddKafka("kafka", port: 9092)
    .WithKafkaUI()
    .WithDataVolume(isReadOnly: false);

var processor1 = builder.AddProject<Projects.KafkaRequestResponse_Processor>("processor1")
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithArgs(kafka.Resource.Name)
    .WithHttpsEndpoint(name: "httpProxies", port: 5111)
    .WithHttpsEndpoint(name: "httpsProxies", port: 7111)
    .WithEnvironment("service.name", "processor1");

var processor2 = builder.AddProject<Projects.KafkaRequestResponse_Processor>("processor2")
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithArgs(kafka.Resource.Name)
    .WithHttpsEndpoint(name: "httpProxies", port: 5222)
    .WithHttpsEndpoint(name: "httpsProxies", port: 7222)
    .WithEnvironment("service.name", "processor2");

var gateway = builder.AddProject<Projects.KafkaRequestResponse_Gateway>("gateway")
    .WithReference(kafka)
    .WaitFor(kafka)
    .WithArgs(kafka.Resource.Name)
    .WaitFor(processor1)
    .WaitFor(processor2);

builder.Build().Run();
