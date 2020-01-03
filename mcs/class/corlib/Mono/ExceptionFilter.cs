using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mono {
    public abstract class ExceptionFilter {
        private class HasFilterRunTable : Dictionary<ExceptionFilter, bool> {
        }

        public static readonly int exception_continue_search = 0;
        public static readonly int exception_execute_handler = 1;

        public int Result;

        public static readonly ThreadLocal<List<ExceptionFilter>> ExceptionFilters = 
            new ThreadLocal<List<ExceptionFilter>> (() => new List<ExceptionFilter> (128));

        private static object LastEvaluatedException = null;
        private static readonly object FilterHasRunSentinel = new object ();

        private static readonly ThreadLocal<ConditionalWeakTable<object, HasFilterRunTable>> HasFilterRun =
            new ThreadLocal<ConditionalWeakTable<object, HasFilterRunTable>> (
                () => new ConditionalWeakTable<object, HasFilterRunTable> ()
            );

        public abstract int Evaluate (object exc);

        public static void Push (ExceptionFilter filter) 
        {
            filter.Result = exception_continue_search;
            ExceptionFilters.Value.Add (filter);
        }

        public static void Pop (ExceptionFilter filter) 
        {
            var ef = ExceptionFilters.Value;
            if (ef.Count == 0)
                throw new Exception ("Corrupt exception filter stack");
            var current = ef[ef.Count - 1];
            ef.RemoveAt (ef.Count - 1);
            if (current != filter)
                throw new Exception ("Corrupt exception filter stack");
        }

        /// <summary>
        /// Automatically runs any active exception filters for the exception exc, 
        ///  then returns true if the provided filter indicated that the current block
        ///  should run.
        /// </summary>
        /// <param name="exc">The exception to pass to the filters</param>
        /// <returns>true if this filter selected the exception handler to run</returns>
        public bool ShouldRunHandler (object exc) 
        {
            if (exc == null)
                throw new ArgumentNullException ("exc");

            PerformEvaluate (exc);

            var result = Result == exception_execute_handler;
            // Console.WriteLine($"ShouldRunHandler for {this.GetType().Name} == {result}");
            return result;
        }

        /// <summary>
        /// Runs all active exception filters until one of them returns execute_handler.
        /// Afterward, the filters will have an initialized Result and the selected one will have
        ///  a result with the value exception_continue_search.
        /// If filters have already been run for the active exception they will not be run again.
        /// </summary>
        /// <param name="exc">The exception filters are being run for.</param>
        public static void PerformEvaluate (object exc) 
        {
            // FIXME: Attempt to avoid running filters multiple times when unwinding.
            // I think this doesn't work right for rethrow?
            if (LastEvaluatedException == exc)
                return;

            var ef = ExceptionFilters.Value;
            var hasLocatedValidHandler = false;

            // Set in advance in case the filter throws.
            // These two state variables allow us to early out in the case where Evaluate() is triggered
            //  in multiple stack frames while unwinding even though filters have already run.
            LastEvaluatedException = exc;

            var hfrByException = HasFilterRun.Value;
            HasFilterRunTable hfrt;
            if (!hfrByException.TryGetValue (exc, out hfrt)) {
                hfrt = new HasFilterRunTable ();
                hfrByException.Add (exc, hfrt);
            }

            for (int i = ef.Count - 1; i >= 0; i--) {
                var filter = ef[i];
                if (hasLocatedValidHandler) {
                    filter.Result = exception_continue_search;
                    continue;
                }

                if (hfrt.ContainsKey (filter)) {
                    // Console.WriteLine($"Skipping filter {filter} because it already ran for exc {exc}");
                    continue;
                }

                var result = filter.Evaluate (exc);
                hfrt[filter] = result == exception_execute_handler;
                filter.Result = result;
                if (result == exception_execute_handler)
                    hasLocatedValidHandler = true;
            }
        }
    }
}
