//
// AuthenticodeFormatter.cs: Authenticode signature generator
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Net;

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.Authenticode {

	public class AuthenticodeFormatter : AuthenticodeBase {

		private Authority authority;
		private X509CertificateCollection certs;
		private ArrayList crls;
		private string hash;
		private RSA rsa;
		private Uri timestamp;
		private ASN1 authenticode;
		private PKCS7.SignedData pkcs7;
		private string description;
		private Uri url;
		private byte [] entry;

		public AuthenticodeFormatter () : base () 
		{
			certs = new X509CertificateCollection ();
			crls = new ArrayList ();
			authority = Authority.Maximum;
			pkcs7 = new PKCS7.SignedData ();
		}

		public Authority Authority {
			get { return authority; }
			set { authority = value; }
		}

		public X509CertificateCollection Certificates {
			get { return certs; }
		}

		public ArrayList Crl {
			get { return crls; }
		}

		public string Hash {
			get { 
				if (hash == null)
					hash = "MD5";
				return hash; 
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("Hash");

				string h = value.ToUpper (CultureInfo.InvariantCulture);
				switch (h) {
					case "MD5":
					case "SHA1":
						hash = h;
						break;
					default:
						throw new ArgumentException ("Invalid Authenticode hash algorithm");
				}
			}
		}

		public RSA RSA {
			get { return rsa; }
			set { rsa = value; }
		}

		public Uri TimestampUrl {
			get { return timestamp; }
			set { timestamp = value; }
		}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public Uri Url {
			get { return url; }
			set { url = value; }
		}

		private ASN1 AlgorithmIdentifier (string oid) 
		{
			ASN1 ai = new ASN1 (0x30);
			ai.Add (ASN1Convert.FromOid (oid));
			ai.Add (new ASN1 (0x05));	// NULL
			return ai;
		}

		private ASN1 Attribute (string oid, ASN1 value)
		{
			ASN1 attr = new ASN1 (0x30);
			attr.Add (ASN1Convert.FromOid (oid));
			ASN1 aset = attr.Add (new ASN1 (0x31));
			aset.Add (value);
			return attr;
		}

		private ASN1 Opus (string description, string url) 
		{
			ASN1 opus = new ASN1 (0x30);
			if (description != null) {
				ASN1 part1 = opus.Add (new ASN1 (0xA0));
				part1.Add (new ASN1 (0x80, Encoding.BigEndianUnicode.GetBytes (description)));
			}
			if (url != null) {
				ASN1 part2 = opus.Add (new ASN1 (0xA1));
				part2.Add (new ASN1 (0x80, Encoding.ASCII.GetBytes (url)));
			}
			return opus;
		}

		// pkcs 1
		private const string rsaEncryption = "1.2.840.113549.1.1.1";
		// pkcs 7
		private const string data = "1.2.840.113549.1.7.1";
		private const string signedData = "1.2.840.113549.1.7.2";
		// pkcs 9
		private const string contentType = "1.2.840.113549.1.9.3";
		private const string messageDigest  = "1.2.840.113549.1.9.4";
		private const string countersignature = "1.2.840.113549.1.9.6";
		// microsoft spc (software publisher certificate)
		private const string spcStatementType = "1.3.6.1.4.1.311.2.1.11";
		private const string spcSpOpusInfo = "1.3.6.1.4.1.311.2.1.12";
		private const string spcPelmageData = "1.3.6.1.4.1.311.2.1.15";
		private const string individualCodeSigning = "1.3.6.1.4.1.311.2.1.21";
		private const string commercialCodeSigning = "1.3.6.1.4.1.311.2.1.22";
		private const string timestampCountersignature = "1.3.6.1.4.1.311.3.2.1";

		//private static byte[] version = { 0x01 };
		private static byte[] obsolete = { 0x03, 0x01, 0x00, 0xA0, 0x20, 0xA2, 0x1E, 0x80, 0x1C, 0x00, 0x3C, 0x00, 0x3C, 0x00, 0x3C, 0x00, 0x4F, 0x00, 0x62, 0x00, 0x73, 0x00, 0x6F, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x74, 0x00, 0x65, 0x00, 0x3E, 0x00, 0x3E, 0x00, 0x3E };

		private byte[] Header (byte[] fileHash, string hashAlgorithm) 
		{
			string hashOid = CryptoConfig.MapNameToOID (hashAlgorithm);
			ASN1 content = new ASN1 (0x30);
			ASN1 c1 = content.Add (new ASN1 (0x30));
			c1.Add (ASN1Convert.FromOid (spcPelmageData));
			c1.Add (new ASN1 (0x30, obsolete));
			ASN1 c2 = content.Add (new ASN1 (0x30));
			c2.Add (AlgorithmIdentifier (hashOid));
			c2.Add (new ASN1 (0x04, fileHash));

			pkcs7.HashName = hashAlgorithm;
			pkcs7.Certificates.AddRange (certs);
			pkcs7.ContentInfo.ContentType = spcIndirectDataContext;
			pkcs7.ContentInfo.Content.Add (content);

			pkcs7.SignerInfo.Certificate = certs [0];
			pkcs7.SignerInfo.Key = rsa;

			ASN1 opus = null;
			if (url == null)
				Attribute (spcSpOpusInfo, Opus (description, null));
			else
				Attribute (spcSpOpusInfo, Opus (description, url.ToString ()));
			pkcs7.SignerInfo.AuthenticatedAttributes.Add (opus);
			pkcs7.SignerInfo.AuthenticatedAttributes.Add (Attribute (contentType, ASN1Convert.FromOid (spcIndirectDataContext)));
			pkcs7.SignerInfo.AuthenticatedAttributes.Add (Attribute (spcStatementType, new ASN1 (0x30, ASN1Convert.FromOid (commercialCodeSigning).GetBytes ())));
			pkcs7.GetASN1 (); // sign
			return pkcs7.SignerInfo.Signature;
		}

		public ASN1 TimestampRequest (byte[] signature) 
		{
			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (PKCS7.Oid.data);
			ci.Content.Add (new ASN1 (0x04, signature));
			return PKCS7.AlgorithmIdentifier (timestampCountersignature, ci.ASN1);
		}

		public void ProcessTimestamp (byte[] response)
		{
			ASN1 ts = new ASN1 (Convert.FromBase64String (Encoding.ASCII.GetString (response)));
			// first validate the received message
			// TODO

			// add the supplied certificates inside our signature
			for (int i=0; i < ts[1][0][3].Count; i++)
				pkcs7.Certificates.Add (new X509Certificate (ts[1][0][3][i].GetBytes ()));

			// add an unauthentified attribute to our signature
			pkcs7.SignerInfo.UnauthenticatedAttributes.Add (Attribute (countersignature, ts[1][0][4][0]));
		}

		public bool Sign (string fileName) 
		{
			string hashAlgorithm = "MD5";
			byte[] file = null;

			using (FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				file = new byte [fs.Length];
				fs.Read (file, 0, file.Length);
				fs.Close ();
			}

			// MZ - DOS header
			if (BitConverter.ToUInt16 (file, 0) != 0x5A4D)
				return false;

			// find offset of PE header
			int peOffset = BitConverter.ToInt32 (file, 60);
			if (peOffset > file.Length)
				return false;

			// PE - NT header
			if (BitConverter.ToUInt16 (file, peOffset) != 0x4550)
				return false;

			// IMAGE_DIRECTORY_ENTRY_SECURITY
			int dirSecurityOffset = BitConverter.ToInt32 (file, peOffset + 152);
			int dirSecuritySize = BitConverter.ToInt32 (file, peOffset + 156);

			if (dirSecuritySize > 8) {
				entry = new byte [dirSecuritySize - 8];
				Buffer.BlockCopy (file, dirSecurityOffset + 8, entry, 0, entry.Length);
			}
			else
				entry = null;

			HashAlgorithm hash = HashAlgorithm.Create (hashAlgorithm);
			// 0 to 215 (216) then skip 4 (checksum)
			int pe = peOffset + 88;
			hash.TransformBlock (file, 0, pe, file, 0);
			pe += 4;
			// 220 to 279 (60) then skip 8 (IMAGE_DIRECTORY_ENTRY_SECURITY)
			hash.TransformBlock (file, pe, 60, file, pe);
			pe += 68;
			// 288 to end of file
			int n = file.Length - pe;
			// minus any authenticode signature (with 8 bytes header)
			if (dirSecurityOffset != 0)
				n -= (dirSecuritySize);
			hash.TransformFinalBlock (file, pe, n);

			//
			byte[] signature = Header (hash.Hash, hashAlgorithm);
			if (timestamp != null) {
				ASN1 tsreq = TimestampRequest (signature);
				WebClient wc = new WebClient ();
				wc.Headers.Add ("Content-Type", "application/octet-stream");
				wc.Headers.Add ("Accept", "application/octet-stream");
				byte[] tsdata = Encoding.ASCII.GetBytes (Convert.ToBase64String (tsreq.GetBytes ()));
				byte[] tsres = wc.UploadData (timestamp.ToString (), tsdata);
				ProcessTimestamp (tsres);
			}
			PKCS7.ContentInfo sign = new PKCS7.ContentInfo (signedData);
			sign.Content.Add (pkcs7.ASN1);
			authenticode = sign.ASN1;

			byte[] asn = authenticode.GetBytes ();
			// debug
			if (Environment.GetEnvironmentVariable ("MONO_DEBUG") != null) {
				using (FileStream fs = File.Open (fileName + ".sig", FileMode.Create, FileAccess.Write)) {
					fs.Write (asn, 0, asn.Length);
					fs.Close ();
				}
			}

			File.Copy (fileName, fileName + ".bak", true);

			using (FileStream fs = File.Open (fileName, FileMode.Create, FileAccess.Write)) {
				// IMAGE_DIRECTORY_ENTRY_SECURITY (offset, size)
				byte[] data = BitConverter.GetBytes (file.Length);
				file [peOffset + 152] = data [0];
				file [peOffset + 153] = data [1];
				file [peOffset + 154] = data [2];
				file [peOffset + 155] = data [3];
				int size = asn.Length + 8;
				// must be a multiple of 8 bytes
				int addsize = (size % 8);
				if (addsize > 0)
					addsize = 8 - addsize;
				size += addsize;
				data = BitConverter.GetBytes (size);		// header
				file [peOffset + 156] = data [0];
				file [peOffset + 157] = data [1];
				file [peOffset + 158] = data [2];
				file [peOffset + 159] = data [3];
				fs.Write (file, 0, file.Length);
				fs.Write (data, 0, data.Length);		// length (again)
				data = BitConverter.GetBytes (0x00020200);	// magic
				fs.Write (data, 0, data.Length);
				fs.Write (asn, 0, asn.Length);
				// fill up
				byte[] fillup = new byte [addsize];
				fs.Write (fillup, 0, fillup.Length);
				fs.Close ();
			}
			return true;
		}

		// in case we just want to timestamp the file
		public bool Timestamp (string fileName) 
		{
			return true;
		}
	}
}
