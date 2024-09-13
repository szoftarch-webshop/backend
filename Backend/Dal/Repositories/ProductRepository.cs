using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repositories
{
	public class ProductRepository : IProductRepository
	{
		private readonly DataContext _context;

		public ProductRepository(DataContext context)
		{
			_context = context;
		}

		public async Task<int> AddProductAsync(ProductDto productDto)
		{
			var product = new Product
			{
				SerialNumber = productDto.SerialNumber,
				Name = productDto.Name,
				Weight = productDto.Weight,
				Material = productDto.Material,
				Description = productDto.Description,
				Price = productDto.Price,
				Stock = productDto.Stock,
				ImageUrl = productDto.ImageUrl
			};

			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			return product.Id;
		}

		public async Task<bool> DeleteProductAsync(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize, string sortBy, string sortDirection, int? minPrice, int? maxPrice, string category, string material, string searchString)
		{
			var query = _context.Products.AsQueryable();

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
			query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

			return await query
				.Select(p => ConvertToProductDto(p)).ToListAsync();
		}

		public async Task<ProductDto?> GetProductByIdAsync(int id)
		{
			var product = await _context.Products
			.FirstOrDefaultAsync(p => p.Id == id);

			return ConvertToProductDto(product);
		}

		public async Task<ProductDto?> GetProductBySerialNumberAsync(string serialNumber)
		{
			var product = await _context.Products
			.FirstOrDefaultAsync(p => p.SerialNumber == serialNumber);

			return ConvertToProductDto(product);
		}

		public async Task<bool> RestockProductAsync(int id, int additionalStock)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			product.Stock += additionalStock;
			_context.Products.Update(product);
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task<bool> UpdateProductAsync(int id, ProductDto productDto)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return false;
			}

			product.SerialNumber = productDto.SerialNumber;
			product.Name = productDto.Name;
			product.Weight = productDto.Weight;
			product.Material = productDto.Material;
			product.Description = productDto.Description;
			product.Price = productDto.Price;
			product.Stock = productDto.Stock;
			product.ImageUrl = productDto.ImageUrl;

			_context.Products.Update(product);
			await _context.SaveChangesAsync();
			return true;
		}

		private static ProductDto? ConvertToProductDto(Product? product)
		{
			return product == null ? null : new ProductDto(
				product.Id,
				product.SerialNumber,
				product.Name,
				product.Weight,
				product.Material,
				product.Description,
				product.Price,
				product.Stock,
				product.ImageUrl,
				product.Categories.Select(c => c.Name).ToList()
			);
		}
	}
}
