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
        private int tailleOffset = 54;
        private int bitsPixel = 3;

        private Pixel[,] pixelsImage;

        #region constructeurs
        public MyImage(string chemin)
        {
            if (File.Exists(MyImage.chemin + chemin))
            {

                if (Path.GetExtension(MyImage.chemin + chemin) == ".bmp")
                {
                    byte[] datas = File.ReadAllBytes(MyImage.chemin + chemin);

                    this.type = "bmp";
                    ReadHeaderBMP(datas);
                    this.pixelsImage = new Pixel[this.hauteur, this.largeur];

                    ReadImageBMP(datas);
                }else if (Path.GetExtension(MyImage.chemin + chemin) == ".csv")
                {
                    this.type = "csv";
                    ReadHeaderCSV(chemin);

                    this.pixelsImage = new Pixel[this.hauteur, this.largeur];

                    ReadImageCSV(chemin);
                }
                else
                {
                    Console.WriteLine("Ce n'est pas une image reconnue. Construction annulée.");
                }
               
            }
        }
        #endregion

        #region proprietes
        public Pixel[,] Couleurs
        {
            get { return this.pixelsImage; }
        }
        #endregion

        #region methodes

        public void FromImageToFile(string chemin)
        {
            byte[] datas = new byte[this.taille];
            string extensionExport = Path.GetExtension(chemin);

            // Construction en-tête
            switch (this.type)
            {
                case "BMP":
                    datas = BuildBMPHeader(datas);
                    break;
                default:
                    datas = BuildBMPHeader(datas);
                    break;
            }

            // Ecriture
            switch (extensionExport)
            {
                case ".bmp":
                    WriteBMP(datas, chemin);
                    break;
                case ".csv":
                    WriteCSV(datas, chemin);
                    break;
            }

            
        }

        #region filters
        public void GreyShadesFilter()
        {
            int temp;

            for (int i = 0; i < pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < pixelsImage.GetLength(1); j++)
                {
                    temp = Convert.ToByte((this.pixelsImage[i, j].Red + this.pixelsImage[i, j].Green + this.pixelsImage[i, j].Blue) / 3);

                    this.pixelsImage[i, j].Red = temp;
                    this.pixelsImage[i, j].Green = temp;
                    this.pixelsImage[i, j].Blue = temp;
                }
            }
        }

        public void BlackAndWhiteFilter()
        {
            byte temp;
            int med = 127;

            for (int i = 0; i < pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < pixelsImage.GetLength(1); j++)
                {
                    temp = Convert.ToByte((this.pixelsImage[i, j].Red + this.pixelsImage[i, j].Green + this.pixelsImage[i, j].Blue) / 3);

                    if (temp <= med)
                    {
                        this.pixelsImage[i, j].Red = 0;
                        this.pixelsImage[i, j].Green = 0;
                        this.pixelsImage[i, j].Blue = 0;
                    }
                    else
                    {
                        this.pixelsImage[i, j].Red = 255;
                        this.pixelsImage[i, j].Green = 255;
                        this.pixelsImage[i, j].Blue = 255;
                    }

                }
            }
        }
        #endregion

        #region write
            private void WriteCSV(byte[] datas, string chemin)
        {
            StreamWriter strWriter = new StreamWriter(MyImage.chemin + chemin);
            string line = "";

            for (int i = 0; i < this.pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixelsImage.GetLength(0); j++)
                {
                    line += this.pixelsImage[i, j].Red + ";";
                    line += this.pixelsImage[i, j].Green + ";";
                    line += this.pixelsImage[i, j].Blue + ";";
                }

                strWriter.WriteLine(line);
                line = "";
            }

            strWriter.Close();
        }

        private void WriteBMP(byte[] datas, string chemin)
        {
            FileStream fs = new FileStream(MyImage.chemin + chemin, FileMode.Create, FileAccess.Write);
            int index = this.tailleOffset;

            // Construction des données
            for (int i = 0; i < this.pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixelsImage.GetLength(1); j++)
                {
                    datas[index] = Convert.ToByte(this.pixelsImage[i, j].Blue);
                    datas[index + 1] = Convert.ToByte(this.pixelsImage[i, j].Green);
                    datas[index + 2] = Convert.ToByte(this.pixelsImage[i, j].Red);

                    index += 3;
                }
            }

            // Ajout des lignes
            fs.Write(datas, 0, datas.Length);
            fs.Close();
        }
        #endregion

        #region read
        private void ReadImageCSV(string chemin)
        {
            StreamReader strReader = new StreamReader(MyImage.chemin + chemin);
            string line;

            while ((line = strReader.ReadLine()) != null)
            {

            }
        }

        private void ReadHeaderCSV(string chemin)
        {
            StreamReader strReader = new StreamReader(MyImage.chemin + chemin);
            int hauteur = 0, largeur = 0;
            string line;

            while ((line = strReader.ReadLine()) != null)
            {
                hauteur++;

                if (largeur == 0)
                    largeur = line.Split(new char[] { ';' }).Length;
            }

            this.largeur = largeur / 3;
            this.hauteur = hauteur;
            this.taille = largeur * hauteur + this.tailleOffset;

            strReader.Close();
        }

        private void ReadImageBMP(byte[] datas)
        {
            int index = this.tailleOffset;

            for (int i = 0; i < this.hauteur; i++)
            {
                for (int j = 0; j < this.largeur; j++)
                {
                    this.pixelsImage[i, j] = new Pixel(i, j, datas[index + 2], datas[index + 1], datas[index]);
                    index += 3;
                }
            }
        }
        
        private void ReadHeaderBMP(byte[] datas)
        {
            // Taille Fichier
            this.taille = ConvertLEToDec(GetLittleEndian(datas, 2, 4));

            // Offset
            this.tailleOffset = ConvertLEToDec(GetLittleEndian(datas, 10, 4));

            // largeur
            this.largeur = ConvertLEToDec(GetLittleEndian(datas, 18, 4));

            // hauteur
            this.hauteur = ConvertLEToDec(GetLittleEndian(datas, 22, 4));

            // hauteur
            this.bitsPixel = ConvertLEToDec(GetLittleEndian(datas, 28, 2)) / 8;
        }
        #endregion

        #region header
        private byte[] BuildBMPHeader(byte[] datas)
        {
            byte[] taille = ConvertDecToLE(this.taille, 4),
                tailleOffset = ConvertDecToLE(this.tailleOffset, 4),
                largeur = ConvertDecToLE(this.largeur, 4),
                hauteur = ConvertDecToLE(this.hauteur, 4),
                bitsPixel = ConvertDecToLE(this.bitsPixel * 8, 2),
                headerInfoSize = ConvertDecToLE(40, 4);

            datas[0] = 66;
            datas[1] = 77;
            InsertLittleEndian(datas, taille, 2);
            CompleteWith(datas, 0, 6, 4);

            InsertLittleEndian(datas, tailleOffset, 10);
            InsertLittleEndian(datas, headerInfoSize, 14);

            InsertLittleEndian(datas, largeur, 18);
            InsertLittleEndian(datas, hauteur, 22);
            datas[26] = 1;
            datas[27] = 0;

            InsertLittleEndian(datas, bitsPixel, 28);
            CompleteWith(datas, 0, 32, 22);

            return datas;
        }
        #endregion

        #region tools
        private byte[] ConvertDecToLE(int number, int size)
        {
            byte[] result = new byte[size];

            for (int i = (size - 1); i >= 0;  i--)
            {
                result[i] = (byte) (number / (Math.Pow(256, Convert.ToDouble(i))));
            }

            return result;
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

        private void InsertLittleEndian(byte[] datas, byte[] littleEndian, int begin)
        {
            for (int i = begin; i < (begin + littleEndian.Length); i++)
            {
                datas[i] = littleEndian[i - begin];
            }
        }

        private void CompleteWith(byte[] datas, byte b, int begin, int number)
        {
            for (int i = begin; i < (begin + number); i++)
            {
                datas[i] = b;
            }
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

        private int ConvertLEToDec(byte[] datas)
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
        #endregion

        #endregion
    }
}
