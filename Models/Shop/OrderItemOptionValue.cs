namespace katachi.Models.Shop
{
    public class OrderItemOptionValue
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }
        public int ProductOptionValueId { get; set; }
        public ProductOptionValue? ProductOptionValue { get; set; }
    }
}
