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
	// public sealed class MySqlCommand : Component, IDbCommand, ICloneable
	public sealed class MySqlCommand : IDbCommand {

		#region Fields

		private string sql = "";
		private int timeout = 30; 
		// default is 30 seconds 
		// for command execution

		private MySqlConnection conn = null;
		private MySqlTransaction trans = null;
		private CommandType cmdType = CommandType.Text;
		private bool designTime = false;
		//private MySqlParameterCollection parmCollection = new 
		//	MySqlParameterCollection();

		// MySqlDataReader state data for ExecuteReader()
		//private MySqlDataReader dataReader = null;
		private string[] queries = null;
		private int currentQuery = -1;
		private CommandBehavior cmdBehavior = CommandBehavior.Default;

		//private ParmUtil parmUtil = null;
		
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
			//return CreateParameter ();
			return null;
		}

		/*
		[MonoTODO]
		public MySqlParameter CreateParameter () {
			return new MySqlParameter ();
		}
		*/

		[MonoTODO]
		public int ExecuteNonQuery () {	
			int rowsAffected = -1;
			string msg = "";
			//TODO: need to do this correctly
			//      this is just something quick
			//      thrown together to see if we can
			//      execute a SQL Command
			int rcq = MySql.Query(conn.NativeMySqlInitStruct, sql);
			if (rcq != 0) {
				msg = 
					"MySql Error: " + 
					"Could not execute command [" + 
					sql + 
					"] on server because: " +
					MySql.Error(conn.NativeMySqlInitStruct);
				throw new MySqlException(msg);
			}
			// TODO: need to return the number of rows affected for an INSERT, UPDATE, or DELETE
			//       otherwise, it is -1
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
			string msg = "";

			// TODO: check to see if you everything you need
			//       and the connection is open, etc...
			//       and through any exceptions if not
			
			// execute query
			int rcq = MySql.Query(conn.NativeMySqlInitStruct, sql);
			if (rcq != 0) {
				msg = "MySql Error: " +
					"Could not execute [" +
					sql +
					"] on server because: " +
					MySql.Error(conn.NativeMySqlInitStruct);
				throw new MySqlException(msg);
			}

			reader = new MySqlDataReader(this, behavior);
			reader.NextResult();

			return reader;
		}
		
		[MonoTODO]
		public object ExecuteScalar () {
			object obj = null;
			string msg = "";
			
			int rcq = MySql.Query(conn.NativeMySqlInitStruct, sql);
			if (rcq != 0) {
				msg = "MySqlError: Could not execute [" +
					sql +
					"] on server because: " +
					MySql.Error(conn.NativeMySqlInitStruct);
				throw new MySqlException(msg);
			}

			IntPtr res = MySql.StoreResult(conn.NativeMySqlInitStruct);
			int numRows = MySql.NumRows(res);
			
			int numFields = MySql.NumFields(res);
						
			MySqlMarshalledField fd;
			fd = (MySqlMarshalledField) Marshal.PtrToStructure(MySql.FetchField(res), 
				typeof(MySqlMarshalledField));
			string fieldName = fd.Name;
			int fieldType = fd.FieldType; 
			DbType fieldDbType = MySqlHelper.MySqlTypeToDbType((MySqlEnumFieldTypes)fieldType);

			//Console.WriteLine("*** DEBUG: MySql FieldType: " + fieldType);
						
			IntPtr row;
			row = MySql.FetchRow(res);
			if(row == IntPtr.Zero) {
				// EOF
				obj = null;
			}
			else {
				// only get first column/first row
				string objValue = GetColumnData(row, 0);
				obj = MySqlHelper.ConvertDbTypeToSystem (fieldDbType, objValue);
			}
			MySql.FreeResult(res);
			res = IntPtr.Zero;

			return obj;
		}

		[MonoTODO]
		public XmlReader ExecuteXmlReader () {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Prepare () {
			// FIXME: parameters have to be implemented for this
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public MySqlCommand Clone () {
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

		// FIXME: for property Connection, is this the correct
		//        way to handle a return of a stronger type?
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

		// FIXME; for property Parameters, is this the correct
		//        way to handle a stronger return type?
		IDataParameterCollection IDbCommand.Parameters	{
			get { 
				//return Parameters;
				return null;
			}
		}

		//public MySqlParameterCollection Parameters {
		//	get { 
		//		return parmCollection;
		//	}
		//}

		// FIXME: for property Transaction, is this the correct
		//        way to handle a return of a stronger type?
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

		[MonoTODO]
		public void Dispose() {
			// FIXME: need proper way to release resources
			// Dispose(true);
		}

		[MonoTODO]
		~MySqlCommand() {
			// FIXME: need proper way to release resources
			// Dispose(false);
		}

		#endregion //Destructors
	}
}
