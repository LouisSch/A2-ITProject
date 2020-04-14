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
            /*MyImage image = new MyImage("/bin/Debug/Images/testLol.bmp");
            MyImage newImage = image.Rotate(76);
            newImage.FromImageToFile("/bin/Debug/Exports/ExportTest.bmp");*/

            MyImage MandelBrot = MyImage.MandelBrot(100);
            MandelBrot.FromImageToFile("/bin/Debug/Exports/ExportMandelbrot.bmp");

            Console.ReadKey();
        }
    }
}
