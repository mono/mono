// 
// MonoTaskSchedulerTests.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2011 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Mono.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.Mono.Threading.Tasks
{
	[TestFixtureAttribute]
	public class MonoTaskSchedulerTests
	{
		class DummyScheduler : TaskScheduler, IMonoTaskScheduler
		{
			public bool ParticipateMethod1 {
				get; set;
			}

			public bool ParticipateMethod2 {
				get; set;
			}

			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				return Enumerable.Empty<Task> ();
			}

			protected override void QueueTask (Task task)
			{
				
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				throw new NotSupportedException ();
			}

			public void ParticipateUntil (Task task)
			{
				ParticipateMethod1 = true;
			}

			public bool ParticipateUntil (Task task, ManualResetEventSlim predicateEvt, int millisecondsTimeout)
			{
				ParticipateMethod2 = true;
				return true;
			}
		}

		[Test]
		public void MethodRegisteringTest ()
		{
			DummyScheduler sched = new DummyScheduler ();

			Task t = new Task (delegate { Thread.Sleep (100); });
			t.Start (sched);
			t.Wait ();

			Assert.IsTrue (sched.ParticipateMethod1);
		}

		[Test]
		public void Method2RegisteringTest ()
		{
			DummyScheduler sched = new DummyScheduler ();

			Task t = new Task (delegate { Thread.Sleep (100); });
			t.Start (sched);
			t.Wait (100);

			Assert.IsTrue (sched.ParticipateMethod2);
		}

	}
}

#endif
