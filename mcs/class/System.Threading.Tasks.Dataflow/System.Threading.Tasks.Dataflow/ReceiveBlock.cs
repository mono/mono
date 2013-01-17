// ReceiveBlock.cs
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
	/// <summary>
	/// This internal block is used by the <see cref="DataflowBlock.Receive"/> methods
	/// to retrieve elements in either blocking or asynchronous way.
	/// </summary>
	class ReceiveBlock<TOutput> : ITargetBlock<TOutput> {
		readonly TaskCompletionSource<TOutput> completion =
			new TaskCompletionSource<TOutput> ();

		readonly CancellationToken token;
		CancellationTokenRegistration cancellationRegistration;
		readonly Timer timeoutTimer;

		IDisposable linkBridge;

		public ReceiveBlock (CancellationToken token, int timeout)
		{
			this.token = token;
			cancellationRegistration = token.Register (() =>
			{
				lock (completion) {
					completion.TrySetCanceled ();
				}
				CompletionSet ();
			});
			timeoutTimer = new Timer (
				_ =>
				{
					lock (completion) {
						completion.TrySetException (new TimeoutException ());
					}
					CompletionSet ();
				}, null, timeout,
				Timeout.Infinite);
		}

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TOutput messageValue,
			ISourceBlock<TOutput> source, bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				return DataflowMessageStatus.Declined;

			if (completion.Task.Status != TaskStatus.WaitingForActivation)
				return DataflowMessageStatus.DecliningPermanently;

			lock (completion) {
				if (completion.Task.Status != TaskStatus.WaitingForActivation)
					return DataflowMessageStatus.DecliningPermanently;

				if (consumeToAccept) {
					bool consummed;
					if (!source.ReserveMessage (messageHeader, this))
						return DataflowMessageStatus.NotAvailable;
					messageValue = source.ConsumeMessage (messageHeader, this, out consummed);
					if (!consummed)
						return DataflowMessageStatus.NotAvailable;
				}

				completion.TrySetResult (messageValue);
			}
			CompletionSet ();
			return DataflowMessageStatus.Accepted;
		}

		/// <summary>
		/// Synchronously waits until an item is available.
		/// </summary>
		/// <param name="bridge">The disposable object returned by <see cref="ISourceBlock{TOutput}.LinkTo"/>.</param>
		public TOutput WaitAndGet (IDisposable bridge)
		{
			try {
				return AsyncGet (bridge).Result;
			} catch (AggregateException e) {
				if (e.InnerException is TaskCanceledException)
					throw new OperationCanceledException (token);
				// resets the stack trace, but that shouldn't matter here
				throw e.InnerException;
			}
		}

		/// <summary>
		/// Asynchronously waits until an item is available.
		/// </summary>
		/// <param name="bridge">The disposable object returned by <see cref="ISourceBlock{TOutput}.LinkTo"/>.</param>
		public Task<TOutput> AsyncGet (IDisposable bridge)
		{
			linkBridge = bridge;

			return completion.Task;
		}

		/// <summary>
		/// Called after the result has been set,
		/// cleans up after this block.
		/// </summary>
		void CompletionSet ()
		{
			if (linkBridge != null) {
				linkBridge.Dispose ();
				linkBridge = null;
			}

			cancellationRegistration.Dispose ();
			timeoutTimer.Dispose ();
		}

		public Task Completion {
			get { throw new NotSupportedException (); }
		}

		public void Complete ()
		{
			lock (completion) {
				completion.TrySetException (new InvalidOperationException (
					"No item could be received from the source."));
			}
			CompletionSet ();
		}

		public void Fault (Exception exception)
		{
			Complete ();
		}
	}
}