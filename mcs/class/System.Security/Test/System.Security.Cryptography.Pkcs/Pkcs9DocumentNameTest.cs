//
// Pkcs9DocumentNameTest.cs - NUnit tests for Pkcs9DocumentName
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using NUnit.Framework;

using System;
using System.Collections;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;

namespace MonoTests.System.Security.Cryptography.Pkcs {

	[TestFixture]
	public class Pkcs9DocumentNameTest : Assertion {

		[Test]
		public void Constructor () {
			Pkcs9DocumentName dn = new Pkcs9DocumentName ("mono");
			AssertNull ("Oid.FriendlyName", dn.Oid.FriendlyName);
			AssertEquals ("Oid.Value", "1.3.6.1.4.1.311.88.2.1", dn.Oid.Value);
			AssertEquals ("Values", 1, dn.Values.Count);
			AssertEquals ("Values[0]", "mono", (string) dn.Values [0]);
		}

		[Test]
			//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull () {
			Pkcs9DocumentName dn = new Pkcs9DocumentName (null);
			AssertNull ("Oid.FriendlyName", dn.Oid.FriendlyName);
			AssertEquals ("Oid.Value", "1.3.6.1.4.1.311.88.2.1", dn.Oid.Value);
			AssertEquals ("Values", 1, dn.Values.Count);
			AssertNull ("Values[0]", dn.Values [0]);
		}
	}
}

#endif
