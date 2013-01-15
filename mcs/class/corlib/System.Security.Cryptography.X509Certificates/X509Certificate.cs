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
#if NET_2_1
	public partial class X509Certificate {
#else
	public partial class X509Certificate : IDeserializationCallback, ISerializable {
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
#if NET_2_1
			// this works on Windows-only so it's of no use for Moonlight
			// even more since this ctor is [SecurityCritical]
			throw new NotSupportedException ();
#else
			InitFromHandle (handle);
#endif
		}

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode = true)]
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
	
		public X509Certificate (System.Security.Cryptography.X509Certificates.X509Certificate cert) 
		{
			if (cert == null)
				throw new ArgumentNullException ("cert");

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
					if (x509 == null)
						return true;
					throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
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
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
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
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			return x509.ValidFrom.ToLocalTime ().ToString ();
		}
	
		// strangly there are no DateTime returning function
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			return x509.ValidUntil.ToLocalTime ().ToString ();
		}
	
		// well maybe someday there'll be support for PGP or SPKI ?
		public virtual string GetFormat () 
		{
			return "X509";	// DO NOT TRANSLATE
		}
	
		public override int GetHashCode ()
		{
			if (x509 == null)
				return 0;
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

		[Obsolete ("Use the Issuer property.")]
		public virtual string GetIssuerName () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.IssuerName;
		}
	
		public virtual string GetKeyAlgorithm () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.KeyAlgorithm;
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));

			byte[] kap = x509.KeyAlgorithmParameters;
			if (kap == null)
				throw new CryptographicException (Locale.GetText ("Parameters not part of the certificate"));

			return kap;
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return tostr (GetKeyAlgorithmParameters ());
		}
	
		[Obsolete ("Use the Subject property.")]
		public virtual string GetName ()
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.SubjectName;
		}
	
		public virtual byte[] GetPublicKey () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.PublicKey;
		}
	
		public virtual string GetPublicKeyString () 
		{
			return tostr (GetPublicKey ());
		}
	
		public virtual byte[] GetRawCertData () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.RawData;
		}
	
		public virtual string GetRawCertDataString () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return tostr (x509.RawData);
		}
	
		public virtual byte[] GetSerialNumber () 
		{
			if (x509 == null)
				throw new CryptographicException (Locale.GetText ("Certificate instance is empty."));
			return x509.SerialNumber;
		}
	
		public virtual string GetSerialNumberString () 
		{
			byte[] sn = GetSerialNumber ();
			Array.Reverse (sn);
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
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, Subject);
			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, Issuer);
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, GetEffectiveDateString ());
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, GetExpirationDateString ());
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}", nl, GetCertHashString ());
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
#if NET_4_0
		protected static string FormatDate (DateTime date)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
