using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize(Roles = "AccountingStaff,Owner,Admin")]
    public class AccountingController : Controller
    {
        private readonly PetShopRepository _repo;

        public AccountingController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าภาพรวมการเงิน ตรวจสอบชำระเงิน และคำขอคืนเงิน (Req 19, 20, 21)
        public IActionResult Index()
        {
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var summary = _repo.GetFinancialSummary();
            ViewBag.FinancialSummary = summary;

            ViewBag.PendingRefunds = _repo.Orders
                .Where(o => o.Status == OrderStatus.RefundRequested)
                .ToList();

            var recentOrders = _repo.Orders.OrderByDescending(o => o.OrderDate).Take(10).ToList();
            return View(recentOrders);
        }

        // อนุมัติการคืนเงิน (Req 20)
        [HttpPost]
        public IActionResult ApproveRefund(string orderId)
        {
            var order = _repo.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null)
            {
                order.Status = OrderStatus.Refunded;
                order.RefundedDate = DateTime.Now;
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"อนุมัติการคืนเงินจำนวน ฿{order.RefundAmount:N2} สำหรับคำสั่งซื้อ {orderId} สำเร็จ";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
