using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrdersWebApi.Models
{
    public class ItemDTO
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int Price { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public DateTime Created { get; set; }
    }
}
