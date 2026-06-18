using CuoiKy.Models;

namespace CuoiKy.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsSelected { get; set; } = true; // Default to selected when added
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(Product product, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.DiscountPrice.HasValue && product.DiscountPrice.Value < product.Price 
                            ? product.DiscountPrice.Value 
                            : product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }
        }

        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    RemoveItem(productId);
                }
            }
        }

        public decimal GetTotalPrice()
        {
            return Items.Where(i => i.IsSelected).Sum(i => i.Price * i.Quantity);
        }
    }
}
