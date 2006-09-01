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
// Authors:
//
//	Copyright (C) 2006 Jordi Mas i Hernandez <jordimash@gmail.com>
//

using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Workflow.Runtime;
using System.Reflection;

namespace MonoTests.System.Workflow.Runtime
{

	[TestFixture]
	[Category ("NotDotNet")] // Cannot test on .Net since internal constructors (there no public) are different
	public class TimerEventSubscriptionCollectionTest
	{
		[Test]
		public void TestCollection ()
		{
			TimerEventSubscriptionCollection col;

        		// There is no public constructor for TimerEventSubscriptionCollection
			col = (TimerEventSubscriptionCollection) Activator.CreateInstance
				(typeof (TimerEventSubscriptionCollection),
				BindingFlags.Instance |BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
				null, null, null);

			TimerEventSubscription event1 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2006, 07, 30));

			TimerEventSubscription event2 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2006, 07, 28));

			TimerEventSubscription event3 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2006, 08, 28));

			TimerEventSubscription event4 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2007, 08, 28));

			TimerEventSubscription event5 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2006, 05, 28));

			TimerEventSubscription event6 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2008, 02, 28));

			TimerEventSubscription event7 = new TimerEventSubscription (Guid.NewGuid (),
				new DateTime (2005, 05, 28));

			col.Add (event1);
			col.Add (event2);
			col.Add (event3);
			col.Add (event4);
			col.Add (event5);
			col.Add (event6);
			col.Add (event7);

			Assert.AreEqual (event7.ExpiresAt, col.Peek ().ExpiresAt, "C1#1");
			col.Remove (col.Peek ());
			Assert.AreEqual (event5.ExpiresAt, col.Peek ().ExpiresAt, "C1#2");
			col.Remove (col.Peek ());
			Assert.AreEqual (event2.ExpiresAt, col.Peek ().ExpiresAt, "C1#3");
			col.Remove (col.Peek ());
			Assert.AreEqual (event1.ExpiresAt, col.Peek ().ExpiresAt, "C1#4");
			col.Remove (col.Peek ());
			Assert.AreEqual (event3.ExpiresAt, col.Peek ().ExpiresAt, "C1#5");
			col.Remove (col.Peek ());
			Assert.AreEqual (event4.ExpiresAt, col.Peek ().ExpiresAt, "C1#6");

		}



	}
}

