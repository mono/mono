
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;

namespace IBM.Data.DB2
{
	/// <summary>
	/// Summary description for DB2ClientDataReader.
	/// DB2ClientDataReader. 
	/// </summary>
	public sealed class DB2DataReader : MarshalByRefObject, IDataReader
	{
		private struct ColumnInfo
		{
			public string	Colname;
			public int		Sqltype;
		}

		private object[] _resultSet;
		private ColumnInfo[] columnInfo;
		private Hashtable columnsNames;
		private const int internalBufferSize = 100;
		private IntPtr internalBuffer;
		internal DB2Connection db2Conn; 
		internal DB2Command db2Comm; 
		internal IntPtr hwndStmt;
		private int recordsAffected;
		private bool hasData = false;
		private int fieldCount = -1;
		private CommandBehavior behavior;
		private bool hasRows;
		private bool skipReadOnce;
	
		
		#region Constructors and destructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="con"></Connection object to Db2>
		/// <param name="com"></Command object>
		internal DB2DataReader(DB2Connection con, DB2Command com, CommandBehavior behavior)
		{
			db2Conn = con;
			db2Comm = com;
			this.behavior = behavior;
			hwndStmt = com.statementHandle;    //We have access to the results through the statement handle
			
			_resultSet = null;
			
			GetFieldCountAndAffectedRows();
			internalBuffer = Marshal.AllocHGlobal(internalBufferSize);

			isClosed = false;
		}

		#endregion

		private void GetFieldCountAndAffectedRows()
		{
			short sqlRet;
			recordsAffected = -1;
			if((behavior & CommandBehavior.SchemaOnly) == 0)
			{
				//How many rows affected.  numRows will be -1 if we aren't dealing with an Insert, Delete or Update, or if the statement did not execute successfully
				sqlRet = DB2CLIWrapper.SQLRowCount(hwndStmt, out recordsAffected);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "SQLExecDirect error.", db2Conn);
			}			
			short colCount;
			sqlRet = DB2CLIWrapper.SQLNumResultCols(hwndStmt, out colCount);
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLNumResultCols", db2Conn);
			fieldCount = colCount;
		}

		#region Properties

		#region Depth property 
		///
		///Depth of nesting for the current row, need to figure out what this translates into 
		///with DB2.
		///
		private int depth = 0;
		public int Depth
		{
			get
			{
				if(isClosed)
				{
					throw new InvalidOperationException("Reader is closed");
				}
				return depth;
			}
		}
		#endregion

		#region IsClosed property
		/// <summary>
		/// True if the reader is closed.
		/// </summary>
		private bool isClosed = true;
		public bool IsClosed
		{
			get
			{
				return isClosed;
			}
		}
		#endregion

		#region RecordsAffected property
		///
		/// Number of records affected by this operation.  Will be zero until we close the 
		/// reader
		/// 
		
		public int RecordsAffected
		{
			get
			{
				return recordsAffected;
			}
		}
		#endregion

		#endregion

		#region Methods



		#region Close method

		public void Close()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if(!isClosed) 
			{
				if(disposing)
				{
					short sqlRet;
					do
					{
						sqlRet = DB2CLIWrapper.SQLMoreResults(this.hwndStmt);
						DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLMoreResults", db2Conn);
					} while(sqlRet != DB2Constants.SQL_NO_DATA_FOUND);

					_resultSet = null;
					hasData = false;
					isClosed=true;

					if(db2Comm != null)
					{
						db2Comm.DataReaderClosed();
						db2Comm = null;
					}
				}
				Marshal.FreeHGlobal(internalBuffer);
			}
			isClosed = true;
		}

		~DB2DataReader()
		{
			Dispose(false);
		}

		#endregion

		#region GetSchemaTable 

		public DataTable GetSchemaTable()
		{
			if(isClosed)
			{
				throw new InvalidOperationException("No data exists for the row/column.");
			}

			DataTable _schemaTable = BuildNewSchemaTable();
			
			short sqlRet;
			IntPtr ptrCharacterAttribute = IntPtr.Zero;
			InitMem(256, ref ptrCharacterAttribute);
			short buflen = 256;
			short strlen = 256;
			int numericattr = 0;
			int colsize;
			string colname;
			int sqltype;
			int precision;
			int scale;
			int nullable;
			int updatable;
			int isautoincrement;
			string baseschemaname;
			//string basecatalogname;
			string basetablename;
			string basecolumnname;

			string previousTableName = null;
			string previousSchemaName = null;
			bool differentTablesUsed = false;

			for (short i=1; i<=fieldCount; i++) 
			{
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_COLUMN_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				colname = Marshal.PtrToStringUni(ptrCharacterAttribute);
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_CONCISE_TYPE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				sqltype = numericattr;
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_OCTET_LENGTH, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				colsize = numericattr;
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_PRECISION, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				precision = numericattr;
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_SCALE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				scale = numericattr;

				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_NULLABLE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				nullable = numericattr;

				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_UPDATABLE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				updatable = numericattr;
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_AUTO_UNIQUE_VALUE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				isautoincrement = numericattr;
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_BASE_COLUMN_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				basecolumnname = Marshal.PtrToStringUni(ptrCharacterAttribute);

				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_BASE_TABLE_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				basetablename = Marshal.PtrToStringUni(ptrCharacterAttribute);
				
				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)DB2Constants.SQL_DESC_SCHEMA_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable", db2Conn);
				baseschemaname = Marshal.PtrToStringUni(ptrCharacterAttribute);
				DataRow r = _schemaTable.NewRow();
				
				r["ColumnName"] = colname;
				r["ColumnOrdinal"] = i - 1;
				r["ColumnSize"] = colsize;
				r["NumericPrecision"] = precision;
				r["NumericScale"] = scale;
				r["DataType"] = GetManagedType((short)sqltype);
				r["ProviderType"] = sqltype;
				r["IsLong"] = IsLong((short)sqltype);
				r["AllowDBNull"] = (nullable==0) ? false : true;
				r["IsReadOnly"] = (basecolumnname == null) || (basecolumnname == "");
				r["IsRowVersion"] = false;
				r["IsUnique"] = false;
				r["IsKeyColumn"] = false;
				r["IsAutoIncrement"] = (isautoincrement==0) ? false : true;
				r["BaseSchemaName"] = baseschemaname;
				r["BaseCatalogName"] = "";
				r["BaseTableName"] = basetablename;
				r["BaseColumnName"] = basecolumnname;
				
				_schemaTable.Rows.Add(r);

				if(!differentTablesUsed)
				{
					if(((previousSchemaName == baseschemaname) && (previousTableName == basetablename)) || 
						(previousTableName == null))
					{
						previousTableName = basetablename;
						previousSchemaName = baseschemaname;
					}
					else
					{
						differentTablesUsed = true;
					}
				}
			}
			if(!differentTablesUsed && 
				((behavior & CommandBehavior.KeyInfo) != 0) &&
				(db2Comm.Transaction == null) &&
				(previousTableName != null) &&
				(previousTableName != ""))
			{
				DB2Command schemaInfoCommand = db2Conn.CreateCommand();
				schemaInfoCommand.CommandText = 
					"select concat(concat(INDSCHEMA,'.'),INDNAME), COLNAMES, UNIQUERULE from syscat.INDEXES " +
					"where TABSCHEMA=? and TABNAME=? and uniquerule in ('P','U') order by UNIQUERULE";
				schemaInfoCommand.Parameters.Add("TABSCHEMA", previousSchemaName);
				schemaInfoCommand.Parameters.Add("TABNAME", previousTableName);
				using(DB2DataReader reader = schemaInfoCommand.ExecuteReader())
				{
					bool keyColumnSet = false;
					while(reader.Read())
					{
						string indexName = reader.GetString(0);
						string[] indexColumns = reader.GetString(1).TrimStart('-', '+').Split('-', '+');
						bool primary = reader.GetString(2) == "P";

						bool allColumnsFound = true;
						for(int i= 0; i < indexColumns.Length; i++)
						{
							int ordinal = FieldNameLookup(_schemaTable, indexColumns[i]);
							if(ordinal < 0)
							{
								allColumnsFound = false;
								break;
							}
							if(indexColumns.Length == 1)
								_schemaTable.Rows[ordinal]["IsUnique"] = true;
						}
						if(allColumnsFound && !keyColumnSet)
						{
							for(int i= 0; i < indexColumns.Length; i++)
								_schemaTable.Rows[FieldNameLookup(_schemaTable, indexColumns[i])]["IsKeyColumn"] = true;
							keyColumnSet = true;
						}
					}
				}
				if(db2Conn.openConnection.MajorVersion >= 8)
				{
					try
					{
						schemaInfoCommand.CommandText = 
							"select COLNAME from SYSCAT.COLIDENTATTRIBUTES where TABSCHEMA=? and TABNAME=?";
						using(DB2DataReader reader = schemaInfoCommand.ExecuteReader())
						{
							while(reader.Read())
							{
								string columnName = reader.GetString(0);

								int ordinal = FieldNameLookup(_schemaTable, columnName);
								if(ordinal >= 0)
									_schemaTable.Rows[ordinal]["IsAutoIncrement"] = true;
							}
						}
					}
					catch{}
				}
			}
			return _schemaTable;
		}
		#endregion

		#region NextResult 

		public bool NextResult()
		{
			hasRows = false;
			skipReadOnce = false;
			hasData = false;
			columnInfo = null;
			_resultSet = null;
		
			if((behavior & (CommandBehavior.SchemaOnly | CommandBehavior.SingleResult)) != 0)
				return false;

			short sqlRet = DB2CLIWrapper.SQLMoreResults(this.hwndStmt);
			if(sqlRet == DB2Constants.SQL_NO_DATA_FOUND) 
				return false;
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLMoreResults", db2Conn);
			return true;
		}
		#endregion

		#region Read

#if !NET_1_0
		public bool HasRows
		{
			get
			{
				if(hasData)
					return true;

				hasRows = Read();
				hasData = false;
				skipReadOnce = true;
				return hasRows;
			}
		}
#endif

		public bool Read()
		{
			if (isClosed)
				throw new InvalidOperationException("Reader is closed");
			if((behavior & CommandBehavior.SchemaOnly) != 0)
				return false;

			if(skipReadOnce)
			{
				skipReadOnce = false;
				hasData = hasRows;
				return hasRows;
			}

			_resultSet = null;
			hasData = false;

			short sqlRet = DB2CLIWrapper.SQLFetch(hwndStmt);
			if(sqlRet == DB2Constants.SQL_NO_DATA_FOUND)
				return false;
			DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLFetch 1", db2Conn);

			hasData = true;
			return true;
		}
		#endregion

		#region GetColumnInfo
		private void GetColumnInfo()
		{
			if(isClosed)
				throw new InvalidOperationException("Reader is closed");
			if(fieldCount <= 0)
				throw new InvalidOperationException("No Fields found"); // TODO: check error
			if(columnInfo != null)
				return;
		
			columnInfo = new ColumnInfo[fieldCount];
			columnsNames = new Hashtable(fieldCount);
			
			StringBuilder sb = new StringBuilder(400);
			for(int i = 0; i < columnInfo.Length; i++)
			{
				short sqlRet;
				short strlen;
				int numericAttribute;

				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)(i + 1), (short)DB2Constants.SQL_DESC_BASE_COLUMN_NAME, sb, (short)sb.Capacity, out strlen, out numericAttribute);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable");
				columnInfo[i].Colname = sb.ToString();
				columnsNames[columnInfo[i].Colname.ToUpper()] = i;

				sqlRet = DB2CLIWrapper.SQLColAttribute(hwndStmt, (short)(i + 1), (short)DB2Constants.SQL_DESC_CONCISE_TYPE, sb, (short)sb.Capacity, out strlen, out columnInfo[i].Sqltype);
				DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_STMT, hwndStmt, "GetSchemaTable");


			}
		}
		#endregion

		#region Describe/Bind/Fetch functions
		///
		///Broke these out so that we can use different paths for Immediate executions and Prepared executions
		/// <summary>
		/// Does the describe and bind steps for the query result set.  Called for both immediate and prepared queries. 
		/// </summary>
		
/// <summary>
/// FetchResults does  what it says.
/// </summary>
/// <param name="dbVals"></param>
/// <param name="sqlLen_or_IndPtr"></param>
/// <param name="_resultSet"></param>
		private int FieldNameLookup(DataTable _schemaTable, string name)
		{
			for(int i = 0; i < _schemaTable.Rows.Count; i++)
			{
				if(CultureInfo.CurrentCulture.CompareInfo.Compare(name, (string)_schemaTable.Rows[i]["BaseColumnName"],
					CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase) == 0)
				{
					return i;
				}
			}
			return -1;
		}
		
		#endregion

		#region IDataRecord Interface
		///Code for the IDataRecord interface
		///
		#region FieldCount
		///
		///
		public int FieldCount
		{
			get
			{
				if (isClosed)
					throw new InvalidOperationException("Reader is closed");

				return fieldCount;
			}
		}
		#endregion

		#region Item accessors
		public object this[string name]
		{
			get
			{
				int ordinal = GetOrdinal(name);
				return this[ordinal];
			}
		}
		public object this[int col]
		{
			get
			{
				if(columnInfo == null)
				{
					GetColumnInfo();
				}
				switch(columnInfo[col].Sqltype)
				{
					case DB2Constants.SQL_INTEGER:
						return GetInt32Internal(col);
					case DB2Constants.SQL_SMALLINT:
						return GetInt16Internal(col);
					case DB2Constants.SQL_BIGINT:
						return GetInt64Internal(col);
					case DB2Constants.SQL_DOUBLE:
						return GetDoubleInternal(col);
					case DB2Constants.SQL_REAL:
						return GetFloatInternal(col);
					case DB2Constants.SQL_DECIMAL:
						return GetDecimalInternal(col);
					case DB2Constants.SQL_DATETIME:
					case DB2Constants.SQL_TYPE_TIMESTAMP:
						return GetDateTimeInternal(col);
					case DB2Constants.SQL_TYPE_DATE:
						return GetDateInternal(col);
					case DB2Constants.SQL_TYPE_TIME:
						return GetTimeInternal(col);
					case DB2Constants.SQL_TYPE_CLOB:
					case DB2Constants.SQL_CHAR:
					case DB2Constants.SQL_VARCHAR:
						return GetStringInternal(col);
					case DB2Constants.SQL_TYPE_BLOB:
					case DB2Constants.SQL_TYPE_BINARY:
					case DB2Constants.SQL_LONGVARBINARY:
					case DB2Constants.SQL_VARBINARY:
						return GetBlobDataInternal(col);
				}
				throw new NotImplementedException("Unknown SQL type " + columnInfo[col].Sqltype);
			}
		}
		#endregion

		#region GetBytes
		///
		///  GetBytes, return a stream of bytes
		///
		public long GetBytes(int col, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			// TODO: need better implementation for big BLOBs

			byte[] sourceArray = (byte[])this[col];
#if NET_1_0
			if(buffer == null)
			{
				Array.Copy(sourceArray, (int)fieldOffset, buffer, bufferOffset, length);
			}
			return sourceArray.Length;
#else
			if(buffer == null)
			{
				Array.Copy(sourceArray, fieldOffset, buffer, bufferOffset, length);
			}
			return sourceArray.LongLength;
#endif
		}
		#endregion

		#region GetChars
		///
		///GetChars, returns char array
		///
		public long GetChars(int col, long fieldOffset, char[] buffer, int bufferOffset, int length)
		{
			// TODO: need better implementation for big CLOBs

			string sourceString = GetString(col);
			if(buffer == null)
			{
				sourceString.CopyTo((int)fieldOffset, buffer, bufferOffset, length);
			}
			return (long)sourceString.Length;
		}
		#endregion

		#region GetBoolean method

		public Boolean GetBoolean(int col)
		{
			return (Boolean)GetBooleanInternal(col);
		}
		internal object GetBooleanInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_BIT, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.ReadByte(internalBuffer) != 0;
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region GetGuid
		///
		/// GetDateTime method
		/// 
		public Guid GetGuid(int col)
		{
			return (Guid)GetGuidInternal(col);
		}
		internal object GetGuidInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_GUID, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(Guid)); 
				}
			}		
			return _resultSet[col];
		}

		#endregion

		#region GetByte
		///
		///GetByte
		///
		public Byte GetByte(int col)
		{
			return (Byte)GetByteInternal(col);
		}
		internal object GetByteInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_UTINYINT, internalBuffer, 10, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.ReadByte(internalBuffer);
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region GetChar
		///
		///GetChar, return column as a char
		///
		public Char GetChar(int col)
		{
			return (Char)GetCharInternal(col);
		}
		internal object GetCharInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_WCHAR, internalBuffer, 10, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(char));
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region GetData
		///
		/// GetData method
		/// 
		public IDataReader GetData(int col)
		{
			//Have to research this one, not quite sure what the docs mean
			//DB2 does have some structured data types, is that what this is for?
			throw new NotSupportedException();
		}
		#endregion

		#region GetDataTypeName
		///
		///GetDataTypeName return the type of data
		///
		public string GetDataTypeName(int col)
		{
			if(columnInfo == null)
			{
				GetColumnInfo();
			}
			switch(columnInfo[col].Sqltype)
			{
				case DB2Constants.SQL_INTEGER:
					return "INTEGER";
				case DB2Constants.SQL_SMALLINT:
					return "SMALLINT";
				case DB2Constants.SQL_BIGINT:
					return "BIGINT";
				case DB2Constants.SQL_DOUBLE:
					return "DOUBLE";
				case DB2Constants.SQL_REAL:
					return "REAL";
				case DB2Constants.SQL_DECIMAL:
					return "DECIMAL";
				case DB2Constants.SQL_DATETIME:
					return "DATETIME";
				case DB2Constants.SQL_TYPE_TIMESTAMP:
					return "TIMESTAMP";
				case DB2Constants.SQL_TYPE_DATE:
					return "DATE";
				case DB2Constants.SQL_TYPE_TIME:
					return "TIME";
				case DB2Constants.SQL_TYPE_CLOB:
					return "CLOB";
				case DB2Constants.SQL_CHAR:
					return "CHAR";
				case DB2Constants.SQL_VARCHAR:
					return "VARCHAR";
				case DB2Constants.SQL_TYPE_BLOB:
					return "BLOB";
				case DB2Constants.SQL_TYPE_BINARY:
					return "BINARY";
				case DB2Constants.SQL_LONGVARBINARY:
					return "LONGVARBINARY";
				case DB2Constants.SQL_VARBINARY:
					return "VARBINARY";
			}
			throw new NotImplementedException("Unknown SQL type " + columnInfo[col].Sqltype);
		}
		#endregion

		#region GetDateTime
		///
		/// GetDateTime method
		/// 

		public DateTime GetDateTime(int col)
		{
			return (DateTime)GetDateTimeInternal(col);
		}
		internal object GetDateTimeInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_TIMESTAMP, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					DateTime ret = new DateTime(
						Marshal.ReadInt16(internalBuffer, 0),  // year
						Marshal.ReadInt16(internalBuffer, 2),  // month
						Marshal.ReadInt16(internalBuffer, 4),  // day
						Marshal.ReadInt16(internalBuffer, 6),  // hour
						Marshal.ReadInt16(internalBuffer, 8),  // minute
						Marshal.ReadInt16(internalBuffer, 10));// second
					_resultSet[col] = ret.AddTicks(Marshal.ReadInt32(internalBuffer, 12) / 100); // nanoseconds 
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region GetDate
		///
		/// GetDate method
		/// 
		public DateTime GetDate(int col)
		{
			return (DateTime)GetDateInternal(col);
		}
		internal object GetDateInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_DATE, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = new DateTime(
						Marshal.ReadInt16(internalBuffer, 0),  // year
						Marshal.ReadInt16(internalBuffer, 2),  // month
						Marshal.ReadInt16(internalBuffer, 4));  // day
				}
			}		
			return _resultSet[col];
		}

		#endregion

		#region GetTime
		///
		/// GetTime method
		/// 
		public TimeSpan GetTimeSpan(int col)
		{
			return (TimeSpan)GetTimeInternal(col);
		}
		public TimeSpan GetTime(int col)
		{
			return (TimeSpan)GetTimeInternal(col);
		}
		internal object GetTimeInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_TIME, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = new TimeSpan(
						Marshal.ReadInt16(internalBuffer, 0),  // Hour
						Marshal.ReadInt16(internalBuffer, 2),  // Minute
						Marshal.ReadInt16(internalBuffer, 4)); // Second
				}
			}		
			return _resultSet[col];
		}

		#endregion


		#region GetDecimal
		///
		///GetDecimal method
		///

		public Decimal GetDecimal(int col)
		{
			return (Decimal)GetDecimalInternal(col);
		}
		internal object GetDecimalInternal(int col)
		{
			object tmp = GetStringInternal(col);
			if(tmp is string)
			{
				_resultSet[col] = decimal.Parse(((string)_resultSet[col]).Replace(',','.'),	// sometimes we get a '.' and sometimes we get a ','
					System.Globalization.CultureInfo.InvariantCulture);
			}
			//			if((col < 0) || (col >= fieldCount))	// only works on windows UDB DB2 V8?
			//			{
			//				throw new IndexOutOfRangeException("col");
			//			}
			//			if(!hasData)
			//			{
			//				throw new InvalidOperationException("No data");
			//			}
			//			if(_resultSet == null)
			//			{
			//				_resultSet = new object[fieldCount];
			//			}
			//			if(_resultSet[col] == null)
			//			{
			//				int len;
			//				short sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)Db2Constants.SQL_C_DECIMAL_OLEDB, internalBuffer, internalBufferSize, out len);
			//				if(len == Db2Constants.SQL_NULL_DATA)
			//				{
			//					_resultSet[col] = DBNull.Value;
			//				}
			//				else
			//				{
			//					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(decimal));
			//				}
			//			}		
			return _resultSet[col];
		}
		#endregion

		#region GetDouble 
		///
		/// GetDouble 
		/// 
		public Double GetDouble(int col)
		{
			return (Double)GetDoubleInternal(col);
		}
		internal object GetDoubleInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_DOUBLE, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(double));
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region GetFieldType
		///
		/// Type GetFieldType
		///
		public Type GetFieldType(int col)
		{
			if(columnInfo == null)
			{
				GetColumnInfo();
			}
			return GetManagedType(columnInfo[col].Sqltype);
		}
		#endregion

		#region GetFloat
		///
		/// GetFloat
		/// 
		public float GetFloat(int col)
		{
			return (float)GetFloatInternal(col);
		}
		internal object GetFloatInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_REAL, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(float));
				}
			}		
			return _resultSet[col];
		}
		#endregion

		#region The GetInt?? series
		///
		///GetInt16
		///
		public short GetInt16(int col)
		{
			return (short)GetInt16Internal(col);
		}

		internal object GetInt16Internal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_SSHORT, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(short));
				}
			}		
			return _resultSet[col];
		}
		///
		///GetInt32
		///
		public int GetInt32(int col)
		{
			return (int)GetInt32Internal(col);
		}

		internal object GetInt32Internal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_SLONG, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(int));
				}
			}		
			return _resultSet[col];
		}

		///
		///GetInt64
		///
		public long GetInt64(int col)
		{
			return (long)GetInt64Internal(col);
		}

		internal object GetInt64Internal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int len;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_SBIGINT, internalBuffer, internalBufferSize, out len);
				if(len == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					_resultSet[col] = Marshal.PtrToStructure(internalBuffer, typeof(long));
				}
			}		
			return _resultSet[col];
		}

		#endregion

		#region GetName
		///
		///GetName, returns the name of the field
		///
		public string GetName(int col)
		{
			if(columnInfo == null)
			{
				GetColumnInfo();
			}
			return columnInfo[col].Colname;
		}
		#endregion

		#region GetOrdinal
		///
		/// GetOrdinal, return the index of the named column
		/// 
		public int GetOrdinal(string name)
		{
			if(columnInfo == null)
			{
				GetColumnInfo();
			}
			object ordinal = columnsNames[name.ToUpper()];
			if(ordinal == null)
			{
				throw new IndexOutOfRangeException("name");
			}
			return (int)ordinal;
		}
		#endregion

		#region GetString
		///
		/// GetString returns a string
		/// 
		public string GetString(int col)
		{
			return (string)GetStringInternal(col);
		}

		public object GetStringInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int length;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_WCHAR, (StringBuilder)null, 0, out length);
				if(length == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					IntPtr mem = Marshal.AllocHGlobal(length + 2);
					sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_WCHAR, mem, length + 2, out length);
					_resultSet[col] = Marshal.PtrToStringUni(mem);
					Marshal.FreeHGlobal(mem);
				}
			}			
			return _resultSet[col];
		}
		#endregion

		#region GetValue
		///
		/// GetVCalue, returns an object
		/// 
		public object GetValue(int col)
		{
			return this[col];
		}
		#endregion

		#region GetValues
		///
		/// GetValues returns all columns in the row through the argument, and the number of columns in the return value
		/// 
		public int GetValues(object[] values)
		{
			int count = Math.Min(fieldCount, values.Length);

			for (int i = 0; i < count; i++)
			{
				values[i] = this[i];
			 
			}
			  
			return count;
		}
		#endregion

		#region IsDBNull
		///
		/// IsDBNull Is the column null
		/// 
		public bool IsDBNull(int col)
		{
			//Proper implementation once I get the SQLDescribe/SQLBind/SQLFetch stuff in place
			return Convert.IsDBNull(this[col]);
		}
		#endregion

		#endregion  ///For IDataRecord

		#region private methods
		
		private DataTable BuildNewSchemaTable()
		{
			DataTable schemaTable = new DataTable("SchemaTable");

			schemaTable.Columns.Add(new DataColumn("ColumnName", typeof(string)));
			schemaTable.Columns.Add(new DataColumn("ColumnOrdinal", typeof(int)));
			schemaTable.Columns.Add(new DataColumn("ColumnSize", typeof(int)));
			schemaTable.Columns.Add(new DataColumn("NumericPrecision", typeof(short)));
			schemaTable.Columns.Add(new DataColumn("NumericScale", typeof(short)));
			schemaTable.Columns.Add(new DataColumn("DataType", typeof(System.Type)));
			schemaTable.Columns.Add(new DataColumn("ProviderType", typeof(int)));
			schemaTable.Columns.Add(new DataColumn("IsLong", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("AllowDBNull", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsReadOnly", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsRowVersion", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsUnique", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsKey", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsKeyColumn", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("IsAutoIncrement", typeof(bool)));
			schemaTable.Columns.Add(new DataColumn("BaseSchemaName", typeof(string)));
			schemaTable.Columns.Add(new DataColumn("BaseCatalogName", typeof(string)));
			schemaTable.Columns.Add(new DataColumn("BaseTableName", typeof(string)));
			schemaTable.Columns.Add(new DataColumn("BaseColumnName", typeof(string)));

			return schemaTable;
		}
		#endregion
		
		private void InitMem(int memSize, ref IntPtr ptr){
			if (ptr.ToInt32() == 0){
				unsafe{
					fixed(byte* arr = new byte[memSize]){
						ptr = new IntPtr(arr); 
					}
				}
			}	
		}
		
		private Type GetManagedType(int sql_type)
		{
			switch(sql_type)
			{
				case DB2Constants.SQL_INTEGER:
					return typeof(int);
				case DB2Constants.SQL_SMALLINT:
					return typeof(short);
				case DB2Constants.SQL_BIGINT:
					return typeof(long);
				case DB2Constants.SQL_DOUBLE:
					return typeof(double);
				case DB2Constants.SQL_DECIMAL:
					return typeof(decimal);
				case DB2Constants.SQL_DATETIME:
				case DB2Constants.SQL_TYPE_DATE:
				case DB2Constants.SQL_TYPE_TIMESTAMP:
					return typeof(DateTime);
				case DB2Constants.SQL_TYPE_TIME:
					return typeof(TimeSpan);
				case DB2Constants.SQL_CHAR:
				case DB2Constants.SQL_VARCHAR:
				case DB2Constants.SQL_TYPE_CLOB:
					return typeof(string);
				case DB2Constants.SQL_TYPE_BLOB:
				case DB2Constants.SQL_TYPE_BINARY:
				case DB2Constants.SQL_LONGVARBINARY:
				case DB2Constants.SQL_VARBINARY:
					return typeof(byte[]);
			}
			throw new NotImplementedException("Unknown SQL type " + sql_type);
		}
		
		private bool IsLong(short sql_type)
		{
			switch(sql_type)
			{
				case DB2Constants.SQL_TYPE_CLOB:
				case DB2Constants.SQL_TYPE_BLOB:
					return true;
			}
			return false;
		}
		private object GetBlobDataInternal(int col)
		{
			if((col < 0) || (col >= fieldCount))
			{
				throw new IndexOutOfRangeException("col");
			}
			if(!hasData)
			{
				throw new InvalidOperationException("No data");
			}
			if(_resultSet == null)
			{
				_resultSet = new object[fieldCount];
			}
			if(_resultSet[col] == null)
			{
				int length;
				short sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_BINARY, (StringBuilder)null, 0, out length);
				if(length == DB2Constants.SQL_NULL_DATA)
				{
					_resultSet[col] = DBNull.Value;
				}
				else
				{
					byte[] result = new byte[length];
					sqlRet = DB2CLIWrapper.SQLGetData(this.hwndStmt, (short)(col + 1), (short)DB2Constants.SQL_C_TYPE_BINARY, result, length, out length);
					_resultSet[col] = result;
				}
			}			
			return _resultSet[col];
		}
	}

}
#endregion
