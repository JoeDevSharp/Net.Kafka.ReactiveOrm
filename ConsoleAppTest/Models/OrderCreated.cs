namespace ConsoleAppTest.Models
{
    public class OrderCreated
    {
        public int Id { get; set; }
        public string Customer { get; set; } = "";
        public double Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
