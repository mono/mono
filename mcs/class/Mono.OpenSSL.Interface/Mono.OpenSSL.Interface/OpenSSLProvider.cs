//
// OpenSSLProvider.cs
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

namespace Mono.OpenSSL.Interface
{
	public static class OpenSSLProvider
	{
		public static bool IsSupported ()
		{
			return MNS.MonoTlsProviderFactory.IsOpenSSLSupported ();
		}

		public static MonoTlsProvider GetProvider ()
		{
			return new MonoOpenSSLProvider ();
		}

		public static OpenSSLX509 CreateNative (byte[] data, OpenSSLX509Format format)
		{
			var x509 = MonoOpenSSLX509.LoadFromData (data, (MonoOpenSSLX509Format)format);
			return new OpenSSLX509 (x509);
		}

		public static X509Certificate CreateCertificate (byte[] data, OpenSSLX509Format format, bool disallowFallback = false)
		{
			return MonoOpenSSLProvider.CreateCertificate (data, (MonoOpenSSLX509Format)format, disallowFallback);
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, OpenSSLX509Format format, bool disallowFallback = false)
		{
			return MonoOpenSSLProvider.CreateCertificate2 (data, (MonoOpenSSLX509Format)format, disallowFallback);
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, string password, bool disallowFallback = false)
		{
			return MonoOpenSSLProvider.CreateCertificate2 (data, password, disallowFallback);
		}

		public static OpenSSLX509Chain CreateNativeChain ()
		{
			return new OpenSSLX509Chain (new MonoOpenSSLX509Chain ());
		}

		public static OpenSSLX509Store CreateNativeStore ()
		{
			return new OpenSSLX509Store (new MonoOpenSSLX509Store ());
		}

		public static OpenSSLX509StoreCtx CreateNativeStoreCtx ()
		{
			return new OpenSSLX509StoreCtx (new MonoOpenSSLX509StoreCtx ());
		}

		public static X509Chain CreateChain ()
		{
			return MonoOpenSSLProvider.CreateChain ();
		}

		public static string GetSystemStoreLocation ()
		{
			return MonoOpenSSLProvider.GetSystemStoreLocation ();
		}

		public static OpenSSLX509VerifyParam GetVerifyParam_SslClient ()
		{
			return new OpenSSLX509VerifyParam (MonoOpenSSLX509VerifyParam.GetSslClient ());
		}

		public static OpenSSLX509VerifyParam GetVerifyParam_SslServer ()
		{
			return new OpenSSLX509VerifyParam (MonoOpenSSLX509VerifyParam.GetSslServer ());
		}

		public static X509Chain GetManagedChain (OpenSSLX509Chain chain)
		{
			return MonoOpenSSLProvider.GetManagedChain (chain.Instance);
		}
	}
}

