//
// Ximian.Mono.Tests.AllTests.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//
// (C) 2001
//

using System;
using NUnit.Framework;

namespace Ximian.Mono.Tests {
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
                                suite.AddTest(new TestSuite(typeof(BitArrayTest)));
                                suite.AddTest(Testsuite.System.Collections.CaseInsensitiveComparerTest.Suite);
                                suite.AddTest(Testsuite.System.Collections.CaseInsensitiveHashCodeProviderTest.Suite);
                                suite.AddTest(CollectionBaseTest.Suite);
                                suite.AddTest(Testsuite.System.Collections.ComparerTest.Suite);
                                suite.AddTest(Testsuite.System.Collections.HashtableTest.Suite);
                                suite.AddTest(new TestSuite(typeof(MemoryStreamTest)));
                                suite.AddTest(new TestSuite(typeof(PathTest)));
                                // suite.AddTest(Testsuite.System.Collections.QueueTest.Suite);
                                suite.AddTest(new TestSuite(typeof(RandomTest)));
                                suite.AddTest(ReadOnlyCollectionBaseTest.Suite);
                                suite.AddTest(StackTest.Suite);
                                suite.AddTest(StackFrameTest.Suite);
                                suite.AddTest(StackTraceTest.Suite);
                                suite.AddTest(new TestSuite(typeof(StringBuilderTest)));
                                suite.AddTest(new TestSuite(typeof(StringReaderTest)));
                                suite.AddTest(new TestSuite(typeof(StringWriterTest)));
                        	
                                suite.AddTest(Testsuite.System.AllSystemTests.Suite);
                                suite.AddTest(Testsuite.System.Security.Cryptography.AllCryptoTests.Suite);
                                return suite;
                        }
                }
        }
}
