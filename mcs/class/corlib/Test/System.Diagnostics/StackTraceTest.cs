//
// MonoTests.System.Diagnostics.StackTraceTest.cs
//
// Author:
//      Alexander Klyubin (klyubin@aqris.com)
//
// (C) 2001
//

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics {
        public class StackTraceTest {
                private StackTraceTest() {}
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(StackTraceTest1.Suite);
                                return suite;
                        }
                }

        /// <summary>
        ///   Tests the case where StackTrace is created for specified
        ///   stack frame.
        /// </summary>
        private class StackTraceTest1 : TestCase {
                public StackTraceTest1(string name) : base(name) {}
                
                private StackTrace trace;
                private StackFrame frame;
                
                internal static ITest Suite 
                { 
                        get 
                        {
                                return  new TestSuite(typeof(StackTraceTest1));
                        }
                }
                
                protected override void SetUp() {
                        frame = new StackFrame("dir/someFile",
                                               13,
                                               45);
                        trace = new StackTrace(frame);
                }
                
                protected override void TearDown() {
                        trace = null;
                }
                
                
                
                /// <summary>
                ///   Tests whether getting number of frames works.
                /// </summary>
                public void TestFrameCount() {
                        AssertEquals("Frame count",
                                     1,
                                     trace.FrameCount);
                }
                
                /// <summary>
                ///   Tests whether getting frames by index which is out of
                ///   range works.
                /// </summary>
                public void TestGetFrameOutOfRange() {
                        Assert("Frame with index -1 == null",
                               (trace.GetFrame(-1) == null));
                        
                        Assert("Frame with index -129 = null",
                               (trace.GetFrame(-129) == null));
                               
                        Assert("Frame with index 1 = null",
                               (trace.GetFrame(1) == null));
                               
                        Assert("Frame with index 145 = null",
                               (trace.GetFrame(145) == null));

                }
        
        
                /// <summary>
                ///   Tests whether getting frames by index works.
                /// </summary>
                public void TestGetFrame() {
                        AssertEquals("Frame with index 0",
                                     frame,
                                     trace.GetFrame(0));
                }                
	}
        }
}
