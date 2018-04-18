//
// System.Net.Sockets.UdpClient.cs
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
using System.Threading.Tasks;

namespace System.Net.Sockets
{
	public class UdpClient : IDisposable
	{
		const string EXCEPTION_MESSAGE = "System.Net.Sockets.UdpClient is not supported on the current platform.";

		public UdpClient ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public UdpClient(AddressFamily family)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public UdpClient (int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public UdpClient (IPEndPoint localEP)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public UdpClient (int port, AddressFamily family)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public UdpClient (string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Close ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (IPEndPoint endPoint)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (IPAddress addr, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void Connect (string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void DropMulticastGroup (IPAddress multicastAddr)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void DropMulticastGroup (IPAddress multicastAddr, int ifindex)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void JoinMulticastGroup (IPAddress multicastAddr)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void JoinMulticastGroup (int ifindex, IPAddress multicastAddr)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void JoinMulticastGroup (IPAddress multicastAddr, int timeToLive)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public void JoinMulticastGroup (IPAddress multicastAddr, IPAddress localAddress)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public byte [] Receive (ref IPEndPoint remoteEP)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int Send (byte [] dgram, int bytes)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int Send (byte [] dgram, int bytes, IPEndPoint endPoint)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int Send (byte [] dgram, int bytes, string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginSend (byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginSend (byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginSend (byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public int EndSend (IAsyncResult asyncResult)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public IAsyncResult BeginReceive (AsyncCallback requestCallback, object state)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public byte[] EndReceive (IAsyncResult asyncResult, ref IPEndPoint remoteEP)
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

		public bool DontFragment {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool EnableBroadcast {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool ExclusiveAddressUse {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool MulticastLoopback {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public short Ttl {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void AllowNatTraversal (bool allowed)
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

		~UdpClient ()
		{
		}

		public Task<UdpReceiveResult> ReceiveAsync ()
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes, IPEndPoint endPoint)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public Task<int> SendAsync (byte[] datagram, int bytes, string hostname, int port)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
