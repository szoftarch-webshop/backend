using Backend.Dtos;
using Backend.Dtos.Products;

namespace Backend.Dal.Interfaces
{
    public interface IProductRepository
	{
		// Végfelhasználói
		Task<PaginatedResult<ProductDto>> GetAllProductsAsync(
			int pageNumber,
			int pageSize,
			string sortBy,
			string sortDirection,
			int? minPrice,
			int? maxPrice,
			string? category,
			string? material,
			string? searchString
		);
		Task<ProductDto?> GetProductBySerialNumberAsync(string serialNumber);
		Task<ProductDto?> GetProductByIdAsync(int id);

		// Admin
		Task<int> AddProductAsync(CreateProductDto productDto);
		Task<bool> UpdateProductAsync(int id, CreateProductDto productDto);
		Task<bool> DeleteProductAsync(int id);
		Task<bool> RestockProductAsync(int id, int additionalStock);
	}
}
