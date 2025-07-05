using Net.Kafka.ReactiveOrm.Attributes;
using System.Reflection;

namespace Net.Kafka.ReactiveOrm
{
    /// <summary>
    /// Base class for defining reactive Kafka contexts.
    /// Works similarly to Entity Framework's DbContext.
    /// </summary>
    public abstract class KafkaOrmContext
    {
        private readonly IKafkaBus _bus;

        protected KafkaOrmContext(IKafkaBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            InitializeTopicSets();
        }

        /// <summary>
        /// Resolves a TopicSet<T> for the given entity type.
        /// </summary>
        /// <typeparam name="T">The entity type mapped to a Kafka topic.</typeparam>
        /// <param name="topicOverride">Optional topic override if using dynamic topic templates.</param>
        public TopicSet<T> Set<T>(string? topicOverride = null)
        {
            var property = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(p => p.PropertyType == typeof(TopicSet<T>));

            if (property == null)
                throw new InvalidOperationException($"No TopicSet<{typeof(T).Name}> property found in the context.");

            var attr = property.GetCustomAttribute<TopicAttribute>();
            if (attr == null)
                throw new InvalidOperationException($"Missing [Topic] attribute on property '{property.Name}'.");

            var topic = topicOverride != null
                ? attr.Topic.Replace("@", topicOverride)
                : attr.Topic;

            return new TopicSet<T>(_bus, topic, attr.ConsumerGroup);
        }

        /// <summary>
        /// Initializes all TopicSet<T> properties defined in the derived context class.
        /// </summary>
        private void InitializeTopicSets()
        {
            var properties = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(TopicSet<>));

            foreach (var property in properties)
            {
                var attr = property.GetCustomAttribute<TopicAttribute>();
                if (attr == null) continue;

                var topicType = property.PropertyType.GetGenericArguments()[0];
                var topicName = attr.Topic;
                var consumerGroup = attr.ConsumerGroup;

                var topicSet = Activator.CreateInstance(
                    typeof(TopicSet<>).MakeGenericType(topicType),
                    _bus,
                    topicName,
                    consumerGroup
                );

                property.SetValue(this, topicSet);
            }
        }
    }
}
