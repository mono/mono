// Testsuite.System.AllSystemTests.cs
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using NUnit.Framework;

namespace MonoTests.System.Collections {
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
                                suite.AddTest(HashtableTest.Suite);
                                suite.AddTest(CaseInsensitiveComparerTest.Suite);
                                suite.AddTest(CaseInsensitiveHashCodeProviderTest.Suite);
                                suite.AddTest(BitArrayTest.Suite);
                                suite.AddTest(CollectionBaseTest.Suite);
                                suite.AddTest(ReadOnlyCollectionBaseTest.Suite);
                                suite.AddTest(ComparerTest.Suite);
                                suite.AddTest(StackTest.Suite);
                                return suite;
                        }
                }
        }
}

