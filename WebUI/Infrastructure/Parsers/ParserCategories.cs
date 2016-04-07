using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                        Text = workSheet.Cells[rowIterator, 2].Value!=null? workSheet.Cells[rowIterator, 2].Value.ToString():"",
                        Description = workSheet.Cells[rowIterator, 1].Value.ToString()
                    };

                    if (workSheet.Cells[rowIterator, 5].Value != null) //если заполнено поле применение в файле эксель
                    {
                        ctg.Application = workSheet.Cells[rowIterator, 5].Value.ToString().Trim();
                    }

                    string ImgPath = HostingEnvironment.MapPath("~/Uploads/CategoryImages/");

                    var imagesnames = Directory.EnumerateFiles(ImgPath, "*.*", SearchOption.AllDirectories);

                    string image = workSheet.Cells[rowIterator, 3].Value!=null? workSheet.Cells[rowIterator, 3].Value.ToString():"";

                    image = imagesnames.FirstOrDefault(x => x.ToLower().Contains(image.ToLower()) && !x.ToLower().Contains("-mini"));

                    string[] imagesphys = new string[] { };

                    string[] str = new string[] { };

                    if (workSheet.Cells[rowIterator, 4].Value!=null)
                    str = workSheet.Cells[rowIterator, 4].Value.ToString().Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                        .ToArray();

                    foreach (var st in str)
                        {
                            imagesphys = imagesphys.Union(imagesnames.Where(x => x.ToLower().Contains(st.ToLower())
                                && !x.ToLower().Contains("-mini"))).ToArray();
                        }


                    if (!String.IsNullOrEmpty(image))
                        {
                            Image miniimg = Parser1.MakeMini(Image.FromFile(image), 300, 200);

                            string path = Path.GetDirectoryName(image);
                            string fname = Path.GetFileNameWithoutExtension(image);
                            string ext = Path.GetExtension(image);
                            string fullname = path + "\\" + fname + "-mini" + ext;
                            /*if (File.Exists(fullname))
                            {
                                File.Delete(fullname);
                            }*/
                            miniimg.Save(fullname);
                            image = image.Replace(ImgPath, String.Empty);
                        }

                    List<string> extraImages = new List<string>();

                    for (int i = 0; i < imagesphys.Count(); i++)
                    {
                        extraImages.Add(imagesphys.ElementAt(i).Replace(ImgPath, String.Empty));
                    }

                    repos.UpdateCategoryFromXls(ctg, image, extraImages);

                }
                pack.Dispose();
            }

        }
    }
}