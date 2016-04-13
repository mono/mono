using System;
using System.Text;
using System.Runtime.InteropServices;
using XamMac.CoreFoundation;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates
{
	class X509CertificateImplApple : X509CertificateImpl
	{
		IntPtr handle;
		X509CertificateImpl fallback;

		public X509CertificateImplApple (IntPtr handle, bool owns)
		{
			this.handle = handle;
			if (!owns)
				CFHelpers.CFRetain (handle);
		}

		public override bool IsValid {
			get { return handle != IntPtr.Zero; }
		}

		public override IntPtr Handle {
			get { return handle; }
		}

		public override IntPtr GetNativeAppleCertificate ()
		{
			ThrowIfContextInvalid ();
			return handle;
		}

		public override X509CertificateImpl Clone ()
		{
			ThrowIfContextInvalid ();
			return new X509CertificateImplApple (handle, false);
		}

		[DllImport (CFHelpers.SecurityLibrary)]
		extern static IntPtr SecCertificateCopySubjectSummary (IntPtr cert);

		[DllImport (CFHelpers.SecurityLibrary)]
		extern static IntPtr SecCertificateCopyData (IntPtr cert);

		public override byte[] GetRawCertData ()
		{
			ThrowIfContextInvalid ();
			var data = SecCertificateCopyData (handle);
			if (data == IntPtr.Zero)
				throw new ArgumentException ("Not a valid certificate");

			try {
				return CFHelpers.FetchDataBuffer (data);
			} finally {
				CFHelpers.CFRelease (data);
			}
		}

		public string GetSubjectSummary ()
		{
			ThrowIfContextInvalid ();
			IntPtr cfstr = SecCertificateCopySubjectSummary (handle);
			string ret = CFHelpers.FetchString (cfstr);
			CFHelpers.CFRelease (cfstr);
			return ret;
		}

		protected override byte[] GetCertHash (bool lazy)
		{
			// FIXME: might just return 'null' when 'lazy' is true.
			ThrowIfContextInvalid ();
			SHA1 sha = SHA1.Create ();
			return sha.ComputeHash (GetRawCertData ());
		}

		public override bool Equals (X509CertificateImpl other, out bool result)
		{
			var otherAppleImpl = other as X509CertificateImplApple;
			if (otherAppleImpl != null && otherAppleImpl.handle == handle) {
				result = true;
				return true;
			}

			result = false;
			return false;
		}

		void MustFallback ()
		{
			ThrowIfContextInvalid ();
			if (fallback != null)
				return;
			var mxCert = new MX.X509Certificate (GetRawCertData ());
			fallback = new X509CertificateImplMono (mxCert);
		}

		public X509CertificateImpl FallbackImpl {
			get {
				MustFallback ();
				return fallback;
			}
		}

		public override string GetSubjectName (bool legacyV1Mode)
		{
			return FallbackImpl.GetSubjectName (legacyV1Mode);
		}

		public override string GetIssuerName (bool legacyV1Mode)
		{
			return FallbackImpl.GetIssuerName (legacyV1Mode);
		}

		public override DateTime GetValidFrom ()
		{
			return FallbackImpl.GetValidFrom ();
		}

		public override DateTime GetValidUntil ()
		{
			return FallbackImpl.GetValidUntil ();
		}

		public override string GetKeyAlgorithm ()
		{
			return FallbackImpl.GetKeyAlgorithm ();
		}

		public override byte[] GetKeyAlgorithmParameters ()
		{
			return FallbackImpl.GetKeyAlgorithmParameters ();
		}

		public override byte[] GetPublicKey ()
		{
			return FallbackImpl.GetPublicKey ();
		}

		public override byte[] GetSerialNumber ()
		{
			return FallbackImpl.GetSerialNumber ();
		}

		public override byte[] Export (X509ContentType contentType, byte[] password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return GetRawCertData ();
			case X509ContentType.Pfx: // this includes Pkcs12
				// TODO
				throw new NotSupportedException ();
			case X509ContentType.SerializedCert:
				// TODO
				throw new NotSupportedException ();
			default:
				string msg = Locale.GetText ("This certificate format '{0}' cannot be exported.", contentType);
				throw new CryptographicException (msg);
			}
		}

		public override string ToString (bool full)
		{
			ThrowIfContextInvalid ();

			if (!full || fallback == null) {
				var summary = GetSubjectSummary ();
				return string.Format ("[X509Certificate: {0}]", summary);
			}

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, GetSubjectName (false));

			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, GetIssuerName (false));
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, GetValidFrom ().ToLocalTime ());
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, GetValidUntil ().ToLocalTime ());
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}", nl, X509Helper.ToHexString (GetCertHash ()));

			sb.Append (nl);
			return sb.ToString ();
		}

		protected override void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFHelpers.CFRelease (handle);
				handle = IntPtr.Zero;
			}
			if (fallback != null) {
				fallback.Dispose ();
				fallback = null;
			}
		}
	}
}
