//------------------------------------------------------------------------------
// <copyright file="HttpTaskAsyncHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Assists in converting an HTTP Handler written using the Task Asynchronous Pattern to an IHttpAsyncHandler.
 * 
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;

    public abstract class HttpTaskAsyncHandler : IHttpAsyncHandler {

        public virtual bool IsReusable {
            get {
                // Default implementation - can be overridden by developer.
                return false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessRequest(HttpContext context) {
            // Default implementation is that this isn't synchronously callable - can be overridden by developer.
            string errorMessage = SR.GetString(SR.HttpTaskAsyncHandler_CannotExecuteSynchronously, GetType());
            throw new NotSupportedException(errorMessage);
        }

        // This is the method we actually expect developers to override.
        public abstract Task ProcessRequestAsync(HttpContext context);

        #region IHttpAsyncHandler methods
        IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
            return TaskAsyncHelper.BeginTask(() => ProcessRequestAsync(context), cb, extraData);
        }

        void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) {
            TaskAsyncHelper.EndTask(result);
        }
        #endregion

    }
}
