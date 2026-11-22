# คู่มือและโครงสร้างการจัดทำเล่มรายงานฉบับสมบูรณ์ (CSI402 Project Report Blueprint)
## โครงการ: PetStore Pro — ระบบการซื้อขายและของใช้สัตว์เลี้ยงออนไลน์
**วิชา:** CSI402 การเชื่อมต่อโปรแกรมส่วนหน้ากับโปรแกรมส่วนหลัง (Front-end and Back-end Integration)  
**คณะเทคโนโลยีสารสนเทศ มหาวิทยาลัยศรีปทุม**  
**อ้างอิงมาตรฐาน:** รูปแบบเอกสารเล่มรายงาน 135 หน้า (BakeQuery Standard)

---

## 📑 สารบัญโครงสร้างเล่มรายงานที่ต้องมี (Master Table of Contents)

หากต้องการทำเล่มรายงานให้ได้คะแนนเต็มและได้มาตรฐานเดียวกับตัวอย่างเล่มรายงาน เล่มจะต้องประกอบด้วย **5 บทหลัก** ดังนี้:

* **ส่วนนำ (Preliminary Pages):**
  * ปกนอก / ปกใน (ระบุชื่อระบบ, รหัสนักศึกษา, ชื่อ-นามสกุล, อาจารย์ที่ปรึกษา, วิชา, คณะ, สถาบัน)
  * กิตติกรรมประกาศ (Acknowledgements)
  * บทคัดย่อภาษาไทย / ภาษาอังกฤษ (Abstract)
  * สารบัญเนื้อหา (Table of Contents), สารบัญตาราง (List of Tables), สารบัญภาพ (List of Figures)
* **บทที่ 1: บทนำและวัตถุประสงค์ (Introduction & Project Scope):**
  * 1.1 ความเป็นมาและความสำคัญของปัญหา
  * 1.2 วัตถุประสงค์ของโครงการ
  * 1.3 ขอบเขตของโครงการ (10 บทบาทผู้ใช้งาน, 27 ข้อกำหนดความต้องการ, 5 แคมเปญโปรโมชั่น)
  * 1.4 ประโยชน์ที่คาดว่าจะได้รับ
  * 1.5 เครื่องมือและเทคโนโลยีที่ใช้ในการพัฒนา (.NET 10, ASP.NET Core MVC, Entity Framework Core, MySQL, Bootstrap/Custom CSS)
* **บทที่ 2: ทฤษฎีและงานวิจัยที่เกี่ยวข้อง (Literature Review & Conceptual Framework):**
  * 2.1 สถาปัตยกรรม MVC (Model-View-Controller)
  * 2.2 การจัดการความปลอดภัย Role-Based Access Control (RBAC) และ Session Management
  * 2.3 การคำนวณโปรโมชั่นแบบ Real-time และระบบโลจิสติกส์คำนวณตามน้ำหนัก (Weight-based Shipping)
  * 2.4 ระบบติดตามการดำเนินงาน (Audit Trail & Logging)
* **บทที่ 3: การวิเคราะห์และออกแบบระบบ (System Analysis & Design):**
  * 3.1 ผังการทำงานของระบบ (System Flowcharts: ครบทุกฟังก์ชันงาน 15 ผังงาน) ⭐
  * 3.2 แผนภาพกระแสข้อมูล (Data Flow Diagrams: Context Diagram Level 0, DFD Level 1, DFD Level 2)
  * 3.3 พจนานุกรมข้อมูล (Data Dictionary: 13 ตารางฐานข้อมูล และ Enums E1–E6)
  * 3.4 แผนภาพความสัมพันธ์ของข้อมูล (ER-Diagram / Database Relational Schema)
  * 3.5 การออกแบบส่วนติดต่อผู้ใช้งาน (UX/UI Low-Fidelity Wireframes 17 หน้า)
* **บทที่ 4: การพัฒนาและการทำงานของระบบ (System Implementation & Output Results):**
  * บันทึกรายละเอียดการทำงานของทุก Controller และ Action Method (แบ่งเป็น 30+ ฟังก์ชัน)
  * ในแต่ละฟังก์ชันระบุ: (1) ชื่อฟังก์ชัน (2) Controller/Action (3) ViewModel (4) ตาราง DB ที่เกี่ยวข้อง (5) ลำดับการทำงาน (6) ซอร์สโค้ดส่วนสำคัญ (7) รูปภาพหน้าจอผลลัพธ์การทำงานจริง
* **บทที่ 5: สรุปผล ปัญหาอุปสรรค และข้อเสนอแนะ (Conclusion & Future Work):**
  * 5.1 สรุปผลการดำเนินงานเทียบกับวัตถุประสงค์
  * 5.2 ปัญหาและอุปสรรคที่พบระหว่างการพัฒนา พร้อมแนวทางแก้ไข
  * 5.3 ข้อเสนอแนะในการพัฒนาต่อยอด (Future Enhancements เช่น Payment Gateway จริง, Webhook ขนส่ง)
* **บรรณานุกรม (References) & ภาคผนวก (Appendix):**
  * สคริปต์ฐานข้อมูล SQL (DDL Schema), คู่มือการติดตั้งและทดสอบระบบ

---

## 🧭 รวมผังงานระบบ (System Flowcharts ครบทุกฟังก์ชันงาน 15 ผังงาน)

ส่วนนี้คืองานสำคัญที่สุดที่อาจารย์ตรวจเข้มงวด โดยได้ออกแบบผังงานตาม Flow การทำงานจริงของระบบ PetStore Pro ในโปรเจกต์นี้ ครบทุกฟังก์ชัน ทุกบทบาท:

---

### ผังงานที่ 1: ภาพรวมระบบและการจำแนกบทบาท (Overall System Architecture Flowchart)
ผังงานภาพรวมการเข้าใช้งานระบบตั้งแต่ผู้ใช้เข้าสู่เว็บไซต์ จนถึงการแยกเส้นทางระหว่างหน้าร้าน (Storefront) และหลังบ้าน (Back-Office)

```mermaid
flowchart TD
    Start([เริ่มต้น: ผู้ใช้เปิดเว็บไซต์]) --> CheckAuth{ผู้ใช้เข้าสู่ระบบหรือไม่?}
    
    CheckAuth -- ยังไม่เข้าสู่ระบบ --> GuestRole[กำหนดบทบาทอัตโนมัติ: Guest]
    GuestRole --> ViewStorefront[เข้าสู่หน้าหลักร้านค้า / แคตตาล็อกสินค้า]
    ViewStorefront --> BrowseCatalog[ค้นหา / กรองตามประเภทสัตว์ ช่วงวัย น้ำหนัก]
    BrowseCatalog --> SimCart[เพิ่มสินค้าทดลองใส่ตะกร้า / ดูส่วนลดจำลอง]
    SimCart --> ReqLoginPrompt{ต้องการกด Checkout ชำระเงิน?}
    ReqLoginPrompt -- ใช่ --> LoginPage[นำทางไปหน้าเข้าสู่ระบบ / สมัครสมาชิก]
    ReqLoginPrompt -- ไม่ใช่ --> BrowseCatalog
    
    CheckAuth -- เข้าสู่ระบบแล้ว --> CheckUserRole{ตรวจสอบ Role ผู้ใช้งาน}
    
    CheckUserRole -- Customer --> CustPortal[เข้าสู่ระบบหน้าร้าน: สั่งซื้อ, ตะกร้าจริง, ติดตามพัสดุ, แจ้งปัญหา]
    CheckUserRole -- InventoryStaff --> InvPortal[Back-Office: คลังสินค้า / สต็อก / Low Stock Alert]
    CheckUserRole -- OrderStaff --> OrdPortal[Back-Office: จัดเตรียมออเดอร์ / คัดของแถม / ใบแพ็กเกจ]
    CheckUserRole -- ShippingStaff --> ShipPortal[Back-Office: ชั่งน้ำหนักจริง / คีย์ Tracking / เปลี่ยนสถานะส่ง]
    CheckUserRole -- MarketingStaff --> MktPortal[Back-Office: จัดการ 5 แคมเปญโปรโมชั่น / ดูสถิติการใช้งาน]
    CheckUserRole -- AccountingStaff --> AccPortal[Back-Office: ตรวจสลิปโอน / อนุมัติคืนเงิน / สรุปรายรับรายจ่าย]
    CheckUserRole -- ITSupport --> ItPortal[Back-Office: จัดการตั๋ว Helpdesk / บันทึกประวัติการแก้ไขระบบ]
    CheckUserRole -- Admin --> AdmPortal[Back-Office: จัดการบัญชีผู้ใช้ / กำหนดสิทธิ์ Role Matrix / ดู Audit Log]
    CheckUserRole -- Owner --> OwnPortal[Back-Office: Executive BI Dashboard / สรุปกำไร / เข้าถึงทุกแผนก]
    
    CustPortal --> End([สิ้นสุดการทำงาน])
    InvPortal --> End
    OrdPortal --> End
    ShipPortal --> End
    MktPortal --> End
    AccPortal --> End
    ItPortal --> End
    AdmPortal --> End
    OwnPortal --> End
```

---

### ผังงานที่ 2: ระบบยืนยันตัวตนและการสลับสิทธิ์ (Authentication & Fast Role Switch Flowchart)
ครอบคลุมการ Login, Register, Logout และกลไก Fast Role Switch เพื่อการทดสอบสิทธิ์ 10 บทบาท

```mermaid
flowchart TD
    StartAuth([เริ่มต้น: ผู้ใช้เลือกเข้าสู่ระบบ]) --> InputCreds[กรอก Username และ Password หรือเลือกปุ่ม Fast Switch]
    InputCreds --> VerifyDB{ตรวจสอบข้อมูลในตาราง User}
    
    VerifyDB -- ไม่พบ / รหัสผิด --> LoginFail[แจ้งเตือนข้อผิดพลาด: Username หรือ Password ไม่ถูกต้อง]
    LoginFail --> InputCreds
    
    VerifyDB -- ข้อมูลถูกต้อง --> CheckActive{บัญชีเปิดใช้งานอยู่หรือไม่? IsActive == 1}
    CheckActive -- โดนระงับ --> SuspendedAlert[แจ้งเตือน: บัญชีนี้ถูกระงับโดยผู้ดูแลระบบ]
    SuspendedAlert --> EndAuth([สิ้นสุด])
    
    CheckActive -- ใช้งานได้ปกติ --> SetClaims[สร้าง User Claims: Id, Name, Role, GrantedPermissions]
    SetClaims --> SignInCookie[บันทึก Cookie Authentication & Session Storage]
    SignInCookie --> AuditLogLogin[บันทึกประวัติการล็อกอินลงตาราง AuditLog]
    AuditLogLogin --> RoleRoute{ตรวจสอบ Role}
    
    RoleRoute -- Customer / Guest --> GoHome[ไปที่หน้าแรก Storefront / Home/Index]
    RoleRoute -- Staff / Admin / Owner --> GoBackOffice[ไปที่หน้างานหลังบ้านของ Role นั้นๆ]
    
    GoHome --> EndAuth
    GoBackOffice --> EndAuth
```

---

### ผังงานที่ 3: หน้าร้าน แคตตาล็อก และระบบกรองสินค้าอัจฉริยะ (Storefront Multi-Filter & Social Proof)
รองรับ Requirements ข้อ 1, 2, 7, 8, 9, 10 (แยกหมา/แมว, กรองน้ำหนัก, กรองช่วงวัย Layer, ตัวนับคนใส่ตะกร้า)

```mermaid
flowchart TD
    StartCat([เริ่มต้น: เข้าหน้าสินค้า Home/Index]) --> LoadFilters[รับค่า Parameter ตัวกรอง: PetType, Category, AgeStage, Weight, Search]
    LoadFilters --> QueryDB[ดึงข้อมูลสินค้าจากตาราง Product เฉพาะ IsActive == 1]
    
    QueryDB --> FilterProcess[ประมวลผลตัวกรอง: สุนัข/แมว/สัตว์อื่น, ช่วงวัย ลูกสัตว์/โตเต็มวัย/สูงอายุ, พิกัดน้ำหนัก]
    FilterProcess --> CountCartUsers[คำนวณจำนวนผู้ใช้ที่ใส่สินค้านี้ในตะกร้า CartCount จากตาราง CartItem]
    CountCartUsers --> RenderUI[แสดงรายการสินค้าแบบ Card Grid พร้อม Badge ช่วงวัย และจำนวนคนใส่ตะกร้า]
    
    RenderUI --> UserAction{ผู้ใช้งานทำอะไรต่อ?}
    UserAction -- คลิกดูรายละเอียด --> ViewDetail[ไปที่หน้า Home/Details แสดงข้อมูลโภชนาการ น้ำหนัก และสินค้าแนะนำ]
    UserAction -- กดปุ่มเพิ่มลงตะกร้า --> CallAddToCart[เรียกฟังก์ชัน Cart/AddToCart ด้วย AJAX 0ms]
    UserAction -- เปลี่ยนตัวกรอง --> LoadFilters
```

---

### ผังงานที่ 4: ระบบตะกร้าสินค้าและการคำนวณ 5 โปรโมชั่นอัตโนมัติ (Cart & 5-Promotion Engine)
รองรับ Requirements ข้อ 11, 12, 13 และโปรโมชั่นทั้ง 5 รูปแบบ (คำนวณส่วนลดอัตโนมัติแบบ Real-time)

```mermaid
flowchart TD
    StartCart([เริ่มต้น: เปิดหน้าตะกร้าสินค้า Cart/Index]) --> FetchCart[ดึงรายการ CartItems ของลูกค้ารายนี้]
    FetchCart --> CalcSubtotal[คำนวณยอดรวมราคาสินค้า Subtotal และน้ำหนักรวม TotalWeight]
    
    CalcSubtotal --> EvalPromo1{โปร 1: ซื้อครบ 3 ชิ้น?}
    EvalPromo1 -- ครบ 3 ชิ้นขึ้นไป --> ApplyPromo1[หักส่วนลดทันที 50 บาท บันทึกลง AppliedPromotionSummary]
    EvalPromo1 -- ไม่เข้าเกณฑ์ --> EvalPromo2
    ApplyPromo1 --> EvalPromo2
    
    EvalPromo2{โปร 2: ซื้ออาหาร >= 2 รายการ?}
    EvalPromo2 -- ใช่ --> ApplyPromo2[ลดราคาเฉพาะสินค้าอาหาร 10%]
    EvalPromo2 -- ไม่ใช่ --> EvalPromo4
    ApplyPromo2 --> EvalPromo4
    
    EvalPromo4{โปร 4: ยอดซื้อครบ 1,000 บาท?}
    EvalPromo4 -- ครบ --> ApplyPromo4[ปลดล็อกของแถมฟรี: เสื้อสัตว์เลี้ยงพรีเมียม 1 ตัว]
    EvalPromo4 -- ไม่ครบ --> EvalPromo5
    ApplyPromo4 --> EvalPromo5
    
    EvalPromo5{โปร 5: อาหารแบรนด์เดียวกัน >= 2 ชิ้น?}
    EvalPromo5 -- ใช่ --> ApplyPromo5[ลดราคาอาหารแบรนด์ที่ซ้ำกัน 15%]
    EvalPromo5 -- ไม่ใช่ --> CalcShipping
    ApplyPromo5 --> CalcShipping
    
    CalcShipping[ส่งข้อมูลไปคำนวณค่าจัดส่งตามน้ำหนัก และตรวจสอบโปรโมชั่นที่ 3]
    CalcShipping --> RenderCartView[แสดงผล: ราคารวม ส่วนลด ของแถม และค่าจัดส่ง]
    RenderCartView --> EndCart([สิ้นสุด])
```

---

### ผังงานที่ 5: ระบบคำนวณค่าจัดส่งตามน้ำหนักรวม (Weight-Based Shipping Calculation Flowchart)
รองรับ Requirements ข้อ 3, 4, 14, 15, 16 (เกณฑ์น้ำหนัก 10 kg, ส่วนเกิน 20 บาท/kg, และโปรโมชั่นจัดส่งฟรี)

```mermaid
flowchart TD
    StartShip([เริ่มต้น: คำนวณค่าจัดส่ง]) --> SumWeight[คำนวณน้ำหนักรวมของสินค้าทั้งหมด: TotalWeight]
    SumWeight --> CheckPromo3{ยอดซื้อถึง 800 บาท และน้ำหนักรวม <= 10 kg หรือไม่? โปร 3}
    
    CheckPromo3 -- เข้าเกณฑ์โปร 3 --> FreeBaseShip[ค่าจัดส่งฐาน BaseShipping = 0 บาท ได้รับสิทธิ์ส่งฟรี]
    CheckPromo3 -- ไม่เข้าเกณฑ์โปร 3 --> NormalBaseShip[ค่าจัดส่งฐาน BaseShipping = 50 บาท มาตรฐาน]
    
    FreeBaseShip --> CheckWeightLimit{น้ำหนักรวม TotalWeight > 10 kg หรือไม่?}
    NormalBaseShip --> CheckWeightLimit
    
    CheckWeightLimit -- ไม่เกิน 10 kg --> FinalShipCost[ค่าจัดส่งสุทธิ = BaseShipping]
    CheckWeightLimit -- เกิน 10 kg --> CalcOverweight[คำนวณน้ำหนักส่วนเกิน: OverWeight = TotalWeight - 10]
    CalcOverweight --> AddExtraCost[คิดค่าบริการเพิ่ม: ExtraCost = OverWeight * 20 บาท]
    AddExtraCost --> FinalShipCostWithExtra[ค่าจัดส่งสุทธิ = BaseShipping + ExtraCost]
    
    FinalShipCost --> DisplaySummary[แสดงค่าจัดส่งในหน้าตะกร้าและใบสั่งซื้อ]
    FinalShipCostWithExtra --> DisplaySummary
    DisplaySummary --> EndShip([สิ้นสุด])
```

---

### ผังงานที่ 6: การสั่งซื้อ ชำระเงิน และแนบสลิป (Checkout & Payment Flowchart)
รองรับ Requirements ข้อ 19 (เลือกที่อยู่จัดส่ง, ชำระเงิน QR PromptPay, แนบหลักฐานสลิป)

```mermaid
flowchart TD
    StartCheckout([เริ่มต้น: ลูกค้ากดดำเนินการสั่งซื้อ Checkout]) --> LoadAddress[โหลดสมุดที่อยู่จัดส่ง UserAddress ของลูกค้า]
    LoadAddress --> SelectAddr{ลูกค้ามีที่อยู่หรือไม่?}
    
    SelectAddr -- ไม่มีที่อยู่ --> AddNewAddr[กรอกฟอร์มเพิ่มที่อยู่ใหม่ บันทึกลงตาราง UserAddress]
    AddNewAddr --> ConfirmAddr
    SelectAddr -- มีที่อยู่แล้ว --> ChooseAddr[เลือกที่อยู่ปลายทางที่ต้องการจัดส่ง]
    ChooseAddr --> ConfirmAddr[ยืนยันที่อยู่จัดส่ง]
    
    ConfirmAddr --> CalcFinalBill[สรุปยอดรวมสุทธิ: ราคาสินค้า - ส่วนลด + ค่าส่ง]
    CalcFinalBill --> GenQR[สร้าง QR Code พร้อมเพย์สำหรับโอนเงินตามยอดสุทธิ]
    GenQR --> CustPayment[ลูกค้าสแกนจ่ายเงินผ่านแอปธนาคาร]
    CustPayment --> UploadSlipForm[ลูกค้าแนบรูปภาพสลิปหลักฐานการโอนเงิน]
    
    UploadSlipForm --> SubmitOrder[กดปุ่มยืนยันคำสั่งซื้อ Confirm Order]
    SubmitOrder --> SaveOrderDB[สร้าง Order ในตาราง Orders: สถานะ = PendingPayment]
    SaveOrderDB --> SaveItemsDB[คัดลอกสินค้าจาก CartItem ไปยัง OrderItem]
    SaveItemsDB --> DeductStock[ตัดสต็อกสินค้าในตาราง Product ทันที]
    DeductStock --> ClearUserCart[ล้างตะกร้าสินค้าของลูกค้า]
    ClearUserCart --> ShowSuccess[แสดงหน้าคำสั่งซื้อสำเร็จ พร้อมเลข OrderId]
    ShowSuccess --> EndCheckout([สิ้นสุด])
```

---

### ผังงานที่ 7: ลูกค้าติดตามคำสั่งซื้อ ขอคืนเงิน และแจ้งปัญหา (Customer Orders, Refund & Helpdesk)
รองรับ Requirements ข้อ 20, 22, 24 (Step Progress, ขอคืนเงิน, และเปิดตั๋ว Support Ticket)

```mermaid
flowchart TD
    StartCustHub([เริ่มต้น: ลูกค้าเข้าหน้าประวัติคำสั่งซื้อ /Account/Orders]) --> FetchOrders[ดึงรายการคำสั่งซื้อของตนเอง]
    FetchOrders --> DisplayTimeline[แสดง Step Progress: รอชำระ -> กำลังเตรียม -> จัดส่งแล้ว -> สำเร็จ]
    
    DisplayTimeline --> CustAction{ลูกค้าเลือกทำรายการใด?}
    
    CustAction -- ดูรายละเอียดคำสั่งซื้อ --> ViewOrderDetail[แสดงรายการสินค้า น้ำหนัก ของแถม และเลข Tracking]
    
    CustAction -- ขอยกเลิก / ขอคืนเงิน --> CheckEligible{สถานะเป็น PendingPayment หรือได้รับสินค้าแล้วชำรุด?}
    CheckEligible -- ยกเลิกได้ --> FillRefundForm[กรอกเหตุผลขอคืนเงิน แนบภาพสินค้าชำรุด]
    FillRefundForm --> SetRefundStatus[อัปเดต Orders.OrderStatus = RefundRequested]
    SetRefundStatus --> AlertAccounting[ส่งเรื่องไปยัง Accounting Staff เพื่อรอการอนุมัติ]
    
    CustAction -- แจ้งปัญหาการใช้งาน --> OpenTicket[เข้าหน้าเปิดตั๋ว /Support/CreateTicket]
    OpenTicket --> FillTicket[กรอกหัวข้อปัญหา หมวดหมู่ และรายละเอียดปัญหา]
    FillTicket --> SaveTicket[บันทึกลงตาราง SupportTicket: สถานะ = New]
    SaveTicket --> NotifyIT[แจ้งเตือนทีม IT Support เข้าตรวจสอบ]
    
    AlertAccounting --> EndCustHub([สิ้นสุด])
    NotifyIT --> EndCustHub
    ViewOrderDetail --> EndCustHub
```

---

### ผังงานที่ 8: คลังสินค้าและสินค้าคงคลัง (Inventory Staff Flowchart)
รองรับ Requirements ข้อ 6, 17, 18 (สต็อกสินค้า, ปรับยอดด่วน In/Out, แจ้งเตือน Low Stock, เพิ่ม/แก้สินค้า)

```mermaid
flowchart TD
    StartInv([เริ่มต้น: เจ้าหน้าที่คลังเข้า /Inventory]) --> LoadProducts[โหลดรายการสินค้าทั้งหมดในระบบ]
    LoadProducts --> CheckLowStock{มีสินค้าที่ StockQuantity <= MinStockThreshold หรือไม่?}
    
    CheckLowStock -- มีสินค้าใกล้หมด --> ShowAlertBadge[แสดงแถบแจ้งเตือน Low Stock Alert สีแดงเตือนเจ้าหน้าที่]
    CheckLowStock -- ไม่มี --> NormalGrid[แสดงตารางสต็อกสินค้าปกติ]
    
    ShowAlertBadge --> StaffAction{เจ้าหน้าที่เลือกดำเนินการ}
    NormalGrid --> StaffAction
    
    StaffAction -- ปรับปรุงยอดด่วน Quick In/Out --> DirectAdjust[กดปุ่ม + หรือ - หน้าตาราง เพื่อเพิ่ม/ตัดยอดสต็อก]
    DirectAdjust --> UpdateDB[อัปเดตตาราง Product บันทึกลง AuditLog]
    
    StaffAction -- เพิ่มสินค้าใหม่ --> OpenCreateForm[กรอกชื่อ, หมวด, ประเภทสัตว์, ช่วงวัย, ราคา, น้ำหนัก, รูป]
    OpenCreateForm --> InsertProduct[บันทึกลงตาราง Product และเพิ่มสต็อกเริ่มต้น]
    
    StaffAction -- แก้ไขข้อมูลสินค้า --> OpenEditForm[แก้ไขรายละเอียดสินค้า น้ำหนัก จุดสั่งซื้อซ้ำ]
    OpenEditForm --> UpdateProductDB[อัปเดตตาราง Product]
    
    UpdateDB --> EndInv([สิ้นสุด])
    InsertProduct --> EndInv
    UpdateProductDB --> EndInv
```

---

### ผังงานที่ 9: การจัดการคำสั่งซื้อและเตรียมสินค้า (Order Staff Flowchart)
รองรับ Requirements ข้อ 42, 43, 44 (ตรวจสลิป, ตรวจสอบของแถม, เตรียมสินค้า, ออกใบแพ็กของ)

```mermaid
flowchart TD
    StartOrd([เริ่มต้น: เจ้าหน้าที่ออเดอร์เข้า /Order]) --> FetchPaidOrders[ดึงรายการคำสั่งซื้อที่ชำระเงินแล้ว PaymentVerified]
    FetchPaidOrders --> DisplayQueue[แสดงคิวงานคำสั่งซื้อที่ต้องเตรียมสินค้า]
    
    DisplayQueue --> SelectOrder[เลือกคำสั่งซื้อเพื่อดำเนินการเตรียมสินค้า]
    SelectOrder --> CheckGiftItems[ตรวจสอบของแถมโปรโมชั่นที่ระบบคำนวณไว้ เช่น เสื้อฟรี]
    CheckGiftItems --> PrintPackingSlip[พิมพ์ใบรายการจัดเตรียมสินค้า Packing Slip]
    
    PrintPackingSlip --> PackGoods[พนักงานหยิบสินค้าและของแถมลงกล่องพัสดุ]
    PackGoods --> UpdateStatusPreparing[กดปุ่มเปลี่ยนสถานะ Order เป็น Preparing กำลังจัดเตรียม]
    UpdateStatusPreparing --> PackComplete[แพ็กสินค้าเรียบร้อย รอส่งต่อให้ฝ่ายจัดส่ง]
    PackComplete --> EndOrd([สิ้นสุด])
```

---

### ผังงานที่ 10: ฝ่ายจัดส่งและเลขพัสดุ (Shipping Staff Flowchart)
รองรับ Requirements ข้อ 4, 14, 16, 45, 46, 47 (ชั่งน้ำหนักจริง, กรอกเลข Tracking, เปลี่ยนสถานะเป็น Shipped)

```mermaid
flowchart TD
    StartShipStaff([เริ่มต้น: พนักงานฝ่ายจัดส่งเข้า /Shipping]) --> LoadReadyOrders[ดึงรายการคำสั่งซื้อที่สถานะเป็น Preparing]
    LoadReadyOrders --> PickPackage[เลือกกล่องพัสดุที่รอส่ง]
    
    PickPackage --> WeighPackage[ชั่งน้ำหนักจริงของกล่องพัสดุ RealWeight]
    WeighPackage --> CompareWeight{น้ำหนักจริงตรงกับที่คำนวณในระบบหรือไม่?}
    
    CompareWeight -- เกินเกณฑ์ผิดปกติ --> FlagDiscrepancy[บันทึกหมายเหตุส่วนต่างน้ำหนักเพื่อตรวจสอบ]
    CompareWeight -- ถูกต้องปกติ --> InputTracking[กรอกเลขพัสดุ Tracking Number เช่น KERRY12345678]
    FlagDiscrepancy --> InputTracking
    
    InputTracking --> SelectCarrier[เลือกบริษัทขนส่ง: Kerry, Flash, Thailand Post]
    SelectCarrier --> UpdateShipped[กดปุ่มบันทึก: เปลี่ยน OrderStatus = Shipped]
    UpdateShipped --> NotifyCustomer[ระบบอัปเดตให้ลูกค้าสามารถนำเลข Tracking ไปตรวจเช็กได้]
    NotifyCustomer --> EndShipStaff([สิ้นสุด])
```

---

### ผังงานที่ 11: ฝ่ายการตลาดและแคมเปญโปรโมชั่น (Marketing Staff Flowchart)
รองรับ Requirements ข้อ 5, 25, 26, 27 (จัดการ 5 แคมเปญ, Rule Builder, เปิด/ปิด, ดูสถิติการใช้งาน)

```mermaid
flowchart TD
    StartMkt([เริ่มต้น: เจ้าหน้าที่การตลาดเข้า /Marketing]) --> LoadPromos[โหลดรายการแคมเปญโปรโมชั่นทั้ง 5 แคมเปญ]
    LoadPromos --> ShowPromoStats[แสดงจำนวนครั้งที่แต่ละโปรโมชั่นถูกนำไปใช้งาน TimesApplied]
    
    ShowPromoStats --> MktChoice{เจ้าหน้าที่เลือกการทำงาน}
    
    MktChoice -- เปิด/ปิดใช้งานสวิตช์ด่วน --> ToggleActive[คลิก Toggle เพื่อเปลี่ยน IsActive = True/False]
    ToggleActive --> SaveActiveStatus[บันทึกลงตาราง Promotion มีผลกับหน้าร้านทันที]
    
    MktChoice -- แก้ไขเงื่อนไขโปรโมชั่น --> OpenRuleBuilder[เปิดฟอร์ม Rule Builder]
    OpenRuleBuilder --> SetConditions[กำหนด: ยอดซื้อขั้นต่ำ, จำนวนชิ้นขั้นต่ำ, แบรนด์ที่ร่วมรายการ]
    SetConditions --> SetDateWindow[กำหนดวันเวลาเริ่มต้น StartDate และสิ้นสุด EndDate]
    SetDateWindow --> SavePromoRules[บันทึกลงตาราง Promotion และแจ้งเตือน AuditLog]
    
    SaveActiveStatus --> EndMkt([สิ้นสุด])
    SavePromoRules --> EndMkt
```

---

### ผังงานที่ 12: การเงิน บัญชี และตรวจสอบสลิป (Accounting Staff Flowchart)
รองรับ Requirements ข้อ 19, 20, 21, 51, 52 (ตรวจสลิป, อนุมัติคืนเงิน, รายงานสรุปรายรับ-รายจ่าย)

```mermaid
flowchart TD
    StartAcc([เริ่มต้น: ฝ่ายการเงินเข้า /Accounting]) --> LoadFinanceDashboard[โหลดหน้าแดชบอร์ดการเงินและแท็บงาน]
    
    LoadFinanceDashboard --> AccTask{เลือกแท็บการทำงาน}
    
    AccTask -- ตรวจสอบสลิปโอนเงิน --> LoadUnverified[ดึงคำสั่งซื้อที่ลูกค้ารอตรวจสลิป]
    LoadUnverified --> ViewSlip[เปิดดูรูปภาพสลิป และตรวจสอบยอดโอนวันเวลา]
    ViewSlip --> CheckSlipValid{ยอดเงินในสลิปถูกต้องหรือไม่?}
    CheckSlipValid -- ถูกต้อง --> ApprovePayment[เปลี่ยนสถานะเป็น PaymentVerified ส่งต่อให้ฝ่าย Order]
    CheckSlipValid -- ไม่ถูกต้อง/สลิปปลอม --> RejectPayment[เปลี่ยนสถานะเป็น PaymentFailed แจ้งลูกค้าอัปโหลดใหม่]
    
    AccTask -- จัดการการขอคืนเงิน Refund --> LoadRefundReqs[ดึงรายการที่ลูกค้ายื่นขอคืนเงิน RefundRequested]
    LoadRefundReqs --> ReviewRefundProof[ตรวจสอบเหตุผลและรูปถ่ายสินค้าชำรุด]
    ReviewRefundProof --> ApproveRefund{อนุมัติการคืนเงินหรือไม่?}
    ApproveRefund -- อนุมัติ --> ProcessRefund[แนบสลิปโอนเงินคืนลูกค้า และเปลี่ยนสถานะเป็น Refunded]
    ApproveRefund -- ปฏิเสธ --> RejectRefund[ระบุเหตุผลไม่อนุมัติคืนเงิน]
    
    AccTask -- ดูรายงานรายรับ-รายจ่าย --> ViewFinancialReport[ดึงข้อมูลจาก FinancialSummary และ MonthlyFinancialEntry]
    ViewFinancialReport --> ExportStatement[แสดงผลกำไรสุทธิ รายรับรวม รายจ่ายต้นทุน และพิมพ์รายงาน]
    
    ApprovePayment --> EndAcc([สิ้นสุด])
    RejectPayment --> EndAcc
    ProcessRefund --> EndAcc
    RejectRefund --> EndAcc
    ExportStatement --> EndAcc
```

---

### ผังงานที่ 13: ศูนย์ช่วยเหลือและฝ่ายเทคนิค (IT Support Helpdesk Flowchart)
รองรับ Requirements ข้อ 22, 23, 24, 54, 55 (กระดาน Ticket Helpdesk, ตรวจสอบ, อัปเดตสถานะ, บันทึกการแก้ไข)

```mermaid
flowchart TD
    StartIT([เริ่มต้น: เจ้าหน้าที่ไอทีเข้า /Support]) --> LoadTicketBoard[โหลดกระดาน Helpdesk Ticket Board]
    LoadTicketBoard --> GroupTickets[จัดกลุ่มตามสถานะ: New, InProgress, Resolved]
    
    GroupTickets --> PickTicket[เลือกตั๋วปัญหาที่ลูกค้าหรือพนักงานแจ้งเข้ามา]
    PickTicket --> Investigate[ตรวจสอบรายละเอียดและสาเหตุข้อผิดพลาดทางเทคนิค]
    
    Investigate --> ChangeStatusWorking[เปลี่ยนสถานะตั๋วเป็น InProgress กำลังดำเนินการ]
    ChangeStatusWorking --> FixSystem[ดำเนินการแก้ไขปัญหาในระบบ]
    
    FixSystem --> LogResolution[กรอกบันทึกผลการแก้ไข และสาเหตุแห่งปัญหา Resolution History]
    LogResolution --> MarkResolved[เปลี่ยนสถานะตั๋วเป็น Resolved เรียบร้อย]
    MarkResolved --> NotifyCustomerTicket[ระบบส่งข้อความแจ้งผลการแก้ไขให้ผู้แจ้งปัญหารับทราบ]
    NotifyCustomerTicket --> EndIT([สิ้นสุด])
```

---

### ผังงานที่ 14: ผู้ดูแลระบบและจัดการสิทธิ์ (Admin Flowchart)
รองรับ Requirements ข้อ 57, 58 (จัดการบัญชี 10 บทบาท, กำหนด Role & Permission Matrix, ตรวจสอบ Audit Log)

```mermaid
flowchart TD
    StartAdm([เริ่มต้น: ผู้ดูแลระบบเข้า /Admin]) --> LoadAdminHub[โหลดหน้าระบบจัดการผู้ใช้งานและสิทธิ์]
    
    LoadAdminHub --> AdminTask{เลือกงานที่ต้องการทำ}
    
    AdminTask -- จัดการบัญชีพนักงานและลูกค้า --> ViewUserTable[แสดงตารางผู้ใช้ ค้นหา กรองตาม Role และสถานะ]
    ViewUserTable --> ModifyAccount[สร้างบัญชีใหม่ / แก้ไขข้อมูล / ระงับการใช้งาน IsActive=0]
    ModifyAccount --> SaveUserDB[อัปเดตลงตาราง User]
    
    AdminTask -- กำหนดสิทธิ์ Role & Permission Matrix --> ViewMatrix[เปิดตาราง Matrix สิทธิ์การเข้าถึง 10 บทบาท]
    ViewMatrix --> TogglePerms[ติ๊กเลือกหรือตัดสิทธิ์พิเศษ GrantedPermissions]
    TogglePerms --> SavePermissions[บันทึกโครงสร้างสิทธิ์ JSON ลงฐานข้อมูล]
    
    AdminTask -- ตรวจสอบ Audit Log --> ViewAuditLogs[เปิดดูบันทึกกิจกรรมย้อนหลัง Action, Timestamp, IP, UserId]
    ViewAuditLogs --> ExportAudit[ตรวจสอบความปลอดภัยและส่งออกรายงานการใช้งาน]
    
    SaveUserDB --> EndAdm([สิ้นสุด])
    SavePermissions --> EndAdm
    ExportAudit --> EndAdm
```

---

### ผังงานที่ 15: เจ้าของร้านและภาพรวมธุรกิจ (Owner Executive BI Dashboard Flowchart)
รองรับ Requirements ข้อ 60, 61 (Executive BI Dashboard, สัดส่วนยอดขาย, สินค้าขายดี, ทางลัดเข้าถึงทุกแผนก)

```mermaid
flowchart TD
    StartOwner([เริ่มต้น: เจ้าของร้านเข้า /Owner]) --> QueryKPIs[คำนวณ KPI: ยอดขายรวม Gross Revenue, กำไรสุทธิ Net Profit, ยอดคืนเงิน]
    
    QueryKPIs --> CalcPetBreakdown[คำนวณสัดส่วนยอดขายแยกตามประเภทสัตว์เลี้ยง: สุนัข vs แมว]
    CalcPetBreakdown --> QueryTop5[ดึง 5 อันดับสินค้าขายดี และสินค้าที่มีคนใส่ตะกร้ามากที่สุด]
    QueryTop5 --> QueryPromoRank[จัดอันดับแคมเปญโปรโมชั่นที่สร้างยอดขายได้สูงสุด]
    
    QueryPromoRank --> RenderDashboard[แสดงผลหน้า BI Dashboard ด้วยการ์ด KPI สถิติแบบโต้ตอบ]
    
    RenderDashboard --> OwnerAction{เจ้าของร้านต้องการทำอะไร?}
    OwnerAction -- ตรวจสอบการทำงานของแต่ละแผนก --> SuperAccess[คลิกลิงก์ Super-Access ไปยัง Inventory, Order, Shipping, Marketing, Accounting]
    OwnerAction -- ส่งออกข้อมูลสรุปผู้บริหาร --> ExportExecReport[พิมพ์หรือดาวน์โหลดรายงานภาพรวมธุรกิจ]
    
    SuperAccess --> EndOwner([สิ้นสุด])
    ExportExecReport --> EndOwner
```

---

## 📊 รายการเอกสารและสิ่งที่ต้องแนบในแต่ละบท (Checklist ส่งงานอาจารย์)

| หมวดหมู่เอกสาร | สิ่งที่ต้องใส่ในเล่ม | มีพร้อมในโปรเจกต์นี้แล้วหรือยัง? | แหล่งที่มา / วิธีนำไปใช้ |
|---|---|:---:|---|
| **ข้อกำหนดระบบ (Req 1–27)** | ตารางแจกแจง Functional Requirements ทั้ง 27 ข้อ | ✅ พร้อม 100% | มีอยู่ใน `req.md` ก๊อปปี้ไปใส่บทที่ 1 หรือ 2 ได้ทันที |
| **บทบาทผู้ใช้ (10 Roles)** | ตาราง Role & Permission Matrix แสดงสิทธิ์ 10 บทบาท | ✅ พร้อม 100% | มีอยู่ใน `UX_UI_SPECIFICATION.md` ข้อ 4 |
| **พจนานุกรมข้อมูล (Data Dictionary)** | โครงสร้างตาราง 13 ตาราง และ Enums E1–E6 | ✅ พร้อม 100% | มีอยู่ใน `DATA_DICTIONARY.md` สมบูรณ์ครบทุกฟิลด์ |
| **ผังงานระบบ (Flowcharts)** | แผนผัง Flowchart ครบทั้ง 15 ผังงาน | ✅ พร้อม 100% | อยู่ในไฟล์นี้ (หัวข้อที่ 2 ด้านบน) วาดด้วย Mermaid |
| **แบบร่างหน้าจอ (Wireframes 17 หน้า)** | ภาพ Wireframe Grayscale ครบ 17 หน้า | ✅ พร้อม 100% | เปิด `wireframe_viewer.html` แล้วกดปุ่ม "Print All to PDF" |
| **ภาพหน้าจอจริง (Live Screenshots)** | ถ่ายภาพหน้าจอระบบจริงเพื่อใส่บทที่ 4 | ⏳ รอแคปภาพ | รันโปรเจกต์ `http://localhost:5135` แล้วแคปหน้าจอจริง |
| **ซอร์สโค้ด (Source Code Snippets)** | โค้ดสำคัญของ Controller, Model, View | ✅ พร้อม 100% | ดึงจากโฟลเดอร์ `Controllers/` และ `Models/` |

---

## 💡 ลำดับขั้นตอนแนะนำในการทำเล่มส่งอาจารย์ (Step-by-Step Action Plan)

1. **ขั้นตอนที่ 1 (เตรียมโครงเอกสาร Word):**
   * สร้างไฟล์ Microsoft Word ใหม่ ตั้งค่าหน้ากระดาษ ขอบบน-ล่าง-ขวา 1 นิ้ว ขอบซ้าย 1.5 นิ้ว ฟอนต์ TH Sarabun PSK / ChulaCharasNew ขนาด 16 pt
2. **ขั้นตอนที่ 2 (คัดลอกส่วนวิเคราะห์และออกแบบใส่บทที่ 1 - 3):**
   * ก๊อปปี้เนื้อหาจาก `req.md` ใส่บทที่ 1
   * ก๊อปปี้ตาราง 10 บทบาทจาก `UX_UI_SPECIFICATION.md` ใส่บทที่ 2
   * นำ Mermaid Flowcharts ทั้ง 15 ผังงานในไฟล์นี้ Export เป็นรูปภาพ PNG ใส่บทที่ 3
   * ก๊อปปี้ตาราง 13 ตารางจาก `DATA_DICTIONARY.md` ใส่หัวข้อ Data Dictionary ในบทที่ 3
   * บันทึกรูปจาก `wireframe_viewer.html` แนบในบทที่ 3 ส่วนการออกแบบหน้าจอ
3. **ขั้นตอนที่ 3 (จัดทำบทที่ 4 การทำงานของระบบ 30+ ฟังก์ชัน):**
   * เปิดระบบจริง `http://localhost:5135`
   * ทดสอบสลับ 10 บทบาท และแคปเจอร์รูปภาพผลลัพธ์การทำงานของแต่ละหน้าจอ
   * นำโค้ด Action Method ที่เกี่ยวข้องมาวางคู่กับลำดับขั้นตอนการทำงาน (เลียนแบบตัวอย่างเล่ม BakeQuery หน้า 30–135)
4. **ขั้นตอนที่ 4 (สรุปผลและตรวจทานบทที่ 5):**
   * เขียนสรุปความสำเร็จเทียบกับ Functional Requirements ทั้ง 27 ข้อว่าทำได้ครบถ้วน
   * สร้างสารบัญอัตโนมัติ และบันทึกเป็น PDF เพื่อส่งให้อาจารย์ตรวจ


---

## 🔍 บัญชีแจกแจงฟังก์ชันการทำงานระดับ Action Method ทั้งหมด (53 Functions Inventory)

เพื่อให้เล่มรายงานของคุณสมบูรณ์แบบ 100% ตรงตามโครงสร้างเดียวกับเล่มตัวอย่าง BakeQuery (หน้า 30–135) ที่แจกแจงทุก Action ใน Controller อย่างละเอียด ระบบ PetStore Pro มี Action Methods ที่ใช้งานจริงใน 12 Controllers ทั้งหมด **53 ฟังก์ชัน** ดังนี้:

### หมวดที่ 1: AccountController (10 ฟังก์ชัน - จัดการโปรไฟล์ ที่อยู่ และคำสั่งซื้อลูกค้า)
1. **`AccountController.Profile` [GET]:** โหลดหน้าโปรไฟล์ สมุดที่อยู่จัดส่ง และประวัติส่วนตัว (ตาราง `User`, `UserAddress`)
2. **`AccountController.UpdateProfile` [POST]:** บันทึกการแก้ไขชื่อ นามสกุล เบอร์โทร และอีเมล (ตาราง `User`)
3. **`AccountController.SaveAddress` [POST]:** เพิ่มที่อยู่จัดส่งใหม่ หรือแก้ไขที่อยู่เดิมที่มีอยู่ (ตาราง `UserAddress`)
4. **`AccountController.DeleteAddress` [POST]:** ลบที่อยู่จัดส่งที่ไม่ต้องการออกจากสมุดที่อยู่ (ตาราง `UserAddress`)
5. **`AccountController.SetDefaultAddress` [POST]:** กำหนดให้ที่อยู่นี้เป็นที่อยู่จัดส่งหลัก (Default Address) (ตาราง `UserAddress`)
6. **`AccountController.Orders` [GET]:** แสดงรายการคำสั่งซื้อของตนเอง พร้อมสถานะ Step Progress และเลข Tracking (ตาราง `Orders`, `OrderItem`)
7. **`AccountController.UploadSlip` [POST]:** อัปโหลดรูปภาพสลิปการโอนเงินเพิ่มเติมในภายหลัง (Req 19) (ตาราง `Orders`)
8. **`AccountController.CancelOrder` [POST]:** ลูกค้าขอยกเลิกคำสั่งซื้อ หรือยื่นเรื่องขอคืนเงิน Refund (Req 20) (ตาราง `Orders`)
9. **`AccountController.Support` [GET]:** หน้าศูนย์ช่วยเหลือและเปิดตั๋วแจ้งปัญหาการใช้งาน (Req 22) (ตาราง `SupportTicket`)
10. **`AccountController.SubmitTicket` [POST]:** บันทึกการเปิดตั๋วแจ้งปัญหาใหม่เข้าสู่ระบบ (Req 22) (ตาราง `SupportTicket`)

### หมวดที่ 2: AccountingController (6 ฟังก์ชัน - งานบัญชี การเงิน และตรวจสลิป)
11. **`AccountingController.Index` [GET]:** หน้าภาพรวมการเงิน ยอดขายรวม กำไรสุทธิ ยอดรอตรวจสลิป และยอดขอคืนเงิน (Req 19, 20, 21)
12. **`AccountingController.ApprovePayment` [POST]:** อนุมัติสลิปโอนเงิน เปลี่ยนสถานะออเดอร์เป็น `PaymentVerified` (Req 19) (ตาราง `Orders`, `FinancialSummary`)
13. **`AccountingController.RejectPayment` [POST]:** ปฏิเสธสลิปโอนเงินกรณีสลิปไม่ถูกต้อง และแจ้งเหตุผล (Req 19) (ตาราง `Orders`)
14. **`AccountingController.ApproveRefund` [POST]:** อนุมัติการคืนเงินให้ลูกค้า เปลี่ยนสถานะเป็น `Refunded` (Req 20) (ตาราง `Orders`, `FinancialSummary`)
15. **`AccountingController.SaveMonthlyExpense` [POST]:** บันทึกต้นทุนและรายจ่ายจริงประจำเดือน (Req 21) (ตาราง `MonthlyFinancialEntry`, `FinancialSummary`)
16. **`AccountingController.ExportFinancialCsv` [GET]:** ส่งออกรายงานรายรับ-รายจ่ายเป็นไฟล์ CSV (Req 21) (ตาราง `FinancialSummary`, `MonthlyFinancialEntry`)

### หมวดที่ 3: AdminController (3 ฟังก์ชัน - งานผู้ดูแลระบบและจัดการสิทธิ์)
17. **`AdminController.Index` [GET]:** หน้าบริหารจัดการบัญชีผู้ใช้งาน เมทริกซ์สิทธิ์ 10 บทบาท และ Audit Log (Req 57, 58) (ตาราง `User`, `AuditLog`)
18. **`AdminController.UpdateUserRole` [POST]:** ปรับเปลี่ยนบทบาท (Role) และสถานะเปิด/ระงับบัญชี (`IsActive`) (ตาราง `User`, `AuditLog`)
19. **`AdminController.CreateUser` [POST]:** สร้างบัญชีพนักงานหรือผู้ใช้งานใหม่เข้าสู่ฐานข้อมูล (ตาราง `User`, `AuditLog`)

### หมวดที่ 4: AuthController (8 ฟังก์ชัน - ความปลอดภัยและการยืนยันตัวตน)
20. **`AuthController.Login` [GET]:** แสดงหน้าจอเข้าสู่ระบบ
21. **`AuthController.Login` [POST]:** ตรวจสอบ Username/Password, แฮช, เช็ก `IsActive`, ออก Cookie Claims (ตาราง `User`, `AuditLog`)
22. **`AuthController.QuickLogin` [POST]:** ทางลัดคลิกเดียวเพื่อสลับเข้าทดสอบระบบตามบัญชี Demo
23. **`AuthController.SwitchRole` [GET/POST]:** สลับมุมมองบทบาทแบบ Real-time พร้อมออก Session Cookie ตามบทบาทนั้น
24. **`AuthController.Register` [GET]:** แสดงหน้าจอลงทะเบียนสมาชิกใหม่สำหรับลูกค้า
25. **`AuthController.Register` [POST]:** ตรวจสอบความถูกต้อง บันทึกผู้ใช้ใหม่บทบาท Customer และสร้าง Cart (ตาราง `User`, `Cart`, `AuditLog`)
26. **`AuthController.Logout` [GET/POST]:** ออกจากระบบ ล้าง Cookie Authentication และ Session Storage
27. **`AuthController.AccessDenied` [GET]:** หน้าแจ้งเตือนปฏิเสธการเข้าถึงกรณีผู้ใช้ไม่มีสิทธิ์ในหน้างานนั้น

### หมวดที่ 5: CartController (7 ฟังก์ชัน - ตะกร้าสินค้าและระบบคำนวณ)
28. **`CartController.Index` [GET]:** คำนวณราคาสินค้า, 5 โปรโมชั่น, ของแถม, และค่าจัดส่งตามน้ำหนัก 10kg (Req 3, 4, 11–16) (ตาราง `Cart`, `CartItem`, `Product`, `Promotion`, `AppliedPromotionSummary`)
29. **`CartController.Add` [POST]:** เพิ่มสินค้าลงตะกร้า พร้อมตรวจสอบสต็อก (AJAX Optimistic UI 0ms) (ตาราง `Cart`, `CartItem`, `Product`)
30. **`CartController.Update` [POST]:** ปรับปรุงจำนวนชิ้นสินค้าในตะกร้า (ตาราง `CartItem`, `Product`)
31. **`CartController.Remove` [POST]:** ลบรายการสินค้าออกจากตะกร้า (ตาราง `CartItem`)
32. **`CartController.Checkout` [GET]:** หน้าตรวจสอบสรุปคำสั่งซื้อ เลือกที่อยู่จัดส่ง และแสดง QR Code PromptPay (ตาราง `UserAddress`, `Cart`, `CartItem`)
33. **`CartController.SubmitOrder` [POST - PromptPay]:** บันทึกคำสั่งซื้อ แนบสลิป ตัดสต็อก ล้างตะกร้า (Req 19) (ตาราง `Orders`, `OrderItem`, `Product`, `Cart`, `CartItem`)
34. **`CartController.SubmitOrder` [GET]:** ฟังก์ชันสำรองกรณีผู้ใช้กด Refresh หรือกดย้อนกลับหน้าชำระเงิน

### หมวดที่ 6: HomeController (3 ฟังก์ชัน - หน้าร้านและรายละเอียดสินค้า)
35. **`HomeController.Index` [GET]:** แคตตาล็อกสินค้า, ตัวกรองหมา/แมว, กรองช่วงวัย Layer, กรองน้ำหนัก, นับคนใส่ตะกร้า (Req 1, 2, 7, 8, 9, 10) (ตาราง `Product`, `CartItem`)
36. **`HomeController.Details` [GET]:** หน้ารายละเอียดสินค้า, ข้อมูลโภชนาการ, แนะนำสินค้าตามช่วงวัยและสายพันธุ์ (Req 7, 8, 10) (ตาราง `Product`, `CartItem`)
37. **`HomeController.SwitchRole` [POST]:** สลับบทบาทหน้าแรกผ่าน Role Switcher Bar

### หมวดที่ 7: InventoryController (4 ฟังก์ชัน - คลังสินค้าและสต็อก)
38. **`InventoryController.Index` [GET]:** รายการสต็อก, แถบแจ้งเตือน Low Stock Alert (< เกณฑ์), ตัวกรองสินค้าใกล้หมด (Req 18) (ตาราง `Product`)
39. **`InventoryController.UpdateStock` [POST]:** ปรับยอดสต็อกด่วน Quick In/Out (+/-) หน้าตาราง (ตาราง `Product`, `AuditLog`)
40. **`InventoryController.SaveProduct` [POST]:** เพิ่มสินค้าใหม่ หรือแก้ไขข้อมูลสินค้า น้ำหนัก รูปภาพ จุดสั่งซื้อซ้ำ (Req 6, 17) (ตาราง `Product`, `AuditLog`)
41. **`InventoryController.DeleteProduct` [POST]:** ระงับการขายหรือลบสินค้าออกจากระบบ (ตาราง `Product`, `AuditLog`)

### หมวดที่ 8: MarketingController (5 ฟังก์ชัน - การตลาดและแคมเปญ)
42. **`MarketingController.Index` [GET]:** แดชบอร์ดโปรโมชั่น 5 แบบ และสถิติจำนวนครั้งที่ถูกนำไปใช้ (Req 5, 27) (ตาราง `Promotion`)
43. **`MarketingController.ToggleStatus` [POST]:** สวิตช์เปิด/ปิดการใช้งานโปรโมชั่นแบบ Real-time (Req 5) (ตาราง `Promotion`, `AuditLog`)
44. **`MarketingController.Save` [POST]:** บันทึกเงื่อนไข Rule Builder (ยอดขั้นต่ำ, แบรนด์ที่ร่วม, วันที่เริ่ม-สิ้นสุด) (Req 5, 25, 26) (ตาราง `Promotion`, `AuditLog`)
45. **`MarketingController.Create` [POST]:** สร้างแคมเปญโปรโมชั่นใหม่ (ตาราง `Promotion`, `AuditLog`)
46. **`MarketingController.Delete` [POST]:** ลบแคมเปญโปรโมชั่น (ตาราง `Promotion`, `AuditLog`)

### หมวดที่ 9: OrderController (2 ฟังก์ชัน - จัดการคำสั่งซื้อและเตรียมแพ็ก)
47. **`OrderController.Index` [GET]:** กระดานคิวคำสั่งซื้อที่ชำระเงินแล้ว, ตรวจสอบของแถมโปรโมชั่น, พิมพ์ Packing Slip (Req 42, 43, 44) (ตาราง `Orders`, `OrderItem`)
48. **`OrderController.UpdateStatus` [POST]:** อัปเดตสถานะออเดอร์เป็น `Preparing` (กำลังจัดเตรียม) หรือ `ReadyToShip` (ตาราง `Orders`)

### หมวดที่ 10: OwnerController (1 ฟังก์ชัน - ผู้บริหารสูงสุด)
49. **`OwnerController.Index` [GET]:** แดชบอร์ดผู้บริหาร (BI Dashboard), ยอดขายรวม Gross Revenue, กำไรสุทธิ Net Profit, สัดส่วนสุนัข/แมว, สินค้าขายดี Top 5, ทางลัด Super-Access (Req 60, 61) (ตาราง `Orders`, `OrderItem`, `Product`, `Promotion`, `FinancialSummary`)

### หมวดที่ 11: ShippingController (2 ฟังก์ชัน - จัดส่งและโลจิสติกส์)
50. **`ShippingController.Index` [GET]:** ตารางพัสดุที่รอจัดส่ง (สถานะ Preparing) (Req 4, 14, 16) (ตาราง `Orders`)
51. **`ShippingController.DispatchOrder` [POST]:** บันทึกน้ำหนักพัสดุจริง, คีย์เลข Tracking Number, เลือกบริษัทขนส่ง, เปลี่ยนสถานะเป็น `Shipped` (ตาราง `Orders`)

### หมวดที่ 12: SupportController (2 ฟังก์ชัน - ฝ่ายเทคนิค Helpdesk)
52. **`SupportController.Index` [GET]:** Helpdesk Ticket Board แบ่งกลุ่มตั๋วตามสถานะ New, InProgress, Resolved (Req 23) (ตาราง `SupportTicket`)
53. **`SupportController.ResolveTicket` [POST]:** บันทึกผลการแก้ไขปัญหา (Resolution History), ชื่อผู้แก้ปัญหา, และปิดงานตั๋วเป็น `Resolved` (Req 24) (ตาราง `SupportTicket`, `AuditLog`)
