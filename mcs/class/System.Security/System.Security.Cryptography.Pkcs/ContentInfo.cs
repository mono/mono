//
// ContentInfo.cs - System.Security.Cryptography.Pkcs.ContentInfo
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

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
			: this (new Oid ("1.2.840.113549.1.7.1", "PKCS 7 Data"), content) {} 

		public ContentInfo (Oid oid, byte[] content) 
		{
			if (oid == null)
				throw new ArgumentNullException ("oid");
			if (content == null)
				throw new ArgumentNullException ("content");

			_oid = oid;
			_content = content;
		}

		// properties

		public byte[] Content { 
			get { return _content; }
		}

		public Oid ContentType { 
			get { return _oid; }
		}

		// static methods

		[MonoTODO]
		public static Oid GetContentType (byte[] encodedMessage)
		{
// FIXME: compatibility with fx 1.2.3400.0
			if (encodedMessage == null)
				throw new NullReferenceException ();
//				throw new ArgumentNullException ("algorithm");

			try {
				return null;
			}
			catch (Exception e) {
				throw new CryptographicException ("Bad ASN1", e);
			}
		}
	}
}
