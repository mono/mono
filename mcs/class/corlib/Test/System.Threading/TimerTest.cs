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

		// Doesn't work when I do a full run #72535
		[Category("NotWorking")]
		public void TestDueTime ()
		{
			counter = 0;
			Timer t = new Timer (new TimerCallback (Callback), null, 200, Timeout.Infinite);
			Thread.Sleep (50);
			AssertEquals ("t0", 0, counter);
			Thread.Sleep (200);
			AssertEquals ("t1", 1, counter);
			Thread.Sleep (500);
			AssertEquals ("t2", 1, counter);
			
			t.Change (10, 10);
			Thread.Sleep (500);
			Assert ("t3", counter > 20);
			t.Dispose ();
		}
		
		public void TestChange ()
		{
			counter = 0;
			Timer t = new Timer (new TimerCallback (Callback), null, 1, 1);
			Thread.Sleep (500);
			int c = counter;
			Assert ("t1", c > 20);
			t.Change (100, 100);
			Thread.Sleep (500);
			Assert ("t2", counter <= c + 6);
			t.Dispose ();
		}

		public void TestZeroDueTime () {
			counter = 0;

			Timer t = new Timer (new TimerCallback (Callback), null, 0, Timeout.Infinite);
			Thread.Sleep (100);
			AssertEquals (1, counter);
			t.Change (0, Timeout.Infinite);
			Thread.Sleep (100);
			AssertEquals (2, counter);
			t.Dispose ();
		}
		
		public void TestDispose ()
		{
			counter = 0;
			Timer t = new Timer (new TimerCallback (CallbackTestDispose), null, 10, 10);
			Thread.Sleep (200);
			t.Dispose ();
			Thread.Sleep (20);
			int c = counter;
			Assert (counter > 5);
			Thread.Sleep (200);
			AssertEquals (c, counter);
		}
		
		private void CallbackTestDispose (object foo) {
			counter ++;
		}

		Timer t1;
		public void TestDisposeOnCallback () {
			counter = 0;
			t1 = new Timer (new TimerCallback (CallbackTestDisposeOnCallback), null, 0, 10);
			Thread.Sleep (200);
			AssertNull (t1);
			
			counter = 2;
			t1 = new Timer (new TimerCallback (CallbackTestDisposeOnCallback), null, 50, 0);
			Thread.Sleep (200);
			AssertNull (t1);
		}
		
		private void CallbackTestDisposeOnCallback (object foo)
		{
			if (++counter == 3) {
				t1.Dispose ();
				t1 = null;
			}
		}
	}
}
