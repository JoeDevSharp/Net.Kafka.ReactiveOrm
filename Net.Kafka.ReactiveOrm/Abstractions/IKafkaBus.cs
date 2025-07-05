namespace Net.Kafka.ReactiveOrm
{
    /// <summary>
    /// Abstraction over the underlying Kafka client.
    /// Responsible for producing, consuming, and exposing message streams.
    /// </summary>
    public interface IKafkaBus
    {
        /// <summary>
        /// Subscribes to the given topic and returns an observable stream of deserialized messages of type T.
        /// </summary>
        /// <typeparam name="T">The message type to deserialize and stream.</typeparam>
        /// <param name="topic">The Kafka topic to subscribe to.</param>
        /// <param name="consumerGroup">Optional consumer group ID override.</param>
        /// <returns>An observable stream of messages.</returns>
        IObservable<T> Observe<T>(string topic, string? consumerGroup = null);

        /// <summary>
        /// Publishes a message of type T to the given Kafka topic.
        /// </summary>
        /// <typeparam name="T">The message type to serialize and publish.</typeparam>
        /// <param name="topic">The Kafka topic to publish to.</param>
        /// <param name="message">The message instance.</param>
        /// <returns>A task that completes when the message has been published.</returns>
        Task PublishAsync<T>(string topic, T message);
    }
}
