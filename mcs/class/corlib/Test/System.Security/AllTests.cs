//
// TestSuite.System.Security.AllSecurityTests.cs
//
// Lawrence Pit <loz@cable.a2000.nl>
// 

using System;
using NUnit.Framework;

namespace MonoTests.System.Security {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {

                public AllTests (string name) : base (name) {}
                
                public static ITest Suite { 
                        get {
                                TestSuite suite = new TestSuite ();
                                suite.AddTest (SecurityElementTest.Suite);
				return suite;
                        }
                }
        }
}

