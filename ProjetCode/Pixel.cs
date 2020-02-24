using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetCode
{
    class Pixel
    {
        private int coordX;
        private int coordY;
        private int R;
        private int G;
        private int B;

        #region constructeurs
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
        public int Red { get { return this.R; } set { this.R = value; } }
        public int Green { get { return this.G; } set { this.G = value; } }
        public int Blue { get { return this.B; } set { this.B = value; } }
        #endregion

        #region methodes
        public int[] GetCouleur()
        {
            return new int[3] { this.R, this.G, this.B };
        }

        public override string ToString()
        {
            return this.R + " " + this.G + " " + this.B;
        }
        #endregion
    }
}
