//------------------------------------------------------------------------------
// <copyright file="ImplicitAsyncPreloadModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * ImplicitAsyncPreloadModule preloads the request entity for form posts.
 *
 * Copyright (c) 2010 Microsoft Corporation
 */

namespace System.Web {
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Handlers;
    using System.Web.Management;
    using System.Web.Util;
    
    internal class ImplicitAsyncPreloadModule {
        HttpApplication _app;
        AsyncCallback _callback;
        Stream _inputStream;
        
        internal void GetEventHandlers(out BeginEventHandler beginHandler, out EndEventHandler endHandler) {
            beginHandler = new BeginEventHandler(OnEnter);
            endHandler = new EndEventHandler(OnLeave);
        }
        
        private void Reset() {
            if (_inputStream != null) {
                _inputStream.Close();
                _inputStream = null;
            }
        }
        
        private IAsyncResult OnEnter(Object sender, EventArgs e, AsyncCallback cb, Object state) {
            Debug.Assert(_inputStream == null);
            _app = (HttpApplication)sender;
            HttpContext context = _app.Context;
            HttpRequest request = context.Request;
            HttpWorkerRequest wr = context.WorkerRequest;
            HttpAsyncResult httpAsyncResult = new HttpAsyncResult(cb, state);
            AsyncPreloadModeFlags asyncPreloadMode = context.AsyncPreloadMode;
            int contentLength;
            bool isForm = false;
            bool isFormMultiPart = false;

            if (asyncPreloadMode == AsyncPreloadModeFlags.None
                || request.ReadEntityBodyMode != ReadEntityBodyMode.None
                || wr == null
                || !wr.SupportsAsyncRead
                || !wr.HasEntityBody()
                || wr.IsEntireEntityBodyIsPreloaded()
                || context.Handler == null
                || context.Handler is TransferRequestHandler
                || context.Handler is DefaultHttpHandler
                || (contentLength = request.ContentLength) > RuntimeConfig.GetConfig(context).HttpRuntime.MaxRequestLengthBytes
                || ((isForm = StringUtil.StringStartsWithIgnoreCase(request.ContentType, "application/x-www-form-urlencoded"))
                    && (asyncPreloadMode & AsyncPreloadModeFlags.Form) != AsyncPreloadModeFlags.Form)
                || ((isFormMultiPart = StringUtil.StringStartsWithIgnoreCase(request.ContentType, "multipart/form-data"))
                    && (asyncPreloadMode & AsyncPreloadModeFlags.FormMultiPart) != AsyncPreloadModeFlags.FormMultiPart)
                || !isForm && !isFormMultiPart && (asyncPreloadMode & AsyncPreloadModeFlags.NonForm) != AsyncPreloadModeFlags.NonForm
                ) {
                Debug.Trace("AsyncPreload", " *** AsyncPreload skipped *** ");
                httpAsyncResult.Complete(true, null, null);
                return httpAsyncResult;
            }

            Debug.Trace("AsyncPreload", " *** AsyncPreload started *** ");
            try {
                if (_callback == null) {
                    _callback = new AsyncCallback(OnAsyncCompletion);
                }
                _inputStream = request.GetBufferedInputStream();
                byte[] buffer = _app.EntityBuffer;
                int bytesRead = 0;
                // loop to prevent recursive calls and potential stack overflow if/when it completes synchronously
                do {
                    IAsyncResult readAsyncResult = _inputStream.BeginRead(buffer, 0, buffer.Length, _callback, httpAsyncResult);
                    if (!readAsyncResult.CompletedSynchronously) {
                        return httpAsyncResult;
                    }
                    bytesRead = _inputStream.EndRead(readAsyncResult);
                } while (bytesRead != 0);
            }
            catch {
                Reset();
                throw;
            }
            httpAsyncResult.Complete(true, null, null);
            return httpAsyncResult;
        }
        
        private void OnLeave(IAsyncResult httpAsyncResult) {
            Reset();
            Debug.Trace("AsyncPreload", " *** AsyncPreload finished *** ");
            ((HttpAsyncResult)httpAsyncResult).End();
        }

        
        private void OnAsyncCompletion(IAsyncResult readAsyncResult) {
            if (readAsyncResult.CompletedSynchronously) {
                return;
            }
            HttpAsyncResult httpAsyncResult = readAsyncResult.AsyncState as HttpAsyncResult;
            Exception error = null;

            try {
                int bytesRead = _inputStream.EndRead(readAsyncResult);
                byte[] buffer = _app.EntityBuffer;
                // loop to prevent recursive calls and potential stack overflow when it completes synchronously
                while (bytesRead != 0) {
                    readAsyncResult = _inputStream.BeginRead(buffer, 0, buffer.Length, _callback, httpAsyncResult);
                    if (!readAsyncResult.CompletedSynchronously) {
                        return;
                    }
                    bytesRead = _inputStream.EndRead(readAsyncResult);
                }
            }
            catch(Exception e) {
                error = e;
            }
            httpAsyncResult.Complete(false, null, error);
        }
    }
}
