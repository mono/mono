// ObservableDataflowBlock.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	internal class ObservableDataflowBlock<TSource> : IObservable<TSource>
	{
		class ObserverWrapper : ITargetBlock<TSource>
		{
			IObserver<TSource> observer;

			public ObserverWrapper (IObserver<TSource> observer)
			{
				this.observer = observer;
			}

			public void Complete ()
			{
				observer.OnCompleted ();
			}

			public void Fault (Exception ex)
			{
				observer.OnError (ex);
			}

			public Task Completion {
				get {
					return null;
				}
			}

			public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
			                                           TSource messageValue,
			                                           ISourceBlock<TSource> source,
			                                           bool consumeToAccept)
			{
				if (consumeToAccept) {
					if (!source.ReserveMessage (messageHeader, this))
						return DataflowMessageStatus.NotAvailable;
					bool consumed;
					messageValue = source.ConsumeMessage (messageHeader, this, out consumed);
					if (!consumed)
						return DataflowMessageStatus.NotAvailable;
				}

				observer.OnNext (messageValue);

				return DataflowMessageStatus.Accepted;
			}
		}

		ISourceBlock<TSource> source;

		public ObservableDataflowBlock (ISourceBlock<TSource> source)
		{
			this.source = source;
		}

		public IDisposable Subscribe (IObserver<TSource> observer)
		{
			ObserverWrapper wrapper = new ObserverWrapper (observer);
			return source.LinkTo (wrapper);
		}
	}
}

#endif
