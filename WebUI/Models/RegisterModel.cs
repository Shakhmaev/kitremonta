using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Store.WebUI.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage="Поле \"Имя\" обязательно")]
        [Display(Name="Имя")]
        [StringLength(15, ErrorMessage="Можно ввести до 15 символов в поле ")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Required(ErrorMessage="Поле \"e-mail\" обязательно.")]
        [Display(Name = "e-mail")]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "Некорректный формат адреса")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [Display(Name = "Пароль")]
        [StringLength(20,ErrorMessage="Пароль не должен быть длиннее 20 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage="Введите подтверждение пароля")]
        [Display(Name="Подтверждение пароля")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }


}