using Backend.Dtos;
using Backend.Dtos.Orders;

namespace Backend.Dal.Interfaces;

public interface IOrderRepository
{
    // GET: Get list of orders with pagination, sorting, and filters
    Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string status, DateTime? startDate, DateTime? endDate);

	Task<OrderDto?> GetOrderByIdAsync(int orderId);

	// POST: Create a new order
	Task<int> CreateOrderAsync(CreateOrderDto orderDto);

	// PUT: Update the status of an existing order
	Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);

	// DELETE: Delete an existing order
	Task<bool> DeleteOrderAsync(int orderId);
}