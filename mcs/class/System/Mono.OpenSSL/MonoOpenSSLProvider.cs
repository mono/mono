//
// MonoOpenSSLProvider.cs
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
#if SECURITY_DEP && MONO_FEATURE_OPENSSL
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using Microsoft.Win32.SafeHandles;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
using MX = MonoSecurity::Mono.Security.X509;
#else
using Mono.Security.Interface;
using MX = Mono.Security.X509;
#endif

using MNS = Mono.Net.Security;

namespace Mono.OpenSSL
{
	class MonoOpenSSLProvider : MonoTlsProvider
	{
		public override Guid ID {
			get { return MNS.MonoTlsProviderFactory.OpenSSLId; }
		}
		public override string Name {
			get { return "openssl"; }
		}

		internal MonoOpenSSLProvider ()
		{
			if (!MNS.MonoTlsProviderFactory.IsOpenSSLSupported ())
				throw new NotSupportedException ("OpenSSL is not supported in this runtime.");
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

		internal override bool SupportsCleanShutdown {
			get { return true; }
		}

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		public override IMonoSslStream CreateSslStream (
			Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings = null)
		{
			return SslStream.CreateMonoSslStream (innerStream, leaveInnerStreamOpen, this, settings);
		}

		internal override IMonoSslStream CreateSslStreamInternal (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings)
		{
			return new MonoOpenSSLStream (
				innerStream, leaveInnerStreamOpen, sslStream, settings, this);
		}

		internal override bool HasNativeCertificates {
			get { return true; }
		}

		internal override X509Certificate2Impl GetNativeCertificate (
			byte[] data, string password, X509KeyStorageFlags flags)
		{
			using (var handle = new SafePasswordHandle (password))
				return GetNativeCertificate (data, handle, flags);
		}

		internal override X509Certificate2Impl GetNativeCertificate (
			X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplOpenSSL;
			if (impl != null)
				return (X509Certificate2Impl)impl.Clone ();

			var data = certificate.GetRawCertData ();
			return new X509CertificateImplOpenSSL (data, MonoOpenSSLX509Format.DER, false);
		}

		internal X509Certificate2Impl GetNativeCertificate (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags flags)
		{
			var impl = new X509CertificateImplOpenSSL (false);
			impl.Import (data, password, flags);
			return impl;
		}

		internal static MonoOpenSSLX509VerifyParam GetVerifyParam (MonoTlsSettings settings, string targetHost, bool serverMode)
		{
			MonoOpenSSLX509VerifyParam param;
			if (serverMode)
				param = MonoOpenSSLX509VerifyParam.GetSslClient ();
			else
				param = MonoOpenSSLX509VerifyParam.GetSslServer ();

			if (targetHost == null && settings?.CertificateValidationTime == null)
				return param;

			try {
				var copy = param.Copy ();
				if (targetHost != null)
					copy.SetHost (targetHost);
				if (settings?.CertificateValidationTime != null)
					copy.SetTime (settings.CertificateValidationTime.Value);
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
				var chainImpl = (X509ChainImplOpenSSL)chain.Impl;
				var success = chainImpl.StoreCtx.VerifyResult == 1;
				CheckValidationResult (
					validator, targetHost, serverMode, certificates,
					wantsChain, chain, chainImpl.StoreCtx,
					success, ref errors, ref status11);
				return success;
			}

			using (var store = new MonoOpenSSLX509Store ())
			using (var nativeChain = MonoOpenSSLProvider.GetNativeChain (certificates))
			using (var param = GetVerifyParam (validator.Settings, targetHost, serverMode))
			using (var storeCtx = new MonoOpenSSLX509StoreCtx ()) {
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

		internal static bool ValidateCertificate (MonoOpenSSLX509Chain chain, MonoOpenSSLX509VerifyParam param)
		{
			using (var store = new MonoOpenSSLX509Store ())
			using (var storeCtx = new MonoOpenSSLX509StoreCtx ()) {
				/*
				 * We're called from X509Certificate2.Verify() via X509CertificateImplOpenSSL.Verify().
				 *
				 * Use the default settings and assume client-mode.
				 */
				SetupCertificateStore (store, MonoTlsSettings.DefaultSettings, false);

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
			X509Chain chain, MonoOpenSSLX509StoreCtx storeCtx,
			bool success, ref MonoSslPolicyErrors errors, ref int status11)
		{
			status11 = unchecked((int)0);
			if (success)
				return;
			errors = MonoSslPolicyErrors.RemoteCertificateChainErrors;
			if (!wantsChain || storeCtx == null || chain == null) {
				status11 = unchecked((int)0x800B010B);
				return;
			}
			var error = storeCtx.GetError();
			if (error != Mono.OpenSSL.MonoOpenSSLX509Error.OK &
			    error != Mono.OpenSSL.MonoOpenSSLX509Error.CRL_NOT_YET_VALID) {
				chain.Impl.AddStatus(MapVerifyErrorToChainStatus(error));
				status11 = unchecked((int)0x800B010B);
			}
		}

		internal static X509ChainStatusFlags MapVerifyErrorToChainStatus (MonoOpenSSLX509Error code)
		{
			switch (code) {
			case MonoOpenSSLX509Error.OK :
				return X509ChainStatusFlags.NoError;

			case MonoOpenSSLX509Error.CERT_NOT_YET_VALID :
			case MonoOpenSSLX509Error.CERT_HAS_EXPIRED:
			case MonoOpenSSLX509Error.ERROR_IN_CERT_NOT_BEFORE_FIELD:
			case MonoOpenSSLX509Error.ERROR_IN_CERT_NOT_AFTER_FIELD:
				return X509ChainStatusFlags.NotTimeValid;

			case MonoOpenSSLX509Error.CERT_REVOKED:
				return X509ChainStatusFlags.Revoked;

			case MonoOpenSSLX509Error.UNABLE_TO_DECODE_ISSUER_PUBLIC_KEY:
			case MonoOpenSSLX509Error.CERT_SIGNATURE_FAILURE:
				return X509ChainStatusFlags.NotSignatureValid;

			case MonoOpenSSLX509Error.CERT_UNTRUSTED:
			case MonoOpenSSLX509Error.DEPTH_ZERO_SELF_SIGNED_CERT:
			case MonoOpenSSLX509Error.SELF_SIGNED_CERT_IN_CHAIN:
				return X509ChainStatusFlags.UntrustedRoot;

			case MonoOpenSSLX509Error.CRL_HAS_EXPIRED:
				return X509ChainStatusFlags.OfflineRevocation;

			case MonoOpenSSLX509Error.CRL_NOT_YET_VALID:
			case MonoOpenSSLX509Error.CRL_SIGNATURE_FAILURE:
			case MonoOpenSSLX509Error.ERROR_IN_CRL_LAST_UPDATE_FIELD:
			case MonoOpenSSLX509Error.ERROR_IN_CRL_NEXT_UPDATE_FIELD:
			case MonoOpenSSLX509Error.KEYUSAGE_NO_CRL_SIGN:
			case MonoOpenSSLX509Error.UNABLE_TO_DECRYPT_CRL_SIGNATURE:
			case MonoOpenSSLX509Error.UNABLE_TO_GET_CRL:
			case MonoOpenSSLX509Error.UNABLE_TO_GET_CRL_ISSUER:
			case MonoOpenSSLX509Error.UNHANDLED_CRITICAL_CRL_EXTENSION:
				return X509ChainStatusFlags.RevocationStatusUnknown;

			case MonoOpenSSLX509Error.INVALID_EXTENSION:
				return X509ChainStatusFlags.InvalidExtension;

			case MonoOpenSSLX509Error.UNABLE_TO_GET_ISSUER_CERT:
			case MonoOpenSSLX509Error.UNABLE_TO_GET_ISSUER_CERT_LOCALLY:
			case MonoOpenSSLX509Error.UNABLE_TO_VERIFY_LEAF_SIGNATURE:
				return X509ChainStatusFlags.PartialChain;

			case MonoOpenSSLX509Error.INVALID_PURPOSE:
				return X509ChainStatusFlags.NotValidForUsage;

			case MonoOpenSSLX509Error.INVALID_CA:
			case MonoOpenSSLX509Error.INVALID_NON_CA:
			case MonoOpenSSLX509Error.PATH_LENGTH_EXCEEDED:
			case MonoOpenSSLX509Error.KEYUSAGE_NO_CERTSIGN:
			case MonoOpenSSLX509Error.KEYUSAGE_NO_DIGITAL_SIGNATURE:
				return X509ChainStatusFlags.InvalidBasicConstraints;

			case MonoOpenSSLX509Error.INVALID_POLICY_EXTENSION:
			case MonoOpenSSLX509Error.NO_EXPLICIT_POLICY:
				return X509ChainStatusFlags.InvalidPolicyConstraints;

			case MonoOpenSSLX509Error.CERT_REJECTED:
				return X509ChainStatusFlags.ExplicitDistrust;

			case MonoOpenSSLX509Error.UNHANDLED_CRITICAL_EXTENSION:
				return X509ChainStatusFlags.HasNotSupportedCriticalExtension;

			case MonoOpenSSLX509Error.HOSTNAME_MISMATCH:
				// FIXME: we should have a better error flag for this.
				return X509ChainStatusFlags.UntrustedRoot;

			case MonoOpenSSLX509Error.CERT_CHAIN_TOO_LONG:
				throw new CryptographicException ();

			case MonoOpenSSLX509Error.OUT_OF_MEM:
				throw new OutOfMemoryException ();

			default:
				throw new CryptographicException ("Unrecognized X509VerifyStatusCode:" + code);
			}
		}

		internal static void SetupCertificateStore (MonoOpenSSLX509Store store, MonoTlsSettings settings, bool server)
		{
			/*
			 * In server-mode, we only add certificates which are explicitly trusted via
			 * MonoTlsSettings.TrustAnchors.
			 * 
			 * MonoTlsSettings.CertificateSearchPaths is ignored on Android.
			 * 
			 */

#if MONODROID
			AddTrustedRoots (store, settings, server);
			if (!server)
				SetupDefaultCertificateStore (store);
			return;
#else
			if (server || settings?.CertificateSearchPaths == null) {
				AddTrustedRoots (store, settings, server);
				if (!server)
					SetupDefaultCertificateStore (store);
				return;
			}

			foreach (var path in settings.CertificateSearchPaths) {
				switch (path) {
				case "@default":
					AddTrustedRoots (store, settings, server);
					AddUserStore (store);
					AddMachineStore (store);
					break;
				case "@trusted":
					AddTrustedRoots (store, settings, server);
					break;
				case "@user":
					AddUserStore (store);
					break;
				case "@machine":
					AddMachineStore (store);
					break;
				default:
					if (path.StartsWith ("@pem:")) {
						var realPath = path.Substring (5);
						if (Directory.Exists (realPath))
							store.AddDirectoryLookup (realPath, MonoOpenSSLX509FileType.PEM);
						break;
					} else if (path.StartsWith ("@der:")) {
						var realPath = path.Substring (5);
						if (Directory.Exists (realPath))
							store.AddDirectoryLookup (realPath, MonoOpenSSLX509FileType.ASN1);
						break;
					}
					throw new NotSupportedException (string.Format ("Invalid item `{0}' in MonoTlsSettings.CertificateSearchPaths.", path));
				}
			}
#endif
		}

		static void SetupDefaultCertificateStore (MonoOpenSSLX509Store store)
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
		static void AddUserStore (MonoOpenSSLX509Store store)
		{
			var userPath = MonoOpenSSLX509StoreManager.GetStorePath (MonoOpenSSLX509StoreType.UserTrustedRoots);
			if (Directory.Exists (userPath))
				store.AddDirectoryLookup (userPath, MonoOpenSSLX509FileType.PEM);
		}

		static void AddMachineStore (MonoOpenSSLX509Store store)
		{
			var machinePath = MonoOpenSSLX509StoreManager.GetStorePath (MonoOpenSSLX509StoreType.MachineTrustedRoots);
			if (Directory.Exists (machinePath))
				store.AddDirectoryLookup (machinePath, MonoOpenSSLX509FileType.PEM);
		}
#endif

		static void AddTrustedRoots (MonoOpenSSLX509Store store, MonoTlsSettings settings, bool server)
		{
			if (settings?.TrustAnchors == null)
				return;
			var trust = server ? MonoOpenSSLX509TrustKind.TRUST_CLIENT : MonoOpenSSLX509TrustKind.TRUST_SERVER;
			store.AddCollection (settings.TrustAnchors, trust);
		}

		public static string GetSystemStoreLocation ()
		{
#if MONODROID
			return "/system/etc/security/cacerts";
#else
			return MonoOpenSSLX509StoreManager.GetStorePath (MonoOpenSSLX509StoreType.MachineTrustedRoots);
#endif
		}

		public static X509Certificate CreateCertificate (byte[] data, MonoOpenSSLX509Format format, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplOpenSSL (data, format, disallowFallback)) {
				return new X509Certificate (impl);
			}
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, MonoOpenSSLX509Format format, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplOpenSSL (data, format, disallowFallback)) {
				return new X509Certificate2 (impl);
			}
		}

		public static X509Certificate2 CreateCertificate2 (byte[] data, string password, bool disallowFallback = false)
		{
			using (var impl = new X509CertificateImplOpenSSL (disallowFallback))
			using (var handle = new SafePasswordHandle (password)) {
				impl.Import (data, handle, X509KeyStorageFlags.DefaultKeySet);
				return new X509Certificate2 (impl);
			}
		}

		public static X509Certificate CreateCertificate (MonoOpenSSLX509 x509)
		{
			using (var impl = new X509CertificateImplOpenSSL (x509, true))
				return new X509Certificate (impl);
		}

		public static X509Chain CreateChain ()
		{
			using (var impl = new X509ChainImplOpenSSL ())
				return new X509Chain (impl);
		}

		public static X509Chain GetManagedChain (MonoOpenSSLX509Chain chain)
		{
			var impl = new X509ChainImplOpenSSL (chain);
			return new X509Chain (impl);
		}

		public static MonoOpenSSLX509 GetOpenSSLCertificate (X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplOpenSSL;
			if (impl != null)
				return impl.X509.Copy ();

			return MonoOpenSSLX509.LoadFromData (certificate.GetRawCertData (), MonoOpenSSLX509Format.DER);
		}

		public static MonoOpenSSLX509Chain GetNativeChain (X509CertificateCollection certificates)
		{
			var chain = new MonoOpenSSLX509Chain ();
			try {
				foreach (var cert in certificates) {
					using (var x509 = GetOpenSSLCertificate (cert))
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
