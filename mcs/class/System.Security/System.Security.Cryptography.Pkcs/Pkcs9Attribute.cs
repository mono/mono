//
// Pkcs9Attribute.cs - System.Security.Cryptography.Pkcs.Pkcs9Attribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;
using System.Collections;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs9Attribute : CryptographicAttribute {

		// constructors

		public Pkcs9Attribute (Oid oid) : base (oid) {}

		public Pkcs9Attribute (Oid oid, ArrayList values) : base (oid, values) {}

		public Pkcs9Attribute (Oid oid, object value) : base (oid, value) {}
	}
}

#endif