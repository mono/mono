using System;
using System.Runtime.InteropServices;
using MX = Mono.Security.X509;
using XamMac.CoreFoundation;

namespace System.Security.Cryptography.X509Certificates
{
	static partial class X509Helper
	{
		public static X509CertificateImpl InitFromHandle (IntPtr handle)
		{
			return new X509CertificateImplApple (handle, false);
		}

		public static X509CertificateImpl Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			MX.X509Certificate x509;
			IntPtr handle;
			if (password == null) {
				handle = CFHelpers.CreateCertificateFromData (rawData);
				if (handle != IntPtr.Zero)
					return new X509CertificateImplApple (handle, true);

				try {
					x509 = new MX.X509Certificate (rawData);
				} catch (Exception e) {
					try {
						x509 = X509Helper.ImportPkcs12 (rawData, null);
					} catch {
						string msg = Locale.GetText ("Unable to decode certificate.");
						// inner exception is the original (not second) exception
						throw new CryptographicException (msg, e);
					}
				}
			} else {
				// try PKCS#12
				try {
					x509 = X509Helper.ImportPkcs12 (rawData, password);
				}
				catch {
					// it's possible to supply a (unrequired/unusued) password
					// fix bug #79028
					x509 = new MX.X509Certificate (rawData);
				}
			}

			return new X509CertificateImplMono (x509);
		}
	}
}
