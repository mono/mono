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

	public class RNGCryptoServiceProviderTest : TestCase {
		private RNGCryptoServiceProvider _algo;
		
		protected override void SetUp() {
			_algo = new RNGCryptoServiceProvider();
		}

		private void SetDefaultData() {
		}
		
		public void TestProperties() {
			Assert("Properties (1)", _algo != null);
			
			byte[] random = new Byte[25];

			// The C code doesn't throw an exception yet.
			_algo.GetBytes(random);
			
			// This one we can check...
			_algo.GetNonZeroBytes(random);
			
			foreach (Byte rnd_byte in random) {
				Assert("Properties (2)", rnd_byte != 0);
			}
		}
	}
}
