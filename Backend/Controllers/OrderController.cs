using Backend.Dal.Interfaces;
using Backend.Dtos.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

		// GET: api/order?pageNumber=1&pageSize=10&sortBy=date&status=Shipped&startDate=2023-09-01&endDate=2023-09-10
		// Gets a paginated list of orders with sorting and filtering options
		[HttpGet]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetOrders(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "date", 
            [FromQuery] string sortDirection = "asc", 
            [FromQuery] string status = null,
			[FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
		{
			var orders = await _orderRepository.GetOrdersAsync(pageNumber, pageSize, sortBy, sortDirection, status, startDate, endDate);
			return Ok(orders);
		}

        // GET: api/order/{id}
        // Gets a single order by id
        [HttpGet("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetOrderById(int id)
        {
			var order = await _orderRepository.GetOrderByIdAsync(id);
			return order != null ? Ok(order) : NotFound();
		}

		// POST: api/order
		// Creates a new order
		[HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var createdOrder = await _orderRepository.CreateOrderAsync(createOrderDto);
                return Ok(createdOrder);
            }
            catch (Exception ex)
            {
				return BadRequest(new { message = ex.Message });
			}
        }
       
        // PUT: api/order/{id}/status
        // Updates the status of an existing order
        [HttpPut("{id}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _orderRepository.UpdateOrderStatusAsync(id, updateOrderStatusDto);
            return updated ? NoContent() : NotFound();
        }

        // DELETE: api/order/{id}
        // Deletes an existing order
        [HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> DeleteOrder(int id)
        {
			var deleted = await _orderRepository.DeleteOrderAsync(id);
			return deleted ? NoContent() : NotFound();
		}
    }
}
