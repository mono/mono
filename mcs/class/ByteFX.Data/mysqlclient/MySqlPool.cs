using System;
using ByteFX.Data.Common;
using System.Collections;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlPool.
	/// </summary>
	internal sealed class MySqlPool
	{
		private ArrayList				inUsePool;
		private ArrayList				idlePool;
		private MySqlConnectionString	settings;
		private int						minSize;
		private int						maxSize;

		public MySqlPool(MySqlConnectionString settings)
		{
			minSize = settings.MinPoolSize;
			maxSize = settings.MaxPoolSize;
			this.settings = settings;
			inUsePool =new ArrayList();
			idlePool = new ArrayList( settings.MinPoolSize );

			// prepopulate the idle pool to minSize
			for (int i=0; i < minSize; i++) 
				CreateNewPooledConnection();
		}

		private void CheckOutConnection(MySqlInternalConnection conn) 
		{
		}

		private MySqlInternalConnection GetPooledConnection()
		{
			MySqlInternalConnection conn = null;

			// if there are no idle connections and the in use pool is full
			// then return null to indicate that we cannot provide a connection
			// at this time.
			if (idlePool.Count == 0 && inUsePool.Count == maxSize) return null;

			lock (idlePool.SyncRoot) 
			{
				for (int i=idlePool.Count-1; i >=0; i--)
				{
					conn = (idlePool[i] as MySqlInternalConnection);
					if (conn.IsAlive()) 
					{
						lock (inUsePool) 
						{
							inUsePool.Add( conn );
						}
						idlePool.RemoveAt( i );
						return conn;
					}
					else 
					{
						conn.Close();
						idlePool.RemoveAt(i);
						conn = null;
					}
				}
			}

			// if we couldn't get a pooled connection and there is still room
			// make a new one
			if (conn == null && (idlePool.Count+inUsePool.Count) < maxSize)
			{
				conn = CreateNewPooledConnection();
				if (conn == null) return null;

				lock (idlePool.SyncRoot)
					lock (inUsePool.SyncRoot) 
					{
						idlePool.Remove( conn );
						inUsePool.Add( conn );
					}
			}

			return conn;
		}

		private MySqlInternalConnection CreateNewPooledConnection()
		{
			lock(idlePool.SyncRoot) 
				lock (inUsePool.SyncRoot)
				{
					// first we check if we are allowed to create another
					if ((inUsePool.Count + idlePool.Count) == maxSize) return null;

					MySqlInternalConnection conn = new MySqlInternalConnection( settings );
					conn.Open();
					idlePool.Add( conn );
					return conn;
				}
		}

		public void ReleaseConnection( MySqlInternalConnection connection )
		{
			lock (idlePool.SyncRoot)
				lock (inUsePool.SyncRoot) 
				{
					inUsePool.Remove( connection );
					if (connection.Settings.ConnectionLifetime != 0 && connection.IsTooOld())
						connection.Close();
					else
						idlePool.Add( connection );
				}
		}

		public MySqlInternalConnection GetConnection() 
		{
			MySqlInternalConnection conn = null;

			int start = Environment.TickCount;
			int ticks = settings.ConnectionTimeout * 1000;

			// wait timeOut seconds at most to get a connection
			while (conn == null && (Environment.TickCount - start) < ticks)
				conn = GetPooledConnection();
					 
			// if pool size is at maximum, then we must have reached our timeout so we simply
			// throw our exception
			if (conn == null)
				throw new MySqlException("error connecting: Timeout expired.  The timeout period elapsed " + 
					"prior to obtaining a connection from the pool.  This may have occurred because all " +
					"pooled connections were in use and max pool size was reached.");

			return conn;
		}

	}
}
