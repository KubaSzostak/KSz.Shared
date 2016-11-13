using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO
{


    public class TextStorageWriter : StreamStorage
    {
        public override void Init(Stream stream, string path)
        {
            base.Init(stream, path);
            Storage = new StreamWriter(stream, new UTF8Encoding(false));
        }

        // BinaryWriter
        protected TextWriter Storage { get; private set; }

        protected override void OnDisposing()
        {
            if (this.Storage != null)
            {
                this.Storage.Flush();
                this.Storage.Dispose();
                this.Storage = null;
            }
            base.OnDisposing();
        }

        public void Write(string s)
        {
            Storage.Write(s);
        }

        public void WriteLine()
        {
            Storage.WriteLine();
        }

        public void WriteLine(string ln)
        {
            Storage.WriteLine(ln);
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            foreach (var ln in lines)
            {
                Storage.WriteLine(ln);
            }
        }
    }
    

    public class TextStorageReaderBase : StreamStorage
    {
        public override void Init(Stream stream, string path)
        {
            base.Init(stream, path);
            Storage = new StreamReader(stream, new UTF8Encoding(false));
            // TODO: SysUtils.TrimBom() -> move BaseStream.Position
            Storage.BaseStream.Position = 0;

            FileName = System.IO.Path.GetFileName(Path);
            OnPropertyChanged(nameof(FileName));
        }

        public string FileName { get; private set; }
        

        // BinaryReader
        protected StreamReader Storage { get; private set; }

        protected override void OnDisposing()
        {
            if (this.Storage != null)
            {
                this.Storage.Dispose();
                this.Storage = null;
            }
            base.OnDisposing();
        }

        public bool EndOfStream
        {
            get { return Storage.EndOfStream; }
        }

        public long Position
        {
            get { return Storage.BaseStream.Position; }
        }

        public double Percent
        {
            get { return 100.0 * Convert.ToDouble(Position) / Convert.ToDouble(Size); }
        }
    }

    public class TextStorageReader : TextStorageReaderBase
    {

        public string ReadToEnd()
        {
            var res = Storage.ReadToEnd();
            return SysUtils.TrimBom(res);
        }

        public string ReadLine()
        {
            var res = Storage.ReadLine();
            return SysUtils.TrimBom(res);
        }

        public List<string> ReadAll()
        {
            var res = new List<string>();

            while (!Storage.EndOfStream)
            {
                var ln = Storage.ReadLine();
                if (ln != null)
                    res.Add(ln);
            }

            res[0] = SysUtils.TrimBom(res[0]);
            return res;
        }

    }


}
