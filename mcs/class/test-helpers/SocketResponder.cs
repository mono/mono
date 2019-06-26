//
// SocketResponder.cs - Utility class for tests that require a listener
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2007 Gert Driesen
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonoTests.Helpers
{
	public delegate byte [] SocketRequestHandler (Socket socket);

	public class SocketResponderException : Exception
	{
		public SocketResponderException (string message)
			: base (message)
		{
		}
	}

	public class SocketResponder : IDisposable
	{
		private TcpListener tcpListener;
		private Task listenTask;
		private Socket listenSocket;
		private SocketRequestHandler requestHandler;
		private bool disposed;

		private const int SOCKET_CLOSED = 10004;
		private const int SOCKET_INVALID_ARGS = 10022;

		public SocketResponder (IPEndPoint ep, SocketRequestHandler rh)
		{
			requestHandler = rh;

			tcpListener = new TcpListener (ep);
			tcpListener.Start ();

			listenTask = Task.Run ((Action) Listen);
		}

		// Starts listening on IPAddress.Loopback on a system-assigned port.
		// Returns the resulting IPEndPoint (which contains the assigned port).
		public SocketResponder (out IPEndPoint ep, SocketRequestHandler rh)
			: this (new IPEndPoint (IPAddress.Loopback, 0), rh)
		{
			ep = (IPEndPoint) tcpListener.LocalEndpoint;
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;

			tcpListener.Stop ();

			if (listenSocket != null)
				listenSocket.Close ();

			if (!listenTask.Wait (5000))
				throw new SocketResponderException ("Failed to stop in less than 5 seconds");
		}

		private void Listen ()
		{
			while (!disposed) {
				listenSocket = null;
				try {
					listenSocket = tcpListener.AcceptSocket ();
					listenSocket.Send (requestHandler (listenSocket));
					try {
						// On Windows a Receive() is needed here before Shutdown() to consume the data some tests send.
						listenSocket.ReceiveTimeout = 10 * 1000;
						listenSocket.Receive (new byte [0]);
						listenSocket.Shutdown (SocketShutdown.Send);
						listenSocket.Shutdown (SocketShutdown.Receive);
					} catch {
					}
				} catch (SocketException ex) {
					// ignore interruption of blocking call
					if (ex.ErrorCode != SOCKET_CLOSED && ex.ErrorCode != SOCKET_INVALID_ARGS && !disposed)
						throw;
				} catch (ObjectDisposedException ex) {
					if (!disposed)
						throw;
#if MOBILE
				} catch (InvalidOperationException ex) {
					// This breaks some tests running on Android. The problem is that the stack trace
					// doesn't point to where the exception is actually thrown from but the entire process
					// is aborted because of unhandled exception.
					Console.WriteLine ("SocketResponder.Listen failed:");
					Console.WriteLine (ex);
#endif
				} finally {
					if (listenSocket != null)
						listenSocket.Close ();
				}
			}
		}
	}
}
