// DataflowBlock.cs
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
//
//


using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace System.Threading.Tasks.Dataflow
{
	public static class DataflowBlock
	{
		static DataflowMessageHeader globalHeader = new DataflowMessageHeader ();

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

		public static Task<int> Choose<T1, T2> (ISourceBlock<T1> source1, Action<T1> action1, ISourceBlock<T2> source2, Action<T2> action2)
		{
			return Choose<T1, T2> (source1, action1, source2, action2, new DataflowBlockOptions ());
		}

		public static Task<int> Choose<T1, T2> (ISourceBlock<T1> source1,
		                                        Action<T1> action1,
		                                        ISourceBlock<T2> source2,
		                                        Action<T2> action2,
		                                        DataflowBlockOptions dataflowBlockOptions)
		{
			if (source1 == null)
				throw new ArgumentNullException ("source1");
			if (source2 == null)
				throw new ArgumentNullException ("source2");

			var chooser = new ChooserBlock<T1, T2, object> (action1, action2, null, dataflowBlockOptions);
			source1.LinkTo (chooser.Target1);
			source2.LinkTo (chooser.Target2);

			return chooser.Completion;
		}

		public static Task<int> Choose<T1, T2, T3> (ISourceBlock<T1> source1,
		                                            Action<T1> action1,
		                                            ISourceBlock<T2> source2,
		                                            Action<T2> action2,
		                                            ISourceBlock<T3> source3,
		                                            Action<T3> action3)
		{
			return Choose (source1, action1, source2, action2, source3, action3, new DataflowBlockOptions ());
		}

		public static Task<int> Choose<T1, T2, T3> (ISourceBlock<T1> source1,
		                                            Action<T1> action1,
		                                            ISourceBlock<T2> source2,
		                                            Action<T2> action2,
		                                            ISourceBlock<T3> source3,
		                                            Action<T3> action3,
		                                            DataflowBlockOptions dataflowBlockOptions)
		{
			if (source1 == null)
				throw new ArgumentNullException ("source1");
			if (source2 == null)
				throw new ArgumentNullException ("source2");
			if (source3 == null)
				throw new ArgumentNullException ("source3");

			var chooser = new ChooserBlock<T1, T2, T3> (action1, action2, action3, dataflowBlockOptions);
			source1.LinkTo (chooser.Target1);
			source2.LinkTo (chooser.Target2);
			source3.LinkTo (chooser.Target3);

			return chooser.Completion;
		}

		public static IPropagatorBlock<TInput, TOutput> Encapsulate<TInput, TOutput> (ITargetBlock<TInput> target, ISourceBlock<TOutput> source)
		{
			return new PropagatorWrapperBlock<TInput, TOutput> (target, source);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target)
		{
			return source.LinkTo (target, (_) => true);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source, ITargetBlock<TOutput> target, Predicate<TOutput> predicate)
		{
			return source.LinkTo (target, predicate, true);
		}

		public static IDisposable LinkTo<TOutput> (this ISourceBlock<TOutput> source,
		                                           ITargetBlock<TOutput> target,
		                                           Predicate<TOutput> predicate,
		                                           bool discardsMessages)
		{
			return source.LinkTo (target, false);
		}

		[MonoTODO]
		public static Task<bool> OutputAvailableAsync<TOutput> (this ISourceBlock<TOutput> source)
		{
			throw new NotImplementedException ();
		}

		public static bool Post<TInput> (this ITargetBlock<TInput> target, TInput item)
		{
			if (target == null)
				throw new ArgumentNullException ("target");

			return target.OfferMessage (globalHeader.Increment (), item, null, false) == DataflowMessageStatus.Accepted;
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source)
		{
			return Receive<TOutput> (source, TimeSpan.FromMilliseconds (-1), CancellationToken.None);
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			return Receive<TOutput> (source, TimeSpan.FromMilliseconds (-1), cancellationToken);
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			return Receive<TOutput> (source, timeout, CancellationToken.None);
		}

		public static TOutput Receive<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (timeout.TotalMilliseconds < -1)
				throw new ArgumentOutOfRangeException ("timeout");

			cancellationToken.ThrowIfCancellationRequested ();

			long tm = (long)timeout.TotalMilliseconds;
			ReceiveBlock<TOutput> block = new ReceiveBlock<TOutput> ();
			var bridge = source.LinkTo (block);
			return block.WaitAndGet (bridge, cancellationToken, tm);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source)
		{
			return ReceiveAsync<TOutput> (source, TimeSpan.FromMilliseconds (-1), CancellationToken.None);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, CancellationToken cancellationToken)
		{
			return ReceiveAsync (source, TimeSpan.FromMilliseconds (-1), cancellationToken);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout)
		{
			return ReceiveAsync<TOutput> (source, timeout, CancellationToken.None);
		}

		public static Task<TOutput> ReceiveAsync<TOutput> (this ISourceBlock<TOutput> source, TimeSpan timeout, CancellationToken cancellationToken)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			if (timeout.TotalMilliseconds < -1)
				throw new ArgumentOutOfRangeException ("timeout");

			cancellationToken.ThrowIfCancellationRequested ();

			long tm = (long)timeout.TotalMilliseconds;
			ReceiveBlock<TOutput> block = new ReceiveBlock<TOutput> ();
			var bridge = source.LinkTo (block);
			return block.AsyncGet (bridge, cancellationToken, tm);
		}

		public static bool TryReceive<TOutput> (this IReceivableSourceBlock<TOutput> source, out TOutput item)
		{
			item = default (TOutput);
			if (source == null)
				throw new ArgumentNullException ("source");

			return source.TryReceive (null, out item);
		}

		[MonoTODO]
		public static Task<bool> SendAsync<TInput> (this ITargetBlock<TInput> target, TInput item)
		{
			throw new NotImplementedException ();
		}
	}
}

