namespace WebStore.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ParentId { get; set; }

        public virtual Region? Parent { get; set; }
        public virtual ICollection<Region>? Children { get; set;}
    }
}
