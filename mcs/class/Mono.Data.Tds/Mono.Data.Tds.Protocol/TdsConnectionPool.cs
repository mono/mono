//
// Mono.Data.TdsClient.TdsConnectionPool.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.Threading;

namespace Mono.Data.Tds.Protocol 
{
	public class TdsConnectionPoolManager
	{
		Hashtable pools = new Hashtable ();
		TdsVersion version;
		
		public TdsConnectionPoolManager (TdsVersion version)
		{
			this.version = version;
		}
		
		public TdsConnectionPool GetConnectionPool (string connectionString, TdsConnectionInfo info)
		{
			lock (pools)
			{
				TdsConnectionPool pool = (TdsConnectionPool) pools [connectionString];
				if (pool == null) {
					pool = new TdsConnectionPool (this, info);
					pools [connectionString] = pool;
				}
				return pool;
			}
		}
		
		public virtual ITds CreateConnection (TdsConnectionInfo info)
		{
			switch (version)
			{
				case TdsVersion.tds42:
					return new Tds42 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
				case TdsVersion.tds50:
					return new Tds50 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
				case TdsVersion.tds70:
					return new Tds70 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
				case TdsVersion.tds80:
					return new Tds80 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
			}
			throw new NotSupportedException ();
		}
	}
	
	public class TdsConnectionInfo
	{
		public TdsConnectionInfo (string dataSource, int port, int packetSize, int timeout, int minSize, int maxSize)
		{
			DataSource = dataSource;
			Port = port;
			PacketSize = packetSize;
			Timeout = timeout;
			PoolMinSize = minSize;
			PoolMaxSize = maxSize;
		}
		
		public string DataSource;
		public int Port;
		public int PacketSize;
		public int Timeout;
		public int PoolMinSize;
		public int PoolMaxSize;
	}
	
	public class TdsConnectionPool
	{
		ArrayList list = new ArrayList ();
		TdsConnectionInfo info;
		bool initialized;
		int activeConnections = 0;
		TdsConnectionPoolManager manager;

		public TdsConnectionPool (TdsConnectionPoolManager manager, TdsConnectionInfo info)
		{
			this.info = info;
			this.manager = manager;
		}

		#region Methods

		public ITds GetConnection ()
		{
			ITds connection = null;
				
			lock (list)
			{
				if (!initialized) 
				{
					for (int n=0; n<info.PoolMinSize; n++)
						list.Add (CreateConnection ());
					initialized = true;
				}
				
				do {
					if (list.Count > 0) 
					{
						// There are available connections
						connection = (ITds) list [list.Count - 1];
						list.RemoveAt (list.Count - 1);
						if (!connection.Reset ()) {
							connection = null;
							continue;
						}
					}

					if (connection == null && activeConnections < info.PoolMaxSize)
					{
						// No connections available, but the connection limit
						// has not been reached yet, so a new one can be created
						connection = CreateConnection();
					}

					// No available connections in the pool
					// Wait for somewone to release one.
					if (connection == null)
					{
						Monitor.Wait (list);
					}
				} 
				while (connection == null);
			}

			return connection;
		}

		public void ReleaseConnection (ITds tds)
		{
			lock (list)
			{
				list.Add (tds);
				Monitor.Pulse (list);
			}
		}
		
		ITds CreateConnection ()
		{
			activeConnections++;
			return manager.CreateConnection (info);
		}
		
		#endregion // Methods
	}
}
