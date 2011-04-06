//
// TestSuite.System.Security.Cryptography.AsymmetricAlgorithmTest.cs
//
// Author:
//      Thomas Neidhart (tome@sbox.tugraz.at)
//


using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
	
	[TestFixture]
	public class AsymmetricAlgorithmTest : Assertion {
		private AsymmetricAlgorithm _algo;
		[SetUp]
		public void SetUp() {
			_algo = AsymmetricAlgorithm.Create();
		}

		private void SetDefaultData() {
		}
		
		[Test]
		public void TestProperties() {
			Assert("Properties (1)", _algo != null);
			
			bool thrown = false;
			try {
				KeySizes[] keys = _algo.LegalKeySizes;
				foreach (KeySizes myKey in keys) {
					for (int i=myKey.MinSize; i<=myKey.MaxSize; i+=myKey.SkipSize) {
						_algo.KeySize = i;
					}
				}
			} catch (CryptographicException) {thrown=true;}
			Assert("Properties (2)", !thrown);			
		}
	}
}
