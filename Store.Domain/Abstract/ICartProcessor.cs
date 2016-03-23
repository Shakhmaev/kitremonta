using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Store.Domain.Abstract
{
    public interface ICartProcessor
    {
        IEnumerable<Cart> Carts { get; }
        void Add(Item item, int quantity, string id);
        void RemoveLine(Item item, string id);
        void Clear(string id);
        Cart GetCart(string id);
        void SetExpiration(DateTime dt, Cart cart);
    }
}
