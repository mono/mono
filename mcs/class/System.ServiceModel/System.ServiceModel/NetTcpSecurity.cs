//
// NetTcpSecurity.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Net.Security;
using System.ServiceModel.Channels;

namespace System.ServiceModel
{
	public sealed class NetTcpSecurity
	{
#if NET_4_0
		public NetTcpSecurity ()
			: this (SecurityMode.Transport)
		{
		}
#endif

		internal NetTcpSecurity (SecurityMode mode)
		{
			this.mode = mode;
			this.message = new MessageSecurityOverTcp ();
			this.transport = new TcpTransportSecurity ();
		}

		MessageSecurityOverTcp message;
		SecurityMode mode;
		TcpTransportSecurity transport;

		public MessageSecurityOverTcp Message {
			get { return message; }
#if NET_4_0
			set { message = value; }
#endif
		}

		public SecurityMode Mode {
			get { return mode; }
			set { mode = value; }
		}

		public TcpTransportSecurity Transport {
			get { return transport; }
#if NET_4_0
			set { transport = value; }
#endif
		}
	}
}
