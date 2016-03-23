using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Models
{
    public class ItemsListFiltersModel
    {
        public int HigherPrice { get; set; }
        public int LowerPrice { get; set; }
        public int PageSize { get; set; }
        public bool WithDiscount { get; set; }
        public bool Hot { get; set; }
        public string SortBy { get; set; }
        public IEnumerable<string> AllBrands { get; set; }
        public IEnumerable<string> SelectedBrands { get; set; }
        public IEnumerable<string> AllCountries { get; set; }
        public IEnumerable<string> SelectedCountries { get; set; }
        public IEnumerable<string> AllPurposes { get; set; }
        public IEnumerable<string> SelectedPurposes { get; set; }
        public ItemsListFiltersModel() { }
        public ItemsListFiltersModel(int high, int low, int ps, string sort)
        {
            HigherPrice = high;
            LowerPrice = low;
            PageSize = ps;
            SortBy = sort;
        }

    }
}