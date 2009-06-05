//
// System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.Text;

using Mono.Security;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs9DocumentDescription : Pkcs9AttributeObject {

		internal const string oid = "1.3.6.1.4.1.311.88.2.2";
		internal const string friendlyName = null;

		private string _desc;

		public Pkcs9DocumentDescription ()
		{
			// Pkcs9Attribute remove the "set" accessor on Oid :-(
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
		}

		public Pkcs9DocumentDescription (string documentDescription)
		{
			if (documentDescription == null)
				throw new ArgumentNullException ("documentName");

			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			_desc = documentDescription;
			RawData = Encode ();
		}

		public Pkcs9DocumentDescription (byte[] encodedDocumentDescription)
		{
			if (encodedDocumentDescription == null)
				throw new ArgumentNullException ("encodedDocumentDescription");

			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			RawData = encodedDocumentDescription;
			Decode (encodedDocumentDescription);
		}

		public string DocumentDescription {
			get { return _desc; }
		}

		public override void CopyFrom (AsnEncodedData asnEncodedData)
		{
			base.CopyFrom (asnEncodedData);
			Decode (this.RawData);
		}

		// internal stuff

		internal void Decode (byte[] attribute)
		{
			if (attribute [0] != 0x04)
				return; // throw ?

			ASN1 attr = new ASN1 (attribute);
			byte[] str = attr.Value;
			int length = str.Length;
			if (str [length - 2] == 0x00)
				length -= 2;	// zero-terminated (normal)
			_desc = Encoding.Unicode.GetString (str, 0, length);
		}

		internal byte[] Encode ()
		{
			// OCTETSTRING (0x04) Of the zero-terminated unicode string
			ASN1 attr = new ASN1 (0x04, Encoding.Unicode.GetBytes (_desc + (char)0));
			return attr.GetBytes ();
		}
	}
}

#endif
