using OfficeOpenXml;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Store.Domain.Concrete;
using Store.Domain.Abstract;
using System.Text;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class ParserXlsDyn
    {
        ExcelPackage pack;
        string path;
        ExcelWorksheet workSheet;
        IItemRepository repos;
        Translitter translit = new Translitter();
        public ParserXlsDyn(HttpPostedFileBase file, IItemRepository repo)
        {
            repos = repo;
            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
            {
                path = HttpContext.Current.Server.MapPath("~/Uploads/file.xls");
                file.SaveAs(path);
                FileInfo fi = new FileInfo(path);
                pack = new ExcelPackage(fi);
            }
        }
        public ParserXlsDyn(IItemRepository repo) //экспериментально
        {
            repos = repo;
            path = HttpContext.Current.Server.MapPath("~/Uploads/file.xls");
            FileInfo fi = new FileInfo(path);
            pack = new ExcelPackage(fi);
        }
        public void Parse()
        {
            if (pack != null)
            {
                var currentSheet = pack.Workbook.Worksheets;
                workSheet = currentSheet.First();
                var noOfCol = workSheet.Dimension.End.Column;
                var noOfRow = workSheet.Dimension.End.Row;

                int EdCol = 3;

                Regex RegCategory = new Regex(@"\b(\d{1,2}[.])\s*([А-ЯЁа-яё]+)");
                Regex RegSubcategory = new Regex(@"([А-ЯЁа-яё]+.*)$");
                Regex RegBrand = new Regex(@"([A-Za-zа-яА-ЯЁё].*)$");
                
                Regex begin = new Regex(@"^(.+(\s*[+]\s*[А-ЯЁа-яё])*)");
                Regex ri = new Regex(@"((\s([-])\b[А-ЯЁа-яё]+.*$)|(\b[А-ЯЁа-яё]+.*$))");

                Regex RegDnt = new Regex(@"^DNT");

                string category = "";
                string subcategory = "";
                string brand = "";
                int level = 1;

                for (int rowIterator = 7; rowIterator <= noOfRow; rowIterator++)
                {
                    string str = workSheet.Cells[rowIterator, 2].Value.ToString();
                    string id = workSheet.Cells[rowIterator, 1].Value.ToString();

                    if (id == "1")
                    {
                        category = RegCategory.Replace(str, "$2");
                        subcategory = "";
                        brand = "";
                        level=1;
                        continue;
                    }
                    else if (id == "2")
                    {
                        subcategory = RegSubcategory.Match(str).ToString();
                        brand = "";
                        level=2;
                        continue;
                    }
                    else if (id == "3")
                    {
                        brand = RegBrand.Match(str).ToString();
                        level=3;
                        continue;
                    }
                    else if (RegDnt.IsMatch(id))
                    {
                        if (level == 1)
                        {
                            item(category, String.Empty, String.Empty, rowIterator);
                        }
                        else if (level == 2)
                        {
                            item(category, subcategory, String.Empty, rowIterator);
                        }
                        else if (level == 3)
                        {
                            item(category, subcategory, brand, rowIterator);
                        }
                    }
                }
                pack.Dispose();
            }

        }

        public void item(string category, string subcategory, string brand, int index)
        {
            Regex RegName = new Regex(@"((\s*[-]\s*[А-ЯЁа-яё]+.*$)|(\s*[А-ЯЁа-яё]+.*$))",
                    RegexOptions.Singleline);
            Item it = new Item { 
                Name = RegName.Replace(workSheet.Cells[index, 2].Value.ToString(), ""),
                Brand = brand,
                Description = "123",
                IsHot = false,
                Price = 10
            };
            string categ = subcategory.First().ToString().ToUpper()+subcategory.ToLower().Substring(1);
            it.SubCategory = new Category { 
                Description = categ,
                Name = translit.GetTranslit(categ.ToLower())
            };
            repos.SaveOrUpdateItemFromXls(it, category);
        }

    }


}