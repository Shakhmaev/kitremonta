using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using Store.WebUI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Web.UI;
using System.Web.Script.Serialization;
using OfficeOpenXml;
using Store.WebUI.Infrastructure.Parsers;
using System.Net;

namespace Store.WebUI.Controllers
{   
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
        IItemRepository repository;
        IOrderProcessor proc;

        public AdminController(IItemRepository repo, IOrderProcessor processor)
        {
            repository = repo;
            proc = processor;
        }

        public ViewResult Index()
        {
            return View();
        }

        public ViewResult Items(AdminItemsViewModel model, int page = 1)
        {
            if (String.IsNullOrEmpty(model.search))
            {
                model.Items = repository.Items
                        .Skip((page - 1) * 50)
                        .Take(50);
                model.PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = 50,
                    TotalItems = repository.Items.Count()
                };
            }
            else
            {
                model.Items = repository.Items.Where(x => x.Name.ToLower()
                    .Contains(model.search.ToLower()))
                        .Skip((page - 1) * 50)
                        .Take(50);
                model.PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = 50,
                    TotalItems = repository.Items
                    .Where(x => x.Name.ToLower()
                        .Contains(model.search.ToLower())).Count()
                };
            }
            return View(model);
        }

        public ViewResult Categories()
        {
            var categories = repository.Categories;
            return View(categories.ToList());
        }

        public ActionResult CreateCategory(string name, int parentID)
        {
            if (!String.IsNullOrEmpty(name))
            {
                Translitter tr = new Translitter();
                Category ctg;
                if (parentID == -1)
                {
                    ctg = new Category()
                    {
                        Description = name,
                        Name = tr.GetTranslit(name),
                    };
                }
                else ctg = new Category()
                {
                    Description = name,
                    Name = tr.GetTranslit(name),
                    ParentID = parentID
                };
                repository.SubCategoryGetOrCreate(ctg);
            }
            return RedirectToAction("Categories");
        }

        public ViewResult Orders()
        {
            return View(proc.GetUncompletedOrders());
        }

        public ViewResult Edit(int Id)
        {
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            ViewBag.CategoriesList = FormCategoriesList();

            ViewBag.SubCategories = GetSubCategories(item.Id);

            return View(item);
        }

        [HttpPost]
        public ActionResult Edit(Item item, HttpPostedFileBase image = null)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    /*item.ImageMimeType = image.ContentType;
                    item.ImageData = new byte[image.ContentLength];
                    image.InputStream.Read(item.ImageData, 0, image.ContentLength);*/
                }
                repository.SaveItem(item);
                TempData["message"] = string.Format("Изменения в товаре \"{0}\" были сохранены", item.Name);
                return RedirectToAction("Index");
            }
            else
            {
                return View(item);
            }
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(string url)
        {
            AdminNewItemViewModel model = new AdminNewItemViewModel();
            string str = "";
            using (var client = new WebClient())
            {
                str = client.DownloadString("http://"+url);
            }
            int index = str.IndexOf(@"<tr>\n<td>\n<table cellpadding='5'");
            str = str.Remove(0,index);
            return View(model);
        }

        public ActionResult UploadMainPhoto()
        {
            HttpPostedFileBase image = Request.Files[0];
            int Id = Convert.ToInt32(Request.Params.GetValues("id")[0]);
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            item.ImageMimeType = image.ContentType;
            item.ImageData = new byte[image.ContentLength];
            image.InputStream.Read(item.ImageData, 0, image.ContentLength);

            repository.SaveItem(item);
            return RedirectToAction("GetImage", "Item", new { id = Id });
        }

        public void UploadExtraPhoto()
        {
            int Id = Convert.ToInt32(Request.Params.GetValues("id")[0]);
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase image = Request.Files[i];

                //image.SaveAs(HttpContext.Server.MapPath("~/Uploads/Images/"));

                //Photo photo = new Photo();

                //item.Photos.Add(photo);
                //repository.SaveChanges();
            }
        }

        public void DeletePhoto(int Id, int PhotoId)
        {
            repository.DeleteExtraPhoto(Id, PhotoId);
        }

        public ViewResult Create()
        {
            ViewBag.CategoriesList = FormCategoriesList();
            ViewBag.SubCategories = new List<KeyValuePair<int, string>>();
            return View("Edit", new Item { ParentCategories = new List<Category>() });
        }

        public ActionResult Delete(int Id)
        {
            Item deletedItem = repository.DeleteItem(Id);
            if (deletedItem != null)
            {
                TempData["message"] = string.Format("Товар \"{0}\" был удалён", deletedItem.Name);
            }
            return RedirectToAction("Index");
        }

        public List<KeyValuePair<int, string>> FormCategoriesList()
        {
            var CategoriesList = new List<KeyValuePair<int, string>>();

            foreach (var cat in repository.Categories.Where(x => x.Parent == null))
            {
                CategoriesList.Add(new KeyValuePair<int, string>(cat.CategoryId, cat.Name));
            }

            return CategoriesList;
        }

        public string GetSubCategories(int itemId)
        {
            var SubsList = new List<KeyValuePair<int, string>>();
            var Categories = repository.Categories.Where(x => x.items.Contains(repository.Items.FirstOrDefault(k => k.Id == itemId)));
            foreach (var cat in Categories)
            {
                SubsList.Add(new KeyValuePair<int,string>(cat.CategoryId, cat.Name));
            }

            string ret = new JavaScriptSerializer().Serialize(SubsList);
            return ret;
        }

        public ActionResult UploadXls()
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["Sheet"];

                Parser1 p = new Parser1(file,repository);
                List<string> msgs = p.Parse();
                string str = "";
                foreach (var msg in msgs)
                {
                    str = String.Concat(str, msg + "\n");
                }
                TempData["message"] = string.Format("Файл был загружен и обработан. {0} ",str);
            }
            return RedirectToAction("Index");
        }

        public ActionResult UploadKMPriceXls()
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["Sheet"];

                ParserKMdealer p = new ParserKMdealer(file, repository);
                List<string> msgs = p.Parse();
                string str = "Количество ненайденных = " + msgs.Count;
                
                foreach (var msg in msgs)
                {
                    str = String.Concat(str, msg + "\r\n");
                }
                TempData["message"] = string.Format("Файл был загружен и обработан. {0} ", str);
            }
            return RedirectToAction("Index");
        }

        public ActionResult UploadCategoriesXls()
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["Sheet"];

                ParserCategories p = new ParserCategories(file, repository);
                p.Parse();
                TempData["message"] = string.Format("Файл был загружен и обработан.");
            }
            return RedirectToAction("Index");
        }
	}
}