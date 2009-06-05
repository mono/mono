//
// Pkcs9ContentType.cs - System.Security.Cryptography.Pkcs.Pkcs9ContentType
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) Tim Coleman, 2004
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

using Mono.Security;

namespace System.Security.Cryptography.Pkcs {

	public sealed class Pkcs9ContentType : Pkcs9AttributeObject {

		internal const string oid = "1.2.840.113549.1.9.3";
		internal const string friendlyName = "Content Type";

		private Oid _contentType;
		private byte[] _encoded;

		// constructors

		public Pkcs9ContentType () 
		{
			// Pkcs9Attribute remove the "set" accessor on Oid :-(
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			_encoded = null;
		}

		internal Pkcs9ContentType (string contentType) 
		{
			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			_contentType = new Oid (contentType);
			RawData = Encode ();
			_encoded = null;
		}

		internal Pkcs9ContentType (byte[] encodedContentType) 
		{
			if (encodedContentType == null)
				throw new ArgumentNullException ("encodedContentType");

			(this as AsnEncodedData).Oid = new Oid (oid, friendlyName);
			RawData = encodedContentType;
			Decode (encodedContentType);
		}

		// properties

		public Oid ContentType {
			get {
				if (_encoded != null)
					Decode (_encoded);
				return _contentType;
			}
		}

		// methods

		public override void CopyFrom (AsnEncodedData asnEncodedData)
		{
			base.CopyFrom (asnEncodedData);
			_encoded = asnEncodedData.RawData;
		}

		// internal stuff

		internal void Decode (byte[] attribute)
		{
			if ((attribute == null) || (attribute [0] != 0x06))
				throw new CryptographicException (Locale.GetText ("Expected an OID."));

			ASN1 oid = new ASN1 (attribute);
			_contentType = new Oid (ASN1Convert.ToOid (oid));
			_encoded = null;
		}

		internal byte[] Encode ()
		{
			if (_contentType == null)
				return null;
			return ASN1Convert.FromOid (_contentType.Value).GetBytes ();
		}
	}
}

#endif
