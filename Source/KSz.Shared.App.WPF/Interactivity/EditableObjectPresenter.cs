using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace System
{


    public class EditableObjectPresenter<T> : Presenter where T : EditableObject
    {
        public T Item { get; protected set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand ShowValidationErrorsCommand { get; set; }

        public EditableObjectPresenter(T eObj)
        {
            SaveCommand = new DelegateCommand(SaveRecord, CanSaveRecord);
            CancelCommand = new DelegateCommand(CancelEdits);
            ShowValidationErrorsCommand = new DelegateCommand(ShowValidationErrors, CanShowValidationErrors);
            this.Item = eObj;
            this.Item.BeginEdit();
        }

        protected virtual void SaveRecord()
        {
            Item.EndEdit();
        }

        protected virtual void CancelEdits()
        {
            Item.CancelEdit();
        }

        protected virtual List<string> GetValidationErrors()
        {
            return new List<string>();
        }

        protected virtual bool CanSaveRecord()
        {
            return GetValidationErrors().Count < 1;
        }

        protected virtual bool CanShowValidationErrors()
        {
            return GetValidationErrors().Count > 0;
        }

        protected virtual void ShowValidationErrors()
        {
            AppUI.Dialog.ShowWarning(GetValidationErrors().Join("\r\n"));
        }
    }


    public abstract class ItemsPresenter<TItem> : Presenter
    {

        public ItemsPresenter()
        {
            ItemsChanged = false;
        }

        private ObservableCollection<TItem> mItems = new ObservableCollection<TItem>();
        public ObservableCollection<TItem> Items
        {
            get
            {
                return mItems;
            }
            set
            {
                if (object.Equals(mItems, value))
                    return;

                mItems = value;
                OnPropertyChanged(() => Items);

                mGroupedItems.Source = mItems;
                OnPropertyChanged(() => GroupedItems);
            }
        }


        private CollectionViewSource mGroupedItems = new CollectionViewSource();
        public CollectionViewSource GroupedItems
        {
            get
            {
                return mGroupedItems;
                //GroupedItems.GroupDescriptions.Add(new PropertyGroupDescription("PowGmiText"));
            }
        }



        protected virtual TItem NewItem()
        {
            return Activator.CreateInstance<TItem>();
        }

        /// <summary>
        /// Creates presenter that implements SaveCommand and CancelCommand
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected abstract object ItemPresenter(TItem item);

        protected virtual void DeleteItem(TItem item)
        {
            SelectedItem = default(TItem);
            Items.Remove(item);
            ItemsChanged = true;
            OnSelectedItemChanged();
        }

        public event Action SelectedItemChanged;

        protected virtual void OnHasItemsChanged()
        {
            OnPropertyChanged(() => HasItems);
        }

        public ICommand AddItemCommand
        {
            get { return new DelegateCommand(AddItem); }
        }

        private void AddItem()
        {
            var newItem = NewItem();
            if (ShowItemDialog(newItem))
            {
                if (!Items.Contains(newItem))
                {
                    Items.Add(newItem);
                    ItemsChanged = true;
                }
                SelectedItem = newItem;
                OnHasItemsChanged();
            }
        }

        protected virtual void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged();
        }

        public ICommand DeleteItemCommand
        {
            get { return new DelegateCommand(DeleteItem, () => IsItemSelected); }
        }

        private void DeleteItem()
        {
            var item = SelectedItem;
            string msg = string.Format(SysUtils.Strings.DeleteQ);
            
            if (!AppUI.Dialog.ConfirmDeleteItem(item))
                return;

            DeleteItem(item);
            OnHasItemsChanged();
        }

        public ICommand EditItemCommand
        {
            get { return new DelegateCommand(EditItem, () => IsItemSelected); }
        }

        private void EditItem()
        {
            if (ShowItemDialog(SelectedItem))
                ItemsChanged = true;
        }

        protected abstract bool ShowItemDialog(TItem item);


        public bool ItemsChanged { get; protected set; }

        public ICommand SaveItemsCommand
        {
            get { return new DelegateCommand(SaveItemsCore, () => ItemsChanged); }
        }

        private void SaveItemsCore()
        {
            SaveItems();
            ItemsChanged = false;
        }

        public virtual void SaveItems()
        {
            throw new NotImplementedException();
        }

        public bool IsItemSelected
        {
            get
            {
                return SelectedItem != null;
            }
        }

        private TItem mSelectedItem;
        public TItem SelectedItem
        {
            get { return mSelectedItem; }
            set
            {
                mSelectedItem = value;
                OnPropertyChanged(() => SelectedItem);
                OnPropertyChanged(() => IsItemSelected);
                OnPropertyChanged(() => EditItemCommand);
                OnPropertyChanged(() => DeleteItemCommand);
                OnPropertyChanged(() => HasItems);
                OnSelectedItemChanged();
            }
        }

        public bool HasItems
        {
            get { return Items.Count > 0; }
        }
    }

}
