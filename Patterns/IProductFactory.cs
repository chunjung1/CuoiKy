using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Factory Method] - [Nhóm: Creational]
// Mục đích: Tạo đối tượng Product theo category.
public interface IProductFactory
{
    Product CreateProduct(int categoryId, string name, decimal price, int stockQuantity, string description);
}

public class ProductFactory : IProductFactory
{
    public Product CreateProduct(int categoryId, string name, decimal price, int stockQuantity, string description)
    {
        return new Product
        {
            CategoryId = categoryId,
            Name = name,
            Price = price,
            StockQuantity = stockQuantity,
            Description = description
        };
    }
}
