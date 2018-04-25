//------------------------------------------------------------------------------
// <copyright file="DisposableAction.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;
    using System.Threading;

    // Helper class for wrapping a continuation inside an IDisposable.
    // This class is thread-safe.

    internal sealed class DisposableAction : IDisposable {

        public static readonly DisposableAction Empty = new DisposableAction(null);

        private Action _disposeAction;

        public DisposableAction(Action disposeAction) {
            _disposeAction = disposeAction;
        }

        public void Dispose() {
            // Interlocked allows the continuation to be executed only once
            Action continuation = Interlocked.Exchange(ref _disposeAction, null);
            if (continuation != null) {
                continuation();
            }
        }

    }
}
