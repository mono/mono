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
		private ArrayList			inUsePool;
		private ArrayList			idlePool;
		private int					minSize;
		private int					maxSize;

		public MySqlPool(int minSize, int maxSize)
		{
			this.minSize = minSize;
			this.maxSize = maxSize;
			inUsePool =new ArrayList(minSize);
			idlePool = new ArrayList(minSize);
		}

		private MySqlInternalConnection GetPooledConnection()
		{
			lock (idlePool.SyncRoot) 
			{
				foreach (MySqlInternalConnection conn in idlePool)
				{
					if (conn.IsAlive()) 
					{
						lock (inUsePool) 
						{
							inUsePool.Add( conn );
						}
						idlePool.Remove( conn );
						return conn;
					}
					else 
					{
						conn.Close();
						idlePool.Remove(conn);
					}
				}
			}
			return null;
		}

		private MySqlInternalConnection CreateNewPooledConnection( MySqlConnectionString settings )
		{
			MySqlInternalConnection conn = new MySqlInternalConnection( settings );
			conn.Open();
			return conn;
		}

		public void ReleaseConnection( MySqlInternalConnection connection )
		{
			lock (inUsePool.SyncRoot)
				lock (idlePool.SyncRoot) 
				{
					inUsePool.Remove( connection );
					if (connection.Settings.ConnectionLifetime != 0 && connection.IsTooOld())
						connection.Close();
					else
						idlePool.Add( connection );
				}
		}

		public MySqlInternalConnection GetConnection(MySqlConnectionString settings) 
		{
			MySqlInternalConnection conn;

			DateTime start = DateTime.Now;
			TimeSpan ts;
			do 
			{
				conn = GetPooledConnection();
				if (conn == null)
					conn = CreateNewPooledConnection( settings );
				ts = DateTime.Now.Subtract( start );
			} while (conn == null && ts.Seconds < settings.ConnectTimeout );

					 
			// if pool size is at maximum, then we must have reached our timeout so we simply
			// throw our exception
			if (conn == null)
				throw new MySqlException("error connecting: Timeout expired.  The timeout period elapsed " + 
					"prior to obtaining a connection from the pool.  This may have occurred because all " +
					"pooled connections were in use and max pool size was reached.");

			return conn;
			//System.Diagnostics.Debug.WriteLine("Creating a new driver");
/*			Driver driver = new Driver();
			try 
			{
				driver.Connection = conn;
				driver.Open( conn.DataSource, conn.Port, conn.User, conn.Password, conn.UseCompression );

				driver.SendCommand( DBCmd.INIT_DB, connString["database"] );
			}
			catch (Exception ex)
			{
				throw new MySqlException("Database initialization failed with message: " + ex.Message);
			}

			pool.Add( driver );
			return driver;*/
		}

	}
}
