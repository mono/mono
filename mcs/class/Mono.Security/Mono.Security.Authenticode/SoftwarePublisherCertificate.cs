//
// SoftwarePublisherCertificate.cs 
//	- Software Publisher Certificates Implementation
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

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

using Mono.Security;
using Mono.Security.X509;

namespace Mono.Security.Authenticode {

	public class SoftwarePublisherCertificate {

		private PKCS7.SignedData pkcs7;

		public SoftwarePublisherCertificate () 
		{
			pkcs7 = new PKCS7.SignedData ();
			pkcs7.ContentInfo.ContentType = PKCS7.Oid.data;
		}

		public SoftwarePublisherCertificate (byte[] data) : this ()
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (data);
			if (ci.ContentType != PKCS7.Oid.signedData) {
				throw new ArgumentException (
					Locale.GetText ("Unsupported ContentType"));
			}
			pkcs7 = new PKCS7.SignedData (ci.Content);
		}

		public X509CertificateCollection Certificates {
			get { return pkcs7.Certificates; }
		}

		public ArrayList Crls {
			get { return pkcs7.Crls; }
		}

		public byte[] GetBytes () 
		{
			PKCS7.ContentInfo ci = new PKCS7.ContentInfo (PKCS7.Oid.signedData);
			ci.Content.Add (pkcs7.ASN1);
			return ci.GetBytes ();
		}

		static public SoftwarePublisherCertificate CreateFromFile (string filename) 
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			byte[] data = null;
			using (FileStream fs = File.Open (filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return new SoftwarePublisherCertificate (data);
		}
	}
}
