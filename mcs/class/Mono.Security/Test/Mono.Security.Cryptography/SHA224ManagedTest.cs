//
// SHA224ManagedTest.cs - NUnit Test Cases for SHA224Managed
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Text;
using Mono.Security.Cryptography;

namespace MonoTests.System.Security.Cryptography {

	// References:
	// a.	RFC 3874 - A 224-bit One-way Hash Function: SHA-224, September 2004
	//	http://www.faqs.org/rfc/rfc3874.txt
	// b.	FIPS PUB 180-2: Secure Hash Standard
	//	http://csrc.nist.gov/publications/fips/fips180-2/fips180-2.pdf

	// we inherit from SHA224Test because all SHA224 implementation must return the 
	// same results (hence should run a common set of unit tests).

	[TestFixture]
	public class SHA224ManagedTest : SHA224Test {

		[SetUp]
		public override void SetUp () 
		{
			hash = new SHA224Managed ();
		}

		[Test]
		public override void Create () 
		{
			// no need to repeat this test
		}

		// none of those values changes for a particuliar implementation of SHA224
		[Test]
		public override void StaticInfo () 
		{
			// test all values static for SHA224
			base.StaticInfo ();
			string className = hash.ToString ();
			Assert.AreEqual (true, hash.CanReuseTransform, className + ".CanReuseTransform");
			Assert.AreEqual (true, hash.CanTransformMultipleBlocks, className + ".CanTransformMultipleBlocks");
// FIXME: Change namespace when (or if) classes are moved into corlib
			Assert.AreEqual ("Mono.Security.Cryptography.SHA224Managed", className, className + ".ToString()");
		}

		[Test]
		public void FIPSCompliance_Test1 () 
		{
			SHA224 sha = (SHA224) hash;
			// First test, we hash the string "abc"
			FIPS186_Test1 (sha);
		}

		[Test]
		public void FIPSCompliance_Test2 () 
		{
			SHA224 sha = (SHA224) hash;
			// Second test, we hash the string "abcdefghbcdefghicdefghijdefghijkefghijklfghijklmghijklmnhijklmnoijklmnopjklmnopqklmnopqrlmnopqrsmnopqrstnopqrstu"
			FIPS186_Test2 (sha);
		}

		[Test]
		public void FIPSCompliance_Test3 () 
		{
			SHA224 sha = (SHA224) hash;
			// Third test, we hash 1,000,000 times the character "a"
			FIPS186_Test3 (sha);
		}
	}
}
