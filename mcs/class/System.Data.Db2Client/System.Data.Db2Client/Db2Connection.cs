using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.Db2Client
{

	
	public class Db2Connection : IDbConnection
	{
		private WeakReference refTransaction;

		private string connectionString = null;
		private StringBuilder outConnectStr;
		private string dbName = null;
		private int connectionTimeout;
		
		private IntPtr penvHandle=IntPtr.Zero;
		private IntPtr dbHandle = IntPtr.Zero;
		
		private bool disposed = false;

		string serverName, userName, authentication, dsn;

		public Db2Connection()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public Db2Connection(string conString)
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

		unsafe		 public ConnectionState State
		{
			get
			{   
				if ((long)dbHandle.ToPointer() == Db2Constants.SQL_NULL_HANDLE)
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
		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel isolationL)
		{
			return BeginTransaction(isolationL);
		}

		public Db2Transaction BeginTransaction()
		{
			CheckOldTransaction();

			Db2Transaction tran = new Db2Transaction(this, IsolationLevel.ReadCommitted);
			refTransaction = new WeakReference(tran);
			return tran;
		}

		public Db2Transaction BeginTransaction(IsolationLevel isolationL)
		{
			CheckOldTransaction();

			Db2Transaction tran = new Db2Transaction(this, isolationL);
			refTransaction = new WeakReference(tran);
			return tran;
		}

		private void CheckOldTransaction()
		{
			if((refTransaction != null) && (!refTransaction.IsAlive))
			{
				try
				{
					Db2CLIWrapper.SQLEndTran(Db2Constants.SQL_HANDLE_DBC, this.dbHandle, Db2Constants.SQL_ROLLBACK);
				}
				finally
				{
					refTransaction = null;
				}
			}

			if ((refTransaction != null) && (refTransaction.IsAlive))
			{
				throw new Db2Exception("Cannot open another transaction");
			}
		}
		#endregion
		
		#region ChangeDatabase
		public void ChangeDatabase(string newDBName)
		{
			short sqlRet;
			this.Close();
			
			sqlRet = Db2CLIWrapper.SQLAllocHandle(Db2Constants.SQL_HANDLE_DBC, penvHandle, ref dbHandle);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_ENV, penvHandle, "Unable to allocate database handle in Db2Connection.");
			
			this.serverName = newDBName;
			this.dbName = newDBName;
			sqlRet = Db2CLIWrapper.SQLConnect(dbHandle,serverName, (short)serverName.Length, userName, (short)userName.Length, authentication, (short)authentication.Length);
			Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_ENV, penvHandle, "Unable to connect to the database.");

		}
		#endregion
		
		#region Close
		///Close, per MSDN documentation
		///
		 public void Close()
		{
			short sqlRet = 0;
//			Db2ClientUtils util = new Db2ClientUtils();

			sqlRet = Db2CLIWrapper.SQLDisconnect(dbHandle);
//			util.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_DBC, dbHandle, "Error in Connection->Close: ");
			dbHandle = new IntPtr(Db2Constants.SQL_NULL_HANDLE);
		}
		#endregion
		
		#region CreateCommand
		/// <summary>
		/// CreateCommand per MSDN
		/// </summary>
		/// <returns></returns>
		IDbCommand IDbConnection.CreateCommand()
		{
			return CreateCommand();
		}

		public Db2Command CreateCommand()
		{
			CheckState();
			return new Db2Command();
		}
		#endregion
		
		#region Open
		/// <summary>
		/// Open, per MSDN
		/// </summary>
		public void Open()
		{
			outConnectStr = new StringBuilder(60);  //Set some initial size, we know we're gettig a chunk of data back
			IntPtr numOutCharsReturned = IntPtr.Zero;
			short sqlRet=0;

			if (this.State == ConnectionState.Open || this.State == ConnectionState.Connecting || this.State == ConnectionState.Executing || this.State == ConnectionState.Fetching)
			{
				throw new Db2Exception("Connection already open");
			}

			try
			{
				string[] parts = connectionString.Split(new char[]{';'});
				foreach(string part in parts)
				{
					string[] pairs = part.Split(new char[]{'='});
					switch(pairs[0])
					{
						case "database":
							serverName = pairs[1];
							break;
						case "uid":
							userName = pairs[1];
							break;
						case "pwd":
							authentication = pairs[1];
							break;
						case "dsn":
							dsn = pairs[1];
							break;
						default:
							break;
					}
				}
			}
			catch(Exception)
			{
				throw new Db2Exception("Bad connection string");
			}

			try
			{
				sqlRet = Db2CLIWrapper.SQLAllocHandle(Db2Constants.SQL_HANDLE_ENV, IntPtr.Zero, ref penvHandle);
				Db2ClientUtils.Db2CheckReturn(sqlRet, 0, IntPtr.Zero, "Unable to allocate Environment handle in Db2Connection.");

				sqlRet = Db2CLIWrapper.SQLAllocHandle(Db2Constants.SQL_HANDLE_DBC, penvHandle, ref dbHandle);
				Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_ENV, penvHandle, "Unable to allocate database handle in Db2Connection.");

				if(dsn != null)
				{
					sqlRet = Db2CLIWrapper.SQLDriverConnect(dbHandle, 0, connectionString,
						connectionString.Length, outConnectStr, 100, numOutCharsReturned, 
						Db2Constants.SQL_DRIVER_COMPLETE);
				}
				else
				{
					sqlRet = Db2CLIWrapper.SQLConnect(dbHandle,serverName, (short)serverName.Length, userName, (short)userName.Length, authentication, (short)authentication.Length);
					Db2ClientUtils.Db2CheckReturn(sqlRet, Db2Constants.SQL_HANDLE_ENV, penvHandle, "Unable to connect to the database.");
				}
			}
			catch (Db2Exception Db2E)
			{
				Console.WriteLine(Db2E.Message);
				Dispose();
				throw Db2E;
			}
		}
		#endregion
		
		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
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
				throw new Db2Exception ("Connection is currently closed.");
		}

		void SetConnectionString (string connectionString) 
		{
			this.connectionString = connectionString;
		}

		internal WeakReference WeakRefTransaction
		{
			get
			{
				return refTransaction;
			}
			set
			{
				refTransaction = value;
			}
		}

	}
}

