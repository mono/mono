using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;

namespace System.Data.Db2Client
{
	/// <summary>
	/// Summary description for Db2ClientDataReader.
	/// Db2ClientDataReader. 
	/// </summary>
	public class Db2DataReader : MarshalByRefObject, IDataReader
	{
		internal DataTable _resultSet;			
		internal Db2Connection db2Conn; 
		internal IntPtr hwndStmt;
		private int row=-1;
		private int numCols=0;

		internal IntPtr Ptr = IntPtr.Zero;
		BLOBWrapperCollection _blobs = new BLOBWrapperCollection();
		
		private DataTable _schemaTable ;
		private DataTable _resultInfo;

		IntPtr intPtr = IntPtr.Zero;
		IntPtr[] ipTarget;
		
		#region Constructors and destructors
		/// <summary>
		/// 
		/// </summary>
		/// <param name="con"></Connection object to Db2>
		/// <param name="com"></Command object>
		internal Db2DataReader(Db2Connection con, Db2Command com)
		{
			_schemaTable = BuildNewSchemaTable();
			db2Conn = con;
			hwndStmt = com.statementHandle;    //We have access to the results through the statement handle

			short sqlRet;
			
			_resultSet = new DataTable();
			
			sqlRet = Db2CLIWrapper.SQLNumResultCols(hwndStmt, ref numCols);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLNumResultCols");

			//IntPtr[] sqlLen_or_IndPtr = new IntPtr[numCols];
			//ipTarget = new IntPtr[numCols];
			
			_schemaTable = BuildNewSchemaTable();

			PrepareResults(/*sqlLen_or_IndPtr*/);
			//FetchResults(sqlLen_or_IndPtr, _resultSet);
			isClosed = false;
		}
		/// <summary>
		/// Constructor for use with prepared statements
		/// </summary>
		/// 
		internal Db2DataReader(Db2Connection con, Db2Command com, bool prepared)
		{
			db2Conn = con;
			hwndStmt = com.statementHandle;    //We have access to the results through the statement handle

			short sqlRet;
			
			sqlRet = Db2CLIWrapper.SQLNumResultCols(hwndStmt, ref numCols);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLNumResultCols");

			
			//IntPtr[] sqlLen_or_IndPtr = new IntPtr[numCols];
			//ipTarget = new IntPtr[numCols];

			PrepareResults(/*sqlLen_or_IndPtr*/);
			isClosed = false;
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
		///with Db2.
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
			if (_resultSet != null) 
			{
				recordsAffected = _resultSet.Rows.Count;
				_resultSet.Dispose();
				_resultSet = null;
				isClosed=true;
			}
		}
		#endregion

		#region GetSchemaTable 

		public DataTable GetSchemaTable()
		{
			if (null ==_schemaTable)
			{
				_schemaTable = BuildNewSchemaTable();
			}
			
			return _schemaTable;
		}
		#endregion

		#region NextResult 

		public bool NextResult()
		{
		
			//throw new Db2Exception("To be done");
			short result = Db2CLIWrapper.SQLMoreResults(this.hwndStmt);
			Db2ClientUtils.Db2CheckReturn(result, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLMoreResults");
			
			
			if (Db2Constants.SQL_SUCCESS == result){
				result = Db2CLIWrapper.SQLNumResultCols(hwndStmt, ref numCols);
				Db2ClientUtils.Db2CheckReturn(result, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLNumResultCols");
				
				row=-1;
				_schemaTable = new DataTable();
				_schemaTable = BuildNewSchemaTable();
				_resultSet = new DataTable();
				_blobs = new BLOBWrapperCollection();
				
				PrepareResults();
				return true;
			}
			return false;

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
			if (FetchResults(_resultSet)) return true; else return false;
			
		}
		#endregion

		#region Describe/Bind/Fetch functions
		///
		///Broke these out so that we can use different paths for Immediate executions and Prepared executions
		/// <summary>
		/// Does the describe and bind steps for the query result set.  Called for both immediate and prepared queries. 
		/// </summary>
		private void PrepareResults(/*IntPtr[] sqlLen_or_IndPtr*/)
		{

			
			short sqlRet;
			IntPtr ptrCharacterAttribute = IntPtr.Zero;
			InitMem(20, ref ptrCharacterAttribute);
			short buflen = 18;
			short strlen = 18;
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
			string basecatalogname;
			string basetablename;
			string basecolumnname;
			

			for (short i=1; i<=numCols; i++) 
			{
			
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_BASE_COLUMN_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				colname = Marshal.PtrToStringAnsi(ptrCharacterAttribute);
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_CONCISE_TYPE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				sqltype = numericattr;
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_LENGTH, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				colsize = numericattr;
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_PRECISION, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				precision = numericattr;
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_SCALE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				scale = numericattr;

				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_NULLABLE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				nullable = numericattr;

				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_UPDATABLE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				updatable = numericattr;
				
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_AUTO_UNIQUE_VALUE, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				isautoincrement = numericattr;
				
				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_BASE_COLUMN_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				basecolumnname = Marshal.PtrToStringAnsi(ptrCharacterAttribute);

				sqlRet = Db2CLIWrapper.SQLColAttribute(hwndStmt, (short)i, (short)Db2Constants.SQL_DESC_BASE_TABLE_NAME, ptrCharacterAttribute, buflen, ref strlen, ref numericattr);
				basetablename = Marshal.PtrToStringAnsi(ptrCharacterAttribute);
				
				_resultSet.Columns.Add(colname);

				DataRow r = _schemaTable.NewRow();
				
				switch((int)sqltype){
					case Db2Constants.SQL_TYPE_BLOB:
					case Db2Constants.SQL_TYPE_BINARY:
					_blobs.Add(i);
					
					break;
				}

				r["ColumnName"] = colname;
				r["ColumnOrdinal"] = i;
				r["ColumnSize"] = colsize;
				r["NumericPrecision"] = precision;
				r["NumericScale"] = scale;
				r["DataType"] = GetManagedType((short)sqltype);
				r["ProviderType"] = sqltype;
				r["IsLong"] = IsLong((short)sqltype);
				r["AllowDBNull"] = (nullable==0)?true:false;
				r["IsReadOnly"] = false;
				r["IsRowVersion"] = false;
				r["IsKey"] = false;
				r["IsAutoIncrement"] = (isautoincrement==0)?true:false;
				r["BaseColumnName"] = basecolumnname;
				r["BaseTableName"] = basetablename;
				r["BaseCatalogName"] = "";
				r["BaseSchemaName"] = "";
				
				_schemaTable.Rows.Add(r);
			}
		}
/// <summary>
/// FetchResults does  what it says.
/// </summary>
/// <param name="dbVals"></param>
/// <param name="sqlLen_or_IndPtr"></param>
/// <param name="_resultSet"></param>
		private bool FetchResults(/*IntPtr[] sqlLen_or_IndPtr, */DataTable _resultSet) 
		{
			short sqlRet = 0;
			string str = String.Empty;

			sqlRet = Db2CLIWrapper.SQLFetch(hwndStmt);
			
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLFetch 1");

			if(Db2Constants.SQL_NO_DATA_FOUND == sqlRet) return false;
			//while(sqlRet != Db2Constants.SQL_NO_DATA_FOUND)
			//{
				DataRow newRow = _resultSet.NewRow();

				for (short y=1;y<=numCols;y++)
				{
					newRow[y-1] = GetDataObject(y);	
				}
				_resultSet.Rows.Add(newRow);
				return true;
				//sqlRet = Db2CLIWrapper.SQLFetch(hwndStmt);
				//Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_STMT, hwndStmt, "Db2ClientDataReader - SQLFetch 2");
			//}
		}
		
		private object GetDataObject(int i){
			int len = (int)_schemaTable.Rows[i-1][2] + 2;
			short sqlRet;
			IntPtr StrLen_or_IndStr = IntPtr.Zero;
			
			switch((int)_schemaTable.Rows[i-1][6]){
				case Db2Constants.SQL_INTEGER:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_SLONG, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				return (int) Marshal.PtrToStructure(Ptr, typeof(int));
				break;
				
				case Db2Constants.SQL_SMALLINT:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_SSHORT, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				return (short) Marshal.PtrToStructure(Ptr, typeof(short));
				break;
				
				case Db2Constants.SQL_DOUBLE:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_DOUBLE, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				return (double)Marshal.PtrToStructure(Ptr, typeof(double));
				break;
				
				case Db2Constants.SQL_DECIMAL:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_DECIMAL_OLEDB, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				return (decimal)Marshal.PtrToStructure(Ptr, typeof(decimal));
				break;
				
				case Db2Constants.SQL_DATETIME:
				case Db2Constants.SQL_TYPE_DATE:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_TYPE_DATE, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				short year = Marshal.ReadInt16(Ptr, 0);
				short month = Marshal.ReadInt16(Ptr, 2);
				short day = Marshal.ReadInt16(Ptr, 4);
				return new DateTime(year, month, day);
				break;
				
				case Db2Constants.SQL_CHAR:
				case Db2Constants.SQL_VARCHAR:
				InitMem(len, ref Ptr);
				sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)(i), (short)Db2Constants.SQL_C_CHAR, Ptr, new IntPtr(len), ref StrLen_or_IndStr);
				return Marshal.PtrToStringAnsi(Ptr);
				break;
				
				case Db2Constants.SQL_TYPE_BLOB:
				case Db2Constants.SQL_TYPE_BINARY:
				BLOBWrapper _blob = _blobs[i];
				_blob.ByteArray = GetBlobData(i);
				//Console.WriteLine(len.ToString());
				return null;
				break; 
			default:
			Console.WriteLine(_schemaTable.Rows[i-1][6].ToString());
				throw new Db2Exception("Unsuported data type");
			}
			return null;
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
					fieldCount = _resultSet.Columns.Count;
				return fieldCount;
			}
		}
		#endregion

		#region Item accessors
		public object this[string name]
		{
			get
			{
				return _resultSet.Rows[row][name];
			}
		}
		public object this[int col]
		{
			get
			{
				return _resultSet.Rows[row][col];
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
			return _blobs.GetBytes(col, fieldOffset, buffer, bufferOffset, length);
			//return 0;
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
			//Db2 does have some structured data types, is that what this is for?
			throw new Db2Exception("Not yet supported.");
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
			throw new Db2Exception("Not yet implemented");
		}
		#endregion

		#region GetDateTime
		///
		/// GetDateTime method
		/// 
		public string NewGetDateTime(int col)
		{
			return Convert.ToString(this[col]);
		}
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
			// a Guid is a 128 bit unique value.  Could be like a GENERATE UNIQUE in Db2
			// as usual, need more research
			throw new Db2Exception("TBD");
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
			return (_resultSet.Columns[col].ColumnName);
		}
		#endregion

		#region GetOrdinal
		///
		/// GetOrdinal, return the index of the named column
		/// 
		public int GetOrdinal(string name)
		{
			return _resultSet.Columns[name].Ordinal;
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

		#region GetLobLocator
		///
		///Returns a LOB Locator class
		///
		//Db2ClientLOBLocator GetLobLocator(int col)
		//{

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
			
			if (values.Length<numCols)
				throw new Db2Exception("GetValues argument too small for number of columns in row.");
			for (int i = 0; i<numCols; i++){
			  values[i] = this[i];
			 
			  }
			  
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
		
		private Type GetManagedType(short sql_type){
			switch(sql_type){
				case Db2Constants.SQL_INTEGER:
					return typeof(int);
				case Db2Constants.SQL_SMALLINT:
					return typeof(short);
				case Db2Constants.SQL_DOUBLE:
					return typeof(double);
				case Db2Constants.SQL_DECIMAL:
					return typeof(decimal);
				case Db2Constants.SQL_DATETIME:
				case Db2Constants.SQL_TYPE_DATE:
					return typeof(DateTime);
				case Db2Constants.SQL_CHAR:
				case Db2Constants.SQL_VARCHAR:
					return typeof(string);
				case Db2Constants.SQL_TYPE_BLOB:
				case Db2Constants.SQL_TYPE_BINARY:
					return typeof(IntPtr);
			}
			return null;
		}
		
		private bool IsLong(short sql_type){
			switch(sql_type){
				default:
					return false;
			}
		}
		private byte[] GetBlobData(int column){
			IntPtr StrLen_or_IndStr = new IntPtr(0);
			long sqlRet = 0;
			int length;
			byte[] result = new byte[0];
			
			sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)column, Db2Constants.SQL_C_TYPE_BINARY, result, new IntPtr(0), ref StrLen_or_IndStr); 
			length = StrLen_or_IndStr.ToInt32();
			result = new byte[length];
			
			sqlRet = Db2CLIWrapper.SQLGetData(this.hwndStmt, (short)column, Db2Constants.SQL_C_TYPE_BINARY, result, new IntPtr(length), ref StrLen_or_IndStr);
			
			return result;
		}
	}

}
#endregion
