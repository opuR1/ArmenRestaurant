using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class Role : BaseEntity
    {
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Наименование роли обязательно")]
        [MaxLength(100)]
        public string RoleName { get; set; }
    }
}
