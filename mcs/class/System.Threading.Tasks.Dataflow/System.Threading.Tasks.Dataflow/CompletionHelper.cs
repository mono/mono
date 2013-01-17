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
	/// Used to implement Dataflow completion tracking,
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
			if (options != null && options.CancellationToken != CancellationToken.None)
				options.CancellationToken.Register (RequestCancel);
		}

		[Obsolete ("Use ctor")]
		public static CompletionHelper GetNew (DataflowBlockOptions options)
		{
			return new CompletionHelper (options);
		}

		public Task Completion {
			get { return source.Task; }
		}

		/// <summary>
		/// Whether <see cref="Completion"/> can be faulted or cancelled immediatelly.
		/// It can't for example when a block is currently executing user action.
		/// In that case, the fault (or cancellation) is queued,
		/// and is actually acted upon when this property is set back to <c>true</c>.
		/// </summary>
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

		/// <summary>
		/// Whether the block can act as if it's not completed
		/// (accept new items, start executing user action).
		/// </summary>
		public bool CanRun {
			get { return !Completion.IsCompleted && !requestedFaultOrCancel.Value; }
		}

		/// <summary>
		/// Sets the block as completed.
		/// Should be called only when the block is really completed
		/// (e.g. the output queue is empty) and not right after
		/// the user calls <see cref="IDataflowBlock.Complete"/>.
		/// </summary>
		public void Complete ()
		{
			source.TrySetResult (null);
		}

		/// <summary>
		/// Requests faulting of the block using a given exception.
		/// If the block can't be faulted immediatelly (see <see cref="CanFaultOrCancelImmediatelly"/>),
		/// the exception will be queued, and the block will fault as soon as it can.
		/// </summary>
		/// <param name="exception">The exception that is the cause of the fault.</param>
		/// <param name="canBeIgnored">Can this exception be ignored, if there are more exceptions?</param>
		/// <remarks>
		/// When calling <see cref="IDataflowBlock.Fault"/> repeatedly, only the first exception counts,
		/// even in the cases where the block can't be faulted immediatelly.
		/// But exceptions from user actions in execution blocks count always,
		/// which is the reason for the <paramref name="canBeIgnored"/> parameter.
		/// </remarks>
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

		/// <summary>
		/// Actually faults the block with a single exception.
		/// </summary>
		/// <remarks>
		/// Should be only called when <see cref="CanFaultOrCancelImmediatelly"/> is <c>true</c>.
		/// </remarks>
		void Fault (Exception exception)
		{
			source.TrySetException (exception);
		}

		/// <summary>
		/// Actually faults the block with a multiple exceptions.
		/// </summary>
		/// <remarks>
		/// Should be only called when <see cref="CanFaultOrCancelImmediatelly"/> is <c>true</c>.
		/// </remarks>
		void Fault (IEnumerable<Exception> exceptions)
		{
			source.TrySetException (exceptions);
		}

		/// <summary>
		/// Requests cancellation of the block.
		/// If the block can't be cancelled immediatelly (see <see cref="CanFaultOrCancelImmediatelly"/>),
		/// the cancellation will be queued, and the block will cancel as soon as it can.
		/// </summary>
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

		/// <summary>
		/// Actually cancels the block.
		/// </summary>
		/// <remarks>
		/// Should be only called when <see cref="CanFaultOrCancelImmediatelly"/> is <c>true</c>.
		/// </remarks>
		void Cancel ()
		{
			source.TrySetCanceled ();
		}
	}
}