using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class OrderDTO
    {
        public int OrderID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerLogin { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public int TableNumber { get; set; }
        public int? ReservationID { get; set; }
    }
}
