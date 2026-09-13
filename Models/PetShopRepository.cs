using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CSI402_Project_final.Models
{
    public class PetShopDatabase
    {
        public List<Product> Products { get; set; } = new();
        public List<Promotion> Promotions { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<SupportTicket> SupportTickets { get; set; } = new();
        public List<SystemUser> Users { get; set; } = new();
    }

    public class PetShopRepository
    {
        private readonly string _dbFilePath;
        private readonly object _lock = new();

        public List<Product> Products { get; set; } = new();
        public List<Promotion> Promotions { get; set; } = new();
        public List<Order> Orders { get; set; } = new();
        public List<SupportTicket> SupportTickets { get; set; } = new();
        public List<SystemUser> Users { get; set; } = new();

        public Cart CurrentCart { get; set; } = new();
        public UserRole CurrentSimulatedRole { get; set; } = UserRole.Guest;

        public PetShopRepository()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _dbFilePath = Path.Combine(dataDir, "petshop_database.json");

            LoadDatabase();
        }

        // โหลดข้อมูลจากไฟล์จริง (ไม่มี Mock Data ฮาร์ดโค้ดในโค้ด)
        private void LoadDatabase()
        {
            lock (_lock)
            {
                if (File.Exists(_dbFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_dbFilePath);
                        var data = JsonSerializer.Deserialize<PetShopDatabase>(json);
                        if (data != null)
                        {
                            Products = data.Products ?? new();
                            Promotions = data.Promotions ?? new();
                            Orders = data.Orders ?? new();
                            SupportTickets = data.SupportTickets ?? new();
                            Users = data.Users ?? new();
                            return;
                        }
                    }
                    catch
                    {
                        // หากไฟล์เสียหาย จะสร้างโครงสร้างตั้งต้น
                    }
                }

                // สร้างโครงสร้างระบบตั้งต้น พร้อมบัญชีผู้ใช้สำหรับ 10 Role
                InitializeCleanDatabase();
                SaveDatabase();
            }
        }

        public void SaveDatabase()
        {
            lock (_lock)
            {
                try
                {
                    var data = new PetShopDatabase
                    {
                        Products = this.Products,
                        Promotions = this.Promotions,
                        Orders = this.Orders,
                        SupportTickets = this.SupportTickets,
                        Users = this.Users
                    };
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_dbFilePath, json);
                }
                catch
                {
                    // Logging error if needed
                }
            }
        }

        private void InitializeCleanDatabase()
        {
            // บัญชีผู้ใช้งานระบบสำหรับแต่ละแผนก (ไม่มี mock ขยะ เป็น system accounts)
            Users = new List<SystemUser>
            {
                new SystemUser { Id = 1, Username = "owner", Password = "123456", FullName = "คุณภูไพศาล (Owner)", Email = "owner@petshop.com", Phone = "081-999-0001", Role = UserRole.Owner },
                new SystemUser { Id = 2, Username = "admin", Password = "123456", FullName = "ธนพล (Admin)", Email = "admin@petshop.com", Phone = "081-999-0002", Role = UserRole.Admin },
                new SystemUser { Id = 3, Username = "marketing", Password = "123456", FullName = "อลิสา (Marketing Staff)", Email = "marketing@petshop.com", Phone = "081-999-0003", Role = UserRole.MarketingStaff },
                new SystemUser { Id = 4, Username = "inventory", Password = "123456", FullName = "สมชาย (Inventory Staff)", Email = "inventory@petshop.com", Phone = "081-999-0004", Role = UserRole.InventoryStaff },
                new SystemUser { Id = 5, Username = "order", Password = "123456", FullName = "วิภา (Order Staff)", Email = "order@petshop.com", Phone = "081-999-0005", Role = UserRole.OrderStaff },
                new SystemUser { Id = 6, Username = "shipping", Password = "123456", FullName = "เดชา (Shipping Staff)", Email = "shipping@petshop.com", Phone = "081-999-0006", Role = UserRole.ShippingStaff },
                new SystemUser { Id = 7, Username = "accounting", Password = "123456", FullName = "อรวรรณ (Accounting Staff)", Email = "accounting@petshop.com", Phone = "081-999-0007", Role = UserRole.AccountingStaff },
                new SystemUser { Id = 8, Username = "support", Password = "123456", FullName = "สมศักดิ์ (IT Support)", Email = "support@petshop.com", Phone = "081-999-0008", Role = UserRole.ITSupport },
                new SystemUser { Id = 9, Username = "customer", Password = "123456", FullName = "คุณกิตติชัย (Customer)", Email = "customer@example.com", Phone = "081-234-5678", Role = UserRole.Customer }
            };

            // โครงสร้างโปรโมชั่น 5 ข้อตามข้อกำหนดระบบ (สร้างไว้เพื่อให้ระบบ Promotion Engine ทำงานได้)
            Promotions = new List<Promotion>
            {
                new Promotion
                {
                    Id = 1,
                    Code = "BUY3SAVE",
                    Title = "โปรโมชั่น 1: ซื้อครบ 3 ชิ้น ลดทันที 50 บาท",
                    Description = "ซื้อสินค้าใดก็ได้ครบ 3 ชิ้น รับส่วนลดทันที 50 บาท",
                    Type = PromotionType.Buy3ItemsDiscount,
                    MinQuantity = 3,
                    DiscountAmount = 50m,
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true,
                    UsageCount = 0
                },
                new Promotion
                {
                    Id = 2,
                    Code = "FOOD2VARIETY",
                    Title = "โปรโมชั่น 2: ซื้ออาหารอย่างน้อย 2 รายการ ลดพิเศษ 10%",
                    Description = "ซื้ออาหารสัตว์เลี้ยงในหมวดอาหารตั้งแต่ 2 รายการขึ้นไป ลด 10%",
                    Type = PromotionType.FoodCategoryDiscount,
                    TargetCategory = ProductCategory.Food,
                    MinQuantity = 2,
                    DiscountPercent = 10m,
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true,
                    UsageCount = 0
                },
                new Promotion
                {
                    Id = 3,
                    Code = "FREESHIP10KG",
                    Title = "โปรโมชั่น 3: จัดส่งฟรีเมื่อยอดถึง 800 บาท (น้ำหนักไม่เกิน 10 kg)",
                    Description = "ยอดซื้อครบ 800 บาทขึ้นไป จัดส่งฟรีหากน้ำหนักรวมไม่เกิน 10 kg",
                    Type = PromotionType.FreeShippingWeightCap,
                    MinSpend = 800m,
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true,
                    UsageCount = 0
                },
                new Promotion
                {
                    Id = 4,
                    Code = "FREEGIFT1000",
                    Title = "โปรโมชั่น 4: ช้อปครบ 1,000 บาท รับฟรีเสื้อสัตว์เลี้ยงสุดน่ารัก",
                    Description = "ยอดซื้อครบ 1,000 บาท รับของแถมฟรี เสื้อยืดสัตว์เลี้ยง Pet Lover",
                    Type = PromotionType.FreeGiftMilestone,
                    MinSpend = 1000m,
                    GiftItemName = "เสื้อยืดสัตว์เลี้ยง Pet Lover ลิมิเต็ด 🎽",
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true,
                    UsageCount = 0
                },
                new Promotion
                {
                    Id = 5,
                    Code = "SAMEBRAND15",
                    Title = "โปรโมชั่น 5: ซื้ออาหารแบรนด์เดียวกันตั้งแต่ 2 ชิ้น ลด 15%",
                    Description = "ซื้ออาหารจากแบรนด์เดียวกัน 2 ชิ้นขึ้นไป ลดเพิ่ม 15%",
                    Type = PromotionType.SameBrandFoodDiscount,
                    DiscountPercent = 15m,
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(3),
                    IsActive = true,
                    UsageCount = 0
                }
            };

            // รายการสินค้าจริงเริ่มต้นที่ผู้ดูแลระบบกำหนด (สามารถเพิ่ม/ลบ/แก้ไขได้จริงผ่านหน้า Inventory)
            Products = new List<Product>
            {
                new Product
                {
                    Id = 1,
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
                    ImageUrl = "https://images.unsplash.com/photo-1589924691995-400dc9ecc119?w=500&q=80",
                    Description = "อาหารเม็ดสำหรับสุนัขโตพันธุ์กลาง ช่วยเสริมสร้างภูมิคุ้มกันและดูแลทางเดินอาหาร"
                },
                new Product
                {
                    Id = 2,
                    Name = "Royal Canin Puppy Maxi (อาหารลูกสุนัข)",
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
                    ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500&q=80",
                    Description = "สำหรับลูกสุนัขพันธุ์ใหญ่ อายุ 2-15 เดือน โปรตีนคุณภาพสูง"
                },
                new Product
                {
                    Id = 3,
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
                    ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=500&q=80",
                    Description = "อาหารลูกแมวอุดมด้วย DHA จากน้ำมันปลาทะเล และพรีไบโอติก"
                },
                new Product
                {
                    Id = 4,
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
                    ImageUrl = "https://images.unsplash.com/photo-1574158622682-e40e69881006?w=500&q=80",
                    Description = "จับตัวเป็นก้อนเร็ว ละลายน้ำได้ ทิ้งลงชักโครกได้ ไร้ฝุ่น 99%"
                }
            };

            // คำสั่งซื้อเริ่มต้น: สะอาด ว่างเปล่า (ไม่มี mock orders)
            Orders = new List<Order>();

            // ตั๋วแจ้งปัญหาเริ่มต้น: สะอาด ว่างเปล่า (ไม่มี mock tickets)
            SupportTickets = new List<SupportTicket>();
        }

        // ==========================================
        // Business Methods จัดการฐานข้อมูลจริง
        // ==========================================

        public SystemUser? ValidateUser(string username, string password)
        {
            return Users.FirstOrDefault(u => 
                (u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) || 
                 u.Email.Equals(username, StringComparison.OrdinalIgnoreCase)) && 
                u.Password == password && 
                u.IsActive);
        }

        public SystemUser? GetUserByUsername(string username)
        {
            return Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateUserProfile(string username, string fullName, string email, string phone)
        {
            var user = GetUserByUsername(username);
            if (user != null)
            {
                user.FullName = fullName.Trim();
                user.Email = email.Trim();
                user.Phone = phone.Trim();
                SaveDatabase();
            }
        }

        public void AddOrUpdateAddress(string username, UserAddress address)
        {
            var user = GetUserByUsername(username);
            if (user == null) return;

            if (address.IsDefault)
            {
                foreach (var a in user.Addresses) a.IsDefault = false;
            }

            if (address.Id == 0)
            {
                address.Id = user.Addresses.Any() ? user.Addresses.Max(a => a.Id) + 1 : 1;
                if (!user.Addresses.Any()) address.IsDefault = true;
                user.Addresses.Add(address);
            }
            else
            {
                var existing = user.Addresses.FirstOrDefault(a => a.Id == address.Id);
                if (existing != null)
                {
                    existing.Title = address.Title;
                    existing.RecipientName = address.RecipientName;
                    existing.Phone = address.Phone;
                    existing.FullAddress = address.FullAddress;
                    existing.Subdistrict = address.Subdistrict;
                    existing.District = address.District;
                    existing.Province = address.Province;
                    existing.PostalCode = address.PostalCode;
                    existing.IsDefault = address.IsDefault;
                }
            }
            SaveDatabase();
        }

        public void DeleteAddress(string username, int addressId)
        {
            var user = GetUserByUsername(username);
            if (user == null) return;

            var target = user.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (target != null)
            {
                user.Addresses.Remove(target);
                if (target.IsDefault && user.Addresses.Any())
                {
                    user.Addresses[0].IsDefault = true;
                }
                SaveDatabase();
            }
        }

        public void SetDefaultAddress(string username, int addressId)
        {
            var user = GetUserByUsername(username);
            if (user == null) return;

            foreach (var a in user.Addresses)
            {
                a.IsDefault = (a.Id == addressId);
            }
            SaveDatabase();
        }

        public SystemUser? RegisterCustomer(RegisterViewModel model)
        {
            if (Users.Any(u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) || 
                               u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var newUser = new SystemUser
            {
                Id = Users.Any() ? Users.Max(u => u.Id) + 1 : 1,
                Username = model.Username.Trim(),
                Password = model.Password,
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim(),
                Phone = model.Phone.Trim(),
                Role = UserRole.Customer,
                IsActive = true
            };

            Users.Add(newUser);
            SaveDatabase();
            return newUser;
        }

        public int AddToCart(int productId, int qty = 1)
        {
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return 0;

            var existing = CurrentCart.Items.FirstOrDefault(i => i.Product.Id == productId);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                CurrentCart.Items.Add(new CartItem { Product = product, Quantity = qty });
            }
            product.CartAddCount++;
            CurrentCart.Recalculate(Promotions);
            SaveDatabase();
            return product.CartAddCount;
        }

        public void UpdateCartItem(int productId, int qty)
        {
            var item = CurrentCart.Items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                if (qty <= 0) CurrentCart.Items.Remove(item);
                else item.Quantity = qty;
            }
            CurrentCart.Recalculate(Promotions);
        }

        public void RemoveFromCart(int productId)
        {
            var item = CurrentCart.Items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                CurrentCart.Items.Remove(item);
            }
            CurrentCart.Recalculate(Promotions);
        }

        public Order CheckoutCurrentCart(string name, string phone, string address)
        {
            CurrentCart.Recalculate(Promotions);
            var newOrder = new Order
            {
                OrderId = $"ORD-{DateTime.Now:yyyyMMdd}-{Orders.Count + 1:D3}",
                CustomerName = string.IsNullOrWhiteSpace(name) ? "ลูกค้าทั่วไป (Customer)" : name,
                CustomerPhone = string.IsNullOrWhiteSpace(phone) ? "081-000-0000" : phone,
                ShippingAddress = string.IsNullOrWhiteSpace(address) ? "กรุงเทพมหานคร 10100" : address,
                OrderDate = DateTime.Now,
                EstimatedWeightKg = CurrentCart.TotalWeightKg,
                ItemsTotal = CurrentCart.ProductPriceBeforePromo,
                DiscountTotal = CurrentCart.TotalPromotionDiscount,
                ShippingFee = CurrentCart.ShippingFee,
                GrandTotal = CurrentCart.GrandTotal,
                FreeGiftItem = CurrentCart.FreeGiftItem,
                Status = OrderStatus.PaymentVerified,
                Items = CurrentCart.Items.Select(i => new OrderItem
                {
                    ProductId = i.Product.Id,
                    ProductName = i.Product.Name,
                    UnitPrice = i.Product.EffectivePrice,
                    Quantity = i.Quantity,
                    UnitWeightKg = i.Product.WeightKg
                }).ToList()
            };

            foreach (var applied in CurrentCart.AppliedPromotions)
            {
                var promo = Promotions.FirstOrDefault(p => p.Title == applied.PromotionTitle);
                promo?.IncrementUsage();
            }

            foreach (var item in CurrentCart.Items)
            {
                var prod = Products.FirstOrDefault(p => p.Id == item.Product.Id);
                if (prod != null)
                {
                    prod.StockQuantity = Math.Max(0, prod.StockQuantity - item.Quantity);
                }
            }

            Orders.Insert(0, newOrder);
            CurrentCart.Items.Clear();
            CurrentCart.Recalculate(Promotions);

            SaveDatabase();
            return newOrder;
        }

        public FinancialSummary GetFinancialSummary()
        {
            decimal totalRev = Orders.Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Refunded).Sum(o => o.GrandTotal);
            decimal totalRefund = Orders.Where(o => o.Status == OrderStatus.Refunded).Sum(o => o.RefundAmount ?? 0);
            decimal totalShipIncome = Orders.Sum(o => o.ShippingFee);
            decimal costEst = totalRev * 0.55m;

            return new FinancialSummary
            {
                TotalRevenue = totalRev,
                TotalExpenses = costEst + (totalRev > 0 ? 5000m : 0m),
                TotalRefunds = totalRefund,
                ShippingCollected = totalShipIncome,
                ShippingCarrierCost = totalShipIncome * 0.8m,
                MonthlyBreakdown = new List<MonthlyFinancialEntry>
                {
                    new MonthlyFinancialEntry { Month = "เดือนปัจจุบัน", Revenue = totalRev, Expenses = costEst }
                }
            };
        }
    }
}
