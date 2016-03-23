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
        void Process(OrderDetails details);
        IEnumerable<Order> GetUncompletedOrders();
        void CompleteOrder(int OrderId);
        void ChangeStatus(int OrderId, string status);
    }
}
