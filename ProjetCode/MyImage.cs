using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    /// <summary>
    /// Classe image
    /// </summary>
    public class MyImage
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

        /// <summary>
        /// Constructeur principal de MyImage
        /// </summary>
        /// <param name="chemin">Chemin relatif au dossier du projet, dans lequel se trouve l'image à convertir en objet</param>
        public MyImage(string chemin)
        {
            if (File.Exists(MyImage.chemin + chemin))
            {
                byte[] datas = File.ReadAllBytes(MyImage.chemin + chemin);

                if (IsBitMap(datas))
                {

                    this.type = "bmp";
                    ReadHeaderBMP(datas);
                    this.pixelsImage = new Pixel[this.hauteur, this.largeur];

                    ReadImageBMP(datas);
                }
                else
                {
                    Console.WriteLine("Ce n'est pas une image reconnue. Construction annulée.");
                }
               
            }
        }

        /// <summary>
        /// Constructeur secondaire de MyImage
        /// </summary>
        /// <param name="type">Type de fichier (CSV, BitMap...)</param>
        /// <param name="largeur">Largeur</param>
        /// <param name="hauteur">Hauteur</param>
        /// <param name="taille">Taille du fichier</param>
        /// <param name="tailleOffset">Taille de l'offset de l'header</param>
        public MyImage(string type, int largeur, int hauteur, int taille, int tailleOffset)
        {
            this.type = type;
            this.largeur = largeur;
            this.hauteur = hauteur;
            this.taille = taille;
            this.tailleOffset = tailleOffset;
        }

        #endregion

        #region proprietes
        /// <summary>
        /// Propriétés de la matrice PixelsImage
        /// </summary>
        public Pixel[,] PixelsImage
        {
            get { return this.pixelsImage; }
            set { this.pixelsImage = value; }
        }

        /// <summary>
        /// Propriétés de Largeur
        /// </summary>
        public int Largeur
        {
            get { return this.largeur; }
        }

        /// <summary>
        /// Propriétés de Hauteur
        /// </summary>
        public int Hauteur
        {
            get { return this.hauteur; }
        }
        #endregion

        #region methodes

        /// <summary>
        /// Permet d'exporter une image
        /// </summary>
        /// <param name="chemin">Chemin relatif (par rapport au dossier du projet) avec le nom du fichier et son extension</param>
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

        #region actions

        /// <summary>
        /// Permet de faire une symétrie par rapport à l'horizontale
        /// </summary>
        public void MirrorX()
        {
            Pixel[,] newImage = new Pixel[this.hauteur, this.largeur];

            for (int j = this.pixelsImage.GetLength(1) - 1; j >= 0; j--)
            {
                for (int i = this.pixelsImage.GetLength(0) - 1; i >= 0; i--)
                {
                    newImage[(this.hauteur - 1) - i, (this.largeur - 1) - j] = this.pixelsImage[i, j];
                }
            }

            this.pixelsImage = newImage;
        }

        /// <summary>
        /// Permet de faire une symétrie par rapport à la verticale
        /// </summary>
        public void MirrorY()
        {
            Pixel[,] newImage = new Pixel[this.hauteur, this.largeur];

            for (int i = this.pixelsImage.GetLength(1) - 1; i >= 0; i--)
            {
                for (int j = this.pixelsImage.GetLength(0) - 1; j >= 0; j--)
                {
                    newImage[j, (this.largeur - 1) - i] = this.pixelsImage[j, i];
                }
            }

            this.pixelsImage = newImage;
        }

        /// <summary>
        /// Permet d'effectuer une rotation sur une image
        /// </summary>
        /// <param name="angle">Angle de rotation en degrés</param>
        public MyImage Rotate(double angle)
        {
            int newTaille = this.bitsPixel * 4 * this.hauteur * this.largeur + this.tailleOffset, coordFinX = 0, coordFinY = 0;
            double centeredCoordHauteur = 0, centeredCoordLargeur = 0, newCoordHauteur = 0, newCoordLargeur = 0;

            MyImage newImage = new MyImage(this.type, this.largeur*2, this.hauteur*2, newTaille, this.tailleOffset);
            Pixel[,] image = new Pixel[this.hauteur*2, this.largeur*2];

            angle = ConvertDegreeToRad(angle);

            // On remplit l'image d'arrivée
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    // On recentre les coordonnées
                    centeredCoordHauteur = i - (newImage.Hauteur / 2);
                    centeredCoordLargeur = j - (newImage.Largeur / 2);

                    // Transformation des coordonnées pour obtenir celles de l'image de base
                    newCoordHauteur = Math.Cos(-1 * angle) * centeredCoordHauteur + Math.Sin(-1 *angle) * centeredCoordLargeur;
                    newCoordLargeur = Math.Cos(-1 * angle) * centeredCoordLargeur - Math.Sin(-1 * angle) * centeredCoordHauteur;

                    // On remet les coordonnées sur le repère de base de l'image originale
                    coordFinX = (int)(Math.Floor(newCoordHauteur) + this.hauteur / 2);
                    coordFinY = (int)(Math.Floor(newCoordLargeur) + this.largeur / 2);
                     
                    // On complète si possible avec les pixels
                    if (coordFinX >= 0 && coordFinX < this.hauteur && coordFinY >= 0 && coordFinY < this.largeur)
                    {
                        image[i, j] = this.pixelsImage[coordFinX, coordFinY];
                    }
                }
            }

            CompleteWithBlackPixels(image);
            newImage.PixelsImage = image;

            return newImage;
        }

        /// <summary>
        /// Permet de réduire la taille d'une image
        /// </summary>
        /// <param name="ratio">Le ratio pour réduire la largeur et la hauteur</param>
        /// <returns>Une nouvelle instance contenant la nouvelle image</returns>
        public MyImage Minimize(int ratio)
        {
            double initNewHauteur = this.hauteur / ratio, initNewLargeur = this.largeur / ratio;
            int[] dimensions = new int[2] { (int)initNewHauteur, (int)initNewLargeur };

            // Ajustement des dimensions de l'image pour qu'elles soient divisibles pas 4
            ToFormat4(dimensions);

            int averageR = 0, averageG = 0, averageB = 0, newTaille = (dimensions[0] * dimensions[1] * this.bitsPixel) + this.tailleOffset;

            MyImage newImage = new MyImage(this.type, dimensions[1], dimensions[0], newTaille, this.tailleOffset);
            Pixel[,] image = new Pixel[newImage.Hauteur, newImage.Largeur];

            // On parcourt toute la matrice en faisant des carrées de côté ratio
            for (int i = 0; i < (initNewHauteur * ratio); i += ratio)
            {
                for (int j = 0; j < (initNewLargeur * ratio); j += ratio)
                {
                    // On fait la moyenne sur le R, V et B (respectivement) des pixels du carré
                    for (int k = 0; k < ratio; k++)
                    {
                        for (int l = 0; l < ratio; l++)
                        {
                            averageR += this.pixelsImage[i + k, j + l].Red;
                            averageG += this.pixelsImage[i + k, j + l].Green;
                            averageB += this.pixelsImage[i + k, j + l].Blue;
                        }
                    }

                    averageR /= (ratio * ratio);
                    averageG /= (ratio * ratio);
                    averageB /= (ratio * ratio);

                    image[i / ratio, j / ratio] = new Pixel(i / ratio, j / ratio, averageR, averageG, averageB);

                    averageR = 0;
                    averageG = 0;
                    averageB = 0;
                }
            }

            // Si jamais les dimensions initiales n'étaient pas divisibles pas 4, on comble les trous par des pixels noirs
            if ((int)initNewHauteur != dimensions[0] && (int)initNewLargeur != dimensions[1])
            {
                CompleteWithBlackPixels(image);
            }

            newImage.PixelsImage = image;

            return newImage;
        }

        /// <summary>
        /// Permet d'agrandir la taille d'une image
        /// </summary>
        /// <param name="ratio">Le ratio pour augmenter la largeur et la hauteur</param>
        /// <returns>Une nouvelle instance contenant la nouvelle image</returns>
        public MyImage Maximize(int ratio)
        {
            int newHauteur = this.hauteur * ratio, newLargeur = this.largeur * ratio, newTaille = (newHauteur * newLargeur * this.bitsPixel) + this.tailleOffset;
            MyImage newImage = new MyImage(this.type, newLargeur, newHauteur, newTaille, this.tailleOffset);
            Pixel[,] image = new Pixel[newImage.Hauteur, newImage.Largeur];

            for (int i = 0; i < image.GetLength(0); i += ratio)
            {
                for (int j = 0; j < image.GetLength(1); j += ratio)
                {
                    for (int k = 0; k < ratio; k++)
                    {
                        for (int l = 0; l < ratio; l++)
                        {
                            image[i + k, j + l] = this.pixelsImage[i / ratio, j / ratio];
                        }
                    }
                }
            }

            newImage.PixelsImage = image;

            return newImage;
        }

        /// <summary>
        /// Permet de passer l'image en nuances de gris
        /// </summary>
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

        /// <summary>
        /// Permet de passer l'image en noir et blanc
        /// </summary>
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
        /// <summary>
        /// Permet d'écrire un fichier CSV
        /// </summary>
        /// <param name="datas">Le tableau de données qui devra être écrit</param>
        /// <param name="chemin">Le chemin où le fichier sera écrit</param>
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

        /// <summary>
        /// Permet d'écrire un fichier BitMap
        /// </summary>
        /// <param name="datas">Le tableau de données qui devra être écrit</param>
        /// <param name="chemin">Le chemin où le fichier sera écrit</param>
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
        
        /// <summary>
        /// Permet de lire l'header d'une image en format CSV
        /// </summary>
        /// <param name="chemin">Chemin vers le fichier cible</param>
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

        /// <summary>
        /// Permet de lire les octets d'une image BMP
        /// </summary>
        /// <param name="datas">Le tableau de données dans lequel se trouvent les données de l'image</param>
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

        /// <summary>
        /// Permet de lire l'header d'une image en format BMP
        /// </summary>
        /// <param name="datas">Le tableau de données dans lequel se trouvent les données du header</param>
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

        /// <summary>
        /// Permet de construire un header BMP à partir des champs de classe
        /// </summary>
        /// <param name="datas">Le tableau de données dans lequel le header va être ajouté</param>
        /// <returns></returns>
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

        /// <summary>
        /// Permet d'insérer un tableau de byte en little endian
        /// </summary>
        /// <param name="datas">Le tableau dans lequel va être inséré le little endian</param>
        /// <param name="littleEndian">Little endian à insérer</param>
        /// <param name="begin">Indexe à laquelle l'insérer</param>
        private void InsertLittleEndian(byte[] datas, byte[] littleEndian, int begin)
        {
            for (int i = begin; i < (begin + littleEndian.Length); i++)
            {
                datas[i] = littleEndian[i - begin];
            }
        }

        /// <summary>
        /// Permet d'ajouter une succession d'octets dans un tableau
        /// </summary>
        /// <param name="datas">Tableau dans lequel ajouter les octets</param>
        /// <param name="b">Byte à ajouter</param>
        /// <param name="begin">Indexe à laquelle commencer</param>
        /// <param name="number">Nombre de fois que l'on doit répéter l'octet</param>
        public static void CompleteWith(byte[] datas, byte b, int begin, int number)
        {
            for (int i = begin; i < (begin + number); i++)
            {
                datas[i] = b;
            }
        }

        /// <summary>
        /// Permet de convertir Un décimal en little endian
        /// </summary>
        /// <param name="number">Nombre à convertir</param>
        /// <param name="size">Sur combien d'octets le décimal doit être écrit</param>
        /// <returns></returns>
        public static byte[] ConvertDecToLE(int number, int size)
        {
            byte[] result = new byte[size];

            for (int i = (size - 1); i >= 0; i--)
            {
                result[i] = (byte)(number / (Math.Pow(256, Convert.ToDouble(i))));
            }

            return result;
        }

        /// <summary>
        /// Permet de convertir un little endian en décimal
        /// </summary>
        /// <param name="datas">Tableau contenant les octets à convertir</param>
        /// <returns>Décimal converti depuis le little endian</returns>
        public static int ConvertLEToDec(byte[] datas)
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

        /// <summary>
        /// Permet de vérifier si le fichier est bien un BitMap
        /// </summary>
        /// <param name="datas">Tableau de données du fichier</param>
        /// <returns>True si c'est un bitmap, false sinon</returns>
        public static bool IsBitMap(byte[] datas)
        {
            bool result = false;

            if (datas[0] == 66 && datas[1] == 77)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Permet de récupérer une partie de code en format little endian
        /// </summary>
        /// <param name="datas">Tableau de données</param>
        /// <param name="k">Indexe à laquelle on commence à extraire les données</param>
        /// <param name="longueur">Nombre d'octets du code en little endian</param>
        /// <returns>Tableau d'octets avec le code en format little endian</returns>
        public static byte[] GetLittleEndian(byte[] datas, int k, int longueur)
        {
            int compteur = 0;
            byte[] result = new byte[longueur];

            for (int i = k; i < k + longueur; i++)
            {
                result[compteur] = datas[i];
                compteur++;
            }

            return result;
        }

        /// <summary>
        /// Permet de compléter les pixels vides d'une image par des pixels noirs
        /// </summary>
        /// <param name="image">Matrice contenant les pixels de l'image</param>
        private void CompleteWithBlackPixels(Pixel[,] image)
        {
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    if (image[i, j] == null)
                        image[i, j] = new Pixel(i, j, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Permet de rendre les dimensions d'une image divisibles par 4
        /// </summary>
        /// <param name="dimensions">Dimensions à (éventuellement) modifier</param>
        private void ToFormat4(int[] dimensions)
        {
            while ((dimensions[0] % 4) != 0 || (dimensions[1] % 4) != 0)
            {
                if ((dimensions[0] % 4) != 0)
                {
                    dimensions[0]++;
                }

                if ((dimensions[1] % 4) != 0)
                {
                    dimensions[1]++;
                }
            }
        }

        /// <summary>
        /// Permet de convertir de degrés en radians
        /// </summary>
        /// <param name="angle">Angle à convertir</param>
        /// <returns>L'angle en radians</returns>
        private double ConvertDegreeToRad(double angle)
        {
            return (angle * Math.PI) / 180;
        }
        #endregion

        #endregion
    }
}
