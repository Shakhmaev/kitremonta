using Ninject;
using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace Store.WebUI.Infrastructure.Parsers
{
    public class Parser1
    {
        ExcelPackage pack;
        string path;
        ExcelWorksheet workSheet;

        IItemRepository repos;
        Translitter translit = new Translitter();
        Regex CatMatch = new Regex(@"^(\s*([\w-\s]+?)\s*$)");

        public Parser1(string filePath, IItemRepository repo)
        {
            repos = repo;
            if (!string.IsNullOrEmpty(filePath))
            {
                FileInfo fi = new FileInfo(filePath);
                pack = new ExcelPackage(fi);
            }
        }
        public Parser1(HttpPostedFileBase file, IItemRepository repo)
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
        public Parser1(IItemRepository repo) //экспериментально
        {
            repos = repo;
            path = HttpContext.Current.Server.MapPath("~/Uploads/filef.xlsx");
            FileInfo fi = new FileInfo(path);
            pack = new ExcelPackage(fi);
        }

        public List<string> Parse()
        {
            List<string> messages = new List<string>();
            if (pack != null)
            {
                try
                {
                    var currentSheet = pack.Workbook.Worksheets;
                    workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;


                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        if (workSheet.Cells[rowIterator, 6].Value == null) break;
                        Item item = new Item
                        {
                            Brand = ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 3].Value.ToString()),
                            Type = ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 1].Value.ToString().Split(',').Last()),
                            Name = ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 6].Value.ToString()),
                            IsHot = false,
                            Country = ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 2].Value.ToString()),
                            Price = Convert.ToInt32(workSheet.Cells[rowIterator, 7].Value)
                        };

                        item.props = new List<Property>();
                        item.propValues = new List<PropValue>();
                        

                        SetPropValue("CountInPack",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 9].Value.ToString()),ref item,false);
                        SetPropValue("Purpose",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 10].Value.ToString()),ref item, true);
                        SetPropValue("Surface",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 11].Value.ToString()),ref item, true);
                        SetPropValue("Picture",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 12].Value.ToString()),ref item, true);
                        SetPropValue("Color",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 13].Value.ToString()),ref item, true);
                        SetPropValue("Weight",ToLowerCapitalizeFirstTrim(workSheet.Cells[rowIterator, 16].Value.ToString()), ref item, false);

                        item.article = workSheet.Cells[rowIterator, 5].Value != null ? item.Brand.Substring(0,2) + "-" + workSheet.Cells[rowIterator, 5].Value.ToString() : item.Brand.Substring(0,2) + "-" + Guid.NewGuid().ToString("N");


                        var pru = workSheet.Cells[rowIterator, 8].Value.ToString();
                        item.PriceUnit = pru.Contains("шт") ? "шт" : "м2";

                        string inpack = item.props.FirstOrDefault(x=>x.PropName=="CountInPack").Values.ElementAt(0).Val.Replace('.', ',');
                        Regex reginpack = new Regex(@"^([0-9]+[,]*[0-9]*)*(.+[/\\]\s*)*([0-9]*)(.*)*");
                        Match m = reginpack.Match(inpack);

                        double m2 = 0;
                        int sht = 0;
                        if (m.Groups[3].Value != "")
                        {
                            bool m2p = double.TryParse(m.Groups[1].ToString(), out m2); ;
                            bool shtp = int.TryParse(m.Groups[3].ToString(), out sht);
                            if (m2p) SetPropValue("m2", m2.ToString(), ref item, false); else SetPropValue("m2", "0", ref item, false);
                            if (shtp) SetPropValue("sht", sht.ToString(), ref item, false); else SetPropValue("sht", "0", ref item, false);
                        }
                        else
                        {
                            int.TryParse(m.Groups[1].ToString(), out sht);
                            SetPropValue("m2", "0", ref item, false);
                            SetPropValue("sht", sht.ToString(), ref item, false);
                        }

                        SetPropValue("Size", Regex.Replace(Regex.Replace(workSheet.Cells[rowIterator, 14].Value.ToString(), "[xXхХ×*]", "x"), @"\s", "").Replace(".",","), ref item, true); //удаляем все пробелы и заменяем х на обычный

                        Regex sizeregx = new Regex(@"^([0-9]+[,]*[0-9]*)[xXхХ×*]([0-9]+[,]*[0-9]*)");
                        m = sizeregx.Match(item.props.FirstOrDefault(x => x.PropName == "Size").Values.ElementAt(0).Val);
                        if (m.Success)
                        {
                            SetPropValue("SizeInM2", ((double.Parse(m.Groups[1].Value) * double.Parse(m.Groups[2].Value)) / 10000).ToString(), ref item, false);
                        }
                        else SetPropValue("SizeInM2", "0", ref item, false);

                        if (item.PriceUnit.ToLower().Contains("м2")) SetPropValue("PriceForM2", true.ToString(), ref item, false); 
                        else SetPropValue("PriceForM2", false.ToString(), ref item, false);

                        item.ItemType = "keram";

                        //string oip = workSheet.Cells[rowIterator, 10].Value.ToString();
                        if (workSheet.Cells[rowIterator, 8].Value.ToString().Contains("м2"))
                        {
                            SetPropValue("OnlyInPacks", true.ToString(), ref item, false);
                        }
                        else
                        {
                            SetPropValue("OnlyInPacks", false.ToString(), ref item, false);
                        }

                        string[] hierarchy = new string[] 
                        {
                            workSheet.Cells[rowIterator, 2].Value.ToString(), //страна
                            workSheet.Cells[rowIterator, 3].Value.ToString(), //производитель
                        };

                        hierarchy = workSheet.Cells[rowIterator, 1].Value.ToString().Split(',').Union(hierarchy).ToArray();

                        string[] hierarchyCollections = workSheet.Cells[rowIterator, 4].Value.ToString().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        List<string[]> hierarchylist = new List<string[]>();
                        List<string[]> translitnameslist = new List<string[]>();

                        for (int i = 0; i < hierarchyCollections.Length; i++)
                        {
                            string[] splitting = hierarchyCollections[i].Split(',');
                            splitting = hierarchy.Union(splitting).ToArray();
                            string[] translits = new string[splitting.Length];
                            for (int j = 0; j < splitting.Count(); j++)
                            {
                                splitting[j] = splitting[j].First().ToString().ToUpper() + splitting[j].Substring(1);
                                if (CatMatch.IsMatch(splitting[j]))
                                {
                                    splitting[j] = CatMatch.Replace(splitting[j], "$2");
                                    splitting[j] = splitting[j].First().ToString().ToUpper() + splitting[j].Substring(1);
                                }
                                translits[j] = translit.GetTranslit(splitting[j].ToLower());
                            }
                            hierarchylist.Add(splitting);
                            translitnameslist.Add(translits);
                        }

                        string ImgPath = HostingEnvironment.MapPath("~/Uploads/Images/");

                        var imagesnames = Directory.EnumerateFiles(ImgPath, "*.*", SearchOption.AllDirectories);

                        //string[] imgsames = workSheet.Cells[rowIterator,16].Value.


                        if (workSheet.Cells[rowIterator, 15].Value != null)
                        {
                            string[] str = workSheet.Cells[rowIterator, 15].Value.ToString().Split(',').ToArray();

                            //var imagesphys = new string[] { };

                            var imagesphys = new List<string>();

                            List<string> ToDelete = new List<string>();

                            foreach (var st in str)
                            {
                                if (String.IsNullOrEmpty(st)) continue;

                                imagesphys = imagesphys.Union(imagesnames.Where(x => x.ToLower().Contains(st.ToLower())
                                    && !x.ToLower().Contains("-mini"))).ToList();

                                foreach (var image in imagesphys.Skip(1).Where(x => !x.ToLower().Contains("-mini")))
                                {
                                    if (Regex.IsMatch(Path.GetFileNameWithoutExtension(image), @"_\d+$"))
                                    {
                                        ToDelete.Add(image);
                                    }
                                }
                            }

                            foreach (var image in ToDelete)
                            {
                                imagesphys.Remove(image);
                                File.Delete(image);
                            }


                            if (imagesphys.Count > 0)
                            {
                                Image img = Image.FromFile(imagesphys[0]);

                                Image miniimg = MakeMini(img, 300, 200);

                                string path = Path.GetDirectoryName(imagesphys[0]);
                                string fname = Path.GetFileNameWithoutExtension(imagesphys[0]);
                                string ext = Path.GetExtension(imagesphys[0]);
                                string fullname = path + "\\" + fname + "-mini" + ext;
                                if (!File.Exists(fullname))
                                {
                                    miniimg.Save(fullname);
                                }

                            }

                            List<string> images = new List<string>();

                            for (int i = 0; i < imagesphys.Count; i++)
                            {
                                images.Add(imagesphys.ElementAt(i).Replace(ImgPath, String.Empty));
                            }

                            repos.SaveOrUpdateItemFromXlsOneAsync(item, hierarchylist, translitnameslist, images);
                        }
                        else repos.SaveOrUpdateItemFromXlsOneAsync(item, hierarchylist, translitnameslist, new List<string>());
                    }
                    pack.Dispose();
                }
                catch (Exception e)
                {
                    messages.Add(e.Message + "/r/n" + e.StackTrace);
                }
            }
            return messages;
        }

        public string ToLowerCapitalizeFirstTrim(string str)
        {
            str = str.ToLower().Trim();
            str = String.Concat(str.First().ToString().ToUpper(), str.Substring(1));
            return str;
        }

        public void SetPropValue(string PropName, string value, ref Item item, bool inFilter)
        {
            Property prop = new Property() { PropName = PropName, IsInFilter = inFilter };
            prop.Values = new List<PropValue>();
            var propVal = new PropValue() { Val = value, Items = new List<Item>() };
            prop.Values.Add(propVal);
            item.propValues.Add(propVal);
            item.props.Add(prop);
        }

        public static Image MakeMini(Image img, int x1, int y1) {
            int x2 = 3;
            int y2 = 3;
            if (img.Width >= img.Height)
            {
                x1 = 200;
                y1 = (int)Math.Round((double)img.Height / ((double)img.Width / 200));
                y2 = (int)Math.Round((double)((200 - y1) / 2));

            }
            else
            {
                if (img.Width < img.Height)
                {
                    y1 = 200;
                    x1 = (int)Math.Round((double)img.Width / ((double)img.Height / 200));
                    x2 = (int)Math.Round((double)((200 - x1) / 2));
                }
            }
            img = ScaleImage(img, x1, y1);
            return img;
        }
        private static Image ScaleImage(Image source, int width, int height)
        {

            Image dest = new Bitmap(width, height);
            using (Graphics gr = Graphics.FromImage(dest))
            {
                gr.FillRectangle(Brushes.White, 0, 0, width, height);  // Очищаем экран
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;

                float srcwidth = source.Width;
                float srcheight = source.Height;
                float dstwidth = width;
                float dstheight = height;

                if (srcwidth <= dstwidth && srcheight <= dstheight)  // Исходное изображение меньше целевого
                {
                    int left = (width - source.Width) / 2;
                    int top = (height - source.Height) / 2;
                    gr.DrawImage(source, left, top, source.Width, source.Height);
                }
                else if (srcwidth / srcheight > dstwidth / dstheight)  // Пропорции исходного изображения более широкие
                {
                    float cy = srcheight / srcwidth * dstwidth;
                    float top = ((float)dstheight - cy) / 2.0f;
                    if (top < 1.0f) top = 0;
                    gr.DrawImage(source, 0, top, dstwidth, cy);
                }
                else  // Пропорции исходного изображения более узкие
                {
                    float cx = srcwidth / srcheight * dstheight;
                    float left = ((float)dstwidth - cx) / 2.0f;
                    if (left < 1.0f) left = 0;
                    gr.DrawImage(source, left, 0, cx, dstheight);
                }

                return dest;
            }
        }
    }
}