//
// Mono.Data.TdsClient.TdsConnectionPool.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//   Christian Hergert (christian.hergert@gmail.com)
//
// Copyright (C) 2004 Novell, Inc.
//

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
#if NET_2_0
using System.Collections.Generic;
#else
using System.Collections;
#endif
using System.Threading;

namespace Mono.Data.Tds.Protocol 
{
	public class TdsConnectionPoolManager
	{
#if NET_2_0
		Dictionary <string, TdsConnectionPool> pools = new Dictionary <string, TdsConnectionPool> ();
#else
		Hashtable pools = new Hashtable ();
#endif
		TdsVersion version;
		
		public TdsConnectionPoolManager (TdsVersion version)
		{
			this.version = version;
		}
		
		public TdsConnectionPool GetConnectionPool (string connectionString, TdsConnectionInfo info)
		{
			lock (pools)
			{
				TdsConnectionPool pool = null;
#if NET_2_0
				pools.TryGetValue (connectionString, out pool);
#else
				pool = (TdsConnectionPool) pools [connectionString];
#endif
				if (pool == null) {
					pool = new TdsConnectionPool (this, info);
					pools [connectionString] = pool;
				}
				return pool;
			}
		}

		public TdsConnectionPool GetConnectionPool (string connectionString)
		{
			TdsConnectionPool pool = null;
#if NET_2_0
			pools.TryGetValue (connectionString, out pool);
#else
			pool = (TdsConnectionPool) pools [connectionString];
#endif
			return pool;
		}

#if NET_2_0
		public IDictionary <string, TdsConnectionPool> GetConnectionPool ()
#else
		public IDictionary GetConnectionPool ()
#endif
		{
			return pools;
		}
		
		public virtual Tds CreateConnection (TdsConnectionInfo info)
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
#if NET_2_0
		Tds[] list;
#else
		object [] list;
#endif

		TdsConnectionInfo info;
		bool pooling = true;
		TdsConnectionPoolManager manager;
		ManualResetEvent connAvailable;
		
		public TdsConnectionPool (TdsConnectionPoolManager manager, TdsConnectionInfo info)
		{
			int n = 0;
			
			this.info = info;
			this.manager = manager;
#if NET_2_0
			list = new Tds[info.PoolMaxSize];
#else
			list = new object [info.PoolMaxSize];
#endif

			// Placeholder for future connections are at the beginning of the array.
			for (; n < info.PoolMaxSize - info.PoolMinSize; n++)
				list [n] = null;

			// Pre-populate with minimum number of connections
			for (; n < list.Length; n++) {
				list [n] = CreateConnection ();
			}
			
			// Event that notifies a connection is available in the pool
			connAvailable = new ManualResetEvent (false);
		}

		public bool Pooling {
			get { return pooling; }
			set { pooling = value; }
		}

		#region Methods

		public Tds GetConnection ()
		{
			Tds connection = null;
			int index;

		retry:
			// Reset the connection available event
			connAvailable.Reset ();

			index = list.Length - 1;
			
			do {
#if NET_2_0
				connection = list [index];
#else
				connection = (Tds) list [index];
#endif

				if (connection == null) {
					// Attempt take-over of array position
					connection = CreateConnection ();
					(connection as Tds).poolStatus = 1;

#if NET_2_0
					if (Interlocked.CompareExchange<Tds> (ref list [index], connection, null) != null) {
#else
					if (Interlocked.CompareExchange (ref list [index], connection, null) != null) {
#endif
						// Someone beat us to the punch
						connection = null;
					} else {
						continue;
					}
				} else {
					if (Interlocked.CompareExchange (ref (connection as Tds).poolStatus, 1, 0) != 0) {
						// Someone else owns this connection
						connection = null;
					} else {
						if (!connection.Reset ()) {
							ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), connection);
							// remove connection from pool
							list [index] = connection = null;
							// allow slot be re-used in same run
							continue;
						} else {
							continue;
						}
					}
				}
				
				index--;
				
				if (index < 0) {
					// TODO: Maintain a list of indices of released connection to save some loop over
					// Honor timeout - if pool is full, and no connections are available within the 
					// timeout period - just throw the exception
					if (info.Timeout > 0 
						&& !connAvailable.WaitOne (new TimeSpan (0, 0, info.Timeout), true))
							throw new InvalidOperationException (
								"Timeout expired. The timeout period elapsed before a " +
								"connection could be obtained. A possible explanation " +
								"is that all the connections in the pool are in use, " +
								"and the maximum pool size is reached.");
					goto retry;
				}

			} while (connection == null);

			return connection;
		}

		public void ReleaseConnection (Tds connection)
		{
			connection.poolStatus = 0;
			connAvailable.Set ();
		}

#if NET_2_0
		public void ReleaseConnection (ref Tds connection)
		{
			if (pooling == false) {
				int index = Array.IndexOf (list, connection);
				ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), connection);
				list [index] = connection = null;
			} else {
				connection.poolStatus = 0;
			}
			connAvailable.Set ();
		}

		public void ResetConnectionPool ()
		{
			Tds connection = null;
			int index = list.Length - 1;

			while (index >= 0)
			{
				connection = list [index];

				// skip free slots
				if (connection == null) {
					index--;
					continue;
				}

				if (Interlocked.CompareExchange (ref connection.poolStatus, 1, 0) == 0)
					ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), connection);
				connection.Pooling = false;

				list [index] = connection = null;
				connAvailable.Set ();

				index--;
			}
		}

		public void ResetConnectionPool (Tds connection)
		{
			int index = Array.IndexOf (list, connection);

			if (index != -1) {
				if (connection != null && !connection.Reset ()) {
					ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), connection);
					list [index] = connection = null;
					connAvailable.Set ();
				}
			}
		}
#endif

		Tds CreateConnection ()
		{
			return manager.CreateConnection (info);
		}
		
		void DestroyConnection (object state)
		{
			Tds connection = state as Tds;
			if (connection != null) {
				try {
					connection.Disconnect ();
				} finally {
					connection = null;
				}
			}
		}
		
		#endregion // Methods
	}
}
