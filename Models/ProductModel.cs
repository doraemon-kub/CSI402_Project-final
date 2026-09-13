namespace CSI402_Project_final.Models
{
    public enum PetCategory
    {
        Dog,
        Cat,
        Other
    }

    public enum ProductCategory
    {
        Food,
        Supply,
        Hygiene
    }

    public enum AgeStage
    {
        AllAges,
        PuppyOrKitten,
        Adult,
        Senior
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

        // ฟังก์ชันช่วยตรวจสอบสถานะสต็อก (Req 18)
        public bool IsLowStock => StockQuantity <= LowStockThreshold && StockQuantity > 0;
        public bool IsOutOfStock => StockQuantity <= 0;

        // ราคาปัจจุบันที่แสดง
        public decimal EffectivePrice => SpecialDiscountPrice ?? Price;
    }
}
