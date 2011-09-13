//
// TaskFactory_T_Test.cs
//
// Author:
//       Marek Safar <marek.safargmail.com>
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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
//
//

#if NET_4_0 || MOBILE

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	public class TaskFactory_T_Tests
	{
		class CompletedAsyncResult : IAsyncResult
		{
			public object AsyncState
			{
				get { throw new NotImplementedException (); }
			}

			public WaitHandle AsyncWaitHandle
			{
				get { throw new NotImplementedException (); }
			}

			public bool CompletedSynchronously
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsCompleted
			{
				get { return true; }
			}
		}

		class TestAsyncResult : IAsyncResult
		{
			WaitHandle wh = new ManualResetEvent (true);

			public object AsyncState
			{
				get { throw new NotImplementedException (); }
			}

			public WaitHandle AsyncWaitHandle
			{
				get
				{
					return wh;
				}
			}

			public bool CompletedSynchronously
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsCompleted
			{
				get { return false; }
			}
		}

		[SetUp]
		public void Setup ()
		{
		}
		
		[Test]
		public void ConstructorTest ()
		{
			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.LongRunning);
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.OnlyOnRanToCompletion);
				Assert.Fail ("#2");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				new TaskFactory<int> (TaskCreationOptions.None, TaskContinuationOptions.NotOnRanToCompletion);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}
		}

		[Test]
		public void NoDefaultScheduler ()
		{
			var tf = new TaskFactory<object> ();
			Assert.IsNull (tf.Scheduler, "#1");
		}

		[Test]
		public void FromAsync_ArgumentsCheck ()
		{
			var factory = new TaskFactory<object> ();

			var result = new CompletedAsyncResult ();
			try {
				factory.FromAsync (null, l => 1);
				Assert.Fail ("#1");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (result, null);
				Assert.Fail ("#2");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (result, l => 1, TaskCreationOptions.LongRunning);
				Assert.Fail ("#3");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				factory.FromAsync (result, l => 1, TaskCreationOptions.PreferFairness);
				Assert.Fail ("#4");
			} catch (ArgumentOutOfRangeException) {
			}

			try {
				factory.FromAsync (result, l => 1, TaskCreationOptions.None, null);
				Assert.Fail ("#5");
			} catch (ArgumentNullException) {
			}

			try {
				factory.FromAsync (null, l => 1, null, TaskCreationOptions.None);
				Assert.Fail ("#6");
			} catch (ArgumentNullException) {
			}
		}

		[Test]
		public void FromAsync_SimpleAsyncResult ()
		{
			var result = new TestAsyncResult ();

			var factory = new TaskFactory<int> ();
			var task = factory.FromAsync (result, l => 5);

			Assert.IsTrue (task.Wait (1000), "#1");
			Assert.AreEqual (5, task.Result, "#2");
		}
	}
}

#endif