//
// TestSuite.System.Security.Cryptography.AllCryptoTests.cs
//
// Author:
//      Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Security.Cryptography {
        /// <summary>
        ///   Combines all available crypto unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(SymmetricAlgorithmTest.Suite);
				suite.AddTest(AsymmetricAlgorithmTest.Suite); 
                        	suite.AddTest(RNGCryptoServiceProviderTest.Suite);
				suite.AddTest(FromBase64TransformTest.Suite);
				suite.AddTest(RijndaelManagedTest.Suite);
                                return suite;
                        }
                }
        }
}
