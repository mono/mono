// System.Net.Sockets.SocketAsyncEventArgs.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@xamarin.com)
//
// Copyright (c) 2008,2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2011 Xamarin, Inc. (http://xamarin.com)
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
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Threading;

namespace System.Net.Sockets
{
	public class SocketAsyncEventArgs : EventArgs, IDisposable
	{
		bool disposed;

		internal volatile int in_progress;
		internal EndPoint remote_ep;
		internal Socket current_socket;

		internal SocketAsyncResult socket_async_result = new SocketAsyncResult ();

		public Exception ConnectByNameError {
			get;
			internal set;
		}

		public Socket AcceptSocket {
			get;
			set;
		}

		public byte[] Buffer {
			get;
			private set;
		}

		IList <ArraySegment <byte>> _bufferList;
		public IList<ArraySegment<byte>> BufferList {
			get { return _bufferList; }
			set {
				if (Buffer != null && value != null)
					throw new ArgumentException ("Buffer and BufferList properties cannot both be non-null.");
				_bufferList = value;
			}
		}

		public int BytesTransferred {
			get;
			internal set;
		}

		public int Count {
			get;
			internal set;
		}

		public bool DisconnectReuseSocket {
			get;
			set;
		}

		public SocketAsyncOperation LastOperation {
			get;
			private set;
		}

		public int Offset {
			get;
			private set;
		}

		public EndPoint RemoteEndPoint {
			get { return remote_ep; }
			set { remote_ep = value; }
		}

		public IPPacketInformation ReceiveMessageFromPacketInfo {
			get;
			private set;
		}

		public SendPacketsElement[] SendPacketsElements {
			get;
			set;
		}

#if !NET_2_1
		public TransmitFileOptions SendPacketsFlags {
			get;
			set;
		}
#endif

		[MonoTODO ("unused property")]
		public int SendPacketsSendSize {
			get;
			set;
		}

		public SocketError SocketError {
			get;
			set;
		}

		public SocketFlags SocketFlags {
			get;
			set;
		}

		public object UserToken {
			get;
			set;
		}

		public Socket ConnectSocket {
			get {
				switch (SocketError) {
				case SocketError.AccessDenied:
					return null;
				default:
					return current_socket;
				}
			}
		}

		internal bool PolicyRestricted {
			get;
			private set;
		}

		public event EventHandler<SocketAsyncEventArgs> Completed;

		internal SocketAsyncEventArgs (bool policy)
			: this ()
		{
			PolicyRestricted = policy;
		}

		public SocketAsyncEventArgs ()
		{
			SendPacketsSendSize = -1;
		}

		~SocketAsyncEventArgs ()
		{
			Dispose (false);
		}

		void Dispose (bool disposing)
		{
			disposed = true;

			if (disposing && in_progress != 0)
				return;

			AcceptSocket = null;
			Buffer = null;
			BufferList = null;
			RemoteEndPoint = null;
			UserToken = null;
			SendPacketsElements = null;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		internal void SetLastOperation (SocketAsyncOperation op)
		{
			if (disposed)
				throw new ObjectDisposedException ("System.Net.Sockets.SocketAsyncEventArgs");
			if (Interlocked.Exchange (ref in_progress, 1) != 0)
				throw new InvalidOperationException ("Operation already in progress");

			LastOperation = op;
		}

		internal void Complete ()
		{
			OnCompleted (this);
		}

		protected virtual void OnCompleted (SocketAsyncEventArgs e)
		{
			if (e == null)
				return;
			
			EventHandler<SocketAsyncEventArgs> handler = e.Completed;
			if (handler != null)
				handler (e.current_socket, e);
		}

		public void SetBuffer (int offset, int count)
		{
			SetBuffer (Buffer, offset, count);
		}

		public void SetBuffer (byte[] buffer, int offset, int count)
		{
			if (buffer != null) {
				if (BufferList != null)
					throw new ArgumentException ("Buffer and BufferList properties cannot both be non-null.");
				
				int buflen = buffer.Length;
				if (offset < 0 || (offset != 0 && offset >= buflen))
					throw new ArgumentOutOfRangeException ("offset");

				if (count < 0 || count > buflen - offset)
					throw new ArgumentOutOfRangeException ("count");

				Count = count;
				Offset = offset;
			}

			Buffer = buffer;
		}
	}
}
