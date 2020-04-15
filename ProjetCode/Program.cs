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
            MyImage image = new MyImage("/bin/Debug/Images/Coco.bmp");
            MyImage blur = image.Repoussage();
            blur.FromImageToFile("/bin/Debug/Exports/Repouss.bmp");

            /*MyImage histogramme = image.HistogrammeCouleurs('B');
            histogramme.FromImageToFile("/bin/Debug/Exports/ExportTest.bmp");*/

            Console.ReadKey();
        }
    }
}
