using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Store.WebUI.Models
{
    public class CalculatorViewModel
    {
        public double m2 { get; set; }
        public int sht { get; set; }
        public bool PriceForM2 { get; set; }
        public double SizeInM2 { get; set; }
        public int ItemID { get; set; }
        public int Price { get; set; }
        public string InPack { get; set; }
        public bool OnlyInPacks { get; set; }
    }
}