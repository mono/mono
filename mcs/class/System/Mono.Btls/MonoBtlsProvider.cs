//
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

namespace Mono.Btls
{
	class MonoBtlsProvider : MNS.MobileTlsProvider
	{
		public override Guid ID {
			get { return MNS.MonoTlsProviderFactory.BtlsId; }
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

		internal override bool SupportsCleanShutdown {
			get { return true; }
		}

		public override SslProtocols SupportedProtocols {
			get { return SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls; }
		}

		internal override MNS.MobileAuthenticatedStream CreateSslStream (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings)
		{
			return new MonoBtlsStream (
				innerStream, leaveInnerStreamOpen, sslStream, settings, this);
		}

		internal override bool HasNativeCertificates {
			get { return true; }
		}

		internal X509Certificate2Impl GetNativeCertificate (
			byte[] data, string password, X509KeyStorageFlags flags)
		{
			using (var handle = new SafePasswordHandle (password))
				return GetNativeCertificate (data, handle, flags);
		}

		internal X509Certificate2Impl GetNativeCertificate (
			X509Certificate certificate)
		{
			var impl = certificate.Impl as X509CertificateImplBtls;
			if (impl != null)
				return (X509Certificate2Impl)impl.Clone ();

			var data = certificate.GetRawCertData ();
			return new X509CertificateImplBtls (data, MonoBtlsX509Format.DER);
		}

		internal X509Certificate2Impl GetNativeCertificate (
			byte[] data, SafePasswordHandle password, X509KeyStorageFlags flags)
		{
			return new X509CertificateImplBtls (data, password, flags);
		}

		internal static MonoBtlsX509VerifyParam GetVerifyParam (MonoTlsSettings settings, string targetHost, bool serverMode)
		{
			MonoBtlsX509VerifyParam param;
			if (serverMode)
				param = MonoBtlsX509VerifyParam.GetSslClient ();
			else
				param = MonoBtlsX509VerifyParam.GetSslServer ();

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
			MNS.ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref SslPolicyErrors errors, ref int status11)
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
			using (var param = GetVerifyParam (validator.Settings, targetHost, serverMode))
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
				/*
				 * We're called from X509Certificate2.Verify() via X509CertificateImplBtls.Verify().
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
			MNS.ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain,
			X509Chain chain, MonoBtlsX509StoreCtx storeCtx,
			bool success, ref SslPolicyErrors errors, ref int status11)
		{
			status11 = unchecked((int)0);
			if (success)
				return;
			errors = SslPolicyErrors.RemoteCertificateChainErrors;
			if (!wantsChain || storeCtx == null || chain == null) {
				status11 = unchecked((int)0x800B010B);
				return;
			}
			var error = storeCtx.GetError ();
			switch (error) {
			case Mono.Btls.MonoBtlsX509Error.OK:
				errors = SslPolicyErrors.None;
				break;
			case Mono.Btls.MonoBtlsX509Error.CRL_NOT_YET_VALID:
				break;
			case MonoBtlsX509Error.HOSTNAME_MISMATCH:
				errors = SslPolicyErrors.RemoteCertificateNameMismatch;
				chain.Impl.AddStatus (X509ChainStatusFlags.UntrustedRoot);
				status11 = unchecked ((int)0x800B010B);
				break;

			default:
				chain.Impl.AddStatus (MapVerifyErrorToChainStatus (error));
				status11 = unchecked ((int)0x800B010B);
				break;
			}
		}

		internal static X509ChainStatusFlags MapVerifyErrorToChainStatus (MonoBtlsX509Error code)
		{
			switch (code) {
			case MonoBtlsX509Error.OK:
				return X509ChainStatusFlags.NoError;

			case MonoBtlsX509Error.CERT_NOT_YET_VALID:
			case MonoBtlsX509Error.CERT_HAS_EXPIRED:
			case MonoBtlsX509Error.ERROR_IN_CERT_NOT_BEFORE_FIELD:
			case MonoBtlsX509Error.ERROR_IN_CERT_NOT_AFTER_FIELD:
				return X509ChainStatusFlags.NotTimeValid;

			case MonoBtlsX509Error.CERT_REVOKED:
				return X509ChainStatusFlags.Revoked;

			case MonoBtlsX509Error.UNABLE_TO_DECODE_ISSUER_PUBLIC_KEY:
			case MonoBtlsX509Error.CERT_SIGNATURE_FAILURE:
				return X509ChainStatusFlags.NotSignatureValid;

			case MonoBtlsX509Error.CERT_UNTRUSTED:
			case MonoBtlsX509Error.DEPTH_ZERO_SELF_SIGNED_CERT:
			case MonoBtlsX509Error.SELF_SIGNED_CERT_IN_CHAIN:
				return X509ChainStatusFlags.UntrustedRoot;

			case MonoBtlsX509Error.CRL_HAS_EXPIRED:
				return X509ChainStatusFlags.OfflineRevocation;

			case MonoBtlsX509Error.CRL_NOT_YET_VALID:
			case MonoBtlsX509Error.CRL_SIGNATURE_FAILURE:
			case MonoBtlsX509Error.ERROR_IN_CRL_LAST_UPDATE_FIELD:
			case MonoBtlsX509Error.ERROR_IN_CRL_NEXT_UPDATE_FIELD:
			case MonoBtlsX509Error.KEYUSAGE_NO_CRL_SIGN:
			case MonoBtlsX509Error.UNABLE_TO_DECRYPT_CRL_SIGNATURE:
			case MonoBtlsX509Error.UNABLE_TO_GET_CRL:
			case MonoBtlsX509Error.UNABLE_TO_GET_CRL_ISSUER:
			case MonoBtlsX509Error.UNHANDLED_CRITICAL_CRL_EXTENSION:
				return X509ChainStatusFlags.RevocationStatusUnknown;

			case MonoBtlsX509Error.INVALID_EXTENSION:
				return X509ChainStatusFlags.InvalidExtension;

			case MonoBtlsX509Error.UNABLE_TO_GET_ISSUER_CERT:
			case MonoBtlsX509Error.UNABLE_TO_GET_ISSUER_CERT_LOCALLY:
			case MonoBtlsX509Error.UNABLE_TO_VERIFY_LEAF_SIGNATURE:
				return X509ChainStatusFlags.PartialChain;

			case MonoBtlsX509Error.INVALID_PURPOSE:
				return X509ChainStatusFlags.NotValidForUsage;

			case MonoBtlsX509Error.INVALID_CA:
			case MonoBtlsX509Error.INVALID_NON_CA:
			case MonoBtlsX509Error.PATH_LENGTH_EXCEEDED:
			case MonoBtlsX509Error.KEYUSAGE_NO_CERTSIGN:
			case MonoBtlsX509Error.KEYUSAGE_NO_DIGITAL_SIGNATURE:
				return X509ChainStatusFlags.InvalidBasicConstraints;

			case MonoBtlsX509Error.INVALID_POLICY_EXTENSION:
			case MonoBtlsX509Error.NO_EXPLICIT_POLICY:
				return X509ChainStatusFlags.InvalidPolicyConstraints;

			case MonoBtlsX509Error.CERT_REJECTED:
				return X509ChainStatusFlags.ExplicitDistrust;

			case MonoBtlsX509Error.UNHANDLED_CRITICAL_EXTENSION:
				return X509ChainStatusFlags.HasNotSupportedCriticalExtension;

			case MonoBtlsX509Error.HOSTNAME_MISMATCH:
				// FIXME: we should have a better error flag for this.
				return X509ChainStatusFlags.UntrustedRoot;

			case MonoBtlsX509Error.CERT_CHAIN_TOO_LONG:
				throw new CryptographicException ();

			case MonoBtlsX509Error.OUT_OF_MEM:
				throw new OutOfMemoryException ();

			default:
				throw new CryptographicException ("Unrecognized X509VerifyStatusCode:" + code);
			}
		}

		internal static void SetupCertificateStore (MonoBtlsX509Store store, MonoTlsSettings settings, bool server)
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
							store.AddDirectoryLookup (realPath, MonoBtlsX509FileType.PEM);
						break;
					} else if (path.StartsWith ("@der:")) {
						var realPath = path.Substring (5);
						if (Directory.Exists (realPath))
							store.AddDirectoryLookup (realPath, MonoBtlsX509FileType.ASN1);
						break;
					}
					throw new NotSupportedException (string.Format ("Invalid item `{0}' in MonoTlsSettings.CertificateSearchPaths.", path));
				}
			}
#endif
		}

		static void SetupDefaultCertificateStore (MonoBtlsX509Store store)
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
#endif

		static void AddTrustedRoots (MonoBtlsX509Store store, MonoTlsSettings settings, bool server)
		{
			if (settings?.TrustAnchors == null)
				return;
			var trust = server ? MonoBtlsX509TrustKind.TRUST_CLIENT : MonoBtlsX509TrustKind.TRUST_SERVER;
			store.AddCollection (settings.TrustAnchors, trust);
		}

		public static string GetSystemStoreLocation ()
		{
#if MONODROID
			return "/system/etc/security/cacerts";
#else
			return MonoBtlsX509StoreManager.GetStorePath (MonoBtlsX509StoreType.MachineTrustedRoots);
#endif
		}

		public static X509Certificate2 CreateCertificate (byte[] data, MonoBtlsX509Format format)
		{
			using (var impl = new X509CertificateImplBtls (data, format)) {
				return new X509Certificate2 (impl);
			}
		}

		public static X509Certificate2 CreateCertificate (byte[] data, string password, bool disallowFallback = false)
		{
			using (var handle = new SafePasswordHandle (password))
			using (var impl = new X509CertificateImplBtls (data, handle, X509KeyStorageFlags.DefaultKeySet))
				return new X509Certificate2 (impl);
		}

		public static X509Certificate2 CreateCertificate (MonoBtlsX509 x509)
		{
			using (var impl = new X509CertificateImplBtls (x509))
				return new X509Certificate2 (impl);
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
