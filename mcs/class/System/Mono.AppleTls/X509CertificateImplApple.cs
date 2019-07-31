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
	class X509CertificateImplApple : X509Certificate2ImplUnix
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

		protected override byte[] GetRawCertData ()
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

		#region X509Certificate2Impl implementation

		/*
		 * The AppleTls backend does not support X509Certificate2 yet, so we can safely throw
		 * PlatformNotSupportedException here.
		 */

		public override bool HasPrivateKey => throw new PlatformNotSupportedException ();

		public override AsymmetricAlgorithm PrivateKey {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public override RSA GetRSAPrivateKey ()
		{
			throw new PlatformNotSupportedException ();
		}

		public override DSA GetDSAPrivateKey ()
		{
			throw new PlatformNotSupportedException ();
		}

		public override PublicKey PublicKey => throw new PlatformNotSupportedException ();

		internal override X509CertificateImplCollection IntermediateCertificates => throw new PlatformNotSupportedException ();

		internal override X509Certificate2Impl FallbackImpl => throw new PlatformNotSupportedException ();

		public override bool Verify (X509Certificate2 thisCertificate)
		{
			throw new PlatformNotSupportedException ();
		}

		#endregion

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
