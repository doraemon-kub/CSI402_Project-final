using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using CSI402_Project_final.Models;
using CSI402_Project_final.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "ShippingStaff,Owner,Admin")]
    public class ShippingController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IHubContext<PetShopHub> _hubContext;

        public ShippingController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)
        {
            _repo = repo;
            _hubContext = hubContext;
        }

        // หน้าจัดการการจัดส่งและพัสดุ
        public IActionResult Index()
        {
            var allOrders = _repo.Orders;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.ReadyToShipCount = allOrders.Count(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.PaymentVerified);
            ViewBag.ShippedTodayCount = allOrders.Count(o => o.Status == OrderStatus.Shipped && o.ShippedDate.HasValue && o.ShippedDate.Value.Date == DateTime.Today);

            var orders = allOrders
                .Where(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Shipped || o.Status == OrderStatus.PaymentVerified)
                .OrderBy(o => o.Status == OrderStatus.Shipped ? 1 : 0)
                .ThenByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // บันทึกเลขพัสดุและน้ำหนักจริง (Req 4, 14, 16)
        [HttpPost]
        public async Task<IActionResult> DispatchOrder(string orderId, string courier, string trackingNumber, double actualWeightKg, string? role = null)
        {
            if (_repo.DispatchOrder(orderId, courier, trackingNumber, actualWeightKg))
            {
                // SignalR: แจ้งเตือน Real-Time ว่าพัสดุจัดส่งแล้ว
                await _hubContext.Clients.All.SendAsync("ReceiveOrderShippedAlert", orderId, trackingNumber, courier);

                TempData["SuccessMessage"] = $"บันทึกเลขพัสดุ {trackingNumber} สำหรับคำสั่งซื้อ {orderId} และเปลี่ยนสถานะเป็น 'จัดส่งแล้ว' เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index), new { role });
        }
    }
}
