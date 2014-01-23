//
// SHA256CngTest.cs - NUnit Test Cases for SHA256Cng
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007-2008 Novell, Inc (http://www.novell.com)
//

#if !MOBILE

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	FIPS PUB 180-2: Secure Hash Standard
	//	http://csrc.nist.gov/publications/fips/fips180-2/fips180-2.pdf

	// we inherit from SHA256Test because all SHA256 implementation must return the 
	// same results (hence should run a common set of unit tests).

	[TestFixture]
	public class SHA256CngTest : SHA256Test {

		[SetUp]
		public override void SetUp ()
		{
			hash = new SHA256Cng ();
		}

		[Test]
		public override void Create ()
		{
			// no need to repeat this test
		}

		// none of those values changes for a particuliar implementation of SHA256
		[Test]
		public override void StaticInfo ()
		{
			// test all values static for SHA256
			base.StaticInfo ();
			string className = hash.ToString ();
			Assert.IsTrue (hash.CanReuseTransform, className + ".CanReuseTransform");
			Assert.IsTrue (hash.CanTransformMultipleBlocks, className + ".CanTransformMultipleBlocks");
			Assert.AreEqual ("System.Security.Cryptography.SHA256Cng", className, className + ".ToString()");
		}

		[Test]
		public void FIPSCompliance_Test1 ()
		{
			SHA256 sha = (SHA256) hash;
			// First test, we hash the string "abc"
			FIPS186_Test1 (sha);
		}

		[Test]
		public void FIPSCompliance_Test2 ()
		{
			SHA256 sha = (SHA256) hash;
			// Second test, we hash the string "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
			FIPS186_Test2 (sha);
		}

		[Test]
		public void FIPSCompliance_Test3 ()
		{
			SHA256 sha = (SHA256) hash;
			// Third test, we hash 1,000,000 times the character "a"
			FIPS186_Test3 (sha);
		}
	}
}

#endif
