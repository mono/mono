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
                
                public static ITest Suite { 
                        get 
                        {
                                TestSuite suite = new TestSuite();
                                suite.AddTest (ArrayTest.Suite);
				suite.AddTest (AttributeTest.Suite);
				suite.AddTest (BitConverterTest.Suite);
				suite.AddTest (BooleanTest.Suite);
                                suite.AddTest (ByteTest.Suite);
				suite.AddTest (CharEnumeratorTest.Suite);
                                suite.AddTest (CharTest.Suite);
                                suite.AddTest (ConsoleTest.Suite);
				suite.AddTest (ConvertTest.Suite);
				suite.AddTest (DateTimeTest.Suite);
				suite.AddTest (DecimalTest.Suite);
				suite.AddTest (DecimalTest2.Suite);
				suite.AddTest (DoubleTest.Suite);
				suite.AddTest (EnumTest.Suite);
				suite.AddTest (ExceptionTest.Suite);
				suite.AddTest (GuidTest.Suite);
                                suite.AddTest (Int16Test.Suite);
                                suite.AddTest (Int32Test.Suite);
                                suite.AddTest (Int64Test.Suite);
                                suite.AddTest (IntegerFormatterTest.Suite);
				suite.AddTest (MathTest.Suite);
				suite.AddTest (MulticastDelegateTest.Suite);
                                suite.AddTest (ObjectTest.Suite);
                                suite.AddTest (RandomTest.Suite);
                                suite.AddTest (ResolveEventArgsTest.Suite);
                                suite.AddTest (SByteTest.Suite);
				suite.AddTest (StringTest.Suite);
				suite.AddTest (TimeSpanTest.Suite);
				suite.AddTest (TimeZoneTest.Suite);
				suite.AddTest (UInt16Test.Suite);
                                suite.AddTest (UInt32Test.Suite);
                                suite.AddTest (UInt64Test.Suite);
				suite.AddTest (VersionTest.Suite);
				return suite;
                        }
                }
        }
}

