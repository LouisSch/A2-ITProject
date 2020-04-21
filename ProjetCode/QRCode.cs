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
        private Dictionary<char, int> codesAlphaNum = new Dictionary<char, int>();

        #region constructeurs

        public QRCode()
        {
            InitializeDictionary();
            Console.WriteLine(codesAlphaNum['$']);
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
