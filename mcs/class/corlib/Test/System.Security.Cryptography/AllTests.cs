//
// TestSuite.System.Security.Cryptography.AllCryptoTests.cs
//
// Author:
//      Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot (spouliot@motus.com)
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
        /// <summary>
        ///   Combines all available crypto unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
        	// because most crypto stuff works with byte[] buffers
        	static public void AssertEquals (string msg, byte[] array1, byte[] array2)
        	{
        		if ((array1 == null) && (array2 == null))
        			return;
        		if (array1 == null)
        			Fail (msg + " -> First array is NULL");
        		if (array2 == null)
        			Fail (msg + " -> Second array is NULL");
        
        		bool a = (array1.Length == array2.Length);
        		if (a) {
        			for (int i = 0; i < array1.Length; i++) {
        				if (array1 [i] != array2 [i]) {
        					a = false;
        					break;
        				}
        			}
        		}
        		msg += " -> Expected " + BitConverter.ToString (array1, 0);
        		msg += " is different than " + BitConverter.ToString (array2, 0);
        		Assert (msg, a);
        	}

                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite ();
                                suite.AddTest (SymmetricAlgorithmTest.Suite);
				suite.AddTest (AsymmetricAlgorithmTest.Suite); 
                        	suite.AddTest (RNGCryptoServiceProviderTest.Suite);
				suite.AddTest (FromBase64TransformTest.Suite);
				suite.AddTest (RijndaelManagedTest.Suite);
				suite.AddTest (MD5Test.Suite);
				suite.AddTest (RC2Test.Suite);
				suite.AddTest (CryptoConfigTest.Suite);
//				suite.AddTest (DSATest.Suite);
				suite.AddTest (RSATest.Suite);
                                suite.AddTest (RandomNumberGeneratorTest.Suite);
                                suite.AddTest (HashAlgorithmTest.Suite);
                                suite.AddTest (SHA1Test.Suite);
                                suite.AddTest (SHA1CryptoServiceProviderTest.Suite);
				suite.AddTest (KeyedHashAlgorithmTest.Suite);
				suite.AddTest (HMACSHA1Test.Suite);
				suite.AddTest (PKCS1MaskGenerationMethodTest.Suite);
				suite.AddTest (RijndaelTest.Suite);
				suite.AddTest (PasswordDeriveBytesTest.Suite);
				suite.AddTest (SHA256Test.Suite);
				suite.AddTest (SHA256ManagedTest.Suite);
				suite.AddTest (SHA384Test.Suite);
//				suite.AddTest (SHA384ManagedTest.Suite);
				suite.AddTest (SHA512Test.Suite);
//				suite.AddTest (SHA512ManagedTest.Suite);
                                return suite;
                        }
                }
        }
}
