//
// X509Certificate.cs: Handles X.509 certificates.
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

using System.Runtime.Serialization;
using Mono.Security.Authenticode;

namespace System.Security.Cryptography.X509Certificates {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt
	
	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
	[Serializable]
#if MOBILE
	public partial class X509Certificate {
#else
	public partial class X509Certificate : IDeserializationCallback, ISerializable {
#endif
		X509CertificateImpl impl;

		private bool hideDates;
	
		// static methods
	
		public static X509Certificate CreateFromCertFile (string filename) 
		{
			byte[] data = File.ReadAllBytes (filename);
			return new X509Certificate (data);
		}

		[MonoTODO ("Incomplete - minimal validation in this version")]
		public static X509Certificate CreateFromSignedFile (string filename)
		{
			try {
				AuthenticodeDeformatter a = new AuthenticodeDeformatter (filename);
				if (a.SigningCertificate != null) {
					return new X509Certificate (a.SigningCertificate.RawData);
				}
			}
			catch (SecurityException) {
				// don't wrap SecurityException into a COMException
				throw;
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Couldn't extract digital signature from {0}.", filename);
				throw new COMException (msg, e);
			}
			throw new CryptographicException (Locale.GetText ("{0} isn't signed.", filename));
		}

		// constructors
	
		// special constructor for Publisher (and related classes).
		// Dates strings are null
		internal X509Certificate (byte[] data, bool dates) 
		{
			if (data != null) {
				Import (data, (string)null, X509KeyStorageFlags.DefaultKeySet);
				hideDates = !dates;
			}
		}
	
		public X509Certificate (byte[] data) : this (data, true)
		{
		}

		public X509Certificate (IntPtr handle) 
		{
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Invalid handle.");

			impl = X509Helper.InitFromHandle (handle);
		}

		internal X509Certificate (X509CertificateImpl impl)
		{
			if (impl == null)
				throw new ArgumentNullException ("impl");

			this.impl = X509Helper.InitFromCertificate (impl);
		}

		public X509Certificate (System.Security.Cryptography.X509Certificates.X509Certificate cert) 
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");

			impl = X509Helper.InitFromCertificate (cert);
			hideDates = false;
		}

		internal void ImportHandle (X509CertificateImpl impl)
		{
			Reset ();
			this.impl = impl;
		}

		internal X509CertificateImpl Impl {
			get {
				X509Helper.ThrowIfContextInvalid (impl);
				return impl;
			}
		}

		internal bool IsValid {
			get { return X509Helper.IsValid (impl); }
		}

		internal void ThrowIfContextInvalid ()
		{
			X509Helper.ThrowIfContextInvalid (impl);
		}

		// public methods
	
		public virtual bool Equals (System.Security.Cryptography.X509Certificates.X509Certificate other)
		{
			if (other == null) {
				return false;
			} else {
				if (!X509Helper.IsValid (other.impl)) {
					if (!X509Helper.IsValid (impl))
						return true;
					throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
				}

				return X509CertificateImpl.Equals (impl, other.impl);
			}
		}

		// LAMESPEC: This is the equivalent of the "thumbprint" that can be seen
		// in the certificate viewer of Windows. This is ALWAYS the SHA1 hash of
		// the certificate (i.e. it has nothing to do with the actual hash 
		// algorithm used to sign the certificate).
		public virtual byte[] GetCertHash () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetCertHash ();
		}
	
		public virtual string GetCertHashString () 
		{
			// must call GetCertHash (not variable) or optimization wont work
			return X509Helper.ToHexString (GetCertHash ());
		}
	
		// strangly there are no DateTime returning function
		public virtual string GetEffectiveDateString ()
		{
			if (hideDates)
				return null;
			X509Helper.ThrowIfContextInvalid (impl);

			return impl.GetValidFrom ().ToLocalTime ().ToString ();
		}
	
		// strangly there are no DateTime returning function
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
			X509Helper.ThrowIfContextInvalid (impl);

			return impl.GetValidUntil ().ToLocalTime ().ToString ();
		}
	
		// well maybe someday there'll be support for PGP or SPKI ?
		public virtual string GetFormat () 
		{
			return "X509";	// DO NOT TRANSLATE
		}
	
		public override int GetHashCode ()
		{
			if (!X509Helper.IsValid (impl))
				return 0;
			return impl.GetHashCode ();
		}

		[Obsolete ("Use the Issuer property.")]
		public virtual string GetIssuerName () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetIssuerName (true);
		}
	
		public virtual string GetKeyAlgorithm () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetKeyAlgorithm ();
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
			X509Helper.ThrowIfContextInvalid (impl);

			byte[] kap = impl.GetKeyAlgorithmParameters ();
			if (kap == null)
				throw new CryptographicException (Locale.GetText ("Parameters not part of the certificate"));

			return kap;
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return X509Helper.ToHexString (GetKeyAlgorithmParameters ());
		}
	
		[Obsolete ("Use the Subject property.")]
		public virtual string GetName ()
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetSubjectName (true);
		}
	
		public virtual byte[] GetPublicKey () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetPublicKey ();
		}
	
		public virtual string GetPublicKeyString () 
		{
			return X509Helper.ToHexString (GetPublicKey ());
		}
	
		public virtual byte[] GetRawCertData () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetRawCertData ();
		}
	
		public virtual string GetRawCertDataString () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return X509Helper.ToHexString (impl.GetRawCertData ());
		}
	
		public virtual byte[] GetSerialNumber () 
		{
			X509Helper.ThrowIfContextInvalid (impl);
			return impl.GetSerialNumber ();
		}
	
		public virtual string GetSerialNumberString () 
		{
			byte[] sn = GetSerialNumber ();
			Array.Reverse (sn);
			return X509Helper.ToHexString (sn);
		}
	
		// to please corcompare ;-)
		public override string ToString () 
		{
			return base.ToString ();
		}
	
		public virtual string ToString (bool fVerbose) 
		{
			if (!fVerbose || !X509Helper.IsValid (impl))
				return base.ToString ();

			return impl.ToString (true);
		}

		protected static string FormatDate (DateTime date)
		{
			throw new NotImplementedException ();
		}
	}
}
