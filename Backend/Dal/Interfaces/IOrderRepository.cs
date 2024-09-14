using Backend.Dtos;
using Backend.Dtos.Orders;

namespace Backend.Dal.Interfaces;

public interface IOrderRepository
{
    // POST: Create a new order
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto);

    // GET: Get list of orders with pagination, sorting, and filters
    Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string status, DateTime? startDate, DateTime? endDate);

    // PUT: Update the status of an existing order
    Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto);
}