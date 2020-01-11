//
// ServicePoint.cs
//
// Author:
//       Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc.
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

using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace System.Net
{
	public class ServicePoint
	{
		const string EXCEPTION_MESSAGE = "System.Net.ServicePoint is not supported on the current platform.";

		ServicePoint () {}

		public Uri Address {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}

		public BindIPEndPoint BindIPEndPointDelegate
		{
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ConnectionLeaseTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ConnectionLimit {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public string ConnectionName {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int CurrentConnections {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public DateTime IdleSince {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int MaxIdleTime {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public virtual Version ProtocolVersion {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public int ReceiveBufferSize {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool SupportsPipelining {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool Expect100Continue {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public bool UseNagleAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public void SetTcpKeepAlive (bool enabled, int keepAliveTime, int keepAliveInterval)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public bool CloseConnectionGroup (string connectionGroupName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public  X509Certificate Certificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public  X509Certificate ClientCertificate {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}
	}
}
