//
// BigIntegerTest.cs - NUnit Test Cases for BigInteger
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
using Mono.Math;

namespace MonoTests.Mono.Math {

	[TestFixture]
	public class BigIntegerTest {

		[Test]
		public void DefaultBitCount () 
		{
			BigInteger bi = new BigInteger ();
			Assert.AreEqual (0, bi.BitCount (), "default BitCount");
			// note: not bit are set so BitCount is zero
		}

		[Test]
		public void DefaultRandom () 
		{
			// based on bugzilla entry #68452
			BigInteger bi = new BigInteger ();
			Assert.AreEqual (0, bi.BitCount (), "before randomize");
			bi.Randomize ();
			// Randomize returns a random number of BitCount length
			// so in this case it will ALWAYS return 0
			Assert.AreEqual (0, bi.BitCount (), "after randomize");
			Assert.AreEqual (new BigInteger (0), bi, "Zero");
		}
	}
}
