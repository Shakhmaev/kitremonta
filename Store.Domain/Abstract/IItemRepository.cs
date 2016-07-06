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
        IQueryable<Item> Items { get; }

        IQueryable<AppUser> Users { get; }

        IQueryable<IdentityRole> Roles { get; }

        IQueryable<Photo> Photos { get; }

        IQueryable<Category> Categories { get; }

        IQueryable<Supplier> Suppliers { get; }

        Task SaveItemAsync(Item item);
        Task<Category> SubCategoryGetOrCreateAsync(Category category);
        Category SubCategoryGetOrCreate(Category category);
        void SaveOrUpdateItemFromXlsOne(Item item, List<string[]> hierarchy, List<string[]> Names, IEnumerable<string> images);

        void UpdateCategoryFromXls(Category ctg, string image, List<string> extraImages);
        bool UpdateItemPriceFromXls(string article, string price, string supplier = "unknown");
        Task<Item> DeleteItemAsync(int Id);

        void SaveChanges();
        Task SaveChangesAsync();

        void DeleteExtraPhoto(int Id, int PhotoId);

        string GetCategoryImageUrl(int id);

        string GetCategoryImageMiniUrl(int id);


        Task<IEnumerable<Category>> GetBrandsByCountryAsync(string country);

        Task<IEnumerable<Category>> GetDescendantCollectionsAsync(Category ctg);

        Task<IEnumerable<Category>> GetChildCollectionsAvoidLowerSubsAsync(Category ctg);

        Task<IEnumerable<Category>> FindInDescendantBrandsAsync(Category ctg);

        Task<IEnumerable<Category>> FindInDescendantCountriesAsync(Category ctg);
    }
}
