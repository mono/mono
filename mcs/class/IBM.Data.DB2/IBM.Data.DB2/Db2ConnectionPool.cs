using System;
using System.Collections;
using System.Text;

namespace IBM.Data.DB2
{
	internal sealed class Db2Environment
	{
		private static Db2Environment environment;
		internal Hashtable connectionPools;
		internal IntPtr penvHandle = IntPtr.Zero;

		private static System.IO.StreamWriter log;
		public static void Log(string format, params object[] args)
		{
			if(log == null)
			{
				log = new System.IO.StreamWriter(DB2Constants.LOG_PATH, false, System.Text.Encoding.UTF8);
			}
			if(args[0] is IntPtr)
			{
				args[0] = ((IntPtr)args[0]).ToInt32() / 0x1000 + ":" + ((IntPtr)args[0]).ToInt32() % 0x1000;
			}
			log.WriteLine(format, args);
			log.Flush();
			return;
		}

		private Db2Environment()
		{
			connectionPools = Hashtable.Synchronized(new Hashtable());

			short sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_ENV, IntPtr.Zero, ref penvHandle);
			Db2Environment.Log("Alloc env {0,-4}  {1}", penvHandle, sqlRet);
			DB2ClientUtils.DB2CheckReturn(sqlRet, 0, IntPtr.Zero, "Unable to allocate Environment handle.");

			// SQLSetEnvAttr( hEnv=0:1, fAttribute=SQL_ATTR_APP_TYPE 2473, vParam=4, cbParam=0 )	// 4=ADO.NET apptype????
			// SQLSetEnvAttr( hEnv=0:1, fAttribute=SQL_ATTR_OUTPUT_NTS 10001, vParam=0, cbParam=0 ) // strings not 0-terminated
		}

		public static Db2Environment Instance
		{
			get
			{
				if(environment == null)
				{
					lock(typeof(Db2Environment))
					{
						if(environment == null)
						{
							environment = new Db2Environment();
						}
					}
				}
				return environment;
			}
		}

	}

	internal sealed class Db2OpenConnection : IDisposable
	{
		private IntPtr dbHandle = IntPtr.Zero;

		private Db2ConnectionSettings settings;
		private bool disposed = false;
		

		public IntPtr DBHandle
		{
			get { return dbHandle; }
		}
		
		public Db2OpenConnection(Db2ConnectionSettings settings)
		{
			if(dbHandle == IntPtr.Zero)
			{
				this.settings = settings;
				try
				{
					short sqlRet = DB2CLIWrapper.SQLAllocHandle(DB2Constants.SQL_HANDLE_DBC, Db2Environment.Instance.penvHandle, ref dbHandle);
					Db2Environment.Log("Alloc dbc {0,-4}  {1}", dbHandle, sqlRet);
					DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, Db2Environment.Instance.penvHandle, "Unable to allocate database handle in Db2Connection.");

					Console.WriteLine("DB2OpenConnection SQLConnect... User: {0}, Password: {1}", settings.UserName, settings.PassWord);

					/*if(settings.Server.Length > 0)
					{
						StringBuilder outConnectStr = new StringBuilder(60);  // TODO: ????
						IntPtr numOutCharsReturned = IntPtr.Zero;

						sqlRet = DB2CLIWrapper.SQLDriverConnect(dbHandle, 0, settings.ConnectionString,
							settings.ConnectionString.Length, outConnectStr, 100, numOutCharsReturned, 
							DB2Constants.SQL_DRIVER_COMPLETE);
						DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, dbHandle, "Unable to connect to the database.");
					}
					else
					{*/
						sqlRet = DB2CLIWrapper.SQLConnect(dbHandle, 
							settings.DatabaseAlias,	(short)settings.DatabaseAlias.Length,
							settings.UserName,		(short)settings.UserName.Length,
							settings.PassWord,		(short)settings.PassWord.Length);

						Db2Environment.Log("Connect   {0,-4}  {1}", dbHandle, sqlRet);
						DB2ClientUtils.DB2CheckReturn(sqlRet, DB2Constants.SQL_HANDLE_DBC, dbHandle, "Unable to connect to the database.");
					//}
				}
				catch
				{
					if(dbHandle != IntPtr.Zero)
					{
						DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_DBC, dbHandle);
						dbHandle = IntPtr.Zero;
					}
					throw;
				}
			}
		}

		public void Close()
		{
			if(settings.Pool != null)
			{
				settings.Pool.AddToFreeConnections(this);
			}
			else
			{
				Dispose();
			}
		}

		private void FreeHandles()
		{
			if(dbHandle != IntPtr.Zero)
			{
				short sqlRet = DB2CLIWrapper.SQLDisconnect(dbHandle);
				Db2Environment.Log("Disconnec {0,-4}  {1}", dbHandle, sqlRet);
				// Note that SQLDisconnect() automatically drops any statements and
				// descriptors open on the connection.
				sqlRet = DB2CLIWrapper.SQLFreeHandle(DB2Constants.SQL_HANDLE_DBC, dbHandle);
				Db2Environment.Log("Free  dbc {0,-4}  {1}", dbHandle, sqlRet);

				dbHandle = IntPtr.Zero;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		internal void Dispose(bool disposing)
		{
			if(!disposed) 
			{
				if(disposing)
				{
					// dispose managed resources
				}
				FreeHandles();
			}
			disposed = true;
		}


		~Db2OpenConnection()
		{
			if(settings.Pool != null)
			{
				settings.Pool.OpenConnectionFinalized();
			}
			Dispose(false);
		}
		#endregion
	}

	internal sealed class Db2ConnectionPool
	{
		private ArrayList	openFreeConnections;
		private int			connectionsOpen;
		private int			connectionsInUse;
		//private int			connectionsUsableOffset;

		private Db2ConnectionSettings connectionSettings;

		public Db2ConnectionPool(Db2ConnectionSettings connectionSettings)
		{
			this.connectionSettings = connectionSettings;
			openFreeConnections = new ArrayList();
		}

		public Db2ConnectionSettings ConnectionSettings
		{
			get { return connectionSettings; }
		}


		public Db2OpenConnection GetOpenConnection()
		{
			Db2OpenConnection connection = null;
			lock(openFreeConnections.SyncRoot)
			{
				while(connectionsOpen > connectionsInUse)
				{
					connection = (Db2OpenConnection)openFreeConnections[openFreeConnections.Count - 1];
					openFreeConnections.RemoveAt(openFreeConnections.Count - 1);

					// check if connection is s
					int isDead;
					short sqlRet = DB2CLIWrapper.SQLGetConnectAttr(connection.DBHandle, DB2Constants.SQL_ATTR_CONNECTION_DEAD, out isDead, 0, IntPtr.Zero);
					if(((sqlRet == DB2Constants.SQL_SUCCESS_WITH_INFO) || (sqlRet == DB2Constants.SQL_SUCCESS)) &&
						(isDead == DB2Constants.SQL_CD_FALSE))
					{
						connectionsInUse++;
						break;
					}
					else
					{
						connectionsOpen--;
						connection.Dispose();
						connection = null;
					}

				}
			}
			if(connection == null)
			{
				connection = new Db2OpenConnection(connectionSettings);
				connectionsOpen++;
				connectionsInUse++;
			}

			return connection;
		}

		public void AddToFreeConnections(Db2OpenConnection connection)
		{
			lock(openFreeConnections.SyncRoot)
			{
				connectionsInUse--;
				openFreeConnections.Add(connection);
			}
		}

		public void OpenConnectionFinalized()
		{
			lock(openFreeConnections.SyncRoot)
			{
				connectionsOpen--;
				connectionsInUse--;
			}
		}

		/// <summary>
		/// Find a specific connection pool
		/// </summary>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		static public Db2ConnectionPool FindConnectionPool(string connectionString)
		{
			return (Db2ConnectionPool)Db2Environment.Instance.connectionPools[connectionString];
		}

		/// <summary>
		/// Get a connection pool. If it doesn't exist yet, create it
		/// </summary>
		/// <param name="connectionSettings"></param>
		/// <returns></returns>
		static public Db2ConnectionPool GetConnectionPool(Db2ConnectionSettings connectionSettings)
		{
			Db2Environment environment = Db2Environment.Instance;

			lock(environment.connectionPools.SyncRoot)
			{
				Db2ConnectionPool pool = (Db2ConnectionPool)environment.connectionPools[connectionSettings.ConnectionString];
				if(pool == null)
				{
					pool = new Db2ConnectionPool(connectionSettings);
					environment.connectionPools.Add(connectionSettings.ConnectionString, pool);
				}
				return pool;
			}
		}
	}
}
