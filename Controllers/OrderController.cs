using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "OrderStaff,Owner,Admin")]
    public class OrderController : Controller
    {
        private readonly PetShopRepository _repo;

        public OrderController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าตรวจสอบคำสั่งซื้อและจัดเตรียมสินค้า
        public IActionResult Index(OrderStatus? status = null)
        {
            var allOrders = _repo.Orders;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.SelectedStatus = status;
            ViewBag.PendingCount = allOrders.Count(o => o.Status == OrderStatus.PaymentVerified);
            ViewBag.PreparingCount = allOrders.Count(o => o.Status == OrderStatus.Preparing);

            var filtered = status.HasValue
                ? allOrders.Where(o => o.Status == status.Value).ToList()
                : allOrders;

            return View(filtered);
        }

        // อัปเดตสถานะการเตรียมสินค้า (บันทึกลง DB)
        [HttpPost]
        public IActionResult UpdateStatus(string orderId, OrderStatus newStatus, string? role = null)
        {
            if (_repo.UpdateOrderStatus(orderId, newStatus))
            {
                TempData["SuccessMessage"] = $"อัปเดตสถานะออเดอร์ {orderId} เป็น '{newStatus}' เรียบร้อยแล้ว";
            }
            if (!string.IsNullOrEmpty(role))
            {
                return RedirectToAction(nameof(Index), new { role = role });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
