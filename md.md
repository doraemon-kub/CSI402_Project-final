# เอกสารแจกแจงโค้ดและอธิบายการทำงานทุกบรรทัดของ Controller ทุกไฟล์ (PetStore Pro)
**วิชา:** CSI402 การเชื่อมต่อโปรแกรมส่วนหน้ากับโปรแกรมส่วนหลัง  
**คณะเทคโนโลยีสารสนเทศ มหาวิทยาลัยศรีปทุม**  
**มาตรฐานเอกสาร:** บันทึกซอร์สโค้ดเต็ม 100% พร้อมอธิบายการทำงานอย่างละเอียดทีละบรรทัด (Line-by-Line Code Breakdown) ครบทั้ง 12 Controllers รวม 1,743 บรรทัด

---

## 📑 รายชื่อ Controllers ทั้ง 12 ไฟล์ในระบบ
1. `AccountController.cs` (204 บรรทัด) - จัดการโปรไฟล์, สมุดที่อยู่, ประวัติคำสั่งซื้อ, แนบสลิป, แจ้งปัญหา
2. `AccountingController.cs` (123 บรรทัด) - ตรวจสอบสลิป, อนุมัติ/ปฏิเสธเงิน, คืนเงิน, สรุปรายจ่าย, ส่งออก CSV
3. `AdminController.cs` (212 บรรทัด) - จัดการผู้ใช้, เปลี่ยนบทบาท, ระงับบัญชี (เตะ SignalR), Audit Log
4. `AuthController.cs` (338 บรรทัด) - ล็อกอิน, Quick Demo Login, สลับบทบาท, สมัครสมาชิก, ออกจากระบบ
5. `CartController.cs` (245 บรรทัด) - ตะกร้าสินค้า, AJAX Add/Update, Checkout, คำนวณค่าส่ง, SignalR Broadcast
6. `HomeController.cs` (118 บรรทัด) - แคตตาล็อกหน้าร้าน, ตัวกรองสินค้าขั้นสูง, หน้าเจาะลึกสินค้า, สินค้าแนะนำ
7. `InventoryController.cs` (206 บรรทัด) - จัดการคลังสินค้า, ปรับสต็อกด่วน, เพิ่ม/ลบสินค้า, Low Stock Alert
8. `MarketingController.cs` (71 บรรทัด) - แดชบอร์ดโปรโมชั่น, สวิตช์เปิด/ปิด, สร้าง/แก้ไข Rule Builder
9. `OrderController.cs` (49 บรรทัด) - คิวงานจัดเตรียมสินค้าหลังชำระเงิน, เปลี่ยนสถานะ Preparing
10. `OwnerController.cs` (71 บรรทัด) - Executive BI Dashboard ภาพรวมรายได้ ยอดขาย สัดส่วนหมวดหมู่
11. `ShippingController.cs` (59 บรรทัด) - คิวงานพัสดุรอส่ง, ชั่งน้ำหนักจริง, ออกเลข Tracking, แจ้งเตือน Shipped
12. `SupportController.cs` (47 บรรทัด) - IT Helpdesk Board รับเรื่องแจ้งปัญหา, บันทึกการแก้ไขและปิดตั๋ว

---

# 1. AccountController.cs (204 บรรทัด)

### โค้ดต้นฉบับเต็มของ AccountController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Hosting;
3: using Microsoft.AspNetCore.Http;
4: using Microsoft.AspNetCore.Mvc;
5: using CSI402_Project_final.Helpers;
6: using CSI402_Project_final.Models;
7: using System.Threading.Tasks;
8: 
9: namespace CSI402_Project_final.Controllers
10: {
11:     [Authorize]
12:     public class AccountController : Controller
13:     {
14:         private readonly PetShopRepository _repo;
15:         private readonly IWebHostEnvironment _env;
16: 
17:         public AccountController(PetShopRepository repo, IWebHostEnvironment env)
18:         {
19:             _repo = repo;
20:             _env = env;
21:         }
22: 
23:         // หน้าโปรไฟล์และจัดการที่อยู่จัดส่ง
24:         [HttpGet]
25:         public IActionResult Profile()
26:         {
27:             var username = User.Identity?.Name ?? "";
28:             var user = _repo.GetUserByUsername(username);
29:             if (user == null) return RedirectToAction("Login", "Auth");
30: 
31:             var model = new ProfileViewModel
32:             {
33:                 Username = user.Username,
34:                 FullName = user.FullName,
35:                 Email = user.Email,
36:                 Phone = user.Phone,
37:                 Role = user.Role,
38:                 Addresses = user.Addresses ?? new()
39:             };
40: 
41:             return View(model);
42:         }
43: 
44:         // อัปเดตข้อมูลส่วนตัว
45:         [HttpPost]
46:         [ValidateAntiForgeryToken]
47:         public IActionResult UpdateProfile(ProfileViewModel model)
48:         {
49:             var username = User.Identity?.Name ?? "";
50:             _repo.UpdateUserProfile(username, model.FullName, model.Email, model.Phone);
51:             TempData["SuccessMessage"] = "บันทึกข้อมูลส่วนตัวเรียบร้อยแล้ว";
52:             return RedirectToAction(nameof(Profile));
53:         }
54: 
55:         // เพิ่มหรือแก้ไขที่อยู่จัดส่ง
56:         [HttpPost]
57:         public IActionResult SaveAddress(UserAddress address, string? returnUrl = null)
58:         {
59:             var username = User.Identity?.Name ?? "";
60:             if (string.IsNullOrWhiteSpace(address.FullAddress) || string.IsNullOrWhiteSpace(address.Province))
61:             {
62:                 if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
63:                 {
64:                     return Json(new { success = false, message = "กรุณากรอกข้อมูลที่อยู่และจังหวัดให้ครบถ้วน" });
65:                 }
66:                 TempData["ErrorMessage"] = "กรุณากรอกข้อมูลที่อยู่ให้ครบถ้วน";
67:                 return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
68:                     ? LocalRedirect(returnUrl)
69:                     : RedirectToAction(nameof(Profile));
70:             }
71: 
72:             _repo.AddOrUpdateAddress(username, address);
73: 
74:             if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
75:             {
76:                 return Json(new
77:                 {
78:                     success = true,
79:                     id = address.Id,
80:                     title = address.Title,
81:                     recipientName = address.RecipientName,
82:                     phone = address.Phone,
83:                     displayAddress = address.DisplayAddress,
84:                     isDefault = address.IsDefault,
85:                     message = "บันทึกที่อยู่จัดส่งใหม่เรียบร้อยแล้ว"
86:                 });
87:             }
88: 
89:             TempData["SuccessMessage"] = "บันทึกที่อยู่จัดส่งเรียบร้อยแล้ว";
90:             return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
91:                 ? LocalRedirect(returnUrl)
92:                 : RedirectToAction(nameof(Profile));
93:         }
94: 
95:         // ลบที่อยู่จัดส่ง
96:         [HttpPost]
97:         [ValidateAntiForgeryToken]
98:         public IActionResult DeleteAddress(int addressId)
99:         {
100:             var username = User.Identity?.Name ?? "";
101:             _repo.DeleteAddress(username, addressId);
102:             TempData["SuccessMessage"] = "ลบที่อยู่จัดส่งเรียบร้อยแล้ว";
103:             return RedirectToAction(nameof(Profile));
104:         }
105: 
106:         // ตั้งเป็นที่อยู่จัดส่งหลัก
107:         [HttpPost]
108:         [ValidateAntiForgeryToken]
109:         public IActionResult SetDefaultAddress(int addressId)
110:         {
111:             var username = User.Identity?.Name ?? "";
112:             _repo.SetDefaultAddress(username, addressId);
113:             TempData["SuccessMessage"] = "ตั้งเป็นที่อยู่จัดส่งหลักเรียบร้อยแล้ว";
114:             return RedirectToAction(nameof(Profile));
115:         }
116: 
117:         // หน้าประวัติคำสั่งซื้อของลูกค้า (กรองเฉพาะของบัญชีผู้ใช้นี้)
118:         public IActionResult Orders(string? successOrderId = null)
119:         {
120:             var username = User.Identity?.Name ?? "";
121:             ViewBag.SuccessOrderId = successOrderId;
122:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
123:             var orders = _repo.GetOrdersForUser(username);
124:             return View(orders);
125:         }
126: 
127:         // อัปโหลดหรืออัปเดตหลักฐานสลิปโอนเงินภายหลัง (Req 19)
128:         [HttpPost]
129:         public async Task<IActionResult> UploadSlip(string orderId, IFormFile? paymentSlip)
130:         {
131:             if (paymentSlip == null || paymentSlip.Length == 0)
132:             {
133:                 TempData["ErrorMessage"] = "กรุณาเลือกไฟล์รูปภาพสลิปการโอนเงิน";
134:                 return RedirectToAction(nameof(Orders));
135:             }
136: 
137:             var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");
138:             if (uploadErr != null || string.IsNullOrEmpty(uploadedUrl))
139:             {
140:                 TempData["ErrorMessage"] = uploadErr ?? "ไม่สามารถอัปโหลดสลิปได้";
141:                 return RedirectToAction(nameof(Orders));
142:             }
143: 
144:             var username = User.Identity?.Name ?? "";
145:             if (_repo.AttachPaymentSlip(username, orderId, uploadedUrl))
146:             {
147:                 TempData["SuccessMessage"] = $"แนบหลักฐานสลิปสำหรับคำสั่งซื้อ #{orderId} เรียบร้อยแล้ว ฝ่ายบัญชีจะตรวจสอบในลำดับถัดไป";
148:             }
149:             else
150:             {
151:                 TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือเปลี่ยนรูปสลิปได้ เนื่องจากคำสั่งซื้อนี้มีการแนบหลักฐานสลิปและถูกล็อกแล้ว";
152:             }
153: 
154:             return RedirectToAction(nameof(Orders));
155:         }
156: 
157:         // ลูกค้าขอยกเลิกคำสั่งซื้อ / ขอคืนเงิน (Req 20)
158:         [HttpPost]
159:         public IActionResult CancelOrder(string orderId, string reason)
160:         {
161:             var username = User.Identity?.Name ?? "";
162:             if (_repo.CancelOrder(username, orderId, reason))
163:             {
164:                 TempData["SuccessMessage"] = $"ดำเนินการยกเลิกคำสั่งซื้อ {orderId} เรียบร้อยแล้ว";
165:             }
166:             else
167:             {
168:                 TempData["ErrorMessage"] = "ไม่สามารถขอยกเลิกคำสั่งซื้อนี้ได้";
169:             }
170:             return RedirectToAction(nameof(Orders));
171:         }
172: 
173:         // หน้าแจ้งปัญหาการใช้งานระบบ (Helpdesk สำหรับลูกค้า Req 22)
174:         public IActionResult Support()
175:         {
176:             var username = User.Identity?.Name ?? "";
177:             var user = _repo.GetUserByUsername(username);
178: 
179:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
180:             ViewBag.CurrentUser = user;
181:             ViewBag.MyTickets = _repo.GetSupportTickets(customerEmail: user?.Email);
182:             return View();
183:         }
184: 
185:         [HttpPost]
186:         public IActionResult SubmitTicket(string customerName, string email, string? phone, string category, string subject, string description, TicketPriority priority)
187:         {
188:             var username = User.Identity?.Name ?? "";
189:             var user = _repo.GetUserByUsername(username);
190: 
191:             var ticket = _repo.CreateSupportTicket(
192:                 customerName,
193:                 email,
194:                 phone ?? user?.Phone ?? "",
195:                 category,
196:                 subject,
197:                 description,
198:                 priority);
199: 
200:             TempData["SuccessMessage"] = $"บันทึกการแจ้งปัญหาเรียบร้อยแล้ว (รหัสอ้างอิง: {ticket.TicketCode}) ทางทีม IT Support จะรีบตรวจสอบ";
201:             return RedirectToAction(nameof(Support));
202:         }
203:     }
204: }
```

### คำอธิบายการทำงานของ AccountController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าไลบรารีสิทธิ์การเข้าถึง เพื่อใช้งาน Attribute `[Authorize]` ตรวจสอบว่าผู้ใช้ผ่านการล็อกอินแล้วหรือไม่
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Hosting;`  
  นำเข้า `IWebHostEnvironment` เพื่อใช้ค้นหาพาธโฟลเดอร์ของเซิร์ฟเวอร์ เช่น `wwwroot` สำหรับบันทึกไฟล์สลิป
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.Http;`  
  นำเข้าออบเจกต์ HTTP เช่น `IFormFile` สำหรับรับไฟล์อัปโหลดรูปภาพจากฟอร์มหน้าบ้าน
* **บรรทัดที่ 4:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาสหลักของ ASP.NET Core MVC เช่น `Controller`, `IActionResult`, `HttpGet`, `HttpPost`, `Json`
* **บรรทัดที่ 5:** `using CSI402_Project_final.Helpers;`  
  นำเข้าคลาสตัวช่วยของระบบ `FileUploadHelper` สำหรับบันทึกไฟล์สลิปโอนเงินอย่างปลอดภัย
* **บรรทัดที่ 6:** `using CSI402_Project_final.Models;`  
  นำเข้า Data Model เช่น `ProfileViewModel`, `UserAddress`, `PetShopRepository`
* **บรรทัดที่ 7:** `using System.Threading.Tasks;`  
  นำเข้าคลาส `Task` สำหรับประมวลผลฟังก์ชันแบบ Asynchronous (`async/await`)
* **บรรทัดที่ 8:** *(บรรทัดว่าง)*
* **บรรทัดที่ 9:** `namespace CSI402_Project_final.Controllers`  
  กำหนดขอบเขตชื่อเนมสเปซของ Controller
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกของเนมสเปซ
* **บรรทัดที่ 11:** `[Authorize]`  
  แอตทริบิวต์กำหนดสิทธิ์ระดับคลาส บังคับให้ผู้ใช้งานต้องผ่านการพิสูจน์ตัวตน (Login) ก่อนจึงจะเรียกใช้ฟังก์ชันใดๆ ในคอนโทรลเลอร์นี้ได้
* **บรรทัดที่ 12:** `public class AccountController : Controller`  
  ประกาศคลาส `AccountController` สืบทอดคุณสมบัติมาจากคลาสฐาน `Controller`
* **บรรทัดที่ 13:** `{`  
  เปิดบล็อกของคลาส
* **บรรทัดที่ 14:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บอ็อบเจกต์ Repository สำหรับเข้าถึงและจัดการข้อมูลผู้ใช้, ที่อยู่, ออเดอร์ และ Support Ticket
* **บรรทัดที่ 15:** `private readonly IWebHostEnvironment _env;`  
  ฟิลด์เก็บอ็อบเจกต์สภาพแวดล้อมโฮสต์เพื่ออ้างอิงตำแหน่งไดเรกทอรีจัดเก็บไฟล์ในเครื่องเซิร์ฟเวอร์
* **บรรทัดที่ 16:** *(บรรทัดว่าง)*
* **บรรทัดที่ 17:** `public AccountController(PetShopRepository repo, IWebHostEnvironment env)`  
  Constructor รับการส่งมอบ Repository และ WebHostEnvironment ผ่านระบบ Dependency Injection ของ ASP.NET Core
* **บรรทัดที่ 18:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 19:** `_repo = repo;`  
  กำหนดค่าตัวแปร repository ให้กับฟิลด์ภายในคลาส
* **บรรทัดที่ 20:** `_env = env;`  
  กำหนดค่าตัวแปรสภาพแวดล้อมโฮสต์ให้กับฟิลด์ภายในคลาส
* **บรรทัดที่ 21:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 22:** *(บรรทัดว่าง)*
* **บรรทัดที่ 23:** `// หน้าโปรไฟล์และจัดการที่อยู่จัดส่ง`  
  คอมเมนต์อธิบายวัตถุประสงค์ของฟังก์ชัน Profile
* **บรรทัดที่ 24:** `[HttpGet]`  
  ระบุว่า Action นี้รองรับคำขอด้วยเมธอด HTTP GET
* **บรรทัดที่ 25:** `public IActionResult Profile()`  
  ประกาศ Action เมธอด `Profile` เพื่อดึงข้อมูลบัญชีลูกค้าและสมุดที่อยู่จัดส่งมาแสดงผล
* **บรรทัดที่ 26:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 27:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้งาน (Username) ของผู้ใช้ที่ล็อกอินอยู่ในปัจจุบันจาก Claims Identity
* **บรรทัดที่ 28:** `var user = _repo.GetUserByUsername(username);`  
  คิวรีข้อมูลบัญชีผู้ใช้จาก Repository ด้วยชื่อผู้ใช้งาน
* **บรรทัดที่ 29:** `if (user == null) return RedirectToAction("Login", "Auth");`  
  หากค้นหาไม่พบบัญชีผู้ใช้ ให้ Redirect กลับไปยังหน้าล็อกอิน
* **บรรทัดที่ 30:** *(บรรทัดว่าง)*
* **บรรทัดที่ 31:** `var model = new ProfileViewModel`  
  สร้าง ViewModel ใหม่สำหรับส่งไปแสดงผลในหน้าเว็บ
* **บรรทัดที่ 32:** `{`  
  เปิดการกำหนดค่าให้กับ Properties ของ ViewModel
* **บรรทัดที่ 33:** `Username = user.Username,`  
  กำหนดค่าชื่อผู้ใช้
* **บรรทัดที่ 34:** `FullName = user.FullName,`  
  กำหนดค่าชื่อและนามสกุล
* **บรรทัดที่ 35:** `Email = user.Email,`  
  กำหนดค่าอีเมล
* **บรรทัดที่ 36:** `Phone = user.Phone,`  
  กำหนดค่าเบอร์โทรศัพท์
* **บรรทัดที่ 37:** `Role = user.Role,`  
  กำหนดค่าระดับบทบาทของผู้ใช้
* **บรรทัดที่ 38:** `Addresses = user.Addresses ?? new()`  
  กำหนดรายการสมุดที่อยู่จัดส่ง หากไม่มีให้สร้าง List ใหม่ว่างๆ
* **บรรทัดที่ 39:** `};`  
  ปิดการสร้าง ViewModel
* **บรรทัดที่ 40:** *(บรรทัดว่าง)*
* **บรรทัดที่ 41:** `return View(model);`  
  ส่ง ViewModel ไปเรนเดอร์ในมุมมอง `Views/Account/Profile.cshtml`
* **บรรทัดที่ 42:** `}`  
  ปิดบล็อกเมธอด Profile
* **บรรทัดที่ 43:** *(บรรทัดว่าง)*
* **บรรทัดที่ 44:** `// อัปเดตข้อมูลส่วนตัว`  
  คอมเมนต์อธิบายฟังก์ชันบันทึกข้อมูลส่วนตัว
* **บรรทัดที่ 45:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 46:** `[ValidateAntiForgeryToken]`  
  ป้องกันการโจมตีแบบ Cross-Site Request Forgery (CSRF)
* **บรรทัดที่ 47:** `public IActionResult UpdateProfile(ProfileViewModel model)`  
  Action รับข้อมูลส่วนตัวที่แก้ไขมาจากหน้าบ้านผ่าน Model Binding
* **บรรทัดที่ 48:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 49:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้จากระบบยืนยันตัวตน
* **บรรทัดที่ 50:** `_repo.UpdateUserProfile(username, model.FullName, model.Email, model.Phone);`  
  บันทึกข้อมูลส่วนตัวใหม่ลงฐานข้อมูล พร้อมลงบันทึก Audit Log อัตโนมัติ
* **บรรทัดที่ 51:** `TempData["SuccessMessage"] = "บันทึกข้อมูลส่วนตัวเรียบร้อยแล้ว";`  
  เก็บข้อความแจ้งเตือนความสำเร็จใน TempData สำหรับแสดงผลบน Toast
* **บรรทัดที่ 52:** `return RedirectToAction(nameof(Profile));`  
  ส่งผู้ใช้กลับไปยังหน้า Profile แบบ Post-Redirect-Get
* **บรรทัดที่ 53:** `}`  
  ปิดบล็อกเมธอด UpdateProfile
* **บรรทัดที่ 54:** *(บรรทัดว่าง)*
* **บรรทัดที่ 55:** `// เพิ่มหรือแก้ไขที่อยู่จัดส่ง`  
  คอมเมนต์อธิบายฟังก์ชันบันทึกที่อยู่จัดส่ง
* **บรรทัดที่ 56:** `[HttpPost]`  
  ระบุว่ารับคำขอผ่าน HTTP POST
* **บรรทัดที่ 57:** `public IActionResult SaveAddress(UserAddress address, string? returnUrl = null)`  
  Action เมธอดบันทึกที่อยู่ รองรับทั้งการ Submit แบบปกติ และการเรียกผ่าน AJAX
* **บรรทัดที่ 58:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 59:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้จาก Claims
* **บรรทัดที่ 60:** `if (string.IsNullOrWhiteSpace(address.FullAddress) || string.IsNullOrWhiteSpace(address.Province))`  
  ตรวจสอบความถูกต้องของข้อมูลว่าระบุรายละเอียดที่อยู่และจังหวัดหรือไม่
* **บรรทัดที่ 61:** `{`  
  เปิดบล็อกกรณีข้อมูลไม่ครบถ้วน
* **บรรทัดที่ 62:** `if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")`  
  ตรวจสอบว่าคำขอส่งมาจาก JavaScript AJAX หรือไม่
* **บรรทัดที่ 63:** `{`  
  เปิดบล็อกตอบกลับ AJAX กรณี Error
* **บรรทัดที่ 64:** `return Json(new { success = false, message = "กรุณากรอกข้อมูลที่อยู่และจังหวัดให้ครบถ้วน" });`  
  ส่ง JSON แจ้งข้อความผิดพลาดกลับไปยังหน้าบ้าน
* **บรรทัดที่ 65:** `}`  
  ปิดบล็อกตอบกลับ AJAX
* **บรรทัดที่ 66:** `TempData["ErrorMessage"] = "กรุณากรอกข้อมูลที่อยู่ให้ครบถ้วน";`  
  กรณีฟอร์มปกติ เก็บข้อความ Error ลงใน TempData
* **บรรทัดที่ 67:** `return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)`  
  ตรวจสอบเงื่อนไข returnUrl ว่ามีค่าและเป็น Local URL ภายในระบบหรือไม่
* **บรรทัดที่ 68:** `? LocalRedirect(returnUrl)`  
  หากใช่ ให้ Redirect กลับไปยัง URL ปลายทางนั้นอย่างปลอดภัย
* **บรรทัดที่ 69:** `: RedirectToAction(nameof(Profile));`  
  หากไม่มี ให้ Redirect กลับไปหน้า Profile
* **บรรทัดที่ 70:** `}`  
  ปิดบล็อกกรณีข้อมูลไม่ครบถ้วน
* **บรรทัดที่ 71:** *(บรรทัดว่าง)*
* **บรรทัดที่ 72:** `_repo.AddOrUpdateAddress(username, address);`  
  เรียก Repository เพื่อเพิ่มที่อยู่ใหม่ หรืออัปเดตที่อยู่เดิมของผู้ใช้นี้
* **บรรทัดที่ 73:** *(บรรทัดว่าง)*
* **บรรทัดที่ 74:** `if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")`  
  ตรวจสอบว่าการบันทึกสำเร็จนี้มาจากคำขอ AJAX หรือไม่
* **บรรทัดที่ 75:** `{`  
  เปิดบล็อกตอบกลับ AJAX กรณีสำเร็จ
* **บรรทัดที่ 76:** `return Json(new`  
  สร้างและตอบกลับด้วยออบเจกต์ JSON
* **บรรทัดที่ 77:** `{`  
  เปิดบล็อก JSON
* **บรรทัดที่ 78:** `success = true,`  
  ระบุสถานะสำเร็จ
* **บรรทัดที่ 79:** `id = address.Id,`  
  ส่งรหัสที่อยู่ใหม่กลับไป
* **บรรทัดที่ 80:** `title = address.Title,`  
  ส่งชื่อป้ายกำกับที่อยู่กลับไป
* **บรรทัดที่ 81:** `recipientName = address.RecipientName,`  
  ส่งชื่อผู้รับกลับไป
* **บรรทัดที่ 82:** `phone = address.Phone,`  
  ส่งเบอร์โทรศัพท์กลับไป
* **บรรทัดที่ 83:** `displayAddress = address.DisplayAddress,`  
  ส่งข้อความที่อยู่แบบรวม formatted กลับไปให้ JavaScript นำไปใส่ Dropdown ทันที
* **บรรทัดที่ 84:** `isDefault = address.IsDefault,`  
  ส่งสถานะที่อยู่เริ่มต้นกลับไป
* **บรรทัดที่ 85:** `message = "บันทึกที่อยู่จัดส่งใหม่เรียบร้อยแล้ว"`  
  ส่งข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 86:** `});`  
  ปิดออบเจกต์ JSON
* **บรรทัดที่ 87:** `}`  
  ปิดบล็อกตอบกลับ AJAX
* **บรรทัดที่ 88:** *(บรรทัดว่าง)*
* **บรรทัดที่ 89:** `TempData["SuccessMessage"] = "บันทึกที่อยู่จัดส่งเรียบร้อยแล้ว";`  
  กรณีฟอร์มปกติ ตั้งค่าข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 90:** `return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)`  
  ตรวจสอบความปลอดภัยของ returnUrl
* **บรรทัดที่ 91:** `? LocalRedirect(returnUrl)`  
  ส่งกลับไปยัง returnUrl
* **บรรทัดที่ 92:** `: RedirectToAction(nameof(Profile));`  
  ส่งกลับไปยังหน้า Profile
* **บรรทัดที่ 93:** `}`  
  ปิดบล็อกเมธอด SaveAddress
* **บรรทัดที่ 94:** *(บรรทัดว่าง)*
* **บรรทัดที่ 95:** `// ลบที่อยู่จัดส่ง`  
  คอมเมนต์อธิบายฟังก์ชันลบที่อยู่
* **บรรทัดที่ 96:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 97:** `[ValidateAntiForgeryToken]`  
  ป้องกันการโจมตี CSRF
* **บรรทัดที่ 98:** `public IActionResult DeleteAddress(int addressId)`  
  Action เมธอดรับรหัสที่อยู่ที่ต้องการลบ
* **บรรทัดที่ 99:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 100:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้จาก Claims
* **บรรทัดที่ 101:** `_repo.DeleteAddress(username, addressId);`  
  สั่ง Repository ให้ลบรายการที่อยู่นี้ออกจากบัญชีผู้ใช้
* **บรรทัดที่ 102:** `TempData["SuccessMessage"] = "ลบที่อยู่จัดส่งเรียบร้อยแล้ว";`  
  แจ้งเตือนข้อความสำเร็จ
* **บรรทัดที่ 103:** `return RedirectToAction(nameof(Profile));`  
  ส่งกลับหน้า Profile
* **บรรทัดที่ 104:** `}`  
  ปิดบล็อกเมธอด DeleteAddress
* **บรรทัดที่ 105:** *(บรรทัดว่าง)*
* **บรรทัดที่ 106:** `// ตั้งเป็นที่อยู่จัดส่งหลัก`  
  คอมเมนต์อธิบายฟังก์ชันตั้งที่อยู่เริ่มต้น
* **บรรทัดที่ 107:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 108:** `[ValidateAntiForgeryToken]`  
  ป้องกันการโจมตี CSRF
* **บรรทัดที่ 109:** `public IActionResult SetDefaultAddress(int addressId)`  
  Action เมธอดรับรหัสที่อยู่ที่ต้องการตั้งเป็นที่อยู่หลัก
* **บรรทัดที่ 110:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 111:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้
* **บรรทัดที่ 112:** `_repo.SetDefaultAddress(username, addressId);`  
  สั่ง Repository ปรับให้ที่อยู่นี้เป็นค่าเริ่มต้น และปลดที่อยู่อื่นทั้งหมดออก
* **บรรทัดที่ 113:** `TempData["SuccessMessage"] = "ตั้งเป็นที่อยู่จัดส่งหลักเรียบร้อยแล้ว";`  
  เก็บข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 114:** `return RedirectToAction(nameof(Profile));`  
  ส่งกลับหน้า Profile
* **บรรทัดที่ 115:** `}`  
  ปิดบล็อกเมธอด SetDefaultAddress
* **บรรทัดที่ 116:** *(บรรทัดว่าง)*
* **บรรทัดที่ 117:** `// หน้าประวัติคำสั่งซื้อของลูกค้า (กรองเฉพาะของบัญชีผู้ใช้นี้)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 118:** `public IActionResult Orders(string? successOrderId = null)`  
  Action เมธอดแสดงรายการประวัติคำสั่งซื้อของลูกค้า
* **บรรทัดที่ 119:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 120:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้
* **บรรทัดที่ 121:** `ViewBag.SuccessOrderId = successOrderId;`  
  ส่งรหัสออเดอร์ที่เพิ่งสั่งซื้อสำเร็จไปยัง View สำหรับแสดง Modal ความสำเร็จ
* **บรรทัดที่ 122:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 123:** `var orders = _repo.GetOrdersForUser(username);`  
  ดึงรายการคำสั่งซื้อเฉพาะของลูกค้ารายนี้จากฐานข้อมูล
* **บรรทัดที่ 124:** `return View(orders);`  
  ส่ง List คำสั่งซื้อไปเรนเดอร์ในหน้า `Views/Account/Orders.cshtml`
* **บรรทัดที่ 125:** `}`  
  ปิดบล็อกเมธอด Orders
* **บรรทัดที่ 126:** *(บรรทัดว่าง)*
* **บรรทัดที่ 127:** `// อัปโหลดหรืออัปเดตหลักฐานสลิปโอนเงินภายหลัง (Req 19)`  
  คอมเมนต์ระบุ Req 19
* **บรรทัดที่ 128:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 129:** `public async Task<IActionResult> UploadSlip(string orderId, IFormFile? paymentSlip)`  
  Action เมธอดรับรหัสออเดอร์และไฟล์รูปภาพสลิปที่ลูกค้าแนบเพิ่มเติม
* **บรรทัดที่ 130:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 131:** `if (paymentSlip == null || paymentSlip.Length == 0)`  
  ตรวจสอบว่ามีการเลือกไฟล์และขนาดไฟล์มากกว่า 0 ไบต์หรือไม่
* **บรรทัดที่ 132:** `{`  
  เปิดบล็อกตรวจสอบไฟล์ว่าง
* **บรรทัดที่ 133:** `TempData["ErrorMessage"] = "กรุณาเลือกไฟล์รูปภาพสลิปการโอนเงิน";`  
  แจ้งเตือนให้เลือกไฟล์
* **บรรทัดที่ 134:** `return RedirectToAction(nameof(Orders));`  
  ส่งกลับหน้า Orders
* **บรรทัดที่ 135:** `}`  
  ปิดบล็อกตรวจสอบไฟล์ว่าง
* **บรรทัดที่ 136:** *(บรรทัดว่าง)*
* **บรรทัดที่ 137:** `var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");`  
  เรียกใช้ Helper ตรวจสอบความถูกต้องของรูปภาพ และบันทึกไฟล์สลิปลงใน `wwwroot/uploads/slips`
* **บรรทัดที่ 138:** `if (uploadErr != null || string.IsNullOrEmpty(uploadedUrl))`  
  ตรวจสอบหากเกิดข้อผิดพลาดในการบันทึกรูป
* **บรรทัดที่ 139:** `{`  
  เปิดบล็อกอัปโหลดล้มเหลว
* **บรรทัดที่ 140:** `TempData["ErrorMessage"] = uploadErr ?? "ไม่สามารถอัปโหลดสลิปได้";`  
  กำหนดข้อความผิดพลาด
* **บรรทัดที่ 141:** `return RedirectToAction(nameof(Orders));`  
  ส่งกลับหน้า Orders
* **บรรทัดที่ 142:** `}`  
  ปิดบล็อกอัปโหลดล้มเหลว
* **บรรทัดที่ 143:** *(บรรทัดว่าง)*
* **บรรทัดที่ 144:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้ปัจจุบัน
* **บรรทัดที่ 145:** `if (_repo.AttachPaymentSlip(username, orderId, uploadedUrl))`  
  ผูก URL ของรูปสลิปเข้ากับออเดอร์ในฐานข้อมูล และปรับสถานะเป็นรอตรวจสอบ
* **บรรทัดที่ 146:** `{`  
  เปิดบล็อกกรณีแนบสลิปสำเร็จ
* **บรรทัดที่ 147:** `TempData["SuccessMessage"] = $"แนบหลักฐานสลิปสำหรับคำสั่งซื้อ #{orderId} เรียบร้อยแล้ว ฝ่ายบัญชีจะตรวจสอบในลำดับถัดไป";`  
  แสดงข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 148:** `}`  
  ปิดบล็อกแนบสลิปสำเร็จ
* **บรรทัดที่ 149:** `else`  
  กรณีแนบสลิปไม่สำเร็จ (เนื่องจากออเดอร์ถูกตรวจสอบหรือล็อกแล้ว)
* **บรรทัดที่ 150:** `{`
* **บรรทัดที่ 151:** `TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือเปลี่ยนรูปสลิปได้ เนื่องจากคำสั่งซื้อนี้มีการแนบหลักฐานสลิปและถูกล็อกแล้ว";`  
  แจ้งเตือนว่าคำสั่งซื้อนี้ถูกล็อกแล้ว
* **บรรทัดที่ 152:** `}`
* **บรรทัดที่ 153:** *(บรรทัดว่าง)*
* **บรรทัดที่ 154:** `return RedirectToAction(nameof(Orders));`  
  ส่งกลับไปยังหน้าประวัติคำสั่งซื้อ
* **บรรทัดที่ 155:** `}`  
  ปิดบล็อกเมธอด UploadSlip
* **บรรทัดที่ 156:** *(บรรทัดว่าง)*
* **บรรทัดที่ 157:** `// ลูกค้าขอยกเลิกคำสั่งซื้อ / ขอคืนเงิน (Req 20)`  
  คอมเมนต์ระบุ Req 20
* **บรรทัดที่ 158:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 159:** `public IActionResult CancelOrder(string orderId, string reason)`  
  Action เมธอดรับรหัสออเดอร์และเหตุผลที่ลูกค้าต้องการยกเลิก
* **บรรทัดที่ 160:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 161:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้
* **บรรทัดที่ 162:** `if (_repo.CancelOrder(username, orderId, reason))`  
  เรียกใช้คำสั่งยกเลิกออเดอร์ใน Repository (หากยังไม่จ่ายเงินจะคืนสต็อกทันที หากจ่ายแล้วจะส่งต่อเป็นคำขอคืนเงิน RefundRequested)
* **บรรทัดที่ 163:** `{`
* **บรรทัดที่ 164:** `TempData["SuccessMessage"] = $"ดำเนินการยกเลิกคำสั่งซื้อ {orderId} เรียบร้อยแล้ว";`  
  แจ้งผลสำเร็จ
* **บรรทัดที่ 165:** `}`
* **บรรทัดที่ 166:** `else`
* **บรรทัดที่ 167:** `{`
* **บรรทัดที่ 168:** `TempData["ErrorMessage"] = "ไม่สามารถขอยกเลิกคำสั่งซื้อนี้ได้";`  
  แจ้งข้อผิดพลาดกรณีสถานะออเดอร์ไม่อยู่ในเกณฑ์ที่ยกเลิกได้
* **บรรทัดที่ 169:** `}`
* **บรรทัดที่ 170:** `return RedirectToAction(nameof(Orders));`  
  ส่งกลับสู่หน้าประวัติคำสั่งซื้อ
* **บรรทัดที่ 171:** `}`  
  ปิดบล็อกเมธอด CancelOrder
* **บรรทัดที่ 172:** *(บรรทัดว่าง)*
* **บรรทัดที่ 173:** `// หน้าแจ้งปัญหาการใช้งานระบบ (Helpdesk สำหรับลูกค้า Req 22)`  
  คอมเมนต์ระบุ Req 22
* **บรรทัดที่ 174:** `public IActionResult Support()`  
  Action เมธอดแสดงหน้าแบบฟอร์มเปิดตั๋ว Support และรายการตั๋วปัญหาที่ลูกค้าเคยแจ้งไว้
* **บรรทัดที่ 175:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 176:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้
* **บรรทัดที่ 177:** `var user = _repo.GetUserByUsername(username);`  
  คิวรีข้อมูลโปรไฟล์ผู้ใช้
* **บรรทัดที่ 178:** *(บรรทัดว่าง)*
* **บรรทัดที่ 179:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทจำลองไปยัง View
* **บรรทัดที่ 180:** `ViewBag.CurrentUser = user;`  
  ส่งข้อมูลผู้ใช้ไปยัง View สำหรับแสดงข้อมูลผู้แจ้งล่วงหน้า
* **บรรทัดที่ 181:** `ViewBag.MyTickets = _repo.GetSupportTickets(customerEmail: user?.Email);`  
  คิวรีตั๋วปัญหาที่เปิดด้วยอีเมลของลูกค้ารายนี้
* **บรรทัดที่ 182:** `return View();`  
  เรนเดอร์หน้า `Views/Account/Support.cshtml`
* **บรรทัดที่ 183:** `}`  
  ปิดบล็อกเมธอด Support
* **บรรทัดที่ 184:** *(บรรทัดว่าง)*
* **บรรทัดที่ 185:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 186:** `public IActionResult SubmitTicket(string customerName, string email, string? phone, string category, string subject, string description, TicketPriority priority)`  
  Action เมธอดรับข้อมูลรายละเอียดตั๋วปัญหาที่ลูกค้ากรอกจากฟอร์ม
* **บรรทัดที่ 187:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 188:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้
* **บรรทัดที่ 189:** `var user = _repo.GetUserByUsername(username);`  
  ดึงข้อมูลผู้ใช้เพื่อนำเบอร์โทรศัพท์สำรองมาใช้กรณีในฟอร์มเว้นว่าง
* **บรรทัดที่ 190:** *(บรรทัดว่าง)*
* **บรรทัดที่ 191:** `var ticket = _repo.CreateSupportTicket(`  
  เรียกคำสั่งสร้าง Support Ticket ใหม่ใน Repository
* **บรรทัดที่ 192:** `    customerName,`  
  ส่งชื่อผู้แจ้ง
* **บรรทัดที่ 193:** `    email,`  
  ส่งอีเมล
* **บรรทัดที่ 194:** `    phone ?? user?.Phone ?? "",`  
  ส่งเบอร์โทรศัพท์
* **บรรทัดที่ 195:** `    category,`  
  ส่งหมวดหมู่ปัญหา
* **บรรทัดที่ 196:** `    subject,`  
  ส่งหัวข้อปัญหา
* **บรรทัดที่ 197:** `    description,`  
  ส่งรายละเอียดของปัญหา
* **บรรทัดที่ 198:** `    priority);`  
  ส่งระดับความสำคัญของปัญหา
* **บรรทัดที่ 199:** *(บรรทัดว่าง)*
* **บรรทัดที่ 200:** `TempData["SuccessMessage"] = $"บันทึกการแจ้งปัญหาเรียบร้อยแล้ว (รหัสอ้างอิง: {ticket.TicketCode}) ทางทีม IT Support จะรีบตรวจสอบ";`  
  แจ้งรหัสอ้างอิงตั๋วปัญหาให้ลูกค้าทราบ
* **บรรทัดที่ 201:** `return RedirectToAction(nameof(Support));`  
  ส่งกลับไปยังหน้า Support
* **บรรทัดที่ 202:** `}`  
  ปิดบล็อกเมธอด SubmitTicket
* **บรรทัดที่ 203:** `}`  
  ปิดบล็อกคลาส AccountController
* **บรรทัดที่ 204:** `}`  
  ปิดบล็อกเนมสเปซ


---

# 2. AccountingController.cs (123 บรรทัด)

### โค้ดต้นฉบับเต็มของ AccountingController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using CSI402_Project_final.Models;
4: using System.Globalization;
5: using System.Linq;
6: using System.Text;
7: 
8: namespace CSI402_Project_final.Controllers
9: {
10:     [Authorize(Roles = "AccountingStaff,Owner,Admin")]
11:     public class AccountingController : Controller
12:     {
13:         private readonly PetShopRepository _repo;
14: 
15:         public AccountingController(PetShopRepository repo)
16:         {
17:             _repo = repo;
18:         }
19: 
20:         // หน้าภาพรวมการเงิน ตรวจสอบสลิป/ชำระเงิน คำขอคืนเงิน และรายรับ-รายจ่าย (Req 19, 20, 21)
21:         public IActionResult Index()
22:         {
23:             var allOrders = _repo.Orders;
24:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
25:             ViewBag.FinancialSummary = _repo.GetFinancialSummary();
26: 
27:             ViewBag.PendingPayments = allOrders
28:                 .Where(o => o.Status == OrderStatus.PendingPayment)
29:                 .ToList();
30: 
31:             ViewBag.PendingRefunds = allOrders
32:                 .Where(o => o.Status == OrderStatus.RefundRequested)
33:                 .ToList();
34: 
35:             var recentOrders = allOrders.Take(15).ToList();
36:             return View(recentOrders);
37:         }
38: 
39:         // อนุมัติการชำระเงิน / สลิปโอนเงิน (Req 19)
40:         [HttpPost]
41:         public IActionResult ApprovePayment(string orderId, string? returnUrl = null, string? role = null)
42:         {
43:             var order = _repo.ApprovePayment(orderId);
44:             if (order != null)
45:             {
46:                 TempData["SuccessMessage"] = $"อนุมัติการชำระเงินคำสั่งซื้อ #{orderId} (฿{order.GrandTotal:N2}) และส่งเข้าคิวฝ่ายจัดเตรียมสินค้าแล้ว";
47:             }
48:             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
49:             {
50:                 return LocalRedirect(returnUrl);
51:             }
52:             return RedirectToAction(nameof(Index), new { role });
53:         }
54: 
55:         // ปฏิเสธสลิปการชำระเงิน (Req 19)
56:         [HttpPost]
57:         public IActionResult RejectPayment(string orderId, string reason, string? returnUrl = null, string? role = null)
58:         {
59:             var order = _repo.RejectPayment(orderId, reason);
60:             if (order != null)
61:             {
62:                 TempData["SuccessMessage"] = $"ปฏิเสธหลักฐานชำระเงินและยกเลิกคำสั่งซื้อ #{orderId} พร้อมคืนสต็อกสินค้าเรียบร้อยแล้ว";
63:             }
64:             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
65:             {
66:                 return LocalRedirect(returnUrl);
67:             }
68:             return RedirectToAction(nameof(Index), new { role });
69:         }
70: 
71:         // อนุมัติการคืนเงิน (Req 20)
72:         [HttpPost]
73:         public IActionResult ApproveRefund(string orderId, string? returnUrl = null, string? role = null)
74:         {
75:             var order = _repo.ApproveRefund(orderId);
76:             if (order != null)
77:             {
78:                 TempData["SuccessMessage"] = $"อนุมัติการคืนเงินจำนวน ฿{order.RefundAmount:N2} สำหรับคำสั่งซื้อ {orderId} สำเร็จ";
79:             }
80:             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
81:             {
82:                 return LocalRedirect(returnUrl);
83:             }
84:             return RedirectToAction(nameof(Index), new { role });
85:         }
86: 
87:         // บันทึกรายจ่ายจริงรายเดือน (Req 21)
88:         [HttpPost]
89:         public IActionResult SaveMonthlyExpense(string month, decimal expenses)
90:         {
91:             if (string.IsNullOrWhiteSpace(month))
92:             {
93:                 TempData["ErrorMessage"] = "กรุณาระบุเดือนที่ต้องการบันทึกรายจ่าย";
94:                 return RedirectToAction(nameof(Index));
95:             }
96: 
97:             _repo.SaveMonthlyExpense(month, expenses);
98:             TempData["SuccessMessage"] = $"บันทึกรายจ่ายประจำเดือน {month} จำนวน ฿{expenses:N2} ลงฐานข้อมูลเรียบร้อยแล้ว";
99:             return RedirectToAction(nameof(Index));
100:         }
101: 
102:         // ส่งออกรายงานรายรับ-รายจ่าย CSV (Req 21)
103:         public IActionResult ExportFinancialCsv()
104:         {
105:             var summary = _repo.GetFinancialSummary();
106:             var sb = new StringBuilder();
107:             sb.AppendLine("Month,Revenue(THB),Expenses(THB),Profit(THB)");
108:             foreach (var row in summary.MonthlyBreakdown)
109:             {
110:                 sb.AppendLine($"{row.Month},{row.Revenue:F2},{row.Expenses:F2},{row.Profit:F2}");
111:             }
112:             sb.AppendLine();
113:             sb.AppendLine($"Total Revenue,{summary.TotalRevenue:F2}");
114:             sb.AppendLine($"Total Expenses,{summary.TotalExpenses:F2}");
115:             sb.AppendLine($"Total Refunds,{summary.TotalRefunds:F2}");
116:             sb.AppendLine($"Net Profit,{summary.NetProfit:F2}");
117: 
118:             byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
119:             string fileName = $"financial_report_{System.DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";
120:             return File(bytes, "text/csv; charset=utf-8", fileName);
121:         }
122:     }
123: }
```

### คำอธิบายการทำงานของ AccountingController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าเนมสเปซสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาส Controller, IActionResult, HttpGet, HttpPost, File
* **บรรทัดที่ 3:** `using CSI402_Project_final.Models;`  
  นำเข้าคลาส Model ทางการเงิน, Order, OrderStatus, PetShopRepository
* **บรรทัดที่ 4:** `using System.Globalization;`  
  นำเข้า CultureInfo สำหรับจัดรูปแบบวันที่ของชื่อไฟล์ CSV
* **บรรทัดที่ 5:** `using System.Linq;`  
  นำเข้าคำสั่งจัดการคอลเลกชัน เช่น `.Where()`, `.Take()`, `.Concat()`
* **บรรทัดที่ 6:** `using System.Text;`  
  นำเข้า StringBuilder และ Encoding สำหรับสร้างข้อความ CSV และแปลงเป็น UTF-8 BOM
* **บรรทัดที่ 7:** *(บรรทัดว่าง)*
* **บรรทัดที่ 8:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 9:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 10:** `[Authorize(Roles = "AccountingStaff,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะบัญชีที่มีบทบาทเป็นพนักงานบัญชี (AccountingStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin) เท่านั้น
* **บรรทัดที่ 11:** `public class AccountingController : Controller`  
  ประกาศคลาส `AccountingController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 12:** `{`  
  เปิดบล็อกของคลาส
* **บรรทัดที่ 13:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการข้อมูลคำสั่งซื้อและบัญชีการเงิน
* **บรรทัดที่ 14:** *(บรรทัดว่าง)*
* **บรรทัดที่ 15:** `public AccountingController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 16:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 17:** `_repo = repo;`  
  กำหนดค่าให้กับฟิลด์ภายใน
* **บรรทัดที่ 18:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 19:** *(บรรทัดว่าง)*
* **บรรทัดที่ 20:** `// หน้าภาพรวมการเงิน ตรวจสอบสลิป/ชำระเงิน คำขอคืนเงิน และรายรับ-รายจ่าย (Req 19, 20, 21)`  
  คอมเมนต์อธิบายฟังก์ชันแดชบอร์ดการเงิน
* **บรรทัดที่ 21:** `public IActionResult Index()`  
  Action เมธอดแสดงแดชบอร์ดฝ่ายบัญชีผ่าน HTTP GET
* **บรรทัดที่ 22:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 23:** `var allOrders = _repo.Orders;`  
  ดึงคำสั่งซื้อทั้งหมดในระบบจาก Repository
* **บรรทัดที่ 24:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 25:** `ViewBag.FinancialSummary = _repo.GetFinancialSummary();`  
  คิวรีสรุปยอดการเงิน (ยอดขาย, รายจ่าย, คืนเงิน, กำไรสุทธิ, ตารางรายเดือน) บรรจุใน ViewBag
* **บรรทัดที่ 26:** *(บรรทัดว่าง)*
* **บรรทัดที่ 27:** `ViewBag.PendingPayments = allOrders`  
  เริ่มคัดกรองออเดอร์ที่รอตรวจสอบสลิป
* **บรรทัดที่ 28:** `    .Where(o => o.Status == OrderStatus.PendingPayment)`  
  เลือกเฉพาะออเดอร์ที่มีสถานะ `PendingPayment`
* **บรรทัดที่ 29:** `    .ToList();`  
  แปลงเป็น List และใส่ใน ViewBag.PendingPayments
* **บรรทัดที่ 30:** *(บรรทัดว่าง)*
* **บรรทัดที่ 31:** `ViewBag.PendingRefunds = allOrders`  
  เริ่มคัดกรองออเดอร์ที่มีลูกค้ายื่นขอคืนเงิน
* **บรรทัดที่ 32:** `    .Where(o => o.Status == OrderStatus.RefundRequested)`  
  เลือกเฉพาะออเดอร์ที่มีสถานะ `RefundRequested`
* **บรรทัดที่ 33:** `    .ToList();`  
  แปลงเป็น List และใส่ใน ViewBag.PendingRefunds
* **บรรทัดที่ 34:** *(บรรทัดว่าง)*
* **บรรทัดที่ 35:** `var recentOrders = allOrders.Take(15).ToList();`  
  ดึงคำสั่งซื้อล่าสุด 15 รายการ
* **บรรทัดที่ 36:** `return View(recentOrders);`  
  ส่งรายการออเดอร์ล่าสุดไปแสดงผลใน `Views/Accounting/Index.cshtml`
* **บรรทัดที่ 37:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 38:** *(บรรทัดว่าง)*
* **บรรทัดที่ 39:** `// อนุมัติการชำระเงิน / สลิปโอนเงิน (Req 19)`  
  คอมเมนต์ระบุ Req 19
* **บรรทัดที่ 40:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 41:** `public IActionResult ApprovePayment(string orderId, string? returnUrl = null, string? role = null)`  
  Action เมธอดรับรหัสออเดอร์ที่ต้องการอนุมัติสลิป
* **บรรทัดที่ 42:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 43:** `var order = _repo.ApprovePayment(orderId);`  
  สั่ง Repository ให้เปลี่ยนสถานะออเดอร์เป็น `PaymentVerified` และบันทึกยอดเงินเข้าบัญชีรายรับจริง
* **บรรทัดที่ 44:** `if (order != null)`  
  ตรวจสอบว่าอนุมัติสำเร็จ
* **บรรทัดที่ 45:** `{`
* **บรรทัดที่ 46:** `TempData["SuccessMessage"] = $"อนุมัติการชำระเงินคำสั่งซื้อ #{orderId} (฿{order.GrandTotal:N2}) และส่งเข้าคิวฝ่ายจัดเตรียมสินค้าแล้ว";`  
  เก็บข้อความแจ้งเตือนความสำเร็จพร้อมระบุยอดเงิน
* **บรรทัดที่ 47:** `}`
* **บรรทัดที่ 48:** `if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))`  
  ตรวจสอบความถูกต้องของ returnUrl
* **บรรทัดที่ 49:** `{`
* **บรรทัดที่ 50:** `return LocalRedirect(returnUrl);`  
  ส่งกลับไปยัง URL หน้าเดิม
* **บรรทัดที่ 51:** `}`
* **บรรทัดที่ 52:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับไปยังหน้า Index ของฝ่ายบัญชี
* **บรรทัดที่ 53:** `}`  
  ปิดบล็อกเมธอด ApprovePayment
* **บรรทัดที่ 54:** *(บรรทัดว่าง)*
* **บรรทัดที่ 55:** `// ปฏิเสธสลิปการชำระเงิน (Req 19)`  
  คอมเมนต์ระบุ Req 19
* **บรรทัดที่ 56:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 57:** `public IActionResult RejectPayment(string orderId, string reason, string? returnUrl = null, string? role = null)`  
  Action เมธอดรับรหัสออเดอร์และเหตุผลในการปฏิเสธสลิป
* **บรรทัดที่ 58:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 59:** `var order = _repo.RejectPayment(orderId, reason);`  
  สั่ง Repository ปรับสถานะเป็นยกเลิก และทำการคืนสต็อกสินค้าทุกชิ้นในออเดอร์กลับเข้าคลังอัตโนมัติ
* **บรรทัดที่ 60:** `if (order != null)`  
  ตรวจสอบว่าการปฏิเสธสำเร็จ
* **บรรทัดที่ 61:** `{`
* **บรรทัดที่ 62:** `TempData["SuccessMessage"] = $"ปฏิเสธหลักฐานชำระเงินและยกเลิกคำสั่งซื้อ #{orderId} พร้อมคืนสต็อกสินค้าเรียบร้อยแล้ว";`  
  แสดงข้อความแจ้งเตือนความสำเร็จและยืนยันการคืนสต็อก
* **บรรทัดที่ 63:** `}`
* **บรรทัดที่ 64:** `if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))`  
  ตรวจสอบ returnUrl
* **บรรทัดที่ 65:** `{`
* **บรรทัดที่ 66:** `return LocalRedirect(returnUrl);`  
  ส่งกลับหน้าเดิม
* **บรรทัดที่ 67:** `}`
* **บรรทัดที่ 68:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 69:** `}`  
  ปิดบล็อกเมธอด RejectPayment
* **บรรทัดที่ 70:** *(บรรทัดว่าง)*
* **บรรทัดที่ 71:** `// อนุมัติการคืนเงิน (Req 20)`  
  คอมเมนต์ระบุ Req 20
* **บรรทัดที่ 72:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 73:** `public IActionResult ApproveRefund(string orderId, string? returnUrl = null, string? role = null)`  
  Action เมธอดอนุมัติการคืนเงินให้ลูกค้า
* **บรรทัดที่ 74:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 75:** `var order = _repo.ApproveRefund(orderId);`  
  สั่ง Repository เปลี่ยนสถานะเป็น `Refunded` และตัดลดยอดขายสุทธิในระบบบัญชี
* **บรรทัดที่ 76:** `if (order != null)`  
  ตรวจสอบผลลัพธ์
* **บรรทัดที่ 77:** `{`
* **บรรทัดที่ 78:** `TempData["SuccessMessage"] = $"อนุมัติการคืนเงินจำนวน ฿{order.RefundAmount:N2} สำหรับคำสั่งซื้อ {orderId} สำเร็จ";`  
  แจ้งเตือนจำนวนเงินที่คืนให้ลูกค้าสำเร็จ
* **บรรทัดที่ 79:** `}`
* **บรรทัดที่ 80:** `if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))`
* **บรรทัดที่ 81:** `{`
* **บรรทัดที่ 82:** `return LocalRedirect(returnUrl);`
* **บรรทัดที่ 83:** `}`
* **บรรทัดที่ 84:** `return RedirectToAction(nameof(Index), new { role });`
* **บรรทัดที่ 85:** `}`  
  ปิดบล็อกเมธอด ApproveRefund
* **บรรทัดที่ 86:** *(บรรทัดว่าง)*
* **บรรทัดที่ 87:** `// บันทึกรายจ่ายจริงรายเดือน (Req 21)`  
  คอมเมนต์ระบุ Req 21
* **บรรทัดที่ 88:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 89:** `public IActionResult SaveMonthlyExpense(string month, decimal expenses)`  
  Action เมธอดรับชื่อเดือน (เช่น "2026-11") และยอดเงินรายจ่ายจริง
* **บรรทัดที่ 90:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 91:** `if (string.IsNullOrWhiteSpace(month))`  
  ตรวจสอบว่ามีการระบุเดือนหรือไม่
* **บรรทัดที่ 92:** `{`
* **บรรทัดที่ 93:** `TempData["ErrorMessage"] = "กรุณาระบุเดือนที่ต้องการบันทึกรายจ่าย";`  
  แจ้งเตือนข้อผิดพลาด
* **บรรทัดที่ 94:** `return RedirectToAction(nameof(Index));`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 95:** `}`
* **บรรทัดที่ 96:** *(บรรทัดว่าง)*
* **บรรทัดที่ 97:** `_repo.SaveMonthlyExpense(month, expenses);`  
  สั่ง Repository บันทึกรายจ่ายประจำเดือนลงฐานข้อมูลและคำนวณกำไรสุทธิใหม่
* **บรรทัดที่ 98:** `TempData["SuccessMessage"] = $"บันทึกรายจ่ายประจำเดือน {month} จำนวน ฿{expenses:N2} ลงฐานข้อมูลเรียบร้อยแล้ว";`  
  แสดงข้อความสำเร็จ
* **บรรทัดที่ 99:** `return RedirectToAction(nameof(Index));`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 100:** `}`  
  ปิดบล็อกเมธอด SaveMonthlyExpense
* **บรรทัดที่ 101:** *(บรรทัดว่าง)*
* **บรรทัดที่ 102:** `// ส่งออกรายงานรายรับ-รายจ่าย CSV (Req 21)`  
  คอมเมนต์ระบุ Req 21
* **บรรทัดที่ 103:** `public IActionResult ExportFinancialCsv()`  
  Action เมธอดสร้างและส่งออกไฟล์สเปรดชีต CSV ผ่าน HTTP GET
* **บรรทัดที่ 104:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 105:** `var summary = _repo.GetFinancialSummary();`  
  ดึงข้อมูลสรุปการเงินทั้งหมด
* **บรรทัดที่ 106:** `var sb = new StringBuilder();`  
  สร้าง StringBuilder สำหรับต่อข้อความ CSV
* **บรรทัดที่ 107:** `sb.AppendLine("Month,Revenue(THB),Expenses(THB),Profit(THB)");`  
  เขียนแถวหัวตาราง (Header Row)
* **บรรทัดที่ 108:** `foreach (var row in summary.MonthlyBreakdown)`  
  วนลูปข้อมูลรายรับ-รายจ่ายของแต่ละเดือน
* **บรรทัดที่ 109:** `{`
* **บรรทัดที่ 110:** `sb.AppendLine($"{row.Month},{row.Revenue:F2},{row.Expenses:F2},{row.Profit:F2}");`  
  เขียนข้อมูลรายเดือนลงในแต่ละแถว โดยแสดงทศนิยม 2 ตำแหน่ง
* **บรรทัดที่ 111:** `}`
* **บรรทัดที่ 112:** `sb.AppendLine();`  
  เว้นหนึ่งบรรทัดว่าง
* **บรรทัดที่ 113:** `sb.AppendLine($"Total Revenue,{summary.TotalRevenue:F2}");`  
  เขียนสรุปยอดรายรับรวม
* **บรรทัดที่ 114:** `sb.AppendLine($"Total Expenses,{summary.TotalExpenses:F2}");`  
  เขียนสรุปยอดรายจ่ายรวม
* **บรรทัดที่ 115:** `sb.AppendLine($"Total Refunds,{summary.TotalRefunds:F2}");`  
  เขียนสรุปยอดการคืนเงินรวม
* **บรรทัดที่ 116:** `sb.AppendLine($"Net Profit,{summary.NetProfit:F2}");`  
  เขียนสรุปยอดกำไรสุทธิ
* **บรรทัดที่ 117:** *(บรรทัดว่าง)*
* **บรรทัดที่ 118:** `byte[] bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();`  
  แปลงข้อความเป็น Byte Array พร้อมทั้งแทรก **UTF-8 BOM (Byte Order Mark)** ไว้หน้าสุด เพื่อให้โปรแกรม Microsoft Excel เปิดอ่านภาษาไทยได้อย่างถูกต้อง
* **บรรทัดที่ 119:** `string fileName = $"financial_report_{System.DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";`  
  สร้างชื่อไฟล์แบบไดนามิกตามวันที่ปัจจุบัน
* **บรรทัดที่ 120:** `return File(bytes, "text/csv; charset=utf-8", fileName);`  
  ส่งคืนไฟล์สตรีมให้เบราว์เซอร์ดาวน์โหลดทันทีด้วย MIME Type `text/csv; charset=utf-8`
* **บรรทัดที่ 121:** `}`  
  ปิดบล็อกเมธอด ExportFinancialCsv
* **บรรทัดที่ 122:** `}`  
  ปิดบล็อกคลาส AccountingController
* **บรรทัดที่ 123:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 3. AdminController.cs (212 บรรทัด)

### โค้ดต้นฉบับเต็มของ AdminController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using Microsoft.AspNetCore.SignalR;
4: using CSI402_Project_final.Models;
5: using CSI402_Project_final.Hubs;
6: using System.Linq;
7: using System.Threading.Tasks;
8: 
9: namespace CSI402_Project_final.Controllers
10: {
11:     [Authorize(Roles = "Admin,Owner")]
12:     public class AdminController : Controller
13:     {
14:         private readonly PetShopRepository _repo;
15:         private readonly IHubContext<PetShopHub> _hubContext;
16: 
17:         public AdminController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)
18:         {
19:             _repo = repo;
20:             _hubContext = hubContext;
21:         }
22: 
23:         // ตรวจสอบว่าผู้ใช้งานปัจจุบันที่กำลังทำรายการมีสถานะเป็น Owner หรือไม่ (รองรับทั้ง Claims และ Multi-Role Tab Switcher)
24:         private bool IsCurrentOperatorOwner(string? roleParam)
25:         {
26:             if (!string.IsNullOrEmpty(roleParam))
27:             {
28:                 var resolved = _repo.ResolveTabUserInfo(roleParam);
29:                 return string.Equals(resolved.RoleName, "Owner", System.StringComparison.OrdinalIgnoreCase);
30:             }
31:             return User.IsInRole("Owner");
32:         }
33: 
34:         // หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin) พร้อมระบบกรองตามบทบาท (Role Filter) & ค้นหา
35:         public IActionResult Index(string? role = null, string? roleFilter = "all", string? search = null)
36:         {
37:             var allUsers = _repo.Users;
38:             foreach (var u in allUsers)
39:             {
40:                 _repo.CheckAndRefreshUserSuspension(u);
41:             }
42:             allUsers = _repo.Users;
43: 
44:             bool isOwner = IsCurrentOperatorOwner(role);
45:             ViewBag.IsCurrentOwner = isOwner;
46:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
47:             ViewBag.TotalUsers = allUsers.Count;
48:             ViewBag.TotalStaff = allUsers.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
49:             ViewBag.TotalCustomers = allUsers.Count(u => u.Role == UserRole.Customer);
50: 
51:             string filter = (roleFilter ?? "all").Trim().ToLowerInvariant();
52:             IEnumerable<SystemUser> filtered = allUsers;
53: 
54:             if (filter == "staff")
55:             {
56:                 filtered = filtered.Where(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);
57:             }
58:             else if (filter == "customer")
59:             {
60:                 filtered = filtered.Where(u => u.Role == UserRole.Customer);
61:             }
62:             else if (Enum.TryParse<UserRole>(filter, true, out var specificRole))
63:             {
64:                 filtered = filtered.Where(u => u.Role == specificRole);
65:             }
66: 
67:             if (!string.IsNullOrWhiteSpace(search))
68:             {
69:                 string kw = search.Trim();
70:                 filtered = filtered.Where(u =>
71:                     u.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
72:                     u.FullName.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
73:                     u.Email.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
74:             }
75: 
76:             ViewBag.RoleFilter = filter;
77:             ViewBag.Search = search;
78: 
79:             return View(filtered.ToList());
80:         }
81: 
82:         // ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน + ระงับบัญชีพร้อมเหตุผลและระยะเวลา (เตะออกทันทีผ่าน SignalR)
83:         [HttpPost]
84:         public async Task<IActionResult> UpdateUserRole(
85:             int userId,
86:             UserRole newRole,
87:             bool isActive,
88:             string? suspendReason = null,
89:             int suspendDurationMinutes = -1,
90:             string? role = null,
91:             string? roleFilter = null,
92:             string? search = null)
93:         {
94:             bool isOwner = IsCurrentOperatorOwner(role);
95:             var targetUser = _repo.Users.FirstOrDefault(u => u.Id == userId);
96:             if (targetUser == null)
97:             {
98:                 TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้งานที่ต้องการแก้ไข";
99:                 return RedirectToAction(nameof(Index), new { role, roleFilter, search });
100:             }
101: 
102:             // กฎความปลอดภัย: Admin ไม่สามารถแก้ไข/ระงับบัญชี Admin หรือ Owner ได้ และไม่สามารถแต่งตั้งใครเป็น Admin หรือ Owner ได้ (เฉพาะ Owner เท่านั้น)
103:             if (!isOwner)
104:             {
105:                 if (targetUser.Role == UserRole.Admin || targetUser.Role == UserRole.Owner)
106:                 {
107:                     TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือระงับบัญชี Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
108:                     return RedirectToAction(nameof(Index), new { role, roleFilter, search });
109:                 }
110:                 if (newRole == UserRole.Admin || newRole == UserRole.Owner)
111:                 {
112:                     TempData["ErrorMessage"] = "ไม่สามารถแต่งตั้งผู้ใช้เป็น Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
113:                     return RedirectToAction(nameof(Index), new { role, roleFilter, search });
114:                 }
115:             }
116: 
117:             if (_repo.UpdateUserRole(userId, newRole, isActive, suspendReason, suspendDurationMinutes))
118:             {
119:                 if (!isActive)
120:                 {
121:                     string reasonText = targetUser.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
122:                     string untilText = targetUser.SuspensionDisplayUntil;
123: 
124:                     // ส่งสัญญาณ Real-Time เตะผู้ใช้ออกจากระบบทุกแท็บที่กำลังล็อกอินบัญชีนี้อยู่ทันที
125:                     await _hubContext.Clients.All.SendAsync(
126:                         "ReceiveAccountSuspended",
127:                         targetUser.Username,
128:                         targetUser.Role.ToString(),
129:                         reasonText,
130:                         untilText);
131: 
132:                     TempData["SuccessMessage"] = $"ระงับบัญชี '{targetUser.Username}' เรียบร้อยแล้ว ({untilText} | เหตุผล: {reasonText})";
133:                 }
134:                 else
135:                 {
136:                     TempData["SuccessMessage"] = $"อัปเดตบัญชีผู้ใช้งาน #{userId} ({targetUser.Username}) เป็น '{newRole}' (สถานะ: เปิดใช้งานปกติ) สำเร็จ";
137:                 }
138:             }
139:             return RedirectToAction(nameof(Index), new { role, roleFilter, search });
140:         }
141: 
142:         // เพิ่มผู้ใช้งานใหม่ (บันทึกลง DB) - Admin ไม่สามารถสร้าง User บทบาท Admin ได้ ต้องให้ Owner ทำเท่านั้น
143:         [HttpPost]
144:         public IActionResult CreateUser(SystemUser user, string? newUserRole = null, string? role = null)
145:         {
146:             // ใช้ชื่อฟิลด์ newUserRole แยกจาก role (บทบาทของแท็บผู้ทำรายการ) เพื่อไม่ให้ Model Binding ชนกัน
147:             if (string.IsNullOrWhiteSpace(newUserRole) || !Enum.TryParse<UserRole>(newUserRole, true, out var requestedRole))
148:             {
149:                 TempData["ErrorMessage"] = "กรุณาเลือกบทบาทหน้าที่ (Role) ให้ถูกต้อง";
150:                 return RedirectToAction(nameof(Index), new { role });
151:             }
152:             user.Role = requestedRole;
153: 
154:             bool isOwner = IsCurrentOperatorOwner(role);
155:             if ((requestedRole == UserRole.Admin || requestedRole == UserRole.Owner) && !isOwner)
156:             {
157:                 TempData["ErrorMessage"] = "ไม่สามารถสร้างบัญชีบทบาท Admin ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";
158:                 return RedirectToAction(nameof(Index), new { role });
159:             }
160: 
161:             var (success, message) = _repo.CreateUser(user);
162:             if (success)
163:                 TempData["SuccessMessage"] = message;
164:             else
165:                 TempData["ErrorMessage"] = message;
166: 
167:             return RedirectToAction(nameof(Index), new { role });
168:         }
169: 
170:         // หน้าตรวจสอบบันทึกกิจกรรมย้อนหลัง (Audit Log)
171:         public IActionResult AuditLogs(string? search = null, string? roleFilter = null, string? actionFilter = null, string? role = null)
172:         {
173:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
174:             var logs = _repo.GetAuditLogs(500);
175: 
176:             ViewBag.TotalLogsCount = logs.Count;
177:             ViewBag.SecurityLogsCount = logs.Count(l => l.Action == "UPDATE_USER_ROLE" || l.Action == "SUSPEND_ACCOUNT" || l.Action == "CREATE_USER");
178:             ViewBag.FinanceLogsCount = logs.Count(l => l.Action.Contains("PAYMENT") || l.Action.Contains("REFUND") || l.Action.Contains("EXPENSE"));
179:             ViewBag.InventoryLogsCount = logs.Count(l => l.Action.Contains("PRODUCT") || l.Action.Contains("STOCK"));
180: 
181:             if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all")
182:             {
183:                 logs = logs.Where(l => string.Equals(l.UserRole, roleFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
184:             }
185: 
186:             if (!string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "all")
187:             {
188:                 logs = logs.Where(l => string.Equals(l.Action, actionFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();
189:             }
190: 
191:             if (!string.IsNullOrWhiteSpace(search))
192:             {
193:                 string kw = search.Trim();
194:                 logs = logs.Where(l =>
195:                     (!string.IsNullOrEmpty(l.Username) && l.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
196:                     (!string.IsNullOrEmpty(l.Action) && l.Action.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
197:                     (!string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
198:                     (!string.IsNullOrEmpty(l.RecordId) && l.RecordId.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
199:                     (!string.IsNullOrEmpty(l.Details) && l.Details.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||
200:                     (!string.IsNullOrEmpty(l.IpAddress) && l.IpAddress.Contains(kw, System.StringComparison.OrdinalIgnoreCase))
201:                 ).ToList();
202:             }
203: 
204:             ViewBag.Search = search;
205:             ViewBag.RoleFilter = roleFilter ?? "all";
206:             ViewBag.ActionFilter = actionFilter ?? "all";
207:             ViewBag.FilteredCount = logs.Count;
208: 
209:             return View(logs);
210:         }
211:     }
212: }
```

### คำอธิบายการทำงานของ AdminController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาส Controller, IActionResult, HttpPost, RedirectToAction
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.SignalR;`  
  นำเข้า SignalR `IHubContext` สำหรับส่งสัญญาณเตะผู้ใช้ออกจากระบบแบบ Real-Time
* **บรรทัดที่ 4:** `using CSI402_Project_final.Models;`  
  นำเข้า Data Model ของระบบผู้ใช้, บทบาท UserRole, AuditLog, PetShopRepository
* **บรรทัดที่ 5:** `using CSI402_Project_final.Hubs;`  
  นำเข้า `PetShopHub` สำหรับเชื่อมต่อ WebSocket
* **บรรทัดที่ 6:** `using System.Linq;`  
  นำเข้าคำสั่งจัดการคอลเลกชัน LINQ เช่น `.Where()`, `.Count()`, `.FirstOrDefault()`
* **บรรทัดที่ 7:** `using System.Threading.Tasks;`  
  นำเข้า Asynchronous Task สำหรับ SignalR
* **บรรทัดที่ 8:** *(บรรทัดว่าง)*
* **บรรทัดที่ 9:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 11:** `[Authorize(Roles = "Admin,Owner")]`  
  จำกัดสิทธิ์ระดับคลาส เฉพาะผู้ดูแลระบบ (Admin) หรือผู้บริหาร (Owner) เท่านั้น
* **บรรทัดที่ 12:** `public class AdminController : Controller`  
  ประกาศคลาส `AdminController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 13:** `{`  
  เปิดบล็อกของคลาส
* **บรรทัดที่ 14:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับเข้าถึงข้อมูลผู้ใช้และ Audit Log
* **บรรทัดที่ 15:** `private readonly IHubContext<PetShopHub> _hubContext;`  
  ฟิลด์เก็บ SignalR Hub Context สำหรับการส่งสัญญาณ Broadcast เตะบัญชีที่ถูกสั่งระงับ
* **บรรทัดที่ 16:** *(บรรทัดว่าง)*
* **บรรทัดที่ 17:** `public AdminController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)`  
  Constructor รับ Repository และ HubContext ผ่าน Dependency Injection
* **บรรทัดที่ 18:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 19:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 20:** `_hubContext = hubContext;`  
  ผูก SignalR Hub เข้ากับตัวแปรภายใน
* **บรรทัดที่ 21:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 22:** *(บรรทัดว่าง)*
* **บรรทัดที่ 23:** `// ตรวจสอบว่าผู้ใช้งานปัจจุบันที่กำลังทำรายการมีสถานะเป็น Owner หรือไม่ (รองรับทั้ง Claims และ Multi-Role Tab Switcher)`  
  คอมเมนต์อธิบายฟังก์ชัน Helper ความปลอดภัย
* **บรรทัดที่ 24:** `private bool IsCurrentOperatorOwner(string? roleParam)`  
  ฟังก์ชัน Helper ตรวจสอบว่าผู้กระทำการเป็น Owner ตัวจริงหรือไม่
* **บรรทัดที่ 25:** `{`  
  เปิดบล็อกฟังก์ชัน Helper
* **บรรทัดที่ 26:** `if (!string.IsNullOrEmpty(roleParam))`  
  หากมีการส่งค่าพารามิเตอร์จำลองบทบาทของแท็บมา
* **บรรทัดที่ 27:** `{`
* **บรรทัดที่ 28:** `var resolved = _repo.ResolveTabUserInfo(roleParam);`  
  ถอดรหัสข้อมูลผู้ใช้ของแท็บนั้นจาก Repository
* **บรรทัดที่ 29:** `return string.Equals(resolved.RoleName, "Owner", System.StringComparison.OrdinalIgnoreCase);`  
  ตรวจสอบว่าชื่อบทบาทของแท็บนั้นคือ Owner หรือไม่
* **บรรทัดที่ 30:** `}`
* **บรรทัดที่ 31:** `return User.IsInRole("Owner");`  
  กรณีไม่มีพารามิเตอร์ ให้ตรวจสอบจาก Claims ในคุกกี้ของผู้ใช้
* **บรรทัดที่ 32:** `}`  
  ปิดบล็อกฟังก์ชัน Helper
* **บรรทัดที่ 33:** *(บรรทัดว่าง)*
* **บรรทัดที่ 34:** `// หน้าจัดการผู้ใช้และสิทธิ์การเข้าถึง (Admin) พร้อมระบบกรองตามบทบาท (Role Filter) & ค้นหา`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 35:** `public IActionResult Index(string? role = null, string? roleFilter = "all", string? search = null)`  
  Action เมธอดแสดงหน้ารายชื่อผู้ใช้และระบบจัดการสิทธิ์
* **บรรทัดที่ 36:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 37:** `var allUsers = _repo.Users;`  
  ดึงรายชื่อผู้ใช้ทั้งหมดจาก Repository
* **บรรทัดที่ 38:** `foreach (var u in allUsers)`  
  วนลูปตรวจสอบผู้ใช้ทุกคนในระบบ
* **บรรทัดที่ 39:** `{`
* **บรรทัดที่ 40:** `_repo.CheckAndRefreshUserSuspension(u);`  
  ตรวจสอบว่าบัญชีที่ถูกระงับชั่วคราวหมดเวลาหรือยัง หากครบกำหนดเวลาแล้วให้ปลดระงับอัตโนมัติ
* **บรรทัดที่ 41:** `}`
* **บรรทัดที่ 42:** `allUsers = _repo.Users;`  
  รีเฟรชรายชื่อผู้ใช้อีกครั้งหลังการปลดระงับ
* **บรรทัดที่ 43:** *(บรรทัดว่าง)*
* **บรรทัดที่ 44:** `bool isOwner = IsCurrentOperatorOwner(role);`  
  ตรวจสอบสิทธิ์ว่าผู้ควบคุมหน้าจอนี้เป็น Owner หรือไม่
* **บรรทัดที่ 45:** `ViewBag.IsCurrentOwner = isOwner;`  
  ส่งค่าความปลอดภัยไปยัง View เพื่อควบคุมการเปิด/ปิดปุ่มแก้ไขบัญชีระดับสูง
* **บรรทัดที่ 46:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 47:** `ViewBag.TotalUsers = allUsers.Count;`  
  นับจำนวนผู้ใช้ทั้งหมดในระบบ
* **บรรทัดที่ 48:** `ViewBag.TotalStaff = allUsers.Count(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);`  
  นับจำนวนพนักงานทุกแผนก
* **บรรทัดที่ 49:** `ViewBag.TotalCustomers = allUsers.Count(u => u.Role == UserRole.Customer);`  
  นับจำนวนลูกค้าสมาชิก
* **บรรทัดที่ 50:** *(บรรทัดว่าง)*
* **บรรทัดที่ 51:** `string filter = (roleFilter ?? "all").Trim().ToLowerInvariant();`  
  จัดรูปแบบตัวกรองบทบาท
* **บรรทัดที่ 52:** `IEnumerable<SystemUser> filtered = allUsers;`  
  กำหนดตัวแปรคัดกรองเริ่มต้น
* **บรรทัดที่ 53:** *(บรรทัดว่าง)*
* **บรรทัดที่ 54:** `if (filter == "staff")`  
  กรองเฉพาะพนักงาน
* **บรรทัดที่ 55:** `{`
* **บรรทัดที่ 56:** `filtered = filtered.Where(u => u.Role != UserRole.Customer && u.Role != UserRole.Guest);`
* **บรรทัดที่ 57:** `}`
* **บรรทัดที่ 58:** `else if (filter == "customer")`  
  กรองเฉพาะลูกค้า
* **บรรทัดที่ 59:** `{`
* **บรรทัดที่ 60:** `filtered = filtered.Where(u => u.Role == UserRole.Customer);`
* **บรรทัดที่ 61:** `}`
* **บรรทัดที่ 62:** `else if (Enum.TryParse<UserRole>(filter, true, out var specificRole))`  
  กรองเฉพาะบทบาทเจาะจง เช่น Marketing, Inventory
* **บรรทัดที่ 63:** `{`
* **บรรทัดที่ 64:** `filtered = filtered.Where(u => u.Role == specificRole);`
* **บรรทัดที่ 65:** `}`
* **บรรทัดที่ 66:** *(บรรทัดว่าง)*
* **บรรทัดที่ 67:** `if (!string.IsNullOrWhiteSpace(search))`  
  หากมีคำค้นหา
* **บรรทัดที่ 68:** `{`
* **บรรทัดที่ 69:** `string kw = search.Trim();`  
  ตัดช่องว่างคำค้น
* **บรรทัดที่ 70:** `filtered = filtered.Where(u =>`  
  คัดกรองให้ตรงกับชื่อผู้ใช้, ชื่อนามสกุล หรืออีเมล
* **บรรทัดที่ 71:** `    u.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 72:** `    u.FullName.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 73:** `    u.Email.Contains(kw, System.StringComparison.OrdinalIgnoreCase));`
* **บรรทัดที่ 74:** `}`
* **บรรทัดที่ 75:** *(บรรทัดว่าง)*
* **บรรทัดที่ 76:** `ViewBag.RoleFilter = filter;`  
  ส่งค่าตัวกรองปัจจุบันไปยัง View
* **บรรทัดที่ 77:** `ViewBag.Search = search;`  
  ส่งคำค้นปัจจุบันไปยัง View
* **บรรทัดที่ 78:** *(บรรทัดว่าง)*
* **บรรทัดที่ 79:** `return View(filtered.ToList());`  
  ส่งรายชื่อผู้ใช้ที่ผ่านการคัดกรองไปแสดงผลใน `Views/Admin/Index.cshtml`
* **บรรทัดที่ 80:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 81:** *(บรรทัดว่าง)*
* **บรรทัดที่ 82:** `// ปรับเปลี่ยนบทบาทและสิทธิ์ของพนักงาน + ระงับบัญชีพร้อมเหตุผลและระยะเวลา (เตะออกทันทีผ่าน SignalR)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 83:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 84:** `public async Task<IActionResult> UpdateUserRole(`  
  ประกาศ Action เมธอดแบบ Asynchronous สำหรับปรับบทบาทและระงับบัญชี
* **บรรทัดที่ 85:** `    int userId,`  
  รหัสผู้ใช้ที่ต้องการแก้ไข
* **บรรทัดที่ 86:** `    UserRole newRole,`  
  บทบาทใหม่ที่ต้องการแต่งตั้ง
* **บรรทัดที่ 87:** `    bool isActive,`  
  สถานะเปิดใช้งาน (`true`) หรือระงับบัญชี (`false`)
* **บรรทัดที่ 88:** `    string? suspendReason = null,`  
  เหตุผลในการระงับบัญชี
* **บรรทัดที่ 89:** `    int suspendDurationMinutes = -1,`  
  ระยะเวลาที่ถูกระงับ (นาที) หรือ -1 คือระงับถาวร
* **บรรทัดที่ 90:** `    string? role = null,`  
  บทบาทของแท็บผู้ทำรายการ
* **บรรทัดที่ 91:** `    string? roleFilter = null,`  
  ตัวกรองเดิมสำหรับส่งคืนกลับ
* **บรรทัดที่ 92:** `    string? search = null)`  
  คำค้นเดิม
* **บรรทัดที่ 93:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 94:** `bool isOwner = IsCurrentOperatorOwner(role);`  
  ตรวจสอบสิทธิ์ว่าผู้ทำรายการเป็น Owner หรือไม่
* **บรรทัดที่ 95:** `var targetUser = _repo.Users.FirstOrDefault(u => u.Id == userId);`  
  ค้นหาผู้ใช้เป้าหมายที่ต้องการแก้ไข
* **บรรทัดที่ 96:** `if (targetUser == null)`  
  หากไม่พบผู้ใช้
* **บรรทัดที่ 97:** `{`
* **บรรทัดที่ 98:** `TempData["ErrorMessage"] = "ไม่พบข้อมูลผู้ใช้งานที่ต้องการแก้ไข";`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 99:** `return RedirectToAction(nameof(Index), new { role, roleFilter, search });`  
  ส่งกลับหน้า Index พร้อมตัวกรองเดิม
* **บรรทัดที่ 100:** `}`
* **บรรทัดที่ 101:** *(บรรทัดว่าง)*
* **บรรทัดที่ 102:** `// กฎความปลอดภัย: Admin ไม่สามารถแก้ไข/ระงับบัญชี Admin หรือ Owner ได้ และไม่สามารถแต่งตั้งใครเป็น Admin หรือ Owner ได้ (เฉพาะ Owner เท่านั้น)`  
  คอมเมนต์อธิบายกฎความปลอดภัย
* **บรรทัดที่ 103:** `if (!isOwner)`  
  หากผู้ทำรายการไม่ใช่ระดับ Owner
* **บรรทัดที่ 104:** `{`
* **บรรทัดที่ 105:** `if (targetUser.Role == UserRole.Admin || targetUser.Role == UserRole.Owner)`  
  บล็อกไม่ให้แก้ไขหรือระงับบัญชี Admin หรือ Owner ด้วยกัน
* **บรรทัดที่ 106:** `{`
* **บรรทัดที่ 107:** `TempData["ErrorMessage"] = "ไม่สามารถแก้ไขหรือระงับบัญชี Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";`  
  แจ้งเตือนข้อห้ามด้านความปลอดภัย
* **บรรทัดที่ 108:** `return RedirectToAction(nameof(Index), new { role, roleFilter, search });`
* **บรรทัดที่ 109:** `}`
* **บรรทัดที่ 110:** `if (newRole == UserRole.Admin || newRole == UserRole.Owner)`  
  บล็อกไม่ให้ Admin แต่งตั้งใครเป็น Admin หรือ Owner
* **บรรทัดที่ 111:** `{`
* **บรรทัดที่ 112:** `TempData["ErrorMessage"] = "ไม่สามารถแต่งตั้งผู้ใช้เป็น Admin หรือ Owner ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";`
* **บรรทัดที่ 113:** `return RedirectToAction(nameof(Index), new { role, roleFilter, search });`
* **บรรทัดที่ 114:** `}`
* **บรรทัดที่ 115:** `}`  
  ปิดบล็อกความปลอดภัย
* **บรรทัดที่ 116:** *(บรรทัดว่าง)*
* **บรรทัดที่ 117:** `if (_repo.UpdateUserRole(userId, newRole, isActive, suspendReason, suspendDurationMinutes))`  
  สั่ง Repository บันทึกการเปลี่ยนบทบาท/ระงับบัญชีลงฐานข้อมูลและ Audit Log
* **บรรทัดที่ 118:** `{`
* **บรรทัดที่ 119:** `if (!isActive)`  
  กรณีที่บัญชีถูกสั่งระงับการใช้งาน (`!isActive`)
* **บรรทัดที่ 120:** `{`
* **บรรทัดที่ 121:** `string reasonText = targetUser.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";`  
  ดึงข้อความเหตุผลการระงับ
* **บรรทัดที่ 122:** `string untilText = targetUser.SuspensionDisplayUntil;`  
  ดึงข้อความระยะเวลาสิ้นสุดการระงับ
* **บรรทัดที่ 123:** *(บรรทัดว่าง)*
* **บรรทัดที่ 124:** `// ส่งสัญญาณ Real-Time เตะผู้ใช้ออกจากระบบทุกแท็บที่กำลังล็อกอินบัญชีนี้อยู่ทันที`  
  คอมเมนต์การเตะผ่าน SignalR
* **บรรทัดที่ 125:** `await _hubContext.Clients.All.SendAsync(`  
  ส่งสัญญาณ WebSocket ไปยังไคลเอนต์ทั้งหมดทันที
* **บรรทัดที่ 126:** `    "ReceiveAccountSuspended",`  
  ชื่อ Event ของ SignalR
* **บรรทัดที่ 127:** `    targetUser.Username,`  
  ส่งชื่อบัญชีที่ถูกระงับ
* **บรรทัดที่ 128:** `    targetUser.Role.ToString(),`  
  ส่งบทบาท
* **บรรทัดที่ 129:** `    reasonText,`  
  ส่งเหตุผล
* **บรรทัดที่ 130:** `    untilText);`  
  ส่งระยะเวลาสิ้นสุด เพื่อให้เบราว์เซอร์เป้าหมายเด้งออกจากระบบทันที
* **บรรทัดที่ 131:** *(บรรทัดว่าง)*
* **บรรทัดที่ 132:** `TempData["SuccessMessage"] = $"ระงับบัญชี '{targetUser.Username}' เรียบร้อยแล้ว ({untilText} | เหตุผล: {reasonText})";`  
  แสดงข้อความแจ้งเตือนผลการระงับ
* **บรรทัดที่ 133:** `}`
* **บรรทัดที่ 134:** `else`  
  กรณีเปิดใช้งานหรือเปลี่ยนบทบาทปกติ
* **บรรทัดที่ 135:** `{`
* **บรรทัดที่ 136:** `TempData["SuccessMessage"] = $"อัปเดตบัญชีผู้ใช้งาน #{userId} ({targetUser.Username}) เป็น '{newRole}' (สถานะ: เปิดใช้งานปกติ) สำเร็จ";`  
  แสดงข้อความผลการอัปเดต
* **บรรทัดที่ 137:** `}`
* **บรรทัดที่ 138:** `}`
* **บรรทัดที่ 139:** `return RedirectToAction(nameof(Index), new { role, roleFilter, search });`  
  ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
* **บรรทัดที่ 140:** `}`  
  ปิดบล็อกเมธอด UpdateUserRole
* **บรรทัดที่ 141:** *(บรรทัดว่าง)*
* **บรรทัดที่ 142:** `// เพิ่มผู้ใช้งานใหม่ (บันทึกลง DB) - Admin ไม่สามารถสร้าง User บทบาท Admin ได้ ต้องให้ Owner ทำเท่านั้น`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 143:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 144:** `public IActionResult CreateUser(SystemUser user, string? newUserRole = null, string? role = null)`  
  Action เมธอดสร้างบัญชีพนักงานใหม่
* **บรรทัดที่ 145:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 146:** `// ใช้ชื่อฟิลด์ newUserRole แยกจาก role (บทบาทของแท็บผู้ทำรายการ) เพื่อไม่ให้ Model Binding ชนกัน`  
  คอมเมนต์ Model Binding
* **บรรทัดที่ 147:** `if (string.IsNullOrWhiteSpace(newUserRole) || !Enum.TryParse<UserRole>(newUserRole, true, out var requestedRole))`  
  ตรวจสอบความถูกต้องของบทบาทใหม่ที่เลือก
* **บรรทัดที่ 148:** `{`
* **บรรทัดที่ 149:** `TempData["ErrorMessage"] = "กรุณาเลือกบทบาทหน้าที่ (Role) ให้ถูกต้อง";`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 150:** `return RedirectToAction(nameof(Index), new { role });`
* **บรรทัดที่ 151:** `}`
* **บรรทัดที่ 152:** `user.Role = requestedRole;`  
  ผูกบทบาทเข้ากับ Entity ผู้ใช้ใหม่
* **บรรทัดที่ 153:** *(บรรทัดว่าง)*
* **บรรทัดที่ 154:** `bool isOwner = IsCurrentOperatorOwner(role);`  
  ตรวจสอบสิทธิ์ว่าผู้สร้างเป็น Owner หรือไม่
* **บรรทัดที่ 155:** `if ((requestedRole == UserRole.Admin || requestedRole == UserRole.Owner) && !isOwner)`  
  บล็อกไม่ให้ Admin สร้างบัญชีระดับ Admin หรือ Owner
* **บรรทัดที่ 156:** `{`
* **บรรทัดที่ 157:** `TempData["ErrorMessage"] = "ไม่สามารถสร้างบัญชีบทบาท Admin ได้: สิทธิ์นี้สงวนไว้สำหรับผู้บริหารสูงสุด (Owner) เท่านั้น";`
* **บรรทัดที่ 158:** `return RedirectToAction(nameof(Index), new { role });`
* **บรรทัดที่ 159:** `}`
* **บรรทัดที่ 160:** *(บรรทัดว่าง)*
* **บรรทัดที่ 161:** `var (success, message) = _repo.CreateUser(user);`  
  สั่ง Repository บันทึกผู้ใช้ใหม่ลงฐานข้อมูล
* **บรรทัดที่ 162:** `if (success)`
* **บรรทัดที่ 163:** `    TempData["SuccessMessage"] = message;`  
  แจ้งผลสำเร็จ
* **บรรทัดที่ 164:** `else`
* **บรรทัดที่ 165:** `    TempData["ErrorMessage"] = message;`  
  แจ้งข้อผิดพลาดกรณีชื่อผู้ใช้หรืออีเมลซ้ำ
* **บรรทัดที่ 166:** *(บรรทัดว่าง)*
* **บรรทัดที่ 167:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 168:** `}`  
  ปิดบล็อกเมธอด CreateUser
* **บรรทัดที่ 169:** *(บรรทัดว่าง)*
* **บรรทัดที่ 170:** `// หน้าตรวจสอบบันทึกกิจกรรมย้อนหลัง (Audit Log)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 171:** `public IActionResult AuditLogs(string? search = null, string? roleFilter = null, string? actionFilter = null, string? role = null)`  
  Action เมธอดแสดงหน้า Audit Trail บันทึกกิจกรรมทั้งหมดในระบบ
* **บรรทัดที่ 172:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 173:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 174:** `var logs = _repo.GetAuditLogs(500);`  
  ดึงประวัติ Audit Log ล่าสุด 500 รายการ
* **บรรทัดที่ 175:** *(บรรทัดว่าง)*
* **บรรทัดที่ 176:** `ViewBag.TotalLogsCount = logs.Count;`  
  จำนวน Log ทั้งหมด
* **บรรทัดที่ 177:** `ViewBag.SecurityLogsCount = logs.Count(l => l.Action == "UPDATE_USER_ROLE" || l.Action == "SUSPEND_ACCOUNT" || l.Action == "CREATE_USER");`  
  นับจำนวน Log ด้านความปลอดภัย (เปลี่ยนสิทธิ์, ระงับบัญชี, สร้างผู้ใช้)
* **บรรทัดที่ 178:** `ViewBag.FinanceLogsCount = logs.Count(l => l.Action.Contains("PAYMENT") || l.Action.Contains("REFUND") || l.Action.Contains("EXPENSE"));`  
  นับจำนวน Log ด้านการเงิน
* **บรรทัดที่ 179:** `ViewBag.InventoryLogsCount = logs.Count(l => l.Action.Contains("PRODUCT") || l.Action.Contains("STOCK"));`  
  นับจำนวน Log ด้านคลังสินค้า
* **บรรทัดที่ 180:** *(บรรทัดว่าง)*
* **บรรทัดที่ 181:** `if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "all")`  
  กรอง Log ตามบทบาท
* **บรรทัดที่ 182:** `{`
* **บรรทัดที่ 183:** `logs = logs.Where(l => string.Equals(l.UserRole, roleFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();`
* **บรรทัดที่ 184:** `}`
* **บรรทัดที่ 185:** *(บรรทัดว่าง)*
* **บรรทัดที่ 186:** `if (!string.IsNullOrWhiteSpace(actionFilter) && actionFilter != "all")`  
  กรอง Log ตามประเภท Action
* **บรรทัดที่ 187:** `{`
* **บรรทัดที่ 188:** `logs = logs.Where(l => string.Equals(l.Action, actionFilter, System.StringComparison.OrdinalIgnoreCase)).ToList();`
* **บรรทัดที่ 189:** `}`
* **บรรทัดที่ 190:** *(บรรทัดว่าง)*
* **บรรทัดที่ 191:** `if (!string.IsNullOrWhiteSpace(search))`  
  ค้นหาคำสำคัญใน Log
* **บรรทัดที่ 192:** `{`
* **บรรทัดที่ 193:** `string kw = search.Trim();`
* **บรรทัดที่ 194:** `logs = logs.Where(l =>`  
  ค้นหาจากชื่อผู้ใช้, แอ็กชัน, ชื่อตาราง, รหัสเรคคอร์ด, รายละเอียด หรือไอพีแอดเดรส
* **บรรทัดที่ 195:** `    (!string.IsNullOrEmpty(l.Username) && l.Username.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||`
* **บรรทัดที่ 196:** `    (!string.IsNullOrEmpty(l.Action) && l.Action.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||`
* **บรรทัดที่ 197:** `    (!string.IsNullOrEmpty(l.TableName) && l.TableName.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||`
* **บรรทัดที่ 198:** `    (!string.IsNullOrEmpty(l.RecordId) && l.RecordId.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||`
* **บรรทัดที่ 199:** `    (!string.IsNullOrEmpty(l.Details) && l.Details.Contains(kw, System.StringComparison.OrdinalIgnoreCase)) ||`
* **บรรทัดที่ 200:** `    (!string.IsNullOrEmpty(l.IpAddress) && l.IpAddress.Contains(kw, System.StringComparison.OrdinalIgnoreCase))`
* **บรรทัดที่ 201:** `).ToList();`
* **บรรทัดที่ 202:** `}`
* **บรรทัดที่ 203:** *(บรรทัดว่าง)*
* **บรรทัดที่ 204:** `ViewBag.Search = search;`  
  ส่งคำค้นไปยัง View
* **บรรทัดที่ 205:** `ViewBag.RoleFilter = roleFilter ?? "all";`  
  ส่งตัวกรองบทบาทไปยัง View
* **บรรทัดที่ 206:** `ViewBag.ActionFilter = actionFilter ?? "all";`  
  ส่งตัวกรองแอ็กชันไปยัง View
* **บรรทัดที่ 207:** `ViewBag.FilteredCount = logs.Count;`  
  จำนวน Log ที่คัดกรองได้
* **บรรทัดที่ 208:** *(บรรทัดว่าง)*
* **บรรทัดที่ 209:** `return View(logs);`  
  ส่งรายการ Log ไปเรนเดอร์ใน `Views/Admin/AuditLogs.cshtml`
* **บรรทัดที่ 210:** `}`  
  ปิดบล็อกเมธอด AuditLogs
* **บรรทัดที่ 211:** `}`  
  ปิดบล็อกคลาส AdminController
* **บรรทัดที่ 212:** `}`  
  ปิดบล็อกเนมสเปซ


---

# 4. AuthController.cs (338 บรรทัด)

### โค้ดต้นฉบับเต็มของ AuthController.cs
```csharp
1: using Microsoft.AspNetCore.Authentication;
2: using Microsoft.AspNetCore.Authentication.Cookies;
3: using Microsoft.AspNetCore.Mvc;
4: using CSI402_Project_final.Models;
5: using System.Security.Claims;
6: using System.Collections.Generic;
7: using System.Linq;
8: using System.Threading.Tasks;
9: 
10: namespace CSI402_Project_final.Controllers
11: {
12:     public class AuthController : Controller
13:     {
14:         private readonly PetShopRepository _repo;
15: 
16:         public AuthController(PetShopRepository repo)
17:         {
18:             _repo = repo;
19:         }
20: 
21:         // หน้า Login (GET) - แสดงหน้าเข้าสู่ระบบเสมอเพื่อให้แต่ละแท็บล็อกอินแยกบทบาทได้อย่างอิสระ
22:         [HttpGet]
23:         public IActionResult Login(
24:             string? returnUrl = null,
25:             bool suspended = false,
26:             string? u = null,
27:             string? reason = null,
28:             string? until = null)
29:         {
30:             if (suspended || !string.IsNullOrEmpty(u))
31:             {
32:                 var dbUser = !string.IsNullOrEmpty(u) ? _repo.FindUserByUsernameOrEmail(u) : null;
33:                 if (dbUser != null && !dbUser.IsActive)
34:                 {
35:                     ViewBag.SuspendedUser = dbUser.Username;
36:                     ViewBag.SuspendedReason = dbUser.SuspensionReason ?? reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
37:                     ViewBag.SuspendedUntil = dbUser.SuspensionDisplayUntil;
38:                 }
39:                 else if (suspended)
40:                 {
41:                     ViewBag.SuspendedUser = u ?? "บัญชีของคุณ";
42:                     ViewBag.SuspendedReason = reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
43:                     ViewBag.SuspendedUntil = until ?? "ระงับถาวร";
44:                 }
45:             }
46: 
47:             ViewBag.ReturnUrl = returnUrl;
48:             ViewBag.AllDemoUsers = GetDemoRoleUsers();
49:             return View(new LoginViewModel { ReturnUrl = returnUrl });
50:         }
51: 
52:         private List<SystemUser> GetDemoRoleUsers()
53:         {
54:             var systemUsernames = new[]
55:             {
56:                 "owner", "admin", "marketing", "inventory", "order",
57:                 "shipping", "accounting", "support", "customer"
58:             };
59:             var allUsers = _repo.Users;
60:             foreach (var u in allUsers)
61:             {
62:                 _repo.CheckAndRefreshUserSuspension(u);
63:             }
64:             allUsers = _repo.Users;
65:             var prioritized = allUsers
66:                 .Where(u => systemUsernames.Contains(u.Username.ToLowerInvariant()))
67:                 .OrderByDescending(u => u.Role)
68:                 .ToList();
69:             return prioritized.Any() ? prioritized : allUsers.Take(10).ToList();
70:         }
71: 
72:         // การประมวลผล Login (POST)
73:         [HttpPost]
74:         [IgnoreAntiforgeryToken]
75:         public async Task<IActionResult> Login(LoginViewModel model)
76:         {
77:             ViewBag.AllDemoUsers = GetDemoRoleUsers();
78: 
79:             if (!ModelState.IsValid)
80:             {
81:                 return View(model);
82:             }
83: 
84:             var candidate = _repo.FindUserByUsernameOrEmail(model.Username);
85:             if (candidate != null && candidate.Password == model.Password && !candidate.IsActive)
86:             {
87:                 ViewBag.SuspendedUser = candidate.Username;
88:                 ViewBag.SuspendedReason = candidate.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";
89:                 ViewBag.SuspendedUntil = candidate.SuspensionDisplayUntil;
90:                 ModelState.AddModelError(string.Empty, $"บัญชี '{candidate.Username}' ถูกระงับการใช้งาน ({candidate.SuspensionDisplayUntil}) เนื่องจาก: {ViewBag.SuspendedReason}");
91:                 return View(model);
92:             }
93: 
94:             var user = _repo.ValidateUser(model.Username, model.Password);
95:             if (user == null)
96:             {
97:                 ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
98:                 return View(model);
99:             }
100: 
101:             await SignInUserAsync(user, model.RememberMe);
102:             _repo.MergeGuestCartToUser(user.Id);
103:             _repo.LogAudit("LOGIN", "user", user.Id.ToString(), $"เข้าสู่ระบบด้วย Username '{user.Username}'", user);
104: 
105:             TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
106:             TempData["SuccessTargetRole"] = user.Role.ToString();
107:             TempData["SuccessTargetUser"] = user.Username;
108: 
109:             return RedirectToRoleHome(user.Role.ToString(), user.Username);
110:         }
111: 
112:         // Quick Demo Login สำหรับทดสอบคลิกเดียว (ตรวจสอบสถานะการระงับบัญชีก่อนอนุญาตให้เข้าสู่ระบบ)
113:         [HttpGet]
114:         [HttpPost]
115:         [IgnoreAntiforgeryToken]
116:         public async Task<IActionResult> QuickLogin(string? username = null, string? returnUrl = null)
117:         {
118:             if (string.IsNullOrWhiteSpace(username)) return RedirectToAction(nameof(Login));
119: 
120:             var user = _repo.FindUserByUsernameOrEmail(username);
121:             if (user == null)
122:             {
123:                 var info = DemoUserHelper.GetDemoUserInfo(username);
124:                 user = _repo.FindUserByUsernameOrEmail(info.Username);
125:             }
126: 
127:             if (user == null) return RedirectToAction(nameof(Login));
128: 
129:             // หากบัญชีถูกระงับอยู่ ห้ามเข้าสู่ระบบเด็ดขาด และแสดงเหตุผลพร้อมระยะเวลาที่ถูกระงับ
130:             if (!user.IsActive)
131:             {
132:                 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
133:                 return RedirectToAction(nameof(Login), new
134:                 {
135:                     suspended = true,
136:                     u = user.Username,
137:                     reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
138:                     until = user.SuspensionDisplayUntil
139:                 });
140:             }
141: 
142:             await SignInUserAsync(user, isPersistent: true);
143:             _repo.MergeGuestCartToUser(user.Id);
144:             _repo.LogAudit("QUICK_LOGIN", "user", user.Id.ToString(), $"Quick Login '{user.Username}'", user);
145: 
146:             TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
147:             TempData["SuccessTargetRole"] = user.Role.ToString();
148:             TempData["SuccessTargetUser"] = user.Username;
149: 
150:             return RedirectToRoleHome(user.Role.ToString(), user.Username);
151:         }
152: 
153:         // Multi-Role Switcher: สลับบทบาทผู้ใช้แบบเรียลไทม์ (หากบัญชีของบทบาทนั้นถูกระงับ จะไม่อนุญาตให้เข้าใช้งาน)
154:         [HttpGet]
155:         public async Task<IActionResult> SwitchRole(string role, string? returnUrl = null)
156:         {
157:             var info = DemoUserHelper.GetDemoUserInfo(role);
158:             var user = !string.IsNullOrEmpty(info.Username)
159:                 ? _repo.FindUserByUsernameOrEmail(info.Username)
160:                 : null;
161: 
162:             if (user != null && !user.IsActive)
163:             {
164:                 await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
165:                 return RedirectToAction(nameof(Login), new
166:                 {
167:                     suspended = true,
168:                     u = user.Username,
169:                     reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",
170:                     until = user.SuspensionDisplayUntil
171:                 });
172:             }
173: 
174:             if (user != null)
175:             {
176:                 await SignInUserAsync(user, isPersistent: true);
177:                 _repo.MergeGuestCartToUser(user.Id);
178:                 _repo.LogAudit("ROLE_SWITCH", "user", user.Id.ToString(), $"สลับบทบาทเป็น '{user.Username}' ({info.RoleName})", user);
179:                 TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);
180:                 TempData["SuccessTargetRole"] = info.RoleName;
181:                 TempData["SuccessTargetUser"] = user.Username;
182:             }
183: 
184:             if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
185:             {
186:                 return LocalRedirect(returnUrl);
187:             }
188: 
189:             return RedirectToRoleHome(info.RoleName);
190:         }
191: 
192:         // หน้า Register (GET) - แสดงหน้าสมัครสมาชิกเสมอโดยไม่เด้งกลับหน้าหลักแม้แท็บอื่นจะล็อกอินอยู่
193:         [HttpGet]
194:         public IActionResult Register()
195:         {
196:             return View(new RegisterViewModel());
197:         }
198: 
199:         // การประมวลผล Register (POST)
200:         [HttpPost]
201:         [IgnoreAntiforgeryToken]
202:         public async Task<IActionResult> Register(RegisterViewModel model)
203:         {
204:             if (!ModelState.IsValid)
205:             {
206:                 return View(model);
207:             }
208: 
209:             var newUser = _repo.RegisterCustomer(model);
210:             if (newUser == null)
211:             {
212:                 ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรืออีเมลนี้มีอยู่ในระบบแล้ว กรุณาใช้อันอื่น");
213:                 return View(model);
214:             }
215: 
216:             await SignInUserAsync(newUser, isPersistent: true);
217:             _repo.MergeGuestCartToUser(newUser.Id);
218:             TempData["SuccessMessage"] = $"สมัครสมาชิกสำเร็จ! ยินดีต้อนรับคุณ {newUser.FullName}";
219:             TempData["SuccessTargetRole"] = "Customer";
220:             TempData["SuccessTargetUser"] = newUser.Username;
221: 
222:             return RedirectToAction("Index", "Home", new { role = newUser.Username });
223:         }
224: 
225:         // Logout
226:         [HttpPost]
227:         [HttpGet]
228:         [IgnoreAntiforgeryToken]
229:         public async Task<IActionResult> Logout()
230:         {
231:             if (User.Identity?.IsAuthenticated == true)
232:             {
233:                 _repo.LogAudit("LOGOUT", "user", User.FindFirstValue(ClaimTypes.NameIdentifier), "ออกจากระบบ");
234:             }
235:             await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
236:             TempData["SuccessMessage"] = "คุณได้ออกจากระบบเรียบร้อยแล้ว";
237:             TempData["SuccessTargetRole"] = "Guest";
238:             TempData["SuccessTargetUser"] = "";
239:             return RedirectToAction("Index", "Home", new { role = "Guest" });
240:         }
241: 
242:         // หน้า Access Denied เมื่อไม่มีสิทธิ์
243:         [HttpGet]
244:         public IActionResult AccessDenied()
245:         {
246:             return View();
247:         }
248: 
249:         private static string CleanName(string? fullName)
250:         {
251:             if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
252:             return System.Text.RegularExpressions.Regex.Replace(fullName, @"\s*\([^)]*\)", "").Trim();
253:         }
254: 
255:         private static string FormatWelcomeMessage(string? fullName)
256:         {
257:             var clean = CleanName(fullName);
258:             if (string.IsNullOrWhiteSpace(clean))
259:             {
260:                 return "ยินดีต้อนรับเข้าสู่ระบบ";
261:             }
262: 
263:             if (clean.StartsWith("คุณ "))
264:             {
265:                 clean = clean.Substring(4).Trim();
266:             }
267:             else if (clean.StartsWith("คุณ"))
268:             {
269:                 clean = clean.Substring(3).Trim();
270:             }
271: 
272:             return string.IsNullOrWhiteSpace(clean) ? "ยินดีต้อนรับเข้าสู่ระบบ" : $"ยินดีต้อนรับคุณ {clean}";
273:         }
274: 
275:         private async Task SignInUserAsync(SystemUser user, bool isPersistent)
276:         {
277:             var cleanFullName = CleanName(user.FullName);
278:             if (string.IsNullOrWhiteSpace(cleanFullName)) cleanFullName = user.Username;
279: 
280:             var claims = new List<Claim>
281:             {
282:                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
283:                 new Claim(ClaimTypes.Name, user.Username),
284:                 new Claim(ClaimTypes.Role, user.Role.ToString()),
285:                 new Claim("FullName", cleanFullName),
286:                 new Claim(ClaimTypes.Email, user.Email),
287:                 new Claim("Phone", user.Phone)
288:             };
289: 
290:             // Multi-Role Support: สำหรับ Owner และ Admin ให้สิทธิ์เข้าถึงทุก Role พร้อมกันได้ เพื่อให้เปิดหลายแท็บ/หลายหน้าต่างทดสอบได้ทันที
291:             if (user.Role == UserRole.Owner || user.Role == UserRole.Admin)
292:             {
293:                 foreach (UserRole r in Enum.GetValues(typeof(UserRole)))
294:                 {
295:                     if (r != user.Role)
296:                     {
297:                         claims.Add(new Claim(ClaimTypes.Role, r.ToString()));
298:                     }
299:                 }
300:             }
301: 
302:             var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
303:             var authProperties = new AuthenticationProperties
304:             {
305:                 IsPersistent = isPersistent,
306:                 ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
307:             };
308: 
309:             await HttpContext.SignInAsync(
310:                 CookieAuthenticationDefaults.AuthenticationScheme,
311:                 new ClaimsPrincipal(claimsIdentity),
312:                 authProperties);
313:         }
314: 
315:         // Helper: นำทางไปยังหน้าหลักของแต่ละ Role (รองรับแยก Username ของแต่ละแท็บอิสระ)
316:         private IActionResult RedirectToRoleHome(string? role, string? username = null)
317:         {
318:             var canonical = DemoUserHelper.GetDemoUserInfo(role);
319:             string tabRoleKey = (!string.IsNullOrWhiteSpace(username) &&
320:                                  !string.Equals(username, canonical.Username, System.StringComparison.OrdinalIgnoreCase))
321:                 ? username
322:                 : (role ?? "Customer");
323: 
324:             return role switch
325:             {
326:                 nameof(UserRole.InventoryStaff) => RedirectToAction("Index", "Inventory", new { role = tabRoleKey }),
327:                 nameof(UserRole.OrderStaff) => RedirectToAction("Index", "Order", new { role = tabRoleKey }),
328:                 nameof(UserRole.ShippingStaff) => RedirectToAction("Index", "Shipping", new { role = tabRoleKey }),
329:                 nameof(UserRole.MarketingStaff) => RedirectToAction("Index", "Marketing", new { role = tabRoleKey }),
330:                 nameof(UserRole.AccountingStaff) => RedirectToAction("Index", "Accounting", new { role = tabRoleKey }),
331:                 nameof(UserRole.ITSupport) => RedirectToAction("Index", "Support", new { role = tabRoleKey }),
332:                 nameof(UserRole.Admin) => RedirectToAction("Index", "Admin", new { role = tabRoleKey }),
333:                 nameof(UserRole.Owner) => RedirectToAction("Index", "Owner", new { role = tabRoleKey }),
334:                 _ => RedirectToAction("Index", "Home", new { role = tabRoleKey })
335:             };
336:         }
337:     }
338: }
```

### คำอธิบายการทำงานของ AuthController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authentication;`  
  นำเข้าอินเทอร์เฟซและฟังก์ชันหลักของการพิสูจน์ตัวตน เช่น `SignInAsync`, `SignOutAsync`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Authentication.Cookies;`  
  นำเข้า CookieAuthenticationDefaults สำหรับระบุ Schema การทำงานของระบบคุกกี้เซสชัน
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาส Controller, IActionResult, HttpGet, HttpPost, RedirectToAction
* **บรรทัดที่ 4:** `using CSI402_Project_final.Models;`  
  นำเข้า Model การล็อกอิน การสมัครสมาชิก ข้อมูลผู้ใช้ และ PetShopRepository
* **บรรทัดที่ 5:** `using System.Security.Claims;`  
  นำเข้าคลาส Claim, ClaimsIdentity, ClaimsPrincipal สำหรับสร้างบัตรประจำตัวผู้ใช้
* **บรรทัดที่ 6:** `using System.Collections.Generic;`  
  นำเข้า List และ Collection
* **บรรทัดที่ 7:** `using System.Linq;`  
  นำเข้าฟังก์ชัน LINQ สำหรับเรียงลำดับและค้นหาผู้ใช้
* **บรรทัดที่ 8:** `using System.Threading.Tasks;`  
  นำเข้า Task สำหรับการทำงานแบบ Asynchronous
* **บรรทัดที่ 9:** *(บรรทัดว่าง)*
* **บรรทัดที่ 10:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 11:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 12:** `public class AuthController : Controller`  
  ประกาศคลาส `AuthController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 13:** `{`  
  เปิดบล็อกของคลาส
* **บรรทัดที่ 14:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับตรวจสอบผู้ใช้และบันทึกประวัติการล็อกอิน
* **บรรทัดที่ 15:** *(บรรทัดว่าง)*
* **บรรทัดที่ 16:** `public AuthController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 17:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 18:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 19:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 20:** *(บรรทัดว่าง)*
* **บรรทัดที่ 21:** `// หน้า Login (GET) - แสดงหน้าเข้าสู่ระบบเสมอเพื่อให้แต่ละแท็บล็อกอินแยกบทบาทได้อย่างอิสระ`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 22:** `[HttpGet]`  
  ระบุว่ารับคำขอด้วย HTTP GET
* **บรรทัดที่ 23:** `public IActionResult Login(`  
  ประกาศ Action เมธอด Login สำหรับแสดงหน้าจอเข้าสู่ระบบ
* **บรรทัดที่ 24:** `    string? returnUrl = null,`  
  URL ที่ต้องการส่งกลับหลังจากล็อกอินเสร็จ
* **บรรทัดที่ 25:** `    bool suspended = false,`  
  ตัวบ่งชี้ว่าถูกส่งมาเพราะบัญชีถูกระงับหรือไม่
* **บรรทัดที่ 26:** `    string? u = null,`  
  ชื่อผู้ใช้ที่ถูกระงับ
* **บรรทัดที่ 27:** `    string? reason = null,`  
  เหตุผลในการระงับ
* **บรรทัดที่ 28:** `    string? until = null)`  
  ระยะเวลาที่ถูกระงับ
* **บรรทัดที่ 29:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 30:** `if (suspended || !string.IsNullOrEmpty(u))`  
  ตรวจสอบว่าบัญชีนี้ติดสถานะถูกระงับหรือไม่
* **บรรทัดที่ 31:** `{`
* **บรรทัดที่ 32:** `var dbUser = !string.IsNullOrEmpty(u) ? _repo.FindUserByUsernameOrEmail(u) : null;`  
  ค้นหาข้อมูลผู้ใช้ในฐานข้อมูล
* **บรรทัดที่ 33:** `if (dbUser != null && !dbUser.IsActive)`  
  หากพบผู้ใช้และสถานะ `IsActive == false`
* **บรรทัดที่ 34:** `{`
* **บรรทัดที่ 35:** `ViewBag.SuspendedUser = dbUser.Username;`  
  ส่งชื่อผู้ใช้ที่ถูกระงับไปยัง View
* **บรรทัดที่ 36:** `ViewBag.SuspendedReason = dbUser.SuspensionReason ?? reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";`  
  ส่งเหตุผลการระงับ
* **บรรทัดที่ 37:** `ViewBag.SuspendedUntil = dbUser.SuspensionDisplayUntil;`  
  ส่งเวลาสิ้นสุดการระงับ
* **บรรทัดที่ 38:** `}`
* **บรรทัดที่ 39:** `else if (suspended)`  
  กรณีไม่มีข้อมูลใน DB แต่ส่ง Flag ระงับมา
* **บรรทัดที่ 40:** `{`
* **บรรทัดที่ 41:** `ViewBag.SuspendedUser = u ?? "บัญชีของคุณ";`
* **บรรทัดที่ 42:** `ViewBag.SuspendedReason = reason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";`
* **บรรทัดที่ 43:** `ViewBag.SuspendedUntil = until ?? "ระงับถาวร";`
* **บรรทัดที่ 44:** `}`
* **บรรทัดที่ 45:** `}`
* **บรรทัดที่ 46:** *(บรรทัดว่าง)*
* **บรรทัดที่ 47:** `ViewBag.ReturnUrl = returnUrl;`  
  ส่ง returnUrl ไปยัง View
* **บรรทัดที่ 48:** `ViewBag.AllDemoUsers = GetDemoRoleUsers();`  
  ดึงรายชื่อผู้ใช้เดโมทุกบทบาทมาแสดงบนปุ่ม Quick Login ในหน้าเว็บ
* **บรรทัดที่ 49:** `return View(new LoginViewModel { ReturnUrl = returnUrl });`  
  ส่งโมเดลไปยัง `Views/Auth/Login.cshtml`
* **บรรทัดที่ 50:** `}`  
  ปิดบล็อกเมธอด Login [GET]
* **บรรทัดที่ 51:** *(บรรทัดว่าง)*
* **บรรทัดที่ 52:** `private List<SystemUser> GetDemoRoleUsers()`  
  ฟังก์ชัน Helper ภายในสำหรับจัดเตรียมรายชื่อบัญชีทดสอบ
* **บรรทัดที่ 53:** `{`  
  เปิดบล็อก Helper
* **บรรทัดที่ 54:** `var systemUsernames = new[]`  
  กำหนดชื่อบัญชีระบบมาตรฐานที่ต้องการนำมาแสดง
* **บรรทัดที่ 55:** `{`
* **บรรทัดที่ 56:** `    "owner", "admin", "marketing", "inventory", "order",`
* **บรรทัดที่ 57:** `    "shipping", "accounting", "support", "customer"`
* **บรรทัดที่ 58:** `};`
* **บรรทัดที่ 59:** `var allUsers = _repo.Users;`  
  ดึงผู้ใช้ทั้งหมด
* **บรรทัดที่ 60:** `foreach (var u in allUsers)`  
  วนลูปตรวจสอบการระงับบัญชี
* **บรรทัดที่ 61:** `{`
* **บรรทัดที่ 62:** `_repo.CheckAndRefreshUserSuspension(u);`  
  ปลดระงับอัตโนมัติหากครบกำหนดเวลา
* **บรรทัดที่ 63:** `}`
* **บรรทัดที่ 64:** `allUsers = _repo.Users;`  
  รีเฟรชข้อมูลผู้ใช้
* **บรรทัดที่ 65:** `var prioritized = allUsers`  
  กรองและจัดเรียงผู้ใช้ระบบ
* **บรรทัดที่ 66:** `    .Where(u => systemUsernames.Contains(u.Username.ToLowerInvariant()))`
* **บรรทัดที่ 67:** `    .OrderByDescending(u => u.Role)`  
  จัดเรียงตามระดับสิทธิ์ Role
* **บรรทัดที่ 68:** `    .ToList();`
* **บรรทัดที่ 69:** `return prioritized.Any() ? prioritized : allUsers.Take(10).ToList();`  
  ส่งคืนรายการผู้ใช้เดโม
* **บรรทัดที่ 70:** `}`  
  ปิดบล็อก Helper GetDemoRoleUsers
* **บรรทัดที่ 71:** *(บรรทัดว่าง)*
* **บรรทัดที่ 72:** `// การประมวลผล Login (POST)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 73:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 74:** `[IgnoreAntiforgeryToken]`  
  ยกเว้นการตรวจสอบ Antiforgery Token เพื่อความคล่องตัวในการทดสอบสลับแท็บ
* **บรรทัดที่ 75:** `public async Task<IActionResult> Login(LoginViewModel model)`  
  Action เมธอดรับข้อมูลล็อกอิน (Username, Password, RememberMe)
* **บรรทัดที่ 76:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 77:** `ViewBag.AllDemoUsers = GetDemoRoleUsers();`  
  เตรียมปุ่มเดโมส่งกลับไปกรณีล็อกอินไม่ผ่าน
* **บรรทัดที่ 78:** *(บรรทัดว่าง)*
* **บรรทัดที่ 79:** `if (!ModelState.IsValid)`  
  ตรวจสอบความสมบูรณ์ของฟอร์ม (Validation)
* **บรรทัดที่ 80:** `{`
* **บรรทัดที่ 81:** `return View(model);`  
  หากกรอกไม่ครบ ส่งกลับไปหน้าเดิมพร้อมแสดงข้อความเตือน
* **บรรทัดที่ 82:** `}`
* **บรรทัดที่ 83:** *(บรรทัดว่าง)*
* **บรรทัดที่ 84:** `var candidate = _repo.FindUserByUsernameOrEmail(model.Username);`  
  ค้นหาผู้ใช้จาก Username หรือ Email
* **บรรทัดที่ 85:** `if (candidate != null && candidate.Password == model.Password && !candidate.IsActive)`  
  ตรวจสอบว่ารหัสผ่านถูกแต่บัญชีถูกสั่งระงับหรือไม่
* **บรรทัดที่ 86:** `{`  
  เปิดบล็อกกรณีบัญชีถูกระงับ
* **บรรทัดที่ 87:** `ViewBag.SuspendedUser = candidate.Username;`  
  ส่งชื่อผู้ใช้
* **บรรทัดที่ 88:** `ViewBag.SuspendedReason = candidate.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ";`  
  ส่งเหตุผลการระงับ
* **บรรทัดที่ 89:** `ViewBag.SuspendedUntil = candidate.SuspensionDisplayUntil;`  
  ส่งระยะเวลาที่ถูกระงับ
* **บรรทัดที่ 90:** `ModelState.AddModelError(string.Empty, $"บัญชี '{candidate.Username}' ถูกระงับการใช้งาน ({candidate.SuspensionDisplayUntil}) เนื่องจาก: {ViewBag.SuspendedReason}");`  
  แสดงข้อความปฏิเสธการเข้าสู่ระบบ
* **บรรทัดที่ 91:** `return View(model);`  
  ส่งกลับหน้า Login
* **บรรทัดที่ 92:** `}`  
  ปิดบล็อกบัญชีถูกระงับ
* **บรรทัดที่ 93:** *(บรรทัดว่าง)*
* **บรรทัดที่ 94:** `var user = _repo.ValidateUser(model.Username, model.Password);`  
  ตรวจสอบความถูกต้องของชื่อผู้ใช้และรหัสผ่าน
* **บรรทัดที่ 95:** `if (user == null)`  
  หากตรวจสอบไม่ผ่าน
* **บรรทัดที่ 96:** `{`
* **บรรทัดที่ 97:** `ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");`  
  แจ้งข้อความรหัสผ่านผิด
* **บรรทัดที่ 98:** `return View(model);`
* **บรรทัดที่ 99:** `}`
* **บรรทัดที่ 100:** *(บรรทัดว่าง)*
* **บรรทัดที่ 101:** `await SignInUserAsync(user, model.RememberMe);`  
  ออกคุกกี้เซสชันและกำหนดสิทธิ์ Claims
* **บรรทัดที่ 102:** `_repo.MergeGuestCartToUser(user.Id);`  
  ดึงรายการสินค้าที่หยิบใส่ตะกร้าไว้ตอนเป็น Guest เข้าสู่ตะกร้าของ User ในฐานข้อมูล
* **บรรทัดที่ 103:** `_repo.LogAudit("LOGIN", "user", user.Id.ToString(), $"เข้าสู่ระบบด้วย Username '{user.Username}'", user);`  
  บันทึกประวัติการเข้าสู่ระบบลงใน Audit Log
* **บรรทัดที่ 104:** *(บรรทัดว่าง)*
* **บรรทัดที่ 105:** `TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);`  
  แสดงข้อความต้อนรับ
* **บรรทัดที่ 106:** `TempData["SuccessTargetRole"] = user.Role.ToString();`  
  ส่งบทบาทไปยังหน้าถัดไป
* **บรรทัดที่ 107:** `TempData["SuccessTargetUser"] = user.Username;`  
  ส่งชื่อผู้ใช้
* **บรรทัดที่ 108:** *(บรรทัดว่าง)*
* **บรรทัดที่ 109:** `return RedirectToRoleHome(user.Role.ToString(), user.Username);`  
  ส่งผู้ใช้ไปยังหน้าหลักของบทบาทตนเอง
* **บรรทัดที่ 110:** `}`  
  ปิดบล็อกเมธอด Login [POST]
* **บรรทัดที่ 111:** *(บรรทัดว่าง)*
* **บรรทัดที่ 112:** `// Quick Demo Login สำหรับทดสอบคลิกเดียว (ตรวจสอบสถานะการระงับบัญชีก่อนอนุญาตให้เข้าสู่ระบบ)`  
  คอมเมนต์อธิบาย Quick Login
* **บรรทัดที่ 113:** `[HttpGet]`  
  รองรับคำขอ HTTP GET
* **บรรทัดที่ 114:** `[HttpPost]`  
  รองรับคำขอ HTTP POST
* **บรรทัดที่ 115:** `[IgnoreAntiforgeryToken]`  
  ยกเว้น Antiforgery Token
* **บรรทัดที่ 116:** `public async Task<IActionResult> QuickLogin(string? username = null, string? returnUrl = null)`  
  Action เมธอดรับชื่อผู้ใช้หรือชื่อบทบาทที่ต้องการเข้าใช้งานด่วน
* **บรรทัดที่ 117:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 118:** `if (string.IsNullOrWhiteSpace(username)) return RedirectToAction(nameof(Login));`  
  ถ้าไม่ระบุชื่อผู้ใช้ ส่งกลับหน้า Login
* **บรรทัดที่ 119:** *(บรรทัดว่าง)*
* **บรรทัดที่ 120:** `var user = _repo.FindUserByUsernameOrEmail(username);`  
  ค้นหาผู้ใช้จาก Username
* **บรรทัดที่ 121:** `if (user == null)`  
  ถ้าไม่พบ
* **บรรทัดที่ 122:** `{`
* **บรรทัดที่ 123:** `var info = DemoUserHelper.GetDemoUserInfo(username);`  
  ค้นหาจากชื่อบทบาทผ่าน DemoUserHelper
* **บรรทัดที่ 124:** `user = _repo.FindUserByUsernameOrEmail(info.Username);`  
  ดึงผู้ใช้ตามบทบาทนั้น
* **บรรทัดที่ 125:** `}`
* **บรรทัดที่ 126:** *(บรรทัดว่าง)*
* **บรรทัดที่ 127:** `if (user == null) return RedirectToAction(nameof(Login));`  
  ถ้ายังไม่พบผู้ใช้ ให้กลับหน้า Login
* **บรรทัดที่ 128:** *(บรรทัดว่าง)*
* **บรรทัดที่ 129:** `// หากบัญชีถูกระงับอยู่ ห้ามเข้าสู่ระบบเด็ดขาด และแสดงเหตุผลพร้อมระยะเวลาที่ถูกระงับ`  
  คอมเมนต์ตรวจสอบการระงับ
* **บรรทัดที่ 130:** `if (!user.IsActive)`  
  ถ้าบัญชีถูกระงับ
* **บรรทัดที่ 131:** `{`
* **บรรทัดที่ 132:** `await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);`  
  สั่งออกจากระบบทันที
* **บรรทัดที่ 133:** `return RedirectToAction(nameof(Login), new`  
  ส่งกลับหน้า Login พร้อมพารามิเตอร์แสดงการระงับ
* **บรรทัดที่ 134:** `{`
* **บรรทัดที่ 135:** `    suspended = true,`
* **บรรทัดที่ 136:** `    u = user.Username,`
* **บรรทัดที่ 137:** `    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",`
* **บรรทัดที่ 138:** `    until = user.SuspensionDisplayUntil`
* **บรรทัดที่ 139:** `});`
* **บรรทัดที่ 140:** `}`  
  ปิดบล็อกตรวจสอบการระงับ
* **บรรทัดที่ 141:** *(บรรทัดว่าง)*
* **บรรทัดที่ 142:** `await SignInUserAsync(user, isPersistent: true);`  
  ออกคุกกี้เซสชัน
* **บรรทัดที่ 143:** `_repo.MergeGuestCartToUser(user.Id);`  
  รวมตะกร้าสินค้า
* **บรรทัดที่ 144:** `_repo.LogAudit("QUICK_LOGIN", "user", user.Id.ToString(), $"Quick Login '{user.Username}'", user);`  
  บันทึก Audit Log
* **บรรทัดที่ 145:** *(บรรทัดว่าง)*
* **บรรทัดที่ 146:** `TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);`  
  ข้อความต้อนรับ
* **บรรทัดที่ 147:** `TempData["SuccessTargetRole"] = user.Role.ToString();`
* **บรรทัดที่ 148:** `TempData["SuccessTargetUser"] = user.Username;`
* **บรรทัดที่ 149:** *(บรรทัดว่าง)*
* **บรรทัดที่ 150:** `return RedirectToRoleHome(user.Role.ToString(), user.Username);`  
  ส่งไปยังหน้าแรกของบทบาทนั้น
* **บรรทัดที่ 151:** `}`  
  ปิดบล็อกเมธอด QuickLogin
* **บรรทัดที่ 152:** *(บรรทัดว่าง)*
* **บรรทัดที่ 153:** `// Multi-Role Switcher: สลับบทบาทผู้ใช้แบบเรียลไทม์ (หากบัญชีของบทบาทนั้นถูกระงับ จะไม่อนุญาตให้เข้าใช้งาน)`  
  คอมเมนต์สลับบทบาท
* **บรรทัดที่ 154:** `[HttpGet]`  
  รับคำขอ HTTP GET
* **บรรทัดที่ 155:** `public async Task<IActionResult> SwitchRole(string role, string? returnUrl = null)`  
  Action เมธอดรับชื่อบทบาทที่ต้องการสลับ
* **บรรทัดที่ 156:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 157:** `var info = DemoUserHelper.GetDemoUserInfo(role);`  
  ค้นหาบัญชีของบทบาทนั้น
* **บรรทัดที่ 158:** `var user = !string.IsNullOrEmpty(info.Username)`  
  ตรวจสอบชื่อผู้ใช้
* **บรรทัดที่ 159:** `    ? _repo.FindUserByUsernameOrEmail(info.Username)`  
  ดึงข้อมูลผู้ใช้จากฐานข้อมูล
* **บรรทัดที่ 160:** `    : null;`
* **บรรทัดที่ 161:** *(บรรทัดว่าง)*
* **บรรทัดที่ 162:** `if (user != null && !user.IsActive)`  
  หากบัญชีของบทบาทนั้นถูกระงับอยู่
* **บรรทัดที่ 163:** `{`
* **บรรทัดที่ 164:** `await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);`  
  สั่ง SignOut คุกกี้เดิมออก
* **บรรทัดที่ 165:** `return RedirectToAction(nameof(Login), new`  
  ส่งกลับหน้า Login พร้อมแจ้งการระงับ
* **บรรทัดที่ 166:** `{`
* **บรรทัดที่ 167:** `    suspended = true,`
* **บรรทัดที่ 168:** `    u = user.Username,`
* **บรรทัดที่ 169:** `    reason = user.SuspensionReason ?? "ระงับการใช้งานโดยผู้ดูแลระบบ",`
* **บรรทัดที่ 170:** `    until = user.SuspensionDisplayUntil`
* **บรรทัดที่ 171:** `});`
* **บรรทัดที่ 172:** `}`
* **บรรทัดที่ 173:** *(บรรทัดว่าง)*
* **บรรทัดที่ 174:** `if (user != null)`  
  หากพบบัญชีและสถานะปกติ
* **บรรทัดที่ 175:** `{`
* **บรรทัดที่ 176:** `await SignInUserAsync(user, isPersistent: true);`  
  ออกคุกกี้สิทธิ์ของบทบาทใหม่
* **บรรทัดที่ 177:** `_repo.MergeGuestCartToUser(user.Id);`  
  รวมตะกร้า
* **บรรทัดที่ 178:** `_repo.LogAudit("ROLE_SWITCH", "user", user.Id.ToString(), $"สลับบทบาทเป็น '{user.Username}' ({info.RoleName})", user);`  
  บันทึกประวัติการสลับบทบาทลงใน Audit Log
* **บรรทัดที่ 179:** `TempData["SuccessMessage"] = FormatWelcomeMessage(user.FullName);`
* **บรรทัดที่ 180:** `TempData["SuccessTargetRole"] = info.RoleName;`
* **บรรทัดที่ 181:** `TempData["SuccessTargetUser"] = user.Username;`
* **บรรทัดที่ 182:** `}`
* **บรรทัดที่ 183:** *(บรรทัดว่าง)*
* **บรรทัดที่ 184:** `if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))`  
  ตรวจสอบ returnUrl
* **บรรทัดที่ 185:** `{`
* **บรรทัดที่ 186:** `return LocalRedirect(returnUrl);`
* **บรรทัดที่ 187:** `}`
* **บรรทัดที่ 188:** *(บรรทัดว่าง)*
* **บรรทัดที่ 189:** `return RedirectToRoleHome(info.RoleName);`  
  ส่งกลับหน้าหลักของบทบาทที่เพิ่งสลับไป
* **บรรทัดที่ 190:** `}`  
  ปิดบล็อกเมธอด SwitchRole
* **บรรทัดที่ 191:** *(บรรทัดว่าง)*
* **บรรทัดที่ 192:** `// หน้า Register (GET) - แสดงหน้าสมัครสมาชิกเสมอโดยไม่เด้งกลับหน้าหลักแม้แท็บอื่นจะล็อกอินอยู่`  
  คอมเมนต์สมัครสมาชิก
* **บรรทัดที่ 193:** `[HttpGet]`  
  รับคำขอ HTTP GET
* **บรรทัดที่ 194:** `public IActionResult Register()`  
  Action เมธอดแสดงหน้าสมัครสมาชิก
* **บรรทัดที่ 195:** `{`
* **บรรทัดที่ 196:** `return View(new RegisterViewModel());`  
  ส่งโมเดลว่างไปแสดงผลใน `Views/Auth/Register.cshtml`
* **บรรทัดที่ 197:** `}`
* **บรรทัดที่ 198:** *(บรรทัดว่าง)*
* **บรรทัดที่ 199:** `// การประมวลผล Register (POST)`  
  คอมเมนต์บันทึกสมัครสมาชิก
* **บรรทัดที่ 200:** `[HttpPost]`  
  รับคำขอ HTTP POST
* **บรรทัดที่ 201:** `[IgnoreAntiforgeryToken]`  
  ยกเว้น Antiforgery Token
* **บรรทัดที่ 202:** `public async Task<IActionResult> Register(RegisterViewModel model)`  
  Action เมธอดรับข้อมูลสมัครสมาชิกจากฟอร์ม
* **บรรทัดที่ 203:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 204:** `if (!ModelState.IsValid)`  
  ตรวจสอบความถูกต้องของฟอร์ม
* **บรรทัดที่ 205:** `{`
* **บรรทัดที่ 206:** `return View(model);`  
  หากกรอกไม่ถูกต้อง ส่งกลับหน้าเดิม
* **บรรทัดที่ 207:** `}`
* **บรรทัดที่ 208:** *(บรรทัดว่าง)*
* **บรรทัดที่ 209:** `var newUser = _repo.RegisterCustomer(model);`  
  เรียกใช้คำสั่งลงทะเบียนลูกค้าใหม่ใน Repository
* **บรรทัดที่ 210:** `if (newUser == null)`  
  หากบันทึกไม่สำเร็จ (ชื่อผู้ใช้หรืออีเมลซ้ำ)
* **บรรทัดที่ 211:** `{`
* **บรรทัดที่ 212:** `ModelState.AddModelError(string.Empty, "ชื่อผู้ใช้หรืออีเมลนี้มีอยู่ในระบบแล้ว กรุณาใช้อันอื่น");`  
  แจ้งเตือนความซ้ำซ้อน
* **บรรทัดที่ 213:** `return View(model);`
* **บรรทัดที่ 214:** `}`
* **บรรทัดที่ 215:** *(บรรทัดว่าง)*
* **บรรทัดที่ 216:** `await SignInUserAsync(newUser, isPersistent: true);`  
  ล็อกอินให้อัตโนมัติทันที
* **บรรทัดที่ 217:** `_repo.MergeGuestCartToUser(newUser.Id);`  
  รวมตะกร้าสินค้าเดิมเข้ากับบัญชีใหม่
* **บรรทัดที่ 218:** `TempData["SuccessMessage"] = $"สมัครสมาชิกสำเร็จ! ยินดีต้อนรับคุณ {newUser.FullName}";`  
  แสดงข้อความต้อนรับ
* **บรรทัดที่ 219:** `TempData["SuccessTargetRole"] = "Customer";`
* **บรรทัดที่ 220:** `TempData["SuccessTargetUser"] = newUser.Username;`
* **บรรทัดที่ 221:** *(บรรทัดว่าง)*
* **บรรทัดที่ 222:** `return RedirectToAction("Index", "Home", new { role = newUser.Username });`  
  ส่งผู้ใช้ไปยังหน้าร้านค้าหลัก
* **บรรทัดที่ 223:** `}`  
  ปิดบล็อกเมธอด Register [POST]
* **บรรทัดที่ 224:** *(บรรทัดว่าง)*
* **บรรทัดที่ 225:** `// Logout`  
  คอมเมนต์ออกจากระบบ
* **บรรทัดที่ 226:** `[HttpPost]`
* **บรรทัดที่ 227:** `[HttpGet]`  
  รองรับทั้ง GET และ POST
* **บรรทัดที่ 228:** `[IgnoreAntiforgeryToken]`
* **บรรทัดที่ 229:** `public async Task<IActionResult> Logout()`  
  Action เมธอดออกจากระบบ
* **บรรทัดที่ 230:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 231:** `if (User.Identity?.IsAuthenticated == true)`  
  ตรวจสอบว่าเคยล็อกอินอยู่หรือไม่
* **บรรทัดที่ 232:** `{`
* **บรรทัดที่ 233:** `_repo.LogAudit("LOGOUT", "user", User.FindFirstValue(ClaimTypes.NameIdentifier), "ออกจากระบบ");`  
  บันทึกประวัติการออกจากระบบลงใน Audit Log
* **บรรทัดที่ 234:** `}`
* **บรรทัดที่ 235:** `await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);`  
  เพิกถอนและลบ Cookie Authentication ออกจากเบราว์เซอร์
* **บรรทัดที่ 236:** `TempData["SuccessMessage"] = "คุณได้ออกจากระบบเรียบร้อยแล้ว";`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 237:** `TempData["SuccessTargetRole"] = "Guest";`
* **บรรทัดที่ 238:** `TempData["SuccessTargetUser"] = "";`
* **บรรทัดที่ 239:** `return RedirectToAction("Index", "Home", new { role = "Guest" });`  
  ส่งกลับไปยังหน้า Home ในสถานะ Guest
* **บรรทัดที่ 240:** `}`  
  ปิดบล็อกเมธอด Logout
* **บรรทัดที่ 241:** *(บรรทัดว่าง)*
* **บรรทัดที่ 242:** `// หน้า Access Denied เมื่อไม่มีสิทธิ์`  
  คอมเมนต์
* **บรรทัดที่ 243:** `[HttpGet]`
* **บรรทัดที่ 244:** `public IActionResult AccessDenied()`  
  Action เมธอดแสดงหน้าแจ้งเตือนเมื่อไม่มีสิทธิ์เข้าถึง (403 Forbidden)
* **บรรทัดที่ 245:** `{`
* **บรรทัดที่ 246:** `return View();`  
  เรนเดอร์หน้า `Views/Auth/AccessDenied.cshtml`
* **บรรทัดที่ 247:** `}`  
  ปิดบล็อกเมธอด AccessDenied
* **บรรทัดที่ 248:** *(บรรทัดว่าง)*
* **บรรทัดที่ 249:** `private static string CleanName(string? fullName)`  
  ฟังก์ชันตัดคำที่ไม่จำเป็นออกจากชื่อ (เช่น ข้อความในวงเล็บ)
* **บรรทัดที่ 250:** `{`
* **บรรทัดที่ 251:** `if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;`
* **บรรทัดที่ 252:** `return System.Text.RegularExpressions.Regex.Replace(fullName, @"\s*\([^)]*\)", "").Trim();`  
  ใช้ Regex ตัดวงเล็บออก
* **บรรทัดที่ 253:** `}`
* **บรรทัดที่ 254:** *(บรรทัดว่าง)*
* **บรรทัดที่ 255:** `private static string FormatWelcomeMessage(string? fullName)`  
  ฟังก์ชันจัดรูปแบบข้อความต้อนรับ
* **บรรทัดที่ 256:** `{`
* **บรรทัดที่ 257:** `var clean = CleanName(fullName);`  
  ล้างชื่อให้สะอาด
* **บรรทัดที่ 258:** `if (string.IsNullOrWhiteSpace(clean))`
* **บรรทัดที่ 259:** `{`
* **บรรทัดที่ 260:** `return "ยินดีต้อนรับเข้าสู่ระบบ";`
* **บรรทัดที่ 261:** `}`
* **บรรทัดที่ 262:** *(บรรทัดว่าง)*
* **บรรทัดที่ 263:** `if (clean.StartsWith("คุณ "))`  
  ตรวจสอบคำนำหน้า
* **บรรทัดที่ 264:** `{`
* **บรรทัดที่ 265:** `clean = clean.Substring(4).Trim();`
* **บรรทัดที่ 266:** `}`
* **บรรทัดที่ 267:** `else if (clean.StartsWith("คุณ"))`
* **บรรทัดที่ 268:** `{`
* **บรรทัดที่ 269:** `clean = clean.Substring(3).Trim();`
* **บรรทัดที่ 270:** `}`
* **บรรทัดที่ 271:** *(บรรทัดว่าง)*
* **บรรทัดที่ 272:** `return string.IsNullOrWhiteSpace(clean) ? "ยินดีต้อนรับเข้าสู่ระบบ" : $"ยินดีต้อนรับคุณ {clean}";`  
  สร้างข้อความต้อนรับที่ไพเราะ
* **บรรทัดที่ 273:** `}`
* **บรรทัดที่ 274:** *(บรรทัดว่าง)*
* **บรรทัดที่ 275:** `private async Task SignInUserAsync(SystemUser user, bool isPersistent)`  
  ฟังก์ชัน Helper ออก Claims และสร้าง Cookie Authentication Ticket
* **บรรทัดที่ 276:** `{`  
  เปิดบล็อกฟังก์ชัน
* **บรรทัดที่ 277:** `var cleanFullName = CleanName(user.FullName);`  
  ล้างชื่อผู้ใช้
* **บรรทัดที่ 278:** `if (string.IsNullOrWhiteSpace(cleanFullName)) cleanFullName = user.Username;`
* **บรรทัดที่ 279:** *(บรรทัดว่าง)*
* **บรรทัดที่ 280:** `var claims = new List<Claim>`  
  สร้างชุด Claims บันทึกตัวตน
* **บรรทัดที่ 281:** `{`
* **บรรทัดที่ 282:** `    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),`  
  บันทึก User ID
* **บรรทัดที่ 283:** `    new Claim(ClaimTypes.Name, user.Username),`  
  บันทึก Username
* **บรรทัดที่ 284:** `    new Claim(ClaimTypes.Role, user.Role.ToString()),`  
  บันทึก Role หลัก
* **บรรทัดที่ 285:** `    new Claim("FullName", cleanFullName),`  
  บันทึกชื่อ-สกุล
* **บรรทัดที่ 286:** `    new Claim(ClaimTypes.Email, user.Email),`  
  บันทึกอีเมล
* **บรรทัดที่ 287:** `    new Claim("Phone", user.Phone)`  
  บันทึกเบอร์โทร
* **บรรทัดที่ 288:** `};`
* **บรรทัดที่ 289:** *(บรรทัดว่าง)*
* **บรรทัดที่ 290:** `// Multi-Role Support: สำหรับ Owner และ Admin ให้สิทธิ์เข้าถึงทุก Role พร้อมกันได้ เพื่อให้เปิดหลายแท็บ/หลายหน้าต่างทดสอบได้ทันที`  
  คอมเมนต์ระบบสิทธิ์ทดสอบ
* **บรรทัดที่ 291:** `if (user.Role == UserRole.Owner || user.Role == UserRole.Admin)`  
  ตรวจสอบว่าผู้ใช้เป็น Owner หรือ Admin หรือไม่
* **บรรทัดที่ 292:** `{`
* **บรรทัดที่ 293:** `foreach (UserRole r in Enum.GetValues(typeof(UserRole)))`  
  วนลูปทุกบทบาทในระบบ
* **บรรทัดที่ 294:** `{`
* **บรรทัดที่ 295:** `if (r != user.Role)`  
  ถ้าไม่ใช่บทบาทหลักของตนเอง
* **บรรทัดที่ 296:** `{`
* **บรรทัดที่ 297:** `claims.Add(new Claim(ClaimTypes.Role, r.ToString()));`  
  เพิ่ม Claim สิทธิ์ของ Role นั้นเข้าไปด้วย เพื่อให้เปิดดูได้ทุกหน้าจอพร้อมกันโดยไม่ติด Access Denied
* **บรรทัดที่ 298:** `}`
* **บรรทัดที่ 299:** `}`
* **บรรทัดที่ 300:** `}`
* **บรรทัดที่ 301:** *(บรรทัดว่าง)*
* **บรรทัดที่ 302:** `var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);`  
  สร้าง ClaimsIdentity ด้วย Cookie Scheme
* **บรรทัดที่ 303:** `var authProperties = new AuthenticationProperties`  
  กำหนดคุณสมบัติการยืนยันตัวตน
* **บรรทัดที่ 304:** `{`
* **บรรทัดที่ 305:** `    IsPersistent = isPersistent,`  
  จดจำการเข้าสู่ระบบ
* **บรรทัดที่ 306:** `    ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)`  
  กำหนดอายุคุกกี้ 8 ชั่วโมง
* **บรรทัดที่ 307:** `};`
* **บรรทัดที่ 308:** *(บรรทัดว่าง)*
* **บรรทัดที่ 309:** `await HttpContext.SignInAsync(`  
  บันทึก Cookie ลงในเบราว์เซอร์
* **บรรทัดที่ 310:** `    CookieAuthenticationDefaults.AuthenticationScheme,`
* **บรรทัดที่ 311:** `    new ClaimsPrincipal(claimsIdentity),`
* **บรรทัดที่ 312:** `    authProperties);`
* **บรรทัดที่ 313:** `}`  
  ปิดบล็อก SignInUserAsync
* **บรรทัดที่ 314:** *(บรรทัดว่าง)*
* **บรรทัดที่ 315:** `// Helper: นำทางไปยังหน้าหลักของแต่ละ Role (รองรับแยก Username ของแต่ละแท็บอิสระ)`  
  คอมเมนต์การนำทางตามบทบาท
* **บรรทัดที่ 316:** `private IActionResult RedirectToRoleHome(string? role, string? username = null)`  
  ฟังก์ชัน Helper ตัดสินใจว่าจะส่งผู้ใช้ไปยัง Controller และ Action ไหนตามบทบาท
* **บรรทัดที่ 317:** `{`  
  เปิดบล็อก Helper
* **บรรทัดที่ 318:** `var canonical = DemoUserHelper.GetDemoUserInfo(role);`  
  ค้นหาข้อมูลบทบาท
* **บรรทัดที่ 319:** `string tabRoleKey = (!string.IsNullOrWhiteSpace(username) &&`
* **บรรทัดที่ 320:** `                     !string.Equals(username, canonical.Username, System.StringComparison.OrdinalIgnoreCase))`
* **บรรทัดที่ 321:** `    ? username`
* **บรรทัดที่ 322:** `    : (role ?? "Customer");`  
  กำหนดคีย์ประจำแท็บ
* **บรรทัดที่ 323:** *(บรรทัดว่าง)*
* **บรรทัดที่ 324:** `return role switch`  
  ใช้ switch expression เลือกว่าจะ Redirect ไปไหน
* **บรรทัดที่ 325:** `{`
* **บรรทัดที่ 326:** `    nameof(UserRole.InventoryStaff) => RedirectToAction("Index", "Inventory", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายคลังสินค้า ไปที่ `Inventory/Index`
* **บรรทัดที่ 327:** `    nameof(UserRole.OrderStaff) => RedirectToAction("Index", "Order", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายคำสั่งซื้อ ไปที่ `Order/Index`
* **บรรทัดที่ 328:** `    nameof(UserRole.ShippingStaff) => RedirectToAction("Index", "Shipping", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายจัดส่ง ไปที่ `Shipping/Index`
* **บรรทัดที่ 329:** `    nameof(UserRole.MarketingStaff) => RedirectToAction("Index", "Marketing", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายการตลาด ไปที่ `Marketing/Index`
* **บรรทัดที่ 330:** `    nameof(UserRole.AccountingStaff) => RedirectToAction("Index", "Accounting", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายบัญชี ไปที่ `Accounting/Index`
* **บรรทัดที่ 331:** `    nameof(UserRole.ITSupport) => RedirectToAction("Index", "Support", new { role = tabRoleKey }),`  
  ถ้าเป็นฝ่ายไอทีซัพพอร์ต ไปที่ `Support/Index`
* **บรรทัดที่ 332:** `    nameof(UserRole.Admin) => RedirectToAction("Index", "Admin", new { role = tabRoleKey }),`  
  ถ้าเป็นผู้ดูแลระบบ ไปที่ `Admin/Index`
* **บรรทัดที่ 333:** `    nameof(UserRole.Owner) => RedirectToAction("Index", "Owner", new { role = tabRoleKey }),`  
  ถ้าเป็นผู้บริหาร ไปที่ `Owner/Index`
* **บรรทัดที่ 334:** `    _ => RedirectToAction("Index", "Home", new { role = tabRoleKey })`  
  ถ้าเป็นลูกค้าหรือทั่วไป ไปที่ `Home/Index`
* **บรรทัดที่ 335:** `};`  
  ปิด switch expression
* **บรรทัดที่ 336:** `}`  
  ปิดบล็อกเมธอด RedirectToRoleHome
* **บรรทัดที่ 337:** `}`  
  ปิดบล็อกคลาส AuthController
* **บรรทัดที่ 338:** `}`  
  ปิดบล็อกเนมสเปซ


---

# 5. CartController.cs (245 บรรทัด)

### โค้ดต้นฉบับเต็มของ CartController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Hosting;
3: using Microsoft.AspNetCore.Http;
4: using Microsoft.AspNetCore.Mvc;
5: using Microsoft.AspNetCore.SignalR;
6: using CSI402_Project_final.Helpers;
7: using CSI402_Project_final.Hubs;
8: using CSI402_Project_final.Models;
9: using System.Linq;
10: using System.Threading.Tasks;
11: 
12: namespace CSI402_Project_final.Controllers
13: {
14:     public class CartController : Controller
15:     {
16:         private readonly PetShopRepository _repo;
17:         private readonly IHubContext<PetShopHub> _hubContext;
18:         private readonly IWebHostEnvironment _env;
19: 
20:         public CartController(PetShopRepository repo, IHubContext<PetShopHub> hubContext, IWebHostEnvironment env)
21:         {
22:             _repo = repo;
23:             _hubContext = hubContext;
24:             _env = env;
25:         }
26: 
27:         // หน้าตะกร้าสินค้า (Req 3, 4, 11, 12, 13, 14, 15, 16)
28:         public IActionResult Index()
29:         {
30:             ViewBag.ActivePromotions = _repo.GetValidPromotions();
31:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
32:             return View(_repo.CurrentCart);
33:         }
34: 
35:         [HttpPost]
36:         public async Task<IActionResult> Add(int productId, int quantity = 1, string returnUrl = "/Cart")
37:         {
38:             // ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง (Guest ไม่สามารถหยิบสินค้าลงในตะกร้าได้)
39:             if (User?.Identity?.IsAuthenticated != true)
40:             {
41:                 string targetLoginUrl = Url.Action("Login", "Auth", new { returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl }) ?? "/Auth/Login";
42:                 if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
43:                     Request.Headers.Accept.ToString().Contains("application/json"))
44:                 {
45:                     return Json(new
46:                     {
47:                         success = false,
48:                         requireLogin = true,
49:                         redirectUrl = targetLoginUrl,
50:                         message = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า"
51:                     });
52:                 }
53:                 TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า";
54:                 return Redirect(targetLoginUrl);
55:             }
56: 
57:             int newCount = _repo.AddToCart(productId, quantity);
58:             var product = _repo.GetProductById(productId);
59:             string productName = product?.Name ?? "สินค้า";
60: 
61:             // SignalR: กระจายการอัปเดตจำนวนคนใส่ตะกร้าและสต็อกคงเหลือแบบ Real-Time (Req 8) ในพื้นหลังโดยไม่บล็อกการตอบกลับ
62:             _ = Task.Run(async () =>
63:             {
64:                 try
65:                 {
66:                     await _hubContext.Clients.All.SendAsync("ReceiveCartCountUpdate", productId, newCount);
67:                     if (product != null)
68:                     {
69:                         await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
70:                         if (product.IsLowStock)
71:                         {
72:                             await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);
73:                         }
74:                     }
75:                 }
76:                 catch { }
77:             });
78: 
79:             // รองรับ AJAX ไม่ต้องโหลดหน้าใหม่ เพื่อความลื่นไหล (Smooth User Experience)
80:             if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
81:                 Request.Headers.Accept.ToString().Contains("application/json"))
82:             {
83:                 return Json(new
84:                 {
85:                     success = true,
86:                     productId = productId,
87:                     productName = productName,
88:                     cartCount = _repo.CurrentCart.TotalItemCount,
89:                     productCartCount = newCount,
90:                     remainingStock = product?.StockQuantity ?? 0,
91:                     isOutOfStock = (product?.StockQuantity ?? 0) <= 0,
92:                     message = $"เพิ่ม \"{productName}\" ลงในตะกร้าเรียบร้อยแล้ว"
93:                 });
94:             }
95: 
96:             if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
97:             {
98:                 return LocalRedirect(returnUrl);
99:             }
100:             return RedirectToAction(nameof(Index));
101:         }
102: 
103:         [HttpPost]
104:         public async Task<IActionResult> Update(int productId, int quantity)
105:         {
106:             _repo.UpdateCartItem(productId, quantity);
107:             var product = _repo.GetProductById(productId);
108:             if (product != null)
109:             {
110:                 await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
111:                 if (product.IsLowStock)
112:                 {
113:                     await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);
114:                 }
115:             }
116:             return RedirectToAction(nameof(Index));
117:         }
118: 
119:         [HttpPost]
120:         public async Task<IActionResult> Remove(int productId)
121:         {
122:             _repo.RemoveFromCart(productId);
123:             var product = _repo.GetProductById(productId);
124:             if (product != null)
125:             {
126:                 await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);
127:             }
128:             return RedirectToAction(nameof(Index));
129:         }
130: 
131:         // หน้าสรุปคำสั่งซื้อและชำระเงิน (ต้องเข้าสู่ระบบก่อนเพื่อเชื่อม UserId ลง DB)
132:         [Authorize]
133:         public IActionResult Checkout()
134:         {
135:             var cart = _repo.CurrentCart;
136:             if (!cart.Items.Any())
137:             {
138:                 return RedirectToAction(nameof(Index));
139:             }
140:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
141:             ViewBag.PromptPayPayload = PromptPayHelper.GeneratePayload("0812345678", cart.GrandTotal);
142:             return View(cart);
143:         }
144: 
145:         // คำนวณค่าจัดส่งและยอดสุทธิรวมตามผู้ให้บริการขนส่งแบบ Real-Time (EMS ด่วนพิเศษ +30 บาท)
146:         [HttpGet]
147:         public IActionResult GetCourierCalculation(string courier)
148:         {
149:             var cart = _repo.CurrentCart;
150:             var activePromos = _repo.GetValidPromotions();
151:             cart.Recalculate(activePromos);
152: 
153:             decimal surcharge = Cart.GetCourierSurcharge(courier);
154:             decimal shippingFee = cart.ShippingFee + surcharge;
155:             decimal grandTotal = cart.NetProductPrice + shippingFee;
156:             string payload = PromptPayHelper.GeneratePayload("0812345678", grandTotal);
157:             string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={Uri.EscapeDataString(payload)}";
158: 
159:             return Json(new
160:             {
161:                 success = true,
162:                 courier = courier,
163:                 baseShippingFee = cart.ShippingFee,
164:                 surcharge = surcharge,
165:                 shippingFee = shippingFee,
166:                 grandTotal = grandTotal,
167:                 qrUrl = qrUrl,
168:                 formattedShippingFee = shippingFee.ToString("N2"),
169:                 formattedGrandTotal = grandTotal.ToString("N2"),
170:                 isEms = surcharge > 0
171:             });
172:         }
173: 
174:         // หากมีการเปิดหน้า /Cart/SubmitOrder ด้วย GET (เช่น กดรีเฟรชหน้า หรือกด Back) ให้กลับไปหน้า Checkout อย่างถูกต้อง
175:         [HttpGet]
176:         public IActionResult SubmitOrder()
177:         {
178:             return RedirectToAction(nameof(Checkout));
179:         }
180: 
181:         [Authorize]
182:         [HttpPost]
183:         public async Task<IActionResult> SubmitOrder(
184:             string customerName,
185:             string customerPhone,
186:             string shippingAddress,
187:             string paymentMethod = "โอนผ่านธนาคาร / PromptPay",
188:             string courier = "Flash Express",
189:             IFormFile? paymentSlip = null)
190:         {
191:             try
192:             {
193:                 string? slipUrl = null;
194:                 if (paymentSlip != null && paymentSlip.Length > 0)
195:                 {
196:                     var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");
197:                     if (uploadErr != null)
198:                     {
199:                         TempData["ErrorMessage"] = uploadErr;
200:                         return RedirectToAction(nameof(Checkout));
201:                     }
202:                     slipUrl = uploadedUrl;
203:                 }
204: 
205:                 var username = User.Identity?.Name ?? "";
206:                 var (newOrder, errorMessage) = _repo.CheckoutCurrentCart(
207:                     username, customerName, customerPhone, shippingAddress, paymentMethod, slipUrl, courier);
208: 
209:                 if (newOrder == null)
210:                 {
211:                     TempData["ErrorMessage"] = errorMessage ?? "ไม่สามารถดำเนินการสั่งซื้อได้";
212:                     return RedirectToAction(nameof(Checkout));
213:                 }
214: 
215:                 // SignalR: แจ้งเตือนฝ่ายคำสั่งซื้อ/บัญชี แบบ Real-Time ทันที (มี try-catch เพื่อป้องกันข้อผิดพลาดของ socket ไม่ให้กระทบการสั่งซื้อ)
216:                 try
217:                 {
218:                     await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", newOrder.OrderId, newOrder.CustomerName, newOrder.GrandTotal);
219: 
220:                     // ตรวจสอบสินค้าที่เหลือเพื่อเตือน Low Stock (Req 18)
221:                     var productIds = newOrder.Items.Select(i => i.ProductId).ToList();
222:                     var affectedProducts = _repo.Products.Where(p => productIds.Contains(p.Id)).ToList();
223:                     foreach (var prod in affectedProducts)
224:                     {
225:                         if (prod.IsLowStock)
226:                         {
227:                             await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", prod.Name, prod.StockQuantity);
228:                         }
229:                     }
230:                 }
231:                 catch
232:                 {
233:                     // การส่ง SignalR ไม่ควรทำให้กระบวนการสั่งซื้อล้มเหลว
234:                 }
235: 
236:                 return RedirectToAction("Orders", "Account", new { successOrderId = newOrder.OrderId });
237:             }
238:             catch (Exception ex)
239:             {
240:                 TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการสั่งซื้อ: {ex.Message}";
241:                 return RedirectToAction(nameof(Checkout));
242:             }
243:         }
244:     }
245: }
```

### คำอธิบายการทำงานของ CartController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize]` ในขั้นตอน Checkout และ SubmitOrder
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Hosting;`  
  นำเข้า `IWebHostEnvironment` สำหรับอ้างอิงตำแหน่งเซฟสลิปโอนเงินในโฟลเดอร์ `wwwroot`
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.Http;`  
  นำเข้า `IFormFile` สำหรับรับไฟล์รูปภาพสลิปการโอนเงินจากฟอร์ม
* **บรรทัดที่ 4:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาสพื้นฐาน Controller, IActionResult, HttpGet, HttpPost, Json, Redirect
* **บรรทัดที่ 5:** `using Microsoft.AspNetCore.SignalR;`  
  นำเข้า `IHubContext` สำหรับส่งการแจ้งเตือน Real-Time ทั่วทั้งระบบ
* **บรรทัดที่ 6:** `using CSI402_Project_final.Helpers;`  
  นำเข้าคลาสตัวช่วย `FileUploadHelper` (บันทึกสลิป) และ `PromptPayHelper` (คำนวณ QR Code พร้อมเพย์)
* **บรรทัดที่ 7:** `using CSI402_Project_final.Hubs;`  
  นำเข้า `PetShopHub` สำหรับการเชื่อมต่อ SignalR WebSocket
* **บรรทัดที่ 8:** `using CSI402_Project_final.Models;`  
  นำเข้า Model ตะกร้าสินค้า Cart, Product, Promotion, PetShopRepository
* **บรรทัดที่ 9:** `using System.Linq;`  
  นำเข้าคำสั่งจัดการคอลเลกชัน LINQ เช่น `.Any()`, `.Select()`, `.Where()`
* **บรรทัดที่ 10:** `using System.Threading.Tasks;`  
  นำเข้าการทำงานแบบ Asynchronous Task
* **บรรทัดที่ 11:** *(บรรทัดว่าง)*
* **บรรทัดที่ 12:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 13:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 14:** `public class CartController : Controller`  
  ประกาศคลาส `CartController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 15:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 16:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บอินสแตนซ์ของ Repository สำหรับจัดการตะกร้าสินค้าและคำสั่งซื้อ
* **บรรทัดที่ 17:** `private readonly IHubContext<PetShopHub> _hubContext;`  
  ฟิลด์เก็บ SignalR Hub Context สำหรับส่งสัญญาณ Real-Time
* **บรรทัดที่ 18:** `private readonly IWebHostEnvironment _env;`  
  ฟิลด์เก็บสภาพแวดล้อมโฮสต์เพื่อหาโฟลเดอร์เก็บไฟล์สลิป
* **บรรทัดที่ 19:** *(บรรทัดว่าง)*
* **บรรทัดที่ 20:** `public CartController(PetShopRepository repo, IHubContext<PetShopHub> hubContext, IWebHostEnvironment env)`  
  Constructor รับการส่งมอบ Repository, HubContext และ WebHostEnvironment ผ่าน Dependency Injection
* **บรรทัดที่ 21:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 22:** `_repo = repo;`  
  กำหนดค่าตัวแปร Repository
* **บรรทัดที่ 23:** `_hubContext = hubContext;`  
  กำหนดค่าตัวแปร Hub Context
* **บรรทัดที่ 24:** `_env = env;`  
  กำหนดค่าตัวแปร WebHostEnvironment
* **บรรทัดที่ 25:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 26:** *(บรรทัดว่าง)*
* **บรรทัดที่ 27:** `// หน้าตะกร้าสินค้า (Req 3, 4, 11, 12, 13, 14, 15, 16)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 28:** `public IActionResult Index()`  
  Action เมธอดแสดงหน้าตะกร้าสินค้าผ่าน HTTP GET
* **บรรทัดที่ 29:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 30:** `ViewBag.ActivePromotions = _repo.GetValidPromotions();`  
  ดึงรายการแคมเปญโปรโมชั่นที่เปิดใช้งานอยู่ ใส่ใน ViewBag
* **บรรทัดที่ 31:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 32:** `return View(_repo.CurrentCart);`  
  ส่งตะกร้าสินค้าของผู้ใช้ปัจจุบันเป็น Model ไปเรนเดอร์ใน `Views/Cart/Index.cshtml`
* **บรรทัดที่ 33:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 34:** *(บรรทัดว่าง)*
* **บรรทัดที่ 35:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 36:** `public async Task<IActionResult> Add(int productId, int quantity = 1, string returnUrl = "/Cart")`  
  Action เมธอดแบบ Asynchronous สำหรับหยิบสินค้าลงตะกร้า
* **บรรทัดที่ 37:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 38:** `// ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง (Guest ไม่สามารถหยิบสินค้าลงในตะกร้าได้)`  
  คอมเมนต์ตรวจสอบสิทธิ์
* **บรรทัดที่ 39:** `if (User?.Identity?.IsAuthenticated != true)`  
  ตรวจสอบว่าผู้ใช้ล็อกอินหรือยัง ถ้ายังไม่ล็อกอินจะเข้าสู่เงื่อนไขนี้
* **บรรทัดที่ 40:** `{`
* **บรรทัดที่ 41:** `string targetLoginUrl = Url.Action("Login", "Auth", new { returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl }) ?? "/Auth/Login";`  
  สร้าง URL หน้า Login พร้อมแนบ returnUrl
* **บรรทัดที่ 42:** `if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||`  
  ตรวจสอบว่าส่งมาจากคำขอ AJAX หรือไม่
* **บรรทัดที่ 43:** `    Request.Headers.Accept.ToString().Contains("application/json"))`  
  หรือร้องขอข้อมูลแบบ JSON
* **บรรทัดที่ 44:** `{`
* **บรรทัดที่ 45:** `return Json(new`  
  ส่งผลลัพธ์เป็น JSON กลับไปให้ JavaScript
* **บรรทัดที่ 46:** `{`
* **บรรทัดที่ 47:** `    success = false,`
* **บรรทัดที่ 48:** `    requireLogin = true,`  
  แจ้งให้หน้าบ้านรู้ว่าต้องล็อกอินก่อน
* **บรรทัดที่ 49:** `    redirectUrl = targetLoginUrl,`  
  ส่ง URL หน้าล็อกอิน
* **บรรทัดที่ 50:** `    message = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า"`
* **บรรทัดที่ 51:** `});`
* **บรรทัดที่ 52:** `}`
* **บรรทัดที่ 53:** `TempData["ErrorMessage"] = "กรุณาเข้าสู่ระบบก่อนหยิบสินค้าลงในตะกร้า";`  
  กรณีส่งฟอร์มปกติ ตั้งค่า Flash Message
* **บรรทัดที่ 54:** `return Redirect(targetLoginUrl);`  
  Redirect ไปหน้า Login
* **บรรทัดที่ 55:** `}`
* **บรรทัดที่ 56:** *(บรรทัดว่าง)*
* **บรรทัดที่ 57:** `int newCount = _repo.AddToCart(productId, quantity);`  
  สั่ง Repository นำสินค้าเข้าตะกร้า และรับจำนวนสินค้าชิ้นนี้ในตะกร้ากลับมา
* **บรรทัดที่ 58:** `var product = _repo.GetProductById(productId);`  
  คิวรีข้อมูลสินค้าเพื่อนำชื่อและสต็อกคงเหลือมาเตรียมบรอดแคสต์
* **บรรทัดที่ 59:** `string productName = product?.Name ?? "สินค้า";`  
  ดึงชื่อสินค้า
* **บรรทัดที่ 60:** *(บรรทัดว่าง)*
* **บรรทัดที่ 61:** `// SignalR: กระจายการอัปเดตจำนวนคนใส่ตะกร้าและสต็อกคงเหลือแบบ Real-Time (Req 8) ในพื้นหลังโดยไม่บล็อกการตอบกลับ`  
  คอมเมนต์ Real-Time
* **บรรทัดที่ 62:** `_ = Task.Run(async () =>`  
  เปิดเธรดทำงานในพื้นหลัง (Background Task) ไม่ให้บล็อกการตอบกลับของ Action
* **บรรทัดที่ 63:** `{`
* **บรรทัดที่ 64:** `try`
* **บรรทัดที่ 65:** `{`
* **บรรทัดที่ 66:** `await _hubContext.Clients.All.SendAsync("ReceiveCartCountUpdate", productId, newCount);`  
  บรอดแคสต์ยอดคนใส่ตะกร้าไปยังทุกหน้าจอ
* **บรรทัดที่ 67:** `if (product != null)`  
  ตรวจสอบว่ามีสินค้า
* **บรรทัดที่ 68:** `{`
* **บรรทัดที่ 69:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);`  
  บรอดแคสต์สต็อกคงเหลือล่าสุด
* **บรรทัดที่ 70:** `if (product.IsLowStock)`  
  ตรวจสอบว่าสต็อกต่ำกว่าเกณฑ์หรือไม่
* **บรรทัดที่ 71:** `{`
* **บรรทัดที่ 72:** `await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);`  
  ส่งสัญญาณเตือนภัย Low Stock Alert ไปยังหน้าจอฝ่ายคลังสินค้า
* **บรรทัดที่ 73:** `}`
* **บรรทัดที่ 74:** `}`
* **บรรทัดที่ 75:** `}`
* **บรรทัดที่ 76:** `catch { }`  
  ดักจับข้อผิดพลาด Socket
* **บรรทัดที่ 77:** `});`
* **บรรทัดที่ 78:** *(บรรทัดว่าง)*
* **บรรทัดที่ 79:** `// รองรับ AJAX ไม่ต้องโหลดหน้าใหม่ เพื่อความลื่นไหล (Smooth User Experience)`  
  คอมเมนต์ Optimistic UI
* **บรรทัดที่ 80:** `if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||`  
  ตรวจสอบว่าส่งมาจาก AJAX หรือไม่
* **บรรทัดที่ 81:** `    Request.Headers.Accept.ToString().Contains("application/json"))`
* **บรรทัดที่ 82:** `{`
* **บรรทัดที่ 83:** `return Json(new`  
  ส่งผลลัพธ์เป็น JSON ตอบกลับ
* **บรรทัดที่ 84:** `{`
* **บรรทัดที่ 85:** `    success = true,`
* **บรรทัดที่ 86:** `    productId = productId,`
* **บรรทัดที่ 87:** `    productName = productName,`
* **บรรทัดที่ 88:** `    cartCount = _repo.CurrentCart.TotalItemCount,`  
  ส่งยอดรวมสินค้าในตะกร้าให้หน้าบ้านอัปเดตตัวเลข Badge ทันที
* **บรรทัดที่ 89:** `    productCartCount = newCount,`
* **บรรทัดที่ 90:** `    remainingStock = product?.StockQuantity ?? 0,`
* **บรรทัดที่ 91:** `    isOutOfStock = (product?.StockQuantity ?? 0) <= 0,`
* **บรรทัดที่ 92:** `    message = $"เพิ่ม \"{productName}\" ลงในตะกร้าเรียบร้อยแล้ว"`
* **บรรทัดที่ 93:** `});`
* **บรรทัดที่ 94:** `}`
* **บรรทัดที่ 95:** *(บรรทัดว่าง)*
* **บรรทัดที่ 96:** `if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))`  
  กรณีฟอร์มปกติ ตรวจสอบความปลอดภัยของ returnUrl
* **บรรทัดที่ 97:** `{`
* **บรรทัดที่ 98:** `return LocalRedirect(returnUrl);`  
  ส่งกลับ URL เดิม
* **บรรทัดที่ 99:** `}`
* **บรรทัดที่ 100:** `return RedirectToAction(nameof(Index));`  
  ส่งไปหน้าตะกร้าสินค้า
* **บรรทัดที่ 101:** `}`  
  ปิดบล็อกเมธอด Add
* **บรรทัดที่ 102:** *(บรรทัดว่าง)*
* **บรรทัดที่ 103:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 104:** `public async Task<IActionResult> Update(int productId, int quantity)`  
  Action เมธอดรับรหัสสินค้าและจำนวนชิ้นใหม่จากหน้าตะกร้า
* **บรรทัดที่ 105:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 106:** `_repo.UpdateCartItem(productId, quantity);`  
  สั่ง Repository ปรับปรุงจำนวนสินค้าในตะกร้า
* **บรรทัดที่ 107:** `var product = _repo.GetProductById(productId);`  
  ค้นหาข้อมูลสินค้า
* **บรรทัดที่ 108:** `if (product != null)`
* **บรรทัดที่ 109:** `{`
* **บรรทัดที่ 110:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);`  
  ส่งสัญญาณ SignalR ปรับสต็อกสด
* **บรรทัดที่ 111:** `if (product.IsLowStock)`
* **บรรทัดที่ 112:** `{`
* **บรรทัดที่ 113:** `await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);`  
  ส่งเตือน Low Stock Alert
* **บรรทัดที่ 114:** `}`
* **บรรทัดที่ 115:** `}`
* **บรรทัดที่ 116:** `return RedirectToAction(nameof(Index));`  
  โหลดหน้าตะกร้าใหม่เพื่อคำนวณโปรโมชั่นและยอดเงินใหม่
* **บรรทัดที่ 117:** `}`  
  ปิดบล็อกเมธอด Update
* **บรรทัดที่ 118:** *(บรรทัดว่าง)*
* **บรรทัดที่ 119:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 120:** `public async Task<IActionResult> Remove(int productId)`  
  Action เมธอดรับรหัสสินค้าที่ต้องการลบ
* **บรรทัดที่ 121:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 122:** `_repo.RemoveFromCart(productId);`  
  สั่ง Repository ลบสินค้ารายการนั้นออกจากตะกร้า
* **บรรทัดที่ 123:** `var product = _repo.GetProductById(productId);`  
  ค้นหาข้อมูลสินค้า
* **บรรทัดที่ 124:** `if (product != null)`
* **บรรทัดที่ 125:** `{`
* **บรรทัดที่ 126:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", productId, product.StockQuantity);`  
  บรอดแคสต์อัปเดตสต็อกสด
* **บรรทัดที่ 127:** `}`
* **บรรทัดที่ 128:** `return RedirectToAction(nameof(Index));`  
  รีเฟรชหน้าตะกร้าสินค้า
* **บรรทัดที่ 129:** `}`  
  ปิดบล็อกเมธอด Remove
* **บรรทัดที่ 130:** *(บรรทัดว่าง)*
* **บรรทัดที่ 131:** `// หน้าสรุปคำสั่งซื้อและชำระเงิน (ต้องเข้าสู่ระบบก่อนเพื่อเชื่อม UserId ลง DB)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 132:** `[Authorize]`  
  จำกัดสิทธิ์เฉพาะผู้ใช้ที่เข้าสู่ระบบแล้ว
* **บรรทัดที่ 133:** `public IActionResult Checkout()`  
  Action เมธอดแสดงหน้าสรุปคำสั่งซื้อและชำระเงินผ่าน HTTP GET
* **บรรทัดที่ 134:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 135:** `var cart = _repo.CurrentCart;`  
  ดึงตะกร้าสินค้าปัจจุบัน
* **บรรทัดที่ 136:** `if (!cart.Items.Any())`  
  ตรวจสอบว่ามีสินค้าหรือไม่
* **บรรทัดที่ 137:** `{`
* **บรรทัดที่ 138:** `return RedirectToAction(nameof(Index));`  
  ถ้าไม่มีสินค้า ส่งกลับไปหน้าตะกร้า
* **บรรทัดที่ 139:** `}`
* **บรรทัดที่ 140:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 141:** `ViewBag.PromptPayPayload = PromptPayHelper.GeneratePayload("0812345678", cart.GrandTotal);`  
  สร้างรหัส EMVCo PromptPay สำหรับยอดเงินสุทธิเพื่อนำไปแสดงเป็นภาพ QR Code
* **บรรทัดที่ 142:** `return View(cart);`  
  ส่งตะกร้าสินค้าไปเรนเดอร์ใน `Views/Cart/Checkout.cshtml`
* **บรรทัดที่ 143:** `}`  
  ปิดบล็อกเมธอด Checkout
* **บรรทัดที่ 144:** *(บรรทัดว่าง)*
* **บรรทัดที่ 145:** `// คำนวณค่าจัดส่งและยอดสุทธิรวมตามผู้ให้บริการขนส่งแบบ Real-Time (EMS ด่วนพิเศษ +30 บาท)`  
  คอมเมนต์
* **บรรทัดที่ 146:** `[HttpGet]`  
  ระบุว่ารับคำขอด้วย HTTP GET
* **บรรทัดที่ 147:** `public IActionResult GetCourierCalculation(string courier)`  
  Action เมธอดรับชื่อบริษัทขนส่งที่ผู้ใช้เลือกผ่าน AJAX
* **บรรทัดที่ 148:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 149:** `var cart = _repo.CurrentCart;`  
  ดึงตะกร้าสินค้า
* **บรรทัดที่ 150:** `var activePromos = _repo.GetValidPromotions();`  
  ดึงโปรโมชั่นที่เปิดใช้งาน
* **บรรทัดที่ 151:** `cart.Recalculate(activePromos);`  
  คำนวณโปรโมชั่นและส่วนลดใหม่
* **บรรทัดที่ 152:** *(บรรทัดว่าง)*
* **บรรทัดที่ 153:** `decimal surcharge = Cart.GetCourierSurcharge(courier);`  
  คำนวณค่าธรรมเนียมพิเศษของขนส่ง (EMS +30 บาท ส่วนขนส่งอื่น 0 บาท)
* **บรรทัดที่ 154:** `decimal shippingFee = cart.ShippingFee + surcharge;`  
  คำนวณค่าจัดส่งรวม
* **บรรทัดที่ 155:** `decimal grandTotal = cart.NetProductPrice + shippingFee;`  
  คำนวณยอดชำระสุทธิ
* **บรรทัดที่ 156:** `string payload = PromptPayHelper.GeneratePayload("0812345678", grandTotal);`  
  สร้าง PromptPay Payload ใหม่ที่ตรงตามยอดสุทธิ
* **บรรทัดที่ 157:** `string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=180x180&data={Uri.EscapeDataString(payload)}";`  
  สร้าง URL ลิงก์รูป QR Code ใหม่
* **บรรทัดที่ 158:** *(บรรทัดว่าง)*
* **บรรทัดที่ 159:** `return Json(new`  
  ส่งคืนข้อมูลเป็น JSON ให้หน้าบ้าน
* **บรรทัดที่ 160:** `{`
* **บรรทัดที่ 161:** `    success = true,`
* **บรรทัดที่ 162:** `    courier = courier,`
* **บรรทัดที่ 163:** `    baseShippingFee = cart.ShippingFee,`
* **บรรทัดที่ 164:** `    surcharge = surcharge,`
* **บรรทัดที่ 165:** `    shippingFee = shippingFee,`
* **บรรทัดที่ 166:** `    grandTotal = grandTotal,`
* **บรรทัดที่ 167:** `    qrUrl = qrUrl,`
* **บรรทัดที่ 168:** `    formattedShippingFee = shippingFee.ToString("N2"),`
* **บรรทัดที่ 169:** `    formattedGrandTotal = grandTotal.ToString("N2"),`
* **บรรทัดที่ 170:** `    isEms = surcharge > 0`
* **บรรทัดที่ 171:** `});`
* **บรรทัดที่ 172:** `}`  
  ปิดบล็อกเมธอด GetCourierCalculation
* **บรรทัดที่ 173:** *(บรรทัดว่าง)*
* **บรรทัดที่ 174:** `// หากมีการเปิดหน้า /Cart/SubmitOrder ด้วย GET (เช่น กดรีเฟรชหน้า หรือกด Back) ให้กลับไปหน้า Checkout อย่างถูกต้อง`  
  คอมเมนต์
* **บรรทัดที่ 175:** `[HttpGet]`  
  รับคำขอ HTTP GET
* **บรรทัดที่ 176:** `public IActionResult SubmitOrder()`  
  Action เมธอดป้องกันข้อผิดพลาดกรณีเข้าหน้า SubmitOrder ด้วย GET
* **บรรทัดที่ 177:** `{`
* **บรรทัดที่ 178:** `return RedirectToAction(nameof(Checkout));`  
  ส่งผู้ใช้กลับไปหน้า Checkout ทันที
* **บรรทัดที่ 179:** `}`  
  ปิดบล็อกเมธอด SubmitOrder [GET]
* **บรรทัดที่ 180:** *(บรรทัดว่าง)*
* **บรรทัดที่ 181:** `[Authorize]`  
  จำกัดสิทธิ์เฉพาะผู้ใช้ที่เข้าสู่ระบบ
* **บรรทัดที่ 182:** `[HttpPost]`  
  รับคำขอ HTTP POST
* **บรรทัดที่ 183:** `public async Task<IActionResult> SubmitOrder(`  
  Action เมธอดประมวลผลการสั่งซื้อ
* **บรรทัดที่ 184:** `    string customerName,`  
  ชื่อผู้รับ
* **บรรทัดที่ 185:** `    string customerPhone,`  
  เบอร์โทรศัพท์
* **บรรทัดที่ 186:** `    string shippingAddress,`  
  ที่อยู่จัดส่ง
* **บรรทัดที่ 187:** `    string paymentMethod = "โอนผ่านธนาคาร / PromptPay",`  
  วิธีชำระเงิน
* **บรรทัดที่ 188:** `    string courier = "Flash Express",`  
  บริษัทขนส่ง
* **บรรทัดที่ 189:** `    IFormFile? paymentSlip = null)`  
  ไฟล์รูปสลิปการโอนเงิน
* **บรรทัดที่ 190:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 191:** `try`  
  เปิดบล็อก try
* **บรรทัดที่ 192:** `{`
* **บรรทัดที่ 193:** `string? slipUrl = null;`  
  ตัวแปรเก็บ URL ของสลิป
* **บรรทัดที่ 194:** `if (paymentSlip != null && paymentSlip.Length > 0)`  
  ตรวจสอบว่ามีการแนบไฟล์สลิปมาหรือไม่
* **บรรทัดที่ 195:** `{`
* **บรรทัดที่ 196:** `var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(paymentSlip, _env.WebRootPath, "slips");`  
  บันทึกไฟล์รูปสลิปลงใน `wwwroot/uploads/slips`
* **บรรทัดที่ 197:** `if (uploadErr != null)`  
  หากบันทึกรูปไม่สำเร็จ
* **บรรทัดที่ 198:** `{`
* **บรรทัดที่ 199:** `TempData["ErrorMessage"] = uploadErr;`  
  บันทึกข้อความผิดพลาด
* **บรรทัดที่ 200:** `return RedirectToAction(nameof(Checkout));`  
  ส่งกลับหน้า Checkout
* **บรรทัดที่ 201:** `}`
* **บรรทัดที่ 202:** `slipUrl = uploadedUrl;`  
  กำหนดค่า URL สลิป
* **บรรทัดที่ 203:** `}`
* **บรรทัดที่ 204:** *(บรรทัดว่าง)*
* **บรรทัดที่ 205:** `var username = User.Identity?.Name ?? "";`  
  ดึงชื่อผู้ใช้ปัจจุบัน
* **บรรทัดที่ 206:** `var (newOrder, errorMessage) = _repo.CheckoutCurrentCart(`  
  เรียกกระบวนการสั่งซื้อหลักใน Repository
* **บรรทัดที่ 207:** `    username, customerName, customerPhone, shippingAddress, paymentMethod, slipUrl, courier);`  
  ตัดสต็อก, สร้างออเดอร์ลง DB, เคลียร์ตะกร้า และบันทึก Audit Log
* **บรรทัดที่ 208:** *(บรรทัดว่าง)*
* **บรรทัดที่ 209:** `if (newOrder == null)`  
  หากสั่งซื้อไม่สำเร็จ
* **บรรทัดที่ 210:** `{`
* **บรรทัดที่ 211:** `TempData["ErrorMessage"] = errorMessage ?? "ไม่สามารถดำเนินการสั่งซื้อได้";`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 212:** `return RedirectToAction(nameof(Checkout));`  
  กลับหน้า Checkout
* **บรรทัดที่ 213:** `}`
* **บรรทัดที่ 214:** *(บรรทัดว่าง)*
* **บรรทัดที่ 215:** `// SignalR: แจ้งเตือนฝ่ายคำสั่งซื้อ/บัญชี แบบ Real-Time ทันที (มี try-catch เพื่อป้องกันข้อผิดพลาดของ socket ไม่ให้กระทบการสั่งซื้อ)`  
  คอมเมนต์ Real-Time
* **บรรทัดที่ 216:** `try`  
  ครอบ try-catch ป้องกันข้อผิดพลาดของ Socket
* **บรรทัดที่ 217:** `{`
* **บรรทัดที่ 218:** `await _hubContext.Clients.All.SendAsync("ReceiveNewOrderAlert", newOrder.OrderId, newOrder.CustomerName, newOrder.GrandTotal);`  
  ส่งสัญญาณเตือนคำสั่งซื้อใหม่ไปยังฝ่ายจัดเตรียมสินค้า ฝ่ายบัญชี และผู้บริหาร
* **บรรทัดที่ 219:** *(บรรทัดว่าง)*
* **บรรทัดที่ 220:** `// ตรวจสอบสินค้าที่เหลือเพื่อเตือน Low Stock (Req 18)`  
  คอมเมนต์ Low Stock
* **บรรทัดที่ 221:** `var productIds = newOrder.Items.Select(i => i.ProductId).ToList();`  
  ดึงรายการรหัสสินค้าในคำสั่งซื้อ
* **บรรทัดที่ 222:** `var affectedProducts = _repo.Products.Where(p => productIds.Contains(p.Id)).ToList();`  
  ค้นหาสินค้าเหล่านั้นในฐานข้อมูล
* **บรรทัดที่ 223:** `foreach (var prod in affectedProducts)`  
  วนลูปตรวจสอบสินค้าทุกชิ้นที่ถูกตัดสต็อก
* **บรรทัดที่ 224:** `{`
* **บรรทัดที่ 225:** `if (prod.IsLowStock)`  
  ถ้าสต็อกต่ำกว่าเกณฑ์
* **บรรทัดที่ 226:** `{`
* **บรรทัดที่ 227:** `await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", prod.Name, prod.StockQuantity);`  
  ส่งสัญญาณเตือนภัย Low Stock Alert ไปยังหน้าจอฝ่ายคลังสินค้าทันที
* **บรรทัดที่ 228:** `}`
* **บรรทัดที่ 229:** `}`
* **บรรทัดที่ 230:** `}`
* **บรรทัดที่ 231:** `catch`
* **บรรทัดที่ 232:** `{`
* **บรรทัดที่ 233:** `// การส่ง SignalR ไม่ควรทำให้กระบวนการสั่งซื้อล้มเหลว`  
  คอมเมนต์
* **บรรทัดที่ 234:** `}`
* **บรรทัดที่ 235:** *(บรรทัดว่าง)*
* **บรรทัดที่ 236:** `return RedirectToAction("Orders", "Account", new { successOrderId = newOrder.OrderId });`  
  ส่งผู้ใช้ไปยังหน้าประวัติคำสั่งซื้อ (`AccountController.Orders`) พร้อมรหัสออเดอร์เพื่อแสดง Modal แสดงความยินดี
* **บรรทัดที่ 237:** `}`  
  ปิดบล็อก try
* **บรรทัดที่ 238:** `catch (Exception ex)`  
  ดักจับ Unhandled Exception
* **บรรทัดที่ 239:** `{`
* **บรรทัดที่ 240:** `TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการสั่งซื้อ: {ex.Message}";`
* **บรรทัดที่ 241:** `return RedirectToAction(nameof(Checkout));`
* **บรรทัดที่ 242:** `}`
* **บรรทัดที่ 243:** `}`  
  ปิดบล็อกเมธอด SubmitOrder [POST]
* **บรรทัดที่ 244:** `}`  
  ปิดบล็อกคลาส CartController
* **บรรทัดที่ 245:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 6. HomeController.cs (118 บรรทัด)

### โค้ดต้นฉบับเต็มของ HomeController.cs
```csharp
1: using Microsoft.AspNetCore.Mvc;
2: using CSI402_Project_final.Models;
3: using System.Linq;
4: 
5: namespace CSI402_Project_final.Controllers
6: {
7:     public class HomeController : Controller
8:     {
9:         private readonly PetShopRepository _repo;
10: 
11:         public HomeController(PetShopRepository repo)
12:         {
13:             _repo = repo;
14:         }
15: 
16:         // หน้าร้านหลัก พร้อมฟิลเตอร์แยกประเภทสัตว์ (Req 1), หมวดหมู่ของใช้/อาหาร, น้ำหนัก (Req 2), ช่วงวัย/โรค (Req 7, 9, 10)
17:         public IActionResult Index(
18:             PetCategory? petType = null,
19:             ProductCategory? category = null,
20:             AgeStage? ageStage = null,
21:             string? specialNeed = null,
22:             double? maxWeight = null,
23:             string? search = null)
24:         {
25:             var allCatalog = _repo.QueryProducts().ToList();
26: 
27:             // คำนวณจำนวนสินค้าแยกตามหมวดหมู่ (ตามประเภทสัตว์ที่เลือกอยู่)
28:             var baseForCategoryCount = petType.HasValue
29:                 ? allCatalog.Where(p => p.PetType == petType.Value).ToList()
30:                 : allCatalog;
31: 
32:             ViewBag.AllCategoryCount = baseForCategoryCount.Count;
33:             ViewBag.FoodCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Food);
34:             ViewBag.SupplyCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Supply);
35:             ViewBag.HygieneCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Hygiene);
36: 
37:             IEnumerable<Product> products = allCatalog;
38: 
39:             if (petType.HasValue)
40:                 products = products.Where(p => p.PetType == petType.Value);
41: 
42:             if (category.HasValue)
43:                 products = products.Where(p => p.Category == category.Value);
44: 
45:             if (ageStage.HasValue && ageStage.Value != AgeStage.AllAges)
46:                 products = products.Where(p => p.AgeStage == ageStage.Value || p.AgeStage == AgeStage.AllAges);
47: 
48:             if (!string.IsNullOrWhiteSpace(specialNeed))
49:                 products = products.Where(p => p.SpecialNeed.Contains(specialNeed, System.StringComparison.OrdinalIgnoreCase));
50: 
51:             if (maxWeight.HasValue)
52:                 products = products.Where(p => p.WeightKg <= maxWeight.Value);
53: 
54:             if (!string.IsNullOrWhiteSpace(search))
55:             {
56:                 string term = search.Trim();
57:                 products = products.Where(p =>
58:                     p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
59:                     p.Brand.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
60:                     p.SpecialNeed.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||
61:                     p.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase));
62:             }
63: 
64:             ViewBag.ActivePromotions = _repo.GetValidPromotions();
65:             ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
66:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
67:             ViewBag.SelectedPetType = petType;
68:             ViewBag.SelectedCategory = category;
69:             ViewBag.SelectedAgeStage = ageStage;
70:             ViewBag.SelectedSpecialNeed = specialNeed;
71:             ViewBag.SelectedMaxWeight = maxWeight;
72:             ViewBag.SelectedSearch = search;
73: 
74:             return View(products.ToList());
75:         }
76: 
77:         // หน้ารายละเอียดสินค้า (แสดง Layer ช่วงวัย & Social proof คนใส่ตะกร้า Req 7, 8, 10)
78:         public IActionResult Details(int id)
79:         {
80:             var allProducts = _repo.QueryProducts().ToList();
81:             var product = allProducts.FirstOrDefault(p => p.Id == id);
82:             if (product == null) return NotFound();
83: 
84:             // สินค้าแนะนำในหมวดหมู่และช่วงวัยเดียวกัน (รองรับทั้งอาหารและของใช้สัตว์เลี้ยง)
85:             var related = allProducts
86:                 .Where(p => p.Id != id && p.PetType == product.PetType && p.Category == product.Category)
87:                 .OrderByDescending(p => p.AgeStage == product.AgeStage)
88:                 .Take(4)
89:                 .ToList();
90: 
91:             if (related.Count < 4)
92:             {
93:                 var additional = allProducts
94:                     .Where(p => p.Id != id && !related.Any(r => r.Id == p.Id) && (p.Category == product.Category || p.PetType == product.PetType))
95:                     .Take(4 - related.Count);
96:                 related.AddRange(additional);
97:             }
98: 
99:             ViewBag.RelatedProducts = related;
100:             ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;
101:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
102: 
103:             return View(product);
104:         }
105: 
106:         [HttpPost]
107:         public IActionResult SwitchRole(UserRole role, string returnUrl = "/")
108:         {
109:             return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));
110:         }
111: 
112:         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
113:         public IActionResult Error()
114:         {
115:             return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
116:         }
117:     }
118: }
```

### คำอธิบายการทำงานของ HomeController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้าคลาส Controller, IActionResult, HttpPost, ResponseCache, View, NotFound
* **บรรทัดที่ 2:** `using CSI402_Project_final.Models;`  
  นำเข้า Model แคตตาล็อกสินค้า Product, PetCategory, ProductCategory, AgeStage, PetShopRepository
* **บรรทัดที่ 3:** `using System.Linq;`  
  นำเข้าคำสั่ง LINQ สำหรับการคัดกรองและเรียงลำดับสินค้า
* **บรรทัดที่ 4:** *(บรรทัดว่าง)*
* **บรรทัดที่ 5:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 6:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 7:** `public class HomeController : Controller`  
  ประกาศคลาส `HomeController` สำหรับควบคุมหน้าร้านค้าหลัก
* **บรรทัดที่ 8:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 9:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับคิวรีสินค้าในแคตตาล็อก
* **บรรทัดที่ 10:** *(บรรทัดว่าง)*
* **บรรทัดที่ 11:** `public HomeController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 12:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 13:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 14:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 15:** *(บรรทัดว่าง)*
* **บรรทัดที่ 16:** `// หน้าร้านหลัก พร้อมฟิลเตอร์แยกประเภทสัตว์ (Req 1), หมวดหมู่ของใช้/อาหาร, น้ำหนัก (Req 2), ช่วงวัย/โรค (Req 7, 9, 10)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 17:** `public IActionResult Index(`  
  Action เมธอดแสดงหน้าร้านค้าหลักผ่าน HTTP GET
* **บรรทัดที่ 18:** `    PetCategory? petType = null,`  
  ตัวกรองประเภทสัตว์เลี้ยง (สุนัข, แมว, อื่นๆ)
* **บรรทัดที่ 19:** `    ProductCategory? category = null,`  
  ตัวกรองหมวดหมู่สินค้า (อาหาร, ของใช้, สุขอนามัย)
* **บรรทัดที่ 20:** `    AgeStage? ageStage = null,`  
  ตัวกรองช่วงวัย (ลูกสัตว์, โตเต็มวัย, สูงวัย, ทุกช่วงวัย)
* **บรรทัดที่ 21:** `    string? specialNeed = null,`  
  ตัวกรองความต้องการพิเศษ/โรคเฉพาะ (เช่น ทำหมัน, บำรุงไต, บำรุงขน)
* **บรรทัดที่ 22:** `    double? maxWeight = null,`  
  ตัวกรองน้ำหนักแพ็กเกจสินค้าสูงสุด (กิโลกรัม)
* **บรรทัดที่ 23:** `    string? search = null)`  
  คำค้นหาข้อความ
* **บรรทัดที่ 24:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 25:** `var allCatalog = _repo.QueryProducts().ToList();`  
  ดึงรายการสินค้าทั้งหมดในแคตตาล็อก
* **บรรทัดที่ 26:** *(บรรทัดว่าง)*
* **บรรทัดที่ 27:** `// คำนวณจำนวนสินค้าแยกตามหมวดหมู่ (ตามประเภทสัตว์ที่เลือกอยู่)`  
  คอมเมนต์คำนวณจำนวนสินค้าในแต่ละแท็บ
* **บรรทัดที่ 28:** `var baseForCategoryCount = petType.HasValue`  
  ตรวจสอบว่ามีการเลือกประเภทสัตว์หรือไม่
* **บรรทัดที่ 29:** `    ? allCatalog.Where(p => p.PetType == petType.Value).ToList()`  
  ถ้าเลือกประเภทสัตว์ ให้ใช้รายการสินค้าเฉพาะสัตว์นั้นเป็นฐานในการนับ
* **บรรทัดที่ 30:** `    : allCatalog;`  
  ถ้าไม่เลือก ให้ใช้สินค้าทั้งหมดเป็นฐานในการนับ
* **บรรทัดที่ 31:** *(บรรทัดว่าง)*
* **บรรทัดที่ 32:** `ViewBag.AllCategoryCount = baseForCategoryCount.Count;`  
  จำนวนสินค้าทั้งหมดในประเภทสัตว์นั้น
* **บรรทัดที่ 33:** `ViewBag.FoodCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Food);`  
  จำนวนสินค้าหมวดอาหาร
* **บรรทัดที่ 34:** `ViewBag.SupplyCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Supply);`  
  จำนวนสินค้าหมวดของใช้สัตว์เลี้ยง
* **บรรทัดที่ 35:** `ViewBag.HygieneCount = baseForCategoryCount.Count(p => p.Category == ProductCategory.Hygiene);`  
  จำนวนสินค้าหมวดสุขอนามัย
* **บรรทัดที่ 36:** *(บรรทัดว่าง)*
* **บรรทัดที่ 37:** `IEnumerable<Product> products = allCatalog;`  
  เตรียมตัวแปรเริ่มต้นสำหรับคัดกรองสินค้า
* **บรรทัดที่ 38:** *(บรรทัดว่าง)*
* **บรรทัดที่ 39:** `if (petType.HasValue)`  
  ถ้าเลือกประเภทสัตว์
* **บรรทัดที่ 40:** `    products = products.Where(p => p.PetType == petType.Value);`  
  กรองตามประเภทสัตว์
* **บรรทัดที่ 41:** *(บรรทัดว่าง)*
* **บรรทัดที่ 42:** `if (category.HasValue)`  
  ถ้าเลือกหมวดหมู่
* **บรรทัดที่ 43:** `    products = products.Where(p => p.Category == category.Value);`  
  กรองตามหมวดหมู่
* **บรรทัดที่ 44:** *(บรรทัดว่าง)*
* **บรรทัดที่ 45:** `if (ageStage.HasValue && ageStage.Value != AgeStage.AllAges)`  
  ถ้าเลือกช่วงวัยเจาะจง
* **บรรทัดที่ 46:** `    products = products.Where(p => p.AgeStage == ageStage.Value || p.AgeStage == AgeStage.AllAges);`  
  กรองตามช่วงวัย และรวมสินค้าที่ทานได้ทุกช่วงวัยด้วย
* **บรรทัดที่ 47:** *(บรรทัดว่าง)*
* **บรรทัดที่ 48:** `if (!string.IsNullOrWhiteSpace(specialNeed))`  
  ถ้าเลือกความต้องการพิเศษ
* **บรรทัดที่ 49:** `    products = products.Where(p => p.SpecialNeed.Contains(specialNeed, System.StringComparison.OrdinalIgnoreCase));`  
  กรองตามคุณสมบัติการดูแลเฉพาะ
* **บรรทัดที่ 50:** *(บรรทัดว่าง)*
* **บรรทัดที่ 51:** `if (maxWeight.HasValue)`  
  ถ้ากำหนดน้ำหนักสูงสุด
* **บรรทัดที่ 52:** `    products = products.Where(p => p.WeightKg <= maxWeight.Value);`  
  กรองตามน้ำหนักแพ็กเกจ
* **บรรทัดที่ 53:** *(บรรทัดว่าง)*
* **บรรทัดที่ 54:** `if (!string.IsNullOrWhiteSpace(search))`  
  ถ้ามีคำค้นหา
* **บรรทัดที่ 55:** `{`
* **บรรทัดที่ 56:** `string term = search.Trim();`  
  ตัดช่องว่างคำค้น
* **บรรทัดที่ 57:** `products = products.Where(p =>`  
  ค้นหาจากชื่อสินค้า, แบรนด์, ความต้องการพิเศษ หรือรายละเอียด
* **บรรทัดที่ 58:** `    p.Name.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 59:** `    p.Brand.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 60:** `    p.SpecialNeed.Contains(term, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 61:** `    p.Description.Contains(term, System.StringComparison.OrdinalIgnoreCase));`
* **บรรทัดที่ 62:** `}`
* **บรรทัดที่ 63:** *(บรรทัดว่าง)*
* **บรรทัดที่ 64:** `ViewBag.ActivePromotions = _repo.GetValidPromotions();`  
  ส่งโปรโมชั่นที่เปิดใช้งานไปยัง View
* **บรรทัดที่ 65:** `ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;`  
  ส่งจำนวนสินค้าในตะกร้าปัจจุบัน
* **บรรทัดที่ 66:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทจำลองปัจจุบัน
* **บรรทัดที่ 67:** `ViewBag.SelectedPetType = petType;`  
  ส่งค่าตัวกรองสัตว์ที่เลือก
* **บรรทัดที่ 68:** `ViewBag.SelectedCategory = category;`  
  ส่งค่าตัวกรองหมวดหมู่ที่เลือก
* **บรรทัดที่ 69:** `ViewBag.SelectedAgeStage = ageStage;`  
  ส่งค่าตัวกรองช่วงวัยที่เลือก
* **บรรทัดที่ 70:** `ViewBag.SelectedSpecialNeed = specialNeed;`  
  ส่งค่าตัวกรองโรคเฉพาะที่เลือก
* **บรรทัดที่ 71:** `ViewBag.SelectedMaxWeight = maxWeight;`  
  ส่งค่าน้ำหนักสูงสุดที่เลือก
* **บรรทัดที่ 72:** `ViewBag.SelectedSearch = search;`  
  ส่งคำค้นหา
* **บรรทัดที่ 73:** *(บรรทัดว่าง)*
* **บรรทัดที่ 74:** `return View(products.ToList());`  
  ส่งรายการสินค้าที่ผ่านการคัดกรองไปเรนเดอร์ใน `Views/Home/Index.cshtml`
* **บรรทัดที่ 75:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 76:** *(บรรทัดว่าง)*
* **บรรทัดที่ 77:** `// หน้ารายละเอียดสินค้า (แสดง Layer ช่วงวัย & Social proof คนใส่ตะกร้า Req 7, 8, 10)`  
  คอมเมนต์
* **บรรทัดที่ 78:** `public IActionResult Details(int id)`  
  Action เมธอดรับรหัสสินค้าเพื่อแสดงหน้ารายละเอียดสินค้าผ่าน HTTP GET
* **บรรทัดที่ 79:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 80:** `var allProducts = _repo.QueryProducts().ToList();`  
  ดึงรายการสินค้าทั้งหมด
* **บรรทัดที่ 81:** `var product = allProducts.FirstOrDefault(p => p.Id == id);`  
  ค้นหาสินค้าตามรหัส ID
* **บรรทัดที่ 82:** `if (product == null) return NotFound();`  
  หากไม่พบ ให้ตอบกลับสถานะ HTTP 404 Not Found
* **บรรทัดที่ 83:** *(บรรทัดว่าง)*
* **บรรทัดที่ 84:** `// สินค้าแนะนำในหมวดหมู่และช่วงวัยเดียวกัน (รองรับทั้งอาหารและของใช้สัตว์เลี้ยง)`  
  คอมเมนต์ Recommender System
* **บรรทัดที่ 85:** `var related = allProducts`  
  เริ่มคำนวณหาสินค้าแนะนำ
* **บรรทัดที่ 86:** `    .Where(p => p.Id != id && p.PetType == product.PetType && p.Category == product.Category)`  
  เลือกสินค้าสัตว์ชนิดเดียวกันและหมวดเดียวกันที่ไม่ใช่ตัวเอง
* **บรรทัดที่ 87:** `    .OrderByDescending(p => p.AgeStage == product.AgeStage)`  
  จัดลำดับให้สินค้าที่มีช่วงวัยตรงกันขึ้นก่อน
* **บรรทัดที่ 88:** `    .Take(4)`  
  นำมา 4 รายการ
* **บรรทัดที่ 89:** `    .ToList();`
* **บรรทัดที่ 90:** *(บรรทัดว่าง)*
* **บรรทัดที่ 91:** `if (related.Count < 4)`  
  หากได้สินค้าไม่ครบ 4 รายการ
* **บรรทัดที่ 92:** `{`
* **บรรทัดที่ 93:** `var additional = allProducts`  
  ดึงสินค้าเพิ่มเติมที่มีหมวดหมู่หรือประเภทสัตว์ตรงกัน
* **บรรทัดที่ 94:** `    .Where(p => p.Id != id && !related.Any(r => r.Id == p.Id) && (p.Category == product.Category || p.PetType == product.PetType))`
* **บรรทัดที่ 95:** `    .Take(4 - related.Count);`  
  เอามาเติมส่วนที่ยังขาด
* **บรรทัดที่ 96:** `related.AddRange(additional);`  
  รวมเข้าใน List สินค้าแนะนำ
* **บรรทัดที่ 97:** `}`
* **บรรทัดที่ 98:** *(บรรทัดว่าง)*
* **บรรทัดที่ 99:** `ViewBag.RelatedProducts = related;`  
  ส่งรายการสินค้าแนะนำไปยัง View
* **บรรทัดที่ 100:** `ViewBag.CurrentCartCount = _repo.CurrentCart.TotalItemCount;`  
  ส่งยอดสินค้าในตะกร้า
* **บรรทัดที่ 101:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบัน
* **บรรทัดที่ 102:** *(บรรทัดว่าง)*
* **บรรทัดที่ 103:** `return View(product);`  
  ส่งสินค้าไปแสดงผลใน `Views/Home/Details.cshtml`
* **บรรทัดที่ 104:** `}`  
  ปิดบล็อกเมธอด Details
* **บรรทัดที่ 105:** *(บรรทัดว่าง)*
* **บรรทัดที่ 106:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 107:** `public IActionResult SwitchRole(UserRole role, string returnUrl = "/")`  
  Action เมธอดรับคำขอสลับบทบาทจาก Role Switcher Bar หน้าร้าน
* **บรรทัดที่ 108:** `{`
* **บรรทัดที่ 109:** `return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction(nameof(Index));`  
  ส่งกลับไปยัง URL หน้าเดิมอย่างปลอดภัย
* **บรรทัดที่ 110:** `}`  
  ปิดบล็อกเมธอด SwitchRole
* **บรรทัดที่ 111:** *(บรรทัดว่าง)*
* **บรรทัดที่ 112:** `[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]`  
  ปิดการแคชสำหรับหน้า Error Page เพื่อให้แสดงข้อมูลล่าสุดเสมอ
* **บรรทัดที่ 113:** `public IActionResult Error()`  
  Action เมธอดแสดงหน้าต่างแจ้งข้อผิดพลาดระบบ
* **บรรทัดที่ 114:** `{`
* **บรรทัดที่ 115:** `return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });`  
  ส่ง RequestId ไปเรนเดอร์ใน `Views/Shared/Error.cshtml`
* **บรรทัดที่ 116:** `}`  
  ปิดบล็อกเมธอด Error
* **บรรทัดที่ 117:** `}`  
  ปิดบล็อกคลาส HomeController
* **บรรทัดที่ 118:** `}`  
  ปิดบล็อกเนมสเปซ


---

# 7. InventoryController.cs (206 บรรทัด)

### โค้ดต้นฉบับเต็มของ InventoryController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Hosting;
3: using Microsoft.AspNetCore.Http;
4: using Microsoft.AspNetCore.Mvc;
5: using Microsoft.AspNetCore.SignalR;
6: using CSI402_Project_final.Helpers;
7: using CSI402_Project_final.Hubs;
8: using CSI402_Project_final.Models;
9: using System.Linq;
10: using System.Threading.Tasks;
11: 
12: namespace CSI402_Project_final.Controllers
13: {
14:     [Authorize(Roles = "InventoryStaff,Owner,Admin")]
15:     public class InventoryController : Controller
16:     {
17:         private readonly PetShopRepository _repo;
18:         private readonly IWebHostEnvironment _env;
19:         private readonly IHubContext<PetShopHub> _hubContext;
20: 
21:         public InventoryController(PetShopRepository repo, IWebHostEnvironment env, IHubContext<PetShopHub> hubContext)
22:         {
23:             _repo = repo;
24:             _env = env;
25:             _hubContext = hubContext;
26:         }
27: 
28:         // หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18) พร้อมตัวกรอง Low Stock / Out of Stock และกรองหมวดหมู่ของใช้/อาหาร
29:         public IActionResult Index(string? stockFilter = "all", string? categoryFilter = "all", string? search = null)
30:         {
31:             var allProducts = _repo.Products.OrderBy(p => p.StockQuantity).ToList();
32: 
33:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
34:             ViewBag.TotalProducts = allProducts.Count;
35:             ViewBag.LowStockCount = allProducts.Count(p => p.IsLowStock);
36:             ViewBag.OutOfStockCount = allProducts.Count(p => p.IsOutOfStock);
37:             ViewBag.NormalStockCount = allProducts.Count(p => !p.IsLowStock && !p.IsOutOfStock);
38: 
39:             ViewBag.FoodCount = allProducts.Count(p => p.Category == ProductCategory.Food);
40:             ViewBag.SupplyCount = allProducts.Count(p => p.Category == ProductCategory.Supply);
41:             ViewBag.HygieneCount = allProducts.Count(p => p.Category == ProductCategory.Hygiene);
42: 
43:             string normalizedFilter = (stockFilter ?? "all").Trim().ToLowerInvariant();
44:             string normalizedCategory = (categoryFilter ?? "all").Trim();
45:             IEnumerable<Product> filtered = allProducts;
46: 
47:             filtered = normalizedFilter switch
48:             {
49:                 "low" => filtered.Where(p => p.IsLowStock),
50:                 "out" => filtered.Where(p => p.IsOutOfStock),
51:                 "normal" => filtered.Where(p => !p.IsLowStock && !p.IsOutOfStock),
52:                 "alert" => filtered.Where(p => p.IsLowStock || p.IsOutOfStock),
53:                 _ => filtered
54:             };
55: 
56:             if (!string.Equals(normalizedCategory, "all", System.StringComparison.OrdinalIgnoreCase) &&
57:                 System.Enum.TryParse<ProductCategory>(normalizedCategory, true, out var parsedCat))
58:             {
59:                 filtered = filtered.Where(p => p.Category == parsedCat);
60:             }
61: 
62:             if (!string.IsNullOrWhiteSpace(search))
63:             {
64:                 string kw = search.Trim();
65:                 filtered = filtered.Where(p =>
66:                     p.Name.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
67:                     p.Brand.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||
68:                     p.SpecialNeed.Contains(kw, System.StringComparison.OrdinalIgnoreCase));
69:             }
70: 
71:             ViewBag.StockFilter = normalizedFilter;
72:             ViewBag.CategoryFilter = normalizedCategory;
73:             ViewBag.Search = search;
74: 
75:             return View(filtered.ToList());
76:         }
77: 
78:         // อัปเดตสต็อกรวดเร็ว (คงสถานะ Filter เดิมไว้)
79:         [HttpPost]
80:         public async Task<IActionResult> UpdateStock(int id, int adjustment, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)
81:         {
82:             var product = _repo.UpdateProductStock(id, adjustment);
83:             if (product != null)
84:             {
85:                 TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว";
86: 
87:                 // SignalR Real-Time Socket Broadcast: แจ้งเตือนไปยังทุกหน้าจอและลูกค้า
88:                 await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity);
89:                 if (product.IsLowStock)
90:                 {
91:                     await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);
92:                 }
93:             }
94:             return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });
95:         }
96: 
97:         // หน้าเพิ่ม/แก้ไขสินค้าแบบ GET (รองรับเมื่อเข้าผ่าน URL หรือรีเฟรชหน้าจอ) -> นำทางไปยังหน้าคลังและเปิด Modal เพิ่มสินค้าทันที
98:         [HttpGet]
99:         public IActionResult SaveProduct(int? id = null, string? role = null)
100:         {
101:             return RedirectToAction(nameof(Index), new { role, openCreate = id == null ? "true" : null, editId = id });
102:         }
103: 
104:         [HttpGet]
105:         public IActionResult CreateProduct(string? role = null)
106:         {
107:             return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
108:         }
109: 
110:         [HttpGet]
111:         public IActionResult AddProduct(string? role = null)
112:         {
113:             return RedirectToAction(nameof(Index), new { role, openCreate = "true" });
114:         }
115: 
116:         // เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ
117:         [HttpPost]
118:         public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile, string? role = null)
119:         {
120:             try
121:             {
122:                 if (imageFile != null && imageFile.Length > 0)
123:                 {
124:                     string webRoot = !string.IsNullOrEmpty(_env.WebRootPath)
125:                         ? _env.WebRootPath
126:                         : System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");
127: 
128:                     var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, webRoot, "products");
129:                     if (uploadErr != null)
130:                     {
131:                         TempData["ErrorMessage"] = uploadErr;
132:                         return RedirectToAction(nameof(Index), new { role, openCreate = model.Id == 0 ? "true" : null, editId = model.Id > 0 ? (int?)model.Id : null });
133:                     }
134:                     if (!string.IsNullOrEmpty(uploadedUrl))
135:                     {
136:                         model.ImageUrl = uploadedUrl;
137:                     }
138:                 }
139: 
140:                 if (_repo.SaveProduct(model, out var message))
141:                 {
142:                     TempData["SuccessMessage"] = message;
143: 
144:                     try
145:                     {
146:                         // SignalR Real-Time Socket Broadcast
147:                         await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", model.Id, model.StockQuantity);
148:                         if (model.IsLowStock)
149:                         {
150:                             await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", model.Name, model.StockQuantity);
151:                         }
152:                     }
153:                     catch
154:                     {
155:                         // ป้องกัน SignalR error ส่งผลกระทบต่อการบันทึกสินค้า
156:                     }
157:                 }
158:                 else
159:                 {
160:                     TempData["ErrorMessage"] = message;
161:                 }
162:             }
163:             catch (Exception ex)
164:             {
165:                 TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการบันทึกข้อมูลสินค้า: {ex.Message}";
166:             }
167: 
168:             return RedirectToAction(nameof(Index), new { role });
169:         }
170: 
171:         [HttpGet]
172:         public IActionResult DeleteProduct(string? role = null)
173:         {
174:             return RedirectToAction(nameof(Index), new { role });
175:         }
176: 
177:         [HttpPost]
178:         public async Task<IActionResult> DeleteProduct(int id, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)
179:         {
180:             try
181:             {
182:                 var (success, message) = _repo.DeleteProduct(id);
183:                 if (success)
184:                 {
185:                     TempData["SuccessMessage"] = message;
186:                     try
187:                     {
188:                         // SignalR Real-Time Socket Broadcast
189:                         await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", id, 0);
190:                     }
191:                     catch { }
192:                 }
193:                 else
194:                 {
195:                     TempData["ErrorMessage"] = message;
196:                 }
197:             }
198:             catch (Exception ex)
199:             {
200:                 TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบสินค้า: {ex.Message}";
201:             }
202: 
203:             return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });
204:         }
205:     }
206: }
```

### คำอธิบายการทำงานของ InventoryController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Hosting;`  
  นำเข้า `IWebHostEnvironment` สำหรับจัดการไดเรกทอรีจัดเก็บรูปภาพสินค้าใน `wwwroot`
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.Http;`  
  นำเข้า `IFormFile` สำหรับรับไฟล์อัปโหลดรูปภาพสินค้า
* **บรรทัดที่ 4:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult, HttpGet, HttpPost, RedirectToAction
* **บรรทัดที่ 5:** `using Microsoft.AspNetCore.SignalR;`  
  นำเข้า `IHubContext` สำหรับการบรอดแคสต์สต็อกคงเหลือและการแจ้งเตือน Low Stock Alert
* **บรรทัดที่ 6:** `using CSI402_Project_final.Helpers;`  
  นำเข้า `FileUploadHelper` สำหรับบันทึกรูปภาพสินค้า
* **บรรทัดที่ 7:** `using CSI402_Project_final.Hubs;`  
  นำเข้า `PetShopHub` สำหรับการเชื่อมต่อ SignalR WebSocket
* **บรรทัดที่ 8:** `using CSI402_Project_final.Models;`  
  นำเข้า Product, ProductCategory, PetShopRepository
* **บรรทัดที่ 9:** `using System.Linq;`  
  นำเข้าคำสั่ง LINQ สำหรับการจัดเรียงและคัดกรองสต็อก
* **บรรทัดที่ 10:** `using System.Threading.Tasks;`  
  นำเข้า Asynchronous Task
* **บรรทัดที่ 11:** *(บรรทัดว่าง)*
* **บรรทัดที่ 12:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 13:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 14:** `[Authorize(Roles = "InventoryStaff,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะพนักงานคลังสินค้า (InventoryStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
* **บรรทัดที่ 15:** `public class InventoryController : Controller`  
  ประกาศคลาส `InventoryController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 16:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 17:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการสินค้าและสต็อก
* **บรรทัดที่ 18:** `private readonly IWebHostEnvironment _env;`  
  ฟิลด์เก็บสภาพแวดล้อมโฮสต์สำหรับอ้างอิงโฟลเดอร์เซฟรูป
* **บรรทัดที่ 19:** `private readonly IHubContext<PetShopHub> _hubContext;`  
  ฟิลด์เก็บ SignalR Hub Context สำหรับบรอดแคสต์สต็อก
* **บรรทัดที่ 20:** *(บรรทัดว่าง)*
* **บรรทัดที่ 21:** `public InventoryController(PetShopRepository repo, IWebHostEnvironment env, IHubContext<PetShopHub> hubContext)`  
  Constructor รับการฉีด Dependency Injection
* **บรรทัดที่ 22:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 23:** `_repo = repo;`  
  ผูก Repository
* **บรรทัดที่ 24:** `_env = env;`  
  ผูก WebHostEnvironment
* **บรรทัดที่ 25:** `_hubContext = hubContext;`  
  ผูก SignalR Hub Context
* **บรรทัดที่ 26:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 27:** *(บรรทัดว่าง)*
* **บรรทัดที่ 28:** `// หน้าหลักคลังสินค้า: สต็อกสินค้า & แจ้งเตือนสินค้าต่ำกว่าระดับที่กำหนด (Req 18) พร้อมตัวกรอง Low Stock / Out of Stock และกรองหมวดหมู่ของใช้/อาหาร`  
  คอมเมนต์ระบุ Req 18
* **บรรทัดที่ 29:** `public IActionResult Index(string? stockFilter = "all", string? categoryFilter = "all", string? search = null)`  
  Action เมธอดแสดงแดชบอร์ดคลังสินค้าผ่าน HTTP GET
* **บรรทัดที่ 30:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 31:** `var allProducts = _repo.Products.OrderBy(p => p.StockQuantity).ToList();`  
  ดึงสินค้าทั้งหมด เรียงลำดับจากสต็อกน้อยไปมากเพื่อให้เห็นสินค้าใกล้หมดก่อน
* **บรรทัดที่ 32:** *(บรรทัดว่าง)*
* **บรรทัดที่ 33:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 34:** `ViewBag.TotalProducts = allProducts.Count;`  
  นับจำนวนสินค้าทั้งหมด
* **บรรทัดที่ 35:** `ViewBag.LowStockCount = allProducts.Count(p => p.IsLowStock);`  
  นับจำนวนสินค้าที่สต็อกต่ำกว่าเกณฑ์
* **บรรทัดที่ 36:** `ViewBag.OutOfStockCount = allProducts.Count(p => p.IsOutOfStock);`  
  นับจำนวนสินค้าที่หมดสต็อก
* **บรรทัดที่ 37:** `ViewBag.NormalStockCount = allProducts.Count(p => !p.IsLowStock && !p.IsOutOfStock);`  
  นับจำนวนสินค้าที่มีสต็อกปกติ
* **บรรทัดที่ 38:** *(บรรทัดว่าง)*
* **บรรทัดที่ 39:** `ViewBag.FoodCount = allProducts.Count(p => p.Category == ProductCategory.Food);`  
  จำนวนสินค้าหมวดอาหาร
* **บรรทัดที่ 40:** `ViewBag.SupplyCount = allProducts.Count(p => p.Category == ProductCategory.Supply);`  
  จำนวนสินค้าหมวดของใช้
* **บรรทัดที่ 41:** `ViewBag.HygieneCount = allProducts.Count(p => p.Category == ProductCategory.Hygiene);`  
  จำนวนสินค้าหมวดสุขอนามัย
* **บรรทัดที่ 42:** *(บรรทัดว่าง)*
* **บรรทัดที่ 43:** `string normalizedFilter = (stockFilter ?? "all").Trim().ToLowerInvariant();`  
  ปรับแต่งสตริงตัวกรองสต็อก
* **บรรทัดที่ 44:** `string normalizedCategory = (categoryFilter ?? "all").Trim();`  
  ปรับแต่งสตริงตัวกรองหมวดหมู่
* **บรรทัดที่ 45:** `IEnumerable<Product> filtered = allProducts;`  
  ตัวแปรคัดกรองเริ่มต้น
* **บรรทัดที่ 46:** *(บรรทัดว่าง)*
* **บรรทัดที่ 47:** `filtered = normalizedFilter switch`  
  ใช้ switch expression คัดกรองตามสถานะสต็อก
* **บรรทัดที่ 48:** `{`
* **บรรทัดที่ 49:** `    "low" => filtered.Where(p => p.IsLowStock),`  
  เฉพาะสินค้าใกล้หมด
* **บรรทัดที่ 50:** `    "out" => filtered.Where(p => p.IsOutOfStock),`  
  เฉพาะสินค้าหมดสต็อก
* **บรรทัดที่ 51:** `    "normal" => filtered.Where(p => !p.IsLowStock && !p.IsOutOfStock),`  
  เฉพาะสินค้าสต็อกปกติ
* **บรรทัดที่ 52:** `    "alert" => filtered.Where(p => p.IsLowStock || p.IsOutOfStock),`  
  รวมทั้งใกล้หมดและหมดสต็อก
* **บรรทัดที่ 53:** `    _ => filtered`  
  สินค้าทั้งหมด
* **บรรทัดที่ 54:** `};`
* **บรรทัดที่ 55:** *(บรรทัดว่าง)*
* **บรรทัดที่ 56:** `if (!string.Equals(normalizedCategory, "all", System.StringComparison.OrdinalIgnoreCase) &&`  
  ถ้าเลือกหมวดหมู่เฉพาะ
* **บรรทัดที่ 57:** `    System.Enum.TryParse<ProductCategory>(normalizedCategory, true, out var parsedCat))`
* **บรรทัดที่ 58:** `{`
* **บรรทัดที่ 59:** `    filtered = filtered.Where(p => p.Category == parsedCat);`  
  กรองตามหมวดหมู่
* **บรรทัดที่ 60:** `}`
* **บรรทัดที่ 61:** *(บรรทัดว่าง)*
* **บรรทัดที่ 62:** `if (!string.IsNullOrWhiteSpace(search))`  
  ถ้ามีคำค้นหา
* **บรรทัดที่ 63:** `{`
* **บรรทัดที่ 64:** `string kw = search.Trim();`
* **บรรทัดที่ 65:** `filtered = filtered.Where(p =>`  
  ค้นหาจากชื่อ, แบรนด์ หรือความต้องการพิเศษ
* **บรรทัดที่ 66:** `    p.Name.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 67:** `    p.Brand.Contains(kw, System.StringComparison.OrdinalIgnoreCase) ||`
* **บรรทัดที่ 68:** `    p.SpecialNeed.Contains(kw, System.StringComparison.OrdinalIgnoreCase));`
* **บรรทัดที่ 69:** `}`
* **บรรทัดที่ 70:** *(บรรทัดว่าง)*
* **บรรทัดที่ 71:** `ViewBag.StockFilter = normalizedFilter;`  
  ส่งค่าตัวกรองสต็อกไปยัง View
* **บรรทัดที่ 72:** `ViewBag.CategoryFilter = normalizedCategory;`  
  ส่งค่าตัวกรองหมวดหมู่ไปยัง View
* **บรรทัดที่ 73:** `ViewBag.Search = search;`  
  ส่งคำค้นหา
* **บรรทัดที่ 74:** *(บรรทัดว่าง)*
* **บรรทัดที่ 75:** `return View(filtered.ToList());`  
  ส่งรายการสินค้าไปแสดงผลใน `Views/Inventory/Index.cshtml`
* **บรรทัดที่ 76:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 77:** *(บรรทัดว่าง)*
* **บรรทัดที่ 78:** `// อัปเดตสต็อกรวดเร็ว (คงสถานะ Filter เดิมไว้)`  
  คอมเมนต์ Quick In/Out
* **บรรทัดที่ 79:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 80:** `public async Task<IActionResult> UpdateStock(int id, int adjustment, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)`  
  Action เมธอดรับรหัสสินค้าและจำนวนปรับสต็อก (+/-)
* **บรรทัดที่ 81:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 82:** `var product = _repo.UpdateProductStock(id, adjustment);`  
  สั่ง Repository ปรับยอดสต็อกและบันทึก Audit Log
* **บรรทัดที่ 83:** `if (product != null)`  
  ถ้าอัปเดตสำเร็จ
* **บรรทัดที่ 84:** `{`
* **บรรทัดที่ 85:** `TempData["SuccessMessage"] = $"อัปเดตสต็อกสินค้า '{product.Name}' เป็น {product.StockQuantity} ชิ้น เรียบร้อยแล้ว";`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 86:** *(บรรทัดว่าง)*
* **บรรทัดที่ 87:** `// SignalR Real-Time Socket Broadcast: แจ้งเตือนไปยังทุกหน้าจอและลูกค้า`  
  คอมเมนต์ SignalR
* **บรรทัดที่ 88:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", product.Id, product.StockQuantity);`  
  บรอดแคสต์สต็อกใหม่ไปยังทุกหน้าจอ
* **บรรทัดที่ 89:** `if (product.IsLowStock)`  
  ถ้าสต็อกต่ำกว่าเกณฑ์
* **บรรทัดที่ 90:** `{`
* **บรรทัดที่ 91:** `await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", product.Name, product.StockQuantity);`  
  ส่งสัญญาณเตือนภัย Low Stock Alert
* **บรรทัดที่ 92:** `}`
* **บรรทัดที่ 93:** `}`
* **บรรทัดที่ 94:** `return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });`  
  ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
* **บรรทัดที่ 95:** `}`  
  ปิดบล็อกเมธอด UpdateStock
* **บรรทัดที่ 96:** *(บรรทัดว่าง)*
* **บรรทัดที่ 97:** `// หน้าเพิ่ม/แก้ไขสินค้าแบบ GET (รองรับเมื่อเข้าผ่าน URL หรือรีเฟรชหน้าจอ) -> นำทางไปยังหน้าคลังและเปิด Modal เพิ่มสินค้าทันที`  
  คอมเมนต์
* **บรรทัดที่ 98:** `[HttpGet]`  
  รับคำขอ HTTP GET
* **บรรทัดที่ 99:** `public IActionResult SaveProduct(int? id = null, string? role = null)`  
  Action เมธอดรองรับการเข้าหน้านี้ด้วย GET
* **บรรทัดที่ 100:** `{`
* **บรรทัดที่ 101:** `return RedirectToAction(nameof(Index), new { role, openCreate = id == null ? "true" : null, editId = id });`  
  ส่งกลับไปหน้า Index พร้อมสั่งให้เปิด Modal สร้างหรือแก้ไขสินค้าทันที
* **บรรทัดที่ 102:** `}`  
  ปิดบล็อกเมธอด SaveProduct [GET]
* **บรรทัดที่ 103:** *(บรรทัดว่าง)*
* **บรรทัดที่ 104:** `[HttpGet]`
* **บรรทัดที่ 105:** `public IActionResult CreateProduct(string? role = null)`  
  ทางลัดเปิด Modal เพิ่มสินค้า
* **บรรทัดที่ 106:** `{`
* **บรรทัดที่ 107:** `return RedirectToAction(nameof(Index), new { role, openCreate = "true" });`
* **บรรทัดที่ 108:** `}`
* **บรรทัดที่ 109:** *(บรรทัดว่าง)*
* **บรรทัดที่ 110:** `[HttpGet]`
* **บรรทัดที่ 111:** `public IActionResult AddProduct(string? role = null)`  
  ทางลัดเปิด Modal เพิ่มสินค้า
* **บรรทัดที่ 112:** `{`
* **บรรทัดที่ 113:** `return RedirectToAction(nameof(Index), new { role, openCreate = "true" });`
* **บรรทัดที่ 114:** `}`
* **บรรทัดที่ 115:** *(บรรทัดว่าง)*
* **บรรทัดที่ 116:** `// เพิ่มหรือแก้ไขสินค้า (Req 6, 17) รองรับอัปโหลดรูปภาพและลิงก์รูปภาพ`  
  คอมเมนต์ระบุ Req 6, 17
* **บรรทัดที่ 117:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 118:** `public async Task<IActionResult> SaveProduct(Product model, IFormFile? imageFile, string? role = null)`  
  Action เมธอดรับข้อมูลสินค้าและไฟล์รูปภาพ
* **บรรทัดที่ 119:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 120:** `try`  
  เปิดบล็อก try
* **บรรทัดที่ 121:** `{`
* **บรรทัดที่ 122:** `if (imageFile != null && imageFile.Length > 0)`  
  ตรวจสอบว่ามีการอัปโหลดไฟล์รูปภาพหรือไม่
* **บรรทัดที่ 123:** `{`
* **บรรทัดที่ 124:** `string webRoot = !string.IsNullOrEmpty(_env.WebRootPath)`  
  กำหนดพาธโฟลเดอร์ wwwroot
* **บรรทัดที่ 125:** `    ? _env.WebRootPath`
* **บรรทัดที่ 126:** `    : System.IO.Path.Combine(_env.ContentRootPath, "wwwroot");`
* **บรรทัดที่ 127:** *(บรรทัดว่าง)*
* **บรรทัดที่ 128:** `var (uploadedUrl, uploadErr) = await FileUploadHelper.SaveImageAsync(imageFile, webRoot, "products");`  
  บันทึกรูปภาพสินค้าลงใน `wwwroot/uploads/products`
* **บรรทัดที่ 129:** `if (uploadErr != null)`  
  หากเกิดข้อผิดพลาดในการบันทึกรูป
* **บรรทัดที่ 130:** `{`
* **บรรทัดที่ 131:** `TempData["ErrorMessage"] = uploadErr;`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 132:** `return RedirectToAction(nameof(Index), new { role, openCreate = model.Id == 0 ? "true" : null, editId = model.Id > 0 ? (int?)model.Id : null });`  
  ส่งกลับหน้า Index พร้อมเปิด Modal เดิมค้างไว้
* **บรรทัดที่ 133:** `}`
* **บรรทัดที่ 134:** `if (!string.IsNullOrEmpty(uploadedUrl))`  
  ถ้าได้ URL รูปภาพ
* **บรรทัดที่ 135:** `{`
* **บรรทัดที่ 136:** `model.ImageUrl = uploadedUrl;`  
  ผูก URL รูปเข้ากับ Entity สินค้า
* **บรรทัดที่ 137:** `}`
* **บรรทัดที่ 138:** `}`
* **บรรทัดที่ 139:** *(บรรทัดว่าง)*
* **บรรทัดที่ 140:** `if (_repo.SaveProduct(model, out var message))`  
  สั่ง Repository บันทึกสินค้าใหม่หรืออัปเดตสินค้าเดิม
* **บรรทัดที่ 141:** `{`
* **บรรทัดที่ 142:** `TempData["SuccessMessage"] = message;`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 143:** *(บรรทัดว่าง)*
* **บรรทัดที่ 144:** `try`  
  ครอบ SignalR ด้วย try-catch
* **บรรทัดที่ 145:** `{`
* **บรรทัดที่ 146:** `// SignalR Real-Time Socket Broadcast`  
  คอมเมนต์
* **บรรทัดที่ 147:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", model.Id, model.StockQuantity);`  
  บรอดแคสต์สต็อกใหม่
* **บรรทัดที่ 148:** `if (model.IsLowStock)`  
  ตรวจสอบสถานะสินค้าใกล้หมด
* **บรรทัดที่ 149:** `{`
* **บรรทัดที่ 150:** `await _hubContext.Clients.All.SendAsync("ReceiveLowStockAlert", model.Name, model.StockQuantity);`  
  ส่งเตือน Low Stock Alert
* **บรรทัดที่ 151:** `}`
* **บรรทัดที่ 152:** `}`
* **บรรทัดที่ 153:** `catch`
* **บรรทัดที่ 154:** `{`
* **บรรทัดที่ 155:** `// ป้องกัน SignalR error ส่งผลกระทบต่อการบันทึกสินค้า`  
  คอมเมนต์
* **บรรทัดที่ 156:** `}`
* **บรรทัดที่ 157:** `}`
* **บรรทัดที่ 158:** `else`
* **บรรทัดที่ 159:** `{`
* **บรรทัดที่ 160:** `TempData["ErrorMessage"] = message;`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 161:** `}`
* **บรรทัดที่ 162:** `}`
* **บรรทัดที่ 163:** `catch (Exception ex)`  
  ดักจับ Exception
* **บรรทัดที่ 164:** `{`
* **บรรทัดที่ 165:** `TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการบันทึกข้อมูลสินค้า: {ex.Message}";`
* **บรรทัดที่ 166:** `}`
* **บรรทัดที่ 167:** *(บรรทัดว่าง)*
* **บรรทัดที่ 168:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 169:** `}`  
  ปิดบล็อกเมธอด SaveProduct [POST]
* **บรรทัดที่ 170:** *(บรรทัดว่าง)*
* **บรรทัดที่ 171:** `[HttpGet]`
* **บรรทัดที่ 172:** `public IActionResult DeleteProduct(string? role = null)`  
  ป้องกันการเข้า DeleteProduct ด้วย GET
* **บรรทัดที่ 173:** `{`
* **บรรทัดที่ 174:** `return RedirectToAction(nameof(Index), new { role });`
* **บรรทัดที่ 175:** `}`
* **บรรทัดที่ 176:** *(บรรทัดว่าง)*
* **บรรทัดที่ 177:** `[HttpPost]`  
  รับคำขอ HTTP POST
* **บรรทัดที่ 178:** `public async Task<IActionResult> DeleteProduct(int id, string? stockFilter = null, string? categoryFilter = null, string? search = null, string? role = null)`  
  Action เมธอดลบหรือระงับการจำหน่ายสินค้า
* **บรรทัดที่ 179:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 180:** `try`  
  เปิดบล็อก try
* **บรรทัดที่ 181:** `{`
* **บรรทัดที่ 182:** `var (success, message) = _repo.DeleteProduct(id);`  
  สั่ง Repository ลบสินค้าและบันทึก Audit Log
* **บรรทัดที่ 183:** `if (success)`
* **บรรทัดที่ 184:** `{`
* **บรรทัดที่ 185:** `TempData["SuccessMessage"] = message;`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 186:** `try`
* **บรรทัดที่ 187:** `{`
* **บรรทัดที่ 188:** `// SignalR Real-Time Socket Broadcast`  
  คอมเมนต์
* **บรรทัดที่ 189:** `await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", id, 0);`  
  บรอดแคสต์บอกทุกหน้าจอว่าสต็อกสินค้ารหัสนี้เป็น 0
* **บรรทัดที่ 190:** `}`
* **บรรทัดที่ 191:** `catch { }`
* **บรรทัดที่ 192:** `}`
* **บรรทัดที่ 193:** `else`
* **บรรทัดที่ 194:** `{`
* **บรรทัดที่ 195:** `TempData["ErrorMessage"] = message;`
* **บรรทัดที่ 196:** `}`
* **บรรทัดที่ 197:** `}`
* **บรรทัดที่ 198:** `catch (Exception ex)`
* **บรรทัดที่ 199:** `{`
* **บรรทัดที่ 200:** `TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบสินค้า: {ex.Message}";`
* **บรรทัดที่ 201:** `}`
* **บรรทัดที่ 202:** *(บรรทัดว่าง)*
* **บรรทัดที่ 203:** `return RedirectToAction(nameof(Index), new { stockFilter, categoryFilter, search, role });`  
  ส่งกลับหน้า Index พร้อมคงตัวกรองเดิมไว้
* **บรรทัดที่ 204:** `}`  
  ปิดบล็อกเมธอด DeleteProduct [POST]
* **บรรทัดที่ 205:** `}`  
  ปิดบล็อกคลาส InventoryController
* **บรรทัดที่ 206:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 8. MarketingController.cs (71 บรรทัด)

### โค้ดต้นฉบับเต็มของ MarketingController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using CSI402_Project_final.Models;
4: using System.Linq;
5: 
6: namespace CSI402_Project_final.Controllers
7: {
8:     [Authorize(Roles = "MarketingStaff,Owner,Admin")]
9:     public class MarketingController : Controller
10:     {
11:         private readonly PetShopRepository _repo;
12: 
13:         public MarketingController(PetShopRepository repo)
14:         {
15:             _repo = repo;
16:         }
17: 
18:         // หน้าแดชบอร์ดโปรโมชั่น & สถิติ (Req 5, 27)
19:         public IActionResult Index()
20:         {
21:             var promotions = _repo.Promotions;
22:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
23:             ViewBag.TotalPromotions = promotions.Count;
24:             ViewBag.ActivePromotionsCount = promotions.Count(p => p.IsValidNow);
25:             ViewBag.TotalUsageAllPromos = promotions.Sum(p => p.UsageCount);
26: 
27:             return View(promotions);
28:         }
29: 
30:         // สวิตช์เปิด/ปิดโปรโมชั่น (Req 5)
31:         [HttpPost]
32:         public IActionResult ToggleStatus(int id, string? role = null)
33:         {
34:             var promo = _repo.TogglePromotionStatus(id);
35:             if (promo != null)
36:             {
37:                 TempData["SuccessMessage"] = $"ปรับสถานะโปรโมชั่น '{promo.Title}' เป็น {(promo.IsActive ? "เปิดใช้งาน (Active)" : "ปิดชั่วคราว (Inactive)")} เรียบร้อยแล้ว";
38:             }
39:             return RedirectToAction(nameof(Index), new { role });
40:         }
41: 
42:         // สร้างหรือแก้ไขแคมเปญโปรโมชั่น (Req 5, 25, 26)
43:         [HttpPost]
44:         public IActionResult Save(Promotion model, string? role = null)
45:         {
46:             var (success, message) = _repo.SavePromotion(model);
47:             if (success)
48:                 TempData["SuccessMessage"] = message;
49:             else
50:                 TempData["ErrorMessage"] = message;
51: 
52:             return RedirectToAction(nameof(Index), new { role });
53:         }
54: 
55:         [HttpPost]
56:         public IActionResult Create(Promotion model, string? role = null) => Save(model, role);
57: 
58:         // ลบโปรโมชั่น (Req 5)
59:         [HttpPost]
60:         public IActionResult Delete(int id, string? role = null)
61:         {
62:             var (success, message) = _repo.DeletePromotion(id);
63:             if (success)
64:                 TempData["SuccessMessage"] = message;
65:             else
66:                 TempData["ErrorMessage"] = message;
67: 
68:             return RedirectToAction(nameof(Index), new { role });
69:         }
70:     }
71: }
```

### คำอธิบายการทำงานของ MarketingController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
* **บรรทัดที่ 3:** `using CSI402_Project_final.Models;`  
  นำเข้า Model โปรโมชั่น Promotion, PetShopRepository
* **บรรทัดที่ 4:** `using System.Linq;`  
  นำเข้าฟังก์ชัน LINQ เช่น `.Count()`, `.Sum()`
* **บรรทัดที่ 5:** *(บรรทัดว่าง)*
* **บรรทัดที่ 6:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 7:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 8:** `[Authorize(Roles = "MarketingStaff,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะฝ่ายการตลาด (MarketingStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
* **บรรทัดที่ 9:** `public class MarketingController : Controller`  
  ประกาศคลาส `MarketingController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 11:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการแคมเปญโปรโมชั่น
* **บรรทัดที่ 12:** *(บรรทัดว่าง)*
* **บรรทัดที่ 13:** `public MarketingController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 14:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 15:** `_repo = repo;`  
  ผูก Repository
* **บรรทัดที่ 16:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 17:** *(บรรทัดว่าง)*
* **บรรทัดที่ 18:** `// หน้าแดชบอร์ดโปรโมชั่น & สถิติ (Req 5, 27)`  
  คอมเมนต์ระบุ Req 5, 27
* **บรรทัดที่ 19:** `public IActionResult Index()`  
  Action เมธอดแสดงแดชบอร์ดการตลาดผ่าน HTTP GET
* **บรรทัดที่ 20:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 21:** `var promotions = _repo.Promotions;`  
  ดึงรายการแคมเปญโปรโมชั่นทั้งหมดจาก Repository
* **บรรทัดที่ 22:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 23:** `ViewBag.TotalPromotions = promotions.Count;`  
  นับจำนวนโปรโมชั่นทั้งหมดในระบบ
* **บรรทัดที่ 24:** `ViewBag.ActivePromotionsCount = promotions.Count(p => p.IsValidNow);`  
  นับจำนวนโปรโมชั่นที่กำลังเปิดใช้งานและยังไม่หมดอายุ
* **บรรทัดที่ 25:** `ViewBag.TotalUsageAllPromos = promotions.Sum(p => p.UsageCount);`  
  รวมจำนวนครั้งที่ลูกค้าได้รับสิทธิ์ส่วนลดจากทุกแคมเปญ
* **บรรทัดที่ 26:** *(บรรทัดว่าง)*
* **บรรทัดที่ 27:** `return View(promotions);`  
  ส่งรายการโปรโมชั่นไปแสดงผลใน `Views/Marketing/Index.cshtml`
* **บรรทัดที่ 28:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 29:** *(บรรทัดว่าง)*
* **บรรทัดที่ 30:** `// สวิตช์เปิด/ปิดโปรโมชั่น (Req 5)`  
  คอมเมนต์ระบุ Req 5
* **บรรทัดที่ 31:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 32:** `public IActionResult ToggleStatus(int id, string? role = null)`  
  Action เมธอดรับรหัสโปรโมชั่นที่ต้องการเปิดหรือปิดการใช้งาน
* **บรรทัดที่ 33:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 34:** `var promo = _repo.TogglePromotionStatus(id);`  
  สั่ง Repository สลับสถานะ `IsActive` (Active <-> Inactive) และบันทึก Audit Log
* **บรรทัดที่ 35:** `if (promo != null)`  
  ถ้าสลับสถานะสำเร็จ
* **บรรทัดที่ 36:** `{`
* **บรรทัดที่ 37:** `TempData["SuccessMessage"] = $"ปรับสถานะโปรโมชั่น '{promo.Title}' เป็น {(promo.IsActive ? "เปิดใช้งาน (Active)" : "ปิดชั่วคราว (Inactive)")} เรียบร้อยแล้ว";`  
  แสดงข้อความแจ้งเตือนสถานะใหม่
* **บรรทัดที่ 38:** `}`
* **บรรทัดที่ 39:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 40:** `}`  
  ปิดบล็อกเมธอด ToggleStatus
* **บรรทัดที่ 41:** *(บรรทัดว่าง)*
* **บรรทัดที่ 42:** `// สร้างหรือแก้ไขแคมเปญโปรโมชั่น (Req 5, 25, 26)`  
  คอมเมนต์ระบุ Req 5, 25, 26
* **บรรทัดที่ 43:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 44:** `public IActionResult Save(Promotion model, string? role = null)`  
  Action เมธอดรับข้อมูล Rule Builder ของโปรโมชั่น (ส่วนลด, ยอดขั้นต่ำ, สัตว์, หมวดหมู่, วันที่)
* **บรรทัดที่ 45:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 46:** `var (success, message) = _repo.SavePromotion(model);`  
  สั่ง Repository บันทึกโปรโมชั่นใหม่หรืออัปเดตโปรโมชั่นเดิม พร้อมบันทึก Audit Log
* **บรรทัดที่ 47:** `if (success)`
* **บรรทัดที่ 48:** `    TempData["SuccessMessage"] = message;`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 49:** `else`
* **บรรทัดที่ 50:** `    TempData["ErrorMessage"] = message;`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 51:** *(บรรทัดว่าง)*
* **บรรทัดที่ 52:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 53:** `}`  
  ปิดบล็อกเมธอด Save
* **บรรทัดที่ 54:** *(บรรทัดว่าง)*
* **บรรทัดที่ 55:** `[HttpPost]`
* **บรรทัดที่ 56:** `public IActionResult Create(Promotion model, string? role = null) => Save(model, role);`  
  Expression-bodied method ส่งต่อการทำงานไปยังเมธอด `Save` เพื่อสร้างโปรโมชั่นใหม่
* **บรรทัดที่ 57:** *(บรรทัดว่าง)*
* **บรรทัดที่ 58:** `// ลบโปรโมชั่น (Req 5)`  
  คอมเมนต์ระบุ Req 5
* **บรรทัดที่ 59:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 60:** `public IActionResult Delete(int id, string? role = null)`  
  Action เมธอดรับรหัสโปรโมชั่นที่ต้องการลบ
* **บรรทัดที่ 61:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 62:** `var (success, message) = _repo.DeletePromotion(id);`  
  สั่ง Repository ลบโปรโมชั่นและบันทึก Audit Log
* **บรรทัดที่ 63:** `if (success)`
* **บรรทัดที่ 64:** `    TempData["SuccessMessage"] = message;`  
  แจ้งข้อความสำเร็จ
* **บรรทัดที่ 65:** `else`
* **บรรทัดที่ 66:** `    TempData["ErrorMessage"] = message;`  
  แจ้งข้อผิดพลาด
* **บรรทัดที่ 67:** *(บรรทัดว่าง)*
* **บรรทัดที่ 68:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 69:** `}`  
  ปิดบล็อกเมธอด Delete
* **บรรทัดที่ 70:** `}`  
  ปิดบล็อกคลาส MarketingController
* **บรรทัดที่ 71:** `}`  
  ปิดบล็อกเนมสเปซ


---

# 9. OrderController.cs (49 บรรทัด)

### โค้ดต้นฉบับเต็มของ OrderController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using CSI402_Project_final.Models;
4: using System.Linq;
5: 
6: namespace CSI402_Project_final.Controllers
7: {
8:     [Authorize(Roles = "OrderStaff,Owner,Admin")]
9:     public class OrderController : Controller
10:     {
11:         private readonly PetShopRepository _repo;
12: 
13:         public OrderController(PetShopRepository repo)
14:         {
15:             _repo = repo;
16:         }
17: 
18:         // หน้าตรวจสอบคำสั่งซื้อและจัดเตรียมสินค้า
19:         public IActionResult Index(OrderStatus? status = null)
20:         {
21:             var allOrders = _repo.Orders;
22:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
23:             ViewBag.SelectedStatus = status;
24:             ViewBag.PendingCount = allOrders.Count(o => o.Status == OrderStatus.PaymentVerified);
25:             ViewBag.PreparingCount = allOrders.Count(o => o.Status == OrderStatus.Preparing);
26: 
27:             var filtered = status.HasValue
28:                 ? allOrders.Where(o => o.Status == status.Value).ToList()
29:                 : allOrders;
30: 
31:             return View(filtered);
32:         }
33: 
34:         // อัปเดตสถานะการเตรียมสินค้า (บันทึกลง DB)
35:         [HttpPost]
36:         public IActionResult UpdateStatus(string orderId, OrderStatus newStatus, string? role = null)
37:         {
38:             if (_repo.UpdateOrderStatus(orderId, newStatus))
39:             {
40:                 TempData["SuccessMessage"] = $"อัปเดตสถานะออเดอร์ {orderId} เป็น '{newStatus}' เรียบร้อยแล้ว";
41:             }
42:             if (!string.IsNullOrEmpty(role))
43:             {
44:                 return RedirectToAction(nameof(Index), new { role = role });
45:             }
46:             return RedirectToAction(nameof(Index));
47:         }
48:     }
49: }
```

### คำอธิบายการทำงานของ OrderController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
* **บรรทัดที่ 3:** `using CSI402_Project_final.Models;`  
  นำเข้า Order, OrderStatus, PetShopRepository
* **บรรทัดที่ 4:** `using System.Linq;`  
  นำเข้าคำสั่ง LINQ สำหรับการคัดกรองสถานะออเดอร์
* **บรรทัดที่ 5:** *(บรรทัดว่าง)*
* **บรรทัดที่ 6:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 7:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 8:** `[Authorize(Roles = "OrderStaff,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะฝ่ายคำสั่งซื้อและจัดเตรียมสินค้า (OrderStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
* **บรรทัดที่ 9:** `public class OrderController : Controller`  
  ประกาศคลาส `OrderController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 11:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการคำสั่งซื้อ
* **บรรทัดที่ 12:** *(บรรทัดว่าง)*
* **บรรทัดที่ 13:** `public OrderController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 14:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 15:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 16:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 17:** *(บรรทัดว่าง)*
* **บรรทัดที่ 18:** `// หน้าตรวจสอบคำสั่งซื้อและจัดเตรียมสินค้า`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 19:** `public IActionResult Index(OrderStatus? status = null)`  
  Action เมธอดแสดงคิวงานคำสั่งซื้อผ่าน HTTP GET รับตัวกรองสถานะออเดอร์
* **บรรทัดที่ 20:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 21:** `var allOrders = _repo.Orders;`  
  ดึงคำสั่งซื้อทั้งหมดในระบบ
* **บรรทัดที่ 22:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 23:** `ViewBag.SelectedStatus = status;`  
  ส่งสถานะที่เลือกคัดกรองไปยัง View
* **บรรทัดที่ 24:** `ViewBag.PendingCount = allOrders.Count(o => o.Status == OrderStatus.PaymentVerified);`  
  นับจำนวนออเดอร์ที่ชำระเงินเรียบร้อยแล้วและรอจัดเตรียม
* **บรรทัดที่ 25:** `ViewBag.PreparingCount = allOrders.Count(o => o.Status == OrderStatus.Preparing);`  
  นับจำนวนออเดอร์ที่กำลังจัดเตรียม/หยิบสินค้า
* **บรรทัดที่ 26:** *(บรรทัดว่าง)*
* **บรรทัดที่ 27:** `var filtered = status.HasValue`  
  ตรวจสอบว่ามีการกรองสถานะหรือไม่
* **บรรทัดที่ 28:** `    ? allOrders.Where(o => o.Status == status.Value).ToList()`  
  ถ้ามี ให้เลือกเฉพาะสถานะนั้น
* **บรรทัดที่ 29:** `    : allOrders;`  
  ถ้าไม่มี ให้แสดงทั้งหมด
* **บรรทัดที่ 30:** *(บรรทัดว่าง)*
* **บรรทัดที่ 31:** `return View(filtered);`  
  ส่งรายการคำสั่งซื้อไปเรนเดอร์ใน `Views/Order/Index.cshtml`
* **บรรทัดที่ 32:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 33:** *(บรรทัดว่าง)*
* **บรรทัดที่ 34:** `// อัปเดตสถานะการเตรียมสินค้า (บันทึกลง DB)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 35:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 36:** `public IActionResult UpdateStatus(string orderId, OrderStatus newStatus, string? role = null)`  
  Action เมธอดรับรหัสออเดอร์และสถานะใหม่ (เช่น เปลี่ยนเป็น `Preparing` เมื่อเริ่มแพ็กของ)
* **บรรทัดที่ 37:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 38:** `if (_repo.UpdateOrderStatus(orderId, newStatus))`  
  สั่ง Repository ปรับปรุงสถานะคำสั่งซื้อและบันทึก Audit Log
* **บรรทัดที่ 39:** `{`
* **บรรทัดที่ 40:** `TempData["SuccessMessage"] = $"อัปเดตสถานะออเดอร์ {orderId} เป็น '{newStatus}' เรียบร้อยแล้ว";`  
  แสดงข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 41:** `}`
* **บรรทัดที่ 42:** `if (!string.IsNullOrEmpty(role))`  
  ตรวจสอบว่ามีพารามิเตอร์ role หรือไม่
* **บรรทัดที่ 43:** `{`
* **บรรทัดที่ 44:** `return RedirectToAction(nameof(Index), new { role = role });`  
  ส่งกลับหน้า Index พร้อมส่ง role ต่อไป
* **บรรทัดที่ 45:** `}`
* **บรรทัดที่ 46:** `return RedirectToAction(nameof(Index));`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 47:** `}`  
  ปิดบล็อกเมธอด UpdateStatus
* **บรรทัดที่ 48:** `}`  
  ปิดบล็อกคลาส OrderController
* **บรรทัดที่ 49:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 10. OwnerController.cs (71 บรรทัด)

### โค้ดต้นฉบับเต็มของ OwnerController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using CSI402_Project_final.Models;
4: using System.Linq;
5: 
6: namespace CSI402_Project_final.Controllers
7: {
8:     [Authorize(Roles = "Owner")]
9:     public class OwnerController : Controller
10:     {
11:         private readonly PetShopRepository _repo;
12: 
13:         public OwnerController(PetShopRepository repo)
14:         {
15:             _repo = repo;
16:         }
17: 
18:         // หน้า Executive Dashboard ภาพรวมธุรกิจ (Owner)
19:         public IActionResult Index()
20:         {
21:             var orders = _repo.Orders;
22:             var products = _repo.Products;
23:             var promotions = _repo.Promotions;
24: 
25:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
26:             ViewBag.Financial = _repo.GetFinancialSummary();
27: 
28:             var productTypesById = products.ToDictionary(p => p.Id, p => p.PetType);
29:             int dogProductOrders = orders.SelectMany(o => o.Items)
30:                 .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Dog);
31:             int catProductOrders = orders.SelectMany(o => o.Items)
32:                 .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Cat);
33:             int otherProductOrders = orders.SelectMany(o => o.Items)
34:                 .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Other);
35: 
36:             ViewBag.DogOrderCount = dogProductOrders;
37:             ViewBag.CatOrderCount = catProductOrders;
38:             ViewBag.OtherOrderCount = otherProductOrders;
39: 
40:             // สัดส่วนสินค้าในคลังและยอดสั่งซื้อแยกตามหมวดหมู่ (อาหาร / ของใช้สัตว์เลี้ยง / สุขอนามัย)
41:             ViewBag.FoodCatalogCount = products.Count(p => p.Category == ProductCategory.Food);
42:             ViewBag.SupplyCatalogCount = products.Count(p => p.Category == ProductCategory.Supply);
43:             ViewBag.HygieneCatalogCount = products.Count(p => p.Category == ProductCategory.Hygiene);
44: 
45:             var productCatById = products.ToDictionary(p => p.Id, p => p.Category);
46:             ViewBag.FoodOrderCount = orders.SelectMany(o => o.Items)
47:                 .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Food);
48:             ViewBag.SupplyOrderCount = orders.SelectMany(o => o.Items)
49:                 .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Supply);
50:             ViewBag.HygieneOrderCount = orders.SelectMany(o => o.Items)
51:                 .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Hygiene);
52: 
53:             var topProducts = products
54:                 .OrderByDescending(p => p.CartAddCount)
55:                 .Take(5)
56:                 .ToList();
57:             ViewBag.TopProducts = topProducts;
58: 
59:             var topPromotions = promotions
60:                 .OrderByDescending(p => p.UsageCount)
61:                 .Take(4)
62:                 .ToList();
63:             ViewBag.TopPromotions = topPromotions;
64: 
65:             ViewBag.TotalOrderCount = orders.Count;
66:             ViewBag.RecentOrders = orders.Take(5).ToList();
67: 
68:             return View();
69:         }
70:     }
71: }
```

### คำอธิบายการทำงานของ OwnerController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = "Owner")]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult
* **บรรทัดที่ 3:** `using CSI402_Project_final.Models;`  
  นำเข้า Data Model ของระบบ Order, Product, PetCategory, ProductCategory, PetShopRepository
* **บรรทัดที่ 4:** `using System.Linq;`  
  นำเข้า LINQ สำหรับวิเคราะห์ข้อมูลสถิติ เช่น `.ToDictionary()`, `.SelectMany()`, `.Count()`
* **บรรทัดที่ 5:** *(บรรทัดว่าง)*
* **บรรทัดที่ 6:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 7:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 8:** `[Authorize(Roles = "Owner")]`  
  จำกัดสิทธิ์ระดับสูงสุด เฉพาะผู้บริหารสูงสุด (Owner) เท่านั้น
* **บรรทัดที่ 9:** `public class OwnerController : Controller`  
  ประกาศคลาส `OwnerController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 11:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับเข้าถึงข้อมูลรวมทั้งระบบ
* **บรรทัดที่ 12:** *(บรรทัดว่าง)*
* **บรรทัดที่ 13:** `public OwnerController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 14:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 15:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 16:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 17:** *(บรรทัดว่าง)*
* **บรรทัดที่ 18:** `// หน้า Executive Dashboard ภาพรวมธุรกิจ (Owner)`  
  คอมเมนต์อธิบายแดชบอร์ดผู้บริหาร
* **บรรทัดที่ 19:** `public IActionResult Index()`  
  Action เมธอดแสดงกระดาน BI Dashboard ผ่าน HTTP GET
* **บรรทัดที่ 20:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 21:** `var orders = _repo.Orders;`  
  ดึงคำสั่งซื้อทั้งหมดในระบบ
* **บรรทัดที่ 22:** `var products = _repo.Products;`  
  ดึงสินค้าทั้งหมดในระบบ
* **บรรทัดที่ 23:** `var promotions = _repo.Promotions;`  
  ดึงโปรโมชั่นทั้งหมดในระบบ
* **บรรทัดที่ 24:** *(บรรทัดว่าง)*
* **บรรทัดที่ 25:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 26:** `ViewBag.Financial = _repo.GetFinancialSummary();`  
  ดึงสรุปบัญชีการเงิน (ยอดขายรวม, กำไรสุทธิ, รายจ่ายรวม, คืนเงินรวม) ใส่ใน ViewBag
* **บรรทัดที่ 27:** *(บรรทัดว่าง)*
* **บรรทัดที่ 28:** `var productTypesById = products.ToDictionary(p => p.Id, p => p.PetType);`  
  สร้างพจนานุกรม Dictionary แมป ProductId กับประเภทสัตว์เลี้ยง เพื่อความรวดเร็วในการวิเคราะห์
* **บรรทัดที่ 29:** `int dogProductOrders = orders.SelectMany(o => o.Items)`  
  แตกรายการสินค้าทั้งหมดในทุกออเดอร์
* **บรรทัดที่ 30:** `    .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Dog);`  
  นับจำนวนยอดสั่งซื้อที่เป็นสินค้าสุนัข
* **บรรทัดที่ 31:** `int catProductOrders = orders.SelectMany(o => o.Items)`
* **บรรทัดที่ 32:** `    .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Cat);`  
  นับจำนวนยอดสั่งซื้อที่เป็นสินค้าแมว
* **บรรทัดที่ 33:** `int otherProductOrders = orders.SelectMany(o => o.Items)`
* **บรรทัดที่ 34:** `    .Count(i => productTypesById.TryGetValue(i.ProductId, out var pt) && pt == PetCategory.Other);`  
  นับจำนวนยอดสั่งซื้อที่เป็นสินค้าสัตว์เลี้ยงอื่นๆ
* **บรรทัดที่ 35:** *(บรรทัดว่าง)*
* **บรรทัดที่ 36:** `ViewBag.DogOrderCount = dogProductOrders;`  
  ส่งสถิติสินค้าสุนัขไปยัง View
* **บรรทัดที่ 37:** `ViewBag.CatOrderCount = catProductOrders;`  
  ส่งสถิติสินค้าแมวไปยัง View
* **บรรทัดที่ 38:** `ViewBag.OtherOrderCount = otherProductOrders;`  
  ส่งสถิติสินค้าสัตว์อื่นๆ ไปยัง View
* **บรรทัดที่ 39:** *(บรรทัดว่าง)*
* **บรรทัดที่ 40:** `// สัดส่วนสินค้าในคลังและยอดสั่งซื้อแยกตามหมวดหมู่ (อาหาร / ของใช้สัตว์เลี้ยง / สุขอนามัย)`  
  คอมเมนต์วิเคราะห์หมวดหมู่
* **บรรทัดที่ 41:** `ViewBag.FoodCatalogCount = products.Count(p => p.Category == ProductCategory.Food);`  
  นับสินค้าหมวดอาหารในแคตตาล็อก
* **บรรทัดที่ 42:** `ViewBag.SupplyCatalogCount = products.Count(p => p.Category == ProductCategory.Supply);`  
  นับสินค้าหมวดของใช้ในแคตตาล็อก
* **บรรทัดที่ 43:** `ViewBag.HygieneCatalogCount = products.Count(p => p.Category == ProductCategory.Hygiene);`  
  นับสินค้าหมวดสุขอนามัยในแคตตาล็อก
* **บรรทัดที่ 44:** *(บรรทัดว่าง)*
* **บรรทัดที่ 45:** `var productCatById = products.ToDictionary(p => p.Id, p => p.Category);`  
  สร้าง Dictionary แมป ProductId กับหมวดหมู่สินค้า
* **บรรทัดที่ 46:** `ViewBag.FoodOrderCount = orders.SelectMany(o => o.Items)`  
  นับยอดสั่งซื้อหมวดอาหาร
* **บรรทัดที่ 47:** `    .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Food);`
* **บรรทัดที่ 48:** `ViewBag.SupplyOrderCount = orders.SelectMany(o => o.Items)`  
  นับยอดสั่งซื้อหมวดของใช้สัตว์เลี้ยง
* **บรรทัดที่ 49:** `    .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Supply);`
* **บรรทัดที่ 50:** `ViewBag.HygieneOrderCount = orders.SelectMany(o => o.Items)`  
  นับยอดสั่งซื้อหมวดสุขอนามัย
* **บรรทัดที่ 51:** `    .Count(i => productCatById.TryGetValue(i.ProductId, out var cat) && cat == ProductCategory.Hygiene);`
* **บรรทัดที่ 52:** *(บรรทัดว่าง)*
* **บรรทัดที่ 53:** `var topProducts = products`  
  เริ่มจัดอันดับสินค้าขายดี/ยอดนิยม
* **บรรทัดที่ 54:** `    .OrderByDescending(p => p.CartAddCount)`  
  เรียงลำดับจากจำนวนครั้งที่ถูกหยิบใส่ตะกร้ามากที่สุด
* **บรรทัดที่ 55:** `    .Take(5)`  
  นำมา 5 อันดับแรก
* **บรรทัดที่ 56:** `    .ToList();`
* **บรรทัดที่ 57:** `ViewBag.TopProducts = topProducts;`  
  ส่งรายการสินค้าขายดีไปยัง View
* **บรรทัดที่ 58:** *(บรรทัดว่าง)*
* **บรรทัดที่ 59:** `var topPromotions = promotions`  
  เริ่มจัดอันดับโปรโมชั่นยอดนิยม
* **บรรทัดที่ 60:** `    .OrderByDescending(p => p.UsageCount)`  
  เรียงตามจำนวนครั้งที่ถูกใช้งาน
* **บรรทัดที่ 61:** `    .Take(4)`  
  นำมา 4 อันดับแรก
* **บรรทัดที่ 62:** `    .ToList();`
* **บรรทัดที่ 63:** `ViewBag.TopPromotions = topPromotions;`  
  ส่งรายการโปรโมชั่นยอดนิยมไปยัง View
* **บรรทัดที่ 64:** *(บรรทัดว่าง)*
* **บรรทัดที่ 65:** `ViewBag.TotalOrderCount = orders.Count;`  
  นับจำนวนคำสั่งซื้อรวมทั้งหมด
* **บรรทัดที่ 66:** `ViewBag.RecentOrders = orders.Take(5).ToList();`  
  ดึงคำสั่งซื้อล่าสุด 5 รายการ
* **บรรทัดที่ 67:** *(บรรทัดว่าง)*
* **บรรทัดที่ 68:** `return View();`  
  เรนเดอร์หน้า `Views/Owner/Index.cshtml`
* **บรรทัดที่ 69:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 70:** `}`  
  ปิดบล็อกคลาส OwnerController
* **บรรทัดที่ 71:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 11. ShippingController.cs (59 บรรทัด)

### โค้ดต้นฉบับเต็มของ ShippingController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using Microsoft.AspNetCore.SignalR;
4: using CSI402_Project_final.Models;
5: using CSI402_Project_final.Hubs;
6: using System;
7: using System.Linq;
8: using System.Threading.Tasks;
9: 
10: namespace CSI402_Project_final.Controllers
11: {
12:     [Authorize(Roles = "ShippingStaff,Owner,Admin")]
13:     public class ShippingController : Controller
14:     {
15:         private readonly PetShopRepository _repo;
16:         private readonly IHubContext<PetShopHub> _hubContext;
17: 
18:         public ShippingController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)
19:         {
20:             _repo = repo;
21:             _hubContext = hubContext;
22:         }
23: 
24:         // หน้าจัดการการจัดส่งและพัสดุ (เฉพาะออเดอร์ที่ฝ่ายคำสั่งซื้อแพ็กเสร็จส่งต่องานมาให้แล้ว)
25:         public IActionResult Index()
26:         {
27:             var allOrders = _repo.Orders;
28:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
29:             // พัสดุรอจัดส่ง: ออเดอร์ที่ฝ่าย Order Staff จัดเตรียม/แพ็กเสร็จแล้ว (Preparing)
30:             ViewBag.ReadyToShipCount = allOrders.Count(o => o.Status == OrderStatus.Preparing);
31:             ViewBag.ShippedTodayCount = allOrders.Count(o =>
32:                 o.Status == OrderStatus.Shipped &&
33:                 !string.IsNullOrWhiteSpace(o.TrackingNumber) &&
34:                 (!o.ShippedDate.HasValue || o.ShippedDate.Value.Date == DateTime.Today));
35: 
36:             var orders = allOrders
37:                 .Where(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Shipped)
38:                 .OrderBy(o => o.Status == OrderStatus.Preparing ? 0 : 1)
39:                 .ThenByDescending(o => o.OrderDate)
40:                 .ToList();
41: 
42:             return View(orders);
43:         }
44: 
45:         // บันทึกเลขพัสดุและน้ำหนักจริง (Req 4, 14, 16)
46:         [HttpPost]
47:         public async Task<IActionResult> DispatchOrder(string orderId, string courier, string trackingNumber, double actualWeightKg, string? role = null)
48:         {
49:             if (_repo.DispatchOrder(orderId, courier, trackingNumber, actualWeightKg))
50:             {
51:                 // SignalR: แจ้งเตือน Real-Time ว่าพัสดุจัดส่งแล้ว
52:                 await _hubContext.Clients.All.SendAsync("ReceiveOrderShippedAlert", orderId, trackingNumber, courier);
53: 
54:                 TempData["SuccessMessage"] = $"บันทึกเลขพัสดุ {trackingNumber} สำหรับคำสั่งซื้อ {orderId} และเปลี่ยนสถานะเป็น 'จัดส่งแล้ว' เรียบร้อยแล้ว";
55:             }
56:             return RedirectToAction(nameof(Index), new { role });
57:         }
58:     }
59: }
```

### คำอธิบายการทำงานของ ShippingController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
* **บรรทัดที่ 3:** `using Microsoft.AspNetCore.SignalR;`  
  นำเข้า `IHubContext` สำหรับส่งการแจ้งเตือน Real-Time เมื่อพัสดุจัดส่งแล้ว
* **บรรทัดที่ 4:** `using CSI402_Project_final.Models;`  
  นำเข้า Order, OrderStatus, PetShopRepository
* **บรรทัดที่ 5:** `using CSI402_Project_final.Hubs;`  
  นำเข้า `PetShopHub` สำหรับการเชื่อมต่อ WebSocket
* **บรรทัดที่ 6:** `using System;`  
  นำเข้าคลาสระบบพื้นฐาน เช่น `DateTime`
* **บรรทัดที่ 7:** `using System.Linq;`  
  นำเข้าฟังก์ชัน LINQ เช่น `.Where()`, `.OrderBy()`, `.Count()`
* **บรรทัดที่ 8:** `using System.Threading.Tasks;`  
  นำเข้า Task สำหรับ Asynchronous SignalR
* **บรรทัดที่ 9:** *(บรรทัดว่าง)*
* **บรรทัดที่ 10:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 11:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 12:** `[Authorize(Roles = "ShippingStaff,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะฝ่ายจัดส่ง (ShippingStaff), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
* **บรรทัดที่ 13:** `public class ShippingController : Controller`  
  ประกาศคลาส `ShippingController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 14:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 15:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการคำสั่งซื้อและการจัดส่ง
* **บรรทัดที่ 16:** `private readonly IHubContext<PetShopHub> _hubContext;`  
  ฟิลด์เก็บ SignalR Hub Context สำหรับบรอดแคสต์สถานะพัสดุ
* **บรรทัดที่ 17:** *(บรรทัดว่าง)*
* **บรรทัดที่ 18:** `public ShippingController(PetShopRepository repo, IHubContext<PetShopHub> hubContext)`  
  Constructor รับ Repository และ HubContext ผ่าน Dependency Injection
* **บรรทัดที่ 19:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 20:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 21:** `_hubContext = hubContext;`  
  ผูก SignalR Hub เข้ากับตัวแปรภายใน
* **บรรทัดที่ 22:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 23:** *(บรรทัดว่าง)*
* **บรรทัดที่ 24:** `// หน้าจัดการการจัดส่งและพัสดุ (เฉพาะออเดอร์ที่ฝ่ายคำสั่งซื้อแพ็กเสร็จส่งต่องานมาให้แล้ว)`  
  คอมเมนต์อธิบาย
* **บรรทัดที่ 25:** `public IActionResult Index()`  
  Action เมธอดแสดงหน้าคิวงานพัสดุผ่าน HTTP GET
* **บรรทัดที่ 26:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 27:** `var allOrders = _repo.Orders;`  
  ดึงคำสั่งซื้อทั้งหมด
* **บรรทัดที่ 28:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 29:** `// พัสดุรอจัดส่ง: ออเดอร์ที่ฝ่าย Order Staff จัดเตรียม/แพ็กเสร็จแล้ว (Preparing)`  
  คอมเมนต์
* **บรรทัดที่ 30:** `ViewBag.ReadyToShipCount = allOrders.Count(o => o.Status == OrderStatus.Preparing);`  
  นับจำนวนพัสดุที่แพ็กเสร็จและรอการจัดส่ง
* **บรรทัดที่ 31:** `ViewBag.ShippedTodayCount = allOrders.Count(o =>`  
  นับจำนวนพัสดุที่จัดส่งแล้วในวันนี้
* **บรรทัดที่ 32:** `    o.Status == OrderStatus.Shipped &&`  
  สถานะจัดส่งแล้ว
* **บรรทัดที่ 33:** `    !string.IsNullOrWhiteSpace(o.TrackingNumber) &&`  
  มีเลขพัสดุ
* **บรรทัดที่ 34:** `    (!o.ShippedDate.HasValue || o.ShippedDate.Value.Date == DateTime.Today));`  
  วันที่จัดส่งเป็นวันนี้
* **บรรทัดที่ 35:** *(บรรทัดว่าง)*
* **บรรทัดที่ 36:** `var orders = allOrders`  
  คัดกรองรายการออเดอร์
* **บรรทัดที่ 37:** `    .Where(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Shipped)`  
  เลือกเฉพาะออเดอร์ที่กำลังเตรียมส่งหรือจัดส่งแล้ว
* **บรรทัดที่ 38:** `    .OrderBy(o => o.Status == OrderStatus.Preparing ? 0 : 1)`  
  จัดให้ออเดอร์ที่รอส่ง (`Preparing`) อยู่ด้านบนสุด
* **บรรทัดที่ 39:** `    .ThenByDescending(o => o.OrderDate)`  
  ตามด้วยเรียงจากวันที่สั่งซื้อล่าสุด
* **บรรทัดที่ 40:** `    .ToList();`
* **บรรทัดที่ 41:** *(บรรทัดว่าง)*
* **บรรทัดที่ 42:** `return View(orders);`  
  ส่งรายการพัสดุไปแสดงผลใน `Views/Shipping/Index.cshtml`
* **บรรทัดที่ 43:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 44:** *(บรรทัดว่าง)*
* **บรรทัดที่ 45:** `// บันทึกเลขพัสดุและน้ำหนักจริง (Req 4, 14, 16)`  
  คอมเมนต์ระบุ Req 4, 14, 16
* **บรรทัดที่ 46:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 47:** `public async Task<IActionResult> DispatchOrder(string orderId, string courier, string trackingNumber, double actualWeightKg, string? role = null)`  
  Action เมธอดรับรหัสออเดอร์, บริษัทขนส่ง, หมายเลขพัสดุ และน้ำหนักชั่งจริง
* **บรรทัดที่ 48:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 49:** `if (_repo.DispatchOrder(orderId, courier, trackingNumber, actualWeightKg))`  
  สั่ง Repository บันทึกข้อมูลขนส่ง, ปรับสถานะเป็น `Shipped`, บันทึกเวลาจัดส่ง และลง Audit Log
* **บรรทัดที่ 50:** `{`
* **บรรทัดที่ 51:** `// SignalR: แจ้งเตือน Real-Time ว่าพัสดุจัดส่งแล้ว`  
  คอมเมนต์ SignalR
* **บรรทัดที่ 52:** `await _hubContext.Clients.All.SendAsync("ReceiveOrderShippedAlert", orderId, trackingNumber, courier);`  
  บรอดแคสต์แจ้งเตือน Real-Time ว่าพัสดุถูกจัดส่งแล้วพร้อมส่งเลข Tracking
* **บรรทัดที่ 53:** *(บรรทัดว่าง)*
* **บรรทัดที่ 54:** `TempData["SuccessMessage"] = $"บันทึกเลขพัสดุ {trackingNumber} สำหรับคำสั่งซื้อ {orderId} และเปลี่ยนสถานะเป็น 'จัดส่งแล้ว' เรียบร้อยแล้ว";`  
  แสดงข้อความแจ้งเตือนความสำเร็จ
* **บรรทัดที่ 55:** `}`
* **บรรทัดที่ 56:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 57:** `}`  
  ปิดบล็อกเมธอด DispatchOrder
* **บรรทัดที่ 58:** `}`  
  ปิดบล็อกคลาส ShippingController
* **บรรทัดที่ 59:** `}`  
  ปิดบล็อกเนมสเปซ

---

# 12. SupportController.cs (47 บรรทัด)

### โค้ดต้นฉบับเต็มของ SupportController.cs
```csharp
1: using Microsoft.AspNetCore.Authorization;
2: using Microsoft.AspNetCore.Mvc;
3: using CSI402_Project_final.Models;
4: using System.Linq;
5: 
6: namespace CSI402_Project_final.Controllers
7: {
8:     [Authorize(Roles = "ITSupport,Owner,Admin")]
9:     public class SupportController : Controller
10:     {
11:         private readonly PetShopRepository _repo;
12: 
13:         public SupportController(PetShopRepository repo)
14:         {
15:             _repo = repo;
16:         }
17: 
18:         // หน้า Helpdesk Board ของ IT Support (Req 23)
19:         public IActionResult Index(TicketStatus? status = null)
20:         {
21:             var allTickets = _repo.GetSupportTickets();
22:             ViewBag.CurrentRole = _repo.CurrentSimulatedRole;
23:             ViewBag.SelectedStatus = status;
24:             ViewBag.NewCount = allTickets.Count(t => t.Status == TicketStatus.New);
25:             ViewBag.InvestigatingCount = allTickets.Count(t => t.Status == TicketStatus.Investigating);
26:             ViewBag.ResolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved);
27: 
28:             var filtered = status.HasValue
29:                 ? allTickets.Where(t => t.Status == status.Value).ToList()
30:                 : allTickets;
31: 
32:             return View(filtered);
33:         }
34: 
35:         // บันทึกรายละเอียดการแก้ไขปัญหาและปิดงาน (Req 24)
36:         [HttpPost]
37:         public IActionResult ResolveTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus, string? role = null)
38:         {
39:             var ticket = _repo.ResolveSupportTicket(ticketId, technician, resolutionNote, newStatus);
40:             if (ticket != null)
41:             {
42:                 TempData["SuccessMessage"] = $"บันทึกประวัติการแก้ไขและอัปเดตสถานะ Ticket #{ticket.TicketCode} เรียบร้อยแล้ว";
43:             }
44:             return RedirectToAction(nameof(Index), new { role });
45:         }
46:     }
47: }
```

### คำอธิบายการทำงานของ SupportController.cs ทุกบรรทัด

* **บรรทัดที่ 1:** `using Microsoft.AspNetCore.Authorization;`  
  นำเข้าสิทธิ์การเข้าถึงสำหรับใช้งาน `[Authorize(Roles = ...)]`
* **บรรทัดที่ 2:** `using Microsoft.AspNetCore.Mvc;`  
  นำเข้า Controller, IActionResult, HttpPost, RedirectToAction
* **บรรทัดที่ 3:** `using CSI402_Project_final.Models;`  
  นำเข้า SupportTicket, TicketStatus, PetShopRepository
* **บรรทัดที่ 4:** `using System.Linq;`  
  นำเข้าคำสั่ง LINQ สำหรับการนับและกรองตั๋วปัญหา
* **บรรทัดที่ 5:** *(บรรทัดว่าง)*
* **บรรทัดที่ 6:** `namespace CSI402_Project_final.Controllers`  
  ขอบเขตชื่อเนมสเปซ
* **บรรทัดที่ 7:** `{`  
  เปิดบล็อกเนมสเปซ
* **บรรทัดที่ 8:** `[Authorize(Roles = "ITSupport,Owner,Admin")]`  
  จำกัดสิทธิ์เฉพาะเจ้าหน้าที่ฝ่ายสนับสนุนด้านเทคนิค (ITSupport), ผู้บริหาร (Owner) หรือผู้ดูแลระบบ (Admin)
* **บรรทัดที่ 9:** `public class SupportController : Controller`  
  ประกาศคลาส `SupportController` สืบทอดมาจาก `Controller`
* **บรรทัดที่ 10:** `{`  
  เปิดบล็อกคลาส
* **บรรทัดที่ 11:** `private readonly PetShopRepository _repo;`  
  ฟิลด์เก็บ Repository สำหรับจัดการตั๋วแจ้งปัญหา IT Helpdesk
* **บรรทัดที่ 12:** *(บรรทัดว่าง)*
* **บรรทัดที่ 13:** `public SupportController(PetShopRepository repo)`  
  Constructor รับ Repository ผ่าน Dependency Injection
* **บรรทัดที่ 14:** `{`  
  เปิดบล็อก Constructor
* **บรรทัดที่ 15:** `_repo = repo;`  
  ผูก Repository เข้ากับตัวแปรภายใน
* **บรรทัดที่ 16:** `}`  
  ปิดบล็อก Constructor
* **บรรทัดที่ 17:** *(บรรทัดว่าง)*
* **บรรทัดที่ 18:** `// หน้า Helpdesk Board ของ IT Support (Req 23)`  
  คอมเมนต์ระบุ Req 23
* **บรรทัดที่ 19:** `public IActionResult Index(TicketStatus? status = null)`  
  Action เมธอดแสดงกระดานตั๋วแจ้งปัญหา IT Support Helpdesk ผ่าน HTTP GET
* **บรรทัดที่ 20:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 21:** `var allTickets = _repo.GetSupportTickets();`  
  ดึงรายการตั๋วปัญหาทั้งหมดในระบบ
* **บรรทัดที่ 22:** `ViewBag.CurrentRole = _repo.CurrentSimulatedRole;`  
  ส่งบทบาทปัจจุบันไปยัง View
* **บรรทัดที่ 23:** `ViewBag.SelectedStatus = status;`  
  ส่งสถานะตัวกรองที่เลือกไปยัง View
* **บรรทัดที่ 24:** `ViewBag.NewCount = allTickets.Count(t => t.Status == TicketStatus.New);`  
  นับจำนวนตั๋วปัญหาใหม่ที่รอรับเรื่อง
* **บรรทัดที่ 25:** `ViewBag.InvestigatingCount = allTickets.Count(t => t.Status == TicketStatus.Investigating);`  
  นับจำนวนตั๋วที่อยู่ระหว่างการตรวจสอบ/ดำเนินการ
* **บรรทัดที่ 26:** `ViewBag.ResolvedCount = allTickets.Count(t => t.Status == TicketStatus.Resolved);`  
  นับจำนวนตั๋วที่แก้ไขเสร็จสิ้นและปิดงานแล้ว
* **บรรทัดที่ 27:** *(บรรทัดว่าง)*
* **บรรทัดที่ 28:** `var filtered = status.HasValue`  
  ตรวจสอบว่ามีการกรองสถานะหรือไม่
* **บรรทัดที่ 29:** `    ? allTickets.Where(t => t.Status == status.Value).ToList()`  
  ถ้ามี ให้เลือกเฉพาะสถานะนั้น
* **บรรทัดที่ 30:** `    : allTickets;`  
  ถ้าไม่มี ให้แสดงทั้งหมด
* **บรรทัดที่ 31:** *(บรรทัดว่าง)*
* **บรรทัดที่ 32:** `return View(filtered);`  
  ส่งรายการตั๋วปัญหาไปเรนเดอร์ใน `Views/Support/Index.cshtml`
* **บรรทัดที่ 33:** `}`  
  ปิดบล็อกเมธอด Index
* **บรรทัดที่ 34:** *(บรรทัดว่าง)*
* **บรรทัดที่ 35:** `// บันทึกรายละเอียดการแก้ไขปัญหาและปิดงาน (Req 24)`  
  คอมเมนต์ระบุ Req 24
* **บรรทัดที่ 36:** `[HttpPost]`  
  ระบุว่ารับคำขอด้วย HTTP POST
* **บรรทัดที่ 37:** `public IActionResult ResolveTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus, string? role = null)`  
  Action เมธอดรับรหัสตั๋วปัญหา, ชื่อช่างเทคนิค, รายละเอียดการแก้ไข และสถานะใหม่
* **บรรทัดที่ 38:** `{`  
  เปิดบล็อกเมธอด
* **บรรทัดที่ 39:** `var ticket = _repo.ResolveSupportTicket(ticketId, technician, resolutionNote, newStatus);`  
  สั่ง Repository บันทึกผลการแก้ไข, วันเวลาที่ปิดงาน และลงบันทึก Audit Log
* **บรรทัดที่ 40:** `if (ticket != null)`  
  ถ้าบันทึกสำเร็จ
* **บรรทัดที่ 41:** `{`
* **บรรทัดที่ 42:** `TempData["SuccessMessage"] = $"บันทึกประวัติการแก้ไขและอัปเดตสถานะ Ticket #{ticket.TicketCode} เรียบร้อยแล้ว";`  
  แสดงข้อความแจ้งเตือนความสำเร็จพร้อมระบุรหัส Ticket
* **บรรทัดที่ 43:** `}`
* **บรรทัดที่ 44:** `return RedirectToAction(nameof(Index), new { role });`  
  ส่งกลับหน้า Index
* **บรรทัดที่ 45:** `}`  
  ปิดบล็อกเมธอด ResolveTicket
* **บรรทัดที่ 46:** `}`  
  ปิดบล็อกคลาส SupportController
* **บรรทัดที่ 47:** `}`  
  ปิดบล็อกเนมสเปซ
