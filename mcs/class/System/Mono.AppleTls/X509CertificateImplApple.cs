#if MONO_FEATURE_APPLETLS || MONO_FEATURE_APPLE_X509
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MX = MonoSecurity::Mono.Security.X509;
#else
using MX = Mono.Security.X509;
#endif

using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Win32.SafeHandles;
using XamMac.CoreFoundation;

namespace Mono.AppleTls
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

		public override byte[] RawData {
			get {
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
		}

		public string GetSubjectSummary ()
		{
			ThrowIfContextInvalid ();
			IntPtr cfstr = SecCertificateCopySubjectSummary (handle);
			string ret = CFHelpers.FetchString (cfstr);
			CFHelpers.CFRelease (cfstr);
			return ret;
		}

		public override byte[] Thumbprint {
			get {
				// FIXME: might just return 'null' when 'lazy' is true.
				ThrowIfContextInvalid ();
				SHA1 sha = SHA1.Create ();
				return sha.ComputeHash (RawData);
			}
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
			var mxCert = new MX.X509Certificate (RawData);
			fallback = new X509Certificate2ImplMono (mxCert);
		}

		public X509CertificateImpl FallbackImpl {
			get {
				MustFallback ();
				return fallback;
			}
		}

		public override string Subject => FallbackImpl.Subject;

		public override string Issuer => FallbackImpl.Issuer;

		public override string LegacySubject => FallbackImpl.LegacySubject;

		public override string LegacyIssuer => FallbackImpl.LegacyIssuer;

		public override DateTime NotAfter => FallbackImpl.NotAfter;

		public override DateTime NotBefore => FallbackImpl.NotBefore;

		public override string KeyAlgorithm => FallbackImpl.KeyAlgorithm;

		public override byte[] KeyAlgorithmParameters => FallbackImpl.KeyAlgorithmParameters;

		public override byte[] PublicKeyValue => FallbackImpl.PublicKeyValue;

		public override byte[] SerialNumber => FallbackImpl.SerialNumber;

		public override byte[] Export (X509ContentType contentType, SafePasswordHandle password)
		{
			ThrowIfContextInvalid ();

			switch (contentType) {
			case X509ContentType.Cert:
				return RawData;
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
#endif
