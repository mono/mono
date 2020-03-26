//
// SystemCertificateProvider.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2018 Xamarin, Inc.
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
#if MONO_FEATURE_BTLS || MONO_FEATURE_APPLETLS
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
using MNS = Mono.Net.Security;
#endif

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;

namespace Mono
{
	class SystemCertificateProvider : ISystemCertificateProvider
	{
#if MONO_FEATURE_BTLS || MONO_FEATURE_APPLETLS
		public MonoTlsProvider Provider {
			get {
				EnsureInitialized ();
				return provider;
			}
		}

		static MonoTlsProvider provider;
#endif

		static X509PalImpl GetX509Pal ()
		{
#if MONO_FEATURE_APPLETLS
			if (provider?.ID == MNS.MonoTlsProviderFactory.AppleTlsId)
				return new Mono.AppleTls.X509PalImplApple ();
#elif MONO_FEATURE_APPLE_X509
			return new Mono.AppleTls.X509PalImplApple ();
#endif
#if MONO_FEATURE_BTLS
			if (provider?.ID == MNS.MonoTlsProviderFactory.BtlsId)
				return new Mono.Btls.X509PalImplBtls (provider);
#endif

			return new X509PalImplMono ();
		}

		static int initialized;
		static X509PalImpl x509pal;
		static object syncRoot = new object ();

		static void EnsureInitialized ()
		{
			/*
			 * We need to lazily initialize because we might be called from
			 * MonoTlsProviderFactory.InitializeInternal().
			 *
			 */
			lock (syncRoot) {
				if (Interlocked.CompareExchange (ref initialized, 1, 0) != 0)
					return;

#if MONO_FEATURE_BTLS || MONO_FEATURE_APPLETLS
				provider = MonoTlsProviderFactory.GetProvider ();
#endif
				x509pal = GetX509Pal ();
			}
		}

		public X509PalImpl X509Pal {
			get {
				EnsureInitialized ();
				return x509pal;
			}
		}

		public X509CertificateImpl Import (
			byte[] data, CertificateImportFlags importFlags = CertificateImportFlags.None)
		{
			if (data == null || data.Length == 0)
				return null;

			X509CertificateImpl impl = null;
			if ((importFlags & CertificateImportFlags.DisableNativeBackend) == 0) {
				impl = X509Pal.Import (data);
				if (impl != null)
					return impl;
			}

			if ((importFlags & CertificateImportFlags.DisableAutomaticFallback) != 0)
				return null;

			return X509Pal.ImportFallback (data);
		}

		X509CertificateImpl ISystemCertificateProvider.Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags,
			CertificateImportFlags importFlags)
		{
			return Import (data, password, keyStorageFlags, importFlags);
		}

		public X509Certificate2Impl Import (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags keyStorageFlags,
			CertificateImportFlags importFlags = CertificateImportFlags.None)
		{
			if (data == null || data.Length == 0)
				return null;

			X509Certificate2Impl impl = null;
			if ((importFlags & CertificateImportFlags.DisableNativeBackend) == 0) {
				impl = X509Pal.Import (data, password, keyStorageFlags);
				if (impl != null)
					return impl;
			}

			if ((importFlags & CertificateImportFlags.DisableAutomaticFallback) != 0)
				return null;

			return X509Pal.ImportFallback (data, password, keyStorageFlags);
		}

		X509CertificateImpl ISystemCertificateProvider.Import (X509Certificate cert, CertificateImportFlags importFlags)
		{
			return Import (cert, importFlags);
		}

		public X509Certificate2Impl Import (
			X509Certificate cert, CertificateImportFlags importFlags = CertificateImportFlags.None)
		{
			if (cert.Impl == null)
				return null;

			var impl = cert.Impl as X509Certificate2Impl;
			if (impl != null)
				return (X509Certificate2Impl)impl.Clone ();

			if ((importFlags & CertificateImportFlags.DisableNativeBackend) == 0) {
				impl = X509Pal.Import (cert);
				if (impl != null)
					return impl;
			}

			if ((importFlags & CertificateImportFlags.DisableAutomaticFallback) != 0)
				return null;

			return X509Pal.ImportFallback (cert.GetRawCertData ());
		}
	}
}
