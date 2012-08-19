// SendBlock.cs
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

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// This block is used in <see cref="DataflowBlock.SendAsync"/>
	/// to asynchronously wait until a single item is sent to a given target.
	/// </summary>
	class SendBlock<T> : ISourceBlock<T> {
		readonly ITargetBlock<T> sendTarget;
		readonly T item;
		CancellationToken cancellationToken;
		readonly TaskCompletionSource<bool> taskCompletionSource =
			new TaskCompletionSource<bool> ();
		readonly DataflowMessageHeader sendHeader = new DataflowMessageHeader (1);
		CancellationTokenRegistration cancellationTokenRegistration;

		bool isReserved;

		volatile bool cancelDisabled;

		public SendBlock (ITargetBlock<T> sendTarget, T item,
		                  CancellationToken cancellationToken)
		{
			this.sendTarget = sendTarget;
			this.item = item;
			this.cancellationToken = cancellationToken;
		}

		/// <summary>
		/// Sends the item given in the constructor to the target block.
		/// </summary>
		/// <returns>Task that completes when the sending is done, or can't be performed.</returns>
		public Task<bool> Send ()
		{
			cancellationTokenRegistration = cancellationToken.Register (
				() =>
				{
					if (!cancelDisabled)
						taskCompletionSource.SetCanceled ();
				});

			PerformSend ();

			return taskCompletionSource.Task;
		}

		/// <summary>
		/// Offers the item to the target and hadles its response.
		/// </summary>
		void PerformSend ()
		{
			DisableCancel ();

			if (taskCompletionSource.Task.IsCanceled)
				return;

			var status = sendTarget.OfferMessage (sendHeader, item, this, false);

			if (status == DataflowMessageStatus.Accepted)
				SetResult (true);
			else if (status != DataflowMessageStatus.Postponed)
				SetResult (false);
			else
				EnableCancel ();
		}

		public Task Completion {
			get { throw new NotSupportedException (); }
		}

		public void Complete ()
		{
			throw new NotSupportedException ();
		}

		public void Fault (Exception exception)
		{
			throw new NotSupportedException ();
		}

		public T ConsumeMessage (DataflowMessageHeader messageHeader,
		                         ITargetBlock<T> target, out bool messageConsumed)
		{
			if (!messageHeader.IsValid)
				throw new ArgumentException ("The messageHeader is not valid.",
					"messageHeader");
			if (target == null)
				throw new ArgumentNullException("target");

			DisableCancel ();

			messageConsumed = false;

			if (taskCompletionSource.Task.IsCanceled)
				return default(T);

			if (messageHeader != sendHeader || target != sendTarget) {
				EnableCancel ();
				return default(T);
			}

			SetResult (true);

			messageConsumed = true;
			return item;
		}

		public IDisposable LinkTo (ITargetBlock<T> target, DataflowLinkOptions linkOptions)
		{
			throw new NotSupportedException ();
		}

		public void ReleaseReservation (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			if (messageHeader != sendHeader || target != sendTarget || !isReserved)
				throw new InvalidOperationException (
					"The target did not have the message reserved.");

			isReserved = false;
			EnableCancel ();
			PerformSend ();
		}

		public bool ReserveMessage (DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			DisableCancel ();

			if (messageHeader == sendHeader && target == sendTarget) {
				isReserved = true;
				return true;
			}

			EnableCancel ();

			return false;
		}

		/// <summary>
		/// Temporarily disables cancelling.
		/// </summary>
		void DisableCancel ()
		{
			cancelDisabled = true;
		}

		/// <summary>
		/// Enables cancelling after it was disabled.
		/// If cancellation was attempted in the meantime,
		/// actually performs the cancelling.
		/// </summary>
		void EnableCancel ()
		{
			cancelDisabled = false;

			if (cancellationToken.IsCancellationRequested)
				taskCompletionSource.SetCanceled ();
		}

		/// <summary>
		/// Sets the result of the operation.
		/// </summary>
		void SetResult (bool result)
		{
			cancellationTokenRegistration.Dispose ();
			taskCompletionSource.SetResult (result);
		}
	}
}