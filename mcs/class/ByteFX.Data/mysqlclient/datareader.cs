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
//		private Packet			rowPacket = null;

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
			get  { return ! isOpen; }
		}

		public void Dispose() 
		{
			if (isOpen)
				Close();
		}

		public int RecordsAffected 
		{
			// RecordsAffected returns the number of rows affected in batch
			// statments from insert/delete/update statments.  This property
			// is not completely accurate until .Close() has been called.
			get { return command.UpdateCount; }
		}

		public bool HasRows
		{
			get { return hasRows; }
		}

		/// <summary>
		/// 
		/// </summary>
		public void Close()
		{
			// finish any current command
			ClearCurrentResult();
			command.ExecuteBatch(false);

			connection.Reader = null;
			if (0 != (commandBehavior & CommandBehavior.CloseConnection))
				connection.Close();

			isOpen = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool NextResult()
		{
			if (! isOpen)
				throw new MySqlException("Invalid attempt to NextResult when reader is closed.");

			Driver driver = connection.InternalConnection.Driver;

			ClearCurrentResult();

			// tell our command to execute the next sql batch
			Packet packet = command.ExecuteBatch(true);

			// if there was no more batches, then signal done
			if (packet == null) return false;

			// When executing query statements, the result byte that is returned
			// from MySql is the column count.  That is why we reference the LastResult
			// property here to dimension our field array
			connection.SetState( ConnectionState.Fetching );
			
			_fields = new MySqlField[ packet.ReadLenInteger() ];
			for (int x=0; x < _fields.Length; x++) 
			{
				_fields[x] = new MySqlField();
				_fields[x].ReadSchemaInfo( packet );
			}

			// now take a quick peek at the next packet to see if we have rows
			// 
			packet = driver.PeekPacket();
			hasRows = packet.Type != PacketType.Last;
			canRead = hasRows;

			connection.SetState( ConnectionState.Open );
			return true;
		}

		/// <summary>
		/// 
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
				Packet rowPacket = driver.ReadPacket();
				if (rowPacket.Type == PacketType.Last) 
				{
					canRead = false;
					return false;
				}
				rowPacket.Position = 0;

				for (int col=0; col < _fields.Length; col++)
				{
					int len = (int)rowPacket.ReadLenInteger();
					_fields[col].SetValueData( rowPacket.GetBytes(), (int)rowPacket.Position, len, driver.Encoding );
					rowPacket.Position += len;
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
				int prec = f.NumericPrecision();
				int pscale = f.NumericScale();
				if (prec != -1)
					r["NumericPrecision"] = (short)prec;
				if (pscale != -1)
					r["NumericScale"] = (short)pscale;
				r["DataType"] = f.GetFieldType();
				r["ProviderType"] = (int)f.GetMySqlDbType();
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
				if (_fields != null)
					return _fields.Length;
				return 0;
			}
		}

		public String GetName(int i)
		{
			return _fields[i].ColumnName;
		}

		public String GetDataTypeName(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			// return the name of the type used on the backend
			return _fields[i].GetFieldTypeName();
		}

		public Type GetFieldType(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
			if (i >= _fields.Length) throw new IndexOutOfRangeException();

			return _fields[i].GetFieldType();
		}

		public object GetValue(int i)
		{
			if (! isOpen) throw new Exception("No current query in data reader");
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

		private MySqlField GetField(int i)
		{
			if (i >= _fields.Length) throw new IndexOutOfRangeException();
			return _fields[i];
		}

		#region TypeSafe Accessors
		public bool GetBoolean(int i)
		{
			return Convert.ToBoolean(GetValue(i));
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(GetValue(i));
		}

		public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
		{
			if (i >= _fields.Length) 
				throw new IndexOutOfRangeException();

			byte[] bytes = (byte[])GetValue(i);

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
			return Convert.ToChar(GetValue(i));
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
			return Convert.ToInt16(GetValue(i));
		}

		public UInt16 GetUInt16( int i )
		{
			return Convert.ToUInt16(GetValue(i));
		}

		public Int32 GetInt32(int i)
		{
			return Convert.ToInt32(GetValue(i));
		}

		public UInt32 GetUInt32( int i )
		{
			return Convert.ToUInt32(GetValue(i));
		}

		public Int64 GetInt64(int i)
		{
			return Convert.ToInt64(GetValue(i));
		}

		public UInt64 GetUInt64( int i )
		{
			return Convert.ToUInt64(GetValue(i));
		}

		public float GetFloat(int i)
		{
			return Convert.ToSingle(GetValue(i));
		}

		public double GetDouble(int i)
		{
			return Convert.ToDouble(GetValue(i));
		}

		public String GetString(int i)
		{
			return GetValue(i).ToString();
		}

		public Decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(GetValue(i));
		}

		public DateTime GetDateTime(int i)
		{
			return Convert.ToDateTime(GetValue(i));
		}
		#endregion

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

		#region Private Methods

		private void ClearCurrentResult() 
		{
			if (! canRead) return;

			Packet packet = connection.InternalConnection.Driver.ReadPacket();
			// clean out any current resultset
			while (packet.Type != PacketType.Last)
				packet = connection.InternalConnection.Driver.ReadPacket();
		}

		#endregion

		#region IEnumerator
		public IEnumerator	GetEnumerator()
		{
			return new System.Data.Common.DbEnumerator(this);
		}
		#endregion
  }
}
