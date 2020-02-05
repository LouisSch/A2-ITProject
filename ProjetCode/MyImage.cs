using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    class MyImage
    {
        private static string chemin = System.IO.Path.GetFullPath(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory())));

        private string type;
        private int largeur;
        private int hauteur;
        private int taille;
        private int tailleOffset;
        private int bitsPixel;

        private Pixel[,] couleurs;

        #region constructeurs
        public MyImage(string chemin)
        {
            if (File.Exists(MyImage.chemin + chemin))
            {
                byte[] datas = File.ReadAllBytes(MyImage.chemin + chemin);

                if (MyImage.IsBitMap(datas, chemin))
                {
                    this.type = "bmp";
                    ReadHeaderBMP(datas);
                    this.couleurs = new Pixel[this.hauteur, this.largeur];

                    ReadImageBMP(datas);
                }
                else
                {
                    Console.WriteLine("Ce n'est pas une image Bitmap. Construction annulée.");
                }
               
            }
        }
        #endregion

        #region proprietes
        public Pixel[,] Couleurs
        {
            get { return this.couleurs; }
        }
        #endregion

        #region methodes
        private void ReadImageBMP(byte[] datas)
        {
            int index = this.tailleOffset;

            for (int i = 0; i < this.hauteur; i++)
            {
                for (int j = 0; j < this.largeur; j++)
                {
                    this.couleurs[i, j] = new Pixel(i, j, datas[index + 2], datas[index + 1], datas[index]);
                    index += 3;
                }
            }
        }
        
        private void ReadHeaderBMP(byte[] datas)
        {
            // Taille Fichier
            this.taille = ConvertToDec(GetLittleEndian(datas, 2, 4));

            // Offset
            this.tailleOffset = ConvertToDec(GetLittleEndian(datas, 10, 4));

            // largeur
            this.largeur = ConvertToDec(GetLittleEndian(datas, 18, 4));

            // hauteur
            this.hauteur = ConvertToDec(GetLittleEndian(datas, 22, 4));

            // hauteur
            this.bitsPixel = ConvertToDec(GetLittleEndian(datas, 28, 2)) / 8;
        }

        private int ConvertToDec(byte[] datas)
        {
            double result = 0;
            int compteur = 0;

            foreach (byte b in datas)
            {
                result += Math.Pow(256, Convert.ToDouble(compteur)) * b;
                compteur++;
            }

            return Convert.ToInt32(result);
        }

        private byte[] GetLittleEndian(byte[] datas, int k, int longueur)
        {
            int compteur = 0;
            byte[] result = new byte[longueur];

            for (int i = k; i < k+longueur; i++)
            {
                result[compteur] = datas[i];
                compteur++;
            }

            return result;
        }

        public static bool IsBitMap(byte[] datas, string chemin)
        {
            bool result = false;

            if (File.Exists(MyImage.chemin + chemin))
            {
                if (datas[0] == 66 && datas[1] == 77)
                {
                    result = true;
                }
            }

            return result;
        }
        #endregion
    }
}
