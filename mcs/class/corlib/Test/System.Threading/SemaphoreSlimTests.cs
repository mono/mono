// SemaphoreSlimTests.cs
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
using System.Linq;
using System.Threading;

using MonoTests.System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Threading
{
	[TestFixture]
	public class SemaphoreSlimTests
	{
		SemaphoreSlim sem;
		
		[SetUp]
		public void Setup()
		{
			sem = new SemaphoreSlim(5);			
		}	
		
		[Test]
		public void CurrentCountMaxTestCase()
		{
			using (var semMax = new SemaphoreSlim(5, 5)) {
				semMax.Wait();
				try {
					semMax.Release(3);
					Assert.Fail ();
				} catch (SemaphoreFullException) {}
			}
		}
		
		[Test]
		public void CurrentCountTestCase()
		{
			sem.Wait();
			sem.Wait();
			sem.Release();
			Assert.AreEqual(4, sem.CurrentCount);
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void WaitStressTest()
		{
			int count = -1;
			bool[] array = new bool[7];
			int worker = 0;
			bool coherent = true;

			ParallelTestHelper.ParallelStressTest (sem, delegate (SemaphoreSlim s) {
				int index = Interlocked.Increment (ref count);
				s.Wait ();
				if (Interlocked.Increment (ref worker) > 5)
					coherent = false;
				Thread.Sleep (40);
				Interlocked.Decrement (ref worker);
				s.Release ();
				array[index] = true;
			}, 7);
			
			bool result = array.Aggregate ((acc, e) => acc && e);
			
			Assert.IsTrue (result, "#1");
			Assert.AreEqual (5, sem.CurrentCount, "#2");
			Assert.IsTrue (coherent, "#3");
		}
	}
}
