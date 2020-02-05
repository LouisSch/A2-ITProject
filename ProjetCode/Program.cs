using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    class Program
    {
        static void Main(string[] args)
        {
            MyImage image = new MyImage("/bin/Debug/Images/Test.bmp");
            Console.ReadKey();
        }
    }
}
