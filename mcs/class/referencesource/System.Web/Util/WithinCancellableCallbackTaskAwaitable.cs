//------------------------------------------------------------------------------
// <copyright file="WithinCancellableCallbackTaskAwaitable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    // An awaitable type that invokes a continuation callback under a call to HttpContext.InvokeCancellableCallback.
    // This type is for compiler use; the ASP.NET runtime is not expected to call into these APIs directly.
    internal struct WithinCancellableCallbackTaskAwaitable {

        internal static readonly WithinCancellableCallbackTaskAwaitable Completed = new WithinCancellableCallbackTaskAwaitable(null, ((Task)Task.FromResult((object)null)).GetAwaiter());

        private readonly WithinCancellableCallbackTaskAwaiter _awaiter;

        public WithinCancellableCallbackTaskAwaitable(HttpContext context, TaskAwaiter innerAwaiter) {
            _awaiter = new WithinCancellableCallbackTaskAwaiter(context, innerAwaiter);
        }

        public WithinCancellableCallbackTaskAwaiter GetAwaiter() {
            return _awaiter;
        }

        // The awaiter type that backs WithinCancellableCallbackTaskAwaitable.
        internal struct WithinCancellableCallbackTaskAwaiter : ICriticalNotifyCompletion {

            private static readonly WaitCallback _shunt = state => ((Action)state)();

            private readonly HttpContext _context;
            private readonly TaskAwaiter _innerAwaiter;

            internal WithinCancellableCallbackTaskAwaiter(HttpContext context, TaskAwaiter innerAwaiter) {
                _context = context;
                _innerAwaiter = innerAwaiter;
            }

            public bool IsCompleted {
                get { return _innerAwaiter.IsCompleted; }
            }

            public void GetResult() {
                _innerAwaiter.GetResult();

                // If Response.End was called, need to observe it here.
                HttpContext context = _context;
                if (context != null) {
                    context.Response.ObserveResponseEndCalled();
                }
            }

            public void OnCompleted(Action continuation) {
                Action wrappedContinuation = WrapContinuation(continuation);
                _innerAwaiter.OnCompleted(wrappedContinuation);
            }

            [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)] // equivalent of [SecurityCritical]
            public void UnsafeOnCompleted(Action continuation) {
                Action wrappedContinuation = WrapContinuation(continuation);
                _innerAwaiter.UnsafeOnCompleted(wrappedContinuation);
            }

            private Action WrapContinuation(Action continuation) {
                HttpContext context = _context;
                return (context != null)
                    ? () => context.InvokeCancellableCallback(_shunt, continuation)
                    : continuation;
            }

        }

    }
}
