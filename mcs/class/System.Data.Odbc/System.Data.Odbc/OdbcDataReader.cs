//
// System.Data.Odbc.OdbcDataReader
//
// Author:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
//

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
	public sealed class OdbcDataReader : MarshalByRefObject, IDataReader, 
IDisposable, IDataRecord, IEnumerable
	{
		#region Fields

		private OdbcCommand command;
		private bool open;
		private int currentRow;
		private DataColumn[] cols;
		private int hstmt;

		#endregion

		#region Constructors

		internal OdbcDataReader (OdbcCommand command)
		{
			this.command = command;
			this.command.Connection.DataReader = this;
			open = true;
			currentRow = -1;
			hstmt=command.hStmt;
			LoadColumns();
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

		public DataColumn[] Columns
		{
			get {
				return cols;
			}
		}

		public object this[string name] {
			get {
				ushort pos;

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
				return (object) GetODBCData (index);
			}
		}

		public int RecordsAffected {
			get {
				return -1;
			}
		}

		#endregion

		#region Methods

		private Type SQLTypeToCILType(short DataType)
		{
			switch (DataType)
			{
				case 12:
				case 1:
					return typeof(string);
				case 4:
					return typeof(int);
				case 5:
					return typeof(short);
				case 2:
				case 3:
				case 6:
				case 7:
				case 8:
					return typeof(float);
				case 90:
				case 91:
				case 92:
				case 9:
					return typeof(DateTime);
				default:
					Console.WriteLine("WARNING: Unknown type {0}", DataType);
					return typeof(string);
			}
		}

		private short CILTypeToSQLType(Type type)
		{
			if (type==typeof(int))
				return 4;
			else if (type==typeof(string))
				return 12;
			else
				return 12;
		}

		private void LoadColumns()
		{
			ArrayList colsArray=new ArrayList();
			short colcount=0;
			short bufsize=255;
			byte[] colname_buffer=new byte[bufsize];
			string colname;
			short colname_size=0;
			short DataType=0, ColSize=0, DecDigits=0, Nullable=0;

			libodbc.SQLNumResultCols(hstmt, ref colcount);
			for (ushort i=1;i<=colcount;i++)
			{
				libodbc.SQLDescribeCol(hstmt, i, colname_buffer, bufsize, ref 
colname_size, ref DataType, ref ColSize, ref DecDigits, ref Nullable);
				colname=System.Text.Encoding.Default.GetString(colname_buffer);
				DataColumn c=new DataColumn(colname, SQLTypeToCILType(DataType));
				c.AllowDBNull=(Nullable!=0);
				if (c.DataType==typeof(string))
					c.MaxLength=ColSize;
				colsArray.Add(c);
			}
			cols=(DataColumn[]) colsArray.ToArray(typeof(DataColumn));
		}

		private ushort ColIndex(string colname)
		{
			ushort i=0;
			foreach (DataColumn col in cols)
			{
				if (col.ColumnName==colname)
					return i;
				i++;
			}
			return 0;
		}

		private object GetODBCData(int colindex)
		{
			return GetODBCData(Convert.ToUInt16(colindex));
		}

		private object GetODBCData(ushort colindex)
		{
			OdbcReturn ret;
			int outsize=0;
			DataColumn col=cols[colindex];
			colindex+=1;
			if (col.DataType==typeof(int))
			{
				int data=0;
				ret=libodbc.SQLGetData(hstmt, colindex, 4, ref data, 0, ref outsize);
				libodbc.DisplayError("SQLGetData(int)",ret);
				return data;
			}
			else if (col.DataType==typeof(string))
			{
				byte[] strbuffer=new byte[255];
				ret=libodbc.SQLGetData(hstmt, colindex, 1, strbuffer, 255, ref outsize);
				libodbc.DisplayError("SQLGetData("+col.ColumnName+","+colindex.ToString()+")",ret);
				return System.Text.Encoding.Default.GetString(strbuffer);
			}
			else if (col.DataType==typeof(float))
			{
				float data=0;
				ret=libodbc.SQLGetData(hstmt, colindex, 7, ref data, 0, ref outsize);
				return data;
			}
			else if (col.DataType==typeof(DateTime))
			{
				OdbcTimestamp data=new OdbcTimestamp();
				ret=libodbc.SQLGetData(hstmt, colindex, 91, ref data, 0, ref outsize);
				return new 
DateTime(data.year,data.month,data.day,data.hour,data.minute,data.second,Convert.ToInt32(data.fraction));
			}
			else return "";
		}


		public void Close ()
		{
			// libodbc.SQLFreeHandle((ushort) OdbcHandleType.Stmt, hstmt);

			OdbcReturn ret=libodbc.SQLCloseCursor(hstmt);
			libodbc.DisplayError("SQLCancel",ret);

			open = false;
			currentRow = -1;

			this.command.Connection.DataReader = null;
		}

		~OdbcDataReader ()
		{
			if (open)
				Close ();
		}

		public bool GetBoolean (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public byte GetByte (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetBytes (int ordinal, long dataIndex, byte[] buffer, int 
bufferIndex, int length)
		{
			throw new NotImplementedException ();
		}

		public char GetChar (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public long GetChars (int ordinal, long dataIndex, char[] buffer, int 
bufferIndex, int length)
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
			return "";
		}

		public DateTime GetDateTime (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public decimal GetDecimal (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public double GetDouble (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Type GetFieldType (int index)
		{
			throw new NotImplementedException ();
		}

		public float GetFloat (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Guid GetGuid (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public short GetInt16 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public int GetInt32 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public long GetInt64 (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public string GetName (int index)
		{
			if (currentRow == -1)
				return null;
			return cols[index].ColumnName;
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

		public DataTable GetSchemaTable ()
		{
			DataTable table = new DataTable ();

			// FIXME: implement
			return table;
		}

		public string GetString (int ordinal)
		{
			throw new NotImplementedException ();
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

			return (object) GetODBCData(ordinal);
		}

		[MonoTODO]
		public int GetValues (object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IDataReader IDataRecord.GetData (int ordinal)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		public bool IsDBNull (int ordinal)
		{
			throw new NotImplementedException ();
		}

		public bool NextResult ()
		{
			OdbcReturn ret=libodbc.SQLFetch(hstmt);
			return (ret==OdbcReturn.Success);
		}

		public bool Read ()
		{
			return NextResult();
		}

		#endregion
	}
}

