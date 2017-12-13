#if SECURITY_DEP
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;

#if MONO_SECURITY_ALIAS
using MonoSecurity::Mono.Security.Interface;
#else
using Mono.Security.Interface;
#endif

using MNS = Mono.Net.Security;

namespace Mono.Unity
{
	unsafe internal class UnityTlsProvider : MonoTlsProvider
	{
		static readonly Guid id = new Guid("06414A97-74F6-488F-877B-A6CA9BBEB82E");

		public override Guid ID {
			get { return id; }
		}
		public override string Name {
			get { return "unitytls"; }
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
			return new UnityTlsStream (innerStream, leaveInnerStreamOpen, settings, this);
		}

		internal override bool ValidateCertificate (
			ICertificateValidator2 validator, string targetHost, bool serverMode,
			X509CertificateCollection certificates, bool wantsChain, ref X509Chain chain,
			ref MonoSslPolicyErrors errors, ref int status11)
		{
			if (wantsChain)
				chain = MNS.SystemCertificateValidator.CreateX509Chain (certificates);

			if (certificates == null || certificates.Count == 0) {
				errors |= MonoSslPolicyErrors.RemoteCertificateNotAvailable;
				return false;
			}

			// fixup targetHost name by removing port
			if (!string.IsNullOrEmpty (targetHost)) {
				var pos = targetHost.IndexOf (':');
				if (pos > 0)
					targetHost = targetHost.Substring (0, pos);
			}

			// convert cert to native
			UnityTls.unitytls_errorstate errorState = UnityTls.unitytls_errorstate_create();
			UnityTls.unitytls_x509verify_result result;

			UnityTls.unitytls_x509list* certificatesNative = UnityTls.unitytls_x509list_create(&errorState);
			try
			{
				foreach (X509Certificate certificate in certificates) {
					byte[] certDer = certificate.GetRawCertData();
					fixed(byte* certDerPtr = certDer) {
						UnityTls.unitytls_x509list_append_der(certificatesNative, certDerPtr, certDer.Length, &errorState);
					}
				}

				// validate
				UnityTls.unitytls_x509list_ref certificatesNativeRef = UnityTls.unitytls_x509list_get_ref(certificatesNative, &errorState);
				byte[] targetHostUtf8 = Encoding.UTF8.GetBytes(targetHost);
				fixed (byte* targetHostUtf8Ptr = targetHostUtf8) {
					result = UnityTls.unitytls_x509verify_default_ca(certificatesNativeRef, targetHostUtf8Ptr, targetHostUtf8.Length, null, null, &errorState);
				}
			}
			finally
			{
				UnityTls.unitytls_x509list_free(certificatesNative);
			}

			return result == UnityTls.unitytls_x509verify_result.UNITYTLS_X509VERIFY_SUCCESS && 
					errorState.code == UnityTls.unitytls_error_code.UNITYTLS_SUCCESS;
		}
	}
}
#endif
