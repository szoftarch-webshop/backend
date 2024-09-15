using Backend.Dal.Context;
using Backend.Dal.Interfaces;
using Backend.Dal.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

		builder.Services.AddScoped<CategoryService>();

		// Add Identity services
		builder.Services.AddIdentityApiEndpoints<IdentityUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<DataContext>()
			.AddDefaultTokenProviders();

		// Add Authorization (skip adding any policies or configurations here if not needed)
		builder.Services.AddAuthorization();

		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowSpecificOrigin",
				builder =>
				{
					builder.WithOrigins("http://localhost:3000")
						   .AllowAnyMethod()
						   .AllowAnyHeader()
						   .AllowCredentials(); // Allow credentials (cookies)
				});
		});

		builder.Services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin cookie sharing
			options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Required for HTTPS
			options.Cookie.HttpOnly = true;
		});

		var app = builder.Build();

		// Configure the HTTP request pipeline.
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		app.UseCors("AllowSpecificOrigin");
		
		app.UseAuthorization();
		
		// Map identity API endpoints
		app.MapIdentityApi<IdentityUser>();
		app.MapControllers();

		app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager) =>
		{

			await signInManager.SignOutAsync();
			return Results.Ok();

		}).RequireAuthorization();

		app.MapGet("/pingauth", (ClaimsPrincipal user) =>
		{
			var email = user.FindFirstValue(ClaimTypes.Email); // get the user's email from the claim
			return Results.Json(new { Email = email }); ; // return the email as a plain text response
		}).RequireAuthorization();

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
