using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System
{


    // There are many file dialogs, eg. OpenFileDialog exists in:
    //   WinForms   : Windows.Forms.OpenFileDialog
    //   WPF        : Microsoft.Win32.OpenFileDialog
    //   Silverlight: System.Windows.Controls.OpenFileDialog
    //   WinRT      : Windows.Storage.Pickers.FileOpenPicker (with quiet different filter approach)

    // And all of them are sealed... so you can't use interfaces

    // In all platforms you have access to Stream, but sometimes you need to know file path (eg. *.SHP)

    public interface IFileDialogService
    {
        string FilePath { get; set; }
        void AddStorage(IStreamStorage storage);
        void Reset();
        IStreamStorage ShowDialog();
    }

    public interface IStreamStorage
    {
        string Description { get; }
        string[] Extensions { get; }
        void Init(Stream stream, string filePath);
    }



    public abstract class FileDialogService : DisposableObject, IFileDialogService
    {
        public abstract string FilePath { get; set; }
        public abstract IStreamStorage ShowDialog();
        public abstract Stream OpenFile();

        protected IList<IStreamStorage> Storages { get; private set; } = new List<IStreamStorage>();

        public void Reset()
        {
            this.Storages.Clear();
        }

        public void AddStorage(IStreamStorage action)
        {
            Storages.Add(action);
        }
        
        protected IStreamStorage GetStorage(int index)
        {
            if ((index < 0) || (index >= this.Storages.Count))
                throw new IndexOutOfRangeException("Action with index=" + index.ToString() + " not found in " + this.GetType().Name );
            
            return this.Storages[index];
        }

        private void AddStarPrefixToExtension(string[] fileExtensions)
        {
            for (int i = 0; i < fileExtensions.Length; i++)
            {
                if (!fileExtensions[i].StartsWith("*"))
                {
                    
                    if (fileExtensions[i].StartsWith("."))
                        fileExtensions[i] = "*" + fileExtensions[i]; // .doc -> *.doc
                    else
                        fileExtensions[i] = "*." + fileExtensions[i]; // doc -> *.doc
                }
            }
        }

        protected void AddStarPrefixToExtensions()
        {
            foreach (var a in Storages)
            {
                AddStarPrefixToExtension(a.Extensions);
            }
        }

        protected string GetFilter()
        {
            var filters = new List<string>();
            foreach (var a in Storages)
            {
                var filter = a.Description + "|" + a.Extensions.Join(";"); // + "|" + SysUtils.Strings.AllFiles + "|*.*";
                filters.Add(filter);
            }
            // Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*
            return filters.Join("|");
        }


    }

    public static class FileDialogServiceEx
    {

        public static string OpenText(this IFileDialogService dlg, string fileDescription, params string[] fileExtensions)
        {
            var storage = new TextStorageReader();
            storage.Description = fileDescription;
            storage.Extensions = fileExtensions;

            dlg.Reset();
            dlg.AddStorage(storage);

            using (var res = dlg.ShowDialog() as TextStorageReader)
            {
                if (res == null)
                {
                    return null;
                }
                return res.ReadToEnd();
            }
        }

        public static bool SaveText(this IFileDialogService dlg, string text, string fileDescription, params string[] fileExtensions)
        {
            var storage = new TextStorageWriter();
            storage.Description = fileDescription;
            storage.Extensions = fileExtensions;

            dlg.Reset();
            dlg.AddStorage(storage);

            using (var res = dlg.ShowDialog() as TextStorageWriter)
            {
                if (res != null)
                {
                    res.Write(text);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public static IList<string> OpenLines(this IFileDialogService dlg, string fileDescription, params string[] fileExtensions)
        {
            var storage = new TextStorageReader();
            storage.Description = fileDescription;
            storage.Extensions = fileExtensions;

            dlg.Reset();
            dlg.AddStorage(storage);

            using (var res = dlg.ShowDialog() as TextStorageReader)
            {
                if (res == null)
                {
                    return null;
                }
                return res.ReadAll();
            }
        }

        public static bool SaveLines(this IFileDialogService dlg, IEnumerable<string> lines, string fileDescription, params string[] fileExtensions)
        {
            var storage = new TextStorageWriter();
            storage.Description = fileDescription;
            storage.Extensions = fileExtensions;

            dlg.Reset();
            dlg.AddStorage(storage);
            using (var res = dlg.ShowDialog() as TextStorageWriter)
            {
                if (res != null)
                {
                    res.WriteLines(lines);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
    
    
}
