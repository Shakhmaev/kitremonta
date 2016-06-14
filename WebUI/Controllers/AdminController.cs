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
using System.Threading.Tasks;
using System.Threading;
using Hangfire;
using Microsoft.AspNet.SignalR;
using Store.WebUI.Infrastructure.Hubs;
using System.Runtime.Remoting.Contexts;

namespace Store.WebUI.Controllers
{   
    [System.Web.Mvc.Authorize(Roles="Admin")]
    public class AdminController : AsyncController
    {
        IItemRepository repository;
        IOrderProcessor proc;

        public AdminController(IItemRepository repo, IOrderProcessor processor)
        {
            repository = repo;
            proc = processor;
        }

        public async Task<ViewResult> Index()
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

        public JsonResult RefreshOrderList()
        {
            var orders = proc.GetUncompletedOrders();
            List<int> ids = new List<int>();
            foreach (var ord in orders){
                ids.Add(ord.Id);
            }
            return Json(ids,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ViewResult OrderDetails(int orderId)
        {
            var order = proc.GetOrder(orderId);
            if (order != null)
            {
                return View(order);
            }
            return View();
        }

        [HttpPost]
        public ActionResult OrderDetails(Order order)
        {
            bool upd = proc.UpdateOrder(order.Id, order);
            if (upd)
            {
                TempData["message"] = string.Format("Изменения в заказе \"{0}\" были сохранены", order.Id);
                return RedirectToAction("Orders");
            }
            else
            {
                TempData["message"] = string.Format("Ошибка, проверьте данные", order.Id);
                return View(order);
            }
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
            /*HttpPostedFileBase image = Request.Files[0];
            int Id = Convert.ToInt32(Request.Params.GetValues("id")[0]);
            Item item = repository.Items.FirstOrDefault(i => i.Id == Id);

            item.ImageMimeType = image.ContentType;
            item.ImageData = new byte[image.ContentLength];
            image.InputStream.Read(item.ImageData, 0, image.ContentLength);

            repository.SaveItem(item);
            return RedirectToAction("GetImage", "Item", new { id = Id });*/
            return null;
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
                string path = "";
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    path = HttpContext.Server.MapPath("~/Uploads/filef.xlsx");
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    file.SaveAs(path);
                    BackgroundJob.Enqueue(() => UploadXlsOneJob(path));
                }
            }
            return RedirectToAction("Index");
        }

        public void UploadXlsOneJob(string path)
        {
            Parser1 p = new Parser1(path);
            List<string> msgs = p.Parse();
            string str = "";
            foreach (var msg in msgs)
            {
                str = String.Concat(str, msg + "</br>");
            }

            var hub = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
            (hub as NotifyHub).SendChatMessage((hub as NotifyHub).GetName(), str);
            TempData["message"] = string.Format("Файл был загружен и обработан. {0} ", str);
        }

        
        public void UploadKMPriceXls(HttpPostedFileBase Sheet)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["Sheet"];
                string path = "";
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    path = HttpContext.Server.MapPath("~/Uploads/filef.xlsx");
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    file.SaveAs(path);
                    BackgroundJob.Enqueue(() => ParseKMPriceJob(path));
                    
                }
            }
        }

        public void ParseKMPriceJob(string path)
        {
            ParserKMdealer p = new ParserKMdealer(path);
            List<string> msgs = new List<string>();
            msgs = p.Parse();

            string str = "Количество ненайденных = " + msgs.Count;

            foreach (var msg in msgs)
            {
                str = String.Concat(str, msg + "</br>");
            }

            var hub = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
            (hub as NotifyHub).SendChatMessage((hub as NotifyHub).GetName(), str);
        }


        public ActionResult UploadLincerStopsPriceXls()
        {
            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["Sheet"];

                ParserStops p = new ParserStops(file, repository);
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