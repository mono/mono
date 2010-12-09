#if NET_4_0
// CountdownEventTests.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

using System;
using System.Threading;

using NUnit.Framework;

using MonoTests.System.Threading.Tasks;

namespace MonoTests.System.Threading
{
	[TestFixtureAttribute]
	public class CountdownEventTests
	{
		CountdownEvent evt;
		
		[SetUpAttribute]
		public void Setup()
		{
			evt = new CountdownEvent(5);
		}
		
		[Test]
		public void InitialTestCase()
		{
			Assert.AreEqual(5, evt.InitialCount, "#1");
			evt.AddCount();
			evt.Signal(3);
			Assert.AreEqual(5, evt.InitialCount, "#2");
		}
		
		[Test]
		public void CurrentCountTestCase()
		{
			Assert.AreEqual(5, evt.CurrentCount, "#1");
			
			evt.AddCount();
			Assert.AreEqual(6, evt.CurrentCount, "#2");
			
			evt.TryAddCount(2);
			Assert.AreEqual(8, evt.CurrentCount, "#3");
			
			evt.Signal(4);
			Assert.AreEqual(4, evt.CurrentCount, "#4");
			
			evt.Reset();
			Assert.AreEqual(5, evt.CurrentCount, "#5");
		}
		
		[Test]
		public void IsSetTestCase()
		{
			Assert.IsFalse(evt.IsSet, "#1");
			
			evt.Signal(5);
			Assert.IsTrue(evt.IsSet, "#2");
			
			evt.Reset();
			Assert.IsFalse(evt.IsSet, "#3");
		}
		
		[Test]
		public void TryAddCountTestCase()
		{
			Assert.IsTrue(evt.TryAddCount(2), "#1");
			evt.Signal(7);
			Assert.IsFalse(evt.TryAddCount(), "#2");
		}
		
		[Test]
		public void WaitTestCase()
		{
			int count = 0;
			bool s = false;
			
			ParallelTestHelper.ParallelStressTest(evt, delegate (CountdownEvent e) {
				if (Interlocked.Increment(ref count) % 2 == 0) {
					Thread.Sleep(100);
					while(!e.IsSet)
						e.Signal();
				} else {
					e.Wait();
					s = true;
				}
			});
			
			Assert.IsTrue(s, "#1");
			Assert.IsTrue(evt.IsSet, "#2");
		}
		
		[Test]
		public void AddCountSignalStressTestCase()
		{
			int count = 0;
			ParallelTestHelper.ParallelStressTest(evt, delegate (CountdownEvent e) {
				int num = Interlocked.Increment(ref count);
				if (num % 2 == 0)
					e.AddCount();
				else
					e.Signal();
			}, 7);
			
			Assert.AreEqual(4, evt.CurrentCount, "#1");
			Assert.IsFalse(evt.IsSet, "#2");
		}

		[Test]
		public void ResetTest ()
		{
			Assert.AreEqual (5, evt.CurrentCount);
			evt.Signal ();
			Assert.AreEqual (4, evt.CurrentCount);
			evt.Reset ();
			Assert.AreEqual (5, evt.CurrentCount);
			Assert.AreEqual (5, evt.InitialCount);
			evt.Signal ();
			evt.Signal ();
			Assert.AreEqual (3, evt.CurrentCount);
			Assert.AreEqual (5, evt.InitialCount);
			evt.Reset (10);
			Assert.AreEqual (10, evt.CurrentCount);
			Assert.AreEqual (10, evt.InitialCount);
		}
	}
}
#endif
