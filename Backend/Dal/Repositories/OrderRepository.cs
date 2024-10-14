using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Backend.Dtos.Dashboard;
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

        // GET: Get list of orders with pagination, sorting, and filters
        public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy, string sortDirection, string status, DateTime? startDate, DateTime? endDate)
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
                "date" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate),
                "customer" => sortDirection.ToLower() == "desc" ? query.OrderByDescending(o => o.Invoice.CustomerName) : query.OrderBy(o => o.Invoice.CustomerName),
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

    // GET: Get a single order by ID
    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
		var order = await _context.Order
			.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Categories)
			.Include(o => o.ShippingAddress)
			.Include(o => o.Invoice).ThenInclude(i => i.PaymentMethod)
			.FirstOrDefaultAsync(o => o.Id == orderId);

		return order == null ? null : MapToOrderDto(order);
	}

	// POST: Create a new order
	public async Task<int> CreateOrderAsync(CreateOrderDto orderDto)
	{
		// Check if the payment method exists
		var paymentMethod = await _context.PaymentMethod
			.FirstOrDefaultAsync(pm => pm.Name == orderDto.Invoice.PaymentMethod);

		if (paymentMethod == null)
		{
			throw new Exception("Payment method does not exist.");
		}

		// Check if the products exist and map OrderItems
		var orderItems = new List<OrderItem>();
		foreach (var itemDto in orderDto.OrderItems)
		{
			var product = await _context.Product.FindAsync(itemDto.ProductId);
			if (product == null)
			{
				throw new Exception($"Product with ID {itemDto.ProductId} does not exist.");
			}

			orderItems.Add(new OrderItem
			{
				ProductId = itemDto.ProductId,
				Amount = itemDto.Amount,
				OrderedPrice = itemDto.OrderedPrice
			});
		}

		// Create the new Shipping Address
		var shippingAddress = new ShippingAddress
		{
			Name = orderDto.ShippingAddress.Name,
			PhoneNumber = orderDto.ShippingAddress.PhoneNumber,
			Email = orderDto.ShippingAddress.Email,
			Country = orderDto.ShippingAddress.Country,
			Street = orderDto.ShippingAddress.Street,
			City = orderDto.ShippingAddress.City,
			ZipCode = orderDto.ShippingAddress.ZipCode
		};

		_context.ShippingAddress.Add(shippingAddress);
		await _context.SaveChangesAsync();  // Save to generate the ID

		// Create the new Invoice
		var invoice = new Invoice
		{
			CustomerName = orderDto.Invoice.CustomerName,
			CustomerEmail = orderDto.Invoice.CustomerEmail,
			CustomerPhoneNumber = orderDto.Invoice.CustomerPhoneNumber,
			CustomerZipCode = orderDto.Invoice.CustomerZipCode,
			CustomerCountry = orderDto.Invoice.CustomerCountry,
			CustomerCity = orderDto.Invoice.CustomerCity,
			CustomerStreet = orderDto.Invoice.CustomerStreet,
			CreationDate = orderDto.Invoice.CreationDate,
			PaymentMethodId = paymentMethod.Id
		};

		_context.Invoice.Add(invoice);
		await _context.SaveChangesAsync();  // Save to generate the ID

		// Create the new Order
		var order = new Order
		{
			Status = "New",
			OrderDate = orderDto.OrderDate,
			ShippingAddressId = shippingAddress.Id,
			InvoiceId = invoice.Id,
			OrderItems = orderItems
		};

		_context.Order.Add(order);
		await _context.SaveChangesAsync();

		return order.Id;
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

    // DELETE: Delete an existing order
    public async Task<bool> DeleteOrderAsync(int orderId)
    {
		var order = await _context.Order
            .Include(o => o.Invoice)
            .Include(o => o.OrderItems)
            .Include(o => o.ShippingAddress)
            .SingleOrDefaultAsync(o => o.Id == orderId);
		if (order == null)
        {
			return false;
		}
        _context.Invoice.Remove(order.Invoice);
        _context.ShippingAddress.Remove(order.ShippingAddress);
        _context.OrderItem.RemoveRange(order.OrderItems);
		_context.Order.Remove(order);
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
                Name = order.ShippingAddress.Name,
                PhoneNumber = order.ShippingAddress.PhoneNumber,
                Email = order.ShippingAddress.Email,
                Country = order.ShippingAddress.Country,
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                ZipCode = order.ShippingAddress.ZipCode,
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
    
    public async Task<int> GetTotalSalesAsync(int? categoryId = null)
    {
	    // If no categoryId is provided, calculate total sales for all categories
	    if (categoryId == null)
	    {
		    return await _context.OrderItem.SumAsync(oi => oi.Amount);
	    }

	    // Get only the child categories of the provided parent category (excluding the parent itself)
	    var categoryIds = await _context.Category
		    .Where(c => c.ParentCategoryId == categoryId) // Only get child categories, not the parent
		    .Select(c => c.Id)
		    .ToListAsync();

	    // Calculate total sales for the products in these child categories
	    return await _context.OrderItem
		    .Where(oi => oi.Product.Categories.Any(c => categoryIds.Contains(c.Id)))
		    .SumAsync(oi => oi.Amount);
    }

    
    public async Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync(int? categoryId = null)
    {
	    var query = _context.OrderItem.AsQueryable();

	    if (categoryId.HasValue)
	    {
		    // Get the direct children of the selected category (excluding the parent)
		    var categoryIds = await _context.Category
			    .Where(c => c.ParentCategoryId == categoryId) // Only direct children
			    .Select(c => c.Id)
			    .ToListAsync();

		    if (!categoryIds.Any())
		    {
			    // If there are no child categories, return an empty result
			    return new List<CategorySalesDto>();
		    }

		    // Filter the order items by the products that belong to these child categories
		    query = query.Where(oi => oi.Product.Categories.Any(cp => categoryIds.Contains(cp.Id)));
	    }

	    // Group by the actual categories that belong to the child categories
	    return await query
		    .Select(oi => new
		    {
			    CategoryName = oi.Product.Categories.FirstOrDefault(c => categoryId == null || c.ParentCategoryId == categoryId).Name, // Get the category name
			    Amount = oi.Amount // Get the sales amount
		    })
		    .GroupBy(x => x.CategoryName)
		    .Select(g => new CategorySalesDto
		    {
			    CategoryName = g.Key,
			    SalesCount = g.Sum(x => x.Amount) // Sum the sales amount for each category
		    })
		    .ToListAsync();
    }








    public async Task<IEnumerable<ProductSalesDto>> GetTopSellingProductsAsync(int topN)
    {
	    return await _context.OrderItem
		    .Include(oi => oi.Product)
		    .GroupBy(oi => new { oi.Product.Name, oi.Product.Id })
		    .Select(g => new ProductSalesDto
		    {
			    ProductName = g.Key.Name,
			    ProductId = g.Key.Id,
			    SalesCount = g.Sum(oi => oi.Amount)
		    })
		    .OrderByDescending(ps => ps.SalesCount)
		    .Take(topN)
		    .ToListAsync();
    }

    public async Task<IEnumerable<MonthlyCategorySalesDto>> GetMonthlySalesByCategoryAsync()
    {
	    return await _context.OrderItem
		    .Include(oi => oi.Product)
		    .ThenInclude(p => p.Categories)
		    .GroupBy(oi => new
			    { Month = oi.Order.OrderDate.Month, Category = oi.Product.Categories.FirstOrDefault().Name })
		    .Select(g => new MonthlyCategorySalesDto
		    {
			    Month = g.Key.Month,
			    Category = g.Key.Category,
			    SalesCount = g.Sum(oi => oi.Amount)
		    })
		    .ToListAsync();
    }
    
    private async Task<List<int>> GetCategoryAndChildIdsAsync(int categoryId)
    {
	    var categoryIds = new List<int> { categoryId };

	    // Recursive method to get all child category IDs
	    async Task GetChildCategories(int parentCategoryId)
	    {
		    var childCategories = await _context.Category
			    .Where(c => c.ParentCategoryId == parentCategoryId)
			    .ToListAsync();

		    foreach (var childCategory in childCategories)
		    {
			    categoryIds.Add(childCategory.Id);
			    await GetChildCategories(childCategory.Id); // Recursively add child categories
		    }
	    }

	    // Fetch the child categories
	    await GetChildCategories(categoryId);

	    return categoryIds;
    }
}