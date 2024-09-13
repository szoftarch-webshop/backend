using Backend.Dal.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend;

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

		builder.Services.AddAuthorization();
		builder.Services.AddIdentityApiEndpoints<IdentityUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<DataContext>();

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseAuthorization();
		app.MapIdentityApi<IdentityUser>();
		app.MapControllers();
		
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
				var user = new IdentityUser();
				user.UserName = email;
				user.Email = email;

				await userManager.CreateAsync(user, password);

				await userManager.AddToRoleAsync(user, "Admin");
			}
		}

		// Run the application
		await app.RunAsync();
	}
}