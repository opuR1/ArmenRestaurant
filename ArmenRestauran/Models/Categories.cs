using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Category : BaseEntity
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Название категории обязательно")]
        [MaxLength(100)]
        public string CategoryName { get; set; }
    }
}
