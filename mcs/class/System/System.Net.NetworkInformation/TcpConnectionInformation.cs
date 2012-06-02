//
// System.Net.NetworkInformation.TcpConnectionInformation
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (c) 2006-2007 Novell, Inc. (http://www.novell.com)
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
using System.Net;

namespace System.Net.NetworkInformation {
	public abstract class TcpConnectionInformation {
		protected TcpConnectionInformation ()
		{
		}
		
		public abstract IPEndPoint LocalEndPoint { get; }
		public abstract IPEndPoint RemoteEndPoint { get; }
		public abstract TcpState State { get; }
	}

	class TcpConnectionInformationImpl : TcpConnectionInformation
	{
		IPEndPoint local;
		IPEndPoint remote;
		TcpState state;

		public TcpConnectionInformationImpl (IPEndPoint local, IPEndPoint remote, TcpState state)
		{
			this.local = local;
			this.remote = remote;
			this.state = state;
		}

		public override IPEndPoint LocalEndPoint {
			get { return local; }
		}
		public override IPEndPoint RemoteEndPoint {
			get { return remote; }
		}
		public override TcpState State {
			get { return state; }
		}
	}
}

