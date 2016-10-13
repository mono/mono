//
// ServicePointManager.cs
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

using System.Net.Security;

namespace System.Net
{
	public partial class ServicePointManager {
		const string EXCEPTION_MESSAGE = "System.Net.ServicePointManager is not supported on the current platform.";

		public const int DefaultNonPersistentConnectionLimit = 4;
#if MOBILE
		public const int DefaultPersistentConnectionLimit = 10;
#else
		public const int DefaultPersistentConnectionLimit = 2;
#endif

		private ServicePointManager ()
		{
		}

		public static ICertificatePolicy CertificatePolicy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool CheckCertificateRevocationList {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static int DefaultConnectionLimit {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static int DnsRefreshTimeout {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool EnableDnsRoundRobin {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static int MaxServicePointIdleTime {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static int MaxServicePoints {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool ReusePort {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static SecurityProtocolType SecurityProtocol {
			get;
			set;
		} = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

		public static RemoteCertificateValidationCallback ServerCertificateValidationCallback {
			get;
			set;
		}

		public static EncryptionPolicy EncryptionPolicy {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool Expect100Continue {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static bool UseNagleAlgorithm {
			get { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
			set { throw new PlatformNotSupportedException (EXCEPTION_MESSAGE); }
		}

		public static void SetTcpKeepAlive (bool enabled, int keepAliveTime, int keepAliveInterval)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static ServicePoint FindServicePoint (Uri address)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static ServicePoint FindServicePoint (string uriString, IWebProxy proxy)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		public static ServicePoint FindServicePoint (Uri address, IWebProxy proxy)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}
	}
}
