using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    /// <summary>
    /// Classe Pixel
    /// </summary>
    public class Pixel
    {
        private int coordX;
        private int coordY;
        private int R;
        private int G;
        private int B;

        #region constructeurs

        /// <summary>
        /// Constructeur de Pixel
        /// </summary>
        /// <param name="x">Coordonnée en ligne</param>
        /// <param name="y">Coordonnée en colonne</param>
        /// <param name="R">Rouge</param>
        /// <param name="G">Vert</param>
        /// <param name="B">Bleu</param>
        public Pixel(int x, int y, int R, int G, int B)
        {
            this.coordX = x;
            this.coordY = y;
            this.R = R;
            this.G = G;
            this.B = B;
        }

        #endregion

        #region proprietes

        /// <summary>
        /// Propriété de Rouge
        /// </summary>
        public int Red { get { return this.R; } set { this.R = value; } }

        /// <summary>
        /// Propriété de Vert
        /// </summary>
        public int Green { get { return this.G; } set { this.G = value; } }

        /// <summary>
        /// Propriété de Bleu
        /// </summary>
        public int Blue { get { return this.B; } set { this.B = value; } }

        #endregion

        #region methodes

        /// <summary>
        /// Permet de récupérer les valeurs r, v, b d'un pixel
        /// </summary>
        /// <returns>Tableau contenant les valeurs</returns>
        public int[] GetCouleur()
        {
            return new int[3] { this.R, this.G, this.B };
        }

        /// <summary>
        /// Converti les valeurs r, v, b en string
        /// </summary>
        /// <returns>Un string contenant les valeurs r, v, b</returns>
        public override string ToString()
        {
            return this.R + " " + this.G + " " + this.B;
        }

        #endregion
    }
}
