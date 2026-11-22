using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSI402_Project_final.Models
{
    // ตรงตาม DATA_DICTIONARY.md และค่าเริ่มต้น User.Role = 0 (Customer) ใน csi402db
    public enum UserRole
    {
        Customer = 0,
        Admin = 1,
        InventoryStaff = 2,
        OrderStaff = 3,
        ShippingStaff = 4,
        MarketingStaff = 5,
        AccountingStaff = 6,
        ITSupport = 7,
        Owner = 8,
        Guest = 9
    }

    public class UserAddress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = "ที่อยู่บ้าน"; // เช่น บ้าน, ที่ทำงาน, คอนโด
        public string RecipientName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string Subdistrict { get; set; } = string.Empty; // ตำบล/แขวง
        public string District { get; set; } = string.Empty;    // อำเภอ/เขต
        public string Province { get; set; } = string.Empty;    // จังหวัด
        public string PostalCode { get; set; } = string.Empty;  // รหัสไปรษณีย์
        public bool IsDefault { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string DisplayAddress =>
            $"{FullAddress} แขวง/ตำบล{Subdistrict} เขต/อำเภอ{District} จ.{Province} {PostalCode}".Trim();
    }

    public class SystemUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = "123456";
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
        public bool IsActive { get; set; } = true;
        public List<string>? GrantedPermissions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ระบบสมุดที่อยู่จัดส่งของลูกค้า
        public List<UserAddress> Addresses { get; set; } = new();

        [NotMapped]
        public UserAddress? DefaultAddress => Addresses.Find(a => a.IsDefault) ?? (Addresses.Count > 0 ? Addresses[0] : null);

        // ==========================================
        // ข้อมูลการระงับบัญชี (จัดเก็บใน JSON GrantedPermissions เพื่อรองรับ MySQL Table `user` ทันที)
        // ==========================================
        private const string PrefixReason = "__SUSPEND_REASON__:";
        private const string PrefixUntil = "__SUSPEND_UNTIL__:";
        private const string PrefixLabel = "__SUSPEND_LABEL__:";

        [NotMapped]
        public string? SuspensionReason
        {
            get
            {
                var entry = GrantedPermissions?.Find(p => p.StartsWith(PrefixReason, StringComparison.Ordinal));
                return entry != null ? entry.Substring(PrefixReason.Length) : null;
            }
        }

        [NotMapped]
        public DateTime? SuspendedUntil
        {
            get
            {
                var entry = GrantedPermissions?.Find(p => p.StartsWith(PrefixUntil, StringComparison.Ordinal));
                if (entry == null) return null;
                var val = entry.Substring(PrefixUntil.Length);
                if (val == "PERMANENT") return null;
                return DateTime.TryParse(val, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt) ? dt : null;
            }
        }

        [NotMapped]
        public bool IsPermanentSuspension
        {
            get
            {
                var entry = GrantedPermissions?.Find(p => p.StartsWith(PrefixUntil, StringComparison.Ordinal));
                return entry == null || entry.Substring(PrefixUntil.Length) == "PERMANENT";
            }
        }

        [NotMapped]
        public string? SuspensionDurationLabel
        {
            get
            {
                var entry = GrantedPermissions?.Find(p => p.StartsWith(PrefixLabel, StringComparison.Ordinal));
                return entry != null ? entry.Substring(PrefixLabel.Length) : null;
            }
        }

        [NotMapped]
        public string SuspensionDisplayUntil
        {
            get
            {
                var label = SuspensionDurationLabel ?? "ระงับถาวร";
                if (SuspendedUntil.HasValue)
                {
                    return $"{label} (สิ้นสุด {SuspendedUntil.Value:dd/MM/yyyy HH:mm} น.)";
                }
                return $"{label} (จนกว่าผู้ดูแลระบบจะปลดระงับ)";
            }
        }

        public void SetSuspension(string? reason, int durationMinutes)
        {
            GrantedPermissions ??= new List<string>();
            GrantedPermissions.RemoveAll(p =>
                p.StartsWith(PrefixReason, StringComparison.Ordinal) ||
                p.StartsWith(PrefixUntil, StringComparison.Ordinal) ||
                p.StartsWith(PrefixLabel, StringComparison.Ordinal));

            string cleanReason = string.IsNullOrWhiteSpace(reason) ? "ระงับการใช้งานโดยผู้ดูแลระบบ" : reason.Trim();
            string label = durationMinutes switch
            {
                15 => "15 นาที",
                60 => "1 ชั่วโมง",
                360 => "6 ชั่วโมง",
                1440 => "1 วัน",
                4320 => "3 วัน",
                10080 => "7 วัน",
                43200 => "30 วัน",
                _ => "ระงับถาวร"
            };

            string untilVal = durationMinutes > 0
                ? DateTime.Now.AddMinutes(durationMinutes).ToString("O")
                : "PERMANENT";

            GrantedPermissions.Add(PrefixReason + cleanReason);
            GrantedPermissions.Add(PrefixUntil + untilVal);
            GrantedPermissions.Add(PrefixLabel + label);
        }

        public void ClearSuspension()
        {
            GrantedPermissions?.RemoveAll(p =>
                p.StartsWith(PrefixReason, StringComparison.Ordinal) ||
                p.StartsWith(PrefixUntil, StringComparison.Ordinal) ||
                p.StartsWith(PrefixLabel, StringComparison.Ordinal));
        }
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        public string Phone { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public List<UserAddress> Addresses { get; set; } = new();
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้หรืออีเมล")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกเบอร์โทรศัพท์")]
        [Phone(ErrorMessage = "เบอร์โทรศัพท์ไม่ถูกต้อง")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [MinLength(6, ErrorMessage = "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
        [Compare("Password", ErrorMessage = "รหัสผ่านยืนยันไม่ตรงกัน")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public static class DemoUserHelper
    {
        public static (string Username, string FullName, string RoleName) GetDemoUserInfo(string? roleOrUsername)
        {
            string r = (roleOrUsername ?? "").Trim().ToLowerInvariant();
            return r switch
            {
                "owner" => ("owner", "คุณภูไพศาล", "Owner"),
                "admin" => ("admin", "ธนพล", "Admin"),
                "marketing" or "marketingstaff" => ("marketing", "อลิสา", "MarketingStaff"),
                "inventory" or "inventorystaff" => ("inventory", "สมชาย", "InventoryStaff"),
                "order" or "orderstaff" => ("order", "วิภา", "OrderStaff"),
                "shipping" or "shippingstaff" => ("shipping", "เดชา", "ShippingStaff"),
                "accounting" or "accountingstaff" => ("accounting", "อรวรรณ", "AccountingStaff"),
                "support" or "itsupport" => ("support", "สมศักดิ์", "ITSupport"),
                "guest" => ("", "ผู้เยี่ยมชม", "Guest"),
                _ => ("customer", "คุณกิตติชัย", "Customer")
            };
        }

        public static string GetRoleHomeUrl(string? role)
        {
            string r = (role ?? "").Trim().ToLowerInvariant();
            return r switch
            {
                "inventorystaff" or "inventory" => "/Inventory",
                "orderstaff" or "order" => "/Order",
                "shippingstaff" or "shipping" => "/Shipping",
                "marketingstaff" or "marketing" => "/Marketing",
                "accountingstaff" or "accounting" => "/Accounting",
                "itsupport" or "support" => "/Support",
                "admin" => "/Admin",
                "owner" => "/Owner",
                _ => "/"
            };
        }

        public static string GetDemoUserId(string? roleOrUsername)
        {
            string r = (roleOrUsername ?? "").Trim().ToLowerInvariant();
            return r switch
            {
                "owner" => "1",
                "admin" => "2",
                "marketing" or "marketingstaff" => "3",
                "inventory" or "inventorystaff" => "4",
                "order" or "orderstaff" => "5",
                "shipping" or "shippingstaff" => "6",
                "accounting" or "accountingstaff" => "7",
                "support" or "itsupport" => "8",
                "guest" => "0",
                _ => "9"
            };
        }
    }
}
