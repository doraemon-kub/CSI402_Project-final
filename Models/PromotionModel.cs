using System;

namespace CSI402_Project_final.Models
{
    public enum PromotionType
    {
        Buy3ItemsDiscount,      // โปร 1: ซื้อครบ 3 ชิ้นได้ส่วนลด
        FoodCategoryDiscount,   // โปร 2: ซื้ออาหารอย่างน้อย 2 รายการได้ส่วนลดพิเศษ
        FreeShippingWeightCap,  // โปร 3: ส่งฟรีเมื่อยอดถึงกำหนด แต่น้ำหนัก <= 10 kg
        FreeGiftMilestone,      // โปร 4: ซื้อยอดถึงกำหนด รับของใช้ฟรี (เช่น เสื้อ)
        SameBrandFoodDiscount   // โปร 5: ซื้ออาหารจากแบรนด์เดียวกันได้ส่วนลดพิเศษ
    }

    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PromotionType Type { get; set; }
        
        // เงื่อนไขโปรโมชั่น (Req 25)
        public int MinQuantity { get; set; } = 0;
        public decimal MinSpend { get; set; } = 0;
        public string? ParticipatingBrand { get; set; } // สำหรับแบรนด์ที่เข้าร่วม
        public ProductCategory? TargetCategory { get; set; }
        
        // สิทธิประโยชน์
        public decimal DiscountAmount { get; set; } = 0; // ลดเป็นบาท
        public decimal DiscountPercent { get; set; } = 0; // ลดเป็น %
        public string? GiftItemName { get; set; } // ของแถม เช่น "เสื้อยืดสัตว์เลี้ยง"

        // ระยะเวลา (Req 26)
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // สถิติการใช้งาน (Req 27)
        public int UsageCount { get; set; } = 0;

        // ฟังก์ชันตรวจสอบว่าโปรโมชั่นยังสามารถใช้ได้หรือไม่
        public bool IsValidNow => IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        public void IncrementUsage()
        {
            UsageCount++;
        }
    }
}
