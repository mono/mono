//
// System.Security.Cryptography.Pkcs.ContentInfo
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

	/*
	* ContentInfo ::= SEQUENCE {
	*	contentType ContentType,
	*	content [0] EXPLICIT ANY DEFINED BY contentType OPTIONAL 
	* }
	* ContentType ::= OBJECT IDENTIFIER
	*/

	public sealed class ContentInfo {

		private Oid _oid;
		private byte[] _content;

		// constructors

		public ContentInfo (byte[] content) 
			: this (new Oid ("1.2.840.113549.1.7.1"), content)
		{
		} 

		public ContentInfo (Oid oid, byte[] content) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");
			if (content == null)
				throw new ArgumentNullException ("content");

			_oid = oid;
			_content = content;
		}

		~ContentInfo () 
		{
		}

		// properties

		public byte[] Content { 
			get { return (byte[]) _content.Clone (); }
		}

		public Oid ContentType { 
			get { return _oid; }
		}

		// static methods

		[MonoTODO ("MS is stricter than us about the content structure")]
		public static Oid GetContentType (byte[] encodedMessage)
		{
			if (encodedMessage == null)
				throw new ArgumentNullException ("algorithm");

			try {
				PKCS7.ContentInfo ci = new PKCS7.ContentInfo (encodedMessage);
				switch (ci.ContentType) {
				case PKCS7.Oid.data:
				case PKCS7.Oid.signedData:		// see SignedCms class
				case PKCS7.Oid.envelopedData:		// see EnvelopedCms class
				case PKCS7.Oid.digestedData:
				case PKCS7.Oid.encryptedData:
					return new Oid (ci.ContentType);
				default:
					// Note: the constructor will accept any "valid" OID (but that 
					// doesn't mean it's a valid ContentType structure - ASN.1 wise).
					string msg = Locale.GetText ("Bad ASN1 - invalid OID '{0}'");
					throw new CryptographicException (String.Format (msg, ci.ContentType));
				}
			}
			catch (Exception e) {
				throw new CryptographicException (Locale.GetText ("Bad ASN1 - invalid structure"), e);
			}
		}
	}
}

#endif
