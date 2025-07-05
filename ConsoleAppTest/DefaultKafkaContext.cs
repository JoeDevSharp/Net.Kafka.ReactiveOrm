using ConsoleAppTest.Models;
using Net.Kafka.ReactiveOrm;
using Net.Kafka.ReactiveOrm.Attributes;

namespace ConsoleAppTest
{
    public class DefaultKafkaContext : KafkaContext
    {
        [Topic("orders.created", ConsumerGroup = "console-app")]
        public TopicSet<OrderCreated> OrdersCreated { get; set; }

        public DefaultKafkaContext(IKafkaBus bus) : base(bus)
        {
        }
    }
}
