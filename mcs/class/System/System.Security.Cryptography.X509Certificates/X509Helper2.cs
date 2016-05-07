//
// X509Helper2.cs
//
// Authors:
//	Martin Baulig  <martin.baulig@xamarin.com>
//
// Copyright (C) 2016 Xamarin, Inc. (http://www.xamarin.com)
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

#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

namespace System.Security.Cryptography.X509Certificates
{
	internal static class X509Helper2
	{
		internal static void Initialize ()
		{
			X509Helper.InstallNativeHelper (new MyNativeHelper ());
		}

		internal static void ThrowIfContextInvalid (X509CertificateImpl impl)
		{
			X509Helper.ThrowIfContextInvalid (impl);
		}

		internal static X509Certificate2Impl Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			var provider = MonoTlsProviderFactory.GetProvider ();
			if (provider.HasNativeCertificates) {
				var impl = provider.GetNativeCertificate (rawData, password, keyStorageFlags);
				return impl;
			} else {
				var impl = new X509Certificate2ImplMono ();
				impl.Import (rawData, password, keyStorageFlags);
				return impl;
			}
		}

		internal static X509Certificate2Impl Import (X509Certificate cert)
		{
			var provider = MonoTlsProviderFactory.GetProvider ();
			if (provider.HasNativeCertificates) {
				var impl = provider.GetNativeCertificate (cert);
				return impl;
			}
			var impl2 = cert.Impl as X509Certificate2Impl;
			if (impl2 != null)
				return (X509Certificate2Impl)impl2.Clone ();
			return Import (cert.GetRawCertData (), null, X509KeyStorageFlags.DefaultKeySet);
		}

		internal static X509ChainImpl CreateChainImpl (bool useMachineContext)
		{
			return new X509ChainImplMono (useMachineContext);
		}

		public static bool IsValid (X509ChainImpl impl)
		{
			return impl != null && impl.IsValid;
		}

		internal static void ThrowIfContextInvalid (X509ChainImpl impl)
		{
			if (!IsValid (impl))
				throw GetInvalidChainContextException ();
		}

		internal static Exception GetInvalidChainContextException ()
		{
			return new CryptographicException (Locale.GetText ("Chain instance is empty."));
		}

		class MyNativeHelper : INativeCertificateHelper
		{
			public X509CertificateImpl Import (
				byte[] data, string password, X509KeyStorageFlags flags)
			{
				return X509Helper2.Import (data, password, flags);
			}

			public X509CertificateImpl Import (X509Certificate cert)
			{
				return X509Helper2.Import (cert);
			}
		}
	}
}
#endif
