using OfficeOpenXml;
using Store.Domain.Abstract;
using Store.Domain.Concrete;
using Store.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelChecker
{
    public partial class Form1 : Form
    {
        public static Form1 frm1;
        public Form1()
        {
            InitializeComponent();
            frm1 = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            richTextBox1.Clear();
            string fname = openFileDialog1.FileName;
            Parser1 prser = new Parser1(fname, Path.GetDirectoryName(fname));
            prser.Parse();
        }

        public void showmsg(string msg)
        {
            richTextBox1.AppendText("\r\n" + msg);
        }

    }

    public class Parser1
    {
        ExcelPackage pack;
        string path;
        string dirpath;
        ExcelWorksheet workSheet;
        public Parser1(string filepath, string dir)
        {
            path = filepath;
            dirpath = dir;
            FileInfo fi = new FileInfo(path);
            try
            {
                pack = new ExcelPackage(fi);
            }
            catch (Exception e)
            {
                Form1.frm1.showmsg("Ошибка при открытии файла. Скорее всего файл уже открыт в другой программе");
            }
        }

        public void Parse()
        {
            try
            {
                if (pack != null)
                {
                    var currentSheet = pack.Workbook.Worksheets;
                    workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;

                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        try
                        {
                            for (int i = 1; i <= 17; i++)
                            {
                                if (workSheet.Cells[rowIterator, i].Value == null)
                                {
                                    throw new Exception("Колонка " + i + " пуста");
                                }
                                switch (i)
                                {
                                    case 17:
                                        string photocell = workSheet.Cells[rowIterator, i].Value.ToString();
                                        if (photocell.Length != 36)
                                        {
                                            throw new Exception("В колонке фото больше или меньше 36 знаков. Если фото больше одного, тогда всё в порядке");
                                        }
                                        break;
                                }
                            }

                            Item item = new Item
                            {
                                Brand = workSheet.Cells[rowIterator, 4].Value.ToString(),
                                Type = workSheet.Cells[rowIterator, 1].Value.ToString(),
                                article = workSheet.Cells[rowIterator, 6].Value.ToString(),
                                Name = workSheet.Cells[rowIterator, 7].Value.ToString(),
                                IsHot = false,
                                Price = Convert.ToInt32(workSheet.Cells[rowIterator, 8].Value),
                                CountInPack = workSheet.Cells[rowIterator, 11].Value.ToString(),
                                PriceUnit = workSheet.Cells[rowIterator, 9].Value.ToString(),
                                Country = workSheet.Cells[rowIterator, 3].Value.ToString(),
                                Purpose = workSheet.Cells[rowIterator, 12].Value.ToString(),
                                Surface = workSheet.Cells[rowIterator, 13].Value.ToString(),
                                Picture = workSheet.Cells[rowIterator, 14].Value.ToString(),
                                Color = workSheet.Cells[rowIterator, 15].Value.ToString(),
                                Size = workSheet.Cells[rowIterator, 16].Value.ToString(),
                            };

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
                            item.SizeInM2 = (double)((double.Parse(m.Groups[1].Value) * double.Parse(m.Groups[2].Value)) / 10000);

                            item.PriceForM2 = item.PriceUnit.ToLower().Contains("м2") ? true : false;

                            item.ItemType = "keram";

                            string oip = workSheet.Cells[rowIterator, 10].Value.ToString();
                            if (oip == "+")
                            {
                                item.OnlyInPacks = true;
                            }
                            else
                            {
                                item.OnlyInPacks = false;
                            }

                            string[] hierarchy = new string[] { workSheet.Cells[rowIterator, 1].Value.ToString(), //тип
                        workSheet.Cells[rowIterator, 2].Value.ToString(), //применение
                        workSheet.Cells[rowIterator, 3].Value.ToString(), //страна
                        workSheet.Cells[rowIterator, 4].Value.ToString(), //производитель
                    };
                            hierarchy = hierarchy.Concat(workSheet.Cells[rowIterator, 5].Value.ToString().Split(',')).ToArray();

                            string[] TranslitNames = new string[hierarchy.Count()];
                            hierarchy.CopyTo(TranslitNames, 0);

                            for (int i = 0; i < hierarchy.Count(); i++)
                            {
                                hierarchy[i] = hierarchy[i].First().ToString().ToUpper() + hierarchy[i].Substring(1);
                            }

                            string ImgPath = dirpath + "/Images";

                            var imagesnames = Directory.EnumerateFiles(ImgPath, "*.*", SearchOption.AllDirectories);

                            //string[] imgsames = workSheet.Cells[rowIterator,16].Value.

                            string[] str = workSheet.Cells[rowIterator, 17].Value.ToString().Split(',').ToArray();


                            var imagesphys = new string[] { };

                            foreach (var st in str)
                                imagesphys = imagesphys.Union(imagesnames.Where(x => x.ToLower().Contains(st.ToLower())
                                    && !x.ToLower().Contains("-mini"))).ToArray();

                            if (imagesphys.Count() == 0)
                            {
                                throw new Exception("Не удается найти фото! Проверьте наличие файла, совпадение имени файла и записи в таблице.");
                            }

                            Image img = Image.FromFile(imagesphys[0]);

                            List<string> images = new List<string>();

                            for (int i = 0; i < imagesphys.Count(); i++)
                            {
                                images.Add(imagesphys.ElementAt(i).Replace(ImgPath, String.Empty));
                            }
                        }
                        catch (Exception e)
                        {
                            string msg = "Ошибка в строке " + rowIterator + "\r\n" + e.Message + "\r\n";
                                
                            Form1.frm1.showmsg(msg);
                        }
                    }
                    pack.Dispose();
                }
            }
            catch (Exception e)
            {
                Form1.frm1.showmsg(e.Message);
            }
        }
    }
}
