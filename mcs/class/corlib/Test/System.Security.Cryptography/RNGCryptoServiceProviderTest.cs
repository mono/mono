//
// TestSuite.System.Security.Cryptography.RNGCryptoServiceProviderTest.cs
//
// Author:
//      Mark Crichton (crichton@gimp.org)
//

using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	[TestFixture]
	public class RNGCryptoServiceProviderTest : Assertion {

		private RNGCryptoServiceProvider _algo;
		
		[SetUp]
		private void SetUp () 
		{
			_algo = new RNGCryptoServiceProvider ();
		}

		[Test]
		public void GetBytes () 
		{
			byte[] random = new byte [25];
			// The C code doesn't throw an exception yet.
			_algo.GetBytes (random);
		}

		[Test]
		public void GetNonZeroBytes () 
		{
			byte[] random = new byte [25];
			// This one we can check...
			_algo.GetNonZeroBytes (random);
			
			foreach (Byte rnd_byte in random) {
				Assert("Properties (2)", rnd_byte != 0);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetBytesNull () 
		{
			_algo.GetBytes (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNonZeroBytesNull () 
		{
			_algo.GetNonZeroBytes (null);
		}
	}
}
