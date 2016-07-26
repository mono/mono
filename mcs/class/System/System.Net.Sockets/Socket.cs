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
		const int SOCKET_CLOSED_CODE = 10004;
		const string TIMEOUT_EXCEPTION_MSG = "A connection attempt failed because the connected party did not properly respond" +
			"after a period of time, or established connection failed because connected host has failed to respond";

		/*
		 *	These two fields are looked up by name by the runtime, don't change
		 *  their name without also updating the runtime code.
		 */
		static int ipv4_supported = -1;
		static int ipv6_supported = -1;

		/* true if we called Close_internal */
		bool is_closed;

		bool is_listening;
		bool use_overlapped_io;

		int linger_timeout;

		AddressFamily address_family;
		SocketType socket_type;
		ProtocolType protocol_type;

		/* the field "safe_handle" is looked up by name by the runtime */
		internal SafeSocketHandle safe_handle;

		/*
		 * This EndPoint is used when creating new endpoints. Because
		 * there are many types of EndPoints possible,
		 * seed_endpoint.Create(addr) is used for creating new ones.
		 * As such, this value is set on Bind, SentTo, ReceiveFrom,
		 * Connect, etc.
		 */
		internal EndPoint seed_endpoint = null;

		internal Queue<KeyValuePair<IntPtr, IOSelectorJob>> readQ = new Queue<KeyValuePair<IntPtr, IOSelectorJob>> (2);
		internal Queue<KeyValuePair<IntPtr, IOSelectorJob>> writeQ = new Queue<KeyValuePair<IntPtr, IOSelectorJob>> (2);

		internal bool is_blocking = true;
		internal bool is_bound;

		/* When true, the socket was connected at the time of the last IO operation */
		internal bool is_connected;

		internal bool is_disposed;
		internal bool connect_in_progress;

#region Constructors

		static Socket ()
		{
			if (ipv4_supported == -1) {
				try {
					Socket tmp = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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

		//
		// This constructor is used by servers that want to listen for instance on both
		// ipv4 and ipv6.   Mono has historically done that if you use InterNetworkV6 (at
		// least on Unix), because that is the default behavior unless the IPV6_V6ONLY
		// option is explicitly set by using setsockopt (sock, IPPROTO_IPV6, IPV6_ONLY)
		//
		public Socket (SocketType socketType, ProtocolType protocolType)
			: this (AddressFamily.InterNetworkV6, socketType, protocolType)
		{
			DualMode = true;
		}
		
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
			this.address_family = addressFamily;
			this.socket_type = socketType;
			this.protocol_type = protocolType;

			int error;
			this.safe_handle = new SafeSocketHandle (Socket_internal (addressFamily, socketType, protocolType, out error), true);

			if (error != 0)
				throw new SocketException (error);

#if !NET_2_1 || MOBILE
			SocketDefaults ();
#endif
		}

#if !MOBILE
		public Socket (SocketInformation socketInformation)
		{
			this.is_listening      = (socketInformation.Options & SocketInformationOptions.Listening) != 0;
			this.is_connected      = (socketInformation.Options & SocketInformationOptions.Connected) != 0;
			this.is_blocking       = (socketInformation.Options & SocketInformationOptions.NonBlocking) == 0;
			this.use_overlapped_io = (socketInformation.Options & SocketInformationOptions.UseOnlyOverlappedIO) != 0;

			var result = Mono.DataConverter.Unpack ("iiiil", socketInformation.ProtocolInformation, 0);

			this.address_family = (AddressFamily) (int) result [0];
			this.socket_type = (SocketType) (int) result [1];
			this.protocol_type = (ProtocolType) (int) result [2];
			this.is_bound = (ProtocolType) (int) result [3] != 0;
			this.safe_handle = new SafeSocketHandle ((IntPtr) (long) result [4], true);

			SocketDefaults ();
		}
#endif

		/* private constructor used by Accept, which already has a socket handle to use */
		internal Socket(AddressFamily family, SocketType type, ProtocolType proto, SafeSocketHandle safe_handle)
		{
			this.address_family = family;
			this.socket_type = type;
			this.protocol_type = proto;
			
			this.safe_handle = safe_handle;
			this.is_connected = true;
		}

		~Socket ()
		{
			Dispose (false);
		}

		void SocketDefaults ()
		{
			try {
				/* Need to test IPv6 further */
				if (address_family == AddressFamily.InterNetwork
					// || address_family == AddressFamily.InterNetworkV6
				) {
					/* This is the default, but it probably has nasty side
					 * effects on Linux, as the socket option is kludged by
					 * turning on or off PMTU discovery... */
					this.DontFragment = false;
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

		[ObsoleteAttribute ("Use OSSupportsIPv4 instead")]
		public static bool SupportsIPv4 {
			get { return ipv4_supported == 1; }
		}

		[ObsoleteAttribute ("Use OSSupportsIPv6 instead")]
		public static bool SupportsIPv6 {
			get { return ipv6_supported == 1; }
		}

#if NET_2_1
		public static bool OSSupportsIPv4 {
			get { return ipv4_supported == 1; }
		}
#else
		public static bool OSSupportsIPv4 {
			get {
				NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces ();

				foreach (NetworkInterface adapter in nics) {
					if (adapter.Supports (NetworkInterfaceComponent.IPv4))
						return true;
				}

				return false;
			}
		}
#endif

#if NET_2_1
		public static bool OSSupportsIPv6 {
			get { return ipv6_supported == 1; }
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

		public int Available {
			get {
				ThrowIfDisposedAndClosed ();

				int ret, error;
				ret = Available_internal (safe_handle, out error);

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

		public bool DontFragment {
			get {
				ThrowIfDisposedAndClosed ();

				switch (address_family) {
				case AddressFamily.InterNetwork:
					return ((int) GetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment)) != 0;
				case AddressFamily.InterNetworkV6:
					return ((int) GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment)) != 0;
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
			set {
				ThrowIfDisposedAndClosed ();

				switch (address_family) {
				case AddressFamily.InterNetwork:
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.DontFragment, value ? 1 : 0);
					break;
				case AddressFamily.InterNetworkV6:
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.DontFragment, value ? 1 : 0);
					break;
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
		}

		public bool EnableBroadcast {
			get {
				ThrowIfDisposedAndClosed ();

				if (protocol_type != ProtocolType.Udp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				return ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast)) != 0;
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (protocol_type != ProtocolType.Udp)
					throw new SocketException ((int) SocketError.ProtocolOption);

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Broadcast, value ? 1 : 0);
			}
		}

		public bool ExclusiveAddressUse {
			get {
				ThrowIfDisposedAndClosed ();

				return ((int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse)) != 0;
			}
			set {
				ThrowIfDisposedAndClosed ();

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
				ThrowIfDisposedAndClosed ();

				return (LingerOption) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger);
			}
			set {
				ThrowIfDisposedAndClosed ();
				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger, value);
			}
		}

		public bool MulticastLoopback {
			get {
				ThrowIfDisposedAndClosed ();

				/* Even though this option can be set for TCP sockets on Linux, throw
				 * this exception anyway to be compatible (the MSDN docs say
				 * "Setting this property on a Transmission Control Protocol (TCP)
				 * socket will have no effect." but the MS runtime throws the
				 * exception...) */
				if (protocol_type == ProtocolType.Tcp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				switch (address_family) {
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
				if (protocol_type == ProtocolType.Tcp)
					throw new SocketException ((int)SocketError.ProtocolOption);

				switch (address_family) {
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

		public bool DualMode {
			get {
				if (AddressFamily != AddressFamily.InterNetworkV6) 
					throw new NotSupportedException("This protocol version is not supported");

				return ((int)GetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only) == 0);
			}
			set {
				if (AddressFamily != AddressFamily.InterNetworkV6) 
					throw new NotSupportedException("This protocol version is not supported");

				SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, value ? 0 : 1);
			}
		}

		private bool IsDualMode {
			get {
				return AddressFamily == AddressFamily.InterNetworkV6 && DualMode;
			}
		}

		[MonoTODO ("This doesn't do anything on Mono yet")]
		public bool UseOnlyOverlappedIO {
			get { return use_overlapped_io; }
			set { use_overlapped_io = value; }
		}

		public IntPtr Handle {
			get { return safe_handle.DangerousGetHandle (); }
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
				SocketAddress sa = LocalEndPoint_internal (safe_handle, (int) address_family, out error);

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

		public SocketType SocketType {
			get { return socket_type; }
		}

		public int SendTimeout {
			get {
				ThrowIfDisposedAndClosed ();

				return (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than -1");

				/* According to the MSDN docs we should adjust values between 1 and
				 * 499 to 500, but the MS runtime doesn't do this. */
				if (value == -1)
					value = 0;

				SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
			}
		}

		public int ReceiveTimeout {
			get {
				ThrowIfDisposedAndClosed ();

				return (int) GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than -1");

				if (value == -1)
					value = 0;

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
			}
		}

		public AddressFamily AddressFamily {
			get { return address_family; }
		}

		public bool Blocking {
			get { return is_blocking; }
			set {
				ThrowIfDisposedAndClosed ();

				int error;
				Blocking_internal (safe_handle, value, out error);

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

		public ProtocolType ProtocolType {
			get { return protocol_type; }
		}

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

		public int ReceiveBufferSize {
			get {
				ThrowIfDisposedAndClosed ();

				return (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
			}
		}

		public int SendBufferSize {
			get {
				ThrowIfDisposedAndClosed ();

				return (int) GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero");

				SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
			}
		}

		public short Ttl {
			get {
				ThrowIfDisposedAndClosed ();

				switch (address_family) {
				case AddressFamily.InterNetwork:
					return (short) (int) GetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive);
				case AddressFamily.InterNetworkV6:
					return (short) (int) GetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit);
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
			}
			set {
				ThrowIfDisposedAndClosed ();

				if (value < 0 || value > 255)
					throw new ArgumentOutOfRangeException ("value", "The value specified for a set operation is less than zero or greater than 255.");

				switch (address_family) {
				case AddressFamily.InterNetwork:
					SetSocketOption (SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, value);
					break;
				case AddressFamily.InterNetworkV6:
					SetSocketOption (SocketOptionLevel.IPv6, SocketOptionName.HopLimit, value);
					break;
				default:
					throw new NotSupportedException ("This property is only valid for InterNetwork and InterNetworkV6 sockets");
				}
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
				SocketAddress sa = RemoteEndPoint_internal (safe_handle, (int) address_family, out error);

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
						throw new ArgumentNullException ("name", "Contains a null element");
					sockets.Add (sock);
				}
			}

			sockets.Add (null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Select_internal (ref Socket [] sockets, int microSeconds, out int error);

#endregion

#region Poll

		public bool Poll (int time_us, SelectMode mode)
		{
			ThrowIfDisposedAndClosed ();

			if (mode != SelectMode.SelectRead && mode != SelectMode.SelectWrite && mode != SelectMode.SelectError)
				throw new NotSupportedException ("'mode' parameter is not valid.");

			int error;
			bool result = Poll_internal (safe_handle, mode, time_us, out error);

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
			SafeSocketHandle safe_handle = Accept_internal (this.safe_handle, out error, is_blocking);

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
			SafeSocketHandle safe_handle = Accept_internal (this.safe_handle, out error, is_blocking);

			if (error != 0) {
				if (is_closed)
					error = SOCKET_CLOSED_CODE;
				throw new SocketException (error);
			}

			acceptSocket.address_family = this.AddressFamily;
			acceptSocket.socket_type = this.SocketType;
			acceptSocket.protocol_type = this.ProtocolType;
			acceptSocket.safe_handle = safe_handle;
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

			QueueIOSelectorJob (readQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptCallback, e.socket_async_result));

			return true;
		}

		static AsyncCallback AcceptAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.AcceptSocket = e.current_socket.EndAccept (ares);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				if (e.AcceptSocket == null)
					e.AcceptSocket = new Socket (e.current_socket.AddressFamily, e.current_socket.SocketType, e.current_socket.ProtocolType, null);
				e.Complete ();
			}
		});

		public IAsyncResult BeginAccept(AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_bound || !is_listening)
				throw new InvalidOperationException ();

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Accept);

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptCallback, sockares));

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

		public IAsyncResult BeginAccept (int receiveSize, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (receiveSize < 0)
				throw new ArgumentOutOfRangeException ("receiveSize", "receiveSize is less than zero");

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.AcceptReceive) {
				Buffer = new byte [receiveSize],
				Offset = 0,
				Size = receiveSize,
				SockFlags = SocketFlags.None,
			};

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptReceiveCallback, sockares));

			return sockares;
		}

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

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginAcceptReceiveCallback, sockares));

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
					total = acc_socket.Receive_nochecks (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, out error);
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
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndAccept", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			sockares.CheckIfThrowDelayedException ();

			buffer = sockares.Buffer;
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

		public void Bind (EndPoint local_end)
		{
			ThrowIfDisposedAndClosed ();

			if (local_end == null)
				throw new ArgumentNullException("local_end");
				
			var ipEndPoint = local_end as IPEndPoint;
			if (ipEndPoint != null) {
				local_end = RemapIPEndPoint (ipEndPoint);	
			}
			
			int error;
			Bind_internal (safe_handle, local_end.Serialize(), out error);

			if (error != 0)
				throw new SocketException (error);
			if (error == 0)
				is_bound = true;

			seed_endpoint = local_end;
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
			Listen_internal(safe_handle, backlog, out error);

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

		public void Connect (IPAddress[] addresses, int port)
		{
			ThrowIfDisposedAndClosed ();

			if (addresses == null)
				throw new ArgumentNullException ("addresses");
			if (this.AddressFamily != AddressFamily.InterNetwork && this.AddressFamily != AddressFamily.InterNetworkV6)
				throw new NotSupportedException ("This method is only valid for addresses in the InterNetwork or InterNetworkV6 families");
			if (is_listening)
				throw new InvalidOperationException ();

			// FIXME: do non-blocking sockets Poll here?
			int error = 0;
			foreach (IPAddress address in addresses) {
				IPEndPoint iep = new IPEndPoint (address, port);
				
				iep = RemapIPEndPoint (iep);

				Connect_internal (safe_handle, iep.Serialize (), out error);
				if (error == 0) {
					is_connected = true;
					is_bound = true;
					seed_endpoint = iep;
					return;
				}
				if (error != (int)SocketError.InProgress && error != (int)SocketError.WouldBlock)
					continue;

				if (!is_blocking) {
					Poll (-1, SelectMode.SelectWrite);
					error = (int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
					if (error == 0) {
						is_connected = true;
						is_bound = true;
						seed_endpoint = iep;
						return;
					}
				}
			}

			if (error != 0)
				throw new SocketException (error);
		}


		public void Connect (EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			IPEndPoint ep = remoteEP as IPEndPoint;
			/* Dgram uses Any to 'disconnect' */
			if (ep != null && socket_type != SocketType.Dgram) {
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
			Connect_internal (safe_handle, serial, out error);

			if (error == 0 || error == 10035)
				seed_endpoint = remoteEP; // Keep the ep around for non-blocking sockets

			if (error != 0) {
				if (is_closed)
					error = SOCKET_CLOSED_CODE;
				throw new SocketException (error);
			}

			is_connected = !(socket_type == SocketType.Dgram && ep != null && (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)));
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

				if (!GetCheckedIPs (e, out addresses)) {
					e.socket_async_result.EndPoint = e.RemoteEndPoint;
					ares = (SocketAsyncResult) BeginConnect (e.RemoteEndPoint, ConnectAsyncCallback, e);
				} else {
					DnsEndPoint dep = (e.RemoteEndPoint as DnsEndPoint);
					e.socket_async_result.Addresses = addresses;
					e.socket_async_result.Port = dep.Port;
					ares = (SocketAsyncResult) BeginConnect (addresses, dep.Port, ConnectAsyncCallback, e);
				}

				if (ares.IsCompleted && ares.CompletedSynchronously) {
					ares.CheckIfThrowDelayedException ();
					return false;
				}
			} catch (Exception exc) {
				e.socket_async_result.Complete (exc, true);
				return false;
			}

			return true;
		}

		public static bool ConnectAsync (SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e)
		{
			var sock = new Socket (e.RemoteEndPoint.AddressFamily, socketType, protocolType);
			return sock.ConnectAsync (e);
		}

		public static void CancelConnectAsync (SocketAsyncEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			if (e.in_progress != 0 && e.LastOperation == SocketAsyncOperation.Connect)
				e.current_socket.Close();
		}

		static AsyncCallback ConnectAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.current_socket.EndConnect (ares);
			} catch (SocketException se) {
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
			}
		});

		public IAsyncResult BeginConnect (IPAddress address, int port, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

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
			ThrowIfDisposedAndClosed ();

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

		public IAsyncResult BeginConnect (EndPoint end_point, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (end_point == null)
				throw new ArgumentNullException ("end_point");

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Connect) {
				EndPoint = end_point,
			};

			// Bug #75154: Connect() should not succeed for .Any addresses.
			if (end_point is IPEndPoint) {
				IPEndPoint ep = (IPEndPoint) end_point;
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)) {
					sockares.Complete (new SocketException ((int) SocketError.AddressNotAvailable), true);
					return sockares;
				}
				
				end_point = RemapIPEndPoint (ep);
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

			bool blk = is_blocking;
			if (blk)
				Blocking = false;
			Connect_internal (safe_handle, end_point.Serialize (), out error);
			if (blk)
				Blocking = true;

			if (error == 0) {
				// succeeded synch
				is_connected = true;
				is_bound = true;
				sockares.Complete (true);
				return sockares;
			}

			if (error != (int) SocketError.InProgress && error != (int) SocketError.WouldBlock) {
				// error synch
				is_connected = false;
				is_bound = false;
				sockares.Complete (new SocketException (error), true);
				return sockares;
			}

			// continue asynch
			is_connected = false;
			is_bound = false;
			connect_in_progress = true;

			IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, BeginConnectCallback, sockares));

			return sockares;
		}

		public IAsyncResult BeginConnect (IPAddress[] addresses, int port, AsyncCallback callback, object state)
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

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Connect) {
				Addresses = addresses,
				Port = port,
			};

			is_connected = false;

			return BeginMConnect (sockares);
		}

		internal IAsyncResult BeginMConnect (SocketAsyncResult sockares)
		{
			SocketAsyncResult ares = null;
			Exception exc = null;
			AsyncCallback callback;

			for (int i = sockares.CurrentAddress; i < sockares.Addresses.Length; i++) {
				try {
					sockares.CurrentAddress++;

					ares = (SocketAsyncResult) BeginConnect (new IPEndPoint (sockares.Addresses [i], sockares.Port), null, sockares);
					if (ares.IsCompleted && ares.CompletedSynchronously) {
						ares.CheckIfThrowDelayedException ();

						callback = ares.AsyncCallback;
						if (callback != null)
							ThreadPool.UnsafeQueueUserWorkItem (_ => callback (ares), null);
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

		static IOAsyncCallback BeginConnectCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;

			if (sockares.EndPoint == null) {
				sockares.Complete (new SocketException ((int)SocketError.AddressNotAvailable));
				return;
			}

			SocketAsyncResult mconnect = sockares.AsyncState as SocketAsyncResult;
			bool is_mconnect = mconnect != null && mconnect.Addresses != null;

			try {
				EndPoint ep = sockares.EndPoint;
				int error_code = (int) sockares.socket.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);

				if (error_code == 0) {
					if (is_mconnect)
						sockares = mconnect;

					sockares.socket.seed_endpoint = ep;
					sockares.socket.is_connected = true;
					sockares.socket.is_bound = true;
					sockares.socket.connect_in_progress = false;
					sockares.error = 0;
					sockares.Complete ();
					return;
				}

				if (!is_mconnect) {
					sockares.socket.connect_in_progress = false;
					sockares.Complete (new SocketException (error_code));
					return;
				}

				if (mconnect.CurrentAddress >= mconnect.Addresses.Length) {
					mconnect.Complete (new SocketException (error_code));
					return;
				}

				mconnect.socket.BeginMConnect (mconnect);
			} catch (Exception e) {
				sockares.socket.connect_in_progress = false;

				if (is_mconnect)
					sockares = mconnect;

				sockares.Complete (e);
				return;
			}
		});

		public void EndConnect (IAsyncResult result)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (result, "EndConnect", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();
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

		/* Connects to the remote address */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static void Connect_internal(IntPtr sock, SocketAddress sa, out int error);

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
				return true;
			} else {
				e.ConnectByNameError = null;
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
			Disconnect_internal (safe_handle, reuseSocket, out error);

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
				e.current_socket.EndDisconnect (ares);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
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

		public int Receive (byte [] buffer)
		{
			return Receive (buffer, SocketFlags.None);
		}

		public int Receive (byte [] buffer, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, buffer.Length);

			SocketError error;
			int ret = Receive_nochecks (buffer, 0, buffer.Length, flags, out error);

			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && is_blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, TIMEOUT_EXCEPTION_MSG);
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int size, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, size);

			SocketError error;
			int ret = Receive_nochecks (buffer, 0, size, flags, out error);

			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && is_blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, TIMEOUT_EXCEPTION_MSG);
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int offset, int size, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			SocketError error;
			int ret = Receive_nochecks (buffer, offset, size, flags, out error);

			if (error != SocketError.Success) {
				if (error == SocketError.WouldBlock && is_blocking) // This might happen when ReceiveTimeout is set
					throw new SocketException ((int) error, TIMEOUT_EXCEPTION_MSG);
				throw new SocketException ((int) error);
			}

			return ret;
		}

		public int Receive (byte [] buffer, int offset, int size, SocketFlags flags, out SocketError error)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

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

			return(ret);
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

			/* Only example I can find of sending a byte array reference directly into an internal
			 * call is in System.Runtime.Remoting/System.Runtime.Remoting.Channels.Ipc.Win32/NamedPipeSocket.cs,
			 * so taking a lead from that... */
			WSABUF[] bufarray = new WSABUF[numsegments];
			GCHandle[] gch = new GCHandle[numsegments];

			for (int i = 0; i < numsegments; i++) {
				ArraySegment<byte> segment = buffers[i];

				if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray[i].len = segment.Count;
				bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
			}

			try {
				ret = Receive_internal (safe_handle, bufarray, socketFlags, out nativeError);
			} finally {
				for (int i = 0; i < numsegments; i++) {
					if (gch[i].IsAllocated)
						gch[i].Free ();
				}
			}

			errorCode = (SocketError) nativeError;

			return ret;
		}

		public bool ReceiveAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			// LAME SPEC: the ArgumentException is never thrown, instead an NRE is
			// thrown when e.Buffer and e.BufferList are null (works fine when one is
			// set to a valid object)
			if (e.Buffer == null && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			if (e.Buffer == null) {
				InitSocketAsyncEventArgs (e, ReceiveAsyncCallback, e, SocketOperation.ReceiveGeneric);

				e.socket_async_result.Buffers = e.BufferList;

				QueueIOSelectorJob (readQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveGenericCallback, e.socket_async_result));
			} else {
				InitSocketAsyncEventArgs (e, ReceiveAsyncCallback, e, SocketOperation.Receive);

				e.socket_async_result.Buffer = e.Buffer;
				e.socket_async_result.Offset = e.Offset;
				e.socket_async_result.Size = e.Count;

				QueueIOSelectorJob (readQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveCallback, e.socket_async_result));
			}

			return true;
		}

		static AsyncCallback ReceiveAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.BytesTransferred = e.current_socket.EndReceive (ares);
			} catch (SocketException se){
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
			}
		});

		public IAsyncResult BeginReceive (byte[] buffer, int offset, int size, SocketFlags socket_flags, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Receive) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
			};

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveCallback, sockares));

			return sockares;
		}

		public IAsyncResult BeginReceive (byte[] buffer, int offset, int size, SocketFlags flags, out SocketError error, AsyncCallback callback, object state)
		{
			/* As far as I can tell from the docs and from experimentation, a pointer to the
			 * SocketError parameter is not supposed to be saved for the async parts.  And as we don't
			 * set any socket errors in the setup code, we just have to set it to Success. */
			error = SocketError.Success;
			return BeginReceive (buffer, offset, size, flags, callback, state);
		}

		static IOAsyncCallback BeginReceiveCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				total = Receive_internal (sockares.socket.safe_handle, sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, out sockares.error);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null)
				throw new ArgumentNullException ("buffers");

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.ReceiveGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveGenericCallback, sockares));

			return sockares;
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginReceive (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			/* I assume the same SocketError semantics as above */
			errorCode = SocketError.Success;
			return BeginReceive (buffers, socketFlags, callback, state);
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

		public int EndReceive (IAsyncResult result)
		{
			SocketError error;
			int bytesReceived = EndReceive (result, out error);

			if (error != SocketError.Success) {
				if (error != SocketError.WouldBlock && error != SocketError.InProgress)
					is_connected = false;
				throw new SocketException ((int)error);
			}

			return bytesReceived;
		}

		public int EndReceive (IAsyncResult asyncResult, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndReceive", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			// If no socket error occurred, call CheckIfThrowDelayedException in case there are other
			// kinds of exceptions that should be thrown.
			if ((errorCode = sockares.ErrorCode) == SocketError.Success)
				sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		int Receive_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			int nativeError;
			int ret = Receive_internal (safe_handle, buf, offset, size, flags, out nativeError);

			error = (SocketError) nativeError;
			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				is_connected = false;
				is_bound = false;
			} else {
				is_connected = true;
			}

			return ret;
		}

		static int Receive_internal (SafeSocketHandle safeHandle, WSABUF[] bufarray, SocketFlags flags, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return Receive_internal (safeHandle.DangerousGetHandle (), bufarray, flags, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int Receive_internal (IntPtr sock, WSABUF[] bufarray, SocketFlags flags, out int error);

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
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);

			return ReceiveFrom (buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP);
		}

		public int ReceiveFrom (byte [] buffer, SocketFlags flags, ref EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);

			return ReceiveFrom (buffer, 0, buffer.Length, flags, ref remoteEP);
		}

		public int ReceiveFrom (byte [] buffer, int size, SocketFlags flags, ref EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, size);

			return ReceiveFrom (buffer, 0, size, flags, ref remoteEP);
		}

		public int ReceiveFrom (byte [] buffer, int offset, int size, SocketFlags flags, ref EndPoint remoteEP)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			int error;
			return ReceiveFrom_nochecks_exc (buffer, offset, size, flags, ref remoteEP, true, out error);
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

			QueueIOSelectorJob (readQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveFromCallback, e.socket_async_result));

			return true;
		}

		static AsyncCallback ReceiveFromAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.BytesTransferred = e.current_socket.EndReceiveFrom (ares, ref e.remote_ep);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
			}
		});

		public IAsyncResult BeginReceiveFrom (byte[] buffer, int offset, int size, SocketFlags socket_flags, ref EndPoint remote_end, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remote_end == null)
				throw new ArgumentNullException ("remote_end");

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.ReceiveFrom) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
				EndPoint = remote_end,
			};

			QueueIOSelectorJob (readQ, sockares.Handle, new IOSelectorJob (IOOperation.Read, BeginReceiveFromCallback, sockares));

			return sockares;
		}

		static IOAsyncCallback BeginReceiveFromCallback = new IOAsyncCallback (ares => {
			SocketAsyncResult sockares = (SocketAsyncResult) ares;
			int total = 0;

			try {
				int error;
				total = sockares.socket.ReceiveFrom_nochecks_exc (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, ref sockares.EndPoint, true, out error);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			sockares.Complete (total);
		});

		public int EndReceiveFrom(IAsyncResult result, ref EndPoint end_point)
		{
			ThrowIfDisposedAndClosed ();

			if (end_point == null)
				throw new ArgumentNullException ("remote_end");

			SocketAsyncResult sockares = ValidateEndIAsyncResult (result, "EndReceiveFrom", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			end_point = sockares.EndPoint;

			return sockares.Total;
		}

		internal int ReceiveFrom_nochecks_exc (byte [] buf, int offset, int size, SocketFlags flags, ref EndPoint remote_end, bool throwOnError, out int error)
		{
			SocketAddress sockaddr = remote_end.Serialize();

			int cnt = ReceiveFrom_internal (safe_handle, buf, offset, size, flags, ref sockaddr, out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress) {
					is_connected = false;
				} else if (err == SocketError.WouldBlock && is_blocking) { // This might happen when ReceiveTimeout is set
					if (throwOnError)	
						throw new SocketException ((int) SocketError.TimedOut, TIMEOUT_EXCEPTION_MSG);
					error = (int) SocketError.TimedOut;
					return 0;
				}

				if (throwOnError)
					throw new SocketException (error);

				return 0;
			}

			is_connected = true;
			is_bound = true;

			/* If sockaddr is null then we're a connection oriented protocol and should ignore the
			 * remote_end parameter (see MSDN documentation for Socket.ReceiveFrom(...) ) */
			if (sockaddr != null) {
				/* Stupidly, EndPoint.Create() is an instance method */
				remote_end = remote_end.Create (sockaddr);
			}

			seed_endpoint = remote_end;

			return cnt;
		}

		static int ReceiveFrom_internal (SafeSocketHandle safeHandle, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return ReceiveFrom_internal (safeHandle.DangerousGetHandle (), buffer, offset, count, flags, ref sockaddr, out error);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static int ReceiveFrom_internal(IntPtr sock, byte[] buffer, int offset, int count, SocketFlags flags, ref SocketAddress sockaddr, out int error);

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

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndReceiveMessageFrom", "asyncResult");

			throw new NotImplementedException ();
		}

#endregion

#region Send

		public int Send (byte [] buffer)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, buffer.Length);

			SocketError error;
			int ret = Send_nochecks (buffer, 0, buffer.Length, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buffer, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, buffer.Length);

			SocketError error;
			int ret = Send_nochecks (buffer, 0, buffer.Length, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buffer, int size, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, 0, size);

			SocketError error;
			int ret = Send_nochecks (buffer, 0, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buffer, int offset, int size, SocketFlags flags)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			SocketError error;
			int ret = Send_nochecks (buffer, offset, size, flags, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public int Send (byte [] buffer, int offset, int size, SocketFlags flags, out SocketError error)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			return Send_nochecks (buffer, offset, size, flags, out error);
		}

		public
		int Send (IList<ArraySegment<byte>> buffers)
		{
			SocketError error;
			int ret = Send (buffers, SocketFlags.None, out error);

			if (error != SocketError.Success)
				throw new SocketException ((int) error);

			return ret;
		}

		public
		int Send (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags)
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
			ThrowIfDisposedAndClosed ();

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

				if (segment.Offset < 0 || segment.Count < 0 || segment.Count > segment.Array.Length - segment.Offset)
					throw new ArgumentOutOfRangeException ("segment");

				gch[i] = GCHandle.Alloc (segment.Array, GCHandleType.Pinned);
				bufarray[i].len = segment.Count;
				bufarray[i].buf = Marshal.UnsafeAddrOfPinnedArrayElement (segment.Array, segment.Offset);
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

			errorCode = (SocketError)nativeError;

			return ret;
		}

		int Send_nochecks (byte [] buf, int offset, int size, SocketFlags flags, out SocketError error)
		{
			if (size == 0) {
				error = SocketError.Success;
				return 0;
			}

			int nativeError;
			int ret = Send_internal (safe_handle, buf, offset, size, flags, out nativeError);

			error = (SocketError)nativeError;

			if (error != SocketError.Success && error != SocketError.WouldBlock && error != SocketError.InProgress) {
				is_connected = false;
				is_bound = false;
			} else {
				is_connected = true;
			}

			return ret;
		}

		public bool SendAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			ThrowIfDisposedAndClosed ();

			if (e.Buffer == null && e.BufferList == null)
				throw new NullReferenceException ("Either e.Buffer or e.BufferList must be valid buffers.");

			if (e.Buffer == null) {
				InitSocketAsyncEventArgs (e, SendAsyncCallback, e, SocketOperation.SendGeneric);

				e.socket_async_result.Buffers = e.BufferList;

				QueueIOSelectorJob (writeQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, BeginSendGenericCallback, e.socket_async_result));
			} else {
				InitSocketAsyncEventArgs (e, SendAsyncCallback, e, SocketOperation.Send);

				e.socket_async_result.Buffer = e.Buffer;
				e.socket_async_result.Offset = e.Offset;
				e.socket_async_result.Size = e.Count;

				QueueIOSelectorJob (writeQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, 0), e.socket_async_result));
			}

			return true;
		}

		static AsyncCallback SendAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.BytesTransferred = e.current_socket.EndSend (ares);
			} catch (SocketException se){
				e.SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
			}
		});

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			if (!is_connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int) errorCode);
			}

			errorCode = SocketError.Success;
			return BeginSend (buffer, offset, size, socketFlags, callback, state);
		}

		public IAsyncResult BeginSend (byte[] buffer, int offset, int size, SocketFlags socket_flags, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (!is_connected)
				throw new SocketException ((int)SocketError.NotConnected);

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.Send) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
			};

			QueueIOSelectorJob (writeQ, sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, 0), sockares));

			return sockares;
		}

		static void BeginSendCallback (SocketAsyncResult sockares, int sent_so_far)
		{
			int total = 0;

			try {
				total = Socket.Send_internal (sockares.socket.safe_handle, sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, out sockares.error);
			} catch (Exception e) {
				sockares.Complete (e);
				return;
			}

			if (sockares.error == 0) {
				sent_so_far += total;
				sockares.Offset += total;
				sockares.Size -= total;

				if (sockares.socket.is_disposed) {
					sockares.Complete (total);
					return;
				}

				if (sockares.Size > 0) {
					IOSelector.Add (sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendCallback ((SocketAsyncResult) s, sent_so_far), sockares));
					return; // Have to finish writing everything. See bug #74475.
				}

				sockares.Total = sent_so_far;
			}

			sockares.Complete (total);
		}

		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (buffers == null)
				throw new ArgumentNullException ("buffers");
			if (!is_connected)
				throw new SocketException ((int)SocketError.NotConnected);

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.SendGeneric) {
				Buffers = buffers,
				SockFlags = socketFlags,
			};

			QueueIOSelectorJob (writeQ, sockares.Handle, new IOSelectorJob (IOOperation.Write, BeginSendGenericCallback, sockares));

			return sockares;
		}

		[CLSCompliant (false)]
		public IAsyncResult BeginSend (IList<ArraySegment<byte>> buffers, SocketFlags socketFlags, out SocketError errorCode, AsyncCallback callback, object state)
		{
			if (!is_connected) {
				errorCode = SocketError.NotConnected;
				throw new SocketException ((int)errorCode);
			}

			errorCode = SocketError.Success;
			return BeginSend (buffers, socketFlags, callback, state);
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

		public int EndSend (IAsyncResult result)
		{
			SocketError error;
			int bytesSent = EndSend (result, out error);

			if (error != SocketError.Success) {
				if (error != SocketError.WouldBlock && error != SocketError.InProgress)
					is_connected = false;
				throw new SocketException ((int)error);
			}

			return bytesSent;
		}

		public int EndSend (IAsyncResult asyncResult, out SocketError errorCode)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (asyncResult, "EndSend", "asyncResult");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne ();

			/* If no socket error occurred, call CheckIfThrowDelayedException in
			 * case there are other kinds of exceptions that should be thrown.*/
			if ((errorCode = sockares.ErrorCode) == SocketError.Success)
				sockares.CheckIfThrowDelayedException ();

			return sockares.Total;
		}

		static int Send_internal (SafeSocketHandle safeHandle, WSABUF[] bufarray, SocketFlags flags, out int error)
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
		extern static int Send_internal (IntPtr sock, WSABUF[] bufarray, SocketFlags flags, out int error);

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

#region SendTo

		public int SendTo (byte [] buffer, EndPoint remote_end)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);

			return SendTo (buffer, 0, buffer.Length, SocketFlags.None, remote_end);
		}

		public int SendTo (byte [] buffer, SocketFlags flags, EndPoint remote_end)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);

			return SendTo (buffer, 0, buffer.Length, flags, remote_end);
		}

		public int SendTo (byte [] buffer, int size, SocketFlags flags, EndPoint remote_end)
		{
			return SendTo (buffer, 0, size, flags, remote_end);
		}

		public int SendTo (byte [] buffer, int offset, int size, SocketFlags flags, EndPoint remote_end)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			if (remote_end == null)
				throw new ArgumentNullException("remote_end");

			return SendTo_nochecks (buffer, offset, size, flags, remote_end);
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

			QueueIOSelectorJob (writeQ, e.socket_async_result.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendToCallback ((SocketAsyncResult) s, 0), e.socket_async_result));

			return true;
		}

		static AsyncCallback SendToAsyncCallback = new AsyncCallback (ares => {
			SocketAsyncEventArgs e = (SocketAsyncEventArgs) ((SocketAsyncResult) ares).AsyncState;

			if (Interlocked.Exchange (ref e.in_progress, 0) != 1)
				throw new InvalidOperationException ("No operation in progress");

			try {
				e.BytesTransferred = e.current_socket.EndSendTo (ares);
			} catch (SocketException ex) {
				e.SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				e.SocketError = SocketError.OperationAborted;
			} finally {
				e.Complete ();
			}
		});

		public IAsyncResult BeginSendTo(byte[] buffer, int offset, int size, SocketFlags socket_flags, EndPoint remote_end, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();
			ThrowIfBufferNull (buffer);
			ThrowIfBufferOutOfRange (buffer, offset, size);

			SocketAsyncResult sockares = new SocketAsyncResult (this, callback, state, SocketOperation.SendTo) {
				Buffer = buffer,
				Offset = offset,
				Size = size,
				SockFlags = socket_flags,
				EndPoint = remote_end,
			};

			QueueIOSelectorJob (writeQ, sockares.Handle, new IOSelectorJob (IOOperation.Write, s => BeginSendToCallback ((SocketAsyncResult) s, 0), sockares));

			return sockares;
		}

		static void BeginSendToCallback (SocketAsyncResult sockares, int sent_so_far)
		{
			int total = 0;
			try {
				total = sockares.socket.SendTo_nochecks (sockares.Buffer, sockares.Offset, sockares.Size, sockares.SockFlags, sockares.EndPoint);

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

		public int EndSendTo (IAsyncResult result)
		{
			ThrowIfDisposedAndClosed ();

			SocketAsyncResult sockares = ValidateEndIAsyncResult (result, "EndSendTo", "result");

			if (!sockares.IsCompleted)
				sockares.AsyncWaitHandle.WaitOne();

			sockares.CheckIfThrowDelayedException();

			return sockares.Total;
		}

		int SendTo_nochecks (byte [] buffer, int offset, int size, SocketFlags flags, EndPoint remote_end)
		{
			int error;
			int ret = SendTo_internal (safe_handle, buffer, offset, size, flags, remote_end.Serialize (), out error);

			SocketError err = (SocketError) error;
			if (err != 0) {
				if (err != SocketError.WouldBlock && err != SocketError.InProgress)
					is_connected = false;
				throw new SocketException (error);
			}

			is_connected = true;
			is_bound = true;
			seed_endpoint = remote_end;

			return ret;
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
		extern static int SendTo_internal (IntPtr sock, byte[] buffer, int offset, int count, SocketFlags flags, SocketAddress sa, out int error);

#endregion

#region SendFile

		public void SendFile (string fileName)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new NotSupportedException ();
			if (!is_blocking)
				throw new InvalidOperationException ();

			SendFile (fileName, null, null, 0);
		}

		public void SendFile (string fileName, byte[] preBuffer, byte[] postBuffer, TransmitFileOptions flags)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new NotSupportedException ();
			if (!is_blocking)
				throw new InvalidOperationException ();

			if (!SendFile_internal (safe_handle, fileName, preBuffer, postBuffer, flags)) {
				SocketException exc = new SocketException ();
				if (exc.ErrorCode == 2 || exc.ErrorCode == 3)
					throw new FileNotFoundException ();
				throw exc;
			}
		}

		public IAsyncResult BeginSendFile (string fileName, AsyncCallback callback, object state)
		{
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
				throw new NotSupportedException ();
			if (!File.Exists (fileName))
				throw new FileNotFoundException ();

			return BeginSendFile (fileName, null, null, 0, callback, state);
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

		static bool SendFile_internal (SafeSocketHandle safeHandle, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags)
		{
			try {
				safeHandle.RegisterForBlockingSyscall ();
				return SendFile_internal (safeHandle.DangerousGetHandle (), filename, pre_buffer, post_buffer, flags);
			} finally {
				safeHandle.UnRegisterForBlockingSyscall ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool SendFile_internal (IntPtr sock, string filename, byte [] pre_buffer, byte [] post_buffer, TransmitFileOptions flags);

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

#if !MOBILE
		[MonoLimitation ("We do not support passing sockets across processes, we merely allow this API to pass the socket across AppDomains")]
		public SocketInformation DuplicateAndClose (int targetProcessId)
		{
			var si = new SocketInformation ();
			si.Options =
				(is_listening      ? SocketInformationOptions.Listening : 0) |
				(is_connected      ? SocketInformationOptions.Connected : 0) |
				(is_blocking       ? 0 : SocketInformationOptions.NonBlocking) |
				(use_overlapped_io ? SocketInformationOptions.UseOnlyOverlappedIO : 0);

			si.ProtocolInformation = Mono.DataConverter.Pack ("iiiil", (int)address_family, (int)socket_type, (int)protocol_type, is_bound ? 1 : 0, (long)Handle);
			safe_handle = null;

			return si;
		}
#endif

#endregion

#region GetSocketOption

		public void GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, byte [] optionValue)
		{
			ThrowIfDisposedAndClosed ();

			if (optionValue == null)
				throw new SocketException ((int) SocketError.Fault, "Error trying to dereference an invalid pointer");

			int error;
			GetSocketOption_arr_internal (safe_handle, optionLevel, optionName, ref optionValue, out error);

			if (error != 0)
				throw new SocketException (error);
		}

		public byte [] GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int length)
		{
			ThrowIfDisposedAndClosed ();

			int error;
			byte[] byte_val = new byte [length];
			GetSocketOption_arr_internal (safe_handle, optionLevel, optionName, ref byte_val, out error);

			if (error != 0)
				throw new SocketException (error);

			return byte_val;
		}

		public object GetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName)
		{
			ThrowIfDisposedAndClosed ();

			int error;
			object obj_val;
			GetSocketOption_obj_internal (safe_handle, optionLevel, optionName, out obj_val, out error);

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
			SetSocketOption_internal (safe_handle, optionLevel, optionName, null, optionValue, 0, out error);

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
			int int_val = optionValue ? 1 : 0;

			SetSocketOption (optionLevel, optionName, int_val);
		}

		public void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
		{
			ThrowIfDisposedAndClosed ();

			if (optionLevel == SocketOptionLevel.Socket && optionName == SocketOptionName.ReuseAddress && optionValue != 0 && !SupportsPortReuse (protocol_type))
				throw new SocketException ((int) SocketError.OperationNotSupported, "Operating system sockets do not support ReuseAddress.\nIf your socket is not intended to bind to the same address and port multiple times remove this option, otherwise you should ignore this exception inside a try catch and check that ReuseAddress is true before binding to the same address and port multiple times.");

			int error;
			SetSocketOption_internal (safe_handle, optionLevel, optionName, null, null, optionValue, out error);

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

		public int IOControl (int ioctl_code, byte [] in_value, byte [] out_value)
		{
			if (is_disposed)
				throw new ObjectDisposedException (GetType ().ToString ());

			int error;
			int result = IOControl_internal (safe_handle, ioctl_code, in_value, out_value, out error);

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
			ThrowIfDisposedAndClosed ();

			if (!is_connected)
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
		internal extern static void Shutdown_internal (IntPtr socket, SocketShutdown how, out int error);

#endregion

#region Dispose

		protected virtual void Dispose (bool disposing)
		{
			if (is_disposed)
				return;

			is_disposed = true;
			bool was_connected = is_connected;
			is_connected = false;

			if (safe_handle != null) {
				is_closed = true;
				IntPtr x = Handle;

				if (was_connected)
					Linger (x);

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
			if (socket.is_disposed && socket.is_closed)
				throw new ObjectDisposedException (socket.GetType ().ToString ());
		}

		void ThrowIfDisposedAndClosed ()
		{
			if (is_disposed && is_closed)
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
#if !NET_2_1 || MOBILE
			if (protocol_type == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
#endif
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

		void QueueIOSelectorJob (Queue<KeyValuePair<IntPtr, IOSelectorJob>> queue, IntPtr handle, IOSelectorJob job)
		{
			int count;
			lock (queue) {
				queue.Enqueue (new KeyValuePair<IntPtr, IOSelectorJob> (handle, job));
				count = queue.Count;
			}

			if (count == 1)
				IOSelector.Add (handle, job);
		}

		void InitSocketAsyncEventArgs (SocketAsyncEventArgs e, AsyncCallback callback, object state, SocketOperation operation)
		{
			e.socket_async_result.Init (this, callback, state, operation);
			if (e.AcceptSocket != null) {
				e.socket_async_result.AcceptSocket = e.AcceptSocket;
			}
			e.current_socket = this;
			e.SetLastOperation (SocketOperationToSocketAsyncOperation (operation));
			e.SocketError = SocketError.Success;
			e.BytesTransferred = 0;
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
	}
}

