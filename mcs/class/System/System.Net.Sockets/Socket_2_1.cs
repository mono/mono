// System.Net.Sockets.Socket.cs
//
// Authors:
//	Phillip Pearson (pp@myelin.co.nz)
//	Dick Porter <dick@ximian.com>
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sridhar Kulkarni (sridharkulkarni@gmail.com)
//	Brian Nickel (brian.nickel@gmail.com)
//
// Copyright (C) 2001, 2002 Phillip Pearson and Ximian, Inc.
//    http://www.myelin.co.nz
// (c) 2004-2011 Novell, Inc. (http://www.novell.com)
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
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Security;
using System.Text;

#if !NET_2_1
using System.Net.Configuration;
using System.Net.NetworkInformation;
#endif

namespace System.Net.Sockets {

	public partial class Socket : IDisposable {
		[StructLayout (LayoutKind.Sequential)]
		struct WSABUF {
			public int len;
			public IntPtr buf;
		}

		void Linger (IntPtr handle)
		{
			if (!is_connected || linger_timeout <= 0)
				return;

			// We don't want to receive any more data
			int error;
			Shutdown_internal (handle, SocketShutdown.Receive, out error);
			if (error != 0)
				return;

			int seconds = linger_timeout / 1000;
			int ms = linger_timeout % 1000;
			if (ms > 0) {
				// If the other end closes, this will return 'true' with 'Available' == 0
				Poll_internal (handle, SelectMode.SelectRead, ms * 1000, out error);
				if (error != 0)
					return;

			}
			if (seconds > 0) {
				LingerOption linger = new LingerOption (true, seconds);
				SetSocketOption_internal (handle, SocketOptionLevel.Socket, SocketOptionName.Linger, linger, null, 0, out error);
				/* Not needed, we're closing upon return */
				/*if (error != 0)
					return; */
			}
		}
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void cancel_blocking_socket_operation (Thread thread);

		protected virtual void Dispose (bool disposing)
		{
			if (is_disposed)
				return;

			is_disposed = true;
			bool was_connected = is_connected;
			is_connected = false;
			
			if (safe_handle != null) {
				is_closed = true;
				IntPtr x = Handle;

				if (was_connected)
					Linger (x);

				safe_handle.Dispose ();
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void Shutdown_internal (IntPtr socket, SocketShutdown how, out int error);
		
		private static void Shutdown_internal (SafeSocketHandle safeHandle, SocketShutdown how, out int error)
		{
			bool release = false;
			try {
				safeHandle.DangerousAddRef (ref release);
				Shutdown_internal (safeHandle.DangerousGetHandle (), how, out error);
			} finally {
				if (release)
					safeHandle.DangerousRelease ();
			}
		}

		public void Shutdown (SocketShutdown how)
		{
			if (is_disposed && is_closed)
				throw new ObjectDisposedException (GetType ().ToString ());

			if (!is_connected)
				throw new SocketException (10057); // Not connected

			int error;
			
			Shutdown_internal (safe_handle, how, out error);
			if (error != 0)
				throw new SocketException (error);
		}

		Exception InvalidAsyncOp (string method)
		{
			return new InvalidOperationException (method + " can only be called once per asynchronous operation");
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void socket_pool_queue (SocketAsyncCallback d, SocketAsyncResult r);
	}
}

