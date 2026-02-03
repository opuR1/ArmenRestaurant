using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Ingredient : BaseEntity
    {
        public int IngredientID { get; set; }

        [Required(ErrorMessage = "Название ингредиента обязательно")]
        public string IngredientName { get; set; }

        [Range(0, 10000, ErrorMessage = "Количество не может быть отрицательным")]
        public float StockQuantity { get; set; }

        [Required(ErrorMessage = "Укажите единицу измерения")]
        [MaxLength(20)]
        public string Unit { get; set; } 
    }
}
