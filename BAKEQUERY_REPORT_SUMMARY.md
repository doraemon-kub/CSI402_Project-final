# สรุปเล่มรายงานโครงการ: BakeQuery ระบบเว็บไซต์ขายเบเกอรี่ (ฉบับสมบูรณ์)

---

## 📌 ข้อมูลโครงการ (Project Information)
* **ชื่อโครงการ:** BakeQuery — ระบบเว็บไซต์ขายเบเกอรี่ออนไลน์ (Bakery E-Commerce Platform)
* **ผู้จัดทำ:** นายยศยุต ฤธาทิพย์ (รหัสนักศึกษา: 66033872)
* **อาจารย์ที่ปรึกษา:** อาจารย์ ณัฐพล พุทธจรรยาวงศ์
* **รายวิชา:** CSI402 การเชื่อมต่อโปรแกรมส่วนหน้ากับโปรแกรมส่วนหลัง (Front-end and Back-end Integration)
* **ภาคเรียน/ปีการศึกษา:** ภาคเรียนที่ 2 ปีการศึกษา 2568
* **สถาบัน:** คณะเทคโนโลยีสารสนเทศ มหาวิทยาลัยศรีปทุม วิทยาเขตบางเขน
* **สถาปัตยกรรมระบบ:** ASP.NET Core MVC (.NET 8/9), Entity Framework Core, MySQL Database, Cookie Authentication, Session State (OTP Verification)

---

## 1. ภาพรวมและวัตถุประสงค์ของระบบ (System Overview & Objectives)
ระบบ **BakeQuery** พัฒนาขึ้นเพื่อแก้ปัญหาการจัดการร้านเบเกอรี่แบบดั้งเดิม โดยเปลี่ยนผ่านสู่แพลตฟอร์มพาณิชย์อิเล็กทรอนิกส์เต็มรูปแบบ:
1. **ช่องทางการจำหน่ายออนไลน์:** ลูกค้าสามารถเลือกชมเบเกอรี่ ค้นหา กรองตามหมวดหมู่ ตรวจสอบโปรโมชั่น และสั่งซื้อได้ตลอด 24 ชั่วโมง
2. **ระบบคิดคำนวณและโปรโมชั่นอัตโนมัติ:** คำนวณราคาสินค้า ส่วนลดโปรโมชั่นแบบ Real-time ตามเงื่อนไข (เช่น ซื้อครบยอด, ซื้อสินค้าตามหมวดหมู่)
3. **ระบบจัดการหลังบ้านครบวงจร (BackOffice):** รองรับการจัดการคำสั่งซื้อ การตรวจสอบสลิปโอนเงิน การเปลี่ยนสถานะจัดส่ง การจัดการสต็อกสินค้า หมวดหมู่ และบัญชีผู้ใช้งาน
4. **ความปลอดภัยและการแยกบทบาท (RBAC):** มีการควบคุมสิทธิ์อย่างรัดกุม 5 บทบาท พร้อมการยืนยันตัวตนผ่าน OTP ทางอีเมลสำหรับการรีเซ็ตรหัสผ่าน

---

## 2. บทบาทและสิทธิ์ผู้ใช้งาน (User Roles & Permission Matrix)

ระบบแบ่งผู้ใช้งานออกเป็น **5 บทบาท (Roles)** ชัดเจน:

| สิทธิ์ / ความสามารถ | Guest (ผู้เยี่ยมชม) | Customer (ลูกค้า) | Staff (พนักงาน) | Admin (ผู้ดูแลระบบ) | Manager (ผู้บริหาร) |
| :--- | :---: | :---: | :---: | :---: | :---: |
| ดูรายการสินค้า & โปรโมชั่น | ✅ | ✅ | ✅ | ✅ | ✅ |
| สมัครสมาชิก / เข้าสู่ระบบ | ✅ | ✅ | ✅ | ✅ | ✅ |
| จัดการตะกร้าสินค้า & สั่งซื้อ | ❌ | ✅ | ❌ | ❌ | ❌ |
| แนบสลิปชำระเงิน & ติดตามคำสั่งซื้อ | ❌ | ✅ | ❌ | ❌ | ❌ |
| ยกเลิกคำสั่งซื้อ / ยืนยันการรับสินค้า | ❌ | ✅ | ❌ | ❌ | ❌ |
| ตรวจสอบ & อัปเดตสถานะคำสั่งซื้อ | ❌ | ❌ | ✅ | ✅ | ✅ |
| ตรวจสอบสลิป & อัปเดตสถานะชำระเงิน | ❌ | ❌ | ✅ | ✅ | ✅ |
| ตอบกลับลูกค้าในคำสั่งซื้อ (Reply Order) | ❌ | ❌ | ✅ | ✅ | ✅ |
| จัดการสินค้า & หมวดหมู่ (CRUD) | ❌ | ❌ | ❌ | ✅ | ✅ |
| จัดการโปรโมชั่น & ส่วนลด (CRUD) | ❌ | ❌ | ❌ | ✅ | ✅ |
| จัดการบัญชีผู้ใช้งาน & เปลี่ยนรหัสผ่าน | ❌ | ❌ | ❌ | ✅ | ✅ |
| ดูแดชบอร์ดภาพรวมรายได้และรายงานสถิติ | ❌ | ❌ | ❌ | ❌ | ✅ |

---

## 3. ขอบเขตการทำงานของระบบ (System Scope - 9 Modules)
1. **ระบบจัดการผู้ใช้งานและการยืนยันตัวตน (Authentication & Authorization Module):**
   * เข้าสู่ระบบ / ออกจากระบบ / สมัครสมาชิก
   * ลืมรหัสผ่านด้วยการส่งรหัส OTP ผ่าน Email และตั้งรหัสผ่านใหม่
   * หน้าแจ้งเตือนปฏิเสธการเข้าถึง (Access Denied) เมื่อไม่มีสิทธิ์
2. **ระบบโปรไฟล์ผู้ใช้งาน (User Profile Module):**
   * ดูและแก้ไขข้อมูลส่วนตัว ที่อยู่ในการจัดส่ง และเบอร์โทรศัพท์
3. **ระบบแคตตาล็อกสินค้า (Product Catalog Module):**
   * แสดงสินค้าเบเกอรี่ แยกตามหมวดหมู่ การค้นหาชื่อสินค้า และตัวกรองราคา
4. **ระบบโปรโมชั่น (Promotion Engine Module):**
   * แสดงแคมเปญโปรโมชั่นที่เปิดใช้งาน คำนวณส่วนลดตามเงื่อนไข (Percentage / Fixed Amount / Minimum Spend)
5. **ระบบตะกร้าสินค้า (Shopping Cart Module):**
   * เพิ่ม/ลดจำนวน ลบรายการสินค้า ล้างตะกร้า และบันทึกตะกร้าลงฐานข้อมูลตามบัญชีผู้ใช้
6. **ระบบสั่งซื้อและชำระเงิน (Checkout & Order Placement Module):**
   * สรุปยอดเงิน หักส่วนลดโปรโมชั่น สร้าง Order และ OrderItems แนบหลักฐานสลิปโอนเงิน (Payment Proof)
7. **ระบบประวัติและการติดตามคำสั่งซื้อ (Order History & Tracking Module):**
   * แสดงสถานะคำสั่งซื้อแบบ Step Tracking (Pending Payment ➔ Verifying ➔ Preparing ➔ Shipping ➔ Completed / Cancelled)
   * ลูกค้ายืนยันรับสินค้า (Confirm Received) หรือขอยกเลิก (Cancel Order)
8. **ระบบงานหลังบ้าน (BackOffice Operations Module):**
   * ตรวจสอบสลิปโอนเงิน อัปเดตสถานะคำสั่งซื้อและสถานะชำระเงิน พร้อมระบบสนทนา/ตอบกลับลูกค้าในแต่ละ Order
9. **ระบบบริหารจัดการข้อมูลหลัก (Master Data Management & Analytics Module):**
   * เพิ่ม/ลบ/แก้ไขสินค้า หมวดหมู่ โปรโมชั่น บัญชีผู้ใช้งาน และดู Dashboard ภาพรวมยอดขาย

---

## 4. แผนผังการทำงานของระบบ (Flowcharts Breakdown)

### 4.1 ผังงานภาพรวมระบบ (Overall System Flowchart)
* **กระบวนการ:** เริ่มต้น ➔ ผู้ใช้เข้าสู่ระบบ ➔ ตรวจสอบ Session/Cookie ➔ หากเป็น Guest ให้เข้าสู่หน้า Public Menu / หน้าโปรโมชั่น หากต้องการสั่งซื้อระบบจะบังคับ Login ➔ เมื่อ Login สำเร็จ ตรวจสอบ Role:
  * ถ้าเป็น `Customer` ➔ ไปยังหน้าเลือกสินค้า ตะกร้าสินค้า และสั่งซื้อ
  * ถ้าเป็น `Staff`, `Admin`, `Manager` ➔ ไปยัง BackOffice Portal ตามสิทธิ์

### 4.2 ผังงานการยืนยันตัวตนและกู้คืนรหัสผ่าน (Authentication & OTP Flowchart)
* **กระบวนการ:** กรอกอีเมล ➔ ตรวจสอบว่ามีอีเมลในฐานข้อมูลหรือไม่ ➔ ถือสิทธิ์ส่ง OTP (6 หลัก) บันทึกลง Session พร้อมเวลาหมดอายุ (5 นาที) ➔ ผู้ใช้กรอก OTP ➔ ตรวจสอบความถูกต้อง ➔ หากถูกต้อง อนุญาตให้ตั้งรหัสผ่านใหม่ (Password Hash) ➔ อัปเดตตาราง `Users` ➔ ล้าง Session OTP

### 4.3 ผังงานการสั่งซื้อของลูกค้า (Customer Journey Flowchart)
* **กระบวนการ:** เลือกสินค้าลงตะกร้า (`AddToCart`) ➔ ระบบตรวจสอบสต็อก ➔ ไปที่หน้า Cart ➔ ดึงโปรโมชั่นที่เข้าเงื่อนไขมาคำนวณส่วนลด ➔ กด Checkout ➔ เลือกที่อยู่จัดส่ง ➔ สร้างคำสั่งซื้อสถานะ `PendingPayment` ➔ ลูกค้าอัปโหลดสลิป (`UploadPaymentProof`) ➔ เปลี่ยนสถานะเป็น `Verifying` ➔ รอเจ้าหน้าที่ตรวจสอบ

### 4.4 ผังงานการดำเนินงานหลังบ้าน (BackOffice Operation Flowchart)
* **กระบวนการ:** เจ้าหน้าที่เข้า `OrderMenu` ➔ กรองดูคำสั่งซื้อที่รอตรวจสอบ ➔ เปิดดูรูปภาพสลิป (`PaymentProof`) ➔ 
  * หากสลิปถูกต้อง: เปลี่ยน `PaymentStatus` เป็น `Paid` และเปลี่ยน `OrderStatus` เป็น `Preparing` (กำลังเตรียมสินค้า) ➔ `Shipping` (กำลังจัดส่ง)
  * หากสลิปไม่ถูกต้อง: ส่งข้อความตอบกลับ (`ReplyOrder`) แจ้งลูกค้าให้อัปโหลดใหม่ หรือเปลี่ยนสถานะเป็น `Cancelled`

---

## 5. แผนภาพกระแสข้อมูล (Data Flow Diagrams: DFD)

### 5.1 Context Diagram (DFD ระดับ 0)
* **Entities:** Customer, Staff, Admin, Manager, Email Gateway (SMTP)
* **Process 0: BakeQuery Bakery Web Application System**
  * Customer ส่ง: ข้อมูลสมัครสมาชิก, ข้อมูลคำสั่งซื้อ, สลิปชำระเงิน, ข้อความสอบถาม
  * System ส่งให้ Customer: ข้อมูลสินค้า, สถานะตะกร้า, รหัส OTP, สถานะคำสั่งซื้อ, ใบเสร็จ
  * Staff / Admin ส่ง: สถานะคำสั่งซื้อ, ข้อมูลสินค้า/โปรโมชั่น, ผลการตรวจสลิป
  * System ส่งให้ Staff / Admin: รายการ Order รอดำเนินการ, สถิติสต็อก, ข้อมูลบัญชีผู้ใช้
  * System ส่งให้ Manager: แดชบอร์ดสรุปยอดขาย (BI Dashboard)

### 5.2 DFD Level 1 (แจกแจงกระบวนการหลัก)
* **Process 1.0 (Manage Authentication):** ตรวจสอบสิทธิ์ผู้ใช้, จัดการ Register/Login/OTP (เชื่อมต่อ Datastore `D1: Users`, `D2: Roles`)
* **Process 2.0 (Browse Products & Promotions):** ค้นหาสินค้าและเงื่อนไขส่วนลด (เชื่อมต่อ Datastore `D4: Products`, `D5: Categories`, `D6: Promotions`)
* **Process 3.0 (Manage Cart & Checkout):** บันทึกสินค้าในตะกร้า คำนวณส่วนลด และสร้างใบสั่งซื้อ (เชื่อมต่อ Datastore `D7: Carts`, `D8: Cart_items`, `D9: Orders`, `D10: Order_items`)
* **Process 4.0 (Order & Payment Processing):** รับการอัปโหลดสลิป ตรวจสอบ และอัปเดตสถานะ (เชื่อมต่อ Datastore `D11: Payment_proofs`, `D9: Orders`, `D12: Order_replies`)
* **Process 5.0 (BackOffice & Master Data Management):** การจัดการสินค้า หมวดหมู่ โปรโมชั่น และบัญชี (เชื่อมต่อทุกตารางที่เกี่ยวข้อง)

---

## 6. พจนานุกรมข้อมูล (Data Dictionary: 14 Database Tables)

### ตารางที่ 1: `Users` (ข้อมูลบัญชีผู้ใช้งาน)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `user_id` | INT | NO | PK (AI) | รหัสประจำตัวผู้ใช้งาน |
| `username` | VARCHAR(50) | NO | UNIQUE | ชื่อผู้ใช้งาน |
| `email` | VARCHAR(100) | NO | UNIQUE | อีเมลสำหรับติดต่อและกู้รหัสผ่าน |
| `password_hash` | VARCHAR(255) | NO | - | รหัสผ่านที่ผ่านการเข้ารหัสแฮช |
| `full_name` | VARCHAR(100) | NO | - | ชื่อ-นามสกุลจริง |
| `phone_number` | VARCHAR(20) | YES | - | เบอร์โทรศัพท์ |
| `role_id` | INT | NO | FK | รหัสบทบาทผู้ใช้งาน (อ้างอิง `Roles.role_id`) |
| `created_at` | DATETIME | NO | - | วันที่สร้างบัญชี |
| `updated_at` | DATETIME | YES | - | วันที่แก้ไขข้อมูลล่าสุด |

### ตารางที่ 2: `Roles` (ข้อมูลบทบาทและสิทธิ์)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `role_id` | INT | NO | PK (AI) | รหัสบทบาท |
| `role_name` | VARCHAR(50) | NO | UNIQUE | ชื่อบทบาท (Customer, Staff, Admin, Manager) |
| `description` | VARCHAR(255) | YES | - | คำอธิบายสิทธิ์ของบทบาท |

### ตารางที่ 3: `user_addresses` (ข้อมูลที่อยู่จัดส่งของลูกค้า)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `address_id` | INT | NO | PK (AI) | รหัสที่อยู่ |
| `user_id` | INT | NO | FK | รหัสผู้ใช้เจ้าของที่อยู่ |
| `recipient_name`| VARCHAR(100) | NO | - | ชื่อผู้รับสินค้า |
| `phone_number` | VARCHAR(20) | NO | - | เบอร์โทรศัพท์ผู้รับ |
| `address_line` | TEXT | NO | - | รายละเอียดที่อยู่ (บ้านเลขที่ หมู่ ซอย ถนน) |
| `subdistrict` | VARCHAR(100) | NO | - | ตำบล/แขวง |
| `district` | VARCHAR(100) | NO | - | อำเภอ/เขต |
| `province` | VARCHAR(100) | NO | - | จังหวัด |
| `postal_code` | VARCHAR(10) | NO | - | รหัสไปรษณีย์ |
| `is_default` | TINYINT(1) | NO | - | สถานะที่อยู่เริ่มต้น (1=ใช่, 0=ไม่ใช่) |

### ตารางที่ 4: `Categories` (หมวดหมู่สินค้าเบเกอรี่)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `category_id` | INT | NO | PK (AI) | รหัสหมวดหมู่สินค้า |
| `category_name`| VARCHAR(100) | NO | UNIQUE | ชื่อหมวดหมู่ (เช่น เค้ก, ขนมปัง, คุกกี้, ครัวซองต์) |
| `description` | TEXT | YES | - | คำอธิบายหมวดหมู่ |
| `is_active` | TINYINT(1) | NO | - | สถานะเปิด/ปิดการใช้งาน |

### ตารางที่ 5: `Products` (ข้อมูลสินค้าเบเกอรี่)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `product_id` | INT | NO | PK (AI) | รหัสสินค้า |
| `product_name` | VARCHAR(150) | NO | - | ชื่อสินค้าเบเกอรี่ |
| `category_id` | INT | NO | FK | หมวดหมู่สินค้า |
| `description` | TEXT | YES | - | รายละเอียด ส่วนผสม ข้อมูลโภชนาการ |
| `price` | DECIMAL(10,2) | NO | - | ราคาสินค้าต่อหน่วย (บาท) |
| `stock_quantity`| INT | NO | - | จำนวนสินค้าคงเหลือในสต็อก |
| `image_url` | VARCHAR(255) | YES | - | ที่อยู่ไฟล์รูปภาพสินค้า |
| `is_active` | TINYINT(1) | NO | - | สถานะวางจำหน่าย (1=ขาย, 0=หยุดจำหน่าย) |
| `created_at` | DATETIME | NO | - | วันที่เพิ่มสินค้า |

### ตารางที่ 6: `Carts` (ตะกร้าสินค้าของผู้ใช้งาน)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `cart_id` | INT | NO | PK (AI) | รหัสตะกร้าสินค้า |
| `user_id` | INT | NO | FK, UNIQUE | รหัสผู้ใช้ที่เป็นเจ้าของตะกร้า |
| `created_at` | DATETIME | NO | - | วันที่สร้างตะกร้า |
| `updated_at` | DATETIME | YES | - | วันที่อัปเดตล่าสุด |

### ตารางที่ 7: `Cart_items` (รายการสินค้าในตะกร้า)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `cart_item_id` | INT | NO | PK (AI) | รหัสรายการในตะกร้า |
| `cart_id` | INT | NO | FK | รหัสตะกร้าสินค้า |
| `product_id` | INT | NO | FK | รหัสสินค้า |
| `quantity` | INT | NO | - | จำนวนที่เลือกซื้อ |

### ตารางที่ 8: `Orders` (ข้อมูลใบสั่งซื้อสินค้า)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `order_id` | INT | NO | PK (AI) | รหัสคำสั่งซื้อ |
| `order_number` | VARCHAR(50) | NO | UNIQUE | เลขที่เอกสารคำสั่งซื้อ (เช่น ORD-20261122-001) |
| `user_id` | INT | NO | FK | รหัสลูกค้าผู้สั่งซื้อ |
| `total_amount` | DECIMAL(10,2) | NO | - | ยอดรวมก่อนหักส่วนลด (บาท) |
| `discount_amount`| DECIMAL(10,2)| NO | - | มูลค่าส่วนลดโปรโมชั่น (บาท) |
| `net_amount` | DECIMAL(10,2) | NO | - | ยอดเงินสุทธิที่ต้องชำระ (บาท) |
| `order_status` | VARCHAR(30) | NO | - | สถานะคำสั่งซื้อ (PendingPayment, Verifying, Preparing, Shipping, Completed, Cancelled) |
| `payment_status`| VARCHAR(30) | NO | - | สถานะการชำระเงิน (Unpaid, PendingVerification, Paid, Refunded) |
| `shipping_address`| TEXT | NO | - | ที่อยู่จัดส่งที่บันทึก ณ เวลาสั่งซื้อ |
| `order_date` | DATETIME | NO | - | วันที่สั่งซื้อ |

### ตารางที่ 9: `Order_items` (รายการสินค้าในใบสั่งซื้อ)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `order_item_id`| INT | NO | PK (AI) | รหัสรายการสั่งซื้อ |
| `order_id` | INT | NO | FK | รหัสคำสั่งซื้อหลัก |
| `product_id` | INT | NO | FK | รหัสสินค้า |
| `unit_price` | DECIMAL(10,2) | NO | - | ราคาต่อหน่วย ณ ขณะที่สั่งซื้อ |
| `quantity` | INT | NO | - | จำนวนที่สั่งซื้อ |
| `subtotal` | DECIMAL(10,2) | NO | - | ราคารวมของรายการ (`unit_price * quantity`) |

### ตารางที่ 10: `Payment_proofs` (ข้อมูลหลักฐานการโอนเงิน/สลิป)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `proof_id` | INT | NO | PK (AI) | รหัสหลักฐานการชำระเงิน |
| `order_id` | INT | NO | FK | รหัสคำสั่งซื้อที่ชำระ |
| `slip_image_url`| VARCHAR(255) | NO | - | ที่อยู่รูปภาพสลิปโอนเงิน |
| `transferred_amount`| DECIMAL(10,2)| NO | - | จำนวนเงินที่โอนตามสลิป |
| `transferred_at`| DATETIME | NO | - | วันเวลาที่โอนเงิน |
| `uploaded_at` | DATETIME | NO | - | วันเวลาที่ลูกค้าอัปโหลดสลิป |
| `verified_by` | INT | YES | FK | รหัสเจ้าหน้าที่ผู้ตรวจสอบ (`Users.user_id`) |
| `verified_at` | DATETIME | YES | - | วันเวลาที่ตรวจสอบเสร็จสิ้น |

### ตารางที่ 11: `Order_replies` (ประวัติการตอบกลับ/สนทนาในคำสั่งซื้อ)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `reply_id` | INT | NO | PK (AI) | รหัสข้อความตอบกลับ |
| `order_id` | INT | NO | FK | รหัสคำสั่งซื้อ |
| `sender_id` | INT | NO | FK | รหัสผู้ส่งข้อความ (`Users.user_id`) |
| `message` | TEXT | NO | - | ข้อความแจ้งเตือนหรือสอบถาม |
| `created_at` | DATETIME | NO | - | วันเวลาที่ส่งข้อความ |

### ตารางที่ 12: `Promotions` (ข้อมูลแคมเปญโปรโมชั่น)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `promotion_id` | INT | NO | PK (AI) | รหัสโปรโมชั่น |
| `promo_code` | VARCHAR(50) | NO | UNIQUE | โค้ดส่วนลด |
| `title` | VARCHAR(150) | NO | - | ชื่อแคมเปญโปรโมชั่น |
| `description` | TEXT | YES | - | เงื่อนไขและรายละเอียดโปรโมชั่น |
| `discount_type` | VARCHAR(20) | NO | - | ประเภทส่วนลด (Percentage, FixedAmount, FreeShipping) |
| `discount_value`| DECIMAL(10,2)| NO | - | มูลค่าส่วนลด (% หรือ บาท) |
| `start_date` | DATETIME | NO | - | วันเริ่มต้นโปรโมชั่น |
| `end_date` | DATETIME | NO | - | วันสิ้นสุดโปรโมชั่น |
| `is_active` | TINYINT(1) | NO | - | สถานะเปิดใช้งาน |

### ตารางที่ 13: `promotion_rules` (เงื่อนไขและกฎเกณฑ์โปรโมชั่น)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `rule_id` | INT | NO | PK (AI) | รหัสกฎโปรโมชั่น |
| `promotion_id` | INT | NO | FK | รหัสโปรโมชั่น |
| `min_order_amount`| DECIMAL(10,2)| YES | - | ยอดสั่งซื้อขั้นต่ำที่ได้รับสิทธิ์ |
| `min_item_quantity`| INT | YES | - | จำนวนชิ้นขั้นต่ำที่ต้องซื้อ |
| `target_category_id`| INT | YES | FK | เฉพาะหมวดหมู่สินค้าที่ระบุ |

### ตารางที่ 14: `promotion_redemptions` (ประวัติการใช้สิทธิ์โปรโมชั่น)
| Column Name | Data Type | Nullable | Key | Description |
| :--- | :--- | :---: | :---: | :--- |
| `redemption_id`| INT | NO | PK (AI) | รหัสการใช้สิทธิ์ |
| `promotion_id` | INT | NO | FK | รหัสโปรโมชั่น |
| `user_id` | INT | NO | FK | รหัสผู้ใช้งานที่ใช้สิทธิ์ |
| `order_id` | INT | NO | FK | รหัสคำสั่งซื้อที่ใช้สิทธิ์ |
| `redeemed_at` | DATETIME | NO | - | วันเวลาที่ใช้สิทธิ์ |

---

## 7. รายละเอียดฟังก์ชันการทำงานและซอร์สโค้ดทั้ง 40 ฟังก์ชัน (Inventory of 40 MVC Actions)

เล่มรายงานได้จัดเรียงโครงสร้างของทั้ง 40 ฟังก์ชันการทำงานอย่างเป็นระบบ โดยแต่ละฟังก์ชันประกอบด้วย:
* **Controller และ Action Method**
* **ViewModel ที่ใช้งาน**
* **ลำดับขั้นตอนการทำงาน (Process Workflow Steps)**
* **ตารางฐานข้อมูลที่เกี่ยวข้อง (Database Tables Involved)**

---

### 🔹 กลุ่มที่ 1: ระบบสมาชิกและการยืนยันตัวตน (AccountController - 8 ฟังก์ชัน)

#### 1. เข้าสู่ระบบ (Login)
* **Controller/Action:** `AccountController.Login(LoginViewModel model)` [GET/POST]
* **ViewModel:** `LoginViewModel` (`Username`, `Password`, `RememberMe`, `ReturnUrl`)
* **ขั้นตอนการทำงาน:**
  1. รับข้อมูล `Username` และ `Password` จาก Form
  2. ค้นหาผู้ใช้ในตาราง `Users` โดยตรวจสอบชื่อผู้ใช้
  3. ตรวจสอบรหัสผ่านด้วย `PasswordHasher.VerifyHashedPassword`
  4. สร้าง Claims (`NameIdentifier`, `Name`, `Role`, `Email`)
  5. บันทึก Cookie Authentication (`HttpContext.SignInAsync`) และ Redirect ไปยังหน้าเป้าหมายตาม Role
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`

#### 2. สมัครสมาชิก (Register)
* **Controller/Action:** `AccountController.Register(RegisterViewModel model)` [GET/POST]
* **ViewModel:** `RegisterViewModel` (`Username`, `Email`, `FullName`, `Password`, `ConfirmPassword`, `PhoneNumber`)
* **ขั้นตอนการทำงาน:**
  1. ตรวจสอบความถูกต้องของ Model Validation
  2. เช็กความซ้ำซ้อนของ `Username` และ `Email` ในระบบ
  3. แฮชรหัสผ่านด้วย `PasswordHasher`
  4. เพิ่มข้อมูลผู้ใช้ใหม่โดยกำหนด `role_id` เริ่มต้นเป็น Customer
  5. บันทึกข้อมูลลงฐานข้อมูลและสร้าง Cart เริ่มต้นสำหรับผู้ใช้ใหม่
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`, `Carts`

#### 3. ขอรหัส OTP ลืมรหัสผ่าน (Request Forgot Password OTP)
* **Controller/Action:** `AccountController.RequestForgotPasswordOtp(ForgotPasswordRequestOtpViewModel model)` [GET/POST]
* **ViewModel:** `ForgotPasswordRequestOtpViewModel` (`Email`)
* **ขั้นตอนการทำงาน:**
  1. รับอีเมลและตรวจสอบว่ามีผู้ใช้งานนี้อยู่ในฐานข้อมูลหรือไม่
  2. สุ่มรหัส OTP ตัวเลข 6 หลัก (เช่น `Random.Shared.Next(100000, 999999)`)
  3. บันทึก OTP, อีเมล และเวลาหมดอายุ (5 นาที) ลงใน `HttpContext.Session`
  4. ส่งอีเมลรหัส OTP ไปยังผู้ใช้ผ่าน SMTP Service
  5. นำทางผู้ใช้ไปยังหน้ากรอก OTP และตั้งรหัสผ่านใหม่
* **ตารางที่เกี่ยวข้อง:** `Users`

#### 4. ยืนยัน OTP และตั้งรหัสผ่านใหม่ (Forgot Password - Verify OTP & Reset)
* **Controller/Action:** `AccountController.ForgotPassword(ForgotPasswordViewModel model)` [POST]
* **ViewModel:** `ForgotPasswordViewModel` (`Email`, `OtpCode`, `NewPassword`, `ConfirmNewPassword`)
* **ขั้นตอนการทำงาน:**
  1. ดึง OTP และเวลาหมดอายุจาก Session มาตรวจสอบกับค่าที่ผู้ใช้กรอก
  2. หาก OTP ถูกต้องและยังไม่หมดอายุ ดึงผู้ใช้จากฐานข้อมูล
  3. แฮชรหัสผ่านใหม่และอัปเดตลงฟิลด์ `password_hash` ในตาราง `Users`
  4. เคลียร์ Session OTP ทิ้งเพื่อป้องกันการนำกลับมาใช้ซ้ำ
  5. แสดงข้อความสำเร็จและเปลี่ยนเส้นทางไปยังหน้า Login
* **ตารางที่เกี่ยวข้อง:** `Users`

#### 5. ออกจากระบบ (Logout)
* **Controller/Action:** `AccountController.Logout()` [POST]
* **ViewModel:** ไม่มี (ใช้วิธี POST ป้องกัน CSRF)
* **ขั้นตอนการทำงาน:**
  1. เรียก `HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)`
  2. ล้าง Session ข้อมูลชั่วคราว
  3. Redirect ไปยังหน้าแรกของเว็บไซต์ (Home/Menu)
* **ตารางที่เกี่ยวข้อง:** -

#### 6. ปฏิเสธการเข้าถึง (Access Denied)
* **Controller/Action:** `AccountController.AccessDenied(string returnUrl)` [GET]
* **ViewModel:** `AccessDeniedViewModel` (`ReturnUrl`, `UserRole`)
* **ขั้นตอนการทำงาน:**
  1. ตรวจสอบสิทธิ์และ URL ปลายทางที่พยายามเข้าถึง
  2. แสดงหน้าเตือนแจ้งว่าผู้ใช้ไม่มีสิทธิ์ในส่วนนี้ พร้อมปุ่มกลับสู่หน้าร้านหรือสลับบัญชี
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`

#### 7. จัดการข้อมูลโปรไฟล์ (Profile)
* **Controller/Action:** `AccountController.Profile()` [GET/POST]
* **ViewModel:** `UserProfileViewModel` (`FullName`, `Email`, `PhoneNumber`, `Addresses`)
* **ขั้นตอนการทำงาน:**
  1. ดึง `UserId` จาก Claims ของผู้ใช้ที่ล็อกอินอยู่
  2. ดึงข้อมูลผู้ใช้และรายการที่อยู่จัดส่งจากตาราง `user_addresses`
  3. เมื่อมีการส่ง Form แก้ไข อัปเดตข้อมูลส่วนตัวและที่อยู่เริ่มต้น
* **ตารางที่เกี่ยวข้อง:** `Users`, `user_addresses`

#### 8. หน้าหลักของระบบ (HomePage)
* **Controller/Action:** `AccountController.HomePage()` [GET]
* **ViewModel:** `HomePageViewModel` (`BannerPromotions`, `FeaturedProducts`, `CategoryHighlights`)
* **ขั้นตอนการทำงาน:**
  1. ดึงสินค้าแนะนำและสินค้าขายดี
  2. ดึงโปรโมชั่นที่เปิดใช้งานอยู่
  3. ส่งข้อมูลไปยังหน้า View เพื่อแสดงผลแบนเนอร์และรายการสินค้า
* **ตารางที่เกี่ยวข้อง:** `Products`, `Categories`, `Promotions`

---

### 🔹 กลุ่มที่ 2: ระบบหน้าร้านและลูกค้า (PublicPageController - 13 ฟังก์ชัน)

#### 9. แสดงรายการเมนูสินค้าทั้งหมด (Public Menu)
* **Controller/Action:** `PublicPageController.Menu(int? categoryId, string searchTerm, decimal? maxPrice)` [GET]
* **ViewModel:** `ProductListViewModel` (`Products`, `Categories`, `SelectedCategoryId`, `SearchTerm`)
* **ขั้นตอนการทำงาน:** กรองสินค้าตามหมวดหมู่ คำค้นหา และช่วงราคา เฉพาะสินค้าที่ `is_active = 1`
* **ตารางที่เกี่ยวข้อง:** `Products`, `Categories`

#### 10. แสดงรายการโปรโมชั่น (Public Promotion)
* **Controller/Action:** `PublicPageController.Promotion()` [GET]
* **ViewModel:** `PromotionListViewModel` (`ActivePromotions`)
* **ขั้นตอนการทำงาน:** ดึงโปรโมชั่นที่อยู่ในช่วงเวลา `start_date` ถึง `end_date` และ `is_active = 1`
* **ตารางที่เกี่ยวข้อง:** `Promotions`, `promotion_rules`

#### 11. เพิ่มสินค้าเข้าตะกร้า (AddToCart)
* **Controller/Action:** `PublicPageController.AddToCart(int productId, int quantity)` [POST]
* **ViewModel:** `AddToCartRequestModel` (`ProductId`, `Quantity`)
* **ขั้นตอนการทำงาน:**
  1. ตรวจสอบสิทธิ์ผู้ใช้ (ต้องเป็น Customer)
  2. ตรวจสอบสต็อกในตาราง `Products`
  3. หากมีสินค้านี้อยู่ใน `Cart_items` อยู่แล้ว ให้บวกจำนวนเพิ่ม
  4. หากยังไม่มี ให้สร้างแถวใหม่ใน `Cart_items`
* **ตารางที่เกี่ยวข้อง:** `Carts`, `Cart_items`, `Products`

#### 12. แสดงตะกร้าสินค้า (Cart)
* **Controller/Action:** `PublicPageController.Cart()` [GET]
* **ViewModel:** `CartViewModel` (`Items`, `SubTotal`, `AppliedDiscount`, `GrandTotal`, `EligiblePromotions`)
* **ขั้นตอนการทำงาน:**
  1. ดึงรายการสินค้าทั้งหมดในตะกร้าของผู้ใช้
  2. คำนวณยอดรวม (Subtotal)
  3. ประเมินโปรโมชั่นที่เข้าเกณฑ์และคำนวณส่วนลดอัตโนมัติ
* **ตารางที่เกี่ยวข้อง:** `Carts`, `Cart_items`, `Products`, `Promotions`, `promotion_rules`

#### 13. ปรับปรุงจำนวนสินค้าในตะกร้า (UpdateCartItem)
* **Controller/Action:** `PublicPageController.UpdateCartItem(int cartItemId, int quantity)` [POST]
* **ViewModel:** `UpdateCartItemViewModel` (`CartItemId`, `Quantity`)
* **ขั้นตอนการทำงาน:** ปรับยอดจำนวนสินค้าใน `Cart_items` โดยเช็กไม่ให้เกินจำนวนสต็อกที่มี
* **ตารางที่เกี่ยวข้อง:** `Cart_items`, `Products`

#### 14. ลบสินค้าออกจากตะกร้า (RemoveFromCart)
* **Controller/Action:** `PublicPageController.RemoveFromCart(int cartItemId)` [POST]
* **ViewModel:** `RemoveCartItemViewModel` (`CartItemId`)
* **ขั้นตอนการทำงาน:** ลบแถวรายการออกจากตาราง `Cart_items`
* **ตารางที่เกี่ยวข้อง:** `Cart_items`

#### 15. ล้างตะกร้าสินค้า (ClearCart)
* **Controller/Action:** `PublicPageController.ClearCart()` [POST]
* **ViewModel:** ไม่มี
* **ขั้นตอนการทำงาน:** ลบรายการทั้งหมดใน `Cart_items` ที่ผูกกับ `cart_id` ของลูกค้ารายนี้
* **ตารางที่เกี่ยวข้อง:** `Carts`, `Cart_items`

#### 16. สั่งซื้อและชำระเงิน (Checkout)
* **Controller/Action:** `PublicPageController.Checkout(CheckoutViewModel model)` [GET/POST]
* **ViewModel:** `CheckoutViewModel` (`SelectedAddressId`, `PaymentMethod`, `PromoCode`, `OrderSummary`)
* **ขั้นตอนการทำงาน:**
  1. ตรวจสอบสินค้าในตะกร้าและตัดสต็อก
  2. คำนวณส่วนลดโปรโมชั่นที่เลือกใช้
  3. บันทึกคำสั่งซื้อลง `Orders` และเพิ่มรายละเอียดลง `Order_items`
  4. บันทึกประวัติการใช้สิทธิ์ลง `promotion_redemptions`
  5. ล้างตะกร้าสินค้าและนำทางไปยังหน้าอัปโหลดสลิป
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Order_items`, `Carts`, `Cart_items`, `Products`, `Promotions`, `promotion_redemptions`

#### 17. รายการคำสั่งซื้อปัจจุบันของฉัน (My Orders)
* **Controller/Action:** `PublicPageController.MyOrders()` [GET]
* **ViewModel:** `MyOrdersViewModel` (`ActiveOrders`)
* **ขั้นตอนการทำงาน:** ดึงคำสั่งซื้อของผู้ใช้ที่ยังไม่สิ้นสุด (สถานะ Pending, Verifying, Preparing, Shipping)
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Payment_proofs`

#### 18. ประวัติการสั่งซื้อทั้งหมด (Order History)
* **Controller/Action:** `PublicPageController.OrderHistory()` [GET]
* **ViewModel:** `OrderHistoryViewModel` (`CompletedOrders`)
* **ขั้นตอนการทำงาน:** แสดงคำสั่งซื้อที่เสร็จสิ้นสมบูรณ์ (Completed) หรือยกเลิก (Cancelled) ย้อนหลัง
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Order_items`, `Products`

#### 19. ขอยกเลิกคำสั่งซื้อ (Cancel Order)
* **Controller/Action:** `PublicPageController.CancelOrder(int orderId, string reason)` [POST]
* **ViewModel:** `CancelOrderViewModel` (`OrderId`, `Reason`)
* **ขั้นตอนการทำงาน:**
  1. ตรวจสอบว่าคำสั่งซื้อยังอยู่ในสถานะ `PendingPayment` หรือไม่
  2. คืนจำนวนสต็อกสินค้าใน `Products`
  3. เปลี่ยนสถานะคำสั่งซื้อเป็น `Cancelled`
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Order_items`, `Products`

#### 20. ยืนยันการรับสินค้า (Confirm Received)
* **Controller/Action:** `PublicPageController.ConfirmReceived(int orderId)` [POST]
* **ViewModel:** `ConfirmReceivedViewModel` (`OrderId`)
* **ขั้นตอนการทำงาน:** เปลี่ยนสถานะคำสั่งซื้อจาก `Shipping` เป็น `Completed`
* **ตารางที่เกี่ยวข้อง:** `Orders`

#### 21. แนบสลิปชำระเงิน (Upload Payment Proof)
* **Controller/Action:** `PublicPageController.UploadPaymentProof(UploadPaymentProofViewModel model)` [POST]
* **ViewModel:** `UploadPaymentProofViewModel` (`OrderId`, `SlipFile`, `TransferredAmount`, `TransferredAt`)
* **ขั้นตอนการทำงาน:**
  1. อัปโหลดไฟล์รูปภาพสลิปไปยังโฟลเดอร์เซิร์ฟเวอร์ (`wwwroot/uploads/slips`)
  2. บันทึกข้อมูลลงตาราง `Payment_proofs`
  3. ปรับสถานะคำสั่งซื้อใน `Orders` เป็น `Verifying` และ `payment_status` เป็น `PendingVerification`
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Payment_proofs`

---

### 🔹 กลุ่มที่ 3: ระบบหลังบ้านและการบริหารจัดการ (BackOfficeController - 19 ฟังก์ชัน)

#### 22. หน้าแดชบอร์ดภาพรวมผู้บริหาร (BackOffice Dashboard / AdminPage)
* **Controller/Action:** `BackOfficeController.AdminPage()` [GET]
* **ViewModel:** `DashboardSummaryViewModel` (`TotalRevenue`, `NetProfit`, `TotalOrders`, `TopSellingProducts`, `RecentOrders`)
* **ขั้นตอนการทำงาน:** ดึงสถิติยอดขายรวม ออเดอร์ที่รอตรวจสอบ สินค้าขายดี 5 อันดับแรก มาแสดงผลเชิงกราฟและตัวเลขสรุป
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Order_items`, `Products`, `Users`

#### 23. รายการคำสั่งซื้อหลังบ้าน (OrderMenu)
* **Controller/Action:** `BackOfficeController.OrderMenu(string statusFilter, string dateRange)` [GET]
* **ViewModel:** `OrderManagementViewModel` (`Orders`, `FilterStatus`)
* **ขั้นตอนการทำงาน:** กรองและแสดงผลคำสั่งซื้อของร้าน แยกตามสถานะ (รอตรวจสลิป, รอแพ็กสินค้า, กำลังจัดส่ง)
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Users`, `Payment_proofs`

#### 24. อัปเดตสถานะคำสั่งซื้อ (UpdateOrderStatus)
* **Controller/Action:** `BackOfficeController.UpdateOrderStatus(int orderId, string newStatus)` [POST]
* **ViewModel:** `UpdateOrderStatusViewModel` (`OrderId`, `NewStatus`)
* **ขั้นตอนการทำงาน:** เจ้าหน้าที่เปลี่ยนสถานะ เช่น จาก `Preparing` เป็น `Shipping`
* **ตารางที่เกี่ยวข้อง:** `Orders`

#### 25. ตรวจสอบและอัปเดตสถานะชำระเงิน (UpdatePaymentStatus)
* **Controller/Action:** `BackOfficeController.UpdatePaymentStatus(int orderId, string paymentStatus, string note)` [POST]
* **ViewModel:** `UpdatePaymentStatusViewModel` (`OrderId`, `PaymentStatus`, `Note`)
* **ขั้นตอนการทำงาน:**
  1. เจ้าหน้าที่ตรวจสอบยอดเงินและวันเวลาในรูปภาพสลิป
  2. หากสลิปถูกต้อง อัปเดต `payment_status` เป็น `Paid` และปรับสถานะออเดอร์เป็น `Preparing`
  3. บันทึกรหัสเจ้าหน้าที่ผู้ตรวจและเวลาลง `Payment_proofs`
* **ตารางที่เกี่ยวข้อง:** `Orders`, `Payment_proofs`

#### 26. ตอบกลับและส่งข้อความถึงลูกค้าในคำสั่งซื้อ (ReplyOrder)
* **Controller/Action:** `BackOfficeController.ReplyOrder(int orderId, string message)` [POST]
* **ViewModel:** `ReplyOrderViewModel` (`OrderId`, `Message`)
* **ขั้นตอนการทำงาน:** บันทึกข้อความแจ้งเตือนจากเจ้าหน้าที่ลงตาราง `Order_replies` เพื่อให้ลูกค้าเห็นในหน้าติดตามออเดอร์
* **ตารางที่เกี่ยวข้อง:** `Order_replies`

#### 27. ค้นหาและกรองรายการสินค้า (MenuProducts - Search & Filter)
* **Controller/Action:** `BackOfficeController.MenuProducts(int? categoryId, string keyword)` [GET]
* **ViewModel:** `ProductManagementViewModel` (`Products`, `Categories`, `Keyword`)
* **ขั้นตอนการทำงาน:** แสดงรายการสินค้าในระบบทั้งหมดพร้อมสถานะสต็อกและปุ่มจัดการแก้ไข/ลบ
* **ตารางที่เกี่ยวข้อง:** `Products`, `Categories`

#### 28. เพิ่มสินค้าใหม่ (Create Product)
* **Controller/Action:** `BackOfficeController.MenuProducts(CreateProductViewModel model)` [POST]
* **ViewModel:** `CreateProductViewModel` (`ProductName`, `CategoryId`, `Price`, `StockQuantity`, `ImageFile`, `Description`)
* **ขั้นตอนการทำงาน:** อัปโหลดรูปภาพสินค้า และบันทึกข้อมูลสินค้าใหม่ลงตาราง `Products`
* **ตารางที่เกี่ยวข้อง:** `Products`

#### 29. แก้ไขข้อมูลสินค้า (Update Product)
* **Controller/Action:** `BackOfficeController.UpdateProduct(EditProductViewModel model)` [POST]
* **ViewModel:** `EditProductViewModel` (`ProductId`, `ProductName`, `CategoryId`, `Price`, `StockQuantity`, `ImageFile`, `IsActive`)
* **ขั้นตอนการทำงาน:** ปรับปรุงราคา จำนวนสต็อก คำอธิบาย และเปลี่ยนรูปภาพสินค้า
* **ตารางที่เกี่ยวข้อง:** `Products`

#### 30. ลบสินค้า (Delete Product)
* **Controller/Action:** `BackOfficeController.DeleteProduct(int productId)` [POST]
* **ViewModel:** `DeleteProductViewModel` (`ProductId`)
* **ขั้นตอนการทำงาน:** ตรวจสอบการผูกพันกับคำสั่งซื้อ หากมีประวัติคำสั่งซื้อให้ใช้วิธี Soft Delete (`is_active = 0`) หากไม่มีให้ลบออกจากตาราง
* **ตารางที่เกี่ยวข้อง:** `Products`, `Order_items`

#### 31. สร้างหมวดหมู่สินค้าใหม่ (Create Category)
* **Controller/Action:** `BackOfficeController.CreateCategory(CreateCategoryViewModel model)` [POST]
* **ViewModel:** `CreateCategoryViewModel` (`CategoryName`, `Description`)
* **ขั้นตอนการทำงาน:** ตรวจสอบชื่อหมวดหมู่ไม่ให้ซ้ำ และบันทึกลงตาราง `Categories`
* **ตารางที่เกี่ยวข้อง:** `Categories`

#### 32. ค้นหาและกรองบัญชีผู้ใช้งาน (AccountManagement - Search & Filter)
* **Controller/Action:** `BackOfficeController.AccountManagement(int? roleId, string keyword)` [GET]
* **ViewModel:** `AccountManagementViewModel` (`Accounts`, `Roles`, `Keyword`)
* **ขั้นตอนการทำงาน:** แสดงรายชื่อผู้ใช้งานทั้งหมด พร้อมข้อมูล Role และสถานะบัญชี
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`

#### 33. สร้างบัญชีผู้ใช้งานใหม่ (Create Account)
* **Controller/Action:** `BackOfficeController.AccountManagement(CreateAccountViewModel model)` [POST]
* **ViewModel:** `CreateAccountViewModel` (`Username`, `Email`, `FullName`, `Password`, `RoleId`, `PhoneNumber`)
* **ขั้นตอนการทำงาน:** ผู้ดูแลระบบสร้างบัญชีสำหรับพนักงาน (Staff) หรือบทบาทอื่น แฮชรหัสผ่าน และบันทึกลงฐานข้อมูล
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`

#### 34. แก้ไขข้อมูลบัญชีผู้ใช้งาน (Edit Account)
* **Controller/Action:** `BackOfficeController.UpdateAccount(EditAccountViewModel model)` [POST]
* **ViewModel:** `EditAccountViewModel` (`UserId`, `FullName`, `Email`, `RoleId`, `PhoneNumber`)
* **ขั้นตอนการทำงาน:** อัปเดตข้อมูลผู้ใช้และปรับเปลี่ยนระดับสิทธิ์ (Role Assignment)
* **ตารางที่เกี่ยวข้อง:** `Users`, `Roles`

#### 35. ลบบัญชีผู้ใช้งาน (Delete Account)
* **Controller/Action:** `BackOfficeController.DeleteAccount(int userId)` [POST]
* **ViewModel:** `DeleteAccountViewModel` (`UserId`)
* **ขั้นตอนการทำงาน:** ลบบัญชีผู้ใช้งานที่ไม่ต้องการ (ป้องกันไม่ให้ลบบัญชีของตนเองที่กำลังล็อกอินอยู่)
* **ตารางที่เกี่ยวข้อง:** `Users`

#### 36. เปลี่ยนรหัสผ่านให้บัญชีผู้ใช้งาน (Change Account Password)
* **Controller/Action:** `BackOfficeController.ChangeAccountPassword(AdminChangePasswordViewModel model)` [POST]
* **ViewModel:** `AdminChangePasswordViewModel` (`UserId`, `NewPassword`, `ConfirmPassword`)
* **ขั้นตอนการทำงาน:** แอดมินทำการรีเซ็ตรหัสผ่านใหม่ให้กับผู้ใช้งาน แฮชรหัสผ่านใหม่และอัปเดตลงตาราง
* **ตารางที่เกี่ยวข้อง:** `Users`

#### 37. แสดงรายการโปรโมชั่นหลังบ้าน (Promotions Management)
* **Controller/Action:** `BackOfficeController.Promotions()` [GET]
* **ViewModel:** `PromotionManagementViewModel` (`Promotions`, `Categories`)
* **ขั้นตอนการทำงาน:** แสดงตารางโปรโมชั่นทั้งหมด พร้อมปุ่มเปิด/ปิด และปุ่มแก้ไขเงื่อนไข
* **ตารางที่เกี่ยวข้อง:** `Promotions`, `promotion_rules`

#### 38. สร้างแคมเปญโปรโมชั่นใหม่ (Create Promotion)
* **Controller/Action:** `BackOfficeController.Promotions(CreatePromotionViewModel model)` [POST]
* **ViewModel:** `CreatePromotionViewModel` (`PromoCode`, `Title`, `DiscountType`, `DiscountValue`, `MinOrderAmount`, `StartDate`, `EndDate`)
* **ขั้นตอนการทำงาน:** บันทึกข้อมูลแคมเปญลง `Promotions` และบันทึกเงื่อนไขขั้นต่ำลง `promotion_rules`
* **ตารางที่เกี่ยวข้อง:** `Promotions`, `promotion_rules`

#### 39. แก้ไขแคมเปญโปรโมชั่น (Edit Promotion)
* **Controller/Action:** `BackOfficeController.UpdatePromotion(EditPromotionViewModel model)` [POST]
* **ViewModel:** `EditPromotionViewModel` (`PromotionId`, `Title`, `DiscountValue`, `MinOrderAmount`, `StartDate`, `EndDate`, `IsActive`)
* **ขั้นตอนการทำงาน:** อัปเดตวันหมดอายุ มูลค่าส่วนลด และสถานะเปิด/ปิดการใช้งานโปรโมชั่น
* **ตารางที่เกี่ยวข้อง:** `Promotions`, `promotion_rules`

#### 40. ลบแคมเปญโปรโมชั่น (Delete Promotion)
* **Controller/Action:** `BackOfficeController.DeletePromotion(int promotionId)` [POST]
* **ViewModel:** `DeletePromotionViewModel` (`PromotionId`)
* **ขั้นตอนการทำงาน:** ลบกฎเงื่อนไขใน `promotion_rules` และลบแคมเปญใน `Promotions`
* **ตารางที่เกี่ยวข้อง:** `Promotions`, `promotion_rules`

---

## 8. สรุปจุดเด่นด้านเทคนิคและสถาปัตยกรรม (Architectural Highlights & Best Practices)

1. **การแบ่งชั้นสถาปัตยกรรม (Layered Architecture):**
   * **Presentation Layer:** ASP.NET Core MVC (Razor Views + ViewModels)
   * **Business Logic Layer:** มีการตรวจสอบ Validation, คำนวณโปรโมชั่น, คำนวณสต็อก และจัดการ State
   * **Data Access Layer:** Entity Framework Core ทำงานร่วมกับฐานข้อมูล MySQL มีการแยก Entities และ Context ชัดเจน
2. **ความปลอดภัยของระบบ (Security Practices):**
   * รหัสผ่านถูกเข้ารหัสด้วย Secure Hashing Algorithm
   * ระบบยืนยันตัวตน Cookie Authentication และ Role-Based Access Control (RBAC) ควบคุมใน Controller ด้วย `[Authorize(Roles = "...")]`
   * การกู้คืนรหัสผ่านด้วย Time-based OTP ผ่าน Email พร้อมจำกัดอายุ 5 นาที
   * การป้องกันการโจมตีพื้นฐาน: Anti-Forgery Token (`[ValidateAntiForgeryToken]`) ในทุก Form POST
3. **การออกแบบฐานข้อมูลตามหลัก Normalization:**
   * ตารางทั้ง 14 ตารางมีความสัมพันธ์แบบ 1-to-Many และ Many-to-Many ที่ถูกต้อง มี Foreign Key และ Index ครอบคลุมการค้นหา
4. **ความสมบูรณ์ของรายงานตามมาตรฐาน CSI402:**
   * ครอบคลุมทั้ง Requirements, Design (Flowchart & DFD), Database Schema, Implementation (ครบ 40 ฟังก์ชันพร้อมซอร์สโค้ดและ ViewModel) และผลการทดสอบการทำงานของแต่ละหน้าจออย่างละเอียดครบถ้วน 135 หน้า
