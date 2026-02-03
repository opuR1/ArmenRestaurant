using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Reservation : BaseEntity
    {
        public int ReservationID { get; set; }

        public int UserID { get; set; }

        public int TableID { get; set; }

        [Required(ErrorMessage = "Укажите дату и время")]
        public DateTime ReservationDate { get; set; }

        [Range(1, 50, ErrorMessage = "Количество гостей: от 1 до 50")]
        public int GuestCount { get; set; }

        [Required]
        public string Status { get; set; } = "Новое";
    }
}
