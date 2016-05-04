//------------------------------------------------------------------------------
// <copyright file="AsyncPostBackErrorEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Web;

    public class AsyncPostBackErrorEventArgs : EventArgs {
        private readonly Exception _exception;

        public AsyncPostBackErrorEventArgs(Exception exception) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }
            _exception = exception;
        }

        public Exception Exception {
            get {
                return _exception;
            }
        }
    }
}
