namespace Net.Kafka.ReactiveOrm.Attributes
{
    /// <summary>
    /// Marks a class as representing messages from a specific Kafka topic.
    /// Used by the KafkaOrmContext to bind the entity to a Kafka topic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TopicAttribute : Attribute
    {
        /// <summary>
        /// The Kafka topic name. Supports placeholders (e.g., "@") for dynamic resolution.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Indicates whether topic wildcards (e.g., *) are allowed.
        /// </summary>
        public bool AllowWildcards { get; set; } = false;

        /// <summary>
        /// Optional: consumer group ID override for this topic.
        /// </summary>
        public string? ConsumerGroup { get; set; }

        /// <summary>
        /// Creates a new TopicAttribute bound to the given Kafka topic.
        /// </summary>
        /// <param name="topic">The topic string to bind to (may include wildcards or parameters).</param>
        public TopicAttribute(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic name cannot be null or empty", nameof(topic));

            Topic = topic;
        }
    }
}
