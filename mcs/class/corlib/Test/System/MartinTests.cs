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
				suite.AddTest(BooleanTest.Suite);
				suite.AddTest(StringTest.Suite);
				suite.AddTest(TimeSpanTest.Suite);
				suite.AddTest(DoubleTest.Suite);
				suite.AddTest(TimeZoneTest.Suite);
				suite.AddTest(DateTimeTest.Suite);

                                //suite.AddTest(RandomTest.Suite);
                                //suite.AddTest(ArrayTest.Suite);
				//suite.AddTest(BitConverterTest.Suite);
                                //suite.AddTest(ByteTest.Suite);
                                //suite.AddTest(CharTest.Suite);
                                //suite.AddTest(ConsoleTest.Suite);
				//suite.AddTest(EnumTest.Suite);
				//suite.AddTest(GuidTest.Suite);
                                //suite.AddTest(ObjectTest.Suite);
                                //suite.AddTest(ResolveEventArgsTest.Suite);
                                //suite.AddTest(SByteTest.Suite);


				return suite;
                        }
                }
        }
}

