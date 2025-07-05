using Net.Kafka.ReactiveOrm.Extensions;
using System;
using System.Reactive.Linq;

namespace Net.Kafka.ReactiveOrm.Reactive
{
    /// <summary>
    /// Provides reactive subscription logic for Kafka topics.
    /// Used internally by TopicSet<T> to expose filtered and observable message streams.
    /// </summary>
    internal static class ReactiveSubscription
    {
        /// <summary>
        /// Subscribes to the specified Kafka topic using the given Kafka bus and returns an observable stream.
        /// </summary>
        /// <typeparam name="T">The type of the deserialized Kafka message.</typeparam>
        /// <param name="bus">The Kafka bus abstraction.</param>
        /// <param name="topic">The Kafka topic name.</param>
        /// <param name="consumerGroup">Optional consumer group ID override.</param>
        /// <returns>An observable stream of T.</returns>
        public static IObservable<T> Observe<T>(IKafkaBus bus, string topic, string? consumerGroup = null)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic is required.", nameof(topic));

            var observable = bus.Observe<T>(topic, consumerGroup);

            // Si T es una clase, aplicamos WhereNotNull
            if (default(T) == null)
            {
                // T es class o nullable struct
                return observable!
                    .Where(item => item != null)!
                    .Catch<T, Exception>(ex =>
                    {
                        Console.WriteLine($"[KafkaError] {ex.Message}");
                        return System.Reactive.Linq.Observable.Empty<T>();
                    });
            }

            // T es un value type no-nullable, no se aplica null check
            return observable.Catch<T, Exception>(ex =>
            {
                Console.WriteLine($"[KafkaError] {ex.Message}");
                return System.Reactive.Linq.Observable.Empty<T>();
            });
        }

    }
}
