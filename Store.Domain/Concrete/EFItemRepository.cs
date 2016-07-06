using System.Collections.Generic;
using Store.Domain.Entities;
using Store.Domain.Abstract;
using System.Web.Mvc;
using System.Linq;
using System;
using System.Data.Entity;
using System.IO;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Store.Domain.Concrete
{
    public class EFItemRepository:IItemRepository, IDisposable
    {
        EFDbContext context;

        public EFItemRepository(EFDbContext cont)
        {
            context = cont;
        }

        
        public IQueryable<Item> Items
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Items; }
        }


        public IQueryable<IdentityRole> Roles
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Roles; }
        }


        public IQueryable<AppUser> Users
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Users; }
        }


        public IQueryable<Category> Categories
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Categories; }
        }


        public IQueryable<Photo> Photos
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Photos; }
        }


        public IQueryable<Supplier> Suppliers
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Suppliers; }
        }

        public IQueryable<Property> Properties
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.Properties; }
        }

        public IQueryable<PropValue> PropValues
        {
            [OutputCache(Duration = int.MaxValue)]
            get { return context.PropValues; }
        }

        public async Task SaveItemAsync(Item item)
        {
            if (item.Id==0)
            {
                foreach (var ct in item.ParentCategories)
                {
                    Category cat = await SubCategoryGetOrCreateAsync(ct);
                    cat.items.Add(item);
                }
            }
            else
            {
                Item dbEntry = await context.Items.FindAsync(item.Id);
                if (dbEntry != null)
                {
                    dbEntry.Name = item.Name;
                    dbEntry.Description = item.Description;
                    dbEntry.Price = item.Price;
                    dbEntry.IsHot = item.IsHot;

                    foreach (var ctg in dbEntry.ParentCategories)
                    {
                        var CatExists = await context.Categories.FirstOrDefaultAsync(x => x.CategoryId == ctg.CategoryId);
                        if (CatExists != null)
                        {
                            ctg.items.Remove(dbEntry);
                            CatExists.items.Add(dbEntry);
                        }
                        else
                        {
                            Category cat = await SubCategoryGetOrCreateAsync(ctg);
                            cat.items = new List<Item>();
                            ctg.items.Remove(dbEntry);
                            cat.items.Add(dbEntry);
                        }
                    }
                    dbEntry.DiscountPercent = item.DiscountPercent;
                }
            }
            await context.SaveChangesAsync();
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

            List<Property> properties = item.props.ToList();
            List<PropValue> propVals = item.propValues.ToList();

            item.props.Clear();
            item.propValues.Clear();

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
                            it = context.Items.FirstOrDefault(x => x.Brand == item.Brand && x.Country == item.Country && x.ItemType == item.ItemType &&
                                x.Name == item.Name && x.Type == item.Type && x.Description == item.Description);
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
            AddPropsAndValuesToItem(properties, item);
        }

        public Property PropertyGetOrCreate(Property prop)
        {
            var property = Properties.FirstOrDefault(x => x.PropName == prop.PropName);
            if (property == null)
            {
                property = new Property() { PropName = prop.PropName,
                    Values = new List<PropValue>(), Items= new List<Item>(), IsInFilter = prop.IsInFilter };

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
                        var propVal = pr.Values.FirstOrDefault(x => x.Val == pv.Val);
                        if (propVal != null)
                        {
                            if (!propVal.Items.Any(x => x.article == item.article))
                            {
                                propVal.Items.Add(item);
                                pr.Items.Add(item);
                            }
                        }
                        else
                        {
                            pv.Items.Add(item);
                            pr.Values.Add(pv);
                            pr.Items.Add(item);
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        public Supplier SupplierGetOrCreate(string name)
        {
            Supplier supp = Suppliers.FirstOrDefault(x => x.Name == name);
            if (supp == null)
            {
                supp = context.Suppliers.Add(new Supplier()
                {
                    Name = name
                });
                SaveChanges();
            }
            return supp;
        }

        public async Task<Category> SubCategoryGetOrCreateAsync(Category category)
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
                    categ = Categories.FirstOrDefault(x => (x.Name == category.Name || x.Name.Contains(category.Name+"_"))
                        && x.ParentID == category.ParentID); 
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
                await SaveChangesAsync();
            }
            return categ;
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
                    categ = Categories.FirstOrDefault(x => (x.Name == category.Name || x.Name.Contains(category.Name + "_"))
                        && x.ParentID == category.ParentID);
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
            return categ;
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
                if (supplier != "unknown")
                {
                    Supplier sup = SupplierGetOrCreate(supplier);
                    sup.Items.Add(item);
                }
                SaveChanges();
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
        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }

        public async Task<Item> DeleteItemAsync(int Id)
        {
            Item dbEntry = await context.Items.FindAsync(Id);
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


        public async Task<IEnumerable<Category>> FindInDescendantCountriesAsync(Category ctg)
        { 
            return await Task.Run(()=> ctg.SubCategories.Where(x => x.Type == "Country"));
        }

        public async Task<IEnumerable<Category>> FindInDescendantBrandsAsync(Category ctg)
        {
            IEnumerable<Category> categories = new List<Category>();

            await Task.Run(() =>
            {
                foreach (var sub in ctg.SubCategories)
                {
                    categories = categories.Union(sub.SubCategories.Where(x => x.Type == "Brand"));
                }
            });

            return categories;
        }

        public async Task<IEnumerable<Category>> GetChildCollectionsAvoidLowerSubsAsync(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0)
            {
                IEnumerable<Category> collections = new List<Category>();
                foreach (var category in ctg.SubCategories)
                {
                    if (category.Type == "show_collections" || category.Type=="Collection")
                    {
                        (collections as List<Category>).Add(category);
                    }
                    else if (category.Type == "Country" || category.Type == "Brand")
                    {
                        foreach (var sub in category.SubCategories)
                        {
                            var subcols = await GetChildCollectionsAvoidLowerSubsAsync(sub);
                            foreach (var subcol in subcols)
                            {
                                (collections as List<Category>).Add(subcol);
                            }
                        }
                    }
                }
                    return collections.Distinct();
            }
            return new List<Category>();
        }

        public async Task<IEnumerable<Category>> GetDescendantCollectionsAsync(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0)
            {
                IEnumerable<Category> collections = ctg.SubCategories.Where(x => x.Type == "Collection").ToList();
                foreach (var sub in ctg.SubCategories)
                {
                    var subdescs = await GetDescendantCollectionsAsync(sub);
                    foreach (var sd in subdescs)
                    {
                        (collections as List<Category>).Add(sd);
                    }
                }
                return collections.Distinct();
            }
            return new List<Category>();
        }

        public async Task<IEnumerable<Category>> GetBrandsByCountryAsync(string country)
        {
            Category countryctg = await Categories.FirstOrDefaultAsync(x => x.Type == "Country"
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
