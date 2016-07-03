using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;

namespace System
{

    /// <summary>
    /// Można użyć w połączeniu z IEnumerable.ToView(). Gdy record zoststanie usunięty za pomocą
    /// metody DbRecord.Delete() zostaną również usunięte referencje ze wszystkich DbRecordsetView
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecordsView<T> : CollectionView<T> where T : Record
    {
        public RecordsView()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                this.Compare = Comparer<T>.Default.Compare;
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            item.Views.Add(this);
        }
    }

    public class RecordsPresenter<T> : CollectionPresenter<T> where T : Record
    {



        public RecordsPresenter(RecordSet<T> source)
            : base(new List<T>()) 
        {
            this.Recordset = source;
        }

        /// <summary>
        /// Recordset is used for interaction with RecordProvider/Database (Add/Save/Delete) 
        /// and can be used as source of filtered data for Items property
        /// </summary>
        public RecordSet<T> Recordset { get; private set; }

        protected override void AddItemCore(T item)
        {
            Recordset.Add(item);
            base.AddItemCore(item);
            // Implement 
        }

        protected override void DeleteItemCore(T item)
        {
            item.Delete();
            base.DeleteItemCore(item);
        }

        protected override void EditSelectedItemAction(T item)
        {
            base.EditSelectedItemAction(item);
            item.Save();
        }

        protected override IList<T> LoadAllItemsAtion()
        {
            return Recordset.ToView();
        }

        protected override IEnumerable<T> LoadItemsAction()
        {
            return base.LoadItemsAction();
        }

    }

    public static class ObservableExtensions
    {

        public static RecordsView<TRecord> ToView<TRecord>(this IEnumerable<TRecord> records) where TRecord : Record
        {
            var res = new RecordsView<TRecord>();
            res.AddItems(records);
            return res;
            
        }

        public static RecordsView<T> Filter<T>(this IEnumerable<T> items, string filter)
            where T : Record
        {
            return items.Where(r => r.FullSearchStringContains(filter)).ToView();
        }

    }
}
