using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CuoiKy.Data;
using CuoiKy.Models;
using CuoiKy.ViewModels;
using CuoiKy.Extensions;
using CuoiKy.Patterns;
using System.Security.Claims;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace CuoiKy.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly CheckoutFacade _checkoutFacade;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly PayOSClient _payOS;
        private readonly IEnumerable<IOrderObserver> _observers;

        public CartController(ApplicationDbContext dbContext, CheckoutFacade checkoutFacade, IQrCodeGenerator qrCodeGenerator, PayOSClient payOS, IEnumerable<IOrderObserver> observers)
        {
            _dbContext = dbContext;
            _checkoutFacade = checkoutFacade;
            _qrCodeGenerator = qrCodeGenerator;
            _payOS = payOS;
            _observers = observers;
        }

        private bool IsAdminUser()
        {
            return User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
        }

        private string GetCartSessionKey()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var name = User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return $"Cart_User_{name}";
                }
            }
            return "Cart_Guest";
        }

        private Cart GetCart()
        {
            var key = GetCartSessionKey();
            Cart? cart = HttpContext.Session.Get<Cart>(key);
            if (cart == null)
            {
                cart = new Cart();
                SaveCart(cart);
            }
            return cart;
        }

        private void SaveCart(Cart cart)
        {
            var key = GetCartSessionKey();
            HttpContext.Session.Set(key, cart);
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
            if (IsAdminUser())
            {
                return RedirectToAction("Index", "Home");
            }
            return View(GetCart());
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            if (IsAdminUser())
            {
                return CartPartial(new Cart(), "Admin không thể sử dụng giỏ hàng.");
            }
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
            if (IsAdminUser())
            {
                return CartPartial(new Cart(), "Admin không thể sử dụng giỏ hàng.");
            }
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
            if (IsAdminUser())
            {
                return CartPartial(new Cart(), "Admin không thể sử dụng giỏ hàng.");
            }
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);
            return CartPartial(cart);
        }

        [HttpPost]
        public IActionResult ToggleItemSelection(int productId, bool isSelected)
        {
            if (IsAdminUser())
            {
                return CartPartial(new Cart(), "Admin không thể sử dụng giỏ hàng.");
            }
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
            if (IsAdminUser())
            {
                TempData["ErrorMessage"] = "Admin không thể thanh toán.";
                return RedirectToAction("Index", "Home");
            }
            var cart = GetCart();
            if (!cart.Items.Any(i => i.IsSelected))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction(nameof(Index));
            }
            return View(cart);
        }

        [HttpGet]
        public IActionResult ApplyCoupon(string code)
        {
            code = (code ?? string.Empty).ToUpper().Trim();
            if (string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });
            }

            var coupon = _dbContext.Coupons.FirstOrDefault(c => c.Code == code);
            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại." });
            }

            if (coupon.ExpiryDate < DateTime.Now)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn." });
            }

            if (coupon.UsedCount >= coupon.UsageLimit)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng." });
            }

            return Json(new { success = true, discountPercent = coupon.DiscountPercent });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(string customerName, string phoneNumber, string shippingAddress, PaymentMethod paymentMethod, string? couponCode, string shippingPartner, bool giftWrap = false, bool expressShipping = false)
        {
            if (IsAdminUser())
            {
                TempData["ErrorMessage"] = "Admin không thể thanh toán.";
                return RedirectToAction("Index", "Home");
            }
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

            decimal baseTotal = selectedItems.Sum(i => i.Price * i.Quantity);
            decimal discountPercent = 0;
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                var coupon = _dbContext.Coupons.FirstOrDefault(c => c.Code.ToUpper() == couponCode.ToUpper().Trim());
                if (coupon != null)
                {
                    if (coupon.ExpiryDate < DateTime.Now)
                    {
                        TempData["ErrorMessage"] = "Mã giảm giá đã hết hạn.";
                        return RedirectToAction(nameof(Checkout));
                    }
                    if (coupon.UsedCount >= coupon.UsageLimit)
                    {
                        TempData["ErrorMessage"] = "Mã giảm giá đã hết lượt sử dụng.";
                        return RedirectToAction(nameof(Checkout));
                    }
                    discountPercent = coupon.DiscountPercent;
                    coupon.UsedCount++;
                    _dbContext.Coupons.Update(coupon);
                }
            }

            var order = new Order
            {
                UserId = userId,
                GiftWrap = giftWrap,
                ExpressShipping = expressShipping,
                CustomerName = customerName,
                PhoneNumber = phoneNumber,
                ShippingAddress = shippingAddress,
                PaymentMethod = paymentMethod,
                ShippingPartner = shippingPartner,
                TotalAmount = baseTotal * (1 - discountPercent / 100),
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                Items = selectedItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Price
                }).ToList()
            };

            OrderComponent totalComponent = new BasicOrderComponent(order);
            if (giftWrap)
            {
                totalComponent = new GiftWrapDecorator(totalComponent);
            }
            if (expressShipping)
            {
                totalComponent = new ExpressShippingDecorator(totalComponent);
            }
            order.TotalAmount = totalComponent.GetTotal();

            try
            {
                _checkoutFacade.PlaceOrder(order);

                // Nếu chọn PayOS, tạo link thanh toán và redirect khách
                if (paymentMethod == PaymentMethod.PayOS)
                {
                    var items = new List<PaymentLinkItem>();
                    foreach (var item in order.Items)
                    {
                        var product = await _dbContext.Products.FindAsync(item.ProductId);
                        string prodName = product != null ? product.Name : "San pham";
                        items.Add(new PaymentLinkItem
                        {
                            Name = prodName,
                            Quantity = item.Quantity,
                            Price = (long)item.UnitPrice
                        });
                    }

                    string domain = $"{Request.Scheme}://{Request.Host}";
                    string returnUrl = $"{domain}/Cart/PayOSReturn?orderId={order.Id}";
                    string cancelUrl = $"{domain}/Cart/PayOSReturn?orderId={order.Id}&cancel=true";
                    string desc = $"Thanh toan #{order.Id}";

                    var paymentRequest = new CreatePaymentLinkRequest
                    {
                        OrderCode = order.Id,
                        Amount = (long)order.TotalAmount,
                        Description = desc,
                        Items = items,
                        CancelUrl = cancelUrl,
                        ReturnUrl = returnUrl
                    };

                    var paymentResult = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

                    // Xóa các sản phẩm đã mua khỏi giỏ hàng
                    cart.Items.RemoveAll(i => i.IsSelected);
                    SaveCart(cart);

                    return Redirect(paymentResult.CheckoutUrl);
                }

                // Xóa các sản phẩm đã mua khỏi giỏ hàng đối với COD/Chuyển khoản thủ công
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

        [HttpGet]
        public async Task<IActionResult> PayOSReturn(int orderId, string? status, string? code, bool cancel)
        {
            var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (cancel || code != "00" || status != "PAID")
            {
                order.PaymentStatus = PaymentStatus.Failed;
                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();

                TempData["ErrorMessage"] = "Thanh toán qua PayOS đã bị hủy hoặc không thành công.";
                return RedirectToAction("Details", "Order", new { id = order.Id });
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Paid;
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();

            // Kích hoạt các observers khi thanh toán thành công
            foreach (var observer in _observers)
            {
                observer.OnOrderPaid(order);
            }

            TempData["SuccessMessage"] = "Thanh toán qua PayOS thành công!";
            return RedirectToAction(nameof(OrderSuccess), new { id = order.Id });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PayOSWebhook([FromBody] Webhook body)
        {
            try
            {
                var verifiedData = await _payOS.Webhooks.VerifyAsync(body);

                int orderId = (int)verifiedData.OrderCode;
                var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order != null)
                {
                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        order.PaymentStatus = PaymentStatus.Paid;
                        order.Status = OrderStatus.Paid;
                        _dbContext.Orders.Update(order);
                        await _dbContext.SaveChangesAsync();

                        // Kích hoạt các observers khi nhận webhook thanh toán thành công
                        foreach (var observer in _observers)
                        {
                            observer.OnOrderPaid(order);
                        }
                    }
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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

            if (order.PaymentMethod == PaymentMethod.BankTransfer)
            {
                ViewBag.QrUrl = _qrCodeGenerator.BuildImageUrl($"TechStoreOrder_{order.Id}", 250);
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Reorder(int orderId)
        {
            if (IsAdminUser())
            {
                TempData["ErrorMessage"] = "Admin không thể mua lại đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            var order = await _dbContext.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            int addedCount = 0;

            foreach (var item in order.Items)
            {
                if (item.Product != null && item.Product.StockQuantity > 0)
                {
                    int qtyToAdd = Math.Min(item.Quantity, item.Product.StockQuantity);
                    cart.AddItem(item.Product, qtyToAdd);
                    
                    var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                    if (cartItem != null)
                    {
                        cartItem.IsSelected = true;
                    }
                    
                    addedCount++;
                }
            }

            if (addedCount == 0)
            {
                TempData["ErrorMessage"] = "Không thể mua lại đơn hàng vì các sản phẩm đều đã hết hàng.";
                return RedirectToAction("MyOrders", "Order");
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"Đã thêm {addedCount} sản phẩm từ đơn hàng #{orderId} vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }
    }
}
