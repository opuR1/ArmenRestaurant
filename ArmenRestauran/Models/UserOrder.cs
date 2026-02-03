using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class UserOrder
    {
        public int ReservationID { get; set; }
        public int? OrderID { get; set; }
        public DateTime ReservationDate { get; set; }
        public int TableNumber { get; set; }
        public string ReservationStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public string ItemsSummary { get; set; }

        public bool CanCancel => ReservationStatus == "Подтверждена" && ReservationDate > DateTime.Now;
        public bool ShowStatusText => !CanCancel;
        public string StatusColor => ReservationStatus == "Отменена" ? "Red" :
                                     ReservationStatus == "Подтверждена" ? "DarkGreen" : "Gray";
    }
}
