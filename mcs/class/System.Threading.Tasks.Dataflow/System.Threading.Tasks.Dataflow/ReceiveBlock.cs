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
	/// This internal block is used by the Receive methods in DataflowBlock static class
	/// to retrieve elements in a blocking way
	/// </summary>
	internal class ReceiveBlock<TOutput> : ITargetBlock<TOutput> {
		readonly ManualResetEventSlim waitHandle =
			new ManualResetEventSlim (false);
		readonly TaskCompletionSource<TOutput> completion =
			new TaskCompletionSource<TOutput> ();
		IDisposable linkBridge;
		volatile bool completed;

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TOutput messageValue,
			ISourceBlock<TOutput> source, bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				return DataflowMessageStatus.Declined;

			if (consumeToAccept) {
				bool consummed;
				if (!source.ReserveMessage (messageHeader, this))
					return DataflowMessageStatus.NotAvailable;
				messageValue = source.ConsumeMessage (messageHeader, this, out consummed);
				if (!consummed)
					return DataflowMessageStatus.NotAvailable;
			}

			ReceivedValue = messageValue;
			completion.TrySetResult (messageValue);
			Thread.MemoryBarrier ();
			waitHandle.Set ();

			// We do the unlinking here so that we don't get called twice
			if (linkBridge != null) {
				linkBridge.Dispose ();
				linkBridge = null;
			}

			return DataflowMessageStatus.Accepted;
		}

		public TOutput WaitAndGet (IDisposable bridge, CancellationToken token, int timeout)
		{
			this.linkBridge = bridge;
			Wait (token, timeout);
			return ReceivedValue;
		}

		public Task<TOutput> AsyncGet (IDisposable bridge, CancellationToken token, long timeout)
		{
			this.linkBridge = bridge;
			token.Register (() => completion.TrySetCanceled ());
			// TODO : take care of timeout through the TaskEx.Wait thing
			return completion.Task;
		}

		public void Wait (CancellationToken token, int timeout)
		{
			// Wait() throws correct cancellation exception by itself
			if (!waitHandle.Wait (timeout, token))
				throw new TimeoutException ();

			if (completed)
				throw new InvalidOperationException (
					"No item could be received from the source.");
		}

		public TOutput ReceivedValue {
			get;
			private set;
		}

		public Task Completion {
			get {
				return null;
			}
		}

		public void Complete ()
		{
			completed = true;
			waitHandle.Set ();
		}

		public void Fault (Exception exception)
		{
			Complete ();
		}
	}
}