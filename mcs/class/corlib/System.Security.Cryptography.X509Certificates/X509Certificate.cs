//
// X509Certificates.cs: Handles X.509 certificates.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.X509Certificates {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc2459.txt
	
	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
	[Serializable]
	public class X509Certificate {
	
		static private byte[] countryName = { 0x55, 0x04, 0x06 };
		static private byte[] organizationName = { 0x55, 0x04, 0x0A };
		static private byte[] organizationalUnitName = { 0x55, 0x04, 0x0B };
		static private byte[] commonName = { 0x55, 0x04, 0x03 };
		static private byte[] localityName = { 0x55, 0x04, 0x07 };
		static private byte[] stateOrProvinceName = { 0x55, 0x04, 0x08 };
		static private byte[] streetAddress = { 0x55, 0x04, 0x09 };
		static private byte[] serialNumber = { 0x55, 0x04, 0x05 };
		static private byte[] domainComponent = { 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x19 };
		static private byte[] userid = { 0x09, 0x92, 0x26, 0x89, 0x93, 0xF2, 0x2C, 0x64, 0x01, 0x01 };
		static private byte[] email = { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x09, 0x01 };
	
		private byte[] m_encodedcert;
		private byte[] m_certhash;
		private DateTime m_from;
		private DateTime m_until;
		private string m_issuername;
		private string m_keyalgo;
		private byte[] m_keyalgoparams;
		private string m_subject;
		private byte[] m_publickey;
		private byte[] m_serialnumber;
		private bool hideDates;
	
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
	
		/// <summary>
		/// Tranform an RDN to a UTF-8 string representation
		/// The string is reserved from what is defined in RFC2253.
		/// </summary>
		/// <returns>The relative distingued name (RDN) as a string</returns>
		private string RDNToString (ASN1 seq)
		{
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < seq.Count; i++) {
				ASN1 entry = seq.Element (i);
				ASN1 pair = entry.Element (0);
	
				ASN1 s = pair.Element (1);
				if (s == null)
					continue;
	
				ASN1 poid = pair.Element (0);
				if (poid == null)
					continue;
	
				if (poid.CompareValue (countryName))
					sb.Append ("C=");
				else if (poid.CompareValue (organizationName))
					sb.Append ("O=");
				else if (poid.CompareValue (organizationalUnitName))
					sb.Append ("OU=");
				else if (poid.CompareValue (commonName))
					sb.Append ("CN=");
				else if (poid.CompareValue (localityName))
					sb.Append ("L=");
				else if (poid.CompareValue (stateOrProvinceName))
					sb.Append ("S=");	// NOTE: RFC2253 uses ST=
				else if (poid.CompareValue (streetAddress))
					sb.Append ("STREET=");
				else if (poid.CompareValue (domainComponent))
					sb.Append ("DC=");
				else if (poid.CompareValue (userid))
					sb.Append ("UID=");
				else if (poid.CompareValue (email))
					sb.Append ("E=");	// NOTE: Not part of RFC2253
				else {
					// unknown OID
					sb.Append ("OID.");	// NOTE: Not present as RFC2253
					sb.Append (OIDToString (poid.Value));
					sb.Append ("=");
				}
	
				string sValue = null;
				// 16bits or 8bits string ? TODO not complete (+special chars!)
				if (s.Tag == 0x1E) {
					// BMPSTRING
					StringBuilder sb2 = new StringBuilder ();
					for (int j = 1; j < s.Value.Length; j+=2)
						sb2.Append ((char) s.Value[j]);
					sValue = sb2.ToString ();
				}
				else {
					sValue = System.Text.Encoding.UTF8.GetString (s.Value);
					// in some cases we must quote (") the value
					// Note: this doesn't seems to conform to RFC2253
					char[] specials = { ',', '+', '"', '\\', '<', '>', ';' };
					if (sValue.IndexOfAny(specials, 0, sValue.Length) > 0)
						sValue = "\"" + sValue + "\"";
					else if (sValue.StartsWith (" "))
						sValue = "\"" + sValue + "\"";
					else if (sValue.EndsWith (" "))
						sValue = "\"" + sValue + "\"";
				}
	
				sb.Append (sValue);
	
				// separator (not on last iteration)
				if (i < seq.Count - 1)
					sb.Append (", ");
			}
			return sb.ToString ();
		}
	
		/// <summary>
		/// Convert a binary encoded OID to human readable string representation of 
		/// an OID (IETF style). Based on DUMPASN1.C from Peter Gutmann.
		/// </summary>
		/// <param name="aOID">a byte array containing the value of the OID
		/// (no tag, no length)</param>
		/// <returns></returns>
		private string OIDToString (byte[] aOID)
		{
			StringBuilder sb = new StringBuilder ();
			// Pick apart the OID
			byte x = (byte) (aOID[0] / 40);
			byte y = (byte) (aOID[0] % 40);
			if (x > 2) {
				// Handle special case for large y if x = 2
				y += (byte) ((x - 2) * 40);
				x = 2;
			}
			sb.Append (x.ToString ());
			sb.Append (".");
			sb.Append (y.ToString ());
			ulong val = 0;
			for (x = 1; x < aOID.Length; x++) {
				val = ((val << 7) | ((byte) (aOID [x] & 0x7F)));
				if ( !((aOID [x] & 0x80) == 0x80)) {
					sb.Append (".");
					sb.Append (val.ToString ());
					val = 0;
				}
			}
			return sb.ToString ();
		}
	
		private DateTime UTCToDateTime (ASN1 time)
		{
			string t = System.Text.Encoding.ASCII.GetString (time.Value);
			// to support both UTCTime and GeneralizedTime (and not so common format)
			string mask = null;
			switch (t.Length) {
			case 11: mask = "yyMMddHHmmZ"; // illegal I think ... must check
				break;
			case 13: mask = "yyMMddHHmmssZ"; // UTCTime
				break;
			case 15: mask = "yyyyMMddHHmmssZ"; // GeneralizedTime
				break;
			}
			return DateTime.ParseExact (t, mask, null).ToUniversalTime ();
		}
	
		// that's were the real job is!
		// from http://www.ietf.org/rfc/rfc2459.txt
		//
		//Certificate  ::=  SEQUENCE  {
		//     tbsCertificate       TBSCertificate,
		//     signatureAlgorithm   AlgorithmIdentifier,
		//     signature            BIT STRING  }
		//
		//TBSCertificate  ::=  SEQUENCE  {
		//     version         [0]  Version DEFAULT v1,
		//     serialNumber         CertificateSerialNumber,
		//     signature            AlgorithmIdentifier,
		//     issuer               Name,
		//     validity             Validity,
		//     subject              Name,
		//     subjectPublicKeyInfo SubjectPublicKeyInfo,
		//     issuerUniqueID  [1]  IMPLICIT UniqueIdentifier OPTIONAL,
		//                          -- If present, version shall be v2 or v3
		//     subjectUniqueID [2]  IMPLICIT UniqueIdentifier OPTIONAL,
		//                          -- If present, version shall be v2 or v3
		//     extensions      [3]  Extensions OPTIONAL
		//                          -- If present, version shall be v3 --  }
		private void Parse(byte[] data) 
		{
			string e = "Input data cannot be coded as a valid certificate.";
			try {
				ASN1 certdecoder = new ASN1 (data);
				// select root element
				ASN1 Certificate = certdecoder.Element (0, 0x30);
				if (Certificate == null)
					throw new CryptographicException (e);
				ASN1 tbsCertificate = Certificate.Element (0, 0x30);
				if (tbsCertificate == null)
					throw new CryptographicException (e);
		
				int tbs = 0;
				// version (optional) is present only in v2+ certs
				ASN1 version = tbsCertificate.Element (tbs, 0xA0);
				if (version != null)
					tbs++;
		
				ASN1 serialnumber = tbsCertificate.Element (tbs++, 0x02);
				if (serialnumber == null)
					throw new CryptographicException (e);
				m_serialnumber = serialnumber.Value;
				Array.Reverse(m_serialnumber, 0, m_serialnumber.Length);
		
				ASN1 signature = tbsCertificate.Element (tbs++, 0x30); 
		
				ASN1 issuer = tbsCertificate.Element (tbs++, 0x30); 
				m_issuername = RDNToString (issuer);
		
				ASN1 validity = tbsCertificate.Element (tbs++, 0x30);
				ASN1 notBefore = validity.Element (0);
				m_from = UTCToDateTime (notBefore);
				ASN1 notAfter = validity.Element (1);
				m_until = UTCToDateTime (notAfter);
		
				ASN1 subject = tbsCertificate.Element (tbs++, 0x30);
				m_subject = RDNToString (subject);
		
				ASN1 subjectPublicKeyInfo = tbsCertificate.Element (tbs++, 0x30);
		
				ASN1 algorithm = subjectPublicKeyInfo.Element (0, 0x30);
				ASN1 algo = algorithm.Element (0, 0x06);
				m_keyalgo = OIDToString (algo.Value);
				// parameters ANY DEFINED BY algorithm OPTIONAL
				// so we dont ask for a specific (Element) type and return DER
				ASN1 parameters = algorithm.Element (1);
				m_keyalgoparams = parameters.GetBytes ();
		
				ASN1 subjectPublicKey = subjectPublicKeyInfo.Element (1, 0x03); 
				// we must drop th first byte (which is the number of unused bits
				// in the BITSTRING)
				int n = subjectPublicKey.Length - 1;
				m_publickey = new byte [n];
				Array.Copy (subjectPublicKey.Value, 1, m_publickey, 0, n);
	
				// keep original copy
				m_encodedcert = data;
			}
			catch {
				throw new CryptographicException (e);
			}
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
				return null;	// file isn't signed
	
			// \/ for debugging only \/
	//		FileStream debug = new FileStream (@"d:\debug.sig", FileMode.Create);
	//		debug.Write (signature, 0, signature.Length);
	//		debug.Close ();
			// /\ for debugging only /\
	
			// this is a big bad ASN.1 structure
			// Reference: http://www.cs.auckland.ac.nz/~pgut001/pubs/authenticode.txt
			// next we must find the last certificate inside the structure
			try {
				ASN1 sign = new ASN1 (signature);
				// we don't have to understand much of it to get the certificate
				ASN1 certs = sign.Element(0).Element(1).Element(0).Element(3);
				byte[] lastCert = certs.Element(certs.Count - 1).GetBytes();
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
				Parse (data);
				hideDates = !dates;
			}
		}
	
		public X509Certificate (byte[] data) : this (data, true) {}
	
		public X509Certificate (IntPtr handle) 
		{
			// normally a handle to CryptoAPI
			// How does Mono "handle this handle" ???
			throw new NotSupportedException ();
		}
	
		public X509Certificate (X509Certificate cert) 
		{
			if (cert != null) {
				byte[] data = cert.GetRawCertData ();
				if (data != null)
					Parse(data);
				hideDates = false;
			}
		}
	
		// public methods
	
		public virtual bool Equals(X509Certificate cert)
		{
			if (cert != null) {
				byte[] raw = cert.GetRawCertData ();
				if (raw != null) {
					if (raw.Length == m_encodedcert.Length) {
						for (int i = 0; i < raw.Length; i++) {
							if (raw[i] != m_encodedcert[i])
								return false;
						}
						// well no choice must be equals!
						return true;
					}
					else
						return false;
				}
			}
			return (m_encodedcert == null);
		}
	
		// LAMESPEC: This is the equivalent of the "thumbprint" that can be seen
		// in the certificate viewer of Windows. This is ALWAYS the SHA1 hash of
		// the certificate (i.e. it has nothing to do with the actual hash 
		// algorithm used to sign the certificate).
		public virtual byte[] GetCertHash () 
		{
			// we'll hash the cert only once and only if required
			if ((m_certhash == null) && (m_encodedcert != null)) {
				SHA1 sha = SHA1.Create ();
				m_certhash = sha.ComputeHash (m_encodedcert);
			}
			return m_certhash;
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
			DateTime dt = m_from.AddHours (-8);
			return dt.ToString (); //"yyyy-MM-dd HH:mm:ss");
		}
	
		// strangly there are no DateTime returning function
		// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
		// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
			DateTime dt = m_until.AddHours (-8);
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
			if (m_certhash == null)
				GetCertHash();
		
			// return the integer of the first 4 bytes of the cert hash
			if ((m_certhash != null) && (m_certhash.Length >= 4))
				return ((m_certhash[0] << 24) |(m_certhash[1] << 16) |
					(m_certhash[2] << 8) | m_certhash[3]);
			else
				return 0;
		}
	
		public virtual string GetIssuerName () 
		{
			return m_issuername;
		}
	
		public virtual string GetKeyAlgorithm () 
		{
			return m_keyalgo;
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
			return m_keyalgoparams;
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return tostr (m_keyalgoparams);
		}
	
		public virtual string GetName ()
		{
			return m_subject;
		}
	
		public virtual byte[] GetPublicKey () 
		{
			return m_publickey;
		}
	
		public virtual string GetPublicKeyString () 
		{
			return tostr (m_publickey);
		}
	
		public virtual byte[] GetRawCertData () 
		{
			return m_encodedcert;
		}
	
		public virtual string GetRawCertDataString () 
		{
			return tostr (m_encodedcert);
		}
	
		public virtual byte[] GetSerialNumber () 
		{
			return m_serialnumber;
		}
	
		public virtual string GetSerialNumberString () 
		{
			return tostr (m_serialnumber);
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
				if (m_subject != null) {
					sb.Append (nl);
					sb.Append ("\tName:  ");
					sb.Append (GetName ());
				}
				if (m_issuername != null) {
					sb.Append (nl);
					sb.Append ("\tIssuing CA:  ");
					sb.Append (GetIssuerName ());
				}
				if (m_keyalgo != null) {
					sb.Append (nl);
					sb.Append ("\tKey Algorithm:  ");
					sb.Append (GetKeyAlgorithm ());
				}
				if (m_serialnumber != null) {
					sb.Append (nl);
					sb.Append ("\tSerial Number:  ");
					sb.Append (GetSerialNumberString ());
				}
				// Note: Algorithm is not spelled right as the actual 
				// MS implementation (we do exactly the same for the
				// comparison in the unit tests)
				if (m_keyalgoparams != null) {
					sb.Append (nl);
					sb.Append ("\tKey Alogrithm Parameters:  ");
					sb.Append (GetKeyAlgorithmParametersString ());
				}
				if (m_publickey != null) {
					sb.Append (nl);
					sb.Append ("\tPublic Key:  ");
					sb.Append (GetPublicKeyString ());
				}
				sb.Append (nl);
				sb.Append (nl);
				return sb.ToString ();
			}
			else
				return ToString ();
		}
	}
}