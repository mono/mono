// Testsuite.System.AllSystemTests.cs
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using NUnit.Framework;

namespace Testsuite.System {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllSystemTests : TestCase {
                public AllSystemTests(string name) : base(name) {}
                
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
                                return suite;
                        }
                }
        }
}

