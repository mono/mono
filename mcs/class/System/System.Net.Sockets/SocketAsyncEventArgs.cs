// System.Net.Sockets.SocketAsyncEventArgs.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Threading;
#if MOONLIGHT && !INSIDE_SYSTEM
using System.Net.Policy;
#endif

namespace System.Net.Sockets
{
	public class SocketAsyncEventArgs : EventArgs, IDisposable
	{
#if MOONLIGHT || NET_4_0
		public Exception ConnectByNameError { get; internal set; }
#endif

		public event EventHandler<SocketAsyncEventArgs> Completed;

		IList <ArraySegment <byte>> _bufferList;
		
		public Socket AcceptSocket { get; set; }
		public byte[] Buffer { get; private set; }

		[MonoTODO ("not supported in all cases")]
		public IList<ArraySegment<byte>> BufferList {
			get { return _bufferList; }
			set {
				if (Buffer != null && value != null)
					throw new ArgumentException ("Buffer and BufferList properties cannot both be non-null.");
				_bufferList = value;
			}
		}

		public int BytesTransferred { get; private set; }
		public int Count { get; private set; }
		public bool DisconnectReuseSocket { get; set; }
		public SocketAsyncOperation LastOperation { get; private set; }
		public int Offset { get; private set; }
		public EndPoint RemoteEndPoint { get; set; }
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

		[MonoTODO ("Only TCP is currently supported by Moonlight")]
		public SocketClientAccessPolicyProtocol SocketClientAccessPolicyProtocol {
			get { return policy_protocol; }
			set {
				if ((value != SocketClientAccessPolicyProtocol.Tcp) && (value != SocketClientAccessPolicyProtocol.Http))
					throw new ArgumentException ("Invalid value");
				policy_protocol = value;
			}
		}
#endif

		Socket curSocket;
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
			Socket acceptSocket = AcceptSocket;
			if (acceptSocket != null)
				acceptSocket.Close ();

			if (disposing)
				GC.SuppressFinalize (this);
		}		

		public void Dispose ()
		{
			Dispose (true);
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
		void ReceiveCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.Receive;
			SocketError error = SocketError.Success;

			if (!curSocket.Connected) {
				SocketError = SocketError.NotConnected;
				return;
			}
			
			try {
				// FIXME: this does not support using BufferList
				BytesTransferred = curSocket.Receive_nochecks (Buffer, Offset, Count, SocketFlags, out error);
			} finally {
				SocketError = error;
				OnCompleted (this);
			}
		}

		void ConnectCallback ()
		{
			LastOperation = SocketAsyncOperation.Connect;
			SocketError error = SocketError.AccessDenied;
			try {
#if MOONLIGHT || NET_4_0
				// Connect to the first address that match the host name, like:
				// http://blogs.msdn.com/ncl/archive/2009/07/20/new-ncl-features-in-net-4-0-beta-2.aspx
				// while skipping entries that do not match the address family
				DnsEndPoint dep = (RemoteEndPoint as DnsEndPoint);
				if (dep != null) {
					IPAddress[] addresses = Dns.GetHostAddresses (dep.Host);
					foreach (IPAddress addr in addresses) {
						try {
							if (curSocket.AddressFamily == addr.AddressFamily) {
								error = TryConnect (new IPEndPoint (addr, dep.Port));
								if (error == SocketError.Success) {
									ConnectByNameError = null;
									break;
								}
							}
						}
						catch (SocketException se) {
							ConnectByNameError = se;
							error = SocketError.AccessDenied;
						}
					}
				} else {
					ConnectByNameError = null;
					error = TryConnect (RemoteEndPoint);
				}
#else
				error = TryConnect (RemoteEndPoint);
#endif
			} finally {
				SocketError = error;
				OnCompleted (this);
			}
		}

		SocketError TryConnect (EndPoint endpoint)
		{
			curSocket.Connected = false;
			SocketError error = SocketError.Success;
#if MOONLIGHT && !INSIDE_SYSTEM
			// if we're not downloading a socket policy then check the policy
			// and if we're not running with elevated permissions (SL4 OoB option)
			if (!PolicyRestricted && !SecurityManager.HasElevatedPermissions) {
				error = SocketError.AccessDenied;
				if (!CrossDomainPolicyManager.CheckEndPoint (endpoint, policy_protocol)) {
					return error;
				}
				error = SocketError.Success;
			}
#endif
			try {
#if !NET_2_1
				if (!curSocket.Blocking) {
					int success;
					curSocket.Poll (-1, SelectMode.SelectWrite, out success);
					error = (SocketError)success;
					if (success == 0)
						curSocket.Connected = true;
					else
						return error;
				} else
#endif
				{
					curSocket.seed_endpoint = endpoint;
					curSocket.Connect (endpoint);
					curSocket.Connected = true;
				}
			} catch (SocketException se){
				error = se.SocketErrorCode;
			}
			return error;
		}

		void SendCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.Send;
			SocketError error = SocketError.Success;

			if (!curSocket.Connected) {
				SocketError = SocketError.NotConnected;
				return;
			}

			try {
				if (Buffer != null) {
					BytesTransferred = curSocket.Send_nochecks (Buffer, Offset, Count, SocketFlags.None, out error);
				} else if (BufferList != null) {
					BytesTransferred = 0;
					foreach (ArraySegment<byte> asb in BufferList) {
						BytesTransferred += curSocket.Send_nochecks (asb.Array, asb.Offset, asb.Count, 
							SocketFlags.None, out error);
						if (error != SocketError.Success)
							break;
					}
				}
			} finally {
				SocketError = error;
				OnCompleted (this);
			}
		}
#if !NET_2_1
		void AcceptCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.Accept;
			try {
				curSocket.Accept (AcceptSocket);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
				throw;
			} finally {
				OnCompleted (this);
			}
		}

		void DisconnectCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.Disconnect;

			try {
				curSocket.Disconnect (DisconnectReuseSocket);
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
				throw;
			} finally {
				OnCompleted (this);
			}
		}

		void ReceiveFromCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.ReceiveFrom;

			try {
				EndPoint ep = RemoteEndPoint;
				if (Buffer != null) {
					BytesTransferred = curSocket.ReceiveFrom_nochecks (Buffer, Offset, Count, SocketFlags, ref ep);
				} else if (BufferList != null) {
					throw new NotImplementedException ();
				}
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
				throw;
			} finally {
				OnCompleted (this);
			}
		}

		void SendToCallback ()
		{
			SocketError = SocketError.Success;
			LastOperation = SocketAsyncOperation.SendTo;
			int total = 0;
			
			try {
				int count = Count;

				while (total < count)
					total += curSocket.SendTo_nochecks (Buffer, Offset, count, SocketFlags, RemoteEndPoint);
				BytesTransferred = total;
			} catch (SocketException ex) {
				SocketError = ex.SocketErrorCode;
				throw;
			} finally {
				OnCompleted (this);
			}
		}
#endif
		internal void DoOperation (SocketAsyncOperation operation, Socket socket)
		{
			ThreadStart callback;
			curSocket = socket;
			
			switch (operation) {
#if !NET_2_1
				case SocketAsyncOperation.Accept:
					callback = new ThreadStart (AcceptCallback);
					break;

				case SocketAsyncOperation.Disconnect:
					callback = new ThreadStart (DisconnectCallback);
					break;

				case SocketAsyncOperation.ReceiveFrom:
					callback = new ThreadStart (ReceiveFromCallback);
					break;

				case SocketAsyncOperation.SendTo:
					callback = new ThreadStart (SendToCallback);
					break;
#endif
				case SocketAsyncOperation.Receive:
					callback = new ThreadStart (ReceiveCallback);
					break;

				case SocketAsyncOperation.Connect:
					callback = new ThreadStart (ConnectCallback);
					break;

				case SocketAsyncOperation.Send:
					callback = new ThreadStart (SendCallback);
					break;
				
				default:
					throw new NotSupportedException ();
			}

			Thread t = new Thread (callback);
			t.IsBackground = true;
			t.Start ();
		}
#endregion
	}
}
#endif
