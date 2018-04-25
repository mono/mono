//
// Socket.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

namespace System.Net.Sockets
{
	partial class Socket
	{
		internal const int DefaultCloseTimeout = -1;
		// don't change for default, otherwise breaking change

		// this version does not throw.
		internal void InternalShutdown (SocketShutdown how)
		{
			if (!is_connected || CleanedUp)
				return;
			int error;
			Shutdown_internal (m_Handle, how, out error);
		}

		internal IAsyncResult UnsafeBeginConnect (EndPoint remoteEP, AsyncCallback callback, object state)
		{
			return BeginConnect (remoteEP, callback, state);
		}

		internal IAsyncResult UnsafeBeginSend (byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			return BeginSend (buffer, offset, size, socketFlags, callback, state);
		}

		internal IAsyncResult UnsafeBeginReceive (byte[] buffer, int offset, int size, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			return BeginReceive (buffer, offset, size, socketFlags, callback, state);
		}

		internal IAsyncResult BeginMultipleSend (BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			var segments = new ArraySegment<byte> [buffers.Length];
			for (int i = 0; i < buffers.Length; i++)
				segments [i] = new ArraySegment<byte> (buffers [i].Buffer, buffers [i].Offset, buffers [i].Size);
			return BeginSend (segments, socketFlags, callback, state);
		}

		internal IAsyncResult UnsafeBeginMultipleSend (BufferOffsetSize[] buffers, SocketFlags socketFlags, AsyncCallback callback, object state)
		{
			return BeginMultipleSend (buffers, socketFlags, callback, state);
		}

		internal int EndMultipleSend (IAsyncResult asyncResult)
		{
			return EndSend (asyncResult);
		}

		internal void MultipleSend (BufferOffsetSize[] buffers, SocketFlags socketFlags)
		{
			var segments = new ArraySegment<byte> [buffers.Length];
			for (int i = 0; i < buffers.Length; i++)
				segments [i] = new ArraySegment<byte> (buffers [i].Buffer, buffers [i].Offset, buffers [i].Size);
			Send (segments, socketFlags);
		}

		internal void SetSocketOption (SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue, bool silent)
		{
			if (CleanedUp && is_closed) {
				if (silent)
					return;
				throw new ObjectDisposedException (GetType ().ToString ());
			}

			int error;

			SetSocketOption_internal (m_Handle, optionLevel, optionName, null,
				null, optionValue, out error);

			if (!silent && error != 0)
				throw new SocketException (error);
		}
	}
}
