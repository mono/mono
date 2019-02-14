//
// BtlsProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (http://www.xamarin.com)
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
using Mono.Security.Interface;
using System.Security.Cryptography.X509Certificates;
using MNS = Mono.Net.Security;

namespace Mono.Btls.Interface
{
	public static class BtlsProvider
	{
		public static bool IsSupported ()
		{
			return MNS.MonoTlsProviderFactory.IsBtlsSupported ();
		}

		public static MonoTlsProvider GetProvider ()
		{
			return new MonoBtlsProvider ();
		}

		public static BtlsX509 CreateNative (byte[] data, BtlsX509Format format)
		{
			var x509 = MonoBtlsX509.LoadFromData (data, (MonoBtlsX509Format)format);
			return new BtlsX509 (x509);
		}

		public static X509Certificate CreateCertificate (byte[] data, BtlsX509Format format, bool disallowFallback = false)
		{
			return MonoBtlsProvider.CreateCertificate (data, (MonoBtlsX509Format)format);
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, BtlsX509Format format, bool disallowFallback = false)
		{
			return MonoBtlsProvider.CreateCertificate (data, (MonoBtlsX509Format)format);
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, string password, bool disallowFallback = false)
		{
			return MonoBtlsProvider.CreateCertificate (data, password);
		}

		public static BtlsX509Chain CreateNativeChain ()
		{
			return new BtlsX509Chain (new MonoBtlsX509Chain ());
		}

		public static BtlsX509Store CreateNativeStore ()
		{
			return new BtlsX509Store (new MonoBtlsX509Store ());
		}

		public static BtlsX509StoreCtx CreateNativeStoreCtx ()
		{
			return new BtlsX509StoreCtx (new MonoBtlsX509StoreCtx ());
		}

		public static X509Chain CreateChain ()
		{
			return MonoBtlsProvider.CreateChain ();
		}

		public static string GetSystemStoreLocation ()
		{
			return MonoBtlsProvider.GetSystemStoreLocation ();
		}

		public static BtlsX509VerifyParam GetVerifyParam_SslClient ()
		{
			return new BtlsX509VerifyParam (MonoBtlsX509VerifyParam.GetSslClient ());
		}

		public static BtlsX509VerifyParam GetVerifyParam_SslServer ()
		{
			return new BtlsX509VerifyParam (MonoBtlsX509VerifyParam.GetSslServer ());
		}

		public static X509Chain GetManagedChain (BtlsX509Chain chain)
		{
			return MonoBtlsProvider.GetManagedChain (chain.Instance);
		}
	}
}

