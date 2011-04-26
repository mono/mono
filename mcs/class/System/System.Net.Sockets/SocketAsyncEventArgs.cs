// System.Net.Sockets.SocketAsyncEventArgs.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2008,2010 Novell, Inc. (http://www.novell.com)
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
		internal Socket.Worker Worker;
		EndPoint remote_ep;
#if MOONLIGHT || NET_4_0
		public Exception ConnectByNameError { get; internal set; }
#endif

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
		public int Count { get; private set; }
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

#if MOONLIGHT && !INSIDE_SYSTEM
		private SocketClientAccessPolicyProtocol policy_protocol;

		public SocketClientAccessPolicyProtocol SocketClientAccessPolicyProtocol {
			get { return policy_protocol; }
			set {
				if ((value != SocketClientAccessPolicyProtocol.Tcp) && (value != SocketClientAccessPolicyProtocol.Http))
					throw new ArgumentException ("Invalid value");
				policy_protocol = value;
			}
		}
#endif

		internal Socket curSocket;
#if NET_2_1
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
#endif
		
		public SocketAsyncEventArgs ()
		{
			Worker = new Socket.Worker (this);
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

#if MOONLIGHT && !INSIDE_SYSTEM
			policy_protocol = SocketClientAccessPolicyProtocol.Tcp;
#endif
		}

		~SocketAsyncEventArgs ()
		{
			Dispose (false);
		}

		void Dispose (bool disposing)
		{
			if (disposing) {
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
		internal static AsyncCallback Dispatcher = new AsyncCallback (DispatcherCB);

		static void DispatcherCB (IAsyncResult ares)
		{
			SocketAsyncEventArgs args = (SocketAsyncEventArgs) ares.AsyncState;
			SocketAsyncOperation op = args.LastOperation;
			// Notes;
			// 	-SocketOperation.AcceptReceive not used in SocketAsyncEventArgs
			//	-SendPackets and ReceiveMessageFrom are not implemented yet
			if (op == SocketAsyncOperation.Receive)
				args.ReceiveCallback (ares);
			else if (op == SocketAsyncOperation.Send)
				args.SendCallback (ares);
#if !MOONLIGHT
			else if (op == SocketAsyncOperation.ReceiveFrom)
				args.ReceiveFromCallback (ares);
			else if (op == SocketAsyncOperation.SendTo)
				args.SendToCallback (ares);
			else if (op == SocketAsyncOperation.Accept)
				args.AcceptCallback (ares);
			else if (op == SocketAsyncOperation.Disconnect)
				args.DisconnectCallback (ares);
#endif
			else if (op == SocketAsyncOperation.Connect)
				args.ConnectCallback ();
			/*
			else if (op == Socket.SocketOperation.ReceiveMessageFrom)
			else if (op == Socket.SocketOperation.SendPackets)
			*/
			else
				throw new NotImplementedException (String.Format ("Operation {0} is not implemented", op));

		}

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

		void ConnectCallback ()
		{
			try {
				SocketError = (SocketError) Worker.result.error;
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

#if !MOONLIGHT
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
					AcceptSocket = new Socket (curSocket.AddressFamily, curSocket.SocketType, curSocket.ProtocolType, (IntPtr)(-1));
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

#endif
#endregion
	}
}
