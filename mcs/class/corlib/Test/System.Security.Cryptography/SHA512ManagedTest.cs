//
// SHA512ManagedTest.cs - NUnit Test Cases for SHA512Managed
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

// we inherit from SHA512Test because all SHA512 implementation must return the 
// same results (hence should run a common set of unit tests).
public class SHA512ManagedTest : SHA512Test {
	protected override void SetUp () 
	{
		hash = new SHA512Managed ();
	}

	protected override void TearDown () {}

	public override void TestCreate () 
	{
		// no need to repeat this test
	}

	// none of those values changes for a particuliar implementation of SHA512
	public override void TestStaticInfo () 
	{
		// test all values static for SHA512
		base.TestStaticInfo ();
		string className = hash.ToString ();
		AssertEquals (className + ".CanReuseTransform", true, hash.CanReuseTransform);
		AssertEquals (className + ".CanTransformMultipleBlocks", true, hash.CanTransformMultipleBlocks);
		AssertEquals (className + ".ToString()", "System.Security.Cryptography.SHA512Managed", className);
	}

	public void TestSHA512forFIPSCompliance () 
	{
		SHA512 sha = (SHA512) hash;
		// First test, we hash the string "abc"
		FIPS186_Test1 (sha);
		// Second test, we hash the string "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
		FIPS186_Test2 (sha);
		// Third test, we hash 1,000,000 times the character "a"
		FIPS186_Test3 (sha);
	}
}

}
