// Testsuite.System.AllSystemTests.cs
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using NUnit.Framework;

namespace MonoTests.System {
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
                                suite.AddTest(ByteTest.Suite);
                                suite.AddTest(SByteTest.Suite);
                                suite.AddTest(Int16Test.Suite);
                                suite.AddTest(Int32Test.Suite);
                                suite.AddTest(Int64Test.Suite);
				suite.AddTest(UInt16Test.Suite);
                                suite.AddTest(UInt32Test.Suite);
                                suite.AddTest(UInt64Test.Suite);
                                suite.AddTest(RandomTest.Suite);
                                suite.AddTest(ResolveEventArgsTest.Suite);
				suite.AddTest(StringTest.Suite);
				return suite;
                        }
                }
        }
}

