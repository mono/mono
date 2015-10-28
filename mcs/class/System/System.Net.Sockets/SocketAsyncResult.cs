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
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace System.Net.Sockets
{
	[StructLayout (LayoutKind.Sequential)]
	internal sealed class SocketAsyncResult: IOAsyncResult
	{
		public Socket socket;
		public IntPtr handle;
		public SocketOperation operation;

		Exception DelayedException;

		public EndPoint EndPoint;                 // Connect,ReceiveFrom,SendTo
		public byte [] Buffer;                    // Receive,ReceiveFrom,Send,SendTo
		public int Offset;                        // Receive,ReceiveFrom,Send,SendTo
		public int Size;                          // Receive,ReceiveFrom,Send,SendTo
		public SocketFlags SockFlags;             // Receive,ReceiveFrom,Send,SendTo
		public Socket AcceptSocket;               // AcceptReceive
		public IPAddress[] Addresses;             // Connect
		public int Port;                          // Connect
		public IList<ArraySegment<byte>> Buffers; // Receive, Send
		public bool ReuseSocket;                  // Disconnect
		public int CurrentAddress;                // Connect

		// Return values
		Socket accept_socket;
		int total;

		internal int error;

		public int EndCalled;

		public SocketAsyncWorker Worker;

		public SocketAsyncResult ()
			: base (null, null)
		{
		}

		public SocketAsyncResult (Socket socket, AsyncCallback callback, object state, SocketOperation operation)
			: base (callback, state)
		{
			this.socket = socket;
			this.handle = socket != null ? socket.Handle : IntPtr.Zero;
			this.operation = operation;

			Worker = new SocketAsyncWorker (this);
		}

		public SocketAsyncResult (Socket socket, AsyncCallback callback, object state, SocketOperation operation, SocketAsyncWorker worker)
			: base (callback, state)
		{
			this.socket = socket;
			this.handle = socket != null ? socket.Handle : IntPtr.Zero;
			this.operation = operation;

			Worker = worker;
		}

		public Socket Socket {
			get {
				return accept_socket;
			}
		}

		public int Total {
			get { return total; }
			set { total = value; }
		}

		public SocketError ErrorCode {
			get {
				SocketException ex = DelayedException as SocketException;
				if (ex != null)
					return ex.SocketErrorCode;

				if (error != 0)
					return (SocketError) error;

				return SocketError.Success;
			}
		}

		public void Dispose ()
		{
		}

		public void CheckIfThrowDelayedException ()
		{
			if (DelayedException != null) {
				socket.is_connected = false;
				throw DelayedException;
			}

			if (error != 0) {
				socket.is_connected = false;
				throw new SocketException (error);
			}
		}

		internal override void CompleteDisposed ()
		{
			Complete ();
		}

		public void Complete ()
		{
			if (operation != SocketOperation.Receive && socket.is_disposed)
				DelayedException = new ObjectDisposedException (socket.GetType ().ToString ());

			IsCompleted = true;

			AsyncCallback callback = AsyncCallback;
			if (callback != null) {
				ThreadPool.UnsafeQueueUserWorkItem (_ => callback (this), null);
			}

			Queue<KeyValuePair<IntPtr, IOSelectorJob>> queue = null;
			switch (operation) {
			case SocketOperation.Receive:
			case SocketOperation.ReceiveFrom:
			case SocketOperation.ReceiveGeneric:
			case SocketOperation.Accept:
				queue = socket.readQ;
				break;
			case SocketOperation.Send:
			case SocketOperation.SendTo:
			case SocketOperation.SendGeneric:
				queue = socket.writeQ;
				break;
			}

			if (queue != null) {
				lock (queue) {
					/* queue.Count will only be 0 if the socket is closed while receive/send/accept
					 * operation(s) are pending and at least one call to this method is waiting
					 * on the lock while another one calls CompleteAllOnDispose() */
					if (queue.Count > 0)
						queue.Dequeue (); /* remove ourselves */
					if (queue.Count > 0) {
						if (!socket.is_disposed) {
							IOSelector.Add (queue.Peek ().Key, queue.Peek ().Value);
						} else {
							/* CompleteAllOnDispose */
							KeyValuePair<IntPtr, IOSelectorJob> [] jobs = queue.ToArray ();
							for (int i = 0; i < jobs.Length; i++)
								ThreadPool.QueueUserWorkItem (j => ((IOSelectorJob) j).MarkDisposed (), jobs [i].Value);
							queue.Clear ();
						}
					}
				}
			}

			// IMPORTANT: 'callback', if any is scheduled from unmanaged code
		}

		public void Complete (bool synch)
		{
			CompletedSynchronously = synch;
			Complete ();
		}

		public void Complete (int total)
		{
			this.total = total;
			Complete ();
		}

		public void Complete (Exception e, bool synch)
		{
			DelayedException = e;
			CompletedSynchronously = synch;
			Complete ();
		}

		public void Complete (Exception e)
		{
			DelayedException = e;
			Complete ();
		}

		public void Complete (Socket s)
		{
			this.accept_socket = s;
			Complete ();
		}

		public void Complete (Socket s, int total)
		{
			this.accept_socket = s;
			this.total = total;
			Complete ();
		}
	}
}
