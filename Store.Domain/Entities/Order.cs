using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public int Sum { get; set; }
        public int NumberOfItems { 
            get { return Items.Count(); } 
        }
        public bool Completed { get; set; }
        public string Status { get; set; }
        public string Client { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime time { get; set; }
        public virtual ICollection<OrderedItem> Items { get; set; }
    }
    public class OrderedItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}
