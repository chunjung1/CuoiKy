using CuoiKy.Models;

namespace CuoiKy.Patterns;

// [Design Pattern: Iterator] - [Nhóm: Behavioral]
// Mục đích: Duyệt danh sách sản phẩm theo iterator.
public interface IIterator<T>
{
    bool HasNext();
    T Next();
}

public class ProductIterator : IIterator<Product>
{
    private readonly IList<Product> _products;
    private int _current = 0;

    public ProductIterator(IList<Product> products)
    {
        _products = products;
    }

    public bool HasNext() => _current < _products.Count;

    public Product Next()
    {
        return _products[_current++];
    }
}
