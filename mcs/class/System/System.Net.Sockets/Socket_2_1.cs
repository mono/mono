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
using System.IO;
using System.Security;
using System.Text;

#if !NET_2_1
using System.Net.Configuration;
using System.Net.NetworkInformation;
#endif

namespace System.Net.Sockets {

	public partial class Socket : IDisposable {
		[StructLayout (LayoutKind.Sequential)]
		struct WSABUF {
			public int len;
			public IntPtr buf;
		}

		internal Queue readQ = new Queue (2);
		internal Queue writeQ = new Queue (2);

		/*
		 *	These two fields are looked up by name by the runtime, don't change
		 *  their name without also updating the runtime code.
		 */
		private static int ipv4Supported = -1, ipv6Supported = -1;
		int linger_timeout;

		static Socket ()
		{
			// initialize ipv4Supported and ipv6Supported
			CheckProtocolSupport ();
		}

		internal static void CheckProtocolSupport ()
		{
			if(ipv4Supported == -1) {
				try {
					Socket tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					tmp.Close();

					ipv4Supported = 1;
				} catch {
					ipv4Supported = 0;
				}
			}

			if (ipv6Supported == -1) {
				// We need to put a try/catch around ConfigurationManager methods as will always throw an exception 
				// when run in a mono embedded application.  This occurs as embedded applications do not have a setup
				// for application config.  The exception is not thrown when called from a normal .NET application. 
				//
				// We, then, need to guard calls to the ConfigurationManager.  If the config is not found or throws an
				// exception, will fall through to the existing Socket / API directly below in the code.
				//
				// Also note that catching ConfigurationErrorsException specifically would require library dependency
				// System.Configuration, and wanted to avoid that.
#if !NET_2_1
#if CONFIGURATION_DEP
				try {
					SettingsSection config;
					config = (SettingsSection) System.Configuration.ConfigurationManager.GetSection ("system.net/settings");
					if (config != null)
						ipv6Supported = config.Ipv6.Enabled ? -1 : 0;
				} catch {
					ipv6Supported = -1;
				}
#else
				try {
					NetConfig config = System.Configuration.ConfigurationSettings.GetConfig("system.net/settings") as NetConfig;
					if (config != null)
						ipv6Supported = config.ipv6Enabled ? -1 : 0;
				} catch {
					ipv6Supported = -1;
				}
#endif
#endif
				if (ipv6Supported != 0) {
					try {
						Socket tmp = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						tmp.Close();

						ipv6Supported = 1;
					} catch {
						ipv6Supported = 0;
					}
				}
			}
		}

		public static bool SupportsIPv4 {
			get {
				CheckProtocolSupport();
				return ipv4Supported == 1;
			}
		}

		[ObsoleteAttribute ("Use OSSupportsIPv6 instead")]
		public static bool SupportsIPv6 {
			get {
				CheckProtocolSupport();
				return ipv6Supported == 1;
			}
		}
#if NET_2_1
		public static bool OSSupportsIPv4 {
			get {
				CheckProtocolSupport();
				return ipv4Supported == 1;
			}
		}
#endif
#if NET_2_1
		public static bool OSSupportsIPv6 {
			get {
				CheckProtocolSupport();
				return ipv6Supported == 1;
			}
		}
#else
		public static bool OSSupportsIPv6 {
			get {
				NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces ();
				
				foreach (NetworkInterface adapter in nics) {
					if (adapter.Supports (NetworkInterfaceComponent.IPv6))
						return true;
				}
				return false;
			}
		}
#endif

		/* the field "socket" is looked up by name by the runtime */
		private SafeSocketHandle socket;
		private AddressFamily address_family;
		private SocketType socket_type;
		private ProtocolType protocol_type;
		internal bool blocking=true;
		internal bool isbound;
		/* When true, the socket was connected at the time of
		 * the last IO operation
		 */
		internal bool connected;
		/* true if we called Close_internal */
		private bool closed;
		internal bool disposed;
		internal bool connect_in_progress;

		/*
		 * This EndPoint is used when creating new endpoints. Because
		 * there are many types of EndPoints possible,
		 * seed_endpoint.Create(addr) is used for creating new ones.
		 * As such, this value is set on Bind, SentTo, ReceiveFrom,
		 * Connect, etc.
 		 */
		internal EndPoint seed_endpoint = null;

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Socket_internal(AddressFamily family,
						      SocketType type,
						      ProtocolType proto,
						      out int error);
		
		public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
#if NET_2_1 && !MOBILE
			switch (addressFamily) {
			case AddressFamily.InterNetwork:	// ok
			case AddressFamily.InterNetworkV6:	// ok
			case AddressFamily.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			// case AddressFamily.Unspecified:
			default:
				throw new ArgumentException ("addressFamily");
			}

			switch (socketType) {
			case SocketType.Stream:			// ok
			case SocketType.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			default:
				throw new ArgumentException ("socketType");
			}

			switch (protocolType) {
			case ProtocolType.Tcp:			// ok
			case ProtocolType.Unspecified:		// ok
			case ProtocolType.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			default:
				throw new ArgumentException ("protocolType");
			}
#endif
			address_family = addressFamily;
			socket_type = socketType;
			protocol_type = protocolType;
			
			int error;
			
			var handle = Socket_internal (addressFamily, socketType, protocolType, out error);
			socket = new SafeSocketHandle (handle, true);

			if (error != 0)
				throw new SocketException (error);
#if !NET_2_1 || MOBILE
			SocketDefaults ();
#endif
		}

		[MonoTODO ("Currently hardcoded to IPv4. Ideally, support v4/v6 dual-stack.")]
		public Socket (SocketType socketType, ProtocolType protocolType)
			: this (AddressFamily.InterNetwork, socketType, protocolType)
		{
		}

		~Socket ()
		{
			Dispose (false);
		}


		public AddressFamily AddressFamily {
			get { return address_family; }
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void Blocking_internal(IntPtr socket,
							     bool block,
							     out int error);

		private static void Blocking_internal (SafeSocketHandle safeHandle,
							     bool block,
							     out int error)
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

		public bool Blocking {
			get {
				return(blocking);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int error;
				
				Blocking_internal (socket, value, out error);

				if (error != 0)
					throw new SocketException (error);
				
				blocking=value;
			}
		}

		public bool Connected {
			get { return connected; }
			internal set { connected = value; }
		}

		public ProtocolType ProtocolType {
			get { return protocol_type; }
		}

		public bool NoDelay {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				ThrowIfUpd ();

				return (int)(GetSocketOption (
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay)) != 0;
			}

			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				ThrowIfUpd ();

				SetSocketOption (
					SocketOptionLevel.Tcp,
					SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}

		public int ReceiveBufferSize {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				return((int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer));
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				if (value < 0) {
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");
				}
				
				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
			}
		}

		public int SendBufferSize {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				return((int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendBuffer));
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				if (value < 0) {
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");
				}
				
				SetSocketOption (SocketOptionLevel.Socket,
						 SocketOptionName.SendBuffer,
						 value);
			}
		}

		public short Ttl {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				
				short ttl_val;
				
				if (address_family == AddressFamily.InterNetwork) {
					ttl_val = (short)((int)GetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive));
				} else if (address_family == AddressFamily.InterNetworkV6) {
					ttl_val = (short)((int)GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit));
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
				
				return(ttl_val);
			}
			set {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}
				if (value < 0) {
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");
				}
				
				if (address_family == AddressFamily.InterNetwork) {
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, value);
				} else if (address_family == AddressFamily.InterNetworkV6) {
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit, value);
				} else {
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
		}

		// Returns the remote endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress RemoteEndPoint_internal(IntPtr socket, int family, out int error);

		private static SocketAddress RemoteEndPoint_internal (SafeSocketHandle safeHandle, int family, out int error)
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

		public EndPoint RemoteEndPoint {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				
				/*
				 * If the seed EndPoint is null, Connect, Bind,
				 * etc has not yet been called. MS returns null
				 * in this case.
				 */
				if (!connected || seed_endpoint == null)
					return null;
				SocketAddress sa;
				int error;
				
				sa=RemoteEndPoint_internal(socket, (int) address_family, out error);

				if (error != 0)
					throw new SocketException (error);

				return seed_endpoint.Create (sa);
			}
		}

		void Linger (IntPtr handle)
		{
			if (!connected || linger_timeout <= 0)
				return;

			// We don't want to receive any more data
			int error;
			Shutdown_internal (handle, SocketShutdown.Receive, out error);
			if (error != 0)
				return;

			int seconds = linger_timeout / 1000;
			int ms = linger_timeout % 1000;
			if (ms > 0) {
				// If the other end closes, this will return 'true' with 'Available' == 0
				Poll_internal (handle, SelectMode.SelectRead, ms * 1000, out error);
				if (error != 0)
					return;

			}
			if (seconds > 0) {
				LingerOption linger = new LingerOption (true, seconds);
				SetSocketOption_internal (handle, SocketOptionLevel.Socket, SocketOptionName.Linger, linger, null, 0, out error);
				/* Not needed, we're closing upon return */
				/*if (error != 0)
					return; */
			}
		}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void cancel_blocking_socket_operation (Thread thread);

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			disposed = true;
			bool was_connected = connected;
			connected = false;
			
			if (socket != null) {
				closed = true;
				IntPtr x = Handle;

				if (was_connected)
					Linger (x);

				socket.Dispose ();
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		// Closes the socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void Close_internal(IntPtr socket, out int error);

		public void Close ()
		{
			linger_timeout = 0;
			((IDisposable) this).Dispose ();
		}

		public void Close (int timeout) 
		{
			linger_timeout = timeout;
			((IDisposable) this).Dispose ();
		}

		// Connects to the remote address
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Connect_internal(IntPtr sock,
							    SocketAddress sa,
							    out int error);
		
		private static void Connect_internal (SafeSocketHandle safeHandle,
							    SocketAddress sa,
							    out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				Connect_internal (safeHandle.DangerousGetHandle (), sa, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		public void Connect (EndPoint remoteEP)
		{
			SocketAddress serial = null;

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			IPEndPoint ep = remoteEP as IPEndPoint;
			if (ep != null && socket_type != SocketType.Dgram) /* Dgram uses Any to 'disconnect' */
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any))
					throw new SocketException ((int) SocketError.AddressNotAvailable);

			if (islistening)
				throw new InvalidOperationException ();
			serial = remoteEP.Serialize ();

			int error = 0;

			Connect_internal (socket, serial, out error);

			if (error == 0 || error == 10035)
				seed_endpoint = remoteEP; // Keep the ep around for non-blocking sockets

			if (error != 0) {
				if (closed)
					error = SOCKET_CLOSED;
				throw new SocketException (error);
			}

			if (socket_type == SocketType.Dgram && ep != null && (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)))
				connected = false;
			else
				connected = true;
			isbound = true;
		}

		public bool ReceiveAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// LAME SPEC: the ArgumentException is never thrown, instead an NRE is
			// thrown when e.Buffer and e.BufferList are null (works fine when one is
			// set to a valid object)
			if (e.Buffer == null && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			e.curSocket = this;
			SocketOperation op = (e.Buffer != null) ? SocketOperation.Receive : SocketOperation.ReceiveGeneric;
			e.Worker.Init (this, e, op);
			SocketAsyncResult res = e.Worker.result;
			if (e.Buffer != null) {
				res.Buffer = e.Buffer;
				res.Offset = e.Offset;
				res.Size = e.Count;
			} else {
				res.Buffers = e.BufferList;
			}
			res.SockFlags = e.SocketFlags;
			int count;
			lock (readQ) {
				readQ.Enqueue (e.Worker);
				count = readQ.Count;
			}
			if (count == 1) {
				// Receive takes care of ReceiveGeneric
				socket_pool_queue (SocketAsyncWorker.Dispatcher, res);
			}

			return true;
		}

		public bool SendAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (e.Buffer == null && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			e.curSocket = this;
			SocketOperation op = (e.Buffer != null) ? SocketOperation.Send : SocketOperation.SendGeneric;
			e.Worker.Init (this, e, op);
			SocketAsyncResult res = e.Worker.result;
			if (e.Buffer != null) {
				res.Buffer = e.Buffer;
				res.Offset = e.Offset;
				res.Size = e.Count;
			} else {
				res.Buffers = e.BufferList;
			}
			res.SockFlags = e.SocketFlags;
			int count;
			lock (writeQ) {
				writeQ.Enqueue (e.Worker);
				count = writeQ.Count;
			}
			if (count == 1) {
				// Send takes care of SendGeneric
				socket_pool_queue (SocketAsyncWorker.Dispatcher, res);
			}
			return true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Poll_internal (IntPtr socket, SelectMode mode, int timeout, out int error);

		private static bool Poll_internal (SafeSocketHandle safeHandle, SelectMode mode, int timeout, out int error)
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
		private extern static int Receive_internal(IntPtr sock,
							   byte[] buffer,
							   int offset,
							   int count,
							   SocketFlags flags,
							   out int error);

		private static int Receive_internal (SafeSocketHandle safeHandle,
							   byte[] buffer,
							   int offset,
							   int count,
							   SocketFlags flags,
							   out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), buffer, offset, count, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		internal int Receive_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			int nativeError;
			int ret = Receive_internal (socket, buf, offset, size, flags, out nativeError);
			error = (SocketError) nativeError;
			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				connected = false;
				isbound = false;
			} else {
				connected = true;
			}
			
			return ret;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void GetSocketOption_obj_internal(IntPtr socket,
			SocketOptionLevel level, SocketOptionName name, out object obj_val,
			out int error);

		private static void GetSocketOption_obj_internal (SafeSocketHandle safeHandle,
			SocketOptionLevel level, SocketOptionName name, out object obj_val,
			out int error)
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
		private extern static int Send_internal(IntPtr sock,
							byte[] buf, int offset,
							int count,
							SocketFlags flags,
							out int error);

		private static int Send_internal (SafeSocketHandle safeHandle,
							byte[] buf, int offset,
							int count,
							SocketFlags flags,
							out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Send_internal (safeHandle.DangerousGetHandle (), buf, offset, count, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		internal int Send_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (size == 0) {
				error = SocketError.Success;
				return 0;
			}

			int nativeError;

			int ret = Send_internal (socket, buf, offset, size, flags, out nativeError);

			error = (SocketError)nativeError;

			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				connected = false;
				isbound = false;
			} else {
				connected = true;
			}

			return ret;
		}

		public object GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			object obj_val;
			int error;

			GetSocketOption_obj_internal (socket, optionLevel, optionName, out obj_val,
				out error);
			if (error != 0)
				throw new SocketException (error);

			if (optionName == SocketOptionName.Linger) {
				return((LingerOption)obj_val);
			} else if (optionName == SocketOptionName.AddMembership ||
				   optionName == SocketOptionName.DropMembership) {
				return((MulticastOption)obj_val);
			} else if (obj_val is int) {
				return((int)obj_val);
			} else {
				return(obj_val);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void Shutdown_internal (IntPtr socket, SocketShutdown how, out int error);
		
		private static void Shutdown_internal (SafeSocketHandle safeHandle, SocketShutdown how, out int error)
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

		public void Shutdown (SocketShutdown how)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new SocketException (10057); // Not connected

			int error;
			
			Shutdown_internal (socket, how, out error);
			if (error != 0)
				throw new SocketException (error);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void SetSocketOption_internal (IntPtr socket, SocketOptionLevel level,
								     SocketOptionName name, object obj_val,
								     byte [] byte_val, int int_val,
								     out int error);

		private static void SetSocketOption_internal (SafeSocketHandle safeHandle, SocketOptionLevel level,
								     SocketOptionName name, object obj_val,
								     byte [] byte_val, int int_val,
								     out int error)
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

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;

			SetSocketOption_internal (socket, optionLevel, optionName, null,
						 null, optionValue, out error);

			if (error != 0)
				throw new SocketException (error);
		}

		private void ThrowIfUpd ()
		{
#if !NET_2_1 || MOBILE
			if (protocol_type == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
#endif
		}

		public
		IAsyncResult BeginConnect(EndPoint end_point, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (end_point == null)
				throw new ArgumentNullException ("end_point");

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Connect);
			req.EndPoint = end_point;

			// Bug #75154: Connect() should not succeed for .Any addresses.
			if (end_point is IPEndPoint) {
				IPEndPoint ep = (IPEndPoint) end_point;
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)) {
					req.Complete (new SocketException ((int) SocketError.AddressNotAvailable), true);
					return req;
				}
			}

			int error = 0;
			if (connect_in_progress) {
				// This could happen when multiple IPs are used
				// Calling connect() again will reset the connection attempt and cause
				// an error. Better to just close the socket and move on.
				connect_in_progress = false;
				socket.Dispose ();
				var handle = Socket_internal (address_family, socket_type, protocol_type, out error);
				socket = new SafeSocketHandle (handle, true);
				if (error != 0)
					throw new SocketException (error);
			}
			bool blk = blocking;
			if (blk)
				Blocking = false;
			SocketAddress serial = end_point.Serialize ();
			Connect_internal (socket, serial, out error);
			if (blk)
				Blocking = true;
			if (error == 0) {
				// succeeded synch
				connected = true;
				isbound = true;
				req.Complete (true);
				return req;
			}

			if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
				// error synch
				connected = false;
				isbound = false;
				req.Complete (new SocketException (error), true);
				return req;
			}

			// continue asynch
			connected = false;
			isbound = false;
			connect_in_progress = true;
			socket_pool_queue (SocketAsyncWorker.Dispatcher, req);
			return req;
		}

		public
		IAsyncResult BeginConnect (IPAddress[] addresses, int port, AsyncCallback callback, object state)

		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (addresses == null)
				throw new ArgumentNullException ("addresses");

			if (addresses.Length == 0)
				throw new ArgumentException ("Empty addresses list");

			if (this.AddressFamily != AddressFamily.InterNetwork &&
				this.AddressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");

			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");
			if (islistening)
				throw new InvalidOperationException ();

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Connect);
			req.Addresses = addresses;
			req.Port = port;
			connected = false;
			return BeginMConnect (req);
		}

		internal IAsyncResult BeginMConnect (SocketAsyncResult req)
		{
			IAsyncResult ares = null;
			Exception exc = null;
			for (int i = req.CurrentAddress; i < req.Addresses.Length; i++) {
				IPAddress addr = req.Addresses [i];
				IPEndPoint ep = new IPEndPoint (addr, req.Port);
				try {
					req.CurrentAddress++;
					ares = BeginConnect (ep, null, req);
					if (ares.IsCompleted && ares.CompletedSynchronously) {
						((SocketAsyncResult) ares).CheckIfThrowDelayedException ();
						req.DoMConnectCallback ();
					}
					break;
				} catch (Exception e) {
					exc = e;
					ares = null;
				}
			}

			if (ares == null)
				throw exc;

			return req;
		}

		// Returns false when it is ok to use RemoteEndPoint
		//         true when addresses must be used (and addresses could be null/empty)
		bool GetCheckedIPs (SocketAsyncEventArgs e, out IPAddress [] addresses)
		{
			addresses = null;
			// Connect to the first address that match the host name, like:
			// http://blogs.msdn.com/ncl/archive/2009/07/20/new-ncl-features-in-net-4-0-beta-2.aspx
			// while skipping entries that do not match the address family
			DnsEndPoint dep = (e.RemoteEndPoint as DnsEndPoint);
			if (dep != null) {
				addresses = Dns.GetHostAddresses (dep.Host);
				return true;
			} else {
				e.ConnectByNameError = null;
					return false;
			}
		}

		bool ConnectAsyncReal (SocketAsyncEventArgs e)
		{			
			bool use_remoteep = true;
			IPAddress [] addresses = null;
			use_remoteep = !GetCheckedIPs (e, out addresses);
			e.curSocket = this;
			SocketAsyncWorker w = e.Worker;
			w.Init (this, e, SocketOperation.Connect);
			SocketAsyncResult result = w.result;
			IAsyncResult ares = null;
			try {
				if (use_remoteep) {
					result.EndPoint = e.RemoteEndPoint;
					ares = BeginConnect (e.RemoteEndPoint, SocketAsyncEventArgs.Dispatcher, e);
				}
				else {

					DnsEndPoint dep = (e.RemoteEndPoint as DnsEndPoint);
					result.Addresses = addresses;
					result.Port = dep.Port;

					ares = BeginConnect (addresses, dep.Port, SocketAsyncEventArgs.Dispatcher, e);
				}
				if (ares.IsCompleted && ares.CompletedSynchronously) {
					((SocketAsyncResult) ares).CheckIfThrowDelayedException ();
					return false;
				}
			} catch (Exception exc) {
				result.Complete (exc, true);
				return false;
			}
			return true;
		}

		public bool ConnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (islistening)
				throw new InvalidOperationException ("You may not perform this operation after calling the Listen method.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP");

			return ConnectAsyncReal (e);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int Receive_internal (IntPtr sock, WSABUF[] bufarray, SocketFlags flags, out int error);

		private static int Receive_internal (SafeSocketHandle safeHandle, WSABUF[] bufarray, SocketFlags flags, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), bufarray, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		public
		int Receive (IList<ArraySegment<byte>> buffers)
		{
			int ret;
			SocketError error;
			ret = Receive (buffers, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				throw new SocketException ((int)error);
			}
			return(ret);
		}

		[CLSCompliant (false)]
		public
		int Receive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			int ret;
			SocketError error;
			ret = Receive (buffers, socketFlags, out error);
			if (error != SocketError.Success) {
				throw new SocketException ((int)error);
			}
			return(ret);
		}

		[CLSCompliant (false)]
		public
		int Receive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffers == null ||
			    buffers.Count == 0) {
				throw new ArgumentNullException ("buffers");
			}

			int numsegments = buffers.Count;
			int nativeError;
			int ret;

			/* Only example I can find of sending a byte
			 * array reference directly into an internal
			 * call is in
			 * System.Runtime.Remoting/System.Runtime.Remoting.Channels.Ipc.Win32/NamedPipeSocket.cs,
			 * so taking a lead from that...
			 */
			WSABUF[] bufarray = new WSABUF[numsegments];
			GCHandle[] gch = new GCHandle[numsegments];

			for(int i = 0; i < numsegments; i++) {
				ArraySegment<byte> segment = buffers[i];

				if (segment.Offset < 0 || segment.Count < 0 ||
				    segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray[i].len = segment.Count;
				bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
			}

			try {
				ret = Receive_internal (socket, bufarray,
							socketFlags,
							out nativeError);
			} finally {
				for(int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated) {
						gch[i].Free ();
					}
				}
			}

			errorCode = (SocketError)nativeError;
			return(ret);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int Send_internal (IntPtr sock, WSABUF[] bufarray, SocketFlags flags, out int error);

		private static int Send_internal (SafeSocketHandle safeHandle, WSABUF[] bufarray, SocketFlags flags, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return Send_internal (safeHandle.DangerousGetHandle (), bufarray, flags, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		public
		int Send (IList<ArraySegment<byte>> buffers)
		{
			int ret;
			SocketError error;
			ret = Send (buffers, SocketFlags.None, out error);
			if (error != SocketError.Success) {
				throw new SocketException ((int)error);
			}
			return(ret);
		}

		public
		int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			int ret;
			SocketError error;
			ret = Send (buffers, socketFlags, out error);
			if (error != SocketError.Success) {
				throw new SocketException ((int)error);
			}
			return(ret);
		}

		[CLSCompliant (false)]
		public
		int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffers == null)
				throw new ArgumentNullException ("buffers");
			if (buffers.Count == 0)
				throw new ArgumentException ("Buffer is empty", "buffers");
			int numsegments = buffers.Count;
			int nativeError;
			int ret;

			WSABUF[] bufarray = new WSABUF[numsegments];
			GCHandle[] gch = new GCHandle[numsegments];
			for(int i = 0; i < numsegments; i++) {
				ArraySegment<byte> segment = buffers[i];

				if (segment.Offset < 0 || segment.Count < 0 ||
				    segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray[i].len = segment.Count;
				bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
			}

			try {
				ret = Send_internal (socket, bufarray, socketFlags, out nativeError);
			} finally {
				for(int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated) {
						gch[i].Free ();
					}
				}
			}
			errorCode = (SocketError)nativeError;
			return(ret);
		}

		Exception InvalidAsyncOp (string method)
		{
			return new InvalidOperationException (method + " can only be called once per asynchronous operation");
		}

		public
		int EndReceive (IAsyncResult result)
		{
			SocketError error;
			int bytesReceived = EndReceive (result, out error);
			if (error != SocketError.Success) {
				if (error != SocketError.WouldBlock && error != SocketError.InProgress)
					connected = false;
				throw new SocketException ((int)error);
			}
			return bytesReceived;
		}

		public
		int EndReceive (IAsyncResult asyncResult, out SocketError errorCode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			SocketAsyncResult req = asyncResult as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndReceive");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			errorCode = req.ErrorCode;
			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if (errorCode == SocketError.Success)
				req.CheckIfThrowDelayedException();

			return(req.Total);
		}

		public
		int EndSend (IAsyncResult result)
		{
			SocketError error;
			int bytesSent = EndSend (result, out error);
			if (error != SocketError.Success) {
				if (error != SocketError.WouldBlock && error != SocketError.InProgress)
					connected = false;
				throw new SocketException ((int)error);
			}
			return bytesSent;
		}

		public
		int EndSend (IAsyncResult asyncResult, out SocketError errorCode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			SocketAsyncResult req = asyncResult as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndSend");
			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			errorCode = req.ErrorCode;
			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if (errorCode == SocketError.Success)
				req.CheckIfThrowDelayedException ();

			return(req.Total);
		}

		// Used by Udpclient
		public
		int EndReceiveFrom(IAsyncResult result, ref EndPoint end_point)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (result == null)
				throw new ArgumentNullException ("result");

			if (end_point == null)
				throw new ArgumentNullException ("remote_end");

			SocketAsyncResult req = result as SocketAsyncResult;
			if (req == null)
				throw new ArgumentException ("Invalid IAsyncResult", "result");

			if (Interlocked.CompareExchange (ref req.EndCalled, 1, 0) == 1)
				throw InvalidAsyncOp ("EndReceiveFrom");
			if (!result.IsCompleted)
				result.AsyncWaitHandle.WaitOne();

 			req.CheckIfThrowDelayedException();
			end_point = req.EndPoint;
			return req.Total;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void socket_pool_queue (SocketAsyncCallback d, SocketAsyncResult r);
	}
}

