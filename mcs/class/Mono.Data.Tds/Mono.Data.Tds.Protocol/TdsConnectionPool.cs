//
// Mono.Data.TdsClient.TdsConnectionPool.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//   Christian Hergert (christian.hergert@gmail.com)
//   Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2004 Novell, Inc.
// Copyright (C) 2009 Novell, Inc.

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
using System.Threading;

namespace Mono.Data.Tds.Protocol 
{
	public class TdsConnectionPoolManager
	{
		Hashtable pools = Hashtable.Synchronized (new Hashtable ());
		TdsVersion version;
		
		public TdsConnectionPoolManager (TdsVersion version)
		{
			this.version = version;
		}
		
		public TdsConnectionPool GetConnectionPool (string connectionString, TdsConnectionInfo info)
		{
			TdsConnectionPool pool = (TdsConnectionPool) pools [connectionString];
			if (pool == null) {
				pools [connectionString] = new TdsConnectionPool (this, info);
				pool = (TdsConnectionPool) pools [connectionString];
			}
			return pool;
		}

		public TdsConnectionPool GetConnectionPool (string connectionString)
		{
			return (TdsConnectionPool) pools [connectionString];
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

		public IDictionary GetConnectionPool ()
		{
			return pools;
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
		TdsConnectionInfo info;
		bool no_pooling;
		TdsConnectionPoolManager manager;
		Queue available;
		ArrayList conns;
		object next_free;
		
		public TdsConnectionPool (TdsConnectionPoolManager manager, TdsConnectionInfo info)
		{
			this.info = info;
			this.manager = manager;
			conns = new ArrayList (info.PoolMaxSize);
			available = new Queue (info.PoolMaxSize);
			InitializePool ();
		}

		void InitializePool ()
		{
			for (int i = 0; i < info.PoolMinSize; i++) {
				if (i == 0) {
					next_free = manager.CreateConnection (info);
					conns.Add (next_free);
				} else {
					Tds t = manager.CreateConnection (info);
					conns.Add (t);
					available.Enqueue (t);
				}
			}
		}

		public bool Pooling {
			get { return !no_pooling; }
			set { no_pooling = !value; }
		}

		#region Methods

		public Tds GetConnection ()
		{
			if (no_pooling)
				return manager.CreateConnection (info);

retry:
			Tds result = (Tds) Interlocked.Exchange (ref next_free, null);
			if (result != null && !result.IsConnected)
				result = null;

			while (result == null) {
				lock (available) {
					if (available.Count > 0) {
						result = (Tds) available.Dequeue ();
						break; // .. and do the reset out of the loop
					} else {
						result = (Tds) Interlocked.Exchange (ref next_free, null);
						if (result != null && result.IsConnected)
							break;
						result = null;
					}
					Monitor.Enter (conns);
					try {
						if (conns.Count >= info.PoolMaxSize) {
							Monitor.Exit (conns);
							//Console.WriteLine ("GONZ: ENTERING LOCK");
							bool got_lock = Monitor.Wait (available, info.Timeout * 1000);
							if (!got_lock) {
								throw new InvalidOperationException (
									"Timeout expired. The timeout period elapsed before a " +
									"connection could be obtained. A possible explanation " +
									"is that all the connections in the pool are in use, " +
									"and the maximum pool size is reached.");
							}
							continue;
						}
					} finally {
						Monitor.Exit (conns); // Exiting if not owned is ok
					}
				}
				lock (conns) {
					if (conns.Count < info.PoolMaxSize) {
						try {
							//Console.WriteLine ("GONZ: NEW");
							result = manager.CreateConnection (info);
							conns.Add (result);
							return result; // no reset needed
						} catch {
						}
						continue;
					}
				}
			}

			if (!result.IsConnected || !result.Reset ()) {
				//Console.WriteLine ("GONZ: RESET FAILED");
				lock (conns)
					conns.Remove (result);
				ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), result);
				goto retry;
			}
			//Console.WriteLine ("GONZ: REUSED");
			return result;
		}

		public void ReleaseConnection (Tds connection)
		{
			if (no_pooling) {
				ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), connection);
				return;
			}
			lock (available) {
				if (Interlocked.CompareExchange (ref next_free, connection, null) != null)
					available.Enqueue (connection);
				Monitor.Pulse (available);
			}
		}

#if NET_2_0
		public void ResetConnectionPool ()
		{
			lock (available) {
				available.Clear ();
				lock (conns) {
					for (int i = 0; i < conns.Count; i++) {
						Tds tds = (Tds) conns [i];
						ThreadPool.QueueUserWorkItem (new WaitCallback (DestroyConnection), tds);
					}
					conns.Clear ();
					InitializePool ();
					Monitor.PulseAll (available);
				}
			}
		}
#endif

		static void DestroyConnection (object state)
		{
			Tds connection = state as Tds;
			if (connection != null) {
				connection.Disconnect ();
			}
		}
		#endregion // Methods
	}
}

