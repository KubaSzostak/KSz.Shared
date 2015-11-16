using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System
{
    public class Calc
    {

        public const double PI = 3.14159265358979323846264338327950288419716939937510;
                                 
        public const double deg2rad = PI / 180.0;
        public const double rad2deg = 180.0 / PI;

        #region *** More, Less, Equal, Between ***

        public static bool MoreThan<T>(T v1, T v2) where T : IComparable<T>
        {
            return v1.CompareTo(v2) == 1;
        }
        public static bool MoreThanOrEqual<T>(T v1, T v2) where T : IComparable<T>
        {
            return MoreThan<T>(v1, v2) || IsEqual<T>(v1, v2);
        }

        public static bool LessThan<T>(T v1, T v2) where T : IComparable<T>
        {
            return v1.CompareTo(v2) == -1;
        }

        public static bool LessThanOrEqual<T>(T v1, T v2) where T : IComparable<T>
        {
            return LessThan<T>(v1, v2) || IsEqual<T>(v1, v2);
        }

        public static bool IsEqual<T>(T v1, T v2) where T : IComparable<T>
        {
            return v1.CompareTo(v2) == 0;
        }

        public static bool IsEqual(double v1, double v2, double precision)
        {
            return Math.Abs(v1 - v2) < Math.Abs(precision);
        }

        /// <summary>
        /// For min=value || max=value returns true
        /// </summary>
        public static bool IsBetween<T>(T min, T max, T value) where T : IComparable<T>
        {
            IComparable<T> v = (value as IComparable<T>);
            if (v == null)
                return false;
            //      v=5, min=7;  
            if (LessThan<T>(value, min))  //bez OrEqual dla min=value
                return false;
            //      v=5, max=3
            if (MoreThan<T>(value, max))
                return false;
            // v=5, min=5
            return true;
        }

        /// <summary>
        /// For min=value, max=value returns true
        /// </summary>
        public static bool IsBetween(DateTime min, DateTime max, params DateTime[] values)
        {
            foreach (DateTime v in values)
                if ((v < min) || (v > max)) //bez = dla min=value
                    return false;
            return true;
        }

        /// <summary>
        /// For min=value, max=value returns true
        /// </summary>
        public static bool IsBetween(double min, double max, params double[] values)
        {
            foreach (double v in values)
                if ((v < min) || (v > max)) //bez = dla min=value
                    return false;
            return true;
        }


        #endregion

        /// <summary>
        /// Średnia z dwóch wartość
        /// </summary>
        /// <param name="v1">V1</param>
        /// <param name="v2">V2</param>
        /// <param name="w">Waga: 0.0 oznacza wartość V1, 1.0 oznacza wartość V2</param>
        /// <returns></returns>
        public static double Mean(double v1, double v2, double w)
        {
            return v1 + (v2 - v1) * w;
        }

        public static double MeanWeight(double v1, double v2, double meanValue)
        {
            double res = (meanValue - v1) / (v2 - v1);
            if ((res < 0) || (res > 1))
                throw new Exception("meanValue must be between " + v1.ToString() + " and " + v2.ToString() + ".");

            return res;
        }

        public static double Mean(double v1, double v2)
        {
            return Mean(v1, v2, 0.5);
        }


        /// <summary>
        ///     Returns the largest integer less than or equal to the specified double-precision
        ///     floating-point number.
        /// </summary>
        /// <param name="value">A double-precision floating-point number.</param>
        /// <returns>
        ///     The largest integer less than or equal to d. If d is equal to System.Double.NaN,
        ///     System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
        ///     is returned.
        ///</returns>
        public static int Floor(double value)
        {
            return Convert.ToInt32(Math.Floor(value));
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the specified double-precision
        /// floating-point number.
        /// </summary>
        /// <param name="value">A double-precision floating-point number.</param>
        /// <returns>
        /// The smallest integer greater than or equal to a. If a is equal to System.Double.NaN,
        /// System.Double.NegativeInfinity, or System.Double.PositiveInfinity, that value
        /// is returned.
        /// </returns>
        public static int Ceiling(double value)
        {
            return Convert.ToInt32(Math.Ceiling(value));
        }

        /// <summary>
        /// Rounds a double-precision floating-point value to the nearest integer. 
        /// </summary>
        /// <param name="value">A double-precision floating-point number to be rounded.</param>
        /// <returns>
        /// The integer nearest value. If value is halfway between two integers, the even 
        /// value is returned
        /// </returns>
        public static int Round(double value)
        {
            return Convert.ToInt32(Math.Round(value));
            //return Convert.ToInt32(Math.Round(value, MidpointRounding.ToEven));
        }

        public static double Azimuth(double dx, double dy)
        {
            double res = Math.Atan2(dy, dx);
            if (res < 0)
                res = res + PI * 2;
            return res;
        }

        public static double DeltaX(double dist, double az)
        {
            return dist * Math.Cos(az);
        }

        public static double DeltaY(double dist, double az)
        {
            return dist * Math.Sin(az);
        }

        public static double RadToDeg(double a)
        {
            return a * 180.0 / PI;
        }

        public static double DegToRad(double a)
        {
            return a * PI / 180.0;
        }

        public static double RadToGrad(double a)
        {
            return a * 200.0 / PI;
        }

        public static double GradToRad(double a)
        {
            return a * PI / 200.0;
        }

        public static double Dist(double dx, double dy, double dz)
        {
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static double Dist(double dx, double dy)
        {
            return Dist(dx, dy, 0.0);
        }

        public static double Max(params double[] values)
        {
            double max = double.MinValue;
            foreach (double v in values)
            {
                max = Math.Max(max, v);
            }
            return max;
        }

        public static double Sign(double value)
        {
            if (value < 0)
                return -1;
            else
                return 1;
        }

        public static DateTime Min(DateTime v1, DateTime v2)
        {
            if (v1 < v2)
                return v1;
            else
                return v2;
        }

        public static T Min<T>(T v1, T v2) where T : IComparable<T>
        {
            if (v1 == null)
                return v1;
            if (v2 == null)
                return v2;

            if (LessThan<T>(v1, v2))
                return v1;
            else
                return v2;
        }

        public static T Max<T>(T v1, T v2) where T : IComparable<T>
        {
            if (v1 == null)
                return v2;
            if (v2 == null)
                return v1;

            if (MoreThan<T>(v1, v2))
                return v1;
            else
                return v2;
        }

        public static T Max<T>(params T[] vArr) where T : IComparable<T>
        {
            T res = vArr[0];
            foreach (T v in vArr)
            {
                res = Max<T>(res, v);
            }
            return res;
        }

        public static T Min<T>(params T[] vArr) where T : IComparable<T>
        {
            T res = vArr[0];
            foreach (T v in vArr)
            {
                res = Min<T>(res, v);
            }
            return res;
        }

        public static DateTime Max(DateTime v1, DateTime v2)
        {
            if (v1 > v2)
                return v1;
            else
                return v2;
        }

        public static Time Max(params Time[] t1)
        {
            return Max<Time>(t1);
        }
        public static Time Min(params Time[] t1)
        {
            return Min<Time>(t1);
        }

        public static Date Max(params Date[] v1)
        {
            return Max<Date>(v1);
        }
        public static Date Min(params Date[] v1)
        {
            return Min<Date>(v1);
        }
    }

    public struct Sign
    {
        private bool isPlus;

        public Sign(double value)
        {
            isPlus = (value >= 0.0);
        }

        private double Value
        {
            get
            {
                if (this.isPlus)
                    return 1.0;
                else
                    return -1.0;
            }
        }

        public override bool Equals(object obj)
        {
            Sign s;
            if (obj is Sign)
                s = (Sign)obj;
            else if (obj is double)
                s = new Sign((double)obj);
            else
                return false;

            return s.isPlus == this.isPlus;
        }

        public override int GetHashCode()
        {
            return this.isPlus.GetHashCode();
        }

        public override string ToString()
        {
            if (isPlus)
                return "+";
            else
                return "-";
        }

        public static Sign Plus
        {
            get { return new Sign(1); }
        }

        public static Sign Minus
        {
            get { return new Sign(-1); }
        }

        public static implicit operator double(Sign s)
        {
            return s.Value;
        }

        public static implicit operator Sign(double d)
        {
            return new Sign(d);
        }

        public static double operator *(double d, Sign s)
        {
            return d * s.Value;
        }

        public static double operator *(Sign s, double d)
        {
            return d * s.Value;
        }

        public static bool operator ==(Sign s1, Sign s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(Sign s1, Sign s2)
        {
            return !s1.Equals(s2);
        }

    }

    public interface IMeanItem
    {
        double x { get; set; }
        double p { get; set; }
        double v { get; set; }
    }

    public class Mean : List<IMeanItem>
    {

        public void Add(double value)
        {
            Add(value, 1);
        }

        public void Add(double x, double p)
        {
            Item i = new Item();
            i.x = x;
            i.p = p;
            Add(i);
        }

        public Result CalcMean()
        {
            double px = 0.0;
            double p = 0.0;
            double pvv = 0.0;
            Result res;

            foreach (Item i in this)
            {
                px += i.x * i.p;
                p += i.p;
            }
            res.Mean = px / p;

            for (int i = 0; i < Count; i++)
            {
                IMeanItem mi = this[i];
                mi.v = res.Mean - mi.x;
                pvv += mi.v * mi.v * mi.p;
            }

            res.m0 = Math.Sqrt(pvv / (Count - 1));
            res.m = res.m0 / Math.Sqrt(p);
            return res;
        }

        public struct Item : IMeanItem
        {
            public double x; // Value
            public double p; // Value weight
            public double v; // Odchyłka

            #region IMeanItem Members

            double IMeanItem.x
            {
                get { return x; }
                set { x = value; }
            }

            double IMeanItem.p
            {
                get { return p; }
                set { p = value; }
            }

            double IMeanItem.v
            {
                get { return v; }
                set { v = value; }
            }

            #endregion
        }

        public struct Result
        {
            public double Mean;
            public double m;
            public double m0;
        }
    }

    public class Matrix<TRow, TCol, TVal>
    {
        private class MatrixRow : Dictionary<TCol, TVal> { }
        private class MatrixRows : Dictionary<TRow, MatrixRow> { }

        private Dictionary<TCol, TCol> mCols = new Dictionary<TCol, TCol>();
        private MatrixRows mRows = new MatrixRows();

        public TVal this[TRow row, TCol col]
        {
            get
            {
                MatrixRow rowData;
                TVal val;

                if (!mRows.TryGetValue(row, out rowData))
                    return default(TVal);

                // Jeżeli nie znajdzie "key" zwróci i tak default(TVal)
                rowData.TryGetValue(col, out val);
                return val;
            }
            set
            {
                MatrixRow rowData;
                if (!mRows.TryGetValue(row, out rowData))
                {
                    rowData = new MatrixRow();
                    mRows[row] = rowData;
                }
                rowData[col] = value;
                mCols[col] = col;
            }

        }


        public ICollection<TRow> Rows
        {
            get { return mRows.Keys; }
        }

        public ICollection<TCol> Cols
        {
            get { return mCols.Keys; }
        }

    }

    public class Matrix : Matrix<int, int, double>
    {
    }
}
