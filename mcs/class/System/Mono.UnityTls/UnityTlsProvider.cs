#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.Text;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

using MNS = Mono.Net.Security;
#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif
using Mono.Util;

using size_t = System.IntPtr;

namespace Mono.Unity
{
	unsafe internal class UnityTlsProvider : MNS.MobileTlsProvider
	{
		public override string Name {
			get { return "unitytls"; }
		}

		public override Guid ID => MNS.MonoTlsProviderFactory.UnityTlsId;
		public override bool SupportsSslStream => true;
		public override bool SupportsMonoExtensions => true;
		public override bool SupportsConnectionInfo => true;
		internal override bool SupportsCleanShutdown => true;
		public override SslProtocols SupportedProtocols => SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;

		internal override MNS.MobileAuthenticatedStream CreateSslStream (
			SslStream sslStream, Stream innerStream, bool leaveInnerStreamOpen,
			MonoTlsSettings settings)
		{
			return new UnityTlsStream (innerStream, leaveInnerStreamOpen, sslStream, settings, this);
		}

		[MonoPInvokeCallback (typeof (UnityTls.unitytls_x509verify_callback))]
		static UnityTls.unitytls_x509verify_result x509verify_callback(void* userData, UnityTls.unitytls_x509_ref cert, UnityTls.unitytls_x509verify_result result, UnityTls.unitytls_errorstate* errorState)
		{
			if (userData != null)
				UnityTls.NativeInterface.unitytls_x509list_append ((UnityTls.unitytls_x509list*)userData, cert, errorState);
			return result;
		}

		internal override bool ValidateCertificate (
			MNS.ChainValidationHelper validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref SslPolicyErrors errors, ref int status11)
		{
			var errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();

			var unityTlsChainImpl = chain.Impl as X509ChainImplUnityTls;
			if (unityTlsChainImpl == null)
			{
				if (certificates == null || certificates.Count == 0) {
					errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
					return false;
				}
			}
			else
			{
				var cert = UnityTls.NativeInterface.unitytls_x509list_get_x509 (unityTlsChainImpl.NativeCertificateChain, (size_t)0, &errorState);
				if (cert.handle == UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE) {
					errors |= SslPolicyErrors.RemoteCertificateNotAvailable;
					return false;
				}
			}

			// fixup targetHost name by removing port
			if (!string.IsNullOrEmpty (targetHost)) {
				var pos = targetHost.IndexOf (':');
				if (pos > 0)
					targetHost = targetHost.Substring (0, pos);
			}
			else if (targetHost == null)
			{
				targetHost = "";
			}

			// convert cert to native or extract from unityTlsChainImpl.
			var result = UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_NOT_DONE;
			UnityTls.unitytls_x509list* certificatesNative = null;
			UnityTls.unitytls_x509list* finalCertificateChainNative = UnityTls.NativeInterface.unitytls_x509list_create (&errorState);
			try
			{
				// Things the validator provides that we might want to make use of here:
				//validator.Settings.CheckCertificateName				// not used by mono?
				//validator.Settings.CheckCertificateRevocationStatus	// not used by mono?
				//validator.Settings.CertificateValidationTime
				//validator.Settings.CertificateSearchPaths				// currently only used by MonoBtlsProvider

				UnityTls.unitytls_x509list_ref certificatesNativeRef;
				if (unityTlsChainImpl == null)
				{
					certificatesNative = UnityTls.NativeInterface.unitytls_x509list_create (&errorState);
					CertHelper.AddCertificatesToNativeChain (certificatesNative, certificates, &errorState);
					certificatesNativeRef = UnityTls.NativeInterface.unitytls_x509list_get_ref (certificatesNative, &errorState);
				}
				else
					certificatesNativeRef = unityTlsChainImpl.NativeCertificateChain;
				
				var targetHostUtf8 = Encoding.UTF8.GetBytes (targetHost);

				if (validator.Settings.TrustAnchors != null) {
					UnityTls.unitytls_x509list* trustCAnative = null;
					try
					{
						trustCAnative = UnityTls.NativeInterface.unitytls_x509list_create (&errorState);
						CertHelper.AddCertificatesToNativeChain (trustCAnative, validator.Settings.TrustAnchors, &errorState);
						var trustCAnativeRef = UnityTls.NativeInterface.unitytls_x509list_get_ref (trustCAnative, &errorState);

						fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
							result = UnityTls.NativeInterface.unitytls_x509verify_explicit_ca (
								certificatesNativeRef, trustCAnativeRef, targetHostUtf8Ptr, (size_t)targetHostUtf8.Length, x509verify_callback, finalCertificateChainNative, &errorState);
						}
					}
					finally {
						UnityTls.NativeInterface.unitytls_x509list_free (trustCAnative);
					}
				} else {
					fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
						result = UnityTls.NativeInterface.unitytls_x509verify_default_ca (
							certificatesNativeRef, targetHostUtf8Ptr, (size_t)targetHostUtf8.Length, x509verify_callback, finalCertificateChainNative, &errorState);
					}
				}
			}
			catch {
				UnityTls.NativeInterface.unitytls_x509list_free (finalCertificateChainNative);
				throw;
			}
			finally	{
				UnityTls.NativeInterface.unitytls_x509list_free (certificatesNative);
			}

			chain?.Dispose();
			var chainImpl = new X509ChainImplUnityTls(
				UnityTls.NativeInterface.unitytls_x509list_get_ref (finalCertificateChainNative, &errorState),
				reverseOrder: true // the verify callback starts with the root and ends with the leaf. That's the opposite of chain ordering.
			);
			chain = new X509Chain(chainImpl);

			errors = UnityTlsConversions.VerifyResultToPolicyErrror(result);
			// There should be a status per certificate, but once again we're following closely the BTLS implementation
			// https://github.com/mono/mono/blob/1553889bc54f87060158febca7e6b8b9910975f8/mcs/class/System/Mono.Btls/MonoBtlsProvider.cs#L180
			// which also provides only a single status for the entire chain.
			// It is notoriously tricky to implement in OpenSSL to get a status for all invididual certificates without finishing the handshake in the process.
			// This is partially the reason why unitytls_x509verify_X doesn't expose it (TODO!) and likely the reason Mono's BTLS impl ignores this.
			chainImpl.AddStatus(UnityTlsConversions.VerifyResultToChainStatus(result));
			return result == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS && 
					errorState.code == UnityTls.unitytls_error_code.UNITYTLS_SUCCESS;
		}
	}
}
#endif
