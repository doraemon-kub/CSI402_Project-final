using System;
using System.Collections.Generic;
using System.Linq;

namespace CSI402_Project_final.Models
{
    public class CartItem
    {
        public Product Product { get; set; } = new();
        public int Quantity { get; set; } = 1;

        public decimal OriginalSubtotal => Product.Price * Quantity;
        public decimal Subtotal => Product.EffectivePrice * Quantity;
        public double TotalWeightKg => Math.Round(Product.WeightKg * Quantity, 2);
    }

    public class AppliedPromotionSummary
    {
        public string PromotionTitle { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class Cart
    {
        public List<CartItem> Items { get; set; } = new();

        // ผลการคำนวณ
        public decimal OriginalTotalPrice => Items.Sum(i => i.OriginalSubtotal);
        public decimal ProductPriceBeforePromo => Items.Sum(i => i.Subtotal);
        public double TotalWeightKg => Math.Round(Items.Sum(i => i.TotalWeightKg), 2);
        public int TotalItemCount => Items.Sum(i => i.Quantity);

        public List<AppliedPromotionSummary> AppliedPromotions { get; set; } = new();
        public decimal TotalPromotionDiscount => AppliedPromotions.Sum(p => p.DiscountAmount);

        // ของแถม (Req โปรโมชั่น 4)
        public string? FreeGiftItem { get; set; }

        // ค่าจัดส่ง (Req 4, 14, 15, 16)
        public decimal ShippingFee { get; set; }
        public string ShippingCalculationDetail { get; set; } = string.Empty;
        public bool IsFreeShippingQualified { get; set; }

        // ยอดสุทธิหลังหักส่วนลดรวมค่าจัดส่ง (Req 3, 12, 13)
        public decimal NetProductPrice => Math.Max(0, ProductPriceBeforePromo - TotalPromotionDiscount);
        public decimal GrandTotal => NetProductPrice + ShippingFee;

        // -------------------------------------------------------------
        // ฟังก์ชันคำนวณโปรโมชั่นอัตโนมัติ 5 รูปแบบ (Business Logic ใน Models)
        // -------------------------------------------------------------
        public void Recalculate(IEnumerable<Promotion> activePromotions)
        {
            AppliedPromotions.Clear();
            FreeGiftItem = null;
            IsFreeShippingQualified = false;

            if (!Items.Any())
            {
                ShippingFee = 0;
                ShippingCalculationDetail = "ไม่มีสินค้าในตะกร้า";
                return;
            }

            var promoList = activePromotions.Where(p => p.IsValidNow).ToList();

            // 1. โปรโมชั่นที่ 1: ลูกค้าซื้อครบสามชิ้นจะได้ส่วนลด (เช่น ลด 50 บาท)
            var buy3Promo = promoList.FirstOrDefault(p => p.Type == PromotionType.Buy3ItemsDiscount);
            if (buy3Promo != null && TotalItemCount >= 3)
            {
                decimal discount = buy3Promo.DiscountAmount > 0 ? buy3Promo.DiscountAmount : 50m;
                AppliedPromotions.Add(new AppliedPromotionSummary
                {
                    PromotionTitle = buy3Promo.Title,
                    DiscountAmount = discount,
                    Note = $"ซื้อครบ {TotalItemCount} ชิ้น (เกณฑ์ขั้นต่ำ 3 ชิ้น)"
                });
            }

            // 2. โปรโมชั่นที่ 2: รับส่วนลดพิเศษเมื่อซื้ออาหารตามหมวดหมู่อย่างน้อยสองรายการ
            var foodItemsCount = Items.Where(i => i.Product.Category == ProductCategory.Food).Sum(i => i.Quantity);
            var foodVarietyPromo = promoList.FirstOrDefault(p => p.Type == PromotionType.FoodCategoryDiscount);
            if (foodVarietyPromo != null && foodItemsCount >= 2)
            {
                decimal foodTotal = Items.Where(i => i.Product.Category == ProductCategory.Food).Sum(i => i.Subtotal);
                decimal discount = foodVarietyPromo.DiscountPercent > 0 
                    ? Math.Round(foodTotal * (foodVarietyPromo.DiscountPercent / 100m), 2)
                    : 60m;

                AppliedPromotions.Add(new AppliedPromotionSummary
                {
                    PromotionTitle = foodVarietyPromo.Title,
                    DiscountAmount = discount,
                    Note = $"ซื้ออาหารสัตว์เลี้ยง {foodItemsCount} รายการ (เกณฑ์ขั้นต่ำ 2 รายการ)"
                });
            }

            // 3. โปรโมชั่นที่ 5: รับส่วนลดพิเศษเมื่อซื้ออาหารจากแบรนด์เดียวกัน (ตั้งแต่ 2 ชิ้นขึ้นไป)
            var brandFoodPromo = promoList.FirstOrDefault(p => p.Type == PromotionType.SameBrandFoodDiscount);
            if (brandFoodPromo != null)
            {
                var foodByBrand = Items
                    .Where(i => i.Product.Category == ProductCategory.Food)
                    .GroupBy(i => i.Product.Brand)
                    .Where(g => g.Sum(x => x.Quantity) >= 2);

                foreach (var brandGroup in foodByBrand)
                {
                    decimal brandTotal = brandGroup.Sum(x => x.Subtotal);
                    decimal discount = brandFoodPromo.DiscountPercent > 0
                        ? Math.Round(brandTotal * (brandFoodPromo.DiscountPercent / 100m), 2)
                        : 80m;

                    AppliedPromotions.Add(new AppliedPromotionSummary
                    {
                        PromotionTitle = $"{brandFoodPromo.Title} (แบรนด์ {brandGroup.Key})",
                        DiscountAmount = discount,
                        Note = $"ซื้ออาหารแบรนด์ {brandGroup.Key} รวม {brandGroup.Sum(x => x.Quantity)} ชิ้น"
                    });
                }
            }

            // 4. โปรโมชั่นที่ 4: รับของใช้ฟรีเล็กน้อยเมื่อยอดถึงกำหนด (เช่น เสื้อสัตว์เลี้ยง เมื่อยอดครบ 1,000 บาท)
            var giftPromo = promoList.FirstOrDefault(p => p.Type == PromotionType.FreeGiftMilestone);
            if (giftPromo != null && ProductPriceBeforePromo >= (giftPromo.MinSpend > 0 ? giftPromo.MinSpend : 1000m))
            {
                FreeGiftItem = !string.IsNullOrEmpty(giftPromo.GiftItemName) 
                    ? giftPromo.GiftItemName 
                    : "เสื้อยืดสัตว์เลี้ยง Pet Lover ลิมิเต็ด 🎽";
            }

            // 5. โปรโมชั่นที่ 3 & การคำนวณค่าจัดส่งตามเกณฑ์น้ำหนัก (Req 4, 14, 15, 16)
            // เกณฑ์: จัดส่งฟรีเมื่อยอดซื้อถึงกำหนด (เช่น 800 บาท) แต่น้ำหนักต้องไม่เกินเกณฑ์ 10 kg
            // หากน้ำหนักเกิน 10 kg คิดค่าบริการเพิ่มตามน้ำหนักส่วนเกิน (กิโลกรัมละ 20 บาท)
            var shippingPromo = promoList.FirstOrDefault(p => p.Type == PromotionType.FreeShippingWeightCap);
            decimal minSpendForFreeShip = shippingPromo?.MinSpend ?? 800m;
            bool reachedSpendThreshold = ProductPriceBeforePromo >= minSpendForFreeShip;

            if (TotalWeightKg <= 10.0)
            {
                if (reachedSpendThreshold)
                {
                    IsFreeShippingQualified = true;
                    ShippingFee = 0;
                    ShippingCalculationDetail = $"ส่งฟรี! ยอดซื้อครบ ฿{minSpendForFreeShip:N0} และน้ำหนักไม่เกิน 10 kg ({TotalWeightKg} kg)";
                }
                else
                {
                    ShippingFee = 50m;
                    ShippingCalculationDetail = $"ค่าจัดส่งมาตรฐาน ฿50 (น้ำหนัก {TotalWeightKg} kg, ซื้อเพิ่มอีก ฿{minSpendForFreeShip - ProductPriceBeforePromo:N0} เพื่อส่งฟรี)";
                }
            }
            else
            {
                // น้ำหนักเกิน 10 kg (Req 4, 16)
                double excessWeight = Math.Ceiling(TotalWeightKg - 10.0);
                decimal excessFee = (decimal)excessWeight * 20m; // กก. ละ 20 บาท

                if (reachedSpendThreshold)
                {
                    IsFreeShippingQualified = false; // ยกเว้นเฉพาะฐาน แต่มีค่าบริการส่วนเกิน
                    ShippingFee = excessFee;
                    ShippingCalculationDetail = $"ฟรีค่าส่งฐาน แต่มีน้ำหนักเกินเกณฑ์ 10 kg (+{excessWeight} kg x ฿20 = +฿{excessFee:N0})";
                }
                else
                {
                    ShippingFee = 50m + excessFee;
                    ShippingCalculationDetail = $"ค่าส่งฐาน ฿50 + ค่าส่วนเกินน้ำหนัก {excessWeight} kg (+฿{excessFee:N0}) = ฿{ShippingFee:N0}";
                }
            }
        }
    }
}
