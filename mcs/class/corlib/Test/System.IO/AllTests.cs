// Testsuite.System.AllSystemTests.cs
//
// Mario Martinez (mariom925@home.om)
//
// (C) Ximian, Inc.  http://www.ximian.com
// 

using System;
using NUnit.Framework;

namespace MonoTests.System.IO {
        /// <summary>
        ///   Combines all available unit tests into one test suite.
        /// </summary>
        public class AllTests : TestCase {
                public AllTests(string name) : base(name) {}
                
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite ();
                                suite.AddTest (BinaryReaderTest.Suite);
                                suite.AddTest (FileTest.Suite);
                                suite.AddTest (MemoryStreamTest.Suite);
                                suite.AddTest (PathTest.Suite);
                                suite.AddTest (StreamReaderTest.Suite);
                                suite.AddTest (StreamWriterTest.Suite);
                                suite.AddTest (StringReaderTest.Suite);
                                suite.AddTest (StringWriterTest.Suite);
                                return suite;
                        }
                }
        }
}

