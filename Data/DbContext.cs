namespace QuitQ1_Hx.Data
{
    using Microsoft.EntityFrameworkCore;
    using QuitQ1_Hx.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RevokedTokens> RevokedTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ✅ Call only once

            // ✅ Seed Categories data
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Clothing" },
                new Category { Id = 3, Name = "Home & Kitchen" }
            );

            // ✅ Seed Sellers data
            //modelBuilder.Entity<Seller>().HasData(
            //    new Seller
            //    {
            //        Id = 1,
            //        Name = "Default Seller",
            //        Email = "seller@example.com",
            //        PhoneNumber = "1234567890",
            //        IsActive = true,
            //        Address = "123 Main St, City, State 12345",
            //        Description = "Default seller description",
            //        CreatedAt = DateTime.UtcNow
            //    }
            //);

            // ✅ User → Cart (Cascade delete)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Prevent multiple cascade paths for Orders
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ User → Addresses (Cascade delete)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Configure Product ImageUrls conversion
            modelBuilder.Entity<Product>()
                .Property(p => p.ImageUrls)
                .HasConversion(
                    v => v != null ? string.Join(";", v) : string.Empty,  // Prevent null reference in Join
                    v => v != null ? v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>() // Prevent null reference in Split
                );

            // ✅ Prevent multiple cascade paths in Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ Cart → CartItems (Cascade delete)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Prevent multiple cascade paths in CartItems
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // ✅ Order → OrderItems (Cascade delete)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Prevent multiple cascade paths for Orders → Shipping Address
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.SetNull);

            // ✅ Set precision for decimal fields
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // ✅ Indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // ✅ Configure Seller entity
            modelBuilder.Entity<Seller>()
                .HasIndex(s => s.Email)
                .IsUnique();
        }
    }
}