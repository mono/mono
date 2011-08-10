// MessageOutgoingQueue.cs
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

#if NET_4_0 || MOBILE

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	/* This class handles outgoing message that get queued when there is no
	 * block on the other end to proces it. It also allows receive operations.
	 */
	internal class MessageOutgoingQueue<T>
	{
		readonly ConcurrentQueue<T> store = new ConcurrentQueue<T> ();
		readonly BlockingCollection<T> outgoing;
		readonly CompletionHelper compHelper;
		readonly Func<bool> externalCompleteTester;

		public MessageOutgoingQueue (CompletionHelper compHelper, Func<bool> externalCompleteTester)
		{
			this.outgoing = new BlockingCollection<T> (store);
			this.compHelper = compHelper;
			this.externalCompleteTester = externalCompleteTester;
		}

		public void AddData (T data)
		{
			try {
				outgoing.Add (data);
			} catch (InvalidOperationException) {
				VerifyCompleteness ();
			}
		}

		IEnumerable<T> GetNonBlockingConsumingEnumerable ()
		{
			T temp;
			while (outgoing.TryTake (out temp))
				yield return temp;
		}

		public void ProcessForTarget (ITargetBlock<T> target, ISourceBlock<T> source, bool consumeToAccept, ref DataflowMessageHeader headers)
		{
			if (target == null)
				return;

			foreach (var output in GetNonBlockingConsumingEnumerable ())
				target.OfferMessage (headers.Increment (), output, source, consumeToAccept);
		}

		public bool TryReceive (Predicate<T> filter, out T item)
		{
			item = default (T);

			T result;
			bool success = false;
			if (store.TryPeek (out result) && (filter == null || filter (result)))
				success = outgoing.TryTake (out item);

			VerifyCompleteness ();

			return success;
		}

		public bool TryReceiveAll (out IList<T> items)
		{
			items = null;

			if (store.IsEmpty)
				return false;

			List<T> list = new List<T> (outgoing.Count);
			if (list.Count == 0)
				return false;

			list.AddRange (GetNonBlockingConsumingEnumerable ());
			items = list;

			VerifyCompleteness ();

			return list.Count > 0;
		}

		public void Complete ()
		{
			outgoing.CompleteAdding ();
			VerifyCompleteness ();
		}

		void VerifyCompleteness ()
		{
			if (outgoing.IsCompleted && externalCompleteTester ())
				compHelper.Complete ();
		}

		public bool IsEmpty {
			get {
				return store.IsEmpty;
			}
		}

		public int Count {
			get {
				return store.Count;
			}
		}

		public bool IsCompleted {
			get {
				return outgoing.IsCompleted;
			}
		}
	}
}

#endif
