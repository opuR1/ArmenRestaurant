using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Order : BaseEntity
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public int TableID { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
