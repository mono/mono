// Testsuite.System.MartinSystemTests.cs
//
// Martin Baulig (martin@gnome.org)
//
// (C) 2002 Martin Baulig
// 

using System;
using NUnit.Framework;

namespace MonoTests.System {
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

				// ArrayTest: crashes
				suite.AddTest(BitConverterTest.Suite);
				suite.AddTest(BooleanTest.Suite);
                                suite.AddTest(ByteTest.Suite);
				// CharTest: crashes
                                suite.AddTest(ConsoleTest.Suite);
				// EnumTest: crashes
				suite.AddTest(GuidTest.Suite);
				// Int16Test: file codegen-x86.c: line 1489
                                suite.AddTest(Int32Test.Suite);
				// Int64Test: deadly NumberOverflow
                                suite.AddTest(ObjectTest.Suite);
				// RandomTest: tree mismatch
                                suite.AddTest(ResolveEventArgsTest.Suite);
				// SByteTest: tree mismatch
				suite.AddTest(StringTest.Suite);
				suite.AddTest(TimeSpanTest.Suite);
				suite.AddTest(UInt16Test.Suite);
                                // UInt32Test: file codegen-x86.c: line 1562
                                suite.AddTest(UInt64Test.Suite);
				suite.AddTest(DoubleTest.Suite);
				suite.AddTest(TimeZoneTest.Suite);
				suite.AddTest(DateTimeTest.Suite);

				return suite;
                        }
                }
        }
}

