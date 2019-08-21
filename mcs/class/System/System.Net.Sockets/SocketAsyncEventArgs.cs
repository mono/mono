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

		internal void SetLastOperation (SocketAsyncOperation op)
		{
			if (_disposeCalled)
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
	}
}
