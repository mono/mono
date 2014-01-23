// ReceivingTest.cs
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
	public class ReceivingTest {
		[Test]
		public void PostponeTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));
			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			int i;
			Assert.IsTrue (target.RetryPostponed (out i));
			Assert.AreEqual (42, i);
		}

		[Test]
		public void PostponeTwoTargetsTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target1 = new TestTargetBlock<int> { Postpone = true };
			var target2 = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target1));
			Assert.IsNotNull (source.LinkTo (target2));
			Assert.IsFalse (target1.HasPostponed);
			Assert.IsFalse (target2.HasPostponed);

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target1.HasPostponed);
			Assert.IsTrue (target2.HasPostponed);

			int i;
			Assert.IsTrue (target2.RetryPostponed (out i));
			Assert.AreEqual (42, i);

			Assert.IsFalse (target1.RetryPostponed (out i));
			Assert.AreEqual (default(int), i);
		}

		[Test]
		public void DecliningTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target1 = new TestTargetBlock<int> { Decline = true };
			var target2 = new TestTargetBlock<int> ();
			Assert.IsNotNull (source.LinkTo (target1));
			Assert.IsNotNull (source.LinkTo (target2));
			Assert.AreEqual (default(int), target1.DirectlyAccepted);
			Assert.AreEqual (default(int), target2.DirectlyAccepted);

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.AreEqual (default(int), target1.DirectlyAccepted);
			Assert.AreEqual (42, target2.DirectlyAccepted);
		}

		[Test]
		public void ConditionalDecliningTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Decline = true };
			Assert.IsNotNull (source.LinkTo (target));
			Assert.AreEqual (default(int), target.DirectlyAccepted);

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.AreEqual (default(int), target.DirectlyAccepted);

			target.Decline = false;
			Assert.IsTrue (source.Post (43));
			scheduler.ExecuteAll ();
			Assert.AreEqual (default(int), target.DirectlyAccepted);

			Assert.AreEqual (42, source.Receive (TimeSpan.FromMilliseconds (100)));
			scheduler.ExecuteAll ();
			Assert.AreEqual (43, target.DirectlyAccepted);
		}

		[Test]
		public void TryReceiveWithPredicateTest ()
		{
			var source = new BufferBlock<int> ();
			Assert.IsTrue (source.Post (42));
			Assert.IsTrue (source.Post (43));

			int item;
			Assert.IsFalse (source.TryReceive (i => i == 43, out item));
			Assert.AreEqual (default(int), item);

			Assert.AreEqual (42, source.Receive ());

			Assert.IsTrue (source.TryReceive (i => i == 43, out item));
			Assert.AreEqual (43, item);
		}

		[Test]
		public void ReserveTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));
			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (source.Post (42));
			Assert.IsTrue (source.Post (43));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue (target.ReservePostponed ());
			int i;
			Assert.IsFalse (source.TryReceive (null, out i));
			Assert.AreEqual (default(int), i);
			IList<int> items;
			Assert.IsFalse (source.TryReceiveAll (out items));
			Assert.AreEqual (default(IList<int>), items);

			Assert.IsTrue (target.RetryPostponed (out i));
			Assert.AreEqual (42, i);

			Assert.IsTrue (source.TryReceive (null, out i));
			Assert.AreEqual (43, i);
		}

		[Test]
		public void ConsumeAfterReceiveTest ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));
			Assert.IsFalse (target.HasPostponed);

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			target.Postpone = false;

			Assert.AreEqual (42, source.Receive ());
			Assert.IsTrue (source.Post (43));

			Assert.AreEqual (default(int), target.DirectlyAccepted);

			int i;
			Assert.IsFalse (target.RetryPostponed (out i));
			Assert.AreEqual (default(int), i);

			scheduler.ExecuteAll ();

			Assert.AreEqual (43, target.DirectlyAccepted);
		}

		[Test]
		public void FaultConsume ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			((IDataflowBlock)source).Fault (new Exception ());

			scheduler.ExecuteAll ();
			Thread.Sleep (100);

			int value;
			Assert.IsFalse (target.RetryPostponed (out value));
		}

		[Test]
		public void FaultConsumeBroadcast ()
		{
			var scheduler = new TestScheduler ();
			var source = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			var exception = new Exception ();
			((IDataflowBlock)source).Fault (exception);

			scheduler.ExecuteAll ();

			try {
				source.Completion.Wait (1000);
				Assert.Fail ("Task must be faulted");
			} catch (AggregateException ex) {
				Assert.AreEqual (exception, ex.InnerException, "#9");
			}

			Assert.IsTrue (source.Completion.IsFaulted);
			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
			Assert.AreEqual (42, value);
		}

		[Test]
		public void FaultExecutingConsume ()
		{
			var evt = new ManualResetEventSlim ();
			var source = new TransformBlock<int, int> (i =>
			{
				if (i == 2)
					evt.Wait ();
				return i;
			});
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));

			Assert.IsTrue (source.Post (1), "#1");
			Assert.IsTrue (source.Post (2), "#2");
			Assert.IsTrue (source.Post (3), "#3");
			target.PostponedEvent.Wait (1000);
			Assert.IsTrue (target.HasPostponed, "#4");

			var exception = new Exception ();
			((IDataflowBlock)source).Fault (exception);

			source.Completion.Wait (1000);

			Assert.IsFalse (source.Completion.IsFaulted, "#5");
			int value;
			Assert.IsTrue (target.RetryPostponed (out value), "#6");
			Assert.AreEqual (1, value, "#7");

			evt.Set ();

			try {
				source.Completion.Wait (1000);
				Assert.Fail ("Task must be faulted");
			} catch (AggregateException ex) {
				Assert.AreEqual (exception, ex.InnerException, "#9");
			}

			Assert.IsTrue (source.Completion.IsFaulted, "#10");
			Assert.IsFalse (target.RetryPostponed (out value), "#11");
		}

		[Test]
		public void ReserveFaultConsume ()
		{
			var scheduler = new TestScheduler ();
			var source =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue (target.ReservePostponed ());

			((IDataflowBlock)source).Fault (new Exception ());

			scheduler.ExecuteAll ();
			Thread.Sleep (100);

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
		}

		[Test]
		public void ReserveFaultConsumeBroadcast ()
		{
			var scheduler = new TestScheduler ();
			var source = new BroadcastBlock<int> (null,
				new DataflowBlockOptions { TaskScheduler = scheduler });
			var target = new TestTargetBlock<int> { Postpone = true };
			Assert.IsNotNull (source.LinkTo (target));

			Assert.IsTrue (source.Post (42));
			scheduler.ExecuteAll ();
			Assert.IsTrue (target.HasPostponed);

			Assert.IsTrue (target.ReservePostponed ());

			((IDataflowBlock)source).Fault (new Exception ());

			scheduler.ExecuteAll ();
			Thread.Sleep (100);

			int value;
			Assert.IsTrue (target.RetryPostponed (out value));
		}

		[Test]
		public void PostAfterTimeout ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });

			AssertEx.Throws<TimeoutException> (
				() => block.Receive (TimeSpan.FromMilliseconds (100)));

			block.Post (1);

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (block.TryReceive (out item));
			Assert.AreEqual (1, item);
		}

		[Test]
		public void PostAfterCancellation ()
		{
			var scheduler = new TestScheduler ();
			var block =
				new BufferBlock<int> (new DataflowBlockOptions { TaskScheduler = scheduler });

			var tokenSource = new CancellationTokenSource ();

			Task.Factory.StartNew (
				() =>
				{
					Thread.Sleep (100);
					tokenSource.Cancel ();
				});

			AssertEx.Throws<OperationCanceledException> (
				() => block.Receive (tokenSource.Token));

			block.Post (1);

			scheduler.ExecuteAll ();

			int item;
			Assert.IsTrue (block.TryReceive (out item));
			Assert.AreEqual (1, item);
		}
	}

	class TestTargetBlock<T> : ITargetBlock<T> {
		public bool Postpone { get; set; }

		public bool Decline { get; set; }

		Tuple<ISourceBlock<T>, DataflowMessageHeader> postponed;

		public T DirectlyAccepted { get; private set; }

		public ManualResetEventSlim PostponedEvent = new ManualResetEventSlim ();

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source,
			bool consumeToAccept)
		{
			if (Decline)
				return DataflowMessageStatus.Declined;

			if (Postpone) {
				postponed = Tuple.Create (source, messageHeader);
				PostponedEvent.Set ();
				return DataflowMessageStatus.Postponed;
			}

			DirectlyAccepted = messageValue;
			return DataflowMessageStatus.Accepted;
		}

		public bool HasPostponed
		{
			get { return postponed != null; }
		}

		public bool RetryPostponed (out T value)
		{
			bool consumed;
			value = postponed.Item1.ConsumeMessage (
				postponed.Item2, this, out consumed);
			postponed = null;
			return consumed;
		}

		public bool ReservePostponed ()
		{
			return postponed.Item1.ReserveMessage (postponed.Item2, this);
		}

		public void Complete ()
		{
		}

		public void Fault (Exception exception)
		{
		}

		public Task Completion { get; private set; }
	}
}
