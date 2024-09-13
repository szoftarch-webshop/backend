using Backend.Dal.Context;
using Backend.Dal.Interfaces;
using Backend.Dal.Repositories;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ProductController : ControllerBase
	{
		IProductRepository _productRepository;
		public ProductController(IProductRepository productRepository)
		{
			_productRepository = productRepository;
		}

		// GET: api/Product
		[HttpGet]
		public async Task<IActionResult> GetAllProducts(
			[FromQuery] int pageNumber = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string sortBy = "name",
			[FromQuery] string sortDirection = "asc",
			[FromQuery] int? minPrice = null,
			[FromQuery] int? maxPrice = null,
			[FromQuery] string category = null,
			[FromQuery] string material = null,
			[FromQuery] string searchString = null)
		{
			var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize, sortBy, sortDirection, minPrice, maxPrice, category, material, searchString);
			return Ok(products);
		}

		// GET: api/Product/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetProductById(int id)
		{
			var product = await _productRepository.GetProductByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			return Ok(product);
		}

		// GET: api/Product/serial/{serialNumber}
		[HttpGet("serial/{serialNumber}")]
		public async Task<IActionResult> GetProductBySerialNumber(string serialNumber)
		{
			var product = await _productRepository.GetProductBySerialNumberAsync(serialNumber);
			if (product == null)
			{
				return NotFound();
			}
			return Ok(product);
		}

		// POST: api/Product
		[HttpPost]
		public async Task<IActionResult> AddProduct(ProductDto productDto)
		{
			var productId = await _productRepository.AddProductAsync(productDto);
			return CreatedAtAction(nameof(GetProductById), new { id = productId }, productId);
		}

		// PUT: api/Product/{id}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
		{
			var success = await _productRepository.UpdateProductAsync(id, productDto);
			if (!success)
			{
				return NotFound();
			}
			return NoContent();
		}

		// DELETE: api/Product/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var success = await _productRepository.DeleteProductAsync(id);
			if (!success)
			{
				return NotFound();
			}
			return NoContent();
		}

		// PUT: api/Product/restock/{id}
		[HttpPut("restock/{id:int}")]
		public async Task<IActionResult> RestockProduct(int id, [FromBody] int additionalStock)
		{
			var success = await _productRepository.RestockProductAsync(id, additionalStock);
			if (!success)
			{
				return NotFound();
			}
			return NoContent();
		}

	}
}
