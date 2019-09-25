#if MARTIN_USE_MONO_IO
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	internal static class SocketAsyncEngine
	{
		//
		// Encapsulates a particular SocketAsyncContext object's access to a SocketAsyncEngine.  
		//
		public readonly struct Token
		{
			readonly MyIOAsyncResult _ioares;
			readonly IOSelectorJob _readJob;
			readonly IOSelectorJob _writeJob;
			readonly IOSelectorJob _errorJob;
			readonly SafeCloseSocket _socket;

			public Token (SocketAsyncContext context, SafeCloseSocket socket)
			{
				_ioares = new MyIOAsyncResult (context);
				_readJob = new IOSelectorJob (IOOperation.Read, ReadCallback, _ioares);
				_writeJob = new IOSelectorJob (IOOperation.Write, WriteCallback, _ioares);
				_errorJob = new IOSelectorJob (IOOperation.Error, ErrorCallback, _ioares);

				_socket = socket;
				IOSelector.Add (socket.DangerousGetHandle (), _readJob);
				IOSelector.Add (socket.DangerousGetHandle (), _writeJob);
			}

			static void ReadCallback (IOAsyncResult ioares)
			{
				((MyIOAsyncResult)ioares).Context.HandleEvents (Interop.Sys.SocketEvents.Read);
			}

			static void WriteCallback (IOAsyncResult ioares)
			{
				((MyIOAsyncResult)ioares).Context.HandleEvents (Interop.Sys.SocketEvents.Write);
			}

			static void ErrorCallback (IOAsyncResult ioares)
			{
				((MyIOAsyncResult)ioares).Context.HandleEvents (Interop.Sys.SocketEvents.Error);
			}

			public bool WasAllocated => _socket != null;

			public void Free ()
			{
				if (_socket != null)
					IOSelector.Remove (_socket.DangerousGetHandle ());
			}
		}

		class MyIOAsyncResult : IOAsyncResult
		{
			public readonly SocketAsyncContext Context;

			public MyIOAsyncResult (SocketAsyncContext context)
			{
				Context = context;
			}

			internal override void CompleteDisposed ()
			{
				Context.HandleEvents (Interop.Sys.SocketEvents.Close);
			}
		}
	}
}
#endif
