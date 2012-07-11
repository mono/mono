// ActionBlock.cs
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
//
//

namespace System.Threading.Tasks.Dataflow
{
	/// <summary>
	/// This is used to implement a default behavior for Dataflow completion tracking
	/// that is the Completion property, Complete/Fault method combo
	/// and the CancellationToken option.
	/// </summary>
	internal class CompletionHelper
	{
		TaskCompletionSource<object> source;

		private readonly AtomicBoolean canFaultOrCancelImmediatelly =
			new AtomicBoolean { Value = true };
		private readonly AtomicBoolean requestedFaultOrCancel =
			new AtomicBoolean { Value = false };

		Exception requestedException;

		public static CompletionHelper GetNew (DataflowBlockOptions options)
		{
			var completionHelper = new CompletionHelper { source = new TaskCompletionSource<object> () };
			if (options != null)
				completionHelper.SetOptions (options);
			return completionHelper;
		}

		public Task Completion {
			get { return source.Task; }
		}

		public bool CanFaultOrCancelImmediatelly {
			get { return canFaultOrCancelImmediatelly.Value; }
			set {
				if (value) {
					if (canFaultOrCancelImmediatelly.TrySet () && requestedFaultOrCancel.Value) {
						if (requestedException == null)
							Cancel ();
						else
							Fault (requestedException);
					}
				} else
					canFaultOrCancelImmediatelly.Value = false;
			}
		}

		public bool CanRun {
			get {
				return source.Task.Status == TaskStatus.WaitingForActivation
				       && !requestedFaultOrCancel.Value;
			}
		}

		public void Complete ()
		{
			source.TrySetResult (null);
		}

		public void RequestFault (Exception exception)
		{
			if (exception == null)
				throw new ArgumentNullException("exception");

			if (CanFaultOrCancelImmediatelly)
				Fault (exception);
			else {
				Interlocked.CompareExchange (ref requestedException, exception, null);
				requestedFaultOrCancel.Value = true;
			}
		}

		void Fault (Exception ex)
		{
			source.TrySetException (ex);
		}

		void RequestCancel ()
		{
			if (CanFaultOrCancelImmediatelly)
				Cancel();
			else
				requestedFaultOrCancel.Value = true;
		}

		void Cancel ()
		{
			source.TrySetCanceled ();
		}

		void SetOptions (DataflowBlockOptions options)
		{
			if (options.CancellationToken != CancellationToken.None)
				options.CancellationToken.Register (RequestCancel);
		}
	}
}