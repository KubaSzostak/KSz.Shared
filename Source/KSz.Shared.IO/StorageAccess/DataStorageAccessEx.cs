using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{
    public static class DataStorageAccessEx
    {
        private static bool OpenStreamStorage(Stream stream, StreamStorageAcces storage, string filePath)
        {
            if (stream == null)
                return false;

            using (stream)
            {
                storage.Init(stream, filePath);
                return true;
            }
        }
        
        /// <example>
        /// using (var storage = new TextLineReader())
        /// {
        ///     if (AppServices.FileDialog.OpenStorageReader(ref filePath, storage))
        ///     {
        ///         return storage.ReadAll();
        ///     }
        /// }
        /// </example>
        public static bool OpenStorageReader<T>(this DialogServices dlg, string filePath, T storage) where T : TextStorageReader
        {
            var stream = dlg.OpenFileDialog(ref filePath, storage.StorageInfo.Format, storage.StorageInfo.Extension);
            return OpenStreamStorage(stream, storage, filePath);
        }

        public static bool OpenStorageWriter<T>(this DialogServices dlg, string filePath, T storage) where T : TextStorageWriter
        {
            var stream = dlg.SaveFileDialog(ref filePath, storage.StorageInfo.Format, storage.StorageInfo.Extension);
            return OpenStreamStorage(stream, storage, filePath);
        }
    }
}
