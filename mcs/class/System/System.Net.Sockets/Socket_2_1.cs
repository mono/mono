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
#if MOONLIGHT && !INSIDE_SYSTEM
using System.Net.Policy;
#endif

namespace System.Net.Sockets {

	public partial class Socket : IDisposable {
		[StructLayout (LayoutKind.Sequential)]
		struct WSABUF {
			public int len;
			public IntPtr buf;
		}

		// Used by the runtime
		internal enum SocketOperation {
			Accept = 0,
			Connect,
			Receive,
			ReceiveFrom,
			Send,
			SendTo,
			RecvJustCallback,
			SendJustCallback,
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
			AsyncCallback callback; // used from the runtime
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
			public SocketOperation operation;
			public object ares;
			public int EndCalled;

			// These fields are not in MonoSocketAsyncResult
			public Worker Worker;
			public int CurrentAddress; // Connect

			public SocketAsyncResult ()
			{
			}

			public void Init (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
			{
				this.Sock = sock;
				if (sock != null) {
					this.blocking = sock.blocking;
					this.handle = sock.socket;
				} else {
					this.blocking = true;
					this.handle = IntPtr.Zero;
				}
				this.state = state;
				this.callback = callback;
				GC.KeepAlive (this.callback);
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
				Worker = null;
			}

			public void DoMConnectCallback ()
			{
				if (callback == null)
					return;
#if MOONLIGHT
				ThreadPool.QueueUserWorkItem (_ => { callback (this); }, null);
#else
				ThreadPool.UnsafeQueueUserWorkItem (_ => { callback (this); }, null);
#endif
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
				GC.KeepAlive (this.callback);
				this.operation = operation;
				SockFlags = SocketFlags.None;
				Worker = new Worker (this);
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
					Worker worker = (Worker) pending [i];
					SocketAsyncResult ares = worker.result;
					cb = new WaitCallback (ares.CompleteDisposed);
#if MOONLIGHT
					ThreadPool.QueueUserWorkItem (cb, null);
#else
					ThreadPool.UnsafeQueueUserWorkItem (cb, null);
#endif
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
				if (operation == SocketOperation.Receive ||
				    operation == SocketOperation.ReceiveFrom ||
				    operation == SocketOperation.ReceiveGeneric) {
					queue = Sock.readQ;
				} else if (operation == SocketOperation.Send ||
					   operation == SocketOperation.SendTo ||
					   operation == SocketOperation.SendGeneric) {

					queue = Sock.writeQ;
				}

				if (queue != null) {
					Worker worker = null;
					SocketAsyncCall sac = null;
					lock (queue) {
						// queue.Count will only be 0 if the socket is closed while receive/send
						// operation(s) are pending and at least one call to this method is
						// waiting on the lock while another one calls CompleteAllOnDispose()
						if (queue.Count > 0)
							queue.Dequeue (); // remove ourselves
						if (queue.Count > 0) {
							worker = (Worker) queue.Peek ();
							if (!Sock.disposed) {
								sac = Worker.Dispatcher;
							} else {
								CompleteAllOnDispose (queue);
							}
						}
					}

					if (sac != null)
						Socket.socket_pool_queue (sac, worker.result);
				}
				// IMPORTANT: 'callback', if any is scheduled from unmanaged code
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

			public SocketError ErrorCode {
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
				result.Worker = this;
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

			public static SocketAsyncCall Dispatcher = new SocketAsyncCall (DispatcherCB);

			static void DispatcherCB (SocketAsyncResult sar)
			{
				SocketOperation op = sar.operation;
				if (op == Socket.SocketOperation.Receive || op == Socket.SocketOperation.ReceiveGeneric ||
					op == Socket.SocketOperation.RecvJustCallback)
					sar.Worker.Receive ();
				else if (op == Socket.SocketOperation.Send || op == Socket.SocketOperation.SendGeneric ||
					op == Socket.SocketOperation.SendJustCallback)
					sar.Worker.Send ();
#if !MOONLIGHT
				else if (op == Socket.SocketOperation.ReceiveFrom)
					sar.Worker.ReceiveFrom ();
				else if (op == Socket.SocketOperation.SendTo)
					sar.Worker.SendTo ();
#endif
				else if (op == Socket.SocketOperation.Connect)
					sar.Worker.Connect ();
#if !MOONLIGHT
				else if (op == Socket.SocketOperation.Accept)
					sar.Worker.Accept ();
				else if (op == Socket.SocketOperation.AcceptReceive)
					sar.Worker.AcceptReceive ();
				else if (op == Socket.SocketOperation.Disconnect)
					sar.Worker.Disconnect ();

				// SendPackets and ReceiveMessageFrom are not implemented yet
				/*
				else if (op == Socket.SocketOperation.ReceiveMessageFrom)
					async_op = SocketAsyncOperation.ReceiveMessageFrom;
				else if (op == Socket.SocketOperation.SendPackets)
					async_op = SocketAsyncOperation.SendPackets;
				*/
#endif
				else
					throw new NotImplementedException (String.Format ("Operation {0} is not implemented", op));
			}

			/* This is called when reusing a SocketAsyncEventArgs */
			public void Init (Socket sock, SocketAsyncEventArgs args, SocketOperation op)
			{
				result.Init (sock, args, SocketAsyncEventArgs.Dispatcher, op);
				result.Worker = this;
				SocketAsyncOperation async_op;

				// Notes;
				// 	-SocketOperation.AcceptReceive not used in SocketAsyncEventArgs
				//	-SendPackets and ReceiveMessageFrom are not implemented yet
				if (op == Socket.SocketOperation.Connect)
					async_op = SocketAsyncOperation.Connect;
#if !MOONLIGHT
				else if (op == Socket.SocketOperation.Accept)
					async_op = SocketAsyncOperation.Accept;
				else if (op == Socket.SocketOperation.Disconnect)
					async_op = SocketAsyncOperation.Disconnect;
#endif
				else if (op == Socket.SocketOperation.Receive || op == Socket.SocketOperation.ReceiveGeneric)
					async_op = SocketAsyncOperation.Receive;
#if !MOONLIGHT
				else if (op == Socket.SocketOperation.ReceiveFrom)
					async_op = SocketAsyncOperation.ReceiveFrom;
#endif
				/*
				else if (op == Socket.SocketOperation.ReceiveMessageFrom)
					async_op = SocketAsyncOperation.ReceiveMessageFrom;
				*/
				else if (op == Socket.SocketOperation.Send || op == Socket.SocketOperation.SendGeneric)
					async_op = SocketAsyncOperation.Send;
#if !MOONLIGHT
				/*
				else if (op == Socket.SocketOperation.SendPackets)
					async_op = SocketAsyncOperation.SendPackets;
				*/
				else if (op == Socket.SocketOperation.SendTo)
					async_op = SocketAsyncOperation.SendTo;
#endif
				else
					throw new NotImplementedException (String.Format ("Operation {0} is not implemented", op));

				args.SetLastOperation (async_op);
				args.SocketError = SocketError.Success;
				args.BytesTransferred = 0;
                                args.Count = 0;
			}

			public void Accept ()
			{
#if !MOONLIGHT
				Socket acc_socket = null;
				try {
					if (args != null && args.AcceptSocket != null) {
						result.Sock.Accept (args.AcceptSocket);
						acc_socket = args.AcceptSocket;
					} else {
						acc_socket = result.Sock.Accept ();
						if (args != null)
							args.AcceptSocket = acc_socket;
					}
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete (acc_socket);
#endif
			}

			/* only used in 2.0 profile and newer, but
			 * leave in older profiles to keep interface
			 * to runtime consistent
			 */
			public void AcceptReceive ()
			{
#if !MOONLIGHT
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
						if (error != 0) {
							result.Complete (new SocketException ((int) error));
							return;
						}
					} catch (Exception e) {
						result.Complete (e);
						return;
					}
				}

				result.Complete (acc_socket, total);
#endif
			}

			public void Connect ()
			{
				if (result.EndPoint == null) {
					result.Complete (new SocketException ((int)SocketError.AddressNotAvailable));
					return;
				}

				SocketAsyncResult mconnect = result.AsyncState as SocketAsyncResult;
#if !MOONLIGHT
				bool is_mconnect = (mconnect != null && mconnect.Addresses != null);
#else
				if (result.ErrorCode == SocketError.AccessDenied) {
					result.Complete ();
					result.DoMConnectCallback ();
					return;
				}
				bool is_mconnect = false;
#endif
				try {
					int error_code;
					EndPoint ep = result.EndPoint;
					error_code = (int) result.Sock.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
					if (error_code == 0) {
						if (is_mconnect)
							result = mconnect;
						result.Sock.seed_endpoint = ep;
						result.Sock.connected = true;
						result.Sock.isbound = true;
						result.Sock.connect_in_progress = false;
						result.error = 0;
						result.Complete ();
						if (is_mconnect)
							result.DoMConnectCallback ();
						return;
					}

					if (!is_mconnect) {
						result.Sock.connect_in_progress = false;
						result.Complete (new SocketException (error_code));
						return;
					}

					if (mconnect.CurrentAddress >= mconnect.Addresses.Length) {
						mconnect.Complete (new SocketException (error_code));
						if (is_mconnect)
							mconnect.DoMConnectCallback ();
						return;
					}
					mconnect.Sock.BeginMConnect (mconnect);
				} catch (Exception e) {
					result.Sock.connect_in_progress = false;
					if (is_mconnect)
						result = mconnect;
					result.Complete (e);
					if (is_mconnect)
						result.DoMConnectCallback ();
					return;
				}
			}

			/* Also only used in 2.0 profile and newer */
			public void Disconnect ()
			{
#if !MOONLIGHT
				try {
					if (args != null)
						result.ReuseSocket = args.DisconnectReuseSocket;
					result.Sock.Disconnect (result.ReuseSocket);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete ();
#endif
			}

			public void Receive ()
			{
				if (result.operation == SocketOperation.ReceiveGeneric) {
					ReceiveGeneric ();
					return;
				}
				// Actual recv() done in the runtime
				result.Complete ();
			}

			public void ReceiveFrom ()
			{
#if !MOONLIGHT
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
#endif
			}

			public void ReceiveGeneric ()
			{
				int total = 0;
				try {
					total = result.Sock.Receive (result.Buffers, result.SockFlags);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete (total);
			}

			void UpdateSendValues (int last_sent)
			{
				if (result.error == 0) {
					result.Offset += last_sent;
					result.Size -= last_sent;
				}
			}

			public void Send ()
			{
				if (result.operation == SocketOperation.SendGeneric) {
					SendGeneric ();
					return;
				}
				// Actual send() done in the runtime
				if (result.error == 0) {
					UpdateSendValues (result.Total);
					if (result.Sock.disposed) {
						result.Complete ();
						return;
					}

					if (result.Size > 0) {
						Socket.socket_pool_queue (Worker.Dispatcher, result);
						return; // Have to finish writing everything. See bug #74475.
					}
				}
				result.Complete ();
			}

			public void SendTo ()
			{
#if !MOONLIGHT
				int total = 0;
				try {
					total = result.Sock.SendTo_nochecks (result.Buffer,
								    result.Offset,
								    result.Size,
								    result.SockFlags,
								    result.EndPoint);

					UpdateSendValues (total);
					if (result.Size > 0) {
						Socket.socket_pool_queue (Worker.Dispatcher, result);
						return; // Have to finish writing everything. See bug #74475.
					}
					result.Total = total;
				} catch (Exception e) {
					result.Complete (e);
					return;
				}

				result.Complete ();
#endif
			}

			public void SendGeneric ()
			{
				int total = 0;
				try {
					total = result.Sock.Send (result.Buffers, result.SockFlags);
				} catch (Exception e) {
					result.Complete (e);
					return;
				}
				result.Complete (total);
			}
		}

		private Queue readQ = new Queue (2);
		private Queue writeQ = new Queue (2);

		internal delegate void SocketAsyncCall (SocketAsyncResult sar);

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
#if CONFIGURATION_DEP
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
		private IntPtr socket;
		private AddressFamily address_family;
		private SocketType socket_type;
		private ProtocolType protocol_type;
		internal bool blocking=true;
		Thread blocking_thread;
		private bool isbound;
		/* When true, the socket was connected at the time of
		 * the last IO operation
		 */
		private bool connected;
		/* true if we called Close_internal */
		private bool closed;
		internal bool disposed;
		bool connect_in_progress;

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
			
			socket = Socket_internal (addressFamily, socketType, protocolType, out error);
			if (error != 0)
				throw new SocketException (error);
#if !NET_2_1 || MOBILE
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

		public EndPoint RemoteEndPoint {
			get {
				if (disposed && closed)
					throw new ObjectDisposedException (GetType ().ToString ());
				
#if MOONLIGHT
				if (!connected)
					return seed_endpoint;
#else
				/*
				 * If the seed EndPoint is null, Connect, Bind,
				 * etc has not yet been called. MS returns null
				 * in this case.
				 */
				if (!connected || seed_endpoint == null)
					return null;
#endif			
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

		protected virtual void Dispose (bool disposing)
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

#if NET_2_1 || NET_4_0
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
#else
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
				socket_pool_queue (Worker.Dispatcher, res);
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
				socket_pool_queue (Worker.Dispatcher, res);
			}
			return true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern static bool Poll_internal (IntPtr socket, SelectMode mode, int timeout, out int error);

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
#if !NET_2_1 || MOBILE
			if (protocol_type == ProtocolType.Udp)
				throw new SocketException ((int)SocketError.ProtocolOption);
#endif
		}

#if !MOONLIGHT
		public
#endif
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
				Close_internal (socket, out error);
				socket = Socket_internal (address_family, socket_type, protocol_type, out error);
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
			socket_pool_queue (Worker.Dispatcher, req);
			return req;
		}

#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
			if (islistening)
				throw new InvalidOperationException ();
#endif

			SocketAsyncResult req = new SocketAsyncResult (this, state, callback, SocketOperation.Connect);
			req.Addresses = addresses;
			req.Port = port;
			connected = false;
			return BeginMConnect (req);
		}

		IAsyncResult BeginMConnect (SocketAsyncResult req)
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
#if MOONLIGHT || NET_4_0
			// Connect to the first address that match the host name, like:
			// http://blogs.msdn.com/ncl/archive/2009/07/20/new-ncl-features-in-net-4-0-beta-2.aspx
			// while skipping entries that do not match the address family
			DnsEndPoint dep = (e.RemoteEndPoint as DnsEndPoint);
			if (dep != null) {
				addresses = Dns.GetHostAddresses (dep.Host);
				IPEndPoint endpoint;
#if MOONLIGHT && !INSIDE_SYSTEM
				if (!e.PolicyRestricted && !SecurityManager.HasElevatedPermissions) {
					List<IPAddress> valid = new List<IPAddress> ();
					foreach (IPAddress a in addresses) {
						// if we're not downloading a socket policy then check the policy
						// and if we're not running with elevated permissions (SL4 OoB option)
						endpoint = new IPEndPoint (a, dep.Port);
						if (!CrossDomainPolicyManager.CheckEndPoint (endpoint, e.SocketClientAccessPolicyProtocol))
							continue;
						valid.Add (a);
					}
					if (valid.Count == 0)
		 				e.SocketError = SocketError.AccessDenied;
					addresses = valid.ToArray ();
				}
#endif
				return true;
			} else {
				e.ConnectByNameError = null;
#if MOONLIGHT && !INSIDE_SYSTEM
				if (!e.PolicyRestricted && !SecurityManager.HasElevatedPermissions) {
					if (CrossDomainPolicyManager.CheckEndPoint (e.RemoteEndPoint, e.SocketClientAccessPolicyProtocol))
						return false;
		 			else
						e.SocketError = SocketError.AccessDenied;
				} else
#endif
					return false;
			}
			return true; // do not use remote endpoint
#else
			return false; // < NET_4_0 -> use remote endpoint
#endif
		}

		bool ConnectAsyncReal (SocketAsyncEventArgs e)
		{
			IPAddress [] addresses = null;
			bool use_remoteep = true;
#if MOONLIGHT || NET_4_0
			use_remoteep = !GetCheckedIPs (e, out addresses);
			bool policy_failed = (e.SocketError == SocketError.AccessDenied);
#endif
			e.curSocket = this;
			Worker w = e.Worker;
			w.Init (this, e, SocketOperation.Connect);
			SocketAsyncResult result = w.result;
#if MOONLIGHT
			if (policy_failed) {
				// SocketAsyncEventArgs.Completed must be called
				connected = false;
				result.EndPoint = e.RemoteEndPoint;
				result.error = (int) SocketError.AccessDenied;
				result.Complete ();
				socket_pool_queue (Worker.Dispatcher, result);
				return true;
			}
#endif
			IAsyncResult ares = null;
			try {
				if (use_remoteep) {
					result.EndPoint = e.RemoteEndPoint;
					ares = BeginConnect (e.RemoteEndPoint, SocketAsyncEventArgs.Dispatcher, e);
				}
#if MOONLIGHT || NET_4_0
				else {

					DnsEndPoint dep = (e.RemoteEndPoint as DnsEndPoint);
					result.Addresses = addresses;
					result.Port = dep.Port;

					ares = BeginConnect (addresses, dep.Port, SocketAsyncEventArgs.Dispatcher, e);
				}
#endif
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

#if !MOONLIGHT
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
#endif
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

			// connected, not yet connected or even policy denied, the Socket.RemoteEndPoint is always 
			// available after the ConnectAsync call
			seed_endpoint = e.RemoteEndPoint;
			return ConnectAsyncReal (e);
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
			return s.ConnectAsyncReal (e);
		}

		public static void CancelConnectAsync (SocketAsyncEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException ("e");

			// FIXME: this is canceling a synchronous connect, not an async one
			Socket s = e.ConnectSocket;
			if ((s != null) && (s.blocking_thread != null))
				s.blocking_thread.Abort ();
		}
#endif
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static int Receive_internal (IntPtr sock, WSABUF[] bufarray, SocketFlags flags, out int error);
#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
		public
#else
		internal
#endif
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

#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
		public
#else
		internal
#endif
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

#if !MOONLIGHT
		public
#else
		internal
#endif
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

#if !MOONLIGHT
		public
#else
		internal
#endif
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

#if !MOONLIGHT
		public
#else
		internal
#endif
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

#if !MOONLIGHT
		public
#else
		internal
#endif
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
#if !MOONLIGHT
		public
#else
		internal
#endif
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
		static extern void socket_pool_queue (SocketAsyncCall d, SocketAsyncResult r);
	}
}

