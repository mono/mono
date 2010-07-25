
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace IBM.Data.DB2
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>One connection pool per connectionstring</remarks>
	internal sealed class DB2ConnectionPool
	{
		private ArrayList	openFreeConnections; // list of pooled connections sorted by age. First connection is present at index 'connectionsUsableOffset'
		private Queue		openFreeMinimalAllocated;
		private int			connectionsOpen;	// total number of connections open (in pool, an in use by application)
		private int			connectionsInUse;	// total connection in use by application
		private int			connectionsUsableOffset; // Offset to the first pooled connection in 'openFreeConnections'
		private Timer		timer;
		public string		databaseProductName;
		public string		databaseVersion;
		public int			majorVersion;
		public int			minorVersion;

		private DB2ConnectionSettings connectionSettings;

		public DB2ConnectionPool(DB2ConnectionSettings connectionSettings)
		{
			this.connectionSettings = connectionSettings;
			openFreeConnections = new ArrayList();
		}

		public DB2ConnectionSettings ConnectionSettings
		{
			get { return connectionSettings; }
		}

		public DB2OpenConnection GetOpenConnection(DB2Connection db2Conn)
		{
			DB2OpenConnection connection = null;
			lock(openFreeConnections.SyncRoot)
			{
				if((connectionSettings.ConnectionPoolSizeMax > 0) &&
					(connectionsOpen >= connectionSettings.ConnectionPoolSizeMax))
				{
					throw new ArgumentException("Maximum connections reached for connectionstring");
				}

				while(connectionsOpen > connectionsInUse)
				{
					connection = (DB2OpenConnection)openFreeConnections[openFreeConnections.Count - 1];
					openFreeConnections.RemoveAt(openFreeConnections.Count - 1);

					// check if connection is dead
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
				if(connectionsOpen == connectionsInUse)
				{
					if(timer != null)
					{
						timer.Dispose();
						timer = null;
					}
				}
			}
			if(connection == null)
			{
				openFreeConnections.Clear();
				connectionsUsableOffset = 0;

				connection = new DB2OpenConnection(connectionSettings, db2Conn);
				connectionsOpen++;
				connectionsInUse++;
			}

			return connection;
		}

		private void DisposeTimedoutConnections(object state)
		{
			lock(openFreeConnections.SyncRoot)
			{
				if(timer != null)
				{
					TimeSpan timeToDispose = TimeSpan.Zero;
					DB2OpenConnection connection;
					while(connectionsOpen > connectionsInUse)
					{
						connection = (DB2OpenConnection)openFreeConnections[connectionsUsableOffset];
						timeToDispose = connection.poolDisposalTime.Subtract(DateTime.Now);
						if((timeToDispose.Ticks < 0) ||								 // time to die
							(timeToDispose > connectionSettings.ConnectionLifeTime)) // messing with system clock
						{
							connection.Dispose();
							openFreeConnections[connectionsUsableOffset] = null;
							connectionsOpen--;
							connectionsUsableOffset++;
						}
						else
						{
							break;
						}
					}
					if(connectionsOpen > connectionsInUse)
					{
						connection = (DB2OpenConnection)openFreeConnections[connectionsUsableOffset];
						timer.Change(timeToDispose, new TimeSpan(-1));
					}
					else
					{
						timer.Dispose();
						timer = null;
					}
				}
				if((connectionsUsableOffset > (openFreeConnections.Capacity / 2)) &&
					(connectionsOpen > connectionsInUse))
				{
					openFreeConnections.RemoveRange(0, connectionsUsableOffset); // cleanup once in a while
					connectionsUsableOffset = 0;
				}
			}
		}

		public void AddToFreeConnections(DB2OpenConnection connection)
		{
			lock(openFreeConnections.SyncRoot)
			{
				connection.poolDisposalTime = DateTime.Now.Add(connectionSettings.ConnectionLifeTime);
				if(timer == null)
				{
					timer = new Timer(new TimerCallback(DisposeTimedoutConnections), null, 
						connectionSettings.ConnectionLifeTime, new TimeSpan(-1));
				}
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
		static public DB2ConnectionPool FindConnectionPool(string connectionString)
		{
			return (DB2ConnectionPool)DB2Environment.Instance.connectionPools[connectionString];
		}

		/// <summary>
		/// Get a connection pool. If it doesn't exist yet, create it
		/// </summary>
		/// <param name="connectionSettings"></param>
		/// <returns></returns>
		static public DB2ConnectionPool GetConnectionPool(DB2ConnectionSettings connectionSettings)
		{
			DB2Environment environment = DB2Environment.Instance;

			lock(environment.connectionPools.SyncRoot)
			{
				DB2ConnectionPool pool = (DB2ConnectionPool)environment.connectionPools[connectionSettings.ConnectionString];
				if(pool == null)
				{
					pool = new DB2ConnectionPool(connectionSettings);
					environment.connectionPools.Add(connectionSettings.ConnectionString, pool);
				}
				return pool;
			}
		}
	}
}
