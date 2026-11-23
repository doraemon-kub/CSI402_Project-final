# เอกสารนำเสนอผังงานระบบ (System Flowchart Specification)
## เรื่อง ระบบจัดการร้านค้าสัตว์เลี้ยงออนไลน์ (PetStore Pro)

**จัดทำโดย:** นายพูห์ไพศาล อุดมเพชรไพศาล (รหัสนักศึกษา: 67088026)  
**เสนอ:** อาจารย์ ณัฐพล พุทธจรรยาวงศ์  
**รายงานฉบับนี้เป็นส่วนหนึ่งของการศึกษา:**  
วิชา CSI402 การเชื่อมต่อโปรแกรมส่วนหน้ากับโปรแกรมส่วนหลัง  
ภาคเรียนที่ 2 ปีการศึกษา 2568  
มหาวิทยาลัยศรีปทุม กรุงเทพมหานคร  

---

## 1. ความต้องการของผู้ใช้ (Requirement)
**PetStore Pro** เป็นระบบขายอาหารและของใช้สัตว์เลี้ยงออนไลน์ที่รองรับการใช้งานครบวงจร ตั้งแต่การสมัครสมาชิก เลือกสินค้า กรองตามประเภทสัตว์เลี้ยง (สุนัข/แมว) และช่วงวัย (Layer) ใส่ตะกร้า สั่งซื้อ ชำระเงินผ่าน PromptPay QR และแนบสลิป ไปจนถึงการติดตามสถานะการจัดส่งแบบ Step Progress โดยลูกค้าสามารถจัดการข้อมูลส่วนตัว สมุดที่อยู่จัดส่ง ดูประวัติคำสั่งซื้อ ขอยกเลิก/คืนเงิน รวมถึงแจ้งปัญหาการใช้งาน (Support Ticket) ได้

ระบบยังมีส่วนหลังบ้านสำหรับพนักงานที่แบ่งหน้าที่อย่างชัดเจน:
* **แผนกการเงินและบัญชี (Accounting Staff):** ตรวจสอบและอนุมัติสลิปโอนเงิน อนุมัติการคืนเงิน และสรุปรายรับ-รายจ่าย
* **แผนกคลังสินค้า (Inventory Staff):** ดูแลสต็อกสินค้า แจ้งเตือน Low Stock Alert และจัดการเพิ่ม/แก้ไขข้อมูลสินค้า
* **แผนกจัดเตรียมคำสั่งซื้อ (Order Staff):** รับออเดอร์ที่จ่ายเงินแล้ว ตรวจสอบของแถมโปรโมชั่น และพิมพ์ใบ Packing Slip
* **แผนกจัดส่ง (Shipping Staff):** ชั่งน้ำหนักจริง บันทึกเลขพัสดุ (Tracking Number) และอัปเดตสถานะจัดส่ง
* **แผนกการตลาด (Marketing Staff):** จัดการแคมเปญโปรโมชั่น 5 รูปแบบ สวิตช์เปิด/ปิด และกำหนดเงื่อนไข Rule Builder
* **ฝ่ายบริการเทคนิค (IT Support):** รับเรื่องแจ้งปัญหาจากลูกค้าและพนักงาน บันทึกประวัติและผลการแก้ไขปัญหา
* **ผู้ดูแลระบบ (Admin):** ควบคุมและจัดการบัญชีผู้ใช้งาน กำหนดสิทธิ์การเข้าถึง (RBAC Matrix) และตรวจสอบ Audit Log
* **ผู้บริหารสูงสุด (Owner):** ดูแดชบอร์ดภาพรวมธุรกิจ (Executive BI) ยอดขายรวม กำไรสุทธิ สัดส่วนสุนัข/แมว และสิทธิ์ Super-Access

---

## 2. บทบาทผู้ใช้และสิทธิ์การใช้งาน
ระบบแบ่งผู้ใช้งานออกเป็น **10 บทบาท**:
1. **Guest (ผู้เยี่ยมชม):** ดูสินค้า กรองหมวดหมู่/ช่วงวัย ทดลองใส่ตะกร้าดูส่วนลดจำลอง และสมัครสมาชิก
2. **Customer (ลูกค้า):** สั่งซื้อสินค้า ชำระเงิน แนบสลิป จัดการที่อยู่ ติดตามพัสดุ ขอคืนเงิน และเปิดตั๋วแจ้งปัญหา
3. **Inventory Staff (พนักงานคลังสินค้า):** ตรวจสอบสต็อก ปรับยอดด่วน Quick In/Out และเพิ่ม/แก้ไขข้อมูลสินค้า
4. **Order Staff (พนักงานฝ่ายคำสั่งซื้อ):** ตรวจสอบออเดอร์ที่จ่ายเงินแล้ว ตรวจของแถมโปรโมชั่น และพิมพ์ใบจัดของ
5. **Shipping Staff (พนักงานฝ่ายจัดส่ง):** ชั่งน้ำหนักพัสดุจริง บันทึกเลข Tracking และเปลี่ยนสถานะเป็น Shipped
6. **Marketing Staff (พนักงานฝ่ายการตลาด):** จัดการ 5 แคมเปญโปรโมชั่น ตั้งเงื่อนไขส่วนลด และดูสถิติการใช้งาน
7. **Accounting Staff (พนักงานฝ่ายการเงิน):** ตรวจสอบสลิปโอนเงิน อนุมัติคำขอคืนเงิน และสรุปบัญชีรายรับ-รายจ่าย
8. **IT Support (เจ้าหน้าที่ฝ่ายไอที):** ตรวจสอบตั๋วแจ้งปัญหา ช่วยเหลือผู้ใช้งาน และบันทึกผลการแก้ไขปัญหา
9. **Admin (ผู้ดูแลระบบ):** จัดการบัญชีผู้ใช้ กำหนดบทบาทและสิทธิ์การเข้าถึง ตรวจสอบความปลอดภัย Audit Log
10. **Owner (เจ้าของธุรกิจ/ผู้บริหาร):** ตรวจสอบภาพรวมธุรกิจ ยอดขายรวม กำไรสุทธิ และเข้าถึงหน้างานของทุกแผนกได้

---

## 3. ขอบเขตของระบบ
ระบบ PetStore Pro รองรับการทำงานหลักดังนี้:
* 3.1 ระบบสมัครสมาชิก เข้าสู่ระบบ และสลับสิทธิ์การใช้งาน (Multi-Role Switcher)
* 3.2 ระบบแสดงสินค้า แคตตาล็อก ตัวกรองตามประเภทสัตว์ และตัวกรองช่วงวัย (Age Layer)
* 3.3 ระบบตะกร้าสินค้า และระบบคำนวณส่วนลด 5 โปรโมชั่นอัตโนมัติ
* 3.4 ระบบคำนวณค่าจัดส่งตามเกณฑ์น้ำหนัก (Weight-based Shipping เกณฑ์ 10 kg)
* 3.5 ระบบชำระเงิน QR พร้อมเพย์ แนบสลิป และตรวจสอบสถานะออเดอร์
* 3.6 ระบบติดตามการจัดส่งสินค้า (Step Progress Tracking) และการขอคืนเงิน
* 3.7 ระบบแจ้งปัญหาการใช้งาน (Customer Helpdesk) และบันทึกประวัติการแก้ไข
* 3.8 ระบบจัดการสินค้าในคลังและการแจ้งเตือนสินค้าใกล้หมด (Low Stock Alert)
* 3.9 ระบบจัดการคำสั่งซื้อและออกใบจัดเตรียมสินค้า (Packing Slip)
* 3.10 ระบบบริหารแคมเปญโปรโมชั่นและบันทึกเงื่อนไข (Rule Builder)
* 3.11 ระบบการเงิน ตรวจสอบสลิป และจัดทำรายงานรายรับ-รายจ่าย
* 3.12 ระบบจัดการผู้ใช้งานและกำหนดสิทธิ์ตามบทบาท (RBAC)
* 3.13 ระบบแดชบอร์ดภาพรวมธุรกิจสำหรับผู้บริหาร (Executive BI Dashboard)

---

# 📊 ผังงานกระบวนการภาพรวม (System Flowcharts)

## ผังงานกระบวนการฝั่ง Customer (ลูกค้า)
แสดงลำดับการทำงานของลูกค้าตั้งแต่เริ่มต้นเข้าใช้งานระบบจนถึงการสั่งซื้อสินค้าสำเร็จ โดยใช้สัญลักษณ์ตัวเชื่อมโยงในหน้าเดียวกัน (On-page Connector `(( ))`) เมื่อมีการตรวจสอบเงื่อนไขไม่ผ่านแล้วโยงเส้นกลับมา

```mermaid
flowchart TD
    Start([start]) --> HasAccount{มีบัญชีแล้ว?}
    
    HasAccount -- NO --> Register[Register]
    Register --> ConnReg(( ))
    ConnReg --> FillReg[กรอกข้อมูลสมัคร\nชื่อ, email, เบอร์, password]
    FillReg --> CheckReg{ตรวจสอบ}
    CheckReg -- NO --> ConnReg
    CheckReg -- YES --> Login
    
    HasAccount -- YES --> Login[Login]
    Login --> ConnLogin(( ))
    ConnLogin --> FillLogin[กรอก email / password\nตรวจสอบข้อมูล]
    FillLogin --> CheckLogin{ตรวจสอบ}
    CheckLogin -- NO --> ConnLogin
    CheckLogin -- YES --> Home["หน้าหลัก (Home)"]
    
    Home --> BrowseProduct[เลือกสินค้า / เพิ่มสินค้า]
    BrowseProduct --> ViewCart[ตะกร้าสินค้า\nเพิ่ม/ลบสินค้า]
    ViewCart --> Checkout[Checkout / ชำระเงิน\nที่อยู่, วิธีจัดส่ง, วิธีชำระ]
    Checkout --> TrackOrder[ดูสถานะออเดอร์\nรอจัดส่ง / กำลังจัดส่ง / จัดส่งแล้ว]
    TrackOrder --> ConfirmReceive[ยืนยันรับสินค้า]
    ConfirmReceive --> End([End])
```

---

## ผังงานกระบวนการฝั่ง Admin และพนักงานหลังบ้าน
แสดงลำดับการทำงานของผู้ดูแลระบบและพนักงานฝ่ายต่างๆ หลังจากล็อกอินสำเร็จแล้วจะเข้าสู่หน้า Dashboard ของแผนกตนเอง โดยมีตัวเชื่อมโยง (Connector `(( ))`) รองรับการวนซ้ำและการรวมจุดกระบวนการ

```mermaid
flowchart TD
    Start([start]) --> ConnLogin(( ))
    ConnLogin --> Login[Login]
    Login --> FillCreds[กรอก email / password\nตรวจสอบข้อมูล]
    FillCreds --> CheckCreds{ตรวจสอบ}
    CheckCreds -- NO --> ConnLogin
    CheckCreds -- YES --> CheckRole{Role}
    
    CheckRole -- ลูกค้า --> GoStore[หน้าหลักร้านค้า]
    CheckRole -- Admin --> AdminBranch[Admin Portal]
    CheckRole -- พนักงาน --> CheckDept{ตำแหน่ง / แผนก}
    
    CheckDept -- แผนกการเงิน --> FinDept[จัดการการเงิน & ตรวจสลิป]
    FinDept --> CheckSlip[ดูสลิปลูกค้า]
    CheckSlip --> IsSlipValid{สลิปถูกต้อง?}
    IsSlipValid -- YES --> ApprovePay[อนุมัติออเดอร์\nสถานะ = รอจัดส่ง]
    IsSlipValid -- NO --> RejectPay[ตอบกลับลูกค้า\nให้ส่งสลิปใหม่]
    ApprovePay --> ConnFinDone(( ))
    RejectPay --> ConnFinDone
    ConnFinDone --> FinDone[สำเร็จ / หน้าหลัก]
    
    CheckDept -- แผนกคลัง --> WhDept[จัดการสินค้า\nเพิ่ม/ลด/อัปเดตสต็อก]
    WhDept --> WhDone[สำเร็จ / หน้าหลัก]
    
    CheckDept -- แผนกจัดส่ง --> ShipDept[ชั่งน้ำหนักจริง\nบันทึกเลข Tracking]
    ShipDept --> ShipDone[สำเร็จ / หน้าหลัก]
    
    CheckDept -- แผนกการตลาด --> MktDept[จัดการ 5 โปรโมชั่น\nเปิด/ปิดแคมเปญ]
    MktDept --> MktDone[สำเร็จ / หน้าหลัก]
    
    AdminBranch --> AdminAction[จัดการพนักงาน & สิทธิ์]
    AdminAction --> ConnUser(( ))
    ConnUser --> AddUserForm[กรอกข้อมูลพนักงาน\nชื่อ, email, เบอร์, ตำแหน่ง]
    AddUserForm --> CheckAdminRole{Role ถูกต้อง?}
    CheckAdminRole -- YES --> SaveUserDB[บันทึกข้อมูลสำเร็จ\nSuccess]
    CheckAdminRole -- NO --> AlertRoleErr[แจ้ง: ไม่มี Role ที่ถูกต้อง]
    AlertRoleErr --> ConnUser
    SaveUserDB --> AdminDone[สำเร็จ / หน้าหลัก]
    
    FinDone --> End([End])
    WhDone --> End
    ShipDone --> End
    MktDone --> End
    AdminDone --> End
    GoStore --> End
```

---

# 📑 Flowchart แต่ละหน้า (Page-by-Page Flowcharts)
> ทุกผังงานใช้สัญลักษณ์ **ตัวเชื่อมโยงในหน้าเดียวกัน (On-page Connector `(( ))`)** ตามมาตรฐาน ANSI/ISO 5807 เมื่อตรวจสอบเงื่อนไขไม่ผ่านหรือต้องการวนกลับมาทำซ้ำ

---

### 1. LOGIN (การเข้าสู่ระบบ)
กระบวนการเข้าสู่ระบบ ระบบจะตรวจสอบ Email และ Password จากฐานข้อมูล หากถูกต้องจะบันทึก Session แล้ว Redirect ไปยังหน้าหลัก หากไม่ถูกต้องจะแสดงข้อความแจ้งเตือนและวนกลับมาที่ตัวเชื่อมโยงเพื่อกรอกใหม่

```mermaid
flowchart TD
    Start([start]) --> ConnLogin(( ))
    ConnLogin --> InputLogin[กรอก email / password]
    InputLogin --> VerifyUser{ตรวจสอบ\nข้อมูลผู้ใช้}
    VerifyUser -- NO --> LoginRetry[Login ไม่สำเร็จ]
    LoginRetry --> ConnLogin
    VerifyUser -- YES --> CheckRoleBranch{ตรวจสอบประเภทบัญชี}
    CheckRoleBranch -- ลูกค้า --> GoCustHome["เข้าสู่หน้าหลักลูกค้า (Home)"]
    CheckRoleBranch -- พนักงาน / ผู้บริหาร --> GoStaffPortal[เข้าสู่หน้าหลักพนักงาน / Back-Office]
    GoCustHome --> ConnEnd(( ))
    GoStaffPortal --> ConnEnd
    ConnEnd --> End([End])
```

---

### 2. REGISTER (การสมัครสมาชิก)
กระบวนการสมัครสมาชิก ผู้ใช้กรอกข้อมูล Email, ชื่อ, เบอร์โทร และรหัสผ่าน หากข้อมูลไม่ครบจะแจ้งเตือนและวนกลับมาที่ตัวเชื่อมโยง หากถูกต้องระบบจะสร้าง ID อัตโนมัติแล้วบันทึกลงฐานข้อมูล จากนั้น Redirect ไปหน้า Login

```mermaid
flowchart TD
    Start([start]) --> ConnReg(( ))
    ConnReg --> InputReg[กรอก ชื่อ / email / password / เบอร์โทร]
    InputReg --> CheckComplete{ข้อมูลครบ?}
    CheckComplete -- NO --> AlertErr[แจ้งข้อผิดพลาด]
    AlertErr --> ConnReg
    CheckComplete -- YES --> SaveCustomer[บันทึกลง Customer\nลงฐานข้อมูล]
    SaveCustomer --> GoToLogin[ไปหน้า Login]
    GoToLogin --> End([End])
```

---

### 3. HOME (หน้าหลักร้านค้า)
หน้าหลักแสดงรายการสินค้าและหมวดหมู่ ลูกค้าสามารถกดดูสินค้าแยกตามประเภทสัตว์เลี้ยง (สุนัข/แมว) ช่วงวัย (Layer) หรือคลิกดูรายละเอียดสินค้าแต่ละชิ้น

```mermaid
flowchart TD
    Start([start]) --> LoadDB[โหลดข้อมูลจาก DB\nสินค้า, หมวดหมู่, สัตว์เลี้ยง]
    LoadDB --> DisplayCatalog[แสดงสินค้า / หมวดหมู่สินค้า]
    DisplayCatalog --> FilterLayer[ตัวกรองอัจฉริยะ: สุนัข แมว ช่วงวัย]
    FilterLayer --> DisplayFeatured[แสดงสินค้าแนะนำ / สินค้าขายดี]
    DisplayFeatured --> ViewProductList[ดูรายการสินค้า]
    ViewProductList --> End([End])
```

---

### 4. PRODUCT DETAIL (User) (หน้ารายละเอียดสินค้า)
หน้ารายละเอียดสินค้า แสดงรูป ชื่อ ราคา น้ำหนัก และข้อมูลสินค้า ลูกค้าสามารถกดเพิ่มลงตะกร้าหรือซื้อเลยได้ โดยมีตัวเชื่อมโยงรวมจุดกระบวนการก่อนบันทึกออเดอร์

```mermaid
flowchart TD
    Start([start]) --> LoadProductInfo[โหลดข้อมูลสินค้า\nชื่อ, ราคา, รูป, รายละเอียด, น้ำหนัก]
    LoadProductInfo --> SelectQty[เลือกจำนวนสินค้า]
    SelectQty --> CheckAction{เพิ่มตะกร้าสินค้า?}
    CheckAction -- YES --> AddCartDB["บันทึกสินค้าลงตะกร้า\n(CartItem DB)"]
    CheckAction -- NO --> BuyNow["ซื้อเลย\n(ไปหน้า Checkout)"]
    AddCartDB --> ConnAction(( ))
    BuyNow --> ConnAction
    ConnAction --> MakeOrder[สั่งออเดอร์]
    MakeOrder --> SaveOrderDetail[บันทึก OrderDetail ลง DB]
    SaveOrderDetail --> End([End])
```

---

### 5. CART (User) (หน้าตะกร้าสินค้า)
หน้าตะกร้าสินค้า แสดงรายการสินค้าที่เลือกไว้ สามารถปรับจำนวน ลบสินค้า โดยหากมีการแก้ไขจะวนกลับมาที่ตัวเชื่อมโยงเพื่อคำนวณ 5 โปรโมชั่นและน้ำหนักใหม่ และกดสั่งซื้อเพื่อไปหน้า Checkout

```mermaid
flowchart TD
    Start([start]) --> LoadCartDB[โหลดตะกร้า\nดึง CartItem จาก DB]
    LoadCartDB --> ConnCart(( ))
    ConnCart --> CheckMod{เพิ่ม/ลดสินค้า?}
    CheckMod -- YES --> EditCart[แก้ไขจำนวนสินค้าในตะกร้า]
    EditCart --> RecalcPromo[ระบบคำนวณ 5 โปรโมชั่น & น้ำหนัก]
    RecalcPromo --> ConnCart
    CheckMod -- NO --> ShowSummary[แสดงรายการ + ยอดรวมสุทธิ + ส่วนลด]
    ShowSummary --> GoCheckout[Checkout]
    GoCheckout --> End([End])
```

---

### 6. CHECKOUT (User) (หน้ายืนยันคำสั่งซื้อและชำระเงิน)
หน้ายืนยันคำสั่งซื้อ ลูกค้ากรอกที่อยู่จัดส่ง เลือกวิธีชำระเงิน (PromptPay QR) และการจัดส่ง ตรวจสอบโปรโมชั่น และรวมจุดที่ตัวเชื่อมโยงเพื่อยืนยันสร้างออเดอร์

```mermaid
flowchart TD
    Start([start]) --> InputAddress[กรอกชื่อ, เบอร์, ที่อยู่จัดส่ง]
    InputAddress --> SelectCourier[เลือกวิธีจัดส่ง\nKerry / Flash / Thailand Post]
    SelectCourier --> HasPromo{ใช้โปรโมชั่น?}
    HasPromo -- YES --> ApplyDiscount[หักส่วนลดโปรโมชั่น]
    HasPromo -- NO --> CalcStandard[ส่งออกออเดอร์ยอดปกติ]
    ApplyDiscount --> ConnCheckout(( ))
    CalcStandard --> ConnCheckout
    ConnCheckout --> ConfirmCheckout[ยืนยันออเดอร์\nอัปเดต order -> 'รอชำระ/รอตรวจสลิป']
    ConfirmCheckout --> OrderSuccess[ไปหน้า OrderSuccessUser]
    OrderSuccess --> End([End])
```

---

### 7. ORDER (User) (หน้าติดตามสถานะออเดอร์)
หน้าติดตามสถานะออเดอร์ แสดงรายการคำสั่งซื้อทั้งหมดของลูกค้าพร้อมสถานะปัจจุบันแบบ Step Progress หากยังไม่จัดส่งจะวนกลับมาที่ตัวเชื่อมโยงเพื่อรอสถานะล่าสุด

```mermaid
flowchart TD
    Start([start]) --> LoadOrders[โหลดคลังออเดอร์ของลูกค้า]
    LoadOrders --> ShowOrderList[แสดงรายการออเดอร์\nเลข order, สถานะ, วันที่, ยอดเงิน]
    ShowOrderList --> ConnOrder(( ))
    ConnOrder --> CheckShipped{สถานะ == จัดส่งแล้ว?}
    CheckShipped -- YES --> ConfirmReceived[ลูกค้ายืนยันรับสินค้า\nสถานะ = สำเร็จ]
    CheckShipped -- NO --> WaitStatus[รออัปเดตสถานะจากร้านค้า]
    WaitStatus --> ConnOrder
    ConfirmReceived --> End([End])
```

---

### 8. DASHBOARD (Admin / Owner) (หน้าสรุปภาพรวมระบบ)
หน้าสรุปภาพรวมระบบสำหรับ Admin และ Owner แสดงข้อมูล ยอดขายรวม กำไรสุทธิ ออเดอร์ สต็อกสินค้า และลูกค้า

```mermaid
flowchart TD
    Start([start]) --> LoadKPI[โหลดข้อมูล\nยอดขาย, ออเดอร์, สต็อก, กำไร]
    LoadKPI --> ShowMetricCards[แสดง Cards KPI ผู้บริหาร]
    ShowMetricCards --> ShowRecentOrders[แสดงออเดอร์ล่าสุด]
    ShowRecentOrders --> ShowTopProducts[แสดงสินค้าขายดี / สต็อกใกล้หมด]
    ShowTopProducts --> End([End])
```

---

### 9. AddProduct (Admin / Inventory) (การเพิ่มสินค้าใหม่)
กระบวนการเพิ่มสินค้าใหม่เข้าระบบ พนักงานกรอกข้อมูลสินค้า น้ำหนัก ช่วงวัย หากข้อมูลไม่ครบจะวนกลับมาที่ตัวเชื่อมโยงเพื่อกรอกใหม่ หากครบจะอัปโหลดรูปภาพและบันทึกลงฐานข้อมูล

```mermaid
flowchart TD
    Start([start]) --> ConnAddProd(( ))
    ConnAddProd --> InputBasic[กรอกข้อมูลสินค้า\nชื่อ, แบรนด์, หมวด, จำนวนสต็อก]
    InputBasic --> InputDetail[กรอกรายละเอียด\nราคา, รูปสินค้า, ต้นทุน, น้ำหนัก kg, ช่วงวัย]
    InputDetail --> CheckProductComplete{ข้อมูลครบ?}
    CheckProductComplete -- NO --> ConnAddProd
    CheckProductComplete -- YES --> UploadImage[upload รูปภาพสินค้า]
    UploadImage --> SaveProductDB[บันทึก Product ลง DB]
    SaveProductDB --> End([End])
```

---

### 10. EditProduct (Admin / Inventory) (การแก้ไขสินค้า)
กระบวนการแก้ไขข้อมูลสินค้า พนักงานเลือกสินค้าที่ต้องการแก้ไข ระบบโหลดข้อมูลเดิมมาให้ หากกรอกข้อมูลไม่ครบจะวนกลับมาที่ตัวเชื่อมโยง หากครบจะตรวจสอบการเปลี่ยนรูปภาพและบันทึกลงฐานข้อมูล

```mermaid
flowchart TD
    Start([start]) --> ConnEditProd(( ))
    ConnEditProd --> LoadCurrentPID["โหลดข้อมูลสินค้าปัจจุบัน (PID) จาก DB"]
    LoadCurrentPID --> DisplayEditForm[แสดงฟอร์มพร้อมข้อมูลเดิม\nชื่อ, แบรนด์, หมวด, จำนวน]
    DisplayEditForm --> EditFields[แก้ไขราคา / รูปสินค้า\nต้นทุน, น้ำหนัก, จุดสั่งซื้อซ้ำ]
    EditFields --> CheckEditComplete{ข้อมูลครบ?}
    CheckEditComplete -- NO --> ConnEditProd
    CheckEditComplete -- YES --> ChangeImage{เปลี่ยนรูปสินค้า?}
    ChangeImage -- YES --> UploadNewImg[upload รูปใหม่]
    ChangeImage -- NO --> UseOldImg[ใช้รูปเดิม]
    UploadNewImg --> ConnSaveProd(( ))
    UseOldImg --> ConnSaveProd
    ConnSaveProd --> SaveProductEdit[บันทึกการแก้ไข Product ลง DB]
    SaveProductEdit --> End([End])
```

---

### 11. ADD USER (Admin) (การเพิ่มพนักงานใหม่)
กระบวนการเพิ่มข้อมูลพนักงานใหม่เข้าระบบ Admin กรอกข้อมูลและกำหนดสิทธิ์ตามบทบาทหน้าที่ หากข้อมูลไม่ครบจะวนกลับมาที่ตัวเชื่อมโยงเพื่อแก้ไข

```mermaid
flowchart TD
    Start([start]) --> ConnAddUser(( ))
    ConnAddUser --> InputStaffInfo[กรอกข้อมูลพนักงาน\nชื่อ, email, password, เบอร์]
    InputStaffInfo --> SelectRole[เลือก Role / ตำแหน่ง\nAdmin / การเงิน / คลัง / จัดส่ง / การตลาด / IT]
    SelectRole --> CheckUserComplete{ข้อมูลครบ?}
    CheckUserComplete -- NO --> ConnAddUser
    CheckUserComplete -- YES --> SaveRoleDB[บันทึก Role และข้อมูลพนักงานลง DB]
    SaveRoleDB --> End([End])
```

---

### 12. EDIT / DELETE USER (Admin) (การแก้ไขหรือลบพนักงาน)
กระบวนการแก้ไขหรือลบข้อมูลพนักงาน Admin สามารถอัปเดตข้อมูลและสิทธิ์การเข้าถึงของพนักงานแต่ละคน หรือลบ/ระงับบัญชี โดยมีตัวเชื่อมโยงรับการวนกลับตามมาตรฐาน ANSI Flowchart

```mermaid
flowchart TD
    Start([start]) --> ConnUser(( ))
    ConnUser --> LoadUserCurrent[โหลดข้อมูลผู้ใช้ปัจจุบัน]
    LoadUserCurrent --> ActionType{Edit หรือ Delete?}
    
    ActionType -- Delete --> ConfirmDelete{ยืนยันลบผู้ใช้?}
    ConfirmDelete -- YES --> DeleteUserDB[ลบจาก DB / ระงับบัญชี]
    ConfirmDelete -- NO --> ConnUser
    
    ActionType -- Edit --> InputEditUser[กรอกข้อมูลแก้ไข\nชื่อ, email, เบอร์, Role]
    InputEditUser --> CheckUserEdit{ข้อมูลครบ?}
    CheckUserEdit -- NO --> ConnUser
    CheckUserEdit -- YES --> SaveUserEdit[บันทึกการแก้ไข -> AdminList]
    
    DeleteUserDB --> ConnEnd(( ))
    SaveUserEdit --> ConnEnd
    ConnEnd --> End([End])
```

---

### 13. SHIPPING (Shipping Staff) (การจัดส่งและบันทึกเลขพัสดุ)
กระบวนการฝ่ายจัดส่ง รับออเดอร์ที่เตรียมสินค้าเสร็จแล้ว ชั่งน้ำหนักพัสดุจริง บันทึกเลข Tracking Number และเปลี่ยนสถานะเป็นจัดส่งแล้ว

```mermaid
flowchart TD
    Start([start]) --> LoadPreparingOrders["โหลดรายการออเดอร์ที่แพ็กเสร็จ (Preparing)"]
    LoadPreparingOrders --> WeighBox[ชั่งน้ำหนักพัสดุจริง]
    WeighBox --> EnterTracking[กรอกเลขพัสดุ Tracking Number ตามขนส่งที่ลูกค้าเลือก]
    EnterTracking --> UpdateShippedDB[อัปเดต OrderStatus = 'Shipped' ลง DB]
    UpdateShippedDB --> NotifyCustomer[ลูกค้าสามารถตรวจสอบเลข Tracking ได้ทันที]
    NotifyCustomer --> End([End])
```

---

### 14. MARKETING & PROMOTIONS (Marketing Staff) (การจัดการแคมเปญโปรโมชั่น)
กระบวนการฝ่ายการตลาด ตรวจสอบสถิติการใช้งานโปรโมชั่น ควบคุมสวิตช์เปิด/ปิดการใช้งานโปรโมชั่นแบบเรียลไทม์ และกำหนดเงื่อนไขโปรโมชั่น โดยรวมจุดบันทึกที่ตัวเชื่อมโยง

```mermaid
flowchart TD
    Start([start]) --> LoadPromos[โหลดแคมเปญโปรโมชั่น 5 แบบ]
    LoadPromos --> ViewStats[ดูสถิติจำนวนครั้งที่โปรโมชั่นถูกนำไปใช้]
    ViewStats --> PromoAction{เลือกการทำงาน}
    PromoAction -- เปิด/ปิดด่วน --> ToggleActive[คลิก Toggle เปิด/ปิด IsActive]
    PromoAction -- แก้ไขเงื่อนไข --> EditRule[กำหนด: ยอดขั้นต่ำ, แบรนด์, วันหมดอายุ]
    ToggleActive --> ConnPromoSave(( ))
    EditRule --> ConnPromoSave
    ConnPromoSave --> SavePromoDB[บันทึกลงฐานข้อมูล Promotion]
    SavePromoDB --> End([End])
```

---

### 15. ACCOUNTING & FINANCE (Accounting Staff) (การตรวจสอบการชำระเงินและคืนเงิน)
กระบวนการฝ่ายการเงิน ตรวจสอบสลิปโอนเงินของลูกค้า อนุมัติการชำระเงิน และดำเนินการอนุมัติการขอคืนเงิน (Refund) โดยมีตัวเชื่อมโยงรวมผลลัพธ์สิ้นสุด

```mermaid
flowchart TD
    Start([start]) --> LoadUnverifiedOrders[โหลดออเดอร์ที่รอตรวจสลิป และรายการขอคืนเงิน]
    LoadUnverifiedOrders --> SelectTask{เลือกงานที่ต้องการทำ}
    
    SelectTask -- ตรวจสลิปโอนเงิน --> CheckSlipImage[เปิดดูรูปภาพสลิป และตรวจสอบยอดโอน]
    CheckSlipImage --> IsSlipValid{สลิปถูกต้อง?}
    IsSlipValid -- YES --> ApprovePay[อนุมัติการชำระเงิน: สถานะ = PaymentVerified]
    IsSlipValid -- NO --> RejectPay[ปฏิเสธสลิป: แจ้งให้ลูกค้าอัปโหลดใหม่]
    
    SelectTask -- จัดการขอคืนเงิน --> CheckRefundProof[ตรวจสอบเหตุผลและรูปถ่ายสินค้าชำรุด]
    CheckRefundProof --> ApproveRefund[อนุมัติคืนเงิน: บันทึกยอดเงินคืนลง FinancialSummary]
    
    ApprovePay --> ConnAccDone(( ))
    RejectPay --> ConnAccDone
    ApproveRefund --> ConnAccDone
    ConnAccDone --> End([End])
```

---

### 16. IT SUPPORT & HELPDESK (IT Support) (ศูนย์ช่วยเหลือและจัดการปัญหา)
กระบวนการฝ่ายเทคนิค ตรวจสอบตั๋วปัญหาที่ลูกค้าแจ้งเข้ามา ดำเนินการแก้ไขข้อผิดพลาดในระบบ และบันทึกประวัติการแก้ไขเพื่อปิดงาน

```mermaid
flowchart TD
    Start([start]) --> LoadTicketBoard[โหลดกระดาน Helpdesk Ticket Board]
    LoadTicketBoard --> PickTicket[เลือกตั๋วปัญหาที่ลูกค้าแจ้งเข้ามา]
    PickTicket --> StartInvestigate[เปลี่ยนสถานะเป็น InProgress ดำเนินการตรวจสอบ]
    StartInvestigate --> FixIssue[ดำเนินการแก้ไขปัญหาในระบบ]
    FixIssue --> LogResolutionNote["บันทึกรายละเอียดและสาเหตุการแก้ไข (Resolution History)"]
    LogResolutionNote --> CloseTicket[เปลี่ยนสถานะเป็น Resolved เรียบร้อย]
    CloseTicket --> End([End])
```

---

## 4. โครงสร้างฐานข้อมูลหลัก (Database Schema 13 Tables)
ระบบใช้ฐานข้อมูล MySQL ชื่อ **`CSI402DB`** ประกอบด้วย 13 ตารางหลัก:
1. `Product` — ข้อมูลสินค้าสัตว์เลี้ยง (ชื่อ, ราคา, หมวด, ช่วงวัย, น้ำหนัก kg, แบรนด์)
2. `Promotion` — แคมเปญโปรโมชั่น 5 แบบ (ส่วนลด, ยอดขั้นต่ำ, วันเริ่ม-สิ้นสุด)
3. `User` — บัญชีผู้ใช้งานระบบ 10 บทบาท (Username, PasswordHash, Role, IsActive)
4. `UserAddress` — สมุดที่อยู่จัดส่งของลูกค้า
5. `Cart` — ตะกร้าสินค้า
6. `CartItem` — รายการสินค้าในตะกร้า
7. `AppliedPromotionSummary` — บันทึกโปรโมชั่นที่ถูกคำนวณใช้
8. `Orders` — คำสั่งซื้อ ยอดสุทธิ สลิปโอนเงิน เลขพัสดุ
9. `OrderItem` — รายการสินค้าแต่ละชิ้นในคำสั่งซื้อ
10. `SupportTicket` — ตั๋วแจ้งปัญหาของลูกค้าและประวัติการแก้ไข
11. `FinancialSummary` — สรุปภาพรวมรายได้ กำไรสุทธิ และยอดคืนเงิน
12. `MonthlyFinancialEntry` — สรุปการเงินแยกรายเดือน
13. `AuditLog` — ตารางบันทึกกิจกรรมย้อนหลัง (Auditing)

---

## 5. แหล่งข้อมูลเพิ่มเติมและเครื่องมือส่งงานอาจารย์
* 🎨 **แบบร่างหน้าจอ Low-Fidelity Wireframes (17 หน้า):** เปิดไฟล์ `wireframe_viewer.html` บนเบราว์เซอร์
* 📊 **ระบบแสดงผังงานแบบโต้ตอบ (Interactive Flowchart Studio):** เปิดไฟล์ `flowchart_viewer.html`
* 📖 **พจนานุกรมข้อมูลฉบับสมบูรณ์:** อ่านได้ที่ [DATA_DICTIONARY.md](DATA_DICTIONARY.md)
* 📑 **บัญชีแจกแจงฟังก์ชันละเอียดครบทั้ง 53 ฟังก์ชัน:** อ่านได้ที่ [ALL_53_FUNCTIONS_FLOWCHARTS.md](ALL_53_FUNCTIONS_FLOWCHARTS.md)

---
© 2026 PetStore Pro. เอกสารนำเสนอโครงงานวิชา CSI402 คณะเทคโนโลยีสารสนเทศ มหาวิทยาลัยศรีปทุม
