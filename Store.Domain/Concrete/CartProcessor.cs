using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Store.Domain.Concrete
{
    public class CartProcessor:ICartProcessor
    {
        EFDbContext context = new EFDbContext();
        public CartProcessor(EFDbContext cont)
        {
            context = cont;
        }
        public IEnumerable<Cart> Carts 
        {
            get { return context.Carts; }
        }

        public Cart GetCart(string id)
        {
            Cart cart = Carts.FirstOrDefault(x => x.Id == id);
            if (cart != null)
            {
                return cart;
            }
            else
            {
                context.Carts.Add(new Cart
                {
                    Id = id,
                    Lines = new List<CartLine>(),
                    expires = DateTime.Now.AddDays(7)
                });
                context.SaveChanges();
                return Carts.FirstOrDefault(x => x.Id == id);
            }
        }
        public void SetExpiration(DateTime dt, Cart cart)
        {
            cart.expires = dt;
        }
        public void Add(Item item, int quantity, string id)
        {
            Cart cart = GetCart(id);
            CartLine line = cart.Lines.Where(i => i.Item.Id == item.Id).FirstOrDefault();
            if (quantity > 0)
            {
                if (line == null)
                {
                    context.Carts.FirstOrDefault(x => x.Id == id).Lines.Add(new CartLine
                    {
                        Item = item,
                        Quantity = quantity
                    });
                }
                else
                {
                    line.Quantity += quantity;
                }
                context.SaveChanges();
            }
        }

        public void RemoveLine(Item item, string id)
        {
            Cart cart = GetCart(id);
            context.Lines.Remove(cart.Lines.Where(x => x.Item.Id == item.Id).FirstOrDefault());
            SetExpiration(DateTime.Now.AddDays(7), cart);
            context.SaveChanges();
        }

        public void Clear(string id)
        {
            Cart cart = GetCart(id);
            foreach (var line in cart.Lines.ToArray())
            {
                RemoveLine(line.Item,id);
            }
            context.SaveChanges();
        }
    }
}
