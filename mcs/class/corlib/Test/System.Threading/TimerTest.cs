//
// TimerTest.cs - NUnit test cases for System.Threading.Timer
//
// Author:
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Threading;

namespace MonoTests.System.Threading {

	[TestFixture]
	public class TimerTest : Assertion {

		public int counter;

		private void Callback (object foo) {
			counter ++;
		}

		public void TestZeroDueTime () {
			counter = 0;

			Timer t = new Timer (new TimerCallback (Callback), null, 0, Timeout.Infinite);
			Thread.Sleep (100);
			AssertEquals (1, counter);
			t.Change (0, Timeout.Infinite);
			Thread.Sleep (100);
			AssertEquals (2, counter);
		}
	}
}
