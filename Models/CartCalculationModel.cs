using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CSI402_Project_final.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; } = 1;

        [NotMapped]
        public decimal OriginalSubtotal => (Product?.Price ?? 0m) * Quantity;
        [NotMapped]
        public decimal Subtotal => (Product?.EffectivePrice ?? 0m) * Quantity;
        [NotMapped]
        public double TotalWeightKg => Math.Round((Product?.WeightKg ?? 0d) * Quantity, 2);
    }

    public class AppliedPromotionSummary
    {
        public int Id { get; set; }
        public int? CartId { get; set; }
        public string? OrderId { get; set; }
        public string PromotionTitle { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class Cart
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();

        // ผลการคำนวณ
        [NotMapped]
        public decimal OriginalTotalPrice => Items.Sum(i => i.OriginalSubtotal);
        [NotMapped]
        public decimal ProductPriceBeforePromo => Items.Sum(i => i.Subtotal);
        [NotMapped]
        public double TotalWeightKg => Math.Round(Items.Sum(i => i.TotalWeightKg), 2);
        [NotMapped]
        public int TotalItemCount => Items.Sum(i => i.Quantity);

        public List<AppliedPromotionSummary> AppliedPromotions { get; set; } = new();
        [NotMapped]
        public decimal TotalPromotionDiscount => AppliedPromotions.Sum(p => p.DiscountAmount);

        // ของแถม (Req โปรโมชั่น 4)
        public string? FreeGiftItem { get; set; }

        // ค่าจัดส่ง (Req 4, 14, 15, 16)
        public decimal ShippingFee { get; set; }
        public string ShippingCalculationDetail { get; set; } = string.Empty;
        public bool IsFreeShippingQualified { get; set; }

        // ยอดสุทธิหลังหักส่วนลดรวมค่าจัดส่ง (Req 3, 12, 13)
        [NotMapped]
        public decimal NetProductPrice => Math.Max(0, ProductPriceBeforePromo - TotalPromotionDiscount);
        [NotMapped]
        public decimal GrandTotal => NetProductPrice + ShippingFee;

        // อัตราค่าบริการพิเศษสำหรับขนส่งด่วนพิเศษ (EMS)
        public static decimal GetCourierSurcharge(string? courier)
        {
            if (string.IsNullOrWhiteSpace(courier)) return 0m;
            if (courier.Contains("EMS", StringComparison.OrdinalIgnoreCase) ||
                courier.Contains("ไปรษณีย์ไทย", StringComparison.OrdinalIgnoreCase))
            {
                return 30m; // ไปรษณีย์ไทย EMS ด่วนพิเศษ คิดเพิ่ม +30 บาท
            }
            return 0m;
        }

        public decimal GetShippingFeeForCourier(string? courier)
        {
            return ShippingFee + GetCourierSurcharge(courier);
        }

        public decimal GetGrandTotalForCourier(string? courier)
        {
            return NetProductPrice + GetShippingFeeForCourier(courier);
        }

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

            // 1. โปรโมชั่นที่ 1: ซื้อครบ X ชิ้นจะได้ส่วนลด (รองรับหลายแคมเปญและ MinSpend/Percent ที่ฝ่ายการตลาดกำหนด)
            foreach (var buyPromo in promoList.Where(p => p.Type == PromotionType.Buy3ItemsDiscount))
            {
                int minQty = buyPromo.MinQuantity > 0 ? buyPromo.MinQuantity : 3;
                if (TotalItemCount >= minQty && (buyPromo.MinSpend <= 0 || ProductPriceBeforePromo >= buyPromo.MinSpend))
                {
                    decimal discount = buyPromo.DiscountAmount > 0
                        ? buyPromo.DiscountAmount
                        : (buyPromo.DiscountPercent > 0 ? Math.Round(ProductPriceBeforePromo * (buyPromo.DiscountPercent / 100m), 2) : 50m);

                    AppliedPromotions.Add(new AppliedPromotionSummary
                    {
                        PromotionTitle = buyPromo.Title,
                        DiscountAmount = discount,
                        Note = $"ซื้อครบ {TotalItemCount} ชิ้น (เกณฑ์ขั้นต่ำ {minQty} ชิ้น)"
                    });
                }
            }

            // 2. โปรโมชั่นที่ 2: ซื้อสินค้าตามหมวดหมู่อย่างน้อย X รายการ
            foreach (var catPromo in promoList.Where(p => p.Type == PromotionType.FoodCategoryDiscount))
            {
                var targetCat = catPromo.TargetCategory ?? ProductCategory.Food;
                int minQty = catPromo.MinQuantity > 0 ? catPromo.MinQuantity : 2;
                var matchingItems = Items.Where(i => i.Product != null && i.Product.Category == targetCat).ToList();
                var catItemsCount = matchingItems.Sum(i => i.Quantity);
                decimal catTotal = matchingItems.Sum(i => i.Subtotal);

                if (catItemsCount >= minQty && (catPromo.MinSpend <= 0 || catTotal >= catPromo.MinSpend))
                {
                    decimal discount = catPromo.DiscountPercent > 0
                        ? Math.Round(catTotal * (catPromo.DiscountPercent / 100m), 2)
                        : (catPromo.DiscountAmount > 0 ? catPromo.DiscountAmount : 60m);

                    AppliedPromotions.Add(new AppliedPromotionSummary
                    {
                        PromotionTitle = catPromo.Title,
                        DiscountAmount = discount,
                        Note = $"ซื้อสินค้าหมวด {targetCat} {catItemsCount} รายการ (เกณฑ์ขั้นต่ำ {minQty} รายการ)"
                    });
                }
            }

            // 3. โปรโมชั่นที่ 5: ซื้ออาหารจากแบรนด์เดียวกัน (รองรับ ParticipatingBrand และหลายแคมเปญ)
            foreach (var brandFoodPromo in promoList.Where(p => p.Type == PromotionType.SameBrandFoodDiscount))
            {
                int minQty = brandFoodPromo.MinQuantity > 0 ? brandFoodPromo.MinQuantity : 2;
                var targetCat = brandFoodPromo.TargetCategory ?? ProductCategory.Food;
                var foodItems = Items.Where(i => i.Product != null && i.Product.Category == targetCat);

                if (!string.IsNullOrWhiteSpace(brandFoodPromo.ParticipatingBrand))
                {
                    var allowedBrands = brandFoodPromo.ParticipatingBrand
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foodItems = foodItems.Where(i => allowedBrands.Any(b => i.Product.Brand.Equals(b, StringComparison.OrdinalIgnoreCase)));
                }

                var foodByBrand = foodItems
                    .GroupBy(i => i.Product.Brand)
                    .Where(g => g.Sum(x => x.Quantity) >= minQty &&
                                (brandFoodPromo.MinSpend <= 0 || g.Sum(x => x.Subtotal) >= brandFoodPromo.MinSpend));

                foreach (var brandGroup in foodByBrand)
                {
                    decimal brandTotal = brandGroup.Sum(x => x.Subtotal);
                    decimal discount = brandFoodPromo.DiscountPercent > 0
                        ? Math.Round(brandTotal * (brandFoodPromo.DiscountPercent / 100m), 2)
                        : (brandFoodPromo.DiscountAmount > 0 ? brandFoodPromo.DiscountAmount : 80m);

                    AppliedPromotions.Add(new AppliedPromotionSummary
                    {
                        PromotionTitle = $"{brandFoodPromo.Title} (แบรนด์ {brandGroup.Key})",
                        DiscountAmount = discount,
                        Note = $"ซื้อสินค้าแบรนด์ {brandGroup.Key} รวม {brandGroup.Sum(x => x.Quantity)} ชิ้น"
                    });
                }
            }

            // 4. โปรโมชั่นที่ 4: รับของใช้ฟรีเมื่อยอดถึงกำหนด (เลือกของแถมจากแคมเปญที่ยอดขั้นต่ำสูงสุดที่เข้าเกณฑ์)
            var matchedGifts = promoList
                .Where(p => p.Type == PromotionType.FreeGiftMilestone && ProductPriceBeforePromo >= (p.MinSpend > 0 ? p.MinSpend : 1000m))
                .OrderByDescending(p => p.MinSpend)
                .ToList();
            if (matchedGifts.Any())
            {
                var giftPromo = matchedGifts.First();
                FreeGiftItem = !string.IsNullOrEmpty(giftPromo.GiftItemName)
                    ? giftPromo.GiftItemName
                    : "เสื้อยืดสัตว์เลี้ยง Pet Lover ลิมิเต็ด 🎽";
            }

            // 5. โปรโมชั่นที่ 3 & การคำนวณค่าจัดส่งตามเกณฑ์น้ำหนัก (Req 4, 14, 15, 16)
            var shippingPromo = promoList.FirstOrDefault(p => p.Type == PromotionType.FreeShippingWeightCap);
            decimal minSpendForFreeShip = shippingPromo != null && shippingPromo.MinSpend > 0 ? shippingPromo.MinSpend : 800m;
            bool reachedSpendThreshold = shippingPromo != null && ProductPriceBeforePromo >= minSpendForFreeShip;

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
                    ShippingCalculationDetail = shippingPromo != null
                        ? $"ค่าจัดส่งมาตรฐาน ฿50 (น้ำหนัก {TotalWeightKg} kg, ซื้อเพิ่มอีก ฿{Math.Max(0, minSpendForFreeShip - ProductPriceBeforePromo):N0} เพื่อส่งฟรี)"
                        : $"ค่าจัดส่งมาตรฐาน ฿50 (น้ำหนัก {TotalWeightKg} kg)";
                }
            }
            else
            {
                double excessWeight = Math.Ceiling(TotalWeightKg - 10.0);
                decimal excessFee = (decimal)excessWeight * 20m;

                if (reachedSpendThreshold)
                {
                    IsFreeShippingQualified = false;
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
