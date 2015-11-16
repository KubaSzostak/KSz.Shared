using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System
{


    //EgbIdd
    public interface IG5IdProvider
    {
        G5Id G5Id { get; }
    }
    
    /// <summary>
    /// Identyfikator działki ewidencyjnej w postaci WWPPGG_R.XXXX.NDZ lub WWPPGG_R.XXXX.AR_NR.NDZ
    /// </summary>
    public class G5Id : IComparable<G5Id>, IG5IdProvider
    {
        // WWPPGG_R.0002.AR_138.13
        // 226201_1.0002.AR_138.13
        // 01234567890123456789012

        
        public static string EmptyId = "000000_0.0000.0";
        public static string DefaultId = "226201_1.0002.AR_138.13";

        public static string GetSqlWhere(string fieldName, string tercIdFilter, string inputText, string wildcardManyMatchSymbol)
        {
            // Optimalization for combined query 
            // ((Idd LIKE '2262*.22') OR (Idd LIKE '2262*.21')) AND (Idd LIKE '2262*') run more faster than
            // (Idd LIKE '2262*.22') OR (Idd LIKE '2262*.21') 
            var tercQuery = fieldName + " like '" + tercIdFilter + wildcardManyMatchSymbol + "'";

            if (string.IsNullOrEmpty(inputText))
                return tercQuery;

            var inputValues = inputText.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var queryValues = new List<string>();
            foreach (var inputVal in inputValues)
            {
                var sqlVal = GetSqlQueryValue(tercIdFilter, inputVal, wildcardManyMatchSymbol);
                queryValues.Add("( " +fieldName + " like '" + sqlVal + "' )");
            }
            var queryString = string.Join(" OR ", queryValues.ToArray());

            return tercQuery + " AND (" + queryString + ")";
        }

        private static string GetSqlQueryValue(string tercIdFilter, string inputText, string wildcardManyMatchSymbol)
        {

            if (string.IsNullOrEmpty(inputText))
                return tercIdFilter + wildcardManyMatchSymbol;

            var res = inputText;

            // 17.12/3 -> AR_17.12/3
            if (res.Contains("."))
                res = "AR_" + res;

            // 4/123 -> .4/123
            if (!(res.StartsWith("/")))
                res = "." + res; 

            // 4/ -> 4/*
            if (res.EndsWith("/"))
                res += "*";

            return tercIdFilter + wildcardManyMatchSymbol + res.Replace("*", wildcardManyMatchSymbol);
        }

        public G5Id()
        {
            Id = EmptyId;
        }

        public G5Id(string id)
        {
            Id = id;
        }

        private bool Contains(string s, int pos, char ch)
        {
            if (pos >= s.Length)
                return false;
            return s[pos] == ch;
        }

        public bool ContainsText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            var searchText = "";
            if (this.Gmi != null)
                searchText += "|" + this.Gmi.Nazwa;
            if (this.Pow != null)
                searchText += "|" + this.Pow.Nazwa;
            if (this.Obr != null)
                searchText += "|" + this.Obr.Nazwa;

            return searchText.ToLower().Contains(value.ToLower());
        }

        private string Substring(string s, int start, int length, bool onlyNumeric)
        {
            if (s.Length < start + length)
                return null;

            string res = "";
            if (length < 0)
                res = s.Substring(start);
            else
                res = s.Substring(start, length);

            int test;
            if (onlyNumeric && !int.TryParse(res, out test))
                return null;

            return res;
        }

        private void CheckLengthAndDigit(string s, int length, string paramName)
        {
            if (s == null || s.Length != length)
                throw NewException("{0}={1} has invalid length. Expected {2} chars.'", paramName, s, length);
            CheckIsDigit(s, paramName);
        }

        private void CheckIsDigit(string s, string paramName)
        {
            foreach (var ch in s)
                if (!char.IsDigit(ch))
                    throw NewException("{0}='{1}' is not numeric value.", paramName, s);
        }

        private Exception NewException(string errMsg, params object[] args)
        {
            errMsg = "G5Id(" + this.Id + ") error: " + string.Format(errMsg, args);
            return new Exception(errMsg);
        }

        public bool IddIsValid
        {
            get
            {
                if (ToInt(NrWoj) < 1)
                    return false;
                if (ToInt(NrPow) < 1)
                    return false;
                if (ToInt(NrGmi) < 1)
                    return false;
                if (ToInt(Rodz) < 1)
                    return false;
                if (ToInt(NrObr) < 1)
                    return false;
                if (!string.IsNullOrEmpty(NrAr) && ToInt(NrAr) < 1)
                    return false;
                if (string.IsNullOrEmpty(NrDz))
                    return false;
                if (ToInt(NrDz[0].ToString()) < 1) //EmptyId = "000000_0.0000.0";
                    return false;
                if (NrDz.Contains("BUD"))
                    return false;
                if (!IsFraction(NrDz))
                    return false;

                return true;
            }
        }

        private int ToInt(string s)
        {
            int res = 0;
            int.TryParse(s, out res);
            return res;
        }

        private bool IsFraction(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            var fractionChars = 0;
            foreach (var ch in s)
            {
                var isValidChar = char.IsNumber(ch) || (ch=='/');
                if (!isValidChar)
                    return false;
                if (ch == '/')
                    fractionChars++;
            }

            // 12/34/5
            if (fractionChars > 1)
                return false;

            // '123/', '/223'
            if ((s[0] == '/') || (s[s.Length - 1] == '/'))
                return false;
            return true;
        }

        #region *** NrXXX ***

        private string mNrWoj;
        public string NrWoj {
            set
            { 
                CheckLengthAndDigit(value, 2, "NrWoj");
                mNrWoj = value;
            }
            get { return mNrWoj; }
        }

        private string mNrPow;
        public string NrPow
        {
            set
            {
                CheckLengthAndDigit(value, 2, "NrPow");
                mNrPow = value;
            }
            get { return mNrPow; }
        }

        private string mNrGmi;
        public string NrGmi
        {
            set
            {
                CheckLengthAndDigit(value, 2, "NrGmi");
                mNrGmi = value;
            }
            get { return mNrGmi; }
        }

        private string mRodz;
        public string Rodz
        {
            set
            {
                CheckLengthAndDigit(value, 1, "Rodz");
                mRodz = value;
            }
            get { return mRodz; }
        }

        private string mNrObr;
        public string NrObr
        {
            set
            {
                CheckLengthAndDigit(value, 4, "NrObr");
                mNrObr = value;
            }
            get { return mNrObr; }
        }

        public int NrObrInt
        {
            get { return Convert.ToInt32(NrObr); }
        }

        private string mNrAr;
        public string NrAr
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                    CheckIsDigit(value, "NrAr");
                mNrAr = value;
            }
            get { return mNrAr; }
        }

        private string mNrDz;
        public string NrDz
        {
            set
            {
                mNrDz = value;
            }
            get { return mNrDz; }
        }

        public string NrArDz
        {
            get
            {
                if (string.IsNullOrEmpty(mNrAr))
                    return mNrDz;
                return mNrAr + "." + mNrDz;
            }
        }

        public bool ContainsNrArDz(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            var arDz = s.Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arDz.Length > 1)
                return this.NrAr == arDz[0] && this.ContainsNrDz(arDz[1]);
            else
                return this.ContainsNrDz(s);
        }

        public bool ContainsNrDz(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            if (s.StartsWith("/"))
                return NrDz.EndsWith(s); // Gdynia 2/152, szukamy działki */152
            if (s.EndsWith("/"))
                return NrDz.StartsWith(s); // Gdańsk 152/2, szukamy działki 152/*
            //if (s.Contains("/"))
            //    return NrDz.Contains(s); // Wpisano 152/2, znajdzie działkę 152/22 i 152/23

            return NrDz == s || NrDz.StartsWith(s + "/"); //szukamy działki 152 lub 152/
        }

        #endregion

        #region *** IdXXX ***

        public string Id
        {
            get {
                string res = IdDz;
                if (res != null)
                    return res;

                res = IdObr;
                if (res != null)
                    return res;

                res = IdGmi;
                if (res != null)
                    return res;

                res = IdPow;
                if (res != null)
                    return res;

                return IdWoj;
            }
            set
            {
                mNrWoj = null;
                mNrPow = null;
                mNrGmi = null;
                mRodz = null;
                mNrObr = null;
                mNrAr = null;
                mNrDz = null;

                mNrWoj = Substring(value, 0, 2, true);
                if (mNrWoj == null)
                    return;

                mNrPow = Substring(value, 2, 2, true);
                if (mNrPow == null)
                    return;

                mNrGmi = Substring(value, 4, 2, true);
                if (mNrGmi == null)
                    return;

                if (!Contains(value, 6, '_'))
                    return; // Invalid G5 IDD
                mRodz = Substring(value, 7, 1, true);
                if (mRodz == null)
                    return;

                if (!Contains(value, 8, '.'))
                    return; // Invalid G5 IDD
                mNrObr = Substring(value, 9, 4, true);
                if (mNrObr == null)
                    return;

                if (!Contains(value, 13, '.'))
                    return; // Invalid G5 IDD

                // AR_138.13/2
                var arDz = Substring(value, 14, -1, false);
                if (arDz == null)
                    return;
                if (arDz.StartsWith("AR_"))
                {
                    int dotPos = arDz.IndexOf('.'); // = 6
                    mNrAr = Substring(arDz, 3, dotPos - 3, false); // W gdyni jest np. 226201_1.0002.AR_1M.231/2006, trzeba sprawdzić AR_1M
                    if (dotPos > 0) //226201_1.0002.AR_138
                        mNrDz = Substring(arDz, dotPos + 1, -1, false);
                }
                else
                {
                    mNrAr = null;
                    mNrDz = arDz;
                }
            }
        }

        public string IdWoj
        {
            get { return NrWoj; }
        }
        public string IdPow
        {
            get {
                if (NrWoj == null || NrPow == null)
                    return null;
                return NrWoj + NrPow;
            }
        }
        public string IdGmi
        {
            get
            {
                if (NrWoj == null || NrPow == null || NrGmi == null || Rodz == null)
                    return null;
                return NrWoj + NrPow + NrGmi + "_" + Rodz;
            }
        }
        public string IdObr
        {
            get
            {
                var idg = IdGmi;
                if (idg == null || NrObr == null)
                    return null;
                return idg + "." + NrObr;
            }
        }

        public string IdAr
        {
            get
            {
                var ido = IdObr;
                if (ido == null || NrAr == null)
                    return null;
                return ido + ".AR_" + NrAr;
            }
        }

        // 226101_1.0075.3/57
        // 226201_1.0002.AR_138.13
        public string IdDz
        {
            get
            {
                var ido = IdObr;
                if (ido == null || NrDz == null)
                    return null;

                if (NrAr == null)
                    return ido + "." + NrDz;
                else
                    return ido + ".AR_" + NrAr + "." + NrDz;
            }
        }

        #endregion

        
        public TercObr Obr
        {
            get {  return Terc.Records.TryGetObr(IdObr); }
        }

        public TercGmi Gmi
        {
            get {  return Terc.Records.TryGetGmi(IdGmi); }
        }

        public TercPow Pow
        {
            get { return Terc.Records.TryGetPow(IdPow); }
        }

        public TercWoj Woj
        {
            get { return Terc.Records.TryGetWoj(IdWoj); }
        }

        private class G5IdComparer : IComparer<IG5IdProvider>, IComparer<G5Id>
        {
            private class IdFraction : IComparable<IdFraction>
            {
                public int Ar = 0;
                public int M = 0;
                public int D = 0;
                public string Obr = "";

                public IdFraction(G5Id idd)
                {
                    Obr = idd.IdObr;
                    if (!string.IsNullOrEmpty(idd.NrAr))
                        int.TryParse(idd.NrAr, out Ar); // TryParse is about 1000 x faster than catching exceptions (if fails TryParse set Ar=0)

                    var arr = idd.NrDz.Split('/');
                    int.TryParse(arr[0], out M);
                    if (arr.Length > 1)
                        int.TryParse(arr[1], out D);
                }

                public int CompareTo(IdFraction other)
                {
                    if (this.Obr != other.Obr)
                        return this.Obr.CompareTo(other.Obr);

                    if (this.Ar != other.Ar)
                        return this.Ar.CompareTo(other.Ar);

                    if (this.M != other.M)
                        return this.M.CompareTo(other.M);

                    return this.D.CompareTo(other.D);
                }
            }

            int IComparer<G5Id>.Compare(G5Id x, G5Id y)
            {
                try
                {
                    var xFraction = new IdFraction(x);
                    var yFraction = new IdFraction(y);
                    return xFraction.CompareTo(yFraction);
                }
                catch (Exception)
                {
                    return string.Compare(x.IdDz, y.IdDz);
                }
            }

            public int Compare(IG5IdProvider x, IG5IdProvider y)
            {
                return this.Compare(x.G5Id, y.G5Id);
            }
        }


        public static IComparer<G5Id> Comparer = new G5IdComparer();

        public int CompareTo(G5Id other)
        {
            return Comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            var hc = Id;
            if (hc == null)
                return 0;
            return hc.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var g5obj = obj as G5Id;
            if (g5obj == null)
                return base.Equals(obj);

            return this.Id == g5obj.Id;
        }

        public override string ToString()
        {
            return Id;
        }

        G5Id IG5IdProvider.G5Id
        {
            get { return this; }
        }
    }



}
