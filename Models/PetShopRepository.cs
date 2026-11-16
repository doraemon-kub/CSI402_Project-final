using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using CSI402_Project_final.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CSI402_Project_final.Models
{
    public class GuestCartEntry
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class PetShopRepository
    {
        private const string GuestCartSessionKey = "GuestCart";
        private readonly PetShopDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Cart? _cachedCart;

        public PetShopRepository(PetShopDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        // ==========================================
        // Queryable / Data Accessors (ดึงจาก MySQL csi402db)
        // ==========================================

        public List<Product> Products => _db.Products.AsNoTracking().ToList();
        public List<Promotion> Promotions => _db.Promotions.AsNoTracking().ToList();
        public List<Order> Orders => _db.Orders.Include(o => o.Items).AsNoTracking().OrderByDescending(o => o.OrderDate).ToList();
        public List<SystemUser> Users => _db.Users.Include(u => u.Addresses).AsNoTracking().ToList();

        public IQueryable<Product> QueryProducts() => _db.Products.AsNoTracking();
        public Product? GetProductById(int id) => _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);

        public List<Promotion> GetValidPromotions()
        {
            return _db.Promotions.AsNoTracking().ToList().Where(p => p.IsValidNow).ToList();
        }

        public UserRole CurrentSimulatedRole
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var roleStr = user.FindFirstValue(ClaimTypes.Role);
                    if (Enum.TryParse<UserRole>(roleStr, out var parsed))
                    {
                        return parsed;
                    }
                }
                return UserRole.Guest;
            }
        }

        private int? GetCurrentUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true) return null;

            var idClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idClaim, out int uid) && uid > 0)
            {
                return uid;
            }

            var username = principal.Identity.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                return _db.Users.Where(u => u.Username == username).Select(u => (int?)u.Id).FirstOrDefault();
            }

            return null;
        }

        // ตะกร้าสินค้าแยกตามผู้ใช้ (Authenticated -> MySQL `cart`, Guest -> HttpContext.Session)
        public Cart CurrentCart
        {
            get
            {
                if (_cachedCart != null) return _cachedCart;
                _cachedCart = LoadCurrentCart();
                return _cachedCart;
            }
        }

        private Cart LoadCurrentCart()
        {
            var activePromos = GetValidPromotions();
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                var dbCart = _db.Carts
                    .AsNoTracking()
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .Include(c => c.AppliedPromotions)
                    .FirstOrDefault(c => c.UserId == userId.Value);

                if (dbCart == null)
                {
                    return new Cart { UserId = userId.Value };
                }

                dbCart.Recalculate(activePromos);
                return dbCart;
            }

            // Guest Session Cart
            var guestEntries = ReadGuestCartFromSession();
            var guestCart = new Cart();
            if (guestEntries.Any())
            {
                var productIds = guestEntries.Select(g => g.ProductId).Distinct().ToList();
                var productsById = _db.Products.AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionary(p => p.Id);

                foreach (var entry in guestEntries)
                {
                    if (productsById.TryGetValue(entry.ProductId, out var prod) && entry.Quantity > 0)
                    {
                        guestCart.Items.Add(new CartItem
                        {
                            ProductId = prod.Id,
                            Product = prod,
                            Quantity = Math.Min(entry.Quantity, Math.Max(1, prod.StockQuantity))
                        });
                    }
                }
            }

            guestCart.Recalculate(activePromos);
            return guestCart;
        }

        private List<GuestCartEntry> ReadGuestCartFromSession()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new List<GuestCartEntry>();

            var json = session.GetString(GuestCartSessionKey);
            if (string.IsNullOrWhiteSpace(json)) return new List<GuestCartEntry>();

            try
            {
                return JsonSerializer.Deserialize<List<GuestCartEntry>>(json) ?? new List<GuestCartEntry>();
            }
            catch
            {
                return new List<GuestCartEntry>();
            }
        }

        private void WriteGuestCartToSession(List<GuestCartEntry> entries)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return;

            if (!entries.Any())
            {
                session.Remove(GuestCartSessionKey);
            }
            else
            {
                session.SetString(GuestCartSessionKey, JsonSerializer.Serialize(entries));
            }
        }

        public void MergeGuestCartToUser(int userId)
        {
            var guestEntries = ReadGuestCartFromSession();
            if (!guestEntries.Any()) return;

            var dbCart = GetOrCreateDbCart(userId);
            foreach (var entry in guestEntries)
            {
                var prod = _db.Products.FirstOrDefault(p => p.Id == entry.ProductId);
                if (prod == null || prod.StockQuantity <= 0) continue;

                var existing = dbCart.Items.FirstOrDefault(i => i.ProductId == entry.ProductId);
                if (existing != null)
                {
                    existing.Quantity = Math.Min(prod.StockQuantity, existing.Quantity + entry.Quantity);
                }
                else
                {
                    dbCart.Items.Add(new CartItem
                    {
                        CartId = dbCart.Id,
                        ProductId = prod.Id,
                        Product = prod,
                        Quantity = Math.Min(prod.StockQuantity, entry.Quantity)
                    });
                }
            }

            SyncCartPromotionsAndTotals(dbCart);
            _db.SaveChanges();
            WriteGuestCartToSession(new List<GuestCartEntry>());
            _cachedCart = null;
        }

        private Cart GetOrCreateDbCart(int userId)
        {
            var dbCart = _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .Include(c => c.AppliedPromotions)
                .FirstOrDefault(c => c.UserId == userId);

            if (dbCart == null)
            {
                dbCart = new Cart
                {
                    UserId = userId,
                    ShippingCalculationDetail = "ไม่มีสินค้าในตะกร้า"
                };
                _db.Carts.Add(dbCart);
                _db.SaveChanges();
            }

            return dbCart;
        }

        private void SyncCartPromotionsAndTotals(Cart dbCart)
        {
            // ลบแถว AppliedPromotions เดิมของตะกร้านี้ก่อนสร้างรายการคำนวณใหม่
            var existingPromos = dbCart.AppliedPromotions.ToList();
            if (existingPromos.Any())
            {
                _db.AppliedPromotionSummaries.RemoveRange(existingPromos);
                dbCart.AppliedPromotions.Clear();
            }

            var activePromos = GetValidPromotions();
            dbCart.Recalculate(activePromos);

            foreach (var ap in dbCart.AppliedPromotions)
            {
                ap.Id = 0;
                ap.CartId = dbCart.Id;
                ap.OrderId = null;
            }
        }

        // ==========================================
        // Audit Trail Helper (บันทึกลงตาราง `auditlog`)
        // ==========================================
        public void LogAudit(string action, string tableName, string? recordId, string? details, SystemUser? actorOverride = null)
        {
            try
            {
                int? uid = actorOverride?.Id ?? GetCurrentUserId();
                string uname = actorOverride?.Username
                    ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
                    ?? "guest";
                string urole = actorOverride?.Role.ToString()
                    ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role)
                    ?? "Guest";
                string? ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                _db.AuditLogs.Add(new AuditLog
                {
                    UserId = uid,
                    Username = uname.Length > 50 ? uname[..50] : uname,
                    UserRole = urole.Length > 50 ? urole[..50] : urole,
                    Action = action.Length > 100 ? action[..100] : action,
                    TableName = tableName.Length > 50 ? tableName[..50] : tableName,
                    RecordId = recordId != null && recordId.Length > 50 ? recordId[..50] : recordId,
                    Details = details,
                    IpAddress = ip,
                    Timestamp = DateTime.Now
                });
                _db.SaveChanges();
            }
            catch
            {
                // ไม่ให้การบันทึก Log มาขัดขวางการทำงานหลักหากมีปัญหา
            }
        }

        // ==========================================
        // User & Profile & Address Methods (`user`, `useraddress`)
        // ==========================================

        public SystemUser? ValidateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return null;
            string trimmed = username.Trim();

            var user = _db.Users
                .Include(u => u.Addresses)
                .FirstOrDefault(u => (u.Username == trimmed || u.Email == trimmed) && u.IsActive);

            if (user == null || user.Password != password) return null;
            return user;
        }

        public SystemUser? GetUserByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return _db.Users
                .Include(u => u.Addresses)
                .FirstOrDefault(u => u.Username == username);
        }

        public void UpdateUserProfile(string username, string fullName, string email, string phone)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return;

            user.FullName = fullName.Trim();
            user.Email = email.Trim();
            user.Phone = phone.Trim();
            user.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("UPDATE_PROFILE", "user", user.Id.ToString(), $"อัปเดตโปรไฟล์ของ {user.Username}");
        }

        public void AddOrUpdateAddress(string username, UserAddress address)
        {
            var user = _db.Users.Include(u => u.Addresses).FirstOrDefault(u => u.Username == username);
            if (user == null) return;

            var now = DateTime.Now;
            if (address.IsDefault)
            {
                foreach (var a in user.Addresses)
                {
                    a.IsDefault = false;
                    a.UpdatedAt = now;
                }
            }

            if (address.Id == 0)
            {
                if (!user.Addresses.Any()) address.IsDefault = true;
                address.UserId = user.Id;
                address.CreatedAt = now;
                address.UpdatedAt = now;
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
                    existing.UpdatedAt = now;
                }
            }

            _db.SaveChanges();
            LogAudit("SAVE_ADDRESS", "useraddress", address.Id.ToString(), $"บันทึกที่อยู่ '{address.Title}' ของ {username}");
        }

        public void DeleteAddress(string username, int addressId)
        {
            var user = _db.Users.Include(u => u.Addresses).FirstOrDefault(u => u.Username == username);
            if (user == null) return;

            var target = user.Addresses.FirstOrDefault(a => a.Id == addressId);
            if (target == null) return;

            bool wasDefault = target.IsDefault;
            _db.UserAddresses.Remove(target);
            user.Addresses.Remove(target);

            if (wasDefault && user.Addresses.Any())
            {
                user.Addresses[0].IsDefault = true;
                user.Addresses[0].UpdatedAt = DateTime.Now;
            }

            _db.SaveChanges();
            LogAudit("DELETE_ADDRESS", "useraddress", addressId.ToString(), $"ลบที่อยู่ ID {addressId} ของ {username}");
        }

        public void SetDefaultAddress(string username, int addressId)
        {
            var user = _db.Users.Include(u => u.Addresses).FirstOrDefault(u => u.Username == username);
            if (user == null) return;

            var now = DateTime.Now;
            foreach (var a in user.Addresses)
            {
                a.IsDefault = (a.Id == addressId);
                a.UpdatedAt = now;
            }

            _db.SaveChanges();
            LogAudit("SET_DEFAULT_ADDRESS", "useraddress", addressId.ToString(), $"ตั้งที่อยู่หลัก ID {addressId}");
        }

        public SystemUser? RegisterCustomer(RegisterViewModel model)
        {
            string username = model.Username.Trim();
            string email = model.Email.Trim();

            bool exists = _db.Users.Any(u => u.Username == username || u.Email == email);
            if (exists) return null;

            var now = DateTime.Now;
            var newUser = new SystemUser
            {
                Username = username,
                Password = model.Password,
                FullName = model.FullName.Trim(),
                Email = email,
                Phone = model.Phone.Trim(),
                Role = UserRole.Customer,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            LogAudit("REGISTER", "user", newUser.Id.ToString(), $"สมัครสมาชิกใหม่ {newUser.Username}", newUser);
            return newUser;
        }

        public bool UpdateUserRole(int userId, UserRole newRole, bool isActive)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return false;

            var oldRole = user.Role;
            user.Role = newRole;
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("UPDATE_USER_ROLE", "user", user.Id.ToString(), $"เปลี่ยนสิทธิ์ {user.Username} จาก {oldRole} เป็น {newRole} (Active={isActive})");
            return true;
        }

        public (bool Success, string Message) CreateUser(SystemUser user)
        {
            string uname = user.Username.Trim();
            string email = user.Email.Trim();
            if (_db.Users.Any(u => u.Username == uname || u.Email == email))
            {
                return (false, "ชื่อผู้ใช้หรืออีเมลนี้มีอยู่ในระบบแล้ว");
            }

            var now = DateTime.Now;
            user.Id = 0;
            user.Username = uname;
            user.Email = email;
            user.FullName = user.FullName.Trim();
            user.Phone = user.Phone?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(user.Password)) user.Password = "123456";
            user.CreatedAt = now;
            user.UpdatedAt = now;

            _db.Users.Add(user);
            _db.SaveChanges();

            LogAudit("CREATE_USER", "user", user.Id.ToString(), $"สร้างบัญชีผู้ใช้ {user.Username} ({user.Role})");
            return (true, $"เพิ่มผู้ใช้งาน '{user.FullName}' ในระบบเรียบร้อยแล้ว");
        }

        // ==========================================
        // Cart & Checkout Methods (`cart`, `cartitem`, `orders`, `orderitem`)
        // ==========================================

        public int AddToCart(int productId, int qty = 1)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                // ไม่อนุญาตให้ผู้เยี่ยมชม (Guest) เพิ่มสินค้าลงในตะกร้า
                var p = _db.Products.FirstOrDefault(x => x.Id == productId);
                return p?.CartAddCount ?? 0;
            }

            if (qty < 1) qty = 1;
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null || product.StockQuantity <= 0) return product?.CartAddCount ?? 0;

            // ตรวจสอบจำนวนที่จะสามารถหักออกจากสต็อกได้จริง
            int actualQty = Math.Min(product.StockQuantity, qty);
            if (actualQty <= 0) return product.CartAddCount;

            // หักสต็อกสินค้าทันทีเมื่อใส่ตะกร้าตามที่ผู้ใช้กำหนด
            product.StockQuantity -= actualQty;
            product.CartAddCount++;
            product.UpdatedAt = DateTime.Now;

            var dbCart = GetOrCreateDbCart(userId.Value);
            var existing = dbCart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += actualQty;
            }
            else
            {
                dbCart.Items.Add(new CartItem
                {
                    CartId = dbCart.Id,
                    ProductId = productId,
                    Product = product,
                    Quantity = actualQty
                });
            }

            SyncCartPromotionsAndTotals(dbCart);
            _db.SaveChanges();

            _cachedCart = null;
            return product.CartAddCount;
        }

        public void UpdateCartItem(int productId, int qty)
        {
            int newQty = Math.Max(0, qty);
            var product = _db.Products.FirstOrDefault(p => p.Id == productId);
            var userId = GetCurrentUserId();

            if (userId.HasValue)
            {
                var dbCart = GetOrCreateDbCart(userId.Value);
                var item = dbCart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    int oldQty = item.Quantity;
                    int diff = newQty - oldQty;

                    if (diff > 0)
                    {
                        // ต้องการเพิ่มจำนวนในตะกร้า -> หักสต็อกเพิ่ม
                        if (product != null)
                        {
                            int canDeduct = Math.Min(product.StockQuantity, diff);
                            product.StockQuantity -= canDeduct;
                            product.UpdatedAt = DateTime.Now;
                            item.Quantity = oldQty + canDeduct;
                        }
                    }
                    else if (diff < 0)
                    {
                        // ลดจำนวนหรือลบออกจากตะกร้า -> คืนสต็อกกลับเข้าสินค้า
                        int returnStock = -diff;
                        if (product != null)
                        {
                            product.StockQuantity += returnStock;
                            product.UpdatedAt = DateTime.Now;
                        }

                        if (newQty == 0)
                        {
                            _db.CartItems.Remove(item);
                            dbCart.Items.Remove(item);
                        }
                        else
                        {
                            item.Quantity = newQty;
                        }
                    }

                    SyncCartPromotionsAndTotals(dbCart);
                    _db.SaveChanges();
                }
            }
            else
            {
                var entries = ReadGuestCartFromSession();
                var existing = entries.FirstOrDefault(e => e.ProductId == productId);
                if (existing != null)
                {
                    int oldQty = existing.Quantity;
                    int diff = newQty - oldQty;

                    if (diff > 0)
                    {
                        if (product != null)
                        {
                            int canDeduct = Math.Min(product.StockQuantity, diff);
                            product.StockQuantity -= canDeduct;
                            product.UpdatedAt = DateTime.Now;
                            existing.Quantity = oldQty + canDeduct;
                            _db.SaveChanges();
                        }
                    }
                    else if (diff < 0)
                    {
                        int returnStock = -diff;
                        if (product != null)
                        {
                            product.StockQuantity += returnStock;
                            product.UpdatedAt = DateTime.Now;
                            _db.SaveChanges();
                        }

                        if (newQty == 0)
                        {
                            entries.Remove(existing);
                        }
                        else
                        {
                            existing.Quantity = newQty;
                        }
                    }
                    WriteGuestCartToSession(entries);
                }
            }

            _cachedCart = null;
        }

        public void RemoveFromCart(int productId)
        {
            UpdateCartItem(productId, 0);
        }

        public (Order? Order, string? ErrorMessage) CheckoutCurrentCart(
            string username,
            string name,
            string phone,
            string address,
            string paymentMethod = "โอนผ่านธนาคาร / PromptPay",
            string? paymentSlipUrl = null)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return (null, "กรุณาเข้าสู่ระบบก่อนทำการสั่งซื้อ");
            }

            var dbCart = _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .Include(c => c.AppliedPromotions)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (dbCart == null || !dbCart.Items.Any())
            {
                return (null, "ไม่มีสินค้าในตะกร้า");
            }

            using var tx = _db.Database.BeginTransaction();
            try
            {
                var trackedOldPromos = dbCart.AppliedPromotions.ToList();
                var trackedOldItems = dbCart.Items.ToList();

                var activePromos = GetValidPromotions();
                dbCart.Recalculate(activePromos);
                var computedPromos = dbCart.AppliedPromotions.ToList();

                // ตรวจสอบความถูกต้องของสินค้าในตะกร้า
                foreach (var item in trackedOldItems)
                {
                    var prod = _db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (prod == null)
                    {
                        return (null, $"ไม่พบสินค้า #{item.ProductId} ในระบบ");
                    }
                }

                string datePrefix = $"ORD-{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}-";
                int todaySeq = _db.Orders.Count(o => o.OrderId.StartsWith(datePrefix)) + 1;
                string newOrderId = $"{datePrefix}{todaySeq:D3}";
                while (_db.Orders.Any(o => o.OrderId == newOrderId))
                {
                    todaySeq++;
                    newOrderId = $"{datePrefix}{todaySeq:D3}";
                }

                string cleanMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "โอนผ่านธนาคาร / PromptPay" : paymentMethod.Trim();
                bool isCod = cleanMethod.Contains("COD", StringComparison.OrdinalIgnoreCase);

                var now = DateTime.Now;
                var newOrder = new Order
                {
                    OrderId = newOrderId,
                    UserId = user.Id,
                    CustomerName = string.IsNullOrWhiteSpace(name) ? user.FullName : name.Trim(),
                    CustomerPhone = string.IsNullOrWhiteSpace(phone) ? user.Phone : phone.Trim(),
                    ShippingAddress = string.IsNullOrWhiteSpace(address) ? "กรุงเทพมหานคร 10100" : address.Trim(),
                    OrderDate = now,
                    EstimatedWeightKg = dbCart.TotalWeightKg,
                    ItemsTotal = dbCart.ProductPriceBeforePromo,
                    DiscountTotal = dbCart.TotalPromotionDiscount,
                    ShippingFee = dbCart.ShippingFee,
                    GrandTotal = dbCart.GrandTotal,
                    FreeGiftItem = dbCart.FreeGiftItem,
                    Status = isCod ? OrderStatus.PaymentVerified : OrderStatus.PendingPayment,
                    PaymentMethod = cleanMethod,
                    PaymentSlipUrl = paymentSlipUrl,
                    IsPaymentApproved = false,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Items = trackedOldItems.Select(i => new OrderItem
                    {
                        OrderId = newOrderId,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        UnitPrice = i.Product.EffectivePrice,
                        Quantity = i.Quantity,
                        UnitWeightKg = i.Product.WeightKg
                    }).ToList()
                };

                _db.Orders.Add(newOrder);

                // บันทึก Snapshot โปรโมชั่นที่ใช้เข้ากับ OrderId
                var promoEntities = _db.Promotions.ToList();
                foreach (var applied in computedPromos)
                {
                    _db.AppliedPromotionSummaries.Add(new AppliedPromotionSummary
                    {
                        CartId = null,
                        OrderId = newOrderId,
                        PromotionTitle = applied.PromotionTitle,
                        DiscountAmount = applied.DiscountAmount,
                        Note = applied.Note
                    });

                    var promo = promoEntities.FirstOrDefault(p =>
                        applied.PromotionTitle.Equals(p.Title, StringComparison.Ordinal) ||
                        applied.PromotionTitle.StartsWith(p.Title + " (", StringComparison.Ordinal));
                    if (promo != null)
                    {
                        promo.IncrementUsage();
                        promo.UpdatedAt = now;
                    }
                }

                // สต็อกสินค้าถูกตัดตั้งแต่ตอนเพิ่มลงตะกร้าแล้ว จึงเพียงบันทึกเวลาอัปเดต
                foreach (var item in trackedOldItems)
                {
                    var prod = _db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (prod != null)
                    {
                        prod.UpdatedAt = now;
                    }
                }

                // ล้างตะกร้าใน DB (ลบรายการเก่าที่ถูก Track ไว้)
                dbCart.AppliedPromotions.Clear();
                if (trackedOldPromos.Any())
                {
                    _db.AppliedPromotionSummaries.RemoveRange(trackedOldPromos);
                }
                dbCart.Items.Clear();
                if (trackedOldItems.Any())
                {
                    _db.CartItems.RemoveRange(trackedOldItems);
                }
                dbCart.FreeGiftItem = null;
                dbCart.ShippingFee = 0;
                dbCart.IsFreeShippingQualified = false;
                dbCart.ShippingCalculationDetail = "ไม่มีสินค้าในตะกร้า";

                _db.SaveChanges();
                SyncFinancialSummaryInternal();
                tx.Commit();

                _cachedCart = null;
                LogAudit("CHECKOUT_ORDER", "orders", newOrder.OrderId, $"สร้างคำสั่งซื้อ {newOrder.OrderId} ยอดสุทธิ ฿{newOrder.GrandTotal:N2}", user);
                return (newOrder, null);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (null, $"เกิดข้อผิดพลาดในการบันทึกคำสั่งซื้อ: {ex.Message}");
            }
        }

        // ==========================================
        // Order, Shipping, Accounting Methods
        // ==========================================

        public List<Order> GetOrdersForUser(string username)
        {
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);
            if (user == null) return new List<Order>();

            return _db.Orders
                .Include(o => o.Items)
                .AsNoTracking()
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        public bool CancelOrder(string username, string orderId, string reason)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return false;

            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.OrderId == orderId && o.UserId == user.Id);
            if (order == null) return false;

            if (order.Status == OrderStatus.PendingPayment && !order.IsPaymentApproved)
            {
                order.Status = OrderStatus.Cancelled;
                order.RefundReason = reason;
                order.UpdatedAt = DateTime.Now;
                RestoreStockForOrder(order);
                _db.SaveChanges();
                SyncFinancialSummaryInternal();
                LogAudit("CANCEL_UNPAID_ORDER", "orders", order.OrderId, $"ยกเลิกออเดอร์ {orderId} (ยังไม่ชำระ) เหตุผล: {reason}", user);
                return true;
            }

            if (order.Status != OrderStatus.PaymentVerified && order.Status != OrderStatus.Preparing)
            {
                return false;
            }

            order.Status = OrderStatus.RefundRequested;
            order.RefundReason = reason;
            order.RefundAmount = order.GrandTotal;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("REQUEST_REFUND", "orders", order.OrderId, $"ขอคืนเงินออเดอร์ {orderId} เหตุผล: {reason}", user);
            return true;
        }

        public bool AttachPaymentSlip(string username, string orderId, string slipUrl)
        {
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return false;

            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId && o.UserId == user.Id);
            if (order == null || order.Status != OrderStatus.PendingPayment) return false;

            order.PaymentSlipUrl = slipUrl;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("UPLOAD_PAYMENT_SLIP", "orders", orderId, $"ลูกค้าอัปโหลดหลักฐานสลิปโอนเงิน {slipUrl}", user);
            return true;
        }

        public Order? ApprovePayment(string orderId)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return null;

            order.IsPaymentApproved = true;
            if (order.Status == OrderStatus.PendingPayment)
            {
                order.Status = OrderStatus.PaymentVerified;
            }
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            SyncFinancialSummaryInternal();
            LogAudit("APPROVE_PAYMENT", "orders", orderId, $"อนุมัติการชำระเงินออเดอร์ {orderId} ยอด ฿{order.GrandTotal:N2}");
            return order;
        }

        public Order? RejectPayment(string orderId, string reason)
        {
            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null || order.Status != OrderStatus.PendingPayment) return null;

            order.IsPaymentApproved = false;
            order.Status = OrderStatus.Cancelled;
            order.RefundReason = string.IsNullOrWhiteSpace(reason) ? "สลิปการชำระเงินไม่ถูกต้อง" : reason.Trim();
            order.UpdatedAt = DateTime.Now;
            RestoreStockForOrder(order);
            _db.SaveChanges();

            SyncFinancialSummaryInternal();
            LogAudit("REJECT_PAYMENT", "orders", orderId, $"ปฏิเสธสลิปออเดอร์ {orderId} เหตุผล: {order.RefundReason}");
            return order;
        }

        private void RestoreStockForOrder(Order order)
        {
            var now = DateTime.Now;
            foreach (var item in order.Items)
            {
                var prod = _db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (prod != null)
                {
                    prod.StockQuantity += item.Quantity;
                    prod.UpdatedAt = now;
                }
            }
        }

        public bool UpdateOrderStatus(string orderId, OrderStatus newStatus)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return false;

            var oldStatus = order.Status;
            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("UPDATE_ORDER_STATUS", "orders", orderId, $"เปลี่ยนสถานะ {orderId}: {oldStatus} -> {newStatus}");
            return true;
        }

        public bool DispatchOrder(string orderId, string courier, string trackingNumber, double actualWeightKg)
        {
            var order = _db.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return false;

            order.Courier = courier;
            order.TrackingNumber = trackingNumber;
            order.ActualWeightKg = actualWeightKg;
            order.Status = OrderStatus.Shipped;
            order.ShippedDate = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("DISPATCH_ORDER", "orders", orderId, $"จัดส่งโดย {courier} เลขพัสดุ {trackingNumber} นน.จริง {actualWeightKg}kg");
            return true;
        }

        public Order? ApproveRefund(string orderId)
        {
            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return null;

            order.Status = OrderStatus.Refunded;
            order.RefundedDate = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            if (!order.ShippedDate.HasValue)
            {
                RestoreStockForOrder(order);
            }
            _db.SaveChanges();

            SyncFinancialSummaryInternal();
            LogAudit("APPROVE_REFUND", "orders", orderId, $"อนุมัติคืนเงิน ฿{order.RefundAmount:N2}");
            return order;
        }

        // ==========================================
        // Inventory & Product Methods (`product`)
        // ==========================================

        public Product? UpdateProductStock(int id, int adjustment)
        {
            var product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return null;

            int oldStock = product.StockQuantity;
            product.StockQuantity = Math.Max(0, product.StockQuantity + adjustment);
            product.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("UPDATE_STOCK", "product", id.ToString(), $"ปรับสต็อก '{product.Name}' จาก {oldStock} เป็น {product.StockQuantity}");
            return product;
        }

        public bool SaveProduct(Product model, out string message)
        {
            var now = DateTime.Now;
            if (model.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    model.ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500&q=80";
                }
                model.CreatedAt = now;
                model.UpdatedAt = now;
                _db.Products.Add(model);
                _db.SaveChanges();

                LogAudit("CREATE_PRODUCT", "product", model.Id.ToString(), $"เพิ่มสินค้าใหม่ '{model.Name}'");
                message = $"เพิ่มสินค้า '{model.Name}' สำเร็จ";
                return true;
            }
            else
            {
                var existing = _db.Products.FirstOrDefault(p => p.Id == model.Id);
                if (existing == null)
                {
                    message = "ไม่พบสินค้าที่ต้องการแก้ไข";
                    return false;
                }

                existing.Name = model.Name;
                existing.Brand = model.Brand;
                existing.PetType = model.PetType;
                existing.Category = model.Category;
                existing.WeightKg = model.WeightKg;
                existing.Price = model.Price;
                existing.SpecialDiscountPrice = model.SpecialDiscountPrice;
                existing.AgeStage = model.AgeStage;
                existing.SpecialNeed = string.IsNullOrWhiteSpace(model.SpecialNeed) ? "ทั่วไป" : model.SpecialNeed;
                existing.StockQuantity = model.StockQuantity;
                existing.LowStockThreshold = model.LowStockThreshold;
                existing.Description = model.Description ?? "";
                if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    existing.ImageUrl = model.ImageUrl;
                }
                existing.UpdatedAt = now;

                _db.SaveChanges();
                LogAudit("UPDATE_PRODUCT", "product", existing.Id.ToString(), $"แก้ไขสินค้า '{existing.Name}'");
                message = $"แก้ไขข้อมูลสินค้า '{model.Name}' สำเร็จ";
                return true;
            }
        }

        public (bool Success, string Message) DeleteProduct(int id)
        {
            var existing = _db.Products.FirstOrDefault(p => p.Id == id);
            if (existing == null) return (false, "ไม่พบสินค้าที่ต้องการลบ");

            if (_db.OrderItems.Any(oi => oi.ProductId == id))
            {
                return (false, $"ไม่สามารถลบ '{existing.Name}' ได้ เนื่องจากมีประวัติการสั่งซื้ออ้างอิงอยู่ (แนะนำให้ปรับสต็อกเป็น 0 แทน)");
            }

            _db.Products.Remove(existing);
            _db.SaveChanges();
            LogAudit("DELETE_PRODUCT", "product", id.ToString(), $"ลบสินค้า '{existing.Name}'");
            return (true, $"ลบสินค้า '{existing.Name}' เรียบร้อยแล้ว");
        }

        // ==========================================
        // Marketing & Promotion Methods (`promotion`)
        // ==========================================

        public Promotion? TogglePromotionStatus(int id)
        {
            var promo = _db.Promotions.FirstOrDefault(p => p.Id == id);
            if (promo == null) return null;

            promo.IsActive = !promo.IsActive;
            promo.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

            LogAudit("TOGGLE_PROMOTION", "promotion", id.ToString(), $"ปรับสถานะ '{promo.Code}' เป็น {(promo.IsActive ? "Active" : "Inactive")}");
            return promo;
        }

        public (bool Success, string Message) SavePromotion(Promotion model)
        {
            string code = model.Code.Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                return (false, "กรุณาระบุรหัสโปรโมชั่น");
            }

            var now = DateTime.Now;
            if (model.Id == 0)
            {
                if (_db.Promotions.Any(p => p.Code == code))
                {
                    return (false, $"รหัสโปรโมชั่น '{code}' มีอยู่ในระบบแล้ว กรุณาใช้รหัสอื่น");
                }

                model.Code = code;
                model.UsageCount = 0;
                model.CreatedAt = now;
                model.UpdatedAt = now;

                _db.Promotions.Add(model);
                _db.SaveChanges();

                LogAudit("CREATE_PROMOTION", "promotion", model.Id.ToString(), $"สร้างแคมเปญ '{model.Code}' - {model.Title}");
                return (true, $"สร้างแคมเปญโปรโมชั่น '{model.Title}' สำเร็จแล้ว");
            }
            else
            {
                var existing = _db.Promotions.FirstOrDefault(p => p.Id == model.Id);
                if (existing == null) return (false, "ไม่พบโปรโมชั่นที่ต้องการแก้ไข");

                if (_db.Promotions.Any(p => p.Code == code && p.Id != model.Id))
                {
                    return (false, $"รหัสโปรโมชั่น '{code}' ถูกใช้งานโดยแคมเปญอื่นแล้ว");
                }

                existing.Code = code;
                existing.Title = model.Title;
                existing.Description = model.Description ?? "";
                existing.Type = model.Type;
                existing.MinQuantity = model.MinQuantity;
                existing.MinSpend = model.MinSpend;
                existing.ParticipatingBrand = string.IsNullOrWhiteSpace(model.ParticipatingBrand) ? null : model.ParticipatingBrand.Trim();
                existing.TargetCategory = model.TargetCategory;
                existing.DiscountAmount = model.DiscountAmount;
                existing.DiscountPercent = model.DiscountPercent;
                existing.GiftItemName = string.IsNullOrWhiteSpace(model.GiftItemName) ? null : model.GiftItemName.Trim();
                existing.StartDate = model.StartDate;
                existing.EndDate = model.EndDate;
                existing.IsActive = model.IsActive;
                existing.UpdatedAt = now;

                _db.SaveChanges();
                LogAudit("UPDATE_PROMOTION", "promotion", existing.Id.ToString(), $"แก้ไขแคมเปญ '{existing.Code}' - {existing.Title}");
                return (true, $"บันทึกการแก้ไขโปรโมชั่น '{existing.Title}' สำเร็จแล้ว");
            }
        }

        public (bool Success, string Message) DeletePromotion(int id)
        {
            var promo = _db.Promotions.FirstOrDefault(p => p.Id == id);
            if (promo == null) return (false, "ไม่พบโปรโมชั่นที่ต้องการลบ");

            _db.Promotions.Remove(promo);
            _db.SaveChanges();

            LogAudit("DELETE_PROMOTION", "promotion", id.ToString(), $"ลบแคมเปญ '{promo.Code}' - {promo.Title}");
            return (true, $"ลบแคมเปญโปรโมชั่น '{promo.Title}' เรียบร้อยแล้ว");
        }

        // ==========================================
        // IT Support & Helpdesk Methods (`supportticket`)
        // ==========================================

        public List<SupportTicket> GetSupportTickets(TicketStatus? status = null, string? customerEmail = null)
        {
            var query = _db.SupportTickets.AsNoTracking().AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                query = query.Where(t => t.ContactEmail == customerEmail);
            }

            var tickets = query.OrderByDescending(t => t.CreatedAt).ToList();
            var staffNames = _db.Users.AsNoTracking()
                .ToDictionary(u => u.Id, u => u.FullName);

            foreach (var t in tickets)
            {
                if (t.AssignedToStaffId.HasValue && staffNames.TryGetValue(t.AssignedToStaffId.Value, out var staffName))
                {
                    t.AssignedTechnician = staffName;
                }
            }

            return tickets;
        }

        public SupportTicket CreateSupportTicket(string customerName, string email, string phone, string category, string subject, string description, TicketPriority priority)
        {
            string datePrefix = $"TCK-{DateTime.Now:yyyyMMdd}-";
            int seq = _db.SupportTickets.Count(t => t.TicketCode.StartsWith(datePrefix)) + 1;
            string code = $"{datePrefix}{seq:D3}";
            while (_db.SupportTickets.Any(t => t.TicketCode == code))
            {
                seq++;
                code = $"{datePrefix}{seq:D3}";
            }

            string fullSubject = string.IsNullOrWhiteSpace(category) ? subject.Trim() : $"[{category.Trim()}] {subject.Trim()}";
            if (fullSubject.Length > 200) fullSubject = fullSubject[..200];

            var ticket = new SupportTicket
            {
                TicketCode = code,
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? "ลูกค้าทั่วไป" : customerName.Trim(),
                ContactEmail = string.IsNullOrWhiteSpace(email) ? "guest@petshop.com" : email.Trim(),
                CustomerPhone = phone?.Trim() ?? "",
                Category = category ?? "ทั่วไป",
                Subject = fullSubject,
                Description = description.Trim(),
                Priority = priority,
                Status = TicketStatus.New,
                CreatedAt = DateTime.Now
            };

            _db.SupportTickets.Add(ticket);
            _db.SaveChanges();

            LogAudit("CREATE_TICKET", "supportticket", ticket.TicketCode, $"เปิดคำร้องแจ้งปัญหา: {ticket.Subject}");
            return ticket;
        }

        public SupportTicket? ResolveSupportTicket(int ticketId, string technician, string resolutionNote, TicketStatus newStatus)
        {
            var ticket = _db.SupportTickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket == null) return null;

            int? staffId = GetCurrentUserId();
            if (!staffId.HasValue && !string.IsNullOrWhiteSpace(technician))
            {
                staffId = _db.Users
                    .Where(u => u.FullName == technician || u.Username == technician)
                    .Select(u => (int?)u.Id)
                    .FirstOrDefault();
            }

            ticket.AssignedToStaffId = staffId;
            ticket.AssignedTechnician = technician;
            ticket.ResolutionDetails = resolutionNote;
            ticket.Status = newStatus;
            if (newStatus == TicketStatus.Resolved || newStatus == TicketStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.Now;
            }

            _db.SaveChanges();
            LogAudit("RESOLVE_TICKET", "supportticket", ticket.TicketCode, $"อัปเดต Ticket #{ticket.TicketCode} เป็น {newStatus}");
            return ticket;
        }

        // ==========================================
        // Financial & Reporting (`financialsummary`, `monthlyfinancialentry`)
        // ==========================================

        private static bool IsPaidOrder(Order o) =>
            o.Status != OrderStatus.Cancelled &&
            o.Status != OrderStatus.Refunded &&
            o.Status != OrderStatus.PendingPayment;

        public void SaveMonthlyExpense(string month, decimal expenses)
        {
            if (string.IsNullOrWhiteSpace(month)) return;
            string cleanMonth = month.Trim();
            decimal cleanExp = Math.Max(0m, expenses);

            var now = DateTime.Now;
            var summary = _db.FinancialSummaries.Include(f => f.MonthlyBreakdown).FirstOrDefault();
            if (summary == null)
            {
                summary = new FinancialSummary { CreatedAt = now, UpdatedAt = now };
                _db.FinancialSummaries.Add(summary);
                _db.SaveChanges();
            }

            var entry = summary.MonthlyBreakdown.FirstOrDefault(m => m.Month == cleanMonth);
            if (entry == null)
            {
                summary.MonthlyBreakdown.Add(new MonthlyFinancialEntry
                {
                    FinancialSummaryId = summary.Id,
                    Month = cleanMonth,
                    Revenue = 0m,
                    Expenses = cleanExp,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else
            {
                entry.Expenses = cleanExp;
                entry.UpdatedAt = now;
            }

            _db.SaveChanges();
            SyncFinancialSummaryInternal();
            LogAudit("SAVE_MONTHLY_EXPENSE", "monthlyfinancialentry", cleanMonth, $"บันทึกรายจ่ายประจำเดือน {cleanMonth} จำนวน ฿{cleanExp:N2}");
        }

        private void SyncFinancialSummaryInternal()
        {
            var allOrders = _db.Orders.AsNoTracking().ToList();
            decimal totalRev = allOrders.Where(IsPaidOrder).Sum(o => o.GrandTotal);
            decimal totalRefund = allOrders.Where(o => o.Status == OrderStatus.Refunded).Sum(o => o.RefundAmount ?? 0m);

            var now = DateTime.Now;
            var summary = _db.FinancialSummaries.Include(f => f.MonthlyBreakdown).FirstOrDefault();
            if (summary == null)
            {
                summary = new FinancialSummary { CreatedAt = now };
                _db.FinancialSummaries.Add(summary);
            }

            // รวมรายรับจริงรายเดือนจากตาราง Orders แยกตาม yyyy-MM
            var revenueByMonth = allOrders
                .Where(IsPaidOrder)
                .GroupBy(o => o.OrderDate.ToString("yyyy-MM", CultureInfo.InvariantCulture))
                .ToDictionary(g => g.Key, g => g.Sum(o => o.GrandTotal));

            string currentMonth = DateTime.Now.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            if (!revenueByMonth.ContainsKey(currentMonth) && summary.MonthlyBreakdown.All(m => m.Month != currentMonth))
            {
                revenueByMonth[currentMonth] = 0m;
            }

            foreach (var kv in revenueByMonth)
            {
                var monthEntry = summary.MonthlyBreakdown.FirstOrDefault(m => m.Month == kv.Key);
                if (monthEntry == null)
                {
                    summary.MonthlyBreakdown.Add(new MonthlyFinancialEntry
                    {
                        Month = kv.Key,
                        Revenue = kv.Value,
                        Expenses = 0m,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    monthEntry.Revenue = kv.Value;
                    monthEntry.UpdatedAt = now;
                }
            }

            decimal totalRecordedExp = summary.MonthlyBreakdown.Sum(m => m.Expenses);

            summary.TotalRevenue = totalRev;
            summary.TotalExpenses = totalRecordedExp;
            summary.NetProfit = totalRev - totalRecordedExp - totalRefund;
            summary.TotalOrdersCount = allOrders.Count;
            summary.SuccessfulOrdersCount = allOrders.Count(IsPaidOrder);
            summary.RefundedOrdersCount = allOrders.Count(o => o.Status == OrderStatus.Refunded);
            summary.UpdatedAt = now;

            _db.SaveChanges();
        }

        public FinancialSummary GetFinancialSummary()
        {
            SyncFinancialSummaryInternal();
            var allOrders = _db.Orders.AsNoTracking().ToList();
            decimal totalRefund = allOrders.Where(o => o.Status == OrderStatus.Refunded).Sum(o => o.RefundAmount ?? 0m);
            decimal totalShipIncome = allOrders.Where(IsPaidOrder).Sum(o => o.ShippingFee);

            var dbSummary = _db.FinancialSummaries.Include(f => f.MonthlyBreakdown).AsNoTracking().FirstOrDefault();
            var breakdown = dbSummary?.MonthlyBreakdown
                .OrderByDescending(m => m.Month)
                .ToList() ?? new List<MonthlyFinancialEntry>();

            return new FinancialSummary
            {
                Id = dbSummary?.Id ?? 0,
                TotalRevenue = dbSummary?.TotalRevenue ?? 0m,
                TotalExpenses = dbSummary?.TotalExpenses ?? 0m,
                NetProfit = dbSummary?.NetProfit ?? 0m,
                TotalRefunds = totalRefund,
                ShippingCollected = totalShipIncome,
                ShippingCarrierCost = totalShipIncome,
                TotalOrdersCount = dbSummary?.TotalOrdersCount ?? 0,
                SuccessfulOrdersCount = dbSummary?.SuccessfulOrdersCount ?? 0,
                RefundedOrdersCount = dbSummary?.RefundedOrdersCount ?? 0,
                MonthlyBreakdown = breakdown
            };
        }
    }
}
