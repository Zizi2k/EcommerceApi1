using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PromotionalProduct> PromotionalProducts { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                .Property(c => c.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PromotionalProduct>(entity =>
            {
                entity.ToTable("PromotionalProducts");
                entity.Property(p => p.Headline).HasMaxLength(256);
                entity.Property(p => p.Subtitle).HasMaxLength(512);
                entity.Property(p => p.BadgeText).HasMaxLength(64);
                entity.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey(p => p.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
                entity.Property(u => u.FullName).HasMaxLength(256).IsRequired();
                entity.Property(u => u.Name).HasMaxLength(256).IsRequired();
                entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
                entity.Property(u => u.Role).HasMaxLength(32).IsRequired();
                entity.Property(u => u.AuthProvider).HasMaxLength(32).IsRequired();
                entity.Property(u => u.GoogleSub).HasMaxLength(128);
                entity.Property(u => u.GoogleId).HasMaxLength(128);
                entity.Property(u => u.AvatarUrl).HasMaxLength(2048);
                entity.Property(u => u.BackgroundUrl).HasMaxLength(2048);
            });
        }
    }
}
