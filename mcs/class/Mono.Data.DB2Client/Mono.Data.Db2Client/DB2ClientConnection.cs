#region Licence
	/// DB2DriverCS - A DB2 driver for .Net
	/// 
	/// Authors:
	///	Christopher Bockner
	///	
	/// Copyright 2003 By Christopher Bockner
	/// 
	/// Released under the terms of the MIT/X11 Licence
	/// Please refer to the Licence.txt file that should be distributed with this package
	/// This software requires that DB2 client software be installed correctly on the machine
	/// (or instance) on which the driver is running.  
#endregion
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace DB2ClientCS
{
	/// <summary>	/// This class is IDbConnection compliant.  Refer to MSDN documentation for reference.
	/// </summary>
	/// 
	
	public class DB2ClientConnection : IDbConnection
	{
		private class ConnectionParameters {
			internal string server = "";
			internal string username = "";
			internal string authentication = "";
		}
		private ConnectionParameters connectionParms = new ConnectionParameters();
		private string connectionString = null;
		private string dbName = null;
		private int connectionTimeout;

		unsafe internal long dbHandle;
		private bool disposed = false;
		public DB2ClientConnection()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public DB2ClientConnection(string conString)
		{
			SetConnectionString(conString);
		}
		#region ConnectionString property
		/// 
		///Accessor for the connectionString property
		public string ConnectionString 
		{
			get
			{
				return connectionString;
			}
			set
			{
				SetConnectionString(value);
			}
		}
		#endregion
		#region ConnectionTimeout property
		public int ConnectionTimeout
		{
			get
			{
				return connectionTimeout;
			}
			set
			{
				connectionTimeout = value;
			}
		}
		#endregion
		#region Database property
		public string Database
		{
			get
			{
				return dbName;
			}
			set
			{
				dbName = value;
			}
		}
		#endregion
		#region State property
		/// <summary>
		/// The Connection State property, open or closed. 
		/// NOTE:  IBM's docs on SqlFreeHandle do not state what is done when a handle is freed
		///	i.e. if the handle is set to SQL_NULL_HANDLE.
		/// </summary>

		unsafe public ConnectionState State
		{
			get
			{   
				if (dbHandle == DB2ClientConstants.SQL_NULL_HANDLE)
					return ConnectionState.Closed;
				else
					return ConnectionState.Open;
			}
		}
		#endregion
		#region Handle
		///
		/// Handle Returns an IntPtr of the dbm handle
		/// 
		public IntPtr Handle
		{
			get
			{
				return new IntPtr(dbHandle);
			}
		}
		#endregion

		#region BeginTransaction Method
		/// <summary>
		/// Opens a transaction against the database at the default isolation level, which will be 
		/// that which the packages were bound at, unless overriden in the connection string, and if nothing was specified at that point
		/// then I believe the default level is Cursor Stability (don't quote me on that, I haven't
		/// found the appropriate reference yet), ODBC equivalent is SQL_TXN_READ_COMMITTED
		/// </summary>
		/// <returns></returns>
		public IDbTransaction BeginTransaction()
		{
			return null;
		}
		#endregion
		#region BeginTransaction (IsolationLevel) Method
		/// <summary>
		/// BeginTransaction again overloadded to let us set the transaction level for the statement
		/// </summary>
		/// <param name="isolationL"></param>
		/// <returns></returns>
		public IDbTransaction BeginTransaction(IsolationLevel isolationL)
		{
			return null;
		}
		#endregion
		#region ChangeDatabase
		unsafe public void ChangeDatabase(string newDBName)
		{
		}
		#endregion
		#region Close
		///Close, per MSDN documentation
		///
		unsafe public void Close()
		{
			DB2ClientPrototypes.SQLDisconnect(dbHandle);
			dbHandle = DB2ClientConstants.SQL_NULL_HANDLE;
		}
		#endregion
		#region CreateCommand
		/// <summary>
		/// CreateCommand per MSDN
		/// </summary>
		/// <returns></returns>
		public IDbCommand CreateCommand()
		{
			CheckState();
			return new DB2ClientCommand();
		}
		#endregion
		#region Open
		/// <summary>
		/// Open, per MSDN
		/// </summary>
		unsafe public void Open()
		{
			IntPtr pdbHandle = Marshal.AllocHGlobal(4);
			IntPtr penvHandle = Marshal.AllocHGlobal(4);
			short sqlReturn;
			try
			{
			IntPtr tempInt = IntPtr.Zero;
			sqlReturn = DB2ClientPrototypes.SQLAllocHandle(DB2ClientConstants.SQL_HANDLE_ENV, tempInt, ref penvHandle);
			sqlReturn = DB2ClientPrototypes.SQLAllocHandle(DB2ClientConstants.SQL_HANDLE_DBC, penvHandle, ref pdbHandle);

			if (sqlReturn == DB2ClientConstants.SQL_ERROR) 
			{
				throw new DB2ClientException(DB2ClientConstants.SQL_HANDLE_ENV, penvHandle, "Alloc Env Handle: ");
			}
			sqlReturn = DB2ClientPrototypes.SQLConnect(pdbHandle, 
				connectionParms.server, (short)connectionParms.server.Length, 
				connectionParms.username, (short)connectionParms.username.Length, 
				connectionParms.authentication, (short)connectionParms.authentication.Length);
			if (sqlReturn == DB2ClientConstants.SQL_ERROR) 
			{
				throw new DB2ClientException(DB2ClientConstants.SQL_HANDLE_DBC, pdbHandle, "Error connecting to DB: ");
			}

			if(IntPtr.Size == 4)
			  dbHandle = pdbHandle.ToInt32();
			else
			  dbHandle = pdbHandle.ToInt64();
		}
		catch (DB2ClientException DB2E)
		{
			Console.WriteLine(DB2E.Message);
			Marshal.FreeHGlobal(pdbHandle);
			Marshal.FreeHGlobal(penvHandle);
			return;
		}
		Marshal.FreeHGlobal(pdbHandle);
		Marshal.FreeHGlobal(penvHandle);
	}
		#endregion
		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		unsafe public void Dispose()
		{
			if(!disposed) 
			{
				this.Close();
				disposed = true;
			} else 
				return;
		}

		#endregion

		private void CheckState()
		{
			if (ConnectionState.Closed == State)
				throw new DB2ClientException ("Connection is currently closed.");
		}

		void SetConnectionString (string connectionString) 
		{
			this.connectionString = connectionString;
			
			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			if (connectionString == String.Empty)
				return;

			bool inQuote = false;
			bool inDQuote = false;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			foreach (char c in connectionString) {
				switch (c) {
				case '\'':
					inQuote = !inQuote;
					break;
				case '"' :
					inDQuote = !inDQuote;
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
							value = sb.ToString ();
							parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
						name = String.Empty;
						value = String.Empty;
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				case '=' :
					if (!inDQuote && !inQuote) {
						name = sb.ToString ();
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				default:
					sb.Append (c);
					break;
				}
			}

			SetProperties (parameters);
		}

		private void SetProperties (NameValueCollection parameters) 
		{
			string value;
			foreach (string name in parameters) {
				value = parameters[name];
				
				switch (name) {
				case "INITIAL CATALOG" :
				case "DATA SOURCE" :
				case "DATABASE" :
					// set Database property
					connectionParms.server = value;
					break;
				case "PASSWORD" :
				case "AUTHENTICATION" :
				case "PWD" :
					connectionParms.authentication = value;
					break;
				case "USER ID" :
				case "UID" :
				case "USERNAME" :
				case "USER" :
					connectionParms.username = value;
					break;
				default:
					throw new ArgumentException("Invalid connection parameter: " + name);
				}
			}
		}
	}
}

