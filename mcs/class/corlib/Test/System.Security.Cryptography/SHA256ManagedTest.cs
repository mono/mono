//
// SHA256ManagedTest.cs - NUnit Test Cases for SHA256Managed
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

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
public class SHA256ManagedTest : SHA256Test {
	protected override void SetUp () 
	{
		hash = new SHA256Managed ();
	}

	protected override void TearDown () {}

	public override void TestCreate () 
	{
		// no need to repeat this test
	}

	// none of those values changes for a particuliar implementation of SHA256
	public override void TestStaticInfo () 
	{
		// test all values static for SHA256
		base.TestStaticInfo ();
		string className = hash.ToString ();
		AssertEquals (className + ".CanReuseTransform", true, hash.CanReuseTransform);
		AssertEquals (className + ".CanTransformMultipleBlocks", true, hash.CanTransformMultipleBlocks);
		AssertEquals (className + ".ToString()", "System.Security.Cryptography.SHA256Managed", className);
	}

	public void TestSHA256forFIPSCompliance () 
	{
		SHA256 sha = (SHA256) hash;
		// First test, we hash the string "abc"
		FIPS186_Test1 (sha);
		// Second test, we hash the string "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
		FIPS186_Test2 (sha);
		// Third test, we hash 1,000,000 times the character "a"
		FIPS186_Test3 (sha);
	}
}

}
