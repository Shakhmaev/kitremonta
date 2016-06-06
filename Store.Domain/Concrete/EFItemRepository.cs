using System.Collections.Generic;
using Store.Domain.Entities;
using Store.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using System;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text.RegularExpressions;

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
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Items; }
        }

        
        public IEnumerable<IdentityRole> Roles
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Roles; }
        }

        
        public IEnumerable<AppUser> Users
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Users; }
        }

        
        public IEnumerable<Category> Categories
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Categories; }
        }

        
        public IEnumerable<Photo> Photos
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Photos; }
        }

        
        public IEnumerable<Supplier> Suppliers
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Suppliers; }
        }

        public IEnumerable<Property> Properties
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Properties; }
        }

        public IEnumerable<PropValue> PropValues
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.PropValues; }
        }

        public void SaveItem(Item item)
        {
            if (item.Id==0)
            {
                foreach (var ct in item.ParentCategories)
                {
                    Category cat = SubCategoryGetOrCreate(ct);
                    cat.items.Add(item);
                }
            }
            else
            {
                Item dbEntry = context.Items.Find(item.Id);
                if (dbEntry != null)
                {
                    dbEntry.Name = item.Name;
                    dbEntry.Description = item.Description;
                    dbEntry.Price = item.Price;
                    dbEntry.IsHot = item.IsHot;

                    foreach (var ctg in dbEntry.ParentCategories)
                    {
                        var CatExists = context.Categories.FirstOrDefault(x => x.CategoryId == ctg.CategoryId);
                        if (CatExists != null)
                        {
                            ctg.items.Remove(dbEntry);
                            CatExists.items.Add(dbEntry);
                        }
                        else
                        {
                            Category cat = SubCategoryGetOrCreate(ctg);
                            cat.items = new List<Item>();
                            ctg.items.Remove(dbEntry);
                            cat.items.Add(dbEntry);
                        }
                    }
                    dbEntry.DiscountPercent = item.DiscountPercent;
                }
            }
            context.SaveChanges();
        }

        public void MapItem(ref Item from, ref Item to)
        {
            to.Brand = from.Brand;
            to.Country = from.Country;
            to.Price = from.Price;
            to.PriceUnit = from.PriceUnit;
            to.Type = from.Type;
            to.Name = from.Name;
            to.article = from.article;
            to.ItemType = from.ItemType;
        }

        public void SaveOrUpdateItemFromXlsOne(Item item, List<string[]> hierarchy, List<string[]> Names, IEnumerable<string> images)
        {
            Category current = null;
            Item it = null;
            foreach (var prop in item.props)
            {
                var pr = PropertyGetOrCreate(prop);
            }

            for (int i = 0; i < hierarchy.Count; i++)
            {
                for (int j = 0; j < hierarchy[i].Count(); j++)
                {
                    Category category = new Category
                    {
                        Name = Names[i][j],
                        Description = hierarchy[i][j]
                    };
                    if (current != null)
                    {
                        category.ParentID = current.CategoryId;
                    }
                    var cat = SubCategoryGetOrCreate(category);
                    if (j == hierarchy[i].Count() - 1)
                    {
                        if (item.Id > 0) 
                        {
                            if (images.Count() > 0)
                            {
                                foreach (var img in images)
                                {
                                    if (item.Photos.Any(x => x.url == img)) { }
                                    else item.Photos.Add(new Photo { url = img });
                                }
                            }
                        } 
                        else
                        {
                            if (images.Count() > 0)
                            {
                                item.Photos = new List<Photo>();
                                foreach (var img in images)
                                {
                                    item.Photos.Add(new Photo { url = img });
                                }
                            }
                            it = context.Items.FirstOrDefault(x => x.article == item.article);
                            if (it != null)
                            {
                                item = it;
                            }
                        }
                        if (!cat.items.Contains(item))
                        {
                            cat.items.Add(item); //добавляем товар в категорию
                        }
                    }
                    current = cat;
                }
                current = null;
            }
            SaveChanges();

            AddPropsAndValuesToItem(item.props, item);
        }

        public Property PropertyGetOrCreate(Property prop)
        {
            var property = Properties.FirstOrDefault(x => x.PropName == prop.PropName);
            if (property == null)
            {
                property = new Property() { PropName = prop.PropName,
                    Values = new List<PropValue>(), IsInFilter = prop.IsInFilter };

                property = context.Properties.Add(property);
                context.SaveChanges();
            }
            return property;
        }

        public void AddPropsAndValuesToItem(IEnumerable<Property> props, Item item)
        {
            foreach (var prop in props)
            {
                if (prop.Values.Count > 0)
                {
                    var pr = PropertyGetOrCreate(prop);
                    foreach (var pv in prop.Values)
                    {
                        if (pr.Values.FirstOrDefault(x=>x.Val==pv.Val && x.Items.Any(i=>i.article==item.article))==null)
                        {
                            pv.Items.Add(item);
                            pr.Values.Add(pv);
                        }
                    }
                }
            }
        }

        public PropValue PropValueGetOrCreate(PropValue pv)
        {
            var propValue = context.PropValues.FirstOrDefault(x => x.Prop.PropName == pv.Prop.PropName && x.Val == pv.Val);
            if (propValue == null)
            {
                propValue = context.PropValues.Add(pv);
                context.SaveChanges();
            }
            return propValue;
        }

        public Supplier SupplierGetOrCreate(string name)
        {
            Supplier supp = Suppliers.FirstOrDefault(x => x.Name == name);
            if (supp == null)
            {
                context.Suppliers.Add(new Supplier()
                {
                    Name = name
                });
                SaveChanges();
                supp = Suppliers.FirstOrDefault(x => x.Name == name);
            }
            return supp;
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
                    categ = Categories.FirstOrDefault(x => x.Name == category.Name && x.ParentID == category.ParentID);
                    if (categ == null)
                    {
                        var categs = context.Categories.Where(x => x.Name.Contains(category.Name) && x.ParentID != category.ParentID);
                        if (categs.Count() > 0) // если с одним именем больше одного, тогда сделать имя уникальным
                        {
                            category.Name = category.Name + "_" + (categs.Count() + 1);
                        }
                    }
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

        public void UpdateCategoryFromXls(Category ctg, string image, List<string> extraImages)
        {
            var categories = Categories.Where(x => x.Description == ctg.Description);
            if (categories.Count() > 0)
            {
                foreach (var category in categories)
                {
                    category.Text = ctg.Text;
                    category.Application = ctg.Application;
                    if (!String.IsNullOrEmpty(image))
                    {
                        if (!String.IsNullOrEmpty(image))
                        {
                            if (category.Photo == null)
                                category.Photo = new Photo() { url = image };
                            else
                            {
                                category.Photo.url = image;
                            }
                        }
                        if (category.ExtraPhotos == null)
                        {
                            category.ExtraPhotos = new List<Photo>();
                            foreach (var pht in extraImages)
                            {
                                category.ExtraPhotos.Add(new Photo() { url = pht });
                            }
                        }
                        else
                        {   
                            foreach (var pht in extraImages)
                            {
                                if (!category.ExtraPhotos.Any(x => x.url == pht))
                                {
                                    category.ExtraPhotos.Add(new Photo() { url = pht });
                                }
                            }
                        }
                    }
                }
            }
            SaveChanges();
        }

        public bool UpdateItemPriceFromXls(string article, string price, string supplier = "unknown")
        {
            Item item = Items.FirstOrDefault(x => x.article == article);
            if (item != null)
            {
                item.Price = Convert.ToInt32(Math.Round(double.Parse(price)*1.15));
                context.SaveChanges();
                if (supplier != "unknown")
                {
                    Supplier sup = SupplierGetOrCreate(supplier);
                    sup.Items.Add(item);
                }
                return true;
            }
            else return false;
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

        public string GetCategoryImageUrl(int id)
        {
            Category ctg = Categories.FirstOrDefault(x => x.CategoryId == id);
            if (ctg != null && ctg.Photo != null)
            {
                return ctg.Photo.url;
            }
            else return null;
        }

        public string GetCategoryImageMiniUrl(int id)
        {
            Category ctg = Categories.FirstOrDefault(x => x.CategoryId == id);

            if (ctg != null && ctg.Photo != null)
            {
                return 
                         Path.GetDirectoryName(ctg.Photo.url) + "/"
                         + Path.GetFileNameWithoutExtension(ctg.Photo.url) + "-mini"
                         + Path.GetExtension(ctg.Photo.url);
            }
            else return null;
        }


        public IEnumerable<Category> FindInDescendantCountries(Category ctg)
        {
            IEnumerable<Category> categories = new List<Category>();

            categories = categories.Union(ctg.SubCategories).Where(x => x.Type == "Country");

            return categories;
        }

        public IEnumerable<Category> FindInDescendantBrands(Category ctg)
        {
            IEnumerable<Category> categories = new List<Category>();

            foreach (var sub in ctg.SubCategories)
            {
                categories = categories.Union(sub.SubCategories).Where(x => x.Type == "Brand");
            }

            return categories;
        }

        public List<Category> GetChildCollectionsAvoidLowerSubs(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0)
            {
                List<Category> collections = new List<Category>();
                foreach (var category in ctg.SubCategories)
                {
                    if (category.Type == "show_collections" || category.Type=="Collection")
                    {
                        collections.Add(category);
                    }
                    else if (category.Type == "Country" || category.Type == "Brand")
                    {
                        foreach (var sub in category.SubCategories)
                        {
                            collections = collections.Union(GetChildCollectionsAvoidLowerSubs(sub)).ToList();
                        }
                    }
                }
                return collections.Distinct().ToList();
            }
            return new List<Category>();
        }

        public IEnumerable<Category> GetDescendantCollections(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0)
            {
                IEnumerable<Category> collections = ctg.SubCategories.Where(x => x.Type == "Collection");
                foreach (var sub in ctg.SubCategories)
                {
                    collections = collections.Union(GetDescendantCollections(sub));
                }
                return collections.Distinct();
            }
            return new List<Category>();
        }

        public IEnumerable<Category> GetBrandsByCountry(string country)
        {
            Category countryctg = Categories.FirstOrDefault(x => x.Type == "Country"
                && x.Name == country);
            if (countryctg.SubCategories.Count() > 0)
            {
                return new List<Category>(countryctg.SubCategories.Where(x => x.Type == "Brand"));
            }
            else return null;
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
