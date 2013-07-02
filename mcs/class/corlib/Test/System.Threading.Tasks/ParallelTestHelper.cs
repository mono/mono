#if NET_4_0
// TestHelper.cs
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
using System.Collections.Concurrent;

namespace MonoTests.System.Threading.Tasks
{
	static class ParallelTestHelper
	{
#if MOBILE
		const int NumRun = 5;
#else
		const int NumRun = 500;
#endif
		
		public static void Repeat (Action action)
		{
			Repeat (action, NumRun);
		}
		
		public static void Repeat (Action action, int numRun)
		{
			for (int i = 0; i < numRun; i++) {
				action ();
			}
		}
		
		public static void ParallelStressTest<TSource>(TSource obj, Action<TSource> action)
		{
			ParallelStressTest(obj, action, Environment.ProcessorCount + 2);
		}
		
		public static void ParallelStressTest<TSource>(TSource obj, Action<TSource> action, int numThread)
		{
			Thread[] threads = new Thread[numThread];
			for (int i = 0; i < numThread; i++) {
				threads[i] = new Thread(new ThreadStart(delegate { action(obj); }));
				threads[i].Start();
			}
			
			// Wait for the completion
			for (int i = 0; i < numThread; i++)
				threads[i].Join();
		}
		
		public static void ParallelAdder(IProducerConsumerCollection<int> collection, int numThread)
		{
			int startIndex = -10;
			ParallelTestHelper.ParallelStressTest(collection, delegate (IProducerConsumerCollection<int> c) {
				int start = Interlocked.Add(ref startIndex, 10);
				for (int i = start; i < start + 10; i++) {
					c.TryAdd(i);
				}
			}, numThread);
		}
		
		public static void ParallelRemover(IProducerConsumerCollection<int> collection, int numThread, int times)
		{
			int t = -1;
			ParallelTestHelper.ParallelStressTest(collection, delegate (IProducerConsumerCollection<int> c) {
				int num = Interlocked.Increment(ref t);
				int value;
				if (num < times)
					c.TryTake (out value);
			}, numThread);
		}
	}
}
#endif
