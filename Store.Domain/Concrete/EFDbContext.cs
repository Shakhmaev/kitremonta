using System.Data.Entity;
using Store.Domain.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Mvc;

namespace Store.Domain.Concrete
{
    public class EFDbContext : IdentityDbContext<AppUser>
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartLine> Lines { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public EFDbContext()
            : base("EFDbContext")
        {
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new Category.Mapping());
            modelBuilder.Configurations.Add(new Item.Mapping());
            modelBuilder.Configurations.Add(new Cart.Mapping());
        }
    }
}
