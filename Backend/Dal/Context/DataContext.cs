using Backend.Dal.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Context
{
    public class DataContext : IdentityDbContext<IdentityUser>
    {
        public DataContext(DbContextOptions<DataContext> options) :
            base(options)
        { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasMany(e => e.Categories)
					.WithMany(e => e.Products)
					.UsingEntity(j => j.ToTable("Category"));
			});

			modelBuilder.Entity<Category>(entity =>
			{
				entity.Property(e => e.Name).HasMaxLength(50);

				entity.HasOne(d => d.ParentCategory)
					.WithMany(p => p.InverseParentCategory)
					.HasForeignKey(d => d.ParentCategoryId);
			});

            modelBuilder.Entity<Order>(entity =>
            {
				entity.HasOne(d => d.PaymentMethod)
					.WithMany(p => p.Orders)
					.HasForeignKey(d => d.PaymentMethodId);

				entity.HasOne(d => d.ShippingAddress)
					.WithOne(p => p.Order)
                    .HasForeignKey<Order>(d => d.ShippingAddressId);

				entity.HasOne(d => d.PaymentMethod)
					.WithMany(p => p.Orders)
					.HasForeignKey(d => d.PaymentMethodId);
			});	

            modelBuilder.Entity<OrderItem>(entity =>
            {
				entity.HasOne(d => d.Order)
					.WithMany(p => p.OrderItems)
					.HasForeignKey(d => d.OrderId);

				entity.HasOne(d => d.Product)
					.WithMany(p => p.OrderItems)
					.HasForeignKey(d => d.ProductId);
			});

			modelBuilder.Entity<PaymentMethod>(entity =>
			{
				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(50);

				entity.HasMany(d => d.Orders)
					.WithOne(p => p.PaymentMethod)
					.HasForeignKey(d => d.PaymentMethodId);

				entity.HasMany(d => d.Invoices)
					.WithOne(p => p.PaymentMethod)
					.HasForeignKey(d => d.PaymentMethodId);

			});

			modelBuilder.Entity<ShippingAddress>(entity =>
			{
				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.Email)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.Street)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.City)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.ZipCode)
					.IsRequired()
					.HasMaxLength(10);

				entity.Property(e => e.Country)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.PhoneNumber)
					.IsRequired()
					.HasMaxLength(20);
			});

			modelBuilder.Entity<Invoice>(entity =>
			{
				entity.Property(e => e.CustomerName)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.CustomerEmail)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.CustomerStreet)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.CustomerCity)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.CustomerZipCode)
					.IsRequired()
					.HasMaxLength(10);

				entity.Property(e => e.CustomerCountry)
					.IsRequired()
					.HasMaxLength(50);

				entity.Property(e => e.CustomerPhoneNumber)
					.IsRequired()
					.HasMaxLength(20);

				entity.HasOne(d => d.Order)
					.WithOne(p => p.Invoice)
					.HasForeignKey<Invoice>(d => d.OrderId);

				entity.HasOne(d => d.PaymentMethod)
					.WithMany(p => p.Invoices)
					.HasForeignKey(i => i.PaymentMethodId)
					.OnDelete(DeleteBehavior.Restrict);
			});
		}
	}
}
