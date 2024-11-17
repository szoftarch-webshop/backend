using Backend.Dal.Context;
using Backend.Dal.Entities;
using Backend.Dal.Interfaces;
using Backend.Dtos;
using Backend.Dtos.Dashboard;
using Backend.Dtos.Orders;
using Backend.Dtos.Orders.InitiatePayment;
using Backend.Dtos.Products;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Backend.Dal.Repositories;

public class OrderRepository : IOrderRepository
{
	private readonly DataContext _context;
	private readonly IServiceProvider _serviceProvider;

	public OrderRepository(DataContext context, IServiceProvider serviceProvider)
	{
		_context = context;
		_serviceProvider = serviceProvider;
	}

	// GET: Get list of orders with pagination, sorting, and filters
	public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(int pageNumber, int pageSize, string sortBy,
		string sortDirection, string status, DateTime? startDate, DateTime? endDate)
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
			"date" => sortDirection.ToLower() == "desc"
				? query.OrderByDescending(o => o.OrderDate)
				: query.OrderBy(o => o.OrderDate),
			"customer" => sortDirection.ToLower() == "desc"
				? query.OrderByDescending(o => o.Invoice.CustomerName)
				: query.OrderBy(o => o.Invoice.CustomerName),
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
		await _context.SaveChangesAsync(); // Save to generate the ID

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
		await _context.SaveChangesAsync(); // Save to generate the ID

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
				PaymentMethod = new PaymentMethodDto
				{
					Id = order.Invoice.PaymentMethod.Id,
					Name = order.Invoice.PaymentMethod.Name,
				},
			}
		};
	}

	public async Task<int> GetTotalSalesAsync(int? categoryId = null)
	{
		if (categoryId == null)
		{
			return await _context.OrderItem.SumAsync(oi => oi.Amount);
		}

		var categoryIds = await _context.Category
			.Where(c => c.ParentCategoryId == categoryId)
			.Select(c => c.Id)
			.ToListAsync();

		return await _context.OrderItem
			.Where(oi => oi.Product.Categories.Any(c => categoryIds.Contains(c.Id)))
			.SumAsync(oi => oi.Amount);
	}


	public async Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync(int? categoryId = null)
	{
		var query = _context.OrderItem.AsQueryable();

		if (categoryId.HasValue)
		{
			var categoryIds = await _context.Category
				.Where(c => c.ParentCategoryId == categoryId)
				.Select(c => c.Id)
				.ToListAsync();

			if (!categoryIds.Any())
			{
				return new List<CategorySalesDto>();
			}

			query = query.Where(oi => oi.Product.Categories.Any(cp => categoryIds.Contains(cp.Id)));
		}

		return await query
			.Select(oi => new
			{
				CategoryName = oi.Product.Categories
					.FirstOrDefault(c => categoryId == null || c.ParentCategoryId == categoryId).Name,
				Amount = oi.Amount
			})
			.GroupBy(x => x.CategoryName)
			.Select(g => new CategorySalesDto
			{
				CategoryName = g.Key,
				SalesCount = g.Sum(x => x.Amount)
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

		async Task GetChildCategories(int parentCategoryId)
		{
			var childCategories = await _context.Category
				.Where(c => c.ParentCategoryId == parentCategoryId)
				.ToListAsync();

			foreach (var childCategory in childCategories)
			{
				categoryIds.Add(childCategory.Id);
				await GetChildCategories(childCategory.Id);
			}
		}

		await GetChildCategories(categoryId);

		return categoryIds;
	}

	public async Task<PaymentResponse> InitializeOrder(PaymentDetails paymentDetails)
	{
	    var stockErrors = new List<string>();
	    var lockedStock = new List<(Product Product, int LockedQuantity)>();

	    using var transaction = await _context.Database.BeginTransactionAsync();
	    try
	    {
	        // Lock stock for each product
	        foreach (var cartItem in paymentDetails.CartItems)
	        {
	            var product = await _context.Product.FindAsync(cartItem.ProductId);
	            if (product == null)
	            {
	                stockErrors.Add($"Product with ID {cartItem.ProductId} does not exist.");
	                continue;
	            }

	            if (product.Stock < cartItem.Quantity)
	            {
	                stockErrors.Add(
	                    $"Product {product.Name} does not have enough stock. Available: {product.Stock}, Requested: {cartItem.Quantity}.");
	                continue;
	            }

	            // Lock stock
	            product.Stock -= cartItem.Quantity;
	            lockedStock.Add((product, cartItem.Quantity));
	        }

	        // Return errors if stock validation fails
	        if (stockErrors.Any())
	        {
	            await transaction.RollbackAsync();
	            return new PaymentResponse
	            {
	                IsSuccessful = false,
	                ErrorBody = string.Join(", ", stockErrors)
	            };
	        }

	        // Save locked stock changes
	        await _context.SaveChangesAsync();

	        // Prepare Barion API request
	        var barionRequest = new
	        {
	            POSKey = "d4443f71-51ed-4c9f-8bd3-b22f4e4ce4bb",
	            PaymentType = "Immediate",
	            PaymentRequestId = Guid.NewGuid().ToString(),
	            FundingSources = new[] { "All" },
	            OrderNumber = "Order_01",
	            RedirectUrl = "https://example.com/",
	            CallbackUrl = "", // Not used in this implementation
	            Currency = "HUF",
	            Locale = "en-US",
	            GuestCheckOut = true,
	            PaymentWindow = "00:01:00", // 1 minute
	            Transactions = new[]
	            {
	                new
	                {
	                    POSTransactionId = Guid.NewGuid().ToString(),
	                    Payee = "s.nandor01@gmail.com",
	                    Total = paymentDetails.TotalAmount,
	                    Comment = "Order Payment",
	                    Items = paymentDetails.CartItems.Select(cartItem => new
	                    {
	                        Name = _context.Product.First(p => p.Id == cartItem.ProductId).Name,
	                        Description = _context.Product.First(p => p.Id == cartItem.ProductId).Description,
	                        Quantity = cartItem.Quantity,
	                        Unit = "db",
	                        UnitPrice = _context.Product.First(p => p.Id == cartItem.ProductId).Price,
	                        ItemTotal = cartItem.Quantity *
	                                    _context.Product.First(p => p.Id == cartItem.ProductId).Price,
	                        SKU = _context.Product.First(p => p.Id == cartItem.ProductId).SerialNumber
	                    }).ToList()
	                }
	            }
	        };

	        // Call Barion API
	        using var httpClient = new HttpClient();
	        var response =
	            await httpClient.PostAsJsonAsync("https://api.dev.barion.com/v2/payment/start", barionRequest);

	        if (!response.IsSuccessStatusCode)
	        {
	            await transaction.RollbackAsync();
	            return new PaymentResponse
	            {
	                IsSuccessful = false,
	                ErrorBody = $"Barion API call failed: {await response.Content.ReadAsStringAsync()}"
	            };
	        }

	        // Parse Barion response
	        var barionResponse = await response.Content.ReadFromJsonAsync<BarionPaymentResponse>();
	        if (barionResponse == null || string.IsNullOrEmpty(barionResponse.GatewayUrl))
	        {
	            await transaction.RollbackAsync();
	            return new PaymentResponse
	            {
	                IsSuccessful = false,
	                ErrorBody = "Failed to parse Barion response or missing RedirectUrl."
	            };
	        }

	        // Commit transaction before starting the manual callback timer
	        await transaction.CommitAsync();

	        // Start the manual callback timer
	        StartPaymentStatusCheck(barionResponse.PaymentId, lockedStock, paymentDetails);

	        return new PaymentResponse
	        {
	            IsSuccessful = true,
	            PaymentUrl = barionResponse.GatewayUrl
	        };
	    }
	    catch (Exception ex)
	    {
	        await transaction.RollbackAsync();
	        return new PaymentResponse
	        {
	            IsSuccessful = false,
	            ErrorBody = $"An unexpected error occurred: {ex.Message}"
	        };
	    }
	}

	
	private void StartPaymentStatusCheck(
	    string paymentId,
	    List<(Product Product, int LockedQuantity)> lockedStock,
	    PaymentDetails paymentDetails)
	{
	    Task.Run(async () =>
	    {
	        using var scope = _serviceProvider.CreateScope();
	        var scopedContext = scope.ServiceProvider.GetRequiredService<DataContext>();

	        try
	        {
	            Console.WriteLine("Before 70 sec");
	            await Task.Delay(TimeSpan.FromSeconds(70));
	            Console.WriteLine("After 70 sec");
	            
	            var orderExists = scopedContext.Order.Any(o => o.PaymentId == paymentId);
	            if (orderExists)
	            {
		            Console.WriteLine($"Order with PaymentId {paymentId} already processed. Skipping duplicate processing.");
		            return;
	            }

	            // Query Barion for payment status
	            using var httpClient = new HttpClient();
	            BarionPaymentStateResponse? response = null;

	            try
	            {
	                response = await httpClient.GetFromJsonAsync<BarionPaymentStateResponse>(
	                    $"https://api.dev.barion.com/v2/Payment/GetPaymentState?POSKey=d4443f71-51ed-4c9f-8bd3-b22f4e4ce4bb&PaymentId={paymentId}");

	                if (response == null)
	                {
	                    Console.WriteLine("Response was null. The server might not have returned a valid JSON response.");
	                }
	            }
	            catch (Exception ex)
	            {
	                Console.WriteLine($"Error querying Barion API: {ex.Message}");
	            }

	            // Restore stock if the payment was not successful
	            if (response == null || response.Status != "Succeeded")
	            {
	                Console.WriteLine("Payment not successful or no valid response received. Restoring stock...");

	                foreach (var (product, lockedQuantity) in lockedStock)
	                {
	                    var dbProduct = await scopedContext.Product.FindAsync(product.Id);
	                    if (dbProduct != null)
	                    {
	                        dbProduct.Stock += lockedQuantity;
	                    }
	                }

	                await scopedContext.SaveChangesAsync();
	                Console.WriteLine("Stock restored successfully.");
	            }
	            else
	            {
	                Console.WriteLine("Payment succeeded. Creating order, invoice, and shipping address, and order items...");

	                // Create a new Invoice
	                var invoice = new Invoice
	                {
	                    CustomerName = paymentDetails.CustomerInfo.Name,
	                    CustomerEmail = paymentDetails.CustomerInfo.EmailAddress,
	                    CustomerPhoneNumber = paymentDetails.CustomerInfo.PhoneNumber,
	                    CustomerZipCode = paymentDetails.CustomerInfo.ZipCode,
	                    CustomerCountry = paymentDetails.CustomerInfo.Country,
	                    CustomerCity = paymentDetails.CustomerInfo.City,
	                    CustomerStreet = paymentDetails.CustomerInfo.Street,
	                    CreationDate = DateTime.UtcNow,
	                    PaymentMethodId = 1,
	                };

	                scopedContext.Invoice.Add(invoice);
	                await scopedContext.SaveChangesAsync();

	                // Create a new Shipping Address
	                var shippingAddress = new ShippingAddress
	                {
	                    Name = paymentDetails.CustomerInfo.Name,
	                    Email = paymentDetails.CustomerInfo.EmailAddress,
	                    PhoneNumber = paymentDetails.CustomerInfo.PhoneNumber,
	                    ZipCode = paymentDetails.CustomerInfo.ZipCode,
	                    Country = paymentDetails.CustomerInfo.Country,
	                    City = paymentDetails.CustomerInfo.City,
	                    Street = paymentDetails.CustomerInfo.Street
	                };

	                scopedContext.ShippingAddress.Add(shippingAddress);
	                await scopedContext.SaveChangesAsync();

	                // Create the Order
	                var order = new Order
	                {
	                    Status = "Paid",
	                    OrderDate = DateTime.UtcNow,
	                    PaymentId = paymentId,
	                    ShippingAddressId = shippingAddress.Id,
	                    InvoiceId = invoice.Id,
	                    OrderItems = paymentDetails.CartItems.Select(cartItem => new OrderItem
	                    {
	                        ProductId = cartItem.ProductId,
	                        Amount = cartItem.Quantity,
	                        OrderedPrice = scopedContext.Product.First(p => p.Id == cartItem.ProductId).Price
	                    }).ToList()
	                };

	                scopedContext.Order.Add(order);
	                await scopedContext.SaveChangesAsync();
	                
	                Console.WriteLine($"Order with ID {order.Id} created successfully with {paymentDetails.CartItems.Count} items.");
	            }
	        }
	        catch (Exception ex)
	        {
	            Console.WriteLine($"Error in payment status check task: {ex.Message}");
	        }
	    });
	}


}