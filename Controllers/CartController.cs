using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CSI402_Project_final.Helpers;
using CSI402_Project_final.Hubs;
using CSI402_Project_final.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    public class CartController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IHubContext<PetShopHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public CartController(PetShopRepository repo, IHubContext<PetShopHub> hubContext, IWebHostEnvironment env)
        {
            _repo = repo;
            _hubContext = hubContext;
            _env = env;
        }

        // หน้าตะกร้าสินค้า (Req 3, 4, 11, 12, 13, 14, 15, 16)
        public IActionResult Index()
        {
            ViewBag.ActivePromotions = _repo.GetValidPromotions();
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            return View(_repo.CurrentCart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string returnUrl = "/Cart")
        {
            // ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง (Guest ไม่สามารถหยิบสินค้าลงในตะกร้าได้)
            if (User?.Identity?.IsAuthenticated != true)
            {
                string targetLoginUrl = Url.Action("Login", "Auth", new { returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl }) ?? "/Auth/Login";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                    Request.Headers.Accept.ToString().Contains("application/json"))
                {
                    return Json(new
                    {
                        success = false,
                        requireLogin = true,
                        redirectUrl = targetLoginUrl,
                        message = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า"
                    });
                }
                TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า";
                return Redirect(targetLoginUrl);
            }

            int newCount = _repo.AddToCart(productId, quantity);
            var product = _repo.GetProductById(productId);
            string productName = product?.Name ?? "สินค้า";

            // SignalR: กระจายการอัปเดตจำนวนคนใส่ตะกร้าและสต็อกคงเหลือแบบ Real-Time (Req 8)
            await _hubContext.Clients.All.SendAsync("ReceiveCartCountUpdate", productId, newCount);
            if (product != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
            }

            // รองรับ AJAX ไม่ต้องโหลดหน้าใหม่ เพื่อความลื่นไหล (Smooth User Experience)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success = true,
                    productId = productId,
                    productName = productName,
                    cartCount = _repo.CurrentCart.TotalItemCount,
                    productCartCount = newCount,
                    remainingStock = product?.StockQuantity ?? 0,
                    isOutOfStock = (product?.StockQuantity ?? 0) <= 0,
                    message = $"เพิ่ม \"{productName}\" ลงในตะกร้าเรียบร้อยแล้ว"
                });
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            _repo.UpdateCartItem(productId, quantity);
            var product = _repo.GetProductById(productId);
            if (product != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            _repo.RemoveFromCart(productId);
            var product = _repo.GetProductById(productId);
            if (product != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
            }
            return RedirectToAction(nameof(Index));
        }

        // หน้าสรุปคำสั่งซื้อและชำระเงิน (ต้องเข้าสู่ระบบก่อนเพื่อเชื่อม UserId ลง DB)
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = _repo.CurrentCart;
            if (!cart.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.PromptPayPayload = PromptPayHelper.GeneratePayload("0812345678", cart.GrandTotal);
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmitOrder(
            string customerName,
            string customerPhone,
            string shippingAddress,
            string paymentMethod = "โอนผ่านธนาคาร / PromptPay",
            IFormFile? paymentSlip = null)
        {
            string? slipUrl = null;
            if (paymentSlip != null && paymentSlip.Length > 0)
            {
                var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");
                if (uploadErr != null)
                {
                    TempData["ErrorMessage"] = uploadErr;
                    return RedirectToAction(nameof(Checkout));
                }
                slipUrl = uploadedUrl;
            }

            var username = User.Identity?.Name ?? "";
            var (newOrder, errorMessage) = _repo.CheckoutCurrentCart(
                username, customerName, customerPhone, shippingAddress, paymentMethod, slipUrl);

            if (newOrder == null)
            {
                TempData["ErrorMessage"] = errorMessage ?? "ไม่สามารถดำเนินการสั่งซื้อได้";
                return RedirectToAction(nameof(Index));
            }

            // SignalR: แจ้งเตือนฝ่ายคำสั่งซื้อ/บัญชี แบบ Real-Time ทันที
            await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", newOrder.OrderId, newOrder.CustomerName, newOrder.GrandTotal);

            // ตรวจสอบสินค้าที่เหลือเพื่อเตือน Low Stock (Req 18)
            var productIds = newOrder.Items.Select(i => i.ProductId).ToList();
            var affectedProducts = _repo.QueryProducts().Where(p => productIds.Contains(p.Id)).ToList();
            foreach (var prod in affectedProducts)
            {
                if (prod.IsLowStock)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", prod.Name, prod.StockQuantity);
                }
            }

            return RedirectToAction("Orders", "Account", new { successOrderId = newOrder.OrderId });
        }
    }
}
