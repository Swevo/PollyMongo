# PollyMongo

[![NuGet](https://img.shields.io/nuget/v/PollyMongo.svg)](https://www.nuget.org/packages/PollyMongo)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PollyMongo.svg)](https://www.nuget.org/packages/PollyMongo)
[![CI](https://github.com/Swevo/PollyMongo/actions/workflows/build.yml/badge.svg)](https://github.com/Swevo/PollyMongo/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Polly v8 resilience pipelines for MongoDB.Driver** — wrap `Find`, `InsertOne`, `UpdateOne`, `DeleteOne`, and other `IMongoCollection<T>` calls with retry, timeout, circuit-breaker, and more using a single `ResilientMongoCollection<T>` decorator. Zero changes to your queries.

```csharp
var resilient = collection.WithPolly(pipeline =>
    pipeline
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(200),
            ShouldHandle = new PredicateBuilder().Handle<MongoException>(),
        })
        .AddTimeout(TimeSpan.FromSeconds(5)));

var orders = await resilient.FindAsync(Builders<Order>.Filter.Eq(o => o.CustomerId, id));
```

Every MongoDB operation is now automatically wrapped with retry + timeout — zero changes to existing queries.

---

## Why PollyMongo?

MongoDB.Driver has no built-in retry or timeout interception beyond connection-level settings. PollyMongo adds operation-level resilience cleanly.

| Without PollyMongo | With PollyMongo |
|---|---|
| Write try/catch + retry loops around every query | One `WithPolly(...)` call |
| Manually cancel long-running operations | Timeout managed by the pipeline |
| Duplicate retry logic across repositories | Single pipeline applied everywhere |
| Must touch every operation to add resilience | Zero changes to existing queries |

---

## Installation

```bash
dotnet add package PollyMongo
```

Targets **net6.0**, **net8.0**, and **net9.0**.

Dependencies: `Polly.Core 8.*`, `MongoDB.Driver 3.*`, `Microsoft.Extensions.DependencyInjection.Abstractions 8.*`

---

## Quick start

### 1. Inline pipeline (ad-hoc usage)

```csharp
using PollyMongo;

var resilient = collection.WithPolly(pipeline =>
    pipeline.AddRetry(new RetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder().Handle<MongoException>(),
    }));

var users = await resilient.FindAsync(Builders<User>.Filter.Empty);
```

### 2. Pre-built pipeline

```csharp
var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
    .AddTimeout(TimeSpan.FromSeconds(10))
    .Build();

var resilient = collection.WithPolly(pipeline);
var count = await resilient.CountDocumentsAsync(Builders<Order>.Filter.Empty);
```

### 3. Dependency injection

```csharp
// Program.cs
builder.Services.AddPollyMongo(pipeline =>
    pipeline
        .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3 })
        .AddTimeout(TimeSpan.FromSeconds(5)));

// Repository
public class OrderRepository(IMongoCollection<Order> collection, ResiliencePipeline pipeline)
{
    public Task<List<Order>> GetAllAsync() =>
        collection.WithPolly(pipeline).FindAsync(Builders<Order>.Filter.Empty);

    public Task InsertAsync(Order order) =>
        collection.WithPolly(pipeline).InsertOneAsync(order);
}
```

---

## Supported operations

| Method | Description |
|--------|-------------|
| `FindAsync<T>` | Returns `List<T>` matching the filter |
| `FindOneAsync<T>` | First matching document or `default` |
| `InsertOneAsync` | Insert a single document |
| `InsertManyAsync` | Insert multiple documents |
| `UpdateOneAsync` | Update first matching document |
| `UpdateManyAsync` | Update all matching documents |
| `ReplaceOneAsync` | Replace first matching document |
| `DeleteOneAsync` | Delete first matching document |
| `DeleteManyAsync` | Delete all matching documents |
| `CountDocumentsAsync` | Count matching documents |
| `FindOneAndUpdateAsync` | Atomic find + update |
| `FindOneAndDeleteAsync` | Atomic find + delete |
| `FindOneAndReplaceAsync` | Atomic find + replace |

---

## Pipeline order

Polly strategies are applied outer-to-inner (left-to-right). The recommended order is:

```
[Timeout] → [Retry] → [Circuit Breaker] → [MongoDB]
```

```csharp
pipeline
    .AddTimeout(TimeSpan.FromSeconds(10))    // 1. Overall deadline
    .AddRetry(retryOptions)                  // 2. Retry on failure
    .AddCircuitBreaker(cbOptions)            // 3. Open circuit if overloaded
```

---

## ASP.NET Core example

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(builder.Configuration.GetConnectionString("Mongo")));

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IMongoClient>()
      .GetDatabase("mydb")
      .GetCollection<Order>("orders"));

builder.Services.AddPollyMongo(pipeline =>
    pipeline
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(100),
            BackoffType = DelayBackoffType.Exponential,
            ShouldHandle = new PredicateBuilder()
                .Handle<MongoConnectionException>()
                .Handle<MongoConnectionPoolWaitQueueFullException>()
                .Handle<MongoExecutionTimeoutException>(),
        })
        .AddTimeout(TimeSpan.FromSeconds(30))
        .AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15),
        }));
```

---

## Related packages

| Package | Downloads | Description |
|---|---|---|
| [PollyEFCore](https://www.nuget.org/packages/PollyEFCore) | [![Downloads](https://img.shields.io/nuget/dt/PollyEFCore.svg)](https://www.nuget.org/packages/PollyEFCore) | Polly v8 resilience for EF Core queries and SaveChanges |
| [PollyCosmosDb](https://www.nuget.org/packages/PollyCosmosDb) | [![Downloads](https://img.shields.io/nuget/dt/PollyCosmosDb.svg)](https://www.nuget.org/packages/PollyCosmosDb) | Polly v8 resilience for Azure Cosmos DB with CosmosTransientErrors predicate |
| [PollyDapper](https://www.nuget.org/packages/PollyDapper) | [![Downloads](https://img.shields.io/nuget/dt/PollyDapper.svg)](https://www.nuget.org/packages/PollyDapper) | Polly v8 resilience for Dapper queries and commands |
| [PollySqlClient](https://www.nuget.org/packages/PollySqlClient) | [![Downloads](https://img.shields.io/nuget/dt/PollySqlClient.svg)](https://www.nuget.org/packages/PollySqlClient) | Polly v8 resilience for SQL Server and Azure SQL with SqlServerTransientErrors predicate |
| [PollyMediatR](https://www.nuget.org/packages/PollyMediatR) | [![Downloads](https://img.shields.io/nuget/dt/PollyMediatR.svg)](https://www.nuget.org/packages/PollyMediatR) | Polly v8 resilience pipelines for MediatR |
| [PollyRedis](https://www.nuget.org/packages/PollyRedis) | [![Downloads](https://img.shields.io/nuget/dt/PollyRedis.svg)](https://www.nuget.org/packages/PollyRedis) | Polly v8 resilience for StackExchange.Redis |
| [PollyHealthChecks](https://www.nuget.org/packages/PollyHealthChecks) | [![Downloads](https://img.shields.io/nuget/dt/PollyHealthChecks.svg)](https://www.nuget.org/packages/PollyHealthChecks) | ASP.NET Core health checks for Polly v8 circuit breakers |
| [PollyOpenAI](https://www.nuget.org/packages/PollyOpenAI) | [![Downloads](https://img.shields.io/nuget/dt/PollyOpenAI.svg)](https://www.nuget.org/packages/PollyOpenAI) | Polly v8 resilience for OpenAI and Azure OpenAI |
| [PollyElasticsearch](https://github.com/Swevo/PollyElasticsearch) | Polly v8 for Elastic.Clients.Elasticsearch |
| [PollyAzureKeyVault](https://github.com/Swevo/PollyAzureKeyVault) | Polly v8 for Azure Key Vault |
| [PollySendGrid](https://github.com/Swevo/PollySendGrid) | Polly v8 for SendGrid |
| [PollyMassTransit](https://github.com/Swevo/PollyMassTransit) | Polly v8 for MassTransit |
| [PollyAzureTableStorage](https://github.com/Swevo/PollyAzureTableStorage) | Polly v8 for Azure Table Storage |
| [PollyMailKit](https://github.com/Swevo/PollyMailKit) | MailKit SMTP email client |
| [PollyAzureQueueStorage](https://github.com/Swevo/PollyAzureQueueStorage) | Azure Queue Storage QueueClient |
| [PollyHangfire](https://github.com/Swevo/PollyHangfire) | Hangfire IBackgroundJobClient |
| [PollyBackoff](https://www.nuget.org/packages/PollyBackoff) | [![Downloads](https://img.shields.io/nuget/dt/PollyBackoff.svg)](https://www.nuget.org/packages/PollyBackoff) | Jitter, linear & custom backoff for Polly v8 retry |
| [PollyChaos](https://www.nuget.org/packages/PollyChaos) | [![Downloads](https://img.shields.io/nuget/dt/PollyChaos.svg)](https://www.nuget.org/packages/PollyChaos) | Fault & latency injection (Simmy for Polly v8) |
| [PollyAzureEventHub](https://github.com/Swevo/PollyAzureEventHub) | Polly v8 for Azure Event Hubs |
| [PollyAzureServiceBus](https://www.nuget.org/packages/PollyAzureServiceBus) | [![Downloads](https://img.shields.io/nuget/dt/PollyAzureServiceBus.svg)](https://www.nuget.org/packages/PollyAzureServiceBus) | Polly v8 resilience for Azure Service Bus |

---

## 💼 Need .NET consulting?

The author of this package is available for consulting on **Polly v8 resilience**, **Azure cloud architecture**, and **clean .NET design**.

**[→ solidqualitysolutions.com](https://www.solidqualitysolutions.com/)** · **[LinkedIn](https://www.linkedin.com/in/justbannister/)**
## License

MIT
