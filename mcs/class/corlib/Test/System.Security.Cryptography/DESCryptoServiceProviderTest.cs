//
// TestSuite.System.Security.Cryptography.DESCryptoServiceProviderTest.cs
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class DESCryptoServiceProviderTest : DESFIPS81Test {

		[SetUp]
		public void SetUp () 
		{
			des = new DESCryptoServiceProvider ();
		}

		[Test]
		public void KeyChecks () 
		{
			byte[] key = des.Key;
			AssertEquals ("Key", 8, key.Length);
			Assert ("IsWeakKey", !DES.IsWeakKey (key));
			Assert ("IsSemiWeakKey", !DES.IsSemiWeakKey (key));
		}

		[Test]
		public void IV () 
		{
			byte[] iv = des.IV;
			AssertEquals ("IV", 8, iv.Length);
		}

		// other tests (test vectors) are inherited from DESFIPS81Test
		// (in DESTest.cs) but executed here
	}
}
