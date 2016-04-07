using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Store.Domain.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Store.Domain.Abstract
{
    public interface IItemRepository
    {
        IEnumerable<Item> Items { get; }

        IEnumerable<AppUser> Users { get; }

        IEnumerable<IdentityRole> Roles { get; }

        IEnumerable<Photo> Photos { get; }

        IEnumerable<Category> Categories { get; }

        void SaveItem(Item item);
        Category SubCategoryGetOrCreate(Category category);
        void SaveOrUpdateItemFromXlsOne(Item item, List<string[]> hierarchy, List<string[]> Names, IEnumerable<string> images);

        void UpdateCategoryFromXls(Category ctg, string image, List<string> extraImages);
        bool UpdateItemPriceFromXls(string article, string price);
        Item DeleteItem(int Id);

        void SaveChanges();

        void DeleteExtraPhoto(int Id, int PhotoId);

        string GetCategoryImageUrl(int id);

        string GetCategoryImageMiniUrl(int id);


        IEnumerable<Category> GetBrandsByCountry(string country);

        IEnumerable<Category> GetDescendantCollections(Category ctg);

        IEnumerable<Category> GetChildCollectionsAvoidLowerSubs(Category ctg);

        IEnumerable<Category> FindInDescendantBrands(Category ctg);

        IEnumerable<Category> FindInDescendantCountries(Category ctg);
    }
}
