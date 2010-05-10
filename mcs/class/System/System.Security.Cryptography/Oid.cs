//
// Oid.cs - System.Security.Cryptography.Oid
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell Inc. (http://www.novell.com)
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

using System.Security.Cryptography.X509Certificates;

namespace System.Security.Cryptography {

	public sealed class Oid {

		private string _value;
		private string _name;

		// constructors

		public Oid ()
		{
		}

		public Oid (string oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid;
			_name = GetName (oid);
		}

		public Oid (string value, string friendlyName)
		{
			_value = value;
			_name = friendlyName;
		}

		public Oid (Oid oid) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");

			_value = oid.Value;
			_name = oid.FriendlyName;
		}

		// properties

		public string FriendlyName {
			get { return _name; }
			set { 
				_name = value;
				_value = GetValue (_name);
			}
		}

		public string Value { 
			get { return _value; }
			set { 
				_value = value; 
				_name = GetName (_value);
			}
		}

		// internal stuff

		// Known OID/Names not defined anywhere else (by OID order)
		internal const string oidRSA = "1.2.840.113549.1.1.1";
		internal const string nameRSA = "RSA";
		internal const string oidPkcs7Data = "1.2.840.113549.1.7.1";
		internal const string namePkcs7Data = "PKCS 7 Data";
		internal const string oidPkcs9ContentType = "1.2.840.113549.1.9.3";
		internal const string namePkcs9ContentType = "Content Type";
		internal const string oidPkcs9MessageDigest = "1.2.840.113549.1.9.4";
		internal const string namePkcs9MessageDigest = "Message Digest";
		internal const string oidPkcs9SigningTime = "1.2.840.113549.1.9.5";
		internal const string namePkcs9SigningTime = "Signing Time";
		internal const string oidMd5 = "1.2.840.113549.2.5";
		internal const string nameMd5 = "md5";
		internal const string oid3Des = "1.2.840.113549.3.7";
		internal const string name3Des = "3des";
		internal const string oidSha1 = "1.3.14.3.2.26";
		internal const string nameSha1 = "sha1";
		internal const string oidSubjectAltName = "2.5.29.17";
		internal const string nameSubjectAltName = "Subject Alternative Name";
		internal const string oidNetscapeCertType = "2.16.840.1.113730.1.1";
		internal const string nameNetscapeCertType = "Netscape Cert Type";

		// TODO - find the complete list
		private string GetName (string oid) 
		{
			switch (oid) {
				case oidRSA:
					return nameRSA;
				case oidPkcs7Data:
					return namePkcs7Data;
				case oidPkcs9ContentType:
					return namePkcs9ContentType;
				case oidPkcs9MessageDigest:
					return namePkcs9MessageDigest;
				case oidPkcs9SigningTime:
					return namePkcs9SigningTime;
				case oid3Des:
					return name3Des;
				case X509BasicConstraintsExtension.oid:
					return X509BasicConstraintsExtension.friendlyName;
				case X509KeyUsageExtension.oid:
					return X509KeyUsageExtension.friendlyName;
				case X509EnhancedKeyUsageExtension.oid:
					return X509EnhancedKeyUsageExtension.friendlyName;
				case X509SubjectKeyIdentifierExtension.oid:
					return X509SubjectKeyIdentifierExtension.friendlyName;
				case oidSubjectAltName:
					return nameSubjectAltName;
				case oidNetscapeCertType:
					return nameNetscapeCertType;
				case oidMd5:
					return nameMd5;
				case oidSha1:
					return nameSha1;
				default:
					return _name;
			}
		}

		// TODO - find the complete list
		private string GetValue (string name) 
		{
			switch (name) {
				case nameRSA:
					return oidRSA;
				case namePkcs7Data:
					return oidPkcs7Data;
				case namePkcs9ContentType:
					return oidPkcs9ContentType;
				case namePkcs9MessageDigest:
					return oidPkcs9MessageDigest;
				case namePkcs9SigningTime:
					return oidPkcs9SigningTime;
				case name3Des:
					return oid3Des;
				case X509BasicConstraintsExtension.friendlyName:
					return X509BasicConstraintsExtension.oid;
				case X509KeyUsageExtension.friendlyName:
					return X509KeyUsageExtension.oid;
				case X509EnhancedKeyUsageExtension.friendlyName:
					return X509EnhancedKeyUsageExtension.oid;
				case X509SubjectKeyIdentifierExtension.friendlyName:
					return X509SubjectKeyIdentifierExtension.oid;
				case nameSubjectAltName:
					return oidSubjectAltName;
				case nameNetscapeCertType:
					return oidNetscapeCertType;
				case nameMd5:
					return oidMd5;
				case nameSha1:
					return oidSha1;
				default:
					return _value;
			}
		}
	}
}

#endif
