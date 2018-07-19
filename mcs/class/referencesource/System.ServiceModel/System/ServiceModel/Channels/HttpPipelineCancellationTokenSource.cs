// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Threading;

    class HttpPipelineCancellationTokenSource : CancellationTokenSource
    {
        static Action<object> onCancelled = Fx.ThunkCallback<object>(OnCancelled);
        HttpRequestContext httpRequestContext;

        public HttpPipelineCancellationTokenSource(HttpRequestContext httpRequestContext)
        {
            Fx.Assert(httpRequestContext != null, "httpRequestContext should not be null.");
            this.httpRequestContext = httpRequestContext;
            this.Token.Register(onCancelled, this);
        }

        static void OnCancelled(object obj)
        {
            Fx.Assert(obj != null, "obj should not be null.");
            HttpPipelineCancellationTokenSource thisPtr = (HttpPipelineCancellationTokenSource)obj;
            thisPtr.HandleCancelCallBack();
        }

        void HandleCancelCallBack()
        {
            this.httpRequestContext.Abort();
        }
    }
}
