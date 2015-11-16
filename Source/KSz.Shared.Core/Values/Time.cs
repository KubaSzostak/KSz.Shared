using System;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public struct Time : IComparable, IComparable<Time>, IEquatable<Time>, IAssignable
    {
        internal TimeSpan mValue;

        public Time(int hours, int minutes)
        {
            mValue = new TimeSpan(hours, minutes, 0);
        }

        public Time(TimeSpan value)
        {
            mValue = value;
        }

        public Time(TimeSpan? value)
        {
            mValue = value.GetValueOrDefault();
        }

        public Time(long ticks)
        {
            mValue = new TimeSpan(ticks);
        }

        public Time(double hours)
        {
            mValue = TimeSpan.FromHours(hours);
        }

        public override string ToString()
        {
            TimeSpan positveValue = mValue.Duration();
            string sign = "";
            if (mValue < TimeSpan.Zero)
                sign = "-";
            int hours = Calc.Floor(positveValue.TotalHours);
            int minutes = positveValue.Minutes;

            return sign + hours.ToString() + ":" + minutes.ToString("00");
        }

        public override bool Equals(object obj)
        {
            if (obj is Time)
                return mValue.Equals(((Time)obj).mValue);
            if (obj is TimeSpan)
                return mValue.Equals((TimeSpan)obj);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return mValue.GetHashCode();
        }

        public static Time Parse(string s)
        {
            s = s.Trim();
            if (s.IsEmpty())
            {
                return new Time(0);
            }

            // "8.23 -> 8:23
            s = s.Replace(",", ":");
            s = s.Replace(".", ":");

            // 9 -> 9:00
            if (!s.Contains(":"))
                s += ":00";

            try
            {
                string[] values = s.Split(':');
                if (values.Length == 2)
                {
                    // Najpierw uwzględnij format np. 32:28, czyli 32h 28min
                    // TimeSpan.Parse wywala się na tym
                    double hours = values[0].ToDouble();
                    double minuts = values[1].ToDouble();
                    TimeSpan ts = TimeSpan.FromHours(hours + minuts / 60);
                    return new Time(ts);
                }
                return new Time(TimeSpan.Parse(s));
            }
            catch (FormatException)
            {
                throw new FormatException(SysUtils.Strings.InvalidTimeFormat);
            }
        }

        public static Time Zero
        {
            get { return new Time(); }
        }


        public static Time FromHours(double h)
        {
            return new Time(TimeSpan.FromHours(h));
        }
        
        #region ***  IComparable  ***

        public int CompareTo(Time other)
        {

            return mValue.CompareTo(other.Value);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is Time)
                return mValue.CompareTo(((Time)obj).Value);
            if (obj is TimeSpan)
                return mValue.CompareTo((TimeSpan)obj);
            if (obj is IComparable)
                (obj as IComparable).CompareTo(obj);
            throw new ArgumentException();
            //return mValue.CompareTo(obj);
        }

        #endregion

        #region *** IEquatable<Time>  ***

        bool IEquatable<Time>.Equals(Time other)
        {
            return mValue.Equals(other);
        }

        #endregion


        #region ***  IAssignable  ***

        public void AssignFromText(string s)
        {
            mValue = Parse(s).mValue;
        }

        #endregion

        #region ***  Operators  ***

        public static implicit operator TimeSpan(Time value)
        {
            return value.mValue;
        }

        public static implicit operator Time(TimeSpan value)
        {
            return new Time(value.Ticks);
        }

        public static Time operator -(Time t)
        {
            return new Time(-t.mValue);
        }

        public static Time operator -(Time t1, Time t2)
        {
            return new Time(t1.mValue - t2.mValue);
        }

        public static Time operator -(TimeSpan t1, Time t2)
        {
            return new Time(t1 - t2.mValue);
        }

        public static Time operator -(Time t1, TimeSpan t2)
        {
            return new Time(t1.mValue - t2);
        }

        public static Time operator +(Time t)
        {
            return new Time(+t.mValue);
        }

        public static Time operator +(Time t1, Time t2)
        {
            return new Time(t1.mValue + t2.mValue);
        }

        public static Time operator +(Time t1, TimeSpan t2)
        {
            return new Time(t1.mValue + t2);
        }

        public static Time operator +(TimeSpan t1, Time t2)
        {
            return new Time(t1 + t2.mValue);
        }

        public static bool operator <(Time t1, Time t2)
        {
            return t1.mValue < t2.mValue;
        }

        public static bool operator <=(Time t1, Time t2)
        {
            return t1.mValue <= t2.mValue;
        }

        public static bool operator ==(Time t1, Time t2)
        {
            return t1.mValue == t2.mValue;
        }

        public static bool operator !=(Time t1, Time t2)
        {
            return t1.mValue != t2.mValue;
        }

        public static bool operator >(Time t1, Time t2)
        {
            return t1.mValue > t2.mValue;
        }

        public static bool operator >=(Time t1, Time t2)
        {
            return t1.mValue >= t2.mValue;
        }

        #endregion

        public Time AddHours(double h)
        {
            return mValue.Add(TimeSpan.FromHours(h));
        }

        public Time AddDays(double d)
        {
            return mValue.Add(TimeSpan.FromDays(d));
        }

        public TimeSpan Value
        {
            get { return mValue; }
        }
    }
}
