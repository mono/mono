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
        /// <summary>
        ///   Tests the case where StackTrace is created for specified
        ///   stack frame.
        /// </summary>
	[TestFixture]
	public class StackTraceTest1 : TestCase {
                private StackTrace trace;
                private StackFrame frame;
                
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
