using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Models;
using System;
using System.Linq;

namespace CSI402_Project_final.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly PetShopRepository _repo;

        public AccountController(PetShopRepository repo)
        {
            _repo = repo;
        }

        // หน้าโปรไฟล์และจัดการที่อยู่จัดส่ง
        [HttpGet]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name ?? "";
            var user = _repo.GetUserByUsername(username);
            if (user == null) return RedirectToAction("Login", "Auth");

            var model = new ProfileViewModel
            {
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Addresses = user.Addresses ?? new()
            };

            return View(model);
        }

        // อัปเดตข้อมูลส่วนตัว
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(ProfileViewModel model)
        {
            var username = User.Identity?.Name ?? "";
            _repo.UpdateUserProfile(username, model.FullName, model.Email, model.Phone);
            TempData["SuccessMessage"] = "บันทึกข้อมูลส่วนตัวเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Profile));
        }

        // เพิ่มหรือแก้ไขที่อยู่จัดส่ง
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveAddress(UserAddress address)
        {
            var username = User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(address.FullAddress) || string.IsNullOrWhiteSpace(address.Province))
            {
                TempData["ErrorMessage"] = "กรุณากรอกข้อมูลที่อยู่ให้ครบถ้วน";
                return RedirectToAction(nameof(Profile));
            }

            _repo.AddOrUpdateAddress(username, address);
            TempData["SuccessMessage"] = "บันทึกที่อยู่จัดส่งเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Profile));
        }

        // ลบที่อยู่จัดส่ง
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAddress(int addressId)
        {
            var username = User.Identity?.Name ?? "";
            _repo.DeleteAddress(username, addressId);
            TempData["SuccessMessage"] = "ลบที่อยู่จัดส่งเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Profile));
        }

        // ตั้งเป็นที่อยู่จัดส่งหลัก
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetDefaultAddress(int addressId)
        {
            var username = User.Identity?.Name ?? "";
            _repo.SetDefaultAddress(username, addressId);
            TempData["SuccessMessage"] = "ตั้งเป็นที่อยู่จัดส่งหลักเรียบร้อยแล้ว";
            return RedirectToAction(nameof(Profile));
        }

        // หน้าประวัติคำสั่งซื้อของลูกค้า
        public IActionResult Orders(string? successOrderId = null)
        {
            ViewBag.SuccessOrderId = successOrderId;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var orders = _repo.Orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // ลูกค้าขอยกเลิกคำสั่งซื้อ / ขอคืนเงิน (Req 20)
        [HttpPost]
        public IActionResult CancelOrder(string orderId, string reason)
        {
            var order = _repo.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order != null && (order.Status == OrderStatus.PaymentVerified || order.Status == OrderStatus.Preparing))
            {
                order.Status = OrderStatus.RefundRequested;
                order.RefundReason = reason;
                order.RefundAmount = order.GrandTotal;
                _repo.SaveDatabase();
                TempData["SuccessMessage"] = $"ส่งคำขอยกเลิกคำสั่งซื้อ {orderId} และขอคืนเงินเรียบร้อยแล้ว แผนกบัญชีจะดำเนินการตรวจสอบ";
            }
            return RedirectToAction(nameof(Orders));
        }

        // หน้าแจ้งปัญหาการใช้งานระบบ (Helpdesk สำหรับลูกค้า Req 22)
        public IActionResult Support()
        {
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.MyTickets = _repo.SupportTickets.OrderByDescending(t => t.CreatedAt).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult SubmitTicket(string customerName, string email, string category, string subject, string description, TicketPriority priority)
        {
            var ticket = new SupportTicket
            {
                Id = _repo.SupportTickets.Count + 1,
                TicketCode = $"TCK-{DateTime.Now:yyyyMMdd}-{_repo.SupportTickets.Count + 1:D3}",
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? "ลูกค้าทั่วไป" : customerName,
                ContactEmail = email,
                Category = category,
                Subject = subject,
                Description = description,
                Priority = priority,
                Status = TicketStatus.New,
                CreatedAt = DateTime.Now
            };

            _repo.SupportTickets.Insert(0, ticket);
            _repo.SaveDatabase();
            TempData["SuccessMessage"] = $"บันทึกการแจ้งปัญหาเรียบร้อยแล้ว (รหัสอ้างอิง: {ticket.TicketCode}) ทางทีม IT Support จะรีบตรวจสอบ";

            return RedirectToAction(nameof(Support));
        }
    }
}
