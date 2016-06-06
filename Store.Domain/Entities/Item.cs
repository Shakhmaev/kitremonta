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
        public string Type { get; set; }

        [Display(Name = "Тип")]
        [HiddenInput]
        [Required(ErrorMessage = "Пожалуйста, введите тип")]
        public string ItemType { get; set; }

        [Display(Name = "Ед. изм. цены")]
        [Required(ErrorMessage = "Пожалуйста, введите единицу измерения")]
        public string PriceUnit { get; set; }
        [Display(Name = "Цена (руб)")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Пожалуйста, введите положительное значение для цены")]
        public int Price { get; set; }
        
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

        public virtual ICollection<Property> props { get; set; }
        public virtual ICollection<PropValue> propValues { get; set; }
        public virtual ICollection<Supplier> Suppliers { get; set; }
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

    public class Property
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string PropName { get; set; }
        public bool IsInFilter { get; set; }
        [Required]
        public virtual ICollection<PropValue> Values { get; set; }
    }

    public class PropValue
    {
        [Required]
        public int Id { get; set; }
        public string Val { get; set; }
        public virtual ICollection<Item> Items { get; set; }
        public virtual Property Prop { get; set; }
    }

    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Item> Items { get; set; }
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
        public string Application { get; set; }
        public int? ParentID { get; set; }
        public virtual Category Parent { get; set; }
        public virtual Photo Photo { get; set; }
        public virtual ICollection<Photo> ExtraPhotos { get; set; }
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
