using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Data.Sql;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class ParserStops
    {
        ExcelPackage pack;
        string path;
        IItemRepository repos;
        static Regex sizeregx = new Regex(@"([0-9]+[,]*[0-9]*)[xXхХ×*]([0-9]+[,]*[0-9]*)");

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

        public ParserStops(string filePath, IItemRepository repo)
        {
            repos = repo;
            if (!string.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
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
                    var brand = "";
                    #region Kerabud
                    if (workSheet.Name.Contains("КПС"))
                    {
                        brand = "Керабуд";
                        Dictionary<string, string> purposes = new Dictionary<string, string>() {
                                        {"блицовочная плитка","Плитка для стен"},
                                        {"д/стен","Плитка для стен"},
                                        {"для пола","Плитка для пола"},
                                        {"екор","Декор"},
                                        {"ордюр","Бордюр"}
                                    };
                        string collection = "";
                        Dictionary<string,int> currentBlock = new Dictionary<string,int>();
                       
                        for (int rowIterator = 12; rowIterator <= noOfRow; rowIterator++)
                        {
                            currentBlock.Clear();
                            var price = workSheet.Cells[rowIterator, pricecol].Value != null ? workSheet.Cells[rowIterator, pricecol].Value.ToString() : "";
                            if (String.IsNullOrEmpty(price))
                            {
                                if (workSheet.Cells[rowIterator, 1].Value == null) break;
                                collection = workSheet.Cells[rowIterator, 1].Value.ToString();
                                price = "1";
                                int offset = 1;
                                while (true)
                                {
                                    price = workSheet.Cells[rowIterator+offset, pricecol].Value != null ? workSheet.Cells[rowIterator+offset, pricecol].Value.ToString() : "";
                                    if (!String.IsNullOrEmpty(price))
                                    {
                                        currentBlock.Add(workSheet.Cells[rowIterator + offset, 1].Value.ToString()+currentBlock.Count.ToString(), offset);
                                        offset++;
                                    }
                                    else break;
                                }


                                while (currentBlock.Count > 0)
                                {
                                    var row = new KeyValuePair<string,int>(currentBlock.ElementAt(0).Key,currentBlock.ElementAt(0).Value);
                                    string name = row.Key;
                                    string purpose = "";
                                    List<string> nameadditions = new List<string>();

                                    foreach (var purp in purposes)
                                    {
                                        if (name.Contains(purp.Key))
                                        {
                                            IEnumerable<KeyValuePair<string, int>> dupls = new List<KeyValuePair<string,int>>(currentBlock.Where(x => x.Key.Contains(purp.Key)));
                                            foreach (var dupl in dupls)
                                            {
                                                currentBlock.Remove(dupl.Key);
                                            }
                                            
                                            purpose = purp.Value;
                                            break;
                                        }
                                    }

                                    string sizescell = workSheet.Cells[rowIterator+row.Value, 2].Value != null ? workSheet.Cells[rowIterator+row.Value, 2].Value.ToString() : "";
                                    List<string> sizes = new List<string>();
                                    if (String.IsNullOrEmpty(sizescell))
                                        sizes.Add("all");
                                    else
                                    {
                                        MatchCollection sizematches = sizeregx.Matches(sizescell);
                                        for (int i = 0; i < sizematches.Count; i++)
                                        {
                                            sizes.Add(sizematches[i].Value);
                                        }
                                    }


                                    if (_Nexp.IsMatch(name))
                                    {
                                        name = _Nexp.Replace(name, "$1");
                                    }
                                    bool success = UpdatePriceForKeram(brand, collection, purpose, workSheet.Cells[rowIterator+row.Value,pricecol].Value.ToString(), sizes, nameadditions);
                                    if (!success)
                                    {
                                        errors.Add("name: " + collection + " " + name);
                                        //workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        //workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                    }
                                }
                                rowIterator += --offset;
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
                                var articles = articleexp.Matches(workSheet.Cells[rowIterator, 1].Value.ToString());
                                for (int i = 0; i < articles.Count; i++)
                                {
                                    bool success = UpdatePriceAsync(articles[i].Value, (int)Math.Ceiling(Double.Parse(price)));
                                    if (!success)
                                    {
                                        errors.Add("Article: " + articles[i].Value);
                                        //workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                        //workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    /*#region LB
                    if (workSheet.Name.Contains("ЛБ "))
                    {
                        Regex articleexp = new Regex(@"\d{6}([/]\d{1})*");
                        IQueryable<Item> lbitems = repos.Items.Where(x => x.Brand == "LB-Ceramics");
                        //ДОДЕЛАТь
                        await lbitems.ForEachAsync(async (item) =>
                        {
                            for (int rowIterator = 10; rowIterator <= noOfRow; rowIterator++)
                            {
                                var firstcol = workSheet.Cells[rowIterator, 1].Value;
                                if (firstcol!=null)
                                {
                                    var str = firstcol.ToString();
                                    if (str.Contains(item.articleOriginal))
                                    {
                                        var price = workSheet.Cells[rowIterator, 6].Value != null ? workSheet.Cells[rowIterator, 6].Value.ToString() : "";
                                        if (!String.IsNullOrEmpty(price) && numberexp.IsMatch(price))
                                        {
                                            bool success = await UpdatePriceAsync(item.article, (int)Math.Ceiling(Double.Parse(price)));
                                            if (!success)
                                            {
                                                errors.Add("Article: " + item.article);
                                                //workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                                //workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                            }
                                        }
                                    }
                                }
                            }
                        });
                    }
                    #endregion
                    #region Azori
                    if (workSheet.Name.Contains("Азори"))
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
                                    purpose = "плитка для стен";
                                }
                                else if (name.Contains("Плитка для пола"))
                                {
                                    purpose = "плитка для пола";
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

                                string sizescell = workSheet.Cells[rowIterator, 1].Value == null ? workSheet.Cells[rowIterator, 1].Value.ToString() : "";
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
                                bool success = await UpdatePriceForKeramAsync(collection, purpose, price, sizes, nameadditions);
                                if (!success)
                                {
                                    errors.Add("name: " + collection + " " + name);
                                    //workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    //workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                                }
                            }
                        }
                    }
                    #endregion*/
                    else continue;


                    
                }
            }
            pack.Save();
            return errors;
        }

        private bool UpdatePriceForKeram(string brand, string collection, string purpose, string price, List<string> sizes, List<string> additions)
        {
            var ctgs = repos.Categories.Where(x => x.Description == collection);
            List<Item> items = new List<Item>();
            foreach (var ctg in ctgs)
            {
                items = items.Union(ctg.items.Where(x=>x.GetPropertyValue("Purpose") == purpose && x.Brand==brand)).ToList();
            }

            if (items.Count > 0)
            {
                /*List<Item> itemsFiltered = new List<Item>();
                foreach (var addition in additions)
                {
                    itemsFiltered = itemsFiltered.Union(items.Where(x=>x.Name.Contains(addition))).ToList();
                }
                items = itemsFiltered;*/
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

        private bool UpdatePriceAsync(string article, int price)
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