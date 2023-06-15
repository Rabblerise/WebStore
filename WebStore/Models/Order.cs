namespace WebStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int RegionId { get; set; }
        public int ItemId { get; set; }
        public decimal Amount { get; set; }

        public virtual Region? Region { get; set; }
        public virtual Item? Item { get; set; }
    }
}
