using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.Domain.Abstract;

namespace Store.WebUI.Controllers
{
    [AllowAnonymous]
    public class NavController : Controller
    {
        private IItemRepository repository;

        public NavController(IItemRepository repo)
        {
            repository = repo;
        }

        public PartialViewResult Menu(string category = null)
        {
            ViewBag.CurrentCategory = category;
            IEnumerable<string> categories = repository.Items
                .Select(item=>item.SubCategory.Name)
                .Distinct()
                .OrderBy(x=>x);
            return PartialView(categories);
        }

        public PartialViewResult Login()
        {
            return PartialView("_LoginPartial");
        }
	}
}