//
// SoftwarePublisherCertificate.cs 
//	- Software Publisher Certificates Implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Mono.Security;

namespace Mono.Security.Authenticode {

	public class SoftwarePublisherCertificate {

		private PKCS7.SignedData pkcs7;

		public SoftwarePublisherCertificate () 
		{
			pkcs7 = new PKCS7.SignedData ();
			pkcs7.ContentInfo.ContentType = PKCS7.data;
		}

		public SoftwarePublisherCertificate (byte[] spc) : this ()
		{
			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (spc);
			if (ci.ContentType != PKCS7.signedData)
				throw new ArgumentException ("Unsupported ContentType");
			pkcs7 = new PKCS7.SignedData (ci.Content);
		}

		public X509CertificateCollection Certificates {
			get { return pkcs7.Certificates; }
		}

		public ArrayList CRLs {
			get { return pkcs7.CRLs; }
		}

		public byte[] GetBytes () 
		{
			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (PKCS7.signedData);
			ci.Content.Add (pkcs7.ASN1);
			return ci.GetBytes ();
		}

		static public SoftwarePublisherCertificate CreateFromFile (string filename) 
		{
			FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] data = new byte [fs.Length];
			fs.Read (data, 0, data.Length);
			fs.Close ();
			return new SoftwarePublisherCertificate (data);
		}
	}
}
