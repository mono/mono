//
// System.Net.Sockets.TcpListener.cs
//
// Author:
//	Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	public class TcpListener
	{
		const string EXCEPTION_MESSAGE = "System.Net.Sockets.TcpListener is not supported on the current platform.";

		public TcpListener (int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpListener (IPEndPoint localEP)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpListener (IPAddress localaddr, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected bool Active {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public EndPoint LocalEndpoint {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Socket Server {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool ExclusiveAddressUse {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Socket AcceptSocket ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpClient AcceptTcpClient ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void AllowNatTraversal (bool allowed)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static TcpListener Create (int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		~TcpListener ()
		{
		}

		public bool Pending ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Start ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Start (int backlog)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginAcceptSocket (AsyncCallback callback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginAcceptTcpClient (AsyncCallback callback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Socket EndAcceptSocket (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpClient EndAcceptTcpClient (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Stop ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<Socket> AcceptSocketAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<TcpClient> AcceptTcpClientAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
