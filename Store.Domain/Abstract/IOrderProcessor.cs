using Store.Domain.Concrete;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Domain.Abstract
{
    public interface IOrderProcessor
    {
        Order Process(OrderDetails details);
        IEnumerable<Order> GetUncompletedOrders();
        Order GetOrder(int orderId);
        bool UpdateOrder(int orderId, Order orderNew);
        void CompleteOrder(int OrderId);
        void ChangeStatus(int OrderId, string status);
    }
}
