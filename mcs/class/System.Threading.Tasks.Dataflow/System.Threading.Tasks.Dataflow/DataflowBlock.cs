// DataflowBlock.cs
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

namespace System.Threading.Tasks.Dataflow {
	public static class DataflowBlock {
		public static IObservable<TOutput> AsObservable<TOutput> (this ISourceBlock<TOutput> source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return new ObservableDataflowBlock<TOutput> (source);
		}

		public static IObserver<TInput> AsObserver<TInput> (this ITargetBlock<TInput> target)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			return new ObserverDataflowBlock<TInput> (target);
		}

		public static Task<int> Choose<T1, T2> (
			ISourceBlock<T1> source1, Action<T1> action1,
			ISourceBlock<T2> source2, Action<T2> action2)
		{
			return Choose (source1, action1, source2, action2,
				DataflowBlockOptions.Default);
		}

		public static Task<int> Choose<T1, T2> (
			ISourceBlock<T1> source1, Action<T1> action1,
			ISourceBlock<T2> source2, Action<T2> action2,
			DataflowBlockOptions dataflowBlockOptions)
		{
			if (source1 == null)
				throw new ArgumentNullException ("source1");
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (action1 == null)
				throw new ArgumentNullException ("action1");
			if (action2 == null)
				throw new ArgumentNullException ("action2");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			var chooser = new ChooserBlock<T1, T2, object> (action1, action2, null, dataflowBlockOptions);
			source1.LinkTo (chooser.Target1);
			source2.LinkTo (chooser.Target2);

			Task.WhenAll (source1.Completion, source2.Completion)
				.ContinueWith (_ => chooser.AllSourcesCompleted ());

			return chooser.Completion;
		}

		public static Task<int> Choose<T1, T2, T3> (
			ISourceBlock<T1> source1, Action<T1> action1,
			ISourceBlock<T2> source2, Action<T2> action2,
			ISourceBlock<T3> source3, Action<T3> action3)
		{
			return Choose (source1, action1, source2, action2, source3, action3,
				DataflowBlockOptions.Default);
		}

		public static Task<int> Choose<T1, T2, T3> (
			ISourceBlock<T1> source1, Action<T1> action1,
			ISourceBlock<T2> source2, Action<T2> action2,
			ISourceBlock<T3> source3, Action<T3> action3,
			DataflowBlockOptions dataflowBlockOptions)
		{
			if (source1 == null)
				throw new ArgumentNullException ("source1");
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (source3 == null)
				throw new ArgumentNullException ("source3");
			if (action1 == null)
				throw new ArgumentNullException ("action1");
			if (action2 == null)
				throw new ArgumentNullException ("action2");
			if (action3 == null)
				throw new ArgumentNullException ("action3");
			if (dataflowBlockOptions == null)
				throw new ArgumentNullException ("dataflowBlockOptions");

			var chooser = new ChooserBlock<T1, T2, T3> (action1, action2, action3, dataflowBlockOptions);
			source1.LinkTo (chooser.Target1);
			source2.LinkTo (chooser.Target2);
			source3.LinkTo (chooser.Target3);

			Task.WhenAll (source1.Completion, source2.Completion, source3.Completion)
				.ContinueWith (_ => chooser.AllSourcesCompleted ());

			return chooser.Completion;
		}

		public static IPropagatorBlock<TInput, TOutput> Encapsulate<TInput, TOutput> (
			ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			return new PropagatorWrapperBlock<TInput, TOutput> (target, source);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.LinkTo (target, DataflowLinkOptions.Default);
		}

		public static IDisposable LinkTo<TOutput> (
			this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target,
			Predicate<TOutput> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.LinkTo (target, DataflowLinkOptions.Default, predicate);
		}

		public static IDisposable LinkTo<TOutput> (
			this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target,
			DataflowLinkOptions linkOptions, Predicate<TOutput> predicate)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (predicate == null)
				throw new ArgumentNullException ("predicate");
			if (target == null)
				throw new ArgumentNullException ("target");

			var predicateBlock = new PredicateBlock<TOutput> (source, target, predicate);

			return source.LinkTo (predicateBlock, linkOptions);
		}

		public static Task<bool> OutputAvailableAsync<TOutput> (
			this ISourceBlock<TOutput> source)
		{
			return OutputAvailableAsync (source, CancellationToken.None);
		}

		public static Task<bool> OutputAvailableAsync<TOutput> (
			this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			cancellationToken.ThrowIfCancellationRequested ();

			if (source.Completion.IsCompleted || source.Completion.IsCanceled
			    || source.Completion.IsFaulted)
				return Task.FromResult (false);

			var block = new OutputAvailableBlock<TOutput> ();
			var bridge = source.LinkTo (block,
				new DataflowLinkOptions { PropagateCompletion = true });
			return block.AsyncGet (bridge, cancellationToken);
		}

		public static bool Post<TInput> (this ITargetBlock<TInput> target, TInput item)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			return target.OfferMessage (new DataflowMessageHeader(1), item, null, false)
			       == DataflowMessageStatus.Accepted;
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source)
		{
			return Receive (source, TimeSpan.FromMilliseconds (-1), CancellationToken.None);
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			return Receive (source, TimeSpan.FromMilliseconds (-1), cancellationToken);
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			return Receive (source, timeout, CancellationToken.None);
		}

		public static TOutput Receive<TOutput> (
			this ISourceBlock<TOutput> source, TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (timeout.TotalMilliseconds < -1)
				throw new ArgumentOutOfRangeException ("timeout");
			if (timeout.TotalMilliseconds > int.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			cancellationToken.ThrowIfCancellationRequested ();

			TOutput item;
			var receivableSource = source as IReceivableSourceBlock<TOutput>;
			if (receivableSource != null && receivableSource.TryReceive (null, out item))
				return item;

			if (source.Completion.IsCompleted || source.Completion.IsCanceled
			    || source.Completion.IsFaulted)
				throw new InvalidOperationException (
					"No item could be received from the source.");

			int timeoutMilliseconds = (int)timeout.TotalMilliseconds;
			var block = new ReceiveBlock<TOutput> (cancellationToken, timeoutMilliseconds);
			var bridge = source.LinkTo (block,
				new DataflowLinkOptions { PropagateCompletion = true });
			return block.WaitAndGet (bridge);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source)
		{
			return ReceiveAsync (source, TimeSpan.FromMilliseconds (-1), CancellationToken.None);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			return ReceiveAsync (source, TimeSpan.FromMilliseconds (-1), cancellationToken);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			return ReceiveAsync (source, timeout, CancellationToken.None);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (
			this ISourceBlock<TOutput> source, TimeSpan timeout,
			CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (timeout.TotalMilliseconds < -1)
				throw new ArgumentOutOfRangeException ("timeout");
			if (timeout.TotalMilliseconds > int.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			cancellationToken.ThrowIfCancellationRequested ();

			int timeoutMilliseconds = (int)timeout.TotalMilliseconds;
			var block = new ReceiveBlock<TOutput> (cancellationToken, timeoutMilliseconds);
			var bridge = source.LinkTo (block);
			return block.AsyncGet (bridge);
		}

		public static bool TryReceive<TOutput> (this IReceivableSourceBlock<TOutput> source, out TOutput item)
		{
			item = default (TOutput);
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.TryReceive (null, out item);
		}

		public static Task<bool> SendAsync<TInput> (
			this ITargetBlock<TInput> target, TInput item)
		{
			return SendAsync (target, item, CancellationToken.None);
		}

		public static Task<bool> SendAsync<TInput> (
			this ITargetBlock<TInput> target, TInput item,
			CancellationToken cancellationToken)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			cancellationToken.ThrowIfCancellationRequested ();

			var status = target.OfferMessage (
				new DataflowMessageHeader (1), item, null, false);

			if (status == DataflowMessageStatus.Accepted)
				return Task.FromResult (true);
			if (status != DataflowMessageStatus.Declined
			    && status != DataflowMessageStatus.Postponed)
				return Task.FromResult (false);

			var block = new SendBlock<TInput> (target, item, cancellationToken);
			return block.Send ();
		}

		public static ITargetBlock<TInput> NullTarget<TInput>()
		{
			return new NullTargetBlock<TInput> ();
		}
	}
}