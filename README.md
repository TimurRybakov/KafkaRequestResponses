# KafkaRequestResponse

A request-response pattern between two applications with Kafka for testing synchronious http requests with asynchronious balancing and processing.

```
 HTTP GET   |------------|------->|------------|------->|------------|
----------> | Gateway API|        |    Kafka   |        | Processor1 |
            |------------|<-------|------------|<-------|------------|
                                         |
                                         |------------->|------------|
                                         |              | Processor2 |
                                         |<-------------|------------|
```
Distributed application built with .NET Aspire SDK + Confluent.Kafka library. Topic creation is self-managed by services.
