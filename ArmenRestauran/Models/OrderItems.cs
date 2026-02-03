using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class OrderItem : BaseEntity
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int ItemID { get; set; }

        [Range(1, 100, ErrorMessage = "Количество порций: от 1 до 100")]
        public int Quantity { get; set; }

        [Required]
        public decimal Subtotal { get; set; }
    }
}
