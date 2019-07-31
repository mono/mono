//
// SpinLockTests.cs
//
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//
// Copyright (c) 2010 Jérémie "Garuma" Laval
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

using NUnit.Framework;

using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class SpinLockTests
	{
		SpinLock sl;

		[SetUp]
		public void Setup ()
		{
			sl = new SpinLock (true);
		}

		[Test, ExpectedException (typeof (LockRecursionException))]
		public void RecursionExceptionTest ()
		{
			sl = new SpinLock (true);
			bool taken = false, taken2 = false;

			sl.Enter (ref taken);
			Assert.IsTrue (taken, "#1");
			sl.Enter (ref taken2);
		}

		[Test]
		public void SimpleEnterExitSchemeTest ()
		{
			bool taken = false;

			for (int i = 0; i < 50000; i++) {
				sl.Enter (ref taken);
				Assert.IsTrue (taken, "#" + i.ToString ());
				sl.Exit ();
				taken = false;
			}
		}

		[Test]
		public void SemanticCorrectnessTest ()
		{
			sl = new SpinLock (false);

			bool taken = false;
			bool taken2 = false;

			sl.Enter (ref taken);
			Assert.IsTrue (taken, "#1");
			sl.TryEnter (ref taken2);
			Assert.IsFalse (taken2, "#2");
			sl.Exit ();

			sl.TryEnter (ref taken2);
			Assert.IsTrue (taken2, "#3");

			sl.Exit ();
			Assert.IsFalse (sl.IsHeld);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void FirstTakenParameterTest ()
		{
			bool taken = true;

			sl.Enter (ref taken);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void SecondTakenParameterTest ()
		{
			bool taken = true;

			sl.TryEnter (ref taken);
		}

		internal class SpinLockWrapper
		{
			public SpinLock Lock = new SpinLock (false);
		}

		[Test]
		[Category ("MultiThreaded")]
		public void LockUnicityTest ()
		{
			ParallelTestHelper.Repeat (delegate {
				int currentCount = 0;
				bool fail = false;
				SpinLockWrapper wrapper = new SpinLockWrapper ();

				ParallelTestHelper.ParallelStressTest (wrapper, delegate {
					bool taken = false;
					wrapper.Lock.Enter (ref taken);
					int current = currentCount++;
					if (current != 0)
						fail = true;

					SpinWait sw = new SpinWait ();
					for (int i = 0; i < 200; i++)
						sw.SpinOnce ();
					currentCount -= 1;

					wrapper.Lock.Exit ();
				}, 4);

				Assert.IsFalse (fail);
			}, 5);
		}

		[Test]
		public void IsHeldByCurrentThreadTest ()
		{
			bool lockTaken = false;

			sl.Enter (ref lockTaken);
			Assert.IsTrue (lockTaken, "#1");
			Assert.IsTrue (sl.IsHeldByCurrentThread, "#2");

			lockTaken = false;
			sl = new SpinLock (true);

			sl.Enter (ref lockTaken);
			Assert.IsTrue (lockTaken, "#3");
			Assert.IsTrue (sl.IsHeldByCurrentThread, "#4");
		}
	}
}
