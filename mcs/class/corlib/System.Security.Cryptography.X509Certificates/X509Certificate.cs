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
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif
using System.Text;

using Mono.Security;
using Mono.Security.X509;

#if NET_2_0
using System.Runtime.Serialization;
#endif
#if !NET_2_1 || MONOTOUCH
using Mono.Security.Authenticode;
#endif

namespace System.Security.Cryptography.X509Certificates {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt
	
	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
	[Serializable]
#if NET_2_1
	public partial class X509Certificate {
#elif NET_2_0
	public partial class X509Certificate : IDeserializationCallback, ISerializable {
#else
	public class X509Certificate {
#endif
		// typedef struct _CERT_CONTEXT {
                //	DWORD                   dwCertEncodingType;
                //	BYTE                    *pbCertEncoded;
		//	DWORD                   cbCertEncoded;
		//	PCERT_INFO              pCertInfo;
		//	HCERTSTORE              hCertStore;
		// } CERT_CONTEXT, *PCERT_CONTEXT;
		// typedef const CERT_CONTEXT *PCCERT_CONTEXT;
		[StructLayout (LayoutKind.Sequential)]
		internal struct CertificateContext {
			public UInt32 dwCertEncodingType;
			public IntPtr pbCertEncoded;
			public UInt32 cbCertEncoded;
			public IntPtr pCertInfo;
			public IntPtr hCertStore;
		}
		// NOTE: We only define the CryptoAPI structure (from WINCRYPT.H)
		// so we don't create any dependencies on Windows DLL in corlib

		private Mono.Security.X509.X509Certificate x509;
		private bool hideDates;
		private byte[] cachedCertificateHash;
	
		// almost every byte[] returning function has a string equivalent
		// sadly the BitConverter insert dash between bytes :-(
		private string tostr (byte[] data) 
		{
			if (data != null) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < data.Length; i++)
					sb.Append (data[i].ToString ("X2"));
				return sb.ToString ();
			}
			else
				return null;
		}
	
		// static methods
	
		public static X509Certificate CreateFromCertFile (string filename) 
		{
			byte[] data = Load (filename);
			return new X509Certificate (data);
		}

#if !NET_2_1 || MONOTOUCH
		[MonoTODO ("Incomplete - minimal validation in this version")]
		public static X509Certificate CreateFromSignedFile (string filename)
		{
			try {
				AuthenticodeDeformatter a = new AuthenticodeDeformatter (filename);
				if (a.SigningCertificate != null) {
#if !NET_2_0
					// before 2.0 the signing certificate is returned only if the signature is valid
					if (a.Reason != 0) {
						string msg = String.Format (Locale.GetText (
							"Invalid digital signature on {0}, reason #{1}."),
							filename, a.Reason);
						throw new COMException (msg);
					}
#endif
					return new X509Certificate (a.SigningCertificate.RawData);
				}
			}
			#if !DISABLE_SECURITY
			catch (SecurityException) {
				// don't wrap SecurityException into a COMException
				throw;
			}
			#endif
#if !NET_2_0
			catch (COMException) {
				// don't wrap COMException into a COMException
				throw;
			}
#endif
			catch (Exception e) {
				string msg = Locale.GetText ("Couldn't extract digital signature from {0}.", filename);
				throw new COMException (msg, e);
			}
#if NET_2_0
			throw new CryptographicException (Locale.GetText ("{0} isn't signed.", filename));
#else
			// if no signature is present return an empty certificate
			byte[] cert = null; // must not confuse compiler about null ;)
			return new X509Certificate (cert);
#endif
		}

#endif // NET_2_1

		// constructors
	
		// special constructor for Publisher (and related classes).
		// Dates strings are null
		internal X509Certificate (byte[] data, bool dates) 
		{
			if (data != null) {
#if NET_2_0
				Import (data, (string)null, X509KeyStorageFlags.DefaultKeySet);
#else
				x509 = new Mono.Security.X509.X509Certificate (data);
#endif
				hideDates = !dates;
			}
		}
	
		public X509Certificate (byte[] data) : this (data, true)
		{
		}
	
		public X509Certificate (IntPtr handle) 
		{
#if NET_2_0
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Invalid handle.");
#endif
#if NET_2_1
			// this works on Windows-only so it's of no use for Moonlight
			// even more since this ctor is [SecurityCritical]
			throw new NotSupportedException ();
#else
			InitFromHandle (handle);
#endif
		}

#if !NET_2_1 || MONOTOUCH
#if !DISABLE_SECURITY
		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
#endif
		private void InitFromHandle (IntPtr handle)
		{
			if (handle != IntPtr.Zero) {
				// both Marshal.PtrToStructure and Marshal.Copy use LinkDemand (so they will always success from here)
				CertificateContext cc = (CertificateContext) Marshal.PtrToStructure (handle, typeof (CertificateContext));
				byte[] data = new byte [cc.cbCertEncoded];
				Marshal.Copy (cc.pbCertEncoded, data, 0, (int)cc.cbCertEncoded);
				x509 = new Mono.Security.X509.X509Certificate (data);
			}
			// for 1.x IntPtr.Zero results in an "empty" certificate instance
		}
#endif
	
		public X509Certificate (System.Security.Cryptography.X509Certificates.X509Certificate cert) 
		{
#if NET_2_0
			if (cert == null)
				throw new ArgumentNullException ("cert");
#endif

			if (cert != null) {
				byte[] data = cert.GetRawCertData ();
				if (data != null)
					x509 = new Mono.Security.X509.X509Certificate (data);
				hideDates = false;
			}
		}


		// public methods
	
		public virtual bool Equals (System.Security.Cryptography.X509Certificates.X509Certificate other)
		{
			if (other == null) {
				return false;
			} else {
				if (other.x509 == null) {
#if NET_2_0
					if (x509 == null)
						return true;
					throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#else
					return (x509 == null);
#endif
				}

				byte[] raw = other.x509.RawData;
				if (raw != null) {
					if (x509 == null)
						return false;
					if (x509.RawData == null)
						return false;
					if (raw.Length == x509.RawData.Length) {
						for (int i = 0; i < raw.Length; i++) {
							if (raw[i] != x509.RawData [i])
								return false;
						}
						// well no choice must be equals!
						return true;
					}
					else
						return false;
				}
			}
			return ((x509 == null) || (x509.RawData == null));
		}
	
		// LAMESPEC: This is the equivalent of the "thumbprint" that can be seen
		// in the certificate viewer of Windows. This is ALWAYS the SHA1 hash of
		// the certificate (i.e. it has nothing to do with the actual hash 
		// algorithm used to sign the certificate).
		public virtual byte[] GetCertHash () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			// we'll hash the cert only once and only if required
			if ((cachedCertificateHash == null) && (x509 != null)) {
				SHA1 sha = SHA1.Create ();
				cachedCertificateHash = sha.ComputeHash (x509.RawData);
			}
			return cachedCertificateHash;
		}
	
		public virtual string GetCertHashString () 
		{
			// must call GetCertHash (not variable) or optimization wont work
			return tostr (GetCertHash ());
		}
	
		// strangly there are no DateTime returning function
		public virtual string GetEffectiveDateString ()
		{
			if (hideDates)
				return null;
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			return x509.ValidFrom.ToLocalTime ().ToString ();
#else
			// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
			// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
			return x509.ValidFrom.ToUniversalTime ().AddHours (-8).ToString ();
#endif
		}
	
		// strangly there are no DateTime returning function
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			return x509.ValidUntil.ToLocalTime ().ToString ();
#else
			// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
			// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
			return x509.ValidUntil.ToUniversalTime ().AddHours (-8).ToString ();
#endif
		}
	
		// well maybe someday there'll be support for PGP or SPKI ?
		public virtual string GetFormat () 
		{
			return "X509";	// DO NOT TRANSLATE
		}
	
		public override int GetHashCode ()
		{
#if NET_2_0
			if (x509 == null)
				return 0;
#endif
			// the cert hash may not be (yet) calculated
			if (cachedCertificateHash == null)
				GetCertHash();
		
			// return the integer of the first 4 bytes of the cert hash
			if ((cachedCertificateHash != null) && (cachedCertificateHash.Length >= 4))
				return ((cachedCertificateHash[0] << 24) |(cachedCertificateHash[1] << 16) |
					(cachedCertificateHash[2] << 8) | cachedCertificateHash[3]);
			else
				return 0;
		}

#if NET_2_0
		[Obsolete ("Use the Issuer property.")]
#endif
		public virtual string GetIssuerName () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			return x509.IssuerName;
		}
	
		public virtual string GetKeyAlgorithm () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			return x509.KeyAlgorithm;
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			byte[] kap = x509.KeyAlgorithmParameters;
			if (kap == null)
				throw new CryptographicException (Locale.GetText ("Parameters not part of the certificate"));

			return kap;
#else
			return x509.KeyAlgorithmParameters;
#endif
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return tostr (GetKeyAlgorithmParameters ());
		}
	
#if NET_2_0
		[Obsolete ("Use the Subject property.")]
#endif
		public virtual string GetName ()
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			return x509.SubjectName;
		}
	
		public virtual byte[] GetPublicKey () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			return x509.PublicKey;
		}
	
		public virtual string GetPublicKeyString () 
		{
			return tostr (GetPublicKey ());
		}
	
		public virtual byte[] GetRawCertData () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.RawData;
#else
			return ((x509 != null) ? x509.RawData : null);
#endif
		}
	
		public virtual string GetRawCertDataString () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return tostr (x509.RawData);
#else
			return ((x509 != null) ? tostr (x509.RawData) : null);
#endif
		}
	
		public virtual byte[] GetSerialNumber () 
		{
#if NET_2_0
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
#endif
			return x509.SerialNumber;
		}
	
		public virtual string GetSerialNumberString () 
		{
			byte[] sn = GetSerialNumber ();
#if NET_2_0
			Array.Reverse (sn);
#endif
			return tostr (sn);
		}
	
		// to please corcompare ;-)
		public override string ToString () 
		{
			return base.ToString ();
		}
	
		public virtual string ToString (bool fVerbose) 
		{
			if (!fVerbose || (x509 == null))
				return base.ToString ();

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
#if NET_2_0
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, Subject);
			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, Issuer);
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, GetEffectiveDateString ());
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, GetExpirationDateString ());
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}", nl, GetCertHashString ());
#else
			sb.Append ("CERTIFICATE:");
			sb.Append (nl);
			sb.Append ("\tFormat:  ");
			sb.Append (GetFormat ());
			if (x509.SubjectName != null) {
				sb.Append (nl);
				sb.Append ("\tName:  ");
				sb.Append (GetName ());
			}
			if (x509.IssuerName != null) {
				sb.Append (nl);
				sb.Append ("\tIssuing CA:  ");
				sb.Append (GetIssuerName ());
			}
			if (x509.SignatureAlgorithm != null) {
				sb.Append (nl);
				sb.Append ("\tKey Algorithm:  ");
				sb.Append (GetKeyAlgorithm ());
			}
			if (x509.SerialNumber != null) {
				sb.Append (nl);
				sb.Append ("\tSerial Number:  ");
				sb.Append (GetSerialNumberString ());
			}
			// Note: Algorithm is not spelled right as the actual 
			// MS implementation (we do exactly the same for the
			// comparison in the unit tests)
			if (x509.KeyAlgorithmParameters != null) {
				sb.Append (nl);
				sb.Append ("\tKey Alogrithm Parameters:  ");
				sb.Append (GetKeyAlgorithmParametersString ());
			}
			if (x509.PublicKey != null) {
				sb.Append (nl);
				sb.Append ("\tPublic Key:  ");
				sb.Append (GetPublicKeyString ());
			}
			sb.Append (nl);
#endif
			sb.Append (nl);
			return sb.ToString ();
		}

		private static byte[] Load (string fileName)
		{
			byte[] data = null;
			using (FileStream fs = File.OpenRead (fileName)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return data;
		}
	}
}
