//
// MonoTests.System.Web.TaskAsyncResultTest.cs
//
// Author:
//   Kornel Pal (kornelpal@gmail.com)
//
// Copyright (C) 2014 Kornel Pal
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


using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MonoTests.System.Web
{
	public abstract class TaskAsyncResultTest
	{
		sealed class TestException : Exception
		{
			public TestException ()
				: base ("Test exception")
			{
			}
		}

		sealed class DummyAsyncResult : IAsyncResult
		{
			public object AsyncState {
				get { throw new AssertionException ("Should not be called."); }
			}

			public WaitHandle AsyncWaitHandle {
				get { throw new AssertionException ("Should not be called."); }
			}

			public bool CompletedSynchronously {
				get { throw new AssertionException ("Should not be called."); }
			}

			public bool IsCompleted {
				get { throw new AssertionException ("Should not be called."); }
			}
		}

		int testThreadId;
		int factoryCount;
		int callbackCount;
		object expectedState;
		Exception expectedException;
		TaskCompletionSource<object> taskCompletion;
		TaskCompletionSource<object> callbackCompletion;
		IAsyncResult taskAsyncResult;

		static Task NullTaskFatory ()
		{
			return null;
		}

		static Task CompletedTaskFatory ()
		{
			return Task.FromResult<object> (null);
		}

		static Task FailingTaskFatory ()
		{
			throw new TestException ();
		}

		void DummyCallback (IAsyncResult result)
		{
			Interlocked.Increment (ref callbackCount);

			Assert.Fail ("Should not be called.");
		}

		void FailingCallback (IAsyncResult result)
		{
			Interlocked.Increment (ref callbackCount);

			throw new TestException ();
		}

		protected abstract void SetNullArguments ();
		protected abstract IAsyncResult GetAsyncResult (Func<Task> taskFactory, AsyncCallback callback, object state);
		protected abstract void Wait (IAsyncResult result);

		[SetUp]
		protected virtual void TestSetUp ()
		{
			testThreadId = Thread.CurrentThread.ManagedThreadId;
			factoryCount = 0;
			callbackCount = 0;
			expectedState = new object ();
			expectedException = null;
			taskCompletion = new TaskCompletionSource<object> ();
			callbackCompletion = new TaskCompletionSource<object> ();
			taskAsyncResult = null;
		}

		[Test]
		public void Invoke_NullArguments ()
		{
			SetNullArguments ();

			IAsyncResult result = GetAsyncResult (CompletedTaskFatory, null, null);
			Wait (result);
		}

		[Test]
		public void Invoke_NullTask ()
		{
			IAsyncResult result = GetAsyncResult (NullTaskFatory, DummyCallback, null);

			Assert.AreEqual (0, callbackCount, "#A01");
			Assert.IsNull (result, "#A02");
		}

		[Test]
		[ExpectedException (typeof (TestException))]
		public void Invoke_TaskFatoryException ()
		{
			try {
				GetAsyncResult (FailingTaskFatory, DummyCallback, expectedState);
			} finally {
				Assert.AreEqual (0, callbackCount, "#A01");
			}
		}

		[Test]
		[ExpectedException (typeof (TestException))]
		public void Invoke_CallbackException ()
		{
			try {
				GetAsyncResult (CompletedTaskFatory, FailingCallback, expectedState);
			} finally {
				Assert.AreEqual (1, callbackCount, "#A01");
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Invoke_NullResult ()
		{
			GetAsyncResult (NullTaskFatory, DummyCallback, null);
			Wait (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Invoke_InvalidResult ()
		{
			GetAsyncResult (NullTaskFatory, DummyCallback, null);
			Wait (new DummyAsyncResult ());
		}

		void SetTaskResult ()
		{
			if (expectedException == null)
				taskCompletion.SetResult (null);
			else
				taskCompletion.SetException (expectedException);
		}

		void WaitTaskResult ()
		{
			if (expectedException == null) {
				Wait (taskAsyncResult);
				return;
			}

			try {
				Wait (taskAsyncResult);

				Assert.Fail ("Expected exception was not thrown.");
			} catch (AssertionException) {
				throw;
			} catch (Exception ex) {
				Assert.AreSame (expectedException, ex, "WaitTaskResult#A01");
			}
		}

		[Test]
		public void InvokeSync ()
		{
			InvokeSyncCore ();
		}

		[Test]
		public void InvokeSync_Failed ()
		{
			expectedException = new TestException ();

			InvokeSyncCore ();
		}

		void InvokeSyncCore ()
		{
			IAsyncResult result = GetAsyncResult (SyncTaskFatory, SyncCallback, expectedState);

			Assert.IsNotNull (result, "InvokeSyncCore#A01");
			Assert.AreSame (taskAsyncResult, result, "InvokeSyncCore#A02");

			WaitTaskResult ();

			Assert.AreEqual (1, factoryCount, "InvokeSyncCore#A03");
			Assert.AreEqual (1, callbackCount, "InvokeSyncCore#A04");
		}

		Task SyncTaskFatory ()
		{
			Interlocked.Increment (ref factoryCount);

			Assert.AreEqual (testThreadId, Thread.CurrentThread.ManagedThreadId, "SyncTaskFatory#A01");

			SetTaskResult ();

			return taskCompletion.Task;
		}

		void SyncCallback (IAsyncResult result)
		{
			Interlocked.Increment (ref callbackCount);

			Assert.AreEqual (testThreadId, Thread.CurrentThread.ManagedThreadId, "SyncCallback#A01");

			Assert.IsNotNull (result, "SyncCallback#A02");
			Assert.AreSame (expectedState, result.AsyncState, "SyncCallback#A03");
			Assert.IsTrue (result.IsCompleted, "SyncCallback#A04");
			Assert.IsTrue (result.CompletedSynchronously, "SyncCallback#A05");
			Assert.IsNotNull (result.AsyncWaitHandle, "SyncCallback#A06");
			Assert.IsTrue (result.AsyncWaitHandle.WaitOne (0), "SyncCallback#A07");

			taskAsyncResult = result;

			Assert.AreEqual (1, factoryCount, "SyncCallback#A08");
			Assert.AreEqual (1, callbackCount, "SyncCallback#A09");
		}

		[Test]
		public void InvokeAsync ()
		{
			InvokeAsyncCore ();
		}

		[Test]
		public void InvokeAsync_Failed ()
		{
			expectedException = new TestException ();

			InvokeAsyncCore ();
		}

		void InvokeAsyncCore ()
		{
			IAsyncResult result = GetAsyncResult (AsyncTaskFatory, AsyncCallback, expectedState);

			Assert.IsNotNull (result, "InvokeAsyncCore#A01");
			Assert.AreSame (expectedState, result.AsyncState, "InvokeAsyncCore#A02");
			Assert.IsFalse (result.IsCompleted, "InvokeAsyncCore#A03");
			Assert.IsFalse (result.CompletedSynchronously, "InvokeAsyncCore#A04");
			Assert.IsNotNull (result.AsyncWaitHandle, "InvokeAsyncCore#A05");
			Assert.IsFalse (result.AsyncWaitHandle.WaitOne (0), "InvokeAsyncCore#A06");

			Assert.AreEqual (1, factoryCount, "InvokeAsyncCore#A07");
			Assert.AreEqual (0, callbackCount, "InvokeAsyncCore#A08");

			taskAsyncResult = result;

			SetTaskResult ();

			callbackCompletion.Task.GetAwaiter ().GetResult ();

			Assert.AreEqual (1, factoryCount, "InvokeAsyncCore#A09");
			Assert.AreEqual (1, callbackCount, "InvokeAsyncCore#A10");
		}

		Task AsyncTaskFatory ()
		{
			Interlocked.Increment (ref factoryCount);

			Assert.AreEqual (testThreadId, Thread.CurrentThread.ManagedThreadId, "AsyncTaskFatory#A01");

			return taskCompletion.Task;
		}

		void AsyncCallback (IAsyncResult result)
		{
			try {
				Interlocked.Increment (ref callbackCount);

				Assert.AreNotEqual (testThreadId, Thread.CurrentThread.ManagedThreadId, "AsyncCallback#A01");

				Assert.IsNotNull (result, "AsyncCallback#A02");
				Assert.AreSame (expectedState, result.AsyncState, "AsyncCallback#A03");
				Assert.IsTrue (result.IsCompleted, "AsyncCallback#A04");
				Assert.IsFalse (result.CompletedSynchronously, "AsyncCallback#A05");
				Assert.IsNotNull (result.AsyncWaitHandle, "AsyncCallback#A06");
				Assert.IsTrue (result.AsyncWaitHandle.WaitOne (0), "AsyncCallback#A07");

				Assert.AreSame (taskAsyncResult, result, "AsyncCallback#A08");

				WaitTaskResult ();

				callbackCompletion.TrySetResult (null);
			} catch (Exception ex) {
				callbackCompletion.TrySetException (ex);
			}
		}
	}
}

