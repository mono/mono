//
// System.Web.HttpClientCertificate class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class HttpClientCertificate : NameValueCollection {

		HttpWorkerRequest hwr;
		int flags;
		DateTime from;
		DateTime until;


		internal HttpClientCertificate (HttpWorkerRequest hwr)
		{
#if NET_2_0
			// we don't check hwr for null so we end up throwing a 
			// NullReferenceException just like MS implementation
			// if the public ctor for HttpRequest is used
#else
			if (hwr == null)
				throw new ArgumentNullException ("hwr");
#endif
			this.hwr = hwr;
			flags = GetIntNoPresense ("CERT_FLAGS");
			if (IsPresent) {
				from = hwr.GetClientCertificateValidFrom ();
				until = hwr.GetClientCertificateValidUntil ();
			} else {
				from = DateTime.Now;
				until = from;
			}
		}


		public byte[] BinaryIssuer {
			get { return hwr.GetClientCertificateBinaryIssuer (); }
		}

		public int CertEncoding {
			get { return hwr.GetClientCertificateEncoding (); }
		}

		public byte[] Certificate {
			get { return hwr.GetClientCertificate (); }
		}

		public string Cookie {
			get { return GetString ("CERT_COOKIE"); }
		}

		public int Flags {
			get { return flags; }
		}

		public bool IsPresent {
			get { return ((flags & 0x01) == 0x01); }
		}

		public string Issuer {
			get { return GetString ("CERT_ISSUER"); }
		}

		public bool IsValid {
			get {
				if (!IsPresent)
					return true; // lame but true
				// low on details
				return ((flags & 0x02) == 0x00);
			}
		}

		public int KeySize {
			get { return GetInt ("CERT_KEYSIZE"); }
		}

		public byte[] PublicKey {
			get { return hwr.GetClientCertificatePublicKey (); }
		}

		public int SecretKeySize {
			get { return GetInt ("CERT_SECRETKEYSIZE"); }
		}

		public string SerialNumber {
			get { return GetString ("CERT_SERIALNUMBER"); }
		}

		public string ServerIssuer {
			get { return GetString ("CERT_SERVER_ISSUER"); }
		}

		public string ServerSubject {
			get { return GetString ("CERT_SERVER_SUBJECT"); }
		}

		public string Subject {
			get { return GetString ("CERT_SUBJECT"); }
		}

		public DateTime ValidFrom {
			get { return from; }
		}

		public DateTime ValidUntil {
			get { return until; }
		}


		// LAMESPEC: this doesn't return values added with Add(string,string)
		public override string Get (string field)
		{
			switch (field) {
			default:
				return String.Empty;
			}
		}

		// stuff
		int GetInt (string variable)
		{
			if (!IsPresent)
				return 0;

			return GetIntNoPresense (variable);
		}

		int GetIntNoPresense (string variable)
		{
			string s = hwr.GetServerVariable (variable);
			if (s == null)
				return 0;

			try {
				return Int32.Parse (s, Helpers.InvariantCulture);
			}
			catch {
				return 0;
			}
		}

		string GetString (string variable)
		{
			if (!IsPresent)
				return String.Empty;

			string s = hwr.GetServerVariable (variable);
			return (s == null) ? String.Empty : s;
		}
	}
}
