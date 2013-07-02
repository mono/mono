//
// SHA1CngTest.cs - NUnit Test Cases for SHA1Cng
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 180-1: Secure Hash Standard
	//	http://csrc.nist.gov/publications/fips/fips180-1/fip180-1.txt

	// we inherit from SHA1Test because all SHA1 implementation must return the 
	// same results (hence should run a common set of unit tests).

	[TestFixture]
	public class SHA1CngTest : SHA1Test {

		[SetUp]
		public override void SetUp ()
		{
			hash = new SHA1Cng ();
		}

		[Test]
		public override void Create ()
		{
			// no need to repeat this test
		}

		// none of those values changes for a particuliar implementation of SHA1
		[Test]
		public override void StaticInfo ()
		{
			// test all values static for SHA1
			base.StaticInfo ();
			string className = hash.ToString ();
			Assert.IsTrue (hash.CanReuseTransform, className + ".CanReuseTransform");
			Assert.IsTrue (hash.CanTransformMultipleBlocks, className + ".CanTransformMultipleBlocks");
			Assert.AreEqual ("System.Security.Cryptography.SHA1Cng", className, className + ".ToString()");
		}

		public void TestSHA1CSPforFIPSCompliance ()
		{
			SHA1 sha = (SHA1) hash;
			// First test, we hash the string "abc"
			FIPS186_Test1 (sha);
			// Second test, we hash the string "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"
			FIPS186_Test2 (sha);
			// Third test, we hash 1,000,000 times the character "a"
			FIPS186_Test3 (sha);
		}
	}
}

#endif
