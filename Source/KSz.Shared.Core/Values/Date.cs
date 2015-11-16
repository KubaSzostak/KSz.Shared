using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public struct Date : IComparable, IComparable<Date>, IEquatable<Date>, IAssignable, IFormattable
    {
        public DateTime Value;

        public Date(int year, int month, int day)
        {
            Value = new DateTime(year, month, day);
        }

        public Date(DateTime date)
        {
            Value = date.Date;
        }

        public static Date Parse(string s)
        {
            Date res = new Date();
            s = s.Replace(",", ".");
            res.Value = DateTime.Parse(s).Date;
            return res;
        }

        #region ***  IComparable  ***

        public int CompareTo(Date other)
        {
            return Value.CompareTo(other);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is Date)
                return CompareTo((Date)obj);
            if (obj is IComparable)
                (obj as IComparable).CompareTo(obj);
            throw new ArgumentException();
        }

        #endregion

        #region *** IEquatable<Time>  ***

        bool IEquatable<Date>.Equals(Date other)
        {
            return Value.Equals(other);
        }

        #endregion
        
        #region ***  IAssignable  ***

        public void AssignFromText(string s)
        {
            Value = Parse(s).Value;
        }

        #endregion

        #region ***  Operators  ***

        public static implicit operator DateTime(Date value)
        {
            return value.Value;
        }

        public static implicit operator Date(DateTime value)
        {
            return new Date(value.Date);
        }

        public static TimeSpan operator -(Date t1, Date t2)
        {
            return t1.Value - t2.Value;
        }

        public static Date operator -(Date t1, TimeSpan t2)
        {
            return new Date(t1.Value - t2);
        }

        public static bool operator !=(Date t1, Date t2)
        {
            return t1.Value != t2.Value;
        }

        public static Date operator +(Date t1, TimeSpan t2)
        {
            return new Date(t1.Value + t2);
        }

        public static bool operator <(Date t1, Date t2)
        {
            return t1.Value < t2.Value;
        }

        public static bool operator <=(Date t1, Date t2)
        {
            return t1.Value <= t2.Value;
        }

        public static bool operator ==(Date t1, Date t2)
        {
            return t1.Value == t2.Value;
        }

        public static bool operator >(Date t1, Date t2)
        {
            return t1.Value > t2.Value;
        }

        public static bool operator >=(Date t1, Date t2)
        {
            return t1.Value >= t2.Value;
        }

        #endregion



        #region ***  IFormattable  ***

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format.IsEmpty())
                return ToString();
            return Value.ToString(format, formatProvider);
        }

        public string ToString(string format)
        {
            if (format.IsEmpty())
                return ToString();
            return Value.ToString(format);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Date)
                return Value.Equals(((Date)obj).Value);
            if (obj is DateTime)
                return Value.Equals(((DateTime)obj).Date);
            return base.Equals(obj);
        }

        public int Month
        {
            get { return Value.Month; }
        }

        public int Year
        {
            get { return Value.Year; }
        }

        public int Day
        {
            get { return Value.Day; }
        }

        public DayOfWeek DayOfWeek
        {
            get { return Value.DayOfWeek; }
        }


    }


}
