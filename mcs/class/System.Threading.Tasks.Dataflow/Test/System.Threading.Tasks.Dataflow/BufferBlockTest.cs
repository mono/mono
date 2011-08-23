// 
// BufferBlockTest.cs
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow
{
	[TestFixture]
	public class BufferBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			int data = -1;
			var evt = new ManualResetEventSlim (false);
			BufferBlock<int> buffer = new BufferBlock<int> ();
			ActionBlock<int> action = new ActionBlock<int> ((i) => { data = i; evt.Set (); });
			buffer.LinkTo (action);

			Assert.IsTrue (buffer.Post (42));
			evt.Wait ();
			Assert.AreEqual (42, data);	
		}

		[Test]
		public void LateBindingTest ()
		{
			BufferBlock<int> buffer = new BufferBlock<int> ();
			var evt = new CountdownEvent (10);

			for (int i = 0; i < 10; i++)
				Assert.IsTrue (buffer.Post (i));
				
			ActionBlock<int> block = new ActionBlock<int> ((i) => evt.Signal ());
			buffer.LinkTo (block);
			
			evt.Wait ();
		}

		[Test]
		public void MultipleBindingTest ()
		{
			BufferBlock<int> buffer = new BufferBlock<int> ();
			var evt = new CountdownEvent (10);

			int count = 0;
				
			ActionBlock<int> block = new ActionBlock<int> ((i) => { Interlocked.Decrement (ref count); evt.Signal (); });
			IDisposable bridge = buffer.LinkTo (block);
			for (int i = 0; i < 10; i++)
				Assert.IsTrue (buffer.Post (i));
			evt.Wait ();

			Assert.AreEqual (-10, count);
			count = 0;
			evt.Reset ();
			bridge.Dispose ();

			ActionBlock<int> block2 = new ActionBlock<int> ((i) => { Interlocked.Increment (ref count); evt.Signal (); });
			buffer.LinkTo (block2);
			for (int i = 0; i < 10; i++)
				Assert.IsTrue (buffer.Post (i));
			evt.Wait ();

			Assert.AreEqual (10, count);
		}
	}
}
