using Backend.Dal.Context;
using Backend.Dal.Interfaces;
using Backend.Dal.Repositories;
using Backend.Dtos.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
			[FromQuery] string? category = null,
			[FromQuery] string? material = null,
			[FromQuery] string? searchString = null)
		{
			var products = await _productRepository.GetAllProductsAsync(pageNumber, pageSize, sortBy, sortDirection, minPrice, maxPrice, category, material, searchString);
			return Ok(products);
		}

		// GET: api/Product/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetProductById(int id)
		{
			var product = await _productRepository.GetProductByIdAsync(id);
			return product != null ? Ok(product): NotFound();
		}

		// GET: api/Product/serial/{serialNumber}
		[HttpGet("serial/{serialNumber}")]
		public async Task<IActionResult> GetProductBySerialNumber(string serialNumber)
		{
			var product = await _productRepository.GetProductBySerialNumberAsync(serialNumber);
			return product != null ? Ok(product) : NotFound(); 
		}

		// POST: api/Product
		[HttpPost]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> AddProduct([FromForm] string productDtoJson, IFormFile image)
		{
			var productDto = JsonConvert.DeserializeObject<ProductDto>(productDtoJson);
    
			if (productDto == null)
			{
				return BadRequest("Invalid productDto data.");
			}
			
			try
			{
				var productId = await _productRepository.AddProductAsync(productDto, image);
				return CreatedAtAction(nameof(GetProductById), new { id = productId }, productId);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// PUT: api/Product/{id}
		[HttpPut("{id:int}")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> UpdateProduct(int id, [FromForm] string productDtoJson, IFormFile image)
		{
			var productDto = JsonConvert.DeserializeObject<ProductDto>(productDtoJson);

			if (productDto == null)
			{
				return BadRequest("Invalid productDto data.");
			}

			try
			{
				var success = await _productRepository.UpdateProductAsync(id, productDto, image);
				return success ? NoContent() : NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		// DELETE: api/Product/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var success = await _productRepository.DeleteProductAsync(id);
			return success ? NoContent() : NotFound();
		}

		// PUT: api/Product/restock/{id}
		[HttpPut("restock/{id:int}")]
		public async Task<IActionResult> RestockProduct(int id, [FromBody] int additionalStock)
		{
			var success = await _productRepository.RestockProductAsync(id, additionalStock);
			return success ? NoContent() : NotFound();
		}

	}
}
