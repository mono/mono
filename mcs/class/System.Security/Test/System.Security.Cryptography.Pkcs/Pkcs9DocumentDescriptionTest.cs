//
// Pkcs9DocumentDescriptionTest.cs - NUnit tests for Pkcs9DocumentDescription
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
	public class Pkcs9DocumentDescriptionTest : Assertion {

		[Test]
		public void Constructor () 
		{
			Pkcs9DocumentDescription dd = new Pkcs9DocumentDescription ("mono");
			AssertNull ("Oid.FriendlyName", dd.Oid.FriendlyName);
			AssertEquals ("Oid.Value", "1.3.6.1.4.1.311.88.2.2", dd.Oid.Value);
			AssertEquals ("Values", 1, dd.Values.Count);
			AssertEquals ("Values[0]", "mono", (string) dd.Values [0]);
		}

		[Test]
		//BUG [ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNull () 
		{
			Pkcs9DocumentDescription dd = new Pkcs9DocumentDescription (null);
			AssertNull ("Oid.FriendlyName", dd.Oid.FriendlyName);
			AssertEquals ("Oid.Value", "1.3.6.1.4.1.311.88.2.2", dd.Oid.Value);
			AssertEquals ("Values", 1, dd.Values.Count);
			AssertNull ("Values[0]", dd.Values [0]);
		}
	}
}

#endif
