#if SECURITY_DEP

using System;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using size_t = System.IntPtr;

namespace Mono.Unity
{
	// Follows mostly X509ChainImplBtls
	class X509ChainImplUnityTls : X509ChainImpl
	{
		X509ChainElementCollection elements;
		UnityTls.unitytls_x509list_ref nativeCertificateChain;
		X509ChainPolicy policy = new X509ChainPolicy ();

		internal X509ChainImplUnityTls (UnityTls.unitytls_x509list_ref nativeCertificateChain)
		{
			this.elements = null;
			this.nativeCertificateChain = nativeCertificateChain;
		}

		public override bool IsValid {
			get { return nativeCertificateChain.handle != UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE; }
		}

		public override IntPtr Handle {
			get { return new IntPtr((long)nativeCertificateChain.handle); }
		}

		internal UnityTls.unitytls_x509list_ref NativeCertificateChain => nativeCertificateChain;

		public override X509ChainElementCollection ChainElements {
			get {
				ThrowIfContextInvalid ();
				if (elements != null)
					return elements;

				unsafe
				{
					elements = new X509ChainElementCollection ();
					UnityTls.unitytls_errorstate errorState = UnityTls.NativeInterface.unitytls_errorstate_create ();
					var cert = UnityTls.NativeInterface.unitytls_x509list_get_x509 (nativeCertificateChain, (size_t)0, &errorState);
					for (int i = 0; cert.handle != UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE; ++i) {
						size_t certBufferSize = UnityTls.NativeInterface.unitytls_x509_export_der (cert, null, (size_t)0, &errorState);
						var certBuffer = new byte[(int)certBufferSize];	// Need to reallocate every time since X509Certificate constructor takes no length but only a byte array.
						fixed(byte* certBufferPtr = certBuffer) {
							UnityTls.NativeInterface.unitytls_x509_export_der (cert, certBufferPtr, certBufferSize, &errorState);
						}
						elements.Add (new X509Certificate2 (certBuffer));

						cert = UnityTls.NativeInterface.unitytls_x509list_get_x509 (nativeCertificateChain, (size_t)i, &errorState);
					}
				}

				return elements;
			}
		}

		public override void AddStatus (X509ChainStatusFlags error) 
		{
		}

		public override X509ChainPolicy ChainPolicy {
			get { return policy; }
			set { policy = value; }
		}

		public override X509ChainStatus[] ChainStatus {
			get { throw new NotImplementedException (); }
		}

		public override bool Build (X509Certificate2 certificate)
		{
			return false;
		}

		public override void Reset ()
		{
			if (elements != null) {
				nativeCertificateChain.handle = UnityTls.NativeInterface.UNITYTLS_INVALID_HANDLE;
				elements.Clear ();
				elements = null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			Reset();
			base.Dispose (disposing);
		}
	}
}

#endif
