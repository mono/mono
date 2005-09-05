//
// CertificateFormatter.cs: Certificate Formatter (not GUI specific)
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Mono.Tools.CertView {

	public class CertificateFormatter {

		public class FieldNames {
			public FieldNames () {}

			public const string Version = "Version";
			public const string SerialNumber = "Serial number";
			public const string SignatureAlgorithm = "Signature algorithm";
			public const string Issuer = "Issuer";
			public const string ValidFrom = "Valid from";
			public const string ValidUntil = "Valid until";
			public const string Subject = "Subject";
			public const string PublicKey = "Public key";
		}

		public class PropertyNames {
			public PropertyNames () {}

			public const string ThumbprintAlgorithm = "Thumbprint algorithm";
			public const string Thumbprint = "Thumbprint";
		}

		public class Help {
			public Help () {}

			public const string IssuedBy = "This is the distinguished name (DN) of the certificate authority (CA) that issued this certificate.";
			public const string IssuedTo = "This is the distinguished name (DN) of the entity (individual, device or organization) to whom the certificate was issued.";
			public const string ValidFrom = "This certificate isn't valid before the specified date.";
			public const string ValidUntil = "This certificate isn't valid after the specified date. This also means that the certificate authority (CA) won't publish the status of the certificate after this date.";
		}

		private const string untrustedRoot = "This root certificate isn't part of your trusted root store. Please read your documentation carefully before adding a new root certificate in your trusted store.";
		private const string unknownCriticalExtension = "This certificate contains unknown critical extensions and shouldn't be used by applications that can't process those extensions.";
		private const string noSignatureCheck = "The signature of the certificate can;t be verified without the issuer certificate.";
		private const string noValidation = "No CRL, nor an OCSP responder, has been found to validate the status of the certificate.";
		private const string unsupportedHash = "The {0} algorithm is unsupported by the .NET Framework. The certificate signature cannot be verified.";

		private string thumbprintAlgorithm;
		private X509Certificate x509;
		private string status;
		private string[] subjectAltName;

		private static string defaultThumbprintAlgo;
		private static Hashtable extensions;

		static CertificateFormatter () 
		{
			IDictionary tb = (IDictionary) ConfigurationSettings.GetConfig ("Thumbprint");
			defaultThumbprintAlgo = ((tb != null) ? (string) tb ["Algorithm"] : "SHA1");

			extensions = new Hashtable ();
			IDictionary exts = (IDictionary) ConfigurationSettings.GetConfig ("X509.Extensions");
			if (exts != null) {
				foreach (DictionaryEntry ext in exts)
					extensions.Add (ext.Key, ext.Value);
			}
		}

		private X509Extension CreateExtensionFromOid (string oid, object[] args) 
		{
			try {
				Type algoClass = null;
				string algo = (string) extensions [oid];
				// do we have an entry
				if (algo == null)
					return (X509Extension) args [0];
				algoClass = Type.GetType (algo);
				// call the constructor for the type
				return (X509Extension) Activator.CreateInstance (algoClass, args);
			}
			catch {
				// method doesn't throw any exception
				return (X509Extension) args [0];
			}
		}

		public CertificateFormatter (string filename)
		{
			byte[] data = null;
			using (FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
        			data = new byte [fs.Length];
        			fs.Read (data, 0, data.Length);
        			fs.Close ();
       			}
       			
       			if ((data != null) && (data.Length > 0)) {
        			X509Certificate x509 = null;
        			if (data [0] != 0x30) {
        				// it may be PEM encoded
        				data = FromPEM (data);
        			}
        			
        			if (data [0] == 0x30) {
        				x509 = new X509Certificate (data);
                			if (x509 != null) {
                				Initialize (x509);
                			}
        			}
			}
		}
		
		private byte[] FromPEM (byte[] data) 
		{
			string pem = Encoding.ASCII.GetString (data);
			int start = pem.IndexOf ("-----BEGIN CERTIFICATE-----");
			if (start < 0)
				return null;

			start += 27; // 27 being the -----BEGIN CERTIFICATE----- length
			int end = pem.IndexOf ("-----END CERTIFICATE-----", start);
			if (end < start)
				return null;
				
			string base64 = pem.Substring (start, (end - start));
			return Convert.FromBase64String (base64);
		}

		public CertificateFormatter (X509Certificate cert)
		{
			Initialize (cert);
		}

		internal void Initialize (X509Certificate cert) 
		{
			x509 = cert;
			thumbprintAlgorithm = defaultThumbprintAlgo;
			try {
				// preprocess some informations
				foreach (X509Extension xe in x509.Extensions) {
					if ((!extensions.ContainsKey (xe.Oid)) && (xe.Critical))
						status = unknownCriticalExtension;
					if (xe.Oid == "2.5.29.17") {
						SubjectAltNameExtension san = new SubjectAltNameExtension (xe);
						subjectAltName = san.RFC822;
					}
				}
				
				if (x509.IsSelfSigned) {
					status = untrustedRoot;
				}
			}
			catch (Exception e) {
				status = e.ToString ();
			}
		}

		public X509Certificate Certificate {
			get { return x509; }
		}

		public string Status {
			get { return status; }
		}

		public X509Extension GetExtension (int i) 
		{
			X509Extension xe = x509.Extensions [i];
			object[] extn = new object [1] { xe };
			return CreateExtensionFromOid (xe.Oid, extn); 
		}

		public string Extension (int i, bool detailed) 
		{
			X509Extension xe = x509.Extensions [i];
			if (!detailed)
				return Array2Word (xe.Value.Value);
			return Extension2String (x509.Extensions[i].Value.Value);
		}

		private string DN (string dname, bool detailed) 
		{
			string[] a = dname.Split (',');
			StringBuilder sb = new StringBuilder ();

			if (detailed) {
				foreach (string s in a) {
					string s2 = s.Trim () + Environment.NewLine;
					sb.Insert (0, s2.Replace ("=", " = "));
				}
			}
			else {
				foreach (string s in a) {
					string s2 = s.Trim ();
					sb.Insert (0, s2.Substring (s2.IndexOf ("=") + 1) + ", ");
				}
				// must remove last ", "
				sb.Remove (sb.Length - 2, 2);
			}

			return sb.ToString();
		}

		public string Issuer (bool detailed) 
		{
			return DN (x509.IssuerName, detailed);
		}

		public string PublicKey (bool detailed) 
		{
			if (detailed)
				return Array2Word (x509.PublicKey);

			if (x509.RSA != null)
				return "RSA (" + x509.RSA.KeySize + " Bits)";
			else if (x509.DSA != null)
				return "DSA (" + x509.DSA.KeySize + " Bits)";
			return "Unknown key type (unknown key size)";
		}

		public string SerialNumber (bool detailed) 
		{
			byte[] sn = (byte[]) x509.SerialNumber.Clone ();
			Array.Reverse (sn);
			return CertificateFormatter.Array2Word (sn);
		}

		public string Subject (bool detailed) 
		{
			return DN (x509.SubjectName, detailed);
		}

		public string SubjectAltName (bool detailed) 
		{
			if ((subjectAltName == null) || (subjectAltName.Length < 1))
				return String.Empty;
			if (!detailed)
				return "mailto:" + subjectAltName [0];

			StringBuilder sb = new StringBuilder ();
			foreach (string s in subjectAltName) {
				sb.Append (s);
				sb.Append (Environment.NewLine);
			}
			return sb.ToString ();
		}

		public string SignatureAlgorithm (bool detailed) 
		{
			string result = null;

			switch (x509.SignatureAlgorithm) {
				case "1.2.840.10040.4.3":
					result = "sha1DSA";
					break;
				case "1.2.840.113549.1.1.2":
					result = "md2RSA";
					status = String.Format (unsupportedHash, "MD2");
					break;
				case "1.2.840.113549.1.1.3":
					result = "md4RSA";
					status = String.Format (unsupportedHash, "MD4");
					break;
				case "1.2.840.113549.1.1.4":
					result = "md5RSA";
					break;
				case "1.2.840.113549.1.1.5":
					result = "sha1RSA";
					break;
				case "1.3.14.3.2.29":
					result = "sha1WithRSASignature";
					break;
				default:
					result = x509.SignatureAlgorithm;
					if (detailed)
						return "unknown (" + result + ")";
					return result;
			}
			if (detailed)
				result += " (" + x509.SignatureAlgorithm + ")";
			return result;
		}

		public string ThumbprintAlgorithm {
			get { return thumbprintAlgorithm.ToLower (); }
			set { thumbprintAlgorithm = value; }
		}

		public byte[] Thumbprint {
			get {
				HashAlgorithm ha = HashAlgorithm.Create (thumbprintAlgorithm);
				return ha.ComputeHash (x509.RawData);
			}
		}

		public string ValidFrom (bool detailed) 
		{
			return x509.ValidFrom.ToString ();
		}

		public string ValidUntil (bool detailed) 
		{
			return x509.ValidUntil.ToString ();
		}

		public string Version (bool detailed)
		{
			return "V" + x509.Version;
		}

		static public string OneLine (string input) 
		{
			// remove tabulation
			string oneline = input.Replace ("\t", "");
			// remove new lines after :
			oneline = oneline.Replace (":" + Environment.NewLine, ":");
			// remove ending new line (if present)
			if (oneline.EndsWith (Environment.NewLine))
				oneline = oneline.Substring (0, oneline.Length - Environment.NewLine.Length);
			// replace remaining new lines by comma + space
			return oneline.Replace (Environment.NewLine, ", ");
		}

		static public string Array2Word (byte[] array) 
		{
			StringBuilder sb = new StringBuilder ();
			int x = 0;
			while (x < array.Length) {
				sb.Append (array [x].ToString ("X2"));
				if (x % 2 == 1)
					sb.Append (" ");
				x++;
			}
			return sb.ToString ();
		}

		static private void WriteLine (StringBuilder sb, byte[] extnValue, int n, int pos) 
		{
			int p = pos;
			StringBuilder preview = new StringBuilder ();
			for (int j=0; j < 8; j++) {
				if (j < n) {
					sb.Append (extnValue [p++].ToString ("X2"));
					sb.Append (" ");
				}
				else
					sb.Append ("   ");
			}
			sb.Append ("  ");
			p = pos;
			for (int j=0; j < n; j++) {
				byte b = extnValue [p++];
				if (b < 0x20)
					sb.Append (".");
				else
					sb.Append (Convert.ToChar (b));
			}
			sb.Append (Environment.NewLine);
		}

		static public string Extension2String (byte[] extnValue) 
		{
			StringBuilder sb = new StringBuilder ();
			int div = (extnValue.Length >> 3);
			int rem = (extnValue.Length - (div << 3));
			int x = 0;
			for (int i=0; i < div; i++) {
				WriteLine (sb, extnValue, 8, x);
				x += 8;
			}
			WriteLine (sb, extnValue, rem, x);
			return sb.ToString ();
		}
	}
}
