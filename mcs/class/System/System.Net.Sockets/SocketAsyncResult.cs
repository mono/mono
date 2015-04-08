// System.Net.Sockets.SocketAsyncResult.cs
//
// Authors:
//	Ludovic Henry <ludovic@xamarin.com>
//
// Copyright (C) 2015 Xamarin, Inc. (https://www.xamarin.com)
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

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Net.Sockets
{
	[StructLayout (LayoutKind.Sequential)]
	internal sealed class SocketAsyncResult: IAsyncResult
	{
		/* Same structure in the runtime. Keep this in sync with
		 * MonoSocketAsyncResult in metadata/socket-io.h and
		 * ProcessAsyncReader in System.Diagnostics/Process.cs. */

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
		public SocketAsyncWorker Worker;
		public int CurrentAddress; // Connect

		public SocketAsyncResult ()
		{
		}

		public void Init (Socket sock, object state, AsyncCallback callback, SocketOperation operation)
		{
			this.Sock = sock;
			if (sock != null) {
				this.blocking = sock.blocking;
				this.handle = sock.Handle;
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
			ThreadPool.UnsafeQueueUserWorkItem (_ => { callback (this); }, null);
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
			this.handle = sock.Handle;
			this.state = state;
			this.callback = callback;
			GC.KeepAlive (this.callback);
			this.operation = operation;
			SockFlags = SocketFlags.None;
			Worker = new SocketAsyncWorker (this);
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
				SocketAsyncWorker worker = (SocketAsyncWorker) pending [i];
				SocketAsyncResult ares = worker.result;
				cb = new WaitCallback (ares.CompleteDisposed);
				ThreadPool.UnsafeQueueUserWorkItem (cb, null);
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
			    operation == SocketOperation.ReceiveGeneric ||
			    operation == SocketOperation.Accept) {
				queue = Sock.readQ;
			} else if (operation == SocketOperation.Send ||
				   operation == SocketOperation.SendTo ||
				   operation == SocketOperation.SendGeneric) {

				queue = Sock.writeQ;
			}

			if (queue != null) {
				SocketAsyncWorker worker = null;
				Socket.SocketAsyncCall sac = null;
				lock (queue) {
					// queue.Count will only be 0 if the socket is closed while receive/send/accept
					// operation(s) are pending and at least one call to this method is
					// waiting on the lock while another one calls CompleteAllOnDispose()
					if (queue.Count > 0)
						queue.Dequeue (); // remove ourselves
					if (queue.Count > 0) {
						worker = (SocketAsyncWorker) queue.Peek ();
						if (!Sock.disposed) {
							sac = SocketAsyncWorker.Dispatcher;
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
}
