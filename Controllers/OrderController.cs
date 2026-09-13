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
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var query = _repo.Orders.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            ViewBag.SelectedStatus = status;
            ViewBag.PendingCount = _repo.Orders.Count(o => o.Status == OrderStatus.PaymentVerified);
            ViewBag.PreparingCount = _repo.Orders.Count(o => o.Status == OrderStatus.Preparing);

            return View(query.OrderByDescending(o => o.OrderDate).ToList());
        }

        // อัปเดตสถานะการเตรียมสินค้า
        [HttpPost]
        public IActionResult UpdateStatus(string orderId, OrderStatus newStatus)
        {
            var order = _repo.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = newStatus;
                TempData["SuccessMessage"] = $"อัปเดตสถานะออเดอร์ {orderId} เป็น '{newStatus}' เรียบร้อยแล้ว";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
