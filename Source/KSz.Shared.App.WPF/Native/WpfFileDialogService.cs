using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System
{

    public abstract class WpfFileDialogService<T> : FileDialogService where T : FileDialog 
    {
        protected T Dialog;

        protected WpfFileDialogService(T dlg)
        {
            if (dlg == null)
                throw new ArgumentNullException(nameof(dlg));
            this.Dialog = dlg;
        }

        public override string FilePath
        {
            get { return this.Dialog.FileName; }
            set
            {
                this.Dialog.FileName = value; // IO.Path.GetFileNameWithoutExtension(value);
                this.Dialog.InitialDirectory = IO.Path.GetDirectoryName(value);
                // this.Dialog.DefaultExt = IO.Path.GetExtension(value);
                // this.Dialog.AddExtension = true;
            }
        }

        public override IStreamStorage ShowDialog()
        {
            this.AddStarPrefixToExtensions();
            this.Dialog.Filter = GetFilter();

            if (!this.Dialog.ShowDialog().GetValueOrDefault(false))
                return null;

            var stm = this.OpenFile();
            if (stm == null)
                return null;

            var storage = this.GetStorage(Dialog.FilterIndex - 1);
            storage.Init(stm, this.Dialog.FileName);
            return storage;
        }
        
    }



    public class WpfOpenFileDialogService : WpfFileDialogService<OpenFileDialog>
    {
        public WpfOpenFileDialogService() : base(new OpenFileDialog())
        {
            this.Dialog.Multiselect = false;
        }

        public override Stream OpenFile()
        {
            return this.Dialog.OpenFile();
        }
    }



    public class WpfSaveFileDialogService : WpfFileDialogService<SaveFileDialog>
    {
        public WpfSaveFileDialogService() : base(new SaveFileDialog())
        { }

        public override Stream OpenFile()
        {
            return this.Dialog.OpenFile();
        }
    }
}
