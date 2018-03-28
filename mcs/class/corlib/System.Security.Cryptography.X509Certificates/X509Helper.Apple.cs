#if MONO_FEATURE_APPLETLS || MONO_FEATURE_APPLE_X509
using System;
using System.Runtime.InteropServices;
using MX = Mono.Security.X509;
using XamMac.CoreFoundation;

namespace System.Security.Cryptography.X509Certificates
{
	static partial class X509Helper
	{
		public static X509CertificateImpl InitFromHandleApple (IntPtr handle)
		{
			return new X509CertificateImplApple (handle, false);
		}

		static X509CertificateImpl ImportApple (byte[] rawData)
		{
			var handle = CFHelpers.CreateCertificateFromData (rawData);
			if (handle != IntPtr.Zero)
				return new X509CertificateImplApple (handle, true);

			MX.X509Certificate x509;
			try {
				x509 = new MX.X509Certificate (rawData);
			} catch (Exception e) {
				try {
					x509 = ImportPkcs12 (rawData, null);
				} catch {
					string msg = Locale.GetText ("Unable to decode certificate.");
					// inner exception is the original (not second) exception
					throw new CryptographicException (msg, e);
				}
			}

			return new X509CertificateImplMono (x509);
		}
	}
}
#endif
