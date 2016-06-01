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
        public Order Process(OrderDetails details)
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
            return order;
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
        public Order GetOrder(int orderId)
        {
            var order = context.Orders.FirstOrDefault(x => x.Id == orderId);
            if (order != null)
            {
                return order;
            }
            return null;
        }

        public bool UpdateOrder(int orderId, Order orderNew)
        {
            var order = context.Orders.FirstOrDefault(x => x.Id == orderId);
            if (order != null)
            {
                order.Client = orderNew.Client;
                order.Completed = orderNew.Completed;
                order.Email = orderNew.Email;
                //order.Items = orderNew.Items; //не изменять заказанные вещи
                order.Phone = orderNew.Phone;
                order.Status = orderNew.Status;
                //order.time = orderNew.time; //не изменять время
                context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
