using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Table : BaseEntity
    {
        public int TableID { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Номер стола должен быть положительным")]
        public int TableNumber { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Вместимость должна быть от 1 до 100")]
        public int Capacity { get; set; }
    }
}
