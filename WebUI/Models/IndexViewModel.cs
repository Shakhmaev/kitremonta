using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Models
{
    public class IndexViewModel
    {
        public IEnumerable<Item> DiscountItems { get; set; }
        public IEnumerable<Item> LastItems { get; set; }
    }
}