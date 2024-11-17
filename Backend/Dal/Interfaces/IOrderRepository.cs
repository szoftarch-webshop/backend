using Backend.Dtos;
using Backend.Dtos.Dashboard;
using Backend.Dtos.Orders;
using Backend.Dtos.Orders.InitiatePayment;

namespace Backend.Dal.Interfaces;

public interface IOrderRepository
{
    // GET: Get list of orders with pagination, sorting, and filters
    Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string sortDirection, string status, DateTime? startDate, DateTime? endDate);

	Task<OrderDto?> GetOrderByIdAsync(int orderId);

	// POST: Create a new order
	Task<int> CreateOrderAsync(CreateOrderDto orderDto);

	// PUT: Update the status of an existing order
	Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);

	// DELETE: Delete an existing order
	Task<bool> DeleteOrderAsync(int orderId);

	public Task<int> GetTotalSalesAsync(int? categoryId = null);

	public Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync(int? categoryId = null);

	public Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topN);

	public Task<IEnumerable<MonthlyCategorySalesDto>> GetMonthlySalesByCategoryAsync();

	public Task<PaymentResponse> InitializeOrder(PaymentDetails paymentDetails);
}