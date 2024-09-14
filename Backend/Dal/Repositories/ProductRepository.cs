using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Backend.Dtos.Products;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repositories
{
    public class ProductRepository : IProductRepository
	{
		private readonly DataContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductRepository(DataContext context, IWebHostEnvironment webHostEnvironment)
		{
			_context = context;
			_webHostEnvironment = webHostEnvironment;
		}

		public async Task<int> AddProductAsync(ProductDto productDto, IFormFile image)
		{
			string? imagePath = null;

			if (image != null)
			{
				var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(image.FileName)}";
				var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

				await using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await image.CopyToAsync(stream);
				}

				imagePath = Path.Combine("images", fileName);
			}

			var productEntity = new Product
			{
				SerialNumber = productDto.SerialNumber,
				Name = productDto.Name,
				Weight = productDto.Weight,
				Material = productDto.Material,
				Description = productDto.Description,
				Price = productDto.Price,
				Stock = productDto.Stock,
				ImageUrl = imagePath,
				Categories = _context.Category.Where(c => productDto.CategoryNames.Contains(c.Name)).ToList()
			};

			_context.Product.Add(productEntity);
			await _context.SaveChangesAsync();

			return productEntity.Id;
		}

		public async Task<bool> DeleteProductAsync(int id)
		{
			var product = await _context.Product.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			_context.Product.Remove(product);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<PaginatedResult<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize, string sortBy, string sortDirection, int? minPrice, int? maxPrice, string? category, string? material, string? searchString)
		{
			var query = _context.Product.Include(p => p.Categories).AsQueryable();

			if (minPrice.HasValue)
			{
				query = query.Where(p => p.Price >= minPrice.Value);
			}
			if (maxPrice.HasValue)
			{
				query = query.Where(p => p.Price <= maxPrice.Value);
			}

			if (!string.IsNullOrEmpty(category))
			{
				query = query.Where(p => p.Categories.Any(c => c.Name == category));
			}

			if (!string.IsNullOrEmpty(material))
			{
				query = query.Where(p => p.Material == material);
			}

			if (!string.IsNullOrEmpty(searchString))
			{
				query = query.Where(p => p.Name.Contains(searchString));
			}

			if (!string.IsNullOrEmpty(sortBy))
			{
				query = sortBy.ToLower() switch
				{
					"name" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
					"price" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
					_ => query // Default sorting logic (if needed)
				};
			}

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			var products = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

			return new PaginatedResult<ProductDto> 
			{ 
				CurrentPage = pageNumber, 
				TotalItems = totalItems, 
				TotalPages = totalPages,
				Items = products.Select(MapToProductDto).ToList()
			}; 
		}

		public async Task<ProductDto?> GetProductByIdAsync(int id)
		{
			var product = await _context.Product
				.Include(p => p.Categories)
				.FirstOrDefaultAsync(p => p.Id == id);

			return product != null ? MapToProductDto(product) : null;
		}

		public async Task<ProductDto?> GetProductBySerialNumberAsync(string serialNumber)
		{
			var product = await _context.Product
				.Include(p => p.Categories)
			.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);

			return product != null ? MapToProductDto(product) : null;
		}

		public async Task<bool> RestockProductAsync(int id, int additionalStock)
		{
			var product = await _context.Product.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			product.Stock += additionalStock;
			_context.Product.Update(product);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UpdateProductAsync(int id, ProductDto productDto, IFormFile image)
		{
			var product = await _context.Product.Include(p => p.Categories).FirstOrDefaultAsync(p => p.Id == id);
			if (product == null)
			{
				return false;
			}

			// Handle image upload if a new image is provided
			if (image != null)
			{
				var fileName = $"{Guid.NewGuid()}-{Path.GetFileName(image.FileName)}";
				var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

				await using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await image.CopyToAsync(stream);
				}

				product.ImageUrl = Path.Combine("images", fileName);
			}

			product.SerialNumber = productDto.SerialNumber;
			product.Name = productDto.Name;
			product.Weight = productDto.Weight;
			product.Material = productDto.Material;
			product.Description = productDto.Description;
			product.Price = productDto.Price;
			product.Stock = productDto.Stock;

			// Get existing categories from the database
			var existingCategories = await _context.Category
				.Where(c => productDto.CategoryNames.Contains(c.Name))
				.ToListAsync();

			// Clear the existing categories
			product.Categories.Clear();

			// Add the existing categories from the DTO
			foreach (var category in existingCategories)
			{
				product.Categories.Add(category);
			}

			_context.Product.Update(product);
			await _context.SaveChangesAsync();
			return true;
		}

		private static ProductDto MapToProductDto(Product product)
		{
			return new ProductDto {
				Id = product.Id,
				SerialNumber = product.SerialNumber,
				Name = product.Name,
				Weight = product.Weight,
				Material = product.Material,
				Description = product.Description,
				Price = product.Price,
				Stock = product.Stock,
				ImageUrl =product.ImageUrl,
				CategoryNames = product.Categories.Select(c => c.Name).ToList()
			};
		}
	}
}
