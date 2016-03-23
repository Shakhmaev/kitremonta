using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Store.WebUI.Models;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using Store.WebUI.Infrastructure;
using System.Net.Mail;
using System.Net;

namespace Store.WebUI.Controllers
{
    [AllowAnonymous]
    
    public class CartController : Controller
    {
        IItemRepository repository;
        ICartProcessor carts;

        public CartController(IItemRepository repo, ICartProcessor crt)
        {
            repository = repo;
            carts = crt;
        }

        public PartialViewResult AddToCartQuantity(CartIdWrapper cartid, int Id)
        {
            Item item = repository.Items.FirstOrDefault(x => x.Id == Id);
            return PartialView("_AddToCartQuantity", item);
        }

        public PartialViewResult AddToCart(CartIdWrapper cartid, int Id, int Quantity)
        {

            Item item = repository.Items.FirstOrDefault(x => x.Id == Id);

            if (item!=null)
            {
                carts.Add(item, Quantity, cartid.id);
            }

            ViewBag.quantity = Quantity;

            return PartialView("_AddToCart", item);
        }

        [HttpPost]
        public void UpdateCartQuantities(CartIdWrapper cartid, List<CartId_Quantity> list)
        {
            foreach (var p in list)
            {
                carts.Add(repository.Items.FirstOrDefault(x => x.Id == p.id),p.quantity,cartid.id);
            }
        }

        public RedirectToRouteResult RemoveFromCart(CartIdWrapper cartid, int Id, string returnUrl)
        {

            Item item = repository.Items.FirstOrDefault(x => x.Id == Id);

            if (item != null)
            {
                carts.RemoveLine(item, cartid.id);
            }
            return RedirectToAction("Index", new { returnUrl });
        }


        public void Clear(CartIdWrapper cartid)
        {
            carts.Clear(cartid.id);
        }

        public ViewResult Index(CartIdWrapper cartid, string returnUrl)
        {

            return View(new CartIndexViewModel
            {
                Cart = carts.GetCart(cartid.id),
                //ReturnUrl = returnUrl
            });
        }

        public PartialViewResult Summary(CartIdWrapper cartid)
        {
            return PartialView(carts.GetCart(cartid.id));
        }

	}
}