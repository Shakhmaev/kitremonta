using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class ParserStops
    {
        ExcelPackage pack;
        string path;
        IItemRepository repos;
        static Regex sizeregx = new Regex(@"^([0-9]+[,]*[0-9]*)[xXхХ×*]([0-9]+[,]*[0-9]*)");

        public ParserStops(HttpPostedFileBase file, IItemRepository repo)
        {
            repos = repo;
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                path = HttpContext.Current.Server.MapPath("~/Uploads/filef.xlsx");
                if (File.Exists(path)) File.Delete(path);
                file.SaveAs(path);
                FileInfo fi = new FileInfo(path);
                pack = new ExcelPackage(fi);
            }
        }

        public List<string> Parse()
        {
            List<string> errors = new List<string>();
            if (pack != null)
            {
                var Sheets = pack.Workbook.Worksheets;

                Regex numberexp = new Regex(@"[\d\,]+");
                Regex _Nexp = new Regex(@"(.+?)(\s+[N]{1})$");

                foreach (var workSheet in Sheets)
                {
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int pricecol = 6;
                    #region Kerabud
                    if (workSheet.Name.Contains("КПС"))
                    {
                        string collection = "";

                        for (int rowIterator = 12; rowIterator <= noOfRow; rowIterator++)
                        {
                            var price = workSheet.Cells[rowIterator, pricecol].Value != null ? workSheet.Cells[rowIterator, pricecol].Value.ToString() : "";
                            if (String.IsNullOrEmpty(price))
                            {
                                if (workSheet.Cells[rowIterator, 1].Value == null) break;
                                collection = workSheet.Cells[rowIterator, 1].Value.ToString();
                            }
                            if (!String.IsNullOrEmpty(price) && numberexp.IsMatch(price))
                            {
                                string name = workSheet.Cells[rowIterator, 1].Value.ToString();
                                string purpose = "";
                                List<string> nameadditions = new List<string>();
                                if (name.Contains("Облицовочная плитка"))
                                {
                                    purpose = "для стен";
                                }
                                else if (name.Contains("Плитка для пола"))
                                {
                                    purpose = "для пола";
                                }
                                else if (name.Contains("Декор"))
                                {
                                    purpose = "декор";
                                }
                                else if (name.Contains("Бордюр"))
                                {
                                    purpose = "бордюр";
                                    Regex classifiers = new Regex("\"w+\"");
                                    MatchCollection matches = classifiers.Matches(name);

                                    for (int i = 0; i < matches.Count; i++)
                                    {
                                        nameadditions.Add(matches[i].Value);
                                    }
                                }
                                
                                string sizescell = workSheet.Cells[rowIterator, 1].Value==null?workSheet.Cells[rowIterator, 1].Value.ToString():"";
                                List<string> sizes = new List<string>();
                                if (String.IsNullOrEmpty(sizescell))
                                    sizes.Add("all");
                                else
                                {
                                    MatchCollection sizematches = sizeregx.Matches(workSheet.Cells[rowIterator, 2].Value.ToString());
                                    for (int i = 0; i < sizematches.Count; i++)
                                    {
                                        nameadditions.Add(sizematches[i].Value);
                                    }
                                }


                                if (_Nexp.IsMatch(name))
                                {
                                    name = _Nexp.Replace(name, "$1");
                                }
                                bool success = UpdatePriceForKeram(collection,purpose,price,sizes,nameadditions);
                                if (!success)
                                {
                                    errors.Add("name: " + collection + " " + name );
                                    workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                }
                            }
                        }
                    }
                    #endregion
                    #region M-kvadrat
                    if (workSheet.Name.Contains("М-Квадрат"))
                    {
                        Regex articleexp = new Regex(@"\d{6}([/]\d{1})*");
                        for (int rowIterator = 15; rowIterator <= noOfRow; rowIterator++)
                        {
                            var price = workSheet.Cells[rowIterator, 5].Value != null ? workSheet.Cells[rowIterator, 5].Value.ToString() : "";
                            if (!String.IsNullOrEmpty(price) && numberexp.IsMatch(price))
                            {
                                var article = articleexp.Match(workSheet.Cells[rowIterator, 1].Value.ToString());
                                if (article.Success)
                                {
                                    bool success = UpdatePrice(article.Value, (int)Math.Ceiling(Double.Parse(price)));
                                    if (!success)
                                    {
                                        errors.Add("Article: " + article);
                                        workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    else continue;


                    
                }
            }
            pack.Save();
            return errors;
        }

        private bool UpdatePriceForKeram(string collection, string purpose, string price, List<string> sizes, List<string> additions)
        {
            var ctgs = repos.Categories.Where(x => x.Description == collection);
            List<Item> items = new List<Item>();
            foreach (var ctg in ctgs)
            {
                items = items.Union(ctg.items.Where(x=>x.GetPropertyValue("Purpose") == purpose)).ToList();
            }

            if (items.Count > 0)
            {
                List<Item> itemsFiltered = new List<Item>();
                foreach (var addition in additions)
                {
                    itemsFiltered = itemsFiltered.Union(items.Where(x=>x.Name.Contains(addition))).ToList();
                }
                items = itemsFiltered;
                if (items.Count > 0)
                {

                    if (sizes[0] != "all")
                    {
                        foreach (var size in sizes)
                        {
                            Match m = sizeregx.Match(size.Replace('.', ','));
                            double SizeInM2 = (double)((double.Parse(m.Groups[1].Value) * double.Parse(m.Groups[2].Value)) / 10000);
                            foreach (var item in items.Where(x => double.Parse(x.GetPropertyValue("SizeInM2")) == SizeInM2))
                            {
                                item.Price = int.Parse(price);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            item.Price = int.Parse(price);
                        }
                    }
                    repos.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        private bool UpdatePrice(string article, int price)
        {
            Item item = repos.Items.FirstOrDefault(x => x.article == article);
            if (item != null)
            {
                item.Price = price;
                repos.SaveChanges();
                return true;
            }
            return false;
        }
    }
}