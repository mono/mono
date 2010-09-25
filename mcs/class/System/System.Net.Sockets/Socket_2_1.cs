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
// (c) 2004-2006 Novell, Inc. (http://www.novell.com)
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Security;
using System.Text;

#if !NET_2_1
using System.Net.Configuration;
#endif

#if NET_2_0
using System.Collections.Generic;
#if !NET_2_1
using System.Net.NetworkInformation;
#endif
#endif

namespace System.Net.Sockets {

	public partial class Socket : IDisposable {

		internal enum SocketOperation {
			Accept,
			Connect,
			Receive,
			ReceiveFrom,
			Send,
			SendTo,
			UsedInManaged1,
			UsedInManaged2,
			UsedInProcess,
			UsedInConsole2,
			Disconnect,
			AcceptReceive,
			ReceiveGeneric,
			SendGeneric
		}

		[StructLayout (LayoutKind.Sequential)]
		internal sealed class SocketAsyncResult: IAsyncResult
		{
			/* Same structure in the runtime */
			/*
			  Keep this in sync with MonoSocketAsyncResult in
			  metadata/socket-io.h and ProcessAsyncReader
			  in System.Diagnostics/Process.cs.
			*/

			public Socket Sock;
			public IntPtr handle;
			object state;
			AsyncCallback callback;
			WaitHandle waithandle;

			Exception delayedException;

			public EndPoint EndPoint;	// Connect,ReceiveFrom,SendTo
			public byte [] Buffer;		// Receive,ReceiveFrom,Send,SendTo
			public int Offset;		// Receive,ReceiveFrom,Send,SendTo
			public int Size;		// Receive,ReceiveFrom,Send,SendTo
			public SocketFlags SockFlags;	// Receive,ReceiveFrom,Send,SendTo
			public Socket AcceptSocket;	// AcceptReceive
			public IPAddress[] Addresses;	// Connect
			public int Port;		// Connect
			public IList<ArraySegment<byte>> Buffers;	// Receive, Send
			public bool ReuseSocket;	// Disconnect

			// Return values
			Socket acc_socket;
			int total;

			bool completed_sync;
			bool completed;
			public bool blocking;
			internal int error;
			SocketOperation operation;
			public object ares;
			public int EndCalled;

			public SocketAsyncResult ()
			{
			}

			public void Init (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
			{
				this.Sock = sock;
				this.blocking = sock.blocking;
				this.handle = sock.socket;
				this.state = state;
				this.callback = callback;
				this.operation = operation;
				SockFlags = SocketFlags.None;
				if (waithandle != null)
					((ManualResetEvent) waithandle).Reset ();

				delayedException = null;

				EndPoint = null;
				Buffer = null;
				Offset = 0;
				Size = 0;
				SockFlags = 0;
				AcceptSocket = null;
				Addresses = null;
				Port = 0;
				Buffers = null;
				ReuseSocket = false;
				acc_socket = null;
				total = 0;

				completed_sync = false;
				completed = false;
				blocking = false;
				error = 0;
				ares = null;
				EndCalled = 0;
			}

			public void Dispose ()
			{
				Init (null, null, null, 0);
				if (waithandle != null) {
					waithandle.Close ();
					waithandle = null;
				}
			}

			public SocketAsyncResult (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
			{
				this.Sock = sock;
				this.blocking = sock.blocking;
				this.handle = sock.socket;
				this.state = state;
				this.callback = callback;
				this.operation = operation;
				SockFlags = SocketFlags.None;
			}

			public void CheckIfThrowDelayedException ()
			{
				if (delayedException != null) {
					Sock.connected = false;
					throw delayedException;
				}

				if (error != 0) {
					Sock.connected = false;
					throw new SocketException (error);
				}
			}

			void CompleteAllOnDispose (Queue queue)
			{
				object [] pending = queue.ToArray ();
				queue.Clear ();

				WaitCallback cb;
				for (int i = 0; i < pending.Length; i++) {
					SocketAsyncResult ares = (SocketAsyncResult) pending [i];
					cb = new WaitCallback (ares.CompleteDisposed);
					ThreadPool.QueueUserWorkItem (cb, null);
				}
			}

			void CompleteDisposed (object unused)
			{
				Complete ();
			}

			public void Complete ()
			{
				if (operation != SocketOperation.Receive && Sock.disposed)
					delayedException = new ObjectDisposedException (Sock.GetType ().ToString ());

				IsCompleted = true;

				Queue queue = null;
				if (operation == SocketOperation.Receive || operation == SocketOperation.ReceiveFrom) {
					queue = Sock.readQ;
				} else if (operation == SocketOperation.Send || operation == SocketOperation.SendTo) {
					queue = Sock.writeQ;
				}

				if (queue != null) {
					Worker worker = null;
					SocketAsyncCall sac = null;
					SocketAsyncResult req = null;
					lock (queue) {
						queue.Dequeue (); // remove ourselves
						if (queue.Count > 0) {
							worker = (Worker) queue.Peek ();
							if (!Sock.disposed) {
								sac = GetDelegate (worker, worker.result.operation);
							} else {
								CompleteAllOnDispose (queue);
							}
						}
					}

					if (sac != null)
						sac.BeginInvoke (null, worker.result);
				}

				if (callback != null)
					callback (this);
			}

			SocketAsyncCall GetDelegate (Worker worker, SocketOperation op)
			{
				switch (op) {
				case SocketOperation.Receive:
					return new SocketAsyncCall (worker.Receive);
				case SocketOperation.ReceiveFrom:
					return new SocketAsyncCall (worker.ReceiveFrom);
				case SocketOperation.Send:
					return new SocketAsyncCall (worker.Send);
				case SocketOperation.SendTo:
					return new SocketAsyncCall (worker.SendTo);
				default:
					return null; // never happens
				}
			}

			public void Complete (bool synch)
			{
				completed_sync = synch;
				Complete ();
			}

			public void Complete (int total)
			{
				this.total = total;
				Complete ();
			}

			public void Complete (Exception e, bool synch)
			{
				completed_sync = synch;
				delayedException = e;
				Complete ();
			}

			public void Complete (Exception e)
			{
				delayedException = e;
				Complete ();
			}

			public void Complete (Socket s)
			{
				acc_socket = s;
				Complete ();
			}

			public void Complete (Socket s, int total)
			{
				acc_socket = s;
				this.total = total;
				Complete ();
			}

			public object AsyncState {
				get {
					return state;
				}
			}

			public WaitHandle AsyncWaitHandle {
				get {
					lock (this) {
						if (waithandle == null)
							waithandle = new ManualResetEvent (completed);
					}

					return waithandle;
				}
				set {
					waithandle=value;
				}
			}

			public bool CompletedSynchronously {
				get {
					return(completed_sync);
				}
			}

			public bool IsCompleted {
				get {
					return(completed);
				}
				set {
					completed=value;
					lock (this) {
						if (waithandle != null && value) {
							((ManualResetEvent) waithandle).Set ();
						}
					}
				}
			}

			public Socket Socket {
				get {
					return acc_socket;
				}
			}

			public int Total {
				get { return total; }
				set { total = value; }
			}

			public SocketError ErrorCode
			{
				get {
					SocketException ex = delayedException as SocketException;
					if (ex != null)
						return(ex.SocketErrorCode);

					if (error != 0)
						return((SocketError)error);

					return(SocketError.Success);
				}
			}
		}

		internal sealed class Worker
		{
			public SocketAsyncResult result;
			SocketAsyncEventArgs args;

			public Worker (SocketAsyncEventArgs args)
			{
				this.args = args;
				result = new SocketAsyncResult ();
			}

			public Worker (SocketAsyncResult ares)
			{
				this.result = ares;
			}

			public void Dispose ()
			{
				if (result != null) {
					result.Dispose ();
					result = null;
					args = null;
				}
			}

			public void Init (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
			{
				result.Init (sock, state, callback, operation);
			}

			public void Accept ()
			{
				Socket acc_socket = null;
				try {
					if (args.AcceptSocket != null) {
						result.Sock.Accept (args.AcceptSocket);
						acc_socket = args.AcceptSocket;
					} else {
						acc_socket = result.Sock.Accept ();
					}
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete (acc_socket);
			}

			/* only used in 2.0 profile and newer, but
			 * leave in older profiles to keep interface
			 * to runtime consistent
			 */
			public void AcceptReceive ()
			{
				Socket acc_socket = null;
				try {
					if (result.AcceptSocket == null) {
						acc_socket = result.Sock.Accept ();
					} else {
						acc_socket = result.AcceptSocket;
						result.Sock.Accept (acc_socket);
					}
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				/* It seems the MS runtime
				 * special-cases 0-length requested
				 * receive data.  See bug 464201.
				 */
				int total = 0;
				if (result.Size > 0) {
					try {
						SocketError error;
						total = acc_socket.Receive_nochecks (result.Buffer,
										     result.Offset,
										     result.Size,
										     result.SockFlags,
										     out error);
					} catch (Exception e) {
						result.Complete (e);
						return;
					}
				}

				result.Complete (acc_socket, total);
			}

			public void Connect ()
			{
				/* If result.EndPoint is non-null,
				 * this is the standard one-address
				 * connect attempt.  Otherwise
				 * Addresses must be non-null and
				 * contain a list of addresses to try
				 * to connect to; the first one to
				 * succeed causes the rest of the list
				 * to be ignored.
				 */
				if (result.EndPoint != null) {
					try {
						if (!result.Sock.Blocking) {
							int success;
							result.Sock.Poll (-1, SelectMode.SelectWrite, out success);
							if (success == 0) {
								result.Sock.seed_endpoint = result.EndPoint;
								result.Sock.connected = true;
							} else {
								result.Complete (new SocketException (success));
								return;
							}
						} else {
							result.Sock.seed_endpoint = result.EndPoint;
							result.Sock.Connect (result.EndPoint);
							result.Sock.connected = true;
						}
					} catch (Exception e) {
						result.Complete (e);
						return;
					}

					result.Complete ();
				} else if (result.Addresses != null) {
					int error = (int) SocketError.InProgress; // why?
					foreach(IPAddress address in result.Addresses) {
						IPEndPoint iep = new IPEndPoint (address, result.Port);
						SocketAddress serial = iep.Serialize ();
						Socket.Connect_internal (result.Sock.socket, serial, out error);
						if (error == 0) {
							result.Sock.connected = true;
							result.Sock.seed_endpoint = iep;
							result.Complete ();
							return;
						} else if (error != (int)SocketError.InProgress &&
							   error != (int)SocketError.WouldBlock) {
							continue;
						}

						if (!result.Sock.Blocking) {
							int success;
							result.Sock.Poll (-1, SelectMode.SelectWrite, out success);
							if (success == 0) {
								result.Sock.connected = true;
								result.Sock.seed_endpoint = iep;
								result.Complete ();
								return;
							}
						}
					}
					result.Complete (new SocketException (error));
				} else {
					result.Complete (new SocketException ((int)SocketError.AddressNotAvailable));
				}
			}

			/* Also only used in 2.0 profile and newer */
			public void Disconnect ()
			{
				try {
					if (args != null)
						result.ReuseSocket = args.DisconnectReuseSocket;
					result.Sock.Disconnect (result.ReuseSocket);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete ();
			}

			public void Receive ()
			{
				// Actual recv() done in the runtime
				result.Complete ();
			}

			public void ReceiveFrom ()
			{
				int total = 0;
				try {
					total = result.Sock.ReceiveFrom_nochecks (result.Buffer,
									 result.Offset,
									 result.Size,
									 result.SockFlags,
									 ref result.EndPoint);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete (total);
			}

			public void ReceiveGeneric ()
			{
				int total = 0;
				try {
					SocketError error;
					total = result.Sock.Receive (result.Buffers, result.SockFlags, out error);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete (total);
			}

			int send_so_far;

			void UpdateSendValues (int last_sent)
			{
				if (result.error == 0) {
					send_so_far += last_sent;
					result.Offset += last_sent;
					result.Size -= last_sent;
				}
			}

			public void Send ()
			{
				// Actual send() done in the runtime
				if (result.error == 0) {
					UpdateSendValues (result.Total);
					if (result.Sock.disposed) {
						result.Complete ();
						return;
					}

					if (result.Size > 0) {
						SocketAsyncCall sac = new SocketAsyncCall (this.Send);
						sac.BeginInvoke (null, result);
						return; // Have to finish writing everything. See bug #74475.
					}
					result.Total = send_so_far;
				}
				result.Complete ();
			}

			public void SendTo ()
			{
				int total = 0;
				try {
					total = result.Sock.SendTo_nochecks (result.Buffer,
								    result.Offset,
								    result.Size,
								    result.SockFlags,
								    result.EndPoint);

					UpdateSendValues (total);
					if (result.Size > 0) {
						SocketAsyncCall sac = new SocketAsyncCall (this.SendTo);
						sac.BeginInvoke (null, result);
						return; // Have to finish writing everything. See bug #74475.
					}
					result.Total = send_so_far;
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete ();
			}

			public void SendGeneric ()
			{
				int total = 0;
				try {
					SocketError error;
					total = result.Sock.Send (result.Buffers, result.SockFlags, out error);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete (total);
			}
		}

		private Queue readQ = new Queue (2);
		private Queue writeQ = new Queue (2);

		delegate void SocketAsyncCall ();

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
#if !NET_2_1
#if NET_2_0 && CONFIGURATION_DEP
				SettingsSection config;
				config = (SettingsSection) System.Configuration.ConfigurationManager.GetSection ("system.net/settings");
				if (config != null)
					ipv6Supported = config.Ipv6.Enabled ? -1 : 0;
#else
				NetConfig config = System.Configuration.ConfigurationSettings.GetConfig("system.net/settings") as NetConfig;
				if (config != null)
					ipv6Supported = config.ipv6Enabled ? -1 : 0;
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

#if NET_2_0
		[ObsoleteAttribute ("Use OSSupportsIPv6 instead")]
#endif
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
#elif NET_2_0
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
		private IntPtr socket;
		private AddressFamily address_family;
		private SocketType socket_type;
		private ProtocolType protocol_type;
		internal bool blocking=true;
		Thread blocking_thread;
#if NET_2_0
		private bool isbound;
#endif
		/* When true, the socket was connected at the time of
		 * the last IO operation
		 */
		private bool connected;
		/* true if we called Close_internal */
		private bool closed;
		internal bool disposed;

		/*
		 * This EndPoint is used when creating new endpoints. Because
		 * there are many types of EndPoints possible,
		 * seed_endpoint.Create(addr) is used for creating new ones.
		 * As such, this value is set on Bind, SentTo, ReceiveFrom,
		 * Connect, etc.
 		 */
		internal EndPoint seed_endpoint = null;

#if !TARGET_JVM
		// Creates a new system socket, returning the handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Socket_internal(AddressFamily family,
						      SocketType type,
						      ProtocolType proto,
						      out int error);
#endif		
		
		public Socket(AddressFamily family, SocketType type, ProtocolType proto)
		{
#if NET_2_1
			switch (family) {
			case AddressFamily.InterNetwork:	// ok
			case AddressFamily.InterNetworkV6:	// ok
			case AddressFamily.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			// case AddressFamily.Unspecified:
			default:
				throw new ArgumentException ("family");
			}

			switch (type) {
			case SocketType.Stream:			// ok
			case SocketType.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			default:
				throw new ArgumentException ("type");
			}

			switch (proto) {
			case ProtocolType.Tcp:			// ok
			case ProtocolType.Unspecified:		// ok
			case ProtocolType.Unknown:		// SocketException will be thrown later (with right error #)
				break;
			default:
				throw new ArgumentException ("proto");
			}
#endif
			address_family=family;
			socket_type=type;
			protocol_type=proto;
			
			int error;
			
			socket = Socket_internal (family, type, proto, out error);
			if (error != 0)
				throw new SocketException (error);
#if !NET_2_1
			SocketDefaults ();
#endif
		}

		~Socket ()
		{
			Dispose (false);
		}


		public AddressFamily AddressFamily {
			get { return address_family; }
		}

#if !TARGET_JVM
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Blocking_internal(IntPtr socket,
							     bool block,
							     out int error);
#endif

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
#if NET_2_0
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
#endif
		// Returns the remote endpoint details in addr and port
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static SocketAddress RemoteEndPoint_internal(IntPtr socket, out int error);

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
				
				sa=RemoteEndPoint_internal(socket, out error);

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

		protected virtual void Dispose (bool explicitDisposing)
		{
			if (disposed)
				return;

			disposed = true;
			bool was_connected = connected;
			connected = false;
			if ((int) socket != -1) {
				int error;
				closed = true;
				IntPtr x = socket;
				socket = (IntPtr) (-1);
				Thread th = blocking_thread;
				if (th != null) {
					th.Abort ();
					blocking_thread = null;
				}

				if (was_connected)
					Linger (x);
				//DateTime start = DateTime.UtcNow;
				Close_internal (x, out error);
				//Console.WriteLine ("Time spent in Close_internal: {0}ms", (DateTime.UtcNow - start).TotalMilliseconds);
				if (error != 0)
					throw new SocketException (error);
			}
		}

#if NET_2_1
		public void Dispose ()
#else
		void IDisposable.Dispose ()
#endif
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		// Closes the socket
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Close_internal(IntPtr socket, out int error);

		public void Close ()
		{
			linger_timeout = 0;
			((IDisposable) this).Dispose ();
		}

#if NET_2_0
		public void Close (int timeout) 
		{
			linger_timeout = timeout;
			((IDisposable) this).Dispose ();
		}
#endif

		// Connects to the remote address
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Connect_internal(IntPtr sock,
							    SocketAddress sa,
							    out int error);

		public void Connect (EndPoint remoteEP)
		{
			SocketAddress serial = null;

			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (remoteEP == null)
				throw new ArgumentNullException ("remoteEP");

			IPEndPoint ep = remoteEP as IPEndPoint;
#if !MOONLIGHT
			if (ep != null && socket_type != SocketType.Dgram) /* Dgram uses Any to 'disconnect' */
#else
			if (ep != null)
#endif
				if (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any))
					throw new SocketException ((int) SocketError.AddressNotAvailable);

#if MOONLIGHT
			if (protocol_type != ProtocolType.Tcp)
				throw new SocketException ((int) SocketError.AccessDenied);
#elif NET_2_0
			/* TODO: check this for the 1.1 profile too */
			if (islistening)
				throw new InvalidOperationException ();
#endif
			serial = remoteEP.Serialize ();

			int error = 0;

			blocking_thread = Thread.CurrentThread;
			try {
				Connect_internal (socket, serial, out error);
			} catch (ThreadAbortException) {
				if (disposed) {
					Thread.ResetAbort ();
					error = (int) SocketError.Interrupted;
				}
			} finally {
				blocking_thread = null;
			}

			if (error == 0 || error == 10035)
				seed_endpoint = remoteEP; // Keep the ep around for non-blocking sockets

			if (error != 0)
				throw new SocketException (error);

#if !MOONLIGHT
			if (socket_type == SocketType.Dgram && (ep.Address.Equals (IPAddress.Any) || ep.Address.Equals (IPAddress.IPv6Any)))
				connected = false;
			else
				connected = true;
#else
			connected = true;
#endif

#if NET_2_0
			isbound = true;
#endif
		}

#if NET_2_0
		public bool ReceiveAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			//
			// LAME SPEC: the ArgumentException is never thrown, instead an NRE is
			// thrown when e.Buffer and e.BufferList are null (works fine when one is
			// set to a valid object)
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			// We do not support recv into multiple buffers yet
			if (e.BufferList != null)
				throw new NotSupportedException ("Mono doesn't support using BufferList at this point.");

			e.curSocket = this;
			e.Worker.Init (this, null, e.ReceiveCallback, SocketOperation.Receive);
			SocketAsyncResult res = e.Worker.result;
			res.Buffer = e.Buffer;
			res.Offset = e.Offset;
			res.Size = e.Count;
			res.SockFlags = e.SocketFlags;
			Worker worker = new Worker (e);
			lock (readQ) {
				readQ.Enqueue (worker);
				if (readQ.Count == 1) {
					SocketAsyncCall sac = new SocketAsyncCall (e.Worker.Receive);
					sac.BeginInvoke (null, res);
				}
			}
			return true;
		}

		public bool SendAsync (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)
			
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());
			if (e.Buffer == null && e.BufferList == null)
				throw new ArgumentException ("Either e.Buffer or e.BufferList must be valid buffers.");

			e.curSocket = this;
			e.Worker.Init (this, null, e.SendCallback, SocketOperation.Send);
			SocketAsyncResult res = e.Worker.result;
			res.Buffer = e.Buffer;
			res.Offset = e.Offset;
			res.Size = e.Count;
			res.SockFlags = e.SocketFlags;
			Worker worker = new Worker (e);
			lock (writeQ) {
				writeQ.Enqueue (worker);
				if (writeQ.Count == 1) {
					SocketAsyncCall sac = new SocketAsyncCall (e.Worker.Send);
					sac.BeginInvoke (null, res);
				}
			}
			return true;
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Poll_internal (IntPtr socket, SelectMode mode, int timeout, out int error);

		/* This overload is needed as the async Connect method
		 * also needs to check the socket error status, but
		 * getsockopt(..., SO_ERROR) clears the error.
		 */
		internal bool Poll (int time_us, SelectMode mode, out int socket_error)
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

			socket_error = (int)GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
			
			if (mode == SelectMode.SelectWrite && result) {
				/* Update the connected state; for
				 * non-blocking Connect()s this is
				 * when we can find out that the
				 * connect succeeded.
				 */
				if (socket_error == 0) {
					connected = true;
					isbound = true;
				}
			}
			
			return result;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Receive_internal(IntPtr sock,
							   byte[] buffer,
							   int offset,
							   int count,
							   SocketFlags flags,
							   out int error);

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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Send_internal(IntPtr sock,
							byte[] buf, int offset,
							int count,
							SocketFlags flags,
							out int error);

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
#if !NET_2_1
			if (protocol_type == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
#endif
		}

#if MOONLIGHT
		static void CheckConnect (SocketAsyncEventArgs e)
		{
			// NO check is made whether e != null in MS.NET (NRE is thrown in such case)

			if (e.RemoteEndPoint == null)
				throw new ArgumentNullException ("remoteEP");
			if (e.BufferList != null)
				throw new ArgumentException ("Multiple buffers cannot be used with this method.");
		}

		public bool ConnectAsync (SocketAsyncEventArgs e)
		{
			if (disposed && closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			CheckConnect (e);
			// if an address family is specified then they must match
			AddressFamily raf = e.RemoteEndPoint.AddressFamily;
			if ((raf != AddressFamily.Unspecified) && (raf != AddressFamily))
				throw new NotSupportedException ("AddressFamily mismatch between socket and endpoint");

			e.DoOperation (SocketAsyncOperation.Connect, this);

			// We always return true for now
			return true;
		}

		public static bool ConnectAsync (SocketType socketType, ProtocolType protocolType, SocketAsyncEventArgs e)
		{
			// exception ordering requires to check before creating the socket (good thing resource wise too)
			CheckConnect (e);

			// create socket based on the endpoint address family (if specified), otherwise default fo IPv4
			AddressFamily raf = e.RemoteEndPoint.AddressFamily;
			if (raf == AddressFamily.Unspecified)
				raf = AddressFamily.InterNetwork;
			Socket s = new Socket (raf, socketType, protocolType);
			e.DoOperation (SocketAsyncOperation.Connect, s);

			// We always return true for now
			return true;
		}

		public static void CancelConnectAsync (SocketAsyncEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");

			Socket s = e.ConnectSocket;
			if ((s != null) && (s.blocking_thread != null))
				s.blocking_thread.Abort ();
		}
#endif
	}
}

