using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebStore.Models;

namespace WebStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Login).IsRequired();
            modelBuilder.Entity<User>().Property(u => u.Password).IsRequired();

            modelBuilder.Entity<Region>().ToTable("Regions");
            modelBuilder.Entity<Region>().HasKey(r => r.Id);
            modelBuilder.Entity<Region>().Property(r => r.Name).IsRequired();
            modelBuilder.Entity<Region>().HasOne(r => r.Parent).WithMany(r => r.Children).HasForeignKey(r => r.ParentId);

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<Order>().HasKey(o => o.Id);
            modelBuilder.Entity<Order>().Property(o => o.Date).IsRequired();
            modelBuilder.Entity<Order>().HasOne(o => o.Region).WithMany().HasForeignKey(o => o.RegionId);
            modelBuilder.Entity<Order>().HasOne(o => o.Item).WithMany().HasForeignKey(o => o.ItemId).OnDelete(DeleteBehavior.Cascade); ;
            modelBuilder.Entity<Order>().Property(o => o.Amount).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Order>().HasOne(o => o.Users).WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<Item>().HasKey(i => i.Id);
            modelBuilder.Entity<Item>().Property(i => i.Name).IsRequired();
            modelBuilder.Entity<Item>().Property(i => i.Price).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Item>().Property(i => i.Group).IsRequired();
            modelBuilder.Entity<Item>().HasOne(i => i.Users).WithMany().HasForeignKey(i => i.UserId);
        }
    }
}
