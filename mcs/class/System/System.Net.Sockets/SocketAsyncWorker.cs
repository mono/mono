// System.Net.Sockets.SocketAsyncWorker.cs
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

namespace System.Net.Sockets
{
	internal sealed class SocketAsyncWorker
	{
		public SocketAsyncResult result;
		SocketAsyncEventArgs args;

		public SocketAsyncWorker (SocketAsyncEventArgs args)
		{
			this.args = args;
			result = new SocketAsyncResult ();
			result.Worker = this;
		}

		public SocketAsyncWorker (SocketAsyncResult ares)
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

		public static SocketAsyncCallback Dispatcher = new SocketAsyncCallback (DispatcherCB);

		static void DispatcherCB (SocketAsyncResult sar)
		{
			/* SendPackets and ReceiveMessageFrom are not implemented yet */
			switch (sar.operation) {
			case SocketOperation.Receive:
			case SocketOperation.ReceiveGeneric:
			case SocketOperation.RecvJustCallback:
				sar.Worker.Receive ();
				break;
			case SocketOperation.Send:
			case SocketOperation.SendGeneric:
			case SocketOperation.SendJustCallback:
				sar.Worker.Send ();
				break;
			case SocketOperation.ReceiveFrom:
				sar.Worker.ReceiveFrom ();
				break;
			case SocketOperation.SendTo:
				sar.Worker.SendTo ();
				break;
			case SocketOperation.Connect:
				sar.Worker.Connect ();
				break;
			case SocketOperation.Accept:
				sar.Worker.Accept ();
				break;
			case SocketOperation.AcceptReceive:
				sar.Worker.AcceptReceive ();
				break;
			case SocketOperation.Disconnect:
				sar.Worker.Disconnect ();
				break;
			// case SocketOperation.ReceiveMessageFrom
			// 	sar.Worker.ReceiveMessageFrom ()
			// 	break;
			// case SocketOperation.SendPackets:
			// 	sar.Worker.SendPackets ();
			// 	break;
			default:
				throw new NotImplementedException (String.Format ("Operation {0} is not implemented", sar.operation));
			}
		}

		/* This is called when reusing a SocketAsyncEventArgs */
		public void Init (Socket sock, SocketAsyncEventArgs args, SocketOperation op)
		{
			result.Init (sock, args, SocketAsyncEventArgs.Dispatcher, op, this);

			SocketAsyncOperation async_op;

			// Notes;
			// 	-SocketOperation.AcceptReceive not used in SocketAsyncEventArgs
			//	-SendPackets and ReceiveMessageFrom are not implemented yet
			switch (op) {
			case SocketOperation.Connect:
				async_op = SocketAsyncOperation.Connect;
				break;
			case SocketOperation.Accept:
				async_op = SocketAsyncOperation.Accept;
				break;
			case SocketOperation.Disconnect:
				async_op = SocketAsyncOperation.Disconnect;
				break;
			case SocketOperation.Receive:
			case SocketOperation.ReceiveGeneric:
				async_op = SocketAsyncOperation.Receive;
				break;
			case SocketOperation.ReceiveFrom:
				async_op = SocketAsyncOperation.ReceiveFrom;
				break;
			// case SocketOperation.ReceiveMessageFrom:
			// 	async_op = SocketAsyncOperation.ReceiveMessageFrom;
			// 	break;
			case SocketOperation.Send:
			case SocketOperation.SendGeneric:
				async_op = SocketAsyncOperation.Send;
				break;
			// case SocketOperation.SendPackets:
			// 	async_op = SocketAsyncOperation.SendPackets;
			// 	break;
			case SocketOperation.SendTo:
				async_op = SocketAsyncOperation.SendTo;
				break;
			default:
				throw new NotImplementedException (String.Format ("Operation {0} is not implemented", op));
			}

			args.SetLastOperation (async_op);
			args.SocketError = SocketError.Success;
			args.BytesTransferred = 0;
		}

		public void Accept ()
		{
			Socket acc_socket = null;
			try {
				if (args != null && args.AcceptSocket != null) {
					result.socket.Accept (args.AcceptSocket);
					acc_socket = args.AcceptSocket;
				} else {
					acc_socket = result.socket.Accept ();
					if (args != null)
						args.AcceptSocket = acc_socket;
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
					acc_socket = result.socket.Accept ();
				} else {
					acc_socket = result.AcceptSocket;
					result.socket.Accept (acc_socket);
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
					total = acc_socket.Receive_nochecks (result.Buffer, result.Offset, result.Size, result.SockFlags, out error);
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
		}

		public void Connect ()
		{
			if (result.EndPoint == null) {
				result.Complete (new SocketException ((int)SocketError.AddressNotAvailable));
				return;
			}

			SocketAsyncResult mconnect = result.AsyncState as SocketAsyncResult;
			bool is_mconnect = (mconnect != null && mconnect.Addresses != null);
			try {
				int error_code;
				EndPoint ep = result.EndPoint;
				error_code = (int) result.socket.GetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Error);
				if (error_code == 0) {
					if (is_mconnect)
						result = mconnect;
					result.socket.seed_endpoint = ep;
					result.socket.is_connected = true;
					result.socket.is_bound = true;
					result.socket.connect_in_progress = false;
					result.error = 0;
					result.Complete ();
					if (is_mconnect)
						result.DoMConnectCallback ();
					return;
				}

				if (!is_mconnect) {
					result.socket.connect_in_progress = false;
					result.Complete (new SocketException (error_code));
					return;
				}

				if (mconnect.CurrentAddress >= mconnect.Addresses.Length) {
					mconnect.Complete (new SocketException (error_code));
					if (is_mconnect)
						mconnect.DoMConnectCallback ();
					return;
				}
				mconnect.socket.BeginMConnect (mconnect);
			} catch (Exception e) {
				result.socket.connect_in_progress = false;
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
			try {
				if (args != null)
					result.ReuseSocket = args.DisconnectReuseSocket;
				result.socket.Disconnect (result.ReuseSocket);
			} catch (Exception e) {
				result.Complete (e);
				return;
			}
			result.Complete ();
		}

		public void Receive ()
		{
			if (result.operation == SocketOperation.ReceiveGeneric) {
				ReceiveGeneric ();
				return;
			}

			int total = 0;
			try {
				total = Socket.Receive_internal (result.socket.safe_handle, result.Buffer, result.Offset, result.Size, result.SockFlags, out result.error);
			} catch (Exception e) {
				result.Complete (e);
				return;
			}

			result.Complete (total);
		}

		public void ReceiveFrom ()
		{
			int total = 0;
			try {
				total = result.socket.ReceiveFrom_nochecks (result.Buffer, result.Offset, result.Size, result.SockFlags, ref result.EndPoint);
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
				total = result.socket.Receive (result.Buffers, result.SockFlags);
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
			if (result.operation == SocketOperation.SendGeneric) {
				SendGeneric ();
				return;
			}

			int total = 0;
			try {
				total = Socket.Send_internal (result.socket.safe_handle, result.Buffer, result.Offset, result.Size, result.SockFlags, out result.error);
			} catch (Exception e) {
				result.Complete (e);
				return;
			}

			if (result.error == 0) {
				UpdateSendValues (total);
				if (result.socket.is_disposed) {
					result.Complete (total);
					return;
				}

				if (result.Size > 0) {
					Socket.socket_pool_queue (SocketAsyncWorker.Dispatcher, result);
					return; // Have to finish writing everything. See bug #74475.
				}
				result.Total = send_so_far;
				send_so_far = 0;
			}
			result.Complete (total);
		}

		public void SendTo ()
		{
			int total = 0;
			try {
				total = result.socket.SendTo_nochecks (result.Buffer, result.Offset, result.Size, result.SockFlags, result.EndPoint);

				UpdateSendValues (total);
				if (result.Size > 0) {
					Socket.socket_pool_queue (SocketAsyncWorker.Dispatcher, result);
					return; // Have to finish writing everything. See bug #74475.
				}
				result.Total = send_so_far;
				send_so_far = 0;
			} catch (Exception e) {
				send_so_far = 0;
				result.Complete (e);
				return;
			}

			result.Complete ();
		}

		public void SendGeneric ()
		{
			int total = 0;
			try {
				total = result.socket.Send (result.Buffers, result.SockFlags);
			} catch (Exception e) {
				result.Complete (e);
				return;
			}
			result.Complete (total);
		}
	}
}
