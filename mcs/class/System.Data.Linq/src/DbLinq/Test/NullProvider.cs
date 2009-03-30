#region MIT license
// 
// MIT license
//
// Copyright (c) 2009 Novell, Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;

namespace DbLinq.Null {

	[System.ComponentModel.DesignerCategory("Code")]
	class NullConnection : DbConnection
	{
		public NullConnection ()
		{
            ConnectionString = "";
		}

		public override string ConnectionString {get; set;}
		public override string Database {get {return "NullDatabase";}}
		public override string DataSource {get {return "NullDataSource";}}
		public override string ServerVersion {get {return "0.0";}}
		public override ConnectionState State {get {return ConnectionState.Closed;}}

		public override void ChangeDatabase (string databaseName)
		{
			throw new NotSupportedException ();
		}

		public override void Close ()
		{
		}

		public override void Open ()
		{
		}

		protected override DbTransaction BeginDbTransaction (IsolationLevel level)
		{
			throw new NotSupportedException ();
		}

		protected override DbCommand CreateDbCommand ()
		{
			return new NullCommand ();
		}
	}

	class NullParameter : DbParameter
	{
		public override DbType DbType {get; set;}
		public override ParameterDirection Direction {get; set;}
		public override bool IsNullable {get; set;}
		public override string ParameterName {get; set;}
		public override int Size {get; set;}
		public override string SourceColumn {get; set;}
		public override bool SourceColumnNullMapping {get; set;}
		public override DataRowVersion SourceVersion {get; set;}
		public override object Value {get; set;}

		public override void ResetDbType ()
		{
			throw new NotSupportedException ();
		}
	}

	class DbParameterCollection<TParameter> : DbParameterCollection
		where TParameter : DbParameter
	{
		List<TParameter> parameters = new List<TParameter> ();

		public DbParameterCollection ()
		{
		}

		public override int Count {get {return parameters.Count;}}
		public override bool IsFixedSize {get {return false;}}
		public override bool IsReadOnly {get {return false;}}
		public override bool IsSynchronized {get {return false;}}
		public override object SyncRoot {get {return parameters;}}

		public override int Add (object value)
		{
			if (!(value is TParameter))
				throw new ArgumentException ("wrong type", "value");
			parameters.Add ((TParameter) value);
			return parameters.Count-1;
		}

		public override void AddRange (Array values)
		{
			foreach (TParameter p in values)
				Add (p);
		}

		public override void Clear ()
		{
			parameters.Clear ();
		}

		public override bool Contains (object value)
		{
			return parameters.Contains ((TParameter) value);
		}

		public override bool Contains (string value)
		{
			return parameters.Any (p => p.ParameterName == value);
		}

		public override void CopyTo (Array array, int index)
		{
			((ICollection) parameters).CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return parameters.GetEnumerator ();
		}

		public override int IndexOf (object value)
		{
			return parameters.IndexOf ((TParameter) value);
		}

		public override int IndexOf (string value)
		{
			for (int i = 0; i < parameters.Count; ++i)
				if (parameters [i].ParameterName == value)
					return i;
			return -1;
		}

		public override void Insert (int index, object value)
		{
			parameters.Insert (index, (TParameter) value);
		}

		public override void Remove (object value)
		{
			parameters.Remove ((TParameter) value);
		}

		public override void RemoveAt (int index)
		{
			parameters.RemoveAt (index);
		}

		public override void RemoveAt (string value)
		{
			int idx = IndexOf (value);
			if (idx >= 0)
				parameters.RemoveAt (idx);
		}

		protected override DbParameter GetParameter (int index)
		{
			return parameters [index];
		}

		protected override DbParameter GetParameter (string value)
		{
			return parameters.Where (p => p.ParameterName == value)
				.FirstOrDefault ();
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			parameters [index] = (TParameter) value;
		}

		protected override void SetParameter (string index, DbParameter value)
		{
			parameters [IndexOf (value)] = (TParameter) value;
		}
	}

	class NullCommand : DbCommand
	{
		DbParameterCollection<NullParameter> parameters = new DbParameterCollection<NullParameter> ();

		public NullCommand ()
		{
		}

		public override string CommandText { get; set; }
		public override int CommandTimeout { get; set; }
		public override CommandType CommandType { get; set; }
		public override bool DesignTimeVisible { get; set; }
		public override UpdateRowSource UpdatedRowSource { get; set; }

		protected override DbConnection DbConnection { get; set; }
		protected override DbParameterCollection DbParameterCollection {get {return parameters;}}
		protected override DbTransaction DbTransaction { get; set; }

		public override void Cancel ()
		{
		}

		public override int ExecuteNonQuery ()
		{
			throw new NotSupportedException ();
		}

		public override object ExecuteScalar ()
		{
			throw new NotSupportedException ();
		}

		public override void Prepare ()
		{
		}

		protected override DbParameter CreateDbParameter ()
		{
			return new NullParameter ();
		}

		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			throw new NotSupportedException ();
		}
	}

    class NullDataReader : DbDataReader
    {
        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public override int FieldCount
        {
            get { throw new NotImplementedException(); }
        }

        public override bool GetBoolean(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override byte GetByte(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override double GetDouble(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override Type GetFieldType(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override float GetFloat(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override short GetInt16(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetInt32(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override long GetInt64(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override string GetName(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public override string GetString(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsClosed
        {
            get { throw new NotImplementedException(); }
        }

        public override bool IsDBNull(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override bool NextResult()
        {
            throw new NotImplementedException();
        }

        public override bool Read()
        {
            throw new NotImplementedException();
        }

        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        public override object this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public override object this[int ordinal]
        {
            get { throw new NotImplementedException(); }
        }
    }
}

