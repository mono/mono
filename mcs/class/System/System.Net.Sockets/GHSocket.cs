using System;
using System.Net;

namespace System.Net.Sockets
{
	/// <summary>
	/// Summary description for GHSocket.
	/// </summary>
	internal interface GHSocket
	{
		int Available_internal(out int error);

		void Blocking_internal(bool block, out int error);

		EndPoint LocalEndPoint_internal(out int error);

		EndPoint RemoteEndPoint_internal(out int error);

		GHSocket Accept_internal(out int error);

		void Bind_internal(EndPoint sa, out int error);

		void Close_internal(out int error);

		void Connect_internal(EndPoint sa, out int error);

		void GetSocketOption_obj_internal(SocketOptionLevel level, SocketOptionName name, 
												   out object obj_val, out int error);
		
		void GetSocketOption_arr_internal(SocketOptionLevel level, SocketOptionName name, 
												   ref byte[] byte_val, out int error);

		int WSAIoctl (int ioctl_code, byte [] input, byte [] output, out int error);

		void Listen_internal(int backlog, out int error);

		bool Poll_internal (SelectMode mode, int timeout, Socket source, out int error);

		int Receive_internal(byte[] buffer,	int offset,	int count, SocketFlags flags,
									  out int error);

		int RecvFrom_internal(byte[] buffer, int offset, int count,	SocketFlags flags,
			                           ref SocketAddress sockaddr, out int error);

		int Send_internal(byte[] buf, int offset,	int count, SocketFlags flags,
								   out int error);

		int SendTo_internal(byte[] buffer, int offset, int count,
			                         SocketFlags flags,	SocketAddress sa, out int error);

		void SetSocketOption_internal (SocketOptionLevel level,
												SocketOptionName name, object obj_val,
												byte [] byte_val, int int_val, out int error);

		void Shutdown_internal(SocketShutdown how, out int error);

		void RegisterSelector(java.nio.channels.Selector selector, int mode, Socket source, out int error);

		bool CheckConnectionFinished();

		GHSocket ChangeToSSL(EndPoint remote_end);
	}
}
