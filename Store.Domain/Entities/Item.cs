using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;


namespace Store.Domain.Entities
{
    public class Item
    {
        [Required]
        [HiddenInput(DisplayValue=false)]
        public int Id { get; set; }

        [Display(Name="Название")]
        [Required(ErrorMessage="Пожалуйста, введите название товара")]
        public string Name { get; set; }

        [Display(Name = "Артикул")]
        [Required(ErrorMessage = "Пожалуйста, введите артикул")]
        public string article { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name="Описание")]
        public string Description { get; set; }

        [Display(Name = "Брэнд")]
        [Required(ErrorMessage = "Пожалуйста, введите название брэнда")]
        public string Brand { get; set; }

        [Display(Name = "Страна")]
        [Required(ErrorMessage = "Пожалуйста, введите страну")]
        public string Country { get; set; }

        [Display(Name = "Тип")]
        [Required(ErrorMessage = "Пожалуйста, введите тип")]
        public string Type { get; set; }

        [Display(Name = "Цвет")]
        [Required(ErrorMessage = "Пожалуйста, введите цвет")]
        public string Color { get; set; }

        [Display(Name = "Поверхность")]
        [Required(ErrorMessage = "Пожалуйста, введите тип поверхности")]
        public string Surface { get; set; }

        [Display(Name = "Назначение")]
        [Required(ErrorMessage = "Пожалуйста, введите назначение")]
        public string Purpose { get; set; }


        [Display(Name = "Применение")]
        [Required(ErrorMessage = "Пожалуйста, введите применение")]
        public string Application { get; set; }

        [Display(Name = "Рисунок")]
        [Required(ErrorMessage = "Пожалуйста, введите тип рисунка")]
        public string Picture { get; set; }

        [Display(Name = "Размер")]
        [Required(ErrorMessage = "Пожалуйста, введите размер")]
        public string Size { get; set; }


        [Display(Name = "Только упаковками")]
        [Required(ErrorMessage = "Укажите, товар продается только упаковками?")]
        public bool OnlyInPacks { get; set; }

        [Display(Name = "Кол-во в упаковке")]
        [Required(ErrorMessage = "Пожалуйста, введите количество")]
        public string CountInPack { get; set; }

        [Display(Name = "Ед. изм. цены")]
        [Required(ErrorMessage = "Пожалуйста, введите единицу измерения")]
        public string PriceUnit { get; set; }
        [Display(Name = "Цена (руб)")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Пожалуйста, введите положительное значение для цены")]
        public int Price { get; set; }

        [HiddenInput]
        [Required]
        public string ItemType { get; set; }
        [HiddenInput]
        public double m2 { get; set; }
        [HiddenInput]
        public int sht { get; set; }
        [HiddenInput]
        public bool PriceForM2 { get; set; }
        [HiddenInput]
        public double SizeInM2 { get; set; }
        

        [Display(Name = "Скидка (%)")]
        [Range(0, 100, ErrorMessage = "Пожалуйста, введите правильное значение в процентах")]
        public int DiscountPercent { get; set; }
        public int CurrentPrice
        {
            get
            {
                return Price - (Price * DiscountPercent / 100);
            }
        }

        [Display(Name = "Горячее?")]
        [Required]
        public bool IsHot { get; set; }

        public byte[] ImageData { get; set; }

        public string ImageMimeType { get; set; }
        public virtual ICollection<Category> ParentCategories { get; set; }
        [HiddenInput]
        public virtual ICollection<Photo> Photos { get; set; }

        public class Mapping : EntityTypeConfiguration<Item>
        {
            public Mapping()
            {

            }
        }
    }

    public class Photo
    {
        public int PhotoId { get; set; }
        public byte[] ImageData { get; set; }
        public string ImageMimeType { get; set; }
        public string url { get; set; }
    }

    public class Category
    {
        [HiddenInput]
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public string Type { get; set; }
        public int? ParentID { get; set; }
        public virtual Category Parent { get; set; }
        public virtual Photo Photo { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }
        public virtual ICollection<Item> items { get; set; }

        public class Mapping : EntityTypeConfiguration<Category>
        {
            public Mapping() 
            {
                HasOptional(x => x.Parent).WithMany(x => x.SubCategories);
                HasMany(x => x.items).WithMany(x=>x.ParentCategories);
            }
        }
    }
}
