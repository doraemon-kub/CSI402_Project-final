# โค้ด Controller ทั้งหมดพร้อมคำอธิบายแบบบรรทัดต่อบรรทัด (Inline Commented Source Code)
## ระบบ PetStore Pro — วิชา CSI402 การเชื่อมต่อโปรแกรมส่วนหน้ากับโปรแกรมส่วนหลัง

เอกสารฉบับนี้รวบรวมซอร์สโค้ดของ Controller ทุกไฟล์ ทุกบรรทัด โดยเขียนคอมเมนต์อธิบายการทำงานต่อท้ายบรรทัดโค้ด (Inline Comment) หรือเขียนกำกับไว้บรรทัดถัดไปโดยตรง เพื่อให้อ่านเข้าใจตรรกะการทำงาน (Logic), สิทธิ์การเข้าถึง (Authorization), การรับส่งข้อมูล (Model Binding), และการเชื่อมต่อส่วนหน้า-ส่วนหลังได้อย่างชัดเจนในที่เดียว

---

## สารบัญ Controller ทั้ง 12 ไฟล์
1. `AccountController.cs` (จัดการโปรไฟล์, ที่อยู่จัดส่ง, ประวัติคำสั่งซื้อ, แนบสลิป, แจ้งปัญหา)
2. `AccountingController.cs` (ตรวจสลิป, อนุมัติ/ปฏิเสธเงิน, คืนเงิน, บันทึกรายจ่าย, ส่งออก CSV)
3. `AdminController.cs` (จัดการสิทธิ์, ระงับบัญชีเตะ SignalR, สร้างผู้ใช้, ตรวจสอบ Audit Log)
4. `AuthController.cs` (เข้าสู่ระบบ, Quick Demo Login, สลับบทบาท, สมัครสมาชิก, ออกจากระบบ)
5. `CartController.cs` (ตะกร้าสินค้า, คำนวณโปรโมชั่น, AJAX Add/Update, สั่งซื้อ, SignalR)
6. `HomeController.cs` (หน้าร้านค้า, ตัวกรองสินค้าขั้นสูง, หน้าเจาะลึกสินค้า, แนะนำสินค้า)
7. `InventoryController.cs` (คลังสินค้า, ปรับสต็อกด่วน, เพิ่ม/แก้ไขสินค้า, Low Stock Alert)
8. `MarketingController.cs` (แดชบอร์ดโปรโมชั่น, สวิตช์เปิด/ปิด, Rule Builder สร้างเงื่อนไข)
9. `OrderController.cs` (คิวงานจัดเตรียมสินค้า, เปลี่ยนสถานะ Preparing)
10. `OwnerController.cs` (Executive BI Dashboard วิเคราะห์รายได้ กำไร และสินค้าขายดี)
11. `ShippingController.cs` (คิวงานพัสดุรอส่ง, ชั่งน้ำหนักจริง, ออกเลข Tracking, แจ้งเตือน Shipped)
12. `SupportController.cs` (IT Helpdesk Board รับเรื่องและบันทึกแนวทางแก้ไขปัญหา)

---

# 1. AccountController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีจัดการสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize]
using Microsoft.AspNetCore.Hosting; // นำเข้า IWebHostEnvironment เพื่อเข้าถึงโฟลเดอร์เซิร์ฟเวอร์ (wwwroot)
using Microsoft.AspNetCore.Http; // นำเข้า IFormFile สำหรับรับไฟล์รูปภาพสลิปที่ลูกค้าอัปโหลดเข้ามา
using Microsoft.AspNetCore.Mvc; // นำเข้าคลาสหลักของ ASP.NET Core MVC เช่น Controller, IActionResult, HttpGet, HttpPost
using CSI402_Project_final.Helpers; // นำเข้า FileUploadHelper สำหรับบันทึกไฟล์รูปภาพลงโฟลเดอร์ slips
using CSI402_Project_final.Models; // นำเข้า Model ข้อมูล เช่น ProfileViewModel, UserAddress, SupportTicket, PetShopRepository
using System.Threading.Tasks; // นำเข้า Task สำหรับรองรับการทำงานแบบ Asynchronous (async/await)

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize] // กำหนดสิทธิ์ระดับคลาส บังคับให้ผู้ใช้งานต้องผ่านการล็อกอินก่อนเท่านั้นจึงจะเข้าถึงฟังก์ชันในนี้ได้
    public class AccountController : Controller // ประกาศคลาส AccountController สืบทอดมาจากคลาสหลัก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับเข้าถึงข้อมูลและตรรกะธุรกิจ
        private readonly IWebHostEnvironment _env; // ฟิลด์เก็บสภาพแวดล้อมโฮสต์สำหรับอ้างอิงตำแหน่งพาธ wwwroot

        public AccountController(PetShopRepository repo, IWebHostEnvironment env) // Constructor รับ DI จากระบบ
        {
            _repo = repo; // กำหนดค่า Repository ที่ถูกฉีดเข้ามาให้ตัวแปรภายในคลาส
            _env = env; // กำหนดค่า WebHostEnvironment ให้ตัวแปรภายในคลาส
        }

        // หน้าโปรไฟล์และจัดการที่อยู่จัดส่ง
        [HttpGet] // ระบุว่ารองรับคำขอด้วย HTTP GET เมื่อผู้ใช้เปิดหน้าเว็บ
        public IActionResult Profile() // ฟังก์ชันแสดงหน้าโปรไฟล์และรายการสมุดที่อยู่
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ที่กำลังล็อกอินอยู่จาก Claims ในคุกกี้เซสชัน
            var user = _repo.GetUserByUsername(username); // คิวรีดึงข้อมูลของผู้ใช้นี้จากฐานข้อมูล Repository
            if (user == null) return RedirectToAction("Login", "Auth"); // หากไม่พบบัญชีผู้ใช้ในระบบ ให้ส่งกลับไปหน้าล็อกอิน

            var model = new ProfileViewModel // สร้าง ViewModel บรรจุข้อมูลเพื่อส่งไปแสดงผลบนหน้าบ้าน
            {
                Username = user.Username, // ชื่อผู้ใช้
                FullName = user.FullName, // ชื่อ-นามสกุล
                Email = user.Email, // อีเมล
                Phone = user.Phone, // เบอร์โทรศัพท์
                Role = user.Role, // บทบาทในระบบ
                Addresses = user.Addresses ?? new() // รายการสมุดที่อยู่จัดส่ง (ถ้าไม่มีให้เป็น List ว่าง)
            };

            return View(model); // ส่ง Model ไปเรนเดอร์ในมุมมอง Views/Account/Profile.cshtml
        }

        // อัปเดตข้อมูลส่วนตัว
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST เมื่อมีการส่งฟอร์มแก้ไขข้อมูล
        [ValidateAntiForgeryToken] // ตรวจสอบโทเค็นความปลอดภัยเพื่อป้องกันการโจมตีแบบ CSRF
        public IActionResult UpdateProfile(ProfileViewModel model) // รับข้อมูลที่แก้ไขมาจากหน้าบ้านผ่าน Model Binding
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบันจาก Claims
            _repo.UpdateUserProfile(username, model.FullName, model.Email, model.Phone); // สั่ง Repository อัปเดตข้อมูลและบันทึก Audit Log
            TempData["SuccessMessage"] = "บันทึกข้อมูลส่วนตัวเรียบร้อยแล้ว"; // กำหนดข้อความแจ้งเตือนความสำเร็จใน TempData
            return RedirectToAction(nameof(Profile)); // Redirect กลับไปยังหน้า Profile เพื่อโหลดข้อมูลใหม่ (Post-Redirect-Get)
        }

        // เพิ่มหรือแก้ไขที่อยู่จัดส่ง
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult SaveAddress(UserAddress address, string? returnUrl = null) // รับข้อมูลที่อยู่และ URL ปลายทาง (รองรับทั้ง Form ปกติ และ AJAX)
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            if (string.IsNullOrWhiteSpace(address.FullAddress) || string.IsNullOrWhiteSpace(address.Province)) // ตรวจสอบความครบถ้วนของที่อยู่และจังหวัด
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") // ตรวจสอบว่าเป็นคำขอแบบ AJAX หรือไม่
                {
                    return Json(new { success = false, message = "กรุณากรอกข้อมูลที่อยู่และจังหวัดให้ครบถ้วน" }); // ตอบกลับเป็น JSON แจ้งเตือนข้อผิดพลาด
                }
                TempData["ErrorMessage"] = "กรุณากรอกข้อมูลที่อยู่ให้ครบถ้วน"; // กรณีฟอร์มปกติ เก็บข้อความผิดพลาดลงใน TempData
                return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) // ตรวจสอบความถูกต้องและปลอดภัยของ returnUrl
                    ? LocalRedirect(returnUrl) // ส่งกลับไปยัง URL หน้าเดิมอย่างปลอดภัย
                    : RedirectToAction(nameof(Profile)); // ถ้าไม่มี returnUrl ให้ส่งกลับหน้า Profile
            }

            _repo.AddOrUpdateAddress(username, address); // สั่ง Repository เพิ่มที่อยู่ใหม่หรือแก้ไขที่อยู่เดิมลงในฐานข้อมูล

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") // ตรวจสอบว่าเป็นคำขอแบบ AJAX หรือไม่
            {
                return Json(new // ส่งข้อมูลที่อยู่ใหม่กลับไปเป็น JSON ให้ JavaScript ฝั่งหน้าบ้านอัปเดต Dropdown ทันทีโดยไม่ต้องรีโหลดหน้า
                {
                    success = true, // สถานะสำเร็จ
                    id = address.Id, // รหัสที่อยู่ใหม่
                    title = address.Title, // ชื่อป้ายกำกับที่อยู่
                    recipientName = address.RecipientName, // ชื่อผู้รับ
                    phone = address.Phone, // เบอร์โทรศัพท์
                    displayAddress = address.DisplayAddress, // ข้อความที่อยู่แบบรวม formatted
                    isDefault = address.IsDefault, // สถานะที่อยู่หลัก
                    message = "บันทึกที่อยู่จัดส่งใหม่เรียบร้อยแล้ว" // ข้อความแจ้งเตือน
                });
            }

            TempData["SuccessMessage"] = "บันทึกที่อยู่จัดส่งเรียบร้อยแล้ว"; // กรณีฟอร์มปกติ แจ้งเตือนความสำเร็จผ่าน TempData
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) // ตรวจสอบ returnUrl
                ? LocalRedirect(returnUrl) // ส่งกลับไปยัง URL เดิม
                : RedirectToAction(nameof(Profile)); // ส่งกลับหน้า Profile
        }

        // ลบที่อยู่จัดส่ง
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี CSRF
        public IActionResult DeleteAddress(int addressId) // รับรหัส ID ของที่อยู่ที่จะลบ
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            _repo.DeleteAddress(username, addressId); // สั่ง Repository ลบที่อยู่นี้ออกจากบัญชีผู้ใช้
            TempData["SuccessMessage"] = "ลบที่อยู่จัดส่งเรียบร้อยแล้ว"; // แจ้งเตือนความสำเร็จ
            return RedirectToAction(nameof(Profile)); // ส่งกลับหน้า Profile
        }

        // ตั้งเป็นที่อยู่จัดส่งหลัก
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี CSRF
        public IActionResult SetDefaultAddress(int addressId) // รับรหัส ID ของที่อยู่ที่จะตั้งเป็นค่าเริ่มต้น
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            _repo.SetDefaultAddress(username, addressId); // สั่ง Repository ปรับที่อยู่นี้เป็นค่าเริ่มต้น และปลดสถานะเดิมของที่อยู่อื่นออก
            TempData["SuccessMessage"] = "ตั้งเป็นที่อยู่จัดส่งหลักเรียบร้อยแล้ว"; // แจ้งเตือนความสำเร็จ
            return RedirectToAction(nameof(Profile)); // ส่งกลับหน้า Profile
        }

        // หน้าประวัติคำสั่งซื้อของลูกค้า (กรองเฉพาะของบัญชีผู้ใช้นี้)
        public IActionResult Orders(string? successOrderId = null) // แสดงประวัติการสั่งซื้อ และรับรหัสออเดอร์ที่เพิ่งทำรายการสำเร็จ
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            ViewBag.SuccessOrderId = successOrderId; // ส่งรหัสออเดอร์ไปให้ View เพื่อเปิด Modal แสดงความยินดี
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทจำลองปัจจุบันไปยัง View
            var orders = _repo.GetOrdersForUser(username); // ดึงรายการคำสั่งซื้อเฉพาะของผู้ใช้นี้จากฐานข้อมูล
            return View(orders); // ส่งรายการออเดอร์ไปเรนเดอร์ใน Views/Account/Orders.cshtml
        }

        // อัปโหลดหรืออัปเดตหลักฐานสลิปโอนเงินภายหลัง (Req 19)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> UploadSlip(string orderId, IFormFile? paymentSlip) // ฟังก์ชันรับรหัสออเดอร์และไฟล์สลิปแบบ Asynchronous
        {
            if (paymentSlip == null || paymentSlip.Length == 0) // ตรวจสอบว่ามีการแนบไฟล์และขนาดไฟล์มากกว่า 0 ไบต์หรือไม่
            {
                TempData["ErrorMessage"] = "กรุณาเลือกไฟล์รูปภาพสลิปการโอนเงิน"; // แจ้งเตือนกรณีไม่ได้เลือกไฟล์
                return RedirectToAction(nameof(Orders)); // ส่งกลับหน้าประวัติออเดอร์
            }

            var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips"); // บันทึกไฟล์สลิปลง wwwroot/uploads/slips
            if (uploadErr != null || string.IsNullOrEmpty(uploadedUrl)) // ตรวจสอบหากเกิดข้อผิดพลาดในการเซฟรูป
            {
                TempData["ErrorMessage"] = uploadErr ?? "ไม่สามารถอัปโหลดสลิปได้"; // แจ้งข้อผิดพลาด
                return RedirectToAction(nameof(Orders)); // ส่งกลับหน้าประวัติออเดอร์
            }

            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            if (_repo.AttachPaymentSlip(username, orderId, uploadedUrl)) // สั่ง Repository ผูก URL สลิปเข้ากับออเดอร์ และปรับสถานะเป็นรอตรวจ
            {
                TempData["SuccessMessage"] = $"แนบหลักฐานสลิปสำหรับคำสั่งซื้อ #{orderId} เรียบร้อยแล้ว ฝ่ายบัญชีจะตรวจสอบในลำดับถัดไป"; // แจ้งความสำเร็จ
            }
            else // กรณีแนบสลิปไม่สำเร็จ (เนื่องจากออเดอร์ถูกตรวจสอบหรือล็อกแล้ว)
            {
                TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือเปลี่ยนรูปสลิปได้ เนื่องจากคำสั่งซื้อนี้มีการแนบหลักฐานสลิปและถูกล็อกแล้ว"; // แจ้งเตือนการล็อก
            }

            return RedirectToAction(nameof(Orders)); // ส่งกลับหน้าประวัติออเดอร์
        }

        // ลูกค้าขอยกเลิกคำสั่งซื้อ / ขอคืนเงิน (Req 20)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult CancelOrder(string orderId, string reason) // รับรหัสคำสั่งซื้อและเหตุผลที่ขอยกเลิก
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            if (_repo.CancelOrder(username, orderId, reason)) // สั่ง Repository ยกเลิกออเดอร์ (คืนสต็อกถ้ายังไม่จ่าย หรือขอคืนเงินถ้าจ่ายแล้ว)
            {
                TempData["SuccessMessage"] = $"ดำเนินการยกเลิกคำสั่งซื้อ {orderId} เรียบร้อยแล้ว"; // แจ้งความสำเร็จ
            }
            else // กรณียกเลิกไม่ได้
            {
                TempData["ErrorMessage"] = "ไม่สามารถขอยกเลิกคำสั่งซื้อนี้ได้"; // แจ้งข้อผิดพลาด
            }
            return RedirectToAction(nameof(Orders)); // ส่งกลับหน้าประวัติออเดอร์
        }

        // หน้าแจ้งปัญหาการใช้งานระบบ (Helpdesk สำหรับลูกค้า Req 22)
        public IActionResult Support() // ฟังก์ชันแสดงหน้าเปิดตั๋ว Support และรายการตั๋วปัญหาเดิม
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            var user = _repo.GetUserByUsername(username); // คิวรีข้อมูลผู้ใช้จากฐานข้อมูล

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.CurrentUser = user; // ส่งข้อมูลโปรไฟล์ผู้ใช้ไปแสดงในฟอร์มล่วงหน้า
            ViewBag.MyTickets = _repo.GetSupportTickets(customerEmail: user?.Email); // คิวรีตั๋วปัญหาที่เปิดด้วยอีเมลของลูกค้ารายนี้
            return View(); // เรนเดอร์หน้า Views/Account/Support.cshtml
        }

        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult SubmitTicket(string customerName, string email, string? phone, string category, string subject, string description, TicketPriority priority) // รับข้อมูลตั๋วปัญหาจากฟอร์ม
        {
            var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
            var user = _repo.GetUserByUsername(username); // คิวรีผู้ใช้เพื่อนำเบอร์โทรศัพท์มาใช้กรณีในฟอร์มว่าง

            var ticket = _repo.CreateSupportTicket( // สั่ง Repository สร้าง Support Ticket ใหม่ลงฐานข้อมูล
                customerName, // ชื่อผู้แจ้ง
                email, // อีเมลติดต่อกลับ
                phone ?? user?.Phone ?? "", // เบอร์โทรศัพท์
                category, // หมวดหมู่ปัญหา
                subject, // หัวข้อเรื่อง
                description, // รายละเอียดของปัญหา
                priority); // ระดับความสำคัญ

            TempData["SuccessMessage"] = $"บันทึกการแจ้งปัญหาเรียบร้อยแล้ว (รหัสอ้างอิง: {ticket.TicketCode}) ทางทีม IT Support จะรีบตรวจสอบ"; // แจ้งรหัสอ้างอิง Ticket ให้ลูกค้า
            return RedirectToAction(nameof(Support)); // ส่งกลับหน้า Support
        }
    }
}
```

---

# 2. AccountingController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, File, LocalRedirect
using CSI402_Project_final.Models; // นำเข้าโมเดลคำสั่งซื้อ Order, OrderStatus, FinancialSummary, PetShopRepository
using System.Globalization; // นำเข้า CultureInfo สำหรับจัดรูปแบบวันที่ของชื่อไฟล์ CSV
using System.Linq; // นำเข้า LINQ เช่น .Where(), .Take(), .Concat()
using System.Text; // นำเข้า StringBuilder และ Encoding สำหรับสร้างข้อความ CSV และแปลงเป็น UTF-8 BOM

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "AccountingStaff,Owner,Admin")] // กำหนดสิทธิ์เฉพาะฝ่ายบัญชี, ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class AccountingController : Controller // ประกาศคลาส AccountingController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับจัดการบัญชีการเงินและคำสั่งซื้อ

        public AccountingController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // กำหนดค่า Repository ให้ตัวแปรภายในคลาส
        }

        // หน้าภาพรวมการเงิน ตรวจสอบสลิป/ชำระเงิน คำขอคืนเงิน และรายรับ-รายจ่าย (Req 19, 20, 21)
        public IActionResult Index() // แดชบอร์ดภาพรวมการเงินผ่าน HTTP GET
        {
            var allOrders = _repo.Orders; // ดึงรายการคำสั่งซื้อทั้งหมดในระบบ
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทจำลองปัจจุบันไปยัง View
            ViewBag.FinancialSummary = _repo.GetFinancialSummary(); // ดึงรายงานสรุปทางการเงิน (รายได้, รายจ่าย, คืนเงิน, กำไรสุทธิ)

            ViewBag.PendingPayments = allOrders // คัดกรองออเดอร์ที่รอตรวจสอบสลิปการโอนเงิน
                .Where(o => o.Status == OrderStatus.PendingPayment) // กรองเฉพาะออเดอร์ที่มีสถานะ PendingPayment
                .ToList(); // แปลงเป็น List เก็บใน ViewBag.PendingPayments

            ViewBag.PendingRefunds = allOrders // คัดกรองออเดอร์ที่มีลูกค้ายื่นคำขอคืนเงิน
                .Where(o => o.Status == OrderStatus.RefundRequested) // กรองเฉพาะออเดอร์ที่มีสถานะ RefundRequested
                .ToList(); // แปลงเป็น List เก็บใน ViewBag.PendingRefunds

            var recentOrders = allOrders.Take(15).ToList(); // ดึงคำสั่งซื้อล่าสุด 15 รายการ
            return View(recentOrders); // ส่งรายการออเดอร์ล่าสุดไปเรนเดอร์ใน Views/Accounting/Index.cshtml
        }

        // อนุมัติการชำระเงิน / สลิปโอนเงิน (Req 19)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult ApprovePayment(string orderId, string? returnUrl = null, string? role = null) // ฟังก์ชันอนุมัติสลิปโอนเงิน
        {
            var order = _repo.ApprovePayment(orderId); // สั่ง Repository ปรับสถานะเป็น PaymentVerified และบันทึกยอดเงินเข้าบัญชีรายรับจริง
            if (order != null) // ตรวจสอบว่าอนุมัติสำเร็จ
            {
                TempData["SuccessMessage"] = $"อนุมัติการชำระเงินคำสั่งซื้อ #{orderId} (฿{order.GrandTotal:N2}) และส่งเข้าคิวฝ่ายจัดเตรียมสินค้าแล้ว"; // แสดงข้อความสำเร็จ
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) // ตรวจสอบความถูกต้องของ returnUrl
            {
                return LocalRedirect(returnUrl); // ส่งกลับไปยัง URL หน้าเดิม
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับไปยังหน้า Index ของฝ่ายบัญชี
        }

        // ปฏิเสธสลิปการชำระเงิน (Req 19)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult RejectPayment(string orderId, string reason, string? returnUrl = null, string? role = null) // ฟังก์ชันปฏิเสธสลิป
        {
            var order = _repo.RejectPayment(orderId, reason); // สั่ง Repository ยกเลิกออเดอร์ และคืนสต็อกสินค้าทั้งหมดในออเดอร์กลับเข้าคลังอัตโนมัติ
            if (order != null) // ตรวจสอบว่าสำเร็จ
            {
                TempData["SuccessMessage"] = $"ปฏิเสธหลักฐานชำระเงินและยกเลิกคำสั่งซื้อ #{orderId} พร้อมคืนสต็อกสินค้าเรียบร้อยแล้ว"; // แจ้งเตือนการยกเลิกและคืนสต็อก
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) // ตรวจสอบ returnUrl
            {
                return LocalRedirect(returnUrl); // ส่งกลับหน้าเดิม
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        // อนุมัติการคืนเงิน (Req 20)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult ApproveRefund(string orderId, string? returnUrl = null, string? role = null) // ฟังก์ชันอนุมัติการคืนเงินให้ลูกค้า
        {
            var order = _repo.ApproveRefund(orderId); // สั่ง Repository ปรับสถานะเป็น Refunded และหักลดยอดขายสุทธิในระบบบัญชี
            if (order != null) // ตรวจสอบผลลัพธ์
            {
                TempData["SuccessMessage"] = $"อนุมัติการคืนเงินจำนวน ฿{order.RefundAmount:N2} สำหรับคำสั่งซื้อ {orderId} สำเร็จ"; // แจ้งยอดเงินคืนสำเร็จ
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) // ตรวจสอบ returnUrl
            {
                return LocalRedirect(returnUrl); // ส่งกลับหน้าเดิม
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        // บันทึกรายจ่ายจริงรายเดือน (Req 21)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult SaveMonthlyExpense(string month, decimal expenses) // รับชื่อเดือน (เช่น "2026-11") และยอดเงินรายจ่ายจริง
        {
            if (string.IsNullOrWhiteSpace(month)) // ตรวจสอบว่าระบุเดือนหรือไม่
            {
                TempData["ErrorMessage"] = "กรุณาระบุเดือนที่ต้องการบันทึกรายจ่าย"; // แจ้งข้อผิดพลาด
                return RedirectToAction(nameof(Index)); // ส่งกลับหน้า Index
            }

            _repo.SaveMonthlyExpense(month, expenses); // สั่ง Repository บันทึกรายจ่ายประจำเดือนลง DB และคำนวณกำไรสุทธิของเดือนนั้นใหม่
            TempData["SuccessMessage"] = $"บันทึกรายจ่ายประจำเดือน {month} จำนวน ฿{expenses:N2} ลงฐานข้อมูลเรียบร้อยแล้ว"; // แจ้งความสำเร็จ
            return RedirectToAction(nameof(Index)); // ส่งกลับหน้า Index
        }

        // ส่งออกรายงานรายรับ-รายจ่าย CSV (Req 21)
        public IActionResult ExportFinancialCsv() // Action ส่งออกไฟล์สเปรดชีต CSV
        {
            var summary = _repo.GetFinancialSummary(); // ดึงข้อมูลสรุปทางการเงินทั้งหมดจากฐานข้อมูล
            var sb = new StringBuilder(); // สร้าง StringBuilder สำหรับต่อข้อความเนื้อหาไฟล์ CSV
            sb.AppendLine("Month,Revenue(THB),Expenses(THB),Profit(THB)"); // เขียนบรรทัดหัวตาราง CSV
            foreach (var row in summary.MonthlyBreakdown) // วนลูปข้อมูลรายรับ-รายจ่ายของแต่ละเดือน
            {
                sb.AppendLine($"{row.Month},{row.Revenue:F2},{row.Expenses:F2},{row.Profit:F2}"); // เขียนข้อมูลรายเดือน ทศนิยม 2 ตำแหน่ง
            }
            sb.AppendLine(); // เว้นบรรทัดว่าง
            sb.AppendLine($"Total Revenue,{summary.TotalRevenue:F2}"); // สรุปยอดรายรับรวม
            sb.AppendLine($"Total Expenses,{summary.TotalExpenses:F2}"); // สรุปยอดรายจ่ายรวม
            sb.AppendLine($"Total Refunds,{summary.TotalRefunds:F2}"); // สรุปยอดการคืนเงินรวม
            sb.AppendLine($"Net Profit,{summary.NetProfit:F2}"); // สรุปยอดกำไรสุทธิ

            byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(); // แปลงข้อความเป็น Byte Array พร้อมแทรก UTF-8 BOM ป้องกันภาษาไทยเพี้ยนใน Excel
            string fileName = $"financial_report_{System.DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv"; // สร้างชื่อไฟล์ตามวันที่ปัจจุบัน
            return File(bytes, "text/csv; charset=utf-8", fileName); // ส่งคืนไฟล์สตรีมให้เบราว์เซอร์ดาวน์โหลดทันที
        }
    }
}
```


---

# 3. AdminController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
using Microsoft.AspNetCore.SignalR; // นำเข้า IHubContext สำหรับส่งสัญญาณ WebSocket สั่งเตะผู้ใช้ออกจากระบบแบบ Real-Time
using CSI402_Project_final.Models; // นำเข้าโมเดลผู้ใช้ SystemUser, UserRole, AuditLog, PetShopRepository
using CSI402_Project_final.Hubs; // นำเข้า PetShopHub สำหรับการสื่อสาร Real-Time
using System.Linq; // นำเข้าฟังก์ชัน LINQ เช่น .Where(), .Count(), .FirstOrDefault()
using System.Threading.Tasks; // นำเข้า Task สำหรับรองรับการทำงาน Asynchronous ของ SignalR

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "Admin,Owner")] // จำกัดสิทธิ์เฉพาะผู้ดูแลระบบ (Admin) หรือผู้บริหาร (Owner) เท่านั้น
    public class AdminController : Controller // ประกาศคลาส AdminController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับจัดการข้อมูลผู้ใช้และ Audit Log
        private readonly IHubContext<PetShopHub> _hubContext; // ฟิลด์เก็บ SignalR Hub Context สำหรับบรอดแคสต์เตะบัญชีที่ถูกสั่งระงับ

        public AdminController(PetShopRepository repo, IHubContext<PetShopHub> hubContext) // Constructor รับ DI จากระบบ
        {
            _repo = repo; // กำหนดค่า Repository ให้ตัวแปรภายในคลาส
            _hubContext = hubContext; // กำหนดค่า Hub Context ให้ตัวแปรภายในคลาส
        }

        // ตรวจสอบว่าผู้ใช้งานปัจจุบันที่กำลังทำรายการมีสถานะเป็น Owner หรือไม่ (รองรับทั้ง Claims และ Multi-Role Tab Switcher)
        private bool IsCurrentOperatorOwner(string? roleParam) // ฟังก์ชัน Helper ตรวจสอบสิทธิ์ว่าผู้ควบคุมเป็น Owner หรือไม่
        {
            if (!string.IsNullOrEmpty(roleParam)) // หากมีการส่งพารามิเตอร์จำลองบทบาทของแท็บมา
            {
                var resolved = _repo.ResolveTabUserInfo(roleParam); // ถอดรหัสข้อมูลผู้ใช้ของแท็บนั้น
                return string.Equals(resolved.RoleName, "Owner", System.StringComparison.OrdinalIgnoreCase); // คืนค่า true หากเป็น Owner
            }
            return User.IsInRole("Owner"); // ตรวจสอบจาก Claims ในคุกกี้ของผู้ใช้
        }

        // หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin) พร้อมระบบกรองตามบทบาท (Role Filter) & ค้นหา
        public IActionResult Index(string? role = null, string? roleFilter = "all", string? search = null) // Action เมธอดแสดงหน้ารายชื่อผู้ใช้
        {
            var allUsers = _repo.Users; // ดึงรายชื่อผู้ใช้ทั้งหมดในระบบ
            foreach (var u in allUsers) // วนลูปตรวจสอบผู้ใช้ทุกคน
            {
                _repo.CheckAndRefreshUserSuspension(u); // ตรวจสอบว่าบัญชีที่ถูกระงับชั่วคราวหมดเวลาหรือยัง ถ้าครบแล้วให้ปลดระงับอัตโนมัติ
            }
            allUsers = _repo.Users; // รีเฟรชรายชื่อผู้ใช้อีกครั้งหลังการปลดระงับ

            bool isOwner = IsCurrentOperatorOwner(role); // ตรวจสอบว่าผู้ควบคุมเป็น Owner หรือไม่
            ViewBag.IsCurrentOwner = isOwner; // ส่งค่าสิทธิ์ Owner ไปยัง View เพื่อควบคุมการเปิด/ปิดปุ่มแก้ไขบัญชีระดับสูง
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.TotalUsers = allUsers.Count; // จำนวนผู้ใช้ทั้งหมด
            ViewBag.TotalStaff = allUsers.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest); // จำนวนพนักงานทุกแผนก
            ViewBag.TotalCustomers = allUsers.Count(u => u.Role == UserRole.Customer); // จำนวนลูกค้าสมาชิก

            string filter = (roleFilter ?? "all").Trim().ToLowerInvariant(); // จัดรูปแบบตัวกรองบทบาท
            IEnumerable<SystemUser> filtered = allUsers; // กำหนดตัวแปรคัดกรองเริ่มต้น

            if (filter == "staff") // กรองเฉพาะพนักงาน
            {
                filtered = filtered.Where(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
            }
            else if (filter == "customer") // กรองเฉพาะลูกค้า
            {
                filtered = filtered.Where(u => u.Role == UserRole.Customer);
            }
            else if (Enum.TryParse<UserRole>(filter, true, out var specificRole)) // กรองเฉพาะบทบาทเจาะจง
            {
                filtered = filtered.Where(u => u.Role == specificRole);
            }

            if (!string.IsNullOrWhiteSpace(search)) // ถ้ามีคำค้นหา
            {
                string kw = search.Trim(); // ตัดช่องว่าง
                filtered = filtered.Where(u => // คัดกรองจากชื่อผู้ใช้, ชื่อนามสกุล หรืออีเมล
                    u.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.RoleFilter = filter; // ส่งค่าตัวกรองบทบาทไปยัง View
            ViewBag.Search = search; // ส่งคำค้นไปยัง View

            return View(filtered.ToList()); // ส่งรายชื่อผู้ใช้ที่คัดกรองแล้วไปแสดงผลใน Views/Admin/Index.cshtml
        }

        // ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน + ระงับบัญชีพร้อมเหตุผลและระยะเวลา (เตะออกทันทีผ่าน SignalR)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> UpdateUserRole( // Action ปรับเปลี่ยนบทบาทและสั่งระงับบัญชีแบบ Asynchronous
            int userId, // รหัสผู้ใช้ที่ต้องการแก้ไข
            UserRole newRole, // บทบาทใหม่ที่ต้องการมอบหมาย
            bool isActive, // สถานะเปิดใช้งาน (true) หรือระงับบัญชี (false)
            string? suspendReason = null, // เหตุผลในการระงับ
            int suspendDurationMinutes = -1, // ระยะเวลาที่ระงับ (นาที) หรือ -1 คือระงับถาวร
            string? role = null, // บทบาทของแท็บผู้ทำรายการ
            string? roleFilter = null, // ตัวกรองเดิม
            string? search = null) // คำค้นเดิม
        {
            bool isOwner = IsCurrentOperatorOwner(role); // ตรวจสอบว่าผู้ทำรายการเป็น Owner หรือไม่
            var targetUser = _repo.Users.FirstOrDefault(u => u.Id == userId); // ค้นหาผู้ใช้เป้าหมายที่ต้องการแก้ไข
            if (targetUser == null) // ถ้าไม่พบผู้ใช้
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้งานที่ต้องการแก้ไข"; // แจ้งข้อผิดพลาด
                return RedirectToAction(nameof(Index), new { role, roleFilter, search }); // ส่งกลับหน้า Index
            }

            // กฎความปลอดภัย: Admin ไม่สามารถแก้ไข/ระงับบัญชี Admin หรือ Owner ได้ และไม่สามารถแต่งตั้งใครเป็น Admin หรือ Owner ได้ (เฉพาะ Owner เท่านั้น)
            if (!isOwner) // หากผู้กระทำการไม่ใช่ระดับ Owner
            {
                if (targetUser.Role == UserRole.Admin || targetUser.Role == UserRole.Owner) // บล็อกไม่ให้แตะต้องบัญชี Admin หรือ Owner
                {
                    TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือระงับบัญชี Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                    return RedirectToAction(nameof(Index), new { role, roleFilter, search });
                }
                if (newRole == UserRole.Admin || newRole == UserRole.Owner) // บล็อกไม่ให้แต่งตั้งใครเป็น Admin หรือ Owner
                {
                    TempData["ErrorMessage"] = "ไม่สามารถแต่งตั้งผู้ใช้เป็น Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                    return RedirectToAction(nameof(Index), new { role, roleFilter, search });
                }
            }

            if (_repo.UpdateUserRole(userId, newRole, isActive, suspendReason, suspendDurationMinutes)) // สั่ง Repository ปรับปรุงข้อมูลและบันทึก Audit Log
            {
                if (!isActive) // กรณีที่บัญชีถูกสั่งระงับการใช้งาน (!isActive)
                {
                    string reasonText = targetUser.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ"; // ข้อความเหตุผลการระงับ
                    string untilText = targetUser.SuspensionDisplayUntil; // ข้อความระยะเวลาสิ้นสุดการระงับ

                    // ส่งสัญญาณ Real-Time เตะผู้ใช้ออกจากระบบทุกแท็บที่กำลังล็อกอินบัญชีนี้อยู่ทันที
                    await _hubContext.Clients.All.SendAsync( // ส่งสัญญาณ WebSocket ไปยังไคลเอนต์ทั้งหมด
                        "ReceiveAccountSuspended", // ชื่อ Event ของ SignalR
                        targetUser.Username, // ชื่อผู้ใช้ที่ถูกระงับ
                        targetUser.Role.ToString(), // บทบาท
                        reasonText, // เหตุผล
                        untilText); // สิ้นสุดเมื่อใด

                    TempData["SuccessMessage"] = $"ระงับบัญชี '{targetUser.Username}' เรียบร้อยแล้ว ({untilText} | เหตุผล: {reasonText})"; // แจ้งความสำเร็จ
                }
                else // กรณีเปิดใช้งานหรือเปลี่ยนบทบาทปกติ
                {
                    TempData["SuccessMessage"] = $"อัปเดตบัญชีผู้ใช้งาน #{userId} ({targetUser.Username}) เป็น '{newRole}' (สถานะ: เปิดใช้งานปกติ) สำเร็จ"; // แจ้งความสำเร็จ
                }
            }
            return RedirectToAction(nameof(Index), new { role, roleFilter, search }); // ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
        }

        // เพิ่มผู้ใช้งานใหม่ (บันทึกลง DB) - Admin ไม่สามารถสร้าง User บทบาท Admin ได้ ต้องให้ Owner ทำเท่านั้น
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult CreateUser(SystemUser user, string? newUserRole = null, string? role = null) // Action สร้างบัญชีผู้ใช้งานใหม่
        {
            // ใช้ชื่อฟิลด์ newUserRole แยกจาก role (บทบาทของแท็บผู้ทำรายการ) เพื่อไม่ให้ Model Binding ชนกัน
            if (string.IsNullOrWhiteSpace(newUserRole) || !Enum.TryParse<UserRole>(newUserRole, true, out var requestedRole)) // ตรวจสอบความถูกต้องของบทบาทใหม่
            {
                TempData["ErrorMessage"] = "กรุณาเลือกบทบาทหน้าที่ (Role) ให้ถูกต้อง"; // แจ้งเตือนข้อผิดพลาด
                return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
            }
            user.Role = requestedRole; // ผูกบทบาทเข้ากับ Entity ผู้ใช้ใหม่

            bool isOwner = IsCurrentOperatorOwner(role); // ตรวจสอบว่าผู้สร้างเป็น Owner หรือไม่
            if ((requestedRole == UserRole.Admin || requestedRole == UserRole.Owner) && !isOwner) // บล็อกไม่ให้ Admin สร้างบัญชีระดับ Admin หรือ Owner
            {
                TempData["ErrorMessage"] = "ไม่สามารถสร้างบัญชีบทบาท Admin ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
                return RedirectToAction(nameof(Index), new { role });
            }

            var (success, message) = _repo.CreateUser(user); // สั่ง Repository บันทึกผู้ใช้ใหม่ลงฐานข้อมูล
            if (success)
                TempData["SuccessMessage"] = message; // แจ้งผลสำเร็จ
            else
                TempData["ErrorMessage"] = message; // แจ้งข้อผิดพลาดกรณีชื่อผู้ใช้หรืออีเมลซ้ำ

            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        // หน้าตรวจสอบบันทึกกิจกรรมย้อนหลัง (Audit Log)
        public IActionResult AuditLogs(string? search = null, string? roleFilter = null, string? actionFilter = null, string? role = null) // Action แสดงหน้า Audit Trail
        {
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            var logs = _repo.GetAuditLogs(500); // ดึงประวัติกิจกรรมย้อนหลังล่าสุด 500 รายการ

            ViewBag.TotalLogsCount = logs.Count; // จำนวน Log ทั้งหมด
            ViewBag.SecurityLogsCount = logs.Count(l => l.Action == "UPDATE_USER_ROLE" || l.Action == "SUSPEND_ACCOUNT" || l.Action == "CREATE_USER"); // นับ Log ด้านความปลอดภัย
            ViewBag.FinanceLogsCount = logs.Count(l => l.Action.Contains("PAYMENT") || l.Action.Contains("REFUND") || l.Action.Contains("EXPENSE")); // นับ Log ด้านการเงิน
            ViewBag.InventoryLogsCount = logs.Count(l => l.Action.Contains("PRODUCT") || l.Action.Contains("STOCK")); // นับ Log ด้านคลังสินค้า

            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all") // กรอง Log ตามบทบาท
            {
                logs = logs.Where(l => string.Equals(l.UserRole, roleFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "all") // กรอง Log ตามประเภท Action
            {
                logs = logs.Where(l => string.Equals(l.Action, actionFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search)) // ถ้ามีคำค้นหา
            {
                string kw = search.Trim(); // ตัดช่องว่าง
                logs = logs.Where(l => // ค้นหาจากชื่อผู้ใช้, แอ็กชัน, ชื่อตาราง, รหัสเรคคอร์ด, รายละเอียด หรือไอพีแอดเดรส
                    (!string.IsNullOrEmpty(l.Username) && l.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Action) && l.Action.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.RecordId) && l.RecordId.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.Details) && l.Details.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(l.IpAddress) && l.IpAddress.Contains(kw, System.StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ViewBag.Search = search; // ส่งคำค้นไปยัง View
            ViewBag.RoleFilter = roleFilter ?? "all"; // ส่งตัวกรองบทบาทไปยัง View
            ViewBag.ActionFilter = actionFilter ?? "all"; // ส่งตัวกรองแอ็กชันไปยัง View
            ViewBag.FilteredCount = logs.Count; // จำนวน Log ที่คัดกรองได้

            return View(logs); // ส่งรายการ Log ไปแสดงผลใน Views/Admin/AuditLogs.cshtml
        }
    }
}
```

---

# 4. AuthController.cs

```csharp
using Microsoft.AspNetCore.Authentication; // นำเข้าอินเทอร์เฟซและฟังก์ชันหลักของการพิสูจน์ตัวตน เช่น SignInAsync, SignOutAsync
using Microsoft.AspNetCore.Authentication.Cookies; // นำเข้า CookieAuthenticationDefaults สำหรับระบุ Schema คุกกี้เซสชัน
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpGet, HttpPost, RedirectToAction
using CSI402_Project_final.Models; // นำเข้า Model การล็อกอิน การสมัครสมาชิก ข้อมูลผู้ใช้ และ PetShopRepository
using System.Security.Claims; // นำเข้า Claim, ClaimsIdentity, ClaimsPrincipal สำหรับสร้างบัตรประจำตัวผู้ใช้
using System.Collections.Generic; // นำเข้า List และ Collection
using System.Linq; // นำเข้าฟังก์ชัน LINQ สำหรับเรียงลำดับและค้นหาผู้ใช้
using System.Threading.Tasks; // นำเข้า Task สำหรับรองรับการทำงาน Asynchronous (async/await)

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    public class AuthController : Controller // ประกาศคลาส AuthController สืบทอดมาจาก Controller (เปิดให้สาธารณะเข้าถึงได้)
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับตรวจสอบผู้ใช้และบันทึกประวัติล็อกอิน

        public AuthController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // กำหนดค่า Repository ให้ตัวแปรภายในคลาส
        }

        // หน้า Login (GET) - แสดงหน้าเข้าสู่ระบบเสมอเพื่อให้แต่ละแท็บล็อกอินแยกบทบาทได้อย่างอิสระ
        [HttpGet] // ระบุว่ารับคำขอด้วย HTTP GET
        public IActionResult Login( // Action แสดงหน้าฟอร์มเข้าสู่ระบบ
            string? returnUrl = null, // URL ที่ต้องการส่งกลับหลังจากล็อกอินเสร็จ
            bool suspended = false, // ตัวบ่งชี้ว่าถูกส่งมาเพราะบัญชีถูกสั่งระงับหรือไม่
            string? u = null, // ชื่อผู้ใช้ที่ถูกระงับ
            string? reason = null, // เหตุผลในการระงับ
            string? until = null) // ระยะเวลาที่ถูกระงับ
        {
            if (suspended || !string.IsNullOrEmpty(u)) // ตรวจสอบว่าบัญชีนี้ติดสถานะถูกระงับหรือไม่
            {
                var dbUser = !string.IsNullOrEmpty(u) ? _repo.FindUserByUsernameOrEmail(u) : null; // ค้นหาข้อมูลผู้ใช้ในฐานข้อมูล
                if (dbUser != null && !dbUser.IsActive) // หากพบผู้ใช้และสถานะ IsActive เป็นเท็จ
                {
                    ViewBag.SuspendedUser = dbUser.Username; // ส่งชื่อผู้ใช้ที่ถูกระงับไปยัง View
                    ViewBag.SuspendedReason = dbUser.SuspensionReason ?? reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ"; // ส่งเหตุผล
                    ViewBag.SuspendedUntil = dbUser.SuspensionDisplayUntil; // ส่งเวลาสิ้นสุดการระงับ
                }
                else if (suspended) // กรณีไม่มีใน DB แต่ส่ง Flag ระงับมา
                {
                    ViewBag.SuspendedUser = u ?? "บัญชีของคุณ";
                    ViewBag.SuspendedReason = reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
                    ViewBag.SuspendedUntil = until ?? "ระงับถาวร";
                }
            }

            ViewBag.ReturnUrl = returnUrl; // ส่ง returnUrl ไปยัง View
            ViewBag.AllDemoUsers = GetDemoRoleUsers(); // ดึงรายชื่อผู้ใช้เดโมทุกบทบาทมาแสดงบนปุ่ม Quick Login ในหน้าเว็บ
            return View(new LoginViewModel { ReturnUrl = returnUrl }); // ส่งโมเดลไปยัง Views/Auth/Login.cshtml
        }

        private List<SystemUser> GetDemoRoleUsers() // ฟังก์ชัน Helper ภายในสำหรับจัดเตรียมรายชื่อบัญชีทดสอบ
        {
            var systemUsernames = new[] // กำหนดชื่อบัญชีระบบมาตรฐานที่ต้องการนำมาแสดง
            {
                "owner", "admin", "marketing", "inventory", "order",
                "shipping", "accounting", "support", "customer"
            };
            var allUsers = _repo.Users; // ดึงผู้ใช้ทั้งหมด
            foreach (var u in allUsers) // วนลูปตรวจสอบการระงับบัญชี
            {
                _repo.CheckAndRefreshUserSuspension(u); // ปลดระงับอัตโนมัติหากครบกำหนดเวลา
            }
            allUsers = _repo.Users; // รีเฟรชข้อมูลผู้ใช้
            var prioritized = allUsers // กรองและจัดเรียงผู้ใช้ระบบ
                .Where(u => systemUsernames.Contains(u.Username.ToLowerInvariant()))
                .OrderByDescending(u => u.Role) // จัดเรียงตามระดับสิทธิ์ Role
                .ToList();
            return prioritized.Any() ? prioritized : allUsers.Take(10).ToList(); // ส่งคืนรายการผู้ใช้เดโม
        }

        // การประมวลผล Login (POST)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        [IgnoreAntiforgeryToken] // ยกเว้นการตรวจสอบ Antiforgery Token เพื่อความคล่องตัวในการทดสอบสลับแท็บ
        public async Task<IActionResult> Login(LoginViewModel model) // Action รับข้อมูลล็อกอิน (Username, Password, RememberMe)
        {
            ViewBag.AllDemoUsers = GetDemoRoleUsers(); // เตรียมปุ่มเดโมส่งกลับไปกรณีล็อกอินไม่ผ่าน

            if (!ModelState.IsValid) // ตรวจสอบความถูกต้องของฟอร์ม (Validation)
            {
                return View(model); // หากกรอกไม่ครบ ส่งกลับไปหน้าเดิมพร้อมแสดงข้อความเตือน
            }

            var candidate = _repo.FindUserByUsernameOrEmail(model.Username); // ค้นหาผู้ใช้จาก Username หรือ Email
            if (candidate != null && candidate.Password == model.Password && !candidate.IsActive) // ตรวจสอบว่ารหัสผ่านถูกแต่บัญชีถูกสั่งระงับหรือไม่
            {
                ViewBag.SuspendedUser = candidate.Username; // ส่งชื่อผู้ใช้
                ViewBag.SuspendedReason = candidate.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ"; // ส่งเหตุผล
                ViewBag.SuspendedUntil = candidate.SuspensionDisplayUntil; // ส่งระยะเวลา
                ModelState.AddModelError(string.Empty, $"บัญชี '{candidate.Username}' ถูกระงับการใช้งาน ({candidate.SuspensionDisplayUntil}) เนื่องจาก: {ViewBag.SuspendedReason}"); // แจ้งข้อผิดพลาด
                return View(model); // ส่งกลับหน้า Login
            }

            var user = _repo.ValidateUser(model.Username, model.Password); // ตรวจสอบความถูกต้องของชื่อผู้ใช้และรหัสผ่าน
            if (user == null) // หากไม่ผ่าน
            {
                ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง"); // แจ้งเตือนรหัสผ่านผิด
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe); // ออกคุกกี้เซสชันและกำหนดสิทธิ์ Claims
            _repo.MergeGuestCartToUser(user.Id); // ดึงรายการสินค้าที่หยิบใส่ตะกร้าไว้ตอนเป็น Guest เข้าสู่ตะกร้าของ User ในฐานข้อมูล
            _repo.LogAudit("LOGIN", "user", user.Id.ToString(), $"เข้าสู่ระบบด้วย Username '{user.Username}'", user); // บันทึกประวัติการล็อกอินลงใน Audit Log

            TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName); // แสดงข้อความต้อนรับ
            TempData["SuccessTargetRole"] = user.Role.ToString(); // ส่งบทบาท
            TempData["SuccessTargetUser"] = user.Username; // ส่งชื่อผู้ใช้

            return RedirectToRoleHome(user.Role.ToString(), user.Username); // ส่งผู้ใช้ไปยังหน้าหลักของบทบาทตนเอง
        }

        // Quick Demo Login สำหรับทดสอบคลิกเดียว (ตรวจสอบสถานะการระงับบัญชีก่อนอนุญาตให้เข้าสู่ระบบ)
        [HttpGet] // รองรับคำขอ HTTP GET
        [HttpPost] // รองรับคำขอ HTTP POST
        [IgnoreAntiforgeryToken] // ยกเว้น Antiforgery Token
        public async Task<IActionResult> QuickLogin(string? username = null, string? returnUrl = null) // Action เข้าสู่ระบบคลิกเดียว
        {
            if (string.IsNullOrWhiteSpace(username)) return RedirectToAction(nameof(Login)); // ถ้าไม่ระบุชื่อผู้ใช้ ส่งกลับหน้า Login

            var user = _repo.FindUserByUsernameOrEmail(username); // ค้นหาผู้ใช้จาก Username
            if (user == null) // ถ้าไม่พบ
            {
                var info = DemoUserHelper.GetDemoUserInfo(username); // ค้นหาจากชื่อบทบาทผ่าน DemoUserHelper
                user = _repo.FindUserByUsernameOrEmail(info.Username); // ดึงผู้ใช้ตามบทบาทนั้น
            }

            if (user == null) return RedirectToAction(nameof(Login)); // ถ้ายังไม่พบ ส่งกลับหน้า Login

            // หากบัญชีถูกระงับอยู่ ห้ามเข้าสู่ระบบเด็ดขาด และแสดงเหตุผลพร้อมระยะเวลาที่ถูกระงับ
            if (!user.IsActive) // ถ้าบัญชีถูกระงับ
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // สั่งออกจากระบบทันที
                return RedirectToAction(nameof(Login), new // ส่งกลับหน้า Login พร้อมพารามิเตอร์แสดงการระงับ
                {
                    suspended = true,
                    u = user.Username,
                    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
                    until = user.SuspensionDisplayUntil
                });
            }

            await SignInUserAsync(user, isPersistent: true); // ออกคุกกี้เซสชัน
            _repo.MergeGuestCartToUser(user.Id); // รวมตะกร้า
            _repo.LogAudit("QUICK_LOGIN", "user", user.Id.ToString(), $"Quick Login '{user.Username}'", user); // บันทึก Audit Log

            TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName); // ข้อความต้อนรับ
            TempData["SuccessTargetRole"] = user.Role.ToString();
            TempData["SuccessTargetUser"] = user.Username;

            return RedirectToRoleHome(user.Role.ToString(), user.Username); // ส่งไปยังหน้าแรกของบทบาทนั้น
        }

        // Multi-Role Switcher: สลับบทบาทผู้ใช้แบบเรียลไทม์ (หากบัญชีของบทบาทนั้นถูกระงับ จะไม่อนุญาตให้เข้าใช้งาน)
        [HttpGet] // รับคำขอ HTTP GET
        public async Task<IActionResult> SwitchRole(string role, string? returnUrl = null) // Action สลับบทบาททันที
        {
            var info = DemoUserHelper.GetDemoUserInfo(role); // ค้นหาบัญชีของบทบาทนั้น
            var user = !string.IsNullOrEmpty(info.Username)
                ? _repo.FindUserByUsernameOrEmail(info.Username)
                : null;

            if (user != null && !user.IsActive) // หากบัญชีของบทบาทนั้นถูกระงับอยู่
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // สั่ง SignOut คุกกี้เดิมออก
                return RedirectToAction(nameof(Login), new // ส่งกลับหน้า Login พร้อมแจ้งการระงับ
                {
                    suspended = true,
                    u = user.Username,
                    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
                    until = user.SuspensionDisplayUntil
                });
            }

            if (user != null) // หากพบบัญชีและสถานะปกติ
            {
                await SignInUserAsync(user, isPersistent: true); // ออกคุกกี้สิทธิ์ของบทบาทใหม่
                _repo.MergeGuestCartToUser(user.Id); // รวมตะกร้า
                _repo.LogAudit("ROLE_SWITCH", "user", user.Id.ToString(), $"สลับบทบาทเป็น '{user.Username}' ({info.RoleName})", user); // บันทึกประวัติการสลับบทบาท
                TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName); // ข้อความต้อนรับ
                TempData["SuccessTargetRole"] = info.RoleName;
                TempData["SuccessTargetUser"] = user.Username;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) // ตรวจสอบ returnUrl
            {
                return LocalRedirect(returnUrl); // ส่งกลับ URL หน้าเดิม
            }

            return RedirectToRoleHome(info.RoleName); // ส่งกลับหน้าหลักของบทบาทที่เพิ่งสลับไป
        }

        // หน้า Register (GET) - แสดงหน้าสมัครสมาชิกเสมอโดยไม่เด้งกลับหน้าหลักแม้แท็บอื่นจะล็อกอินอยู่
        [HttpGet] // รับคำขอ HTTP GET
        public IActionResult Register() // Action แสดงหน้าสมัครสมาชิก
        {
            return View(new RegisterViewModel()); // ส่งโมเดลว่างไปแสดงผลใน Views/Auth/Register.cshtml
        }

        // การประมวลผล Register (POST)
        [HttpPost] // รับคำขอ HTTP POST
        [IgnoreAntiforgeryToken] // ยกเว้น Antiforgery Token
        public async Task<IActionResult> Register(RegisterViewModel model) // Action รับข้อมูลสมัครสมาชิกจากฟอร์ม
        {
            if (!ModelState.IsValid) // ตรวจสอบความถูกต้องของฟอร์ม
            {
                return View(model); // หากกรอกไม่ถูกต้อง ส่งกลับหน้าเดิม
            }

            var newUser = _repo.RegisterCustomer(model); // สั่ง Repository ลงทะเบียนลูกค้าใหม่
            if (newUser == null) // หากบันทึกไม่สำเร็จ (ชื่อผู้ใช้หรืออีเมลซ้ำ)
            {
                ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรืออีเมลนี้มีอยู่ในระบบแล้ว กรุณาใช้อันอื่น"); // แจ้งเตือนความซ้ำซ้อน
                return View(model);
            }

            await SignInUserAsync(newUser, isPersistent: true); // ล็อกอินให้อัตโนมัติทันที
            _repo.MergeGuestCartToUser(newUser.Id); // รวมตะกร้าสินค้าเดิมเข้ากับบัญชีใหม่
            TempData["SuccessMessage"] = $"สมัครสมาชิกสำเร็จ! ยินดีต้อนรับคุณ {newUser.FullName}"; // แสดงข้อความต้อนรับ
            TempData["SuccessTargetRole"] = "Customer";
            TempData["SuccessTargetUser"] = newUser.Username;

            return RedirectToAction("Index", "Home", new { role = newUser.Username }); // ส่งผู้ใช้ไปยังหน้าร้านค้าหลัก
        }

        // Logout
        [HttpPost] // รองรับ HTTP POST
        [HttpGet] // รองรับ HTTP GET
        [IgnoreAntiforgeryToken] // ยกเว้น Antiforgery Token
        public async Task<IActionResult> Logout() // Action ออกจากระบบ
        {
            if (User.Identity?.IsAuthenticated == true) // ตรวจสอบว่าเคยล็อกอินอยู่หรือไม่
            {
                _repo.LogAudit("LOGOUT", "user", User.FindFirstValue(ClaimTypes.NameIdentifier), "ออกจากระบบ"); // บันทึกประวัติการออกจากระบบลงใน Audit Log
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // เพิกถอนและลบ Cookie Authentication ออกจากเบราว์เซอร์
            TempData["SuccessMessage"] = "คุณได้ออกจากระบบเรียบร้อยแล้ว"; // แจ้งข้อความสำเร็จ
            TempData["SuccessTargetRole"] = "Guest";
            TempData["SuccessTargetUser"] = "";
            return RedirectToAction("Index", "Home", new { role = "Guest" }); // ส่งกลับไปยังหน้า Home ในสถานะ Guest
        }

        // หน้า Access Denied เมื่อไม่มีสิทธิ์
        [HttpGet] // รับคำขอ HTTP GET
        public IActionResult AccessDenied() // Action แสดงหน้าต่างแจ้งเตือนเมื่อไม่มีสิทธิ์เข้าถึง (403 Forbidden)
        {
            return View(); // เรนเดอร์หน้า Views/Auth/AccessDenied.cshtml
        }

        private static string CleanName(string? fullName) // ฟังก์ชันตัดคำที่ไม่จำเป็นออกจากชื่อ (เช่น ข้อความในวงเล็บ)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(fullName, @"\s*\([^)]*\)", "").Trim(); // ใช้ Regex ตัดวงเล็บออก
        }

        private static string FormatWelcomeMessage(string? fullName) // ฟังก์ชันจัดรูปแบบข้อความต้อนรับ
        {
            var clean = CleanName(fullName); // ล้างชื่อให้สะอาด
            if (string.IsNullOrWhiteSpace(clean))
            {
                return "ยินดีต้อนรับเข้าสู่ระบบ";
            }

            if (clean.StartsWith("คุณ ")) // ตรวจสอบคำนำหน้า
            {
                clean = clean.Substring(4).Trim();
            }
            else if (clean.StartsWith("คุณ"))
            {
                clean = clean.Substring(3).Trim();
            }

            return string.IsNullOrWhiteSpace(clean) ? "ยินดีต้อนรับเข้าสู่ระบบ" : $"ยินดีต้อนรับคุณ {clean}"; // สร้างข้อความต้อนรับที่ไพเราะ
        }

        private async Task SignInUserAsync(SystemUser user, bool isPersistent) // ฟังก์ชัน Helper ออก Claims และสร้าง Cookie Authentication Ticket
        {
            var cleanFullName = CleanName(user.FullName); // ล้างชื่อผู้ใช้
            if (string.IsNullOrWhiteSpace(cleanFullName)) cleanFullName = user.Username;

            var claims = new List<Claim> // สร้างชุด Claims บันทึกตัวตน
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // บันทึก User ID
                new Claim(ClaimTypes.Name, user.Username), // บันทึก Username
                new Claim(ClaimTypes.Role, user.Role.ToString()), // บันทึก Role หลัก
                new Claim("FullName", cleanFullName), // บันทึกชื่อ-สกุล
                new Claim(ClaimTypes.Email, user.Email), // บันทึกอีเมล
                new Claim("Phone", user.Phone) // บันทึกเบอร์โทร
            };

            // Multi-Role Support: สำหรับ Owner และ Admin ให้สิทธิ์เข้าถึงทุก Role พร้อมกันได้ เพื่อให้เปิดหลายแท็บ/หลายหน้าต่างทดสอบได้ทันที
            if (user.Role == UserRole.Owner || user.Role == UserRole.Admin) // ตรวจสอบว่าผู้ใช้เป็น Owner หรือ Admin หรือไม่
            {
                foreach (UserRole r in Enum.GetValues(typeof(UserRole))) // วนลูปทุกบทบาทในระบบ
                {
                    if (r != user.Role) // ถ้าไม่ใช่บทบาทหลักของตนเอง
                    {
                        claims.Add(new Claim(ClaimTypes.Role, r.ToString())); // เพิ่ม Claim สิทธิ์ของ Role นั้นเข้าไปด้วย เพื่อให้เปิดดูได้ทุกหน้าจอพร้อมกันโดยไม่ติด Access Denied
                    }
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); // สร้าง ClaimsIdentity ด้วย Cookie Scheme
            var authProperties = new AuthenticationProperties // กำหนดคุณสมบัติการยืนยันตัวตน
            {
                IsPersistent = isPersistent, // จดจำการเข้าสู่ระบบ
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8) // กำหนดอายุคุกกี้ 8 ชั่วโมง
            };

            await HttpContext.SignInAsync( // บันทึก Cookie ลงในเบราว์เซอร์
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // Helper: นำทางไปยังหน้าหลักของแต่ละ Role (รองรับแยก Username ของแต่ละแท็บอิสระ)
        private IActionResult RedirectToRoleHome(string? role, string? username = null) // ฟังก์ชัน Helper นำทางผู้ใช้ไปยังหน้าหลักตามบทบาท
        {
            var canonical = DemoUserHelper.GetDemoUserInfo(role); // ค้นหาข้อมูลบทบาท
            string tabRoleKey = (!string.IsNullOrWhiteSpace(username) &&
                                 !string.Equals(username, canonical.Username, System.StringComparison.OrdinalIgnoreCase))
                ? username
                : (role ?? "Customer"); // กำหนดคีย์ประจำแท็บ

            return role switch // ใช้ switch expression เลือกว่าจะ Redirect ไปไหนตามบทบาท
            {
                nameof(UserRole.InventoryStaff) => RedirectToAction("Index", "Inventory", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายคลัง ไปที่ Inventory/Index
                nameof(UserRole.OrderStaff) => RedirectToAction("Index", "Order", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายคำสั่งซื้อ ไปที่ Order/Index
                nameof(UserRole.ShippingStaff) => RedirectToAction("Index", "Shipping", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายจัดส่ง ไปที่ Shipping/Index
                nameof(UserRole.MarketingStaff) => RedirectToAction("Index", "Marketing", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายการตลาด ไปที่ Marketing/Index
                nameof(UserRole.AccountingStaff) => RedirectToAction("Index", "Accounting", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายบัญชี ไปที่ Accounting/Index
                nameof(UserRole.ITSupport) => RedirectToAction("Index", "Support", new { role = tabRoleKey }), // ถ้าเป็นฝ่ายไอทีซัพพอร์ต ไปที่ Support/Index
                nameof(UserRole.Admin) => RedirectToAction("Index", "Admin", new { role = tabRoleKey }), // ถ้าเป็นผู้ดูแลระบบ ไปที่ Admin/Index
                nameof(UserRole.Owner) => RedirectToAction("Index", "Owner", new { role = tabRoleKey }), // ถ้าเป็นผู้บริหาร ไปที่ Owner/Index
                _ => RedirectToAction("Index", "Home", new { role = tabRoleKey }) // ถ้าเป็นลูกค้าหรือทั่วไป ไปที่ Home/Index
            };
        }
    }
}
```


---

# 5. CartController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize] ในขั้นตอน Checkout และ SubmitOrder
using Microsoft.AspNetCore.Hosting; // นำเข้า IWebHostEnvironment สำหรับอ้างอิงตำแหน่งเซฟสลิปโอนเงินใน wwwroot
using Microsoft.AspNetCore.Http; // นำเข้า IFormFile สำหรับรับไฟล์รูปภาพสลิปการโอนเงินจากฟอร์ม
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpGet, HttpPost, Json, Redirect
using Microsoft.AspNetCore.SignalR; // นำเข้า IHubContext สำหรับส่งการแจ้งเตือน Real-Time ทั่วทั้งระบบ
using CSI402_Project_final.Helpers; // นำเข้า FileUploadHelper (บันทึกสลิป) และ PromptPayHelper (คำนวณ QR Code พร้อมเพย์)
using CSI402_Project_final.Hubs; // นำเข้า PetShopHub สำหรับการเชื่อมต่อ SignalR WebSocket
using CSI402_Project_final.Models; // นำเข้า Model ตะกร้าสินค้า Cart, Product, Promotion, PetShopRepository
using System.Linq; // นำเข้า LINQ เช่น .Any(), .Select(), .Where()
using System.Threading.Tasks; // นำเข้า Task สำหรับรองรับ Asynchronous (async/await)

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    public class CartController : Controller // ประกาศคลาส CartController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับจัดการตะกร้าสินค้าและคำสั่งซื้อ
        private readonly IHubContext<PetShopHub> _hubContext; // ฟิลด์เก็บ SignalR Hub Context สำหรับส่งสัญญาณ Real-Time
        private readonly IWebHostEnvironment _env; // ฟิลด์เก็บสภาพแวดล้อมโฮสต์เพื่อหาโฟลเดอร์เก็บไฟล์สลิป

        public CartController(PetShopRepository repo, IHubContext<PetShopHub> hubContext, IWebHostEnvironment env) // Constructor รับ DI จากระบบ
        {
            _repo = repo; // กำหนดค่า Repository
            _hubContext = hubContext; // กำหนดค่า Hub Context
            _env = env; // กำหนดค่า WebHostEnvironment
        }

        // หน้าตะกร้าสินค้า (Req 3, 4, 11, 12, 13, 14, 15, 16)
        public IActionResult Index() // Action แสดงหน้าตะกร้าสินค้าผ่าน HTTP GET
        {
            ViewBag.ActivePromotions = _repo.GetValidPromotions(); // ดึงรายการแคมเปญโปรโมชั่นที่เปิดใช้งานอยู่ ใส่ใน ViewBag
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            return View(_repo.CurrentCart); // ส่งตะกร้าสินค้าของผู้ใช้ปัจจุบันเป็น Model ไปเรนเดอร์ใน Views/Cart/Index.cshtml
        }

        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> Add(int productId, int quantity = 1, string returnUrl = "/Cart") // Action หยิบสินค้าลงตะกร้าแบบ Asynchronous
        {
            // ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง (Guest ไม่สามารถหยิบสินค้าลงในตะกร้าได้)
            if (User?.Identity?.IsAuthenticated != true) // ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง ถ้ายังไม่ล็อกอินจะเข้าสู่เงื่อนไขนี้
            {
                string targetLoginUrl = Url.Action("Login", "Auth", new { returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl }) ?? "/Auth/Login"; // สร้าง URL หน้า Login พร้อมแนบ returnUrl
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || // ตรวจสอบว่าส่งมาจาก AJAX หรือไม่
                    Request.Headers.Accept.ToString().Contains("application/json")) // หรือขอข้อมูลเป็น JSON
                {
                    return Json(new // ส่งผลลัพธ์เป็น JSON ตอบกลับให้หน้าบ้านรู้ว่าต้องล็อกอิน
                    {
                        success = false, // ไม่สำเร็จ
                        requireLogin = true, // ต้องล็อกอินก่อน
                        redirectUrl = targetLoginUrl, // ส่ง URL หน้าล็อกอิน
                        message = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า" // ข้อความแจ้งเตือน
                    });
                }
                TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า"; // กรณีฟอร์มปกติ ตั้งค่า Flash Message
                return Redirect(targetLoginUrl); // Redirect ไปหน้า Login
            }

            int newCount = _repo.AddToCart(productId, quantity); // สั่ง Repository นำสินค้าเข้าตะกร้า และรับจำนวนสินค้าชิ้นนี้ในตะกร้ากลับมา
            var product = _repo.GetProductById(productId); // คิวรีข้อมูลสินค้าเพื่อนำชื่อและสต็อกคงเหลือมาเตรียมบรอดแคสต์
            string productName = product?.Name ?? "สินค้า"; // ดึงชื่อสินค้า

            // SignalR: กระจายการอัปเดตจำนวนคนใส่ตะกร้าและสต็อกคงเหลือแบบ Real-Time (Req 8) ในพื้นหลังโดยไม่บล็อกการตอบกลับ
            _ = Task.Run(async () => // เปิดเธรดทำงานในพื้นหลัง (Background Task) ไม่ให้บล็อกการตอบกลับของ Action
            {
                try
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveCartCountUpdate", productId, newCount); // บรอดแคสต์ยอดคนใส่ตะกร้าไปยังทุกหน้าจอ
                    if (product != null) // ตรวจสอบว่ามีข้อมูลสินค้า
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity); // บรอดแคสต์สต็อกคงเหลือล่าสุด
                        if (product.IsLowStock) // ตรวจสอบว่าสต็อกต่ำกว่าเกณฑ์หรือไม่
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity); // ส่งสัญญาณเตือนภัย Low Stock Alert ไปยังหน้าจอฝ่ายคลัง
                        }
                    }
                }
                catch { } // ดักจับข้อผิดพลาด Socket เพื่อไม่ให้กระทบระบบหลัก
            });

            // รองรับ AJAX ไม่ต้องโหลดหน้าใหม่ เพื่อความลื่นไหล (Smooth User Experience)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || // ตรวจสอบว่าส่งมาจาก AJAX หรือไม่
                Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new // ส่ง JSON ผลลัพธ์กลับไปให้ JavaScript นำไปอัปเดต DOM หน้าจอทันที
                {
                    success = true, // สถานะสำเร็จ
                    productId = productId, // รหัสสินค้า
                    productName = productName, // ชื่อสินค้า
                    cartCount = _repo.CurrentCart.TotalItemCount, // ยอดรวมสินค้าในตะกร้าสำหรับอัปเดตตัวเลข Badge
                    productCartCount = newCount, // ยอดสินค้าชิ้นนี้ในตะกร้า
                    remainingStock = product?.StockQuantity ?? 0, // สต็อกคงเหลือ
                    isOutOfStock = (product?.StockQuantity ?? 0) <= 0, // หมดสต็อกหรือไม่
                    message = $"เพิ่ม \"{productName}\" ลงในตะกร้าเรียบร้อยแล้ว" // ข้อความแจ้งเตือน Toast
                });
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)) // กรณีฟอร์มปกติ ตรวจสอบ returnUrl
            {
                return LocalRedirect(returnUrl); // ส่งกลับ URL หน้าเดิม
            }
            return RedirectToAction(nameof(Index)); // ถ้าไม่มี returnUrl ส่งไปหน้าตะกร้าสินค้า
        }

        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> Update(int productId, int quantity) // Action เมธอดปรับปรุงจำนวนสินค้าในตะกร้า
        {
            _repo.UpdateCartItem(productId, quantity); // สั่ง Repository ปรับปรุงจำนวนสินค้าในตะกร้า
            var product = _repo.GetProductById(productId); // ค้นหาข้อมูลสินค้าเพื่อดึงสต็อก
            if (product != null) // ถ้าพบสินค้า
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity); // ส่งสัญญาณ SignalR ปรับสต็อกสดบนทุกหน้าจอ
                if (product.IsLowStock) // ตรวจสอบสถานะ Low Stock
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity); // ส่งเตือนภัย Low Stock Alert
                }
            }
            return RedirectToAction(nameof(Index)); // โหลดหน้าตะกร้าใหม่เพื่อคำนวณโปรโมชั่นและยอดเงินใหม่
        }

        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> Remove(int productId) // Action เมธอดลบรายการสินค้าออกจากตะกร้า
        {
            _repo.RemoveFromCart(productId); // สั่ง Repository ลบสินค้ารายการนี้ออกจากตะกร้า
            var product = _repo.GetProductById(productId); // ค้นหาข้อมูลสินค้า
            if (product != null) // ถ้าพบสินค้า
            {
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity); // บรอดแคสต์อัปเดตสต็อกสด
            }
            return RedirectToAction(nameof(Index)); // รีเฟรชหน้าตะกร้าสินค้า
        }

        // หน้าสรุปคำสั่งซื้อและชำระเงิน (ต้องเข้าสู่ระบบก่อนเพื่อเชื่อม UserId ลง DB)
        [Authorize] // จำกัดสิทธิ์เฉพาะผู้ใช้ที่เข้าสู่ระบบแล้ว
        public IActionResult Checkout() // Action แสดงหน้าสรุปคำสั่งซื้อและชำระเงินผ่าน HTTP GET
        {
            var cart = _repo.CurrentCart; // ดึงตะกร้าสินค้าปัจจุบัน
            if (!cart.Items.Any()) // ตรวจสอบว่ามีสินค้าหรือไม่
            {
                return RedirectToAction(nameof(Index)); // ถ้าไม่มีสินค้า ส่งกลับไปหน้าตะกร้า
            }
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.PromptPayPayload = PromptPayHelper.GeneratePayload("0812345678", cart.GrandTotal); // สร้างรหัส EMVCo PromptPay สำหรับยอดเงินสุทธิเพื่อนำไปเรนเดอร์ QR Code
            return View(cart); // ส่งตะกร้าสินค้าไปเรนเดอร์ใน Views/Cart/Checkout.cshtml
        }

        // คำนวณค่าจัดส่งและยอดสุทธิรวมตามผู้ให้บริการขนส่งแบบ Real-Time (EMS ด่วนพิเศษ +30 บาท)
        [HttpGet] // ระบุว่ารับคำขอด้วย HTTP GET
        public IActionResult GetCourierCalculation(string courier) // Action เมธอดรับชื่อบริษัทขนส่งที่ผู้ใช้เลือกผ่าน AJAX
        {
            var cart = _repo.CurrentCart; // ดึงตะกร้าสินค้า
            var activePromos = _repo.GetValidPromotions(); // ดึงโปรโมชั่นที่เปิดใช้งาน
            cart.Recalculate(activePromos); // คำนวณโปรโมชั่นและส่วนลดใหม่

            decimal surcharge = Cart.GetCourierSurcharge(courier); // คำนวณค่าธรรมเนียมพิเศษของขนส่ง (EMS +30 บาท ส่วนขนส่งอื่น 0 บาท)
            decimal shippingFee = cart.ShippingFee + surcharge; // คำนวณค่าจัดส่งรวม
            decimal grandTotal = cart.NetProductPrice + shippingFee; // คำนวณยอดชำระสุทธิ
            string payload = PromptPayHelper.GeneratePayload("0812345678", grandTotal); // สร้าง PromptPay Payload ใหม่ที่ตรงตามยอดสุทธิ
            string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={Uri.EscapeDataString(payload)}"; // สร้าง URL ลิงก์รูป QR Code ใหม่

            return Json(new // ส่งคืนข้อมูลเป็น JSON ให้ JavaScript ในหน้าบ้านเปลี่ยนตัวเลขและเปลี่ยนภาพ QR Code ทันที
            {
                success = true, // สถานะสำเร็จ
                courier = courier, // ชื่อขนส่ง
                baseShippingFee = cart.ShippingFee, // ค่าส่งฐาน
                surcharge = surcharge, // ค่าบริการเพิ่ม
                shippingFee = shippingFee, // ค่าส่งรวม
                grandTotal = grandTotal, // ยอดสุทธิ
                qrUrl = qrUrl, // URL ของ QR Code
                formattedShippingFee = shippingFee.ToString("N2"), // ค่าส่ง formatted ทศนิยม 2 ตำแหน่ง
                formattedGrandTotal = grandTotal.ToString("N2"), // ยอดสุทธิ formatted ทศนิยม 2 ตำแหน่ง
                isEms = surcharge > 0 // เป็น EMS หรือไม่
            });
        }

        // หากมีการเปิดหน้า /Cart/SubmitOrder ด้วย GET (เช่น กดรีเฟรชหน้า หรือกด Back) ให้กลับไปหน้า Checkout อย่างถูกต้อง
        [HttpGet] // รับคำขอ HTTP GET
        public IActionResult SubmitOrder() // Action ป้องกันข้อผิดพลาดกรณีเข้าหน้า SubmitOrder ด้วย GET
        {
            return RedirectToAction(nameof(Checkout)); // ส่งผู้ใช้กลับไปหน้า Checkout ทันที
        }

        [Authorize] // จำกัดสิทธิ์เฉพาะผู้ใช้ที่เข้าสู่ระบบ
        [HttpPost] // รับคำขอ HTTP POST
        public async Task<IActionResult> SubmitOrder( // Action เมธอดประมวลผลการสั่งซื้อ
            string customerName, // ชื่อผู้รับ
            string customerPhone, // เบอร์โทรศัพท์
            string shippingAddress, // ที่อยู่จัดส่ง
            string paymentMethod = "โอนผ่านธนาคาร / PromptPay", // วิธีชำระเงิน
            string courier = "Flash Express", // บริษัทขนส่ง
            IFormFile? paymentSlip = null) // ไฟล์รูปสลิปการโอนเงิน
        {
            try // เปิดบล็อก try
            {
                string? slipUrl = null; // ตัวแปรเก็บ URL ของสลิป
                if (paymentSlip != null && paymentSlip.Length > 0) // ตรวจสอบว่ามีการแนบไฟล์สลิปมาหรือไม่
                {
                    var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips"); // บันทึกไฟล์รูปสลิปลงใน wwwroot/uploads/slips
                    if (uploadErr != null) // หากบันทึกรูปไม่สำเร็จ
                    {
                        TempData["ErrorMessage"] = uploadErr; // บันทึกข้อความผิดพลาด
                        return RedirectToAction(nameof(Checkout)); // ส่งกลับหน้า Checkout
                    }
                    slipUrl = uploadedUrl; // กำหนดค่า URL สลิป
                }

                var username = User.Identity?.Name ?? ""; // ดึงชื่อผู้ใช้ปัจจุบัน
                var (newOrder, errorMessage) = _repo.CheckoutCurrentCart( // เรียกกระบวนการสั่งซื้อหลักใน Repository
                    username, customerName, customerPhone, shippingAddress, paymentMethod, slipUrl, courier); // ตัดสต็อก, สร้างออเดอร์ลง DB, เคลียร์ตะกร้า และบันทึก Audit Log

                if (newOrder == null) // หากสั่งซื้อไม่สำเร็จ (เช่น สต็อกไม่พอ)
                {
                    TempData["ErrorMessage"] = errorMessage ?? "ไม่สามารถดำเนินการสั่งซื้อได้"; // แจ้งข้อผิดพลาด
                    return RedirectToAction(nameof(Checkout)); // กลับหน้า Checkout
                }

                // SignalR: แจ้งเตือนฝ่ายคำสั่งซื้อ/บัญชี แบบ Real-Time ทันที (มี try-catch เพื่อป้องกันข้อผิดพลาดของ socket ไม่ให้กระทบการสั่งซื้อ)
                try // ครอบ try-catch ป้องกันข้อผิดพลาดของ Socket
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", newOrder.OrderId, newOrder.CustomerName, newOrder.GrandTotal); // ส่งสัญญาณเตือนคำสั่งซื้อใหม่ไปยังฝ่ายจัดเตรียมสินค้า ฝ่ายบัญชี และผู้บริหาร

                    // ตรวจสอบสินค้าที่เหลือเพื่อเตือน Low Stock (Req 18)
                    var productIds = newOrder.Items.Select(i => i.ProductId).ToList(); // ดึงรายการรหัสสินค้าในคำสั่งซื้อ
                    var affectedProducts = _repo.Products.Where(p => productIds.Contains(p.Id)).ToList(); // ค้นหาสินค้าเหล่านั้นในฐานข้อมูล
                    foreach (var prod in affectedProducts) // วนลูปตรวจสอบสินค้าทุกชิ้นที่ถูกตัดสต็อก
                    {
                        if (prod.IsLowStock) // ถ้าสต็อกต่ำกว่าเกณฑ์
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", prod.Name, prod.StockQuantity); // ส่งสัญญาณเตือนภัย Low Stock Alert ไปยังหน้าจอฝ่ายคลังสินค้าทันที
                        }
                    }
                }
                catch
                {
                    // การส่ง SignalR ไม่ควรทำให้กระบวนการสั่งซื้อล้มเหลว
                }

                return RedirectToAction("Orders", "Account", new { successOrderId = newOrder.OrderId }); // ส่งผู้ใช้ไปยังหน้าประวัติคำสั่งซื้อพร้อมรหัสออเดอร์เพื่อแสดง Modal ความสำเร็จ
            }
            catch (Exception ex) // ดักจับ Unhandled Exception
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการสั่งซื้อ: {ex.Message}"; // แจ้งข้อผิดพลาด
                return RedirectToAction(nameof(Checkout)); // ส่งกลับหน้า Checkout
            }
        }
    }
}
```

---

# 6. HomeController.cs

```csharp
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, ResponseCache, View, NotFound
using CSI402_Project_final.Models; // นำเข้า Model แคตตาล็อกสินค้า Product, PetCategory, ProductCategory, AgeStage, PetShopRepository
using System.Linq; // นำเข้าคำสั่ง LINQ สำหรับการคัดกรองและเรียงลำดับสินค้า

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    public class HomeController : Controller // ประกาศคลาส HomeController สำหรับควบคุมหน้าร้านค้าหลัก
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับคิวรีสินค้าในแคตตาล็อก

        public HomeController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // กำหนดค่า Repository ให้ตัวแปรภายในคลาส
        }

        // หน้าร้านหลัก พร้อมฟิลเตอร์แยกประเภทสัตว์ (Req 1), หมวดหมู่ของใช้/อาหาร, น้ำหนัก (Req 2), ช่วงวัย/โรค (Req 7, 9, 10)
        public IActionResult Index( // Action เมธอดแสดงหน้าร้านค้าหลักผ่าน HTTP GET
            PetCategory? petType = null, // ตัวกรองประเภทสัตว์เลี้ยง (สุนัข, แมว, อื่นๆ)
            ProductCategory? category = null, // ตัวกรองหมวดหมู่สินค้า (อาหาร, ของใช้, สุขอนามัย)
            AgeStage? ageStage = null, // ตัวกรองช่วงวัย (ลูกสัตว์, โตเต็มวัย, สูงวัย, ทุกช่วงวัย)
            string? specialNeed = null, // ตัวกรองความต้องการพิเศษ/โรคเฉพาะ (เช่น ทำหมัน, บำรุงไต, บำรุงขน)
            double? maxWeight = null, // ตัวกรองน้ำหนักแพ็กเกจสินค้าสูงสุด (กิโลกรัม)
            string? search = null) // คำค้นหาข้อความ
        {
            var allCatalog = _repo.QueryProducts().ToList(); // ดึงรายการสินค้าทั้งหมดในแคตตาล็อก

            // คำนวณจำนวนสินค้าแยกตามหมวดหมู่ (ตามประเภทสัตว์ที่เลือกอยู่)
            var baseForCategoryCount = petType.HasValue // ตรวจสอบว่ามีการเลือกประเภทสัตว์หรือไม่
                ? allCatalog.Where(p => p.PetType == petType.Value).ToList() // ถ้าเลือกประเภทสัตว์ ให้ใช้รายการสินค้าเฉพาะสัตว์นั้นเป็นฐานในการนับ
                : allCatalog; // ถ้าไม่เลือก ให้ใช้สินค้าทั้งหมดเป็นฐานในการนับ

            ViewBag.AllCategoryCount = baseForCategoryCount.Count; // จำนวนสินค้าทั้งหมดในประเภทสัตว์นั้น
            ViewBag.FoodCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Food); // จำนวนสินค้าหมวดอาหาร
            ViewBag.SupplyCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Supply); // จำนวนสินค้าหมวดของใช้สัตว์เลี้ยง
            ViewBag.HygieneCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Hygiene); // จำนวนสินค้าหมวดสุขอนามัย

            IEnumerable<Product> products = allCatalog; // เตรียมตัวแปรเริ่มต้นสำหรับคัดกรองสินค้า

            if (petType.HasValue) // ถ้าเลือกประเภทสัตว์
                products = products.Where(p => p.PetType == petType.Value); // กรองตามประเภทสัตว์

            if (category.HasValue) // ถ้าเลือกหมวดหมู่
                products = products.Where(p => p.Category == category.Value); // กรองตามหมวดหมู่

            if (ageStage.HasValue && ageStage.Value != AgeStage.AllAges) // ถ้าเลือกช่วงวัยเจาะจง
                products = products.Where(p => p.AgeStage == ageStage.Value || p.AgeStage == AgeStage.AllAges); // กรองตามช่วงวัย และรวมสินค้าที่ทานได้ทุกช่วงวัยด้วย

            if (!string.IsNullOrWhiteSpace(specialNeed)) // ถ้าเลือกความต้องการพิเศษ
                products = products.Where(p => p.SpecialNeed.Contains(specialNeed, System.StringComparison.OrdinalIgnoreCase)); // กรองตามคุณสมบัติการดูแลเฉพาะ

            if (maxWeight.HasValue) // ถ้ากำหนดน้ำหนักสูงสุด
                products = products.Where(p => p.WeightKg <= maxWeight.Value); // กรองตามน้ำหนักแพ็กเกจ

            if (!string.IsNullOrWhiteSpace(search)) // ถ้ามีคำค้นหา
            {
                string term = search.Trim(); // ตัดช่องว่างคำค้น
                products = products.Where(p => // ค้นหาจากชื่อสินค้า, แบรนด์, ความต้องการพิเศษ หรือรายละเอียด
                    p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.SpecialNeed.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.ActivePromotions = _repo.GetValidPromotions(); // ส่งโปรโมชั่นที่เปิดใช้งานไปยัง View
            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount; // ส่งจำนวนสินค้าในตะกร้าปัจจุบัน
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทจำลองปัจจุบัน
            ViewBag.SelectedPetType = petType; // ส่งค่าตัวกรองสัตว์ที่เลือก
            ViewBag.SelectedCategory = category; // ส่งค่าตัวกรองหมวดหมู่ที่เลือก
            ViewBag.SelectedAgeStage = ageStage; // ส่งค่าตัวกรองช่วงวัยที่เลือก
            ViewBag.SelectedSpecialNeed = specialNeed; // ส่งค่าตัวกรองโรคเฉพาะที่เลือก
            ViewBag.SelectedMaxWeight = maxWeight; // ส่งค่าน้ำหนักสูงสุดที่เลือก
            ViewBag.SelectedSearch = search; // ส่งคำค้นหา

            return View(products.ToList()); // ส่งรายการสินค้าที่ผ่านการคัดกรองไปเรนเดอร์ใน Views/Home/Index.cshtml
        }

        // หน้ารายละเอียดสินค้า (แสดง Layer ช่วงวัย & Social proof คนใส่ตะกร้า Req 7, 8, 10)
        public IActionResult Details(int id) // Action เมธอดรับรหัสสินค้าเพื่อแสดงหน้ารายละเอียดสินค้าผ่าน HTTP GET
        {
            var allProducts = _repo.QueryProducts().ToList(); // ดึงรายการสินค้าทั้งหมด
            var product = allProducts.FirstOrDefault(p => p.Id == id); // ค้นหาสินค้าตามรหัส ID
            if (product == null) return NotFound(); // หากไม่พบ ให้ตอบกลับสถานะ HTTP 404 Not Found

            // สินค้าแนะนำในหมวดหมู่และช่วงวัยเดียวกัน (รองรับทั้งอาหารและของใช้สัตว์เลี้ยง)
            var related = allProducts // เริ่มคำนวณหาสินค้าแนะนำ
                .Where(p => p.Id != id && p.PetType == product.PetType && p.Category == product.Category) // เลือกสินค้าสัตว์ชนิดเดียวกันและหมวดเดียวกันที่ไม่ใช่ตัวเอง
                .OrderByDescending(p => p.AgeStage == product.AgeStage) // จัดลำดับให้สินค้าที่มีช่วงวัยตรงกันขึ้นก่อน
                .Take(4) // นำมา 4 รายการ
                .ToList();

            if (related.Count < 4) // หากได้สินค้าไม่ครบ 4 รายการ
            {
                var additional = allProducts // ดึงสินค้าเพิ่มเติมที่มีหมวดหมู่หรือประเภทสัตว์ตรงกัน
                    .Where(p => p.Id != id && !related.Any(r => r.Id == p.Id) && (p.Category == product.Category || p.PetType == product.PetType))
                    .Take(4 - related.Count); // เอามาเติมส่วนที่ยังขาด
                related.AddRange(additional); // รวมเข้าใน List สินค้าแนะนำ
            }

            ViewBag.RelatedProducts = related; // ส่งรายการสินค้าแนะนำไปยัง View
            ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount; // ส่งยอดสินค้าในตะกร้า
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบัน

            return View(product); // ส่งสินค้าไปแสดงผลใน Views/Home/Details.cshtml
        }

        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult SwitchRole(UserRole role, string returnUrl = "/") // Action เมธอดรับคำขอสลับบทบาทจาก Role Switcher Bar หน้าร้าน
        {
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index)); // ส่งกลับไปยัง URL หน้าเดิมอย่างปลอดภัย
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // ปิดการแคชสำหรับหน้า Error Page เพื่อให้แสดงข้อมูลล่าสุดเสมอ
        public IActionResult Error() // Action เมธอดแสดงหน้าต่างแจ้งข้อผิดพลาดระบบ
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier }); // ส่ง RequestId ไปเรนเดอร์ใน Views/Shared/Error.cshtml
        }
    }
}
```


---

# 7. InventoryController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Hosting; // นำเข้า IWebHostEnvironment สำหรับจัดการไดเรกทอรีจัดเก็บรูปภาพสินค้าใน wwwroot
using Microsoft.AspNetCore.Http; // นำเข้า IFormFile สำหรับรับไฟล์อัปโหลดรูปภาพสินค้า
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpGet, HttpPost, RedirectToAction
using Microsoft.AspNetCore.SignalR; // นำเข้า IHubContext สำหรับการบรอดแคสต์สต็อกคงเหลือและการแจ้งเตือน Low Stock Alert
using CSI402_Project_final.Helpers; // นำเข้า FileUploadHelper สำหรับบันทึกรูปภาพสินค้า
using CSI402_Project_final.Hubs; // นำเข้า PetShopHub สำหรับการเชื่อมต่อ SignalR WebSocket
using CSI402_Project_final.Models; // นำเข้า Product, ProductCategory, PetShopRepository
using System.Linq; // นำเข้าคำสั่ง LINQ สำหรับการจัดเรียงและคัดกรองสต็อก
using System.Threading.Tasks; // นำเข้า Asynchronous Task

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "InventoryStaff,Owner,Admin")] // จำกัดสิทธิ์เฉพาะพนักงานคลังสินค้า (InventoryStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class InventoryController : Controller // ประกาศคลาส InventoryController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับจัดการสินค้าและสต็อก
        private readonly IWebHostEnvironment _env; // ฟิลด์เก็บสภาพแวดล้อมโฮสต์สำหรับอ้างอิงโฟลเดอร์เซฟรูป
        private readonly IHubContext<PetShopHub> _hubContext; // ฟิลด์เก็บ SignalR Hub Context สำหรับบรอดแคสต์สต็อก

        public InventoryController(PetShopRepository repo, IWebHostEnvironment env, IHubContext<PetShopHub> hubContext) // Constructor รับ DI
        {
            _repo = repo; // ผูก Repository
            _env = env; // ผูก WebHostEnvironment
            _hubContext = hubContext; // ผูก SignalR Hub Context
        }

        // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18) พร้อมตัวกรอง Low Stock / Out of Stock และกรองหมวดหมู่ของใช้/อาหาร
        public IActionResult Index(string? stockFilter = "all", string? categoryFilter = "all", string? search = null) // Action เมธอดแสดงแดชบอร์ดคลังสินค้า
        {
            var allProducts = _repo.Products.OrderBy(p => p.StockQuantity).ToList(); // ดึงสินค้าทั้งหมด เรียงจากสต็อกน้อยไปมากเพื่อให้เห็นสินค้าใกล้หมดก่อน

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.TotalProducts = allProducts.Count; // นับจำนวนสินค้าทั้งหมด
            ViewBag.LowStockCount = allProducts.Count(p => p.IsLowStock); // นับจำนวนสินค้าที่สต็อกต่ำกว่าเกณฑ์
            ViewBag.OutOfStockCount = allProducts.Count(p => p.IsOutOfStock); // นับจำนวนสินค้าที่หมดสต็อก
            ViewBag.NormalStockCount = allProducts.Count(p => !p.IsLowStock && !p.IsOutOfStock); // นับจำนวนสินค้าที่มีสต็อกปกติ

            ViewBag.FoodCount = allProducts.Count(p => p.Category == ProductCategory.Food); // จำนวนสินค้าหมวดอาหาร
            ViewBag.SupplyCount = allProducts.Count(p => p.Category == ProductCategory.Supply); // จำนวนสินค้าหมวดของใช้
            ViewBag.HygieneCount = allProducts.Count(p => p.Category == ProductCategory.Hygiene); // จำนวนสินค้าหมวดสุขอนามัย

            string normalizedFilter = (stockFilter ?? "all").Trim().ToLowerInvariant(); // ปรับแต่งสตริงตัวกรองสต็อก
            string normalizedCategory = (categoryFilter ?? "all").Trim(); // ปรับแต่งสตริงตัวกรองหมวดหมู่
            IEnumerable<Product> filtered = allProducts; // ตัวแปรคัดกรองเริ่มต้น

            filtered = normalizedFilter switch // ใช้ switch expression คัดกรองตามสถานะสต็อก
            {
                "low" => filtered.Where(p => p.IsLowStock), // เฉพาะสินค้าใกล้หมด
                "out" => filtered.Where(p => p.IsOutOfStock), // เฉพาะสินค้าหมดสต็อก
                "normal" => filtered.Where(p => !p.IsLowStock && !p.IsOutOfStock), // เฉพาะสินค้าสต็อกปกติ
                "alert" => filtered.Where(p => p.IsLowStock || p.IsOutOfStock), // รวมทั้งใกล้หมดและหมดสต็อก
                _ => filtered // สินค้าทั้งหมด
            };

            if (!string.Equals(normalizedCategory, "all", System.StringComparison.OrdinalIgnoreCase) && // ถ้าเลือกหมวดหมู่เฉพาะ
                System.Enum.TryParse<ProductCategory>(normalizedCategory, true, out var parsedCat))
            {
                filtered = filtered.Where(p => p.Category == parsedCat); // กรองตามหมวดหมู่
            }

            if (!string.IsNullOrWhiteSpace(search)) // ถ้ามีคำค้นหา
            {
                string kw = search.Trim(); // ตัดช่องว่าง
                filtered = filtered.Where(p => // ค้นหาจากชื่อ, แบรนด์ หรือความต้องการพิเศษ
                    p.Name.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Brand.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
                    p.SpecialNeed.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.StockFilter = normalizedFilter; // ส่งค่าตัวกรองสต็อกไปยัง View
            ViewBag.CategoryFilter = normalizedCategory; // ส่งค่าตัวกรองหมวดหมู่ไปยัง View
            ViewBag.Search = search; // ส่งคำค้นหา

            return View(filtered.ToList()); // ส่งรายการสินค้าไปแสดงผลใน Views/Inventory/Index.cshtml
        }

        // อัปเดตสต็อกรวดเร็ว (คงสถานะ Filter เดิมไว้)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> UpdateStock(int id, int adjustment, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null) // Action ปรับยอดสต็อกด่วน
        {
            var product = _repo.UpdateProductStock(id, adjustment); // สั่ง Repository ปรับยอดสต็อกและบันทึก Audit Log
            if (product != null) // ถ้าอัปเดตสำเร็จ
            {
                TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว"; // แจ้งข้อความสำเร็จ

                // SignalR Real-Time Socket Broadcast: แจ้งเตือนไปยังทุกหน้าจอและลูกค้า
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity); // บรอดแคสต์สต็อกใหม่
                if (product.IsLowStock) // ถ้าสต็อกต่ำกว่าเกณฑ์
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity); // ส่งสัญญาณเตือนภัย Low Stock Alert
                }
            }
            return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role }); // ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
        }

        // หน้าเพิ่ม/แก้ไขสินค้าแบบ GET (รองรับเมื่อเข้าผ่าน URL หรือรีเฟรชหน้าจอ) -> นำทางไปยังหน้าคลังและเปิด Modal เพิ่มสินค้าทันที
        [HttpGet] // รับคำขอ HTTP GET
        public IActionResult SaveProduct(int? id = null, string? role = null) // Action เมธอดรองรับการเข้าหน้านี้ด้วย GET
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = id == null ? "true" : null, editId = id }); // ส่งกลับไปหน้า Index พร้อมสั่งให้เปิด Modal สร้างหรือแก้ไขสินค้าทันที
        }

        [HttpGet]
        public IActionResult CreateProduct(string? role = null) // ทางลัดเปิด Modal เพิ่มสินค้า
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
        }

        [HttpGet]
        public IActionResult AddProduct(string? role = null) // ทางลัดเปิด Modal เพิ่มสินค้า
        {
            return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
        }

        // เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile, string? role = null) // Action รับข้อมูลสินค้าและไฟล์รูปภาพ
        {
            try // เปิดบล็อก try
            {
                if (imageFile != null && imageFile.Length > 0) // ตรวจสอบว่ามีการอัปโหลดไฟล์รูปภาพหรือไม่
                {
                    string webRoot = !string.IsNullOrEmpty(_env.WebRootPath) // กำหนดพาธโฟลเดอร์ wwwroot
                        ? _env.WebRootPath
                        : System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");

                    var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, webRoot, "products"); // บันทึกรูปภาพสินค้าลงใน wwwroot/uploads/products
                    if (uploadErr != null) // หากเกิดข้อผิดพลาดในการบันทึกรูป
                    {
                        TempData["ErrorMessage"] = uploadErr; // แจ้งข้อผิดพลาด
                        return RedirectToAction(nameof(Index), new { role, openCreate = model.Id == 0 ? "true" : null, editId = model.Id > 0 ? (int?)model.Id : null }); // ส่งกลับหน้า Index พร้อมเปิด Modal เดิมค้างไว้
                    }
                    if (!string.IsNullOrEmpty(uploadedUrl)) // ถ้าได้ URL รูปภาพ
                    {
                        model.ImageUrl = uploadedUrl; // ผูก URL รูปเข้ากับ Entity สินค้า
                    }
                }

                if (_repo.SaveProduct(model, out var message)) // สั่ง Repository บันทึกสินค้าใหม่หรืออัปเดตสินค้าเดิม
                {
                    TempData["SuccessMessage"] = message; // แจ้งข้อความสำเร็จ

                    try // ครอบ SignalR ด้วย try-catch
                    {
                        // SignalR Real-Time Socket Broadcast
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", model.Id, model.StockQuantity); // บรอดแคสต์สต็อกใหม่
                        if (model.IsLowStock) // ตรวจสอบสถานะสินค้าใกล้หมด
                        {
                            await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", model.Name, model.StockQuantity); // ส่งเตือน Low Stock Alert
                        }
                    }
                    catch
                    {
                        // ป้องกัน SignalR error ส่งผลกระทบต่อการบันทึกสินค้า
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = message; // แจ้งข้อผิดพลาด
                }
            }
            catch (Exception ex) // ดักจับ Exception
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการบันทึกข้อมูลสินค้า: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        [HttpGet]
        public IActionResult DeleteProduct(string? role = null) // ป้องกันการเข้า DeleteProduct ด้วย GET
        {
            return RedirectToAction(nameof(Index), new { role });
        }

        [HttpPost] // รับคำขอ HTTP POST
        public async Task<IActionResult> DeleteProduct(int id, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null) // Action ลบหรือระงับการขายสินค้า
        {
            try // เปิดบล็อก try
            {
                var (success, message) = _repo.DeleteProduct(id); // สั่ง Repository ลบสินค้าและบันทึก Audit Log
                if (success)
                {
                    TempData["SuccessMessage"] = message; // แจ้งข้อความสำเร็จ
                    try
                    {
                        // SignalR Real-Time Socket Broadcast
                        await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", id, 0); // บรอดแคสต์บอกทุกหน้าจอว่าสต็อกสินค้ารหัสนี้เป็น 0
                    }
                    catch { }
                }
                else
                {
                    TempData["ErrorMessage"] = message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบสินค้า: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role }); // ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
        }
    }
}

---

# 8. MarketingController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
using CSI402_Project_final.Models; // นำเข้า Model โปรโมชั่น Promotion, PetShopRepository
using System.Linq; // นำเข้าฟังก์ชัน LINQ เช่น .Count(), .Sum()

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "MarketingStaff,Owner,Admin")] // จำกัดสิทธิ์เฉพาะฝ่ายการตลาด (MarketingStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class MarketingController : Controller // ประกาศคลาส MarketingController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับจัดการแคมเปญโปรโมชั่น

        public MarketingController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // กำหนดค่า Repository
        }

        // หน้าแดชบอร์ดโปรโมชั่น & สถิติ (Req 5, 27)
        public IActionResult Index() // Action แสดงแดชบอร์ดการตลาดผ่าน HTTP GET
        {
            var promotions = _repo.Promotions; // ดึงรายการแคมเปญโปรโมชั่นทั้งหมดจาก Repository
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.TotalPromotions = promotions.Count; // นับจำนวนโปรโมชั่นทั้งหมดในระบบ
            ViewBag.ActivePromotionsCount = promotions.Count(p => p.IsValidNow); // นับจำนวนโปรโมชั่นที่กำลังเปิดใช้งานและยังไม่หมดอายุ
            ViewBag.TotalUsageAllPromos = promotions.Sum(p => p.UsageCount); // รวมจำนวนครั้งที่ลูกค้าได้รับสิทธิ์ส่วนลดจากทุกแคมเปญ

            return View(promotions); // ส่งรายการโปรโมชั่นไปแสดงผลใน Views/Marketing/Index.cshtml
        }

        // สวิตช์เปิด/ปิดโปรโมชั่น (Req 5)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult ToggleStatus(int id, string? role = null) // Action สลับสถานะเปิดหรือปิดใช้งานโปรโมชั่น
        {
            var promo = _repo.TogglePromotionStatus(id); // สั่ง Repository สลับสถานะ IsActive (Active <-> Inactive) และบันทึก Audit Log
            if (promo != null) // ถ้าสลับสถานะสำเร็จ
            {
                TempData["SuccessMessage"] = $"ปรับสถานะโปรโมชั่น '{promo.Title}' เป็น {(promo.IsActive ? "เปิดใช้งาน (Active)" : "ปิดชั่วคราว (Inactive)")} เรียบร้อยแล้ว"; // แสดงข้อความสถานะใหม่
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        // สร้างหรือแก้ไขแคมเปญโปรโมชั่น (Req 5, 25, 26)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult Save(Promotion model, string? role = null) // Action รับข้อมูล Rule Builder ของโปรโมชั่น (ส่วนลด, ยอดขั้นต่ำ, สัตว์, หมวดหมู่, วันที่)
        {
            var (success, message) = _repo.SavePromotion(model); // สั่ง Repository บันทึกโปรโมชั่นใหม่หรืออัปเดตโปรโมชั่นเดิม พร้อมบันทึก Audit Log
            if (success)
                TempData["SuccessMessage"] = message; // แจ้งข้อความสำเร็จ
            else
                TempData["ErrorMessage"] = message; // แจ้งข้อผิดพลาด

            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }

        [HttpPost]
        public IActionResult Create(Promotion model, string? role = null) => Save(model, role); // Expression-bodied method ส่งต่อการทำงานไปยังเมธอด Save เพื่อสร้างโปรโมชั่นใหม่

        // ลบโปรโมชั่น (Req 5)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult Delete(int id, string? role = null) // Action รับรหัสโปรโมชั่นที่ต้องการลบ
        {
            var (success, message) = _repo.DeletePromotion(id); // สั่ง Repository ลบโปรโมชั่นและบันทึก Audit Log
            if (success)
                TempData["SuccessMessage"] = message; // แจ้งข้อความสำเร็จ
            else
                TempData["ErrorMessage"] = message; // แจ้งข้อผิดพลาด

            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }
    }
}

---

# 9. OrderController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
using CSI402_Project_final.Models; // นำเข้า Order, OrderStatus, PetShopRepository
using System.Linq; // นำเข้าคำสั่ง LINQ สำหรับการคัดกรองสถานะออเดอร์

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "OrderStaff,Owner,Admin")] // จำกัดสิทธิ์เฉพาะฝ่ายคำสั่งซื้อและจัดเตรียมสินค้า (OrderStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class OrderController : Controller // ประกาศคลาส OrderController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับจัดการคำสั่งซื้อ

        public OrderController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // ผูก Repository เข้ากับตัวแปรภายใน
        }

        // หน้าตรวจสอบคำสั่งซื้อและจัดเตรียมสินค้า
        public IActionResult Index(OrderStatus? status = null) // Action แสดงคิวงานคำสั่งซื้อผ่าน HTTP GET รับตัวกรองสถานะออเดอร์
        {
            var allOrders = _repo.Orders; // ดึงคำสั่งซื้อทั้งหมดในระบบ
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.SelectedStatus = status; // ส่งสถานะที่เลือกคัดกรองไปยัง View
            ViewBag.PendingCount = allOrders.Count(o => o.Status == OrderStatus.PaymentVerified); // นับจำนวนออเดอร์ที่ชำระเงินเรียบร้อยแล้วและรอจัดเตรียม
            ViewBag.PreparingCount = allOrders.Count(o => o.Status == OrderStatus.Preparing); // นับจำนวนออเดอร์ที่กำลังจัดเตรียม/หยิบสินค้า

            var filtered = status.HasValue // ตรวจสอบว่ามีการกรองสถานะหรือไม่
                ? allOrders.Where(o => o.Status == status.Value).ToList() // ถ้ามี ให้เลือกเฉพาะสถานะนั้น
                : allOrders; // ถ้าไม่มี ให้แสดงทั้งหมด

            return View(filtered); // ส่งรายการคำสั่งซื้อไปเรนเดอร์ใน Views/Order/Index.cshtml
        }

        // อัปเดตสถานะการเตรียมสินค้า (บันทึกลง DB)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult UpdateStatus(string orderId, OrderStatus newStatus, string? role = null) // Action รับรหัสออเดอร์และสถานะใหม่ (เช่น เปลี่ยนเป็น Preparing เมื่อเริ่มแพ็กของ)
        {
            if (_repo.UpdateOrderStatus(orderId, newStatus)) // สั่ง Repository ปรับปรุงสถานะคำสั่งซื้อและบันทึก Audit Log
            {
                TempData["SuccessMessage"] = $"อัปเดตสถานะออเดอร์ {orderId} เป็น '{newStatus}' เรียบร้อยแล้ว"; // แสดงข้อความสำเร็จ
            }
            if (!string.IsNullOrEmpty(role)) // ตรวจสอบว่ามีพารามิเตอร์ role หรือไม่
            {
                return RedirectToAction(nameof(Index), new { role = role }); // ส่งกลับหน้า Index พร้อมส่ง role ต่อไป
            }
            return RedirectToAction(nameof(Index)); // ส่งกลับหน้า Index
        }
    }
}
```


---

# 10. OwnerController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = "Owner")]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult
using CSI402_Project_final.Models; // นำเข้า Model ของระบบ Order, Product, PetCategory, ProductCategory, PetShopRepository
using System.Linq; // นำเข้า LINQ สำหรับวิเคราะห์ข้อมูลสถิติ เช่น .ToDictionary(), .SelectMany(), .Count()

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "Owner")] // จำกัดสิทธิ์ระดับสูงสุด เฉพาะผู้บริหารสูงสุด (Owner) เท่านั้น
    public class OwnerController : Controller // ประกาศคลาส OwnerController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับเข้าถึงข้อมูลรวมทั้งระบบ

        public OwnerController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // ผูก Repository เข้ากับตัวแปรภายใน
        }

        // หน้า Executive Dashboard ภาพรวมธุรกิจ (Owner)
        public IActionResult Index() // Action แสดงกระดาน BI Dashboard ผ่าน HTTP GET
        {
            var orders = _repo.Orders; // ดึงคำสั่งซื้อทั้งหมดในระบบ
            var products = _repo.Products; // ดึงสินค้าทั้งหมดในระบบ
            var promotions = _repo.Promotions; // ดึงโปรโมชั่นทั้งหมดในระบบ

            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.Financial = _repo.GetFinancialSummary(); // ดึงสรุปบัญชีการเงิน (ยอดขายรวม, กำไรสุทธิ, รายจ่ายรวม, คืนเงินรวม) ใส่ใน ViewBag

            var productTypesById = products.ToDictionary(p => p.Id, p => p.PetType); // สร้าง Dictionary แมป ProductId กับประเภทสัตว์เลี้ยง เพื่อความรวดเร็วในการวิเคราะห์
            int dogProductOrders = orders.SelectMany(o => o.Items) // แตกรายการสินค้าทั้งหมดในทุกออเดอร์
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Dog); // นับจำนวนยอดสั่งซื้อที่เป็นสินค้าสุนัข
            int catProductOrders = orders.SelectMany(o => o.Items)
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Cat); // นับจำนวนยอดสั่งซื้อที่เป็นสินค้าแมว
            int otherProductOrders = orders.SelectMany(o => o.Items)
                .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Other); // นับจำนวนยอดสั่งซื้อที่เป็นสินค้าสัตว์เลี้ยงอื่นๆ

            ViewBag.DogOrderCount = dogProductOrders; // ส่งสถิติสินค้าสุนัขไปยัง View
            ViewBag.CatOrderCount = catProductOrders; // ส่งสถิติสินค้าแมวไปยัง View
            ViewBag.OtherOrderCount = otherProductOrders; // ส่งสถิติสินค้าสัตว์อื่นๆ ไปยัง View

            // สัดส่วนสินค้าในคลังและยอดสั่งซื้อแยกตามหมวดหมู่ (อาหาร / ของใช้สัตว์เลี้ยง / สุขอนามัย)
            ViewBag.FoodCatalogCount = products.Count(p => p.Category == ProductCategory.Food); // นับสินค้าหมวดอาหารในแคตตาล็อก
            ViewBag.SupplyCatalogCount = products.Count(p => p.Category == ProductCategory.Supply); // นับสินค้าหมวดของใช้ในแคตตาล็อก
            ViewBag.HygieneCatalogCount = products.Count(p => p.Category == ProductCategory.Hygiene); // นับสินค้าหมวดสุขอนามัยในแคตตาล็อก

            var productCatById = products.ToDictionary(p => p.Id, p => p.Category); // สร้าง Dictionary แมป ProductId กับหมวดหมู่สินค้า
            ViewBag.FoodOrderCount = orders.SelectMany(o => o.Items) // นับยอดสั่งซื้อหมวดอาหาร
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Food);
            ViewBag.SupplyOrderCount = orders.SelectMany(o => o.Items) // นับยอดสั่งซื้อหมวดของใช้สัตว์เลี้ยง
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Supply);
            ViewBag.HygieneOrderCount = orders.SelectMany(o => o.Items) // นับยอดสั่งซื้อหมวดสุขอนามัย
                .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Hygiene);

            var topProducts = products // เริ่มจัดอันดับสินค้าขายดี/ยอดนิยม
                .OrderByDescending(p => p.CartAddCount) // เรียงลำดับจากจำนวนครั้งที่ถูกหยิบใส่ตะกร้ามากที่สุด
                .Take(5) // นำมา 5 อันดับแรก
                .ToList();
            ViewBag.TopProducts = topProducts; // ส่งรายการสินค้าขายดีไปยัง View

            var topPromotions = promotions // เริ่มจัดอันดับโปรโมชั่นยอดนิยม
                .OrderByDescending(p => p.UsageCount) // เรียงตามจำนวนครั้งที่ถูกใช้งาน
                .Take(4) // นำมา 4 อันดับแรก
                .ToList();
            ViewBag.TopPromotions = topPromotions; // ส่งรายการโปรโมชั่นยอดนิยมไปยัง View

            ViewBag.TotalOrderCount = orders.Count; // นับจำนวนคำสั่งซื้อรวมทั้งหมด
            ViewBag.RecentOrders = orders.Take(5).ToList(); // ดึงคำสั่งซื้อล่าสุด 5 รายการ

            return View(); // เรนเดอร์หน้า Views/Owner/Index.cshtml
        }
    }
}

---

# 11. ShippingController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
using Microsoft.AspNetCore.SignalR; // นำเข้า IHubContext สำหรับส่งการแจ้งเตือน Real-Time เมื่อพัสดุจัดส่งแล้ว
using CSI402_Project_final.Models; // นำเข้า Order, OrderStatus, PetShopRepository
using CSI402_Project_final.Hubs; // นำเข้า PetShopHub สำหรับการเชื่อมต่อ WebSocket
using System; // นำเข้าคลาสระบบพื้นฐาน เช่น DateTime
using System.Linq; // นำเข้าฟังก์ชัน LINQ เช่น .Where(), .OrderBy(), .Count()
using System.Threading.Tasks; // นำเข้า Task สำหรับ Asynchronous SignalR

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "ShippingStaff,Owner,Admin")] // จำกัดสิทธิ์เฉพาะฝ่ายจัดส่ง (ShippingStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class ShippingController : Controller // ประกาศคลาส ShippingController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับจัดการคำสั่งซื้อและการจัดส่ง
        private readonly IHubContext<PetShopHub> _hubContext; // ฟิลด์เก็บ SignalR Hub Context สำหรับบรอดแคสต์สถานะพัสดุ

        public ShippingController(PetShopRepository repo, IHubContext<PetShopHub> hubContext) // Constructor รับ DI จากระบบ
        {
            _repo = repo; // ผูก Repository เข้ากับตัวแปรภายใน
            _hubContext = hubContext; // ผูก SignalR Hub เข้ากับตัวแปรภายใน
        }

        // หน้าจัดการการจัดส่งและพัสดุ (เฉพาะออเดอร์ที่ฝ่ายคำสั่งซื้อแพ็กเสร็จส่งต่องานมาให้แล้ว)
        public IActionResult Index() // Action แสดงหน้าคิวงานพัสดุผ่าน HTTP GET
        {
            var allOrders = _repo.Orders; // ดึงคำสั่งซื้อทั้งหมด
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            // พัสดุรอจัดส่ง: ออเดอร์ที่ฝ่าย Order Staff จัดเตรียม/แพ็กเสร็จแล้ว (Preparing)
            ViewBag.ReadyToShipCount = allOrders.Count(o => o.Status == OrderStatus.Preparing); // นับจำนวนพัสดุที่แพ็กเสร็จและรอการจัดส่ง
            ViewBag.ShippedTodayCount = allOrders.Count(o => // นับจำนวนพัสดุที่จัดส่งแล้วในวันนี้
                o.Status == OrderStatus.Shipped && // สถานะจัดส่งแล้ว
                !string.IsNullOrWhiteSpace(o.TrackingNumber) && // มีเลขพัสดุ
                (!o.ShippedDate.HasValue || o.ShippedDate.Value.Date == DateTime.Today)); // วันที่จัดส่งเป็นวันนี้

            var orders = allOrders // คัดกรองรายการออเดอร์
                .Where(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Shipped) // เลือกเฉพาะออเดอร์ที่กำลังเตรียมส่งหรือจัดส่งแล้ว
                .OrderBy(o => o.Status == OrderStatus.Preparing ? 0 : 1) // จัดให้ออเดอร์ที่รอส่ง (Preparing) อยู่ด้านบนสุด
                .ThenByDescending(o => o.OrderDate) // ตามด้วยเรียงจากวันที่สั่งซื้อล่าสุด
                .ToList();

            return View(orders); // ส่งรายการพัสดุไปแสดงผลใน Views/Shipping/Index.cshtml
        }

        // บันทึกเลขพัสดุและน้ำหนักจริง (Req 4, 14, 16)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public async Task<IActionResult> DispatchOrder(string orderId, string courier, string trackingNumber, double actualWeightKg, string? role = null) // Action บันทึกเลข Tracking และน้ำหนักจริง
        {
            if (_repo.DispatchOrder(orderId, courier, trackingNumber, actualWeightKg)) // สั่ง Repository บันทึกข้อมูลขนส่ง, ปรับสถานะเป็น Shipped, บันทึกเวลาจัดส่ง และลง Audit Log
            {
                // SignalR: แจ้งเตือน Real-Time ว่าพัสดุจัดส่งแล้ว
                await _hubContext.Clients.All.SendAsync("ReceiveOrderShippedAlert", orderId, trackingNumber, courier); // บรอดแคสต์แจ้งเตือน Real-Time ว่าพัสดุถูกจัดส่งแล้วพร้อมส่งเลข Tracking

                TempData["SuccessMessage"] = $"บันทึกเลขพัสดุ {trackingNumber} สำหรับคำสั่งซื้อ {orderId} และเปลี่ยนสถานะเป็น 'จัดส่งแล้ว' เรียบร้อยแล้ว"; // แสดงข้อความแจ้งเตือนความสำเร็จ
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }
    }
}

---

# 12. SupportController.cs

```csharp
using Microsoft.AspNetCore.Authorization; // นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน [Authorize(Roles = ...)]
using Microsoft.AspNetCore.Mvc; // นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
using CSI402_Project_final.Models; // นำเข้า SupportTicket, TicketStatus, PetShopRepository
using System.Linq; // นำเข้าคำสั่ง LINQ สำหรับการนับและกรองตั๋วปัญหา

namespace CSI402_Project_final.Controllers // ขอบเขตเนมสเปซของ Controller
{
    [Authorize(Roles = "ITSupport,Owner,Admin")] // จำกัดสิทธิ์เฉพาะเจ้าหน้าที่ฝ่ายสนับสนุนด้านเทคนิค (ITSupport), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
    public class SupportController : Controller // ประกาศคลาส SupportController สืบทอดมาจาก Controller
    {
        private readonly PetShopRepository _repo; // ฟิลด์เก็บ Repository สำหรับจัดการตั๋วแจ้งปัญหา IT Helpdesk

        public SupportController(PetShopRepository repo) // Constructor รับ Repository ผ่าน Dependency Injection
        {
            _repo = repo; // ผูก Repository เข้ากับตัวแปรภายใน
        }

        // หน้า Helpdesk Board ของ IT Support (Req 23)
        public IActionResult Index(TicketStatus? status = null) // Action แสดงกระดานตั๋วแจ้งปัญหา IT Support Helpdesk ผ่าน HTTP GET
        {
            var allTickets = _repo.GetSupportTickets(); // ดึงรายการตั๋วปัญหาทั้งหมดในระบบ
            ViewBag.CurrentRole = _repo.CurrentSimulatedRole; // ส่งบทบาทปัจจุบันไปยัง View
            ViewBag.SelectedStatus = status; // ส่งสถานะตัวกรองที่เลือกไปยัง View
            ViewBag.NewCount = allTickets.Count(t => t.Status == TicketStatus.New); // นับจำนวนตั๋วปัญหาใหม่ที่รอรับเรื่อง
            ViewBag.InvestigatingCount = allTickets.Count(t => t.Status == TicketStatus.Investigating); // นับจำนวนตั๋วที่อยู่ระหว่างการตรวจสอบ/ดำเนินการ
            ViewBag.ResolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved); // นับจำนวนตั๋วที่แก้ไขเสร็จสิ้นและปิดงานแล้ว

            var filtered = status.HasValue // ตรวจสอบว่ามีการกรองสถานะหรือไม่
                ? allTickets.Where(t => t.Status == status.Value).ToList() // ถ้ามี ให้เลือกเฉพาะสถานะนั้น
                : allTickets; // ถ้าไม่มี ให้แสดงทั้งหมด

            return View(filtered); // ส่งรายการตั๋วปัญหาไปเรนเดอร์ใน Views/Support/Index.cshtml
        }

        // บันทึกรายละเอียดการแก้ไขปัญหาและปิดงาน (Req 24)
        [HttpPost] // ระบุว่ารับคำขอด้วย HTTP POST
        public IActionResult ResolveTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus, string? role = null) // Action บันทึกรายละเอียดการแก้ไขและปิดงานตั๋ว
        {
            var ticket = _repo.ResolveSupportTicket(ticketId, technician, resolutionNote, newStatus); // สั่ง Repository บันทึกผลการแก้ไข, วันเวลาที่ปิดงาน และลงบันทึก Audit Log
            if (ticket != null) // ถ้าบันทึกสำเร็จ
            {
                TempData["SuccessMessage"] = $"บันทึกประวัติการแก้ไขและอัปเดตสถานะ Ticket #{ticket.TicketCode} เรียบร้อยแล้ว"; // แสดงข้อความสำเร็จพร้อมระบุรหัส Ticket
            }
            return RedirectToAction(nameof(Index), new { role }); // ส่งกลับหน้า Index
        }
    }
}
```
