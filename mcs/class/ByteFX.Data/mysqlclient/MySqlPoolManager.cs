using System;
using ByteFX.Data.Common;
using System.Collections;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlPoolManager.
	/// </summary>
	internal sealed class MySqlPoolManager
	{
		private static Hashtable	pools;

		public MySqlPoolManager() 
		{
		}

		/// <summary>
		/// 
		/// </summary>
		private static void Initialize()
		{
			pools = new Hashtable();
		}

		public static MySqlInternalConnection GetConnection( MySqlConnectionString settings ) 
		{
			// make sure the manager is initialized
			if (MySqlPoolManager.pools == null)
				MySqlPoolManager.Initialize();

			string text = settings.GetConnectionString();

			lock( pools.SyncRoot ) 
			{
				MySqlPool pool;
				if (!pools.Contains( text )) 
				{
					pool = new MySqlPool( settings );
					pools.Add( text, pool );
				}
				else 
				{
					pool = (pools[text] as MySqlPool);
				}

				return pool.GetConnection();
			}
		}

		public static void ReleaseConnection( MySqlInternalConnection connection )
		{
			lock (pools.SyncRoot) 
			{
				string key = connection.Settings.GetConnectionString();
				MySqlPool pool = (MySqlPool)pools[ key ];
				if (pool == null)
					throw new MySqlException("Pooling exception: Unable to find original pool for connection");
				pool.ReleaseConnection(connection);
			}
		}
	}
}
