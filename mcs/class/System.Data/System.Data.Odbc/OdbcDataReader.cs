//
// System.Data.Odbc.OdbcDataReader
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//   Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Brian Ritchie, 2002
// Copyright (C) Daniel Morgan, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	public sealed class OdbcDataReader : MarshalByRefObject, IDataReader, IDisposable, IDataRecord, IEnumerable
	{
		#region Fields
		
		private OdbcCommand command;
		private bool open;
		private int currentRow;
		private OdbcColumn[] cols;
		private IntPtr hstmt;
		private CommandBehavior behavior;

		#endregion

		#region Constructors

		internal OdbcDataReader (OdbcCommand command, CommandBehavior behavior) 
		{
			this.command = command;
			this.behavior=behavior;
			this.command.Connection.DataReader = this;
			open = true;
			currentRow = -1;
			hstmt=command.hStmt;
			// Init columns array;
			short colcount=0;
			libodbc.SQLNumResultCols(hstmt, ref colcount);
			cols=new OdbcColumn[colcount];
		}

		#endregion

		#region Properties

		public int Depth {
			get {
				return 0; // no nested selects supported
			}
		}

		public int FieldCount {
			get {
				return cols.Length;
			}
		}

		public bool IsClosed {
			get {
				return !open;
			}
		}

		public object this[string name] {
			get {
				int pos;

				if (currentRow == -1)
					throw new InvalidOperationException ();

				pos = ColIndex(name);
				
				if (pos == -1)
					throw new IndexOutOfRangeException ();

				return this[pos];
			}
		}

		public object this[int index] {
			get {
				return (object) GetValue (index);
			}
		}

		public int RecordsAffected {
			get {
				return -1;
			}
		}

		#endregion

		#region Methods
		
		private int ColIndex(string colname)
		{
			int i=0;
			foreach (OdbcColumn col in cols)
			{
				if (col.ColumnName==colname)
					return i;
				i++;
			}
			return 0;
		}

		// Dynamically load column descriptions as needed.
		private OdbcColumn GetColumn(int ordinal)
		{
			if (cols[ordinal]==null)
			{
				short bufsize=255;
				byte[] colname_buffer=new byte[bufsize];
				string colname;
				short colname_size=0;
				short ColSize=0, DecDigits=0, Nullable=0, dt=0;
				OdbcReturn ret=libodbc.SQLDescribeCol(hstmt, Convert.ToUInt16(ordinal+1), 
					colname_buffer, bufsize, ref colname_size, ref dt, ref ColSize, 
					ref DecDigits, ref Nullable);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLDescribeCol",OdbcHandleType.Stmt,hstmt));
				colname=System.Text.Encoding.Default.GetString(colname_buffer);
				colname=colname.Replace((char) 0,' ').Trim();
				OdbcColumn c=new OdbcColumn(colname, (OdbcType) dt);
				c.AllowDBNull=(Nullable!=0);
				c.Digits=DecDigits;
				if (c.IsStringType)
					c.MaxLength=ColSize;
				cols[ordinal]=c;
			}
			return cols[ordinal];
		}
	
		public void Close ()
		{
			// libodbc.SQLFreeHandle((ushort) OdbcHandleType.Stmt, hstmt);
		
			OdbcReturn ret=libodbc.SQLCloseCursor(hstmt);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLCloseCursor",OdbcHandleType.Stmt,hstmt));
	
			open = false;
			currentRow = -1;

			this.command.Connection.DataReader = null;

			if ((behavior & CommandBehavior.CloseConnection)==CommandBehavior.CloseConnection)
				this.command.Connection.Close();
		}

		~OdbcDataReader ()
		{
			if (open)
				Close ();
		}

		public bool GetBoolean (int ordinal)
		{
			return (bool) GetValue(ordinal);
		}

		[MonoTODO]
		public byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes (int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars (int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public OdbcDataReader GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public string GetDataTypeName (int index)
		{
			return GetColumn(index).OdbcType.ToString();
		}

		public DateTime GetDateTime (int ordinal)
		{
			return (DateTime) GetValue(ordinal);
		}

		[MonoTODO]
		public decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public double GetDouble (int ordinal)
		{
			return (double) GetValue(ordinal);
		}

		public Type GetFieldType (int index)
		{
			return GetColumn(index).DataType;
		}

		public float GetFloat (int ordinal)
		{
			return (float) GetValue(ordinal);
		}

		[MonoTODO]
		public Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public short GetInt16 (int ordinal)
		{
			return (short) GetValue(ordinal);
		}

		public int GetInt32 (int ordinal)
		{
			return (int) GetValue(ordinal);
		}

		public long GetInt64 (int ordinal)
		{
			return (long) GetValue(ordinal);
		}

		public string GetName (int index)
		{
			if (currentRow == -1)
				return null;
			return GetColumn(index).ColumnName;
		}

		public int GetOrdinal (string name)
		{
			if (currentRow == -1)
				throw new IndexOutOfRangeException ();

			int i=ColIndex(name);

			if (i==-1)
				throw new IndexOutOfRangeException ();
			else
				return i;
		}

		[MonoTODO]
		public DataTable GetSchemaTable() 
		{	

			DataTable dataTableSchema = null;
			// Only Results from SQL SELECT Queries 
			// get a DataTable for schema of the result
			// otherwise, DataTable is null reference
			if(cols.Length > 0) 
			{
				
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
								
				for (int i = 0; i < cols.Length; i += 1 ) 
				{
					OdbcColumn col=GetColumn(i);
					//Console.WriteLine("{0}:{1}:{2}",col.ColumnName,col.DataType,col.OdbcType);

					schemaRow = dataTableSchema.NewRow ();
					dataTableSchema.Rows.Add (schemaRow);
										
					schemaRow["ColumnName"] = col.ColumnName;
					schemaRow["ColumnOrdinal"] = i + 1;
					
					schemaRow["ColumnSize"] = col.MaxLength;
					schemaRow["NumericPrecision"] = 0;
					schemaRow["NumericScale"] = 0;
					// TODO: need to get KeyInfo
					schemaRow["IsUnique"] = false;
					schemaRow["IsKey"] = DBNull.Value;					

					schemaRow["BaseCatalogName"] = "";				
					schemaRow["BaseColumnName"] = col.ColumnName;
					schemaRow["BaseSchemaName"] = "";
					schemaRow["BaseTableName"] = "";
					schemaRow["DataType"] = col.DataType;

					schemaRow["AllowDBNull"] = col.AllowDBNull;
					
					schemaRow["ProviderType"] = (int) col.OdbcType;
					// TODO: all of these
					schemaRow["IsAliased"] = false;
					schemaRow["IsExpression"] = false;
					schemaRow["IsIdentity"] = false;
					schemaRow["IsAutoIncrement"] = false;
					schemaRow["IsRowVersion"] = false;
					schemaRow["IsHidden"] = false;
					schemaRow["IsLong"] = false;
					schemaRow["IsReadOnly"] = false;
					
					// FIXME: according to Brian, 
					// this does not work on MS .NET
					// however, we need it for Mono 
					// for now
					schemaRow.AcceptChanges();
					
				}

			}
			dataTableSchema.AcceptChanges();
			return dataTableSchema;
		}

		public string GetString (int ordinal)
		{
			return (string) GetValue(ordinal);
		}

		[MonoTODO]
		public TimeSpan GetTimeSpan (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public object GetValue (int ordinal)
		{
			if (currentRow == -1)
				throw new IndexOutOfRangeException ();

			if (ordinal>cols.Length-1 || ordinal<0)
				throw new IndexOutOfRangeException ();

			OdbcReturn ret;
			int outsize=0, bufsize;
			byte[] buffer;
			OdbcColumn col=GetColumn(ordinal);
			object DataValue=null;
			ushort ColIndex=Convert.ToUInt16(ordinal+1);

			// Check cached values
			if (col.Value==null)
			{

				// odbc help file
				// mk:@MSITStore:C:\program%20files\Microsoft%20Data%20Access%20SDK\Docs\odbc.chm::/htm/odbcc_data_types.htm
				switch (col.OdbcType)
				{
					case OdbcType.Decimal:
						bufsize=50;
						buffer=new byte[bufsize];  // According to sqlext.h, use SQL_CHAR for decimal
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.Char, buffer, bufsize, ref outsize);
						if (outsize!=-1)
							DataValue=Decimal.Parse(System.Text.Encoding.Default.GetString(buffer));
						break;
					case OdbcType.TinyInt:
						short short_data=0;
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.TinyInt, ref short_data, 0, ref outsize);
						DataValue=short_data;
						break;
					case OdbcType.Int:
						int int_data=0;
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.Int, ref int_data, 0, ref outsize);
						DataValue=int_data;
						break;
					case OdbcType.BigInt:
						long long_data=0;
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.BigInt, ref long_data, 0, ref outsize);
						DataValue=long_data;
						break;
					case OdbcType.NVarChar:
						bufsize=col.MaxLength*2+1; // Unicode is double byte
						buffer=new byte[bufsize];
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.NVarChar, buffer, bufsize, ref outsize);
						if (outsize!=-1)
							DataValue=System.Text.Encoding.Unicode.GetString(buffer,0,outsize);
						break;
					case OdbcType.VarChar:
						bufsize=col.MaxLength+1;
						buffer=new byte[bufsize];  // According to sqlext.h, use SQL_CHAR for both char and varchar
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.Char, buffer, bufsize, ref outsize);
						if (outsize!=-1)
							DataValue=System.Text.Encoding.Default.GetString(buffer,0,outsize);
						break;
					case OdbcType.Real:
						float float_data=0;
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.Real, ref float_data, 0, ref outsize);
						DataValue=float_data;
						break;
					case OdbcType.Timestamp:
					case OdbcType.DateTime:
						OdbcTimestamp ts_data=new OdbcTimestamp();
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.DateTime, ref ts_data, 0, ref outsize);
						if (outsize!=-1) // This means SQL_NULL_DATA 
							DataValue=new DateTime(ts_data.year,ts_data.month,ts_data.day,ts_data.hour,
								ts_data.minute,ts_data.second,Convert.ToInt32(ts_data.fraction));
						break;
					default:
						//Console.WriteLine("Fetching unsupported data type as string: "+col.OdbcType.ToString());
						bufsize=255;
						buffer=new byte[bufsize];
						ret=libodbc.SQLGetData(hstmt, ColIndex, OdbcType.Char, buffer, bufsize, ref outsize);
						DataValue=System.Text.Encoding.Default.GetString(buffer);
						break;
				}

				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLGetData",OdbcHandleType.Stmt,hstmt));

				if (outsize==-1) // This means SQL_NULL_DATA 
					col.Value=DBNull.Value;
				else
					col.Value=DataValue;
			}
			return col.Value;
		}
		
		public int GetValues (object[] values)
		{
			int numValues = 0;

			// copy values
			for (int i = 0; i < values.Length; i++) {
				if (i < FieldCount) {
					values[i] = GetValue(i);
				}
				else {
					values[i] = null;
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

		[MonoTODO]
		IDataReader IDataRecord.GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int ordinal)
		{
			return (GetValue(ordinal) is DBNull);
		}

		public bool NextResult ()
		{
			OdbcReturn ret=libodbc.SQLFetch(hstmt);
			if (ret!=OdbcReturn.Success)
				currentRow=-1;
			else
				currentRow++;
			// Clear cached values from last record
			foreach (OdbcColumn col in cols)
			{
				if (col!=null)
					col.Value=null;
			}
			return (ret==OdbcReturn.Success);
		}

		public bool Read ()
		{
			return NextResult();
		}

		#endregion
	}
}
