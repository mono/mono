#if NET_4_0
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
			completionSource = new TaskCompletionSource<int> (state, TaskCreationOptions.LongRunning);
		}
		
		[Test]
		public void CreationCheckTest ()
		{
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.AreEqual (TaskCreationOptions.LongRunning, completionSource.Task.CreationOptions, "#2");
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
		}
		
		[Test]
		public void SetExceptionTest ()
		{
			Exception e = new Exception ("foo");
			
			Assert.IsNotNull (completionSource.Task, "#1");
			Assert.IsTrue (completionSource.TrySetException (e), "#2");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#3");
			Assert.IsInstanceOfType (typeof (AggregateException), completionSource.Task.Exception, "#4.1");
			
			AggregateException aggr = (AggregateException)completionSource.Task.Exception;
			Assert.AreEqual (1, aggr.InnerExceptions.Count, "#4.2");
			Assert.AreEqual (e, aggr.InnerExceptions[0], "#4.3");
			
			Assert.IsFalse (completionSource.TrySetResult (42), "#5");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#6");
			Assert.IsFalse (completionSource.TrySetCanceled (), "#8");
			Assert.AreEqual (TaskStatus.Faulted, completionSource.Task.Status, "#9");
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
	}
}
#endif
