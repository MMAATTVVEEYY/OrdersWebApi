using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersWebApi.Data;
using OrdersWebApi.Models;

namespace OrdersWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderItemsAPIController : ControllerBase
    {
        private readonly OrdersAPIDbContext _context;
        private readonly IHttpClientFactory _client;
        public OrderItemsAPIController(OrdersAPIDbContext context, IHttpClientFactory client)
        {
            _context = context;
            _client = client;
        }

        /*[HttpGet("GetItems")]
        public async Task<IEnumerable<ItemDTO>> GetItems()
        {
            //return await _context.Orders.ToListAsync();
            // HttpResponseMessage response = await client.GetAsync(path);
            var httpClient = _client.CreateClient("Catalogue");
            var request = new HttpRequestMessage(HttpMethod.Get, "api/Item");
            var response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<IEnumerable<ItemDTO>>();
                return content;
            }
            else
            {
                return Enumerable.Empty<ItemDTO>();
            }
        }*/

        [HttpGet]
        public async Task<IEnumerable<OrderItem>> Get()
        {
            return await _context.OrderItems.ToListAsync();
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById(int Id)
        {
            var OrderItem = await _context.Orders.FindAsync(Id);
            return OrderItem == null ? NotFound() : Ok(OrderItem);
        }

        [HttpGet("OrderId")]
        public async Task<IActionResult> GetByOrderId(int OrderId)
        {
            var OrderItems = await _context.OrderItems.Where(x => x.OrderId == OrderId).ToListAsync();
            return OrderItems == null ? NotFound() : Ok(OrderItems);
        }

        [HttpGet("ItemId")]
        public async Task<IActionResult> GetByItemId(int ItemId)
        {
            var OrderItems = await _context.OrderItems.Where(x => x.ItemId == ItemId).ToListAsync();
            return OrderItems == null ? NotFound() : Ok(OrderItems);
        }


        [HttpPost]
        public async Task<IActionResult> Post(OrderItem NewOrderItem)
        {
            //проверяем, что есть такой заказ, в который мы хотим добавить айтем
            var Order = await _context.Orders.FindAsync(NewOrderItem.OrderId);
            if (Order == null)
            {
                return NotFound("No such order");
            }
            var OrderItem = await _context.OrderItems.FirstOrDefaultAsync(x => x.ItemId == NewOrderItem.ItemId && x.OrderId==NewOrderItem.OrderId);
            if (OrderItem != null) return Conflict($"Cannot add item with id={NewOrderItem.ItemId}, as it already is in the order. To add more use HttpPatch ");
            //Проверяем, что айтем не равен нулю
            if (NewOrderItem.ItemId == null)
            {
                return Conflict("IemId Field is required");
            }
            //Проверяем, что такой айтем впринипе есть в каталоге 
            var httpClient = _client.CreateClient("Catalogue");
            var GetItemRequest = new HttpRequestMessage(HttpMethod.Get, $"api/Item/id?id={NewOrderItem.ItemId}");
            var GetItemResponce = await httpClient.SendAsync(GetItemRequest);
            
            if (GetItemResponce.IsSuccessStatusCode)
            {
                var Item = await GetItemResponce.Content.ReadFromJsonAsync<ItemDTO>();
                //если к-во итемов в запросе больше чем в каталоге - аборт оперэйшн
                if (NewOrderItem.Quantity > Item.Quantity)
                {
                    return Conflict($"Items in stock = {Item.Quantity}, you request {NewOrderItem.Quantity}");
                }
                //Добавляем итем в ордер
                _context.OrderItems.Add(NewOrderItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                return Conflict("Failed to locate Item In Catalogue ");
            }
            return Ok();
        }

        [HttpDelete("Id")]
        public async Task<IActionResult> Delete(int Id)
        {
            var OrderItemToDelete = await _context.OrderItems.FindAsync(Id);
            if (OrderItemToDelete == null)
            {
                return NotFound();
            }
            _context.OrderItems.Remove(OrderItemToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("UpdateQuantity/id")]
        public async Task<IActionResult> UpdateQuantity(int Id, int NewQuantity)
        {
            if (NewQuantity < 0)
            {
                return BadRequest($"NewQuantity parameter = {NewQuantity} should be > 0");
            }
            var OrderItemToUpdate = await _context.OrderItems.FindAsync(Id);
            if (OrderItemToUpdate == null)
            {
                return NotFound($"No OrderItem with id = {Id}");
            }
            OrderItemToUpdate.Quantity = NewQuantity;
            await _context.SaveChangesAsync();
            return NoContent();
        }



        

    }
}
