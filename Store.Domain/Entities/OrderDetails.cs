using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Domain.Entities
{
    public class OrderDetails
    {
        [Required(ErrorMessage = "Пожалуйста, введите как мы можем к вам обращаться.")]
        [Display(Name = "Как мы можем к вам обращаться?")]
        [DataType(DataType.Text)]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Пожалуйста, введите номер телефона")]
        [Display(Name = "Телефон")]
        [Phone]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Пожалуйста, введите e-mail")]
        [Display(Name = "E-mail")]
        [EmailAddress]
        public string Email { get; set; }
        public Cart cart { get; set; }
    }
}
