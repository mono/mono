//
// Pkcs9DocumentDescription.cs - System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs9DocumentDescription : Pkcs9Attribute {

		private const string oid = "1.3.6.1.4.1.311.88.2.2";

		public Pkcs9DocumentDescription (string documentDescription)
			: base (new Oid (oid), documentDescription) {}
	}
}

#endif