using java.nio.channels;
using java.security;
using javax.net.ssl;

namespace System.Net.Sockets
{
	/// <summary>
	/// Summary description for GHStreamSocket.
	/// </summary>
	internal class GHStreamSocket : GHSocket
	{
		java.net.ServerSocket jServerSocket;
		java.net.Socket jSocket;
		java.nio.channels.ServerSocketChannel jServerSocketChannel;
		java.nio.channels.SocketChannel jSocketChannel;

		// This field I need because a bug in the java.nio.channels.SocketAdapter, which 
		// returns local port 0 if the socket is not connected (even if the socket is bound)
		// so I need temporary use regular socket (not channel socket) to bind it to the 
		// local address and use this address in the LocalPoint property and to create the 
		// actual client/server channel sockets
		// The bug #5076965 (SocketChannel does not report local address after binding to a wildcard )
		// See: http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=5076965
		java.net.InetSocketAddress jTempLocalSocketAddress;

		public GHStreamSocket()
		{
			jSocketChannel = java.nio.channels.SocketChannel.open();
			jSocket = jSocketChannel.socket();
		}

		public GHStreamSocket(java.nio.channels.SocketChannel socketChannel)
		{
			jSocketChannel = socketChannel;
			jSocket = jSocketChannel.socket();
		}

		public override int GetHashCode ()
		{
			if (jSocket == null && jServerSocket == null)
				return -1;

			if (jServerSocket != null) {
				return jServerSocket.ToString ().GetHashCode ();
			}

			return jSocket.ToString ().GetHashCode ();
		}

		public int Available_internal(out int error)
		{
			error = 0;
			int r = 0;

			if (jSocket == null || !jSocket.isConnected())
			{
				return r;
			}

			try
			{
				r = jSocket.getInputStream().available();
			}
			catch (Exception e)
			{
				error = 10054; //WSAECONNRESET (Connection reset by peer)
				r = 0;
#if DEBUG
				Console.WriteLine("Caught exception during Available_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			return r;
		}

		public void Blocking_internal(bool block, out int error)
		{
			error = 0;

			if (jSocket == null && jServerSocket == null)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			try
			{
				if (jServerSocket != null)
				{
					jServerSocketChannel.configureBlocking(block);
				}
				else
				{
					jSocketChannel.configureBlocking(block);
				}
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
				Console.WriteLine("Caught exception during Blocking_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}

		public EndPoint LocalEndPoint_internal(out int error)
		{
			error = 0;
			java.net.InetSocketAddress localAddr = null;

			try
			{
				if (jTempLocalSocketAddress != null)
				{
					localAddr = jTempLocalSocketAddress;
				}
				else if (jServerSocket != null)
				{
					localAddr = (java.net.InetSocketAddress)jServerSocket.getLocalSocketAddress();
				}
				else
				{
					localAddr = (java.net.InetSocketAddress)jSocket.getLocalSocketAddress();
				}
			}
			catch (Exception e)
			{
				localAddr = null;
#if DEBUG
				Console.WriteLine("Caught exception during LocalEndPoint_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (localAddr == null || localAddr.getAddress() == null || localAddr.getPort() < 0)
			{
                return null;
			}

			IPHostEntry lipa = Dns.Resolve(localAddr.getHostName());
			IPEndPoint ret = new IPEndPoint(lipa.AddressList[0], localAddr.getPort());
			return ret;
		}

		public EndPoint RemoteEndPoint_internal(out int error)
		{
			error = 0;
			java.net.InetSocketAddress remoteAddr = null;

			if (jSocket == null || !jSocket.isBound())
			{
				return null;
			}

			try
			{
				remoteAddr = (java.net.InetSocketAddress)jSocket.getRemoteSocketAddress();
			}
			catch (Exception e)
			{
				remoteAddr = null;
#if DEBUG
				Console.WriteLine("Caught exception during RemoteEndPoint_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (remoteAddr == null || remoteAddr.getAddress() == null || remoteAddr.getPort() <= 0)
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return null;
			}

			IPHostEntry lipa = Dns.Resolve(remoteAddr.getHostName());
			IPEndPoint ret = new IPEndPoint(lipa.AddressList[0], remoteAddr.getPort());
			return ret;
		}

		public GHSocket Accept_internal(out int error)
		{
			error = 0;

			if (jServerSocket == null)
			{
				throw new InvalidOperationException("You must call Bind and Listen before calling Accept.");
			}

			try
			{
				/*
					If this channel is in non-blocking mode then this method will immediately 
					return null if there are no pending connections. 
					Otherwise it will block indefinitely until a new connection is 
					available or an I/O error occurs.				 
				*/
				java.nio.channels.SocketChannel acceptedSocket = jServerSocketChannel.accept();
				if (acceptedSocket == null) 
				{
					error = 10035; //WSAEWOULDBLOCK (Resource temporarily unavailable)
#if DEBUG
					Console.WriteLine("The Accept_internal is in non-blocking mode and no pending connections are available");
#endif
					return null;
				}

				return new GHStreamSocket(acceptedSocket);
			}
			catch (AsynchronousCloseException) {
				error = 10004;
			}
			catch (Exception e)
			{
				error = 10061; //WSAECONNREFUSED (Connection refused)
#if DEBUG
				Console.WriteLine("Caught exception during Accept_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			return null;
		}

		public void Bind_internal(EndPoint sa, out int error)
		{
			error = 0;
			IPEndPoint addr = sa as IPEndPoint;
			if (addr == null)
			{
				error = 10044; //WSAESOCKTNOSUPPORT (Socket type not supported)
				return;
			}

			if (jSocket == null || jSocket.isBound() || jSocket.isConnected() || jSocketChannel.isConnectionPending())
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			try
			{
				// This code I need because a bug in the java.nio.channels.SocketAdapter, which 
				// returns local port 0 if the socket is not connected (even if the socket is bound)
				// so I need temporary use regular socket (not channel socket) to bind it to the 
				// local address and use this address in the LocalPoint property and to create the 
				// actual client/server channel sockets
				// The bug #5076965 (SocketChannel does not report local address after binding to a wildcard )
				// See: http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=5076965
				java.net.Socket jTempSocket = new java.net.Socket();
				jTempSocket.bind(new java.net.InetSocketAddress(java.net.InetAddress.getByName(addr.Address.ToString()),
									                        addr.Port));
				jTempLocalSocketAddress = (java.net.InetSocketAddress)jTempSocket.getLocalSocketAddress();
				jTempSocket.close();
				jSocket.bind(jTempLocalSocketAddress);
			}
			catch (Exception e)
			{
				error = 10048; //WSAEADDRINUSE (Address already in use)
#if DEBUG
				Console.WriteLine("Caught exception during Bind_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}

		public void Close_internal(out int error)
		{
			error = 0;

			if (jServerSocket != null)
			{
				try
				{
					jServerSocket.close();
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during Close_internal jServerSocket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
				try
				{
					jServerSocketChannel.close();
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during Close_internal jServerSocketChannel - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
				jServerSocket = null;
				jServerSocketChannel = null;
			}
			else if (jSocket != null)
			{
				try
				{
					jSocket.close();
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during Close_internal jSocket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
				try
				{
					jSocketChannel.close();
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during Close_internal jSocketChannel - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
				jSocket = null;
				jSocketChannel = null;
			}
		}

		public void Connect_internal(EndPoint sa, out int error)
		{
			error = 0;

			IPEndPoint addr = sa as IPEndPoint;
			if (addr == null)
			{
				error = 10044; //WSAESOCKTNOSUPPORT (Socket type not supported)
				return;
			}

			if (jSocket == null)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			if (jSocket.isConnected() || jSocketChannel.isConnectionPending())
			{
				error = 10056; //WSAEISCONN (Socket is already connected)
				return;
			}

			try
			{
				/*
				 If this channel is in non-blocking mode then an invocation of this method
				 initiates a non-blocking connection operation. If the connection is 
				 established immediately, as can happen with a local connection, then this 
				 method returns true. Otherwise this method returns false.  
                 If this channel is in blocking mode then an invocation of this method 
				 will block until the connection is established or an I/O error occurs. 
				 */
				bool status = jSocketChannel.connect(new java.net.InetSocketAddress(
					java.net.InetAddress.getByName(addr.Address.ToString()), 
					addr.Port));
				if (!status)
				{
					error = 10035; //WSAEWOULDBLOCK (Resource temporarily unavailable)
				}
			}
			catch (java.nio.channels.AlreadyConnectedException ae)
			{				
				error = 10056; //WSAEISCONN (Socket is already connected)
			}
			catch (java.nio.channels.ConnectionPendingException cpe)
			{				
				error = 10036; //WSAEINPROGRESS (Operation now in progress)
			}
			catch (java.nio.channels.UnresolvedAddressException uae)
			{				
				error = 10039; //WSAEDESTADDRREQ (Destination address required)
			}
			catch (java.nio.channels.UnsupportedAddressTypeException uate)
			{				
				error = 10041; //WSAEPROTOTYPE (Protocol wrong type for socket)
			}
			catch (AsynchronousCloseException) {
				error = 10004;
			}
			catch (Exception e)
			{				
				error = 10061; //WSAECONNREFUSED (Connection refused)
#if DEBUG
				Console.WriteLine("Caught exception during Connect_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}

		public void Listen_internal(int backlog, out int error)
		{
			error = 0;

			if (jSocket == null || !jSocket.isBound())
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			if (jSocket.isConnected() || jSocketChannel.isConnectionPending())
			{
				error = 10056; //WSAEISCONN (Socket is already connected)
				return;
			}

			bool blockMode = jSocketChannel.isBlocking();
			bool reuseAddr = jSocket.getReuseAddress();

			try
			{
				jSocket.close();
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("Caught exception during Listen_internal close old jSocket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			try
			{
				jSocketChannel.close();
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("Caught exception during Listen_internal close old jSocketChannel - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			jSocket = null;
			jSocketChannel = null;

			try
			{
				jServerSocketChannel = java.nio.channels.ServerSocketChannel.open();
				jServerSocket = jServerSocketChannel.socket();
				jServerSocket.bind(jTempLocalSocketAddress, backlog);
				jServerSocketChannel.configureBlocking(blockMode);
				jServerSocket.setReuseAddress(reuseAddr);
			}
			catch (Exception e)
			{
				error = 10048; //WSAEADDRINUSE (Address already in use)
#if DEBUG
				Console.WriteLine("Caught exception during Listen_internal create server socket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}

		public bool Poll_internal (SelectMode mode, int timeout, Socket source, out int error)
		{
			error = 0;

			if (mode == SelectMode.SelectError && !jSocketChannel.isConnectionPending())
			{
				return false;
			}

			java.nio.channels.Selector selector = java.nio.channels.Selector.open();
			RegisterSelector(selector, ((mode == SelectMode.SelectRead)?0:1), source, out error);

			if (error != 0)
			{
				error = 0;
				GHSocketFactory.CloseSelector(selector);
				return (mode == SelectMode.SelectError);
			}

			bool retVal = false;

			long timeOutMillis = 1;
			if (timeout < 0)
			{
				timeOutMillis = 0;
			} 
			else if (timeout > 999)
			{
				timeOutMillis = (long)(timeout / 1000);
			}

			int readyCount = 0;
			try
			{
				readyCount = selector.select(timeOutMillis);
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
				Console.WriteLine("Caught exception during Poll_internal selector.select - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (readyCount > 0)
			{
				if (jSocket != null && jSocketChannel.isConnectionPending())
				{
					bool status = false;
					try
					{
						status = jSocketChannel.finishConnect();
					}
					catch (Exception e)
					{
#if DEBUG
						Console.WriteLine("Caught exception during Poll_internal, finishConnect - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
					}
					if (status)
					{
						retVal =  (mode != SelectMode.SelectError);
					}
					else 
					{
						retVal =  (mode == SelectMode.SelectError);
					}
				}
				else
				{
					retVal =  true;
				}
			}

			GHSocketFactory.CloseSelector(selector);

			return retVal;
		}

		public void RegisterSelector(java.nio.channels.Selector selector, int mode, Socket source, out int error)
		{
			error = 0;
			if (jServerSocket != null)
			{
				// only accept operation, which included to the read list, is allowed for server sockets
				if (mode != 0)
				{
//					error = 10038; //WSAENOTSOCK (Socket operation on nonsocket)
#if DEBUG
					Console.WriteLine("RegisterSelector, invalid mode {0} for the server socket", mode);
#endif
					return;
				}

				try
				{
					if (jServerSocketChannel.isBlocking())
					{
						/*
							A channel must be placed into non-blocking mode before being registered 
							with a selector, and may not be returned to blocking mode until it has been 
							deregistered. 
						*/
						jServerSocketChannel.configureBlocking(false);
					}

					jServerSocketChannel.register(selector, java.nio.channels.SelectionKey.OP_ACCEPT, source);
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during RegisterSelector, register server socket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
			}
			else
			{
				try
				{
					int ops = java.nio.channels.SelectionKey.OP_READ;
					if (mode > 0)
					{
						if (jSocketChannel.isConnectionPending())
						{
							ops = java.nio.channels.SelectionKey.OP_CONNECT;
						}
						else
						{
							ops = java.nio.channels.SelectionKey.OP_WRITE;
						}
					}
					
					if (jSocketChannel.isBlocking())
					{
						/*
							A channel must be placed into non-blocking mode before being registered 
							with a selector, and may not be returned to blocking mode until it has been 
							deregistered. 
						*/
						jSocketChannel.configureBlocking(false);
					}

					jSocketChannel.register(selector, ops, source);
				}
				catch (Exception e)
				{
					error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
					Console.WriteLine("Caught exception during RegisterSelector, register client socket - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
			}
		}

		public bool CheckConnectionFinished()
		{
			bool status = true;
			if (jSocket != null && jSocketChannel.isConnectionPending())
			{
				try
				{
					status = jSocketChannel.finishConnect();
				}
				catch (Exception e)
				{
					status = false;
#if DEBUG
					Console.WriteLine("Caught exception during Poll_internal, finishConnect - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
				}
			}

			return status;
		}

		public int Receive_internal(byte[] buffer,	int offset,	int count, SocketFlags flags,
			out int error)
		{
			error = 0;
			int ret = 0;

			if (jSocket == null)
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return ret;
			}

			try
			{
				if (jSocketChannel.isConnectionPending())
				{
					bool status = jSocketChannel.finishConnect();
					if (!status)
					{
						error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
						Console.WriteLine("Receive_internal, jSocketChannel.finishConnect return false");
#endif
						return 0;
					}
				}
				else if (!jSocketChannel.isConnected())
				{
					error = 10057; //WSAENOTCONN (Socket is not connected)
					return ret;
				}

				java.nio.ByteBuffer readBuff = java.nio.ByteBuffer.wrap(vmw.common.TypeUtils.ToSByteArray(buffer), offset, count);
				ret = jSocketChannel.read(readBuff);
				if (ret < 0) ret = 0;
			}
			catch (Exception e)
			{
				error = 10054; //WSAECONNRESET (Connection reset by peer)
				ret = 0;
#if DEBUG
				Console.WriteLine("Caught exception during Receive_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (ret == 0 && !jSocketChannel.isBlocking())
			{
				error = 10035; //WSAEWOULDBLOCK (Resource temporarily unavailable)
			}
			return ret;
		}

		public int RecvFrom_internal(byte[] buffer, int offset, int count,	SocketFlags flags,
			ref SocketAddress sockaddr, out int error)
		{
			return Receive_internal(buffer, offset, count, flags, out error);
		}

		public int Send_internal(byte[] buf, int offset, int count, SocketFlags flags,
			out int error)
		{
			error = 0;
			int ret = 0;

			if (jSocket == null)
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return ret;
			}

			try
			{
				if (jSocketChannel.isConnectionPending())
				{
					bool status = jSocketChannel.finishConnect();
					if (!status)
					{
						error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
						Console.WriteLine("Send_internal, jSocketChannel.finishConnect return false");
#endif
						return 0;
					}
				}
				else if (!jSocketChannel.isConnected())
				{
					error = 10057; //WSAENOTCONN (Socket is not connected)
					return ret;
				}

				java.nio.ByteBuffer writeBuff = java.nio.ByteBuffer.wrap(vmw.common.TypeUtils.ToSByteArray(buf), offset, count);
				ret = jSocketChannel.write(writeBuff);
				if (ret < 0) ret = 0;
			}
			catch (Exception e)
			{
				error = 10054; //WSAECONNRESET (Connection reset by peer)
				ret = 0;
#if DEBUG
				Console.WriteLine("Caught exception during Send_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}

			if (ret == 0 && !jSocketChannel.isBlocking())
			{
				error = 10035; //WSAEWOULDBLOCK (Resource temporarily unavailable)
			}
			return ret;
		}

		public int SendTo_internal(byte[] buffer, int offset, int count,
			SocketFlags flags,	SocketAddress sa, out int error)
		{
			return Send_internal(buffer, offset, count, flags, out error);
		}

		public void SetSocketOption_internal (SocketOptionLevel level,
			SocketOptionName name, object obj_val,
			byte [] byte_val, int int_val, out int error)
		{
			error = 0;

			if (byte_val != null)
			{
				error = -1;
				throw new NotImplementedException();
			}

			if (jSocket == null && jServerSocket == null)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			switch (level)
			{
				case SocketOptionLevel.IPv6:
					error = 10042; //WSAENOPROTOOPT (Bad protocol option)
					return;
				case SocketOptionLevel.IP:
					if (name != SocketOptionName.NoDelay)
					{
						error = 10042; //WSAENOPROTOOPT (Bad protocol option)
						return;
					}
					break;
				case SocketOptionLevel.Udp:
					if (name == SocketOptionName.NoDelay)
					{
						error = 10042; //WSAENOPROTOOPT (Bad protocol option)
					}
					else
					{
						error = 10022; //WSAEINVAL (Invalid argument)
					}
					return;
				case SocketOptionLevel.Tcp:
					if (name != SocketOptionName.NoDelay)
					{
						error = 10022; //WSAEINVAL (Invalid argument)
						return;
					}
					break;
			}

			try
			{
				bool bval = false;
				int ival = 0;
				switch (name)
				{
					case SocketOptionName.DontLinger:
						jSocket.setSoLinger(false, 0);
						break;
					case SocketOptionName.Linger:
						LingerOption lval = obj_val as LingerOption;
						if (lval != null)
						{
							jSocket.setSoLinger(lval.Enabled, lval.LingerTime);
						}
						else
						{
							error = 10022; //WSAEINVAL (Invalid argument)
						}
						break;
					case SocketOptionName.KeepAlive:
						if (obj_val != null)
						{
							bval = ((int)obj_val == 0)?false:true;
						}
						else
						{
							bval = (int_val == 0)?false:true;
						}
						jSocket.setKeepAlive(bval);
						break;
					case SocketOptionName.NoDelay:
						if (obj_val != null)
						{
							bval = ((int)obj_val == 0)?false:true;
						}
						else
						{
							bval = (int_val == 0)?false:true;
						}
						jSocket.setTcpNoDelay(bval);
						break;
					case SocketOptionName.ReceiveBuffer:
						ival = int_val;
						if (obj_val != null)
						{
							ival = (int) obj_val;
						}
						if (jServerSocket != null)
						{
							jServerSocket.setReceiveBufferSize(ival);
						}
						else
						{
							jSocket.setReceiveBufferSize(ival);
						}
						break;
					case SocketOptionName.ReceiveTimeout:
						ival = int_val;
						if (obj_val != null)
						{
							ival = (int) obj_val;
						}
						if (jServerSocket != null)
						{
							jServerSocket.setSoTimeout(ival);
						}
						else
						{
							jSocket.setSoTimeout(ival);
						}
						break;
					case SocketOptionName.ReuseAddress:
						if (obj_val != null)
						{
							bval = ((int)obj_val == 0)?false:true;
						}
						else
						{
							bval = (int_val == 0)?false:true;
						}
						if (jServerSocket != null)
						{
							jServerSocket.setReuseAddress(bval);
						}
						else
						{
							jSocket.setReuseAddress(bval);
						}
						break;
					case SocketOptionName.SendBuffer:
						ival = int_val;
						if (obj_val != null)
						{
							ival = (int) obj_val;
						}
						jSocket.setSendBufferSize(ival);
						break;
					case SocketOptionName.OutOfBandInline:
						if (obj_val != null)
						{
							bval = ((int)obj_val == 0)?false:true;
						}
						else
						{
							bval = (int_val == 0)?false:true;
						}
						jSocket.setOOBInline(bval);
						break;
					default:
						error = 10022; //WSAEINVAL (Invalid argument)
						break;
				}
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				obj_val = null;
			}
		}

		public void GetSocketOption_obj_internal(SocketOptionLevel level, SocketOptionName name, 
			out object obj_val, out int error)
		{
			obj_val = null;
			error = 0;

			if (jSocket == null && jServerSocket == null)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			switch (level)
			{
				case SocketOptionLevel.IPv6:
					error = 10042; //WSAENOPROTOOPT (Bad protocol option)
					return;
				case SocketOptionLevel.IP:
					if (name != SocketOptionName.NoDelay)
					{
						error = 10042; //WSAENOPROTOOPT (Bad protocol option)
						return;
					}
					break;
				case SocketOptionLevel.Udp:
					if (name == SocketOptionName.NoDelay)
					{
						error = 10042; //WSAENOPROTOOPT (Bad protocol option)
					}
					else
					{
						error = 10022; //WSAEINVAL (Invalid argument)
					}
					return;
				case SocketOptionLevel.Tcp:
					if (name != SocketOptionName.NoDelay)
					{
						error = 10022; //WSAEINVAL (Invalid argument)
						return;
					}
					break;
			}

			try
			{
				bool bval = false;
				int ival = 0;
				switch (name)
				{
					case SocketOptionName.DontLinger:
						ival = jSocket.getSoLinger();
						if (ival == -1)
						{
							obj_val = 1;
						}
						else
						{
							obj_val = 0;
						}
						break;
					case SocketOptionName.Linger:
						ival = jSocket.getSoLinger();
						if (ival == -1)
						{
							ival = 0;
						}
						LingerOption ret = new LingerOption((ival != 0), ival);
						obj_val = ret;
						break;
					case SocketOptionName.KeepAlive:
						bval = jSocket.getKeepAlive();
						obj_val = ((bval)?1:0);
						break;
					case SocketOptionName.NoDelay:
						bval = jSocket.getTcpNoDelay();
						obj_val = ((bval)?1:0);
						break;
					case SocketOptionName.ReceiveBuffer:
						if (jServerSocket != null)
						{
							ival = jServerSocket.getReceiveBufferSize();
						}
						else
						{
							ival = jSocket.getReceiveBufferSize();
						}
						obj_val = ival;
						break;
					case SocketOptionName.ReceiveTimeout:
						if (jServerSocket != null)
						{
							ival = jServerSocket.getSoTimeout();
						}
						else
						{
							ival = jSocket.getSoTimeout();
						}
						obj_val = ival;
						break;
					case SocketOptionName.ReuseAddress:
						if (jServerSocket != null)
						{
							bval = jServerSocket.getReuseAddress();
						}
						else
						{
							bval = jSocket.getReuseAddress();
						}
						obj_val = ((bval)?1:0);
						break;
					case SocketOptionName.SendBuffer:
						ival = jSocket.getSendBufferSize();
						obj_val = ival;
						break;
					case SocketOptionName.OutOfBandInline:
						bval = jSocket.getOOBInline();
						obj_val = ((bval)?1:0);
						break;
					default:
						error = 10022; //WSAEINVAL (Invalid argument)
						break;
				}
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				obj_val = null;
			}
		}
		
		public void GetSocketOption_arr_internal(SocketOptionLevel level, SocketOptionName name, 
			ref byte[] byte_val, out int error)
		{
			error = -1;
			throw new NotImplementedException();
		}

		public int WSAIoctl (int ioctl_code, byte [] input, byte [] output, out int error)
		{
			error = -1;
			throw new NotImplementedException();
		}

		public void Shutdown_internal(SocketShutdown how, out int error)
		{
			error = 0;

			if (jServerSocket != null || jSocket == null || !jSocket.isConnected())
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return;
			}

			try
			{
				switch (how)
				{
					case SocketShutdown.Receive: 
						                jSocket.shutdownInput();
						                break;
					case SocketShutdown.Send: 
										jSocket.shutdownOutput();
										break;
					case SocketShutdown.Both: 
										jSocket.shutdownInput();
										jSocket.shutdownOutput();
										break;
				}
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
				Console.WriteLine("Caught exception during Shutdown_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
			}
		}

		private java.io.FileInputStream searchDefaultCacerts()
		{
			try
			{
				string javaHome = java.lang.System.getProperty("java.home");
				if(javaHome == null)
					return null;

				string keyStorePath = javaHome + "/lib/security/cacerts";
				//Console.WriteLine("keyStorePath = {0}", keyStorePath);

				java.io.File f = new java.io.File(keyStorePath);
				if(!f.exists())
					return null;
				return new java.io.FileInputStream(f);
			}
			catch(Exception e)
			{
#if DEBUG
				//todo log it
				Console.WriteLine(e.GetType() + ":" + e.Message + "\n" + e.StackTrace);
#endif
				return null;
			}
		}

		private SSLSocketFactory getSSLSocketFactory()
		{
			SSLSocketFactory factory = null;

			try
			{
				//reading the keyStore path and password from the environment properties
				string keyStorePath = java.lang.System.getProperty("javax.net.ssl.keyStore");
				java.io.FileInputStream keyStoreStream = null;
				if (keyStorePath != null)
				{
					java.io.File file = new java.io.File(keyStorePath);
					if(file.exists())
						keyStoreStream = new java.io.FileInputStream(file);
					else
						keyStoreStream = searchDefaultCacerts();
				}
				else
					keyStoreStream = searchDefaultCacerts();

				string keyStorePassWord = java.lang.System.getProperty("javax.net.ssl.keyStorePassword");
				if (keyStorePassWord == null)
					keyStorePassWord = "changeit";
				char[] passphrase = keyStorePassWord.ToCharArray();				
						
				//initiating SSLContext
				SSLContext ctx = SSLContext.getInstance("TLS");
				KeyManagerFactory kmf = KeyManagerFactory.getInstance(KeyManagerFactory.getDefaultAlgorithm());
				TrustManagerFactory tmf = TrustManagerFactory.getInstance(TrustManagerFactory.getDefaultAlgorithm());
				KeyStore ks = KeyStore.getInstance("JKS");
				if (keyStoreStream != null)
					ks.load(keyStoreStream,passphrase);
				else
					ks.load(null,null);
				kmf.init(ks, passphrase);
				tmf.init(ks);
				ctx.init(kmf.getKeyManagers(), tmf.getTrustManagers(), null);

				factory = ctx.getSocketFactory();
			}
			catch (Exception e)
			{
				factory = null;
#if DEBUG
				Console.WriteLine("Can't get SSL Socket Factory, the exception is {0}, {1}", e.GetType(), e.Message);
#endif
			}

			return factory;
		}

		public GHSocket ChangeToSSL(EndPoint remote_end)
		{
			if (jSocket == null)
			{
				throw new InvalidOperationException("The underlying socket is null");
			}

			if (!jSocketChannel.isBlocking())
			{
				throw new NotImplementedException("The SSL Socket for non-blocking mode is not supported");
			}

			SSLSocketFactory factory = getSSLSocketFactory();
			if (factory == null)
			{
				throw new ApplicationException("Can't get SSL Socket Factory");
			}

			int err;

			// The problem with local address, when I closed the socket and try to create the new one
			// bounded to the given local address, I receive exception "Address already in use"
			IPEndPoint localEndPoint = null;
//			IPEndPoint localEndPoint = (IPEndPoint) LocalEndPoint_internal(out err);
//			if (err != 0)
//				localEndPoint = null;

			IPEndPoint remoteEndPoint = remote_end as IPEndPoint;
			if (remoteEndPoint == null)
			{
				remoteEndPoint = (IPEndPoint) RemoteEndPoint_internal(out err);
				if (err != 0)
					remoteEndPoint = null;
			}

			java.net.Socket sslSocket = null;
			try
			{
				if (remoteEndPoint != null)
				{
					if (localEndPoint != null)
					{
						sslSocket = factory.createSocket(
							java.net.InetAddress.getByName(remoteEndPoint.Address.ToString()),
							remoteEndPoint.Port,
							java.net.InetAddress.getByName(localEndPoint.Address.ToString()),
							localEndPoint.Port);
					}
					else
					{
						sslSocket = factory.createSocket(
							jSocket, 
							remoteEndPoint.Address.ToString(),
							remoteEndPoint.Port,
							false);
					}

					if (sslSocket != null)
					{
						String[] protocols = { "TLSv1", "SSLv3" };
						((SSLSocket)sslSocket).setUseClientMode(true);
						((SSLSocket)sslSocket).startHandshake();
					}

				}
				else
				{
					sslSocket = factory.createSocket();
				}
			}
			catch (Exception e)
			{
				sslSocket = null;
#if DEBUG
				Console.WriteLine("Can't create SSL Socket, the exception is {0}, {1}", e.GetType(), e.Message);
#endif
			}

			if (sslSocket == null)
			{
//				throw new ApplicationException("Can't create SSL Socket");
				// it is important to the Socket class to distinguish if the underlying 
				// handle (GHSocket) is still valid and can be used as non-SSL, or it is already
				// closed by this function and can't be used any more.
				return null;
			}

/*
			string[] arr = ((SSLSocket)sslSocket).getEnabledProtocols();
			if (arr != null)
			{
				foreach (string s in arr)
					Console.WriteLine("s:"+s);
			}
			string [] arr1 = ((SSLSocket)sslSocket).getEnabledCipherSuites();
			if (arr1 != null)
			{
				foreach (string s in arr1)
					Console.WriteLine("s:"+s);
			}
*/

			return new GHStreamSocketSSL(sslSocket);
		}
	}
}
