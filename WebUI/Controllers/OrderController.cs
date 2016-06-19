using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using Store.WebUI.Infrastructure.Hubs;
using Store.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Store.WebUI.Controllers
{
    [AllowAnonymous]
    public class OrderController : Controller
    {
        IOrderProcessor orderProc;
        ICartProcessor carts;
        IItemRepository repository;
        //
        // GET: /Order/
        public OrderController(IOrderProcessor processor, ICartProcessor cartproc, IItemRepository repos)
        {
            orderProc = processor;
            carts = cartproc;
            repository = repos;
        }
        

        public ActionResult Index(CartIdWrapper cartid)
        {
            ViewBag.Msg = null;
            if (carts.GetCart(cartid.id).Lines.Count() == 0)
            {
                ViewData.Add("message", "Вы слишком долго бездействовали и все покупки вывалились из корзины :( Добавьте их в корзину снова.");
                return View("OrderResult");
            }
            var model = new OrderDetails();
            return View("Order", model);
        }


        [HttpPost]
        public async Task<ActionResult> Index(OrderDetails model, CartIdWrapper cartid)
        {
            
            if (ModelState.IsValid && VerifyRecaptcha())
            {
                Task send = SendEmailToAdmins();
                model.cart = carts.GetCart(cartid.id);
                Order order = orderProc.Process(model);
                carts.Clear(cartid.id);
                var hub = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
                hub.Clients.Group("Admins").notify("Пришёл новый заказ!");
                (hub as NotifyHub).NewOrder();
                await send;
                return View("OrderResult");
            }
            else
            {
                ViewBag.Msg = "Вы не прошли проверку. Попробуйте ещё раз.";
                return View("Order", model);
            }
        }
        private async Task SendEmailToAdmins()
        {
            var body = "<p>{0}</p>";
            var message = new MailMessage();

            var IdentUserRoles = repository.Roles.FirstOrDefault(x => x.Name == "Admin").Users;

            List<string> ids = new List<string>();
            foreach (var userrole in IdentUserRoles)
            {
                ids.Add(userrole.UserId);
            }

            var appusers = repository.Users.Where(x => ids.Contains(x.Id));

            foreach (var user in appusers)
            {
                message.To.Add(new MailAddress(user.Email));
            }
            message.To.Add(new MailAddress("r.e.a.c.t.o.r@mail.ru"));  // replace with valid value 
            message.From = new MailAddress("office@kitremonta.ru");  // replace with valid value
            message.Subject = "Kitremonta. Новый заказ!";
            message.Body = string.Format(body, "Пришёл новый заказ, который нужно обработать! <a href='" + Url.Action("Orders", "Admin") + "'>Панель управления</a>");
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "office@kitremonta.ru",  // replace with valid value
                    Password = "lindex456"  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "mail.kitremonta.ru";
                smtp.Port = 587;
                await smtp.SendMailAsync(message);
            }
        }

        public bool VerifyRecaptcha()
        {
            var response = Request["g-recaptcha-response"];
            //secret that was generated in key value pair
            const string secret = "6LdOOxsTAAAAAN7vwGLJMsfWv4qdkmLxKozcvACp";

            var client = new WebClient();
            var reply =
                client.DownloadString(
                    string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, response));

            var captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(reply);

            //when response is false check for the error message
            if (!captchaResponse.Success)
            {
                if (captchaResponse.ErrorCodes.Count <= 0) return true;

                var error = captchaResponse.ErrorCodes[0].ToLower();
                switch (error)
                {
                    case ("missing-input-secret"):
                        ViewBag.Msg = "The secret parameter is missing.";
                        break;
                    case ("invalid-input-secret"):
                        ViewBag.Msg = "The secret parameter is invalid or malformed.";
                        break;

                    case ("missing-input-response"):
                        ViewBag.Msg = "The response parameter is missing.";
                        break;
                    case ("invalid-input-response"):
                        ViewBag.Msg = "The response parameter is invalid or malformed.";
                        break;

                    default:
                        ViewBag.Msg = "Error occured. Please try again";
                        break;
                }
                return false;
            }
            else
            {
                ViewBag.Msg = "Valid";
            }

            return true;
        }

        public class CaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }

        
        public void AddNewCall(string number)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
            hub.Clients.Group("Admins").notify("Перезвоните клиенту!");
            (hub as NotifyHub).AddCallToAdminInfo(number);
        }

        public void RemoveCall(string number)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<NotifyHub>();
            (hub as NotifyHub).RemoveCallFromAdminInfo(number);
        }
	}
}