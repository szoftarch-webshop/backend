using Backend.Dal.Interfaces;
using Backend.Dtos.Dashboard;

namespace Backend.Services;

public class DashboardService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public DashboardService(ICategoryRepository categoryRepository, IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // Number of Products by Category (Pie Chart)
    public async Task<IEnumerable<CategoryProductCountDto>> GetProductCountByCategoryAsync(int? categoryId = null)
    {
        return await _productRepository.GetProductCountByCategoryAsync(categoryId);
    }

    // Products Sold by Percentage (Pie Chart)
    public async Task<IEnumerable<CategorySalesPercentageDto>> GetProductSalesPercentageByCategoryAsync(int? categoryId = null)
    {
        // Get the total sales for the parent category and its children (if categoryId is specified)
        var totalSales = await _orderRepository.GetTotalSalesAsync(categoryId);

        // Get the sales for the direct child categories
        var salesByCategory = await _orderRepository.GetSalesByCategoryAsync(categoryId);

        return salesByCategory.Select(s => new CategorySalesPercentageDto
        {
            CategoryName = s.CategoryName,
            Percentage = (double)s.SalesCount / totalSales * 100
        }).ToList();
    }

    // Top 5 Selling Products (Horizontal Bar Chart)
    public async Task<IEnumerable<ProductSalesDto>> GetTop5SellingProductsAsync()
    {
        return await _orderRepository.GetTopSellingProductsAsync(5);
    }

    // Monthly Sales by Main Category (Stacked Bar Chart)
    public async Task<IEnumerable<MonthlyCategorySalesDto>> GetMonthlySalesByCategoryAsync()
    {
        return await _orderRepository.GetMonthlySalesByCategoryAsync();
    }
}