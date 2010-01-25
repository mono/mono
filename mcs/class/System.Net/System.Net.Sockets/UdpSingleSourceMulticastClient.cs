//
// UdpSingleSourceMulticastClient (Moonlight 4.0)
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc.
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

#if NET_2_1

namespace System.Net.Sockets {

	[MonoTODO ("stub (with some validations) to allow SL4 tests compilation")]
	public class UdpSingleSourceMulticastClient : IDisposable {

		const string ObjectDisposed = "UdpSingleSourceMulticastClient instance was disposed.";
		bool disposed;

		public UdpSingleSourceMulticastClient (IPAddress sourceAddress, IPAddress groupAddress, int localPort)
		{
			if (sourceAddress == null)
				throw new ArgumentNullException ("sourceAddress");
			if (groupAddress == null)
				throw new ArgumentNullException ("groupAddress");
			if ((localPort < 0) || (localPort > 65535))
				throw new ArgumentOutOfRangeException ("localPort");
			if (localPort < 1024)
				throw new SocketException ();

			throw new NotImplementedException ();
		}

		public int ReceiveBufferSize { get; set; }
		public int SendBufferSize { get; set; }

		public IAsyncResult BeginJoinGroup (AsyncCallback callback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);

			throw new NotImplementedException ();
		}

		public void EndJoinGroup (IAsyncResult result)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);

			throw new NotImplementedException ();
		}

		public IAsyncResult BeginReceiveFromSource (byte [] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if ((offset < 0) || (offset > buffer.Length))
				throw new ArgumentOutOfRangeException ("offset");
			if ((count < 0) || (count > buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			throw new NotImplementedException ();
		}

		public int EndReceiveFromSource (IAsyncResult result, out int sourcePort)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);
			if (result == null)
				throw new ArgumentNullException ("result");

			throw new NotImplementedException ();
		}

		public IAsyncResult BeginSendToSource (byte [] buffer, int offset, int count, int remotePort, AsyncCallback callback, object state)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if ((offset < 0) || (offset > buffer.Length))
				throw new ArgumentOutOfRangeException ("offset");
			if ((count < 0) || (count > buffer.Length - offset))
				throw new ArgumentOutOfRangeException ("count");

			throw new NotImplementedException ();
		}

		public void EndSendToSource (IAsyncResult result)
		{
			if (disposed)
				throw new ObjectDisposedException (ObjectDisposed);
			if (result == null)
				throw new ArgumentNullException ("result");

			throw new NotImplementedException ();
		}

		public void Dispose ()
		{
			disposed = true;
		}
	}
}

#endif

