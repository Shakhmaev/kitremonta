namespace Store.Domain.OrderMigrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class OrderConfig : DbMigrationsConfiguration<Store.Domain.Concrete.OrderDbContext>
    {
        public OrderConfig()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "Store.Domain.Concrete.OrderDbContext";
            MigrationsDirectory = @"OrderMigrations";
        }

        protected override void Seed(Store.Domain.Concrete.OrderDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
