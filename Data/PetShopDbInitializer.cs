using System;
using System.Collections.Generic;
using System.Linq;
using CSI402_Project_final.Data;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project_final.Models
{
    public static class PetShopDbInitializer
    {
        public static void Initialize(PetShopDbContext db)
        {
            try
            {
                db.Database.OpenConnection();
                db.Database.CloseConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Database Warning] ยังไม่สามารถเชื่อมต่อฐานข้อมูลได้: {ex.Message}");
                return;
            }

            // สร้างโครงสร้างตารางทั้งหมดอัตโนมัติหากยังไม่มีในฐานข้อมูล (สำหรับ Cloud DB ใหม่)
            db.Database.EnsureCreated();

            // 1. Seed บัญชีผู้ใช้งานระบบ 9 บทบาท (เพิ่มเฉพาะบัญชีที่ยังไม่มี เพื่อรักษาข้อมูลที่มีอยู่เดิมในตาราง user)
            var now = DateTime.Now;
            var seedUsers = new List<SystemUser>
            {
                new SystemUser { Username = "owner", Password = "123456", FullName = "คุณภูไพศาล", Email = "owner@petshop.com", Phone = "081-999-0001", Role = UserRole.Owner, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "admin", Password = "123456", FullName = "ธนพล", Email = "admin@petshop.com", Phone = "081-999-0002", Role = UserRole.Admin, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "marketing", Password = "123456", FullName = "อลิสา", Email = "marketing@petshop.com", Phone = "081-999-0003", Role = UserRole.MarketingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "inventory", Password = "123456", FullName = "สมชาย", Email = "inventory@petshop.com", Phone = "081-999-0004", Role = UserRole.InventoryStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "order", Password = "123456", FullName = "วิภา", Email = "order@petshop.com", Phone = "081-999-0005", Role = UserRole.OrderStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "shipping", Password = "123456", FullName = "เดชา", Email = "shipping@petshop.com", Phone = "081-999-0006", Role = UserRole.ShippingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "accounting", Password = "123456", FullName = "อรวรรณ", Email = "accounting@petshop.com", Phone = "081-999-0007", Role = UserRole.AccountingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "support", Password = "123456", FullName = "สมศักดิ์", Email = "support@petshop.com", Phone = "081-999-0008", Role = UserRole.ITSupport, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "customer", Password = "123456", FullName = "คุณกิตติชัย", Email = "customer@example.com", Phone = "081-234-5678", Role = UserRole.Customer, IsActive = true, CreatedAt = now, UpdatedAt = now }
            };

            var existingUsernames = db.Users.Select(u => u.Username.ToLower()).ToHashSet();
            var existingEmails = db.Users.Select(u => u.Email.ToLower()).ToHashSet();

            var usersToAdd = seedUsers
                .Where(u => !existingUsernames.Contains(u.Username.ToLower()) && !existingEmails.Contains(u.Email.ToLower()))
                .ToList();

            if (usersToAdd.Any())
            {
                db.Users.AddRange(usersToAdd);
                db.SaveChanges();
            }

            // ล้างชื่อผู้ใช้งานในฐานข้อมูลที่มีวงเล็บบทบาทต่อท้าย และคืนค่า Role/IsActive มาตรฐานให้บัญชีทดสอบหลักทั้ง 9 บัญชี
            var canonicalRoles = seedUsers.ToDictionary(u => u.Username.ToLowerInvariant(), u => u.Role);
            var usersInDb = db.Users.ToList();
            bool hasCleanedUsers = false;
            foreach (var u in usersInDb)
            {
                if (!string.IsNullOrWhiteSpace(u.FullName) && System.Text.RegularExpressions.Regex.IsMatch(u.FullName, @"\s*\([^)]+\)"))
                {
                    u.FullName = System.Text.RegularExpressions.Regex.Replace(u.FullName, @"\s*\([^)]+\)", "").Trim();
                    hasCleanedUsers = true;
                }

                if (canonicalRoles.TryGetValue(u.Username.ToLowerInvariant(), out var expectedRole))
                {
                    if (u.Role != expectedRole)
                    {
                        u.Role = expectedRole;
                        hasCleanedUsers = true;
                    }
                }
            }
            if (hasCleanedUsers)
            {
                db.SaveChanges();
            }

            // ล้างค่า mock CartAddCount ในฐานข้อมูลทั้งหมดให้เป็น 0 ตามที่ผู้ใช้ต้องการ
            var productsWithMockCart = db.Products.Where(p => p.CartAddCount > 0).ToList();
            if (productsWithMockCart.Any())
            {
                foreach (var p in productsWithMockCart)
                {
                    p.CartAddCount = 0;
                }
                db.SaveChanges();
            }

            // 2. Seed โปรโมชั่น 5 รูปแบบตามข้อกำหนดระบบ (หากตารางยังว่าง)
            if (!db.Promotions.Any())
            {
                db.Promotions.AddRange(
                    new Promotion
                    {
                        Code = "BUY3SAVE",
                        Title = "โปรโมชั่น 1: ซื้อครบ 3 ชิ้น ลดทันที 50 บาท",
                        Description = "ซื้อสินค้าใดก็ได้ครบ 3 ชิ้น รับส่วนลดทันที 50 บาท",
                        Type = PromotionType.Buy3ItemsDiscount,
                        MinQuantity = 3,
                        DiscountAmount = 50m,
                        StartDate = now.AddMonths(-1),
                        EndDate = now.AddMonths(3),
                        IsActive = true,
                        UsageCount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new Promotion
                    {
                        Code = "FOOD2VARIETY",
                        Title = "โปรโมชั่น 2: ซื้ออาหารอย่างน้อย 2 รายการ ลดพิเศษ 10%",
                        Description = "ซื้ออาหารสัตว์เลี้ยงในหมวดอาหารตั้งแต่ 2 รายการขึ้นไป ลด 10%",
                        Type = PromotionType.FoodCategoryDiscount,
                        TargetCategory = ProductCategory.Food,
                        MinQuantity = 2,
                        DiscountPercent = 10m,
                        StartDate = now.AddMonths(-1),
                        EndDate = now.AddMonths(3),
                        IsActive = true,
                        UsageCount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new Promotion
                    {
                        Code = "FREESHIP10KG",
                        Title = "โปรโมชั่น 3: จัดส่งฟรีเมื่อยอดถึง 800 บาท (น้ำหนักไม่เกิน 10 kg)",
                        Description = "ยอดซื้อครบ 800 บาทขึ้นไป จัดส่งฟรีหากน้ำหนักรวมไม่เกิน 10 kg",
                        Type = PromotionType.FreeShippingWeightCap,
                        MinSpend = 800m,
                        StartDate = now.AddMonths(-1),
                        EndDate = now.AddMonths(3),
                        IsActive = true,
                        UsageCount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new Promotion
                    {
                        Code = "FREEGIFT1000",
                        Title = "โปรโมชั่น 4: ช้อปครบ 1,000 บาท รับฟรีเสื้อสัตว์เลี้ยงสุดน่ารัก",
                        Description = "ยอดซื้อครบ 1,000 บาท รับของแถมฟรี เสื้อยืดสัตว์เลี้ยง Pet Lover",
                        Type = PromotionType.FreeGiftMilestone,
                        MinSpend = 1000m,
                        GiftItemName = "เสื้อยืดสัตว์เลี้ยง Pet Lover ลิมิเต็ด 🎽",
                        StartDate = now.AddMonths(-1),
                        EndDate = now.AddMonths(3),
                        IsActive = true,
                        UsageCount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new Promotion
                    {
                        Code = "SAMEBRAND15",
                        Title = "โปรโมชั่น 5: ซื้ออาหารแบรนด์เดียวกันตั้งแต่ 2 ชิ้น ลด 15%",
                        Description = "ซื้ออาหารจากแบรนด์เดียวกัน 2 ชิ้นขึ้นไป ลดเพิ่ม 15%",
                        Type = PromotionType.SameBrandFoodDiscount,
                        MinQuantity = 2,
                        DiscountPercent = 15m,
                        StartDate = now.AddMonths(-1),
                        EndDate = now.AddMonths(3),
                        IsActive = true,
                        UsageCount = 0,
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                );
                db.SaveChanges();
            }

            // 3. Seed สินค้าเริ่มต้นและเพิ่มเติมให้ครอบคลุมทุกหมวดหมู่ พร้อมรูปสินค้าจริงที่ตรงกับชื่อสินค้า 100%
            var existingProductsByName = db.Products
                .AsEnumerable()
                .GroupBy(p => p.Name.ToLower())
                .ToDictionary(g => g.Key, g => g.First());

            var catalog = new List<Product>
            {
                // ================= DOG PRODUCTS =================
                new Product
                {
                    Name = "Royal Canin Medium Adult (อาหารสุนัขโต)",
                    Brand = "Royal Canin",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 3.0,
                    Price = 690m,
                    SpecialDiscountPrice = 620m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 15,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p01_rc_medium_adult.jpg",
                    Description = "อาหารเม็ดสำหรับสุนัขโตพันธุ์กลาง ช่วยเสริมสร้างภูมิคุ้มกันและดูแลทางเดินอาหาร",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Royal Canin Puppy Maxi (อาหารลูกสุนัขพันธุ์ใหญ่)",
                    Brand = "Royal Canin",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 4.0,
                    Price = 790m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.PuppyOrKitten,
                    SpecialNeed = "เสริมสร้างกระดูก",
                    StockQuantity = 8,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p02_rc_puppy_maxi.jpg",
                    Description = "สำหรับลูกสุนัขพันธุ์ใหญ่ อายุ 2-15 เดือน โปรตีนคุณภาพสูง เสริมสร้างโครงสร้างกระดูก",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Royal Canin Mini Ageing 12+ (อาหารสุนัขสูงวัยพันธุ์เล็ก)",
                    Brand = "Royal Canin",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 1.5,
                    Price = 420m,
                    SpecialDiscountPrice = 385m,
                    AgeStage = AgeStage.Senior,
                    SpecialNeed = "บำรุงไตและชะลอวัย",
                    StockQuantity = 10,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p03_rc_mini_ageing.jpg",
                    Description = "เม็ดอาหารแช่น้ำให้นิ่มง่าย สำหรับสุนัขสูงวัย 12 ปีขึ้นไป มีสารต้านอนุมูลอิสระชะลอความเสื่อมของเซลล์ไต",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Hill's Science Diet Puppy Small Bites (อาหารลูกสุนัขเม็ดเล็ก)",
                    Brand = "Hill's",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 2.0,
                    Price = 550m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.PuppyOrKitten,
                    SpecialNeed = "บำรุงสมอง",
                    StockQuantity = 12,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p04_hills_puppy.jpg",
                    Description = "สูตรเม็ดเล็กทานง่าย มี DHA ธรรมชาติจากน้ำมันปลา พัฒนาการสมองและการมองเห็น",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "SmartHeart Gold Holistic Adult Dog (อาหารสุนัขเกรดโฮลิสติก 10kg)",
                    Brand = "SmartHeart",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 10.0,
                    Price = 950m,
                    SpecialDiscountPrice = 890m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "บำรุงขนและผิวหนัง",
                    StockQuantity = 6,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p05_smartheart_holistic.jpg",
                    Description = "อาหารเกรดโฮลิสติก โปรตีนจากเนื้อแกะและข้าวกล้อง ปราศจากข้าวโพดและถั่วเหลือง น้ำหนักพอดีเกณฑ์ 10kg",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Pedigree Pro High Protein Adult (อาหารสุนัขโตโปรตีนสูง 15kg)",
                    Brand = "Pedigree",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 15.0,
                    Price = 1390m,
                    SpecialDiscountPrice = 1250m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "เสริมกล้ามเนื้อ",
                    StockQuantity = 4,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p06_pedigree_pro.jpg",
                    Description = "กระสอบจัมโบ้ 15 กิโลกรัม เหมาะสำหรับสุนัขพลังงานสูง น้ำหนักเกิน 10kg คำนวณค่าส่งส่วนเกินตามเกณฑ์",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Orijen Original Dog Grain-Free (อาหารสุนัขเกรนฟรี 2kg)",
                    Brand = "Orijen",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Food,
                    WeightKg = 2.0,
                    Price = 1150m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "แพ้ง่าย",
                    StockQuantity = 7,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p07_orijen_original.jpg",
                    Description = "อาหารสุนัขเกรดซูเปอร์พรีเมียมจากแคนาดา ปราศจากธัญพืช เนื้อสัตว์สดคุณภาพ 85%",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "KONG Classic ยางกัดของเล่นสุนัขทนทาน",
                    Brand = "KONG",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.3,
                    Price = 320m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "ของเล่นเสริมพัฒนาการ",
                    StockQuantity = 25,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p08_kong_classic.jpg",
                    Description = "ของเล่นสุนัขยางธรรมชาติสำหรับแทะและขัดฟัน ลดความเครียด สามารถสอดไส้ขนมได้",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "เบาะนอนเมมโมรี่โฟม Orthopedic ไซส์ L สำหรับสุนัข",
                    Brand = "PetCare",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 2.5,
                    Price = 890m,
                    SpecialDiscountPrice = 790m,
                    AgeStage = AgeStage.Senior,
                    SpecialNeed = "เสริมข้อต่อกระดูก",
                    StockQuantity = 5,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p09_orthopedic_dog_bed.jpg",
                    Description = "เบาะนอนเพื่อสุขภาพช่วยกระจายแรงกดทับ สำหรับสุนัขโตและสุนัขสูงอายุที่มีปัญหาข้อสะโพก",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ชุดสายจูงและรัดอกสุนัขสะท้อนแสงกันกระชาก Truelove",
                    Brand = "Truelove",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.4,
                    Price = 590m,
                    SpecialDiscountPrice = 490m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "สายจูงและเดินทาง",
                    StockQuantity = 20,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p10_truelove_harness.jpg",
                    Description = "สายรัดอกสุนัขบุฟองน้ำระบายอากาศ ลดแรงดึงกระชาก พร้อมแถบสะท้อนแสง 3M ปลอดภัยยามค่ำคืน",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ชามอาหารสุนัขชะลอการกิน Slow Feeder ป้องกันสำลัก",
                    Brand = "Outward Hound",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.5,
                    Price = 350m,
                    SpecialDiscountPrice = 299m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "อุปกรณ์การกินและน้ำ",
                    StockQuantity = 18,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p11_slow_feeder_bowl.jpg",
                    Description = "ชามดีไซน์เขาวงกตช่วยชะลอความเร็วในการกินอาหารของสุนัข ป้องกันอาการท้องอืดและสำลักอาหาร",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ของเล่นยางนิ่มฝึกเคี้ยวสำหรับลูกสุนัข Nylabone Puppy Chew",
                    Brand = "Nylabone",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.2,
                    Price = 260m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.PuppyOrKitten,
                    SpecialNeed = "ของเล่นเสริมพัฒนาการ",
                    StockQuantity = 22,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p12_nylabone_puppy.jpg",
                    Description = "ยางเคี้ยวเนื้อนุ่มพิเศษสำหรับลูกสุนัขช่วงฟันน้ำนมขึ้น ช่วยนวดเหงือกและลดพฤติกรรมกัดแทะเฟอร์นิเจอร์",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ทางลาดไม้ขึ้นเตียงและโซฟาปรับระดับ สำหรับสุนัขสูงวัย",
                    Brand = "PetCare",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 4.5,
                    Price = 1290m,
                    SpecialDiscountPrice = 1090m,
                    AgeStage = AgeStage.Senior,
                    SpecialNeed = "เสริมข้อต่อกระดูก",
                    StockQuantity = 6,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p13_wooden_dog_ramp.jpg",
                    Description = "ทางลาดไม้สนธรรมชาติปูพรมกันลื่น ปรับความสูงได้ 4 ระดับ ช่วยถนอมข้อต่อและกระดูกสันหลังสุนัขสูงอายุ",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "เสื้อฮู้ดกันหนาวสัตว์เลี้ยงเนื้อผ้าคอตตอนเกรดพรีเมียม",
                    Brand = "PetLover",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.25,
                    Price = 390m,
                    SpecialDiscountPrice = 320m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "เสื้อผ้าและแฟชั่น",
                    StockQuantity = 24,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p14_pet_hoodie.jpg",
                    Description = "เสื้อฮู้ดผ้าคอตตอนผสมฟลีซนุ่มอบอุ่น สวมใส่ง่าย ระบายอากาศดี เหมาะสำหรับห้องแอร์และฤดูหนาว",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Sukina Petto แผ่นรองซับปัสสาวะสุนัขแห้งไว (50 ชิ้น)",
                    Brand = "Sukina",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Hygiene,
                    WeightKg = 1.8,
                    Price = 290m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สุขอนามัย",
                    StockQuantity = 30,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p15_sukina_pee_pad.jpg",
                    Description = "แผ่นอนามัยซึมซับเจลพิเศษ ดับกลิ่นสาบและป้องกันแบคทีเรีย ซึมซับไวไม่รั่วซึม",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "แชมพูสุนัข Sleeky สูตรลดคันและบำรุงขน (500ml)",
                    Brand = "Sleeky",
                    PetType = PetCategory.Dog,
                    Category = ProductCategory.Hygiene,
                    WeightKg = 0.6,
                    Price = 145m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "บำรุงขนและผิวหนัง",
                    StockQuantity = 18,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p16_sleeky_shampoo.jpg",
                    Description = "แชมพูอาบน้ำสูตรอ่อนโยนผสมคอนดิชันเนอร์ ป้องกันขนพันกัน กลิ่นหอมสดชื่น",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // ================= CAT PRODUCTS =================
                new Product
                {
                    Name = "Me-O Gold Kitten (อาหารลูกแมวเกรดพรีเมียม)",
                    Brand = "Me-O",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 1.2,
                    Price = 280m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.PuppyOrKitten,
                    SpecialNeed = "บำรุงสมองและสายตา",
                    StockQuantity = 20,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p17_meo_gold_kitten.jpg",
                    Description = "อาหารลูกแมวอุดมด้วย DHA จากน้ำมันปลาทะเล และพรีไบโอติก เสริมภูมิคุ้มกัน",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Me-O Gold Persian Adult (อาหารแมวเปอร์เซียสูตรป้องกันก้อนขน)",
                    Brand = "Me-O",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 1.2,
                    Price = 310m,
                    SpecialDiscountPrice = 289m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "ขับก้อนขน",
                    StockQuantity = 15,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p18_meo_gold_persian.jpg",
                    Description = "อาหารแมวเปอร์เซียและแมวขนยาว เสริมใยอาหารธรรมชาติป้องกันการเกิดก้อนขนอุดตัน",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Royal Canin Hair & Skin Care (อาหารแมวบำรุงขน 2kg)",
                    Brand = "Royal Canin",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 2.0,
                    Price = 680m,
                    SpecialDiscountPrice = 620m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "บำรุงขนและผิวหนัง",
                    StockQuantity = 12,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p19_rc_hair_skin.jpg",
                    Description = "ช่วยให้ขนเงางามสุขภาพดีขึ้นภายใน 21 วัน มีกรดไขมันโอเมก้า 3 และ 6 สูง",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Royal Canin Indoor 7+ (อาหารแมวสูงวัยในบ้าน)",
                    Brand = "Royal Canin",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 1.5,
                    Price = 590m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.Senior,
                    SpecialNeed = "ชะลอความชรา",
                    StockQuantity = 9,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p20_rc_indoor_7.jpg",
                    Description = "ชะลอความเสื่อมของไต ควบคุมฟอสฟอรัส เสริมสร้างการทำงานของหัวใจในแมวอายุ 7 ปีขึ้นไป",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Kaniva อาหารแมวสูตรเนื้อแกะ ทูน่า และข้าว (1.4kg)",
                    Brand = "Kaniva",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 1.4,
                    Price = 420m,
                    SpecialDiscountPrice = 380m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "บำรุงขนและผิวหนัง",
                    StockQuantity = 16,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p21_kaniva_cat_food.jpg",
                    Description = "โซเดียมต่ำ ไร้ข้าวโพดและกลูเตน ขนสวยเงางาม รสชาติอร่อยถูกใจเจ้านาย",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Whiskas 1+ อาหารแมวโต รสปลาทู (กระสอบ 7kg)",
                    Brand = "Whiskas",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Food,
                    WeightKg = 7.0,
                    Price = 720m,
                    SpecialDiscountPrice = 670m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 8,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p22_whiskas_mackerel.jpg",
                    Description = "อาหารเม็ดกรอบสอดไส้ครีม รสปลาทูยอดนิยม บำรุงสายตาและการมองเห็น",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "คอนโดแมว 3 ชั้น พร้อมเสาลับเล็บเชือกป่านและเปลนอน",
                    Brand = "CatVillas",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 5.5,
                    Price = 990m,
                    SpecialDiscountPrice = 850m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 6,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p23_cat_condo_tree.jpg",
                    Description = "คอนโดแมวเนื้อผ้ากำมะหยี่นุ่มสบาย เสาหุ้มเชือกป่านธรรมชาติ พร้อมเปลนอนและลูกบอลดึ๋งดั๋ง",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ไม้ตกแมวขนนกฟรุ้งฟริ้ง พร้อมกระดิ่งกรุ๊งกริ๊ง",
                    Brand = "PetFun",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.1,
                    Price = 69m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "ของเล่นเสริมพัฒนาการ",
                    StockQuantity = 40,
                    LowStockThreshold = 10,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p24_cat_feather_wand.jpg",
                    Description = "ก้านจับยืดหยุ่น ขนนกสีสันสดใส กระตุ้นสัญชาตญาณนักล่าและการออกกำลังกายของแมว",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "น้ำพุแมวอัตโนมัติไร้สาย PETKIT Eversweet ระบบกรอง 3 ชั้น (2L)",
                    Brand = "PETKIT",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 1.2,
                    Price = 1290m,
                    SpecialDiscountPrice = 1090m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "อุปกรณ์การกินและน้ำ",
                    StockQuantity = 14,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p25_petkit_fountain.jpg",
                    Description = "น้ำพุแมวระบบปั๊มไร้สายเงียบพิเศษ กรองสิ่งสกปรกและคลอรีน 3 ขั้นตอน กระตุ้นให้น้องแมวดื่มน้ำมากขึ้นเพื่อป้องกันโรคไต",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "กระเป๋าเป้อวกาศพาสัตว์เลี้ยงเดินทาง ระบายอากาศรอบทิศทาง",
                    Brand = "Pidan",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 1.5,
                    Price = 890m,
                    SpecialDiscountPrice = 750m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สายจูงและเดินทาง",
                    StockQuantity = 10,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p26_space_pet_backpack.jpg",
                    Description = "กระเป๋าเป้พกพาสัตว์เลี้ยงพร้อมหน้าต่างใสชมวิว ช่องตาข่ายระบายอากาศ 9 จุด แข็งแรงสะพายสบาย",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "กระบะทรายแมวทรงโดมขนาดจัมโบ้ พร้อมที่ตักและแผ่นดักทราย",
                    Brand = "Catit",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 3.2,
                    Price = 850m,
                    SpecialDiscountPrice = 720m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สุขอนามัย",
                    StockQuantity = 9,
                    LowStockThreshold = 3,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p27_catit_litter_box.jpg",
                    Description = "ห้องน้ำแมวแบบโดมปิดเก็บกลิ่นมิดชิด มีฟิลเตอร์คาร์บอนดูดกลิ่นและตะแกรงดักทรายติดเท้าแมว",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "แปรงหวีขนแมวและสุนัข ปุ่มกดดันขนออกในคลิกเดียว",
                    Brand = "Furminator",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.2,
                    Price = 290m,
                    SpecialDiscountPrice = 240m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "บำรุงขนและผิวหนัง",
                    StockQuantity = 25,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p28_pet_grooming_brush.jpg",
                    Description = "ซี่แปรงสแตนเลสหุ้มซิลิโคนถนอมผิวหนัง สางขนเสียและลดการเกิดก้อนขนอุดตัน กดปุ่มทำความสะอาดขนออกจากแปรงได้ทันที",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "รางบอลของเล่นลูกแมว 3 ชั้น พร้อมตุ๊กตาแคทนิปธรรมชาติ",
                    Brand = "PetFun",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.6,
                    Price = 250m,
                    SpecialDiscountPrice = 199m,
                    AgeStage = AgeStage.PuppyOrKitten,
                    SpecialNeed = "ของเล่นเสริมพัฒนาการ",
                    StockQuantity = 18,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p29_cat_tower_ball_toy.jpg",
                    Description = "รางบอลหมุน 3 ชั้นสีพาสเทล ฝึกทักษะการตะปบและสายตาของลูกแมว พร้อมถุงแคทนิปออร์แกนิก",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "เครื่องให้อาหารสัตว์เลี้ยงอัตโนมัติ สั่งงานผ่านสมาร์ตโฟน (4L)",
                    Brand = "Xiaomi Pet",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Supply,
                    WeightKg = 2.8,
                    Price = 1990m,
                    SpecialDiscountPrice = 1690m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "อุปกรณ์การกินและน้ำ",
                    StockQuantity = 7,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p30_xiaomi_pet_feeder.jpg",
                    Description = "ตั้งเวลาและปริมาณการให้อาหารได้แม่นยำผ่านแอปพลิเคชัน มีถังเก็บความชื้นและระบบสำรองไฟฉุกเฉิน",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ทรายแมวเต้าหู้ Eco Catz (7L)",
                    Brand = "EcoCatz",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Hygiene,
                    WeightKg = 2.8,
                    Price = 199m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สุขอนามัย",
                    StockQuantity = 12,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p31_ecocatz_tofu_litter.jpg",
                    Description = "จับตัวเป็นก้อนเร็ว ละลายน้ำได้ ทิ้งลงชักโครกได้ ไร้ฝุ่น 99%",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Kasty ทรายแมวเต้าหู้พรีเมียม กลิ่นนมฮอกไกโด (6L)",
                    Brand = "Kasty",
                    PetType = PetCategory.Cat,
                    Category = ProductCategory.Hygiene,
                    WeightKg = 2.4,
                    Price = 240m,
                    SpecialDiscountPrice = 219m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สุขอนามัย",
                    StockQuantity = 15,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p32_kasty_tofu_litter.jpg",
                    Description = "เม็ดเล็ก 1.5mm ประหยัดทราย ไร้ฝุ่น คุมกลิ่นยอดเยี่ยม ฟู้ดเกรด ปลอดภัยต่อน้องแมว",
                    CreatedAt = now,
                    UpdatedAt = now
                },

                // ================= OTHER PETS PRODUCTS =================
                new Product
                {
                    Name = "Oxbow Essentials Adult Rabbit Food (อาหารเม็ดกระต่ายโต 2.25kg)",
                    Brand = "Oxbow",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Food,
                    WeightKg = 2.25,
                    Price = 520m,
                    SpecialDiscountPrice = 470m,
                    AgeStage = AgeStage.Adult,
                    SpecialNeed = "ไฟเบอร์สูง",
                    StockQuantity = 14,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p33_oxbow_rabbit_food.jpg",
                    Description = "อาหารเม็ดหญ้าทิโมธีคุณภาพสูงสำหรับกระต่ายโต ไฟเบอร์สูง เสริมสร้างระบบทางเดินอาหาร",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Marukan ขนมผลไม้รวมอบแห้งสำหรับแฮมสเตอร์และชูการ์",
                    Brand = "Marukan",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Food,
                    WeightKg = 0.15,
                    Price = 135m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "เสริมวิตามิน",
                    StockQuantity = 22,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p34_marukan_dried_fruit.jpg",
                    Description = "ขนมผลไม้อบแห้งธรรมชาติ สำหรับแฮมสเตอร์ กระต่าย แกสบี้ และชูการ์ไกลเดอร์ ปราศจากน้ำตาลปรุงแต่ง",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "กรงกระต่ายและแกสบี้ขนาดใหญ่ พร้อมถาดดึงทำความสะอาดง่าย",
                    Brand = "PetHome",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Supply,
                    WeightKg = 4.2,
                    Price = 750m,
                    SpecialDiscountPrice = 690m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 7,
                    LowStockThreshold = 2,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p35_rabbit_cage.jpg",
                    Description = "กรงเหล็กพ่นกันสนิมอย่างดี มีบันไดและชั้นลอย ถาดรองด้านล่างดึงล้างทำความสะอาดง่าย สะดวกสบาย",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "วงล้อวิ่งแฮมสเตอร์ไร้เสียง Silent Spinner ขนาด 21 cm",
                    Brand = "Marukan",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.45,
                    Price = 340m,
                    SpecialDiscountPrice = 290m,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "ของเล่นเสริมพัฒนาการ",
                    StockQuantity = 16,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p36_silent_spinner_wheel.jpg",
                    Description = "วงล้อออกกำลังกายระบบลูกปืนไร้เสียงรบกวน พื้นผิวกันลื่นปลอดภัยต่ออุ้งเท้าแฮมสเตอร์และเม่นแคระ",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "ชุดขวดน้ำลูกกลิ้งสแตนเลสติดกรงและชามเซรามิกสำหรับกระต่าย",
                    Brand = "Oxbow",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Supply,
                    WeightKg = 0.5,
                    Price = 280m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "อุปกรณ์การกินและน้ำ",
                    StockQuantity = 20,
                    LowStockThreshold = 5,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p37_rabbit_water_bottle_bowl.jpg",
                    Description = "ขวดน้ำหัวลูกกลิ้งสแตนเลสป้องกันน้ำหยด พร้อมชามเซรามิกฐานหนักป้องกันการคว่ำชามอาหาร",
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Product
                {
                    Name = "Kaytee ขี้เลื่อยกระดาษ Clean & Cozy ไร้ฝุ่น (1kg)",
                    Brand = "Kaytee",
                    PetType = PetCategory.Other,
                    Category = ProductCategory.Hygiene,
                    WeightKg = 1.0,
                    Price = 260m,
                    SpecialDiscountPrice = null,
                    AgeStage = AgeStage.AllAges,
                    SpecialNeed = "สุขอนามัย",
                    StockQuantity = 18,
                    LowStockThreshold = 4,
                    CartAddCount = 0,
                    ImageUrl = "/images/products/p38_kaytee_clean_cozy.jpg",
                    Description = "วัสดุรองกรงทำจากเยื่อกระดาษบริสุทธิ์ ไร้ฝุ่น 99.9% ซึมซับดีกว่าขี้เลื่อยทั่วไป 2 เท่า นุ่มฟูปลอดภัยต่อระบบหายใจ",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            foreach (var item in catalog)
            {
                var key = item.Name.ToLower();
                if (!existingProductsByName.TryGetValue(key, out var existingProd))
                {
                    db.Products.Add(item);
                }
                else
                {
                    bool needsUpdate = false;
                    if (string.IsNullOrEmpty(existingProd.ImageUrl) ||
                        existingProd.ImageUrl.Contains("images.unsplash.com") ||
                        (existingProd.ImageUrl.StartsWith("/images/products/") && existingProd.ImageUrl != item.ImageUrl))
                    {
                        existingProd.ImageUrl = item.ImageUrl;
                        needsUpdate = true;
                    }
                    if (existingProd.SpecialNeed != item.SpecialNeed)
                    {
                        existingProd.SpecialNeed = item.SpecialNeed;
                        needsUpdate = true;
                    }
                    if (needsUpdate)
                    {
                        existingProd.UpdatedAt = now;
                    }
                }
            }
            db.SaveChanges();
            PetShopRepository.InvalidateProductCache();

            // 4. Seed FinancialSummary เริ่มต้น (หากยังไม่มี)
            if (!db.FinancialSummaries.Any())
            {
                db.FinancialSummaries.Add(new FinancialSummary
                {
                    TotalRevenue = 0m,
                    TotalExpenses = 0m,
                    NetProfit = 0m,
                    TotalOrdersCount = 0,
                    SuccessfulOrdersCount = 0,
                    RefundedOrdersCount = 0,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                db.SaveChanges();
            }

            // 5. Seed AuditLog ประวัติกิจกรรมเริ่มต้น (หากยังไม่มี)
            if (!db.AuditLogs.Any())
            {
                var baseTime = DateTime.Now.AddDays(-2);
                db.AuditLogs.AddRange(new List<AuditLog>
                {
                    new AuditLog { Username = "system", UserRole = "System", Action = "SYSTEM_INIT", TableName = "system", RecordId = "INIT", Details = "ระบบ PetStore Pro เริ่มต้นการทำงานและตรวจสอบฐานข้อมูล MySQL", IpAddress = "127.0.0.1", Timestamp = baseTime },
                    new AuditLog { Username = "admin", UserRole = "Admin", Action = "CREATE_USER", TableName = "user", RecordId = "2", Details = "สร้างบัญชีผู้ใช้งานพนักงาน 'inventory' บทบาท InventoryStaff", IpAddress = "192.168.1.10", Timestamp = baseTime.AddHours(2) },
                    new AuditLog { Username = "inventory", UserRole = "InventoryStaff", Action = "UPDATE_STOCK", TableName = "product", RecordId = "1", Details = "ตรวจนับและอัปเดตสต็อกสินค้า 'Royal Canin Hair & Skin' เป็น 25 ชิ้น", IpAddress = "192.168.1.15", Timestamp = baseTime.AddHours(5) },
                    new AuditLog { Username = "marketing", UserRole = "MarketingStaff", Action = "TOGGLE_PROMOTION", TableName = "promotion", RecordId = "1", Details = "เปิดใช้งานแคมเปญโปรโมชั่น 'BUY3SAVE' ซื้อครบ 3 ชิ้นลด 50 บาท", IpAddress = "192.168.1.18", Timestamp = baseTime.AddHours(8) },
                    new AuditLog { Username = "customer", UserRole = "Customer", Action = "CHECKOUT_ORDER", TableName = "orders", RecordId = "ORD-20261121-001", Details = "สร้างคำสั่งซื้อใหม่ ยอดชำระสุทธิ ฿1,250.00 ขนส่ง Flash Express", IpAddress = "171.96.120.45", Timestamp = baseTime.AddDays(1).AddHours(1) },
                    new AuditLog { Username = "accounting", UserRole = "AccountingStaff", Action = "APPROVE_PAYMENT", TableName = "orders", RecordId = "ORD-20261121-001", Details = "ตรวจสอบสลิปโอนเงิน PromptPay ถูกต้อง อนุมัติสถานะเป็น PaymentVerified", IpAddress = "192.168.1.20", Timestamp = baseTime.AddDays(1).AddHours(2) },
                    new AuditLog { Username = "order", UserRole = "OrderStaff", Action = "UPDATE_ORDER_STATUS", TableName = "orders", RecordId = "ORD-20261121-001", Details = "จัดเตรียมและแพ็กสินค้าเสร็จสิ้น เปลี่ยนสถานะเป็น Preparing", IpAddress = "192.168.1.22", Timestamp = baseTime.AddDays(1).AddHours(3) },
                    new AuditLog { Username = "shipping", UserRole = "ShippingStaff", Action = "DISPATCH_ORDER", TableName = "orders", RecordId = "ORD-20261121-001", Details = "ชั่งน้ำหนักจริง 2.40 kg เจนเลขพัสดุ TH0192837465 และเปลี่ยนสถานะเป็น Shipped", IpAddress = "192.168.1.25", Timestamp = baseTime.AddDays(1).AddHours(4) },
                    new AuditLog { Username = "support", UserRole = "ITSupport", Action = "RESOLVE_TICKET", TableName = "supportticket", RecordId = "TCK-20261122-001", Details = "ตรวจสอบและให้คำแนะนำการชำระเงินเรียบร้อย เปลี่ยนสถานะตั๋วเป็น Resolved", IpAddress = "192.168.1.30", Timestamp = baseTime.AddDays(1).AddHours(6) },
                    new AuditLog { Username = "admin", UserRole = "Admin", Action = "UPDATE_USER_ROLE", TableName = "user", RecordId = "9", Details = "ตรวจสอบบัญชีผู้ใช้และยืนยันสิทธิ์ Role Customer สถานะปกติ", IpAddress = "192.168.1.10", Timestamp = DateTime.Now.AddHours(-1) }
                });
                db.SaveChanges();
            }
        }
    }
}


