//
// MonoTests.System.Diagnostics.StackFrameTest.cs
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
        public class StackFrameTest {
                private StackFrameTest() {}
                public static ITest Suite 
                { 
                        get 
                        {
                                TestSuite suite =  new TestSuite();
                                suite.AddTest(StackFrameTest1.Suite);
                                suite.AddTest(StackFrameTest2.Suite);
                                suite.AddTest(StackFrameTest3.Suite);
                                return suite;
                        }
                }

        /// <summary>
        ///   Tests the case where StackFrame is created for specified file name and
        ///   location inside it.
        /// </summary>
        private class StackFrameTest1 : TestCase {
                public StackFrameTest1(string name) : base(name) {}
                
                private StackFrame frame1;
                private StackFrame frame2;
                
                internal static ITest Suite 
                { 
                        get 
                        {
                                return  new TestSuite(typeof(StackFrameTest1));
                        }
                }
                
                protected override void SetUp() {
                        frame1 = new StackFrame("dir/someFile", 13, 45);
                        frame2 = new StackFrame("SomeFile2.cs", 24);
                }
                
                protected override void TearDown() {
                        frame1 = null;
                        frame2 = null;
                }
                
                
                
                /// <summary>
                ///   Tests whether getting file name works.
                /// </summary>
                public void TestGetFileName() {
                        AssertEquals("File name (1)",
                                     "dir/someFile",
                                     frame1.GetFileName());
                                     
                        AssertEquals("File name (2)",
                                     "SomeFile2.cs",
                                     frame2.GetFileName());
                }
                
                /// <summary>
                ///   Tests whether getting file line number works.
                /// </summary>
                public void TestGetFileLineNumber() {
                        AssertEquals("Line number (1)",
                                     13,
                                     frame1.GetFileLineNumber());
                                     
                        AssertEquals("Line number (2)",
                                     24,
                                     frame2.GetFileLineNumber());
                }
                
                /// <summary>
                ///   Tests whether getting file column number works.
                /// </summary>
                public void TestGetFileColumnNumber() {
                        AssertEquals("Column number (1)",
                                     45,
                                     frame1.GetFileColumnNumber());
                                     
                        AssertEquals("Column number (2)",
                                     0,
                                     frame2.GetFileColumnNumber());
                }
                
                                
                /// <summary>
                ///   Tests whether getting method associated with frame works.
                /// </summary>
                public void TestGetMethod() {
                        Assert("Method not null (1)", (frame1.GetMethod() != null));

                        AssertEquals("Class declaring the method (1)",
                                     this.GetType(),
                                     frame1.GetMethod().DeclaringType);
                        AssertEquals("Method name (1)",
                                     "SetUp",
                                     frame1.GetMethod().Name);
                                     
                        Assert("Method not null (2)", (frame2.GetMethod() != null));
                        
                        AssertEquals("Class declaring the method (2)",
                                     this.GetType(),
                                     frame2.GetMethod().DeclaringType);
                        AssertEquals("Method name (2)",
                                     "SetUp",
                                     frame2.GetMethod().Name);
                }
        }
        
        /// <summary>
        ///   Tests the case where StackFrame is created for current method.
        /// </summary>
        /// <remarks>
        ///   FIXME: Must be compiled with /debug switch. Otherwise some file
        ///   information will be incorrect for the following test cases.
        ///   What's the best way to do both types of tests with and without
        ///   debug information?
        /// </remarks>
        private class StackFrameTest2 : TestCase {
                public StackFrameTest2(string name) : base(name) {}
                
                private StackFrame frame1;
                private StackFrame frame2;
                private StackFrame frame3;
                
                internal static ITest Suite 
                { 
                        get 
                        {
                                return  new TestSuite(typeof(StackFrameTest2));
                        }
                }
                
                protected override void SetUp() {
                        frame1 = new StackFrame();
                        frame2 = new StackFrame(true);
                        frame3 = new StackFrame(0);
                }
                
                protected override void TearDown() {
                        frame1 = null;
                        frame2 = null;
                        frame3 = null;
                }
                
                
                
                /// <summary>
                ///   Tests whether getting file name works.
                /// </summary>
                public void TestGetFileName() {
                        AssertNull("File name (1)",
                                   frame1.GetFileName());
                                     
                        Assert("File name (2) " + frame2.GetFileName()
                                        + " ends with StackFrameTest.cs",
                               frame2.GetFileName().EndsWith("StackFrameTest.cs"));
                }
                
                /// <summary>
                ///   Tests whether getting file line number works.
                /// </summary>
                public void TestGetFileLineNumber() {
                        AssertEquals("Line number (1)",
                                     0,
                                     frame1.GetFileLineNumber());
                                     
                        AssertEquals("Line number (2)",
                                     154,
                                     frame2.GetFileLineNumber());
                                     
                        AssertEquals("Line number (3)",
                                     0,
                                     frame3.GetFileLineNumber());
                }
                
                /// <summary>
                ///   Tests whether getting file column number works.
                /// </summary>
                public void TestGetFileColumnNumber() {
                        AssertEquals("Column number (1)",
                                     0,
                                     frame1.GetFileColumnNumber());
                                     
                        AssertEquals("Column number (2)",
                                     25,
                                     frame2.GetFileColumnNumber());
                        
                        AssertEquals("Column number (3)",
                                     0,
                                     frame3.GetFileColumnNumber());
                }
                
                                
                /// <summary>
                ///   Tests whether getting method associated with frame works.
                /// </summary>
                public void TestGetMethod() {
                        Assert("Method not null (1)",
                               (frame1.GetMethod() != null));

                        AssertEquals("Class declaring the method (1)",
                                     this.GetType(),
                                     frame1.GetMethod().DeclaringType);
                        AssertEquals("Method name (1)",
                                     "SetUp",
                                     frame1.GetMethod().Name);
                                     
                        Assert("Method not null (2)",
                               (frame2.GetMethod() != null));
                        
                        AssertEquals("Class declaring the method (2)",
                                     this.GetType(),
                                     frame2.GetMethod().DeclaringType);
                        AssertEquals("Method name (2)",
                                     "SetUp",
                                     frame2.GetMethod().Name);
                                     
                        Assert("Method not null (3)",
                               (frame3.GetMethod() != null));

                        AssertEquals("Class declaring the method (3)",
                                     this.GetType(),
                                     frame3.GetMethod().DeclaringType);
                        AssertEquals("Method name (3)",
                                     "SetUp",
                                     frame3.GetMethod().Name);
                }
        }
        
        
        /// <summary>
        ///   Tests the case where StackFrame is created for current method but
        ///   skipping some frames.
        /// </summary>
        /// <remarks>
        ///   FIXME: Must be compiled with /debug switch. Otherwise some file
        ///   information will be incorrect for the following test cases.
        ///   What's the best way to do both types of tests with and without
        ///   debug information?
        /// </remarks>
        private class StackFrameTest3 : TestCase {
                public StackFrameTest3(string name) : base(name) {}
                
                private StackFrame frame1;
                private StackFrame frame2;
                
                internal static ITest Suite 
                { 
                        get 
                        {
                                return  new TestSuite(typeof(StackFrameTest3));
                        }
                }
                
                protected override void SetUp() {
                        // In order to get better test cases with stack traces
                        NestedSetUp();
                }
                
                private void NestedSetUp() {
                        frame1 = new StackFrame(2);
                        frame2 = new StackFrame(1, true);
                }
                
                protected override void TearDown() {
                        frame1 = null;
                        frame2 = null;
                }
                
                
                
                /// <summary>
                ///   Tests whether getting file name works.
                /// </summary>
                public void TestGetFileName() {
                        AssertNull("File name (1)",
                                   frame1.GetFileName());
                                     
                        Assert("File name (2) " + frame2.GetFileName()
                                        + " ends with StackFrameTest.cs",
                               frame2.GetFileName().EndsWith("StackFrameTest.cs"));
                }
                
                /// <summary>
                ///   Tests whether getting file line number works.
                /// </summary>
                public void TestGetFileLineNumber() {
                        AssertEquals("Line number (1)",
                                     0,
                                     frame1.GetFileLineNumber());
                                     
                        AssertEquals("Line number (2)",
                                     277,
                                     frame2.GetFileLineNumber());
                }
                
                /// <summary>
                ///   Tests whether getting file column number works.
                /// </summary>
                public void TestGetFileColumnNumber() {
                        AssertEquals("Column number (1)",
                                     0,
                                     frame1.GetFileColumnNumber());
                                     
                        AssertEquals("Column number (2)",
                                     17,
                                     frame2.GetFileColumnNumber());
                }
                
                                
                /// <summary>
                ///   Tests whether getting method associated with frame works.
                /// </summary>
                public void TestGetMethod() {
                        Assert("Method not null (1)", (frame1.GetMethod() != null));

                        AssertEquals("Class declaring the method (1)",
                                     typeof(NUnit.Framework.TestCase),
                                     frame1.GetMethod().DeclaringType);
                        AssertEquals("Method name (1)",
                                     "RunBare",
                                     frame1.GetMethod().Name);
                                     
                        Assert("Method not null (2)", (frame2.GetMethod() != null));
                        
                        AssertEquals("Class declaring the method (2)",
                                     this.GetType(),
                                     frame2.GetMethod().DeclaringType);
                        AssertEquals("Method name (2)",
                                     "SetUp",
                                     frame2.GetMethod().Name);
                }
	}
        }
}
