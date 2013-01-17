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
using System.Text;
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
			//Console.WriteLine ("CreateConnection: TdsVersion:{0}", version);
			switch (version)
			{
				case TdsVersion.tds42:
					return new Tds42 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
				case TdsVersion.tds50:
					return new Tds50 (info.DataSource, info.Port, info.PacketSize, info.Timeout);
				case TdsVersion.tds70:
					return new Tds70 (info.DataSource, info.Port, info.PacketSize, info.Timeout, info.LifeTime);
				case TdsVersion.tds80:
					return new Tds80 (info.DataSource, info.Port, info.PacketSize, info.Timeout, info.LifeTime);
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
		[Obsolete ("Use the constructor that receives a lifetime parameter")]
		public TdsConnectionInfo (string dataSource, int port, int packetSize, int timeout, int minSize, int maxSize)
			: this (dataSource, port, packetSize, timeout, minSize, maxSize, 0)
		{
		}

		public TdsConnectionInfo (string dataSource, int port, int packetSize, int timeout, int minSize, int maxSize, int lifeTime)
		{
			DataSource = dataSource;
			Port = port;
			PacketSize = packetSize;
			Timeout = timeout;
			PoolMinSize = minSize;
			PoolMaxSize = maxSize;
			LifeTime = lifeTime;
		}
		
		public string DataSource;
		public int Port;
		public int PacketSize;
		public int Timeout;
		public int LifeTime;
		public int PoolMinSize;
		public int PoolMaxSize;

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("DataSouce: {0}\n", DataSource);
			sb.AppendFormat ("Port: {0}\n", Port);
			sb.AppendFormat ("PacketSize: {0}\n", PacketSize);
			sb.AppendFormat ("Timeout: {0}\n", Timeout);
			sb.AppendFormat ("PoolMinSize: {0}\n", PoolMinSize);
			sb.AppendFormat ("PoolMaxSize: {0}", PoolMaxSize);
			return sb.ToString ();
		}
	}

	public class TdsConnectionPool
	{
		TdsConnectionInfo info;
		bool no_pooling;
		TdsConnectionPoolManager manager;
		Queue available;
		ArrayList conns;
		
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
			/* conns.Count might not be 0 when we are resetting the connection pool */
			for (int i = conns.Count; i < info.PoolMinSize; i++) {
				try {
					Tds t = manager.CreateConnection (info);
					conns.Add (t);
					available.Enqueue (t);
				} catch {
					// Ignore. GetConnection will throw again.
				}
			}
		}

		public bool Pooling {
			get { return !no_pooling; }
			set { no_pooling = !value; }
		}

		#region Methods

		int in_progress;
		public Tds GetConnection ()
		{
			if (no_pooling)
				return manager.CreateConnection (info);

			Tds result = null;
			bool create_new;
			int retries = info.PoolMaxSize * 2;
retry:
			while (result == null) {
				create_new = false;
				lock (available) {
					if (available.Count > 0) {
						result = (Tds) available.Dequeue ();
						break; // .. and do the reset out of the loop
					}
					Monitor.Enter (conns);
					try {
						if (conns.Count >= info.PoolMaxSize - in_progress) {
							Monitor.Exit (conns);
							bool got_lock = Monitor.Wait (available, info.Timeout * 1000);
							if (!got_lock) {
								throw new InvalidOperationException (
									"Timeout expired. The timeout period elapsed before a " +
									"connection could be obtained. A possible explanation " +
									"is that all the connections in the pool are in use, " +
									"and the maximum pool size is reached.");
							} else if (available.Count > 0) {
								result = (Tds) available.Dequeue ();
								break; // .. and do the reset out of the loop
							}
							continue;
						} else {
							create_new = true;
							in_progress++;
						}
					} finally {
						Monitor.Exit (conns); // Exiting if not owned is ok < 2.x
					}
				}
				if (create_new) {
					try {
						result = manager.CreateConnection (info);
						lock (conns)
							conns.Add (result);
						return result;
					} finally {
						lock (available)
							in_progress--;
					}
				}
			}

			bool remove_cnc = true;
			Exception exc = null;
			try {
				remove_cnc = (!result.IsConnected || !result.Reset ());
			} catch (Exception e) {
				remove_cnc = true;
				exc = e;
			}
			if (remove_cnc) {
				lock (conns)
					conns.Remove (result);
				result.Disconnect ();
				retries--;
				if (retries == 0)
					throw exc;
				result = null;
				goto retry;
			}
			return result;
		}

		public void ReleaseConnection (Tds connection)
		{
			if (connection == null)
				return;
			if (no_pooling) {
				connection.Disconnect ();
				return;
			}

			if (connection.poolStatus == 2 || connection.Expired) {
				lock (conns)
					conns.Remove (connection);
				connection.Disconnect ();
				connection = null;
			}
			lock (available) {
				if (connection != null) // connection is still open
 					available.Enqueue (connection);
				// We pulse even if we don't queue, because null means that there's a slot
				// available in 'conns'
 				Monitor.Pulse (available);
			}
		}

#if NET_2_0
		public void ResetConnectionPool ()
		{
			lock (available) {
				lock (conns) {
					Tds tds;
					int i;
					for (i = conns.Count - 1; i >= 0; i--) {
						tds = (Tds) conns [i];
						tds.poolStatus = 2; // 2 -> disconnect me upon release
					}
					for (i = available.Count - 1; i >= 0; i--) {
						tds = (Tds) available.Dequeue ();
						tds.Disconnect ();
						conns.Remove (tds);
					}
					available.Clear ();
					InitializePool ();
				}
				Monitor.PulseAll (available);
			}
		}
#endif
		#endregion // Methods
	}
}

