using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.Data
{
    
    public class DbRecordProvider<T> : RecordProvider<T> where T : Record 
    {     

        protected DbConnection DbConnection{ get; private set; }
        protected internal string DbTableName{ get; private set; }

        public DbRecordProvider(DbConnection conn, string tableName)
        {
            this.DbTableName = tableName;
            this.DbConnection = conn;
        }

        protected void ExecuteNonQuery(IList<DbCommand> commands)
        {
            // Dodatkowo została transakcja. Przy pojedyńczym ExecuteNonQuery
            // użytkownik wybiera czy chce użyć transakcji

            if (commands.Count < 1)
                return;
            
            DbConnection.OpenIfClosed();
            var trans = DbConnection.BeginTransaction();
            try
            {
                foreach (var cmd in commands)
                {
                    cmd.Connection = DbConnection;
                    cmd.Transaction = trans;
                    cmd.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
            finally
            {
                if (!KeepOpenConnection)
                    DbConnection.Close();
            }
        }

        public DbTransaction BeginTransaction()
        {
            DbConnection.OpenIfClosed();
            return DbConnection.BeginTransaction();
        }

        private bool KeepOpenConnection
        {
            get { return DbConnection.GetType().Name.Contains("SQLite"); }
        }

        

        protected override void LoadFromProvider(IEnumerable<Record> records)
        {
            try
            {
                DbConnection.OpenIfClosed();
                foreach (Record rec in records)
                {
                    var idValue = rec.GetRecordId();
                    var whereValue = new Dictionary<string, object>();
                    whereValue[RecordIdProperty.Name] = idValue;
                    var cmd = DbConnection.GetSelectCommand(DbTableName, RecordFieldNames, whereValue);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            foreach (var prop in this.RecordProperties)
                            {
                                this.DeserializePropertyValue(rec, prop, reader[prop.Name]);
                            }
                        }
                        else
                            throw new KeyNotFoundException("Record with key " + DbTableName + "." + RecordIdProperty.Name + "=" + idValue.ToText() + " not found.");
                        reader.Close();
                    }
                }
            }
            finally
            {
                if (!KeepOpenConnection)
                    DbConnection.Close();
            }
        }

        protected override void AddToProvider(IEnumerable<Record> records)
        {
            var cmdList = new List<DbCommand>();
            foreach (var record in records)
            {
                var recValues = this.GetRecordValues(record);
                var cmd = DbConnection.GetInsertCommand(DbTableName, recValues);
                cmdList.Add(cmd);
            }            
            ExecuteNonQuery(cmdList);
        }

        protected override void WriteToProvider(IEnumerable<Record> records)
        {
            var cmdList = new List<DbCommand>();
            foreach (var rec in records)
            {
                var idValue = rec.GetRecordId();
                var values = this.GetRecordValues(rec);
                values.Remove(RecordIdProperty.Name); // ArcGIS does not allow to update OBJECTID fields

                var cmd =  DbConnection.GetUpdateCommand(DbTableName, values, RecordIdProperty.Name, idValue);
                cmdList.Add(cmd);
            }
            ExecuteNonQuery(cmdList);
        }


        protected override void DeleteFromProvider(IEnumerable<Record> records)
        {
            var cmdList = new List<DbCommand>();
            foreach (var rec in records)
            {
                var idValue = rec.GetRecordId();
                var cmd = DbConnection.GetDeleteCommand(DbTableName, RecordIdProperty.Name, idValue);
                cmdList.Add(cmd);
            }
            ExecuteNonQuery(cmdList);
        }

        public override string ToString()
        {
            return this.DbTableName;
        }
    }


    public class DbRecordReader<T> : RecordReader<T> where T: Record
    {

        protected DbCommand mSelectCommand;   

        public DbRecordReader(DbConnection conn, string tableName, string whereClausule)
        {
            mSelectCommand = conn.GetSelectCommand(tableName, RecordFieldNames, whereClausule);
        }

        public DbRecordReader(DbConnection conn, string tableName)
            : this(conn, tableName, "")
        {
        }

        public DbRecordReader(DbConnection conn, string tableName, Dictionary<string, object> whereValues)
            : this(conn, tableName)
        {
            mSelectCommand = conn.GetSelectCommand(tableName, RecordFieldNames, whereValues);
        }

        public DbRecordReader(DbConnection conn, string tableName, string whereField, object whereValue)
            : this(conn, tableName)
        {
            var whereDict = new Dictionary<string, object>();
            whereDict[whereField] = whereValue;
            mSelectCommand = conn.GetSelectCommand(tableName, RecordFieldNames, whereDict);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return new DbEnumerator(mSelectCommand, this);
        }

        public class DbEnumerator : IEnumerator<T>
        {
            private DbCommand mSelectCommand;
            private DbDataReader mReader;
            private DbRecordReader<T> mHost;

            public DbEnumerator(DbCommand selCmd, DbRecordReader<T> host)
            {
                mHost = host;
                mSelectCommand = selCmd;
                mSelectCommand.Connection.OpenIfClosed();
                mReader = mSelectCommand.ExecuteReader();
            }

            public T Current { get; private set; }

            object Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                var res = mReader.Read();
                if (res)
                {
                    Current = Activator.CreateInstance<T>();
                    foreach (var prop in mHost.RecordProperties)
                    {
                        mHost.DeserializePropertyValue(Current, prop, mReader[prop.Name]);
                    }
                    Current.SetRecordSaved();
                    Current.OnRecordInitialized();
                }
                return res;
            }

            public void Reset()
            {
                Dispose();
                mReader = mSelectCommand.ExecuteReader();
            }

            public void Dispose()
            {
                mReader.Close();
                mReader.Dispose();
                mSelectCommand.Dispose();
            }
        }
    }

    public class DbRecordSet<T> : RecordSet<T> where T: Record
    {
        public DbRecordSet(DbConnection conn, string tableName) 
            : base(new DbRecordReader<T>(conn, tableName), new DbRecordProvider<T>(conn, tableName))
        {
        }
    }


    public static class DbExtensions
    {


        public static void OpenIfClosed(this DbConnection conn)
        {
            if (conn.State == Data.ConnectionState.Closed)
                conn.Open();
        }

        public static DbParameter AddParameter(this DbCommand cmd, object value)
        {
            object dbValue = value;
            if (value == null)
                dbValue = DBNull.Value;
            //else if (value is DateTime)
            //    dbValue = DateTimeToString((DateTime)value);

            //SQLite data types:
            //-----------------
            //NULL.     The value is a NULL value.
            //INTEGER.  The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
            //REAL.     The value is a floating point value, stored as an 8-byte IEEE floating point number.
            //TEXT.     The value is a text string, stored using the database encoding (UTF-8, UTF-16BE or UTF-16LE).
            //BLOB.     The value is a blob of data, stored exactly as it was input.


            var param = cmd.CreateParameter();
            param.Value = dbValue;
            cmd.Parameters.Add(param);
            return param;
        }

        public static DbCommand GetInsertCommand(this DbConnection conn, string tableName, Dictionary<string, object> values)
        {
            var cmd = conn.CreateCommand();
            var paramList = new List<string>();
            var paramNames = new List<string>();
            foreach (var kv in values)
            {
                cmd.AddParameter(kv.Value);
                paramList.Add("?");
                paramNames.Add("[" + kv.Key + "]");
            }

            //INSERT INTO StringData(DataId, DataValue) VALUE (?,?)
            cmd.CommandText = string.Format("INSERT INTO [{0}]({1}) VALUES ({2})", tableName, paramNames.Join(","), paramList.Join(","));
            return cmd;
        }

        private static DbCommand GetSelectCommandInternal(this DbConnection conn, string tableName, IEnumerable<string> fieldNames, Dictionary<string, object> whereValues, string whereClausule)
        {
            var fieldNamesWithQuotas = fieldNames.Select(fn => "[" + fn + "]").Join(",");
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format("SELECT {0} FROM [{1}] ", fieldNamesWithQuotas, tableName);
            if (whereValues!=null)
            {
                var whereClausules = new List<string>();
                foreach (var kv in whereValues)
                {
                    whereClausules.Add(string.Format(" ([{0}]=?) ", kv.Key));
                    cmd.AddParameter(kv.Value);                    
                }
                cmd.CommandText = cmd.CommandText + " WHERE " + whereClausules.Join(" AND ");
            }
            else if (!string.IsNullOrEmpty(whereClausule))
            {
                cmd.CommandText = cmd.CommandText + " WHERE " + whereClausule;
            }
            return cmd;
        }

        public static DbCommand GetSelectCommand(this DbConnection conn, string tableName, IEnumerable<string> fieldNames)
        {
            return GetSelectCommandInternal(conn, tableName, fieldNames, null, null);
        }

        public static DbCommand GetSelectCommand(this DbConnection conn, string tableName, IEnumerable<string> fieldNames, Dictionary<string, object> whereValues)
        {
            return GetSelectCommandInternal(conn, tableName, fieldNames, whereValues, null);
        }

        public static DbCommand GetSelectCommand(this DbConnection conn, string tableName, IEnumerable<string> fieldNames, string whereClausule)
        {
            return GetSelectCommandInternal(conn, tableName, fieldNames, null, whereClausule);
        }

        public static DbCommand GetUpdateCommand(this DbConnection conn, string tableName, Dictionary<string, object> values, string idFieldName, object idValue)
        {
            var cmd = conn.CreateCommand();
            var paramList = new List<string>();
            foreach (var kv in values)
            {
                cmd.AddParameter(kv.Value);
                paramList.Add("[" + kv.Key + "]=?");
            }

            cmd.AddParameter(idValue);

            //UPDATE table_name SET column1=value, column2=value2,... WHERE some_column=some_value
            cmd.CommandText = string.Format("UPDATE [{0}] SET {1} WHERE [{2}]=?", tableName, paramList.Join(","), idFieldName);
            return cmd;
        }

        public static DbCommand GetDeleteCommand(this DbConnection conn, string tableName, string idFieldName, object idValue)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format(@"DELETE FROM [{0}] WHERE [{1}] = ?", tableName, idFieldName);
            cmd.AddParameter(idValue);

            return cmd;
        }

        public static MemoryStream GetBinaryStream(this DbDataReader reader, string fieldName)
        {
            const int CHUNK_SIZE = 1024 * 1024;
            byte[] buffer = new byte[CHUNK_SIZE];
            long bytesRead;
            long fieldOffset = 0;
            var docField = reader.GetOrdinal(fieldName);

            MemoryStream stream = new MemoryStream();
            while ((bytesRead = reader.GetBytes(docField, fieldOffset, buffer, 0, buffer.Length)) > 0)
            {
                byte[] actualRead = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, actualRead, 0, (int)bytesRead);
                stream.Write(actualRead, 0, actualRead.Length);
                fieldOffset += bytesRead;
            }
            stream.Position = 0;
            return stream;
        }

        public static byte[] GetBinary(this DbDataReader reader, string docField)
        {
            using (MemoryStream stream = GetBinaryStream(reader, docField))
            {
                return stream.ToArray();
            }
        }


        public static void InsertBinary(this DbConnection connection, string table,  string idField, string binField, string docId, byte[] binData)
        {
            connection.OpenIfClosed();
            var values = new Dictionary<string, object>();
            values[binField] = binData;
            values[idField] = docId;
            var cmd = connection.GetInsertCommand(table, values);
            cmd.ExecuteNonQuery();
        }


    }

}
