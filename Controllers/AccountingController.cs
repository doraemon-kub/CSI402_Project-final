using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System.Globalization;
using System.Linq;
using System.Text;

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

        // หน้าภาพรวมการเงิน ตรวจสอบสลิป/ชำระเงิน คำขอคืนเงิน และรายรับ-รายจ่าย (Req 19, 20, 21)
        public IActionResult Index()
        {
            var allOrders = _repo.Orders;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.FinancialSummary = _repo.GetFinancialSummary();

            ViewBag.PendingPayments = allOrders
                .Where(o => o.Status == OrderStatus.PendingPayment)
                .ToList();

            ViewBag.PendingRefunds = allOrders
                .Where(o => o.Status == OrderStatus.RefundRequested)
                .ToList();

            var recentOrders = allOrders.Take(15).ToList();
            return View(recentOrders);
        }

        // อนุมัติการชำระเงิน / สลิปโอนเงิน (Req 19)
        [HttpPost]
        public IActionResult ApprovePayment(string orderId, string? returnUrl = null, string? role = null)
        {
            var order = _repo.ApprovePayment(orderId);
            if (order != null)
            {
                TempData["SuccessMessage"] = $"อนุมัติการชำระเงินคำสั่งซื้อ #{orderId} (฿{order.GrandTotal:N2}) และส่งเข้าคิวฝ่ายจัดเตรียมสินค้าแล้ว";
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index), new { role });
        }

        // ปฏิเสธสลิปการชำระเงิน (Req 19)
        [HttpPost]
        public IActionResult RejectPayment(string orderId, string reason, string? returnUrl = null, string? role = null)
        {
            var order = _repo.RejectPayment(orderId, reason);
            if (order != null)
            {
                TempData["SuccessMessage"] = $"ปฏิเสธหลักฐานชำระเงินและยกเลิกคำสั่งซื้อ #{orderId} พร้อมคืนสต็อกสินค้าเรียบร้อยแล้ว";
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index), new { role });
        }

        // อนุมัติการคืนเงิน (Req 20)
        [HttpPost]
        public IActionResult ApproveRefund(string orderId, string? returnUrl = null, string? role = null)
        {
            var order = _repo.ApproveRefund(orderId);
            if (order != null)
            {
                TempData["SuccessMessage"] = $"อนุมัติการคืนเงินจำนวน ฿{order.RefundAmount:N2} สำหรับคำสั่งซื้อ {orderId} สำเร็จ";
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index), new { role });
        }

        // บันทึกรายจ่ายจริงรายเดือน (Req 21)
        [HttpPost]
        public IActionResult SaveMonthlyExpense(string month, decimal expenses)
        {
            if (string.IsNullOrWhiteSpace(month))
            {
                TempData["ErrorMessage"] = "กรุณาระบุเดือนที่ต้องการบันทึกรายจ่าย";
                return RedirectToAction(nameof(Index));
            }

            _repo.SaveMonthlyExpense(month, expenses);
            TempData["SuccessMessage"] = $"บันทึกรายจ่ายประจำเดือน {month} จำนวน ฿{expenses:N2} ลงฐานข้อมูลเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Index));
        }

        // ส่งออกรายงานรายรับ-รายจ่าย CSV (Req 21)
        public IActionResult ExportFinancialCsv()
        {
            var summary = _repo.GetFinancialSummary();
            var sb = new StringBuilder();
            sb.AppendLine("Month,Revenue(THB),Expenses(THB),Profit(THB)");
            foreach (var row in summary.MonthlyBreakdown)
            {
                sb.AppendLine($"{row.Month},{row.Revenue:F2},{row.Expenses:F2},{row.Profit:F2}");
            }
            sb.AppendLine();
            sb.AppendLine($"Total Revenue,{summary.TotalRevenue:F2}");
            sb.AppendLine($"Total Expenses,{summary.TotalExpenses:F2}");
            sb.AppendLine($"Total Refunds,{summary.TotalRefunds:F2}");
            sb.AppendLine($"Net Profit,{summary.NetProfit:F2}");

            byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            string fileName = $"financial_report_{System.DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
    }
}
