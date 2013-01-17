// BoundedCapacityTest.cs
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
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace MonoTests.System.Threading.Tasks.Dataflow {
	[TestFixture]
	public class BoundedCapacityTest {
		[Test]
		public void PostTest ()
		{
			var block =
				new BufferBlock<int> (new DataflowBlockOptions { BoundedCapacity = 1 });
			Assert.IsTrue (block.Post (1));
			Assert.IsFalse (block.Post (2));
			Assert.AreEqual (1, block.Receive ());
			Assert.IsTrue (block.Post (3));
			Assert.AreEqual (3, block.Receive ());
		}

		[Test]
		public void OfferMessageTest ()
		{
			var block =
				new BufferBlock<int> (new DataflowBlockOptions { BoundedCapacity = 1 });
			ITargetBlock<int> target = block;

			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (new DataflowMessageHeader (1), 42, null, false));
			Assert.AreEqual (DataflowMessageStatus.Declined,
				target.OfferMessage (new DataflowMessageHeader (2), 43, null, false));

			Assert.AreEqual (42, block.Receive ());

			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (new DataflowMessageHeader (3), 44, null, false));

			Assert.AreEqual (44, block.Receive ());
		}

		[Test]
		public void OfferMessageWithSourceTest ()
		{
			var block =
				new BufferBlock<int> (new DataflowBlockOptions { BoundedCapacity = 1 });
			ITargetBlock<int> target = block;
			var source = new TestSourceBlock<int> ();

			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (new DataflowMessageHeader (1), 42, source, false));
			var header = new DataflowMessageHeader (2);
			source.AddMessage (header, 43);
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header, 43, source, false));

			Assert.AreEqual (42, block.Receive (TimeSpan.FromMilliseconds (100)));

			Assert.IsFalse (block.Completion.Wait (100));

			Assert.IsTrue (source.WasConsumed (header));

			Assert.AreEqual (43, block.Receive (TimeSpan.FromMilliseconds (100)));

			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (new DataflowMessageHeader (3), 44, source, false));

			Assert.AreEqual (44, block.Receive ());
		}

		[Test]
		public void TransformManyBlockTest ()
		{
			var block = new TransformManyBlock<int, int> (
				i => new[] { -i, i },
				new ExecutionDataflowBlockOptions { BoundedCapacity = 1 });

			Assert.IsTrue (block.Post (1));
			Assert.IsFalse (block.Post (2));

			Assert.IsFalse (block.Completion.Wait (100));

			Assert.IsFalse (block.Post (3));

			Assert.AreEqual (-1, block.Receive ());

			Assert.IsFalse (block.Post (4));

			Assert.AreEqual (1, block.Receive ());

			Assert.IsTrue (block.Post (5));

			Assert.AreEqual (-5, block.Receive ());
			Assert.AreEqual (5, block.Receive ());
		}

		[Test]
		public void TransformFullTest ()
		{
			var scheduler = new TestScheduler ();

			int n = 0;
			var transform = new TransformBlock<int, int> (
				i => Interlocked.Increment (ref n),
				new ExecutionDataflowBlockOptions
				{ BoundedCapacity = 2, TaskScheduler = scheduler });

			Assert.IsTrue (transform.Post (1));
			Assert.IsTrue (transform.Post (2));

			Assert.GreaterOrEqual (scheduler.ExecuteAll (), 1);

			Assert.AreEqual (2, Thread.VolatileRead (ref n));
		}

		[Test]
		public void TransformManyOverfullTest ()
		{
			var scheduler = new TestScheduler ();

			int n = 0;
			var transform = new TransformManyBlock<int, int> (
				i =>
				{
					Interlocked.Increment (ref n);
					return new[] { -i, i };
				},
				new ExecutionDataflowBlockOptions
				{ BoundedCapacity = 2, TaskScheduler = scheduler });

			Assert.IsTrue (transform.Post (1));
			Assert.IsTrue (transform.Post (2));

			Assert.GreaterOrEqual (scheduler.ExecuteAll (), 1);

			Assert.AreEqual (2, Thread.VolatileRead (ref n));
		}

		int n;

		[Test]
		public void TransformManyOverfullTest2 ()
		{
			var scheduler = new TestScheduler ();

			n = 0;
			var transform = new TransformManyBlock<int, int> (
				i => ComputeResults (),
				new ExecutionDataflowBlockOptions
				{ BoundedCapacity = 100, TaskScheduler = scheduler });

			for (int i = 0; i < 100; i++)
				Assert.IsTrue (transform.Post (i));

			Assert.IsFalse (transform.Post (101));

			Assert.GreaterOrEqual (scheduler.ExecuteAll (), 1);

			Assert.IsFalse (transform.Post (102));

			Assert.AreEqual (10000, Thread.VolatileRead (ref n));
		}

		IEnumerable<int> ComputeResults ()
		{
			for (int j = 0; j < 100; j++)
				yield return Interlocked.Increment (ref n);
		}

		[Test]
		public void MultipleOffersTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BufferBlock<int> (
				new DataflowBlockOptions { BoundedCapacity = 1, TaskScheduler = scheduler });
			var target = (ITargetBlock<int>)block;
			var source = new TestSourceBlock<int> ();

			var header1 = new DataflowMessageHeader (1);
			Assert.AreEqual (DataflowMessageStatus.Accepted,
				target.OfferMessage (header1, 41, source, false));

			var header2 = new DataflowMessageHeader (2);
			source.AddMessage (header2, 42);
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header2, 42, source, false));

			var header3 = new DataflowMessageHeader (3);
			source.AddMessage (header3, 43);
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header3, 43, source, false));

			Assert.AreEqual (41, block.Receive ());
			scheduler.ExecuteAll ();
			Assert.IsTrue (source.WasConsumed (header3));
			Assert.IsFalse (source.WasConsumed (header2));
		}

		[Test]
		public void DontConsumePostponedAfterCompleteTest ()
		{
			var scheduler = new TestScheduler ();
			var block = new BufferBlock<int> (
				new DataflowBlockOptions { BoundedCapacity = 1, TaskScheduler = scheduler });
			var target = (ITargetBlock<int>)block;
			var source = new TestSourceBlock<int> ();

			Assert.IsTrue (block.Post (11));

			var header = new DataflowMessageHeader (1);
			source.AddMessage (header, 12);
			Assert.AreEqual (DataflowMessageStatus.Postponed,
				target.OfferMessage (header, 12, source, false));

			block.Complete ();

			Assert.AreEqual (11, block.Receive ());

			scheduler.ExecuteAll ();

			Assert.IsFalse (source.WasConsumed (header));
		}
	}

	class TestSourceBlock<T> : ISourceBlock<T> {
		readonly Dictionary<DataflowMessageHeader, T> messages =
			new Dictionary<DataflowMessageHeader, T> ();

		readonly HashSet<DataflowMessageHeader> consumed =
			new HashSet<DataflowMessageHeader> ();
		readonly HashSet<DataflowMessageHeader> reserved =
			new HashSet<DataflowMessageHeader> ();

		public void Complete ()
		{
			throw new NotImplementedException ();
		}

		public void Fault (Exception exception)
		{
			throw new NotImplementedException ();
		}

		public Task Completion { get; private set; }

		public void AddMessage (DataflowMessageHeader header, T item)
		{
			messages.Add (header, item);
		}

		public bool WasConsumed (DataflowMessageHeader header)
		{
			return consumed.Contains (header);
		}

		public bool WasReserved (DataflowMessageHeader header)
		{
			return reserved.Contains (header);
		}

		public Action ConsumeWaiter { get; set; }
			 
		public T ConsumeMessage (DataflowMessageHeader messageHeader,
		                         ITargetBlock<T> target, out bool messageConsumed)
		{
			T item;
			if (messages.TryGetValue (messageHeader, out item)) {
				if (ConsumeWaiter != null)
					ConsumeWaiter ();
				messages.Remove (messageHeader);
				consumed.Add (messageHeader);
				messageConsumed = true;
				return item;
			}
			messageConsumed = false;
			return default(T);
		}

		public IDisposable LinkTo (ITargetBlock<T> target,
		                           DataflowLinkOptions linkOptions)
		{
			throw new NotImplementedException ();
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader,
		                            ITargetBlock<T> target)
		{
			reserved.Add (messageHeader);
			return messages.ContainsKey (messageHeader);
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader,
		                                ITargetBlock<T> target)
		{
			throw new NotImplementedException ();
		}
	}
}