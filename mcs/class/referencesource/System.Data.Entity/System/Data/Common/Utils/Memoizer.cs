//---------------------------------------------------------------------
// <copyright file="Memoizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  	 Microsoft, Microsoft
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
namespace System.Data.Common.Utils
{
    /// <summary>
    /// Remembers the result of evaluating an expensive function so that subsequent
    /// evaluations are faster. Thread-safe.
    /// </summary>
    /// <typeparam name="TArg">Type of the argument to the function.</typeparam>
    /// <typeparam name="TResult">Type of the function result.</typeparam>
    internal sealed class Memoizer<TArg, TResult>
    {
        private readonly Func<TArg, TResult> _function;
        private readonly Dictionary<TArg, Result> _resultCache;
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Constructs
        /// </summary>
        /// <param name="function">Required. Function whose values are being cached.</param>
        /// <param name="argComparer">Optional. Comparer used to determine if two functions arguments
        /// are the same.</param>
        internal Memoizer(Func<TArg, TResult> function, IEqualityComparer<TArg> argComparer)
        {
            EntityUtil.CheckArgumentNull(function, "function");

            _function = function;
            _resultCache = new Dictionary<TArg, Result>(argComparer);
            _lock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Evaluates the wrapped function for the given argument. If the function has already
        /// been evaluated for the given argument, returns cached value. Otherwise, the value
        /// is computed and returned.
        /// </summary>
        /// <param name="arg">Function argument.</param>
        /// <returns>Function result.</returns>
        internal TResult Evaluate(TArg arg)
        {
            Result result;

            // Check to see if a result has already been computed
            if (!TryGetResult(arg, out result))
            {
                // compute the new value
                _lock.EnterWriteLock();
                try
                {
                    // see if the value has been computed in the interim
                    if (!_resultCache.TryGetValue(arg, out result))
                    {
                        result = new Result(() => _function(arg));
                        _resultCache.Add(arg, result);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }

            // note: you need to release the global cache lock before (potentially) acquiring
            // a result lock in result.GetValue()
            return result.GetValue();
        }

        internal bool TryGetValue(TArg arg, out TResult value)
        {
            Result result;
            if (TryGetResult(arg, out result))
            {
                value = result.GetValue();
                return true;
            }
            else
            {
                value = default(TResult);
                return false;
            }
        }

        private bool TryGetResult(TArg arg, out Result result)
        {
            _lock.EnterReadLock();
            try
            {
                return _resultCache.TryGetValue(arg, out result);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Encapsulates a 'deferred' result. The result is constructed with a delegate (must not 
        /// be null) and when the user requests a value the delegate is invoked and stored.
        /// </summary>
        private class Result
        {
            private TResult _value;
            private Func<TResult> _delegate;

            internal Result(Func<TResult> createValueDelegate)
            {
                Debug.Assert(null != createValueDelegate, "delegate must be given");
                _delegate = createValueDelegate;
            }

            internal TResult GetValue()
            {
                if (null == _delegate)
                {
                    // if the delegate has been cleared, it means we have already computed the value
                    return _value;
                }

                // lock the entry while computing the value so that two threads
                // don't simultaneously do the work
                lock (this)
                {
                    if (null == _delegate)
                    {
                        // between our initial check and our acquisition of the lock, some other
                        // thread may have computed the value
                        return _value;
                    }
                    _value = _delegate();

                    // ensure _delegate (and its closure) is garbage collected, and set to null
                    // to indicate that the value has been computed
                    _delegate = null;
                    return _value;
                }
            }
        }
    }
}
