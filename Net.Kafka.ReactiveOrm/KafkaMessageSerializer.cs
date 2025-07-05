using System.Text.Json;

namespace Net.Kafka.ReactiveOrm
{
    /// <summary>
    /// Default serializer/deserializer for Kafka messages using System.Text.Json.
    /// </summary>
    public static class KafkaMessageSerializer
    {
        private static readonly JsonSerializerOptions _defaultOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        public static string Serialize<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return JsonSerializer.Serialize(obj, _defaultOptions);
        }

        /// <summary>
        /// Deserializes a JSON string into an object of type T.
        /// </summary>
        public static T? Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json, _defaultOptions);
        }
    }
}
