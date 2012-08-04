// 
// CompletionTest.cs
//  
// Author:
//       Jérémie "garuma" Laval <jeremie.laval@gmail.com>
//       Petr Onderka <gsvick@gmail.com>
// 
// Copyright (c) 2011 Jérémie "garuma" Laval
// Copyright (c) 2012 Petr Onderka
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
using System.Collections.Generic;
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
		public void WithElementsStillLingering ()
		{
			var block = new BufferBlock<int> ();
			Assert.IsTrue (block.Post (42));
			block.Complete ();

			Assert.IsFalse (block.Completion.Wait (100));
			Assert.IsFalse (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.WaitingForActivation, block.Completion.Status);

			Assert.AreEqual (42, block.Receive ());

			Assert.IsTrue (block.Completion.Wait (100));
			Assert.IsTrue (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.RanToCompletion, block.Completion.Status);
		}

		[Test]
		public void WithElementsStillLingeringButFaulted ()
		{
			var block = new BufferBlock<int> ();
			Assert.IsTrue (block.Post (42));
			((IDataflowBlock)block).Fault (new Exception ());

			AssertEx.Throws<AggregateException> (() => block.Completion.Wait (100));
			Assert.IsTrue (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.Faulted, block.Completion.Status);
			Assert.IsFalse (block.Post (43));
		}

		[Test]
		public void WithElementsStillLingeringButCancelled ()
		{
			var tokenSource = new CancellationTokenSource ();
			var block = new BufferBlock<int> (
				new DataflowBlockOptions { CancellationToken = tokenSource.Token });
			Assert.IsTrue (block.Post (42));
			tokenSource.Cancel ();

			var ae = AssertEx.Throws<AggregateException> (
				() => block.Completion.Wait (100));
			Assert.AreEqual (1, ae.InnerExceptions.Count);
			Assert.AreEqual (typeof(TaskCanceledException), ae.InnerException.GetType ());

			Assert.IsTrue (block.Completion.IsCompleted);
			Assert.AreEqual (TaskStatus.Canceled, block.Completion.Status);
			Assert.IsFalse (block.Post (43));
		}

		static IEnumerable<Tuple<IDataflowBlock, ITargetBlock<T>>>
			GetJoinBlocksWithTargets<T> ()
		{
			Func<IDataflowBlock, ITargetBlock<T>, Tuple<IDataflowBlock, ITargetBlock<T>>>
				createTuple = Tuple.Create;

			var joinBlock = new JoinBlock<T, T> ();
			yield return createTuple (joinBlock, joinBlock.Target1);
			var joinBlock3 = new JoinBlock<T, T, T> ();
			yield return createTuple (joinBlock3, joinBlock3.Target1);
			var batchedJoinBlock = new BatchedJoinBlock<T, T> (2);
			yield return createTuple (batchedJoinBlock, batchedJoinBlock.Target1);
			var batchedJoinBlock3 = new BatchedJoinBlock<T, T, T> (2);
			yield return createTuple (batchedJoinBlock3, batchedJoinBlock3.Target1);
		}

		[Test]
		public void JoinTargetCompletitionTest ()
		{
			foreach (var tuple in GetJoinBlocksWithTargets<int> ()) {
				AssertEx.Throws<NotSupportedException> (
					() => { var x = tuple.Item2.Completion; });
				Assert.IsTrue (tuple.Item2.Post (1));
				tuple.Item2.Complete ();
				Assert.IsFalse (tuple.Item2.Post (2));
			}

			foreach (var tuple in GetJoinBlocksWithTargets<int> ()) {
				Assert.IsTrue (tuple.Item2.Post (1));
				tuple.Item1.Complete ();
				Assert.IsFalse (tuple.Item2.Post (2));
			}
		}
	}
}