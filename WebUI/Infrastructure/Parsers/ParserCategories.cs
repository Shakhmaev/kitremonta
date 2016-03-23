using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class ParserCategories
    {
        ExcelPackage pack;
        string path;
        ExcelWorksheet workSheet;
        IItemRepository repos;
        Translitter translit = new Translitter();
        public ParserCategories(HttpPostedFileBase file, IItemRepository repo)
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
        public ParserCategories(IItemRepository repo) //экспериментально
        {
            repos = repo;
            path = HttpContext.Current.Server.MapPath("~/Uploads/filef.xlsx");
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

                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                {
                    Category ctg = new Category
                    {
                        Text = workSheet.Cells[rowIterator, 2].Value.ToString(),
                        Description = workSheet.Cells[rowIterator, 1].Value.ToString(),
                    };
                    string ImgPath = HostingEnvironment.MapPath("~/Uploads/CategoryImages/");

                    var imagesnames = Directory.EnumerateFiles(ImgPath, "*.*", SearchOption.AllDirectories);

                    string image = workSheet.Cells[rowIterator, 3].Value.ToString();

                    image = imagesnames.FirstOrDefault(x => x.ToLower().Contains(image.ToLower()));

                    image = image.Replace(ImgPath, String.Empty);

                    repos.UpdateCategoryFromXls(ctg, image);

                }
                pack.Dispose();
            }

        }
    }
}