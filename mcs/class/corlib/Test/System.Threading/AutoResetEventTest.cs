//
// AutoResetEventTest.cs - NUnit test cases for System.Threading.AutoResetEvent
//
// Author:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class AutoResetEventTest : Assertion {
		[Test]
		public void MultipleSet ()
		{
			AutoResetEvent evt = new AutoResetEvent (true);
			Assertion.AssertEquals ("#01", true, evt.WaitOne (1000, false));
			evt.Set ();
			evt.Set ();
			Assertion.AssertEquals ("#02", true, evt.WaitOne (1000, false));
			Assertion.AssertEquals ("#03", false, evt.WaitOne (1000, false));
		}
	}
}

