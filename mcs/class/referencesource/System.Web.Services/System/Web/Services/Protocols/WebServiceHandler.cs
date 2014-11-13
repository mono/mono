//------------------------------------------------------------------------------
// <copyright file="WebServiceHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System.Diagnostics;
    using System;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.IO;
    using System.Collections;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.Services.Interop;
    using System.Configuration;
    using Microsoft.Win32;
    using System.Threading;
    using System.Text;
    using System.Web.UI;
    using System.Web.Util;
    using System.Web.UI.WebControls;
    using System.ComponentModel; // for CompModSwitches
    using System.EnterpriseServices;
    using System.Runtime.Remoting.Messaging;
    using System.Web.Services.Diagnostics;

    internal class WebServiceHandler {
        ServerProtocol protocol;
        Exception exception;
        AsyncCallback asyncCallback;
        ManualResetEvent asyncBeginComplete;
        int asyncCallbackCalls;
        bool wroteException;
        object[] parameters = null;

        internal WebServiceHandler(ServerProtocol protocol) {
            this.protocol = protocol;
        }

        // Flush the trace file after each request so that the trace output makes it to the disk.
        static void TraceFlush() {
            Debug.Flush();
        }

        void PrepareContext() {
            this.exception = null;
            this.wroteException = false;
            this.asyncCallback = null;
            this.asyncBeginComplete = new ManualResetEvent(false);
            this.asyncCallbackCalls = 0;
            if (protocol.IsOneWay)
                return;
            HttpContext context = protocol.Context;
            if (context == null) return; // context is null in non-network case

            // we want the default to be no caching on the client
            int cacheDuration = protocol.MethodAttribute.CacheDuration;
            if (cacheDuration > 0) {
                context.Response.Cache.SetCacheability(HttpCacheability.Server);
                context.Response.Cache.SetExpires(DateTime.Now.AddSeconds(cacheDuration));
                context.Response.Cache.SetSlidingExpiration(false);
                // with soap 1.2 the action is a param in the content-type
                context.Response.Cache.VaryByHeaders["Content-type"] = true;
                context.Response.Cache.VaryByHeaders["SOAPAction"] = true;
                context.Response.Cache.VaryByParams["*"] = true;
            }
            else {
                context.Response.Cache.SetNoServerCaching();
                context.Response.Cache.SetMaxAge(TimeSpan.Zero);
            }
            context.Response.BufferOutput = protocol.MethodAttribute.BufferResponse;
            context.Response.ContentType = null;

        }

        void WriteException(Exception e) {
            if (this.wroteException) return;

            if (CompModSwitches.Remote.TraceVerbose) Debug.WriteLine("Server Exception: " + e.ToString());
            if (e is TargetInvocationException) {
                if (CompModSwitches.Remote.TraceVerbose) Debug.WriteLine("TargetInvocationException caught.");
                e = e.InnerException;
            }

            this.wroteException = protocol.WriteException(e, protocol.Response.OutputStream);
            if (!this.wroteException)
                throw e;
        }

        void Invoke() {
            PrepareContext();
            protocol.CreateServerInstance();

            string stringBuffer;
            RemoteDebugger debugger = null;
            if (!protocol.IsOneWay && RemoteDebugger.IsServerCallInEnabled(protocol, out stringBuffer)) {
                debugger = new RemoteDebugger();
                debugger.NotifyServerCallEnter(protocol, stringBuffer);
            }

            try {
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "Invoke") : null;
                TraceMethod userMethod = Tracing.On ? new TraceMethod(protocol.Target, protocol.MethodInfo.Name, this.parameters) : null;
                if (Tracing.On) Tracing.Enter(protocol.MethodInfo.ToString(), caller, userMethod);
                object[] returnValues = protocol.MethodInfo.Invoke(protocol.Target, this.parameters);
                if (Tracing.On) Tracing.Exit(protocol.MethodInfo.ToString(), caller);
                WriteReturns(returnValues);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "Invoke", e);
                if (!protocol.IsOneWay) {
                    WriteException(e);
                    throw;
                }
            }
            finally {
                protocol.DisposeServerInstance();

                if (debugger != null)
                    debugger.NotifyServerCallExit(protocol.Response);
            }
        }

        // By keeping this in a separate method we avoid jitting system.enterpriseservices.dll in cases
        // where transactions are not used.
        void InvokeTransacted() {
            Transactions.InvokeTransacted(new TransactedCallback(this.Invoke), protocol.MethodAttribute.TransactionOption);
        }

        void ThrowInitException() {
            HandleOneWayException(new Exception(Res.GetString(Res.WebConfigExtensionError), protocol.OnewayInitException), "ThrowInitException");
        }

        void HandleOneWayException(Exception e, string method) {
            if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, string.IsNullOrEmpty(method) ? "HandleOneWayException" : method, e);
            // exceptions for one-way calls are dropped because the response is already written
        }

        protected void CoreProcessRequest() {
            try {
                bool transacted = protocol.MethodAttribute.TransactionEnabled;
                if (protocol.IsOneWay) {
                    WorkItemCallback callback = null;
                    TraceMethod callbackMethod = null;
                    if (protocol.OnewayInitException != null) {
                        callback = new WorkItemCallback(this.ThrowInitException);
                        callbackMethod = Tracing.On ? new TraceMethod(this, "ThrowInitException") : null;
                    }
                    else {
                        parameters = protocol.ReadParameters();
                        callback = transacted ? new WorkItemCallback(this.OneWayInvokeTransacted) : new WorkItemCallback(this.OneWayInvoke);
                        callbackMethod = Tracing.On ? transacted ? new TraceMethod(this, "OneWayInvokeTransacted") : new TraceMethod(this, "OneWayInvoke") : null;
                    }

                    if (Tracing.On) Tracing.Information(Res.TracePostWorkItemIn, callbackMethod);
                    WorkItem.Post(callback);
                    if (Tracing.On) Tracing.Information(Res.TracePostWorkItemOut, callbackMethod);

                    protocol.WriteOneWayResponse();
                }
                else if (transacted) {
                    parameters = protocol.ReadParameters();
                    InvokeTransacted();
                }
                else {
                    parameters = protocol.ReadParameters();
                    Invoke();
                }
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "CoreProcessRequest", e);
                if (!protocol.IsOneWay)
                    WriteException(e);
            }

            TraceFlush();
        }

        private HttpContext SwitchContext(HttpContext context) {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HttpContext oldContext = HttpContext.Current;
            HttpContext.Current = context;
            return oldContext;
        }

        private void OneWayInvoke() {
            HttpContext oldContext = null;
            if (protocol.Context != null)
                oldContext = SwitchContext(protocol.Context);

            try {
                Invoke();
            }
            catch (Exception e) {
                HandleOneWayException(e, "OneWayInvoke");
            }
            finally {
                if (oldContext != null)
                    SwitchContext(oldContext);
            }
        }

        private void OneWayInvokeTransacted() {
            HttpContext oldContext = null;
            if (protocol.Context != null)
                oldContext = SwitchContext(protocol.Context);

            try {
                InvokeTransacted();
            }
            catch (Exception e) {
                HandleOneWayException(e, "OneWayInvokeTransacted");
            }
            finally {
                if (oldContext != null)
                    SwitchContext(oldContext);
            }
        }

        private void Callback(IAsyncResult result) {
            if (!result.CompletedSynchronously)
                this.asyncBeginComplete.WaitOne();
            DoCallback(result);
        }

        private void DoCallback(IAsyncResult result) {
            if (this.asyncCallback != null) {
                if (System.Threading.Interlocked.Increment(ref this.asyncCallbackCalls) == 1) {
                    this.asyncCallback(result);
                }
            }
        }

        protected IAsyncResult BeginCoreProcessRequest(AsyncCallback callback, object asyncState) {
            IAsyncResult asyncResult;

            if (protocol.MethodAttribute.TransactionEnabled)
                throw new InvalidOperationException(Res.GetString(Res.WebAsyncTransaction));

            parameters = protocol.ReadParameters();
            if (protocol.IsOneWay) {
                TraceMethod callbackMethod = Tracing.On ? new TraceMethod(this, "OneWayAsyncInvoke") : null;
                if (Tracing.On) Tracing.Information(Res.TracePostWorkItemIn, callbackMethod);
                WorkItem.Post(new WorkItemCallback(this.OneWayAsyncInvoke));
                if (Tracing.On) Tracing.Information(Res.TracePostWorkItemOut, callbackMethod);
                asyncResult = new CompletedAsyncResult(asyncState, true);
                if (callback != null)
                    callback(asyncResult);
            }
            else
                asyncResult = BeginInvoke(callback, asyncState);
            return asyncResult;
        }

        private void OneWayAsyncInvoke() {
            if (protocol.OnewayInitException != null)
                HandleOneWayException(new Exception(Res.GetString(Res.WebConfigExtensionError), protocol.OnewayInitException), "OneWayAsyncInvoke");
            else {
                HttpContext oldContext = null;
                if (protocol.Context != null)
                    oldContext = SwitchContext(protocol.Context);

                try {
                    BeginInvoke(new AsyncCallback(this.OneWayCallback), null);
                }
                catch (Exception e) {
                    HandleOneWayException(e, "OneWayAsyncInvoke");
                }
                finally {
                    if (oldContext != null)
                        SwitchContext(oldContext);
                }
            }
        }

        private IAsyncResult BeginInvoke(AsyncCallback callback, object asyncState) {
            IAsyncResult asyncResult;
            try {
                PrepareContext();
                protocol.CreateServerInstance();
                this.asyncCallback = callback;

                TraceMethod caller = Tracing.On ? new TraceMethod(this, "BeginInvoke") : null;
                TraceMethod userMethod = Tracing.On ? new TraceMethod(protocol.Target, protocol.MethodInfo.Name, this.parameters) : null;
                if (Tracing.On) Tracing.Enter(protocol.MethodInfo.ToString(), caller, userMethod);

                asyncResult = protocol.MethodInfo.BeginInvoke(protocol.Target, this.parameters, new AsyncCallback(this.Callback), asyncState);

                if (Tracing.On) Tracing.Enter(protocol.MethodInfo.ToString(), caller);

                if (asyncResult == null) throw new InvalidOperationException(Res.GetString(Res.WebNullAsyncResultInBegin));
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "BeginInvoke", e);
                // save off the exception and throw it in EndCoreProcessRequest
                exception = e;
                asyncResult = new CompletedAsyncResult(asyncState, true);
                this.asyncCallback = callback;
                this.DoCallback(asyncResult);
            }
            this.asyncBeginComplete.Set();
            TraceFlush();
            return asyncResult;
        }

        private void OneWayCallback(IAsyncResult asyncResult) {
            EndInvoke(asyncResult);
        }

        protected void EndCoreProcessRequest(IAsyncResult asyncResult) {
            if (asyncResult == null) return;

            if (protocol.IsOneWay)
                protocol.WriteOneWayResponse();
            else
                EndInvoke(asyncResult);
        }

        private void EndInvoke(IAsyncResult asyncResult) {
            try {
                if (exception != null)
                    throw (exception);
                object[] returnValues = protocol.MethodInfo.EndInvoke(protocol.Target, asyncResult);
                WriteReturns(returnValues);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "EndInvoke", e);
                if (!protocol.IsOneWay)
                    WriteException(e);
            }
            finally {
                protocol.DisposeServerInstance();
            }
            TraceFlush();
        }

        void WriteReturns(object[] returnValues) {
            if (protocol.IsOneWay) return;

            // By default ASP.NET will fully buffer the response. If BufferResponse=false
            // then we still want to do partial buffering since each write is a named
            // pipe call over to inetinfo.
            bool fullyBuffered = protocol.MethodAttribute.BufferResponse;
            Stream outputStream = protocol.Response.OutputStream;
            if (!fullyBuffered) {
                outputStream = new BufferedResponseStream(outputStream, 16 * 1024);
                //#if DEBUG
                ((BufferedResponseStream)outputStream).FlushEnabled = false;
                //#endif
            }
            protocol.WriteReturns(returnValues, outputStream);
            // This will flush the buffered stream and the underlying stream. Its important
            // that it flushes the Response.OutputStream because we always want BufferResponse=false
            // to mean we are writing back a chunked response. This gives a consistent
            // behavior to the client, independent of the size of the partial buffering.
            if (!fullyBuffered) {
                //#if DEBUG
                ((BufferedResponseStream)outputStream).FlushEnabled = true;
                //#endif
                outputStream.Flush();
            }
        }
    }

    internal class SyncSessionlessHandler : WebServiceHandler, IHttpHandler {

        internal SyncSessionlessHandler(ServerProtocol protocol) : base(protocol) { }

        public bool IsReusable {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context) {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "ProcessRequest") : null;
            if (Tracing.On) Tracing.Enter("IHttpHandler.ProcessRequest", method, Tracing.Details(context.Request));

            CoreProcessRequest();

            if (Tracing.On) Tracing.Exit("IHttpHandler.ProcessRequest", method);
        }
    }

    internal class SyncSessionHandler : SyncSessionlessHandler, IRequiresSessionState {
        internal SyncSessionHandler(ServerProtocol protocol) : base(protocol) { }
    }

    internal class AsyncSessionlessHandler : SyncSessionlessHandler, IHttpAsyncHandler {

        internal AsyncSessionlessHandler(ServerProtocol protocol) : base(protocol) { }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object asyncState) {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "BeginProcessRequest") : null;
            if (Tracing.On) Tracing.Enter("IHttpAsyncHandler.BeginProcessRequest", method, Tracing.Details(context.Request));

            IAsyncResult result = BeginCoreProcessRequest(callback, asyncState);

            if (Tracing.On) Tracing.Exit("IHttpAsyncHandler.BeginProcessRequest", method);

            return result;
        }

        public void EndProcessRequest(IAsyncResult asyncResult) {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "EndProcessRequest") : null;
            if (Tracing.On) Tracing.Enter("IHttpAsyncHandler.EndProcessRequest", method);

            EndCoreProcessRequest(asyncResult);

            if (Tracing.On) Tracing.Exit("IHttpAsyncHandler.EndProcessRequest", method);
        }
    }

    internal class AsyncSessionHandler : AsyncSessionlessHandler, IRequiresSessionState {
        internal AsyncSessionHandler(ServerProtocol protocol) : base(protocol) { }
    }

    class CompletedAsyncResult : IAsyncResult {
        object asyncState;
        bool completedSynchronously;

        internal CompletedAsyncResult(object asyncState, bool completedSynchronously) {
            this.asyncState = asyncState;
            this.completedSynchronously = completedSynchronously;
        }

        public object AsyncState { get { return asyncState; } }
        public bool CompletedSynchronously { get { return completedSynchronously; } }
        public bool IsCompleted { get { return true; } }
        public WaitHandle AsyncWaitHandle { get { return null; } }
    }
}
