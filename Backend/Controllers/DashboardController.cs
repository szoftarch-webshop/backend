using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("product-count-by-category")]
        public async Task<IActionResult> GetProductCountByCategory([FromQuery] int? categoryId = null)
        {
            var result = await _dashboardService.GetProductCountByCategoryAsync(categoryId);
            return Ok(result);
        }

        // Get Product Sales Percentage By Category with optional categoryId
        [HttpGet("product-sales-percentage")]
        public async Task<IActionResult> GetProductSalesPercentageByCategory([FromQuery] int? categoryId = null)
        {
            var result = await _dashboardService.GetProductSalesPercentageByCategoryAsync(categoryId);
            return Ok(result);
        }

        [HttpGet("top-selling-products")]
        public async Task<IActionResult> GetTopSellingProducts()
        {
            var result = await _dashboardService.GetTop5SellingProductsAsync();
            return Ok(result);
        }

        [HttpGet("monthly-sales-by-category")]
        public async Task<IActionResult> GetMonthlySalesByCategory()
        {
            var result = await _dashboardService.GetMonthlySalesByCategoryAsync();
            return Ok(result);
        }
    }
}
