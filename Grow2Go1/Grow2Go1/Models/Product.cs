namespace Grow2Go1.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public int FarmId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsAvailable { get; set; }
        public string ImagePath { get; set; }
    }
}