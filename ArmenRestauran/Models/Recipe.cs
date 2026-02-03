using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class RecipeItem : BaseEntity
    {
        public int RecipeID { get; set; }
        public int ItemID { get; set; }
        public int IngredientID { get; set; }

        [Required]
        [Range(0.001, 100, ErrorMessage = "Расход должен быть больше 0")]
        public float QuantityRequired { get; set; }
    }

    public class RecipeDetailDTO
    {
        public int RecipeID { get; set; }
        public int IngredientID { get; set; }
        public string IngredientName { get; set; }
        public float QuantityRequired { get; set; }
        public string Unit { get; set; }
    }
}
