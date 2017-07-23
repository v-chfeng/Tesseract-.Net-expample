using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using System.IO;
using System.Runtime.InteropServices;

namespace TestTesseract
{
    class Program
    {
        [DllImport("main.dll")]
        extern static int demomain(int parasCnt, string output, string[] paras);

        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(".\\PDF\\", "*.pdf");

            foreach (var str in files)
            {
                if (PDFToImage(str))
                {
                    ImageToTxt(str);
                }
            }
        }

        static bool PDFToImage(string filePath)
        {
            MagickReadSettings settings = new MagickReadSettings();
            // Settings the density to 300 dpi will create an image with a better quality
            settings.Density = new Density(300, 300);

            using (MagickImageCollection images = new MagickImageCollection())
            {
                // Add all the pages of the pdf file to the collection
                images.Read(filePath, settings);

                string name = Path.GetFileName(filePath);

                int page = 1;
                foreach (MagickImage image in images)
                {
                    // Write page to file that contains the page number
                    image.Write(name + page + ".png");
                    // Writing to a specific format works the same as for a single image
                    image.Format = MagickFormat.Ptif;
                    image.Write(name + page + ".tif");
                    page++;
                }
            }

            return true;
        }

        static bool ImageToTxt(string imagePrefixName)
        {
            string[] pngFiles = Directory.GetFiles(".\\PDF\\", imagePrefixName + "*.png");
            string[] paras = new string[] { "demomain", @".\Data\input1.png", @".\output1.txt", "-l", "chi_sim", "--tessdata-dir", ".\\" };

            string txtPath = ".\\PDF\\" + imagePrefixName;
            int i = 0;
            foreach (var png in pngFiles)
            {
                string outPath = txtPath + i.ToString() + ".txt";
                StreamWriter sw = File.CreateText(outPath);
                sw.Close();

                int result = 1;
                try
                {
                    result = demomain(paras.Count(), outPath, paras);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.Read();
                }

                if (result == 0)
                {
                    //read output.txt
                    Console.ReadKey();
                }
            }

            return true;
        }
    }
}
