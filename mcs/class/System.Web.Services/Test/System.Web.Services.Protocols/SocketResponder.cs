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
using System.Threading;

namespace MonoTests.System.Web.Services.Protocols
{
	public delegate byte [] SocketRequestHandler (Socket socket);

	public class SocketResponder : IDisposable
	{
		private TcpListener tcpListener;
		private readonly IPEndPoint _localEndPoint;
		private Thread listenThread;
		private SocketRequestHandler _requestHandler;
		private bool _stopped = true;
		private readonly object _syncRoot = new object ();

		private const int SOCKET_CLOSED = 10004;

		public SocketResponder (IPEndPoint localEP, SocketRequestHandler requestHandler)
		{
			_localEndPoint = localEP;
			_requestHandler = requestHandler;
		}

		public IPEndPoint LocalEndPoint
		{
			get { return _localEndPoint; }
		}

		public void Dispose ()
		{
			Stop ();
		}

		public bool IsStopped
		{
			get
			{
				lock (_syncRoot) {
					return _stopped;
				}
			}
		}

		public void Start ()
		{
			lock (_syncRoot) {
				if (!_stopped)
					return;
				_stopped = false;
				listenThread = new Thread (new ThreadStart (Listen));
				listenThread.Start ();
				Thread.Sleep (20); // allow listener to start
			}
		}

		public void Stop ()
		{
			lock (_syncRoot) {
				if (_stopped)
					return;
				_stopped = true;
				if (tcpListener != null) {
					tcpListener.Stop ();
					tcpListener = null;
				}
			}
		}

		private void Listen ()
		{
			tcpListener = new TcpListener (LocalEndPoint);
			tcpListener.Start ();
			try {
				Socket socket = tcpListener.AcceptSocket ();
				socket.Send (_requestHandler (socket));
				socket.Close ();
			} catch (SocketException ex) {
				// ignore interruption of blocking call
				if (ex.ErrorCode != SOCKET_CLOSED)
					throw;
			}
		}
	}
}
