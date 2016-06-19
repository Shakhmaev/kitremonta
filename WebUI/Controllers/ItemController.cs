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
using System.Web.UI;
using System.Threading.Tasks;
using System.Data.Entity;

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



        [OutputCache(Duration = 1800)]
        public ViewResult Index()
        {
            IndexViewModel model;
            model = new IndexViewModel
            {
                DiscountItems = repository.Items.Where(x=>x.DiscountPercent>0).Take(12),
                LastItems = ((IEnumerable<Item>)repository.Items).Reverse().Where(x=>x.Price>0).Take(12)
            };

            return View(model);
        }

        public ActionResult ItemSummary(int id)
        {
            Item item = repository.Items.FirstOrDefault(x => x.Id == id);
            return PartialView("ItemSummary",item);
        }

        public async Task<ViewResult> List(string category, int page = 1, int PageSize = 12, string application = "all")
        {
            ItemsListViewModel model = new ItemsListViewModel();
            model.Side = new List<KeyValuePair<string, IEnumerable<Category>>>();
            var side = await GetSideCollection();

            IEnumerable<Property> props = new List<Property>();

            IEnumerable<PropValue> propValues = new List<PropValue>();

            Dictionary<string, IEnumerable<string>> FilterList = new Dictionary<string, IEnumerable<string>>();
            Dictionary<string, IEnumerable<string>> FilterSelectedList = new Dictionary<string, IEnumerable<string>>();

            IEnumerable<string> Brands = new List<string>();
            IEnumerable<string> Countries = new List<string>();
            IEnumerable<string> Applications = new List<string>();

            if (category != (string)null)
            {
                var ctg = await repository.Categories.FirstOrDefaultAsync(x => x.Name == category);
                var descCollects = await repository.GetDescendantCollectionsAsync(ctg);
                var items = await GetItems(ctg);
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
                    props = items.SelectMany(x => x.props.Where(p => p.IsInFilter)).Distinct();
                    propValues = items.SelectMany(x => x.propValues).Distinct();


                    foreach (var ctgdesc in descCollects)
                    {
                        if (ctgdesc.Application!=null)
                            (Applications as List<string>).Add(ctgdesc.Application);
                    }
                    Applications = Applications.Distinct();
                }
                switch (ctg.Type)
                {
                    case "Brand":
                    case "show_collections":
                    case "Collection":
                        {
                            model.categoryTypeMessage = "Коллекции";
                            model.Categories = (IEnumerable<Category>) await repository.GetChildCollectionsAvoidLowerSubsAsync(ctg);
                            break;
                        }
                    case "Country":
                        {
                            model.categoryTypeMessage = "Бренды";
                            model.Categories = await repository.GetBrandsByCountryAsync(ctg.Name);
                            break;
                        }
                    default: model.Categories = new List<Category>();
                        break;
                } 
                model.Side = side;
                model.currentctg = ctg;
            }
            else
            {
                model.HigherPrice = repository.Items.Max(x => x.CurrentPrice);
                model.LowerPrice = repository.Items.Min(x => x.CurrentPrice);
                Brands = repository.Items.Select(x => x.Brand).Distinct();
                Countries = repository.Items.Select(x => x.Country).Distinct();

                props = repository.Items.SelectMany(x => x.props.Where(p => p.IsInFilter)).Distinct();
                propValues = repository.Items.SelectMany(x => x.propValues).Distinct();

                foreach (var ctg in repository.Categories)
                {
                    if (ctg.Application != null) 
                        (Applications as List<string>).Add(ctg.Application);
                }
                Applications = Applications.Distinct();
                model.Categories = repository.Categories.Where(x => x.ParentID == null);
                model.categoryTypeMessage = "Разделы";
            }

            foreach (var prop in props)
            {
                var values = propValues.Where(k => k.Prop == prop).Select(x => x.Val).Distinct();
                values = values.ToList();
                (values as List<string>).Sort();
                (values as List<string>).Insert(0, prop.DisplayName);
                FilterList.Add(prop.PropName, new List<string>(values));
            }

            model.filters = new ItemsListFiltersModel(model.HigherPrice, model.LowerPrice, PageSize, "pricelowtohigh");

            model.filters.FiltersList = FilterList;

            model.filters.FiltersSelectedList = FilterSelectedList;

            model.filters.AllBrands = Brands;

            model.filters.SelectedBrands = new List<string>();

            model.filters.AllCountries = Countries;

            model.filters.SelectedCountries = new List<string>();

            model.filters.AllApplications = Applications;

            model.filters.SelectedApplications = new List<string>();

            //ToDo: написать для properties тоже самое как выше

            if (application != "all")
            {
                (model.filters.SelectedApplications as List<string>).Add(application);
            }

            model.CurrentCategory = category;

            return View(model);
        }


        public async Task<PartialViewResult> LoadSummary(string category, ItemsListFiltersModel filters, int page = 1, string search = "all")
        {
            FilteredListViewModel fmodel = new FilteredListViewModel();

            IQueryable<Item> items;

            var categ = await repository.Categories.FirstOrDefaultAsync(x => x.Name == category);

            if (categ == null)
            {
                items = repository.Items;
            }
            else
            {
                items = await GetItems(categ);
            }

            if (search != "all")
            {
                items = items.Where(x => x.Name.ToLower().Contains(search.ToLower()) || x.article.ToLower().Contains(search.ToLower()));
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

            if (filters.SelectedApplications.Count() > 0)
            {
                if (filters.SelectedApplications.First() == "All")
                {

                }
                else 
                {
                    var parents = items.SelectMany(x => x.ParentCategories).Distinct();
                    parents = parents.Where(x=> filters.SelectedApplications.Contains(x.Application) || x.Application=="универсал");
                    items = parents.SelectMany(x => x.items).Distinct();
                }
            }

            foreach (var filter in filters.FiltersSelectedList)
            {
                if (filter.Value.First() == "All") { }
                else items = items.Where(x=>x.propValues.Where(f=>f.Prop.PropName==filter.Key && filter.Value.Contains(f.Val)).Count()>0);
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


            //вначале плитка, потом всё остальное
            if (items.Any(x=>x.ItemType=="keram"))
            {

                var firstgoing = items.Where(x => x.propValues.Where(p => p.Prop.PropName == "Purpose" && p.Val.Contains("для стен") || p.Val.Contains("для пола")) != null && x.Price > 0);
            
                items = firstgoing.Union(items.Except(firstgoing));
            }
            //-

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

        private async Task<List<KeyValuePair<string, IEnumerable<Category>>>> GetSideCollection()
        {
            ISiteMap map = MvcSiteMapProvider.SiteMaps.Current;
            ISiteMapNode node = map.FindSiteMapNodeFromCurrentContext();
            var ctgs = repository.Categories.Where(x => x.Type == "show_collections");

            Category ctg = ClosestRootParent(ctgs,node);

            var desccountries = await repository.FindInDescendantCountriesAsync(ctg);
            var descbrands = await repository.FindInDescendantBrandsAsync(ctg);
            
            return new List<KeyValuePair<string, IEnumerable<Category>>>(){
                new KeyValuePair<string,IEnumerable<Category>>("Countries",
                    new List<Category>(desccountries)),
                new KeyValuePair<string,IEnumerable<Category>>("Brands",
                    new List<Category>(descbrands)),
                new KeyValuePair<string,IEnumerable<Category>>("RootCategory",
                    new List<Category>(){ ctg })
                };

        }

        private Category ClosestRootParent(IEnumerable<Category> categories, ISiteMapNode node)
        {
            ISiteMap map = MvcSiteMapProvider.SiteMaps.Current;
            Category parent = null;
            foreach (var ct in categories)
            {
                if (ct.SubCategories.Any(x => x.Type == "show_collections" 
                    && (node.IsDescendantOf(map.FindSiteMapNodeFromKey(x.CategoryId.ToString())) 
                    || node == map.FindSiteMapNodeFromKey(x.CategoryId.ToString()))))
                {
                    parent = ClosestRootParent(ct.SubCategories, node);
                }
                else if (node.IsDescendantOf(map.FindSiteMapNodeFromKey(ct.CategoryId.ToString())) 
                    || node == map.FindSiteMapNodeFromKey(ct.CategoryId.ToString()))
                {
                    parent = ct;
                }
            }
            return parent;
        }

        private async Task<IQueryable<Item>> GetItems(Category category)
        {
            IQueryable<Item> items = category.items.AsQueryable();

            foreach (var sub in category.SubCategories)
            {
                items = items.Union(await GetItems(sub));
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

        [OutputCache(Duration = int.MaxValue, Location=OutputCacheLocation.Client)]
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

            model.m2 = double.Parse(item.GetPropertyValue("m2"));

            model.sht = int.Parse(item.GetPropertyValue("sht"));

            model.InPack = item.GetPropertyValue("CountInPack");

            model.SizeInM2 = double.Parse(item.GetPropertyValue("SizeInM2"));

            model.PriceForM2 = bool.Parse(item.GetPropertyValue("PriceForM2"));

            model.ItemID = item.Id;

            model.Price = item.Price;

            model.OnlyInPacks = bool.Parse(item.GetPropertyValue("OnlyInPacks"));

            return PartialView(model);
        }

        [OutputCache(Duration = int.MaxValue, Location = OutputCacheLocation.Client)]
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
                SelectedApplications = new List<string> { "All" },
                FiltersList = new Dictionary<string,IEnumerable<string>>(),
                FiltersSelectedList = new Dictionary<string,IEnumerable<string>>(),
                SortBy = "name",
                PageSize = 20,
                HigherPrice = int.MaxValue,
                LowerPrice = 0
            };
            model.search = search;
            return View(model);
        }

        public async Task<JsonResult> GetRandomCollections()
        {
            Random r = new Random();
            var categories = (await repository.GetDescendantCollectionsAsync(await repository.Categories.FirstOrDefaultAsync(x=>x.Description=="Керамическая плитка")))
                .Where(x => x.Type == "Collection");
            int sk = r.Next(0,categories.Count()-7);
            var ctgs = categories.Skip(sk).Take(6);
            List<string[]> list = new List<string[]>();
            foreach (var ctg in ctgs)
            {
                list.Add(new string[] { Url.Action("List", "Item", new { category = ctg.Name }),
                    Url.Content("~/Uploads/CategoryImages/" + repository.GetCategoryImageMiniUrl(ctg.CategoryId)) });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetNeighbourItems(int id)
        {
            List<string[]> list = new List<string[]>();
            foreach (var item in (await repository.Items.FirstOrDefaultAsync(x => x.Id == id)).ParentCategories.SelectMany(x => x.items).Where(x => x.Id != id).Take(10))
            {
                try
                {
                    list.Add(new string[] { Url.Action("ItemDetails", "Item", new { id = item.Id }),
                        Url.Content(GetMiniImage(item.Id)), 
                        item.Name, item.CurrentPrice.ToString()
                    });
                }
                catch { }
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
	}
}