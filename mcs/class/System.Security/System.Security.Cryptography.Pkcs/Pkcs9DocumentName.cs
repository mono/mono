//
// Pkcs9DocumentName.cs - System.Security.Cryptography.Pkcs.Pkcs9DocumentName
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs9DocumentName : Pkcs9Attribute	{

		private const string oid = "1.3.6.1.4.1.311.88.2.1";

		public Pkcs9DocumentName (string documentName) 
			: base (new Oid (oid), documentName) {}
	}
}

#endif