using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CSI402_Project_final.Models
{
    public enum PetCategory
    {
        Dog = 0,
        Cat = 1,
        Other = 2
    }

    public enum ProductCategory
    {
        Food = 0,
        Supply = 1,
        Hygiene = 2
    }

    public enum AgeStage
    {
        AllAges = 0,
        PuppyOrKitten = 1,
        Adult = 2,
        Senior = 3
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public PetCategory PetType { get; set; }
        public ProductCategory Category { get; set; }
        public double WeightKg { get; set; }
        public decimal Price { get; set; }
        public decimal? SpecialDiscountPrice { get; set; }
        public AgeStage AgeStage { get; set; }
        public string SpecialNeed { get; set; } = "ทั่วไป"; // e.g. โรคไต, บำรุงขน, ควบคุมน้ำหนัก
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public int CartAddCount { get; set; } // Req 8: จำนวนคนที่ใส่สินค้านี้ในตะกร้า
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // ฟังก์ชันช่วยตรวจสอบสถานะสต็อก (Req 18)
        [NotMapped]
        public bool IsLowStock => StockQuantity <= LowStockThreshold && StockQuantity > 0;
        [NotMapped]
        public bool IsOutOfStock => StockQuantity <= 0;

        // ราคาปัจจุบันที่แสดง
        [NotMapped]
        public decimal EffectivePrice => SpecialDiscountPrice ?? Price;
    }
}
