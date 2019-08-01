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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Threading;

namespace System.Net.Sockets
{
	public partial class SocketAsyncEventArgs : EventArgs, IDisposable
	{
		bool disposed;

		internal volatile int in_progress;

		internal SocketAsyncResult socket_async_result = new SocketAsyncResult ();

#region Mono-specific settors for private fields

		internal void SetConnectByNameError (Exception error)
		{
			_connectByNameError = error;
		}

		internal void SetBytesTransferred (int value)
		{
			_bytesTransferred = value;
		}

		internal Socket CurrentSocket {
			get { return _currentSocket; }
		}

		internal void SetCurrentSocket (Socket socket)
		{
			_currentSocket = socket;
		}

#endregion

		public Socket ConnectSocket {
			get {
				switch (SocketError) {
				case SocketError.AccessDenied:
					return null;
				default:
					return _currentSocket;
				}
			}
		}

		public event EventHandler<SocketAsyncEventArgs> Completed;

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

			_completedOperation = op;
		}

		internal void Complete_internal ()
		{
			in_progress = 0;
			OnCompleted (this);
		}

		protected virtual void OnCompleted (SocketAsyncEventArgs e)
		{
			if (e == null)
				return;
			
			EventHandler<SocketAsyncEventArgs> handler = e.Completed;
			if (handler != null)
				handler (e._currentSocket, e);
		}

		internal void StartOperationCommon (Socket socket)
		{
			_currentSocket = socket;
		}

		internal void StartOperationWrapperConnect (MultipleConnectAsync args)
		{
			SetLastOperation (SocketAsyncOperation.Connect);

			//m_MultipleConnect = args;
		}

		internal void FinishConnectByNameSyncFailure (Exception exception, int bytesTransferred, SocketFlags flags)
		{
			SetResults (exception, bytesTransferred, flags);

			if (_currentSocket != null)
				_currentSocket.is_connected = false;
			
			Complete_internal ();
		}

		internal void FinishOperationAsyncFailure (Exception exception, int bytesTransferred, SocketFlags flags)
		{
			SetResults (exception, bytesTransferred, flags);

			if (_currentSocket != null)
				_currentSocket.is_connected = false;
			
			Complete_internal ();
		}

		internal void FinishWrapperConnectSuccess (Socket connectSocket, int bytesTransferred, SocketFlags flags)
		{
			SetResults(SocketError.Success, bytesTransferred, flags);
			_currentSocket = connectSocket;

			Complete_internal ();
		}
	}
}
