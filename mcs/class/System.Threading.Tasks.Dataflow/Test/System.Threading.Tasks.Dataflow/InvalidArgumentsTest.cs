// InvalidArgumentsTest.cs
//  
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
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class InvalidArgumentsTest {
		[Test]
		public void FaultNullTest()
		{
			foreach (var block in Blocks.CreateBlocks ()) {
				AssertEx.Throws<ArgumentNullException> (() => block.Fault (null));
			}
		}

		[Test]
		public void  ActionBlockTest ()
		{
			AssertEx.Throws<ArgumentNullException> (
				() => new ActionBlock<int> ((Action<int>)null));
			AssertEx.Throws<ArgumentNullException> (
				() => new ActionBlock<int> (i => { }, null));
		}

		[Test]
		public void BatchBlockTest()
		{
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchBlock<int> (0));
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchBlock<int> (-1));

			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => new BatchBlock<int> (2,
					      new GroupingDataflowBlockOptions { BoundedCapacity = 1 }));

			AssertEx.Throws<ArgumentNullException> (() => new BatchBlock<int> (2, null));
		}

		[Test]
		public void BatchedJoinBlockTest()
		{
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchedJoinBlock<int, int> (0));
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchedJoinBlock<int, int> (-1));

			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int> (1,
					      new GroupingDataflowBlockOptions { BoundedCapacity = 1 }));
			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int> (1,
					      new GroupingDataflowBlockOptions { Greedy = false }));

			AssertEx.Throws<ArgumentNullException> (() => new BatchedJoinBlock<int, int> (2, null));
		}

		[Test]
		public void BatchedJoinBlock3Test()
		{
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchedJoinBlock<int, int, int> (0));
			AssertEx.Throws<ArgumentOutOfRangeException> (() => new BatchedJoinBlock<int, int, int> (-1));

			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int, int> (1,
					      new GroupingDataflowBlockOptions { BoundedCapacity = 1 }));
			AssertEx.Throws<ArgumentException> (
				() => new BatchedJoinBlock<int, int, int> (1,
					      new GroupingDataflowBlockOptions { Greedy = false }));

			AssertEx.Throws<ArgumentNullException> (() => new BatchedJoinBlock<int, int, int> (2, null));
		}

		[Test]
		public void BroadcastBlock()
		{
			// null is valid argument for BroadcastBlock, so this shouldn't throw
			new BroadcastBlock<int> (null);
			AssertEx.Throws<ArgumentNullException> (() => new BroadcastBlock<int> (i => i, null));
		}

		[Test]
		public void BufferBlockTest()
		{
			AssertEx.Throws<ArgumentNullException> (() => new BufferBlock<int> (null));
		}

		[Test]
		public void JoinBlockTest()
		{
			AssertEx.Throws<ArgumentNullException> (() => new JoinBlock<int, int> (null));
		}

		[Test]
		public void JoinBlock3Test()
		{
			AssertEx.Throws<ArgumentNullException> (() => new JoinBlock<int, int, int> (null));
		}

		[Test]
		public void TransformBlockTest()
		{
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformBlock<int, int> ((Func<int, int>)null));
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformBlock<int, int> ((Func<int, int>)null,
					      new ExecutionDataflowBlockOptions ()));
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformBlock<int, int> (i => i, null));
		}

		[Test]
		public void TransformManyBlockTest()
		{
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformManyBlock<int, int> ((Func<int, IEnumerable<int>>)null));
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformManyBlock<int, int> ((Func<int, IEnumerable<int>>)null,
					      new ExecutionDataflowBlockOptions ()));
			AssertEx.Throws<ArgumentNullException> (
				() => new TransformManyBlock<int, int> (i => new int[0], null));
		}

		[Test]
		public void WriteOnceBlock()
		{
			// null is valid argument for WriteOnceBlock, so this shouldn't throw
			new WriteOnceBlock<int> (null);
			AssertEx.Throws<ArgumentNullException> (() => new WriteOnceBlock<int> (i => i, null));
		}

		[Test]
		public void SourceBlockTest()
		{
			foreach (var block in Blocks.CreateSimpleSourceBlocks<int> ())
				SourceBlockTestInternal (block);
			SourceBlockTestInternal (new BatchBlock<int>(5));
			SourceBlockTestInternal (new BatchedJoinBlock<int, int>(5));
			SourceBlockTestInternal (new BatchedJoinBlock<int, int, int>(5));
			SourceBlockTestInternal (new JoinBlock<int, int> ());
			SourceBlockTestInternal (new JoinBlock<int, int, int> ());
		}

		static void SourceBlockTestInternal<T>(ISourceBlock<T> block)
		{
			var target = new BufferBlock<T> ();

			bool consumed;
			// invalid header
			AssertEx.Throws<ArgumentException> (
				() =>
				block.ConsumeMessage (new DataflowMessageHeader (), target, out consumed));

			// header that wasn't sent by the block doesn't throw
			block.ConsumeMessage (new DataflowMessageHeader (1), target, out consumed);

			AssertEx.Throws<ArgumentNullException> (
				() =>
				block.ConsumeMessage (new DataflowMessageHeader (1), null, out consumed));


			AssertEx.Throws<ArgumentException> (
				() =>
				block.ReserveMessage (new DataflowMessageHeader (), target));

			// header that wasn't sent by the block doesn't throw
			block.ReserveMessage (new DataflowMessageHeader (1), target);

			AssertEx.Throws<ArgumentNullException> (
				() =>
				block.ReserveMessage (new DataflowMessageHeader (1), null));

			AssertEx.Throws<ArgumentException> (
				() =>
				block.ReleaseReservation (new DataflowMessageHeader (), target));

			AssertEx.Throws<ArgumentNullException>(() => block.LinkTo(null, new DataflowLinkOptions()));

			AssertEx.Throws<ArgumentNullException>(() => block.LinkTo(target, null));
		}

		[Test]
		public void TargetBlockTest()
		{
			foreach (var block in Blocks.CreateTargetBlocks<int> ()) {
				// invalid header
				AssertEx.Throws<ArgumentException> (
					() => block.OfferMessage (new DataflowMessageHeader (), 42, null, false));

				// consumeToAccept with null source
				AssertEx.Throws<ArgumentException> (
					() => block.OfferMessage (new DataflowMessageHeader (1), 42, null, true));
			}
		}

		[Test]
		public void ReactiveTest()
		{
			IPropagatorBlock<int, int> block = null;

			AssertEx.Throws<ArgumentNullException> (() => block.AsObservable ());
			AssertEx.Throws<ArgumentNullException> (() => block.AsObserver ());
		}

		[Test]
		public void ChooseTest()
		{
			ISourceBlock<int> nullSource = null;
			var realSource = new BufferBlock<int> ();
			var options = new DataflowBlockOptions ();

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, null, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, nullSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, null, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, nullSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, null, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, i => { }, null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, null, realSource, i => { }, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, nullSource, i => { }, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, null, realSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }, nullSource, i => { }));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }, realSource, null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (nullSource, i => { }, realSource, i => { }, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, null, realSource, i => { }, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, nullSource, i => { }, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, null, realSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, i => { }, nullSource, i => { }, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, i => { }, realSource, null, options));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Choose (realSource, i => { }, realSource, i => { }, realSource, i => { }, null));
		}

		[Test]
		public void EncapsulateTest()
		{
			AssertEx.Throws<ArgumentNullException> (
				() =>
				DataflowBlock.Encapsulate ((ITargetBlock<int>)null, new BufferBlock<int> ()));
			AssertEx.Throws<ArgumentNullException> (
				() =>
				DataflowBlock.Encapsulate (new BufferBlock<int> (), (ISourceBlock<int>)null));
		}

		[Test]
		public void LinkToTest()
		{
			IPropagatorBlock<int, int> nullBlock = null;
			var realBlock = new BufferBlock<int> ();

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (nullBlock, realBlock));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, nullBlock));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (nullBlock, realBlock, i => true));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, nullBlock, i => true));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, realBlock, null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (nullBlock, realBlock, new DataflowLinkOptions (), i => true));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, nullBlock, new DataflowLinkOptions (), i => true));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, realBlock, null, i => true));
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.LinkTo (realBlock, realBlock, new DataflowLinkOptions (), null));
		}

		[Test]
		public void PostTest()
		{
			AssertEx.Throws<ArgumentNullException> (() => DataflowBlock.Post (null, 42));
		}

		[Test]
		public void ReceiveTest()
		{
			var source = new BufferBlock<int> ();
			source.Post(1);
			source.Post(2);
			source.Post(3);
			source.Post(4);

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Receive ((ISourceBlock<int>)null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Receive ((ISourceBlock<int>)null,
					new CancellationToken (false)));
			AssertEx.Throws<OperationCanceledException> (
				() => DataflowBlock.Receive (source, new CancellationToken (true)));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Receive ((ISourceBlock<int>)null,
					TimeSpan.FromMinutes (1)));
			// shouldn't throw
			DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (-1));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (-2)));
			// shouldn't throw
			DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (int.MaxValue));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.Receive (source,
					TimeSpan.FromMilliseconds (int.MaxValue + 1L)));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.Receive ((ISourceBlock<int>)null,
					TimeSpan.FromMinutes (1), new CancellationToken(false)));
			AssertEx.Throws<OperationCanceledException> (
				() => DataflowBlock.Receive (source, TimeSpan.FromMinutes (1),
					new CancellationToken (true)));
			// shouldn't throw
			DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (-1),
				new CancellationToken (false));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (-2),
					new CancellationToken (false)));
			// shouldn't throw
			DataflowBlock.Receive (source, TimeSpan.FromMilliseconds (int.MaxValue),
				new CancellationToken (false));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.Receive (source,
					TimeSpan.FromMilliseconds (int.MaxValue + 1L),
					new CancellationToken (false)));
		}

		[Test]
		public void ReceiveAsyncTest()
		{
			var source = new BufferBlock<int> ();

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.ReceiveAsync ((ISourceBlock<int>)null));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.ReceiveAsync ((ISourceBlock<int>)null,
					new CancellationToken (false)));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.ReceiveAsync ((ISourceBlock<int>)null,
					TimeSpan.FromMinutes (1)));
			// shouldn't throw
			DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (-1));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (-2)));
			// shouldn't throw
			DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (int.MaxValue));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.ReceiveAsync (source,
					TimeSpan.FromMilliseconds (int.MaxValue + 1L)));

			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.ReceiveAsync ((ISourceBlock<int>)null,
					TimeSpan.FromMinutes (1), new CancellationToken(false)));
			// shouldn't throw
			DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (-1),
				new CancellationToken (false));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (-2),
					new CancellationToken (false)));
			// shouldn't throw
			DataflowBlock.ReceiveAsync (source, TimeSpan.FromMilliseconds (int.MaxValue),
				new CancellationToken (false));
			AssertEx.Throws<ArgumentOutOfRangeException> (
				() => DataflowBlock.ReceiveAsync (source,
					TimeSpan.FromMilliseconds (int.MaxValue + 1L),
					new CancellationToken (false)));
		}

		[Test]
		public void TryReceiveTest()
		{
			int i;
			AssertEx.Throws<ArgumentNullException> (
				() => DataflowBlock.TryReceive (null, out i));
		}

		[Test]
		public void DataflowBlockOptionsTest()
		{
			var options = new DataflowBlockOptions ();

			AssertEx.Throws<ArgumentOutOfRangeException>(() => options.BoundedCapacity = -2);

			AssertEx.Throws<ArgumentOutOfRangeException>(() => options.MaxMessagesPerTask = -2);

			AssertEx.Throws<ArgumentNullException>(() => options.NameFormat = null);
			// shouldn't throw
			options.NameFormat = "{2}";
			new BufferBlock<int>(options).ToString();

			AssertEx.Throws<ArgumentNullException>(() => options.TaskScheduler = null);
		}

		[Test]
		public void ExecutionDataflowBlockOptionsTest()
		{
			var options = new ExecutionDataflowBlockOptions ();

			AssertEx.Throws<ArgumentOutOfRangeException>(() => options.MaxDegreeOfParallelism = -2);
		}

		[Test]
		public void GroupingDataflowBlockOptionsTest()
		{
			var options = new GroupingDataflowBlockOptions ();

			AssertEx.Throws<ArgumentOutOfRangeException>(() => options.MaxNumberOfGroups = -2);
		}

		[Test]
		public void DataflowLinkOptionsTest()
		{
			var options = new DataflowLinkOptions ();

			AssertEx.Throws<ArgumentOutOfRangeException>(() => options.MaxMessages = -2);
		}
	}
}