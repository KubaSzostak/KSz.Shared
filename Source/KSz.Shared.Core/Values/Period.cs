using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System
{


    public class StartEndValue<T> : IAssignable
        where T : IComparable<T>
    {
        public StartEndValue(T start, T end)
        {
            mStart = start;
            mEnd = end;
        }

        public StartEndValue()
            : this(default(T), default(T))
        { }


        private T mStart;
        public T Start
        {
            get { return mStart; }
            set
            {
                mStart = value;
                if (Calc.LessThan<T>(mStart, mEnd))
                    mEnd = mStart;
            }
        }

        private T mEnd;
        public T End
        {
            get { return mEnd; }
            set
            {
                mEnd = value;
                if (Calc.MoreThan<T>(mStart, mEnd))
                    mStart = mEnd;
            }
        }

        protected virtual string ValueToString(T value)
        {
            return value.ToString();
        }

        protected virtual T StringToValue(string s)
        {
            return (T)Convert.ChangeType(s, typeof(T), CultureInfo.CurrentCulture);
        }

        public override string ToString()
        {
            return ValueToString(Start) + " - " + ValueToString(End);
        }

        public override bool Equals(object obj)
        {
            if (obj is StartEndValue<T>)
            {
                StartEndValue<T> o = obj as StartEndValue<T>;
                return
                    Calc.IsEqual<T>(o.Start, this.Start)
                    && Calc.IsEqual<T>(o.End, this.End);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public virtual void AssignFromText(string value)
        {
            value = value.Trim();
            if (value.IsEmpty())
            {
                mStart = default(T);
                mEnd = default(T);
                return;
            }

            value = value.Replace("-", " ");

            // "8:23 16:55"
            string[] values = value.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != 2)
                throw new FormatException();

            Assign(StringToValue(values[0]), StringToValue(values[1]));
        }

        protected virtual void Assign(T start, T end)
        {
            Start = start;
            End = end;
        }

        public virtual void AssignFrom(StartEndValue<T> src)
        {
            this.Start = src.Start;
            this.End = src.End;
        }

        public object Clone()
        {
            StartEndValue<T> res = Activator.CreateInstance(this.GetType()) as StartEndValue<T>;
            res.Start = this.Start;
            res.End = this.End;
            return res;
        }
    }


    public class TimePeriod : StartEndValue<Time>
    {
        public TimePeriod(Time start, Time end)
            : base(start, end)
        { }

        public TimePeriod()
            : base()
        { }


        #region Operators

        public static implicit operator TimeSpan(TimePeriod value)
        {
            return value.Span;
        }

        #endregion

        protected override Time StringToValue(string value)
        {
            return Time.Parse(value);
        }

        protected override string ValueToString(Time value)
        {
            int hours = Calc.Floor(value.mValue.TotalHours);
            return hours.ToString() + ":" + value.mValue.Minutes.ToString("00");
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public TimeSpan Span
        {
            get { return End.mValue - Start.mValue; }
        }

    }


    public class DatePeriod : StartEndValue<Date>
    {
        public DatePeriod(Date start, Date end)
            : base(start, end)
        { }

        public DatePeriod()
            : base()
        { }


        #region Operators

        public static implicit operator TimeSpan(DatePeriod value)
        {
            return value.Span;
        }

        #endregion

        protected override Date StringToValue(string value)
        {
            return Date.Parse(value);
        }

        public TimeSpan Span
        {
            get { return End.Value - Start.Value; }
        }

    }

}
