// System.Net.Sockets.Socket.cs
//
// Authors:
//	Phillip Pearson (pp@myelin.co.nz)
//	Dick Porter <dick@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sridhar Kulkarni (sridharkulkarni@gmail.com)
//	Brian Nickel (brian.nickel@gmail.com)
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
		private bool islistening;
		private bool useoverlappedIO;
		private const int SOCKET_CLOSED = 10004;

		static void AddSockets (List<Socket> sockets, IList list, string name)
		{
			if (list != null) {
				foreach (Socket sock in list) {
					if (sock == null) // MS throws a NullRef
						throw new ArgumentNullException ("name", "Contains a null element");
					sockets.Add (sock);
				}
			}

			sockets.Add (null);
		}
#if !TARGET_JVM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Select_internal (ref Socket [] sockets,
							int microSeconds,
							out int error);
#endif
		public static void Select (IList checkRead, IList checkWrite, IList checkError, int microSeconds)
		{
			var list = new List<Socket> ();
			AddSockets (list, checkRead, "checkRead");
			AddSockets (list, checkWrite, "checkWrite");
			AddSockets (list, checkError, "checkError");

			if (list.Count == 3) {
				throw new ArgumentNullException ("checkRead, checkWrite, checkError",
								 "All the lists are null or empty.");
			}

			int error;
			/*
			 * The 'sockets' array contains: READ socket 0-n, null,
			 * 				 WRITE socket 0-n, null,
			 *				 ERROR socket 0-n, null
			 */
			Socket [] sockets = list.ToArray ();
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

				if (mode == 1 && currentList == checkWrite && !sock.connected) {
					if ((int) sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0)
						sock.connected = true;
				}

				// Remove non-signaled sockets before the current one
				//int max = currentList.Count;
				while (((Socket) currentList [currentIdx]) != sock) {
					currentList.RemoveAt (currentIdx);
				}
				currentIdx++;
			}
		}

		// private constructor used by Accept, which already
		// has a socket handle to use
		internal Socket(AddressFamily family, SocketType type,
			       ProtocolType proto, IntPtr sock)
		{
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			socket=sock;
			connected=true;
		}

		private void SocketDefaults ()
		{
			try {
				if (address_family == AddressFamily.InterNetwork /* Need to test IPv6 further ||
										   address_family == AddressFamily.InterNetworkV6 */) {
					/* This is the default, but it
					 * probably has nasty side
					 * effects on Linux, as the
					 * socket option is kludged by
					 * turning on or off PMTU
					 * discovery...
					 */
					this.DontFragment = false;
				}

				//
				// Microsoft sets these to 8192, but we are going to keep them
				// both to the OS defaults as these have a big performance impact.
				// on WebClient performance.
				//
				//this.ReceiveBufferSize = 8192;
				//this.SendBufferSize = 8192;
			} catch (SocketException) {
			}
		}

#if !MOBILE
		public Socket (SocketInformation socketInformation)
		{
			var options = socketInformation.Options;
			islistening = (options & SocketInformationOptions.Listening) != 0;
			connected   = (options & SocketInformationOptions.Connected) != 0;
			blocking    = (options & SocketInformationOptions.NonBlocking) == 0;
			useoverlappedIO = (options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;

			var result = Mono.DataConverter.Unpack ("iiiil", socketInformation.ProtocolInformation, 0);
			
			address_family = (AddressFamily) (int) result [0];
			socket_type = (SocketType) (int) result [1];
			protocol_type = (ProtocolType) (int) result [2];
			isbound = (ProtocolType) (int) result [3] != 0;
			socket = (IntPtr) (long) result [4];
			SocketDefaults ();
		}
#endif
	
#if !TARGET_JVM
		// Returns the amount of data waiting to be read on socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Available_internal(IntPtr socket, out int error);
#endif

		public int Available {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int ret, error;
				
				ret = Available_internal(socket, out error);

				if (error != 0)
					throw new SocketException (error);

				return(ret);
			}
		}


		public bool DontFragment {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				bool dontfragment;
				
				if (address_family == AddressFamily.InterNetwork) {
					dontfragment = (int)(GetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment)) != 0;
				} else if (address_family == AddressFamily.InterNetworkV6) {
					dontfragment = (int)(GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment)) != 0;
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
				
				return(dontfragment);
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				if (address_family == AddressFamily.InterNetwork) {
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment, value?1:0);
				} else if (address_family == AddressFamily.InterNetworkV6) {
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment, value?1:0);
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
		}

		public bool EnableBroadcast {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				if (protocol_type != ProtocolType.Udp) {
					throw new SocketException ((int)SocketError.ProtocolOption);
				}
				
				return((int)(GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast)) != 0);
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				if (protocol_type != ProtocolType.Udp) {
					throw new SocketException ((int)SocketError.ProtocolOption);
				}

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, value?1:0);
			}
		}
		
		public bool ExclusiveAddressUse {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				return((int)(GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse)) != 0);
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				if (isbound) {
					throw new InvalidOperationException ("Bind has already been called for this socket");
				}
				
				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, value?1:0);
			}
		}
		
		public bool IsBound {
			get {
				return(isbound);
			}
		}
		
		public LingerOption LingerState {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				return((LingerOption)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger));
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				
				SetSocketOption (SocketOptionLevel.Socket,
						 SocketOptionName.Linger,
						 value);
			}
		}
		
		public bool MulticastLoopback {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				/* Even though this option can be set
				 * for TCP sockets on Linux, throw
				 * this exception anyway to be
				 * compatible (the MSDN docs say
				 * "Setting this property on a
				 * Transmission Control Protocol (TCP)
				 * socket will have no effect." but
				 * the MS runtime throws the
				 * exception...)
				 */
				if (protocol_type == ProtocolType.Tcp) {
					throw new SocketException ((int)SocketError.ProtocolOption);
				}
				
				bool multicastloopback;
				
				if (address_family == AddressFamily.InterNetwork) {
					multicastloopback = (int)(GetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback)) != 0;
				} else if (address_family == AddressFamily.InterNetworkV6) {
					multicastloopback = (int)(GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback)) != 0;
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
				
				return(multicastloopback);
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				/* Even though this option can be set
				 * for TCP sockets on Linux, throw
				 * this exception anyway to be
				 * compatible (the MSDN docs say
				 * "Setting this property on a
				 * Transmission Control Protocol (TCP)
				 * socket will have no effect." but
				 * the MS runtime throws the
				 * exception...)
				 */
				if (protocol_type == ProtocolType.Tcp) {
					throw new SocketException ((int)SocketError.ProtocolOption);
				}
				
				if (address_family == AddressFamily.InterNetwork) {
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value?1:0);
				} else if (address_family == AddressFamily.InterNetworkV6) {
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, value?1:0);
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
		}
		
		
		[MonoTODO ("This doesn't do anything on Mono yet")]
		public bool UseOnlyOverlappedIO {
			get {
				return(useoverlappedIO);
			}
			set {
				useoverlappedIO = value;
			}
		}

		public IntPtr Handle {
			get {
				return(socket);
			}
		}

#if !TARGET_JVM
		// Returns the local endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress LocalEndPoint_internal(IntPtr socket, int family, out int error);
#endif

		// Wish:  support non-IP endpoints.
		public EndPoint LocalEndPoint {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				
				/*
				 * If the seed EndPoint is null, Connect, Bind,
				 * etc has not yet been called. MS returns null
				 * in this case.
				 */
				if (seed_endpoint == null)
					return null;
				
				SocketAddress sa;
				int error;
				
				sa=LocalEndPoint_internal(socket, (int) address_family, out error);

				if (error != 0)
					throw new SocketException (error);

				return seed_endpoint.Create (sa);
			}
		}

		public SocketType SocketType {
			get {
				return(socket_type);
			}
		}

		public int SendTimeout {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (int)GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than -1");

				/* According to the MSDN docs we
				 * should adjust values between 1 and
				 * 499 to 500, but the MS runtime
				 * doesn't do this.
				 */
				if (value == -1)
					value = 0;

				SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.SendTimeout, value);
			}
		}

		public int ReceiveTimeout {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (int)GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than -1");

				if (value == -1) {
					value = 0;
				}
				
				SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReceiveTimeout, value);
			}
		}

		public bool AcceptAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (!IsBound)
				throw new InvalidOperationException ("You must call the Bind method before performing this operation.");
			if (!islistening)
				throw new InvalidOperationException ("You must call the Listen method before performing this operation.");
			if (e.BufferList != null)
				throw new ArgumentException ("Multiple buffers cannot be used with this method.");
			if (e.Count < 0)
				throw new ArgumentOutOfRangeException ("e.Count");

			Socket acceptSocket = e.AcceptSocket;
			if (acceptSocket != null) {
				if (acceptSocket.IsBound || acceptSocket.Connected)
					throw new InvalidOperationException ("AcceptSocket: The socket must not be bound or connected.");
			}

			e.curSocket = this;
			Worker w = e.Worker;
			w.Init (this, e, SocketOperation.Accept);
			int count;
			lock (readQ) {
				readQ.Enqueue (e.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, w.result);
			return true;
		}
		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static IntPtr Accept_internal(IntPtr sock, out int error, bool blocking);

		public Socket Accept() {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;
			IntPtr sock = (IntPtr) (-1);
			try {
				RegisterForBlockingSyscall ();
				sock = Accept_internal(socket, out error, blocking);
			} finally {
				UnRegisterForBlockingSyscall ();
			}

			if (error != 0) {
				if (closed)
					error = SOCKET_CLOSED;
				throw new SocketException(error);
			}

			Socket accepted = new Socket(this.AddressFamily, this.SocketType,
				this.ProtocolType, sock);

			accepted.seed_endpoint = this.seed_endpoint;
			accepted.Blocking = this.Blocking;
			return(accepted);
		}

		internal void Accept (Socket acceptSocket)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			
			int error = 0;
			IntPtr sock = (IntPtr)(-1);
			
			try {
				RegisterForBlockingSyscall ();
				sock = Accept_internal (socket, out error, blocking);
			} finally {
				UnRegisterForBlockingSyscall ();
			}
			
			if (error != 0) {
				if (closed)
					error = SOCKET_CLOSED;
				throw new SocketException (error);
			}
			
			acceptSocket.address_family = this.AddressFamily;
			acceptSocket.socket_type = this.SocketType;
			acceptSocket.protocol_type = this.ProtocolType;
			acceptSocket.socket = sock;
			acceptSocket.connected = true;
			acceptSocket.seed_endpoint = this.seed_endpoint;
			acceptSocket.Blocking = this.Blocking;

			/* FIXME: figure out what if anything else
			 * needs to be reset
			 */
		}

		public IAsyncResult BeginAccept(AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!isbound || !islistening)
				throw new InvalidOperationException ();

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Accept);
			int count;
			lock (readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		public IAsyncResult BeginAccept (int receiveSize,
						 AsyncCallback callback,
						 object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.AcceptReceive);
			req.Buffer = new byte[receiveSize];
			req.Offset = 0;
			req.Size = receiveSize;
			req.SockFlags = SocketFlags.None;
			int count;
			lock (readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		public IAsyncResult BeginAccept (Socket acceptSocket,
						 int receiveSize,
						 AsyncCallback callback,
						 object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");

			if (acceptSocket != null) {
				if (acceptSocket.disposed && acceptSocket.closed)
					throw new ObjectDisposedException (acceptSocket.GetType ().ToString ());

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
			
			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.AcceptReceive);
			req.Buffer = new byte[receiveSize];
			req.Offset = 0;
			req.Size = receiveSize;
			req.SockFlags = SocketFlags.None;
			req.AcceptSocket = acceptSocket;
			int count;
			lock (readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return(req);
		}

		public IAsyncResult BeginConnect (IPAddress address, int port,
						  AsyncCallback callback,
						  object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (address == null)
				throw new ArgumentNullException ("address");

			if (address.ToString ().Length == 0)
				throw new ArgumentException ("The length of the IP address is zero");

			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");

			if (islistening)
				throw new InvalidOperationException ();

			IPEndPoint iep = new IPEndPoint (address, port);
			return(BeginConnect (iep, callback, state));
		}

		public IAsyncResult BeginConnect (string host, int port,
						  AsyncCallback callback,
						  object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (host == null)
				throw new ArgumentNullException ("host");

			if (address_family != AddressFamily.InterNetwork &&
				address_family != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is valid only for sockets in the InterNetwork and InterNetworkV6 families");

			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");

			if (islistening)
				throw new InvalidOperationException ();

			return BeginConnect (Dns.GetHostAddresses (host), port, callback, state);
		}

		public IAsyncResult BeginDisconnect (bool reuseSocket,
						     AsyncCallback callback,
						     object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Disconnect);
			req.ReuseSocket = reuseSocket;
			socket_pool_queue (Worker.Dispatcher, req);
			return(req);
		}

		void CheckRange (byte[] buffer, int offset, int size)
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
		
		public IAsyncResult BeginReceive(byte[] buffer, int offset,
						 int size,
						 SocketFlags socket_flags,
						 AsyncCallback callback,
						 object state) {

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Receive);
			req.Buffer = buffer;
			req.Offset = offset;
			req.Size = size;
			req.SockFlags = socket_flags;
			int count;
			lock (readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		public IAsyncResult BeginReceive (byte[] buffer, int offset,
						  int size, SocketFlags flags,
						  out SocketError error,
						  AsyncCallback callback,
						  object state)
		{
			/* As far as I can tell from the docs and from
			 * experimentation, a pointer to the
			 * SocketError parameter is not supposed to be
			 * saved for the async parts.  And as we don't
			 * set any socket errors in the setup code, we
			 * just have to set it to Success.
			 */
			error = SocketError.Success;
			return (BeginReceive (buffer, offset, size, flags, callback, state));
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers,
						  SocketFlags socketFlags,
						  AsyncCallback callback,
						  object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.ReceiveGeneric);
			req.Buffers = buffers;
			req.SockFlags = socketFlags;
			int count;
			lock(readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}
		
		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers,
						  SocketFlags socketFlags,
						  out SocketError errorCode,
						  AsyncCallback callback,
						  object state)
		{
			/* I assume the same SocketError semantics as
			 * above
			 */
			errorCode = SocketError.Success;
			return (BeginReceive (buffers, socketFlags, callback, state));
		}

		public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset,
						     int size,
						     SocketFlags socket_flags,
						     ref EndPoint remote_end,
						     AsyncCallback callback,
						     object state) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			CheckRange (buffer, offset, size);

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.ReceiveFrom);
			req.Buffer = buffer;
			req.Offset = offset;
			req.Size = size;
			req.SockFlags = socket_flags;
			req.EndPoint = remote_end;
			int count;
			lock (readQ) {
				readQ.Enqueue (req.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		[MonoTODO]
		public IAsyncResult BeginReceiveMessageFrom (
			byte[] buffer, int offset, int size,
			SocketFlags socketFlags, ref EndPoint remoteEP,
			AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			CheckRange (buffer, offset, size);

			throw new NotImplementedException ();
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socket_flags,
					       AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);

			if (!connected)
				throw new SocketException ((int)SocketError.NotConnected);

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Send);
			req.Buffer = buffer;
			req.Offset = offset;
			req.Size = size;
			req.SockFlags = socket_flags;
			int count;
			lock (writeQ) {
				writeQ.Enqueue (req.Worker);
				count = writeQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset,
					       int size,
					       SocketFlags socketFlags,
					       out SocketError errorCode,
					       AsyncCallback callback,
					       object state)
		{
			if (!connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int)errorCode);
			}
			
			errorCode = SocketError.Success;
			
			return (BeginSend (buffer, offset, size, socketFlags, callback,
				state));
		}

		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers,
					       SocketFlags socketFlags,
					       AsyncCallback callback,
					       object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			if (!connected)
				throw new SocketException ((int)SocketError.NotConnected);

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.SendGeneric);
			req.Buffers = buffers;
			req.SockFlags = socketFlags;
			int count;
			lock (writeQ) {
				writeQ.Enqueue (req.Worker);
				count = writeQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers,
					       SocketFlags socketFlags,
					       out SocketError errorCode,
					       AsyncCallback callback,
					       object state)
		{
			if (!connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int)errorCode);
			}
			
			errorCode = SocketError.Success;
			return (BeginSend (buffers, socketFlags, callback, state));
		}

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

		public IAsyncResult BeginSendFile (string fileName,
						   AsyncCallback callback,
						   object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new NotSupportedException ();

			if (!File.Exists (fileName))
				throw new FileNotFoundException ();

			return BeginSendFile (fileName, null, null, 0, callback, state);
		}

		public IAsyncResult BeginSendFile (string fileName,
						   byte[] preBuffer,
						   byte[] postBuffer,
						   TransmitFileOptions flags,
						   AsyncCallback callback,
						   object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new NotSupportedException ();

			if (!File.Exists (fileName))
				throw new FileNotFoundException ();

			SendFileHandler d = new SendFileHandler (SendFile);
			return new SendFileAsyncResult (d, d.BeginInvoke (fileName, preBuffer, postBuffer, flags, ar => {
				SendFileAsyncResult sfar = new SendFileAsyncResult (d, ar);
				callback (sfar);
			}, state));
		}

		public IAsyncResult BeginSendTo(byte[] buffer, int offset,
						int size,
						SocketFlags socket_flags,
						EndPoint remote_end,
						AsyncCallback callback,
						object state) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.SendTo);
			req.Buffer = buffer;
			req.Offset = offset;
			req.Size = size;
			req.SockFlags = socket_flags;
			req.EndPoint = remote_end;
			int count;
			lock (writeQ) {
				writeQ.Enqueue (req.Worker);
				count = writeQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Bind_internal(IntPtr sock,
							 SocketAddress sa,
							 out int error);

		public void Bind(EndPoint local_end) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (local_end == null)
				throw new ArgumentNullException("local_end");
			
			int error;
			
			Bind_internal(socket, local_end.Serialize(), out error);
			if (error != 0)
				throw new SocketException (error);
			if (error == 0)
				isbound = true;
			
			seed_endpoint = local_end;
		}

		public void Connect (IPAddress address, int port)
		{
			Connect (new IPEndPoint (address, port));
		}
		
		public void Connect (IPAddress[] addresses, int port)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (addresses == null)
				throw new ArgumentNullException ("addresses");

			if (this.AddressFamily != AddressFamily.InterNetwork &&
				this.AddressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");

			if (islistening)
				throw new InvalidOperationException ();

			/* FIXME: do non-blocking sockets Poll here? */
			int error = 0;
			foreach (IPAddress address in addresses) {
				IPEndPoint iep = new IPEndPoint (address, port);
				SocketAddress serial = iep.Serialize ();
				
				Connect_internal (socket, serial, out error);
				if (error == 0) {
					connected = true;
					isbound = true;
					seed_endpoint = iep;
					return;
				} else if (error != (int)SocketError.InProgress &&
					   error != (int)SocketError.WouldBlock) {
					continue;
				}
				
				if (!blocking) {
					Poll (-1, SelectMode.SelectWrite);
					error = (int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
					if (error == 0) {
						connected = true;
						isbound = true;
						seed_endpoint = iep;
						return;
					}
				}
			}
			if (error != 0)
				throw new SocketException (error);
		}

		public void Connect (string host, int port)
		{
			IPAddress [] addresses = Dns.GetHostAddresses (host);
			Connect (addresses, port);
		}

		public bool DisconnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.Disconnect);
			socket_pool_queue (Worker.Dispatcher, e.Worker.result);
			return true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Disconnect_internal(IntPtr sock, bool reuse, out int error);

		/* According to the docs, the MS runtime will throw
		 * PlatformNotSupportedException if the platform is
		 * newer than w2k.  We should be able to cope...
		 */
		public void Disconnect (bool reuseSocket)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;
			
			Disconnect_internal (socket, reuseSocket, out error);

			if (error != 0) {
				if (error == 50) {
					/* ERROR_NOT_SUPPORTED */
					throw new PlatformNotSupportedException ();
				} else {
					throw new SocketException (error);
				}
			}

			connected = false;
			
			if (reuseSocket) {
				/* Do managed housekeeping here... */
			}
		}

#if !MOBILE
		[MonoLimitation ("We do not support passing sockets across processes, we merely allow this API to pass the socket across AppDomains")]
		public SocketInformation DuplicateAndClose (int targetProcessId)
		{
			var si = new SocketInformation ();
			si.Options =
				(islistening ? SocketInformationOptions.Listening : 0) |
				(connected ? SocketInformationOptions.Connected : 0) |
				(blocking ? 0 : SocketInformationOptions.NonBlocking) |
				(useoverlappedIO ? SocketInformationOptions.UseOnlyOverlappedIO : 0);

			si.ProtocolInformation = Mono.DataConverter.Pack ("iiiil", (int)address_family, (int)socket_type, (int)protocol_type, isbound ? 1 : 0, (long)socket);
			socket = (IntPtr) (-1);

			return si;
		}
#endif
	
		public Socket EndAccept (IAsyncResult result)
		{
			int bytes;
			byte[] buffer;
			
			return(EndAccept (out buffer, out bytes, result));
		}

		public Socket EndAccept (out byte[] buffer, IAsyncResult asyncResult)
		{
			int bytes;
			return(EndAccept (out buffer, out bytes, asyncResult));
		}

		public Socket EndAccept (out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");
			
			SocketAsyncResult req = asyncResult as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndAccept");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			req.CheckIfThrowDelayedException ();
			
			buffer = req.Buffer;
			bytesTransferred = req.Total;
			
			return(req.Socket);
		}

		public void EndConnect (IAsyncResult result)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndConnect");
			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
		}

		public void EndDisconnect (IAsyncResult asyncResult)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			SocketAsyncResult req = asyncResult as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndDisconnect");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			req.CheckIfThrowDelayedException ();
		}

		[MonoTODO]
		public int EndReceiveMessageFrom (IAsyncResult asyncResult,
						  ref SocketFlags socketFlags,
						  ref EndPoint endPoint,
						  out IPPacketInformation ipPacketInformation)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			SocketAsyncResult req = asyncResult as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndReceiveMessageFrom");
			throw new NotImplementedException ();
		}

		public void EndSendFile (IAsyncResult asyncResult)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			SendFileAsyncResult ares = asyncResult as SendFileAsyncResult;
			if (ares == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			ares.Delegate.EndInvoke (ares.Original);
		}

		public int EndSendTo (IAsyncResult result)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndSendTo");
			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

			req.CheckIfThrowDelayedException();
			return req.Total;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void GetSocketOption_arr_internal(IntPtr socket,
			SocketOptionLevel level, SocketOptionName name, ref byte[] byte_val,
			out int error);

		public void GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault,
					"Error trying to dereference an invalid pointer");

			int error;

			GetSocketOption_arr_internal (socket, optionLevel, optionName, ref optionValue,
				out error);
			if (error != 0)
				throw new SocketException (error);
		}

		public byte [] GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int length)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			byte[] byte_val=new byte[length];
			int error;

			GetSocketOption_arr_internal (socket, optionLevel, optionName, ref byte_val,
				out error);
			if (error != 0)
				throw new SocketException (error);

			return(byte_val);
		}

		// See Socket.IOControl, WSAIoctl documentation in MSDN. The
		// common options between UNIX and Winsock are FIONREAD,
		// FIONBIO and SIOCATMARK. Anything else will depend on the
		// system except SIO_KEEPALIVE_VALS which is properly handled
		// on both windows and linux.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int WSAIoctl (IntPtr sock, int ioctl_code, byte [] input,
			byte [] output, out int error);

		public int IOControl (int ioctl_code, byte [] in_value, byte [] out_value)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int result = WSAIoctl (socket, ioctl_code, in_value, out_value,
				out error);

			if (error != 0)
				throw new SocketException (error);
			
			if (result == -1)
				throw new InvalidOperationException ("Must use Blocking property instead.");

			return result;
		}

		public int IOControl (IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
		{
			return IOControl ((int) ioControlCode, optionInValue, optionOutValue);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Listen_internal(IntPtr sock, int backlog, out int error);

		public void Listen (int backlog)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!isbound)
				throw new SocketException ((int)SocketError.InvalidArgument);

			int error;
			Listen_internal(socket, backlog, out error);

			if (error != 0)
				throw new SocketException (error);

			islistening = true;
		}

		public bool Poll (int time_us, SelectMode mode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (mode != SelectMode.SelectRead &&
			    mode != SelectMode.SelectWrite &&
			    mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (socket, mode, time_us, out error);
			if (error != 0)
				throw new SocketException (error);

			if (mode == SelectMode.SelectWrite && result && !connected) {
				/* Update the connected state; for
				 * non-blocking Connect()s this is
				 * when we can find out that the
				 * connect succeeded.
				 */
				if ((int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error) == 0) {
					connected = true;
				}
			}
			
			return result;
		}

		public int Receive (byte [] buffer)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			SocketError error;

			int ret = Receive_nochecks (buffer, 0, buffer.Length, SocketFlags.None, out error);
			
			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Receive (byte [] buffer, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			SocketError error;

			int ret = Receive_nochecks (buffer, 0, buffer.Length, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, 0, size);

			SocketError error;

			int ret = Receive_nochecks (buffer, 0, size, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int offset, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);
			
			SocketError error;

			int ret = Receive_nochecks (buffer, offset, size, flags, out error);
			
			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, "Operation timed out.");
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);
			
			return Receive_nochecks (buffer, offset, size, flags, out error);
		}

		public bool ReceiveFromAsync (SocketAsyncEventArgs e)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// We do not support recv into multiple buffers yet
			if (e.BufferList != null)
				throw new NotSupportedException ("Mono doesn't support using BufferList at this point.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP", "Value cannot be null.");

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.ReceiveFrom);
			SocketAsyncResult res = e.Worker.result;
			res.Buffer = e.Buffer;
			res.Offset = e.Offset;
			res.Size = e.Count;
			res.EndPoint = e.RemoteEndPoint;
			res.SockFlags = e.SocketFlags;
			int count;
			lock (readQ) {
				readQ.Enqueue (e.Worker);
				count = readQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, res);
			return true;
		}

		public int ReceiveFrom (byte [] buffer, ref EndPoint remoteEP)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			return ReceiveFrom_nochecks (buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP);
		}

		public int ReceiveFrom (byte [] buffer, SocketFlags flags, ref EndPoint remoteEP)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			return ReceiveFrom_nochecks (buffer, 0, buffer.Length, flags, ref remoteEP);
		}

		public int ReceiveFrom (byte [] buffer, int size, SocketFlags flags,
					ref EndPoint remoteEP)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			if (size < 0 || size > buffer.Length)
				throw new ArgumentOutOfRangeException ("size");

			return ReceiveFrom_nochecks (buffer, 0, size, flags, ref remoteEP);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int RecvFrom_internal(IntPtr sock,
							    byte[] buffer,
							    int offset,
							    int count,
							    SocketFlags flags,
							    ref SocketAddress sockaddr,
							    out int error);

		public int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags flags,
					ref EndPoint remoteEP)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			CheckRange (buffer, offset, size);

			return ReceiveFrom_nochecks (buffer, offset, size, flags, ref remoteEP);
		}

		internal int ReceiveFrom_nochecks (byte [] buf, int offset, int size, SocketFlags flags,
						   ref EndPoint remote_end)
		{
			int error;
			return ReceiveFrom_nochecks_exc (buf, offset, size, flags, ref remote_end, true, out error);
		}

		internal int ReceiveFrom_nochecks_exc (byte [] buf, int offset, int size, SocketFlags flags,
						   ref EndPoint remote_end, bool throwOnError, out int error)
		{
			SocketAddress sockaddr = remote_end.Serialize();
			int cnt = RecvFrom_internal (socket, buf, offset, size, flags, ref sockaddr, out error);
			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					connected = false;
				else if (err == SocketError.WouldBlock && blocking) { // This might happen when ReceiveTimeout is set
					if (throwOnError)	
						throw new SocketException ((int) SocketError.TimedOut, "Operation timed out");
					error = (int) SocketError.TimedOut;
					return 0;
				}

				if (throwOnError)
					throw new SocketException (error);
				return 0;
			}

			connected = true;
			isbound = true;

			// If sockaddr is null then we're a connection
			// oriented protocol and should ignore the
			// remote_end parameter (see MSDN
			// documentation for Socket.ReceiveFrom(...) )
			
			if ( sockaddr != null ) {
				// Stupidly, EndPoint.Create() is an
				// instance method
				remote_end = remote_end.Create (sockaddr);
			}
			
			seed_endpoint = remote_end;
			
			return cnt;
		}

		[MonoTODO ("Not implemented")]
		public bool ReceiveMessageFromAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			
			throw new NotImplementedException ();
		}
		
		[MonoTODO ("Not implemented")]
		public int ReceiveMessageFrom (byte[] buffer, int offset,
					       int size,
					       ref SocketFlags socketFlags,
					       ref EndPoint remoteEP,
					       out IPPacketInformation ipPacketInformation)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			CheckRange (buffer, offset, size);

			/* FIXME: figure out how we get hold of the
			 * IPPacketInformation
			 */
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public bool SendPacketsAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			
			throw new NotImplementedException ();
		}

		public int Send (byte [] buf)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Send_nochecks (buf, 0, buf.Length, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			SocketError error;

			int ret = Send_nochecks (buf, 0, buf.Length, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buf");

			CheckRange (buf, 0, size);

			SocketError error;

			int ret = Send_nochecks (buf, 0, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, int offset, int size, SocketFlags flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buf, offset, size);

			SocketError error;

			int ret = Send_nochecks (buf, offset, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buf == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buf, offset, size);

			return Send_nochecks (buf, offset, size, flags, out error);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool SendFile (IntPtr sock, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags);

		public void SendFile (string fileName)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new NotSupportedException ();

			if (!blocking)
				throw new InvalidOperationException ();

			SendFile (fileName, null, null, 0);
		}

		public void SendFile (string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new NotSupportedException ();

			if (!blocking)
				throw new InvalidOperationException ();

			if (!SendFile (socket, fileName, preBuffer, postBuffer, flags)) {
				SocketException exc = new SocketException ();
				if (exc.ErrorCode == 2 || exc.ErrorCode == 3)
					throw new FileNotFoundException ();
				throw exc;
			}
		}

		public bool SendToAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (e.BufferList != null)
				throw new NotSupportedException ("Mono doesn't support using BufferList at this point.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP", "Value cannot be null.");

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.SendTo);
			SocketAsyncResult res = e.Worker.result;
			res.Buffer = e.Buffer;
			res.Offset = e.Offset;
			res.Size = e.Count;
			res.SockFlags = e.SocketFlags;
			res.EndPoint = e.RemoteEndPoint;
			int count;
			lock (writeQ) {
				writeQ.Enqueue (e.Worker);
				count = writeQ.Count;
			}
			if (count == 1)
				socket_pool_queue (Worker.Dispatcher, res);
			return true;
		}
		
		public int SendTo (byte [] buffer, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			return SendTo_nochecks (buffer, 0, buffer.Length, SocketFlags.None, remote_end);
		}

		public int SendTo (byte [] buffer, SocketFlags flags, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");
				
			return SendTo_nochecks (buffer, 0, buffer.Length, flags, remote_end);
		}

		public int SendTo (byte [] buffer, int size, SocketFlags flags, EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			CheckRange (buffer, 0, size);

			return SendTo_nochecks (buffer, 0, size, flags, remote_end);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int SendTo_internal(IntPtr sock,
							  byte[] buffer,
							  int offset,
							  int count,
							  SocketFlags flags,
							  SocketAddress sa,
							  out int error);

		public int SendTo (byte [] buffer, int offset, int size, SocketFlags flags,
				   EndPoint remote_end)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (remote_end == null)
				throw new ArgumentNullException("remote_end");

			CheckRange (buffer, offset, size);

			return SendTo_nochecks (buffer, offset, size, flags, remote_end);
		}

		internal int SendTo_nochecks (byte [] buffer, int offset, int size, SocketFlags flags,
					      EndPoint remote_end)
		{
			SocketAddress sockaddr = remote_end.Serialize ();

			int ret, error;

			ret = SendTo_internal (socket, buffer, offset, size, flags, sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					connected = false;

				throw new SocketException (error);
			}

			connected = true;
			isbound = true;
			seed_endpoint = remote_end;
			
			return ret;
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// I'd throw an ArgumentNullException, but this is what MS does.
			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault,
					"Error trying to dereference an invalid pointer");
			
			int error;

			SetSocketOption_internal (socket, optionLevel, optionName, null,
						 optionValue, 0, out error);

			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// NOTE: if a null is passed, the byte[] overload is used instead...
			if (optionValue == null)
				throw new ArgumentNullException("optionValue");
			
			int error;

			if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.Linger) {
				LingerOption linger = optionValue as LingerOption;
				if (linger == null)
					throw new ArgumentException ("A 'LingerOption' value must be specified.", "optionValue");
				SetSocketOption_internal (socket, optionLevel, optionName, linger, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IP && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				MulticastOption multicast = optionValue as MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (socket, optionLevel, optionName, multicast, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IPv6 && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				IPv6MulticastOption multicast = optionValue as IPv6MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'IPv6MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (socket, optionLevel, optionName, multicast, null, 0, out error);
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
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int int_val = (optionValue) ? 1 : 0;
			SetSocketOption_internal (socket, optionLevel, optionName, null, null, int_val, out error);
			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}
	}
}

