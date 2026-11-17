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
                throw new InvalidOperationException($"ไม่สามารถเชื่อมต่อฐานข้อมูล MySQL ได้: {ex.Message}", ex);
            }

            // สร้างโครงสร้างตารางทั้งหมดอัตโนมัติหากยังไม่มีในฐานข้อมูล (สำหรับ Cloud DB ใหม่)
            db.Database.EnsureCreated();

            // 1. Seed บัญชีผู้ใช้งานระบบ 9 บทบาท (เพิ่มเฉพาะบัญชีที่ยังไม่มี เพื่อรักษาข้อมูลที่มีอยู่เดิมในตาราง user)
            var now = DateTime.Now;
            var seedUsers = new List<SystemUser>
            {
                new SystemUser { Username = "owner", Password = "123456", FullName = "คุณภูไพศาล (Owner)", Email = "owner@petshop.com", Phone = "081-999-0001", Role = UserRole.Owner, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "admin", Password = "123456", FullName = "ธนพล (Admin)", Email = "admin@petshop.com", Phone = "081-999-0002", Role = UserRole.Admin, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "marketing", Password = "123456", FullName = "อลิสา (Marketing Staff)", Email = "marketing@petshop.com", Phone = "081-999-0003", Role = UserRole.MarketingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "inventory", Password = "123456", FullName = "สมชาย (Inventory Staff)", Email = "inventory@petshop.com", Phone = "081-999-0004", Role = UserRole.InventoryStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "order", Password = "123456", FullName = "วิภา (Order Staff)", Email = "order@petshop.com", Phone = "081-999-0005", Role = UserRole.OrderStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "shipping", Password = "123456", FullName = "เดชา (Shipping Staff)", Email = "shipping@petshop.com", Phone = "081-999-0006", Role = UserRole.ShippingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "accounting", Password = "123456", FullName = "อรวรรณ (Accounting Staff)", Email = "accounting@petshop.com", Phone = "081-999-0007", Role = UserRole.AccountingStaff, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "support", Password = "123456", FullName = "สมศักดิ์ (IT Support)", Email = "support@petshop.com", Phone = "081-999-0008", Role = UserRole.ITSupport, IsActive = true, CreatedAt = now, UpdatedAt = now },
                new SystemUser { Username = "customer", Password = "123456", FullName = "คุณกิตติชัย (Customer)", Email = "customer@example.com", Phone = "081-234-5678", Role = UserRole.Customer, IsActive = true, CreatedAt = now, UpdatedAt = now }
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

            // 3. Seed สินค้าเริ่มต้นและเพิ่มเติมให้ครอบคลุมทุกหมวดหมู่
            var existingProductNames = db.Products.Select(p => p.Name.ToLower()).ToHashSet();

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
                    CartAddCount = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1589924691995-400dc9ecc119?w=600&q=80",
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
                    CartAddCount = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=600&q=80",
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
                    CartAddCount = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1534361960057-19889db98a1e?w=600&q=80",
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
                    CartAddCount = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=600&q=80",
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
                    CartAddCount = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=600&q=80",
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
                    CartAddCount = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1568640347023-a616a30bc3bd?w=600&q=80",
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
                    CartAddCount = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1546182990-dffeafbe841d?w=600&q=80",
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
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 25,
                    LowStockThreshold = 5,
                    CartAddCount = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1601758228041-f3b2795255f1?w=600&q=80",
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
                    CartAddCount = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1541599540903-216a46ca1dc0?w=600&q=80",
                    Description = "เบาะนอนเพื่อสุขภาพช่วยกระจายแรงกดทับ สำหรับสุนัขโตและสุนัขสูงอายุที่มีปัญหาข้อสะโพก",
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
                    CartAddCount = 9,
                    ImageUrl = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=600&q=80",
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
                    CartAddCount = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1583511655826-05700d52f4d9?w=600&q=80",
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
                    CartAddCount = 7,
                    ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&q=80",
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
                    CartAddCount = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1518791841217-8f162f1e1131?w=600&q=80",
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
                    CartAddCount = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=600&q=80",
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
                    CartAddCount = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1495360010541-f48722b34f7d?w=600&q=80",
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
                    CartAddCount = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?w=600&q=80",
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
                    CartAddCount = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1526336024174-e58f5cdd8e13?w=600&q=80",
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
                    CartAddCount = 2,
                    ImageUrl = "https://images.unsplash.com/photo-1545249390-6bdfa286032f?w=600&q=80",
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
                    SpecialNeed = "ทั่วไป",
                    StockQuantity = 40,
                    LowStockThreshold = 10,
                    CartAddCount = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1561948955-570b270e7c36?w=600&q=80",
                    Description = "ก้านจับยืดหยุ่น ขนนกสีสันสดใส กระตุ้นสัญชาตญาณนักล่าและการออกกำลังกายของแมว",
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
                    CartAddCount = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1574158622682-e40e69881006?w=600&q=80",
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
                    CartAddCount = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1592194996308-7b43878e84a6?w=600&q=80",
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
                    CartAddCount = 3,
                    ImageUrl = "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=600&q=80",
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
                    CartAddCount = 5,
                    ImageUrl = "https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=600&q=80",
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
                    CartAddCount = 1,
                    ImageUrl = "https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=600&q=80",
                    Description = "กรงเหล็กพ่นกันสนิมอย่างดี มีบันไดและชั้นลอย ถาดรองด้านล่างดึงล้างทำความสะอาดง่าย สะดวกสบาย",
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
                    CartAddCount = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1535268647677-300dbf3d78d1?w=600&q=80",
                    Description = "วัสดุรองกรงทำจากเยื่อกระดาษบริสุทธิ์ ไร้ฝุ่น 99.9% ซึมซับดีกว่าขี้เลื่อยทั่วไป 2 เท่า นุ่มฟูปลอดภัยต่อระบบหายใจ",
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            foreach (var item in catalog)
            {
                if (!existingProductNames.Contains(item.Name.ToLower()))
                {
                    db.Products.Add(item);
                }
            }
            db.SaveChanges();

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
        }
    }
}

