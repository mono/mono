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

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Provides a means of reading a forward-only stream of rows from a MySQL database. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlDataReader.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	public sealed class MySqlDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		// The DataReader should always be open when returned to the user.
		private bool			isOpen = true;

		// Keep track of the results and position
		// within the resultset (starts prior to first record).
		private MySqlField[]	_fields;
		private CommandBehavior	commandBehavior;
		private MySqlCommand	command;
		private bool			canRead;
		private bool			hasRows;
		private CommandResult	currentResult;

		/* 
		 * Keep track of the connection in order to implement the
		 * CommandBehavior.CloseConnection flag. A null reference means
		 * normal behavior (do not automatically close).
		 */
		private MySqlConnection connection = null;

		/*
		 * Because the user should not be able to directly create a 
		 * DataReader object, the constructors are
		 * marked as internal.
		 */
		internal MySqlDataReader( MySqlCommand cmd, CommandBehavior behavior)
		{
			this.command = cmd;
			connection = (MySqlConnection)command.Connection;
			commandBehavior = behavior;
		}

		/// <summary>
		/// Gets a value indicating the depth of nesting for the current row.  This method is not 
		/// supported currently and always returns 0.
		/// </summary>
		public int Depth 
		{
			get { return 0;  }
		}

		/// <summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public bool IsClosed
		{
			get  { return ! isOpen; }
		}

		void IDisposable.Dispose() 
		{
			if (isOpen)
				Close();
		}

		/// <summary>
		/// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
		/// </summary>
		public int RecordsAffected 
		{
			// RecordsAffected returns the number of rows affected in batch
			// statments from insert/delete/update statments.  This property
			// is not completely accurate until .Close() has been called.
			get { return command.UpdateCount; }
		}

		/// <summary>
		/// Gets a value indicating whether the MySqlDataReader contains one or more rows.
		/// </summary>
		public bool HasRows
		{
			get { return hasRows; }
		}

		/// <summary>
		/// Closes the MySqlDataReader object.
		/// </summary>
		public void Close()
		{
			if (! isOpen) return;

			// finish any current command
			if (currentResult != null)
				currentResult.Clear();
			command.ExecuteBatch(false);

			connection.Reader = null;
			if (0 != (commandBehavior & CommandBehavior.CloseConnection))
				connection.Close();

			isOpen = false;
		}




		/// <summary>
		/// Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			// Return the count of the number of columns, which in
			// this case is the size of the column metadata
			// array.
			get 
			{ 
				if (_fields != null)
					return _fields.Length;
				return 0;
			}
		}

		/// <summary>
		/// Overloaded. Gets the value of a column in its native format.
		/// In C#, this property is the indexer for the MySqlDataReader class.
		/// </summary>
		public object this [ int i ]
		{
			get 
			{
				return this.GetValue(i);
			}
		}

		/// <summary>
		/// Gets the value of a column in its native format.
		///	[C#] In C#, this property is the indexer for the MySqlDataReader class.
		/// </summary>
		public object this [ String name ]
		{
			// Look up the ordinal and return 
			// the value at that position.
			get { return this[GetOrdinal(name)]; }
		}

		private MySqlField GetField(int i)
		{
			if (i >= _fields.Length) throw new IndexOutOfRangeException();
			return _fields[i];
		}

		#region TypeSafe Accessors
		/// <summary>
		/// Gets the value of the specified column as a Boolean.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public bool GetBoolean(int i)
		{
			return Convert.ToBoolean(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a byte.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public byte GetByte(int i)
		{
			return Convert.ToByte(GetValue(i));
		}

		/// <summary>
		/// Reads a stream of bytes from the specified column offset into the buffer an array starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal. </param>
		/// <param name="dataIndex">The index within the field from which to begin the read operation. </param>
		/// <param name="buffer">The buffer into which to read the stream of bytes. </param>
		/// <param name="bufferIndex">The index for buffer to begin the read operation. </param>
		/// <param name="length">The maximum length to copy into the buffer. </param>
		/// <returns>The actual number of bytes read.</returns>
		public long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			if (i >= _fields.Length) 
				throw new IndexOutOfRangeException();

			long bufLen = _fields[i].BufferLength;

			if (buffer == null) 
				return bufLen;

			if (bufferIndex >= buffer.Length || bufferIndex < 0)
				throw new IndexOutOfRangeException("Buffer index must be a valid index in buffer");
			if (buffer.Length < (bufferIndex + length))
				throw new ArgumentException( "Buffer is not large enough to hold the requested data" );
			if (dataIndex < 0 || dataIndex >= bufLen )
				throw new IndexOutOfRangeException( "Data index must be a valid index in the field" );

			byte[] bytes = _fields[i].Buffer;
			long fieldIndex = _fields[i].BufferIndex;

			// adjust the length so we don't run off the end
			if ( bufLen < (dataIndex+length)) 
			{
				length = (int)((long)bytes.Length - dataIndex);
			}

			for (long x=0; x < length; x++)
			{
				buffer[bufferIndex+x] = bytes[fieldIndex+dataIndex+x];	
			}

			return length;
		}

		/// <summary>
		/// Gets the value of the specified column as a single character.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public char GetChar(int i)
		{
			return Convert.ToChar(GetValue(i));
		}

		/// <summary>
		/// Reads a stream of characters from the specified column offset into the buffer as an array starting at the given buffer offset.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="fieldOffset"></param>
		/// <param name="buffer"></param>
		/// <param name="bufferoffset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
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

			// adjust the length so we don't run off the end
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

		/// <summary>
		/// Gets the name of the source data type.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public String GetDataTypeName(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			// return the name of the type used on the backend
			return _fields[i].GetFieldTypeName();
		}

		/// <summary>
		/// Gets the value of the specified column as a DateTime object.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a Decimal object.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a double-precision floating point number.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public double GetDouble(int i)
		{
			return Convert.ToDouble(GetValue(i));
		}

		/// <summary>
		/// Gets the Type that is the data type of the object.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Type GetFieldType(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			return _fields[i].GetFieldType();
		}

		/// <summary>
		/// Gets the value of the specified column as a single-precision floating point number.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a globally-unique identifier (GUID).
		/// This is currently not supported.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Guid GetGuid(int i)
		{
			return new Guid( GetString(i) );
		}

		/// <summary>
		/// Gets the value of the specified column as a 16-bit signed integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Int16 GetInt16(int i)
		{
			return Convert.ToInt16(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a 32-bit signed integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Int32 GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a 64-bit signed integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public Int64 GetInt64(int i)
		{
			return Convert.ToInt64(GetValue(i));
		}

		/// <summary>
		/// Gets the name of the specified column.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public String GetName(int i)
		{
			return _fields[i].ColumnName;
		}

		/// <summary>
		/// Gets the column ordinal, given the name of the column.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int GetOrdinal(string name)
		{
			if (! isOpen)
				throw new Exception("No current query in data reader");

			for (int i=0; i < _fields.Length; i ++) 
			{
				if (_fields[i].ColumnName.ToLower().Equals(name.ToLower()))
					return i;
			}

			// Throw an exception if the ordinal cannot be found.
			throw new IndexOutOfRangeException("Could not find specified column in results");
		}

		/// <summary>
		/// Returns a DataTable that describes the column metadata of the MySqlDataReader.
		/// </summary>
		/// <returns></returns>
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
			foreach (MySqlField f in _fields)
			{
				DataRow r = dataTableSchema.NewRow();
				r["ColumnName"] = f.ColumnName;
				r["ColumnOrdinal"] = ord++;
				r["ColumnSize"] = f.ColumnLength;
				int prec = f.NumericPrecision;
				int pscale = f.NumericScale;
				if (prec != -1)
					r["NumericPrecision"] = (short)prec;
				if (pscale != -1)
					r["NumericScale"] = (short)pscale;
				r["DataType"] = f.GetFieldType();
				r["ProviderType"] = (int)f.Type;
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
				r["BaseColumnName"] = f.ColumnName;

				dataTableSchema.Rows.Add( r );
			}

			return dataTableSchema;
		}

		/// <summary>
		/// Gets the value of the specified column as a string.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public String GetString(int i)
		{
			return GetValue(i).ToString();
		}

		/// <summary>
		/// Gets the value of the specified column in its native format.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public object GetValue(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			return _fields[i].GetValue();
		}

		/// <summary>
		/// Gets all attribute columns in the collection for the current row.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public int GetValues(object[] values)
		{
			for (int i=0; i < _fields.Length; i ++) 
			{
				values[i] = GetValue(i);
			}

			return 0;
		}


		/// <summary>
		/// Gets the value of the specified column as a 16-bit unsigned integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public UInt16 GetUInt16( int i )
		{
			return Convert.ToUInt16(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a 32-bit unsigned integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public UInt32 GetUInt32( int i )
		{
			return Convert.ToUInt32(GetValue(i));
		}

		/// <summary>
		/// Gets the value of the specified column as a 64-bit unsigned integer.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public UInt64 GetUInt64( int i )
		{
			return Convert.ToUInt64(GetValue(i));
		}


		#endregion

		IDataReader IDataRecord.GetData(int i)
		{
			throw new NotSupportedException("GetData not supported.");
		}

		/// <summary>
		/// Gets a value indicating whether the column contains non-existent or missing values.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public bool IsDBNull(int i)
		{
			return DBNull.Value == GetValue(i);
		}

		/// <summary>
		/// Advances the data reader to the next result, when reading the results of batch SQL statements.
		/// </summary>
		/// <returns></returns>
		public bool NextResult()
		{
			if (! isOpen)
				throw new MySqlException("Invalid attempt to NextResult when reader is closed.");

			// clear any rows that have not been read from the last rowset
			if (currentResult != null)
				currentResult.Clear();

			// tell our command to continue execution of the SQL batch until it its
			// another resultset
			currentResult = command.ExecuteBatch(true);

			// if there was no more resultsets, then signal done
			if (currentResult == null) 
			{
				canRead = false;
				return false;
			}

			// When executing query statements, the result byte that is returned
			// from MySql is the column count.  That is why we reference the LastResult
			// property here to dimension our field array
			connection.SetState( ConnectionState.Fetching );
			
			_fields = new MySqlField[ currentResult.ColumnCount ];
			for (int x=0; x < _fields.Length; x++) 
				_fields[x] = currentResult.GetField();

			hasRows = currentResult.CheckForRows();
			canRead = hasRows;			

			connection.SetState( ConnectionState.Open );
			return true;
		}

		/// <summary>
		/// Advances the MySqlDataReader to the next record.
		/// </summary>
		/// <returns></returns>
		public bool Read()
		{
			if (! isOpen)
				throw new MySqlException("Invalid attempt to Read when reader is closed.");

			if (! canRead) return false;

			Driver driver = connection.InternalConnection.Driver;
			connection.SetState( ConnectionState.Fetching );

			try 
			{
				if (! currentResult.ReadDataRow())
				{
					canRead = false;
					return false;
				}

				for (int col=0; col < _fields.Length; col++)
				{
					byte[] buf = currentResult.GetFieldBuffer();
					int index = currentResult.GetFieldIndex();
					long len = currentResult.GetFieldLength();
					_fields[col].SetValueData( buf, index, len, driver.Version );
					currentResult.NextField();
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.WriteLine("MySql error: " + ex.Message);
				throw ex;
			}
			finally 
			{
				connection.SetState( ConnectionState.Open );
			}
			return true;
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
		IEnumerator	IEnumerable.GetEnumerator()
		{
			return new System.Data.Common.DbEnumerator(this);
		}
		#endregion
  }
}
