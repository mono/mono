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
		internal int in_progress;
		internal SocketAsyncWorker Worker;
		EndPoint remote_ep;
		public Exception ConnectByNameError { get; internal set; }

		public event EventHandler<SocketAsyncEventArgs> Completed;

		IList <ArraySegment <byte>> _bufferList;
		
		public Socket AcceptSocket { get; set; }
		public byte[] Buffer { get; private set; }

		public IList<ArraySegment<byte>> BufferList {
			get { return _bufferList; }
			set {
				if (Buffer != null && value != null)
					throw new ArgumentException ("Buffer and BufferList properties cannot both be non-null.");
				_bufferList = value;
			}
		}

		public int BytesTransferred { get; internal set; }
		public int Count { get; internal set; }
		public bool DisconnectReuseSocket { get; set; }
		public SocketAsyncOperation LastOperation { get; private set; }
		public int Offset { get; private set; }
		public EndPoint RemoteEndPoint {
			get { return remote_ep; }
			set { remote_ep = value; }
		}
#if !NET_2_1
		public IPPacketInformation ReceiveMessageFromPacketInfo { get; private set; }
		public SendPacketsElement[] SendPacketsElements { get; set; }
		public TransmitFileOptions SendPacketsFlags { get; set; }
#endif
		[MonoTODO ("unused property")]
		public int SendPacketsSendSize { get; set; }
		public SocketError SocketError { get; set; }
		public SocketFlags SocketFlags { get; set; }
		public object UserToken { get; set; }
		internal Socket curSocket;
		public Socket ConnectSocket {
			get {
				switch (SocketError) {
				case SocketError.AccessDenied:
					return null;
				default:
					return curSocket;
				}
			}
		}

		internal bool PolicyRestricted { get; private set; }

		internal SocketAsyncEventArgs (bool policy) : 
			this ()
		{
			PolicyRestricted = policy;
		}
		
		public SocketAsyncEventArgs ()
		{
			Worker = new SocketAsyncWorker (this);
			AcceptSocket = null;
			Buffer = null;
			BufferList = null;
			BytesTransferred = 0;
			Count = 0;
			DisconnectReuseSocket = false;
			LastOperation = SocketAsyncOperation.None;
			Offset = 0;
			RemoteEndPoint = null;
#if !NET_2_1
			SendPacketsElements = null;
			SendPacketsFlags = TransmitFileOptions.UseDefaultWorkerThread;
#endif
			SendPacketsSendSize = -1;
			SocketError = SocketError.Success;
			SocketFlags = SocketFlags.None;
			UserToken = null;
		}

		~SocketAsyncEventArgs ()
		{
			Dispose (false);
		}

		void Dispose (bool disposing)
		{
			disposed = true;

			if (disposing) {
				if (disposed || Interlocked.CompareExchange (ref in_progress, 0, 0) != 0)
					return;
				if (Worker != null) {
					Worker.Dispose ();
					Worker = null;
				}
			}
			AcceptSocket = null;
			Buffer = null;
			BufferList = null;
			RemoteEndPoint = null;
			UserToken = null;
#if !NET_2_1
			SendPacketsElements = null;
#endif
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

		protected virtual void OnCompleted (SocketAsyncEventArgs e)
		{
			if (e == null)
				return;
			
			EventHandler<SocketAsyncEventArgs> handler = e.Completed;
			if (handler != null)
				handler (e.curSocket, e);
		}

		public void SetBuffer (int offset, int count)
		{
			SetBufferInternal (Buffer, offset, count);
		}

		public void SetBuffer (byte[] buffer, int offset, int count)
		{
			SetBufferInternal (buffer, offset, count);
		}

		void SetBufferInternal (byte[] buffer, int offset, int count)
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

#region Internals

		internal void ReceiveCallback (IAsyncResult ares)
		{
			try {
				BytesTransferred = curSocket.EndReceive (ares);
			} catch (SocketException se){
				SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}

		internal void ConnectCallback (IAsyncResult ares)
		{
			try {
				curSocket.EndConnect (ares);
 			} catch (SocketException se) {
				SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}

		internal void SendCallback (IAsyncResult ares)
		{
			try {
				BytesTransferred = curSocket.EndSend (ares);
			} catch (SocketException se){
				SocketError = se.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}

		internal void AcceptCallback (IAsyncResult ares)
		{
			try {
				AcceptSocket = curSocket.EndAccept (ares);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				if (AcceptSocket == null)
					AcceptSocket = new Socket (curSocket.AddressFamily, curSocket.SocketType, curSocket.ProtocolType, null);
				OnCompleted (this);
			}
		}

		internal void DisconnectCallback (IAsyncResult ares)
		{
			try {
				curSocket.EndDisconnect (ares);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}

		internal void ReceiveFromCallback (IAsyncResult ares)
		{
			try {
				BytesTransferred = curSocket.EndReceiveFrom (ares, ref remote_ep);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}

		internal void SendToCallback (IAsyncResult ares)
		{
			try {
				BytesTransferred = curSocket.EndSendTo (ares);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
			} catch (ObjectDisposedException) {
				SocketError = SocketError.OperationAborted;
			} finally {
				OnCompleted (this);
			}
		}
#endregion
	}
}
