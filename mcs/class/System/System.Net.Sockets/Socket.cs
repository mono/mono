// System.Net.Sockets.Socket.cs
//
// Author:
//    Phillip Pearson (pp@myelin.co.nz)
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
//

// NB: This is untested (probably buggy) code - take care if using it
//     Nowhere near finished yet ...

using System;
using System.Net;
using System.Collections;

namespace System.Net.Sockets 
{

	public class Socket : IDisposable 
	{
		// static method:

		/// <summary>
		/// Blocks while waiting for readability, writeability or
		/// error conditions on a number of sockets
		/// </summary>
		/// <param name="read_list">A list of sockets to watch
		/// for readability</param>
		/// <param name="write_list">A list of sockets to watch
		/// for writeability</param>
		/// <param name="err_list">A list of sockets to watch
		/// for errors</param>
		/// <param name="time_us">Timeout, in microseconds</param>
		public static void Select (
			IList read_list,
			IList write_list,
			IList err_list,
			int time_us)
		{
			throw new NotImplementedException();
		}

		// public constructor:

		/// <summary>
		/// Makes a new Socket
		/// </summary>
		/// <param name="family">Address family (e.g. 
		/// AddressFamily.InterNetwork for IPv4)</param>
		/// <param name="type">Socket Type (e.g. SocketType.Stream
		/// for stream sockets)</param>
		/// <param name="proto">Protocol (e.g.
		/// ProtocolType.Tcp for TCP)</param>
		public Socket (
			AddressFamily family,
			SocketType type,
			ProtocolType proto)
		{
			throw new NotImplementedException();
		}

		// public properties:

		/// <summary>
		/// The address family (see contructor)
		/// </summary>
		public AddressFamily AddressFamily
		{
			get 
			{
				throw new NotImplementedException();
				//return AddressFamily.InterNetwork;
			}
		}

		/// <summary>
		/// How much data is waiting to be read (i.e. the amount
		/// of data in the in buffer)
		/// </summary>
		public int Available 
		{
			get {
				throw new NotImplementedException();
				//return 0;
			}
		}

		/// <summary>
		/// A flag to indicate whether the socket is a blocking
		/// socket or not.  Returns true if blocking.
		/// 
		/// A non-blocking socket (Blocking == false) will return
		/// control to the application immediately if any calls are
		/// made that may take a while to complete.  Blocking
		/// sockets will block the app until whatever they are doing
		/// is finished.
		/// </summary>
		public bool Blocking 
		{
			get {
				throw new NotImplementedException();
				//return false;
			}
			set { }
		}

		/// <summary>
		/// A flag to say whether the socket is connected to something
		/// or not.
		/// 
		/// Returns true if connected.
		/// </summary>
		public bool Connected 
		{
			get {
				throw new NotImplementedException();
				//return false;
			}
		}

		/// <summary>
		/// A handle to the socket (its file descriptor?)
		/// </summary>
		public IntPtr Handle 
		{
			get {
				throw new NotImplementedException();
				//return new IntPtr(0);
			}
		}

		/// <summary>
		/// The socket's local endpoint (where it's coming from)
		/// </summary>
		public EndPoint LocalEndPoint 
		{
			get {
				throw new NotImplementedException();
				//return new IPEndPoint(0,0);
			}
		}

		/// <summary>
		/// Protocol type (e.g. Tcp, Udp)
		/// </summary>
		public ProtocolType ProtocolType 
		{
			get {
				throw new NotImplementedException();
				//return ProtocolType.Tcp;
			}
		}

		/// <summary>
		/// The socket's remote endpoint (where it's connected to)
		/// </summary>
		public EndPoint RemoteEndPoint 
		{
			get {
				throw new NotImplementedException();
				//return new IPEndPoint(0,0);
			}
		}

		/// <summary>
		/// Socket type (e.g. datagram, stream)
		/// </summary>
		public SocketType SocketType 
		{
			get {
				throw new NotImplementedException();
				//return SocketType.Stream;
			}
		}

		// public methods:

		/// <summary>
		/// Accepts a new connection
		/// </summary>
		/// <returns>A new socket to handle the connection</returns>
		public Socket Accept () 
		{
			throw new NotImplementedException();
			//return new Socket(AddressFamily.InterNetwork,
			//	SocketType.Stream, ProtocolType.Tcp);
		}

		/// <summary>
		/// Accepts the connection in the background,
		/// calling the application back when finished.
		/// </summary>
		/// <param name="callback">A delegate to be called
		/// when the accept is finished</param>
		/// <param name="state">State information for this call</param>
		/// <returns></returns>
		public IAsyncResult BeginAccept (
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Connects to a remote endpoint in the background,
		/// calling back when finished.
		/// </summary>
		/// <param name="remote_end_point">The endpoint to
		/// connect to</param>
		/// <param name="callback">Where to call when done</param>
		/// <param name="state">State information for this call</param>
		/// <returns></returns>
		public IAsyncResult BeginConnect (
			EndPoint remote_end_point,
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data in the background, calling
		/// back when finished.
		/// </summary>
		/// <param name="buffer">A buffer to put the data into as it
		/// arrives</param>
		/// <param name="offset">Where to put the data (offset into
		/// the buffer)</param>
		/// <param name="size">Buffer size</param>
		/// <param name="socket_flags">Socket flags</param>
		/// <param name="callback">Where to call when the
		/// operation is finished</param>
		/// <param name="state">State info for this call</param>
		/// <returns></returns>
		public IAsyncResult BeginReceive (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags socket_flags,
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data in the background from a specific
		/// point, calling back when finished.
		/// </summary>
		/// <param name="buffer">A buffer to put the data into as it
		/// arrives</param>
		/// <param name="offset">Where to put the data (offset into
		/// the buffer)</param>
		/// <param name="size">Buffer size</param>
		/// <param name="socket_flags">Socket flags</param>
		/// <param name="remote_end_point">Where to receive from</param>
		/// <param name="callback">Where to call when the
		/// operation is finished</param>
		/// <param name="state">State info for this call</param>
		/// <returns></returns>
		public IAsyncResult BeginReceiveFrom (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags socket_flags,
			ref EndPoint remote_end_point,
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts sending data somewhere, in the background.
		/// </summary>
		/// <param name="buffer">Buffer containing the data to send</param>
		/// <param name="offset">Where in the buffer to start sending from</param>
		/// <param name="size">Buffer size</param>
		/// <param name="socket_flags">Socket flags</param>
		/// <param name="callback">Where to call back to when finished</param>
		/// <param name="state">State info for this call</param>
		/// <returns></returns>
		public IAsyncResult BeginSend (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags socket_flags,
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts sending data to a specific point, in the background
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		/// <param name="socket_flags"></param>
		/// <param name="remote_end_point"></param>
		/// <param name="callback">Where to call back to when
		/// finished sending</param>
		/// <param name="state"></param>
		/// <returns></returns>
		public IAsyncResult BeginSendTo (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags socket_flags,
			EndPoint remote_end_point,
			AsyncCallback callback,
			object state)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Binds the socket to a local endpoint
		/// </summary>
		/// <param name="local_end_point">What to
		/// bind it to</param>
		public void Bind (EndPoint local_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Closes the socket
		/// </summary>
		public void Close ()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Connects to a remote system
		/// </summary>
		/// <param name="remote_end_point">Where to connect to</param>
		public void Connect (EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Completes an Accept() operation started
		/// with a BeginAccept() call
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public Socket EndAccept (IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes an asynchronous Connect() operation
		/// started with a BeginConnect() call
		/// </summary>
		/// <param name="result"></param>
		public void EndConnect (IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes an asynchronous Receive() operation
		/// started with a BeginReceive() call
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public int EndReceive (IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes an asynchronous ReceiveFrom() operation
		/// started with a BeginReceiveFrom() call
		/// </summary>
		/// <param name="result"></param>
		/// <param name="end_point"></param>
		/// <returns></returns>
		public int EndReceiveFrom (
			IAsyncResult result,
			ref EndPoint end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes an asynchronous Send() operation
		/// started with a BeginSend() call
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public int EndSend (IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Completes an asynchronous SendTo() operation
		/// started with a BeginSendTo() call
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public int EndSendTo (IAsyncResult result)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a socket option
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public object GetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a socket option
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <param name="opt_value"></param>
		public void GetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name,
			byte[] opt_value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a socket option
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public byte[] GetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name,
			int length)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Does something to the socket, a la ioctl()
		/// </summary>
		/// <param name="ioctl_code">Code of the operation
		/// to perform on the socket</param>
		/// <param name="in_value">Data to pass to ioctl()</param>
		/// <param name="out_value">Data returned from ioctl()</param>
		/// <returns></returns>
		public int IOControl (
			int ioctl_code,
			byte[] in_value,
			byte[] out_value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Tells the socket to start listening
		/// </summary>
		/// <param name="backlog">Connection backlog - the number 
		/// of pending (not Accept()ed) connections that will be
		/// allowed before new connections are automatically
		/// refused</param>
		public void Listen (int backlog)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Blocks the application until the socket is either
		/// readable, writeable or has an error condition
		/// </summary>
		/// <param name="time_us">How long to wait, in microseconds</param>
		/// <param name="mode">What to wait for (reading, writing, error)</param>
		/// <returns></returns>
		public bool Poll (int time_us, SelectMode mode)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Receives data from the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <returns></returns>
		public int Receive (
			byte[] buf)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Receive (
			byte[] buf,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Receive (
			byte[] buf,
			int size,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Receive (
			byte[] buf,
			int offset,
			int size,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Receives data from a specific point, 
		/// through the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int ReceiveFrom (
			byte[] buf,
			ref EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from a specific point, 
		/// through the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int ReceiveFrom (
			byte[] buf,
			SocketFlags flags,
			ref EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from a specific point, 
		/// through the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int ReceiveFrom (
			byte[] buf,
			int size,
			SocketFlags flags,
			ref EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Receives data from a specific point, 
		/// through the socket
		/// </summary>
		/// <param name="buf"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int ReceiveFrom (
			byte[] buf,
			int offset,
			int size,
			SocketFlags flags,
			ref EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Sends data through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public int Send (
			byte[] buffer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Send (
			byte[] buffer,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Send (
			byte[] buffer,
			int size,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public int Send (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags flags)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data to a specific point,
		/// through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int SendTo (
			byte[] buffer,
			EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data to a specific point,
		/// through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int SendTo (
			byte[] buffer,
			SocketFlags flags,
			EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data to a specific point,
		/// through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int SendTo (
			byte[] buffer,
			int size,
			SocketFlags flags,
			EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sends data to a specific point,
		/// through the socket
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="size"></param>
		/// <param name="flags"></param>
		/// <param name="remote_end_point"></param>
		/// <returns></returns>
		public int SendTo (
			byte[] buffer,
			int offset,
			int size,
			SocketFlags flags,
			EndPoint remote_end_point)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets a socket option (like setsockopt())
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <param name="opt_value"></param>
		public void SetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name,
			byte[] opt_value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets a socket option (like setsockopt())
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <param name="opt_value"></param>
		public void SetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name,
			int opt_value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets a socket option (like setsockopt())
		/// </summary>
		/// <param name="level"></param>
		/// <param name="name"></param>
		/// <param name="opt_value"></param>
		public void SetSocketOption (
			SocketOptionLevel level,
			SocketOptionName name,
			object opt_value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Stops anyone from being able to read or write to the socket
		/// </summary>
		/// <param name="how">What people aren't allowed to do any
		/// more (you can disable just reading or just writing, or both)</param>
		public void Shutdown (SocketShutdown how)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// A stringified representation of the socket
		/// </summary>
		/// <returns></returns>
		public override string ToString ()
		{
			throw new NotImplementedException();
			//return "foo";
		}



		// protected methods:

		/// <summary>
		/// Disposes of all unmanaged resources, and
		/// managed resources too if requested
		/// </summary>
		/// <param name="disposing">Set this to true
		/// to dispose of managed resources</param>
		protected virtual void Dispose (bool disposing)
		{
			// file descriptor / socket
			// other things to dispose of?

			// managed things?

			throw new NotImplementedException();
		}

		/// <summary>
		/// Disposes of everything (managed and
		/// unmanaged resources)
		/// </summary>
		public void Dispose ()
		{
			Dispose(true);
		}

		/// <summary>
		/// Destructor - disposes of unmanaged resources
		/// </summary>
		~Socket ()
		{
			Dispose(false);
			throw new NotImplementedException();
		}

	}

}
