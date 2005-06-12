//
// MonoTests.System.Diagnostics.StackTraceTest.cs
//
// Authors:
//      Alexander Klyubin (klyubin@aqris.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Diagnostics {

	[TestFixture]
	public class StackTraceTest {

		private StackTrace trace;
		private StackFrame frame;
		
		[SetUp]
		public void SetUp ()
		{
			frame = new StackFrame ("dir/someFile", 13, 45);
			trace = new StackTrace (frame);
		}

		[TearDown]
		public void TearDown ()
		{
			trace = null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void StackTrace_Int_Negative ()
		{
			new StackTrace (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StackTrace_Exception_Null ()
		{
			Exception e = null;
			new StackTrace (e);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StackTrace_ExceptionBool_Null ()
		{
			Exception e = null;
			new StackTrace (e, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StackTrace_ExceptionInt_Null ()
		{
			Exception e = null;
			new StackTrace (e, 1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void StackTrace_ExceptionInt_Negative ()
		{
			new StackTrace (new Exception (), -1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StackTrace_ExceptionIntBool_Null ()
		{
			Exception e = null;
			new StackTrace (e, 1, true);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void StackTrace_ExceptionIntBool_Negative ()
		{
			new StackTrace (new Exception (), -1, true);
		}

		[Test]
		public void StackTrace_StackFrame_Null ()
		{
			StackFrame sf = null;
			StackTrace st = new StackTrace (sf);
			// no exception
			Assert.AreEqual (1, st.FrameCount, "FrameCount");
			Assert.IsNull (st.GetFrame (0), "Empty Frame");
		}

		[Test]
		[Ignore ("Not supported in Mono")]
		public void StackTrace_Thread_Null ()
		{
			Thread t = null;
			StackTrace st = new StackTrace (t, true);
			// no exception
		}

		static void EmptyThread ()
		{
			Thread.Sleep (1000);
		}

		[Test]
#if !NET_2_0
		// on MS .NET 1.x, ThreadState after Start() is Unstarted
		[Category ("NotDotNet")]
#endif
		[ExpectedException (typeof (ThreadStateException))]
		[Ignore ("Not supported in Mono")]
		public void StackTrace_Thread_NotSuspended ()
		{
			Thread t = new Thread (new ThreadStart (EmptyThread));
			t.Start ();
			new StackTrace (t, true);
		}

		[Test]
		[Ignore ("Not supported in Mono")]
		public void StackTrace_Thread_Suspended ()
		{
			Thread t = new Thread (new ThreadStart (EmptyThread));
			t.Start ();
			t.Suspend ();
			new StackTrace (t, true);
		}

		[Test]
		public void FrameCount ()
		{
			Assert.AreEqual (1, trace.FrameCount, "Frame count");
		}

		[Test]
		public void GetFrame_OutOfRange ()
		{
			Assert.IsNull (trace.GetFrame (-1), "-1");
			Assert.IsNull (trace.GetFrame (-129), "-129");
			Assert.IsNull (trace.GetFrame (1), "1");
			Assert.IsNull (trace.GetFrame (145), "145");

			Assert.IsNull (trace.GetFrame (Int32.MinValue), "MinValue");
			Assert.IsNull (trace.GetFrame (Int32.MaxValue), "MaxValue");
		}

		[Test]
		public void GetFrame ()
		{
			Assert.AreEqual (frame, trace.GetFrame (0), "0");
		}
#if NET_2_0
		[Test]
		public void GetFrames ()
		{
			StackTrace st = new StackTrace ();
			StackFrame[] sf = st.GetFrames ();
			Assert.AreEqual (st.FrameCount, sf.Length, "Count");
			for (int i=0; i < sf.Length; i++) {
				Assert.AreEqual (sf [i], st.GetFrame (i), i.ToString ());
			}
		}
#endif
		[Test]
		public void UnthrownException ()
		{
			Assert.AreEqual (0, new StackTrace (new Exception ()).FrameCount, "Unthrown exception");
		}
	}
}
