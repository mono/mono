// OutputAvailableBlock.cs
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
	/// This internal block is used by the OutputAvailable methods in DataflowBlock static class
	/// to check for available items in an asynchrnousy way
	/// </summary>
	class OutputAvailableBlock<TOutput> : ITargetBlock<TOutput> {
		readonly TaskCompletionSource<bool> completion =
			new TaskCompletionSource<bool> ();
		IDisposable linkBridge;
		CancellationTokenRegistration cancellationRegistration;

		public DataflowMessageStatus OfferMessage (
			DataflowMessageHeader messageHeader, TOutput messageValue,
			ISourceBlock<TOutput> source, bool consumeToAccept)
		{
			if (!messageHeader.IsValid)
				return DataflowMessageStatus.Declined;

			if (completion.Task.Status != TaskStatus.WaitingForActivation)
				return DataflowMessageStatus.DecliningPermanently;

			completion.TrySetResult (true);
			CompletionSet ();

			return DataflowMessageStatus.DecliningPermanently;
		}

		public Task<bool> AsyncGet (IDisposable bridge, CancellationToken token)
		{
			linkBridge = bridge;
			cancellationRegistration = token.Register (() =>
			{
				lock (completion) {
					completion.TrySetCanceled ();
				}
				CompletionSet ();
			});

			return completion.Task;
		}

		void CompletionSet ()
		{
			if (linkBridge != null) {
				linkBridge.Dispose ();
				linkBridge = null;
			}

			cancellationRegistration.Dispose ();
		}

		public Task Completion {
			get { throw new NotSupportedException (); }
		}

		public void Complete ()
		{
			completion.TrySetResult (false);
			CompletionSet ();
		}

		public void Fault (Exception exception)
		{
			Complete ();
		}
	}
}