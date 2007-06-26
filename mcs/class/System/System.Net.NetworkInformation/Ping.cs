//
// System.Net.NetworkInformation.Ping
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel;

namespace System.Net.NetworkInformation {
	public class Ping : Component, IDisposable
	{

		public event PingCompletedEventHandler PingCompleted;

		public Ping ()
		{
		}

		protected void OnPingCompleted (PingCompletedEventArgs e)
		{
			if (PingCompleted != null)
				PingCompleted (this, e);
		}

		[MonoNotSupported("")]
		public PingReply Send (IPAddress address)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (IPAddress address, int timeout)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (IPAddress address, int timeout, byte [] buffer)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (IPAddress address, int timeout, byte [] buffer, PingOptions options)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (string hostNameOrAddress)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (string hostNameOrAddress, int timeout)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (string hostNameOrAddress, int timeout, byte [] buffer)
		{
			return null;
		}

		[MonoNotSupported("")]
		public PingReply Send (string hostNameOrAddress, int timeout, byte [] buffer, PingOptions options)
		{
			return null;
		}

		[MonoNotSupported("")]
		public void SendAsync (IPAddress address, int timeout, byte [] buffer, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (IPAddress address, int timeout, byte [] buffer, PingOptions options, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (IPAddress address, int timeout, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (IPAddress address, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (string hostNameOrAddress, int timeout, byte [] buffer, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (string hostNameOrAddress, int timeout, byte [] buffer, PingOptions options, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (string hostNameOrAddress, int timeout, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsync (string hostNameOrAddress, object userToken)
		{
		}

		[MonoNotSupported("")]
		public void SendAsyncCancel ()
		{
		}

		void IDisposable.Dispose ()
		{
		}
	}
}
#endif

