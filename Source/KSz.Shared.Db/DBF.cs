using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Data;
using System.Diagnostics;
using System.Collections;

/*
 
 Based on  DotNetDBF packege.
 * https://github.com/ekonbenefits/dotnetdbf
 
 original author (javadbf): anil@linuxense.com 2004/03/31
 license: LGPL (http://www.gnu.org/copyleft/lesser.html)

 Support for choosing implemented character Sets as
 suggested by Nick Voznesensky <darkers@mail.ru>
 
 ported to C# (DotNetDBF): Jay Tuley <jay+dotnetdbf@tuley.name> 6/28/2007
 
 */

/*
    var fields = new List<DBFField>();
    fields.Add(new DBFField("IdPom", DBFNativeDbType.Char, 50));
    fields.Add(new DBFField("X", DBFNativeDbType.Float));
    fields.Add(new DBFField("Y", DBFNativeDbType.Float));
    fields.Add(new DBFField("Z", DBFNativeDbType.Float));
    fields.Add(new DBFField("Date", DBFNativeDbType.Date));

    var writer = new DBFWriter(DbfFileName);
    writer.Fields = fields.ToArray();
 */



namespace DBF
{
    abstract public class DBFBase
    {
        protected Encoding _charEncoding = Encoding.GetEncoding("utf-8");
        protected int _blockSize = 512;

        public Encoding CharEncoding
        {
            set { _charEncoding = value; }

            get { return _charEncoding; }
        }
        public int BlockSize
        {
            set { _blockSize = value; }

            get { return _blockSize; }
        }
    }


    public class DBFException : IOException
    {
        public DBFException()
            : base()
        {
        }

        public DBFException(String msg)
            : base(msg)
        {
        }

        public DBFException(String msg, Exception internalException)
            : base(msg, internalException)
        {
        }
    }


    public class DBFMemoValue
    {

        public const string MemoTerminator = "\x1A";
        private bool _loaded;
        private bool _new;



        public DBFMemoValue(string aValue)
        {
            _lockName = string.Format("DotNetDBF.Memo.new.{0}", Guid.NewGuid());
            Value = aValue;

        }


        internal DBFMemoValue(long block, DBFBase aBase, string fileLoc)
        {
            _block = block;
            _base = aBase;
            _fileLoc = fileLoc;
            _lockName = string.Format("DotNetDBF.Memo.read.{0}.{1}.", _fileLoc, _block);
        }

        private readonly DBFBase _base;
        private readonly string _lockName;
        private long _block;
        private readonly string _fileLoc;
        private string _value;

        internal long Block
        {
            get
            {
                return _block;
            }
        }

        internal void Write(DBFWriter aBase)
        {
            lock (_lockName)
            {
                if (!_new)
                    return;
                if (string.IsNullOrEmpty(aBase.DataMemoLoc))
                    throw new Exception("No Memo Location Set");

                var raf =
                    File.Open(aBase.DataMemoLoc,
                              FileMode.OpenOrCreate,
                              FileAccess.ReadWrite);

                /* before proceeding check whether the passed in File object
                    is an empty/non-existent file or not.
                    */


                using (var tWriter = new BinaryWriter(raf, aBase.CharEncoding))
                {
                    if (raf.Length == 0)
                    {
                        var tHeader = new DBTHeader();
                        tHeader.Write(tWriter);
                    }

                    var tValue = _value;
                    if ((tValue.Length + sizeof(int)) % aBase.BlockSize != 0)
                    {
                        tValue = tValue + MemoTerminator;
                    }

                    var tPosition = raf.Seek(0, SeekOrigin.End); //Got To End Of File
                    var tBlockDiff = tPosition % aBase.BlockSize;
                    if (tBlockDiff != 0)
                    {
                        tPosition = raf.Seek(aBase.BlockSize - tBlockDiff, SeekOrigin.Current);
                    }
                    _block = tPosition / aBase.BlockSize;
                    var tData = aBase.CharEncoding.GetBytes(tValue);
                    var tDataLength = tData.Length;
                    var tNewDiff = (tDataLength % aBase.BlockSize);
                    tWriter.Write(tData);
                    if (tNewDiff != 0)
                        tWriter.Seek(aBase.BlockSize - (tDataLength % aBase.BlockSize), SeekOrigin.Current);
                }
            }
        }


        public string Value
        {
            get
            {
                lock (_lockName)
                {

                    if (!_new && !_loaded)
                    {
                        using (var reader = new BinaryReader(
                            File.Open(_fileLoc,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read)))
                        {
                            reader.BaseStream.Seek(_block * _base.BlockSize, SeekOrigin.Begin);
                            string tString;
                            var tStringBuilder = new StringBuilder();
                            int tIndex;
                            var tSoftReturn = _base.CharEncoding.GetString(new byte[] { 0x8d, 0x0a });

                            do
                            {
                                var tData = reader.ReadBytes(_base.BlockSize);

                                tString = _base.CharEncoding.GetString(tData);
                                tIndex = tString.IndexOf(MemoTerminator);
                                if (tIndex != -1)
                                    tString = tString.Substring(0, tIndex);
                                tStringBuilder.Append(tString);
                            } while (tIndex == -1);
                            _value = tStringBuilder.ToString().Replace(tSoftReturn, String.Empty);
                        }
                        _loaded = true;
                    }

                    return _value;
                }
            }
            set
            {
                lock (_lockName)
                {
                    _new = true;


                    _value = value;
                }

            }
        }

        public override int GetHashCode()
        {
            return _lockName.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }
        public override bool Equals(object obj)
        {
            if (obj as DBFMemoValue == null)
                return false;
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Value.Equals(((DBFMemoValue)obj).Value);
        }
    }



    public enum DBFNativeDbType : byte
    {
        Autoincrement = (byte)0x2B, //+ in ASCII
        Timestamp = (byte)0x40, //@ in ASCII
        Binary = (byte)0x42, //B in ASCII
        Char = (byte)0x43, //C in ASCII
        Date = (byte)0x44, //D in ASCII
        Float = (byte)0x46, //F in ASCII
        Ole = (byte)0x47, //G in ASCII
        Long = (byte)0x49, //I in ASCII
        Logical = (byte)0x4C, //L in ASCII
        Memo = (byte)0x4D, //M in ASCII
        Numeric = (byte)0x4E, //N in ASCII
        Double = (byte)0x4F, //O in ASCII
    }

    static public class DBFFieldType
    {
        public const byte EndOfData = 0x1A; //^Z End of File
        public const byte EndOfField = 0x0D; //End of Field
        public const byte False = 0x46; //F in Ascci
        public const byte Space = 0x20; //Space in ascii
        public const byte True = 0x54; //T in ascii
        public const byte UnknownByte = 0x3F; //Unknown Bool value
        public const string Unknown = "?"; //Unknown value
        static public DbType FromNative(DBFNativeDbType aByte)
        {
            switch (aByte)
            {
                case DBFNativeDbType.Char:
                    return DbType.AnsiStringFixedLength;
                case DBFNativeDbType.Logical:
                    return DbType.Boolean;
                case DBFNativeDbType.Numeric:
                    return DbType.Decimal;
                case DBFNativeDbType.Date:
                    return DbType.Date;
                case DBFNativeDbType.Float:
                    return DbType.Decimal;
                case DBFNativeDbType.Memo:
                    return DbType.AnsiString;
                default:
                    throw new DBFException(
                        string.Format("Unsupported Native Type {0}", aByte));
            }
        }

        static public DBFNativeDbType FromDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiStringFixedLength:
                    return DBFNativeDbType.Char;
                case DbType.Boolean:
                    return DBFNativeDbType.Logical;
                case DbType.Decimal:
                    return DBFNativeDbType.Numeric;
                case DbType.Date:
                    return DBFNativeDbType.Date;
                case DbType.AnsiString:
                    return DBFNativeDbType.Memo;
                default:
                    throw new DBFException(
                        string.Format("Unsupported Type {0}", dbType));
            }
        }
    }


    public class DBTHeader
    {
        public const byte FieldTerminator = 0x1A;


        private int _nextBlock; /* 0-3*/
        private byte _version = 0x03;


        internal int NextBlock
        {
            get
            {
                return _nextBlock;
            }
            set
            {
                _nextBlock = value;
            }
        }

        internal byte Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }

        internal void Write(BinaryWriter dataOutput)
        {
            dataOutput.Write(_nextBlock);
            dataOutput.Write(new byte[12]);
            dataOutput.Write(_version);
            dataOutput.Write(new byte[495]);
        }
    }




    static public class DBFSigniture
    {
        public const byte NotSet = 0,
                          WithMemo = 0x80,
                          DBase3 = 0x03,
                          DBase3WithMemo = DBase3 | WithMemo;

    }

    [Flags]
    public enum DBFMemoFlags : byte
    {

    }


    public class DBFHeader
    {

        public const byte HeaderRecordTerminator = 0x0D;

        private byte _day; /* 3 */
        private byte _encryptionFlag; /* 15 */
        private DBFField[] _fieldArray; /* each 32 bytes */
        private int _freeRecordThread; /* 16-19 */
        private short _headerLength; /* 8-9 */
        private byte _incompleteTransaction; /* 14 */
        private byte _languageDriver; /* 29 */
        private byte _mdxFlag; /* 28 */
        private byte _month; /* 2 */
        private int _numberOfRecords; /* 4-7 */
        private short _recordLength; /* 10-11 */
        private short _reserv1; /* 12-13 */
        private int _reserv2; /* 20-23 */
        private int _reserv3; /* 24-27 */
        private short reserv4; /* 30-31 */
        private byte _signature; /* 0 */
        private byte _year; /* 1 */

        public DBFHeader()
        {
            _signature = DBFSigniture.DBase3;
        }

        internal byte Signature
        {
            get
            {
                return _signature;
            }
            set
            {
                _signature = value;
            }
        }

        internal short Size
        {
            get
            {
                return (short)(sizeof(byte) +
                                sizeof(byte) + sizeof(byte) + sizeof(byte) +
                                sizeof(int) +
                                sizeof(short) +
                                sizeof(short) +
                                sizeof(short) +
                                sizeof(byte) +
                                sizeof(byte) +
                                sizeof(int) +
                                sizeof(int) +
                                sizeof(int) +
                                sizeof(byte) +
                                sizeof(byte) +
                                sizeof(short) +
                                (DBFField.SIZE * _fieldArray.Length) +
                                sizeof(byte));
            }
        }

        internal short RecordSize
        {
            get
            {
                int tRecordLength = 0;
                for (int i = 0; i < _fieldArray.Length; i++)
                {
                    tRecordLength += _fieldArray[i].FieldLength;
                }

                return (short)(tRecordLength + 1);
            }
        }

        internal short HeaderLength
        {
            set { _headerLength = value; }

            get { return _headerLength; }
        }

        internal DBFField[] FieldArray
        {
            set { _fieldArray = value; }

            get { return _fieldArray; }
        }

        internal byte Year
        {
            set { _year = value; }

            get { return _year; }
        }

        internal byte Month
        {
            set { _month = value; }

            get { return _month; }
        }

        internal byte Day
        {
            set { _day = value; }

            get { return _day; }
        }

        internal int NumberOfRecords
        {
            set { _numberOfRecords = value; }

            get { return _numberOfRecords; }
        }

        internal short RecordLength
        {
            set { _recordLength = value; }

            get { return _recordLength; }
        }

        internal byte LanguageDriver
        {
            get { return _languageDriver; }
            set { _languageDriver = value; }
        }

        internal void Read(BinaryReader dataInput)
        {
            _signature = dataInput.ReadByte(); /* 0 */
            _year = dataInput.ReadByte(); /* 1 */
            _month = dataInput.ReadByte(); /* 2 */
            _day = dataInput.ReadByte(); /* 3 */
            _numberOfRecords = dataInput.ReadInt32(); /* 4-7 */

            _headerLength = dataInput.ReadInt16(); /* 8-9 */
            _recordLength = dataInput.ReadInt16(); /* 10-11 */

            _reserv1 = dataInput.ReadInt16(); /* 12-13 */
            _incompleteTransaction = dataInput.ReadByte(); /* 14 */
            _encryptionFlag = dataInput.ReadByte(); /* 15 */
            _freeRecordThread = dataInput.ReadInt32(); /* 16-19 */
            _reserv2 = dataInput.ReadInt32(); /* 20-23 */
            _reserv3 = dataInput.ReadInt32(); /* 24-27 */
            _mdxFlag = dataInput.ReadByte(); /* 28 */
            _languageDriver = dataInput.ReadByte(); /* 29 */
            reserv4 = dataInput.ReadInt16(); /* 30-31 */


            List<DBFField> v_fields = new List<DBFField>();

            DBFField field = DBFField.CreateField(dataInput); /* 32 each */
            while (field != null)
            {
                v_fields.Add(field);
                field = DBFField.CreateField(dataInput);
            }

            _fieldArray = v_fields.ToArray();
            //System.out.println( "Number of fields: " + _fieldArray.length);
        }

        internal void Write(BinaryWriter dataOutput)
        {
            dataOutput.Write(_signature); /* 0 */
            DateTime tNow = DateTime.Now;
            _year = (byte)(tNow.Year - 1900);
            _month = (byte)(tNow.Month);
            _day = (byte)(tNow.Day);

            dataOutput.Write(_year); /* 1 */
            dataOutput.Write(_month); /* 2 */
            dataOutput.Write(_day); /* 3 */

            //System.out.println( "Number of records in O/S: " + numberOfRecords);
            dataOutput.Write(_numberOfRecords); /* 4-7 */

            _headerLength = Size;
            dataOutput.Write(_headerLength); /* 8-9 */

            _recordLength = RecordSize;
            dataOutput.Write(_recordLength); /* 10-11 */

            dataOutput.Write(_reserv1); /* 12-13 */
            dataOutput.Write(_incompleteTransaction); /* 14 */
            dataOutput.Write(_encryptionFlag); /* 15 */
            dataOutput.Write(_freeRecordThread); /* 16-19 */
            dataOutput.Write(_reserv2); /* 20-23 */
            dataOutput.Write(_reserv3); /* 24-27 */

            dataOutput.Write(_mdxFlag); /* 28 */
            dataOutput.Write(_languageDriver); /* 29 */
            dataOutput.Write(reserv4); /* 30-31 */

            for (int i = 0; i < _fieldArray.Length; i++)
            {
                //System.out.println( "Length: " + _fieldArray[i].getFieldLength());
                _fieldArray[i].Write(dataOutput);
            }

            dataOutput.Write(HeaderRecordTerminator); /* n+1 */
        }
    }



    [DebuggerDisplay("Field:{Name}, Length:{FieldLength}")]
    public class DBFField
    {
        public const int SIZE = 32;
        public byte dataType; /* 11 */
        public byte decimalCount; /* 17 */
        public int fieldLength; /* 16 */
        public byte[] fieldName = new byte[11]; /* 0-10*/
        public byte indexFieldFlag; /* 31 */

        /* other class variables */
        public int nameNullIndex = 0;
        public int reserv1; /* 12-15 */
        public short reserv2; /* 18-19 */
        public short reserv3; /* 21-22 */
        public byte[] reserv4 = new byte[7]; /* 24-30 */
        public byte setFieldsFlag; /* 23 */
        public byte workAreaId; /* 20 */

        public DBFField()
        {
        }

        public DBFField(string aFieldName, DBFNativeDbType aType)
        {
            Name = aFieldName;
            DataType = aType;
        }

        public DBFField(string aFieldName,
                        DBFNativeDbType aType,
                        Int32 aFieldLength)
        {
            Name = aFieldName;
            DataType = aType;
            FieldLength = aFieldLength;
        }

        public DBFField(string aFieldName,
                        DBFNativeDbType aType,
                        Int32 aFieldLength,
                        Int32 aDecimalCount)
        {
            Name = aFieldName;
            DataType = aType;
            FieldLength = aFieldLength;
            DecimalCount = aDecimalCount;
        }

        public int Size
        {
            get { return SIZE; }
        }

        /**
         Returns the name of the field.
         
         @return Name of the field as String.
         */

        public String Name
        {
            get { return Encoding.ASCII.GetString(fieldName, 0, nameNullIndex); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentException("Field name cannot be null");
                }

                if (value.Length == 0
                    || value.Length > 10)
                {
                    throw new ArgumentException(
                        "Field name should be of length 0-10");
                }

                fieldName = Encoding.ASCII.GetBytes(value);
                nameNullIndex = fieldName.Length;
            }
        }

        /**
         Returns the data type of the field.
         
         @return Data type as byte.
         */

        public Type Type
        {
            get
            {
                return DBFUtils.TypeForNativeDBType(DataType);
            }
        }


        public DBFNativeDbType DataType
        {
            get
            {
                return (DBFNativeDbType)dataType;
            }
            set
            {
                switch (value)
                {
                    case DBFNativeDbType.Date:
                        fieldLength = 8; /* fall through */
                        goto default;
                    case DBFNativeDbType.Memo:
                        fieldLength = 10;
                        goto default;
                    case DBFNativeDbType.Logical:
                        fieldLength = 1;
                        goto default;
                    default:
                        dataType = (byte)value;
                        break;
                }
            }
        }

        /**
         Returns field length.
         
         @return field length as int.
         */

        public int FieldLength
        {
            get
            {
                if (DataType == DBFNativeDbType.Char)
                {
                    return fieldLength + (decimalCount * 256);
                }

                return fieldLength;
            }
            /**
             Length of the field.
             This method should be called before calling setDecimalCount().
             
             @param Length of the field as int.
             */
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException(
                        "Field length should be a positive number");
                }

                if (DataType == DBFNativeDbType.Date || DataType == DBFNativeDbType.Memo || DataType == DBFNativeDbType.Logical)
                {
                    throw new NotSupportedException(
                        "Cannot set length on this type of field");
                }

                if (DataType == DBFNativeDbType.Char && value > 255)
                {
                    fieldLength = value % 256;
                    decimalCount = (byte)(value / 256);
                    return;
                }

                fieldLength = value;
            }
        }

        /**
         Returns the decimal part. This is applicable
         only if the field type if of numeric in nature.
         
         If the field is specified to hold integral values
         the value returned by this method will be zero.
         
         @return decimal field size as int.
         */

        public int DecimalCount
        {
            get { return decimalCount; }
            /**
             Sets the decimal place size of the field.
             Before calling this method the size of the field
             should be set by calling setFieldLength().
             
             @param Size of the decimal field.
             */
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(
                        "Decimal length should be a positive number");
                }

                if (value > fieldLength)
                {
                    throw new ArgumentException(
                        "Decimal length should be less than field length");
                }

                decimalCount = (byte)value;
            }
        }

        public bool Read(BinaryReader aReader)
        {
            byte t_byte = aReader.ReadByte(); /* 0 */
            if (t_byte == DBFFieldType.EndOfField)
            {
                //System.out.println( "End of header found");
                return false;
            }

            aReader.Read(fieldName, 1, 10); /* 1-10 */
            fieldName[0] = t_byte;

            for (int i = 0; i < fieldName.Length; i++)
            {
                if (fieldName[i]
                    == 0)
                {
                    nameNullIndex = i;
                    break;
                }
            }

            dataType = aReader.ReadByte(); /* 11 */
            reserv1 = aReader.ReadInt32(); /* 12-15 */
            fieldLength = aReader.ReadByte(); /* 16 */
            decimalCount = aReader.ReadByte(); /* 17 */
            reserv2 = aReader.ReadInt16(); /* 18-19 */
            workAreaId = aReader.ReadByte(); /* 20 */
            reserv3 = aReader.ReadInt16(); /* 21-22 */
            setFieldsFlag = aReader.ReadByte(); /* 23 */
            aReader.Read(reserv4, 0, 7); /* 24-30 */
            indexFieldFlag = aReader.ReadByte(); /* 31 */
            return true;
        }

        /**
         Writes the content of DBFField object into the stream as per
         DBF format specifications.
         
         @param os OutputStream
         @throws IOException if any stream related issues occur.
         */

        public void Write(BinaryWriter aWriter)
        {
            // Field Name
            aWriter.Write(fieldName); /* 0-10 */
            aWriter.Write(new byte[11 - fieldName.Length],
                          0,
                          11 - fieldName.Length);

            // data type
            aWriter.Write(dataType); /* 11 */
            aWriter.Write(reserv1); /* 12-15 */
            aWriter.Write((byte)fieldLength); /* 16 */
            aWriter.Write(decimalCount); /* 17 */
            aWriter.Write(reserv2); /* 18-19 */
            aWriter.Write(workAreaId); /* 20 */
            aWriter.Write(reserv3); /* 21-22 */
            aWriter.Write(setFieldsFlag); /* 23 */
            aWriter.Write(reserv4); /* 24-30*/
            aWriter.Write(indexFieldFlag); /* 31 */
        }

        /**
         Creates a DBFField object from the data read from the given DataInputStream.
         
         The data in the DataInputStream object is supposed to be organised correctly
         and the stream "pointer" is supposed to be positioned properly.
         
         @param in DataInputStream
         @return Returns the created DBFField object.
         @throws IOException If any stream reading problems occures.
         */

        static internal DBFField CreateField(BinaryReader aReader)
        {
            DBFField field = new DBFField();
            if (field.Read(aReader))
            {
                return field;
            }
            else
            {
                return null;
            }
        }
    }


    public class DBFReader : DBFBase, IDisposable
    {
        private BinaryReader _dataInputStream;
        private DBFHeader _header;
        private string _dataMemoLoc;

        private int[] _selectFields = new int[] { };
        private int[] _orderedSelectFields = new int[] { };
        /* Class specific variables */
        private bool isClosed = true;

        /**
		 Initializes a DBFReader object.
		 
		 When this constructor returns the object
		 will have completed reading the hader (meta date) and
		 header information can be quried there on. And it will
		 be ready to return the first row.
		 
		 @param InputStream where the data is read from.
		 */



        public void SetSelectFields(params string[] aParams)
        {
            _selectFields =
                aParams.Select(
                    it =>
                    Array.FindIndex(_header.FieldArray,
                                    jt => jt.Name.Equals(it, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            _orderedSelectFields = _selectFields.OrderBy(it => it).ToArray();
        }

        public DBFField[] GetSelectFields()
        {
            return _selectFields.Any()
                ? _selectFields.Select(it => _header.FieldArray[it]).ToArray()
                : _header.FieldArray;
        }

        public DBFReader(string anIn)
        {
            try
            {
                _dataInputStream = new BinaryReader(
                    File.Open(anIn,
                              FileMode.Open,
                              FileAccess.Read,
                              FileShare.Read)
                    );

                var dbtPath = Path.ChangeExtension(anIn, "dbt");
                if (File.Exists(dbtPath))
                {
                    _dataMemoLoc = dbtPath;
                }

                isClosed = false;
                _header = new DBFHeader();
                _header.Read(_dataInputStream);

                /* it might be required to leap to the start of records at times */
                int t_dataStartIndex = _header.HeaderLength
                                       - (32 + (32 * _header.FieldArray.Length))
                                       - 1;
                if (t_dataStartIndex > 0)
                {
                    _dataInputStream.ReadBytes((t_dataStartIndex));
                }
            }
            catch (IOException ex)
            {
                throw new DBFException("Failed To Read DBF", ex);
            }
        }

        public DBFReader(Stream anIn)
        {
            try
            {
                _dataInputStream = new BinaryReader(anIn);
                isClosed = false;
                _header = new DBFHeader();
                _header.Read(_dataInputStream);

                /* it might be required to leap to the start of records at times */
                int t_dataStartIndex = _header.HeaderLength
                                       - (32 + (32 * _header.FieldArray.Length))
                                       - 1;
                if (t_dataStartIndex > 0)
                {
                    _dataInputStream.ReadBytes((t_dataStartIndex));
                }
            }
            catch (IOException e)
            {
                throw new DBFException("Failed To Read DBF", e);
            }
        }

        /**
		 Returns the number of records in the DBF.
		 */

        public int RecordCount
        {
            get { return _header.NumberOfRecords; }
        }

        /**
		 Returns the asked Field. In case of an invalid index,
		 it returns a ArrayIndexOutofboundsException.
		 
		 @param index. Index of the field. Index of the first field is zero.
		 */

        public DBFField[] Fields
        {
            get { return _header.FieldArray; }
        }

        #region IDisposable Members

        /// <summary>Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Close();
        }

        #endregion


        public string DataMemoLoc
        {
            get
            {
                return _dataMemoLoc;
            }
            set
            {
                _dataMemoLoc = value;
            }
        }

        public override String ToString()
        {
            StringBuilder sb =
                new StringBuilder(_header.Year + "/" + _header.Month + "/"
                                  + _header.Day + "\n"
                                  + "Total records: " + _header.NumberOfRecords +
                                  "\nHeader length: " + _header.HeaderLength +
                                  "");

            for (int i = 0; i < _header.FieldArray.Length; i++)
            {
                sb.Append(_header.FieldArray[i].Name);
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public void Close()
        {
            _dataInputStream.Close();
            isClosed = true;
        }

        /**
		 Reads the returns the next row in the DBF stream.
		 @returns The next row as an Object array. Types of the elements
		 these arrays follow the convention mentioned in the class description.
		 */
        public Object[] NextRecord()
        {
            return NextRecord(_selectFields, _orderedSelectFields);
        }


        internal Object[] NextRecord(IEnumerable<int> selectIndexes, IList<int> sortedIndexes)
        {
            if (isClosed)
            {
                throw new DBFException("Source is not open");
            }
            IList<int> tOrderdSelectIndexes = sortedIndexes;

            var recordObjects = new Object[_header.FieldArray.Length];

            try
            {
                bool isDeleted = false;
                do
                {
                    if (isDeleted)
                    {
                        _dataInputStream.ReadBytes(_header.RecordLength - 1);
                    }

                    int t_byte = _dataInputStream.ReadByte();
                    if (t_byte == DBFFieldType.EndOfData)
                    {
                        return null;
                    }

                    isDeleted = (t_byte == '*');
                } while (isDeleted);

                int j = 0;
                int k = -1;
                for (int i = 0; i < _header.FieldArray.Length; i++)
                {

                    if (tOrderdSelectIndexes.Count == j && j != 0
                        || (tOrderdSelectIndexes.Count > j && tOrderdSelectIndexes[j] > i && tOrderdSelectIndexes[j] != k))
                    {
                        _dataInputStream.BaseStream.Seek(_header.FieldArray[i].FieldLength, SeekOrigin.Current);
                        continue;
                    }
                    if (tOrderdSelectIndexes.Count > j)
                        k = tOrderdSelectIndexes[j];
                    j++;


                    switch (_header.FieldArray[i].DataType)
                    {
                        case DBFNativeDbType.Char:

                            var b_array = new byte[
                                _header.FieldArray[i].FieldLength
                                ];
                            _dataInputStream.Read(b_array, 0, b_array.Length);

                            recordObjects[i] = CharEncoding.GetString(b_array).TrimEnd();
                            break;

                        case DBFNativeDbType.Date:

                            byte[] t_byte_year = new byte[4];
                            _dataInputStream.Read(t_byte_year,
                                                 0,
                                                 t_byte_year.Length);

                            byte[] t_byte_month = new byte[2];
                            _dataInputStream.Read(t_byte_month,
                                                 0,
                                                 t_byte_month.Length);

                            byte[] t_byte_day = new byte[2];
                            _dataInputStream.Read(t_byte_day,
                                                 0,
                                                 t_byte_day.Length);

                            try
                            {
                                var tYear = CharEncoding.GetString(t_byte_year);
                                var tMonth = CharEncoding.GetString(t_byte_month);
                                var tDay = CharEncoding.GetString(t_byte_day);

                                int tIntYear, tIntMonth, tIntDay;
                                if (Int32.TryParse(tYear, out tIntYear) &&
                                    Int32.TryParse(tMonth, out tIntMonth) &&
                                    Int32.TryParse(tDay, out tIntDay))
                                {
                                    recordObjects[i] = new DateTime(
                                        tIntYear,
                                        tIntMonth,
                                        tIntDay);
                                }
                                else
                                {
                                    recordObjects[i] = null;
                                }
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                /* this field may be empty or may have improper value set */
                                recordObjects[i] = null;
                            }

                            break;

                        case DBFNativeDbType.Float:

                            try
                            {
                                byte[] t_float = new byte[
                                    _header.FieldArray[i].FieldLength
                                    ];
                                _dataInputStream.Read(t_float, 0, t_float.Length);
                                String tParsed = CharEncoding.GetString(t_float);
                                var tLast = tParsed.Substring(tParsed.Length - 1);
                                if (tParsed.Length > 0
                                    && tLast != " "
                                    && tLast != DBFFieldType.Unknown)
                                {
                                    recordObjects[i] = Double.Parse(tParsed, NumberStyles.Float | NumberStyles.AllowLeadingWhite);
                                }
                                else
                                {
                                    recordObjects[i] = null;
                                }
                            }
                            catch (FormatException e)
                            {
                                throw new DBFException("Failed to parse Float",
                                                       e);
                            }

                            break;

                        case DBFNativeDbType.Numeric:

                            try
                            {
                                byte[] t_numeric = new byte[
                                    _header.FieldArray[i].FieldLength
                                    ];
                                _dataInputStream.Read(t_numeric,
                                                     0,
                                                     t_numeric.Length);
                                string tParsed =
                                    CharEncoding.GetString(t_numeric);
                                var tLast = tParsed.Substring(tParsed.Length - 1);
                                if (tParsed.Length > 0
                                    && tLast != " "
                                    && tLast != DBFFieldType.Unknown)
                                {
                                    recordObjects[i] = Decimal.Parse(tParsed, NumberStyles.Float | NumberStyles.AllowLeadingWhite);
                                }
                                else
                                {
                                    recordObjects[i] = null;
                                }
                            }
                            catch (FormatException e)
                            {
                                throw new DBFException(
                                    "Failed to parse Number", e);
                            }

                            break;

                        case DBFNativeDbType.Logical:

                            byte t_logical = _dataInputStream.ReadByte();
                            //todo find out whats really valid
                            if (t_logical == 'Y' || t_logical == 't'
                                || t_logical == 'T'
                                || t_logical == 't')
                            {
                                recordObjects[i] = true;
                            }
                            else if (t_logical == DBFFieldType.UnknownByte)
                            {
                                recordObjects[i] = DBNull.Value;
                            }
                            else
                            {
                                recordObjects[i] = false;
                            }
                            break;

                        case DBFNativeDbType.Memo:
                            if (string.IsNullOrEmpty(_dataMemoLoc))
                                throw new Exception("Memo Location Not Set");


                            var tRawMemoPointer = _dataInputStream.ReadBytes(_header.FieldArray[i].FieldLength);
                            var tMemoPoiner = CharEncoding.GetString(tRawMemoPointer);
                            if (string.IsNullOrEmpty(tMemoPoiner))
                            {
                                recordObjects[i] = DBNull.Value;
                                break;
                            }
                            long tBlock;
                            if (!long.TryParse(tMemoPoiner, out tBlock))
                            {  //Because Memo files can vary and are often the least importat data, 
                                //we will return null when it doesn't match our format.
                                recordObjects[i] = DBNull.Value;
                                break;
                            }


                            recordObjects[i] = new DBFMemoValue(tBlock, this, _dataMemoLoc);
                            break;
                        default:
                            _dataInputStream.ReadBytes(_header.FieldArray[i].FieldLength);
                            recordObjects[i] = DBNull.Value;
                            break;
                    }


                }
            }
            catch (EndOfStreamException)
            {
                return null;
            }
            catch (IOException e)
            {
                throw new DBFException("Problem Reading File", e);
            }

            return selectIndexes.Any() ? selectIndexes.Select(it => recordObjects[it]).ToArray() : recordObjects;
        }
    }




    public class DBFWriter : DBFBase, IDisposable
    {
        private DBFHeader header;
        private Stream raf;
        private int recordCount;
        private ArrayList v_records = new ArrayList();
        private string _dataMemoLoc;

        /// Creates an empty Object.
        public DBFWriter()
        {
            header = new DBFHeader();
        }

        /// Creates a DBFWriter which can append to records to an existing DBF file.
        /// @param dbfFile. The file passed in shouls be a valid DBF file.
        /// @exception Throws DBFException if the passed in file does exist but not a valid DBF file, or if an IO error occurs.
        public DBFWriter(String dbfFile)
        {
            try
            {
                raf =
                    File.Open(dbfFile,
                              FileMode.OpenOrCreate,
                              FileAccess.ReadWrite);

                _dataMemoLoc = Path.ChangeExtension(dbfFile, "dbt");

                /* before proceeding check whether the passed in File object
				 is an empty/non-existent file or not.
				 */
                if (raf.Length == 0)
                {
                    header = new DBFHeader();
                    return;
                }

                header = new DBFHeader();
                header.Read(new BinaryReader(raf));

                /* position file pointer at the end of the raf */
                raf.Seek(-1, SeekOrigin.End);
                /* to ignore the END_OF_DATA byte at EoF */
            }
            catch (FileNotFoundException e)
            {
                throw new DBFException("Specified file is not found. ", e);
            }
            catch (IOException e)
            {
                throw new DBFException(" while reading header", e);
            }
            recordCount = header.NumberOfRecords;
        }

        public DBFWriter(Stream dbfFile)
        {
            raf = dbfFile;

            /* before proceeding check whether the passed in File object
			 is an empty/non-existent file or not.
			 */
            if (raf.Length == 0)
            {
                header = new DBFHeader();
                return;
            }

            header = new DBFHeader();
            header.Read(new BinaryReader(raf));

            /* position file pointer at the end of the raf */
            raf.Seek(-1, SeekOrigin.End);
            /* to ignore the END_OF_DATA byte at EoF */


            recordCount = header.NumberOfRecords;
        }

        public byte Signature
        {
            get
            {
                return header.Signature;
            }
            set
            {
                header.Signature = value;
            }
        }

        public string DataMemoLoc
        {
            get { return _dataMemoLoc; }
            set { _dataMemoLoc = value; }
        }

        public byte LanguageDriver
        {
            set
            {
                if (header.LanguageDriver != 0x00)
                {
                    throw new DBFException("LanguageDriver has already been set");
                }

                header.LanguageDriver = value;
            }
        }


        public DBFField[] Fields
        {
            get
            {
                return header.FieldArray;
            }


            set
            {
                if (header.FieldArray != null)
                {
                    throw new DBFException("Fields has already been set");
                }

                if (value == null
                    || value.Length == 0)
                {
                    throw new DBFException("Should have at least one field");
                }

                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == null)
                    {
                        throw new DBFException("Field " + (i + 1) + " is null");
                    }
                }

                header.FieldArray = value;

                try
                {
                    if (raf != null
                        && raf.Length == 0)
                    {
                        /*
						 this is a new/non-existent file. So write header before proceeding
						 */
                        header.Write(new BinaryWriter(raf));
                    }
                }
                catch (IOException e)
                {
                    throw new DBFException("Error accesing file", e);
                }
            }
        }

        #region IDisposable Members

        /// <summary>Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Close();
        }

        #endregion

        /**
		 Add a record.
		 */

        public void WriteRecord(params Object[] values)
        {
            if (raf == null)
            {
                throw new DBFException(
                    "Not initialized with file for WriteRecord use, use AddRecord instead");
            }
            AddRecord(values, true);
        }

        public void AddRecord(params Object[] values)
        {
            if (raf != null)
            {
                throw new DBFException(
                    "Appending to a file, requires using Writerecord instead");
            }
            AddRecord(values, false);
        }

        private void AddRecord(Object[] values, bool writeImediately)
        {
            if (header.FieldArray == null)
            {
                throw new DBFException(
                    "Fields should be set before adding records");
            }

            if (values == null)
            {
                throw new DBFException("Null cannot be added as row");
            }

            if (values.Length
                != header.FieldArray.Length)
            {
                throw new DBFException(
                    "Invalid record. Invalid number of fields in row");
            }

            for (int i = 0; i < header.FieldArray.Length; i++)
            {
                if (values[i] == null)
                {
                    continue;
                }

                switch (header.FieldArray[i].DataType)
                {
                    case DBFNativeDbType.Char:
                        if (!(values[i] is String) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;

                    case DBFNativeDbType.Logical:
                        if (!(values[i] is Boolean) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;

                    case DBFNativeDbType.Numeric:
                        if (!(values[i] is IConvertible) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;

                    case DBFNativeDbType.Date:
                        if (!(values[i] is DateTime) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;

                    case DBFNativeDbType.Float:
                        if (!(values[i] is IConvertible) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;
                    case DBFNativeDbType.Memo:
                        if (!(values[i] is DBFMemoValue) && !(values[i] is DBNull))
                        {
                            throw new DBFException("Invalid value for field "
                                                   + i);
                        }
                        break;
                }
            }

            if (!writeImediately)
            {
                v_records.Add(values);
            }
            else
            {
                try
                {
                    WriteRecord(new BinaryWriter(raf), values);
                    recordCount++;
                }
                catch (IOException e)
                {
                    throw new DBFException(
                        "Error occured while writing record. ", e);
                }
            }
        }

        ///Writes the set data to the OutputStream.
        public void Write(Stream tOut)
        {
            try
            {
                BinaryWriter outStream = new BinaryWriter(tOut);

                header.NumberOfRecords = v_records.Count;
                header.Write(outStream);

                /* Now write all the records */
                int t_recCount = v_records.Count;
                for (int i = 0; i < t_recCount; i++)
                {
                    /* iterate through records */

                    Object[] t_values = (Object[])v_records[i];

                    WriteRecord(outStream, t_values);
                }

                outStream.Write(DBFFieldType.EndOfData);
                outStream.Flush();
            }
            catch (IOException e)
            {
                throw new DBFException("Error Writing", e);
            }
        }

        public void Close()
        {
            /* everything is written already. just update the header for record count and the END_OF_DATA mark */
            header.NumberOfRecords = recordCount;
            if (raf != null)
            {
                raf.Seek(0, SeekOrigin.Begin);
                header.Write(new BinaryWriter(raf));
                raf.Seek(0, SeekOrigin.End);
                raf.WriteByte(DBFFieldType.EndOfData);
                raf.Close();
            }
        }

        private void WriteRecord(BinaryWriter dataOutput, Object[] objectArray)
        {
            dataOutput.Write((byte)' ');
            for (int j = 0; j < header.FieldArray.Length; j++)
            {
                /* iterate throught fields */

                switch (header.FieldArray[j].DataType)
                {
                    case DBFNativeDbType.Char:
                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            String str_value = objectArray[j].ToString();
                            dataOutput.Write(
                                DBFUtils.textPadding(str_value,
                                                  CharEncoding,
                                                  header.FieldArray[j].
                                                      FieldLength
                                    )
                                );
                        }
                        else
                        {
                            dataOutput.Write(
                                DBFUtils.textPadding("",
                                                  CharEncoding,
                                                  header.FieldArray[j].
                                                      FieldLength
                                    )
                                );
                        }

                        break;

                    case DBFNativeDbType.Date:
                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            DateTime tDate = (DateTime)objectArray[j];

                            dataOutput.Write(
                                CharEncoding.GetBytes(tDate.ToString("yyyyMMdd")));
                        }
                        else
                        {
                            dataOutput.Write(
                                DBFUtils.FillArray(new byte[8], DBFFieldType.Space));
                        }

                        break;

                    case DBFNativeDbType.Float:

                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            Double tDouble = Convert.ToDouble(objectArray[j]);
                            dataOutput.Write(
                                DBFUtils.NumericFormating(
                                    tDouble,
                                    CharEncoding,
                                    header.FieldArray[j].FieldLength,
                                    header.FieldArray[j].DecimalCount
                                    )
                                );
                        }
                        else
                        {
                            dataOutput.Write(
                                DBFUtils.textPadding(
                                    DBFFieldType.Unknown,
                                    CharEncoding,
                                    header.FieldArray[j].FieldLength,
                                    DBFUtils.ALIGN_RIGHT
                                    )
                                );
                        }

                        break;

                    case DBFNativeDbType.Numeric:

                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            Decimal tDecimal = Convert.ToDecimal(objectArray[j]);
                            dataOutput.Write(
                                DBFUtils.NumericFormating(
                                    tDecimal,
                                    CharEncoding,
                                    header.FieldArray[j].FieldLength,
                                    header.FieldArray[j].DecimalCount
                                    )
                                );
                        }
                        else
                        {
                            dataOutput.Write(
                                DBFUtils.textPadding(
                                    DBFFieldType.Unknown,
                                    CharEncoding,
                                    header.FieldArray[j].FieldLength,
                                    DBFUtils.ALIGN_RIGHT
                                    )
                                );
                        }

                        break;
                    case DBFNativeDbType.Logical:

                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            if ((bool)objectArray[j])
                            {
                                dataOutput.Write(DBFFieldType.True);
                            }
                            else
                            {
                                dataOutput.Write(DBFFieldType.False);
                            }
                        }
                        else
                        {
                            dataOutput.Write(DBFFieldType.UnknownByte);
                        }

                        break;

                    case DBFNativeDbType.Memo:
                        if (objectArray[j] != null && objectArray[j] != DBNull.Value)
                        {
                            var tMemoValue = ((DBFMemoValue)objectArray[j]);

                            tMemoValue.Write(this);

                            dataOutput.Write(DBFUtils.NumericFormating(tMemoValue.Block, CharEncoding, 10, 0));
                        }
                        else
                        {
                            dataOutput.Write(
                            DBFUtils.textPadding("",
                                              CharEncoding,
                                              10
                                )
                            );
                        }


                        break;

                    default:
                        throw new DBFException("Unknown field type "
                                               + header.FieldArray[j].DataType);
                }
            } /* iterating through the fields */
        }
    }




    static public class DBFUtils
    {

        public const int ALIGN_LEFT = 10;
        public const int ALIGN_RIGHT = 12;

        static public byte[] FillArray(byte[] anArray, byte value)
        {
            for (int i = 0; i < anArray.Length; i++)
            {
                anArray[i] = value;
            }
            return anArray;
        }

        static public byte[] trimLeftSpaces(byte[] arr)
        {
            List<byte> tList = new List<byte>(arr.Length);

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != ' ')
                {
                    tList.Add(arr[i]);
                }
            }
            return tList.ToArray();
        }

        static public byte[] textPadding(String text,
                                         Encoding charEncoding,
                                         int length)
        {
            return textPadding(text, charEncoding, length, ALIGN_LEFT);
        }

        static public byte[] textPadding(String text,
                                         Encoding charEncoding,
                                         int length,
                                         int alignment)
        {
            return
                textPadding(text,
                            charEncoding,
                            length,
                            alignment,
                            DBFFieldType.Space);
        }

        static public byte[] textPadding(String text,
                                         Encoding charEncoding,
                                         int length,
                                         int alignment,
                                         byte paddingByte)
        {
            Encoding tEncoding = charEncoding;
            if (text.Length >= length)
            {
                return tEncoding.GetBytes(text.Substring(0, length));
            }

            byte[] byte_array = FillArray(new byte[length], paddingByte);

            switch (alignment)
            {
                case ALIGN_LEFT:
                    Array.Copy(tEncoding.GetBytes(text),
                               0,
                               byte_array,
                               0,
                               text.Length);
                    break;

                case ALIGN_RIGHT:
                    int t_offset = length - text.Length;
                    Array.Copy(tEncoding.GetBytes(text),
                               0,
                               byte_array,
                               t_offset,
                               text.Length);
                    break;
            }

            return byte_array;
        }

        static public byte[] NumericFormating(IFormattable doubleNum,
                                              Encoding charEncoding,
                                              int fieldLength,
                                              int sizeDecimalPart)
        {
            int sizeWholePart = fieldLength
                                -
                                (sizeDecimalPart > 0 ? (sizeDecimalPart + 1) : 0);

            StringBuilder format = new StringBuilder(fieldLength);

            for (int i = 0; i < sizeWholePart; i++)
            {

                format.Append(i + 1 == sizeWholePart ? "0" : "#");
            }

            if (sizeDecimalPart > 0)
            {
                format.Append(".");

                for (int i = 0; i < sizeDecimalPart; i++)
                {
                    format.Append("0");
                }
            }


            return
                textPadding(
                    doubleNum.ToString(format.ToString(),
                                       NumberFormatInfo.InvariantInfo),
                    charEncoding,
                    fieldLength,
                    ALIGN_RIGHT);
        }

        static public bool contains(byte[] arr, byte value)
        {
            return
                Array.Exists(arr,
                             delegate(byte anItem) { return anItem == value; });
        }


        static public Type TypeForNativeDBType(DBFNativeDbType aType)
        {
            switch (aType)
            {
                case DBFNativeDbType.Char:
                    return typeof(string);
                case DBFNativeDbType.Date:
                    return typeof(DateTime);
                case DBFNativeDbType.Numeric:
                    return typeof(decimal);
                case DBFNativeDbType.Logical:
                    return typeof(bool);
                case DBFNativeDbType.Float:
                    return typeof(float);
                case DBFNativeDbType.Memo:
                    return typeof(DBFMemoValue);
                default:
                    throw new ArgumentException("Unsupported Type");
            }
        }
    }
}
