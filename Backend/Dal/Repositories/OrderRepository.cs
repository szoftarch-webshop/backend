using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Backend.Dtos.Orders;
using Backend.Dtos.Products;
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
            var order = new Order
            {
                Status = orderDto.Status,
                OrderDate = orderDto.OrderDate,
                ShippingAddressId = orderDto.ShippingAddress.Id,
                OrderItems = orderDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Amount = item.Amount,
                    OrderedPrice = item.OrderedPrice
                }).ToList(),
                Invoice = orderDto.Invoice != null ? 
                new Invoice
                {
					Id = orderDto.Invoice.Id,
                    CreationDate = orderDto.Invoice.CreationDate
                } : null
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            return MapToOrderDto(order);
        }

        // GET: Get list of orders with pagination, sorting, and filters
        public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string status, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Order
				.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Categories)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Invoice).ThenInclude(i => i.PaymentMethod)
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
            var order = await _context.Order.FindAsync(orderId);
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
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    Amount = oi.Amount,
                    OrderedPrice = oi.OrderedPrice,
                    Product = new ProductDto
                    {
                        Id = oi.Product.Id,
                        SerialNumber = oi.Product.SerialNumber,
                        Name = oi.Product.Name,
                        Description = oi.Product.Description,
                        Weight = oi.Product.Weight,
                        Material = oi.Product.Material,
                        Stock = oi.Product.Stock,
                        Price = oi.Product.Price,
                        ImageUrl = oi.Product.ImageUrl,
                        CategoryNames = oi.Product.Categories.Select(c => c.Name).ToList()
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
                    PaymentMethod =new PaymentMethodDto
                    {
                        Id = order.Invoice.PaymentMethod.Id,
                        Name = order.Invoice.PaymentMethod.Name,
                    },
                }
            };
        }
}