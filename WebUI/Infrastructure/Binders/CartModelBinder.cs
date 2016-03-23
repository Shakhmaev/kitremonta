using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Store.Domain.Entities;
using Store.Domain.Abstract;
using Ninject;
using Store.WebUI.Models;

namespace Store.WebUI.Infrastructure.Binders
{
    public class CartIdBinder:IModelBinder
    {
        private const string sessionKey = "Cart";

        public CartIdWrapper wrapper = new CartIdWrapper();

        /*public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            Cart cart = null;
            if (controllerContext.HttpContext.Session!=null)
            {
                cart = (Cart)controllerContext.HttpContext.Session[sessionKey];
            }

            if (cart==null)
            {
                cart = new Cart();
                if (controllerContext.HttpContext.Session!=null)
                {
                    controllerContext.HttpContext.Session[sessionKey] = cart;
                }
            }
            return cart;
        }*/

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            wrapper.id = "";
            
            if (controllerContext.HttpContext.Request.Cookies[sessionKey] == null
                || String.IsNullOrWhiteSpace(controllerContext.HttpContext.Request.Cookies[sessionKey].Value))
            {
                Guid tmpGuid = Guid.NewGuid();
                wrapper.id = tmpGuid.ToString();
                controllerContext.HttpContext.Response.Cookies[sessionKey].Value = tmpGuid.ToString();
                controllerContext.HttpContext.Response.Cookies[sessionKey].Expires = DateTime.Now.AddDays(7);
            }
            else
            {
                wrapper.id = controllerContext.HttpContext.Request.Cookies[sessionKey].Value;
                if (!String.IsNullOrWhiteSpace(wrapper.id))
                {
                    controllerContext.HttpContext.Response.Cookies[sessionKey].Value = wrapper.id;
                    controllerContext.HttpContext.Response.Cookies[sessionKey].Expires = DateTime.Now.AddDays(7);
                }
            }
            return wrapper;
        }


    }
}