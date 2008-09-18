//
// System.Runtime.Remoting.Channels.Tcp.TcpConnectionPool.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Lluis Sanchez Gual
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
using System.Collections;
using System.Threading;
using System.IO;
using System.Net.Sockets;

namespace System.Runtime.Remoting.Channels.Tcp
{
	// This is a pool of Tcp connections. Connections requested
	// by the TCP channel are pooled after their use, and can
	// be reused later. Connections are automaticaly closed
	// if not used after some time, specified in KeepAliveSeconds.
	// The number of allowed open connections can also be specified
	// in MaxOpenConnections. The limit is per host.
	// If a thread requests a connection and the limit has been 
	// reached, the thread is suspended until one is released.

	internal class TcpConnectionPool
	{
		// Table of pools. There is a HostConnectionPool 
		// instance for each host
		static Hashtable _pools = new Hashtable();

		static int _maxOpenConnections = int.MaxValue;
		static int _keepAliveSeconds = 15;

		static Thread _poolThread;

		static TcpConnectionPool()
		{
			// This thread will close unused connections
			_poolThread = new Thread (new ThreadStart (ConnectionCollector));
			_poolThread.IsBackground = true;
			_poolThread.Start();
		}

		public static void Shutdown ()
		{
#if !TARGET_JVM
			if (_poolThread != null)
				_poolThread.Abort();
#endif
		}

		public static int MaxOpenConnections
		{
			get { return _maxOpenConnections; }
			set 
			{ 
				if (value < 1) throw new RemotingException ("MaxOpenConnections must be greater than zero");
				_maxOpenConnections = value; 
			}
		}

		public static int KeepAliveSeconds
		{
			get { return _keepAliveSeconds; }
			set { _keepAliveSeconds = value; }
		}

		public static TcpConnection GetConnection (string host, int port)
		{
			HostConnectionPool hostPool;

			lock (_pools)
			{
				string key = host + ":" + port;
				hostPool = (HostConnectionPool) _pools[key];
				if (hostPool == null)
				{
					hostPool = new HostConnectionPool(host, port);
					_pools[key] = hostPool;
				}
			}

			return hostPool.GetConnection();
		}

		private static void ConnectionCollector ()
		{
			while (true)
			{
				Thread.Sleep(3000);
				lock (_pools)
				{
					ICollection values = _pools.Values;
					foreach (HostConnectionPool pool in values)
						pool.PurgeConnections();
				}
			}
		}
	}

	internal class ReusableTcpClient : TcpClient
	{
		public ReusableTcpClient (string host, int port): base (host, port)
		{
			// Avoid excessive waiting for data by the tcp stack in linux.
			// We can't safely use SetSocketOption for both runtimes because
			// it would break 2.0 TcpClient's property cache.
#if NET_2_0
			Client.NoDelay = true;
#else
			Client.SetSocketOption (SocketOptionLevel.Tcp,
						SocketOptionName.NoDelay, 1);
#endif
		}
		
		public bool IsAlive
		{
			get
			{
				// This Poll will return true if there is data pending to
				// be read. It prob. means that a client object using this
				// connection got an exception and did not finish to read
				// the data. It can also mean that the connection has been
				// closed in the server. In both cases, the connection cannot
				// be reused.
				return !Client.Poll (0, SelectMode.SelectRead);
			}
		}
	}

	internal class TcpConnection
	{
		DateTime _controlTime;
		Stream _stream;
		ReusableTcpClient _client;
		HostConnectionPool _pool;
		byte[] _buffer;

		public TcpConnection (HostConnectionPool pool, ReusableTcpClient client)
		{
			_pool = pool;
			_client = client;
			_stream = new BufferedStream (client.GetStream());
			_controlTime = DateTime.Now;
			_buffer = new byte[TcpMessageIO.DefaultStreamBufferSize];
		}

		public Stream Stream
		{
			get { return _stream; }
		}

		public DateTime ControlTime
		{
			get { return _controlTime; }
			set { _controlTime = value; }
		}

		public bool IsAlive
		{
			get { return _client.IsAlive; }
		}

		// This is a "thread safe" buffer that can be used by 
		// TcpClientTransportSink to read or send data to the stream.
		// The buffer is "thread safe" since only one thread can
		// use a connection at a given time.
		public byte[] Buffer
		{
			get { return _buffer; }
		}

		// Returns the connection to the pool
		public void Release()
		{
			_pool.ReleaseConnection (this);
		}

		public void Close()
		{
			_client.Close();
		}
	}

	internal class HostConnectionPool
	{
		ArrayList _pool = new ArrayList();
		int _activeConnections = 0;

		string _host;
		int _port;

		public HostConnectionPool (string host, int port)
		{
			_host = host;
			_port = port;
		}

		public TcpConnection GetConnection ()
		{
			TcpConnection connection = null;
			lock (_pool)
			{
				do
				{
					if (_pool.Count > 0) 
					{
						// There are available connections

						connection = (TcpConnection)_pool[_pool.Count - 1];
						_pool.RemoveAt(_pool.Count - 1);
						if (!connection.IsAlive) {
							CancelConnection (connection);
							connection = null;
							continue;
						}
					}

					if (connection == null && _activeConnections < TcpConnectionPool.MaxOpenConnections)
					{
						// No connections available, but the max connections
						// has not been reached yet, so a new one can be created
						// Create the connection outside the lock
						break;
					}

					// No available connections in the pool
					// Wait for somewone to release one.

					if (connection == null)
					{
						Monitor.Wait(_pool);
					}
				} 
				while (connection == null);
			}

			if (connection == null)
				return CreateConnection ();
			else
				return connection;
		}

		private TcpConnection CreateConnection()
		{
			try
			{
				ReusableTcpClient client = new ReusableTcpClient(_host, _port);
				TcpConnection entry = new TcpConnection(this, client);
				_activeConnections++;
				return entry;
			}
			catch (Exception ex)
			{
				throw new RemotingException (ex.Message);
			}
		}

		public void ReleaseConnection (TcpConnection entry)
		{
			lock (_pool)
			{
				entry.ControlTime = DateTime.Now;	// Initialize timeout
				_pool.Add (entry);
				Monitor.Pulse (_pool);
			}
		}

		private void CancelConnection(TcpConnection entry)
		{
			try
			{
				entry.Stream.Close();
				_activeConnections--;
			}
			catch
			{
			}
		}

		public void PurgeConnections()
		{
			lock (_pool)
			{
				for (int n=0; n < _pool.Count; n++)
				{
					TcpConnection entry = (TcpConnection)_pool[n];
					if ( (DateTime.Now - entry.ControlTime).TotalSeconds > TcpConnectionPool.KeepAliveSeconds)
					{
						CancelConnection (entry);
						_pool.RemoveAt(n);
						n--;
					}
				}
			}
		}

	}


}
