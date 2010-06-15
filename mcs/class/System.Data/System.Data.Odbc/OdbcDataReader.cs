//
// System.Data.Odbc.OdbcDataReader
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//   Daniel Morgan <danmorg@sc.rr.com>
//   Sureshkumar T <tsureshkumar@novell.com> (2004)
//
// Copyright (C) Brian Ritchie, 2002
// Copyright (C) Daniel Morgan, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;

namespace System.Data.Odbc
{
#if NET_2_0
	public sealed class OdbcDataReader : DbDataReader
#else
	public sealed class OdbcDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
#endif
	{
		#region Fields
		
		private OdbcCommand command;
		private bool open;
		private int currentRow;
		private OdbcColumn[] cols;
		private IntPtr hstmt;
		private int _recordsAffected = -1;
		bool disposed;
		private DataTable _dataTableSchema;
		private CommandBehavior behavior;

		#endregion

		#region Constructors

		internal OdbcDataReader (OdbcCommand command, CommandBehavior behavior)
		{
			this.command = command;
			this.CommandBehavior = behavior;
			open = true;
			currentRow = -1;
			hstmt = command.hStmt;
			// Init columns array;
			short colcount = 0;
			libodbc.SQLNumResultCols (hstmt, ref colcount);
			cols = new OdbcColumn [colcount];
			GetColumns ();
		}

		internal OdbcDataReader (OdbcCommand command, CommandBehavior behavior,
					 int recordAffected) : this (command, behavior)
		{
			_recordsAffected = recordAffected;
		}
		

		#endregion

		#region Properties

		private CommandBehavior CommandBehavior {
			get { return behavior; }
			set { behavior = value; }
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int Depth {
			get {
				return 0; // no nested selects supported
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int FieldCount {
			get {
				if (IsClosed)
					throw new InvalidOperationException ("The reader is closed.");
				return cols.Length;
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool IsClosed {
			get {
				return !open;
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object this [string value] {
			get {
				int pos = GetOrdinal (value);
				return this [pos];
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object this [int i] {
			get {
				return GetValue (i);
			}
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int RecordsAffected {
			get {
				return _recordsAffected;
			}
		}

		[MonoTODO]
		public
#if NET_2_0
		override
#endif // NET_2_0
		bool HasRows {
			get { throw new NotImplementedException(); }
		}

		private OdbcConnection Connection {
			get {
				if (command != null)
					return command.Connection;
				return null;
			}
		}

		#endregion

		#region Methods
		
		private int ColIndex (string colname)
		{
			int i = 0;
			foreach (OdbcColumn col in cols) {
				if (col != null) {
					if (col.ColumnName == colname)
						return i;
					if (String.Compare (col.ColumnName, colname, true) == 0)
						return i;
				}
				i++;
			}
			return -1;
		}

		// Dynamically load column descriptions as needed.
		private OdbcColumn GetColumn (int ordinal)
		{
			if (cols [ordinal] == null) {
				short bufsize = 255;
				byte [] colname_buffer = new byte [bufsize];
				string colname;
				short colname_size = 0;
				uint ColSize = 0;
				short DecDigits = 0, Nullable = 0, dt = 0;
				OdbcReturn ret = libodbc.SQLDescribeCol (hstmt, Convert.ToUInt16 (ordinal + 1), 
									 colname_buffer, bufsize, ref colname_size, ref dt, ref ColSize, 
									 ref DecDigits, ref Nullable);
				if ((ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, hstmt);
				colname = RemoveTrailingNullChar (Encoding.Unicode.GetString (colname_buffer));
				OdbcColumn c = new OdbcColumn (colname, (SQL_TYPE) dt);
				c.AllowDBNull = (Nullable != 0);
				c.Digits = DecDigits;
				if (c.IsVariableSizeType)
					c.MaxLength = (int) ColSize;
				cols [ordinal] = c;
			}
			return cols [ordinal];
		}
		
		// Load all column descriptions
		private void GetColumns ()
		{
			for(int i = 0; i < cols.Length; i++)
				GetColumn (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Close ()
		{
			// FIXME : have to implement output parameter binding
			open = false;
			currentRow = -1;

			this.command.FreeIfNotPrepared ();

			if ((this.CommandBehavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
				this.command.Connection.Close ();
		}

#if ONLY_1_1
		~OdbcDataReader ()
		{
			this.Dispose (false);
		}
#endif

		public 
#if NET_2_0
		override
#endif // NET_2_0
		bool GetBoolean (int i)
		{
			return (bool) GetValue (i);
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		byte GetByte (int i)
		{
			return Convert.ToByte (GetValue (i));
		}

		public 
#if NET_2_0
		override
#endif // NET_2_0
		long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			if (IsClosed)
				throw new InvalidOperationException ("Reader is not open.");
			if (currentRow == -1)
				throw new InvalidOperationException ("No data available.");

			OdbcReturn ret = OdbcReturn.Error;
			bool copyBuffer = false;
			int returnVal = 0, outsize = 0;
			byte [] tbuff = new byte [length+1];

			if (buffer == null)
				length = 0;
			ret=libodbc.SQLGetData (hstmt, (ushort) (i + 1), SQL_C_TYPE.BINARY, tbuff, length, 
						ref outsize);

			if (ret == OdbcReturn.NoData)
				return 0;

			if ( (ret != OdbcReturn.Success) && (ret != OdbcReturn.SuccessWithInfo))
				throw Connection.CreateOdbcException (OdbcHandleType.Stmt, hstmt);

			OdbcException odbcException = null;
			if ( (ret == OdbcReturn.SuccessWithInfo))
				odbcException = Connection.CreateOdbcException (
					OdbcHandleType.Stmt, hstmt);

			if (buffer == null)
				return outsize; //if buffer is null,return length of the field

			if (ret == OdbcReturn.SuccessWithInfo) {
				if (outsize == (int) OdbcLengthIndicator.NoTotal)
					copyBuffer = true;
				else if (outsize == (int) OdbcLengthIndicator.NullData) {
					copyBuffer = false;
					returnVal = -1;
				} else {
					string sqlstate = odbcException.Errors [0].SQLState;
					//SQLState: String Data, Right truncated
					if (sqlstate != libodbc.SQLSTATE_RIGHT_TRUNC)
						throw odbcException;
					copyBuffer = true;
				}
			} else {
				copyBuffer = outsize == -1 ? false : true;
				returnVal = outsize;
			}

			if (copyBuffer) {
				if (outsize == (int) OdbcLengthIndicator.NoTotal) {
					int j = 0;
					while (tbuff [j] != libodbc.C_NULL) {
						buffer [bufferIndex + j] = tbuff [j];
						j++;
					}
					returnVal = j;
				} else {
					int read_bytes = Math.Min (outsize, length);
					for (int j = 0; j < read_bytes; j++)
						buffer [bufferIndex + j] = tbuff [j];
					returnVal = read_bytes;
				}
			}
			return returnVal;
		}
		
		[MonoTODO]
		public 
#if NET_2_0
		override
#endif // NET_2_0
		char GetChar (int i)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public 
#if NET_2_0
		override
#endif // NET_2_0
		long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			if (currentRow == -1)
				throw new InvalidOperationException ("No data available.");
			if (i < 0 || i >= FieldCount)
				throw new IndexOutOfRangeException ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
#if ONLY_1_1
		public
#endif
#if NET_2_0
		new
#endif
		IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetDataTypeName (int i)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			if (i < 0 || i >= FieldCount)
				throw new IndexOutOfRangeException ();
			return GetColumnAttributeStr (i + 1, FieldIdentifier.TypeName);
		}

		public DateTime GetDate (int i)
		{
			return GetDateTime (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		DateTime GetDateTime (int i)
		{
			return (DateTime) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		decimal GetDecimal (int i)
		{
			return (decimal) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		double GetDouble (int i)
		{
			return (double) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		Type GetFieldType (int i)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			return GetColumn (i).DataType;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		float GetFloat (int i)
		{
			return (float) GetValue (i);
		}

		[MonoTODO]
		public
#if NET_2_0
		override
#endif // NET_2_0
		Guid GetGuid (int i)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		short GetInt16 (int i)
		{
			return (short) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int GetInt32 (int i)
		{
			return (int) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		long GetInt64 (int i)
		{
			return (long) GetValue (i);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetName (int i)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			return GetColumn (i).ColumnName;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int GetOrdinal (string value)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			if (value == null)
				throw new ArgumentNullException ("fieldName");

			int i = ColIndex (value);
			if (i == -1)
				throw new IndexOutOfRangeException ();
			return i;
		}

		[MonoTODO]
		public
#if NET_2_0
		override
#endif // NET_2_0
		DataTable GetSchemaTable ()
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");

			// FIXME : 
			// * Map OdbcType to System.Type and assign to DataType.
			//   This will eliminate the need for IsStringType in
			//   OdbcColumn

			if (_dataTableSchema != null)
				return _dataTableSchema;
			
			DataTable dataTableSchema = null;
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if (cols.Length > 0) {
				dataTableSchema = new DataTable ();
				
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

				DataRow schemaRow;

				for (int i = 0; i < cols.Length; i += 1 ) {
					OdbcColumn col=GetColumn(i);

					schemaRow = dataTableSchema.NewRow ();
					dataTableSchema.Rows.Add (schemaRow);

					schemaRow ["ColumnName"] 	= col.ColumnName;
					schemaRow ["ColumnOrdinal"] 	= i;
					schemaRow ["ColumnSize"] 	= col.MaxLength;
					schemaRow ["NumericPrecision"] 	= GetColumnAttribute (i+1, FieldIdentifier.Precision);
					schemaRow ["NumericScale"] 	= GetColumnAttribute (i+1, FieldIdentifier.Scale);
					schemaRow ["BaseTableName"]   	= GetColumnAttributeStr (i+1, FieldIdentifier.TableName);
					schemaRow ["BaseSchemaName"]  	= GetColumnAttributeStr (i+1, FieldIdentifier.SchemaName);
					schemaRow ["BaseCatalogName"] 	= GetColumnAttributeStr (i+1, FieldIdentifier.CatelogName);
					schemaRow ["BaseColumnName"] 	= GetColumnAttributeStr (i+1, FieldIdentifier.BaseColumnName);
					schemaRow ["DataType"] 		= col.DataType;
					schemaRow ["IsUnique"] 		= false;
					schemaRow ["IsKey"] 		= DBNull.Value;
					schemaRow ["AllowDBNull"] 	= GetColumnAttribute (i+1, FieldIdentifier.Nullable) != libodbc.SQL_NO_NULLS;
					schemaRow ["ProviderType"] 	= (int) col.OdbcType;
					schemaRow ["IsAutoIncrement"] 	= GetColumnAttribute (i+1, FieldIdentifier.AutoUniqueValue) == libodbc.SQL_TRUE;
					schemaRow ["IsExpression"] 	= schemaRow.IsNull ("BaseTableName") || (string) schemaRow ["BaseTableName"] == String.Empty;
					schemaRow ["IsAliased"] 	= (string) schemaRow ["BaseColumnName"] != (string) schemaRow ["ColumnName"];
					schemaRow ["IsReadOnly"] 	= ((bool) schemaRow ["IsExpression"]
									   || GetColumnAttribute (i+1, FieldIdentifier.Updatable) == libodbc.SQL_ATTR_READONLY);

					// FIXME: all of these
					schemaRow ["IsIdentity"] 	= false;
					schemaRow ["IsRowVersion"] 	= false;
					schemaRow ["IsHidden"] 		= false;
					schemaRow ["IsLong"] 		= false;

					// FIXME: according to Brian, 
					// this does not work on MS .NET
					// however, we need it for Mono 
					// for now
					// schemaRow.AcceptChanges();
				}

				// set primary keys
				DataRow [] rows = dataTableSchema.Select ("BaseTableName <> ''",
									  "BaseCatalogName, BaseSchemaName, BaseTableName ASC");

				string lastTableName = String.Empty,
					lastSchemaName = String.Empty,
					lastCatalogName = String.Empty;
				string [] keys = null; // assumed to be sorted.
				foreach (DataRow row in rows) {
					string tableName = (string) row ["BaseTableName"];
					string schemaName = (string) row ["BaseSchemaName"];
					string catalogName = (string) row ["BaseCatalogName"];

					if (tableName != lastTableName || schemaName != lastSchemaName
					    || catalogName != lastCatalogName)
						keys = GetPrimaryKeys (catalogName, schemaName, tableName);
				
					if (keys != null &&
					    Array.BinarySearch (keys, (string) row ["BaseColumnName"]) >= 0) {
						row ["IsKey"] = true;
						row ["IsUnique"] = true;
						row ["AllowDBNull"] = false;
						GetColumn ( ColIndex ( (string) row ["ColumnName"])).AllowDBNull = false;
					}
					lastTableName = tableName;
					lastSchemaName = schemaName;
					lastCatalogName = catalogName;
				}
				dataTableSchema.AcceptChanges ();
			}
			return (_dataTableSchema = dataTableSchema);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		string GetString (int i)
		{
			object ret = GetValue (i);

			if (ret != null && ret.GetType () != typeof (string))
				return Convert.ToString (ret);
			else
				return (string) GetValue (i);
		}

		[MonoTODO]
		public TimeSpan GetTime (int i)
		{
			throw new NotImplementedException ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		object GetValue (int i)
		{
			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			if (currentRow == -1)
				throw new InvalidOperationException ("No data available.");
			if (i > cols.Length-1 || i < 0)
				throw new IndexOutOfRangeException ();

			OdbcReturn ret;
			int outsize = 0, bufsize;
			byte[] buffer;
			OdbcColumn col = GetColumn (i);
			object DataValue = null;
			ushort ColIndex = Convert.ToUInt16 (i + 1);

			// Check cached values
			if (col.Value == null) {
				// odbc help file
				// mk:@MSITStore:C:\program%20files\Microsoft%20Data%20Access%20SDK\Docs\odbc.chm::/htm/odbcc_data_types.htm
				switch (col.OdbcType) {
				case OdbcType.Bit:
					short bit_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref bit_data, 0, ref outsize);
					if (outsize != (int) OdbcLengthIndicator.NullData)
						DataValue = bit_data == 0 ? "False" : "True";
					break;
				case OdbcType.Numeric:
				case OdbcType.Decimal:
					bufsize = 50;
					buffer = new byte [bufsize];  // According to sqlext.h, use SQL_CHAR for decimal.
					// FIXME : use Numeric.
					ret = libodbc.SQLGetData (hstmt, ColIndex, SQL_C_TYPE.CHAR, buffer, bufsize, ref outsize);
					if (outsize!=-1) {
						byte [] temp = new byte [outsize];
						for (int j = 0; j < outsize; j++)
							temp [j] = buffer [j];
						DataValue = Decimal.Parse (Encoding.Default.GetString (temp),
							CultureInfo.InvariantCulture);
					}
					break;
				case OdbcType.TinyInt:
					short short_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref short_data, 0, ref outsize);
					DataValue = Convert.ToByte (short_data);
					break;
				case OdbcType.Int:
					int int_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref int_data, 0, ref outsize);
					DataValue = int_data;
					break;

				case OdbcType.SmallInt:
					short sint_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref sint_data, 0, ref outsize);
					DataValue = sint_data;
					break;

				case OdbcType.BigInt:
					long long_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref long_data, 0, ref outsize);
					DataValue = long_data;
					break;
				case OdbcType.NChar:
					bufsize = 255;
					buffer = new byte [bufsize];
					ret = libodbc.SQLGetData (hstmt, ColIndex, SQL_C_TYPE.WCHAR, buffer, bufsize, ref outsize);
					if (outsize != (int) OdbcLengthIndicator.NullData)
						if (!(ret == OdbcReturn.SuccessWithInfo
						       && outsize == (int) OdbcLengthIndicator.NoTotal))
							DataValue = Encoding.Unicode.GetString (buffer, 0, outsize);
					break;
				case OdbcType.NText:
				case OdbcType.NVarChar:
					bufsize = (col.MaxLength < 127 ? (col.MaxLength*2+1) : 255);
					buffer = new byte[bufsize];  // According to sqlext.h, use SQL_CHAR for both char and varchar
					StringBuilder sb = new StringBuilder ();
					do {
						ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, buffer, bufsize, ref outsize);
						if (ret == OdbcReturn.Error)
							break;
						// Fix for strance ODBC drivers (like psqlODBC)
						if (ret == OdbcReturn.Success && outsize==-1)
							ret = OdbcReturn.NoData;

						if (ret != OdbcReturn.NoData && outsize > 0) {
							string value = null;

							if (outsize < bufsize)
								value = Encoding.Unicode.GetString (buffer, 0, outsize);
							else
								value = Encoding.Unicode.GetString (buffer, 0, bufsize);

							sb.Append (RemoveTrailingNullChar (value));
						}
					} while (ret != OdbcReturn.NoData);
					DataValue = sb.ToString ();
					break;
				case OdbcType.Text:
				case OdbcType.VarChar:
					bufsize = (col.MaxLength < 255 ? (col.MaxLength+1) : 255);
					buffer = new byte[bufsize];  // According to sqlext.h, use SQL_CHAR for both char and varchar
					StringBuilder sb1 = new StringBuilder ();
					do { 
						ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, buffer, bufsize, ref outsize);
						if (ret == OdbcReturn.Error)
							break;
						// Fix for strance ODBC drivers (like psqlODBC)
						if (ret == OdbcReturn.Success && outsize==-1)
							ret = OdbcReturn.NoData;
						if (ret != OdbcReturn.NoData && outsize > 0) {
							if (outsize < bufsize)
								sb1.Append (Encoding.Default.GetString (buffer, 0, outsize));
							else
								sb1.Append (Encoding.Default.GetString (buffer, 0, bufsize - 1));
						}
					} while (ret != OdbcReturn.NoData);
					DataValue = sb1.ToString ();
					break;
				case OdbcType.Real:
					float float_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref float_data, 0, ref outsize);
					DataValue = float_data;
					break;
				case OdbcType.Double:
					double double_data = 0;
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref double_data, 0, ref outsize);
					DataValue = double_data;
					break;
				case OdbcType.Timestamp:
				case OdbcType.DateTime:
				case OdbcType.Date:
				case OdbcType.Time:
					OdbcTimestamp ts_data = new OdbcTimestamp();
					ret = libodbc.SQLGetData (hstmt, ColIndex, col.SqlCType, ref ts_data, 0, ref outsize);
					if (outsize != -1) {// This means SQL_NULL_DATA
						if (col.OdbcType == OdbcType.Time) {
							// libodbc returns value in first three fields for OdbcType.Time 
							DataValue = new System.TimeSpan (ts_data.year, ts_data.month, ts_data.day);
						} else {
							DataValue = new DateTime(ts_data.year, ts_data.month,
							                         ts_data.day, ts_data.hour, ts_data.minute,
							                         ts_data.second);
							if (ts_data.fraction != 0)
								DataValue = ((DateTime) DataValue).AddTicks ((long)ts_data.fraction / 100);
						}
					}
					break;
				case OdbcType.VarBinary :
				case OdbcType.Image :
					bufsize = (col.MaxLength < 255 && col.MaxLength > 0 ? col.MaxLength : 255);
					buffer= new byte [bufsize];
					ArrayList al = new ArrayList ();
					//get the size of data to be returned.
					ret = libodbc.SQLGetData (hstmt, ColIndex, SQL_C_TYPE.BINARY, buffer, 0, ref outsize);
					if (outsize != (int) OdbcLengthIndicator.NullData) {
						do {
							ret = libodbc.SQLGetData (hstmt, ColIndex, SQL_C_TYPE.BINARY, buffer, bufsize, ref outsize);
							if (ret == OdbcReturn.Error)
								break;
							if (ret != OdbcReturn.NoData && outsize != -1) {
								if (outsize < bufsize) {
									byte [] tmparr = new byte [outsize];
									Array.Copy (buffer, 0, tmparr, 0, outsize);
									al.AddRange (tmparr);
								} else
									al.AddRange (buffer);
							} else {
								break;
							}
						} while (ret != OdbcReturn.NoData);
					}
					DataValue = al.ToArray (typeof (byte));
					break;
				case OdbcType.Binary :
					bufsize = col.MaxLength;
					buffer = new byte [bufsize];
					long read = GetBytes (i, 0, buffer, 0, bufsize);
					ret = OdbcReturn.Success;
					DataValue = buffer;
					break;
				default:
					bufsize = 255;
					buffer = new byte[bufsize];
					ret = libodbc.SQLGetData (hstmt, ColIndex, SQL_C_TYPE.CHAR, buffer, bufsize, ref outsize);
					if (outsize != (int) OdbcLengthIndicator.NullData)
						if (! (ret == OdbcReturn.SuccessWithInfo
						       && outsize == (int) OdbcLengthIndicator.NoTotal))
							DataValue = Encoding.Default.GetString (buffer, 0, outsize);
					break;
				}

				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo) && (ret!=OdbcReturn.NoData))
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, hstmt);

				if (outsize == -1) // This means SQL_NULL_DATA 
					col.Value = DBNull.Value;
				else
					col.Value = DataValue;
			}
			return col.Value;
		}
		
		public 
#if NET_2_0
		override
#endif // NET_2_0
		int GetValues (object [] values)
		{
			int numValues = 0;

			if (IsClosed)
				throw new InvalidOperationException ("The reader is closed.");
			if (currentRow == -1)
				throw new InvalidOperationException ("No data available.");

			// copy values
			for (int i = 0; i < values.Length; i++) {
				if (i < FieldCount) {
					values [i] = GetValue (i);
				} else {
					values [i] = null;
				}
			}

			// get number of object instances in array
			if (values.Length < FieldCount)
				numValues = values.Length;
			else if (values.Length == FieldCount)
				numValues = FieldCount;
			else
				numValues = FieldCount;

			return numValues;
		}

#if ONLY_1_1
		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}
#endif // ONLY_1_1

#if NET_2_0
		public override IEnumerator GetEnumerator ()
		{
			return new DbEnumerator (this);
		}
#endif

#if NET_2_0
		protected override
#endif
		void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (disposing) {
				// dispose managed resources
				Close ();
			}

			command = null;
			cols = null;
			_dataTableSchema = null;
			disposed = true;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool IsDBNull (int i)
		{
			return (GetValue (i) is DBNull);
		}

		/// <remarks>
		/// Move to the next result set.
		/// </remarks>
		public
#if NET_2_0
		override
#endif // NET_2_0
		bool NextResult ()
		{
			OdbcReturn ret = OdbcReturn.Success;
			ret = libodbc.SQLMoreResults (hstmt);
			if (ret == OdbcReturn.Success) {
				short colcount = 0;
				libodbc.SQLNumResultCols (hstmt, ref colcount);
				cols = new OdbcColumn [colcount];
				_dataTableSchema = null; // force fresh creation
				GetColumns ();
			}
			return (ret == OdbcReturn.Success);
		}

		/// <remarks>
		/// Load the next row in the current result set.
		/// </remarks>
		private bool NextRow ()
		{
			OdbcReturn ret = libodbc.SQLFetch (hstmt);
			if (ret != OdbcReturn.Success)
				currentRow = -1;
			else
				currentRow++;

			// Clear cached values from last record
			foreach (OdbcColumn col in cols) {
				if (col != null)
					col.Value = null;
			}
			return (ret == OdbcReturn.Success);
		}

		private int GetColumnAttribute (int column, FieldIdentifier fieldId)
		{
			OdbcReturn ret = OdbcReturn.Error;
			byte [] buffer = new byte [255];
			short outsize = 0;
			int val = 0;
			ret = libodbc.SQLColAttribute (hstmt, (short)column, fieldId, 
							buffer, (short)buffer.Length, 
							ref outsize, ref val);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw Connection.CreateOdbcException (
					OdbcHandleType.Stmt, hstmt);
			return val;
		}

		private string GetColumnAttributeStr (int column, FieldIdentifier fieldId)
		{
			OdbcReturn ret = OdbcReturn.Error;
			byte [] buffer = new byte [255];
			short outsize = 0;
			int val = 0;
			ret = libodbc.SQLColAttribute (hstmt, (short)column, fieldId,
							buffer, (short)buffer.Length,
							ref outsize, ref val);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw Connection.CreateOdbcException (
					OdbcHandleType.Stmt, hstmt);
			string value = string.Empty;
			if (outsize > 0)
				value = Encoding.Unicode.GetString (buffer, 0, outsize);
			return value;
		}

		private string [] GetPrimaryKeys (string catalog, string schema, string table)
		{
			if (cols.Length <= 0)
				return new string [0];

			ArrayList keys = null;
			try {
				keys = GetPrimaryKeysBySQLPrimaryKey (catalog, schema, table);
			} catch (OdbcException) {
				try {
					keys = GetPrimaryKeysBySQLStatistics (catalog, schema, table);
				} catch (OdbcException) {
				}
			}
			if (keys == null)
				return null;
			keys.Sort ();
			return (string []) keys.ToArray (typeof (string));
		}

		private ArrayList GetPrimaryKeysBySQLPrimaryKey (string catalog, string schema, string table)
		{
			ArrayList keys = new ArrayList ();
			IntPtr handle = IntPtr.Zero;
			OdbcReturn ret;
			try {
				ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, 
							   command.Connection.hDbc, ref handle);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
					throw Connection.CreateOdbcException (
						OdbcHandleType.Dbc, Connection.hDbc);

				ret = libodbc.SQLPrimaryKeys (handle, catalog, -3,
					schema, -3, table, -3);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

				int length = 0;
				byte [] primaryKey = new byte [255];

				ret = libodbc.SQLBindCol (handle, 4, SQL_C_TYPE.CHAR, primaryKey, primaryKey.Length, ref length);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

				while (true) {
					ret = libodbc.SQLFetch (handle);
					if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
						break;
					string pkey = Encoding.Default.GetString (primaryKey, 0, length);
					keys.Add (pkey);
				}
			} finally {
				if (handle != IntPtr.Zero) {
					ret = libodbc.SQLFreeStmt (handle, libodbc.SQLFreeStmtOptions.Close);
					if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
						throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

					ret = libodbc.SQLFreeHandle( (ushort) OdbcHandleType.Stmt, handle);
					if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
						throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);
				}
			}
			return keys;
		}
		
		private unsafe ArrayList GetPrimaryKeysBySQLStatistics (string catalog, string schema, string table)
		{
			ArrayList keys = new ArrayList ();
			IntPtr handle = IntPtr.Zero;
			OdbcReturn ret;
			try {
				ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, 
							   command.Connection.hDbc, ref handle);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
					throw Connection.CreateOdbcException (
						OdbcHandleType.Dbc, Connection.hDbc);

				ret = libodbc.SQLStatistics (handle, catalog, -3,  
							     schema, -3, 
							     table, -3,
							     libodbc.SQL_INDEX_UNIQUE,
							     libodbc.SQL_QUICK);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

				// NON_UNIQUE
				int  nonUniqueLength = 0;
				short nonUnique = libodbc.SQL_FALSE;
				ret = libodbc.SQLBindCol (handle, 4, SQL_C_TYPE.SHORT, ref nonUnique, sizeof (short), ref nonUniqueLength);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

				// COLUMN_NAME
				int length = 0;
				byte [] colName = new byte [255];
				ret = libodbc.SQLBindCol (handle, 9, SQL_C_TYPE.CHAR, colName, colName.Length, ref length);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);
			
				while (true) {
					ret = libodbc.SQLFetch (handle);
					if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
						break;
					if (nonUnique == libodbc.SQL_TRUE) {
						string pkey = Encoding.Default.GetString (colName, 0, length);
						keys.Add (pkey);
						break;
					}
				}
			} finally {
				if (handle != IntPtr.Zero) {
					ret = libodbc.SQLFreeStmt (handle, libodbc.SQLFreeStmtOptions.Close);
					if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
						throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);

					ret = libodbc.SQLFreeHandle ((ushort) OdbcHandleType.Stmt, handle);
					if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo))
						throw Connection.CreateOdbcException (OdbcHandleType.Stmt, handle);
				}
			}
			return keys;
		}
		
		public
#if NET_2_0
		override
#endif // NET_2_0
		bool Read ()
		{
			return NextRow ();
		}

		static string RemoveTrailingNullChar (string value)
		{
			return value.TrimEnd ('\0');
		}

		#endregion
	}
}
