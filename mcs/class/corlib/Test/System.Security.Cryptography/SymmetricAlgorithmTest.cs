//
// TestSuite.System.Security.Cryptography.SymmetricAlgorithmTest.cs
//
// Author:
//      Thomas Neidhart (tome@sbox.tugraz.at)
//


using System;
using System.Security.Cryptography;

using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {

	public class SymmetricAlgorithmTest : TestCase {
		private SymmetricAlgorithm _algo;
		
		public SymmetricAlgorithmTest() : base ("MonoTests.System.Security.Cryptography.SymmetricAlgorithmTest testcase") {
			_algo = null;
		}
		public SymmetricAlgorithmTest(String name) : base(name) {
			_algo = null;
		}
		
		public static ITest Suite {
			get {
				return new TestSuite(typeof(SymmetricAlgorithmTest));
			}
		}

		protected override void SetUp() {
			_algo = SymmetricAlgorithm.Create();
		}

		private void SetDefaultData() {
		}
		
		public void TestProperties() {
			Assert("Properties (1)", _algo != null);
			
			bool thrown = false;
			try 
			{
				// try setting an illegal blocksize -> must throw an exception
				_algo.BlockSize = 12;
			} catch (CryptographicException) {thrown = true;}
			Assert("Properties (2)", thrown);
			
			byte[] key = _algo.Key;
			Assert("Properties (3)", key != null);
			
			thrown = false;
			try {
				KeySizes[] keys = _algo.LegalKeySizes;
				foreach (KeySizes myKey in keys) {
					for (int i=myKey.MinSize; i<=myKey.MaxSize; i+=myKey.SkipSize) {
						_algo.KeySize = i;
					}
				}
			} catch (CryptographicException) {thrown=true;}
			Assert("Properties (4)", !thrown);
		}
	}
}
