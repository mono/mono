//
// Mono.Data.MySql.MySqlCommand.cs
//
// Author:
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Daniel Morgan, 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

namespace Mono.Data.MySql {
	public sealed class MySqlCommand : Component, IDbCommand, ICloneable {
	
		#region Fields

		private string sql = "";
		private int timeout = 30; 
		// default is 30 seconds 
		// for command execution

		private MySqlConnection conn = null;
		private MySqlTransaction trans = null;
		private CommandType cmdType = CommandType.Text;
		private bool designTime = false;
		private MySqlParameterCollection parmCollection = new 
			MySqlParameterCollection();

		// MySqlDataReader state data for ExecuteReader()
		//private MySqlDataReader dataReader = null;
		private string[] commands = null;
		private int currentQuery = -1;
		private CommandBehavior cmdBehavior = CommandBehavior.Default;

		private bool disposed = false;

		private const char bindChar = ':';
		
		#endregion // Fields

		#region Constructors

		public MySqlCommand() {
			sql = "";
		}

		public MySqlCommand (string cmdText) {
			sql = cmdText;
		}

		public MySqlCommand (string cmdText, MySqlConnection connection) {
			sql = cmdText;
			conn = connection;
		}

		public MySqlCommand (string cmdText, MySqlConnection connection, 
			MySqlTransaction transaction) {
			sql = cmdText;
			conn = connection;
			trans = transaction;
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void Cancel () {
			// FIXME: use non-blocking Exec for this
			throw new NotImplementedException ();
		}

		// FIXME: is this the correct way to return a stronger type?
		[MonoTODO]
		IDbDataParameter IDbCommand.CreateParameter () {
			return CreateParameter ();
		}
		
		[MonoTODO]
		public MySqlParameter CreateParameter () {
			return new MySqlParameter ();
		}
		
		public int ExecuteNonQuery () {	
			int rowsAffected = -1;
			
			IntPtr res = ExecuteSQL (sql);

			if(res.Equals(IntPtr.Zero)) {
				// no result set returned, get records affected
				rowsAffected = (int) MySql.AffectedRows(conn.NativeMySqlInitStruct);
			}

			MySql.FreeResult(res);
			res = IntPtr.Zero;

			// >= 0 of the number of rows affected by
			//    INSERT, UPDATE, DELETE
			// otherwise, -1
			return rowsAffected;
		}
		
		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader () {
			return ExecuteReader ();
		}

		[MonoTODO]
		public MySqlDataReader ExecuteReader () {
			return ExecuteReader(CommandBehavior.Default);
		}
		
		[MonoTODO]
		IDataReader IDbCommand.ExecuteReader (
			CommandBehavior behavior) {

			return ExecuteReader (behavior);
		}
		
		[MonoTODO]
		public MySqlDataReader ExecuteReader (CommandBehavior behavior) {	

			MySqlDataReader reader = null;
			
			currentQuery = -1;
			
			commands = sql.Split(new Char[] {';'});			
			reader = new MySqlDataReader(this, behavior);
			reader.NextResult();

			return reader;
		}

		// called by an MySqlDataReader's NextResult()
		internal IntPtr NextResult (out bool result) {
			
			IntPtr mysqlResult = IntPtr.Zero;		
			result = false;

			currentQuery++;
			if(currentQuery < commands.Length) {
				string query = "";

				// don't execute empty queries
				while((query = commands[currentQuery]).Equals("")) {
					currentQuery++;
					if(currentQuery >= commands.Length)
						return IntPtr.Zero;
				}
				mysqlResult = ExecuteSQL (query);
				result = true; // has result
			}

			return mysqlResult;
		}
				
		public object ExecuteScalar () {

			object obj = null;
						
			IntPtr res = ExecuteSQL (sql);

			int numRows = MySql.NumRows(res);		
			int numFields = MySql.NumFields(res);
						
			MySqlMarshalledField fd;
			fd = (MySqlMarshalledField) Marshal.PtrToStructure(MySql.FetchField(res), 
				typeof(MySqlMarshalledField));
			string fieldName = fd.Name;
			int fieldType = fd.FieldType; 
			MySqlEnumFieldTypes mysqlFieldType = (MySqlEnumFieldTypes) fieldType;
			DbType fieldDbType = MySqlHelper.MySqlTypeToDbType(mysqlFieldType);
						
			IntPtr row;
			row = MySql.FetchRow(res);
			if(row == IntPtr.Zero) {
				// EOF
				obj = null;
			}
			else {
				// only get first column/first row
				string objValue = GetColumnData(row, 0);
				obj = MySqlHelper.ConvertDbTypeToSystem (mysqlFieldType, fieldDbType, objValue);
				row = IntPtr.Zero;
			}
			MySql.FreeResult(res);
			res = IntPtr.Zero;

			return obj;
		}

		// command: string in - SQL command
		//          IntPtr (MySqlResult) return - the result
		//          Use of this function needs to check to see if
		//          if the return equal to IntPtr.Zero
		// Example: IntPtr res = ExecuteSQL ("SELECT * FROM DB");
		//          if (res == IntPtr.Zero) { // do something }
		//
		internal IntPtr ExecuteSQL (string command) {
			string msg = "";

			if (conn == null)
				throw new InvalidOperationException(
					"Connection is null");

			if (conn.State != ConnectionState.Open)
				throw new InvalidOperationException(
					"ConnectionState is not Open");

			if (sql.Equals (String.Empty)) 
				throw new InvalidOperationException(
					"CommandText is Empty");

			string query = TweakQuery(sql);
			
			int rcq = MySql.Query(conn.NativeMySqlInitStruct, query);
			if (rcq != 0) {
				msg = 
					"MySql Error: " + 
					"Could not execute command [" + 
					sql + 
					"] on server because: " +
					MySql.Error(conn.NativeMySqlInitStruct);
				throw new MySqlException(msg);
			}
			IntPtr result = MySql.StoreResult(conn.NativeMySqlInitStruct);
			return result;
		}

		string TweakQuery(string query) {
			string statement = "";
			
			switch(cmdType) {
			case CommandType.Text:
				statement = ReplaceParameterPlaceholders (query);
				break;
			case CommandType.StoredProcedure:
				string sParmList = GetStoredProcParmList ();
				statement = "SELECT " + query + "(" + sParmList + ")";
				break;
			case CommandType.TableDirect:
				statement = 
					"SELECT * FROM " + query;
				break;
			}
			return statement;
		}

		string GetStoredProcParmList () {
			StringBuilder s = new StringBuilder();

			int addedCount = 0;
			for(int p = 0; p < parmCollection.Count; p++) {
				MySqlParameter prm = parmCollection[p];
				if(prm.Direction == ParameterDirection.Input) {
					string strObj = MySqlHelper.
						ObjectToString(prm.DbType, 
						prm.Value);
					if(addedCount > 0)
						s.Append(",");
					s.Append(strObj);
					addedCount++;
				}
			}
			return s.ToString();
		}

		// TODO: this only supports input parameters,
		//       need support for output, input/output,
		//       and return parameters
		//       As far as I know, MySQL does not support
		//       parameters so the parameters support in this
		//       provider is just a search and replace.
		string ReplaceParameterPlaceholders (string query) {
			
			string resultSql = "";

			StringBuilder result = new StringBuilder();
			char[] chars = sql.ToCharArray();
			bool bStringConstFound = false;

			for(int i = 0; i < chars.Length; i++) {
				if(chars[i] == '\'') {
					if(bStringConstFound == true)
						bStringConstFound = false;
					else
						bStringConstFound = true;

					result.Append(chars[i]);
				}
				else if(chars[i] == bindChar && 
					bStringConstFound == false) {

					StringBuilder parm = new StringBuilder();
					i++;
					while(i <= chars.Length) {
						char ch;
						if(i == chars.Length)
							ch = ' '; // a space
						else
							ch = chars[i];

						if(Char.IsLetterOrDigit(ch)) {
							parm.Append(ch);
						}
						else {
							string p = parm.ToString();
							bool found = BindReplace(result, p);

							if(found == true)
								break;
							else {						
								// *** Error Handling
								Console.WriteLine("Error: parameter not found: " + p);
								return "";
							}
						}
						i++;
					}
					i--;
				}
				else 
					result.Append(chars[i]);
			}
			
			resultSql = result.ToString();
			return resultSql;
		}

		bool BindReplace (StringBuilder result, string p) {
			// bind variable
			bool found = false;

			if(parmCollection.Contains(p) == true) {
				// parameter found
				MySqlParameter prm = parmCollection[p];

				// convert object to string and place
				// into SQL
				if(prm.Direction == ParameterDirection.Input) {
					string strObj = MySqlHelper.
						ObjectToString(prm.DbType, 
						prm.Value);
					result.Append(strObj);
				}
				else
					result.Append(bindChar + p);

				found = true;
			}
			return found;
		}

		[MonoTODO]
		public XmlReader ExecuteXmlReader () {
			//MySqlDataReader dataReader = ExecuteReader ();
			//MySqlXmlTextReader textReader = new MySqlXmlTextReader (dataReader);
			//XmlReader xmlReader = new XmlTextReader (textReader);
			//return xmlReader;
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Prepare () {
			// FIXME: parameters have to be implemented for this
			throw new NotImplementedException ();
		}

		object ICloneable.Clone() {
			throw new NotImplementedException ();
		}

		// Used to marshal a field value from the database result set.
		// The indexed column data on the current result set row.
		// res = the result set from a MySql.Query().
		// index = the column index.
		internal string GetColumnData(IntPtr res, int index) {
			IntPtr str = Marshal.ReadIntPtr(res, index*IntPtr.Size);
			if (str == IntPtr.Zero)
				return "";
			string s = Marshal.PtrToStringAnsi(str);
			return s;
		}

		#endregion // Methods

		#region Properties

		public string CommandText {
			get { 
				return sql; 
			}

			set { 
				sql = value; 
			}
		}

		public int CommandTimeout {
			get { 
				return timeout;  
			}
			
			set {
				// FIXME: if value < 0, throw
				// ArgumentException
				// if (value < 0)
				//	throw ArgumentException;
				timeout = value;
			}
		}

		public CommandType CommandType	{
			get {
				return cmdType;
			}

			set { 
				cmdType = value;
			}
		}

		IDbConnection IDbCommand.Connection {
			get { 
				return Connection;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during a 
				// transaction in progress

				// csc
				Connection = (MySqlConnection) value; 
				// mcs
				// Connection = value; 
				
				// FIXME: set Transaction property to null
			}
		}
		
		public MySqlConnection Connection {
			get { 
				// conn defaults to null
				return conn;
			}

			set { 
				// FIXME: throw an InvalidOperationException
				// if the change was during 
				// a transaction in progress
				conn = value; 
				// FIXME: set Transaction property to null
			}
		}

		public bool DesignTimeVisible {
			get {
				return designTime;
			} 
			
			set{
				designTime = value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters	{
			get { 
				return Parameters;
			}
		}

		public MySqlParameterCollection Parameters {
			get { 
				return parmCollection;
			}
		}

		IDbTransaction IDbCommand.Transaction 	{
			get { 
				return Transaction;
			}

			set { 
				// FIXME: error handling - do not allow
				// setting of transaction if transaction
				// has already begun
				Transaction = (MySqlTransaction) value;
				
			}
		}
		
		public MySqlTransaction Transaction {
			get { 
				return trans; 
			}

			set { 
				// FIXME: error handling
				trans = value; 
			}
		}
		
		[MonoTODO]
		public UpdateRowSource UpdatedRowSource	{
			// FIXME: do this once DbDataAdaptor 
			// and DataRow are done
			get { 		
				throw new NotImplementedException (); 
			}
			set { 
				throw new NotImplementedException (); 
			}
		}

		#endregion // Properties

		#region Inner Classes

		#endregion // Inner Classes

		#region Destructors

		protected override void Dispose(bool disposing) {
			if(!this.disposed)
				try {
					if(disposing) {
						// release any managed resources
					}
					// release any unmanaged resources
					// close any handles
										
					this.disposed = true;
				}
				finally {
					base.Dispose(disposing);
				}
		}
	
		// aka Finalize()
		~MySqlCommand () {
			Dispose (false);
		}

		#endregion //Destructors
	}
}
