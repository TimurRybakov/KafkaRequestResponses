using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;

namespace KafkaRequestResponse.Gateway.Services;

internal sealed class KafkaSetupService : IKafkaSetupService, IDisposable
{
    private readonly IAdminClient _adminClient;
    private readonly ILogger<KafkaSetupService> _logger;
    private readonly KafkaOptions _options;

    public KafkaSetupService(ILogger<KafkaSetupService> logger, IOptions<KafkaOptions> options)
    {
        var adminConfig = new AdminClientConfig
        {
            BootstrapServers = options.Value.BootstrapServers
                ?? throw new NullReferenceException("Kafka connection string is not defined in the configuration.")
        };
        _adminClient = new AdminClientBuilder(adminConfig).Build();
        _logger = logger;
        _options = options.Value;
    }

    public async Task Configure()
    {
        if (_options.Topics is null || _options.Topics.Count == 0)
        {
            throw new NullReferenceException("No topics configured.");
        }

        var missingTopics = await GetMissingTopicsAsync(_options.Topics);
        if (missingTopics.Count > 0)
        {
            await _adminClient.CreateTopicsAsync(missingTopics);
            _logger.LogInformation("Missing topics created successfully.");
        }
        else
        {
            _logger.LogInformation("All topics exist.");
        }
    }

    private async Task<List<TopicSpecification>> GetMissingTopicsAsync(IReadOnlyList<TopicSpecification> topicSpecifications)
    {
        // Собираем имена
        var topicNames = TopicCollection.OfTopicNames(topicSpecifications.Select(s => s.Name));
        DescribeTopicsResult describeResult = await DescibeTopics(topicNames);

        // Преобразуем список описаний в словарь по имени
        var descriptions = describeResult.TopicDescriptions.ToDictionary(td => td.Name, StringComparer.Ordinal);

        var missingTopicSpecifications = new List<TopicSpecification>(capacity: topicSpecifications.Count);
        foreach (var spec in topicSpecifications)
        {
            if (!descriptions.TryGetValue(spec.Name, out var desc) || desc.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogInformation("Topic {topicName} not found and will be created.", spec.Name);
                missingTopicSpecifications.Add(spec);
            }
        }

        return missingTopicSpecifications;
    }

    private async Task<DescribeTopicsResult> DescibeTopics(TopicCollection topicCollection)
    {
        try
        {
            var options = new DescribeTopicsOptions();
            return await _adminClient.DescribeTopicsAsync(topicCollection, options);
        }
        catch (DescribeTopicsException ex)
        {
            // В случае ошибки DescribeTopicsException доступны частичные результаты
            return new DescribeTopicsResult() { TopicDescriptions = ex.Results.TopicDescriptions };
        }
    }

    public void Dispose() => _adminClient.Dispose();
}
