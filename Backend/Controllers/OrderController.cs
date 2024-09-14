using Backend.Dal.Interfaces;
using Backend.Dtos.Orders;
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

        // POST: api/order
        // Creates a new order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdOrder = await _orderRepository.CreateOrderAsync(createOrderDto);
            return Ok(createdOrder);
        }

        // GET: api/order?pageNumber=1&pageSize=10&sortBy=date&status=Shipped&startDate=2023-09-01&endDate=2023-09-10
        // Gets a paginated list of orders with sorting and filtering options
        [HttpGet]
        public async Task<IActionResult> GetOrders([FromQuery] int pageNumber = 1, int pageSize = 10, string sortBy = "date", string status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var orders = await _orderRepository.GetOrdersAsync(pageNumber, pageSize, sortBy, status, startDate, endDate);
            return Ok(orders);
        }
        
        // PUT: api/order/{id}/status
        // Updates the status of an existing order
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _orderRepository.UpdateOrderStatusAsync(id, updateOrderStatusDto);

            if (!updated)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
