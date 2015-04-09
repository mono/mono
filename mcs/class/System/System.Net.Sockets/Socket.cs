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
		[StructLayout (LayoutKind.Sequential)]
		struct WSABuffer {
			public int len;
			public IntPtr buf;
		}

		const int SOCKET_CLOSED = 10004;
		const string TIMEOUT_EXC_MSG = "A connection attempt failed because the connected party did not properly respond " +
			"after a period of time, or established connection failed because connected host has failed to respond";

		/* These two fields are looked up by name by the runtime, don't
		 * change their name without also updating the runtime code. */
		static int ipv4_supported = -1;
		static int ipv6_supported = -1;

		bool is_listening;
		bool use_overlapped_io;

		int linger_timeout;

		/* the field "safe_handle" is looked up by name by the runtime */
		SafeSocketHandle safe_handle;
		AddressFamily address_family;
		SocketType socket_type;
		ProtocolType protocol_type;

		/* true if we called Close_internal */
		bool closed;

		internal bool blocking = true;
		internal bool is_bound;
		/* When true, the socket was connected at the time of
		 * the last IO operation */
		internal bool connected;
		internal bool disposed;
		internal bool connect_in_progress;

		internal Queue<SocketAsyncWorker> read_queue = new Queue<SocketAsyncWorker> (2);
		internal Queue<SocketAsyncWorker> write_queue = new Queue<SocketAsyncWorker> (2);

		/* This EndPoint is used when creating new endpoints. Because
		 * there are many types of EndPoints possible,
		 * seed_endpoint.Create(addr) is used for creating new ones.
		 * As such, this value is set on Bind, SentTo, ReceiveFrom,
		 * Connect, etc. */
		internal EndPoint seed_endpoint = null;

		static Socket ()
		{
			// initialize ipv4_supported and ipv6_supported
			if (ipv4_supported == -1) {
				try {
					Socket tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					tmp.Close();

					ipv4_supported = 1;
				} catch {
					ipv4_supported = 0;
				}
			}

			if (ipv6_supported == -1) {
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
						ipv6_supported = config.Ipv6.Enabled ? -1 : 0;
				} catch {
					ipv6_supported = -1;
				}
#else
				try {
					NetConfig config = System.Configuration.ConfigurationSettings.GetConfig("system.net/settings") as NetConfig;
					if (config != null)
						ipv6_supported = config.ipv6Enabled ? -1 : 0;
				} catch {
					ipv6_supported = -1;
				}
#endif
#endif
				if (ipv6_supported != 0) {
					try {
						Socket tmp = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
						tmp.Close();

						ipv6_supported = 1;
					} catch {
						ipv6_supported = 0;
					}
				}
			}
		}

#if !MOBILE
		public Socket (SocketInformation socketInformation)
		{
			var options = socketInformation.Options;
			is_listening = (options & SocketInformationOptions.Listening) != 0;
			connected = (options & SocketInformationOptions.Connected) != 0;
			blocking = (options & SocketInformationOptions.NonBlocking) == 0;
			use_overlapped_io = (options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;

			var result = Mono.DataConverter.Unpack ("iiiil", socketInformation.ProtocolInformation, 0);
			address_family = (AddressFamily) (int) result [0];
			socket_type = (SocketType) (int) result [1];
			protocol_type = (ProtocolType) (int) result [2];
			is_bound = (ProtocolType) (int) result [3] != 0;
			safe_handle = new SafeSocketHandle ((IntPtr) (long) result [4], true);

			SocketDefaults ();
		}
#endif

		public Socket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
		{
#if NET_2_1 && !MOBILE
			switch (addressFamily) {
			case AddressFamily.InterNetwork:    // ok
			case AddressFamily.InterNetworkV6:  // ok
			case AddressFamily.Unknown:         // SocketException will be thrown later (with right error #)
				break;
			// case AddressFamily.Unspecified:
			default:
				throw new ArgumentException ("addressFamily");
			}

			switch (socketType) {
			case SocketType.Stream:             // ok
			case SocketType.Unknown:            // SocketException will be thrown later (with right error #)
				break;
			default:
				throw new ArgumentException ("socketType");
			}

			switch (protocolType) {
			case ProtocolType.Tcp:              // ok
			case ProtocolType.Unspecified:      // ok
			case ProtocolType.Unknown:          // SocketException will be thrown later (with right error #)
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

			safe_handle = new SafeSocketHandle (handle, true);

			if (error != 0)
				throw new SocketException (error);

#if !NET_2_1 || MOBILE
			SocketDefaults ();
#endif
		}

		// private constructor used by Accept, which already
		// has a socket handle to use
		internal Socket(AddressFamily family, SocketType type, ProtocolType proto, SafeSocketHandle handle)
		{
			address_family = family;
			socket_type = type;
			protocol_type = proto;
			safe_handle = handle;
			connected = true;
		}

		[MonoTODO ("Currently hardcoded to IPv4. Ideally, support v4/v6 dual-stack.")]
		public Socket (SocketType socketType, ProtocolType protocolType)
			: this (AddressFamily.InterNetwork, socketType, protocolType)
		{
		}

		void SocketDefaults ()
		{
			try {
				if (address_family == AddressFamily.InterNetwork
					/* Need to test IPv6 further || address_family == AddressFamily.InterNetworkV6 */
				) {
					/* This is the default, but it probably has nasty side
					 * effects on Linux, as the socket option is kludged by
					 * turning on or off PMTU discovery... */
					DontFragment = false;
				}

				// Microsoft sets these to 8192, but we are going to keep them
				// both to the OS defaults as these have a big performance impact.
				// on WebClient performance.
				//
				// this.ReceiveBufferSize = 8192;
				// this.SendBufferSize = 8192;
			} catch (SocketException) {
			}
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern IntPtr Socket_internal(AddressFamily family, SocketType type, ProtocolType proto, out int error);

		~Socket ()
		{
			Dispose (false);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			bool was_connected = connected;

			disposed = true;
			connected = false;

			if (safe_handle != null) {
				closed = true;

				IntPtr handle = Handle;
				if (was_connected)
					Linger (handle);

				safe_handle.Dispose ();
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
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

#region Properties

		public int Available {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int ret, error;
				ret = Available_internal(safe_handle, out error);

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

		// Returns the amount of data waiting to be read on socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int Available_internal(IntPtr socket, out int error);

		public bool DontFragment {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				bool dontfragment;

				if (address_family == AddressFamily.InterNetwork)
					dontfragment = (int)(GetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment)) != 0;
				else if (address_family == AddressFamily.InterNetworkV6)
					dontfragment = (int)(GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment)) != 0;
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");

				return dontfragment;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				if (address_family == AddressFamily.InterNetwork)
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment, value ? 1 : 0);
				else if (address_family == AddressFamily.InterNetworkV6)
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment, value ? 1 : 0);
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
			}
		}

		public bool EnableBroadcast {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (protocol_type != ProtocolType.Udp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				return ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast)) != 0;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (protocol_type != ProtocolType.Udp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
			}
		}

		public bool ExclusiveAddressUse {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse)) != 0;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (is_bound)
					throw new InvalidOperationException ("Bind has already been called for this socket");

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, value ? 1 : 0);
			}
		}

		public bool IsBound {
			get {
				return is_bound;
			}
		}

		public LingerOption LingerState {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (LingerOption) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger, value);
			}
		}

		public bool MulticastLoopback {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

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
				if (protocol_type == ProtocolType.Tcp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				bool multicastloopback;

				if (address_family == AddressFamily.InterNetwork)
					multicastloopback = ((int) GetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback)) != 0;
				else if (address_family == AddressFamily.InterNetworkV6)
					multicastloopback = ((int) GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback)) != 0;
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");

				return multicastloopback;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

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
				if (protocol_type == ProtocolType.Tcp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				if (address_family == AddressFamily.InterNetwork)
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, value ? 1 : 0);
				else if (address_family == AddressFamily.InterNetworkV6)
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.MulticastLoopback, value ? 1 : 0);
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
			}
		}


		[MonoTODO ("This doesn't do anything on Mono yet")]
		public bool UseOnlyOverlappedIO {
			get {
				return use_overlapped_io;
			}
			set {
				use_overlapped_io = value;
			}
		}

		public IntPtr Handle {
			get {
				return safe_handle.DangerousGetHandle ();
			}
		}

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

				sa = LocalEndPoint_internal (safe_handle, (int) address_family, out error);

				if (error != 0)
					throw new SocketException (error);

				return seed_endpoint.Create (sa);
			}
		}

		static SocketAddress LocalEndPoint_internal(SafeSocketHandle safeHandle, int family, out int error)
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

		// Returns the local endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static SocketAddress LocalEndPoint_internal(IntPtr socket, int family, out int error);

		public SocketType SocketType {
			get {
				return socket_type;
			}
		}

		public int SendTimeout {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (int) GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
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

				SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
			}
		}

		public int ReceiveTimeout {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (int) GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than -1");
				if (value == -1)
					value = 0;

				SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
			}
		}

		public static bool SupportsIPv4 {
			get {
				return ipv4_supported == 1;
			}
		}

		[ObsoleteAttribute ("Use OSSupportsIPv6 instead")]
		public static bool SupportsIPv6 {
			get {
				return ipv6_supported == 1;
			}
		}

#if NET_2_1
		public static bool OSSupportsIPv4 {
			get {
				return ipv4_supported == 1;
			}
		}
#endif

#if NET_2_1
		public static bool OSSupportsIPv6 {
			get {
				return ipv6_supported == 1;
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

		public AddressFamily AddressFamily {
			get { return address_family; }
		}

		public bool Blocking {
			get {
				return blocking;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				int error;
				Blocking_internal (safe_handle, value, out error);

				if (error != 0)
					throw new SocketException (error);

				blocking=value;
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
			get { return connected; }
		}

		public ProtocolType ProtocolType {
			get { return protocol_type; }
		}

		public bool NoDelay {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				ThrowIfUdp ();

				return ((int) GetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay)) != 0;
			}

			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				ThrowIfUdp ();

				SetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.NoDelay, value ? 1 : 0);
			}
		}

		void ThrowIfUdp ()
		{
#if !NET_2_1 || MOBILE
			if (protocol_type == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
#endif
		}

		public int ReceiveBufferSize {
			get {
				if (disposed && closed) {
					throw new ObjectDisposedException (GetType ().ToString ());
				}

				return (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
			}
		}

		public int SendBufferSize {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				return (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
			}
		}

		public short Ttl {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());

				short ttl_val;

				if (address_family == AddressFamily.InterNetwork)
					ttl_val = (short) (int) GetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive);
				else if (address_family == AddressFamily.InterNetworkV6)
					ttl_val = (short) (int) GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit);
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");

				return ttl_val;
			}
			set {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");

				if (address_family == AddressFamily.InterNetwork)
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, value);
				else if (address_family == AddressFamily.InterNetworkV6)
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit, value);
				else
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
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

				sa = RemoteEndPoint_internal(safe_handle, (int) address_family, out error);

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

		// Returns the remote endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static SocketAddress RemoteEndPoint_internal(IntPtr socket, int family, out int error);

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

			int error;
			/*
			 * The 'sockets' array contains:
			 *  - READ socket 0-n, null,
			 *  - WRITE socket 0-n, null,
			 *  - ERROR socket 0-n, null
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

#region Accept

		public Socket Accept() {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;
			var sock = Accept_internal(safe_handle, out error, blocking);

			if (error != 0) {
				if (closed)
					error = SOCKET_CLOSED;
				throw new SocketException(error);
			}

			Socket accepted = new Socket(this.AddressFamily, this.SocketType, this.ProtocolType, sock) {
				seed_endpoint = this.seed_endpoint,
				Blocking = this.Blocking,
			};

			return accepted ;
		}

		internal void Accept (Socket acceptSocket)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;
			var safe_handle = Accept_internal (this.safe_handle, out error, blocking);

			if (error != 0) {
				if (closed)
					error = SOCKET_CLOSED;
				throw new SocketException (error);
			}

			acceptSocket.address_family = this.AddressFamily;
			acceptSocket.socket_type = this.SocketType;
			acceptSocket.protocol_type = this.ProtocolType;
			acceptSocket.safe_handle = safe_handle;
			acceptSocket.connected = true;
			acceptSocket.seed_endpoint = this.seed_endpoint;
			acceptSocket.Blocking = this.Blocking;

			// FIXME: figure out what if anything else needs to be reset
		}

		public bool AcceptAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
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
				if (acceptSocket.is_bound || acceptSocket.connected)
					throw new InvalidOperationException ("AcceptSocket: The socket must not be bound or connected.");
			}

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.Accept);

			SocketAsyncResult sockares = e.Worker.result;

			QueueSocketAsyncResult (read_queue, e.Worker, sockares);

			return true;
		}

		public IAsyncResult BeginAccept(AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (!is_bound || !is_listening)
				throw new InvalidOperationException ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Accept);

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		public IAsyncResult BeginAccept (int receiveSize, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.AcceptReceive) {
				Buffer = new byte[receiveSize],
				Offset = 0,
				Size = receiveSize,
				SockFlags = SocketFlags.None,
			};

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		public IAsyncResult BeginAccept (Socket acceptSocket, int receiveSize, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");
			if (acceptSocket != null) {
				if (acceptSocket.disposed && acceptSocket.closed)
					throw new ObjectDisposedException (acceptSocket.GetType ().ToString ());
				if (acceptSocket.is_bound)
					throw new InvalidOperationException ();
				/* For some reason the MS runtime barfs if the new socket is not TCP,
				 * even though it's just about to blow away all those parameters */
				if (acceptSocket.ProtocolType != ProtocolType.Tcp)
					throw new SocketException ((int) SocketError.InvalidArgument);
			}

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.AcceptReceive) {
				Buffer = new byte[receiveSize],
				Offset = 0,
				Size = receiveSize,
				SockFlags = SocketFlags.None,
				AcceptSocket = acceptSocket,
			};

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		public Socket EndAccept (IAsyncResult result)
		{
			int bytes;
			byte[] buffer;

			return EndAccept (out buffer, out bytes, result);
		}

		public Socket EndAccept (out byte[] buffer, IAsyncResult asyncResult)
		{
			int bytes;
			return EndAccept (out buffer, out bytes, asyncResult);
		}

		public Socket EndAccept (out byte[] buffer, out int bytesTransferred, IAsyncResult asyncResult)
		{
			var sockares = ValidateEndIAsyncResult (asyncResult, "EndAccept", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();

			buffer = sockares.Buffer;
			bytesTransferred = sockares.Total;

			return sockares.Socket;
		}

		static SafeSocketHandle Accept_internal(SafeSocketHandle safeHandle, out int error, bool blocking)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				var handle = Accept_internal (safeHandle.DangerousGetHandle (), out error, blocking);
				return new SafeSocketHandle (handle, true);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static IntPtr Accept_internal(IntPtr sock, out int error, bool blocking);

#endregion

#region Connect

		public void Connect (EndPoint remoteEP)
		{
			SocketAddress serial = null;

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			IPEndPoint ep = remoteEP as IPEndPoint;
			if (ep != null && socket_type != SocketType.Dgram) { /* Dgram uses Any to 'disconnect' */
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any))
					throw new SocketException ((int) SocketError.AddressNotAvailable);
			}

			if (is_listening)
				throw new InvalidOperationException ();
			serial = remoteEP.Serialize ();

			int error = 0;
			Connect_internal (safe_handle, serial, out error);

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

			is_bound = true;
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
			if (this.AddressFamily != AddressFamily.InterNetwork && this.AddressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");
			if (is_listening)
				throw new InvalidOperationException ();

			/* FIXME: do non-blocking sockets Poll here? */
			int error = 0;
			foreach (IPAddress address in addresses) {
				IPEndPoint iep = new IPEndPoint (address, port);
				SocketAddress serial = iep.Serialize ();

				Connect_internal (safe_handle, serial, out error);
				if (error == 0) {
					connected = true;
					is_bound = true;
					seed_endpoint = iep;
					return;
				} else if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
					continue;
				}

				if (!blocking) {
					Poll (-1, SelectMode.SelectWrite);
					error = (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
					if (error == 0) {
						connected = true;
						is_bound = true;
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
			Connect (Dns.GetHostAddresses (host), port);
		}

		public bool ConnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (is_listening)
				throw new InvalidOperationException ("You may not perform this operation after calling the Listen method.");
			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP");

			IPAddress [] addresses;
			bool use_remote_end_point = !GetCheckedIPs (e, out addresses);

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.Connect);

			SocketAsyncResult sockares = e.Worker.result;

			try {
				IAsyncResult ares = null;

				if (use_remote_end_point) {
					sockares.EndPoint = e.RemoteEndPoint;

					ares = BeginConnect (e.RemoteEndPoint, SocketAsyncEventArgs.Dispatcher, e);
				} else {
					DnsEndPoint dep = (DnsEndPoint) e.RemoteEndPoint;
					sockares.Addresses = addresses;
					sockares.Port = dep.Port;

					ares = BeginConnect (addresses, dep.Port, SocketAsyncEventArgs.Dispatcher, e);
				}

				if (ares.IsCompleted && ares.CompletedSynchronously) {
					((SocketAsyncResult) ares).CheckIfThrowDelayedException ();
					return false;
				}
			} catch (Exception exc) {
				sockares.Complete (exc, true);
				return false;
			}

			return true;
		}

		public IAsyncResult BeginConnect (IPAddress address, int port, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (address == null)
				throw new ArgumentNullException ("address");
			if (address.ToString ().Length == 0)
				throw new ArgumentException ("The length of the IP address is zero");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");
			if (is_listening)
				throw new InvalidOperationException ();

			return BeginConnect (new IPEndPoint (address, port), callback, state);
		}

		public IAsyncResult BeginConnect (string host, int port, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (host == null)
				throw new ArgumentNullException ("host");
			if (address_family != AddressFamily.InterNetwork && address_family != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is valid only for sockets in the InterNetwork and InterNetworkV6 families");
			if (port <= 0 || port > 65535)
				throw new ArgumentOutOfRangeException ("port", "Must be > 0 and < 65536");
			if (is_listening)
				throw new InvalidOperationException ();

			return BeginConnect (Dns.GetHostAddresses (host), port, callback, state);
		}

		public IAsyncResult BeginConnect(EndPoint end_point, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (end_point == null)
				throw new ArgumentNullException ("end_point");

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Connect) {
				EndPoint = end_point,
			};

			// Bug #75154: Connect() should not succeed for .Any addresses.
			IPEndPoint ip_end_point = end_point as IPEndPoint;
			if (ip_end_point != null && (ip_end_point.Address.Equals (IPAddress.Any) || ip_end_point.Address.Equals (IPAddress.IPv6Any))) {
				sockares.Complete (new SocketException ((int) SocketError.AddressNotAvailable), true);
				return sockares;
			}

			int error = 0;

			if (connect_in_progress) {
				// This could happen when multiple IPs are used
				// Calling connect() again will reset the connection attempt and cause
				// an error. Better to just close the socket and move on.
				connect_in_progress = false;

				safe_handle.Dispose ();
				safe_handle = new SafeSocketHandle (Socket_internal (address_family, socket_type, protocol_type, out error), true);

				if (error != 0)
					throw new SocketException (error);
			}

			if (!blocking) {
				Connect_internal (safe_handle, end_point.Serialize (), out error);
			} else {
				Blocking = false;
				Connect_internal (safe_handle, end_point.Serialize (), out error);
				Blocking = true;
			}

			if (error == 0) {
				// succeeded synch
				connected = true;
				is_bound = true;
				sockares.Complete (true);
				return sockares;
			}

			if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
				// error synch
				connected = false;
				is_bound = false;
				sockares.Complete (new SocketException (error), true);
				return sockares;
			}

			// continue asynch
			connected = false;
			is_bound = false;
			connect_in_progress = true;

			QueueIOWorkItem (SocketAsyncWorker.Dispatcher, sockares);

			return sockares;
		}

		public IAsyncResult BeginConnect (IPAddress[] addresses, int port, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
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

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Connect) {
				Addresses = addresses,
				Port = port,
			};

			connected = false;

			return BeginMConnect (sockares);
		}

		internal IAsyncResult BeginMConnect (SocketAsyncResult sockares)
		{
			IAsyncResult ares = null;
			Exception exc = null;

			for (int i = sockares.CurrentAddress; i < sockares.Addresses.Length; i++) {
				IPAddress addr = sockares.Addresses [i];
				IPEndPoint ep = new IPEndPoint (addr, sockares.Port);

				try {
					sockares.CurrentAddress++;
					ares = BeginConnect (ep, null, sockares);
					if (ares.IsCompleted && ares.CompletedSynchronously) {
						((SocketAsyncResult) ares).CheckIfThrowDelayedException ();
						sockares.DoMConnectCallback ();
					}
					break;
				} catch (Exception e) {
					exc = e;
					ares = null;
				}
			}

			if (ares == null)
				throw exc;

			return sockares;
		}

		public void EndConnect (IAsyncResult result)
		{
			var sockares = ValidateEndIAsyncResult (result, "EndConnect", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();
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

		static void Connect_internal (SafeSocketHandle safeHandle, SocketAddress sa, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				Connect_internal (safeHandle.DangerousGetHandle (), sa, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Connect_internal(IntPtr sock, SocketAddress sa, out int error);

#endregion

#region Disconnect

		/* According to the docs, the MS runtime will throw
		 * PlatformNotSupportedException if the platform is
		 * newer than w2k.  We should be able to cope...
		 */
		public void Disconnect (bool reuseSocket)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error = 0;

			Disconnect_internal (safe_handle, reuseSocket, out error);

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

		public bool DisconnectAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			e.curSocket = this;
			e.Worker.Init (this, e, SocketOperation.Disconnect);

			SocketAsyncResult sockares = e.Worker.result;

			QueueIOWorkItem (SocketAsyncWorker.Dispatcher, sockares);

			return true;
		}

		public IAsyncResult BeginDisconnect (bool reuseSocket, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Disconnect) {
				ReuseSocket = reuseSocket,
			};

			QueueIOWorkItem (SocketAsyncWorker.Dispatcher, sockares);

			return sockares;
		}

		public void EndDisconnect (IAsyncResult asyncResult)
		{
			var sockares = ValidateEndIAsyncResult (asyncResult, "EndDisconnect", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();
		}

		static void Disconnect_internal(SafeSocketHandle safeHandle, bool reuse, out int error)
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
		extern static void Disconnect_internal(IntPtr sock, bool reuse, out int error);

#endregion

#region Receive

		public int Receive (byte [] buffer)
		{
			return Receive (buffer, SocketFlags.None);
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
					throw new SocketException ((int) error, TIMEOUT_EXC_MSG);
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
					throw new SocketException ((int) error, TIMEOUT_EXC_MSG);
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
					throw new SocketException ((int) error, TIMEOUT_EXC_MSG);
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

		public int Receive (IList<ArraySegment<byte>> buffers)
		{
			SocketError error;
			int ret = Receive (buffers, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		[CLSCompliant (false)]
		public int Receive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			SocketError error;
			int ret = Receive (buffers, socketFlags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		[CLSCompliant (false)]
		public int Receive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffers == null || buffers.Count == 0)
				throw new ArgumentNullException ("buffers");

			int numsegments = buffers.Count;
			int nativeError;
			int ret;

			/* Only example I can find of sending a byte array reference directly into an internal call is in
			 * System.Runtime.Remoting/System.Runtime.Remoting.Channels.Ipc.Win32/NamedPipeSocket.cs, so taking
			 * a lead from that... */
			WSABuffer[] bufarray = new WSABuffer [numsegments];
			GCHandle[] gch = new GCHandle [numsegments];

			for(int i = 0; i < numsegments; i++) {
				ArraySegment<byte> segment = buffers [i];

				if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch [i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray [i].len = segment.Count;
				bufarray [i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
			}

			try {
				ret = Receive_internal (safe_handle, bufarray, socketFlags, out nativeError);
			} finally {
				for(int i = 0; i < numsegments; i++) {
					if (gch [i].IsAllocated)
						gch [i].Free ();
				}
			}

			errorCode = (SocketError) nativeError;

			return ret;
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
			e.Worker.Init (this, e, e.Buffer != null ? SocketOperation.Receive : SocketOperation.ReceiveGeneric);

			SocketAsyncResult sockares = e.Worker.result;
			sockares.SockFlags = e.SocketFlags;

			if (e.Buffer == null) {
				sockares.Buffers = e.BufferList;
			} else {
				sockares.Buffer = e.Buffer;
				sockares.Offset = e.Offset;
				sockares.Size = e.Count;
			}

			QueueSocketAsyncResult (read_queue, e.Worker, sockares);

			return true;
		}

		public IAsyncResult BeginReceive(byte[] buffer, int offset, int size, SocketFlags socket_flags, AsyncCallback callback, object state) {

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Receive) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
			};

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		public IAsyncResult BeginReceive (byte[] buffer, int offset, int size, SocketFlags flags, out SocketError error, AsyncCallback callback, object state)
		{
			/* As far as I can tell from the docs and from
			 * experimentation, a pointer to the
			 * SocketError parameter is not supposed to be
			 * saved for the async parts.  And as we don't
			 * set any socket errors in the setup code, we
			 * just have to set it to Success.
			 */
			error = SocketError.Success;
			return BeginReceive (buffer, offset, size, flags, callback, state);
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.ReceiveGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			/* I assume the same SocketError semantics as
			 * above
			 */
			errorCode = SocketError.Success;
			return BeginReceive (buffers, socketFlags, callback, state);
		}

		public int EndReceive (IAsyncResult result)
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

		public int EndReceive (IAsyncResult asyncResult, out SocketError errorCode)
		{
			var sockares = ValidateEndIAsyncResult (asyncResult, "EndReceive", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if ((errorCode = sockares.ErrorCode) == SocketError.Success)
				sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		internal int Receive_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			int nativeError;
			int ret = Receive_internal (safe_handle, buf, offset, size, flags, out nativeError);

			error = (SocketError) nativeError;
			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				connected = false;
				is_bound = false;
			} else {
				connected = true;
			}

			return ret;
		}

		static int Receive_internal (SafeSocketHandle safeHandle, WSABuffer[] bufarray, SocketFlags flags, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), bufarray, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int Receive_internal (IntPtr sock, WSABuffer[] bufarray, SocketFlags flags, out int error);

		static int Receive_internal (SafeSocketHandle safeHandle, byte[] buffer, int offset, int count, SocketFlags flags, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), buffer, offset, count, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int Receive_internal(IntPtr sock, byte[] buffer, int offset, int count, SocketFlags flags, out int error);

#endregion

#region ReceiveFrom

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

		public int ReceiveFrom (byte [] buffer, int size, SocketFlags flags, ref EndPoint remoteEP)
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

		public int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags flags, ref EndPoint remoteEP)
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

		internal int ReceiveFrom_nochecks (byte [] buf, int offset, int size, SocketFlags flags, ref EndPoint remote_end)
		{
			int error;
			return ReceiveFrom_nochecks_exc (buf, offset, size, flags, ref remote_end, true, out error);
		}

		internal int ReceiveFrom_nochecks_exc (byte [] buf, int offset, int size, SocketFlags flags, ref EndPoint remote_end, bool throwOnError, out int error)
		{
			SocketAddress sockaddr = remote_end.Serialize();
			int ret = RecvFrom_internal (safe_handle, buf, offset, size, flags, ref sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress) {
					connected = false;
				} else if (err == SocketError.WouldBlock && blocking) {
					// This might happen when ReceiveTimeout is set
					if (throwOnError)
						throw new SocketException ((int) SocketError.TimedOut, TIMEOUT_EXC_MSG);
					error = (int) SocketError.TimedOut;
					return 0;
				}

				if (throwOnError)
					throw new SocketException (error);

				return 0;
			}

			connected = true;
			is_bound = true;

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

			return ret;
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

			SocketAsyncResult sockares = e.Worker.result;
			sockares.Buffer = e.Buffer;
			sockares.Offset = e.Offset;
			sockares.Size = e.Count;
			sockares.EndPoint = e.RemoteEndPoint;
			sockares.SockFlags = e.SocketFlags;

			QueueSocketAsyncResult (read_queue, e.Worker, sockares);

			return true;
		}

		public IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socket_flags, ref EndPoint remote_end, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			CheckRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.ReceiveFrom) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
				EndPoint = remote_end,
			};

			QueueSocketAsyncResult (read_queue, sockares.Worker, sockares);

			return sockares;
		}

		// Used by Udpclient
		public int EndReceiveFrom(IAsyncResult result, ref EndPoint end_point)
		{
			var sockares = ValidateEndIAsyncResult (result, "EndReceiveFrom", "result");

			if (end_point == null)
				throw new ArgumentNullException ("remote_end");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			end_point = sockares.EndPoint;

			return sockares.Total;
		}

		static int RecvFrom_internal (SafeSocketHandle safeHandle, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return RecvFrom_internal (safeHandle.DangerousGetHandle (), buffer, offset, count, flags, ref sockaddr, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int RecvFrom_internal(IntPtr sock, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error);

#endregion

#region ReceiveMessageFrom

		[MonoTODO ("Not implemented")]
		public int ReceiveMessageFrom (byte[] buffer, int offset, int size, ref SocketFlags socketFlags, ref EndPoint remoteEP, out IPPacketInformation ipPacketInformation)
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
		public bool ReceiveMessageFromAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public IAsyncResult BeginReceiveMessageFrom (byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEP, AsyncCallback callback, object state)
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

		[MonoTODO ("Not implemented")]
		public int EndReceiveMessageFrom (IAsyncResult asyncResult, ref SocketFlags socketFlags, ref EndPoint endPoint, out IPPacketInformation ipPacketInformation)
		{
			var sockares = ValidateEndIAsyncResult (asyncResult, "EndReceiveMessageFrom", "asyncResult");

			if (endPoint == null)
				throw new ArgumentNullException ("endPoint");

			throw new NotImplementedException ();
		}

#endregion

#region Send

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

		public int Send (IList<ArraySegment<byte>> buffers)
		{
			SocketError error;
			int ret = Send (buffers, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
		{
			SocketError error;
			int ret = Send (buffers, socketFlags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		[CLSCompliant (false)]
		public int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode)
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

			WSABuffer[] bufarray = new WSABuffer [numsegments];
			GCHandle[] gch = new GCHandle [numsegments];

			for(int i = 0; i < numsegments; i++) {
				ArraySegment<byte> segment = buffers[i];

				if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch [i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray [i].len = segment.Count;
				bufarray [i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
			}

			try {
				ret = Send_internal (safe_handle, bufarray, socketFlags, out nativeError);
			} finally {
				for(int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated) {
						gch[i].Free ();
					}
				}
			}

			errorCode = (SocketError) nativeError;

			return ret;
		}

		public bool SendAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (e.Buffer == null && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			e.curSocket = this;
			e.Worker.Init (this, e, e.Buffer != null ? SocketOperation.Send : SocketOperation.SendGeneric);

			SocketAsyncResult sockares = e.Worker.result;
			sockares.SockFlags = e.SocketFlags;

			if (e.Buffer == null) {
				sockares.Buffers = e.BufferList;
			} else {
				sockares.Buffer = e.Buffer;
				sockares.Offset = e.Offset;
				sockares.Size = e.Count;
			}

			QueueSocketAsyncResult (write_queue, e.Worker, sockares);

			return true;
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socket_flags, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (!connected)
				throw new SocketException ((int) SocketError.NotConnected);

			CheckRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.Send) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
			};

			QueueSocketAsyncResult (write_queue, sockares.Worker, sockares);

			return sockares;
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			if (!connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int)errorCode);
			}

			errorCode = SocketError.Success;

			return BeginSend (buffer, offset, size, socketFlags, callback, state);
		}

		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (buffers == null)
				throw new ArgumentNullException ("buffers");
			if (!connected)
				throw new SocketException ((int) SocketError.NotConnected);

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.SendGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueSocketAsyncResult (write_queue, sockares.Worker, sockares);

			return sockares;
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			if (!connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int)errorCode);
			}

			errorCode = SocketError.Success;
			return BeginSend (buffers, socketFlags, callback, state);
		}

		public int EndSend (IAsyncResult asyncResult, out SocketError errorCode)
		{
			var sockares = ValidateEndIAsyncResult (asyncResult, "EndSend", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if ((errorCode = sockares.ErrorCode) == SocketError.Success)
				sockares.CheckIfThrowDelayedException ();

			return sockares.Total;
		}

		public int EndSend (IAsyncResult result)
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

		internal int Send_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (size == 0) {
				error = SocketError.Success;
				return 0;
			}

			int nativeError;

			int ret = Send_internal (safe_handle, buf, offset, size, flags, out nativeError);

			error = (SocketError)nativeError;

			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				connected = false;
				is_bound = false;
			} else {
				connected = true;
			}

			return ret;
		}

		static int Send_internal (SafeSocketHandle safeHandle, WSABuffer[] bufarray, SocketFlags flags, out int error)
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int Send_internal (IntPtr sock, WSABuffer[] bufarray, SocketFlags flags, out int error);

		static int Send_internal (SafeSocketHandle safeHandle, byte[] buf, int offset, int count, SocketFlags flags, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Send_internal (safeHandle.DangerousGetHandle (), buf, offset, count, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int Send_internal(IntPtr sock, byte[] buf, int offset, int count, SocketFlags flags, out int error);

#endregion

#region SendFile

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

			if (!SendFile (safe_handle, fileName, preBuffer, postBuffer, flags)) {
				SocketException exc = new SocketException ();
				if (exc.ErrorCode == 2 || exc.ErrorCode == 3)
					throw new FileNotFoundException ();
				throw exc;
			}
		}

		public IAsyncResult BeginSendFile (string fileName, AsyncCallback callback, object state)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!connected)
				throw new NotSupportedException ();

			if (!File.Exists (fileName))
				throw new FileNotFoundException ();

			return BeginSendFile (fileName, null, null, 0, callback, state);
		}

		public IAsyncResult BeginSendFile (string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags, AsyncCallback callback, object state)
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

		static bool SendFile (SafeSocketHandle safeHandle, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return SendFile (safeHandle.DangerousGetHandle (), filename, pre_buffer, post_buffer, flags);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool SendFile (IntPtr sock, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags);

#endregion

#region SendTo

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

		public int SendTo (byte [] buffer, int offset, int size, SocketFlags flags, EndPoint remote_end)
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

		internal int SendTo_nochecks (byte [] buffer, int offset, int size, SocketFlags flags, EndPoint remote_end)
		{
			SocketAddress sockaddr = remote_end.Serialize ();

			int ret, error;

			ret = SendTo_internal (safe_handle, buffer, offset, size, flags, sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					connected = false;

				throw new SocketException (error);
			}

			connected = true;
			is_bound = true;
			seed_endpoint = remote_end;

			return ret;
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

			SocketAsyncResult sockares = e.Worker.result;
			sockares.Buffer = e.Buffer;
			sockares.Offset = e.Offset;
			sockares.Size = e.Count;
			sockares.SockFlags = e.SocketFlags;
			sockares.EndPoint = e.RemoteEndPoint;

			QueueSocketAsyncResult (write_queue, e.Worker, sockares);

			return true;
		}

		public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socket_flags, EndPoint remote_end, AsyncCallback callback, object state) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			CheckRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, state, callback, SocketOperation.SendTo) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
				EndPoint = remote_end,
			};

			QueueSocketAsyncResult (write_queue, sockares.Worker, sockares);

			return sockares;
		}

		public int EndSendTo (IAsyncResult result)
		{
			var sockares = ValidateEndIAsyncResult (result, "EndSendTo", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		static int SendTo_internal (SafeSocketHandle safeHandle, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress sa, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return SendTo_internal (safeHandle.DangerousGetHandle (), buffer, offset, count, flags, sa, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int SendTo_internal(IntPtr sock, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress sa, out int error);

#endregion

#region Bind

		public void Bind(EndPoint local_end) {
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (local_end == null)
				throw new ArgumentNullException("local_end");

			int error;
			Bind_internal(safe_handle, local_end.Serialize(), out error);

			if (error != 0)
				throw new SocketException (error);
			if (error == 0)
				is_bound = true;

			seed_endpoint = local_end;
		}

		static void Bind_internal (SafeSocketHandle safeHandle, SocketAddress sa, out int error)
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
		extern static void Bind_internal(IntPtr sock, SocketAddress sa, out int error);

#endregion

#region DuplicateAndClose

#if !MOBILE
		[MonoLimitation ("We do not support passing sockets across processes, we merely allow this API to pass the socket across AppDomains")]
		public SocketInformation DuplicateAndClose (int targetProcessId)
		{
			var si = new SocketInformation ();
			si.Options =
				(is_listening      ? SocketInformationOptions.Listening           : 0) |
				(connected         ? SocketInformationOptions.Connected           : 0) |
				(!blocking         ? SocketInformationOptions.NonBlocking         : 0) |
				(use_overlapped_io ? SocketInformationOptions.UseOnlyOverlappedIO : 0);

			si.ProtocolInformation = Mono.DataConverter.Pack ("iiiil", (int) address_family, (int) socket_type, (int) protocol_type, is_bound ? 1 : 0, (long) Handle);
			safe_handle = null;

			return si;
		}
#endif

#endregion

#region GetSocketOption

		public void GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault, "Error trying to dereference an invalid pointer");

			int error;
			GetSocketOption_arr_internal (safe_handle, optionLevel, optionName, ref optionValue, out error);
			if (error != 0)
				throw new SocketException (error);
		}

		public byte [] GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int length)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			byte[] byte_val = new byte [length];

			int error;
			GetSocketOption_arr_internal (safe_handle, optionLevel, optionName, ref byte_val, out error);
			if (error != 0)
				throw new SocketException (error);

			return byte_val;
		}

		public object GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			object obj_val;

			int error;
			GetSocketOption_obj_internal (safe_handle, optionLevel, optionName, out obj_val, out error);
			if (error != 0)
				throw new SocketException (error);

			switch (optionName) {
			case SocketOptionName.Linger:
				return (LingerOption) obj_val;
			case SocketOptionName.AddMembership:
			case SocketOptionName.DropMembership:
				return (MulticastOption) obj_val;
			default:
				return (obj_val is int) ? (int) obj_val : obj_val;
			}
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

#region IOControl

		public int IOControl (int ioctl_code, byte [] in_value, byte [] out_value)
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int result = WSAIoctl (safe_handle, ioctl_code, in_value, out_value, out error);

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

		static int WSAIoctl (SafeSocketHandle safeHandle, int ioctl_code, byte [] input, byte [] output, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				return WSAIoctl (safeHandle.DangerousGetHandle (), ioctl_code, input, output, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		// See Socket.IOControl, WSAIoctl documentation in MSDN. The
		// common options between UNIX and Winsock are FIONREAD,
		// FIONBIO and SIOCATMARK. Anything else will depend on the
		// system except SIO_KEEPALIVE_VALS which is properly handled
		// on both windows and linux.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int WSAIoctl (IntPtr sock, int ioctl_code, byte [] input, byte [] output, out int error);

#endregion

#region Listen

		public void Listen (int backlog)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (!is_bound)
				throw new SocketException ((int)SocketError.InvalidArgument);

			int error;
			Listen_internal (safe_handle, backlog, out error);

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
		extern static void Listen_internal(IntPtr sock, int backlog, out int error);

#endregion

#region Poll

		public bool Poll (int time_us, SelectMode mode)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (mode != SelectMode.SelectRead && mode != SelectMode.SelectWrite && mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (safe_handle, mode, time_us, out error);
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

#region SendPackets

		[MonoTODO ("Not implemented")]
		public bool SendPacketsAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			throw new NotImplementedException ();
		}

#endregion

#region SetSocketOption

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// I'd throw an ArgumentNullException, but this is what MS does.
			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault, "Error trying to dereference an invalid pointer");

			int error;
			SetSocketOption_internal (safe_handle, optionLevel, optionName, null, optionValue, 0, out error);

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
				SetSocketOption_internal (safe_handle, optionLevel, optionName, linger, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IP && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				MulticastOption multicast = optionValue as MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (safe_handle, optionLevel, optionName, multicast, null, 0, out error);
			} else if (optionLevel == SocketOptionLevel.IPv6 && (optionName == SocketOptionName.AddMembership || optionName == SocketOptionName.DropMembership)) {
				IPv6MulticastOption multicast = optionValue as IPv6MulticastOption;
				if (multicast == null)
					throw new ArgumentException ("A 'IPv6MulticastOption' value must be specified.", "optionValue");
				SetSocketOption_internal (safe_handle, optionLevel, optionName, multicast, null, 0, out error);
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
			SetSocketOption_internal (safe_handle, optionLevel, optionName, null, null, optionValue ? 1 : 0, out error);

			if (error != 0) {
				if (error == (int) SocketError.InvalidArgument)
					throw new ArgumentException ();
				throw new SocketException (error);
			}
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			SetSocketOption_internal (safe_handle, optionLevel, optionName, null, null, optionValue, out error);

			if (error != 0)
				throw new SocketException (error);
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

		// Closes the socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void Close_internal(IntPtr sock, out int error);

#endregion

#region Shutdown

		public void Shutdown (SocketShutdown how)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (!connected)
				throw new SocketException (10057); // Not connected

			int error;
			Shutdown_internal (safe_handle, how, out error);

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
		extern static void Shutdown_internal (IntPtr socket, SocketShutdown how, out int error);

#endregion

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

		SocketAsyncResult ValidateEndIAsyncResult (IAsyncResult iares, string methodname, string argname)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (iares == null)
				throw new ArgumentNullException (argname);

			SocketAsyncResult sockares = iares as SocketAsyncResult;
			if (sockares == null)
				throw new ArgumentException ("Invalid IAsyncResult", argname);
			if (sockares.endcalled != 0 || Interlocked.CompareExchange (ref sockares.endcalled, 1, 0) != 0)
				throw new InvalidOperationException (methodname + " can only be called once per asynchronous operation");

			return sockares;
		}

		void QueueSocketAsyncResult (Queue<SocketAsyncWorker> queue, SocketAsyncWorker worker, SocketAsyncResult sockares)
		{
			int count;
			lock (queue) {
				queue.Enqueue (worker);
				count = queue.Count;
			}

			if (count == 1)
				QueueIOWorkItem (SocketAsyncWorker.Dispatcher, sockares);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void QueueIOWorkItem (SocketAsyncCall d, SocketAsyncResult r);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void cancel_blocking_socket_operation (Thread thread);
	}
}
