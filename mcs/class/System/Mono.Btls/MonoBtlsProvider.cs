﻿//
// MonoBtlsProvider.cs
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
#if SECURITY_DEP && MONO_FEATURE_BTLS
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MX = MonoSecurity::Mono.Security.X509;
#else
using Mono.Security.Interface;
using MX = Mono.Security.X509;
#endif

using MNS = Mono.Net.Security;

namespace Mono.Btls
{
	class MonoBtlsProvider : MonoTlsProvider
	{
		static readonly Guid id = new Guid ("432d18c9-9348-4b90-bfbf-9f2a10e1f15b");

		public override Guid ID {
			get { return id; }
		}
		public override string Name {
			get { return "btls"; }
		}

		internal MonoBtlsProvider ()
		{
			if (!MNS.MonoTlsProviderFactory.IsBtlsSupported ())
				throw new NotSupportedException ("BTLS is not supported in this runtime.");
		}

		public override bool SupportsSslStream {
			get { return true; }
		}

		public override bool SupportsMonoExtensions {
			get { return true; }
		}

		public override bool SupportsConnectionInfo {
			get { return true; }
		}

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		public override IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings = null)
		{
			return new MonoBtlsStream (
				innerStream, leaveInnerStreamOpen, settings, this);
		}

		internal override bool HasNativeCertificates {
			get { return true; }
		}

		internal override X509Certificate2Impl GetNativeCertificate (
			byte[] data, string password, X509KeyStorageFlags flags)
		{
			var impl = new X509CertificateImplBtls (false);
			impl.Import (data, password, flags);
			return impl;
		}

		internal override X509Certificate2Impl GetNativeCertificate (
			X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplBtls;
			if (impl != null)
				return (X509Certificate2Impl)impl.Clone ();

			var data = certificate.GetRawCertData ();
			return new X509CertificateImplBtls (data, MonoBtlsX509Format.DER, false);
		}

		internal static MonoBtlsX509VerifyParam GetVerifyParam (string targetHost, bool serverMode)
		{
			MonoBtlsX509VerifyParam param;
			if (serverMode)
				param = MonoBtlsX509VerifyParam.GetSslClient ();
			else
				param = MonoBtlsX509VerifyParam.GetSslServer ();

			if (targetHost == null)
				return param;

			try {
				var copy = param.Copy ();
				copy.SetHost (targetHost);
				return copy;
			} finally {
				param.Dispose ();
			}
		}

		internal override bool ValidateCertificate (
			ICertificateValidator2 validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref MonoSslPolicyErrors errors, ref int status11)
		{
			if (chain != null) {
				var chainImpl = (X509ChainImplBtls)chain.Impl;
				var success = chainImpl.StoreCtx.VerifyResult == 1;
				CheckValidationResult (
					validator, targetHost, serverMode, certificates,
					wantsChain, chain, chainImpl.StoreCtx,
					success, ref errors, ref status11);
				return success;
			}

			using (var store = new MonoBtlsX509Store ())
			using (var nativeChain = MonoBtlsProvider.GetNativeChain (certificates))
			using (var param = GetVerifyParam (targetHost, serverMode))
			using (var storeCtx = new MonoBtlsX509StoreCtx ()) {
				SetupCertificateStore (store, validator.Settings, serverMode);

				storeCtx.Initialize (store, nativeChain);

				storeCtx.SetVerifyParam (param);

				var ret = storeCtx.Verify ();

				var success = ret == 1;

				if (wantsChain && chain == null) {
					chain = GetManagedChain (nativeChain);
				}

				CheckValidationResult (
					validator, targetHost, serverMode, certificates,
					wantsChain, null, storeCtx,
					success, ref errors, ref status11);
				return success;
			}
		}

		internal static bool ValidateCertificate (MonoBtlsX509Chain chain, MonoBtlsX509VerifyParam param)
		{
			using (var store = new MonoBtlsX509Store ())
			using (var storeCtx = new MonoBtlsX509StoreCtx ()) {
				SetupCertificateStore (store);

				storeCtx.Initialize (store, chain);

				if (param != null)
					storeCtx.SetVerifyParam (param);

				var ret = storeCtx.Verify ();

				return ret == 1;
			}
		}

		void CheckValidationResult (
			ICertificateValidator validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain,
			X509Chain chain, MonoBtlsX509StoreCtx storeCtx,
			bool success, ref MonoSslPolicyErrors errors, ref int status11)
		{
			if (!success) {
				errors = MonoSslPolicyErrors.RemoteCertificateChainErrors;
				status11 = unchecked((int)0x800B010B);
			}
		}

		internal static void SetupCertificateStore (MonoBtlsX509Store store, MonoTlsSettings settings, bool server)
		{
#if MONODROID
			SetupCertificateStore (store);
			return;
#else

			if (settings?.CertificateSearchPaths == null) {
				SetupCertificateStore (store);
				return;
			}

			foreach (var path in settings.CertificateSearchPaths) {
				if (string.Equals (path, "@default", StringComparison.Ordinal)) {
					AddTrustedRoots (store, settings, server);
					AddUserStore (store);
					AddMachineStore (store);
				} else if (string.Equals (path, "@user", StringComparison.Ordinal))
					AddUserStore (store);
				else if (string.Equals (path, "@machine", StringComparison.Ordinal))
					AddMachineStore (store);
				else if (string.Equals (path, "@trusted", StringComparison.Ordinal))
					AddTrustedRoots (store, settings, server);
				else if (path.StartsWith ("@pem:", StringComparison.Ordinal)) {
					var realPath = path.Substring (5);
					if (Directory.Exists (realPath))
						store.AddDirectoryLookup (realPath, MonoBtlsX509FileType.PEM);
				} else if (path.StartsWith ("@der:", StringComparison.Ordinal)) {
					var realPath = path.Substring (5);
					if (Directory.Exists (realPath))
						store.AddDirectoryLookup (realPath, MonoBtlsX509FileType.ASN1);
				} else {
					if (Directory.Exists (path))
						store.AddDirectoryLookup (path, MonoBtlsX509FileType.PEM);
				}
			}
#endif
		}

		internal static void SetupCertificateStore (MonoBtlsX509Store store)
		{
#if MONODROID
			store.SetDefaultPaths ();
			store.AddAndroidLookup ();
#else
			AddUserStore (store);
			AddMachineStore (store);
#endif
		}

#if !MONODROID
		static void AddUserStore (MonoBtlsX509Store store)
		{
			var userPath = MonoBtlsX509StoreManager.GetStorePath (MonoBtlsX509StoreType.UserTrustedRoots);
			if (Directory.Exists (userPath))
				store.AddDirectoryLookup (userPath, MonoBtlsX509FileType.PEM);
		}

		static void AddMachineStore (MonoBtlsX509Store store)
		{
			var machinePath = MonoBtlsX509StoreManager.GetStorePath (MonoBtlsX509StoreType.MachineTrustedRoots);
			if (Directory.Exists (machinePath))
				store.AddDirectoryLookup (machinePath, MonoBtlsX509FileType.PEM);
		}

		static void AddTrustedRoots (MonoBtlsX509Store store, MonoTlsSettings settings, bool server)
		{
			if (settings?.TrustAnchors == null)
				return;
			var trust = server ? MonoBtlsX509TrustKind.TRUST_CLIENT : MonoBtlsX509TrustKind.TRUST_SERVER;
			store.AddCollection (settings.TrustAnchors, trust);
		}
#endif

		public static string GetSystemStoreLocation ()
		{
#if MONODROID
			return "/system/etc/security/cacerts";
#else
			return MonoBtlsX509StoreManager.GetStorePath (MonoBtlsX509StoreType.MachineTrustedRoots);
#endif
		}

		public static X509Certificate CreateCertificate (byte[] data, MonoBtlsX509Format format, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplBtls (data, format, disallowFallback)) {
				return new X509Certificate (impl);
			}
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, MonoBtlsX509Format format, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplBtls (data, format, disallowFallback)) {
				return new X509Certificate2 (impl);
			}
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, string password, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplBtls (disallowFallback)) {
				impl.Import (data, password, X509KeyStorageFlags.DefaultKeySet);
				return new X509Certificate2 (impl);
			}
		}

		public static X509Certificate CreateCertificate (MonoBtlsX509 x509)
		{
			using (var impl = new X509CertificateImplBtls (x509, true))
				return new X509Certificate (impl);
		}

		public static X509Chain CreateChain ()
		{
			using (var impl = new X509ChainImplBtls ())
				return new X509Chain (impl);
		}

		public static X509Chain GetManagedChain (MonoBtlsX509Chain chain)
		{
			var impl = new X509ChainImplBtls (chain);
			return new X509Chain (impl);
		}

		public static MonoBtlsX509 GetBtlsCertificate (X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplBtls;
			if (impl != null)
				return impl.X509.Copy ();

			return MonoBtlsX509.LoadFromData (certificate.GetRawCertData (), MonoBtlsX509Format.DER);
		}

		public static MonoBtlsX509Chain GetNativeChain (X509CertificateCollection certificates)
		{
			var chain = new MonoBtlsX509Chain ();
			try {
				foreach (var cert in certificates) {
					using (var x509 = GetBtlsCertificate (cert))
						chain.AddCertificate (x509);
				}
				return chain;
			} catch {
				chain.Dispose ();
				throw;
			}
		}
	}
}
#endif
