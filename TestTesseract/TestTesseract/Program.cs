using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace TestTesseract
{
    public class Program
    {
        [DllImport("main.dll")]
        extern static int demomain(int parasCnt, string[] paras);

        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(".\\PDF\\", "*.pdf");

            try
            {
                foreach (var str in files)
                {
                    if (PDFToImage(str))
                    {
                        ImageToTxt(str);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static bool PDFToImage(string filePath)
        {
            MagickReadSettings settings = new MagickReadSettings();
            // Settings the density to 300 dpi will create an image with a better quality
            settings.Density = new Density(300, 300);
            settings.BackgroundColor = new MagickColor("white");
            settings.CompressionMethod = CompressionMethod.NoCompression;

            using (MagickImageCollection images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(filePath, settings);

                string name = Path.GetFileName(filePath);

                int page = 1;
                foreach (MagickImage image in images)
                {
                    image.BorderColor = new MagickColor("white");
                    image.HasAlpha = false;
                    // Write page to file that contains the page number

                    string pngName = ".\\PDF\\" + name + page + ".png";
                    image.Write(pngName);

                    RemoveBackGroud(pngName);
                    // Writing to a specific format works the same as for a single image
                    //image.Format = MagickFormat.Ptif;
                    //image.Write(".\\PDF\\" + name + page + ".tif");
                    page++;
                }
            }

            return true;
        }

        public static void RemoveBackGroud(string pngFile)
        {
            using (Bitmap bitmap = (Bitmap)Image.FromFile(pngFile))
            using (Bitmap newBitmap = Process(bitmap))
            {
                string filename = Path.GetFileName(pngFile);

                int first = filename.LastIndexOf('.');

                string newName = ".\\PDF\\" + filename.Remove(first, filename.Length - first) + "Remove.png";

                newBitmap.Save(newName, ImageFormat.Png);
            }
        }
        static Bitmap Process(Bitmap bitmap)
        { 
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)

                {
                    //去掉边框  
                    if (x == 0 || y == 0 || x == bitmap.Width - 1 || y == bitmap.Height - 1)
                    {
                        newBitmap.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        Color color = bitmap.GetPixel(x, y);

                        bool isNoise = false;

                        if (color.Equals(Color.FromArgb(0, 0, 0)))
                        {
                            if (x + 3 < bitmap.Width)
                            {
                                Color color1 = bitmap.GetPixel(x+ 1, y);
                                Color color2 = bitmap.GetPixel(x+ 1, y);

                                if (color1.Equals(Color.FromArgb(255, 255, 255)) || color2.Equals(Color.FromArgb(255, 255, 255)))
                                {
                                    isNoise = true;
                                }
                            }
                        }

                        //如果点的颜色是背景干扰色，则变为白色  
                        if (isNoise)
                        {
                            newBitmap.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            newBitmap.SetPixel(x, y, color);
                        }
                    }
                }
            }

            return newBitmap;
        }

        static bool IsNoise(Color ponit)
        {
            bool isNoise = false;
            if (IsMiddle(ponit.R) && IsMiddle(ponit.G) && IsMiddle(ponit.B))
            {
                isNoise = true;
            }

            return isNoise;
        }

        static bool IsMiddle(byte b)
        {
            bool isMiddle = false;
            byte low = 60;
            byte high = 116;
            if (b < high && b > low)
            {
                isMiddle = true;
            }

            return isMiddle;
        }

        static bool ImageToTxt(string pdfname)
        {
            string imagePrefixName = Path.GetFileName(pdfname);
            string[] pngFiles = Directory.GetFiles(".\\PDF\\", imagePrefixName + "*Remove.png");
            //new string[] { "demomain", @".\Data\input1.png", @".\output1.txt", "-l", "chi_sim", "--tessdata-dir", ".\\" };

            string txtPath = ".\\PDF\\" + imagePrefixName;
            int i = 0;
            foreach (var png in pngFiles)
            {
                string outPath = txtPath + i.ToString() + ".txt";
                string[] paras = new string[] { "", png, outPath, "eng+chi_sim"};
                
                StreamWriter sw = File.CreateText(outPath);
                sw.Close();

                int result = 1;
                try
                {
                    result = demomain(paras.Count(), paras);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Read();
                }

                if (result == 0)
                {
                    //read output.txt
                    TxtToData(outPath);
                }

                i++;
            }

            return true;
        }

        static bool TxtToData(string filepath)
        {
            string[] strs = File.ReadAllLines(filepath);
            return true;
        }
    }
}
