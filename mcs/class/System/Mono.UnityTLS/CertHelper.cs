#if SECURITY_DEP
using System.Security.Cryptography.X509Certificates;

namespace Mono.Unity
{
	internal unsafe static class CertHelper
	{
		public static void AddCertificatesToNativeChain(UnityTls.unitytls_x509list* nativeCertificateChain, X509CertificateCollection certificates, UnityTls.unitytls_errorstate* errorState)
		{
			foreach (var certificate in certificates) {
				AddCertificateToNativeChain(nativeCertificateChain, certificate, errorState);
			}
		}

		public static void AddCertificateToNativeChain(UnityTls.unitytls_x509list* nativeCertificateChain, X509Certificate certificate, UnityTls.unitytls_errorstate* errorState)
		{
			byte[] certDer = certificate.GetRawCertData();
			fixed(byte* certDerPtr = certDer) {
				UnityTls.unitytls_x509list_append_der(nativeCertificateChain, certDerPtr, certDer.Length, errorState);
			}

			var certificateImpl2 = certificate.Impl as X509Certificate2Impl;
			if (certificateImpl2 != null) {
				var intermediates = certificateImpl2.IntermediateCertificates;
				if (intermediates != null && intermediates.Count > 0) {
					for (int i=0; i<intermediates.Count; ++i) {
						AddCertificateToNativeChain(nativeCertificateChain, new X509Certificate(intermediates[i]), errorState);
					}
				}
			}
		}

		public static X509CertificateCollection NativeChainToManagedCollection(UnityTls.unitytls_x509list_ref nativeCertificateChain, UnityTls.unitytls_errorstate* errorState)
		{
			X509CertificateCollection certificates = new X509CertificateCollection ();

			// TODO: Should iterate until we get an invalid handle instead. Can't acces INVALID_HANDLE yet though due to limitations of UnityTls interface
			size_t numCerts = UnityTls.unitytls_x509list_get_size (nativeCertificateChain, errorState);
			for(int i=0; i<numCerts; ++i) {
				UnityTls.unitytls_x509_ref cert = UnityTls.unitytls_x509list_get_x509 (nativeCertificateChain, i, errorState);
				size_t certBufferSize = UnityTls.unitytls_x509_export_pem (cert, null, 0, errorState);
				var certBuffer = new byte[certBufferSize];	// Need to reallocate every time since X509Certificate constructor takes no length but only a byte array.
				
				fixed(byte* certBufferPtr = certBuffer) {
					UnityTls.unitytls_x509_export_pem (cert, certBufferPtr, certBufferSize, errorState);
				}
				
				certificates.Add (new X509Certificate (certBuffer));
			}

			return certificates;
		}
	}
}
#endif