using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

using OrdersWebApi.Data;
using OrdersWebApi.Models;
using System.IO;
using System.Net.Http.Headers;

namespace OrdersWebApi.Controllers
{
    [Route("api/Order")]
    [ApiController]
    public class OrdersAPIController : ControllerBase
    {
        private readonly OrdersAPIDbContext _context;
        private readonly IHttpClientFactory _client;
        public OrdersAPIController(OrdersAPIDbContext context, IHttpClientFactory client)
        {
            _context = context;
            _client = client;
        }

        [HttpGet]
        public async Task<IEnumerable<Order>> Get()
        {
            return await _context.Orders.ToListAsync();
        }



        [HttpGet("Id")]
        [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int Id)
        {
            var Order = await _context.Orders.FindAsync(Id);
            return Order == null ? NotFound() : Ok(Order);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Order NewOrder)
        {
            NewOrder.ReadyToPickUp = false;
            NewOrder.Done = null;
            _context.Orders.Add(NewOrder);
            await _context.SaveChangesAsync();
            //return CreatedAtAction("api/Orders/Post", NewOrder);
            return Ok();
        }

        [HttpPut("id")]
        public async Task<IActionResult> Post(int id, Order UpdatedOrder)
        {
            var OrderToUpdate = await _context.Orders.FindAsync(id);
            if (OrderToUpdate == null)
            {
                return NotFound();
            }
            OrderToUpdate.NeedsDelivery = UpdatedOrder.NeedsDelivery;
            //OrderToUpdate.OrderDone = UpdatedOrder.OrderDone;
            OrderToUpdate.Created = UpdatedOrder.Created;
            OrderToUpdate.Done = UpdatedOrder.Done;
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var OrderToDelete = await _context.Orders.FindAsync(id);
            if (OrderToDelete == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(OrderToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("UpdateNeedsDelivery/id")]
        public async Task<IActionResult> UpdateNeedsDelivery(int id, bool NewNeedsDelivery)
        {
            var OrderToUpdate = await _context.Orders.FindAsync(id);
            if (OrderToUpdate == null)
            {
                return NotFound();
            }
            OrderToUpdate.NeedsDelivery = NewNeedsDelivery;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("UpdateCreated/id")]
        public async Task<IActionResult> UpdateCreated(int id, DateTime NewCreated)
        {
            var OrderToUpdate = await _context.Orders.FindAsync(id);
            if (OrderToUpdate == null)
            {
                return NotFound();
            }
            OrderToUpdate.Created = NewCreated;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("UpdateDone/id")]
        public async Task<IActionResult> UpdateDone(int id, DateTime NewDone)
        {
            var OrderToUpdate = await _context.Orders.FindAsync(id);
            if (OrderToUpdate == null)
            {
                return NotFound();
            }
            OrderToUpdate.Done = NewDone;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //тут будет запрос на изменение заказа (put для поля ReadyToBePickedUp)
        //если ReadyToBePickedUp == true - заказ подтвержден, и его нельзя изменить
        //В самом методе происходит отпрака списка OrderItem на апи каталога
        //В нем есть метод для проверки этого списка и уменьшения количества товаров в каталоге
        //Если результат ok - заказ подтвержлен, ели нет - нужно изменить заказ

        //вобще зачем давать возможность отправить чтото, если мы меняем bool с 0 на 1, а обратно не можем
        [HttpPatch("UpdateReadyToPickUp/id")]
        public async Task<IActionResult> UpdateReadyToPickUp(int Id)
        {
            var OrderToUpdate = await _context.Orders.FindAsync(Id);
            if (OrderToUpdate == null)
            {
                return NotFound($"No OrderItem with id = {Id}");
            }
            if (OrderToUpdate.ReadyToPickUp == true)
            {
                return Conflict($"Order is already set, use HttpDelete if needed to delete the order");
            }
            //для начала соберем все айтемы из этого заказа 
            var Items = _context.OrderItems.Where(x => x.OrderId == Id).ToList();
            //Вызов Клиента для общения с API
            var httpClient = _client.CreateClient("Catalogue");
            var Request = new HttpRequestMessage(HttpMethod.Post, $"api/Item/CheckItemQuantity");
            Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Request.Content = JsonContent.Create(Items);
            // ожидаем ответ
            var Responce = await httpClient.SendAsync(Request);
            // если ответ положительный - подтверждаем заказ
            if (Responce.IsSuccessStatusCode)
            {
                OrderToUpdate.ReadyToPickUp = true;
                await _context.SaveChangesAsync();
                return Ok("Ok");
            }
            //ели нет
            return Conflict(Responce.Content.ReadFromJsonAsync<List<OrderItem>>().Result);
        }

        [HttpGet("GetAllItemsFromOrder/Id")]
        public async Task<IActionResult> GetAllItemsFromOrder(int Id)
        {
            var Order = await _context.Orders.FindAsync(Id);
            if (Order == null)
            {
                return NotFound($"No Order with id = {Id}");
            }
            var OrderItems = _context.OrderItems.Where(x => x.OrderId == Id).ToList();
            var httpClient = _client.CreateClient("Catalogue");
            HttpRequestMessage GetItemRequest;
            HttpResponseMessage GetItemResponce;
            List<ItemDTO> ResultItems= new List<ItemDTO>();
            foreach (OrderItem orderItem in OrderItems)
            {
                GetItemRequest = new HttpRequestMessage(HttpMethod.Get, $"api/Item/id?id={orderItem.ItemId}");
                GetItemResponce = await httpClient.SendAsync(GetItemRequest);
                if (GetItemResponce.IsSuccessStatusCode)
                {
                    var Item = await GetItemResponce.Content.ReadFromJsonAsync<ItemDTO>();
                    //если к-во итемов в запросе больше чем в каталоге - аборт оперэйшн
                    ResultItems.Add(Item);
                }
                else
                {
                    return NotFound($"No Item with id{orderItem.ItemId}");
                }
            }
            return Ok(ResultItems);
        }


    }
}
