using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CSI402_Project_final.Models;

namespace CSI402_Project_final.Data
{
    public class PetShopDbContext : DbContext
    {
        public PetShopDbContext(DbContextOptions<PetShopDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<SystemUser> Users => Set<SystemUser>();
        public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<AppliedPromotionSummary> AppliedPromotionSummaries => Set<AppliedPromotionSummary>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
        public DbSet<FinancialSummary> FinancialSummaries => Set<FinancialSummary>();
        public DbSet<MonthlyFinancialEntry> MonthlyFinancialEntries => Set<MonthlyFinancialEntry>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var doubleToDecimal = new ValueConverter<double, decimal>(
                v => (decimal)v,
                v => (double)v);

            var nullableDoubleToDecimal = new ValueConverter<double?, decimal?>(
                v => v.HasValue ? (decimal?)v.Value : null,
                v => v.HasValue ? (double?)v.Value : null);

            var permissionsConverter = new ValueConverter<List<string>?, string?>(
                v => v == null || v.Count == 0 ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : (JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()));

            var permissionsComparer = new ValueComparer<List<string>?>(
                (c1, c2) => (c1 ?? new List<string>()).SequenceEqual(c2 ?? new List<string>()),
                c => (c ?? new List<string>()).Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? null : c.ToList());

            // 1. product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Brand).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PetType).HasConversion<int>();
                entity.Property(e => e.Category).HasConversion<int>();
                entity.Property(e => e.WeightKg).HasConversion(doubleToDecimal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SpecialDiscountPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AgeStage).HasConversion<int>();
                entity.Property(e => e.SpecialNeed).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasColumnType("text");
            });

            // 2. promotion
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("promotion");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.MinSpend).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ParticipatingBrand).HasMaxLength(100);
                entity.Property(e => e.TargetCategory).HasConversion<int?>();
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.GiftItemName).HasMaxLength(200);
            });

            // 3. user
            modelBuilder.Entity<SystemUser>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Password).HasMaxLength(255).IsRequired();
                entity.Property(e => e.FullName).HasMaxLength(150).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Role).HasConversion<int>();
                entity.Property(e => e.GrantedPermissions)
                    .IsRequired(false)
                    .HasConversion(permissionsConverter)
                    .Metadata.SetValueComparer(permissionsComparer);
                entity.Property(e => e.GrantedPermissions).HasColumnType("json");

                entity.HasMany(e => e.Addresses)
                    .WithOne()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 4. useraddress
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.ToTable("useraddress");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(50);
                entity.Property(e => e.RecipientName).HasMaxLength(150);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.FullAddress).HasMaxLength(300);
                entity.Property(e => e.Subdistrict).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.Province).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
            });

            // 5. cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("cart");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.FreeGiftItem).HasMaxLength(200);
                entity.Property(e => e.ShippingFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ShippingCalculationDetail).HasColumnType("text");

                entity.HasOne<SystemUser>()
                    .WithOne()
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Items)
                    .WithOne()
                    .HasForeignKey(i => i.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.AppliedPromotions)
                    .WithOne()
                    .HasForeignKey(ap => ap.CartId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 6. cartitem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("cartitem");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CartId, e.ProductId }).IsUnique();

                entity.HasOne(i => i.Product)
                    .WithMany()
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 7. appliedpromotionsummary
            modelBuilder.Entity<AppliedPromotionSummary>(entity =>
            {
                entity.ToTable("appliedpromotionsummary");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).HasMaxLength(30);
                entity.Property(e => e.PromotionTitle).HasMaxLength(200);
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Note).HasColumnType("text");
            });

            // 8. orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).HasMaxLength(30);
                entity.Property(e => e.CustomerName).HasMaxLength(150);
                entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                entity.Property(e => e.ShippingAddress).HasMaxLength(500);
                entity.Property(e => e.EstimatedWeightKg).HasConversion(doubleToDecimal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ActualWeightKg).HasConversion(nullableDoubleToDecimal).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ItemsTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ShippingFee).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GrandTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.FreeGiftItem).HasMaxLength(200);
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Courier).HasMaxLength(100);
                entity.Property(e => e.TrackingNumber).HasMaxLength(50);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PaymentSlipUrl).HasMaxLength(500);
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RefundReason).HasColumnType("text");

                entity.HasOne<SystemUser>()
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.Items)
                    .WithOne()
                    .HasForeignKey(i => i.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 9. orderitem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("orderitem");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).HasMaxLength(30);
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitWeightKg).HasConversion(doubleToDecimal).HasColumnType("decimal(10,2)");

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 10. supportticket
            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.ToTable("supportticket");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TicketCode).HasColumnName("TicketNumber").HasMaxLength(50);
                entity.HasIndex(e => e.TicketCode).IsUnique();
                entity.Property(e => e.CustomerName).HasMaxLength(150);
                entity.Property(e => e.ContactEmail).HasColumnName("CustomerEmail").HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                entity.Property(e => e.Subject).HasMaxLength(200);
                entity.Property(e => e.Description).HasColumnType("text");
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Priority).HasConversion<int>();
                entity.Property(e => e.ResolutionDetails).HasColumnName("StaffNotes").HasColumnType("text");
                entity.Property(e => e.RelatedOrderId).HasMaxLength(30);
                entity.Property(e => e.ResolvedAt).HasColumnName("ResolvedDate");
            });

            // 11. financialsummary
            modelBuilder.Entity<FinancialSummary>(entity =>
            {
                entity.ToTable("financialsummary");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalExpenses).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetProfit).HasColumnType("decimal(18,2)");

                entity.HasMany(f => f.MonthlyBreakdown)
                    .WithOne()
                    .HasForeignKey(m => m.FinancialSummaryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // 12. monthlyfinancialentry
            modelBuilder.Entity<MonthlyFinancialEntry>(entity =>
            {
                entity.ToTable("monthlyfinancialentry");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Month).HasMaxLength(30);
                entity.Property(e => e.Revenue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Expenses).HasColumnType("decimal(18,2)");
            });

            // 13. auditlog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("auditlog");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.UserRole).HasMaxLength(50);
                entity.Property(e => e.Action).HasMaxLength(100);
                entity.Property(e => e.TableName).HasMaxLength(50);
                entity.Property(e => e.RecordId).HasMaxLength(50);
                entity.Property(e => e.Details).HasColumnType("text");
                entity.Property(e => e.IpAddress).HasMaxLength(45);

                entity.HasOne<SystemUser>()
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

