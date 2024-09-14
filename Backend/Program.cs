using Backend.Dal.Context;
using Backend.Dal.Interfaces;
using Backend.Dal.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Program
{
	public static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen();

		builder.Services.AddDbContext<DataContext>(options =>
			options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

		builder.Services.AddScoped<IProductRepository, ProductRepository>();
		builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
		builder.Services.AddScoped<IOrderRepository, OrderRepository>();

		// Add Identity services
		builder.Services.AddIdentityApiEndpoints<IdentityUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<DataContext>();

		// Add Authorization (skip adding any policies or configurations here if not needed)
		builder.Services.AddAuthorization();
		
		builder.Services.AddCors();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000"));
		
		// Only use authentication and authorization in non-development environments
		if (!app.Environment.IsDevelopment())
		{
			app.UseAuthentication();
			app.UseAuthorization();
		}
		
		// Map identity API endpoints
		app.MapIdentityApi<IdentityUser>();
		app.MapControllers();

		app.UseStaticFiles();
		
		// Seed roles and an admin user
		using (var scope = app.Services.CreateScope())
		{
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var roles = new[] { "Admin", "Customer" };

			foreach (var role in roles)
			{
				if (!await roleManager.RoleExistsAsync(role))
				{
					await roleManager.CreateAsync(new IdentityRole(role));
				}
			}
		}

		using (var scope = app.Services.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

			string email = "admin@admin.com";
			string password = "Test1234,";

			if (await userManager.FindByEmailAsync(email) == null)
			{
				var user = new IdentityUser
				{
					UserName = email,
					Email = email
				};

				await userManager.CreateAsync(user, password);
				await userManager.AddToRoleAsync(user, "Admin");
			}
		}

		// Run the application
		await app.RunAsync();
	}
}
