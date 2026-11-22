using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSI402_Project_final.Helpers;
using CSI402_Project_final.Models;
using System.Threading.Tasks;

namespace CSI402_Project_final.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly PetShopRepository _repo;
        private readonly IWebHostEnvironment _env;

        public AccountController(PetShopRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
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
        public IActionResult SaveAddress(UserAddress address, string? returnUrl = null)
        {
            var username = User.Identity?.Name ?? "";
            if (string.IsNullOrWhiteSpace(address.FullAddress) || string.IsNullOrWhiteSpace(address.Province))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "กรุณากรอกข้อมูลที่อยู่และจังหวัดให้ครบถ้วน" });
                }
                TempData["ErrorMessage"] = "กรุณากรอกข้อมูลที่อยู่ให้ครบถ้วน";
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Profile));
            }

            _repo.AddOrUpdateAddress(username, address);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    id = address.Id,
                    title = address.Title,
                    recipientName = address.RecipientName,
                    phone = address.Phone,
                    displayAddress = address.DisplayAddress,
                    isDefault = address.IsDefault,
                    message = "บันทึกที่อยู่จัดส่งใหม่เรียบร้อยแล้ว"
                });
            }

            TempData["SuccessMessage"] = "บันทึกที่อยู่จัดส่งเรียบร้อยแล้ว";
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Profile));
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

        // หน้าประวัติคำสั่งซื้อของลูกค้า (กรองเฉพาะของบัญชีผู้ใช้นี้)
        public IActionResult Orders(string? successOrderId = null)
        {
            var username = User.Identity?.Name ?? "";
            ViewBag.SuccessOrderId = successOrderId;
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            var orders = _repo.GetOrdersForUser(username);
            return View(orders);
        }

        // อัปโหลดหรืออัปเดตหลักฐานสลิปโอนเงินภายหลัง (Req 19)
        [HttpPost]
        public async Task<IActionResult> UploadSlip(string orderId, IFormFile? paymentSlip)
        {
            if (paymentSlip == null || paymentSlip.Length == 0)
            {
                TempData["ErrorMessage"] = "กรุณาเลือกไฟล์รูปภาพสลิปการโอนเงิน";
                return RedirectToAction(nameof(Orders));
            }

            var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");
            if (uploadErr != null || string.IsNullOrEmpty(uploadedUrl))
            {
                TempData["ErrorMessage"] = uploadErr ?? "ไม่สามารถอัปโหลดสลิปได้";
                return RedirectToAction(nameof(Orders));
            }

            var username = User.Identity?.Name ?? "";
            if (_repo.AttachPaymentSlip(username, orderId, uploadedUrl))
            {
                TempData["SuccessMessage"] = $"แนบหลักฐานสลิปสำหรับคำสั่งซื้อ #{orderId} เรียบร้อยแล้ว ฝ่ายบัญชีจะตรวจสอบในลำดับถัดไป";
            }
            else
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือเปลี่ยนรูปสลิปได้ เนื่องจากคำสั่งซื้อนี้มีการแนบหลักฐานสลิปและถูกล็อกแล้ว";
            }

            return RedirectToAction(nameof(Orders));
        }

        // ลูกค้าขอยกเลิกคำสั่งซื้อ / ขอคืนเงิน (Req 20)
        [HttpPost]
        public IActionResult CancelOrder(string orderId, string reason)
        {
            var username = User.Identity?.Name ?? "";
            if (_repo.CancelOrder(username, orderId, reason))
            {
                TempData["SuccessMessage"] = $"ดำเนินการยกเลิกคำสั่งซื้อ {orderId} เรียบร้อยแล้ว";
            }
            else
            {
                TempData["ErrorMessage"] = "ไม่สามารถขอยกเลิกคำสั่งซื้อนี้ได้";
            }
            return RedirectToAction(nameof(Orders));
        }

        // หน้าแจ้งปัญหาการใช้งานระบบ (Helpdesk สำหรับลูกค้า Req 22)
        public IActionResult Support()
        {
            var username = User.Identity?.Name ?? "";
            var user = _repo.GetUserByUsername(username);

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
            ViewBag.CurrentUser = user;
            ViewBag.MyTickets = _repo.GetSupportTickets(customerEmail: user?.Email);
            return View();
        }

        [HttpPost]
        public IActionResult SubmitTicket(string customerName, string email, string? phone, string category, string subject, string description, TicketPriority priority)
        {
            var username = User.Identity?.Name ?? "";
            var user = _repo.GetUserByUsername(username);

            var ticket = _repo.CreateSupportTicket(
                customerName,
                email,
                phone ?? user?.Phone ?? "",
                category,
                subject,
                description,
                priority);

            TempData["SuccessMessage"] = $"บันทึกการแจ้งปัญหาเรียบร้อยแล้ว (รหัสอ้างอิง: {ticket.TicketCode}) ทางทีม IT Support จะรีบตรวจสอบ";
            return RedirectToAction(nameof(Support));
        }
    }
}
