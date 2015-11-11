// 
// TaskCompletionSourceTests.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
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


using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;
#if !MOBILE
using NUnit.Framework.SyntaxHelpers;
#endif

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskCompletionSourceTests
	{
		TaskCompletionSource<int> completionSource;
		object state;
		
		[SetUp]
		public void Setup ()
		{
			state = new object ();
			completionSource = new TaskCompletionSource<int> (state, TaskCreationOptions.None);
		}
		
		[Test]
		public void CreationCheckTest ()
		{
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.AreEqual (TaskCreationOptions.None, completionSource.Task.CreationOptions, "#2");
		}

		[Test]
		public void CtorInvalidOptions ()
		{
			try {
				new TaskCompletionSource<long> (TaskCreationOptions.LongRunning);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new TaskCompletionSource<long> (TaskCreationOptions.PreferFairness);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}
		}
		
		[Test]
		public void SetResultTest ()
		{
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.IsTrue (completionSource.TrySetResult (42), "#2");
			Assert.AreEqual (TaskStatus.RanToCompletion, completionSource.Task.Status, "#3");
			Assert.AreEqual (42, completionSource.Task.Result, "#4");
			Assert.IsFalse (completionSource.TrySetResult (43), "#5");
			Assert.AreEqual (TaskStatus.RanToCompletion, completionSource.Task.Status, "#6");
			Assert.AreEqual (42, completionSource.Task.Result, "#7");
			Assert.IsFalse (completionSource.TrySetCanceled (), "#8");
			Assert.AreEqual (TaskStatus.RanToCompletion, completionSource.Task.Status, "#9");
		}
		
		[Test]
		public void SetCanceledTest ()
		{
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.IsTrue (completionSource.TrySetCanceled (), "#2");
			Assert.AreEqual (TaskStatus.Canceled, completionSource.Task.Status, "#3");
			Assert.IsFalse (completionSource.TrySetResult (42), "#4");
			Assert.AreEqual (TaskStatus.Canceled, completionSource.Task.Status, "#5");

			try {
				Console.WriteLine (completionSource.Task.Result);
				Assert.Fail ("#6");
			} catch (AggregateException e) {
				var details = (TaskCanceledException) e.InnerException;
				Assert.AreEqual (completionSource.Task, details.Task, "#6e");
				Assert.IsNull (details.Task.Exception, "#6e2");
			}
		}

		[Test]
		public void SetExceptionTest ()
		{
			Exception e = new Exception ("foo");
			
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.IsTrue (completionSource.TrySetException (e), "#2");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#3");
			Assert.That (completionSource.Task.Exception, Is.TypeOf(typeof (AggregateException)), "#4.1");
			
			AggregateException aggr = (AggregateException)completionSource.Task.Exception;
			Assert.AreEqual (1, aggr.InnerExceptions.Count, "#4.2");
			Assert.AreEqual (e, aggr.InnerExceptions[0], "#4.3");
			
			Assert.IsFalse (completionSource.TrySetResult (42), "#5");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#6");
			Assert.IsFalse (completionSource.TrySetCanceled (), "#8");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#9");
		}

		[Test]
		public void SetExceptionInvalid ()
		{
			try {
				completionSource.TrySetException (new ApplicationException[0]);
				Assert.Fail ("#1");
			} catch (ArgumentException) {
			}

			try {
				completionSource.TrySetException (new [] { new ApplicationException (), null });
				Assert.Fail ("#2");
			} catch (ArgumentException) {
			}

			Assert.AreEqual (TaskStatus.WaitingForActivation, completionSource.Task.Status, "r1");
		}
		
		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void SetResultExceptionTest ()
		{
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.IsTrue (completionSource.TrySetResult (42), "#2");
			Assert.AreEqual (TaskStatus.RanToCompletion, completionSource.Task.Status, "#3");
			Assert.AreEqual (42, completionSource.Task.Result, "#4");
			
			completionSource.SetResult (43);
		}

		[Test]
		public void ContinuationTest ()
		{
			bool result = false;
			var t = completionSource.Task.ContinueWith ((p) => { if (p.Result == 2) result = true; });
			Assert.AreEqual (TaskStatus.WaitingForActivation, completionSource.Task.Status, "#A");
			completionSource.SetResult (2);
			t.Wait ();
			Assert.AreEqual (TaskStatus.RanToCompletion, completionSource.Task.Status, "#1");
			Assert.AreEqual (TaskStatus.RanToCompletion, t.Status, "#2");
			Assert.IsTrue (result);
		}

		[Test]
		public void FaultedFutureTest ()
		{
			var thrown = new ApplicationException ();
			var source = new TaskCompletionSource<int> ();
			source.TrySetException (thrown);
			var f = source.Task;
			AggregateException ex = null;
			try {
				f.Wait ();
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.AreEqual (thrown, ex.InnerException);
			Assert.AreEqual (thrown, f.Exception.InnerException);
			Assert.AreEqual (TaskStatus.Faulted, f.Status);

			ex = null;
			try {
				var result = f.Result;
			} catch (AggregateException e) {
				ex = e;
			}

			Assert.IsNotNull (ex);
			Assert.AreEqual (TaskStatus.Faulted, f.Status);
			Assert.AreEqual (thrown, f.Exception.InnerException);
			Assert.AreEqual (thrown, ex.InnerException);
		}

		[Test]
		[Ignore ("#4550, Mono GC is lame")]
		public void SetExceptionAndUnobservedEvent ()
		{
			bool notFromMainThread = false;
			var mre = new ManualResetEvent (false);
			int mainThreadId = Thread.CurrentThread.ManagedThreadId;
			TaskScheduler.UnobservedTaskException += (o, args) => {
				notFromMainThread = Thread.CurrentThread.ManagedThreadId != mainThreadId;
				args.SetObserved ();
				mre.Set ();
			};
			var inner = new ApplicationException ();
			CreateFaultedTaskCompletionSource (inner);
			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			Assert.IsTrue (mre.WaitOne (5000), "#1");
			Assert.IsTrue (notFromMainThread, "#2");
		}

		void CreateFaultedTaskCompletionSource (Exception inner)
		{
			var tcs = new TaskCompletionSource<int> ();
			tcs.SetException (inner);
			tcs = null;
		}

		[Test]
		public void WaitingTest ()
		{
			var tcs = new TaskCompletionSource<int> ();
			var task = tcs.Task;
			bool result = task.Wait (50);

			Assert.IsFalse (result);
		}
	}
}
