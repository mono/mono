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

				suite.AddTest(ArrayTest.Suite);
				suite.AddTest(BitConverterTest.Suite);
				suite.AddTest(BooleanTest.Suite);
                                suite.AddTest(ByteTest.Suite);
				suite.AddTest(CharTest.Suite);
                                suite.AddTest(ConsoleTest.Suite);
				suite.AddTest(EnumTest.Suite);
				suite.AddTest(DecimalTest.Suite);
				suite.AddTest(DecimalTest2.Suite);
				suite.AddTest(GuidTest.Suite);
				suite.AddTest(Int16Test.Suite);
                                suite.AddTest(Int32Test.Suite);
				suite.AddTest(Int64Test.Suite);

				// MathTest: jit.c: line 1026 (mono_store_tree): assertion failed: (s->svt != VAL_UNKNOWN)
                                suite.AddTest(ObjectTest.Suite);

				// RandomTest: tree mismatch
				// (STIND_I4 ADDR_L[10] (CGT (LDIND_R8 ADDR_L[3]) CONST_R8))
				//
				// (STIND_I4 ADDR_L[10] (CGT (LDIND_R8 ADDR_L[3]) CONST_R8))
        			// BR
				// file emit-x86.c: line 561 (mono_label_cfg): should not be reached

                                suite.AddTest(ResolveEventArgsTest.Suite);

				// SByteTest: tree mismatch
				// (STIND_I4 ADDR_L[18] (MUL_OVF (CONV_OVF_I4 (LDIND_I1 ADDR_L[1])) CONST_I4))
				//
				// (STIND_I4 ADDR_L[18] (MUL_OVF (CONV_OVF_I4 (LDIND_I1 ADDR_L[1])) CONST_I4))
				// (STIND_I1 ADDR_L[1] (CONV_OVF_I1 (ADD_OVF (LDIND_I4 ADDR_L[18]) (SUB_OVF (LDIND_U2 ADDR_L[6]) CONST_I4))))
				// (STIND_I1 ADDR_L[5] CONST_I4)
				// BR
				// file emit-x86.c: line 561 (mono_label_cfg): should not be reached

				suite.AddTest(StringTest.Suite);
				suite.AddTest(TimeSpanTest.Suite);
				suite.AddTest(UInt16Test.Suite);
				suite.AddTest(UInt32Test.Suite);
                                suite.AddTest(UInt64Test.Suite);
				suite.AddTest(DoubleTest.Suite);
				suite.AddTest(TimeZoneTest.Suite);
				suite.AddTest(DateTimeTest.Suite);

				return suite;
                        }
                }
        }
}

