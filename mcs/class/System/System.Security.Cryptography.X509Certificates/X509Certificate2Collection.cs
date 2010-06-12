//
// System.Security.Cryptography.X509Certificates.X509Certificate2Collection class
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Tim Coleman (tim@timcoleman.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
// Copyright (C) 2005, 2006 Novell Inc. (http://www.novell.com)
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

#if SECURITY_DEP || MOONLIGHT

using System.Collections;
using System.Globalization;

namespace System.Security.Cryptography.X509Certificates {

	public class X509Certificate2Collection : X509CertificateCollection {

		// constructors

		public X509Certificate2Collection ()
		{
		}

		public X509Certificate2Collection (X509Certificate2Collection certificates)
		{
			AddRange (certificates);
		}

		public X509Certificate2Collection (X509Certificate2 certificate) 
		{
			Add (certificate);
		}

		public X509Certificate2Collection (X509Certificate2[] certificates) 
		{
			AddRange (certificates);
		}

		// properties

		public new X509Certificate2 this [int index] {
			get {
				if (index < 0)
					throw new ArgumentOutOfRangeException ("negative index");
				if (index >= InnerList.Count)
					throw new ArgumentOutOfRangeException ("index >= Count");
				return (X509Certificate2) InnerList [index];
			}
			set { InnerList [index] = value; }
		}

		// methods

		public int Add (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			return InnerList.Add (certificate);
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void AddRange (X509Certificate2[] certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			for (int i=0; i < certificates.Length; i++)
				InnerList.Add (certificates [i]);
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void AddRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificates");

			InnerList.AddRange (certificates);
		}

		public bool Contains (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			foreach (X509Certificate2 c in InnerList) {
				if (c.Equals (certificate))
					return true;
			}
			return false;
		}

		[MonoTODO ("only support X509ContentType.Cert")]
		public byte[] Export (X509ContentType contentType) 
		{
			return Export (contentType, null);
		}

		[MonoTODO ("only support X509ContentType.Cert")]
		public byte[] Export (X509ContentType contentType, string password) 
		{
			switch (contentType) {
			case X509ContentType.Cert:
#if !MOONLIGHT
			case X509ContentType.Pfx: // this includes Pkcs12
			case X509ContentType.SerializedCert:
#endif
				// if multiple certificates are present we only export the last one
				if (Count > 0)
					return this [Count - 1].Export (contentType, password);
				break;
#if !MOONLIGHT
			case X509ContentType.Pkcs7:
				// TODO
				break;
			case X509ContentType.SerializedStore:
				// TODO
				break;
#endif
			default:
				// this includes Authenticode, Unknown and bad values
				string msg = Locale.GetText ("Cannot export certificate(s) to the '{0}' format", contentType);
				throw new CryptographicException (msg);
			}
			return null;
		}

		[MonoTODO ("Does not support X509FindType.FindByTemplateName, FindByApplicationPolicy and FindByCertificatePolicy")]
		public X509Certificate2Collection Find (X509FindType findType, object findValue, bool validOnly) 
		{
			if (findValue == null)
				throw new ArgumentNullException ("findValue");

			string str = String.Empty;
			string oid = String.Empty;
			X509KeyUsageFlags ku = X509KeyUsageFlags.None;
			DateTime dt = DateTime.MinValue;

			switch (findType) {
			case X509FindType.FindByThumbprint:
			case X509FindType.FindBySubjectName:
			case X509FindType.FindBySubjectDistinguishedName:
			case X509FindType.FindByIssuerName:
			case X509FindType.FindByIssuerDistinguishedName:
			case X509FindType.FindBySerialNumber:
			case X509FindType.FindByTemplateName:
			case X509FindType.FindBySubjectKeyIdentifier:
				try {
					str = (string) findValue;
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Invalid find value type '{0}', expected '{1}'.", 
						findValue.GetType (), "string");
					throw new CryptographicException (msg, e);
				}
				break;
			case X509FindType.FindByApplicationPolicy:
			case X509FindType.FindByCertificatePolicy:
			case X509FindType.FindByExtension:
				try {
					oid = (string) findValue;
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Invalid find value type '{0}', expected '{1}'.", 
						findValue.GetType (), "X509KeyUsageFlags");
					throw new CryptographicException (msg, e);
				}
				// OID validation
				try {
					CryptoConfig.EncodeOID (oid);
				}
				catch (CryptographicUnexpectedOperationException) {
					string msg = Locale.GetText ("Invalid OID value '{0}'.", oid);
					throw new ArgumentException ("findValue", msg);
				}
				break;
			case X509FindType.FindByKeyUsage:
				try {
					ku = (X509KeyUsageFlags) findValue;
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Invalid find value type '{0}', expected '{1}'.", 
						findValue.GetType (), "X509KeyUsageFlags");
					throw new CryptographicException (msg, e);
				}
				break;
			case X509FindType.FindByTimeValid:
			case X509FindType.FindByTimeNotYetValid:
			case X509FindType.FindByTimeExpired:
				try {
					dt = (DateTime) findValue;
				}
				catch (Exception e) {
					string msg = Locale.GetText ("Invalid find value type '{0}', expected '{1}'.", 
						findValue.GetType (), "X509DateTime");
					throw new CryptographicException (msg,e );
				}
				break;
			default:
				{
					string msg = Locale.GetText ("Invalid find type '{0}'.", findType);
					throw new CryptographicException (msg);
				}
			}

			CultureInfo cinv = CultureInfo.InvariantCulture;
			X509Certificate2Collection results = new  X509Certificate2Collection ();
			foreach (X509Certificate2 x in InnerList) {
				bool value_match = false;

				switch (findType) {
				case X509FindType.FindByThumbprint:
					// works with Thumbprint, GetCertHashString in both normal (upper) and lower case
					value_match = ((String.Compare (str, x.Thumbprint, true, cinv) == 0) ||
						(String.Compare (str, x.GetCertHashString (), true, cinv) == 0));
					break;
				case X509FindType.FindBySubjectName:
					string sname = x.GetNameInfo (X509NameType.SimpleName, false);
					value_match = (sname.IndexOf (str, StringComparison.InvariantCultureIgnoreCase) >= 0);
					break;
				case X509FindType.FindBySubjectDistinguishedName:
					value_match = (String.Compare (str, x.Subject, true, cinv) == 0);
					break;
				case X509FindType.FindByIssuerName:
					string iname = x.GetNameInfo (X509NameType.SimpleName, true);
					value_match = (iname.IndexOf (str, StringComparison.InvariantCultureIgnoreCase) >= 0);
					break;
				case X509FindType.FindByIssuerDistinguishedName:
					value_match = (String.Compare (str, x.Issuer, true, cinv) == 0);
					break;
				case X509FindType.FindBySerialNumber:
					value_match = (String.Compare (str, x.SerialNumber, true, cinv) == 0);
					break;
				case X509FindType.FindByTemplateName:
					// TODO - find a valid test case
					break;
				case X509FindType.FindBySubjectKeyIdentifier:
					X509SubjectKeyIdentifierExtension ski = (x.Extensions ["2.5.29.14"] as X509SubjectKeyIdentifierExtension);
					if (ski != null) {
						value_match = (String.Compare (str, ski.SubjectKeyIdentifier, true, cinv) == 0);
					}
					break;
				case X509FindType.FindByApplicationPolicy:
					// note: include when no extensions are present (even if v3)
					value_match = (x.Extensions.Count == 0);
					// TODO - find test case with extension
					break;
				case X509FindType.FindByCertificatePolicy:
					// TODO - find test case with extension
					break;
				case X509FindType.FindByExtension:
					value_match = (x.Extensions [oid] != null);
					break;
				case X509FindType.FindByKeyUsage:
					X509KeyUsageExtension kue = (x.Extensions ["2.5.29.15"] as X509KeyUsageExtension);
					if (kue == null) {
						// key doesn't have any hard coded limitations
						// note: MS doesn't check for ExtendedKeyUsage
						value_match = true; 
					} else {
						value_match = ((kue.KeyUsages & ku) == ku);
					}
					break;
				case X509FindType.FindByTimeValid:
					value_match = ((dt >= x.NotBefore) && (dt <= x.NotAfter));
					break;
				case X509FindType.FindByTimeNotYetValid:
					value_match = (dt < x.NotBefore);
					break;
				case X509FindType.FindByTimeExpired:
					value_match = (dt > x.NotAfter);
					break;
				}

				if (!value_match)
					continue;

				if (validOnly) {
					try {
						if (x.Verify ())
							results.Add (x);
					}
					catch {
					}
				} else {
					results.Add (x);
				}
			}
			return results;
		}

		public new X509Certificate2Enumerator GetEnumerator () 
		{
			return new X509Certificate2Enumerator (this);
		}

		[MonoTODO ("same limitations as X509Certificate2.Import")]
		public void Import (byte[] rawData) 
		{
			// FIXME: can it import multiple certificates, e.g. a pkcs7 file ?
			X509Certificate2 cert = new X509Certificate2 ();
			cert.Import (rawData);
			Add (cert);
		}

		[MonoTODO ("same limitations as X509Certificate2.Import")]
		public void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			// FIXME: can it import multiple certificates, e.g. a pkcs7 file ?
			X509Certificate2 cert = new X509Certificate2 ();
			cert.Import (rawData, password, keyStorageFlags);
			Add (cert);
		}

		[MonoTODO ("same limitations as X509Certificate2.Import")]
		public void Import (string fileName) 
		{
			// FIXME: can it import multiple certificates, e.g. a pkcs7 file ?
			X509Certificate2 cert = new X509Certificate2 ();
			cert.Import (fileName);
			Add (cert);
		}

		[MonoTODO ("same limitations as X509Certificate2.Import")]
		public void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) 
		{
			// FIXME: can it import multiple certificates, e.g. a pkcs7 file ?
			X509Certificate2 cert = new X509Certificate2 ();
			cert.Import (fileName, password, keyStorageFlags);
			Add (cert);
		}

		public void Insert (int index, X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("negative index");
			if (index >= InnerList.Count)
				throw new ArgumentOutOfRangeException ("index >= Count");

			InnerList.Insert (index, certificate);
		}

		public void Remove (X509Certificate2 certificate) 
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			for (int i=0; i < InnerList.Count; i++) {
				X509Certificate c = (X509Certificate) InnerList [i];
				if (c.Equals (certificate)) {
					InnerList.RemoveAt (i);
					// only first instance is removed
					return;
				}
			}
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void RemoveRange (X509Certificate2[] certificates)
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificate");

			foreach (X509Certificate2 x in certificates)
				Remove (x);
		}

		[MonoTODO ("Method isn't transactional (like documented)")]
		public void RemoveRange (X509Certificate2Collection certificates) 
		{
			if (certificates == null)
				throw new ArgumentNullException ("certificate");

			foreach (X509Certificate2 x in certificates)
				Remove (x);
		}
	}
}

#endif
