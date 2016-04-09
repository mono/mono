#if MONOTOUCH || XAMMAC

// this file is a shim to enable compiling monotouch profiles without mono-extensions
namespace System.Security.Cryptography.X509Certificates
{
	static partial class X509Helper
	{
		public static X509CertificateImpl InitFromHandle (IntPtr handle)
		{
			throw new NotSupportedException ();
		}

		public static X509CertificateImpl Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			throw new NotSupportedException ();
		}
	}
}

#endif
