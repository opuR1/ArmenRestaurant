using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class MenuItem : BaseEntity
    {
        public int ItemID { get; set; }

        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Название блюда обязательно")]
        [MaxLength(100)]
        public string ItemName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Цена должна быть больше нуля")]
        public decimal Price { get; set; }

        public string ImageName { get; set; }
    }
}
