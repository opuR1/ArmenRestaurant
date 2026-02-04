using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class TableStatusDTO
    {
        public int TableID { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public int ActiveOrderCount { get; set; }
        public DateTime LastOrderTime { get; set; } 
        public string OrderStatuses { get; set; }
    }
}
