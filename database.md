Field Name,Data type,Description
Id,int,รหัสผู้ใช้
Username,varchar(50),ชื่อผู้ใช้งาน (ไม่ซ้ำกัน)
Password,varchar(200),รหัสผ่าน
FullName,varchar(100),ชื่อ-นามสกุล
Email,varchar(200),อีเมล (ไม่ซ้ำกัน)
Phone,varchar(20),เบอร์โทรศัพท์
Role,int,บทบาทในระบบ (1 ใน 10 บทบาท)
IsActive,tinyint(1),"สถานะบัญชี (1=เปิดใช้งาน, 0=ระงับใช้งานโดย Admin)"
GrantedPermissions,json,สิทธิ์พิเศษที่ Admin กำหนดเพิ่มเติม
CreatedAt,datetime,วัน-เวลาที่สมัคร/สร้างบัญชี
UpdatedAt,datetime,วัน-เวลาที่อัปเดตข้อมูลบัญชีล่าสุด
