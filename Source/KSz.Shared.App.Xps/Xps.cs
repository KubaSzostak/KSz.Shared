using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

namespace System.Windows.Documents
{
    public class XpsPaginator : IDocumentPaginatorSource, IDisposable
    {
        private Stream stream = null;
        public XpsDocument document;
        private Package package;
        private Uri packageUri = null;

        public XpsPaginator(byte[] xpsDocument, string xpsPackageUri)
        {
            var packageUri = new Uri(xpsPackageUri);
            package = PackageStore.GetPackage(packageUri);
            if (package == null)
            {
                stream = new MemoryStream(xpsDocument);
                package = Package.Open(stream, FileMode.Open, FileAccess.Read);
                PackageStore.AddPackage(packageUri, package);
            }
            document = new XpsDocument(package, CompressionOption.Fast, xpsPackageUri);
            Sequence = document.GetFixedDocumentSequence();
        }

        public DocumentPaginator DocumentPaginator
        {
            get
            {
                if (Sequence == null)
                    return null;
                else
                    return Sequence.DocumentPaginator;
            }
        }

        public FixedDocumentSequence Sequence { get; private set; }

        public void Close()
        {
            document.Close();
            package.Close();
            if (packageUri != null)
                PackageStore.RemovePackage(packageUri);
            if (stream != null)
                stream.Close();
        }

        public void Dispose()
        {
            Close();
            Dispose(document, package, stream);
        }

        private void Dispose(params IDisposable[] objects)
        {
            foreach (var obj in objects)
            {
                if (obj != null)
                    obj.Dispose();
            }
        }
    }

    public static class XpsUtils
    {
        public static XpsDocument GetXpsDocument(string xpsPackageUri)
        {
            var packageUri = new Uri(xpsPackageUri);
            var package = PackageStore.GetPackage(packageUri);
            if (package == null)
                return null;

            return new XpsDocument(package, CompressionOption.Fast, xpsPackageUri);
        }

        public static XpsDocument AddXpsDocument(string xpsPackageUri, byte[] xpsDocument)
        {
            var packageUri = new Uri(xpsPackageUri);
            Package package = null;

            //package = PackageStore.GetPackage(packageUri);
            //if (package == null)
            //{
                var stream = new MemoryStream(xpsDocument);
                package = Package.Open(stream, FileMode.Open, FileAccess.Read);                
                PackageStore.AddPackage(packageUri, package);
            //}
            return new XpsDocument(package, CompressionOption.Fast, xpsPackageUri);
        }


        public static List<string> GetTextList(this IDocumentPaginatorSource doc)
        {
            List<string> res = new List<string>();
            Dictionary<string, string> docPageText = new Dictionary<string, string>();
            for (int pageNum = 0; pageNum < doc.DocumentPaginator.PageCount; pageNum++)
            {
                DocumentPage docPage = doc.DocumentPaginator.GetPage(pageNum);
                foreach (System.Windows.UIElement uie in ((FixedPage)docPage.Visual).Children)
                {
                    var glyph = uie as System.Windows.Documents.Glyphs;
                    if (glyph != null)
                    {
                        res.Add(glyph.UnicodeString);
                    }
                }
            }

            return res;
        }

        public static List<string> GetTextWords(this IDocumentPaginatorSource docSeq, params string[] separator)
        {
            var list = docSeq.GetTextList();
            var res = new List<string>();

            foreach (var item in list)
            {
                var trimedItem = item.Trim();
                if (!string.IsNullOrEmpty(trimedItem))
                {
                    var itemWords = trimedItem.SplitValues(true, separator);
                    foreach (var iw in itemWords)
                    {
                        res.Add(iw.Trim());
                    }
                }
            }

            return res;
        }
    }
}
