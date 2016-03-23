using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Store.Domain.Entities;

namespace Store.WebUI.Controllers
{
    public class UserController : Controller
    {
        UserManager<AppUser> UserManager;

        public UserController(UserManager<AppUser> um)
        {
            UserManager = um;
        }
        public ActionResult Index()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && UserManager != null)
            {
                UserManager.Dispose();
            }
            base.Dispose(disposing);
        }
	}
}