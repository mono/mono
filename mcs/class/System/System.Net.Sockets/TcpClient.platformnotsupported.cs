//
// System.Net.Sockets.TcpClient.cs
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
	public class TcpClient : IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.Sockets.TcpClient is not supported on the current platform.";

		public TcpClient ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpClient (AddressFamily family)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpClient (IPEndPoint localEP)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public TcpClient (string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected bool Active {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public Socket Client {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int Available {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool Connected {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool ExclusiveAddressUse {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public LingerOption LingerState {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool NoDelay {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ReceiveBufferSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ReceiveTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int SendBufferSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int SendTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (IPEndPoint remoteEP)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (IPAddress address, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (IPAddress[] ipAddresses, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void EndConnect (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginConnect (IPAddress address, int port, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginConnect (IPAddress[] addresses, int port, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginConnect (string host, int port, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Dispose ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		~TcpClient ()
		{
		}

		public NetworkStream GetStream()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task ConnectAsync (IPAddress address, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task ConnectAsync (IPAddress[] addresses, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task ConnectAsync (string host, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
