using Confluent.Kafka;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Net.Kafka.ReactiveOrm
{
    public class KafkaBus : IKafkaBus, IDisposable
    {
        private readonly ProducerConfig _producerConfig;
        private readonly ConsumerConfig _baseConsumerConfig;
        private readonly ConcurrentDictionary<string, ISubject<object>> _topics = new();
        private readonly ConcurrentDictionary<string, Thread> _consumerThreads = new();
        private readonly CancellationTokenSource _cts = new();

        public KafkaBus(string bootstrapServers, string defaultConsumerGroup)
        {
            _producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _baseConsumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = defaultConsumerGroup,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };
        }

        public IObservable<T> Observe<T>(string topic, string? consumerGroup = null)
        {
            var subject = new Subject<T>();

            // Thread key: topic + group
            var threadKey = $"{consumerGroup ?? _baseConsumerConfig.GroupId}:{topic}";
            if (_consumerThreads.ContainsKey(threadKey))
                return subject.AsObservable();

            var config = new ConsumerConfig(_baseConsumerConfig)
            {
                GroupId = consumerGroup ?? _baseConsumerConfig.GroupId
            };

            var thread = new Thread(() =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(topic);

                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        try
                        {
                            var cr = consumer.Consume(_cts.Token);
                            var data = JsonSerializer.Deserialize<T>(cr.Message.Value);
                            if (data != null)
                                subject.OnNext(data);
                        }
                        catch (ConsumeException ex)
                        {
                            Console.WriteLine($"[KafkaConsumeError] {ex.Error.Reason}");
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[KafkaUnhandledError] {ex.Message}");
                        }
                    }
                }
                finally
                {
                    consumer.Close();
                    subject.OnCompleted();
                }
            });

            thread.IsBackground = true;
            thread.Start();

            _consumerThreads[threadKey] = thread;
            return subject.AsObservable();
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            using var producer = new ProducerBuilder<Null, string>(_producerConfig).Build();
            var json = JsonSerializer.Serialize(message);
            var result = await producer.ProduceAsync(topic, new Message<Null, string> { Value = json });

            Console.WriteLine($"[KafkaPublished] Topic: {topic}, Offset: {result.Offset}");
        }

        public void Dispose()
        {
            _cts.Cancel();
            foreach (var thread in _consumerThreads.Values)
            {
                thread.Join();
            }

            _cts.Dispose();
        }
    }
}
