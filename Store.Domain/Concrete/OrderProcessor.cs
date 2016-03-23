using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Domain.Concrete
{
    public class OrderProcessor : IOrderProcessor
    {
        private OrderDbContext context = new OrderDbContext();
        public void Process(OrderDetails details)
        {
            Order order = new Order();
            order.Status = "Новый";
            order.Sum = Convert.ToInt32(details.cart.ComputeTotalValue());
            order.Completed = false;
            order.Items = new List<OrderedItem>();
            foreach (var line in details.cart.Lines.ToArray())
            {
                OrderedItem item = new OrderedItem
                {
                    Brand = line.Item.Brand,
                    Name = line.Item.Name,
                    Quantity = line.Quantity,
                    Price = line.Item.Price
                };
                order.Items.Add(item);
            }
            order.Client = details.FirstName;
            order.Email = details.Email;
            order.Phone = details.Phone;
            order.time = DateTime.Now;
            context.Orders.Add(order);
            context.SaveChanges();
        }
        public IEnumerable<Order> GetUncompletedOrders()
        {
            return context.Orders.Include("Items").Where(x => x.Completed != true);
        }
        public void CompleteOrder(int OrderId)
        {
            var order = context.Orders.FirstOrDefault(x => x.Id == OrderId);
            if (order != null)
            {
                order.Completed = true;
            }
        }
        public void ChangeStatus(int OrderId, string status)
        {
            var order = context.Orders.FirstOrDefault(x => x.Id == OrderId);
            if (order != null)
            {
                order.Status = status;
            }
        }
    }
}
