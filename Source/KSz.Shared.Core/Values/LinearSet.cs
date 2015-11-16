using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace System
{

    public struct Range
    {
        private double mXMax;
        public double XMax
        {
            get { return mXMax; }
            set { mXMax = value; }
        }

        private double mXMin;
        public double XMin
        {
            get { return mXMin; }
            set { mXMin = value; }
        }

        private double mYMax;
        public double YMax
        {
            get { return mYMax; }
            set { mYMax = value; }
        }

        private double mYMin;
        public double YMin
        {
            get { return mYMin; }
            set { mYMin = value; }
        }


        public double Width
        {
            get { return YMax - YMin; }
        }

        public double Heigth
        {
            get { return XMax - XMin; }
        }
    }


    // TODO: Change LinearSet to RangeSet

    public abstract class LinearSet<T> where T : IComparable<T>
    {

        // Item1:         -------
        // start, end:    *     *

        // Item2:                  ---
        // start, end:             * *

        // Item3:                         --------
        // start, end:                    *      *

        // SpanSum:       -----    ---    --------
        // Min:           *
        // Max:                                  *
        // IsInSet()        *                         () => true
        // IsInSet()             *                    () => false

        public LinearSet()
        {
            Clear();
        }

        public LinearSet(T start, T end)
            : this()
        {
            mFirst = new Item(this);
            First.Start = start;
            First.End = end;
        }

        public LinearSet<T> Clone()
        {
            var res = this.CreateNew();
            res.Add(this);
            return res;
        }

        private Item mFirst;
        public Item First
        {
            get { return mFirst; }
        }

        public Item GetLast()
        {
            Item last = First;
            if (last != null)
            {
                while (last.Next != null)
                {
                    last = last.Next;
                }
            }
            return last;
        }

        #region ***  Algebra  ***

        protected abstract T AddValues(T v1, T v2);

        // Subtract = v1 - v2
        protected abstract T SubtractValues(T v1, T v2);

        internal static bool MoreThan(T v1, T v2)
        {
            return Calc.MoreThan<T>(v1, v2);
        }

        internal static bool MoreThanOrEqual(T v1, T v2)
        {
            return MoreThan(v1, v2) || IsEqual(v1, v2);
        }

        internal static bool LessThan(T v1, T v2)
        {
            return Calc.LessThan<T>(v1, v2);
        }

        internal static bool LessThanOrEqual(T v1, T v2)
        {
            return LessThan(v1, v2) || IsEqual(v1, v2);
        }


        internal static bool IsEqual(T v1, T v2)
        {
            return Calc.IsEqual<T>(v1, v2);
        }

        #endregion

        public T SpanSum
        {
            get
            {
                T res = default(T);
                Item i = First;
                while (i != null)
                {
                    res = AddValues(res, i.Span);
                    i = i.Next;
                }
                return res;
            }
        }

        public T Min
        {
            get
            {
                if (First != null)
                    return First.Start;
                else
                    return default(T);
            }
        }

        public T Max
        {
            get
            {
                Item last = GetLast();
                if (last != null)
                    return last.End;
                else
                    return default(T);
            }
        }

        public bool IsInSet(T value)
        {
            Item i = First;
            while (i != null)
            {
                if (i.InRange(value))
                    return true;
                i = i.Next;
            }
            return false;
        }

        protected abstract LinearSet<T> CreateNew();

        private Item FindFirstBefore(T value)
        {
            if (First == null)
                return null;
            if (LessThan(value, First.Start))
                return null;

            Item res = First;
            while (res.Next != null)
            {
                //if (res.Next.Start > value)
                if (res.Next.Start.CompareTo(value) > 0)
                    return res;
                res = res.Next;
            }
            return res;
        }

        private bool DeleteFromInRangeItem(Item item, T start, T end)
        {
            // Item:         -------------------------------
            // start, end:        ********************
            // item result:  -----                    ------

            if (LessThan(start, item.Start))
                return false;
            if (MoreThan(end, item.End))
                return false;
            if (IsEqual(item.Start, start) && IsEqual(item.End, end))
            {
                item.Remove();
                return true;
            }

            if (IsEqual(item.Start, start))
            {
                item.Start = end;
            }
            else if (IsEqual(item.End, end))
            {
                item.End = start;
            }
            else
            {
                T itemEnd = item.End;
                item.End = start;
                item.AddAfter(end, itemEnd);
            }
            return true;

        }

        public LinearSet<T> Extract(T start, T end)
        {
            LinearSet<T> res = CreateNew();

            // Poaczątek nie może być większy od końca
            if (MoreThanOrEqual(start, end))
                return res;

            Item item = FindFirstBefore(start);
            if (item == null)
                item = First;
            if (First == null)
                return res;


            // Item:         -------------------------------
            // start, end:        ********************
            if (DeleteFromInRangeItem(item, start, end))
            {
                // Cały zakres jest w zakresie jednego elementu
                res.Add(start, end);
                return res;
            }


            // Item:         ----------    -------  ------
            // start, end:        *********************
            if (item.InRange(start))
            {
                res.Add(start, item.End);
                item.End = start;
            }


            // Items:      -----    ------   -----  ----    -------
            // start, end:        ******************************
            if (LessThan(item.End, start)) // Już usunięto poprzedni element
                item = item.Next;
            if (item == null)
                return res;
            while (LessThanOrEqual(start, item.Start) && MoreThanOrEqual(end, item.End))
            {
                Item delItem = item;
                res.Add(item.Start, item.End);
                item = item.Next;
                delItem.Remove();
                if (item == null)
                {
                    //res.RemoveEmptyItems();
                    return res;
                }
            }

            // Item:      -----------    ----------------
            // start, end:   ********************
            if ((item != null) && (item.InRange(end)))
            {
                res.Add(item.Start, end);
                item.Start = end;
            }

            //res.RemoveEmptyItems();
            return res;
        }

        public LinearSet<T> Extract(LinearSet<T> set)
        {
            LinearSet<T> res = CreateNew();
            Item item = set.First;
            while (item != null)
            {
                res.Add(Extract(item.Start, item.End));
                item = item.Next;
            }
            return res;
        }

        public LinearSet<T> ExtractAfter(T start, T spanSum)
        {
            LinearSet<T> res = CreateNew();

            Item item = FindFirstBefore(start);
            if (item == null)
                item = First;
            T startPlusSpanSum = AddValues(start, spanSum);


            while (item != null)
            {
                Item delItem = null;

                if (DeleteFromInRangeItem(item, start, startPlusSpanSum))
                {
                    // Cały spanSum jest w zakresie jednego elementu
                    res.Add(start, startPlusSpanSum);
                    return res;
                }
                if (item.InRange(start))
                {
                    res.Add(start, item.End);
                    item.End = start;
                }
                else if (MoreThanOrEqual(spanSum, AddValues(res.SpanSum, item.Span)))
                {
                    delItem = item;
                    res.Add(item.Start, item.End);
                }
                else
                {
                    T spanToAdd = SubtractValues(spanSum, res.SpanSum);
                    T addedSpanEnd = AddValues(item.Start, spanToAdd);
                    res.Add(item.Start, addedSpanEnd);
                    item.Start = addedSpanEnd;
                }

                item = item.Next;
                if (delItem != null)
                    delItem.Remove();

                if (MoreThan(res.SpanSum, spanSum))
                    throw new Exception("FATAL ERROR: LinearSet.ExtractAfter");
                if (IsEqual(res.SpanSum, spanSum))
                    return res;
            }
            return res;
        }

        public LinearSet<T> ExtractBefore(T end, T spanSum)
        {
            LinearSet<T> res = CreateNew();

            Item item = FindFirstBefore(end);
            if (item == null)
                return res;

            T endMinusSpanSum = SubtractValues(end, spanSum);
            while (item != null)
            {
                Item delItem = null;
                if (DeleteFromInRangeItem(item, endMinusSpanSum, end))
                {
                    // Cały spanSum jest w zakresie jednego elementu
                    res.Add(endMinusSpanSum, end);
                    return res;
                }
                if (item.InRange(end))
                {
                    res.Add(item.Start, end);
                    item.Start = end;
                }
                else if (MoreThanOrEqual(spanSum, AddValues(res.SpanSum, item.Span)))
                {
                    delItem = item;
                    res.Add(item.Start, item.End);
                }
                else
                {
                    T spanToAdd = SubtractValues(spanSum, res.SpanSum);
                    T addedSpanStart = SubtractValues(item.End, spanToAdd);
                    res.Add(addedSpanStart, item.End);
                    item.End = addedSpanStart;
                }

                item = item.Previous;
                if (delItem != null)
                    delItem.Remove();

                if (MoreThan(res.SpanSum, spanSum))
                    throw new Exception("FATAL ERROR: LinearSet.ExtractAfter");
                if (IsEqual(res.SpanSum, spanSum))
                    return res;
            }
            return res;
        }

        public LinearSet<T> ExtractFromStart(T spanSum)
        {
            if (First == null)
                return CreateNew();
            return ExtractAfter(First.Start, spanSum);
        }

        public LinearSet<T> ExtractFromEnd(T spanSum)
        {
            if (First == null)
                return CreateNew();

            // Znajdź ostatni element
            Item last = First;
            while (last.Next != null)
            {
                last = last.Next;
            }
            return ExtractBefore(last.End, spanSum);
        }

        public void Add(T start, T end)
        {
            if (IsEqual(start, end))
                return;

            Item item = FindFirstBefore(start);

            // Item:                     ---------
            // start, end:     ******
            if (item == null)
            {
                Item newItem = new Item(this);
                // Najpierw przypisz do zbioru, żeby przpisanie
                // Start, End wykonywało sprawdzanie
                if (First == null)
                    mFirst = newItem;
                else
                    First.Previous = newItem;
                newItem.Start = start;
                newItem.End = end;
                return;
            }

            // Item:        ---------
            // start, end:     ***************
            if (item.InRange(start))
            {
                if (LessThan(item.End, end))
                    item.End = end;
                return;
            }
            // Item:            ---------
            // start, end: *********
            Item nextItem = item.Next;
            if ((nextItem != null) && (nextItem.InRange(end)))
            {
                if (MoreThan(nextItem.Start, start))
                    nextItem.Start = start;
                return;
            }

            // Jeżeli dwa powyższe warunki nie są spełnione
            // nowy elemen musi być pomiędzy  item i item.Next
            // Item:            ---------               ----------
            // start, end:                 *********
            item.AddAfter(start, end);
        }

        public void Add(LinearSet<T> set)
        {
            Item item = set.First;
            while (item != null)
            {
                Add(item.Start, item.End);
                item = item.Next;
            }

        }

        public void AddAfter(T start, T spanSum)
        {
            if (mFirst == null)
            {
                mFirst = new Item(this);
                mFirst.Start = start;
                mFirst.End = AddValues(start, spanSum);
                return;
            }

            Item item = FindFirstBefore(start);
            if (item == null)
                item = First;

            T addedSpan = SubtractValues(start, start); // Zero
            T span;

            while (LessThan(addedSpan, spanSum))
            {
                if (item.Next != null)
                {   // Wypełnij miejsce miedzy elementami
                    span = SubtractValues(item.Next.Start, item.End);
                    addedSpan = AddValues(addedSpan, span);
                }
                else
                {   // Całą reszztę dodaj na koniec
                    span = SubtractValues(spanSum, addedSpan);
                    item.End = AddValues(item.End, span);
                }
            }
        }

        public void AddAfterStart(T spanSum)
        {
            if (First == null)
                throw new Exception("AddAfterStart: There are no elements in set.");
            AddAfter(First.Start, spanSum);
        }

        public void AddAfterEnd(T spanSum)
        {
            if (First == null)
                throw new Exception("AddAfterEnd: There are no elements in set.");
            Item last = First;
            while (last.Next != null)
            {
                last = last.Next;
            }
            AddAfter(last.End, spanSum);
        }

        public void Clear()
        {
            mFirst = null;
        }

        public void RemoveEmptyItems()
        {
            Item i = First;
            Item delItem;
            while (i != null)
            {
                delItem = i;
                i = i.Next;
                if (IsEqual(delItem.Start, delItem.End))
                    delItem.Remove();
            }
        }

        public override string ToString()
        {
            string res = "[";
            RemoveEmptyItems();
            Item i = First;
            while (i != null)
            {
                res += i.ToString() + ", ";
                i = i.Next;
            }
            if (res.EndsWith(", "))
                res = res.Remove(res.Length - 2);
            return res + "]";
        }

        public class Item
        {
            private LinearSet<T> mSet;

            public Item(LinearSet<T> set)
            {
                this.mSet = set;
            }

            private Item mNext = null;
            public Item Next
            {
                get { return mNext; }
                internal set
                {
                    value.mNext = this.Next;
                    if (value.mNext != null)
                        value.mNext.mPrevious = value;

                    value.mPrevious = this;
                    this.mNext = value;
                }
            }

            private Item mPrevious = null;
            public Item Previous
            {
                get { return mPrevious; }
                internal set
                {
                    value.mPrevious = this.Previous;
                    if (value.Previous != null)
                        value.Previous.mNext = value;

                    value.mNext = this;
                    this.mPrevious = value;

                    if ((mSet != null) && (this == mSet.First))
                        mSet.mFirst = value;
                }
            }

            private T mStart;
            public T Start
            {
                get { return mStart; }
                set
                {
                    mStart = value;
                    if (LessThan(End, mStart))
                        End = mStart;
                    if (mPrevious == null)
                        return;

                    //if (mPrevious.End > mStart)
                    if (MoreThanOrEqual(mPrevious.End, this.mStart))
                    {
                        Item prev = Previous; // po usunięciu this.Remove(), this.mPrevious = null;
                        Remove(); // Usuń ten element
                        prev.mEnd = this.mEnd;
                        // przez property wrazie gdyby przypisanie End chciało przeskoczyć
                        // jeszcze jeden element dalej
                        if (LessThan(this.Start, prev.Start))
                            prev.Start = this.mStart;
                    }
                }
            }
            private T mEnd;
            public T End
            {
                get { return mEnd; }
                set
                {
                    mEnd = value;
                    if (MoreThan(Start, mEnd))
                        Start = mEnd;
                    if (mNext == null)
                        return;

                    //if (mNext.Start < mEnd)
                    if (LessThanOrEqual(mNext.Start, this.mEnd))
                    {
                        Item nxt = this.Next;
                        Remove();
                        nxt.mStart = this.mStart;

                        if (MoreThan(this.End, nxt.End))
                            nxt.End = this.End;
                    }
                }
            }

            public void Remove()
            {
                if (mPrevious != null)
                    mPrevious.mNext = this.mNext;
                if (mNext != null)
                    mNext.mPrevious = this.mPrevious;
                if ((mSet != null) && (this == mSet.First))
                    mSet.mFirst = this.mNext;

                this.mPrevious = null;
                this.mNext = null;
                this.mSet = null;
            }

            internal Item AddAfter(T start, T end)
            {
                Item newItem = new Item(this.mSet);
                this.Next = newItem; // Napierw przypisz do zbioru, żeby zadziałało sprawdzanie
                newItem.Start = start;
                newItem.End = end;
                return newItem;
            }


            public T Span
            {                    // return End-Start;
                get
                {
                    if (mSet == null)
                        return default(T);
                    return mSet.SubtractValues(mEnd, mStart);
                }
            }

            private IDictionary mTags = new Dictionary<object, object>();
            public IDictionary Tags
            {
                get { return mTags; }
            }


            public bool InRange(T value)
            {
                return Calc.IsBetween<T>(Start, End, value);
            }

            public override string ToString()
            {
                return Start.ToString() + " - " + End.ToString();
            }
        }
    }

    public class TimeSpanSet : LinearSet<TimeSpan>
    {
        protected override TimeSpan AddValues(TimeSpan v1, TimeSpan v2)
        {
            return v1 + v2;
        }

        protected override TimeSpan SubtractValues(TimeSpan v1, TimeSpan v2)
        {
            return v1 - v2;
        }

        protected override LinearSet<TimeSpan> CreateNew()
        {
            return new TimeSpanSet();
        }
    }

    public class DateTimeSet : LinearSet<DateTime>
    {
        protected override DateTime AddValues(DateTime v1, DateTime v2)
        {
            return new DateTime(v1.Ticks + v2.Ticks);
        }

        protected override DateTime SubtractValues(DateTime v1, DateTime v2)
        {
            return new DateTime(v1.Ticks - v2.Ticks);
        }

        protected override LinearSet<DateTime> CreateNew()
        {
            return new DateTimeSet();
        }

        public bool IsYearInSet(int Year)
        {
            Item i = First;
            while (i != null)
            {
                if (Calc.IsBetween<int>(i.Start.Year, i.End.Year, Year))
                    return true;
                i = i.Next;
            }
            return false;

        }
    }

    public class TimeSet : LinearSet<Time>
    {
        public TimeSet()
        {
        }

        public TimeSet(Time start, Time end)
            : base(start, end)
        {
        }

        public TimeSet(LinearSet<Time> src)
            : this()
        {
            this.Add(src);
        }

        protected override Time AddValues(Time v1, Time v2)
        {
            return v1 + v2;
        }

        protected override Time SubtractValues(Time v1, Time v2)
        {
            return v1 - v2;
        }

        protected override LinearSet<Time> CreateNew()
        {
            return new TimeSet();
        }
    }

    public class DoubleSet : LinearSet<double>
    {
        protected override double AddValues(double v1, double v2)
        {
            return v1 + v2;
        }

        protected override double SubtractValues(double v1, double v2)
        {
            return v1 - v2;
        }

        protected override LinearSet<double> CreateNew()
        {
            return new DoubleSet();
        }

    }

}
