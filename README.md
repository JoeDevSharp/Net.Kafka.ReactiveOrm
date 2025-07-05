# Net.Kafka.ReactiveOrm

**Net.Kafka.ReactiveOrm** is a lightweight Reactive Object Mapper (ROM) for Kafka in .NET.  
It maps Kafka topics to strongly typed, reactive entities inspired by Entity Framework (`DbContext` / `DbSet<T>`), enabling declarative subscription and publishing with LINQ-style syntax and reactive patterns.

---

### 🔍 Key Features

- **Entity Mapping via Attributes**: Use `[Topic]` attributes to bind C# classes to Kafka topics.
- **Reactive Subscriptions**: Subscribe reactively using `IObservable<T>`, `.Where()`, `.Subscribe()`.
- **Declarative Publishing**: Publish entities directly with `.Publish(entity)` without dealing with low-level Kafka APIs.
- **Context API**: Manage your Kafka topics via a `KafkaOrmContext`, similar to EF's DbContext.
- **LINQ Filtering**: Filter and transform streams with `.Where()`, `.Select()`.
- **Broker Agnostic**: Abstracts the Kafka client, compatible with any standard Kafka broker.

---

### 🔧 Use Case Scenarios

- Real-time IoT data processing
- Industry 4.0 monitoring & control
- Edge analytics and transformations
- Reactive home automation systems

---

### 🧱 Architecture Overview

| Component         | Description                                                  |
| ----------------- | ------------------------------------------------------------ |
| `TopicSet<T>`     | Represents a typed topic with publish/subscribe capabilities |
| `KafkaOrmContext` | Central registry managing all topic sets                     |
| `IKafkaBus`       | Abstraction over the Kafka client API                        |
| `TopicAttribute`  | Declares the Kafka topic and consumer group for an entity    |

---

### 🤝 Philosophy

Instead of low-level Kafka consumers and producers with string topics and byte arrays, **Net.Kafka.ReactiveOrm** provides a **strongly typed, reactive, and declarative API** that treats Kafka topics as first-class reactive data streams.

It minimizes boilerplate and lets you focus on your domain logic using familiar C# idioms.

---

## 📘 Developer Documentation – Net.Kafka.ReactiveOrm

---

### ✅ 1. Introduction

`Net.Kafka.ReactiveOrm` simplifies Kafka integration by mapping topics to observable entity streams, enabling LINQ-style reactive subscriptions and declarative publishing.

---

### 🚀 2. Installation & Setup

```bash
dotnet add package Net.Kafka.ReactiveOrm
dotnet add package Confluent.Kafka
dotnet add package System.Reactive
```

---

### 🧩 3. Defining Kafka Entities

```csharp
using Net.Kafka.ReactiveOrm.Attributes;

[Topic("orders.created", ConsumerGroup = "order-service")]
public class OrderCreated
{
    public int Id { get; set; }
    public string Customer { get; set; } = "";
    public double Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 🏗️ 4. Creating the Kafka Context

```csharp
using Net.Kafka.ReactiveOrm;

public class KafkaContext : KafkaOrmContext
{
    [Topic("orders.created", ConsumerGroup = "order-service")]
    public TopicSet<OrderCreated> OrdersCreated { get; private set; }

    public KafkaContext(IKafkaBus bus) : base(bus)
    {
        // TopicSets auto-initialized by base
    }
}
```

---

### 👂 5. Subscribing to Messages

```csharp
var context = new KafkaContext(new KafkaBus("localhost:9092", "order-service"));

context.OrdersCreated
    .Where(o => o.Amount > 100)
    .Subscribe(o => Console.WriteLine($"Order {o.Id} from {o.Customer} - {o.Amount}"));
```

---

### 📤 6. Publishing Messages

```csharp
await context.OrdersCreated.Publish(new OrderCreated
{
    Id = 123,
    Customer = "JoeDevSharp",
    Amount = 250.5
});
```

---

### 🧪 7. Full Console Example

```csharp
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Net.Kafka.ReactiveOrm;

class Program
{
    static async Task Main()
    {
        using var bus = new KafkaBus("localhost:9092", "order-service");
        var context = new KafkaContext(bus);

        // Reactive subscription filtering by amount
        var subscription = context.OrdersCreated
            .Where(o => o.Amount > 100)
            .Subscribe(o =>
            {
                Console.WriteLine($"[RECEIVED] Order {o.Id} from {o.Customer} with amount {o.Amount}");
            });

        // Publish a new order
        await context.OrdersCreated.Publish(new OrderCreated
        {
            Id = 1,
            Customer = "JoeDevSharp",
            Amount = 150.75
        });
        Console.WriteLine("[PUBLISHED] New order sent.");

        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();

        subscription.Dispose();
    }
}
```

---

### ⚙️ Kafka Local Setup (Docker)

```bash
docker run -d --name zookeeper -p 2181:2181 confluentinc/cp-zookeeper:latest
docker run -d --name kafka -p 9092:9092 \
  -e KAFKA_ZOOKEEPER_CONNECT=localhost:2181 \
  -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
  -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
  confluentinc/cp-kafka:latest
```

---

### 🛠 Best Practices

- Ensure Kafka is running before starting the app.
- Filter aggressively with `.Where()` to reduce message processing overhead.
- Dispose subscriptions when no longer needed.
- Use consumer groups to balance load among instances.

---

### ❓ FAQ

> **Q: Can I use multiple consumer groups?**
> Yes, specify the consumer group in `[Topic]` attribute or in `KafkaBus` constructor.

> **Q: Is this compatible with Kafka cloud providers?**
> Yes, as long as they support standard Kafka protocol and you provide connection config.

> **Q: How does serialization work?**
> By default, JSON serialization is used for message payloads.
