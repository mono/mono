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

namespace MonoTests.System.Workflow.Runtime
{
	public class ourTimerEventSubscription : TimerEventSubscription
	{
		public ourTimerEventSubscription () : base ()
		{

		}

		public void SetQueueName (IComparable name)
		{
			QueueName = name;
		}
	}

	[TestFixture]
	public class TimerEventSubscriptionTest
	{
		[Test]
		public void Constructor1 ()
		{
			TimerEventSubscription timer;
			Guid timerId = Guid.NewGuid ();
			Guid workflowInstanceId = Guid.NewGuid ();
			DateTime expiresAt = DateTime.Today;

			timer = new TimerEventSubscription (timerId, workflowInstanceId, expiresAt);

			Assert.AreEqual (timerId, timer.SubscriptionId, "C1#1");
			Assert.AreEqual (workflowInstanceId, timer.WorkflowInstanceId, "C1#2");
			Assert.AreEqual (timerId, timer.QueueName, "C1#3");
		}

		[Test]
		public void Constructor2 ()
		{
			TimerEventSubscription timer;
			Guid workflowInstanceId = Guid.NewGuid ();
			DateTime expiresAt = DateTime.Today;

			timer = new TimerEventSubscription (workflowInstanceId, expiresAt);
			Assert.AreEqual (workflowInstanceId, timer.WorkflowInstanceId, "C1#1");
			Assert.AreEqual (timer.SubscriptionId, timer.QueueName, "C1#2");
		}

		[Test]
		public void Constructor3 ()
		{
			ourTimerEventSubscription timer = new ourTimerEventSubscription ();
			Assert.AreEqual (Guid.Empty, timer.WorkflowInstanceId, "C1#1");
			Assert.AreEqual (null, timer.QueueName, "C1#2");
			Assert.AreEqual (Guid.Empty, timer.WorkflowInstanceId, "C1#3");

			timer.SetQueueName ("OurName");
			Assert.AreEqual ("OurName", timer.QueueName, "C1#4");
		}

	}
}

