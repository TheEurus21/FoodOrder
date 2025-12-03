using Microsoft.EntityFrameworkCore;
using FoodOrder.Models;

namespace FoodOrder.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Restaurant> Restaurants { get; set; }
        public DbSet<FoodCategory> Categories { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restaurant -> Categories
            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.Categories)
                .WithOne(c => c.Restaurant)
                .HasForeignKey(c => c.RestaurantId)
                .OnDelete(DeleteBehavior.NoAction);


            // FoodCategory -> Foods
            modelBuilder.Entity<FoodCategory>()
                .HasMany(fc => fc.Foods)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.FoodCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            // Food -> Reviews
            modelBuilder.Entity<Food>()
                .HasMany(f => f.Reviews)
                .WithOne(r => r.Food)
                .HasForeignKey(r => r.FoodId)
                .OnDelete(DeleteBehavior.NoAction);

            // Review -> User
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Order -> User
            modelBuilder.Entity<Order>()
     .HasOne(o => o.User)
     .WithMany(u => u.Orders)
     .HasForeignKey(o => o.UserId)
     .OnDelete(DeleteBehavior.Restrict);

            // Order -> Restaurant
           modelBuilder.Entity<Order>()
    .HasOne(o => o.Restaurant)
    .WithMany(r => r.Orders)
    .HasForeignKey(o => o.RestaurantId)
    .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> Order
            modelBuilder.Entity<OrderItem>()
     .HasOne(oi => oi.Order)
     .WithMany(o => o.Items)
     .HasForeignKey(oi => oi.OrderId)
     .OnDelete(DeleteBehavior.Restrict);

            // OrderItem -> Food
            modelBuilder.Entity<OrderItem>()
    .HasOne(oi => oi.Food)
    .WithMany(f => f.Items)
    .HasForeignKey(oi => oi.FoodId)
    .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<Food>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Restaurant>()
       .Property(r => r.RestaurantCode)
       .HasDefaultValueSql("NEWID()");
        }

    }
}
