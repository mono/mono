//
// X509Certificates.cs: Handles X.509 certificates.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt
	
	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
	[Serializable]
	public class X509Certificate {
	
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
			byte[] data = null;
			FileStream fs = new FileStream (filename, FileMode.Open);
			try {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
			}
			finally {
				fs.Close ();
			}
	
			return new X509Certificate (data);
		}
	
		static private int ReadWord (Stream s) 
		{
			int word = s.ReadByte ();
			word = (s.ReadByte () << 8) + word;
			return word;
		}
	
		static private int ReadDWord (Stream s) 
		{
			int b1 = s.ReadByte ();
			int b2 = s.ReadByte ();
			int b3 = s.ReadByte ();
			int b4 = s.ReadByte ();
			return (b4 << 24) + (b3 << 16) + (b2 << 8) + b1;
		}
	
		// http://www.mycgiserver.com/~ultraschall/files/pefile.htm
		static private byte[] GetAuthenticodeSignature (string fileName) 
		{
			FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read);
			try {
				// MZ - DOS header
				if (ReadWord (fs) != 0x5a4d)
					return null;
				// find offset of PE header
				fs.Seek (60, SeekOrigin.Begin);
				int peOffset = ReadDWord (fs);
	
				// PE - NT header
				fs.Seek (peOffset, SeekOrigin.Begin);
				if (ReadWord (fs) != 0x4550)
					return null;
	
				fs.Seek (150, SeekOrigin.Current);
	
				// IMAGE_DIRECTORY_ENTRY_SECURITY
				int secOffset = ReadDWord (fs);
				if (secOffset == 0)
					return null;
				int secSize = ReadDWord (fs);
				if (secSize == 0)
					return null;
	
				// Authenticode signature
				fs.Seek (secOffset, SeekOrigin.Begin);
				if (ReadDWord (fs) != secSize)
					return null;
				if (ReadDWord (fs) != 0x00020200)
					return null;
				
				byte[] signature = new byte [secSize - 8];
				fs.Read (signature, 0, signature.Length);
				fs.Close ();
				return signature;
			}
			catch {
				fs.Close ();
				return null;
			}
		}
	
		// LAMESPEC: How does it differ from CreateFromCertFile ?
		// It seems to get the certificate inside a PE file (maybe a CAB too ?)
		public static X509Certificate CreateFromSignedFile (string filename)
		{
			byte[] signature = GetAuthenticodeSignature (filename);
			if (signature == null)
				throw new COMException ("File doesn't have a signature", -2146762496);
	
			// this is a big bad ASN.1 structure
			// Reference: http://www.cs.auckland.ac.nz/~pgut001/pubs/authenticode.txt
			// next we must find the last certificate inside the structure
			try {
				ASN1 sign = new ASN1 (signature);
				// we don't have to understand much of it to get the certificate
				ASN1 certs = sign [1][0][3];
				byte[] lastCert = certs [certs.Count - 1].GetBytes();
				return new X509Certificate (lastCert);
			}
			catch {
				return null;
			}
		}
	
		// constructors
	
		// special constructor for Publisher (and related classes).
		// Dates strings are null
		internal X509Certificate (byte[] data, bool dates) 
		{
			if (data != null) {
				x509 = new Mono.Security.X509.X509Certificate (data);
				hideDates = !dates;
			}
		}
	
		public X509Certificate (byte[] data) : this (data, true) {}
	
		[MonoTODO("Handle on CryptoAPI certificate")]
		public X509Certificate (IntPtr handle) 
		{
			// normally a handle to CryptoAPI
			// How does Mono "handle this handle" ???
			throw new NotSupportedException ();
		}
	
		public X509Certificate (System.Security.Cryptography.X509Certificates.X509Certificate cert) 
		{
			if (cert != null) {
				byte[] data = cert.GetRawCertData ();
				if (data != null)
					x509 = new Mono.Security.X509.X509Certificate (data);
				hideDates = false;
			}
		}
	
		// public methods
	
		public virtual bool Equals (System.Security.Cryptography.X509Certificates.X509Certificate cert)
		{
			if (cert != null) {
				byte[] raw = cert.GetRawCertData ();
				if (raw != null) {
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
			return (x509.RawData == null);
		}
	
		// LAMESPEC: This is the equivalent of the "thumbprint" that can be seen
		// in the certificate viewer of Windows. This is ALWAYS the SHA1 hash of
		// the certificate (i.e. it has nothing to do with the actual hash 
		// algorithm used to sign the certificate).
		public virtual byte[] GetCertHash () 
		{
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
		// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
		// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
		public virtual string GetEffectiveDateString ()
		{
			if (hideDates)
				return null;
			DateTime dt = x509.ValidFrom.ToUniversalTime().AddHours (-8);
			return dt.ToString (); //"yyyy-MM-dd HH:mm:ss");
		}
	
		// strangly there are no DateTime returning function
		// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
		// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
			DateTime dt = x509.ValidUntil.ToUniversalTime().AddHours (-8);
			return dt.ToString (); //"yyyy-MM-dd HH:mm:ss");
		}
	
		// well maybe someday there'll be support for PGP or SPKI ?
		public virtual string GetFormat () 
		{
			return "X509";	// DO NOT TRANSLATE
		}
	
		public override int GetHashCode ()
		{
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
	
		public virtual string GetIssuerName () 
		{
			return x509.IssuerName;
		}
	
		public virtual string GetKeyAlgorithm () 
		{
			return x509.KeyAlgorithm;
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
			return x509.KeyAlgorithmParameters;
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return tostr (x509.KeyAlgorithmParameters);
		}
	
		public virtual string GetName ()
		{
			return x509.SubjectName;
		}
	
		public virtual byte[] GetPublicKey () 
		{
			return x509.PublicKey;
		}
	
		public virtual string GetPublicKeyString () 
		{
			return tostr (x509.PublicKey);
		}
	
		public virtual byte[] GetRawCertData () 
		{
			return x509.RawData;
		}
	
		public virtual string GetRawCertDataString () 
		{
			return tostr (x509.RawData);
		}
	
		public virtual byte[] GetSerialNumber () 
		{
			return x509.SerialNumber;
		}
	
		public virtual string GetSerialNumberString () 
		{
			return tostr (x509.SerialNumber);
		}
	
		// to please corcompare ;-)
		public override string ToString() 
		{
			return base.ToString ();
		}
	
		public virtual string ToString (bool details) 
		{
			if (details) {
				string nl = Environment.NewLine;
				StringBuilder sb = new StringBuilder ();
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
				sb.Append (nl);
				return sb.ToString ();
			}
			else
				return base.ToString ();
		}
	}
}