//
// Mono.Tests.MartinTests.cs
//
// Author:
//      Martin Baulig (martin@gnome.org)
//
// (C) 2002 Martin Baulig
//

using System;
using NUnit.Framework;

namespace MonoTests {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class MartinTests : TestCase {
                public MartinTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(System.MartinTests.Suite);
                                return suite;
                        }
                }
        }
}
