//
// TestSuite.System.Security.Cryptography.AllCryptoTests.cs
//
// Author:
//      Thomas Neidhart (tome@sbox.tugraz.at)
//

using System;
using NUnit.Framework;

namespace Testsuite.System.Security.Cryptography {
        /// <summary>
        ///   Combines all available crypto unit tests into one test suite.
        /// </summary>
        public class AllCryptoTests : TestCase {
                public AllCryptoTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(SymmetricAlgorithmTest.Suite);
								suite.AddTest(AsymmetricAlgorithmTest.Suite); 	
                                return suite;
                        }
                }
        }
}
