namespace Net.Kafka.ReactiveOrm.Reactive
{
    /// <summary>
    /// Provides a reactive publishing interface for Kafka entities.
    /// Used internally by TopicSet<T> to delegate publish operations.
    /// </summary>
    internal static class ReactivePublisher
    {
        /// <summary>
        /// Publishes an entity to the specified Kafka topic using the provided Kafka bus.
        /// </summary>
        /// <typeparam name="T">The type of the entity/message.</typeparam>
        /// <param name="bus">The Kafka bus abstraction.</param>
        /// <param name="topic">The Kafka topic.</param>
        /// <param name="entity">The entity to publish.</param>
        /// <returns>A task representing the async publish operation.</returns>
        public static Task PublishAsync<T>(IKafkaBus bus, string topic, T entity)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Topic is required.", nameof(topic));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return bus.PublishAsync(topic, entity);
        }
    }
}
