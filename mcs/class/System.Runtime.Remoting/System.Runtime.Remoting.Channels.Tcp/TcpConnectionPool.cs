//
// System.Runtime.Remoting.Channels.Tcp.TcpConnectionPool.cs
//
// Author: Lluis Sanchez (lsg@ctv.es)
//
// 2002 (C) Lluis Sanchez Gual
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

		static int _maxOpenConnections = 2;
		static int _keepAliveSeconds = 15;

		static TcpConnectionPool()
		{
			// This thread will close unused connections
			Thread t = new Thread (new ThreadStart (ConnectionCollector));
			t.Start();
			t.IsBackground = true;
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

	internal class TcpConnection
	{
		DateTime _controlTime;
		Stream _stream;
		TcpClient _client;
		HostConnectionPool _pool;
		byte[] _buffer;

		public TcpConnection (HostConnectionPool pool, TcpClient client)
		{
			_pool = pool;
			_client = client;
			_stream = client.GetStream();
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
			lock (_pool)
			{
				TcpConnection connection = null;

				do
				{
					if (_pool.Count > 0) 
					{
						// There are available connections
						connection = (TcpConnection)_pool[_pool.Count - 1];
						_pool.RemoveAt(_pool.Count - 1);
					}

					if (connection == null && _activeConnections < TcpConnectionPool.MaxOpenConnections)
					{
						// No connections available, but the max connections
						// has not been reached yet, so a new one can be created
						connection = CreateConnection();
					}

					// No available connections in the pool
					// Wait for somewone to release one.

					if (connection == null)
						Monitor.Wait(_pool);
				} 
				while (connection == null);

				return connection;
			}
		}

		private TcpConnection CreateConnection()
		{
			try
			{
				TcpClient client = new TcpClient(_host, _port);
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
