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
		public SocketOperation operation;

		Exception DelayedException;

		public EndPoint EndPoint;                 // Connect,ReceiveFrom,SendTo
		public Memory<byte> Buffer;               // Receive,ReceiveFrom,Send,SendTo
		public int Offset;                        // Receive,ReceiveFrom,Send,SendTo
		public int Size;                          // Receive,ReceiveFrom,Send,SendTo
		public SocketFlags SockFlags;             // Receive,ReceiveFrom,Send,SendTo
		public Socket AcceptSocket;               // AcceptReceive
		public IPAddress[] Addresses;             // Connect
		public int Port;                          // Connect
		public IList<ArraySegment<byte>> Buffers; // Receive, Send
		public bool ReuseSocket;                  // Disconnect
		public int CurrentAddress;                // Connect

		public Socket AcceptedSocket;
		public int Total;

		internal int error;

		public int EndCalled;

		public IntPtr Handle {
			get { return socket != null ? socket.Handle : IntPtr.Zero; }
		}

		/* Used by SocketAsyncEventArgs */
		public SocketAsyncResult ()
			: base ()
		{
		}

		public void Init (Socket socket, AsyncCallback callback, object state, SocketOperation operation)
		{
			base.Init (callback, state);

			this.socket = socket;
			this.operation = operation;

			DelayedException = null;

			EndPoint = null;
			Buffer = null;
			Offset = 0;
			Size = 0;
			SockFlags = SocketFlags.None;
			AcceptSocket = null;
			Addresses = null;
			Port = 0;
			Buffers = null;
			ReuseSocket = false;
			CurrentAddress = 0;

			AcceptedSocket = null;
			Total = 0;

			error = 0;

			EndCalled = 0;
		}

		public SocketAsyncResult (Socket socket, AsyncCallback callback, object state, SocketOperation operation)
			: base (callback, state)
		{
			this.socket = socket;
			this.operation = operation;
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
			if (operation != SocketOperation.Receive && socket.CleanedUp)
				DelayedException = new ObjectDisposedException (socket.GetType ().ToString ());

			IsCompleted = true;

			/* It is possible that this.socket is modified by this.Init which has been called by the callback. This
			 * would lead to inconsistency, as we would for example not release the correct socket.ReadSem or
			 * socket.WriteSem.
			 * For example, this can happen with AcceptAsync followed by a ReceiveAsync on the same
			 * SocketAsyncEventArgs */
			Socket completedSocket = socket;
			SocketOperation completedOperation = operation;

			if (!CompletedSynchronously && AsyncCallback != null) {
				ThreadPool.UnsafeQueueUserWorkItem(state => ((SocketAsyncResult)state).AsyncCallback((SocketAsyncResult)state), this);
			}

			/* Warning: any field on the current SocketAsyncResult might have changed, as the callback might have
			 * called this.Init */

			switch (completedOperation) {
			case SocketOperation.Receive:
			case SocketOperation.ReceiveFrom:
			case SocketOperation.ReceiveGeneric:
			case SocketOperation.Accept:
				completedSocket.ReadSem.Release ();
				break;
			case SocketOperation.Send:
			case SocketOperation.SendTo:
			case SocketOperation.SendGeneric:
				completedSocket.WriteSem.Release ();
				break;
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
			Total = total;
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
			AcceptedSocket = s;
			Complete ();
		}

		public void Complete (Socket s, int total)
		{
			AcceptedSocket = s;
			Total = total;
			Complete ();
		}
	}
}
