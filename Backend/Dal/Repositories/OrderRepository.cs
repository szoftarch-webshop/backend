using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos.Orders;
using Microsoft.EntityFrameworkCore;

namespace Backend.Dal.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DataContext _context;

        public OrderRepository(DataContext context)
        {
            _context = context;
        }

        // POST: Create a new order
        public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
        {
            // Map OrderDto to Order entity
            var order = new Order
            {
                Status = orderDto.Status,
                OrderDate = orderDto.OrderDate,
                ShippingAddressId = orderDto.ShippingAddress.Id, // Assuming ShippingAddress is already saved and has an ID
                PaymentMethodId = orderDto.PaymentMethod.Id, // Assuming PaymentMethod is already saved and has an ID
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Amount = item.Amount,
                    OrderedPrice = item.OrderedPrice
                }).ToList(),
                Invoice = orderDto.Invoice != null ? new Invoice
                {
                    // Map InvoiceDto to Invoice entity
                    Id = orderDto.Invoice.Id,
                    // Add other properties if needed
                    CreationDate = orderDto.Invoice.CreationDate
                } : null
            };

            // Add the order to the context and save changes
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Return the mapped OrderDto
            return MapToOrderDto(order);
        }

        // GET: Get list of orders with pagination, sorting, and filters
        public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.ShippingAddress)
                .Include(o => o.PaymentMethod)
                .Include(o => o.Invoice)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "date" => query.OrderBy(o => o.OrderDate),
                "status" => query.OrderBy(o => o.Status),
                _ => query.OrderBy(o => o.Id)
            };

            // Pagination
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var orders = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<OrderDto>
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                Items = orders.Select(MapToOrderDto).ToList()
            };
        }

        // PUT: Update the status of an existing order
        public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            order.Status = updateOrderStatusDto.NewStatus;
            await _context.SaveChangesAsync();

            return true;
        }

        // Helper method to map Order entity to OrderDto
        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Status = order.Status,
                OrderDate = order.OrderDate,
                ShippingAddress = new ShippingAddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Street = order.ShippingAddress.Street,
                    City = order.ShippingAddress.City,
                    ZipCode = order.ShippingAddress.ZipCode
                },
                PaymentMethod = new PaymentMethodDto
                {
                    Id = order.PaymentMethod.Id,
                    Name = order.PaymentMethod.Name
                },
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Amount = oi.Amount,
                    OrderedPrice = oi.OrderedPrice,
                    Product = new ProductDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        Price = oi.Product.Price,
                        ImageUrl = oi.Product.ImageUrl
                    }
                }).ToList(),
                Invoice = new InvoiceDto
                {
                    Id = order.Invoice.Id,
                    // Assuming InvoiceNumber is managed elsewhere, as it is not part of the entity
                    CreationDate = order.Invoice.CreationDate,
    
                    // Mapping the customer information
                    CustomerName = order.Invoice.CustomerName,
                    CustomerEmail = order.Invoice.CustomerEmail,
                    CustomerPhoneNumber = order.Invoice.CustomerPhoneNumber,
                    CustomerZipCode = order.Invoice.CustomerZipCode,
                    CustomerCountry = order.Invoice.CustomerCountry,
                    CustomerCity = order.Invoice.CustomerCity,
                    CustomerStreet = order.Invoice.CustomerStreet,

                    // Mapping PaymentMethod details (if needed)
                    PaymentMethodId = order.Invoice.PaymentMethodId,
                    PaymentMethod = new PaymentMethodDto
                    {
                        Id = order.Invoice.PaymentMethod.Id,
                        Name = order.Invoice.PaymentMethod.Name
                    }
                }
            };
        }
}