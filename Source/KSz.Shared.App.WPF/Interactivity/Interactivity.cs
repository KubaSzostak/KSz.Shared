using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace System
{

    public class CollectionValue<T>
    {
        public CollectionValue(T value)
        {
            this.Value = value;
            this.DisplayText = value.ToString();
        }

        public CollectionValue(T value, string displayText)
        {
            this.Value = value;
            DisplayText = displayText;
        }

        public string DisplayText { get; set; }

        public T Value { get; private set; }
    }

    public class ObservableValuesCollection<T> : ObservableCollection<CollectionValue<T>>
    {
    }

    public class Model : ObservableObject //, IDatabaseAccess
    {
    }


    interface IPresenterView<T>
    {
        void AcceptAction();
        void CancelAction();

        void SetPresenter(Presenter p);
    }


    public class CollectionPresenter<T> : Presenter
    {
        public CollectionPresenter(IList<T> source)
        {
            mItems = source;
            AddItemCommand = new DelegateCommand(AddItemMethod)
            {
                Caption = SysUtils.Strings.Add
            };
            EditSelectedItemCommand = new DelegateCommand(EditSelectedItemMethod)
            {
                IsEnabled = false,
                Caption = SysUtils.Strings.Edit
            };
            DeleteSelectedItemCommand = new DelegateCommand(DeleteSelectedItemMethod)
            {
                IsEnabled = false,
                Caption = SysUtils.Strings.Delete
            };
        }

        public int Count
        {
            get
            {
                if (Items == null)
                    return -1;
                return Items.Count;
            }
        }

        private IList<T> mItems;
        public IList<T> Items
        {
            get { return mItems; }
            set
            {
                if (mItems == value)
                    return;

                mItems = value;
                OnPropertyChanged(() => Items);

                if ((mItems == null) || (!mItems.Contains(SelectedItem)))
                    SelectedItem = default(T);
            }
        }
        public ICommand LoadItemsCommand
        {
            get
            {
                return new DelegateCommand(() => {
                    var loadedItems = LoadItemsAction();
                    var loadedList = loadedItems as IList<T>;
                    if (loadedList == null)
                        loadedList = loadedItems.ToList();

                    this.Items = loadedList;
                });
            }
        }
        protected virtual IEnumerable<T> LoadItemsAction()
        {
            return LoadAllItemsAtion();
        }


        public ICommand LoadAllItemsCommand
        {
            get
            {
                return new DelegateCommand(() => {
                    this.Items = this.LoadAllItemsAtion();
                });
            }
        }
        protected virtual IList<T> LoadAllItemsAtion()
        {
            throw new NotImplementedException();
        }

        private T mSelectedItem;
        public T SelectedItem
        {
            get { return mSelectedItem; }
            set
            {
                if (object.Equals(mSelectedItem, value))
                    return;

                mSelectedItem = value;
                OnPropertyChanged(() => SelectedItem);

                HasSelectedItem = value != null;
                OnPropertyChanged(() => HasSelectedItem);

                HasItems = Count > 0;
                OnPropertyChanged(() => HasItems);

                EditSelectedItemCommand.IsEnabled = HasSelectedItem;
                DeleteSelectedItemCommand.IsEnabled = HasSelectedItem;
                OnSelectedItemChanged();
            }
        }

        public bool HasSelectedItem { get; private set; }
        public bool HasItems { get; private set; }


        protected virtual void OnSelectedItemChanged()
        {
        }

        protected void SelectIndex(int index)
        {
            if ((Items.Count < 1) || (index < 0))
            {
                SelectedItem = default(T);
                return;
            }

            if (index > Items.Count - 1)
                index = Items.Count - 1;
            SelectedItem = Items[index];
        }


        public DelegateCommand AddItemCommand { get; private set; }
        private void AddItemMethod()
        {
            AddItemAtion(AddItemCore);
        }
        protected virtual void AddItemAtion(Action<T> addItem)
        {
            throw new NotImplementedException();
        }
        protected virtual void AddItemCore(T item)
        {
            Items.Add(item);
            SelectedItem = item;
        }


        public DelegateCommand EditSelectedItemCommand { get; private set; }
        private void EditSelectedItemMethod()
        {
            if (SelectedItem != null)
                EditSelectedItemAction(SelectedItem);
        }
        protected virtual void EditSelectedItemAction(T item)
        {
            throw new NotImplementedException();
        }


        public DelegateCommand DeleteSelectedItemCommand { get; private set; }
        private void DeleteSelectedItemMethod()
        {
            if ((SelectedItem == null) || !AppUI.Dialog.ConfirmDeleteItem(SelectedItem))
                return;

            var delItem = SelectedItem;
            var delIndex = Items.IndexOf(delItem);

            DeleteItemCore(delItem);

            //SelectedItem = default(T);
            SelectIndex(delIndex);
        }

        protected virtual void DeleteItemCore(T item)
        {
            Items.Remove(item);
        }

    }




    public static class InteractivityEx
    {
        public static void WatchDeletions<T>(this CollectionView<T> view, ObservableCollection<T> source)
        {
            source.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        view.RemoveItems(e.OldItems.Cast<T>());
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        view.IntersectWith(source);
                        break;
                    default:
                        break;
                }
            };
        }

        public static void DoEvents(this Dispatcher d)
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        }

    }

}
