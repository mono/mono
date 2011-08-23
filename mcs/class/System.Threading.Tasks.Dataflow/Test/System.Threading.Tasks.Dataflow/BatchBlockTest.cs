// 
// BatchBlockTest.cs
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
	public class BatchBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i => { array = i; evt.Set (); });
			buffer.LinkTo<int[]>(block);
			
			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			evt.Wait (1600);

			Assert.IsNull (array);

			Assert.IsTrue (buffer.Post (42));
			evt.Wait ();

			Assert.IsNotNull (array);
			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 42 }, array);
		}

		[Test]
		public void TriggerBatchTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i => { array = i; evt.Set (); });
			buffer.LinkTo(block);
			
			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			buffer.TriggerBatch ();
			evt.Wait ();

			Assert.IsNotNull (array);
			Assert.IsTrue (buffer.Post (42));
			evt.Wait (1600);

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, array);
		}

		[Test]
		public void TriggerBatchLateBinding ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (10);
			var block = new ActionBlock<int[]> (i => { array = i; evt.Set (); });
			
			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));

			buffer.TriggerBatch ();
			buffer.LinkTo (block);

			evt.Wait ();
			Assert.IsNotNull (array);

			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, array);
		}

		[Test]
		public void LateTriggerBatchKeepCountTest ()
		{
			int[] array = null;
			var evt = new ManualResetEventSlim (false);

			var buffer = new BatchBlock<int> (15);
			var block = new ActionBlock<int[]> (i => { array = i; evt.Set (); });
			
			for (int i = 0; i < 9; i++)
				Assert.IsTrue (buffer.Post (i));
			buffer.TriggerBatch ();
			Assert.IsTrue (buffer.Post (42));
			buffer.LinkTo (block);

			evt.Wait ();

			Assert.IsNotNull (array);
			CollectionAssert.AreEquivalent (new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, array);			
		}
	}
}
