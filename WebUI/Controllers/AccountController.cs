using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store.WebUI.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Profile/
        public ActionResult Index(string id)
        {
            return View();
        }
	}
}