// 
// CompletionTest.cs
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
	public class CompletionTest
	{
		[Test]
		public void WithNoElements ()
		{
			var block = new BufferBlock<int> ();
			//var block2 = new BufferBlock<int> ();
			//block.LinkTo (block2);
			block.Post (42);
			Thread.Sleep (600);
			//((IDataflowBlock)block2).Fault (new Exception ());
			block.Complete ();
			Thread.Sleep (600);
			Console.WriteLine (block.Completion.IsCompleted);
			Console.WriteLine (block.Completion.Status);
			block.Receive ();
			Console.WriteLine (block.Completion.IsCompleted);
			Console.WriteLine (block.Completion.Status);
		}

		[Test]
		public void WithElementsStillLingering ()
		{
			var block = new BufferBlock<int> ();
			//var block2 = new BufferBlock<int> ();
			//block.LinkTo (block2);
			block.Post (42);
			Thread.Sleep (600);
			//((IDataflowBlock)block2).Fault (new Exception ());
			block.Complete ();
			Thread.Sleep (600);
			Console.WriteLine (block.Completion.IsCompleted);
			Console.WriteLine (block.Completion.Status);
			block.Receive ();
			Console.WriteLine (block.Completion.IsCompleted);
			Console.WriteLine (block.Completion.Status);
		}

		[Test]
		public void EmptyAfterReceive ()
		{
			var block = new BufferBlock<int> ();
			block.Post (42);
			Thread.Sleep (600);
			block.Complete ();
			Thread.Sleep (600);
			Assert.IsFalse (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.WaitingForActivation, block.Completion.Status);
			block.Receive ();
			Assert.IsTrue (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.RanToCompletion, block.Completion.Status);
		}

		[Test]
		public void WithElementsStillLingeringButFaulted ()
		{
			var block = new BufferBlock<int> ();
			block.Post (42);
			Thread.Sleep (600);
			((IDataflowBlock)block).Fault (new Exception ());
			Thread.Sleep (600);
			Assert.IsTrue (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.Faulted, block.Completion.Status);
		}
	}
}
