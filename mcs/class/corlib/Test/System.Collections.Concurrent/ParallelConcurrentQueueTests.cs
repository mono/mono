// ParallelConcurrentQueueTests.cs
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
using System.Collections.Concurrent;

using MonoTests.System.Threading.Tasks;

using NUnit.Framework;

namespace MonoTests.System.Collections.Concurrent
{
	[TestFixture()]
	public class ParallelConcurrentQueueTests
	{
		ConcurrentQueue<int> queue;
		
		[SetUpAttribute]
		public void Setup()
		{
			queue = new ConcurrentQueue<int>();
		}
		
		[Test]
		[Category ("MultiThreaded")]
		public void CountTestCase()
		{
			const int numThread = 5;
			ParallelTestHelper.ParallelAdder(queue, numThread);
			Assert.AreEqual(10 * numThread, queue.Count, "#1");
			int value;
			queue.TryPeek(out value);
			ParallelTestHelper.ParallelRemover(queue, numThread, 3);
			Assert.AreEqual(10 * numThread - 3, queue.Count, "#2");
		}
	}
}
