using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ProjetCode
{
    class Program
    {
        static void Main(string[] args)
        {
            /*MyImage julia = MyImage.Julia(500, -0.8, 0.156);
            julia.FromImageToFile("/bin/Debug/Exports/JuliaTest.bmp");*/

            /*MyImage image = new MyImage("/bin/Debug/Images/Coco.bmp");
            MyImage histogramme = image.HistogrammeCouleurs();
            histogramme.FromImageToFile("/bin/Debug/Exports/ExportTest.bmp");*/

            //MyImage img = new MyImage("/bin/Debug/Images/coco2.bmp");
            //MyImage crypted = img.CryptWithImage(new MyImage("/bin/Debug/Images/meme.bmp"));
            //crypted.FromImageToFile("/bin/Debug/Images/TestCrypt.bmp");

            //MyImage img = new MyImage("/bin/Debug/Images/TestCrypt.bmp");
            //MyImage decrypt = img.DecryptWithImage(new MyImage("/bin/Debug/Images/meme.bmp"));
            //decrypt.FromImageToFile("/bin/Debug/Exports/test.bmp");

            QRCode qr = new QRCode();

            Console.ReadKey();
        }
    }
}
