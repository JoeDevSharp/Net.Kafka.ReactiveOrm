using ConsoleAppTest.Models;
using Net.Kafka.ReactiveOrm;

namespace ConsoleAppTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Configura KafkaBus (ajusta bootstrap servers y consumer group)
            using var bus = new KafkaBus("localhost:9092", "console-app");

            // Crea contexto Kafka con bus
            var context = new DefaultKafkaContext(bus);

            // Suscribirse a nuevos pedidos con amount > 100
            var subscription = context.OrdersCreated
                .Where(order => order.Amount > 100)
                .Subscribe(order =>
                {
                    Console.WriteLine($"[RECEIVED] Order {order.Id} from {order.Customer}, Amount: {order.Amount}");
                });

            // Publicar un nuevo pedido
            var newOrder = new OrderCreated
            {
                Id = 1,
                Customer = "JoeDevSharp",
                Amount = 150.75
            };

            await context.OrdersCreated.Publish(newOrder);
            Console.WriteLine("[PUBLISHED] New order sent.");

            Console.WriteLine("Presiona Enter para salir...");
            Console.ReadLine();

            subscription.Dispose();
        }
    }
}
