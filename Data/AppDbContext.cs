using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MobileStore.Models;

namespace MobileStore.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Brand>       Brands       { get; set; }
        public DbSet<Phone>       Phones       { get; set; }
        public DbSet<Order>       Orders       { get; set; }
        public DbSet<OrderItem>   OrderItems   { get; set; }
        public DbSet<CartItem>    CartItems    { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ── Phone ──────────────────────────────────────────────────────
            builder.Entity<Phone>(e =>
            {
                e.HasOne(p => p.Brand)
                 .WithMany(b => b.Phones)
                 .HasForeignKey(p => p.BrandId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Seller)
                 .WithMany()
                 .HasForeignKey(p => p.SellerId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasIndex(p => p.Slug).IsUnique();
            });

            // ── Order ──────────────────────────────────────────────────────
            builder.Entity<Order>(e =>
            {
                e.HasOne(o => o.User)
                 .WithMany(u => u.Orders)
                 .HasForeignKey(o => o.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── OrderItem ──────────────────────────────────────────────────
            builder.Entity<OrderItem>(e =>
            {
                e.HasOne(i => i.Order)
                 .WithMany(o => o.Items)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(i => i.Phone)
                 .WithMany(p => p.OrderItems)
                 .HasForeignKey(i => i.PhoneId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.Ignore(i => i.Subtotal);
            });

            // ── CartItem ───────────────────────────────────────────────────
            builder.Entity<CartItem>(e =>
            {
                e.HasOne(c => c.User)
                 .WithMany(u => u.CartItems)
                 .HasForeignKey(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(c => c.Phone)
                 .WithMany(p => p.CartItems)
                 .HasForeignKey(c => c.PhoneId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(c => new { c.UserId, c.PhoneId }).IsUnique();
            });

            // ── Seed Brands ────────────────────────────────────────────────
            builder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Apple",   LogoUrl = "/images/brands/apple.png" },
                new Brand { Id = 2, Name = "Samsung", LogoUrl = "/images/brands/samsung.png" },
                new Brand { Id = 3, Name = "Xiaomi",  LogoUrl = "/images/brands/xiaomi.png" },
                new Brand { Id = 4, Name = "OPPO",    LogoUrl = "/images/brands/oppo.png" },
                new Brand { Id = 5, Name = "Huawei",  LogoUrl = "/images/brands/huawei.png" }
            );

            // ── Seed Phones ────────────────────────────────────────────────
            builder.Entity<Phone>().HasData(
                new Phone { Id=1, BrandId=1, Name="iPhone 15 Pro Max", Slug="iphone-15-pro-max",
                    Price=55000, OldPrice=60000, RAM=8, Storage=256, ScreenSize=6.7, Battery=4422,
                    Processor="Apple A17 Pro", Camera="48MP+12MP+12MP", OS="iOS 17", Network="5G",
                    Color="Titanium", IsAvailable=true, IsFeatured=true, Stock=15,
                    ImageUrl="/images/phones/iphone15promax.jpg",
                    Description="The most powerful iPhone ever with titanium design.",
                    CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
                new Phone { Id=2, BrandId=1, Name="iPhone 15", Slug="iphone-15",
                    Price=38000, OldPrice=null, RAM=6, Storage=128, ScreenSize=6.1, Battery=3877,
                    Processor="Apple A16 Bionic", Camera="48MP+12MP", OS="iOS 17", Network="5G",
                    Color="Blue", IsAvailable=true, IsFeatured=false, Stock=20,
                    ImageUrl="/images/phones/iphone15.jpg",
                    Description="Dynamic Island comes to all iPhone 15 models.",
                    CreatedAt=new DateTime(2024,1,2,0,0,0,DateTimeKind.Utc) },
                new Phone { Id=3, BrandId=2, Name="Samsung Galaxy S24 Ultra", Slug="samsung-s24-ultra",
                    Price=52000, OldPrice=57000, RAM=12, Storage=256, ScreenSize=6.8, Battery=5000,
                    Processor="Snapdragon 8 Gen 3", Camera="200MP+12MP+10MP+10MP", OS="Android 14", Network="5G",
                    Color="Titanium Black", IsAvailable=true, IsFeatured=true, Stock=10,
                    ImageUrl="/images/phones/s24ultra.jpg",
                    Description="Galaxy AI is here. The most powerful Galaxy ever.",
                    CreatedAt=new DateTime(2024,1,3,0,0,0,DateTimeKind.Utc) },
                new Phone { Id=4, BrandId=2, Name="Samsung Galaxy A56", Slug="samsung-a55",
                    Price=18000, OldPrice=20000, RAM=8, Storage=128, ScreenSize=6.6, Battery=5000,
                    Processor="Exynos 1480", Camera="50MP+12MP+5MP", OS="Android 14", Network="5G",
                    Color="Awesome Navy", IsAvailable=true, IsFeatured=false, Stock=30,
                    ImageUrl="/images/phones/a55.jpg",
                    Description="Pro-grade camera. Epic performance.",
                    CreatedAt=new DateTime(2024,1,4,0,0,0,DateTimeKind.Utc) },
                new Phone { Id=5, BrandId=3, Name="Xiaomi 14 Ultra", Slug="xiaomi-14-ultra",
                    Price=45000, OldPrice=null, RAM=16, Storage=512, ScreenSize=6.73, Battery=5000,
                    Processor="Snapdragon 8 Gen 3", Camera="50MP Leica Quad", OS="Android 14", Network="5G",
                    Color="White", IsAvailable=true, IsFeatured=true, Stock=8,
                    ImageUrl="/images/phones/xiaomi14ultra.jpg",
                    Description="Co-engineered with Leica. Photography redefined.",
                    CreatedAt=new DateTime(2024,1,5,0,0,0,DateTimeKind.Utc) },
                new Phone { Id=6, BrandId=3, Name="Xiaomi Redmi Note 13", Slug="redmi-note-13",
                    Price=9500, OldPrice=11000, RAM=8, Storage=256, ScreenSize=6.67, Battery=5000,
                    Processor="Snapdragon 685", Camera="108MP+8MP+2MP", OS="Android 13", Network="4G",
                    Color="Midnight Black", IsAvailable=true, IsFeatured=false, Stock=50,
                    ImageUrl="/images/phones/redminote13.jpg",
                    Description="108MP camera. Incredible value.",
                    CreatedAt=new DateTime(2024,1,6,0,0,0,DateTimeKind.Utc) }
            );
        }
    }
}
