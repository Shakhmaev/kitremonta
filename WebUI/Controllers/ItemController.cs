using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.Domain.Entities;
using Store.Domain.Abstract;
using Store.WebUI.Models;
using Newtonsoft.Json;
using MvcSiteMapProvider;
using MvcSiteMapProvider.Web.Mvc.Filters;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace Store.WebUI.Controllers
{
    [AllowAnonymous]
    public class ItemController : Controller
    {
        private IItemRepository repository;

        public ItemController(IItemRepository repo)
        {
            repository = repo;
        }

        public ViewResult Index()
        {
            IndexViewModel model;
            model = new IndexViewModel
            {
                DiscountItems = repository.Items.Where(x=>x.DiscountPercent>0).Take(12),
                LastItems = repository.Items.Reverse().Take(12)
            };

            return View(model);
        }

        public ActionResult ItemSummary(int id)
        {
            Item item = repository.Items.FirstOrDefault(x => x.Id == id);
            return PartialView("ItemSummary",item);
        }

        public ViewResult List(string category, int page = 1, int PageSize = 12)
        {
            ItemsListViewModel model = new ItemsListViewModel();
            model.Side = new List<KeyValuePair<string, IEnumerable<Category>>>();
            IEnumerable<string> Brands = new List<string>();
            IEnumerable<string> Countries = new List<string>();
            IEnumerable<string> Purposes = new List<string>();

            if (category != (string)null)
            {
                var ctg = repository.Categories.FirstOrDefault(x => x.Name == category);
                var items = GetItems(ctg);
                if (items.Count() == 0)
                {
                    model.HigherPrice = 0;
                    model.LowerPrice = 0;
                }
                else
                {
                    model.HigherPrice = items.Max(x => x.CurrentPrice);
                    model.LowerPrice = items.Min(x => x.CurrentPrice);
                    Brands = items.Select(x => x.Brand).Distinct();
                    Countries = items.Select(x => x.Country).Distinct();
                    Purposes = items.Select(x => x.Purpose).Distinct();
                }
                switch (ctg.Type)
                {
                    case "Brand":
                    case "show_collections":
                        {
                            model.categoryTypeMessage = "Коллекции";
                            model.Categories = GetChildCollectionsAvoidLowerSubs(ctg);
                            break;
                        }
                    case "Collection":
                    case "show_items":
                        {
                            model.categoryTypeMessage = "";
                            model.Categories = new List<Category>();
                            break;
                        }
                    case "Country":
                        {
                            model.categoryTypeMessage = "Бренды";
                            model.Categories = GetBrandsByCountry(ctg.Description);
                            break;
                        }
                    default: 
                        break;
                } 
                model.Side = GetSideCollection();
                model.currentctg = ctg;
            }
            else
            {
                model.HigherPrice = repository.Items.Max(x => x.CurrentPrice);
                model.LowerPrice = repository.Items.Min(x => x.CurrentPrice);
                Brands = repository.Items.Select(x => x.Brand).Distinct();
                Countries = repository.Items.Select(x => x.Country).Distinct();
                Purposes = repository.Items.Select(x => x.Purpose).Distinct();
                model.Categories = repository.Categories.Where(x => x.ParentID == null);
                model.categoryTypeMessage = "Разделы";
            }
            model.filters = new ItemsListFiltersModel(model.HigherPrice, model.LowerPrice, PageSize, "pricelowtohigh");

            model.filters.AllBrands = Brands;

            model.filters.SelectedBrands = new List<string>();

            model.filters.AllCountries = Countries;

            model.filters.SelectedCountries = new List<string>();

            model.filters.AllPurposes = Purposes;

            model.filters.SelectedPurposes = new List<string>();

            model.CurrentCategory = category;

            return View(model);
        }

        private List<KeyValuePair<string,IEnumerable<Category>>> GetSideCollection()
        {
            ISiteMap map = MvcSiteMapProvider.SiteMaps.Current;
            foreach (var ct in repository.Categories.Where(x => x.ParentID == null))
            {
                ISiteMapNode node = map.FindSiteMapNodeFromCurrentContext();
                ISiteMapNode ctnode = map.FindSiteMapNodeFromKey(ct.CategoryId.ToString());
                if (node
                .IsDescendantOf(ctnode) ||
                    node == ctnode)
                {
                    return new List<KeyValuePair<string, IEnumerable<Category>>>(){
                        new KeyValuePair<string,IEnumerable<Category>>("Countries",
                            new List<Category>(FindInDescendantCountries(ct))),
                        new KeyValuePair<string,IEnumerable<Category>>("Brands",
                            new List<Category>(FindInDescendantBrands(ct))),
                        new KeyValuePair<string,IEnumerable<Category>>("RootCategory",
                            new List<Category>(){ ct })
                        };

                }
            }
            return null;
        }

        private IEnumerable<Category> FindInDescendantCountries(Category ctg)
        {
            IEnumerable<Category> categories = new List<Category>();

                categories = categories.Union(ctg.SubCategories).Where(x=>x.Type=="Country");

            return categories;
        }

        private IEnumerable<Category> FindInDescendantBrands(Category ctg)
        {
            IEnumerable<Category> categories = new List<Category>();

            foreach (var sub in ctg.SubCategories)
            {
                    categories = categories.Union(sub.SubCategories).Where(x => x.Type == "Brand");
            }

            return categories;
        }

        private IEnumerable<Category> GetChildCollectionsAvoidLowerSubs(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0)
            {
                IEnumerable<Category> collections = new List<Category>();
                if (ctg.SubCategories.Any(x => x.Type == "Country" || x.Type == "application" || x.Type == "Brand"))
                {
                    foreach (var sub in ctg.SubCategories)
                    {
                        collections = collections.Union(GetChildCollectionsAvoidLowerSubs(sub));
                    }
                }
                else
                {
                    collections = collections.Union(ctg.SubCategories);
                }
                return collections.Distinct();
            }
            return new List<Category>();
        }

        private IEnumerable<Category> GetDescendantCollections(Category ctg)
        {
            if (ctg.SubCategories.Count() > 0) 
            { 
                IEnumerable<Category> collections = ctg.SubCategories.Where(x=>x.Type == "Collection");
                foreach (var sub in ctg.SubCategories)
                {
                   collections = collections.Union(GetDescendantCollections(sub));
                }
                return collections.Distinct();
            }
            return new List<Category>();
        }

        private IEnumerable<Category> GetBrandsByCountry(string country)
        {
            Category countryctg = repository.Categories.FirstOrDefault(x => x.Type == "Country" 
                && x.Description == country);
            if (countryctg.SubCategories.Count() > 0)
            {
                return new List<Category>(countryctg.SubCategories.Where(x => x.Type == "Brand"));
            }
            else return null;
        }

        public PartialViewResult LoadSummary(string category, ItemsListFiltersModel filters, int page = 1, string search = "all")
        {
            FilteredListViewModel fmodel = new FilteredListViewModel();

            IEnumerable<Item> items;

            var categ = repository.Categories.FirstOrDefault(x => x.Name == category);

            if (categ == null)
            {
                items = repository.Items;
            }
            else
            {
                items = GetItems(categ);
            }

            if (search != "all")
            {
                items = items.Where(x => x.Name.ToLower().Contains(search.ToLower()));
            }


            if (filters.SelectedBrands.Count() > 0)
            {
                if (filters.SelectedBrands.First() == "All")
                {
                    
                }
                else items = items.Where(x => filters.SelectedBrands.Contains(x.Brand));
            }

            if (filters.SelectedCountries.Count() > 0)
            {
                if (filters.SelectedCountries.First() == "All")
                {

                }
                else items = items.Where(x => filters.SelectedCountries.Contains(x.Country));
            }

            if (filters.SelectedPurposes.Count() > 0)
            {
                if (filters.SelectedPurposes.First() == "All")
                {

                }
                else items = items.Where(x => filters.SelectedPurposes.Contains(x.Purpose));
            }

            if (filters.WithDiscount)
            {
                items = items.Where(x => x.DiscountPercent > 0);
            }

            if (filters.Hot)
            {
                items = items.Where(x => x.IsHot);
            }

            switch (filters.SortBy) {
                case "pricehightolow":
                    {
                        items = items.OrderByDescending(x => x.Price);
                        break;
                    }
                case "pricelowtohigh":
                    {
                        items = items.OrderBy(x => x.Price);
                        break;
                    }
                case "name":
                    {
                        items = items.OrderBy(x => x.Name);
                        break;
                    }
                case "date":
                    {
                        items = items.OrderByDescending(x => x.Id);
                        break;
                    }
            }


            if ((page - 1) * filters.PageSize > items
                    .Where(x => x.CurrentPrice >= filters.LowerPrice && x.CurrentPrice <= filters.HigherPrice).Count())
            {
                page = 1;
            }

            items = items
                    .Where(x => x.CurrentPrice >= filters.LowerPrice && x.CurrentPrice <= filters.HigherPrice);

            fmodel.Items = items
                    .Skip((page - 1) * filters.PageSize)
                    .Take(filters.PageSize);

            fmodel.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = filters.PageSize,
                TotalItems = items.Count()
            };

            
            fmodel.CurrentCategory = category;

            return PartialView("_LoadSummary",fmodel);
        }

        private IEnumerable<Item> GetItems(Category category)
        {
            IEnumerable<Item> items = category.items;

            foreach (var sub in category.SubCategories)
            {
                items = items.Union(GetItems(sub));
            }

            return items;
        }


        
         public ViewResult ItemDetails(int id)
        {
            Item item = repository.Items.Where(x => x.Id == id).FirstOrDefault();
            return View("ItemDetailsView",item);
        }

        
        public ActionResult AddToCart(int Id)
        {
            return RedirectToAction("AddToCart", "Cart", new { Id });
        }

        public string GetImage(int Id)
        {
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            if (item != null)
            {
                //return File(item.ImageData, item.ImageMimeType);
                if (item.Photos.FirstOrDefault() != null)
                {
                    return Url.Content("~/Uploads/Images/" + item.Photos.FirstOrDefault().url);
                }
                else return null;
            }
            else
            {
                return null;
            }
        }

        public PartialViewResult Calculator(int itemId)
        {
            Item item = repository.Items.FirstOrDefault(x => x.Id == itemId);
            CalculatorViewModel model = new CalculatorViewModel();

            model.m2 = item.m2;

            model.sht = item.sht;

            model.InPack = item.CountInPack;

            model.SizeInM2 = item.SizeInM2;

            model.PriceForM2 = item.PriceForM2;

            model.ItemID = item.Id;

            model.Price = item.Price;

            model.OnlyInPacks = item.OnlyInPacks;

            return PartialView(model);
        }

        public string GetMiniImage(int Id)
        {
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            if (item != null)
            {
                Photo ph = item.Photos.FirstOrDefault();
                //return File(item.ImageData, item.ImageMimeType);
                if (ph != null)
                {
                    return Url.Content("~/Uploads/Images/" 
                        + Path.GetDirectoryName(ph.url) + "/"
                        + Path.GetFileNameWithoutExtension(ph.url)+"-mini"
                        + Path.GetExtension(ph.url));
                }
                else return null;
            }
            else
            {
                return null;
            }
        }

        public ActionResult Search(string search)
        {
            SearchViewModel model = new SearchViewModel();
            model.filters = new ItemsListFiltersModel
            {
                SelectedBrands = new List<string> { "All" },
                SelectedCountries = new List<string> { "All" },
                SelectedPurposes = new List<string> { "All" },
                SortBy = "name",
                PageSize = 20,
                HigherPrice = int.MaxValue,
                LowerPrice = 0
            };
            model.search = search;
            return View(model);
        }

        public JsonResult GetRandomCollections()
        {
            Random r = new Random();
            int sk = r.Next(0,repository.Categories.Count()-7);
            var ctgs = repository.Categories.Skip(sk).Take(6);
            List<string[]> list = new List<string[]>();
            foreach (var ctg in ctgs)
            {
                list.Add(new string[] { Url.Action("List", "Item", new { category = ctg.Name }),
                    Url.Content("~/Uploads/CategoryImages/" + repository.GetCategoryImageMiniUrl(ctg.CategoryId)) });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
	}
}