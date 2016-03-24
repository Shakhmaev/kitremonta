using System.Collections.Generic;
using Store.Domain.Entities;
using Store.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Store.Domain.Concrete
{
    public class EFItemRepository:IItemRepository, IDisposable
    {
        EFDbContext context;

        public EFItemRepository(EFDbContext cont)
        {
            context = cont;
        }

        public IEnumerable<Item> Items
        {
            get { return context.Items; }
        }

        public IEnumerable<IdentityRole> Roles
        {
            get { return context.Roles; }
        }

        public IEnumerable<AppUser> Users
        {
            get { return context.Users; }
        }

        public IEnumerable<Category> Categories
        {
            get { return context.Categories; }
        }

        public IEnumerable<Photo> Photos
        {
            get { return context.Photos; }
        }

        public void SaveItem(Item item)
        {
            if (item.Id==0)
            {
                Category cat = SubCategoryGetOrCreate(item.SubCategory);
                cat.items.Add(item);
            }
            else
            {
                Item dbEntry = context.Items.Find(item.Id);
                if (dbEntry != null)
                {
                    dbEntry.Name = item.Name;
                    dbEntry.Description = item.Description;
                    dbEntry.Price = item.Price;
                    if (item.ImageData != null)
                    {
                        dbEntry.ImageData = item.ImageData;
                        dbEntry.ImageMimeType = item.ImageMimeType;
                    }
                    dbEntry.IsHot = item.IsHot;

                    var CatExists = context.Categories.FirstOrDefault(x=>x.CategoryId==item.SubCategory.CategoryId);
                    if (CatExists!=null)
                    {
                        dbEntry.SubCategory.items.Remove(dbEntry);
                        CatExists.items.Add(dbEntry);
                    }
                    else
                    {
                        Category cat = SubCategoryGetOrCreate(item.SubCategory);
                        cat.items = new List<Item>();
                        dbEntry.SubCategory.items.Remove(dbEntry);
                        cat.items.Add(dbEntry);
                    }
                    dbEntry.DiscountPercent = item.DiscountPercent;
                }
            }
            context.SaveChanges();
        }

        public void SaveOrUpdateItemFromXls(Item it, string category)
        {
            it.SubCategory.ParentID = context.Categories.First(x => x.Description == category).CategoryId;
            Category sub = SubCategoryGetOrCreate(it.SubCategory);
            var item = sub.items.FirstOrDefault(x => x.Name == it.Name);
            if (item != null) //update item
            {
                item.Name = it.Name;
                item.Brand = it.Brand;
            }
            else //create item
            {
                it.SubCategory = null;
                sub.items.Add(it);
            }
            context.SaveChanges();
        }

        public void MapItem(ref Item from, ref Item to)
        {
            to.Brand = from.Brand;
            to.Color = from.Color;
            to.CountInPack = from.CountInPack;
            to.Country = from.Country;
            to.Price = from.Price;
            to.PriceUnit = from.PriceUnit;
            to.Purpose = from.Purpose;
            to.Size = from.Size;
            to.Surface = from.Surface;
            to.Type = from.Type;
            to.Name = from.Name;
            to.article = from.article;
            to.OnlyInPacks = from.OnlyInPacks;
            to.Picture = from.Picture;
            to.SizeInM2 = from.SizeInM2;
            to.sht = from.sht;
            to.m2 = from.m2;
            to.ItemType = from.ItemType;
            to.PriceForM2 = from.PriceForM2;
        }

        public void SaveOrUpdateItemFromXlsOne(Item item, string[] hierarchy, string[] Names, IEnumerable<string> images)
        {
            Category current = null;
            var it = context.Items.FirstOrDefault(x => x.Name == item.Name);
            if (it != null) //update item
            {
                MapItem(ref item, ref it);
                
                if (images.Count() > 0)
                {
                    foreach (var img in images)
                    {
                        if (it.Photos.Any(x => x.url == img)) { }
                        else it.Photos.Add(new Photo { url = img });
                    }
                }
            }
            else
            {
                for (int i = 0; i < hierarchy.Count(); i++)
                {
                    string type = "";
                    switch (i)
                    {
                        case 0: type = "show_collections"; break;
                        case 1: type = "show_collections"; break;
                        case 2: type = "Country"; break;
                        case 3: type = "Brand"; break;
                        default: type = "Collection"; break;
                    }
                    Category category = new Category
                    {
                        Name = Names[i],
                        Description = hierarchy[i],
                        Type = type
                    };
                    if (current != null)
                    {
                        category.ParentID = current.CategoryId;
                    }
                    var cat = SubCategoryGetOrCreate(category);
                    if (i == hierarchy.Count() - 1)
                    {
                        if (images.Count() > 0)
                        {
                            item.Photos = new List<Photo>();
                            foreach (var img in images)
                            {
                                item.Photos.Add(new Photo { url = img });
                            }
                        }
                        cat.items.Add(item);
                    }
                    current = cat;
                }
            }
            SaveChanges();
        }

        public Category SubCategoryGetOrCreate(Category category)
        {
            Category categ = null;
            if (category.CategoryId != 0)
            {
                categ = context.Categories.FirstOrDefault(x => x.CategoryId == category.CategoryId);
            }
            else if (category.Name != "")
            {
                if (category.ParentID > 0)
                {
                    categ = context.Categories.FirstOrDefault(x => x.Name == category.Name && x.ParentID == category.ParentID);
                }
                else categ = context.Categories.FirstOrDefault(x => x.Name == category.Name);
            }
            if (categ == null)
            {
                categ = context.Categories.Add(category);
                categ.Parent = context.Categories.FirstOrDefault(i => i.CategoryId == category.ParentID);
                categ.items = new List<Item>();
                SaveChanges();
            }
            return context.Categories.First(x => x.Name == categ.Name && x.ParentID == category.ParentID);
        }

        public void UpdateCategoryFromXls(Category ctg, string image)
        {
            Category category = Categories.FirstOrDefault(x => x.Description == ctg.Description);
            if (category != null)
            {
                category.Text = ctg.Text;
                if (!String.IsNullOrEmpty(image))
                {
                    category.Photo = new Photo() { url = image };
                }
            }
            SaveChanges();
        }

        public void DeleteExtraPhoto(int Id, int PhotoId)
        {
            Photo photo = context.Photos.Find(PhotoId);
            context.Photos.Remove(photo);
            context.SaveChanges();
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public Item DeleteItem(int Id)
        {
            Item dbEntry = context.Items.Find(Id);
            if (dbEntry != null)
            {
                context.Items.Remove(dbEntry);
                context.SaveChanges();
            }
            return dbEntry;
        }

        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }
    }
}
