// ReceiveBlock.cs
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
using System.Collections.Concurrent;

namespace System.Threading.Tasks.Dataflow
{
	/* This internal block is used by the Receive methods in DataflowBlock static class
	 * to retrieve elements in a blocking way
	 */
	internal class ReceiveBlock<TOutput> : ITargetBlock<TOutput>
	{
		ManualResetEventSlim waitHandle = new ManualResetEventSlim (false);
		TaskCompletionSource<TOutput> completion = new TaskCompletionSource<TOutput> ();
		IDisposable linkBridge;

		public DataflowMessageStatus OfferMessage (DataflowMessageHeader messageHeader,
		                                           TOutput messageValue,
		                                           ISourceBlock<TOutput> source,
		                                           bool consumeToAccept)
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

			/* We do the unlinking here so that we don't get called twice
			 */
			if (linkBridge != null) {
				linkBridge.Dispose ();
				linkBridge = null;
			}

			return DataflowMessageStatus.Accepted;
		}

		public TOutput WaitAndGet (IDisposable bridge, CancellationToken token, long timeout)
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

		public void Wait (CancellationToken token, long timeout)
		{
			waitHandle.Wait (timeout >= int.MaxValue ? int.MaxValue : (int)timeout, token);
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

		}

		public void Fault (Exception ex)
		{
			
		}
	}
}

