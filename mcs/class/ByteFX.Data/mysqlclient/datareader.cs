// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Collections;

namespace ByteFX.Data.MySQLClient
{
	public sealed class MySQLDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		// The DataReader should always be open when returned to the user.
		private bool m_fOpen = true;

		// Keep track of the results and position
		// within the resultset (starts prior to first record).
		private MySQLField[]	_fields;
		private ArrayList		m_Rows;
		private	bool			m_IsSequential;

		/* 
		 * Keep track of the connection in order to implement the
		 * CommandBehavior.CloseConnection flag. A null reference means
		 * normal behavior (do not automatically close).
		 */
		private MySQLConnection _connection = null;

		/*
		 * Because the user should not be able to directly create a 
		 * DataReader object, the constructors are
		 * marked as internal.
		 */
		internal MySQLDataReader( MySQLConnection conn, bool Sequential)
		{
			_connection = conn;
			m_IsSequential = Sequential;

			m_Rows = new ArrayList();

		}

		/****
		 * METHODS / PROPERTIES FROM IDataReader.
		 ****/
		public int Depth 
		{
			/*
			 * Always return a value of zero if nesting is not supported.
			 */
			get { return 0;  }
		}

		public bool IsClosed
		{
			/*
			 * Keep track of the reader state - some methods should be
			 * disallowed if the reader is closed.
			 */
			get  { return !m_fOpen; }
		}

		public void Dispose() 
		{
		}

		public int RecordsAffected 
		{
			/*
			 * RecordsAffected is only applicable to batch statements
			 * that include inserts/updates/deletes. The sample always
			 * returns -1.
			 */
			get { return -1; }
		}

		internal void LoadResults() 
		{
			Driver d = _connection.Driver;

			// When executing query statements, the result byte that is returned
			// from MySQL is the column count.  That is why we reference the LastResult
			// property here to dimension our field array
			_fields = new MySQLField[d.LastResult];
			for (int x=0; x < _fields.Length; x++) 
			{
				_fields[x] = new MySQLField();
			}

			_connection.m_State = ConnectionState.Fetching;

			// Load in the column defs
			for (int i=0; i < _fields.Length; i++) 
			{
				_fields[i].ReadSchemaInfo( d );
			}

			// read the end of schema packet
			d.ReadPacket();
			if (! d.IsLastPacketSignal())
				throw new MySQLException("Expected end of column data.  Unknown transmission status");

			_connection.m_State = ConnectionState.Open;
		}

		public void Close()
		{
			m_fOpen = false;
			m_Rows.Clear();
		}

		public bool NextResult()
		{
			// The sample only returns a single resultset. However,
			// DbDataAdapter expects NextResult to return a value.
			return false;
		}

		public bool Read()
		{
			_connection.m_State = ConnectionState.Fetching;
			Driver d = _connection.Driver;

			try 
			{
				d.ReadPacket();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("MySQL.Net error: " + e.Message);
				_connection.m_State = ConnectionState.Open;
				return false;
			}

			if (d.IsLastPacketSignal()) 
			{
				_connection.m_State = ConnectionState.Open;
				return false;
			}

			for (int col=0; col < _fields.Length; col++)
			{
				byte [] data = d.ReadColumnData();
				_fields[col].SetValueData( data );
			}

			_connection.m_State = ConnectionState.Open;
			return true;			
		}

		public DataTable GetSchemaTable()
		{
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if (_fields.Length == 0) return null;

			DataTable dataTableSchema = new DataTable ("SchemaTable");
			
			dataTableSchema.Columns.Add ("ColumnName", typeof (string));
			dataTableSchema.Columns.Add ("ColumnOrdinal", typeof (int));
			dataTableSchema.Columns.Add ("ColumnSize", typeof (int));
			dataTableSchema.Columns.Add ("NumericPrecision", typeof (int));
			dataTableSchema.Columns.Add ("NumericScale", typeof (int));
			dataTableSchema.Columns.Add ("IsUnique", typeof (bool));
			dataTableSchema.Columns.Add ("IsKey", typeof (bool));
			DataColumn dc = dataTableSchema.Columns["IsKey"];
			dc.AllowDBNull = true; // IsKey can have a DBNull
			dataTableSchema.Columns.Add ("BaseCatalogName", typeof (string));
			dataTableSchema.Columns.Add ("BaseColumnName", typeof (string));
			dataTableSchema.Columns.Add ("BaseSchemaName", typeof (string));
			dataTableSchema.Columns.Add ("BaseTableName", typeof (string));
			dataTableSchema.Columns.Add ("DataType", typeof(Type));
			dataTableSchema.Columns.Add ("AllowDBNull", typeof (bool));
			dataTableSchema.Columns.Add ("ProviderType", typeof (int));
			dataTableSchema.Columns.Add ("IsAliased", typeof (bool));
			dataTableSchema.Columns.Add ("IsExpression", typeof (bool));
			dataTableSchema.Columns.Add ("IsIdentity", typeof (bool));
			dataTableSchema.Columns.Add ("IsAutoIncrement", typeof (bool));
			dataTableSchema.Columns.Add ("IsRowVersion", typeof (bool));
			dataTableSchema.Columns.Add ("IsHidden", typeof (bool));
			dataTableSchema.Columns.Add ("IsLong", typeof (bool));
			dataTableSchema.Columns.Add ("IsReadOnly", typeof (bool));

			int ord = 1;
			foreach (MySQLField f in _fields)
			{
				DataRow r = dataTableSchema.NewRow();
				r["ColumnName"] = f.ColumnName;
				r["ColumnOrdinal"] = ord++;
				r["ColumnSize"] = f.ColumnLength;
				int prec = f.NumericPrecision();
				int pscale = f.NumericScale();
				if (prec != -1)
					r["NumericPrecision"] = (short)prec;
				if (pscale != -1)
					r["NumericScale"] = (short)pscale;
				r["DataType"] = f.GetFieldType();
				r["ProviderType"] = (int)f.GetDbType();
				r["IsLong"] = f.IsBlob() && f.ColumnLength > 255;
				r["AllowDBNull"] = f.AllowsNull();
				r["IsReadOnly"] = false;
				r["IsRowVersion"] = false;
				r["IsUnique"] = f.IsUnique();
				r["IsKey"] = f.IsPrimaryKey();
				r["IsAutoIncrement"] = f.IsAutoIncrement();
				r["BaseSchemaName"] = null;
				r["BaseCatalogName"] = null;
				r["BaseTableName"] = f.TableName;
				r["BaseColumnName"] = null;

				dataTableSchema.Rows.Add( r );
			}

			return dataTableSchema;
		}

		/****
		 * METHODS / PROPERTIES FROM IDataRecord.
		 ****/
		public int FieldCount
		{
			// Return the count of the number of columns, which in
			// this case is the size of the column metadata
			// array.
			get 
			{ 
				return _fields.Length;
			}
		}

		public String GetName(int i)
		{
			return _fields[i].ColumnName;
		}

		public String GetDataTypeName(int i)
		{
			if (!m_fOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			// return the name of the type used on the backend
			return _fields[i].GetFieldTypeName();
		}

		public Type GetFieldType(int i)
		{
			if (!m_fOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			return _fields[i].GetFieldType();
		}

		public Object GetValue(int i)
		{
			if (!m_fOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			return _fields[i].GetValue();
		}

		public int GetValues(object[] values)
		{
			for (int i=0; i < _fields.Length; i ++) 
			{
				values[i] = GetValue(i);
			}

			return 0;
		}

		public int GetOrdinal(string name)
		{
			if (! m_fOpen)
				throw new Exception("No current query in data reader");

			for (int i=0; i < _fields.Length; i ++) 
			{
				if (_fields[i].ColumnName.ToLower().Equals(name.ToLower()))
					return i;
			}

			// Throw an exception if the ordinal cannot be found.
			throw new IndexOutOfRangeException("Could not find specified column in results");
		}

		public object this [ int i ]
		{
			get 
			{
				return this.GetValue(i);
			}
		}

		public object this [ String name ]
		{
			// Look up the ordinal and return 
			// the value at that position.
			get { return this[GetOrdinal(name)]; }
		}

		public bool GetBoolean(int i)
		{
			return (bool)GetValue(i);
		}

		public byte GetByte(int i)
		{
			return (byte)GetValue(i);
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (i >= _fields.Length) 
				throw new IndexOutOfRangeException();

			byte[] bytes = (m_Rows[0] as ArrayList)[i] as byte[];

			if (buffer == null) 
				return bytes.Length;

			/// adjust the length so we don't run off the end
			if (bytes.Length < (fieldOffset+length)) 
			{
				length = (int)(bytes.Length - fieldOffset);
			}

			for (int x=0; x < length; x++)
			{
				buffer[bufferoffset+x] = bytes[fieldOffset+x];	
			}

			return length;
		}

		public char GetChar(int i)
		{
			return (char)GetValue(i);
		}

		public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
		{
			if (i >= _fields.Length) 
				throw new IndexOutOfRangeException();

			// retrieve the bytes of the column
			long bytesize = GetBytes(i, 0, null, 0, 0);
			byte[] bytes = new byte[bytesize];
			GetBytes(i, 0, bytes, 0, (int)bytesize);

			char[] chars = System.Text.Encoding.UTF8.GetChars(bytes, 0, (int)bytesize);

			if (buffer == null) 
				return chars.Length;

			/// adjust the length so we don't run off the end
			if (chars.Length < (fieldOffset+length)) 
			{
				length = (int)(chars.Length - fieldOffset);
			}

			for (int x=0; x < length; x++)
			{
				buffer[bufferoffset+x] = chars[fieldOffset+x];	
			}

			return length;
		}

		public Guid GetGuid(int i)
		{
			/*
			* Force the cast to return the type. InvalidCastException
			* should be thrown if the data is not already of the correct type.
			*/
			// The sample does not support this method.
			throw new NotSupportedException("GetGUID not supported.");
		}

		public Int16 GetInt16(int i)
		{
			return (Int16)GetValue(i);
		}

		public Int32 GetInt32(int i)
		{
			return (Int32)GetValue(i);
		}

		public Int64 GetInt64(int i)
		{
			return (Int64)GetValue(i);
		}

		public float GetFloat(int i)
		{
			return (float)GetValue(i);
		}

		public double GetDouble(int i)
		{
			return (double)GetValue(i);
		}

		public String GetString(int i)
		{
			return (string)GetValue(i);
		}

		public Decimal GetDecimal(int i)
		{
			return (Decimal)GetValue(i);
		}

		public DateTime GetDateTime(int i)
		{
			return (DateTime)GetValue(i);
		}

		public IDataReader GetData(int i)
		{
			/*
			* The sample code does not support this method. Normally,
			* this would be used to expose nested tables and
			* other hierarchical data.
			*/
			throw new NotSupportedException("GetData not supported.");
		}

		public bool IsDBNull(int i)
		{
			return DBNull.Value == GetValue(i);
		}

		/*
		* Implementation specific methods.
		*/
		private int _cultureAwareCompare(string strA, string strB)
		{
	//      return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
			return 0;
		}

		#region IEnumerator
		public IEnumerator	GetEnumerator()
		{
			return new System.Data.Common.DbEnumerator(this);
		}
		#endregion
  }
}
