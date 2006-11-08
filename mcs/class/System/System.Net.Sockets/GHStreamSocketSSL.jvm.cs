using System;
using System.Net;

namespace System.Net.Sockets
{
	/// <summary>
	/// Summary description for GHStreamSocket.
	/// </summary>
	internal class GHStreamSocketSSL : GHSocket
	{
		java.net.Socket jSocket;

		public GHStreamSocketSSL(java.net.Socket sslSocket)
		{
			jSocket = sslSocket;
		}

		public override int GetHashCode ()
		{
			if (jSocket == null)
				return -1;

			return jSocket.ToString().GetHashCode();
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
			//SVETA: see in the non-blocking io
			error = 0;

			if (block == false)
				throw new NotSupportedException();
		}

		public EndPoint LocalEndPoint_internal(out int error)
		{
			error = 0;
			java.net.InetSocketAddress localAddr = null;

			try
			{
				localAddr = (java.net.InetSocketAddress)jSocket.getLocalSocketAddress();
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
			error = 10022; //WSAEINVAL (Invalid argument)
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

			if (jSocket == null || jSocket.isBound() || jSocket.isConnected())
			{
				error = 10022; //WSAEINVAL (Invalid argument)
				return;
			}

			try
			{
				jSocket.bind(new java.net.InetSocketAddress(java.net.InetAddress.getByName(addr.Address.ToString()),
									                        addr.Port));
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

			try
			{
				if (jSocket != null)
				{
					jSocket.close();
					jSocket = null;
				}
			}
			catch (Exception e)
			{
				error = 10022; //WSAEINVAL (Invalid argument)
#if DEBUG
				Console.WriteLine("Caught exception during Close_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
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

			if (jSocket.isConnected())
			{
				error = 10056; //WSAEISCONN (Socket is already connected)
				return;
			}

			try
			{
				jSocket.connect(new java.net.InetSocketAddress(
									java.net.InetAddress.getByName(addr.Address.ToString()), 
									addr.Port));
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
			error = 10022; //WSAEINVAL (Invalid argument)
			return;
		}

		public bool Poll_internal (SelectMode mode, int timeout, Socket source, out int error)
		{
			error = 0;
			throw new NotImplementedException();
		}

		public int Receive_internal(byte[] buffer,	int offset,	int count, SocketFlags flags,
			out int error)
		{
			error = 0;
			int ret = 0;

			if (jSocket == null || !jSocket.isConnected())
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return ret;
			}

			try
			{
				ret = jSocket.getInputStream().read(vmw.common.TypeUtils.ToSByteArray(buffer), offset, count);
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

			if (jSocket == null || !jSocket.isConnected())
			{
				error = 10057; //WSAENOTCONN (Socket is not connected)
				return ret;
			}

			try
			{
				jSocket.getOutputStream().write(vmw.common.TypeUtils.ToSByteArray(buf), offset, count);
				ret = count;
			}
			catch (Exception e)
			{
				error = 10054; //WSAECONNRESET (Connection reset by peer)
				ret = 0;
#if DEBUG
				Console.WriteLine("Caught exception during Send_internal - {0}: {1}\n{2}", e.GetType(), e.Message, e.StackTrace);
#endif
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

			if (jSocket == null)
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
						jSocket.setReceiveBufferSize(ival);
						break;
					case SocketOptionName.ReceiveTimeout:
						ival = int_val;
						if (obj_val != null)
						{
							ival = (int) obj_val;
						}
						jSocket.setSoTimeout(ival);
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
						jSocket.setReuseAddress(bval);
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

			if (jSocket == null)
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
						ival = jSocket.getReceiveBufferSize();
						obj_val = ival;
						break;
					case SocketOptionName.ReceiveTimeout:
						ival = jSocket.getSoTimeout();
						obj_val = ival;
						break;
					case SocketOptionName.ReuseAddress:
						bval = jSocket.getReuseAddress();
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

			if (jSocket == null || !jSocket.isConnected())
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

		public void RegisterSelector(java.nio.channels.Selector selector, int mode, Socket source, out int error)
		{
			throw new InvalidOperationException();
		}

		public bool CheckConnectionFinished()
		{
			throw new InvalidOperationException();
		}

		public GHSocket ChangeToSSL(EndPoint remote_end)
		{
			return this;
		}

	}
}
