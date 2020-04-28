using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    class QRCode
    {
        private int[] maskBinary = new int[15] { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0};
        private int dimensions = 0;
        private int version = 0;
        private MyImage image = null;
        private Dictionary<char, int> codesAlphaNum = new Dictionary<char, int>();

        #region constructeurs

        /// <summary>
        /// Constructeur pour l'encodage en QRCode.
        /// </summary>
        /// <param name="messageToEncode">Message alpha-numérique à encoder.</param>
        /// <param name="zoom">Coefficient de zoom du QRCode (au moins 1).</param>
        public QRCode(string messageToEncode)
        {
            int messageLength = messageToEncode.Length;
            bool continuFlag = true;

            InitializeDictionary();
            messageToEncode = messageToEncode.ToUpper();

            if (messageLength <= 25)
            {
                this.version = 1;
                this.dimensions = 21;
            }
            else if (messageLength > 25 && messageLength <= 47)
            {
                this.version = 2;
                this.dimensions = 25;

            }
            else
            {
                continuFlag = false;
                Console.WriteLine("Impossible de stocker un message de cette taille.");
            }

            if (continuFlag)
            {
                this.image = new MyImage("bmp", this.dimensions, this.dimensions, this.dimensions * this.dimensions * 3 + 54, 54);
                this.image.PixelsImage = Encode(messageToEncode);
            }
        }

        #endregion

        #region proprietes

        public MyImage Image
        {
            get { return this.image; }
        }

        #endregion

        #region tools

        /// <summary>
        /// Méthode pour encoder le message. (utilisé dans le constructeur)
        /// </summary>
        /// <returns></returns>
        private Pixel[,] Encode(string message)
        {
            Encoding u8 = Encoding.UTF8;
            Pixel[,] pixelsImage = new Pixel[this.dimensions, this.dimensions];
            int sizeData = 0, currentSize = 0;
            string[] doubleLetters = DoubleSeparation(message);
            byte[] correctionBytes = null;
            int[] messageLengthBinary = MyImage.ConvertToBinary(message.Length, 9), correctionInt = null;
            int[][] lettersDougleBinary = new int[doubleLetters.Length][];

            switch (this.version)
            {
                case 1:
                    sizeData = 208;
                    break;
                case 2:
                    sizeData = 368;
                    break;
            }

            int[] data = new int[sizeData];

            // On va remplir les données dans l'odre : code alphaNum -> longueur du message -> données -> correction d'erreurs -> masque

            // Code pour l'alpha-numérique
            data[0] = 0;
            data[1] = 0;
            data[2] = 1;
            data[3] = 0;
            currentSize += 4;

            // Longueur du message
            for (int i = 0; i < messageLengthBinary.Length; i++)
            {
                data[currentSize + i] = messageLengthBinary[i];
            }

            currentSize += messageLengthBinary.Length;

            // Données
            for (int i = 0; i < doubleLetters.Length; i++)
            {
                lettersDougleBinary[i] = MyImage.ConvertToBinary(ConvertAlphaNumToDecimal(doubleLetters[i]), doubleLetters[i].Length == 2 ? 11 : 6);

                for (int j = 0; j < lettersDougleBinary[i].Length; j++)
                {
                    data[currentSize + j] = lettersDougleBinary[i][j];
                }

                currentSize += lettersDougleBinary[i].Length;
            }

            // Ajout de la terminaison et multiple de 8
            currentSize = AddBinaryEnd(data, currentSize);

            // Calcul de l'EC
            correctionBytes = ReedSolomon.ReedSolomonAlgorithm.Encode(GetECBytes(data, currentSize), this.version == 1 ? 7 : 10, ReedSolomon.ErrorCorrectionCodeType.QRCode);

            //Insertion de la EC
            for (int i = 0; i < correctionBytes.Length; i++)
            {
                correctionInt = MyImage.ConvertToBinary((int)correctionBytes[i], 8);

                for (int j = 0; j < 8; j++)
                {
                    data[currentSize + i * 8 + j] = correctionInt[j];
                }

            }

            currentSize += 8 * correctionBytes.Length;

            // Ajout des paterns et timing patterns
            AddFinderAlignmentPaterns(pixelsImage);

            // Ajout des données de format et de version
            AddFormatAndVersion(pixelsImage);

            // Ajout des données
            AddData(pixelsImage, data);

            for (int i = 0; i < pixelsImage.GetLength(0); i++)
            {
                for (int j = 0; j < pixelsImage.GetLength(1); j++)
                {
                    if (pixelsImage[i, j] == null) pixelsImage[i, j] = new Pixel(i, j, 255, 255, 255);
                }
            }

            //for (int i = 0; i < data.Length; i++)
            //{
            //    Console.Write(data[i]);

            //    if (((i + 1) % 8) == 0) Console.Write(" ");
            //}

            return pixelsImage;
        }

        private Pixel GetQRPixel(int bit)
        {
            return ((bit == 0) ? new Pixel(0, 0, 255, 255, 255) : new Pixel(0, 0, 0, 0, 0));
        }

        private int GetMaskedBit(int i, int j, int bit)
        {
            int result = bit;

            if ((i + j) % 2 == 0)
            {
                if (bit == 0) result = 1;
                else result = 0;
            }

            return result;     
        }

        private byte[] GetECBytes(int[] data, int currentSize)
        {
            byte[] ecData = new byte[currentSize / 8];
            int[] temp = new int[8];

            for (int i = 0; i < currentSize; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {

                    temp[j] = data[i + j];
                }

                ecData[i / 8] = MyImage.ConvertBinaryToByte(temp);
            }

            return ecData;
        }
       
        private void AddData(Pixel[,] pixels, int[] data)
        {
            // mode 0 = montée, 1 = descente
            int mode = 0, cpt = 0;
            int[] currentPositions = new int[2] { 0, this.dimensions - 1 };

            for (int k = 0; k < (this.dimensions - 1)/2; k++)
            {

                if (mode == 0)
                {
                    for (int i = 0; i < this.dimensions; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (pixels[currentPositions[0] + i, currentPositions[1] - j] == null && (currentPositions[1] - j) != 6)
                            {
                                pixels[currentPositions[0] + i, currentPositions[1] - j] = GetQRPixel(GetMaskedBit(currentPositions[0] + i, currentPositions[1] - j, data[cpt]));
                                cpt++;
                            }else if ((currentPositions[1] - j) == 6)
                            {
                                currentPositions[1]--;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i > -1 * this.dimensions; i--)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            if (pixels[currentPositions[0] + i, currentPositions[1] - j] == null && (currentPositions[1] - j) != 6)
                            {
                                pixels[currentPositions[0] + i, currentPositions[1] - j] = GetQRPixel(GetMaskedBit(currentPositions[0] + i, currentPositions[1] - j, data[cpt]));
                                cpt++;
                            }
                            else if ((currentPositions[1] - j) == 6)
                            {
                                currentPositions[1]--;
                            }
                        }
                    }
                }
                

                currentPositions[0] = mode == 0 ? this.dimensions - 1 : 0;
                currentPositions[1] -= 2;
                mode = mode == 0 ? 1 : 0;
            }
        }

        private void AddFormatAndVersion(Pixel[,] pixels)
        {
            int dim = this.dimensions - 8 - 1;

            for (int i = 0; i < (int)this.maskBinary.Length/2; i++)
            {
                pixels[i, 8] = GetQRPixel(this.maskBinary[i]);

                if (pixels[dim, i] == null)
                {
                    pixels[dim, i] = GetQRPixel(this.maskBinary[i]);
                }else
                {
                    pixels[dim, i+1] = GetQRPixel(this.maskBinary[i]);
                }
            }

            for (int i = 7; i < this.maskBinary.Length; i++)
            {
                pixels[dim, dim + i - 6] = GetQRPixel(this.maskBinary[i]);

                if (pixels[dim + i - 7, 8] == null)
                {
                    pixels[dim + i - 7, 8] = GetQRPixel(this.maskBinary[i]);
                }
                else
                {
                    pixels[dim + i + 1 - 7, 8] = GetQRPixel(this.maskBinary[i]);
                }
            }
        }

        private void AddFinderAlignmentPaterns(Pixel[,] pixels)
        {
            int cpt = 0;
            int[] startPositions = new int[2] { 0, 0 }, indexStart = new int[2] { 7, 0 }, indexEnd = new int[2] { 0, 7 };

            // Place les finders
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < 7; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        // Pour ajouter les rectangles blancs
                        if ( ((i == 1 || i == 5) && (j >= 1 && j <= 5)) ||
                            ((j == 1 || j == 5) && (i >= 1 && i <= 5))
                            ) pixels[startPositions[0] + i, startPositions[1] + j] = new Pixel(i, j, 255, 255, 255);
                        else pixels[startPositions[0] + i, startPositions[1] + j] = new Pixel(i, j, 0, 0, 0);
                    }                    
                }

                if (k != 2) startPositions[k] = this.dimensions - 7;
            }

            // Zones de séparation
            AddSeparations(pixels);

            // Dark Module
            pixels[this.dimensions - 1 - (4 * this.version + 9), 8] = new Pixel(8, 4 * this.version + 9, 0, 0, 0);

            // Place l'alignement
            if (this.version == 2) {
                pixels[6, 18] = new Pixel(6, 18, 0, 0, 0);

                for (int k = 1; k <= 2; k++)
                {
                    for (int i = -1 * k; i <= 1 * k; i++)
                    {
                        for (int j = -1 * k; j <= 1 * k; j++)
                        {
                            if (pixels[6 + i, 18 + j] == null) {
                                if (k == 2) pixels[6 + i, 18 + j] = new Pixel(i, j, 0, 0, 0);
                                else pixels[6 + i, 18 + j] = new Pixel(i, j, 255, 255, 255);
                            }
                        }
                    }
                }
            }

            // Timing patterns
            startPositions[0] = 8;
            startPositions[1] = 6;

            for (int k = 0; k < 2; k++)
            {
                // Permet d'inverser l'avancement pour le deuxième pattern
                if (k == 1)
                {
                    startPositions[0] = this.dimensions - 6 - 1;
                    startPositions[1] = 8;
                }

                for (int i = 0; i < (this.dimensions - 16); i++)
                {
                    if (k == 0) pixels[startPositions[0] + i, startPositions[1]] = (cpt % 2 == 0) ? new Pixel(0, 0, 0, 0, 0) : new Pixel(0, 0, 255, 255, 255);
                    else pixels[startPositions[0], startPositions[1] + i] = (cpt % 2 == 0) ? new Pixel(0, 0, 0, 0, 0) : new Pixel(0, 0, 255, 255, 255);

                    cpt++;
                }

                cpt = 0;
            }
        }

        private void AddSeparations(Pixel[,] pixels)
        {
            // Bas gauche
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j <= 7; j++)
                {
                    if (pixels[i, j] == null) pixels[i, j] = new Pixel(i, j, 255, 255, 255);
                }
            }

            // Haut gauche
            for (int i = (this.dimensions - 1); i >= (this.dimensions - 8); i--)
            {
                for (int j = 0; j <= 7; j++)
                {
                    if (pixels[i, j] == null) pixels[i, j] = new Pixel(i, j, 255, 255, 255);
                }
            }

            // Haut droit
            for (int i = (this.dimensions - 1); i >= (this.dimensions - 8); i--)
            {
                for (int j = (this.dimensions - 1); j >= (this.dimensions - 8); j--)
                {
                    if (pixels[i, j] == null) pixels[i, j] = new Pixel(i, j, 255, 255, 255);
                }
            }
        }

        private int AddBinaryEnd(int[] data, int currentSize)
        {
            int sizeOfMessage = this.version == 1 ? 152 : 272, cpt = 4;

            if (currentSize < sizeOfMessage)
            {
                // Ajout de la terminaison de 4
                while (cpt != 0 && currentSize < sizeOfMessage)
                {
                    currentSize++;
                    cpt--;
                }

                // Multiple de 8
                while (currentSize < sizeOfMessage && (currentSize % 8) != 0)
                {
                    currentSize++;
                }

                // Ajout des terminaisons finales
                while (currentSize < sizeOfMessage)
                {
                    if (cpt == 0)
                    {
                        AddByte(data, new int[8] { 1, 1, 1, 0, 1, 1, 0, 0 }, currentSize);                   
                        cpt = 1;
                    }
                    else
                    {
                        AddByte(data, new int[8] { 0, 0, 0, 1, 0, 0, 0, 1 }, currentSize);
                        cpt = 0;
                    }

                    currentSize += 8;
                }
            }

            return currentSize;
        }

        private void AddByte(int[] data, int[] b, int startPosition)
        {
            for (int i = 0; i < b.Length; i++)
            {
                data[startPosition + i] = b[i];
            }
        }

        private int ConvertAlphaNumToDecimal(string alphaNum)
        {
            int result = 0, cpt = 1;

            foreach (char c in alphaNum)
            {
                if (alphaNum.Length == 1) cpt = 0;

                result += (int)Math.Pow((double)45, (double)cpt) * this.codesAlphaNum[c];
                cpt--;
            }

            return result;
        }

        private string[] DoubleSeparation(string message)
        {
            int messageLength = message.Length, cpt = 0;
            string[] result = new string[Convert.ToInt32(Math.Ceiling((double)messageLength / 2))];

            while (cpt < messageLength)
            {
                if ((messageLength % 2) == 0 || cpt != (messageLength - 1))
                {
                    result[cpt / 2] = message[cpt] + "" + message[cpt + 1];
                    cpt += 2;
                }else
                {
                    result[cpt / 2] = Convert.ToString(message[cpt]);
                    cpt++;
                }
            }

            return result;
        }

        /// <summary>
        /// Permet d'initialiser le dictionnaire avec les codes de chaque caractère alpha-numérique.
        /// </summary>
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
