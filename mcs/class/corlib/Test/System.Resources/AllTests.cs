//
// MonoTests.System.Resources.AllTests.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;
using NUnit.Framework;

namespace MonoTests.System.Resources {
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
//FIXME: ResourceReader is not ready yet.
//                                suite.AddTest(ResourceReaderTest.Suite);
                                return suite;
                        }
                }
        }
}
