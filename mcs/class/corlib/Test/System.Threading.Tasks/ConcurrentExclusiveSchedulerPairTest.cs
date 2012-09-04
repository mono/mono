//
// ConcurrentExclusiveSchedulerPairTest.cs
//
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2011 Jérémie "garuma" Laval
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

#if NET_4_5

using System;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks
{
	[TestFixture]
	[Ignore ("Not implemented yet")]
	public class ConcurrentExclusiveSchedulerPairTest
	{
		ConcurrentExclusiveSchedulerPair schedPair;
		TaskFactory factory;

		[Test]
		public void BasicExclusiveUsageTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			factory = new TaskFactory (schedPair.ExclusiveScheduler);

			bool launched = false;
			factory.StartNew (() => launched = true);
			Thread.Sleep (600);

			Assert.IsTrue (launched);
		}

		[Test]
		public void BasicConcurrentUsageTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			factory = new TaskFactory (schedPair.ConcurrentScheduler);

			bool launched = false;
			factory.StartNew (() => launched = true);
			Thread.Sleep (600);

			Assert.IsTrue (launched);
		}

		[Test]
		public void ExclusiveUsageTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			factory = new TaskFactory (schedPair.ExclusiveScheduler);

			int count = 0;
			ManualResetEventSlim mreFinish = new ManualResetEventSlim (false);
			ManualResetEventSlim mreStart = new ManualResetEventSlim (false);

			factory.StartNew (() => {
					mreStart.Set ();
					Interlocked.Increment (ref count);
					mreFinish.Wait ();
				});
			mreStart.Wait ();
			factory.StartNew (() => Interlocked.Increment (ref count));
			Thread.Sleep (100);

			Assert.AreEqual (1, count);
			mreFinish.Set ();
		}

		[Test]
		public void ConcurrentUsageTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			factory = new TaskFactory (schedPair.ConcurrentScheduler);

			int count = 0;
			ManualResetEventSlim mreFinish = new ManualResetEventSlim (false);
			CountdownEvent cntd = new CountdownEvent (2);

			factory.StartNew (() => {
					Interlocked.Increment (ref count);
					cntd.Signal ();
					mreFinish.Wait ();
				});
			factory.StartNew (() => {
					Interlocked.Increment (ref count);
					cntd.Signal ();
					mreFinish.Wait ();
				});

			cntd.Wait ();
			Assert.AreEqual (2, count);
			mreFinish.Set ();
		}

		[Test]
		public void ConcurrentUsageWithExclusiveExecutingTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			TaskFactory exclFact = new TaskFactory (schedPair.ExclusiveScheduler);
			TaskFactory concFact = new TaskFactory (schedPair.ConcurrentScheduler);

			int count = 0;
			bool exclStarted = false;
			ManualResetEventSlim mreStart = new ManualResetEventSlim (false);
			ManualResetEventSlim mreFinish = new ManualResetEventSlim (false);

			exclFact.StartNew (() => {
				exclStarted = true;
				mreStart.Set ();
				mreFinish.Wait ();
				exclStarted = false;
			});

			mreStart.Wait ();

			concFact.StartNew (() => Interlocked.Increment (ref count));
			concFact.StartNew (() => Interlocked.Increment (ref count));
			Thread.Sleep (100);

			Assert.IsTrue (exclStarted);
			Assert.AreEqual (0, count);
			mreFinish.Set ();
		}

		[Test]
		public void ExclusiveUsageWithConcurrentExecutingTest ()
		{
			schedPair = new ConcurrentExclusiveSchedulerPair (TaskScheduler.Default, 4);
			TaskFactory exclFact = new TaskFactory (schedPair.ExclusiveScheduler);
			TaskFactory concFact = new TaskFactory (schedPair.ConcurrentScheduler);

			int count = 0;
			bool started = false;
			ManualResetEventSlim mreStart = new ManualResetEventSlim (false);
			ManualResetEventSlim mreFinish = new ManualResetEventSlim (false);

			concFact.StartNew (() => {
				started = true;
				mreStart.Set ();
				mreFinish.Wait ();
				started = false;
			});

			mreStart.Wait ();

			exclFact.StartNew (() => Interlocked.Increment (ref count));
			Thread.Sleep (100);

			Assert.IsTrue (started);
			Assert.AreEqual (0, count);
			mreFinish.Set ();
		}
	}
}

#endif