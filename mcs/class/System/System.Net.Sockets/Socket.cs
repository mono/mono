// System.Net.Sockets.Socket.cs
//
// Authors:
//	Phillip Pearson (pp@myelin.co.nz)
//	Dick Porter <dick@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sridhar Kulkarni (sridharkulkarni@gmail.com)
//	Brian Nickel (brian.nickel@gmail.com)
//	Ludovic Henry (ludovic@xamarin.com)
//
// Copyright (C) 2001, 2002 Phillip Pearson and Ximian, Inc.
//    http://www.myelin.co.nz
// (c) 2004-2011 Novell, Inc. (http://www.novell.com)
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
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Net.Configuration;
using System.Text;
using System.Timers;
using System.Net.NetworkInformation;

namespace System.Net.Sockets
{
	public partial class Socket : IDisposable
	{
		const int SOCKET_CLOSED_CODE = 10004;
		const string TIMEOUT_EXCEPTION_MSG = "A connection attempt failed because the connected party did not properly respond" +
			"after a period of time, or established connection failed because connected host has failed to respond";

		/* true if we called Close_internal */
		bool is_closed;

		bool is_listening;
		bool useOverlappedIO;

		int linger_timeout;

		AddressFamily addressFamily;
		SocketType socketType;
		ProtocolType protocolType;

		/* the field "m_Handle" is looked up by name by the runtime */
		internal SafeSocketHandle m_Handle;

		/*
		 * This EndPoint is used when creating new endpoints. Because
		 * there are many types of EndPoints possible,
		 * seed_endpoint.Create(addr) is used for creating new ones.
		 * As such, this value is set on Bind, SentTo, ReceiveFrom,
		 * Connect, etc.
		 */
		internal EndPoint seed_endpoint = null;

		internal SemaphoreSlim ReadSem = new SemaphoreSlim (1, 1);
		internal SemaphoreSlim WriteSem = new SemaphoreSlim (1, 1);

		internal bool is_blocking = true;
		internal bool is_bound;

		/* When true, the socket was connected at the time of the last IO operation */
		internal bool is_connected;

		int m_IntCleanedUp;
		internal bool connect_in_progress;

#if MONO_WEB_DEBUG
		static int nextId;
		internal readonly int ID = ++nextId;
#else
		internal readonly int ID;
#endif

		#region Constructors


		public Socket (SocketInformation socketInformation)
		{
			this.is_listening      = (socketInformation.Options & SocketInformationOptions.Listening) != 0;
			this.is_connected      = (socketInformation.Options & SocketInformationOptions.Connected) != 0;
			this.is_blocking       = (socketInformation.Options & SocketInformationOptions.NonBlocking) == 0;
			this.useOverlappedIO = (socketInformation.Options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;

			var result = Mono.DataConverter.Unpack ("iiiil", socketInformation.ProtocolInformation, 0);

			this.addressFamily = (AddressFamily) (int) result [0];
			this.socketType = (SocketType) (int) result [1];
			this.protocolType = (ProtocolType) (int) result [2];
			this.is_bound = (ProtocolType) (int) result [3] != 0;
			this.m_Handle = new SafeSocketHandle ((IntPtr) (long) result [4], true);

			InitializeSockets ();

			SocketDefaults ();
		}

		/* private constructor used by Accept, which already has a socket handle to use */
		internal Socket(AddressFamily family, SocketType type, ProtocolType proto, SafeSocketHandle safe_handle)
		{
			this.addressFamily = family;
			this.socketType = type;
			this.protocolType = proto;
			
			this.m_Handle = safe_handle;
			this.is_connected = true;

			InitializeSockets ();	
		}

		void SocketDefaults ()
		{
			try {
				/* Need to test IPv6 further */
				if (addressFamily == AddressFamily.InterNetwork
					// || addressFamily == AddressFamily.InterNetworkV6
				) {
					/* This is the default, but it probably has nasty side
					 * effects on Linux, as the socket option is kludged by
					 * turning on or off PMTU discovery... */
					this.DontFragment = false;
					if (protocolType == ProtocolType.Tcp)
						this.NoDelay = false;
				} else if (addressFamily == AddressFamily.InterNetworkV6) {
					this.DualMode = true;
				}

				/* Microsoft sets these to 8192, but we are going to keep them
				 * both to the OS defaults as these have a big performance impact.
				 * on WebClient performance. */
				// this.ReceiveBufferSize = 8192;
				// this.SendBufferSize = 8192;
			} catch (SocketException) {
			}
		}

		/* Creates a new system socket, returning the handle */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern IntPtr Socket_internal (AddressFamily family, SocketType type, ProtocolType proto, out int error);

#endregion

#region Properties

		public int Available {
			get {
				ThrowIfDisposedAndClosed ();

				int ret, error;
				ret = Available_internal (m_Handle, out error);

				if (error != 0)
					throw new SocketException (error);

				return ret;
			}
		}

		static int Available_internal (SafeSocketHandle safeHandle, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return Available_internal (safeHandle.DangerousGetHandle (), out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		/* Returns the amount of data waiting to be read on socket */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int Available_internal (IntPtr socket, out int error);

		// FIXME: import from referencesource
		public bool EnableBroadcast {
			get {
				ThrowIfDisposedAndClosed ();

				if (protocolType != ProtocolType.Udp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				return ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast)) != 0;
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (protocolType != ProtocolType.Udp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
			}
		}

		public bool IsBound {
			get {
				return is_bound;
			}
		}

		// FIXME: import from referencesource
		public bool MulticastLoopback {
			get {
				ThrowIfDisposedAndClosed ();

				/* Even though this option can be set for TCP sockets on Linux, throw
				 * this exception anyway to be compatible (the MSDN docs say
				 * "Setting this property on a Transmission Control Protocol (TCP)
				 * socket will have no effect." but the MS runtime throws the
				 * exception...) */
				if (protocolType == ProtocolType.Tcp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				switch (addressFamily) {
				case AddressFamily.InterNetwork:
					return ((int) GetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback)) != 0;
				case AddressFamily.InterNetworkV6:
					return ((int) GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback)) != 0;
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
			set {
				ThrowIfDisposedAndClosed ();

				/* Even though this option can be set for TCP sockets on Linux, throw
				 * this exception anyway to be compatible (the MSDN docs say
				 * "Setting this property on a Transmission Control Protocol (TCP)
				 * socket will have no effect." but the MS runtime throws the
				 * exception...) */
				if (protocolType == ProtocolType.Tcp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				switch (addressFamily) {
				case AddressFamily.InterNetwork:
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value ? 1 : 0);
					break;
				case AddressFamily.InterNetworkV6:
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, value ? 1 : 0);
					break;
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
		}

		// Wish:  support non-IP endpoints.
		public EndPoint LocalEndPoint {
			get {
				ThrowIfDisposedAndClosed ();

				/* If the seed EndPoint is null, Connect, Bind, etc has not yet
				 * been called. MS returns null in this case. */
				if (seed_endpoint == null)
					return null;

				int error;
				SocketAddress sa = LocalEndPoint_internal (m_Handle, (int) addressFamily, out error);

				if (error != 0)
					throw new SocketException (error);

				return seed_endpoint.Create (sa);
			}
		}

		static SocketAddress LocalEndPoint_internal (SafeSocketHandle safeHandle, int family, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return LocalEndPoint_internal (safeHandle.DangerousGetHandle (), family, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		/* Returns the local endpoint details in addr and port */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static SocketAddress LocalEndPoint_internal (IntPtr socket, int family, out int error);

		public bool Blocking {
			get { return is_blocking; }
			set {
				ThrowIfDisposedAndClosed ();

				int error;
				Blocking_internal (m_Handle, value, out error);

				if (error != 0)
					throw new SocketException (error);

				is_blocking = value;
			}
		}

		static void Blocking_internal (SafeSocketHandle safeHandle, bool block, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Blocking_internal (safeHandle.DangerousGetHandle (), block, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void Blocking_internal(IntPtr socket, bool block, out int error);

		public bool Connected {
			get { return is_connected; }
			internal set { is_connected = value; }
		}

		// FIXME: import from referencesource
		public bool NoDelay {
			get {
				ThrowIfDisposedAndClosed ();
				ThrowIfUdp ();

				return ((int) GetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay)) != 0;
			}

			set {
				ThrowIfDisposedAndClosed ();
				ThrowIfUdp ();
				SetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}

		public EndPoint RemoteEndPoint {
			get {
				ThrowIfDisposedAndClosed ();

				/* If the seed EndPoint is null, Connect, Bind, etc has
				 * not yet been called. MS returns null in this case. */
				if (!is_connected || seed_endpoint == null)
					return null;

				int error;
				SocketAddress sa = RemoteEndPoint_internal (m_Handle, (int) addressFamily, out error);

				if (error != 0)
					throw new SocketException (error);

				return seed_endpoint.Create (sa);
			}
		}

		static SocketAddress RemoteEndPoint_internal (SafeSocketHandle safeHandle, int family, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return RemoteEndPoint_internal (safeHandle.DangerousGetHandle (), family, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		/* Returns the remote endpoint details in addr and port */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static SocketAddress RemoteEndPoint_internal (IntPtr socket, int family, out int error);

		internal SafeHandle SafeHandle
		{
			get { return m_Handle; }
		}

#endregion

#region Select

		public static void Select (IList checkRead, IList checkWrite, IList checkError, int microSeconds)
		{
			var list = new List<Socket> ();
			AddSockets (list, checkRead, "checkRead");
			AddSockets (list, checkWrite, "checkWrite");
			AddSockets (list, checkError, "checkError");

			if (list.Count == 3)
				throw new ArgumentNullException ("checkRead, checkWrite, checkError", "All the lists are null or empty.");

			/* The 'sockets' array contains:
			 *  - READ socket 0-n, null,
			 *  - WRITE socket 0-n, null,
			 *  - ERROR socket 0-n, null */
			Socket [] sockets = list.ToArray ();

			int error;
			Select_internal (ref sockets, microSeconds, out error);

			if (error != 0)
				throw new SocketException (error);

			if (sockets == null) {
				if (checkRead != null)
					checkRead.Clear ();
				if (checkWrite != null)
					checkWrite.Clear ();
				if (checkError != null)
					checkError.Clear ();
				return;
			}

			int mode = 0;
			int count = sockets.Length;
			IList currentList = checkRead;
			int currentIdx = 0;
			for (int i = 0; i < count; i++) {
				Socket sock = sockets [i];
				if (sock == null) { // separator
					if (currentList != null) {
						// Remove non-signaled sockets after the current one
						int to_remove = currentList.Count - currentIdx;
						for (int k = 0; k < to_remove; k++)
							currentList.RemoveAt (currentIdx);
					}
					currentList = (mode == 0) ? checkWrite : checkError;
					currentIdx = 0;
					mode++;
					continue;
				}

				if (mode == 1 && currentList == checkWrite && !sock.is_connected) {
					if ((int) sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0)
						sock.is_connected = true;
				}

				/* Remove non-signaled sockets before the current one */
				while (((Socket) currentList [currentIdx]) != sock)
					currentList.RemoveAt (currentIdx);

				currentIdx++;
			}
		}

		static void AddSockets (List<Socket> sockets, IList list, string name)
		{
			if (list != null) {
				foreach (Socket sock in list) {
					if (sock == null) // MS throws a NullRef
						throw new ArgumentNullException (name, "Contains a null element");
					sockets.Add (sock);
				}
			}

			sockets.Add (null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Select_internal (ref Socket [] sockets, int microSeconds, out int error);

#endregion

#region Poll

		public bool Poll (int microSeconds, SelectMode mode)
		{
			ThrowIfDisposedAndClosed ();

			if (mode != SelectMode.SelectRead && mode != SelectMode.SelectWrite && mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (m_Handle, mode, microSeconds, out error);

			if (error != 0)
				throw new SocketException (error);

			if (mode == SelectMode.SelectWrite && result && !is_connected) {
				/* Update the is_connected state; for non-blocking Connect()
				 * this is when we can find out that the connect succeeded. */
				if ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0)
					is_connected = true;
			}

			return result;
		}

		static bool Poll_internal (SafeSocketHandle safeHandle, SelectMode mode, int timeout, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return Poll_internal (safeHandle.DangerousGetHandle (), mode, timeout, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Poll_internal (IntPtr socket, SelectMode mode, int timeout, out int error);

#endregion

#region Accept

		public Socket Accept()
		{
			ThrowIfDisposedAndClosed ();

			int error = 0;
			SafeSocketHandle safe_handle = Accept_internal (this.m_Handle, out error, is_blocking);

			if (error != 0) {
				if (is_closed)
					error = SOCKET_CLOSED_CODE;
				throw new SocketException(error);
			}

			Socket accepted = new Socket (this.AddressFamily, this.SocketType, this.ProtocolType, safe_handle) {
				seed_endpoint = this.seed_endpoint,
				Blocking = this.Blocking,
			};

			return accepted;
		}

		internal void Accept (Socket acceptSocket)
		{
			ThrowIfDisposedAndClosed ();

			int error = 0;
			SafeSocketHandle safe_handle = Accept_internal (this.m_Handle, out error, is_blocking);

			if (error != 0) {
				if (is_closed)
					error = SOCKET_CLOSED_CODE;
				throw new SocketException (error);
			}

			acceptSocket.addressFamily = this.AddressFamily;
			acceptSocket.socketType = this.SocketType;
			acceptSocket.protocolType = this.ProtocolType;
			acceptSocket.m_Handle = safe_handle;
			acceptSocket.is_connected = true;
			acceptSocket.seed_endpoint = this.seed_endpoint;
			acceptSocket.Blocking = this.Blocking;

			// FIXME: figure out what if anything else needs to be reset
		}

		public bool AcceptAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			if (!is_bound)
				throw new InvalidOperationException ("You must call the Bind method before performing this operation.");
			if (!is_listening)
				throw new InvalidOperationException ("You must call the Listen method before performing this operation.");
			if (e.BufferList != null)
				throw new ArgumentException ("Multiple buffers cannot be used with this method.");
			if (e.Count < 0)
				throw new ArgumentOutOfRangeException ("e.Count");

			Socket acceptSocket = e.AcceptSocket;
			if (acceptSocket != null) {
				if (acceptSocket.is_bound || acceptSocket.is_connected)
					throw new InvalidOperationException ("AcceptSocket: The socket must not be bound or connected.");
			}

			InitSocketAsyncEventArgs (e, AcceptAsyncCallback, e, SocketOperation.Accept);

			QueueIOSelectorJob (ReadSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptCallback, e.socket_async_result));

			return true;
		}

		static AsyncCallback AcceptAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.AcceptSocket = e.CurrentSocket.EndAccept (ares);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				if (e.AcceptSocket == null)
					e.AcceptSocket = new Socket (e.CurrentSocket.AddressFamily, e.CurrentSocket.SocketType, e.CurrentSocket.ProtocolType, null);
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginAccept(AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_bound || !is_listening)
				throw new InvalidOperationException ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Accept);

			QueueIOSelectorJob (ReadSem, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginAcceptCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			Socket acc_socket = null;
			try {
				if (sockares.AcceptSocket == null) {
					acc_socket = sockares.socket.Accept ();
				} else {
					acc_socket = sockares.AcceptSocket;
					sockares.socket.Accept (acc_socket);
				}

			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}
			sockares.Complete (acc_socket);
		});

		public IAsyncResult BeginAccept (Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");

			if (acceptSocket != null) {
				ThrowIfDisposedAndClosed (acceptSocket);

				if (acceptSocket.IsBound)
					throw new InvalidOperationException ();

				/* For some reason the MS runtime
				 * barfs if the new socket is not TCP,
				 * even though it's just about to blow
				 * away all those parameters
				 */
				if (acceptSocket.ProtocolType != ProtocolType.Tcp)
					throw new SocketException ((int)SocketError.InvalidArgument);
			}

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.AcceptReceive) {
				Buffer = new byte [receiveSize],
				Offset = 0,
				Size = receiveSize,
				SockFlags = SocketFlags.None,
				AcceptSocket = acceptSocket,
			};

			QueueIOSelectorJob (ReadSem, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptReceiveCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginAcceptReceiveCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			Socket acc_socket = null;

			try {
				if (sockares.AcceptSocket == null) {
					acc_socket = sockares.socket.Accept ();
				} else {
					acc_socket = sockares.AcceptSocket;
					sockares.socket.Accept (acc_socket);
				}
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			/* It seems the MS runtime special-cases 0-length requested receive data.  See bug 464201. */
			int total = 0;
			if (sockares.Size > 0) {
				try {
					SocketError error;
					total = acc_socket.Receive (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, out error);
					if (error != 0) {
						sockares.Complete (new SocketException ((int) error));
						return;
					}
				} catch (Exception e) {
					sockares.Complete (e);
					return;
				}
			}

			sockares.Complete (acc_socket, total);
		});

		public Socket EndAccept (IAsyncResult asyncResult)
		{
			int bytes;
			byte[] buffer;
			return EndAccept (out buffer, out bytes, asyncResult);
		}

		public Socket EndAccept (out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndAccept", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();

			buffer = sockares.Buffer.ToArray ();
			bytesTransferred = sockares.Total;

			return sockares.AcceptedSocket;
		}

		static SafeSocketHandle Accept_internal (SafeSocketHandle safeHandle, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				var ret = Accept_internal (safeHandle.DangerousGetHandle (), out error, blocking);
				return new SafeSocketHandle (ret, true);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		/* Creates a new system socket, returning the handle */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr Accept_internal (IntPtr sock, out int error, bool blocking);

#endregion

#region Bind

		public void Bind (EndPoint localEP)
		{
#if FEATURE_NO_BSD_SOCKETS
			throw new PlatformNotSupportedException ("System.Net.Sockets.Socket:Bind is not supported on this platform.");
#else
			ThrowIfDisposedAndClosed ();

			if (localEP == null)
				throw new ArgumentNullException("localEP");
				
			var ipEndPoint = localEP as IPEndPoint;
			if (ipEndPoint != null) {
				localEP = RemapIPEndPoint (ipEndPoint);	
			}
			
			int error;
			Bind_internal (m_Handle, localEP.Serialize(), out error);

			if (error != 0)
				throw new SocketException (error);
			if (error == 0)
				is_bound = true;

			seed_endpoint = localEP;
#endif // FEATURE_NO_BSD_SOCKETS
		}

		private static void Bind_internal (SafeSocketHandle safeHandle, SocketAddress sa, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Bind_internal (safeHandle.DangerousGetHandle (), sa, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Bind_internal(IntPtr sock, SocketAddress sa, out int error);

#endregion

#region Listen

		public void Listen (int backlog)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_bound)
				throw new SocketException ((int) SocketError.InvalidArgument);

			int error;
			Listen_internal(m_Handle, backlog, out error);

			if (error != 0)
				throw new SocketException (error);

			is_listening = true;
		}

		static void Listen_internal (SafeSocketHandle safeHandle, int backlog, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Listen_internal (safeHandle.DangerousGetHandle (), backlog, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Listen_internal (IntPtr sock, int backlog, out int error);

#endregion

#region Connect

		public void Connect (IPAddress address, int port)
		{
			Connect (new IPEndPoint (address, port));
		}

		public void Connect (string host, int port)
		{
			Connect (Dns.GetHostAddresses (host), port);
		}

		public void Connect (EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			IPEndPoint ep = remoteEP as IPEndPoint;
			/* Dgram uses Any to 'disconnect' */
			if (ep != null && socketType != SocketType.Dgram) {
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any))
					throw new SocketException ((int) SocketError.AddressNotAvailable);
			}

			if (is_listening)
				throw new InvalidOperationException ();
				
			if (ep != null) {
				remoteEP = RemapIPEndPoint (ep);
			}

			SocketAddress serial = remoteEP.Serialize ();

			int error = 0;
			Connect_internal (m_Handle, serial, out error, is_blocking);

			if (error == 0 || error == 10035)
				seed_endpoint = remoteEP; // Keep the ep around for non-blocking sockets

			if (error != 0) {
				if (is_closed)
					error = SOCKET_CLOSED_CODE;
				throw new SocketException (error);
			}

			is_connected = !(socketType == SocketType.Dgram && ep != null && (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)));
			is_bound = true;
		}

		public bool ConnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			if (is_listening)
				throw new InvalidOperationException ("You may not perform this operation after calling the Listen method.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP");

			InitSocketAsyncEventArgs (e, null, e, SocketOperation.Connect);

			try {
				IPAddress [] addresses;
				SocketAsyncResult ares;
				bool pending;

				/*
				 * Both BeginSConnect() and BeginMConnect() now return a `bool` indicating whether or
				 * not an async operation is pending.
				 */

				if (!GetCheckedIPs (e, out addresses)) {
					//NOTE: DualMode may cause Socket's RemoteEndpoint to differ in AddressFamily from the
					// SocketAsyncEventArgs, but the SocketAsyncEventArgs itself is not changed

					ares = new SocketAsyncResult (this, ConnectAsyncCallback, e, SocketOperation.Connect) {
						EndPoint = e.RemoteEndPoint
					};

					pending = BeginSConnect (ares);
				} else {
					DnsEndPoint dep = (DnsEndPoint)e.RemoteEndPoint;

					if (addresses == null)
						throw new ArgumentNullException ("addresses");
					if (addresses.Length == 0)
						throw new ArgumentException ("Empty addresses list");
					if (this.AddressFamily != AddressFamily.InterNetwork && this.AddressFamily != AddressFamily.InterNetworkV6)
						throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");
					if (dep.Port <= 0 || dep.Port > 65535)
						throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");

					ares = new SocketAsyncResult (this, ConnectAsyncCallback, e, SocketOperation.Connect) {
						Addresses = addresses,
						Port = dep.Port,
					};

					is_connected = false;

					pending = BeginMConnect (ares);
				}

				if (!pending) {
					/*
					 * On synchronous completion, the async callback will not be invoked.
					 *
					 * We need to call `EndConnect ()` here to close the socket and make sure
					 * that any pending exceptions are properly propagated.
					 *
					 * Note that we're not calling `e.Complete ()` (or resetting `e.in_progress`) here.
					 */
					e.CurrentSocket.EndConnect (ares);
				}

				return pending;
			} catch (SocketException exc) {
				e.SocketError = exc.SocketErrorCode;
				e.socket_async_result.Complete (exc, true);
				return false;
			} catch (Exception exc) {
				e.socket_async_result.Complete (exc, true);
				return false;
			}
		}

		public static void CancelConnectAsync (SocketAsyncEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			if (e.in_progress != 0 && e.LastOperation == SocketAsyncOperation.Connect)
				e.CurrentSocket?.Close ();
		}

		static AsyncCallback ConnectAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.CurrentSocket.EndConnect (ares);
			} catch (SocketException se) {
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginConnect (string host, int port, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (host == null)
				throw new ArgumentNullException ("host");
			if (addressFamily != AddressFamily.InterNetwork && addressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is valid only for sockets in the InterNetwork and InterNetworkV6 families");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");
			if (is_listening)
				throw new InvalidOperationException ();

			var sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Connect) {
				Port = port
			};

			var dnsRequest = Dns.GetHostAddressesAsync (host);
			dnsRequest.ContinueWith (t => {
				if (t.IsFaulted)
					sockares.Complete (t.Exception.InnerException);
				else if (t.IsCanceled)
					sockares.Complete (new OperationCanceledException ());
				else {
					sockares.Addresses = t.Result;
					BeginMConnect (sockares);
				}
			}, TaskScheduler.Default);

			return sockares;
		}

		public IAsyncResult BeginConnect (EndPoint remoteEP, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");
			if (is_listening)
				throw new InvalidOperationException ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Connect) {
				EndPoint = remoteEP,
			};

			BeginSConnect (sockares);
			return sockares;
		}

		public IAsyncResult BeginConnect (IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (addresses == null)
				throw new ArgumentNullException ("addresses");
			if (addresses.Length == 0)
				throw new ArgumentException ("Empty addresses list");
			if (this.AddressFamily != AddressFamily.InterNetwork && this.AddressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");
			if (is_listening)
				throw new InvalidOperationException ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, requestCallback, state, SocketOperation.Connect) {
				Addresses = addresses,
				Port = port,
			};

			is_connected = false;

			BeginMConnect (sockares);
			return sockares;
		}

		static bool BeginMConnect (SocketAsyncResult sockares)
		{
			Exception exc = null;

			for (int i = sockares.CurrentAddress; i < sockares.Addresses.Length; i++) {
				try {
					sockares.CurrentAddress++;
					sockares.EndPoint = new IPEndPoint (sockares.Addresses [i], sockares.Port);

					if (!sockares.socket.CanTryAddressFamily(sockares.EndPoint.AddressFamily))
						continue;

					return BeginSConnect (sockares);
				} catch (Exception e) {
					exc = e;
				}
			}

			sockares.Complete (exc, true);
			return false;
			throw exc;
		}

		static bool BeginSConnect (SocketAsyncResult sockares)
		{
			EndPoint remoteEP = sockares.EndPoint;
			// Bug #75154: Connect() should not succeed for .Any addresses.
			if (remoteEP is IPEndPoint) {
				IPEndPoint ep = (IPEndPoint) remoteEP;
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)) {
					sockares.Complete (new SocketException ((int) SocketError.AddressNotAvailable), true);
					return false;
				}

				sockares.EndPoint = remoteEP = sockares.socket.RemapIPEndPoint (ep);
			}

			if (!sockares.socket.CanTryAddressFamily(sockares.EndPoint.AddressFamily)) {
				sockares.Complete (new ArgumentException(SR.net_invalidAddressList), true);
				return false;
			}

			int error = 0;

			if (sockares.socket.connect_in_progress) {
				// This could happen when multiple IPs are used
				// Calling connect() again will reset the connection attempt and cause
				// an error. Better to just close the socket and move on.
				sockares.socket.connect_in_progress = false;
				sockares.socket.m_Handle.Dispose ();
				sockares.socket.m_Handle = new SafeSocketHandle (sockares.socket.Socket_internal (sockares.socket.addressFamily, sockares.socket.socketType, sockares.socket.protocolType, out error), true);
				if (error != 0) {
					sockares.Complete (new SocketException (error), true);
					return false;
				}
			}

			bool blk = sockares.socket.is_blocking;
			if (blk)
				sockares.socket.Blocking = false;
			Connect_internal (sockares.socket.m_Handle, remoteEP.Serialize (), out error, false);
			if (blk)
				sockares.socket.Blocking = true;

			if (error == 0) {
				// succeeded synch
				sockares.socket.is_connected = true;
				sockares.socket.is_bound = true;
				sockares.Complete (true);
				return false;
			}

			if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
				// error synch
				sockares.socket.is_connected = false;
				sockares.socket.is_bound = false;
				sockares.Complete (new SocketException (error), true);
				return false;
			}

			// continue asynch
			sockares.socket.is_connected = false;
			sockares.socket.is_bound = false;
			sockares.socket.connect_in_progress = true;

			IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, BeginConnectCallback, sockares));
			return true;
		}

		static IOAsyncCallback BeginConnectCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;

			if (sockares.EndPoint == null) {
				sockares.Complete (new SocketException ((int)SocketError.AddressNotAvailable));
				return;
			}

			try {
				int error = (int) sockares.socket.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);

				if (error == 0) {
					sockares.socket.seed_endpoint = sockares.EndPoint;
					sockares.socket.is_connected = true;
					sockares.socket.is_bound = true;
					sockares.socket.connect_in_progress = false;
					sockares.error = 0;
					sockares.Complete ();
					return;
				}

				if (sockares.Addresses == null) {
					sockares.socket.connect_in_progress = false;
					sockares.Complete (new SocketException (error));
					return;
				}

				if (sockares.CurrentAddress >= sockares.Addresses.Length) {
					sockares.Complete (new SocketException (error));
					return;
				}

				BeginMConnect (sockares);
			} catch (Exception e) {
				sockares.socket.connect_in_progress = false;
				sockares.Complete (e);
			}
		});

		public void EndConnect (IAsyncResult asyncResult)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndConnect", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();
		}

		static void Connect_internal (SafeSocketHandle safeHandle, SocketAddress sa, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				Connect_internal (safeHandle.DangerousGetHandle (), sa, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		/* Connects to the remote address */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Connect_internal(IntPtr sock, SocketAddress sa, out int error, bool blocking);

		/* Returns :
		 *  - false when it is ok to use RemoteEndPoint
		 *  - true when addresses must be used (and addresses could be null/empty) */
		bool GetCheckedIPs (SocketAsyncEventArgs e, out IPAddress [] addresses)
		{
			addresses = null;

			// Connect to the first address that match the host name, like:
			// http://blogs.msdn.com/ncl/archive/2009/07/20/new-ncl-features-in-net-4-0-beta-2.aspx
			// while skipping entries that do not match the address family
			DnsEndPoint dep = e.RemoteEndPoint as DnsEndPoint;
			if (dep != null) {
				addresses = Dns.GetHostAddresses (dep.Host);

				if (dep.AddressFamily == AddressFamily.Unspecified)
					return true;

				int last_valid = 0;
				for (int i = 0; i < addresses.Length; ++i) {
					if (addresses [i].AddressFamily != dep.AddressFamily)
						continue;

					addresses [last_valid++] = addresses [i];
				}

				if (last_valid != addresses.Length)
					Array.Resize (ref addresses, last_valid);
				return true;
			} else {
				e.SetConnectByNameError (null);
				return false;
			}
		}

#endregion

#region Disconnect

		/* According to the docs, the MS runtime will throw PlatformNotSupportedException
		 * if the platform is newer than w2k.  We should be able to cope... */
		public void Disconnect (bool reuseSocket)
		{
			ThrowIfDisposedAndClosed ();

			int error = 0;
			Disconnect_internal (m_Handle, reuseSocket, out error);

			if (error != 0) {
				if (error == 50) {
					/* ERROR_NOT_SUPPORTED */
					throw new PlatformNotSupportedException ();
				} else {
					throw new SocketException (error);
				}
			}

			is_connected = false;
			if (reuseSocket) {
				/* Do managed housekeeping here... */
			}
		}

		public bool DisconnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			InitSocketAsyncEventArgs (e, DisconnectAsyncCallback, e, SocketOperation.Disconnect);

			IOSelector.Add (e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, BeginDisconnectCallback, e.socket_async_result));

			return true;
		}

		static AsyncCallback DisconnectAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.CurrentSocket.EndDisconnect (ares);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginDisconnect (bool reuseSocket, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Disconnect) {
				ReuseSocket = reuseSocket,
			};

			IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, BeginDisconnectCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginDisconnectCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;

			try {
				sockares.socket.Disconnect (sockares.ReuseSocket);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete ();
		});

		public void EndDisconnect (IAsyncResult asyncResult)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndDisconnect", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();
		}

		static void Disconnect_internal (SafeSocketHandle safeHandle, bool reuse, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Disconnect_internal (safeHandle.DangerousGetHandle (), reuse, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Disconnect_internal (IntPtr sock, bool reuse, out int error);

#endregion

#region Receive

		public int Receive (byte [] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			int nativeError;
			int ret;
			unsafe {
				fixed (byte* pbuffer = buffer) {
					ret = Receive_internal (m_Handle, &pbuffer[offset], size, socketFlags, out nativeError, is_blocking);
				}
			}

			errorCode = (SocketError) nativeError;
			if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress) {
				is_connected = false;
				is_bound = false;
			} else {
				is_connected = true;
			}

			return ret;
		}

		int Receive (Memory<byte> buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			int nativeError;
			int ret;
			unsafe {
				using (var handle = buffer.Slice (offset, size).Pin ()) {
					ret = Receive_internal (m_Handle, (byte*)handle.Pointer, size, socketFlags, out nativeError, is_blocking);
				}
			}

			errorCode = (SocketError) nativeError;
			if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress) {
				is_connected = false;
				is_bound = false;
			} else {
				is_connected = true;
			}

			return ret;
		}

		[CLSCompliant (false)]
		public int Receive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null || buffers.Count == 0)
				throw new ArgumentNullException ("buffers");

			int numsegments = buffers.Count;
			int nativeError;
			int ret;

			GCHandle[] gch = new GCHandle[numsegments];
			try {
				unsafe {
					fixed (WSABUF* bufarray = new WSABUF[numsegments]) {
						for (int i = 0; i < numsegments; i++) {
							ArraySegment<byte> segment = buffers[i];

							if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
								throw new ArgumentOutOfRangeException ("segment");

							try {} finally {
								gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
							}

							bufarray[i].len = segment.Count;
							bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
						}

						ret = Receive_internal (m_Handle, bufarray, numsegments, socketFlags, out nativeError, is_blocking);
					}
				}
			} finally {
				for (int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated)
						gch[i].Free ();
				}
			}

			errorCode = (SocketError) nativeError;

			return ret;
		}

		public int Receive(Span<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
		{
			byte[] tempBuffer = new byte[buffer.Length];
			int result = Receive(tempBuffer, 0, tempBuffer.Length, socketFlags, out errorCode);
			tempBuffer.CopyTo (buffer);
			return result;
		}

		public int Send(ReadOnlySpan<byte> buffer, SocketFlags socketFlags, out SocketError errorCode)
		{
			byte[] bufferBytes = buffer.ToArray();
			return Send(bufferBytes, 0, bufferBytes.Length, socketFlags, out errorCode);
		}

		public int Receive (Span<byte> buffer, SocketFlags socketFlags)
		{
			byte[] tempBuffer = new byte[buffer.Length];
			int ret = Receive (tempBuffer, SocketFlags.None);
			tempBuffer.CopyTo (buffer);
			return ret;
		}

		public int Receive (Span<byte> buffer) => Receive (buffer, SocketFlags.None);

		public bool ReceiveAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			// LAME SPEC: the ArgumentException is never thrown, instead an NRE is
			// thrown when e.Buffer and e.BufferList are null (works fine when one is
			// set to a valid object)
			if (e.MemoryBuffer.Equals (default) && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			if (e.BufferList != null) {
				InitSocketAsyncEventArgs (e, ReceiveAsyncCallback, e, SocketOperation.ReceiveGeneric);

				e.socket_async_result.Buffers = e.BufferList;

				QueueIOSelectorJob (ReadSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveGenericCallback, e.socket_async_result));
			} else {
				InitSocketAsyncEventArgs (e, ReceiveAsyncCallback, e, SocketOperation.Receive);

				e.socket_async_result.Buffer = e.MemoryBuffer;
				e.socket_async_result.Offset = e.Offset;
				e.socket_async_result.Size = e.Count;

				QueueIOSelectorJob (ReadSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveCallback, e.socket_async_result));
			}

			return true;
		}

		static AsyncCallback ReceiveAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.SetBytesTransferred (e.CurrentSocket.EndReceive (ares));
			} catch (SocketException se){
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginReceive (byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			/* As far as I can tell from the docs and from experimentation, a pointer to the
			 * SocketError parameter is not supposed to be saved for the async parts.  And as we don't
			 * set any socket errors in the setup code, we just have to set it to Success. */
			errorCode = SocketError.Success;

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Receive) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (ReadSem, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginReceiveCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				unsafe {
					using (var pbuffer = sockares.Buffer.Slice (sockares.Offset, sockares.Size).Pin ()) {
						total = Receive_internal (sockares.socket.m_Handle, (byte*)pbuffer.Pointer, sockares.Size, sockares.SockFlags, out sockares.error, sockares.socket.is_blocking);
					}
				}
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			/* I assume the same SocketError semantics as above */
			errorCode = SocketError.Success;

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.ReceiveGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (ReadSem, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveGenericCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginReceiveGenericCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				total = sockares.socket.Receive (sockares.Buffers, sockares.SockFlags);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		public int EndReceive (IAsyncResult asyncResult, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndReceive", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			errorCode = sockares.ErrorCode;

			if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
				is_connected = false;

			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if (errorCode == SocketError.Success)
				sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		static unsafe int Receive_internal (SafeSocketHandle safeHandle, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), bufarray, count, flags, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static unsafe int Receive_internal (IntPtr sock, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking);

		static unsafe int Receive_internal (SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), buffer, count, flags, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static unsafe int Receive_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, out int error, bool blocking);

#endregion

#region ReceiveFrom

		public int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			SocketError errorCode;
			int ret = ReceiveFrom (buffer, offset, size, socketFlags, ref remoteEP, out errorCode);

			if (errorCode != SocketError.Success)
				throw new SocketException (errorCode);

			return ret;
		}

		internal int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, out SocketError errorCode)
		{
			SocketAddress sockaddr = remoteEP.Serialize();

			int nativeError;
			int cnt;
			unsafe {
				fixed (byte* pbuffer = buffer) {
					cnt = ReceiveFrom_internal (m_Handle, &pbuffer[offset], size, socketFlags, ref sockaddr, out nativeError, is_blocking);
				}
			}

			errorCode = (SocketError) nativeError;
			if (errorCode != SocketError.Success) {
				if (errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress) {
					is_connected = false;
				} else if (errorCode == SocketError.WouldBlock && is_blocking) { // This might happen when ReceiveTimeout is set
					errorCode = SocketError.TimedOut;
				}

				return 0;
			}

			is_connected = true;
			is_bound = true;

			/* If sockaddr is null then we're a connection oriented protocol and should ignore the
			 * remoteEP parameter (see MSDN documentation for Socket.ReceiveFrom(...) ) */
			if (sockaddr != null) {
				/* Stupidly, EndPoint.Create() is an instance method */
				remoteEP = remoteEP.Create (sockaddr);
			}

			seed_endpoint = remoteEP;

			return cnt;
		}

		int ReceiveFrom (Memory<byte> buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, out SocketError errorCode)
		{
			SocketAddress sockaddr = remoteEP.Serialize();

			int nativeError;
			int cnt;
			unsafe {
				using (var handle = buffer.Slice (offset, size).Pin ()) {
					cnt = ReceiveFrom_internal (m_Handle, (byte*)handle.Pointer, size, socketFlags, ref sockaddr, out nativeError, is_blocking);
				}
			}

			errorCode = (SocketError) nativeError;
			if (errorCode != SocketError.Success) {
				if (errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress) {
					is_connected = false;
				} else if (errorCode == SocketError.WouldBlock && is_blocking) { // This might happen when ReceiveTimeout is set
					errorCode = SocketError.TimedOut;
				}

				return 0;
			}

			is_connected = true;
			is_bound = true;

			/* If sockaddr is null then we're a connection oriented protocol and should ignore the
			 * remoteEP parameter (see MSDN documentation for Socket.ReceiveFrom(...) ) */
			if (sockaddr != null) {
				/* Stupidly, EndPoint.Create() is an instance method */
				remoteEP = remoteEP.Create (sockaddr);
			}

			seed_endpoint = remoteEP;

			return cnt;
		}

		public bool ReceiveFromAsync (SocketAsyncEventArgs e)
		{
			ThrowIfDisposedAndClosed ();

			// We do not support recv into multiple buffers yet
			if (e.BufferList != null)
				throw new NotSupportedException ("Mono doesn't support using BufferList at this point.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP", "Value cannot be null.");

			InitSocketAsyncEventArgs (e, ReceiveFromAsyncCallback, e, SocketOperation.ReceiveFrom);

			e.socket_async_result.Buffer = e.Buffer;
			e.socket_async_result.Offset = e.Offset;
			e.socket_async_result.Size = e.Count;
			e.socket_async_result.EndPoint = e.RemoteEndPoint;
			e.socket_async_result.SockFlags = e.SocketFlags;

			QueueIOSelectorJob (ReadSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveFromCallback, e.socket_async_result));

			return true;
		}

		static AsyncCallback ReceiveFromAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.SetBytesTransferred (e.CurrentSocket.EndReceiveFrom_internal ((SocketAsyncResult)ares, e));
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginReceiveFrom (byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.ReceiveFrom) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socketFlags,
				EndPoint = remoteEP,
			};

			QueueIOSelectorJob (ReadSem, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveFromCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginReceiveFromCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				SocketError errorCode;
				total = sockares.socket.ReceiveFrom (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, ref sockares.EndPoint, out errorCode);

				if (errorCode != SocketError.Success) {
					sockares.Complete (new SocketException (errorCode));
					return;
				}
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		public int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endPoint)
		{
			ThrowIfDisposedAndClosed ();

			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndReceiveFrom", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			endPoint = sockares.EndPoint;

			return sockares.Total;
		}

		int EndReceiveFrom_internal (SocketAsyncResult sockares, SocketAsyncEventArgs ares)
		{
			ThrowIfDisposedAndClosed ();

			if (Interlocked.CompareExchange (ref sockares.EndCalled, 1, 0) == 1)
				throw new InvalidOperationException ("EndReceiveFrom can only be called once per asynchronous operation");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();
			ares.RemoteEndPoint = sockares.EndPoint;
			return sockares.Total;
		}

		static unsafe int ReceiveFrom_internal (SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return ReceiveFrom_internal (safeHandle.DangerousGetHandle (), buffer, count, flags, ref sockaddr, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static unsafe int ReceiveFrom_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error, bool blocking);

#endregion

#region ReceiveMessageFrom

		[MonoTODO ("Not implemented")]
		public int ReceiveMessageFrom (byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			// FIXME: figure out how we get hold of the IPPacketInformation
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public bool ReceiveMessageFromAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IAsyncResult BeginReceiveMessageFrom (byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int EndReceiveMessageFrom (IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
		{
			ThrowIfDisposedAndClosed ();

			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			/*SocketAsyncResult sockares =*/ ValidateEndIAsyncResult (asyncResult, "EndReceiveMessageFrom", "asyncResult");

			throw new NotImplementedException ();
		}

#endregion

#region Send

		public int Send (byte [] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (size == 0) {
				errorCode = SocketError.Success;
				return 0;
			}

			int nativeError;
			int sent = 0;
			do {
				unsafe {
					fixed (byte *pbuffer = buffer) {
						sent += Send_internal (m_Handle, &pbuffer[offset + sent], size - sent, socketFlags, out nativeError, is_blocking);
					}
				}

				errorCode = (SocketError)nativeError;
				if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress) {
					is_connected = false;
					is_bound = false;
					break;
				} else {
					is_connected = true;
				}
			} while (sent < size);

			return sent;
		}

		[CLSCompliant (false)]
		public int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null)
				throw new ArgumentNullException ("buffers");
			if (buffers.Count == 0)
				throw new ArgumentException ("Buffer is empty", "buffers");

			int numsegments = buffers.Count;
			int nativeError;
			int ret;

			GCHandle[] gch = new GCHandle[numsegments];
			try {
				unsafe {
					fixed (WSABUF* bufarray = new WSABUF[numsegments]) {
						for(int i = 0; i < numsegments; i++) {
							ArraySegment<byte> segment = buffers[i];

							if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
								throw new ArgumentOutOfRangeException ("segment");

							try {} finally {
								gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
							}

							bufarray[i].len = segment.Count;
							bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
						}

						ret = Send_internal (m_Handle, bufarray, numsegments, socketFlags, out nativeError, is_blocking);
					}
				}
			} finally {
				for (int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated)
						gch[i].Free();
				}
			}

			errorCode = (SocketError)nativeError;

			return ret;
		}

		public int Send (ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
		{
			return Send (buffer.ToArray(), socketFlags);
		}

		public int Send (ReadOnlySpan<byte> buffer) => Send (buffer, SocketFlags.None);

		public bool SendAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			if (e.MemoryBuffer.Equals (default) && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			if (e.BufferList != null) {
				InitSocketAsyncEventArgs (e, SendAsyncCallback, e, SocketOperation.SendGeneric);

				e.socket_async_result.Buffers = e.BufferList;

				QueueIOSelectorJob (WriteSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, BeginSendGenericCallback, e.socket_async_result));
			} else {
				InitSocketAsyncEventArgs (e, SendAsyncCallback, e, SocketOperation.Send);

				e.socket_async_result.Buffer = e.MemoryBuffer;
				e.socket_async_result.Offset = e.Offset;
				e.socket_async_result.Size = e.Count;

				QueueIOSelectorJob (WriteSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, 0), e.socket_async_result));
			}

			return true;
		}

		static AsyncCallback SendAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.SetBytesTransferred (e.CurrentSocket.EndSend (ares));
			} catch (SocketException se){
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (!is_connected) {
				errorCode = SocketError.NotConnected;
				return null;
			}

			errorCode = SocketError.Success;

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Send) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (WriteSem, sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, 0), sockares));

			return sockares;
		}

		static void BeginSendCallback (SocketAsyncResult sockares, int sent_so_far)
		{
			int total = 0;

			try {
				unsafe {
					using (var pbuffer = sockares.Buffer.Slice (sockares.Offset, sockares.Size).Pin ()) {
						total = Socket.Send_internal (sockares.socket.m_Handle, (byte*)pbuffer.Pointer, sockares.Size, sockares.SockFlags, out sockares.error, false);
					}
				}
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			if (sockares.error == 0) {
				sent_so_far += total;
				sockares.Offset += total;
				sockares.Size -= total;

				if (sockares.socket.CleanedUp) {
					sockares.Complete (sent_so_far);
					return;
				}

				if (sockares.Size > 0) {
					IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, sent_so_far), sockares));
					return; // Have to finish writing everything. See bug #74475.
				}

				sockares.Total = sent_so_far;
			}

			sockares.Complete (sent_so_far);
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			if (!is_connected) {
				errorCode = SocketError.NotConnected;
				return null;
			}

			errorCode = SocketError.Success;

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.SendGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (WriteSem, sockares.Handle, new IOSelectorJob (IOOperation.Write, BeginSendGenericCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginSendGenericCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				total = sockares.socket.Send (sockares.Buffers, sockares.SockFlags);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		public int EndSend (IAsyncResult asyncResult, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndSend", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			errorCode = sockares.ErrorCode;

			if (errorCode != SocketError.Success && errorCode != SocketError.WouldBlock && errorCode != SocketError.InProgress)
				is_connected = false;

			/* If no socket error occurred, call CheckIfThrowDelayedException in
			 * case there are other kinds of exceptions that should be thrown.*/
			if (errorCode == SocketError.Success)
				sockares.CheckIfThrowDelayedException ();

			return sockares.Total;
		}

		static unsafe int Send_internal (SafeSocketHandle safeHandle, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Send_internal (safeHandle.DangerousGetHandle (), bufarray, count, flags, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static unsafe int Send_internal (IntPtr sock, WSABUF* bufarray, int count, SocketFlags flags, out int error, bool blocking);

		static unsafe int Send_internal (SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Send_internal (safeHandle.DangerousGetHandle (), buffer, count, flags, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static unsafe int Send_internal(IntPtr sock, byte* buffer, int count, SocketFlags flags, out int error, bool blocking);

#endregion

#region SendTo

		public int SendTo (byte [] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException("remoteEP");

			int error;
			int ret;
			unsafe {
				fixed (byte *pbuffer = buffer) {
					ret = SendTo_internal (m_Handle, &pbuffer[offset], size, socketFlags, remoteEP.Serialize (), out error, is_blocking);
				}
			}

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					is_connected = false;
				throw new SocketException (error);
			}

			is_connected = true;
			is_bound = true;
			seed_endpoint = remoteEP;

			return ret;
		}

		int SendTo (Memory<byte> buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();

			if (remoteEP == null)
				throw new ArgumentNullException("remoteEP");

			int error;
			int ret;
			unsafe {
				using (var pbuffer = buffer.Slice (offset, size).Pin ()) {
					ret = SendTo_internal (m_Handle, (byte*)pbuffer.Pointer, size, socketFlags, remoteEP.Serialize (), out error, is_blocking);
				}
			}

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					is_connected = false;
				throw new SocketException (error);
			}

			is_connected = true;
			is_bound = true;
			seed_endpoint = remoteEP;

			return ret;
		}

		public bool SendToAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			if (e.BufferList != null)
				throw new NotSupportedException ("Mono doesn't support using BufferList at this point.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP", "Value cannot be null.");

			InitSocketAsyncEventArgs (e, SendToAsyncCallback, e, SocketOperation.SendTo);

			e.socket_async_result.Buffer = e.Buffer;
			e.socket_async_result.Offset = e.Offset;
			e.socket_async_result.Size = e.Count;
			e.socket_async_result.SockFlags = e.SocketFlags;
			e.socket_async_result.EndPoint = e.RemoteEndPoint;

			QueueIOSelectorJob (WriteSem, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendToCallback ((SocketAsyncResult) s, 0), e.socket_async_result));

			return true;
		}

		static AsyncCallback SendToAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.SetBytesTransferred (e.CurrentSocket.EndSendTo (ares));
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete_internal ();
			}
		});

		public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEP, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.SendTo) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socketFlags,
				EndPoint = remoteEP,
			};

			QueueIOSelectorJob (WriteSem, sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendToCallback ((SocketAsyncResult) s, 0), sockares));

			return sockares;
		}

		static void BeginSendToCallback (SocketAsyncResult sockares, int sent_so_far)
		{
			int total = 0;
			try {
				total = sockares.socket.SendTo (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, sockares.EndPoint);

				if (sockares.error == 0) {
					sent_so_far += total;
					sockares.Offset += total;
					sockares.Size -= total;
				}

				if (sockares.Size > 0) {
					IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendToCallback ((SocketAsyncResult) s, sent_so_far), sockares));
					return; // Have to finish writing everything. See bug #74475.
				}

				sockares.Total = sent_so_far;
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete ();
		}

		public int EndSendTo (IAsyncResult asyncResult)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndSendTo", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		static unsafe int SendTo_internal (SafeSocketHandle safeHandle, byte* buffer, int count, SocketFlags flags, SocketAddress sa, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return SendTo_internal (safeHandle.DangerousGetHandle (), buffer, count, flags, sa, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static unsafe int SendTo_internal (IntPtr sock, byte* buffer, int count, SocketFlags flags, SocketAddress sa, out int error, bool blocking);

#endregion

#region SendFile

		public void SendFile (string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new NotSupportedException ();
			if (!is_blocking)
				throw new InvalidOperationException ();

			int error = 0;
			if (!SendFile_internal (m_Handle, fileName, preBuffer, postBuffer, flags, out error, is_blocking) || error != 0) {
				SocketException exc = new SocketException (error);
				if (exc.ErrorCode == 2 || exc.ErrorCode == 3)
					throw new FileNotFoundException ();
				throw exc;
			}
		}

		public IAsyncResult BeginSendFile (string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new NotSupportedException ();
			if (!File.Exists (fileName))
				throw new FileNotFoundException ();

			SendFileHandler handler = new SendFileHandler (SendFile);

			return new SendFileAsyncResult (handler, handler.BeginInvoke (fileName, preBuffer, postBuffer, flags, ar => callback (new SendFileAsyncResult (handler, ar)), state));
		}

		public void EndSendFile (IAsyncResult asyncResult)
		{
			ThrowIfDisposedAndClosed ();

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			SendFileAsyncResult ares = asyncResult as SendFileAsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			ares.Delegate.EndInvoke (ares.Original);
		}

		static bool SendFile_internal (SafeSocketHandle safeHandle, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return SendFile_internal (safeHandle.DangerousGetHandle (), filename, pre_buffer, post_buffer, flags, out error, blocking);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool SendFile_internal (IntPtr sock, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags, out int error, bool blocking);

		delegate void SendFileHandler (string fileName, byte [] preBuffer, byte [] postBuffer, TransmitFileOptions flags);

		sealed class SendFileAsyncResult : IAsyncResult {
			IAsyncResult ares;
			SendFileHandler d;

			public SendFileAsyncResult (SendFileHandler d, IAsyncResult ares)
			{
				this.d = d;
				this.ares = ares;
			}

			public object AsyncState {
				get { return ares.AsyncState; }
			}

			public WaitHandle AsyncWaitHandle {
				get { return ares.AsyncWaitHandle; }
			}

			public bool CompletedSynchronously {
				get { return ares.CompletedSynchronously; }
			}

			public bool IsCompleted {
				get { return ares.IsCompleted; }
			}

			public SendFileHandler Delegate {
				get { return d; }
			}

			public IAsyncResult Original {
				get { return ares; }
			}
		}

#endregion

#region SendPackets

		[MonoTODO ("Not implemented")]
		public bool SendPacketsAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			throw new NotImplementedException ();
		}

#endregion

#region DuplicateAndClose

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool Duplicate_internal(IntPtr handle, int targetProcessId, out IntPtr duplicateHandle, out MonoIOError error);

		[MonoLimitation ("We do not support passing sockets across processes, we merely allow this API to pass the socket across AppDomains")]
		public SocketInformation DuplicateAndClose (int targetProcessId)
		{
			var si = new SocketInformation ();
			si.Options =
				(is_listening      ? SocketInformationOptions.Listening : 0) |
				(is_connected      ? SocketInformationOptions.Connected : 0) |
				(is_blocking       ? 0 : SocketInformationOptions.NonBlocking) |
				(useOverlappedIO ? SocketInformationOptions.UseOnlyOverlappedIO : 0);

			IntPtr duplicateHandle;
			if (!Duplicate_internal (Handle, targetProcessId, out duplicateHandle, out MonoIOError error))
				throw MonoIO.GetException (error);

			si.ProtocolInformation = Mono.DataConverter.Pack ("iiiil", (int)addressFamily, (int)socketType, (int)protocolType, is_bound ? 1 : 0, (long)duplicateHandle);
 			m_Handle = null;
 
 			return si;
		}

#endregion

#region GetSocketOption

		public void GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			ThrowIfDisposedAndClosed ();

			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault, "Error trying to dereference an invalid pointer");

			int error;
			GetSocketOption_arr_internal (m_Handle, optionLevel, optionName, ref optionValue, out error);

			if (error != 0)
				throw new SocketException (error);
		}

		public byte [] GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
		{
			ThrowIfDisposedAndClosed ();

			int error;
			byte[] byte_val = new byte [optionLength];
			GetSocketOption_arr_internal (m_Handle, optionLevel, optionName, ref byte_val, out error);

			if (error != 0)
				throw new SocketException (error);

			return byte_val;
		}

		public object GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName)
		{
			ThrowIfDisposedAndClosed ();

			int error;
			object obj_val;
			GetSocketOption_obj_internal (m_Handle, optionLevel, optionName, out obj_val, out error);

			if (error != 0)
				throw new SocketException (error);

			if (optionName == SocketOptionName.Linger)
				return (LingerOption) obj_val;
			else if (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)
				return (MulticastOption) obj_val;
			else if (obj_val is int)
				return (int) obj_val;
			else
				return obj_val;
		}

		static void GetSocketOption_arr_internal (SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				GetSocketOption_arr_internal (safeHandle.DangerousGetHandle (), level, name, ref byte_val, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void GetSocketOption_arr_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val, out int error);

		static void GetSocketOption_obj_internal (SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, out object obj_val, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				GetSocketOption_obj_internal (safeHandle.DangerousGetHandle (), level, name, out obj_val, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void GetSocketOption_obj_internal(IntPtr socket, SocketOptionLevel level, SocketOptionName name, out object obj_val, out int error);

#endregion

#region SetSocketOption

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			ThrowIfDisposedAndClosed ();

			// I'd throw an ArgumentNullException, but this is what MS does.
			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault, "Error trying to dereference an invalid pointer");

			int error;
			SetSocketOption_internal (m_Handle, optionLevel, optionName, null, optionValue, 0, out error);

			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
		{
			ThrowIfDisposedAndClosed ();

			// NOTE: if a null is passed, the byte[] overload is used instead...
			if (optionValue == null)
				throw new ArgumentNullException("optionValue");

			int error;

			if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Linger) {
				LingerOption linger = optionValue as LingerOption;
				if (linger == null)
					throw new ArgumentException ("A 'LingerOption' value must be specified.", "optionValue");
				SetSocketOption_internal (m_Handle, optionLevel, optionName, linger, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IP && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				MulticastOption multicast = optionValue as MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (m_Handle, optionLevel, optionName, multicast, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IPv6 && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				IPv6MulticastOption multicast = optionValue as IPv6MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'IPv6MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (m_Handle, optionLevel, optionName, multicast, null, 0, out error);
			} else {
				throw new ArgumentException ("Invalid value specified.", "optionValue");
			}

			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
		{
			int int_val = optionValue ? 1 : 0;

			SetSocketOption (optionLevel, optionName, int_val);
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
		{
			ThrowIfDisposedAndClosed ();

			int error;
			SetSocketOption_internal (m_Handle, optionLevel, optionName, null, null, optionValue, out error);

			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}

		static void SetSocketOption_internal (SafeSocketHandle safeHandle, SocketOptionLevel level, SocketOptionName name, object obj_val, byte [] byte_val, int int_val, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				SetSocketOption_internal (safeHandle.DangerousGetHandle (), level, name, obj_val, byte_val, int_val, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void SetSocketOption_internal (IntPtr socket, SocketOptionLevel level, SocketOptionName name, object obj_val, byte [] byte_val, int int_val, out int error);

#endregion

#region IOControl

		public int IOControl (int ioControlCode, byte [] optionInValue, byte [] optionOutValue)
		{
			if (CleanedUp)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int result = IOControl_internal (m_Handle, ioControlCode, optionInValue, optionOutValue, out error);

			if (error != 0)
				throw new SocketException (error);
			if (result == -1)
				throw new InvalidOperationException ("Must use Blocking property instead.");

			return result;
		}

		static int IOControl_internal (SafeSocketHandle safeHandle, int ioctl_code, byte [] input, byte [] output, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return IOControl_internal (safeHandle.DangerousGetHandle (), ioctl_code, input, output, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		/* See Socket.IOControl, WSAIoctl documentation in MSDN. The common options between UNIX
		 * and Winsock are FIONREAD, FIONBIO and SIOCATMARK. Anything else will depend on the system
		 * except SIO_KEEPALIVE_VALS which is properly handled on both windows and linux. */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int IOControl_internal (IntPtr sock, int ioctl_code, byte [] input, byte [] output, out int error);

#endregion

#region Close

		public void Close ()
		{
			linger_timeout = 0;
			Dispose ();
		}

		public void Close (int timeout)
		{
			linger_timeout = timeout;
			Dispose ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void Close_internal (IntPtr socket, out int error);

#endregion

#region Shutdown

		public void Shutdown (SocketShutdown how)
		{
			const int enotconn = 10057;

			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new SocketException (enotconn); // Not connected

			int error;
			Shutdown_internal (m_Handle, how, out error);

			if (error == enotconn) {
				// POSIX requires this error to be returned from shutdown in some cases,
				//  even if the socket is actually connected.
				// We have already checked is_connected so it isn't meaningful or useful for
				//  us to throw if the OS says the socket was already closed when we tried to
				//  shut it down.
				// See https://bugs.freebsd.org/bugzilla/show_bug.cgi?id=227259
				return;
			}

			if (error != 0)
				throw new SocketException (error);
		}

		static void Shutdown_internal (SafeSocketHandle safeHandle, SocketShutdown how, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Shutdown_internal (safeHandle.DangerousGetHandle (), how, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void Shutdown_internal (IntPtr socket, SocketShutdown how, out int error);

#endregion

#region Dispose

		protected virtual void Dispose (bool disposing)
		{
			if (CleanedUp)
				return;

			m_IntCleanedUp = 1;
			bool was_connected = is_connected;
			is_connected = false;

			if (m_Handle != null) {
				is_closed = true;
				IntPtr x = Handle;

				if (was_connected)
					Linger (x);

				m_Handle.Dispose ();
			}
		}

		void Linger (IntPtr handle)
		{
			if (!is_connected || linger_timeout <= 0)
				return;

			/* We don't want to receive any more data */
			int error;
			Shutdown_internal (handle, SocketShutdown.Receive, out error);

			if (error != 0)
				return;

			int seconds = linger_timeout / 1000;
			int ms = linger_timeout % 1000;
			if (ms > 0) {
				/* If the other end closes, this will return 'true' with 'Available' == 0 */
				Poll_internal (handle, SelectMode.SelectRead, ms * 1000, out error);
				if (error != 0)
					return;
			}

			if (seconds > 0) {
				LingerOption linger = new LingerOption (true, seconds);
				SetSocketOption_internal (handle, SocketOptionLevel.Socket, SocketOptionName.Linger, linger, null, 0, out error);
				/* Not needed, we're closing upon return */
				//if (error != 0)
				//	return;
			}
		}

#endregion

		void ThrowIfDisposedAndClosed (Socket socket)
		{
			if (socket.CleanedUp && socket.is_closed)
				throw new ObjectDisposedException (socket.GetType ().ToString ());
		}

		void ThrowIfDisposedAndClosed ()
		{
			if (CleanedUp && is_closed)
				throw new ObjectDisposedException (GetType ().ToString ());
		}

		void ThrowIfBufferNull (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
		}

		void ThrowIfBufferOutOfRange (byte[] buffer, int offset, int size)
		{
			if (offset < 0)
				throw new ArgumentOutOfRangeException ("offset", "offset must be >= 0");
			if (offset > buffer.Length)
				throw new ArgumentOutOfRangeException ("offset", "offset must be <= buffer.Length");
			if (size < 0)
				throw new ArgumentOutOfRangeException ("size", "size must be >= 0");
			if (size > buffer.Length - offset)
				throw new ArgumentOutOfRangeException ("size", "size must be <= buffer.Length - offset");
		}

		void ThrowIfUdp ()
		{
			if (protocolType == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
		}

		SocketAsyncResult ValidateEndIAsyncResult (IAsyncResult ares, string methodName, string argName)
		{
			if (ares == null)
				throw new ArgumentNullException (argName);

			SocketAsyncResult sockares = ares as SocketAsyncResult;
			if (sockares == null)
				throw new ArgumentException ("Invalid IAsyncResult", argName);
			if (Interlocked.CompareExchange (ref sockares.EndCalled, 1, 0) == 1)
				throw new InvalidOperationException (methodName + " can only be called once per asynchronous operation");

			return sockares;
		}

		void QueueIOSelectorJob (SemaphoreSlim sem, IntPtr handle, IOSelectorJob job)
		{
			var task = sem.WaitAsync();
			// fast path without Task<Action> allocation.
			if (task.IsCompleted) {
				if (CleanedUp) {
					job.MarkDisposed ();
					return;
				}
				IOSelector.Add (handle, job);
			}
			else
			{
				task.ContinueWith( t => {
					if (CleanedUp) {
						job.MarkDisposed ();
						return;
					}
					IOSelector.Add(handle, job);
				});
			}
		}

		void InitSocketAsyncEventArgs (SocketAsyncEventArgs e, AsyncCallback callback, object state, SocketOperation operation)
		{
			e.socket_async_result.Init (this, callback, state, operation);
			if (e.AcceptSocket != null) {
				e.socket_async_result.AcceptSocket = e.AcceptSocket;
			}
			e.SetCurrentSocket (this);
			e.SetLastOperation (SocketOperationToSocketAsyncOperation (operation));
			e.SocketError = SocketError.Success;
			e.SetBytesTransferred (0);
		}

		SocketAsyncOperation SocketOperationToSocketAsyncOperation (SocketOperation op)
		{
			switch (op) {
			case SocketOperation.Connect:
				return SocketAsyncOperation.Connect;
			case SocketOperation.Accept:
				return SocketAsyncOperation.Accept;
			case SocketOperation.Disconnect:
				return SocketAsyncOperation.Disconnect;
			case SocketOperation.Receive:
			case SocketOperation.ReceiveGeneric:
				return SocketAsyncOperation.Receive;
			case SocketOperation.ReceiveFrom:
				return SocketAsyncOperation.ReceiveFrom;
			case SocketOperation.Send:
			case SocketOperation.SendGeneric:
				return SocketAsyncOperation.Send;
			case SocketOperation.SendTo:
				return SocketAsyncOperation.SendTo;
			default:
				throw new NotImplementedException (String.Format ("Operation {0} is not implemented", op));
			}
		}
		
		IPEndPoint RemapIPEndPoint (IPEndPoint input) {
			// If socket is DualMode ensure we automatically handle mapping IPv4 addresses to IPv6.
			if (IsDualMode && input.AddressFamily == AddressFamily.InterNetwork)
				return new IPEndPoint (input.Address.MapToIPv6 (), input.Port);
			
			return input;
		}
		
		[StructLayout (LayoutKind.Sequential)]
		struct WSABUF {
			public int len;
			public IntPtr buf;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void cancel_blocking_socket_operation (Thread thread);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern bool SupportsPortReuse (ProtocolType proto);

		internal static int FamilyHint {
			get {
				// Returns one of
				//	MONO_HINT_UNSPECIFIED		= 0,
				//	MONO_HINT_IPV4				= 1,
				//	MONO_HINT_IPV6				= 2,

				int hint = 0;
				if (OSSupportsIPv4) {
					hint = 1;
				}

				if (OSSupportsIPv6) {
					hint = hint == 0 ? 2 : 0;
				}

				return hint;
			}
		}

		static bool IsProtocolSupported (NetworkInterfaceComponent networkInterface)
		{
#if MOBILE
			return true;
#else
			var nics = NetworkInterface.GetAllNetworkInterfaces ();
			foreach (var adapter in nics) {
				if (adapter.Supports (networkInterface))
					return true;
			}

			return false;
#endif
		}

		internal void ReplaceHandleIfNecessaryAfterFailedConnect ()
		{
			/*
			 * This is called from `DualSocketMultipleConnectAsync.GetNextAddress(out Socket)`
			 * and `SingleSocketMultipleConnectAsync.GetNextAddress(out Socket)` when using
			 * the CoreFX version of `MultipleConnectAsync`.
			 */
		}
	}
}

