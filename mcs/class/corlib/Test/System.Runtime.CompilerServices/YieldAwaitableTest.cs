//
// YieldAwaitebleTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin, Inc (http://www.xamarin.com)
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

#if NET_4_5

using System;
using System.Collections.Generic;
using NUnit.Framework;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;

namespace MonoTests.System.Runtime.CompilerServices
{
	[TestFixture]
	public class YieldAwaitableTest
	{
		class MyScheduler : TaskScheduler
		{
			protected override IEnumerable<Task> GetScheduledTasks ()
			{
				throw new NotImplementedException ();
			}

			protected override void QueueTask (Task task)
			{
				TryExecuteTask (task);
			}

			protected override bool TryExecuteTaskInline (Task task, bool taskWasPreviouslyQueued)
			{
				throw new NotImplementedException ();
			}
		}

		class MyContext : SynchronizationContext
		{
			public int Started;
			public int Completed;
			public int PostCounter;
			public int SendCounter;

			public override void OperationStarted ()
			{
				++Started;
				base.OperationStarted ();
			}

			public override void OperationCompleted ()
			{
				++Completed;
				base.OperationCompleted ();
			}

			public override void Post (SendOrPostCallback d, object state)
			{
				++PostCounter;
				base.Post (d, state);
			}

			public override void Send (SendOrPostCallback d, object state)
			{
				++SendCounter;
				base.Send (d, state);
			}
		}

		YieldAwaitable.YieldAwaiter a;

		[SetUp]
		public void Setup ()
		{
			a = new YieldAwaitable ().GetAwaiter ();
		}

		[Test]
		public void IsCompleted ()
		{
			Assert.IsFalse (a.IsCompleted, "#1");
			a.GetResult ();
			Assert.IsFalse (a.IsCompleted, "#1");
		}

		[Test]
		public void OnCompleted_1 ()
		{
			try {
				a.OnCompleted (null);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}
		}

		[Test]
		public void OnCompleted_2 ()
		{
			TaskScheduler scheduler = null;
			var mre = new ManualResetEvent (false);

			a.OnCompleted (() => {
				scheduler = TaskScheduler.Current;
				mre.Set ();
			});

			Assert.IsTrue (mre.WaitOne (1000), "#1");
			Assert.AreEqual (TaskScheduler.Current, scheduler, "#2");
		}

		[Test]
		public void OnCompleted_3 ()
		{
			var scheduler = new MyScheduler ();
			TaskScheduler ran_scheduler = null;

			var t = Task.Factory.StartNew (() => {
				var mre = new ManualResetEvent (false);

				a.OnCompleted (() => {
					ran_scheduler = TaskScheduler.Current;
					mre.Set ();
				});

				mre.WaitOne (1000);

			}, CancellationToken.None, TaskCreationOptions.None, scheduler);

			Assert.IsTrue (t.Wait (1000), "#1");
			Assert.AreEqual (scheduler, ran_scheduler, "#2");
		}

		[Test]
		public void OnCompleted_4 ()
		{
			SynchronizationContext context_ran = null;
			var mre = new ManualResetEvent (false);

			var context = new MyContext ();
			var old = SynchronizationContext.Current;			
			try {
				SynchronizationContext.SetSynchronizationContext (context);
				a.OnCompleted (() => {
					context_ran = SynchronizationContext.Current;
					mre.Set ();
				});

				Assert.IsTrue (mre.WaitOne (1000), "#1");

			} finally {
				SynchronizationContext.SetSynchronizationContext (old);
			}

			Assert.IsNull (context_ran, "#2");
		}
	}
}

#endif