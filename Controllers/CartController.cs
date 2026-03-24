using Microsoft.AspNetCore.Mvc;
using CuoiKy.Data;
using CuoiKy.Models;
using CuoiKy.ViewModels;
using CuoiKy.Extensions;
using CuoiKy.Patterns;
using System.Security.Claims;

namespace CuoiKy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly CheckoutFacade _checkoutFacade;

        public CartController(ApplicationDbContext dbContext, CheckoutFacade checkoutFacade)
        {
            _dbContext = dbContext;
            _checkoutFacade = checkoutFacade;
        }

        private Cart GetCart()
        {
            Cart? cart = HttpContext.Session.Get<Cart>("Cart");
            if (cart == null)
            {
                cart = new Cart();
                SaveCart(cart);
            }
            return cart;
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.Set("Cart", cart);
        }

        private PartialViewResult CartPartial(Cart cart, string? errorMessage = null)
        {
            ViewBag.ErrorMessage = errorMessage;
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            ViewBag.StockMap = _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.StockQuantity })
                .ToDictionary(x => x.Id, x => x.StockQuantity);
            return PartialView("_CartPartial", cart);
        }

        public IActionResult Index()
        {
            return View(GetCart());
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var product = _dbContext.Products.Find(productId);
            if (product == null) return NotFound();

            if (product.StockQuantity <= 0)
            {
                return CartPartial(GetCart(), $"Sản phẩm {product.Name} đã hết hàng.");
            }

            var cart = GetCart();
            // Luôn cộng dồn 1 sản phẩm khi nhấn từ bên ngoài
            cart.AddItem(product, 1);
            SaveCart(cart);

            return CartPartial(cart);
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            var product = _dbContext.Products.Find(productId);

            if (item != null && product != null)
            {
                if (product.StockQuantity <= 0)
                {
                    item.IsSelected = false;
                    SaveCart(cart);
                    return CartPartial(cart, $"Sản phẩm {product.Name} đã hết hàng.");
                }

                if (quantity > product.StockQuantity)
                {
                    quantity = product.StockQuantity;
                    item.IsSelected = false;
                    cart.UpdateQuantity(productId, quantity);
                    SaveCart(cart);
                    return CartPartial(cart, $"Số lượng tối đa của {product.Name} hiện tại là {product.StockQuantity}.");
                }
            }

            cart.UpdateQuantity(productId, quantity);
            SaveCart(cart);
            return CartPartial(cart);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);
            return CartPartial(cart);
        }

        [HttpPost]
        public IActionResult ToggleItemSelection(int productId, bool isSelected)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (isSelected)
                {
                    var product = _dbContext.Products.Find(productId);
                    if (product == null || product.StockQuantity <= 0)
                    {
                        item.IsSelected = false;
                        SaveCart(cart);
                        return CartPartial(cart, "Sản phẩm đã hết hàng, không thể chọn để đặt hàng.");
                    }

                    if (item.Quantity > product.StockQuantity)
                    {
                        item.IsSelected = false;
                        SaveCart(cart);
                        return CartPartial(cart, $"Số lượng vượt tồn kho, vui lòng giảm số lượng của {product.Name}.");
                    }
                }

                item.IsSelected = isSelected;
                SaveCart(cart);
            }
            return CartPartial(cart);
        }

        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Items.Any(i => i.IsSelected))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            return View(cart);
        }

        [HttpPost]
        public IActionResult ProcessCheckout(string customerName, string phoneNumber, string shippingAddress, PaymentMethod paymentMethod)
        {
            var cart = GetCart();
            var selectedItems = cart.Items.Where(i => i.IsSelected).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            // Tạo Order từ Cart thông qua CheckoutFacade
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity?.Name;
                var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var user = _dbContext.Users.FirstOrDefault(u => u.Username == username || u.Email == email);
                if (user != null)
                {
                    userId = user.Id;
                }
            }

            var order = new Order
            {
                UserId = userId,
                CustomerName = customerName,
                PhoneNumber = phoneNumber,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                TotalAmount = cart.GetTotalPrice(),
                Status = OrderStatus.Pending,
                Items = selectedItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList()
            };

            try
            {
                _checkoutFacade.PlaceOrder(order);

                // Xóa các sản phẩm đã mua khỏi giỏ hàng
                cart.Items.RemoveAll(i => i.IsSelected);
                SaveCart(cart);

                return RedirectToAction(nameof(OrderSuccess), new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Checkout));
            }
        }

        public IActionResult OrderSuccess(int id)
        {
            var order = _dbContext.Orders
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
