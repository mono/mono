using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mono {
	public class ExceptionFilterException : Exception {
		public ExceptionFilterException (string message)
			: base (message) {
		}

		public ExceptionFilterException (string message, Exception innerException)
			: base (message, innerException) {
		}
	}

	public abstract class ExceptionFilter {
		private class ResultTable : Dictionary<object, int> {
		}

		private class ThreadState {
			public readonly List<ExceptionFilter> ExceptionFilters =
				new List<ExceptionFilter> (128);
		}

		private readonly ResultTable Results = new ResultTable ();

		public static readonly int exception_continue_search = 0;
		public static readonly int exception_execute_handler = 1;
		// Tracking whether the filter threw an unhandled exception, for debugging
		public static readonly int exception_continue_search_due_to_unhandled_exception = 2;
		// If we abort our search we fill in the rest of the filters with this value
		//  so that ShouldRunHandler will not throw.
		public static readonly int exception_early_out = 3;

		private static readonly ThreadLocal<ThreadState> ThreadStates =
			new ThreadLocal<ThreadState> (() => new ThreadState ());

		public abstract int Evaluate (object exc);

		public static void Push (ExceptionFilter filter) {
			ThreadStates.Value.ExceptionFilters.Add(filter);
		}

		private static void ThrowException (string message, Exception innerException = null) {
			throw new ExceptionFilterException (message, innerException);
		}

		public static void Pop (ExceptionFilter filter) {
			var ef = ThreadStates.Value.ExceptionFilters;
			if (ef.Count == 0)
				ThrowException ($"Attempted to pop filter {filter} with empty stack");
			var current = ef[ef.Count - 1];
			ef.RemoveAt(ef.Count - 1);
			if (current != filter)
				ThrowException ($"Attempted to pop filter {filter} but current filter on stack was {current}");
		}

		/// <summary>
		/// Checks whether the last filter evaluation selected this handler to run.
		/// </summary>
		/// <param name="exc">The exception being filtered.</param>
		/// <returns>true if this filter selected the exception handler to run</returns>
		public bool ShouldRunHandler (object exc) {
			if (exc == null)
				throw new ArgumentNullException("exc");

			int result;
			if (!Results.TryGetValue (exc, out result))
				ThrowException ($"Filter {this} has not been evaluated for {exc}");

			return result == exception_execute_handler;
		}

		/// <summary>
		/// Runs all active exception filters until one of them returns execute_handler.
		/// Afterward, the filters will have an initialized Result and the selected one will have
		///  a result with the value exception_continue_search.
		/// If filters have already been run for the active exception they will not be run again.
		/// </summary>
		/// <param name="exc">The exception filters are being run for.</param>
		public static void PerformEvaluate (object exc) {
			var ts = ThreadStates.Value;

			var hasLocatedValidHandler = false;

			for (int i = ts.ExceptionFilters.Count - 1; i >= 0; i--) {
				var filter = ts.ExceptionFilters[i];

				int result;
				// If the filter has not already run for this exception, evaluate it and store the result
				if (!filter.Results.TryGetValue (exc, out result) || result == (exception_early_out)) {
					try {
						// If we already located a filter during this search, mark the rest of the filters
						//  as "early out" so that ShouldRunHandler will return false instead of throwing
						if (hasLocatedValidHandler)
							filter.Results[exc] = result = exception_early_out;
						else
							filter.Results[exc] = result = filter.Evaluate(exc);
					} catch {
						// When an exception filter throws on windows netframework, the filter's exception is
						//  silently discarded and search for an exception handler continues as if it returned false
						filter.Results[exc] = result = exception_continue_search_due_to_unhandled_exception;
					}
				}

				if (result == exception_execute_handler)
					hasLocatedValidHandler = true;
			}
		}
	}
}
