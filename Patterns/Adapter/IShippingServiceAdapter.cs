using System.Threading.Tasks;
using CuoiKy.Models;

namespace CuoiKy.Patterns;

public interface IShippingServiceAdapter
{
    Task<string> CreateShippingOrderAsync(Order order);
}
