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

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<ShippingAddress> ShippingAddress { get; set; }
        public DbSet<Invoice> Invoice { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
			});

			modelBuilder.Entity<Category>(entity =>
			{
				entity.Property(e => e.Name).HasMaxLength(50);

			});


			modelBuilder.Entity<PaymentMethod>(entity =>
			{
				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(50);
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
			});
		}
	}
}
