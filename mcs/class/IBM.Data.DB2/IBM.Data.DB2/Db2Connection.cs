using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;

namespace IBM.Data.DB2
{

	
	public class DB2Connection : System.ComponentModel.Component, IDbConnection
	{
		private ArrayList refCommands;
		private WeakReference refTransaction;

		private Db2ConnectionSettings connectionSettings = null;
		//private StringBuilder outConnectStr;
		private int connectionTimeout;
		
		private Db2OpenConnection openConnection;
		
		private bool disposed = false;



		public DB2Connection()
		{
			connectionTimeout = 15;
			
		}
		
		public DB2Connection(string conString):this()
		{
			
			SetConnectionString(conString);
		}
		
		public DB2Connection(DB2Connection db2conn):this(db2conn.ConnectionString)
		{	
		}
		
		#region ConnectionString property

		public string ConnectionString 
		{
			get
			{
				return connectionSettings.ConnectionString;
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
				return connectionSettings.DatabaseAlias;
			}
		}
		#endregion
	
		#region State property


		unsafe public ConnectionState State
		{
			get
			{   
				//if ((long)dbHandle.ToPointer() == DB2Constants.SQL_NULL_HANDLE)
				if (openConnection == null)
				{
					return ConnectionState.Closed;
				}
				
				return ConnectionState.Open;
			}
		}
		#endregion
		
		#region DBHandle

		internal IntPtr DBHandle
		{
			get
			{
				return (openConnection == null) ? IntPtr.Zero : openConnection.DBHandle;
			}
		}
		#endregion

		#region BeginTransaction Method

		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel isolationL)
		{
			return BeginTransaction(isolationL);
		}

		public DB2Transaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public DB2Transaction BeginTransaction(IsolationLevel isolationL)
		{
			if ((refTransaction != null) && (refTransaction.IsAlive))
			{
				throw new InvalidOperationException("Cannot open another transaction");
			}
			if(State != ConnectionState.Open)
				throw new InvalidOperationException("BeginTransaction needs an open connection");

			DB2Transaction tran = new DB2Transaction(this, isolationL);
			refTransaction = new WeakReference(tran);
			return tran;
		}

		#endregion
		
		#region ChangeDatabase
		unsafe public void ChangeDatabase(string newDBName)
		{
			if(connectionSettings == null)
			{
				throw new InvalidOperationException("No connection string");
			}
			this.Close();

			SetConnectionString(connectionSettings.ConnectionString.Replace(connectionSettings.DatabaseAlias, newDBName));

			this.Open();
		}
		#endregion
		
		#region Close
		///Close, per MSDN documentation
		///
		public void Close()
		{
			DB2Transaction transaction = null;
			if(refTransaction != null)
				transaction = (DB2Transaction)refTransaction.Target;
			if((transaction != null) && refTransaction.IsAlive)
			{
				transaction.Dispose();
			}
			if(refCommands != null)
			{
				for(int i = 0; i < refCommands.Count; i++)
				{
					DB2Command command = null;
					if(refCommands[i] != null)
					{
						command = (DB2Command)((WeakReference)refCommands[i]).Target;
					}
					if((command != null) && ((WeakReference)refCommands[i]).IsAlive)
					{
						try
						{
							command.ConnectionClosed();
						}
						catch{}
					}
					refCommands[i] = null;
				}
			}

			if(openConnection != null)
			{
				openConnection.Close();
				openConnection = null;
			}
		}

		#endregion
		
		#region CreateCommand
		IDbCommand IDbConnection.CreateCommand()
		{
			return CreateCommand();
		}

		public DB2Command CreateCommand()
		{
			//CheckState();
			return new DB2Command(null, this);
		}
		#endregion
		
		#region Open
		unsafe public void Open()
		{
			if(disposed)
			{
				throw new ObjectDisposedException("connection disposed");
			}
			//short sqlRet=0;

			if (this.State == ConnectionState.Open || this.State == ConnectionState.Connecting || this.State == ConnectionState.Executing || this.State == ConnectionState.Fetching)
			{
				throw new InvalidOperationException("Connection already open");
			}

			try
			{
				openConnection = connectionSettings.GetRealOpenConnection();
			}
			catch (DB2Exception)
			{
				Close();
				throw;
			}
		}
		#endregion
		
		#region Dispose
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected override void Dispose(bool disposing)
		{
			if(!disposed) 
			{
				if(disposing)
				{
					// dispose managed resources
					Close();
				}
			}
			base.Dispose(disposing);
			disposed = true;
		}


		~DB2Connection()
		{
			Dispose(false);
		}
		#endregion

		private void CheckState()
		{
			if (ConnectionState.Closed == State)
				throw new InvalidOperationException("Connection is currently closed.");
		}

		void SetConnectionString (string connectionString)
		{
			if (State != ConnectionState.Closed)
				throw new InvalidOperationException("Connection is not closed.");

			this.connectionSettings = Db2ConnectionSettings.GetConnectionSettings(connectionString);
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

		internal void AddCommand(DB2Command command)
		{
			if(refCommands == null)
			{
				refCommands = new ArrayList();
			}
			for(int i = 0; i < refCommands.Count; i++)
			{
				WeakReference reference = (WeakReference)refCommands[i];
				if((reference == null) || !reference.IsAlive)
				{
					refCommands[i] = new WeakReference(command);
					return;
				}
			}
			refCommands.Add(new WeakReference(command));

		}

		internal void RemoveCommand(DB2Command command)
		{
			for(int i = 0; i < refCommands.Count; i++)
			{
				WeakReference reference = (WeakReference)refCommands[i];
				if(object.ReferenceEquals(reference, command))
				{
					refCommands[i] = null;
					return;
				}
			}
		}

	}
}

