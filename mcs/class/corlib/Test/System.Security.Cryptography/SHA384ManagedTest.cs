//
// SHA384ManagedTest.cs - NUnit Test Cases for SHA384Managed
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004, 2007 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;

namespace MonoTests.System.Security.Cryptography {

// References:
// a.	FIPS PUB 180-2: Secure Hash Standard
//	http://csrc.nist.gov/publications/fips/fips180-2/fips180-2.pdf

// we inherit from SHA384Test because all SHA384 implementation must return the 
// same results (hence should run a common set of unit tests).

[TestFixture]
public class SHA384ManagedTest : SHA384Test {

	[SetUp]
	public override void SetUp () 
	{
		hash = new SHA384Managed ();
	}

	[Test]
	public override void Create () 
	{
		// no need to repeat this test
	}

	// none of those values changes for a particuliar implementation of SHA384
	[Test]
	public override void StaticInfo () 
	{
		// test all values static for SHA384
		base.StaticInfo ();
		string className = hash.ToString ();
		Assert.IsTrue (hash.CanReuseTransform, className + ".CanReuseTransform");
		Assert.IsTrue (hash.CanTransformMultipleBlocks, className + ".CanTransformMultipleBlocks");
		Assert.AreEqual ("System.Security.Cryptography.SHA384Managed", className, className + ".ToString()");
	}

	[Test]
	public void FIPSCompliance_Test1 () 
	{
		SHA384 sha = (SHA384) hash;
		// First test, we hash the string "abc"
		FIPS186_Test1 (sha);
	}

	[Test]
	public void FIPSCompliance_Test2 () 
	{
		SHA384 sha = (SHA384) hash;
		// Second test, we hash the string "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
		FIPS186_Test2 (sha);
	}

	[Test]
	public void FIPSCompliance_Test3 () 
	{
		SHA384 sha = (SHA384) hash;
		// Third test, we hash 1,000,000 times the character "a"
		FIPS186_Test3 (sha);
	}
}

}
