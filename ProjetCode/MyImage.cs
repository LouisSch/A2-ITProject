using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

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

        /// <summary>
        ///  Propriétés de tailleOffset
        /// </summary>
        public int TailleOffset
        {
            get { return this.tailleOffset; }
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

            if (this.pixelsImage != null) {
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
            }else
            {
                Console.WriteLine("Impossible d'exporter l'image : objet null.");
            }
        }

        #region algo

        /// <summary>
        /// Algorithm permettant de créer une image avec la fractale de Mandelbrot
        /// </summary>
        /// <param name="iterationMax">Nombre d'itérations maximum pour le calcul de la convergence</param>
        /// <returns>Une image contenant la fractale de Mandelbrot</returns>
        public static MyImage MandelBrot(int iterationMax)
        {
            double minRe = -2.0, maxRe = 1.0, minIm = -1.2, maxIm = 1.2;

            // Ratio entre le plan et l'image
            int ratio = 500,
                fractLargeur = (int)((maxRe - minRe) * ratio),
                fractHauteur = (int)((maxIm - minIm) * ratio);

            // Ratio pour avoir les coordonées à l'échelle de base
            double imRatio = (maxIm - minIm) / (fractHauteur - 1),
                   reRatio = (maxRe - minRe) / (fractLargeur - 1);

            while ((fractLargeur % 4) != 0)
            {
                fractLargeur++;
            }

            MyImage fractale = new MyImage("bmp", fractLargeur, fractHauteur, fractLargeur*fractHauteur*3 + 54, 54);
            Pixel[,] fractalePixels = new Pixel[fractHauteur, fractLargeur];

            for (int x = 0; x < fractLargeur; x++)
            {
                for (int y = 0; y < fractHauteur; y++)
                {
                    Complex c = new Complex(minRe + x * reRatio, maxIm - y * imRatio),
                            z = new Complex(0, 0);
                    int compteur = 0;

                    do
                    {
                        z = z*z + c;
                        compteur++;
                    } while ((compteur < iterationMax) && (z.Magnitude < 4));

                    if (compteur == iterationMax)
                    {
                        fractalePixels[y, x] = new Pixel(y, x, 0, 0, 0);
                    }else
                    {
                        fractalePixels[y, x] = new Pixel(y, x, (byte)(compteur + 20 - Math.Log(Math.Log10(z.Magnitude))), (byte)(compteur + 20 - Math.Log(Math.Log10(z.Magnitude))), 40);
                    }
                }
            }

            fractale.PixelsImage = fractalePixels;

            Console.WriteLine("Done");

            return fractale;
        }

        /// <summary>
        /// Algorithme permettant de créer une image avec la fractale de Julia.
        /// </summary>
        /// <param name="iterationMax">Nombre d'itérations maximum pour le calcul de la convergence.</param>
        /// <param name="cRe">Valeur de la partie réelle de la constante.</param>
        /// <param name="cIm">Valeur de la partie imaginaire de la constante.</param>
        /// <returns></returns>
        public static MyImage Julia(int iterationMax, double cRe, double cIm)
        {
            double minRe = -2.1, maxRe = 2.0, minIm = -1.2, maxIm = 1.2;

            // Ratio entre le plan et l'image
            int ratio = 500,
                fractLargeur = (int)((maxRe - minRe) * ratio),
                fractHauteur = (int)((maxIm - minIm) * ratio);

            // Ratio pour avoir les coordonées à l'échelle de base
            double imRatio = (maxIm - minIm) / (fractHauteur - 1),
                   reRatio = (maxRe - minRe) / (fractLargeur - 1);

            while ((fractLargeur % 4) != 0)
            {
                fractLargeur++;
            }

            MyImage fractale = new MyImage("bmp", fractLargeur, fractHauteur, fractLargeur * fractHauteur * 3 + 54, 54);
            Pixel[,] fractalePixels = new Pixel[fractHauteur, fractLargeur];

            for (int x = 0; x < fractLargeur; x++)
            {
                for (int y = 0; y < fractHauteur; y++)
                {
                    Complex c = new Complex(cRe, cIm),
                            z = new Complex(minRe + x * reRatio, minIm + y * imRatio);
                    int compteur = 0;

                    do
                    {
                        z = z * z + c;
                        compteur++;
                    } while ((compteur < iterationMax) && (z.Magnitude < 4));

                    if (compteur == iterationMax)
                    {
                        fractalePixels[y, x] = new Pixel(y, x, 255, 255, 255);
                    }
                    else
                    {
                        fractalePixels[y, x] = new Pixel(y, x, (byte)(Math.Sqrt(compteur / iterationMax)), (byte)(compteur + 20 - Math.Log(Math.Log10(z.Magnitude))), (byte)(compteur + 20 - Math.Log(Math.Log10(z.Magnitude))));
                    }
                }
            }

            fractale.PixelsImage = fractalePixels;

            Console.WriteLine("Done");

            return fractale;
        }
        #endregion

        #region actions

        /// <summary>
        /// Permet de décrypter une image codée dans une image.
        /// </summary>
        /// <param name="image">L'image servant de clé pour le décryptage.</param>
        /// <returns>L'image cachée.</returns>
        public MyImage DecryptWithImage(MyImage image)
        {
            MyImage decryptedImage = null;

            if (this.largeur == image.Largeur && this.hauteur == image.Hauteur) {
                int[] dim = GetCryptedImageData(image);
                int[][] octetSource = new int[3][], octetImage = new int[3][], octetDecrypted = new int[3][];
                int targetColorSource = 0, targetColorImage = 0;

                decryptedImage = new MyImage("bmp", dim[1], dim[0], dim[0] * dim[1] * this.bitsPixel + this.tailleOffset, this.tailleOffset);
                Pixel[,] pixels = new Pixel[dim[0], dim[1]];

                decryptedImage.PixelsImage = pixels;
                decryptedImage.SetBackground(new Pixel(0, 0, 255, 255, 255));

                if (dim[0] != 0 && dim[1] != 0)
                {
                    for (int i = 0; i < dim[0]; i++)
                    {
                        for (int j = 0; j < dim[1]; j++)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        targetColorSource = this.pixelsImage[i, j].Red;
                                        targetColorImage = image.PixelsImage[i, j].Red;
                                        break;
                                    case 1:
                                        targetColorSource = this.pixelsImage[i, j].Green;
                                        targetColorImage = image.PixelsImage[i, j].Green;
                                        break;
                                    case 2:
                                        targetColorSource = this.pixelsImage[i, j].Blue;
                                        targetColorImage = image.PixelsImage[i, j].Blue;
                                        break;
                                }

                                // On définit l'octet en binaire
                                octetSource[k] = ConvertByteToBinary((byte)targetColorSource, 8);
                                octetImage[k] = ConvertByteToBinary((byte)targetColorImage, 8);

                                // On effectue le cryptage
                                for (int l = 0; l < 4; l++)
                                {
                                    if (octetDecrypted[k] == null)
                                        octetDecrypted[k] = new int[8];

                                    octetDecrypted[k][l] = xOr(octetSource[k][4 + l], octetImage[k][l]);
                                    octetDecrypted[k][octetDecrypted[k].Length - 1 - l] = 0;
                                }
                            }

                            pixels[i, j] = new Pixel(i, j, (int)ConvertBinaryToByte(octetDecrypted[0]), (int)ConvertBinaryToByte(octetDecrypted[1]), (int)ConvertBinaryToByte(octetDecrypted[2]));
                        }
                    }

                    decryptedImage.PixelsImage = pixels;
                } else
                {
                    Console.WriteLine("Il n'y a pas d'image cryptée.");
                }

                Console.WriteLine("Done");
            }else
            {
                decryptedImage = this;
                Console.WriteLine("L'image de décryptage doit être de la même taille que l'image.");
            }

            return decryptedImage;
        }

        /// <summary>
        /// Permet de crypter une image grâce à une autre image
        /// </summary>
        /// <param name="image">L'image à utiliser pour crypter l'image (ses dimensions doivent être au moins égales à celle de l'image à crypter).</param>
        /// <returns>Retourne l'image cryptée.</returns>
        public MyImage CryptWithImage(MyImage image)
        {
            MyImage cryptedImage = new MyImage("bmp", image.Largeur, image.Hauteur, image.taille, image.TailleOffset);
            Pixel[,] pixels = new Pixel[image.Hauteur, image.Largeur];
            int targetColorSource = 0, targetColorImage = 0;
            int[][] octetSource = new int[3][], octetImage = new int[3][], octetCrypted = new int[3][];

            cryptedImage.PixelsImage = pixels;
            cryptedImage.SetBackground(new Pixel(0, 0, 255, 255, 255));

            // On vérifie si les dimesions de l'image à crypter sont plus petites que celle utilisée pour crypter
            if (image.Largeur >= this.largeur && image.Hauteur >= this.hauteur)
            {
                // On parcourt l'image la plus grande
                for (int i = 0; i < image.PixelsImage.GetLength(0); i++)
                {
                    for (int j = 0; j < image.PixelsImage.GetLength(1); j++)
                    {
                        // On vérifie que les coordonnées soient communes aux deux images
                        if (i < this.pixelsImage.GetLength(0) && j < this.pixelsImage.GetLength(1))
                        {
                            // On exécute le cryptage pour R, G et B
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        targetColorSource = this.pixelsImage[i, j].Red;
                                        targetColorImage = image.PixelsImage[i, j].Red;
                                        break;
                                    case 1:
                                        targetColorSource = this.pixelsImage[i, j].Green;
                                        targetColorImage = image.PixelsImage[i, j].Green;
                                        break;
                                    case 2:
                                        targetColorSource = this.pixelsImage[i, j].Blue;
                                        targetColorImage = image.PixelsImage[i, j].Blue;
                                        break;
                                }

                                // On définit l'octet en binaire
                                octetSource[k] = ConvertByteToBinary((byte)targetColorSource, 8);
                                octetImage[k] = ConvertByteToBinary((byte)targetColorImage, 8);

                                // On effectue le cryptage
                                for (int l = 0; l < 4; l++)
                                {
                                    if (octetCrypted[k] == null)
                                        octetCrypted[k] = new int[8];

                                    octetCrypted[k][l] = octetImage[k][l];
                                    octetCrypted[k][4 + l] = xOr(octetSource[k][l], octetImage[k][l]);
                                }
                            }

                            pixels[i, j] = new Pixel(0, 0, (int)ConvertBinaryToByte(octetCrypted[0]), (int)ConvertBinaryToByte(octetCrypted[1]), (int)ConvertBinaryToByte(octetCrypted[2]));
                        }
                        else
                        {
                            pixels[i, j] = image.PixelsImage[i, j];
                        }
                    }
                }

                cryptedImage.PixelsImage = pixels;
            }
            else
            {
                Console.WriteLine("Les dimensions de l'image cryptée doivent être au maximum égales à celle de l'image utilisée.");
            }

            Console.WriteLine("Done");

            return cryptedImage;
        }

        /// <summary>
        /// Crée une image contenant l'histogramme des couleurs d'une image.
        /// </summary>
        /// <param name="couleur">La couleur à analyser (R, G ou B, défaut à R).</param>
        /// <returns>Une image contenant l'histogramme de la couleur analysée.</returns>        
        public MyImage HistogrammeCouleurs()
        {
            MyImage newImage = new MyImage("bmp", 512, 150, 230454, 54);
            Pixel[,] pixels = new Pixel[150, 512];
            Pixel[] pixelsHistogramme = new Pixel[3] { new Pixel(0, 0, 255, 0, 0), new Pixel(0, 0, 0, 255, 0), new Pixel(0, 0, 0, 0, 255) };
            int[,] compteurCouleurs = new int[3, 256];
            int[] maxHeight = new int[3];
            int ratio = pixels.GetLength(1) / 256;

            // On compte toutes les teintes de la couleur choisie
            compteurCouleurs = CountPixelShades();
            int maxValue = MaxMatrix(compteurCouleurs);

            // On parcourt l'image initiale
            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                // On incrémente du ratio entre la largeur de l'histogramme et le nombre de teintes
                for (int j = 0; j < pixels.GetLength(1); j += ratio)
                {
                    for (int k = 0; k < ratio; k++)
                    {
                        // On établit la hauteur maximale (pour chaque couleur) de chaque pic grâce au compteur de teintes
                        maxHeight[0] = Convert.ToInt32(Math.Round((double)(compteurCouleurs[0, j / ratio] * ((double)pixels.GetLength(0) / (double)maxValue))));
                        maxHeight[1] = Convert.ToInt32(Math.Round((double)(compteurCouleurs[1, j / ratio] * ((double)pixels.GetLength(0) / (double)maxValue))));
                        maxHeight[2] = Convert.ToInt32(Math.Round((double)(compteurCouleurs[2, j / ratio] * ((double)pixels.GetLength(0) / (double)maxValue))));

                        for (int l = 0; l < 3; l++)
                        {
                            if (maxHeight[l] > i)
                                pixels[i, j + k] = pixelsHistogramme[l]; 
                        }
                    }
                }
            }

            newImage.pixelsImage = pixels;
            newImage.SetBackground(new Pixel(0, 0, 110, 110, 110));

            Console.WriteLine("Done");

            return newImage;
        }

        /// <summary>
        /// Filtre de convolution : détection de contours
        /// </summary>
        /// <returns>un nouvelle image avec le filtre</returns>
        public MyImage DetectContours()
        {
            MyImage newImage = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            double[,] convolution = new double[3, 3] { { -1, -1, -1 }, { -1, 8, -1}, { -1, -1, -1 } };

            newImage.PixelsImage = ApplyConvolution(this.pixelsImage, convolution);

            return newImage;
        }

        /// <summary>
        /// Filtre de convolution : Repoussage
        /// </summary>
        /// <returns>un nouvelle image avec le filtre</returns>
        public MyImage Repoussage()
        {
            MyImage newImage = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            double[,] convolution = new double[3, 3] { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };

            newImage.PixelsImage = ApplyConvolution(this.pixelsImage, convolution);

            return newImage;
        }

        /// <summary>
        /// Filtre de convolution : Renforcement des contours
        /// </summary>
        /// <returns>un nouvelle image avec le filtre</returns>
        public MyImage RenforceContours()
        {
            MyImage newImage = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            double[,] convolution = new double[3, 3] { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };

            newImage.PixelsImage = ApplyConvolution(this.pixelsImage, convolution);

            return newImage;
        }

        /// <summary>
        /// Filtre de convolution : Renforcement des contrastes
        /// </summary>
        /// <returns>un nouvelle image avec le filtre</returns>
        public MyImage RenforceContrastes()
        {
            MyImage newImage = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            double[,] convolution = new double[3, 3] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };

            newImage.PixelsImage = ApplyConvolution(this.pixelsImage, convolution);

            return newImage;
        }

        /// <summary>
        /// Filtre de convolution : Flou
        /// </summary>
        /// <returns>un nouvelle image avec le filtre</returns>
        public MyImage Blur()
        {
            MyImage newImage = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            double[,] convolution = new double[3, 3] { { 0.11, 0.11, 0.11 }, { 0.11, 0.11, 0.11 }, { 0.11, 0.11, 0.11 } };

            newImage.PixelsImage = ApplyConvolution(this.pixelsImage, convolution);

            return newImage;
        }

        /// <summary>
        /// Permet de faire une symétrie par rapport à l'horizontale
        /// </summary>
        public MyImage MirrorX()
        {
            MyImage image = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            Pixel[,] newImage = new Pixel[this.hauteur, this.largeur];

            for (int j = this.pixelsImage.GetLength(1) - 1; j >= 0; j--)
            {
                for (int i = this.pixelsImage.GetLength(0) - 1; i >= 0; i--)
                {
                    newImage[(this.hauteur - 1) - i, (this.largeur - 1) - j] = this.pixelsImage[i, j];
                }
            }

            image.PixelsImage = newImage;

            return image;
        }

        /// <summary>
        /// Permet de faire une symétrie par rapport à la verticale
        /// </summary>
        public MyImage MirrorY()
        {
            MyImage image = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            Pixel[,] newImage = new Pixel[this.hauteur, this.largeur];

            for (int i = this.pixelsImage.GetLength(1) - 1; i >= 0; i--)
            {
                for (int j = this.pixelsImage.GetLength(0) - 1; j >= 0; j--)
                {
                    if (this.pixelsImage[i, j] != null)
                        newImage[j, (this.largeur - 1) - i] = this.pixelsImage[j, i];
                }
            }

            image.PixelsImage = newImage;

            return image;
        }

        /// <summary>
        /// Permet d'effectuer une rotation sur une image
        /// </summary>
        /// <param name="angle">Angle de rotation en degrés</param>
        public MyImage Rotate(double angle)
        {
            double angleDim = angle;
            int initCoordLargeur = this.largeur, initCoordHauteur = this.hauteur, newLargeur = 0, newHauteur = 0;

            // Calcul des nouvelles dimensions de l'image
            if (angle >= 90)
            {
                initCoordLargeur = this.hauteur;
                initCoordHauteur = this.largeur;
                angleDim = angle - 90;
            }

            angleDim = ConvertDegreeToRad(angleDim);
            angle = ConvertDegreeToRad(angle);

            newHauteur = (int)(Math.Cos(angleDim) * initCoordHauteur + Math.Sin(angleDim) * initCoordLargeur);
            newLargeur = (int)(Math.Sin(angleDim) * initCoordHauteur + Math.Cos(angleDim) * initCoordLargeur);

            newLargeur = ToFormat4(newLargeur);

            int newTaille = this.bitsPixel * newLargeur * newHauteur + this.tailleOffset, coordFinX = 0, coordFinY = 0;
            double centeredCoordHauteur = 0, centeredCoordLargeur = 0, newCoordHauteur = 0, newCoordLargeur = 0;

            MyImage newImage = new MyImage(this.type, newLargeur, newHauteur, newTaille, this.tailleOffset);
            Pixel[,] image = new Pixel[newHauteur, newLargeur];

            // On remplit l'image d'arrivée
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < image.GetLength(1); j++)
                {
                    // On recentre les coordonnées
                    centeredCoordHauteur = i - (newImage.Hauteur / 2);
                    centeredCoordLargeur = j - (newImage.Largeur / 2);

                    // Transformation des coordonnées pour obtenir celles de l'image de base
                    newCoordHauteur = Math.Cos(-1 * angle) * centeredCoordHauteur + Math.Sin(-1 * angle) * centeredCoordLargeur;
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
            int newLargeur = 0;

            // Ajustement de la largeur de l'image pour qu'elle soit divisible par 4
            newLargeur = ToFormat4((int)initNewLargeur);

            int averageR = 0, averageG = 0, averageB = 0, newTaille = (int)(newLargeur * initNewHauteur * this.bitsPixel) + this.tailleOffset;

            MyImage newImage = new MyImage(this.type, newLargeur, (int)initNewHauteur, newTaille, this.tailleOffset);
            Pixel[,] image = new Pixel[newImage.Hauteur, newLargeur];

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
            if ((int)initNewLargeur != newLargeur)
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
        public MyImage GreyShadesFilter()
        {
            MyImage image = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            Pixel[,] pixelsImage = new Pixel[this.hauteur, this.largeur];
            int temp;

            for (int i = 0; i < pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < pixelsImage.GetLength(1); j++)
                {
                    temp = Convert.ToByte((this.pixelsImage[i, j].Red + this.pixelsImage[i, j].Green + this.pixelsImage[i, j].Blue) / 3);

                    pixelsImage[i, j] = new Pixel(i ,j , temp, temp, temp);
                }
            }

            image.PixelsImage = pixelsImage;

            return image;
        }

        /// <summary>
        /// Permet de passer l'image en noir et blanc
        /// </summary>
        public MyImage BlackAndWhiteFilter()
        {
            MyImage image = new MyImage(this.type, this.largeur, this.hauteur, this.taille, this.tailleOffset);
            Pixel[,] pixelsImage = new Pixel[this.hauteur, this.largeur];
            byte temp;
            int med = 127;

            for (int i = 0; i < pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < pixelsImage.GetLength(1); j++)
                {
                    temp = Convert.ToByte((this.pixelsImage[i, j].Red + this.pixelsImage[i, j].Green + this.pixelsImage[i, j].Blue) / 3);

                    if (temp <= med)
                    {
                        pixelsImage[i, j] = new Pixel(i, j, 0, 0, 0);
                    }
                    else
                    {
                        pixelsImage[i, j] = new Pixel(i, j, 255, 255, 255);
                    }

                }
            }

            image.PixelsImage = pixelsImage;

            return image;
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
        /// Permet de déterminer les dimensions d'une image cryptée par une autre.
        /// </summary>
        /// <param name="keyImage">L'image clé.</param>
        /// <returns>Les dimensions de l'image.</returns>
        private int[] GetCryptedImageData(MyImage keyImage)
        {
            int[] result = new int[2];

            // Calcul de la hauteur
            for (int i = 0; i < this.PixelsImage.GetLength(0); i++)
            {
                if (!this.PixelsImage[i, 0].Equals(keyImage.PixelsImage[i, 0]))
                {
                    result[0]++;
                }else
                {
                    break;
                }
            }

            // Calcul de la largeur
            for (int j = 0; j < this.PixelsImage.GetLength(1); j++)
            {
                if (!this.PixelsImage[0, j].Equals(keyImage.PixelsImage[0, j]))
                {
                    result[1]++;
                }else
                {
                    break;
                }
            }


            return result;
        }

        /// <summary>
        /// Permet d'effectuer un ou exclusif.
        /// </summary>
        /// <param name="a">Premier entier.</param>
        /// <param name="b">Deuxième entier.</param>
        /// <returns>1 ou 0 selon les cas.</returns>
        private int xOr(int a, int b)
        {
            int result = 0;

            if (a != b)
            {
                result = 1;
            }

            return result;
        }

        /// <summary>
        /// Permet de convertir un nombre binaire en octet.
        /// </summary>
        /// <param name="binary">Nombre binaire à convertir.</param>
        /// <returns>Le nombre convertit en octet.</returns>
        public static byte ConvertBinaryToByte(int[] binary)
        {
            byte result = 0;

            for (int i = 0; i < binary.Length; i++)
            {
                result += (byte) (binary[i] * Math.Pow(2, (double)(binary.Length - i - 1)));
            }

            return result;
        }

        /// <summary>
        /// Convertit un octet en binaire.
        /// </summary>
        /// <param name="b">L'octet à convertir.</param>
        /// <param name="size">Le nombre de bit.</param>
        /// <returns>Le nombre en binaire sur n bit.</returns>
        public static int[] ConvertByteToBinary(byte b, int size)
        {
            int[] result = new int[size];
            int partEntiere = 0, reste = 0;

            for (int i = (size - 1); i >= 0; i--)
            {
                partEntiere = (int)Math.Floor((double)b / 2);
                reste = (int)(b % (Math.Pow(2, Convert.ToDouble(i))));

                result[i] = b - (2 * partEntiere);
                b = (byte)partEntiere;
            }

            return result;
        }

        /// <summary>
        /// Calcule le maximum d'une matrice d'entiers.
        /// </summary>
        /// <param name="m"> La matrice à évaluer.</param>
        /// <returns>Retourne le max de la matrice.</returns>
        private int MaxMatrix(int[,] m)
        {
            int result = 0;

            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    if (m[i, j] > result)
                        result = m[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Permet de compter les différentes nuances des couleurs dans une image.
        /// </summary>
        /// <returns>Un tableau avec le comptage des 256 nuances de chaque couleur.</returns>
        private int[,] CountPixelShades()
        {
            int[,] result = new int[3, 256];

            for (int i = 0; i < this.pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixelsImage.GetLength(1); j++)
                {
                    result[0, this.pixelsImage[i, j].Red]++;
                    result[1, this.pixelsImage[i, j].Green]++;
                    result[2, this.pixelsImage[i, j].Blue]++;
                }
            }

            return result;
        }

        /// <summary>
        /// Permet de définir une couleur d'arrière plan pour une image
        /// </summary>
        /// <param name="pixel">Pixel d'arrière plan</param>
        public void SetBackground(Pixel pixel)
        {
            for (int i = 0; i < this.pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < this.pixelsImage.GetLength(1); j++)
                {
                    if (this.pixelsImage[i, j] == null)
                        this.pixelsImage[i, j] = pixel;
                }
            }
        } 

        /// <summary>
        /// Permet d'appliquer un filtre de convolution à une matrice
        /// </summary>
        /// <param name="image">Matrice des pixels de l'image à modifier</param>
        /// <param name="convolution">Matrice de convolution du filtre</param>
        /// <returns>Un nouvelle matrice de pixel avec le filtre</returns>
        private Pixel[,] ApplyConvolution(Pixel[,] image, double[,] convolution)
        {
            double currentTotal = 0;
            Pixel[,] newImage = new Pixel[image.GetLength(0), image.GetLength(1)];

            // On parcourt la matrice finale
            for (int i = 0; i < newImage.GetLength(0); i++)
            {
                for (int j = 0; j < newImage.GetLength(1); j++)
                {
                    newImage[i, j] = new Pixel(i, j, 0, 0, 0);

                    // Pour chaque pixel de l'image originale correspondant
                    for (int k = 0; k < 3; k++)
                    {
                        // On détermine l'octet à modifier
                        for (int l = -1; l <= 1; l++)
                        {
                            if ((i + l) < image.GetLength(0) && (i + l) >= 0)
                            {
                                for (int m = -1; m <= 1; m++)
                                {
                                    if ((j + m) < image.GetLength(1) && (j + m) >= 0)
                                    {
                                        switch (k)
                                        {
                                            case 0:
                                                currentTotal += image[i + l, j + m].Red * convolution[l + 1, m + 1];
                                                break;
                                            case 1:
                                                currentTotal += image[i + l, j + m].Green * convolution[l + 1, m + 1];
                                                break;
                                            case 2:
                                                currentTotal += image[i + l, j + m].Blue * convolution[l + 1, m + 1];
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        //On détermine si l'octe tdoit être modifié
                        if(currentTotal > 255)
                        {
                            currentTotal = 255;
                        }else if (currentTotal < 0)
                        {
                            currentTotal = 0;
                        }

                        //Attribution de la valeur
                        switch (k)
                        {
                            case 0:
                                newImage[i, j].Red = (int)currentTotal;
                                break;
                            case 1:
                                newImage[i, j].Green = (int)currentTotal;
                                break;
                            case 2:
                                newImage[i, j].Blue = (int)currentTotal;
                                break;
                        }

                        currentTotal = 0;
                    }
                }
            }

            return newImage;
        }

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
        /// Permet de rendre la largeur d'une image divisible par 4
        /// </summary>
        /// <param name="largeur">Dimension à (éventuellement) modifier</param>
        /// <returns>La nouvelle largeur</returns>
        public static int ToFormat4(int largeur)
        {
            while ((largeur % 4) != 0)
            {
                largeur++;
            }

            return largeur;
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
