//
// Pkcs9SigningTime.cs - System.Security.Cryptography.Pkcs.Pkcs9SigningTime
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.Pkcs {

	public class Pkcs9SigningTime : Pkcs9Attribute {

		private const string oid = "1.2.840.113549.1.9.5";
		private const string name = "Signing Time";

		public Pkcs9SigningTime () : this (DateTime.Now) {}

		public Pkcs9SigningTime (DateTime signingTime) 
			: base (new Oid (oid, name), signingTime)  {}
	}
}

#endif