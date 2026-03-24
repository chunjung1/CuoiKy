using System.Text.Json;
using CuoiKy.Patterns;
using CuoiKy.ViewModels;

public static class CartSessionHelper
{
    private const string CartKey = "TechStoreCart";

    public static Cart GetCart(HttpContext context)
    {
        var json = context.Session.GetString    (CartKey);
        if (string.IsNullOrEmpty(json))
        {
            return new Cart();
        }

        return JsonSerializer.Deserialize<Cart>(json) ?? new Cart();
    }

    public static void SetCart(HttpContext context, Cart cart)
    {
        var json = JsonSerializer.Serialize(cart);
        context.Session.SetString(CartKey, json);
    }
}
