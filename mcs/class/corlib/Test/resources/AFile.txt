//
// Mono.Tests.AllTests.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//
// (C) 2001
//

using System;
using NUnit.Framework;

namespace MonoTests {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(System.AllTests.Suite);
                                suite.AddTest(System.Collections.AllTests.Suite);
                                suite.AddTest(System.Security.AllTests.Suite);
                                suite.AddTest(System.Security.Cryptography.AllTests.Suite);
                                suite.AddTest(System.IO.AllTests.Suite);
                                suite.AddTest(System.Net.AllTests.Suite);
                                suite.AddTest(System.Text.AllTests.Suite);
                                suite.AddTest(System.Security.Permissions.AllTests.Suite);
                                suite.AddTest(System.Resources.AllTests.Suite);
//                                suite.AddTest(System.Security.Policy.AllTests.Suite);
                                return suite;
                        }
                }
        }
}
