#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// Copyright 2003 By Christopher Bockner
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;

namespace DB2ClientCS
{
	/// <summary>
	/// Summary description for DB2ClientDataReader.
	/// DB2ClientDataReader. 
	/// </summary>
	unsafe public class DB2ClientDataReader : IDataReader
	{
		internal DataTable rs;			//Our result set is a datatable
		internal DB2ClientConnection db2Conn; //The connection we're working with
		internal IntPtr hwndStmt;		//The statement handle returning the results
		private int row=-1;				//Row pointer
		private int numCols=0;

		#region Constructors and destructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="con"></Connection object to DB2>
		/// <param name="com"></Command object>
		internal DB2ClientDataReader(DB2ClientConnection con, DB2ClientCommand com)
		{
			db2Conn = con;
			hwndStmt = com.statementHandle;    //We have access to the results through the statement handle

			short sqlRet;

			DB2ClientUtils util = new DB2ClientUtils();
			rs = new DataTable();
			
			sqlRet = DB2ClientPrototypes.SQLNumResultCols(hwndStmt, ref numCols);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLNumResultCols");

			IntPtr[] dbVals = new IntPtr[numCols];
			IntPtr[] sqlLen_or_IndPtr = new IntPtr[numCols];

			PrepareResults(dbVals, sqlLen_or_IndPtr);
			FetchResults(dbVals, sqlLen_or_IndPtr, rs);


		}
		/// <summary>
		/// Constructor for use with prepared statements
		/// </summary>
		/// 
		internal DB2ClientDataReader(DB2ClientConnection con, DB2ClientCommand com, bool prepared)
		{
			db2Conn = con;
			hwndStmt = com.statementHandle;    //We have access to the results through the statement handle

			short sqlRet;

			DB2ClientUtils util = new DB2ClientUtils();
			rs = new DataTable();
			
			sqlRet = DB2ClientPrototypes.SQLNumResultCols(hwndStmt, ref numCols);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLNumResultCols");

			IntPtr[] dbVals = new IntPtr[numCols];
			IntPtr[] sqlLen_or_IndPtr = new IntPtr[numCols];

			PrepareResults(dbVals, sqlLen_or_IndPtr);
		}

		public void Dispose()
		{
			Close();
		}
		#endregion
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
		private int recordsAffected = 0;
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
		///
		///
		public void Close()
		{
			if (rs != null) 
			{
				recordsAffected = rs.Rows.Count;
				rs.Dispose();
				rs = null;
				isClosed=true;
			}
		}
		#endregion
		#region GetSchemaTable 
		///
		/// We'll return an empty table for now...ughh this one will be tedious to write
		/// 
		public DataTable GetSchemaTable()
		{
			throw new DB2ClientException ("TBD");
		}
		#endregion
		#region NextResult 
		///
		/// Ummm is this related to SQLBulkOperations stuff..?
		/// 
		public bool NextResult()
		{
			throw new DB2ClientException("To be done");

		//Deferring the meat of this until the batch stuff is implemented
		}
		#endregion
		#region Read
		///
		/// Apparently, this function does nothing other than tell you if you can move to the 
		/// next row in the resultset.  I have to move the fetching stuff elswhere...
		/// 
		public bool Read()
		{
			if (isClosed) return false;
			row++;
			//do something with the fetched data now...
			if(row < rs.Rows.Count)
				return true;
			else
				return false;
			
		}
		#endregion

		#region Describe/Bind/Fetch functions
		///
		///Broke these out so that we can use different paths for Immediate executions and Prepared executions
		/// <summary>
		/// Does the describe and bind steps for the query result set.  Called for both immediate and prepared queries. 
		/// </summary>
		private void PrepareResults(IntPtr[] dbVals, IntPtr[] sqlLen_or_IndPtr)
		{
			short sqlRet;
			StringBuilder colName = new StringBuilder(18);
			short colNameMaxLength=18;
			IntPtr colNameLength=IntPtr.Zero;
			IntPtr sqlDataType=IntPtr.Zero;
			IntPtr colSize=IntPtr.Zero;
			IntPtr scale=IntPtr.Zero;
			IntPtr nullable=IntPtr.Zero;
			DB2ClientUtils util = new DB2ClientUtils();
			for (ushort i=1; i<=numCols; i++) 
			{
				sqlRet = DB2ClientPrototypes.SQLDescribeCol(hwndStmt, i, colName, colNameMaxLength, colNameLength, ref sqlDataType, ref colSize, ref scale, ref nullable);
				util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLDescribeCol");
				///At this point I have the data type information as well, but for now I will insert the data as
				///Ansi strings and see how it goes.  Maybe we can speed things up later...
				///
				rs.Columns.Add(colName.ToString());

				sqlLen_or_IndPtr[i-1] = new IntPtr();
				dbVals[i-1] = Marshal.AllocHGlobal(colSize.ToInt32()+1);

				try 
				{
					switch ((int)sqlDataType) 
					{
						case DB2ClientConstants.SQL_DECIMAL:	//These types are treated as SQL_C_CHAR for binding purposes
						case DB2ClientConstants.SQL_TYPE_DATE:
						case DB2ClientConstants.SQL_TYPE_TIME:
						case DB2ClientConstants.SQL_TYPE_TIMESTAMP:
						case DB2ClientConstants.SQL_VARCHAR:
							sqlRet = DB2ClientPrototypes.SQLBindCol(hwndStmt, i, DB2ClientConstants.SQL_C_CHAR,  dbVals[i-1],(short)colSize.ToInt32()+1, ref sqlLen_or_IndPtr[i-1]);
							break;
						default:
							sqlRet = DB2ClientPrototypes.SQLBindCol(hwndStmt, i, (short)sqlDataType.ToInt32(), dbVals[i-1],(short)colSize.ToInt32()+1, ref sqlLen_or_IndPtr[i-1]);
							break;
					}
					util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLBindCol");
				}
				catch(DB2ClientException e) 
				{
					System.Console.Write(e.Message);
				}
				isClosed = false;
			}
		}
/// <summary>
/// FetchResults does  what it says.
/// </summary>
/// <param name="dbVals"></param>
/// <param name="sqlLen_or_IndPtr"></param>
/// <param name="rs"></param>
		private void FetchResults(IntPtr[] dbVals, IntPtr[] sqlLen_or_IndPtr, DataTable rs) 
		{
			short sqlRet = 0;
			DB2ClientUtils util = new DB2ClientUtils();

			sqlRet = DB2ClientPrototypes.SQLFetch(hwndStmt);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLFetch 1");

			while(sqlRet != DB2ClientConstants.SQL_NO_DATA_FOUND)
			{
				DataRow newRow = rs.NewRow();
				for (short y=1;y<=numCols;y++) 
					newRow[y-1] = Marshal.PtrToStringAnsi(dbVals[y-1]);

				rs.Rows.Add(newRow);
				sqlRet = DB2ClientPrototypes.SQLFetch(hwndStmt);
				util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_STMT, hwndStmt, "DB2ClientDataReader - SQLFetch 2");
			}		
			for (int n=0;n<numCols;n++)
				Marshal.FreeHGlobal(dbVals[n]);
		}
		#endregion

		#region IDataRecord Interface
		///Code for the IDataRecord interface
		///
		#region FieldCount
		///
		///
		private int fieldCount = -1;
		public int FieldCount
		{
			get
			{
				if (IsClosed)
					fieldCount = 0;
				else
					fieldCount = rs.Columns.Count;
				return fieldCount;
			}
		}
		#endregion
		#region Item accessors
		public object this[string name]
		{
			get
			{
				return rs.Rows[row][name];
			}
		}
		public object this[int col]
		{
			get
			{
				return rs.Rows[row][col];
			}
		}
		#endregion
		#region GetBoolean method
		///
		///Use the Convert class for all of these returns
		///
		public bool GetBoolean(int col)
		{
			return Convert.ToBoolean(this[col]);
		}
		#endregion
		#region GetByte
		///
		///GetByte
		///
		public byte GetByte(int col)
		{
			return Convert.ToByte(this[col]);
		}
		#endregion
		#region GetBytes
		///
		///  GetBytes, return a stream of bytes
		///
		public long GetBytes(int col, long fieldOffset, byte[] buffer, int bufferOffset, int length)
		{
			//Hmm... How shall we deal with this one?  
			return 0;
		}
		#endregion
		#region GetChar
		///
		///GetChar, return column as a char
		///
		public char GetChar(int col)
		{
			return Convert.ToChar(this[col]);
		}
		#endregion
		#region GetChars
		///
		///GetChars, returns char array
		///
		public long GetChars(int col, long fieldOffset, char[] buffer, int bufferOffset, int length)
		{
			//Again, not sure how I'll deal with this just yet
			return 0;
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
			throw new DB2ClientException("Not yet supported.");
		}
		#endregion
		#region GetDataTypeName
		///
		///GetDataTypeName return the type of data
		///
		public string GetDataTypeName(int col)
		{
			//I could check the meta data as a starting point for this one, but until I implement 
			//returning the result sets, I'm not exactly sure what info I'll have, so this function
			//waits until then...
			throw new DB2ClientException("Not yet implemented");
		}
		#endregion
		#region GetDateTime
		///
		/// GetDateTime method
		/// 
		public DateTime GetDateTime(int col)
		{
			return Convert.ToDateTime(this[col]);
		}
		#endregion
		#region GetDecimal
		///
		///GetDecimal method
		///
		public decimal GetDecimal(int col)
		{
			return Convert.ToDecimal(this[col]);
		}
		#endregion
		#region GetDouble 
		///
		/// GetDouble 
		/// 
		public double GetDouble(int col)
		{
			return Convert.ToDouble(this[col]);
		}
		#endregion
		#region GetFieldType
		///
		/// Type GetFieldType
		///
		public Type GetFieldType(int col)
		{
			//Again need more research here
			return typeof(int);
		}
		#endregion
		#region GetFloat
		///
		/// GetFloat
		/// 
		public float GetFloat(int col)
		{
			return (float) Convert.ToDouble(this[col].ToString(),new CultureInfo("en-US").NumberFormat);
		}
		#endregion
		#region GetGuid
		///
		/// GetGuid
		/// 
		public Guid GetGuid(int col)
		{
			// a Guid is a 128 bit unique value.  Could be like a GENERATE UNIQUE in DB2
			// as usual, need more research
			throw new DB2ClientException("TBD");
		}
		#endregion
		#region The GetInt?? series
		///
		///GetInt16
		///
		public short GetInt16(int col)
		{
			return Convert.ToInt16(this[col]);
		}
		///
		///GetInt32
		///
		public int GetInt32(int col)
		{
			return Convert.ToInt32(this[col]);
		}
		///
		///GetInt64
		///
		public long GetInt64(int col)
		{
			return Convert.ToInt64(this[col]);
		}
		#endregion
		#region GetName
		///
		///GetName, returns the name of the field
		///
		public string GetName(int col)
		{
			return (rs.Columns[col].ColumnName);
		}
		#endregion
		#region GetOrdinal
		///
		/// GetOrdinal, return the index of the named column
		/// 
		public int GetOrdinal(string name)
		{
			return rs.Columns[name].Ordinal;
		}
		#endregion
		#region GetString
		///
		/// GetString returns a string
		/// 
		public string GetString(int col)
		{
			return Convert.ToString(this[col]);
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
			int numCols = FieldCount;
			if (values.Length<numCols)
				throw new DB2ClientException("GetValues argument too small for number of columns in row.");
			for (int i = 0; i<=numCols; i++)
		 	  values[i] = this[i];
			return numCols;
		}
		#endregion
		#region IsDBNull
		///
		/// IsDBNull Is the column null
		/// 
		public bool IsDBNull(int col)
		{
			//Proper implementation once I get the SQLDescribe/SQLBind/SQLFetch stuff in place
			return false;
		}
		#endregion

		#endregion  ///For IDataRecord
	}

}
#endregion