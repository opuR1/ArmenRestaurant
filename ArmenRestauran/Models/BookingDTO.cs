using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class BookingDTO
    {
        public int BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public string CustomerLogin { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; }
        public int GuestCount { get; set; }
        public int UserID { get; set; }
        public int OrderID { get; set; }
    }
}
