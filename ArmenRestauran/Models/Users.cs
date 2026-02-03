using ArmenRestauran.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmenRestauran.Models
{
    public class User : BaseEntity
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Выберите роль")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [MaxLength(50)]
        public string SurName { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [MaxLength(25)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Логин обязателен")]
        [MinLength(4, ErrorMessage = "Логин слишком короткий")]
        [MaxLength(50)]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; }

        public string RoleName => RoleID switch
        {
            1 => "Администратор",
            2 => "Официант",
            3 => "Менеджер",
            4 => "Клиент",
            _ => "Неизвестно"
        };

        public string FullName => $"{LastName} {FirstName} {SurName}".Trim();
    }
}
