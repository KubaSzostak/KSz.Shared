using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public class KsiegaWieczysta
    {
        public static bool IdIsValid(string kwId)
        {
            //http://www.algorytm.org/numery-identyfikacyjne/numer-ksiegi-wieczystej.html
            //http://www.algorytm.org/numery-identyfikacyjne/numer-ksiegi-wieczystej.html
            //GD1W/00090409/1
            //012345678901234

            if (string.IsNullOrEmpty(kwId))
                return false;
            if (kwId.Length != 15)
                return false;
            if (kwId[4] != '/')
                return false;
            if (kwId[13] != '/')
                return false;

            int i;
            if (!int.TryParse(kwId.Substring(5, 8), out i))
                return false;
            if (!char.IsNumber(kwId[14]))
                return false;

            return true;
        }
    }
}
