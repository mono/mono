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
		private string connectionString = null;
		private StringBuilder outConnectStr;
		private string dbName = null;
		private int connectionTimeout;

		private IntPtr dbHandle = IntPtr.Zero;
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
				if ((long)dbHandle.ToPointer() == DB2ClientConstants.SQL_NULL_HANDLE)
					return ConnectionState.Closed;
				else
					return ConnectionState.Open;
			}
		}
		#endregion
		#region DBHandle
		///
		/// Handle Returns an IntPtr of the dbm handle
		/// 
		public IntPtr DBHandle
		{
			get
			{
				return dbHandle;
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
			short sqlRet = 0;
			DB2ClientUtils util = new DB2ClientUtils();

			sqlRet = DB2ClientPrototypes.SQLDisconnect(dbHandle);
			util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_DBC, dbHandle, "Error in Connection->Close: ");
			dbHandle = new IntPtr(DB2ClientConstants.SQL_NULL_HANDLE);
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
			DB2ClientUtils util = new DB2ClientUtils();
			outConnectStr = new StringBuilder(60);  //Set some initial size, we know we're gettig a chunk of data back
			IntPtr penvHandle=IntPtr.Zero;
			IntPtr numOutCharsReturned = IntPtr.Zero;
			short sqlRet=0;

			try
			{
				sqlRet = DB2ClientPrototypes.SQLAllocHandle(DB2ClientConstants.SQL_HANDLE_ENV, IntPtr.Zero, ref penvHandle);
				util.DB2CheckReturn(sqlRet, 0, IntPtr.Zero, "Unable to allocate Environment handle in DB2ClientConnection.");

				sqlRet = DB2ClientPrototypes.SQLAllocHandle(DB2ClientConstants.SQL_HANDLE_DBC, penvHandle, ref dbHandle);
				util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_ENV, penvHandle, "Unable to allocate database handle in DB2ClientConnection.");

				sqlRet = DB2ClientPrototypes.SQLDriverConnect(dbHandle, 0, connectionString,
					connectionString.Length, outConnectStr, 100, numOutCharsReturned, 
					DB2ClientConstants.SQL_DRIVER_COMPLETE);
				util.DB2CheckReturn(sqlRet, DB2ClientConstants.SQL_HANDLE_ENV, penvHandle, "Unable to connect to the database.");

			}
			catch (DB2ClientException DB2E)
			{
				Console.WriteLine(DB2E.Message);
				Dispose();
				throw DB2E;
			}
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
		}
	}
}

