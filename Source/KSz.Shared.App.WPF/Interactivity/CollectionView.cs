using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System
{

    public class CollectionView<T> : ObservableCollection<T>
    {
        public Comparison<T> Compare = null;

        public CollectionView()
        { }

        public CollectionView(IEnumerable<T> source) : base(source)
        { }

        private bool mSuppressCollectionChangedEvents = false;
        protected bool SuppressCollectionChangedEvents
        {
            get { return mSuppressCollectionChangedEvents; }
            set
            {
                if (mSuppressCollectionChangedEvents == value)
                    return;
                mSuppressCollectionChangedEvents = value;

                if (!mSuppressCollectionChangedEvents)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!SuppressCollectionChangedEvents)
                base.OnCollectionChanged(e);
        }

        void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateItemPosition((T)sender);
        }

        protected override void InsertItem(int index, T item)
        {
            index = GetSortedIndex(index, item);
            base.InsertItem(index, item);

            var npc = item as INotifyPropertyChanged;
            if (npc != null)
                npc.PropertyChanged += Item_PropertyChanged;
        }

        protected override void RemoveItem(int index)
        {
            var npc = this[index] as INotifyPropertyChanged;
            if (npc != null)
                npc.PropertyChanged -= Item_PropertyChanged;

            base.RemoveItem(index);
        }

        protected int GetSortedIndex(int index, T item)
        {
            if (Compare == null)
                return index;

            for (int i = 0; i < this.Count; i++)
            {
                var compRes = Compare(item, this[i]);
                if (compRes <= 0)
                    return i;
            }

            // Nothign greather than this item found - add this item at the end:
            return Count;
        }

        public CollectionView<T> Sort<TKey>(Func<T, TKey> keySelector) where TKey : IComparable<TKey> //BuubleSort
        {
            this.Compare = (x, y) =>
            {
                return keySelector(x).CompareTo(keySelector(y));
            };
            return Sort();
        }

        public CollectionView<T> Sort()
        {
            //http://www.sorting-algorithms.com/bubble-sort

            if (Compare == null)
            {
                if (typeof(T) is IComparable<T>)
                    Compare = Comparer<T>.Default.Compare;
                else
                    throw new InvalidCastException("Cannot sort collection of '" + typeof(T).Name
                        + "'. Implement IComparable<" + typeof(T).Name + "> or use " + this.GetType().Name + ".Sort(keySelector) instead.");
            }

            try
            {
                SuppressCollectionChangedEvents = true;
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    for (int j = 1; j <= i; j++)
                    {
                        T o1 = this[j - 1];
                        T o2 = this[j];

                        //var k1 = keySelector(o1);
                        //var k2 = keySelector(o2);
                        //if ((k1).CompareTo(k2) > 0)
                        if (this.Compare(o1, o2) > 0)
                        {
                            this.Remove(o1);
                            this.Insert(j, o1);
                        }
                    }
                }
            }
            finally
            {
                SuppressCollectionChangedEvents = false;
            }

            return this;
        }

        public void AddItems(IEnumerable<T> items)
        {
            try
            {
                SuppressCollectionChangedEvents = true;
                foreach (var item in items)
                {
                    this.Add(item);
                }
            }
            finally
            {
                SuppressCollectionChangedEvents = false;
            }
        }

        public void SetItems(IEnumerable<T> items)
        {
            try
            {
                SuppressCollectionChangedEvents = true;
                this.Clear();
                foreach (var item in items)
                {
                    this.Add(item);
                }
            }
            finally
            {
                SuppressCollectionChangedEvents = false;
            }
        }

        public void RemoveItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                var i = this.IndexOf(item);
                if (i >= 0)
                    this.RemoveAt(i);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            try
            {
                SuppressCollectionChangedEvents = true;
                var count = other.Count();
                if ((other == null) || count < 1)
                {
                    this.Clear();
                    return;
                }
                var otherSet = new HashSet<T>(other);
                for (int i = this.Count - 1; i >= 0; i--)
                {
                    if (!otherSet.Contains(this[i]))
                        this.RemoveAt(i);
                }
            }
            finally
            {
                SuppressCollectionChangedEvents = false;
            }
        }

        void UpdateItemPosition(T item)
        {
            if ((Compare == null) || (item == null))
                return;

            var itemIndex = this.IndexOf(item);
            if (itemIndex < 0)
                return;

            // First check if there is need for changing position

            int prevComp = -1;
            if (itemIndex > 0)
            {
                var prevItem = this[itemIndex - 1];
                prevComp = Compare(prevItem, item);
            }

            int nextComp = 1;
            if (itemIndex < this.Count - 1)
            {
                var nextItem = this[itemIndex + 1];
                nextComp = Compare(nextItem, item);
            }

            if ((prevComp <= 0) && (nextComp >= 0))
                return; // record in right position, nothing to do

            var newIndex = GetSortedIndex(itemIndex, item);
            this.Move(itemIndex, newIndex);
        }
    }
}
