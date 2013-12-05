//
// TaskAwaiterTest_T.cs
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Runtime.CompilerServices;

namespace MonoTests.System.Runtime.CompilerServices
{
	[TestFixture]
	public class TaskAwaiterTest_T
	{
		class MyContext : SynchronizationContext
		{
			public int PostCounter;
			public ManualResetEvent mre = new ManualResetEvent (false);

			public override void OperationStarted ()
			{
				base.OperationStarted ();
			}

			public override void OperationCompleted ()
			{
				base.OperationCompleted ();
			}

			public override void Post (SendOrPostCallback d, object state)
			{
				++PostCounter;
				mre.Set ();
				base.Post (d, state);
			}

			public override void Send (SendOrPostCallback d, object state)
			{
				base.Send (d, state);
			}
		}

		Task<int> task;

		[Test]
		public void GetResultFaulted ()
		{
			TaskAwaiter<int> awaiter;

			task = new Task<int> (() => { throw new ApplicationException (); });
			awaiter = task.GetAwaiter ();
			task.RunSynchronously (TaskScheduler.Current);


			Assert.IsTrue (awaiter.IsCompleted);

			try {
				awaiter.GetResult ();
				Assert.Fail ();
			} catch (ApplicationException) {
			}
		}

		[Test]
		public void GetResultCanceled ()
		{
			TaskAwaiter<int> awaiter;

			var token = new CancellationToken (true);
			task = new Task<int> (() => 2, token);
			awaiter = task.GetAwaiter ();

			try {
				awaiter.GetResult ();
				Assert.Fail ();
			} catch (TaskCanceledException) {
			}
		}

		[Test]
		public void ContextTest ()
		{
			TaskAwaiter awaiter;

			var task = new Task (() => { throw new ApplicationException (); });
			awaiter = task.GetAwaiter ();
			task.RunSynchronously (TaskScheduler.Current);


			Assert.IsTrue (awaiter.IsCompleted);

			try {
				awaiter.GetResult ();
				Assert.Fail ();
			} catch (ApplicationException) {
			}

			var context = new MyContext ();

			var old = SynchronizationContext.Current;
			SynchronizationContext.SetSynchronizationContext (context);
			try {
				var t = new Task (delegate { });
				var a = t.GetAwaiter ();
				a.OnCompleted (delegate { });
				t.Start ();
				Assert.IsTrue (t.Wait (5000), "#1");
			} finally {
				SynchronizationContext.SetSynchronizationContext (old);
			}

			Assert.IsTrue (context.mre.WaitOne (5000), "#2");
			Assert.AreEqual (1, context.PostCounter, "#3");
		}
	}
}

#endif