using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Store.Domain.Entities;

namespace Store.WebUI.Models
{
    public class ItemsListViewModel
    {
        public IEnumerable<Item> Items { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<KeyValuePair<string, IEnumerable<Category>>> Side { get; set; }
        public string categoryTypeMessage { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public Category currentctg { get; set; }
        public string CurrentCategory { get; set; }
        public string CurrentSubCategory { get; set; }
        public int HigherPrice { get; set; }
        public int LowerPrice { get; set; }
        public ItemsListFiltersModel filters { get; set; }
    }
}