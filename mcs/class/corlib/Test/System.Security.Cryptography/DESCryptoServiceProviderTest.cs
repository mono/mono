//
// TestSuite.System.Security.Cryptography.DESCryptoServiceProviderTest.cs
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
			Assert.AreEqual (8, key.Length, "Key");
			Assert.IsFalse (DES.IsWeakKey (key), "IsWeakKey");
			Assert.IsFalse (DES.IsSemiWeakKey (key), "IsSemiWeakKey");
		}

		[Test]
		public void IV () 
		{
			byte[] iv = des.IV;
			Assert.AreEqual (8, iv.Length, "IV");
		}

		// other tests (test vectors) are inherited from DESFIPS81Test
		// (in DESTest.cs) but executed here
	}
}
