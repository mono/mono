//
// ExecutionContextTest.cs - NUnit tests for ExecutionContext
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Remoting.Messaging;

#if NET_2_0

using System;
using System.Security;
using System.Threading;

using NUnit.Framework;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class ExecutionContextTest {

		static bool success;

		static void Callback (object o)
		{
			success = (bool)o;
		}

		public class CallContextValue : ILogicalThreadAffinative {
			public object Value { get; set; }

			public CallContextValue (object value)
			{
				this.Value = value;
			}
		}

		[SetUp]
		public void SetUp ()
		{
			success = false;
		}

		[TearDown]
		public void TearDown ()
		{
			if (ExecutionContext.IsFlowSuppressed ())
				ExecutionContext.RestoreFlow ();
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void CaptureCallContext ()
		{
			var value = new CallContextValue (true);
			object capturedValue = null;

			CallContext.SetData ("testlc", value);

			ExecutionContext ec = ExecutionContext.Capture ();
			Assert.IsNotNull (ec, "Capture");
			Assert.AreEqual (value, CallContext.GetData ("testlc")); 
			CallContext.SetData ("testlc", null);

			ExecutionContext.Run (ec, new ContextCallback (new Action<object> ((data) => {
				capturedValue = CallContext.GetData ("testlc");
			})), null);

			Assert.AreEqual (value, capturedValue); 
			Assert.AreNotEqual (value, CallContext.GetData ("testlc"));
		}

		[Test]
		public void Capture ()
		{
			ExecutionContext ec = ExecutionContext.Capture ();
			Assert.IsNotNull (ec, "Capture");

			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			try {
				ec = ExecutionContext.Capture ();
				Assert.IsNull (ec, "Capture with SuppressFlow");
			}
			finally {
				afc.Undo ();
			}
		}

		[Test]
		public void Copy ()
		{
			ExecutionContext ec = ExecutionContext.Capture ();
			Assert.IsNotNull (ec, "Capture");

			ExecutionContext copy = ec.CreateCopy ();
			Assert.IsNotNull (copy, "Copy of Capture");

			Assert.IsFalse (ec.Equals (copy));
			Assert.IsFalse (copy.Equals (ec));
			Assert.IsFalse (Object.ReferenceEquals (ec, copy));

			ExecutionContext copy2nd = copy.CreateCopy ();
			Assert.IsNotNull (copy2nd, "2nd level copy of Capture");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		// The context might be the result of capture so no exception is thrown
		[Category ("NotWorking")]
		public void Copy_FromThread ()
		{
			ExecutionContext ec = Thread.CurrentThread.ExecutionContext;
			Assert.IsNotNull (ec, "Thread.CurrentThread.ExecutionContext");

			ExecutionContext copy = ec.CreateCopy ();
		}

		[Test]
		public void IsFlowSuppressed ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");

			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			afc.Undo ();

			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void RestoreFlow_None ()
		{
			ExecutionContext.RestoreFlow ();
		}

		[Test]
		public void RestoreFlow_SuppressFlow ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");
			ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-2");
			ExecutionContext.RestoreFlow ();
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
		}

		[Test]
		[Category ("CAS")] // since r60298 the SecurityContext is only captured if the security manager is active
		public void Run () // see bug #78306 for details
		{
			Assert.IsFalse (success, "pre-check");
			ExecutionContext.Run (ExecutionContext.Capture (), new ContextCallback (Callback), true);
			Assert.IsTrue (success, "post-check");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Run_SuppressFlow ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed ());
			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed ());
			try {
				ExecutionContext.Run (ExecutionContext.Capture (), new ContextCallback (Callback), "Hello world.");
			}
			finally {
				afc.Undo ();
			}
		}

		[Test]
		public void SuppressFlow ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");

			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
			afc.Undo ();

			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void SuppressFlow_Two_Undo ()
		{
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-1");

			AsyncFlowControl afc = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-2");

			AsyncFlowControl afc2 = ExecutionContext.SuppressFlow ();
			Assert.IsTrue (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-3");
			afc2.Undo ();

			// note: afc2 Undo return to the original (not the previous) state
			Assert.IsFalse (ExecutionContext.IsFlowSuppressed (), "IsFlowSuppressed-4");

			// we can't use the first AsyncFlowControl
			afc.Undo ();
		}
	}
}

#endif
