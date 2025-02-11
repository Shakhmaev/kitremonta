﻿using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
                            Brand = workSheet.Cells[rowIterator, 3].Value.ToString(),
                            Type = workSheet.Cells[rowIterator, 1].Value.ToString().Split(',').Last(),
                            Name = workSheet.Cells[rowIterator, 6].Value.ToString(),
                            IsHot = false,
                            Price = Convert.ToInt32(workSheet.Cells[rowIterator, 7].Value),
                            CountInPack = workSheet.Cells[rowIterator, 9].Value.ToString(),
                            Country = workSheet.Cells[rowIterator, 2].Value.ToString(),
                            Purpose = workSheet.Cells[rowIterator, 10].Value.ToString().ToLower(),
                            Surface = workSheet.Cells[rowIterator, 11].Value.ToString().ToLower(),
                            Picture = workSheet.Cells[rowIterator, 12].Value.ToString().ToLower(),
                            Color = workSheet.Cells[rowIterator, 13].Value.ToString().ToLower(),
                            Size = workSheet.Cells[rowIterator, 14].Value.ToString(),
                            Weight = workSheet.Cells[rowIterator, 16].Value.ToString()
                        };

                        item.article = workSheet.Cells[rowIterator, 5].Value != null ? workSheet.Cells[rowIterator, 5].Value.ToString() : item.Brand.Substring(0,2) + "-" + Guid.NewGuid().ToString("N");


                        var pru = workSheet.Cells[rowIterator, 8].Value.ToString();
                        item.PriceUnit = pru.Contains("шт") ? "шт" : "м2";

                        string inpack = item.CountInPack.Replace('.', ',');
                        Regex reginpack = new Regex(@"^([0-9]+[,]*[0-9]*)*(.+[/\\]\s*)*([0-9]*)(.*)*");
                        Match m = reginpack.Match(inpack);

                        double m2 = 0;
                        int sht = 0;
                        if (m.Groups[3].Value != "")
                        {
                            bool m2p = double.TryParse(m.Groups[1].ToString(), out m2); ;
                            bool shtp = int.TryParse(m.Groups[3].ToString(), out sht);
                            if (m2p) item.m2 = m2; else item.m2 = 0;
                            if (shtp) item.sht = sht; else item.sht = 0;
                        }
                        else
                        {
                            int.TryParse(m.Groups[1].ToString(), out sht);
                            item.sht = sht;
                        }

                        Regex sizeregx = new Regex(@"^([0-9]+[,]*[0-9]*)[xXхХ×*]([0-9]+[,]*[0-9]*)");
                        m = sizeregx.Match(item.Size.Replace('.', ','));
                        if (m.Success)
                        {
                            item.SizeInM2 = (double)((double.Parse(m.Groups[1].Value) * double.Parse(m.Groups[2].Value)) / 10000);
                        }
                        item.Size = Regex.Replace(item.Size,"[xXхХ×*]","x");

                        item.PriceForM2 = item.PriceUnit.ToLower().Contains("м2") ? true : false;

                        item.ItemType = "keram";

                        //string oip = workSheet.Cells[rowIterator, 10].Value.ToString();
                        if (workSheet.Cells[rowIterator, 8].Value.ToString().Contains("м2"))
                        {
                            item.OnlyInPacks = true;
                        }
                        else
                        {
                            item.OnlyInPacks = false;
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

                            repos.SaveOrUpdateItemFromXlsOne(item, hierarchylist, translitnameslist, images);
                        }
                        else repos.SaveOrUpdateItemFromXlsOne(item, hierarchylist, translitnameslist, new List<string>());
                    }
                    pack.Dispose();
                }
                catch (Exception e)
                {
                    messages.Add(e.Message + e.Source);
                }
            }
            return messages;
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