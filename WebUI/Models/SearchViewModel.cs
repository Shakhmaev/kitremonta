using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Models
{
    public class SearchViewModel
    {
        public ItemsListFiltersModel filters { get; set; }
        public string search { get; set; }
    }
}