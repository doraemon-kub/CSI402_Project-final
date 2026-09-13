using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CSI402_Project_final.Models;
using CSI402_Project_final.Hubs;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    public class CartController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IHubContext<PetShopHub> _hubContext;

        public CartController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        // หน้าตะกร้าสินค้า (Req 3, 4, 11, 12, 13, 14, 15, 16)
        public IActionResult Index()
        {
            _repo.CurrentCart.Recalculate(_repo.Promotions);
            ViewBag.ActivePromotions = _repo.Promotions.Where(p => p.IsValidNow).ToList();
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            return View(_repo.CurrentCart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1, string returnUrl = "/Cart")
        {
            int newCount = _repo.AddToCart(productId, quantity);

            // SignalR: กระจายการอัปเดตจำนวนคนใส่ตะกร้าแบบ Real-Time (Req 8)
            await _hubContext.Clients.All.SendAsync("ReceiveCartCountUpdate", productId, newCount);

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public IActionResult Update(int productId, int quantity)
        {
            _repo.UpdateCartItem(productId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            _repo.RemoveFromCart(productId);
            return RedirectToAction(nameof(Index));
        }

        // หน้าสรุปคำสั่งซื้อและชำระเงิน
        public IActionResult Checkout()
        {
            _repo.CurrentCart.Recalculate(_repo.Promotions);
            if (!_repo.CurrentCart.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            return View(_repo.CurrentCart);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitOrder(string customerName, string customerPhone, string shippingAddress)
        {
            if (!_repo.CurrentCart.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var newOrder = _repo.CheckoutCurrentCart(customerName, customerPhone, shippingAddress);

            // SignalR: แจ้งเตือนฝ่ายคำสั่งซื้อ (Order Staff) แบบ Real-Time ทันที
            await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", newOrder.OrderId, newOrder.CustomerName, newOrder.GrandTotal);

            // ตรวจสอบสินค้าที่เหลือเพื่อเตือน Low Stock (Req 18)
            foreach (var item in newOrder.Items)
            {
                var prod = _repo.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (prod != null && prod.IsLowStock)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", prod.Name, prod.StockQuantity);
                }
            }

            return RedirectToAction("Orders", "Account", new { successOrderId = newOrder.OrderId });
        }
    }
}
