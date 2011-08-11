// 
// ActionBlockTest.cs
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
	public class ActionBlockTest
	{
		[Test]
		public void BasicUsageTest ()
		{
			bool[] array = new bool[3];
			ActionBlock<int> block = new ActionBlock<int> ((i) => array[i] = true);

			for (int i = 0; i < array.Length; ++i)
				Assert.IsTrue (block.Post (i), "Not accepted");

			// TODO: use a more sensible approach here based on Completion
			Thread.Sleep (1300);
			
			Assert.IsTrue (array.All (b => b), "Some false");
		}

		[Test]
		public void CompleteTest ()
		{
			ActionBlock<int> block = new ActionBlock<int> ((i) => Thread.Sleep (100));

			for (int i = 0; i < 10; i++)
				Assert.IsTrue (block.Post (i), "Not Accepted");

			block.Complete ();
			// Still element to be processed so Completion should be false
			Assert.IsFalse (block.Completion.IsCompleted);
			block.Completion.Wait ();
			Assert.IsTrue (block.Completion.IsCompleted);
		}
	}
}
