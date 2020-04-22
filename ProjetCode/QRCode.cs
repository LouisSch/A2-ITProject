using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    class QRCode
    {
        private int version = 0;
        private int rapportPixelModule = 1;
        private MyImage image = null;
        private Dictionary<char, int> codesAlphaNum = new Dictionary<char, int>();

        #region constructeurs

        public QRCode(int version, string messageToEncode)
        {
            int dimImage = 0;

            InitializeDictionary();

            switch (version)
            {
                case 1:
                    dimImage = 21;
                    break;
                case 2:
                    this.version = 2;
                    dimImage = 25;
                    break;
                default:
                    dimImage = 21;
                    break;
            }

            dimImage = MyImage.ToFormat4(dimImage);

            this.image = new MyImage("bmp", dimImage, dimImage, dimImage * dimImage * 3 + 54, 54);
            Pixel[,] pixelsImage = new Pixel[dimImage, dimImage];
        }
        
        #endregion

        #region tools

        private void InitializeDictionary()
        {
            string alphaNum = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";
            int compteur = 0;

            foreach (char c  in alphaNum)
            {
                this.codesAlphaNum.Add(c, compteur);
                compteur++;
            }
        }

        #endregion
    }
}
