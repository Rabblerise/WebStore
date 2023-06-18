namespace WebStore.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Group { get; set; }

        public string? UserId { get; set; }
        public virtual User? Users { get; set; }
    }
}
