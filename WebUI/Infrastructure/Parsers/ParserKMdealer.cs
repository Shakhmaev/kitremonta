using OfficeOpenXml;
using Store.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class ParserKMdealer
    {
        ExcelPackage pack;
        string path;
        IItemRepository repos;

        public ParserKMdealer(HttpPostedFileBase file, IItemRepository repo)
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
                
                Regex numberexp = new Regex(@"[\d+\,]");
                Regex _Nexp = new Regex(@"(.+?)(\s+[N]{1})$");

                foreach (var workSheet in Sheets)
                {
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;


                    for (int rowIterator = 1; rowIterator <= noOfRow; rowIterator++)
                    {
                        var price = workSheet.Cells[rowIterator, 6].Value != null ? workSheet.Cells[rowIterator, 6].Value.ToString() : "";
                        if (!String.IsNullOrEmpty(price) && numberexp.IsMatch(price))
                        {
                            string article = workSheet.Cells[rowIterator, 1].Value.ToString();
                            if (_Nexp.IsMatch(article)) 
                            {
                                article = _Nexp.Replace(article, "$1"); 
                            }
                            bool success = repos.UpdateItemPriceFromXls(article, price);
                            if (!success)
                            {
                                errors.Add("article: " + article);
                                workSheet.Cells[rowIterator, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                workSheet.Cells[rowIterator, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkSeaGreen);
                            }
                        }
                    }
                }
            }
            pack.Save();
            return errors;
        }
    }
}