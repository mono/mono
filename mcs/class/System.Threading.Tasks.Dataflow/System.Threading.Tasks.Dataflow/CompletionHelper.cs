// CompletionHelper.cs
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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace System.Threading.Tasks.Dataflow {
	/// <summary>
	/// This is used to implement a default behavior for Dataflow completion tracking
	/// that is the Completion property, Complete/Fault method combo
	/// and the CancellationToken option.
	/// </summary>
	class CompletionHelper {
		readonly TaskCompletionSource<object> source =
			new TaskCompletionSource<object> ();

		readonly AtomicBoolean canFaultOrCancelImmediatelly =
			new AtomicBoolean { Value = true };
		readonly AtomicBoolean requestedFaultOrCancel =
			new AtomicBoolean { Value = false };

		readonly ConcurrentQueue<Tuple<Exception, bool>> requestedExceptions =
			new ConcurrentQueue<Tuple<Exception, bool>> ();

		public CompletionHelper (DataflowBlockOptions options)
		{
			if (options != null)
				SetOptions (options);
		}

		[Obsolete ("Use ctor")]
		public static CompletionHelper GetNew (DataflowBlockOptions options)
		{
			return new CompletionHelper (options);
		}

		public Task Completion {
			get { return source.Task; }
		}

		public bool CanFaultOrCancelImmediatelly {
			get { return canFaultOrCancelImmediatelly.Value; }
			set {
				if (value) {
					if (canFaultOrCancelImmediatelly.TrySet () && requestedFaultOrCancel.Value) {
						bool canAllBeIgnored = requestedExceptions.All (t => t.Item2);
						if (canAllBeIgnored) {
							Tuple<Exception, bool> tuple;
							requestedExceptions.TryDequeue (out tuple);
							var exception = tuple.Item1;
							if (exception == null)
								Cancel ();
							else
								Fault (exception);
						} else {
							Tuple<Exception, bool> tuple;
							bool first = true;
							var exceptions = new List<Exception> (requestedExceptions.Count);
							while (requestedExceptions.TryDequeue (out tuple)) {
								var exception = tuple.Item1;
								bool canBeIgnored = tuple.Item2;
								if (first || !canBeIgnored) {
									if (exception != null)
										exceptions.Add (exception);
								}
								first = false;
							}
							Fault (exceptions);
						}
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

		public void RequestFault (Exception exception, bool canBeIgnored = true)
		{
			if (exception == null)
				throw new ArgumentNullException ("exception");

			if (CanFaultOrCancelImmediatelly)
				Fault (exception);
			else {
				// still need to store canBeIgnored, if we don't want to add locking here
				if (!canBeIgnored || requestedExceptions.Count == 0)
					requestedExceptions.Enqueue (Tuple.Create (exception, canBeIgnored));
				requestedFaultOrCancel.Value = true;
			}
		}

		void Fault (Exception exception)
		{
			source.TrySetException (exception);
		}

		void Fault (IEnumerable<Exception> exceptions)
		{
			source.TrySetException (exceptions);
		}

		void RequestCancel ()
		{
			if (CanFaultOrCancelImmediatelly)
				Cancel ();
			else {
				if (requestedExceptions.Count == 0)
					requestedExceptions.Enqueue (Tuple.Create<Exception, bool> (null, true));
				requestedFaultOrCancel.Value = true;
			}
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