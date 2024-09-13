using Backend.Dal.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ProductController
	{
		DataContext _dbContext;
		public ProductController(DataContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpGet]
		public List<Dtos.Product> List([FromQuery] string search = null, [FromQuery] int from = 0)
		{
			List<Dtos.Product> mockList = new List<Dtos.Product>();
			foreach (var product in _dbContext.Products)
			{
				mockList.Add(new Dtos.Product(product.Id,product.Name,product.Weight, product.Material, product.Description, product.Price, product.Stock, product.ImageUrl, product.Categories.Select(c => c.Id).ToList()));
			} 

			return mockList;
	}

	}
}
