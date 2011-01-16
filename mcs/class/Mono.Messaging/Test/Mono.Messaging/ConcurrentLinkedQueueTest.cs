//
// Mono.Messaging
//
// Authors:
//		Michael Barker (mike@middlesoft.co.uk)
//
// (C) 2008 Michael Barker
//

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

using Mono.Messaging;
using NUnit.Framework;

namespace MonoTests.Mono.Messaging {
	
	[TestFixture]
	public class ConcurrentLinkedQueueTest
	{
		
		[Test]
		public void ShouldEnqueueAndDequeue ()
		{
			ConcurrentLinkedQueue<int> pool = new ConcurrentLinkedQueue<int>();
			
			pool.Enqueue (1);
			
			Assert.AreEqual (1, pool.Dequeue ());			
		}

		
		[Test]
		public void ShouldEnqueueAndDequeueMultiple ()
		{
			ConcurrentLinkedQueue<int> pool = new ConcurrentLinkedQueue<int>();
			
			for (int i = 1; i <= 10; i++) {
				pool.Enqueue (i);
			}
			
			for (int i = 1; i <= 10; i++) {
				Assert.AreEqual (i, pool.Dequeue ());
			}
		}
	}
}
