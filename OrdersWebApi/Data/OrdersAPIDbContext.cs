using Microsoft.EntityFrameworkCore;
using OrdersWebApi.Models;

namespace OrdersWebApi.Data
{
    public class OrdersAPIDbContext: DbContext
    {
        public OrdersAPIDbContext(DbContextOptions options): base (options) 
        {

        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }

}
