//------------------------------------------------------------------------------
// <copyright file="HttpClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Diagnostics;
    using System.Web.Services.Diagnostics;

    internal class HttpClientMethod {
        internal Type readerType;
        internal object readerInitializer;
        internal Type writerType;
        internal object writerInitializer;
        internal LogicalMethodInfo methodInfo;
    }

    internal class HttpClientType {
        Hashtable methods = new Hashtable();

        internal HttpClientType(Type type) {
            LogicalMethodInfo[] methodInfos = LogicalMethodInfo.Create(type.GetMethods(), LogicalMethodTypes.Sync);

            Hashtable formatterTypes = new Hashtable();
            for (int i = 0; i < methodInfos.Length; i++) {
                LogicalMethodInfo methodInfo = methodInfos[i];
                try {
                    object[] attributes = methodInfo.GetCustomAttributes(typeof(HttpMethodAttribute));
                    if (attributes.Length == 0) continue;
                    HttpMethodAttribute attribute = (HttpMethodAttribute)attributes[0];
                    HttpClientMethod method = new HttpClientMethod();
                    method.readerType = attribute.ReturnFormatter;
                    method.writerType = attribute.ParameterFormatter;
                    method.methodInfo = methodInfo;
                    AddFormatter(formatterTypes, method.readerType, method);
                    AddFormatter(formatterTypes, method.writerType, method);
                    methods.Add(methodInfo.Name, method);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw new InvalidOperationException(Res.GetString(Res.WebReflectionError, methodInfo.DeclaringType.FullName, methodInfo.Name), e);
                }
            }

            foreach (Type t in formatterTypes.Keys) {
                ArrayList list = (ArrayList)formatterTypes[t];
                LogicalMethodInfo[] m = new LogicalMethodInfo[list.Count];
                for (int j = 0; j < list.Count; j++)
                    m[j] = ((HttpClientMethod)list[j]).methodInfo;
                object[] initializers = MimeFormatter.GetInitializers(t, m);
                bool isWriter = typeof(MimeParameterWriter).IsAssignableFrom(t);
                for (int j = 0; j < list.Count; j++) {
                    if (isWriter) {
                        ((HttpClientMethod)list[j]).writerInitializer = initializers[j];
                    }
                    else {
                        ((HttpClientMethod)list[j]).readerInitializer = initializers[j];
                    }
                }
            }
        }

        static void AddFormatter(Hashtable formatterTypes, Type formatterType, HttpClientMethod method) {
            if (formatterType == null) return;
            ArrayList list = (ArrayList)formatterTypes[formatterType];
            if (list == null) {
                list = new ArrayList();
                formatterTypes.Add(formatterType, list);
            }
            list.Add(method);
        }

        internal HttpClientMethod GetMethod(string name) {
            return (HttpClientMethod)methods[name];
        }
    }

    /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Specifies
    ///       most of the implementation for communicating with an HTTP web service over HTTP.
    ///    </para>
    /// </devdoc>
    [ComVisible(true)]
    public abstract class HttpSimpleClientProtocol : HttpWebClientProtocol {
        HttpClientType clientType;

        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.HttpSimpleClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Services.Protocols.HttpSimpleClientProtocol'/> class.
        ///    </para>
        /// </devdoc>
        protected HttpSimpleClientProtocol()
            : base() {
            Type type = this.GetType();
            clientType = (HttpClientType)GetFromCache(type);
            if (clientType == null) {
                lock (InternalSyncObject) {
                    clientType = (HttpClientType)GetFromCache(type);
                    if (clientType == null) {
                        clientType = new HttpClientType(type);
                        AddToCache(type, clientType);
                    }
                }
            }
        }

        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.Invoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Invokes a method of a HTTP web service.
        ///    </para>
        /// </devdoc>
        protected object Invoke(string methodName, string requestUrl, object[] parameters) {
            WebResponse response = null;
            HttpClientMethod method = GetClientMethod(methodName);
            MimeParameterWriter paramWriter = GetParameterWriter(method);                
            Uri requestUri = new Uri(requestUrl);
            if (paramWriter != null) {
                paramWriter.RequestEncoding = RequestEncoding;
                requestUrl = paramWriter.GetRequestUrl(requestUri.AbsoluteUri, parameters);
                requestUri = new Uri(requestUrl, true);
            }
            WebRequest request = null;
            try {
                request = GetWebRequest(requestUri);
                NotifyClientCallOut(request);
                PendingSyncRequest = request;
                if (paramWriter != null) {
                    paramWriter.InitializeRequest(request, parameters);      
                    // 


                    if (paramWriter.UsesWriteRequest) {                        
                        if (parameters.Length == 0)
                            request.ContentLength = 0;
                        else {
                            Stream requestStream = null;
                            try {
                                requestStream = request.GetRequestStream();
                                paramWriter.WriteRequest(requestStream, parameters);
                            }
                            finally {
                                if (requestStream != null) requestStream.Close();
                            }                            
                        }
                    }
                }
                response = GetWebResponse(request);            
                Stream responseStream = null;
                if (response.ContentLength != 0)
                    responseStream = response.GetResponseStream();

                return ReadResponse(method, response, responseStream);
            }
            finally {
                if (request == PendingSyncRequest)
                    PendingSyncRequest = null;
            }
        }


        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.BeginInvoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Starts an asynchronous invocation of a method of a HTTP web service.
        ///    </para>
        /// </devdoc>
        protected IAsyncResult BeginInvoke(string methodName, string requestUrl, object[] parameters, AsyncCallback callback, object asyncState) {
            HttpClientMethod method = GetClientMethod(methodName);
            MimeParameterWriter paramWriter = GetParameterWriter(method);
            Uri requestUri = new Uri(requestUrl);            
            if (paramWriter != null) {
                paramWriter.RequestEncoding = RequestEncoding;
                requestUrl = paramWriter.GetRequestUrl(requestUri.AbsoluteUri, parameters);
                requestUri = new Uri(requestUrl, true);
            }
            InvokeAsyncState invokeState = new InvokeAsyncState(method, paramWriter, parameters);            
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, invokeState, null, callback, asyncState);
            return BeginSend(requestUri, asyncResult, paramWriter.UsesWriteRequest);
        }


        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.InitializeAsyncRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal override void InitializeAsyncRequest(WebRequest request, object internalAsyncState) {
            InvokeAsyncState invokeState = (InvokeAsyncState)internalAsyncState;
            if (invokeState.ParamWriter.UsesWriteRequest && invokeState.Parameters.Length == 0) 
                request.ContentLength = 0;
        }

        internal override void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState) {
            InvokeAsyncState invokeState = (InvokeAsyncState)internalAsyncState;
            if (invokeState.ParamWriter != null) {
                invokeState.ParamWriter.InitializeRequest(request, invokeState.Parameters);
                if (invokeState.ParamWriter.UsesWriteRequest && invokeState.Parameters.Length > 0)
                    invokeState.ParamWriter.WriteRequest(requestStream, invokeState.Parameters);                        
            }
        }

        class InvokeAsyncState {            
            internal object[] Parameters;
            internal MimeParameterWriter ParamWriter;
            internal HttpClientMethod Method;            

            internal InvokeAsyncState(HttpClientMethod method, MimeParameterWriter paramWriter, object[] parameters) {
                this.Method = method;
                this.ParamWriter = paramWriter;
                this.Parameters = parameters;
            }
        }

        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.EndInvoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Ends an asynchronous invocation of a method of a HTTP web service.
        ///    </para>
        /// </devdoc>
        protected object EndInvoke(IAsyncResult asyncResult) {            
            object o = null;
            Stream responseStream = null;
            WebResponse response = EndSend(asyncResult, ref o, ref responseStream);
            InvokeAsyncState invokeState = (InvokeAsyncState) o;
            return ReadResponse(invokeState.Method, response, responseStream);
        }

        private void InvokeAsyncCallback(IAsyncResult result) {
            object parameter = null;
            Exception exception = null;
            WebClientAsyncResult asyncResult = (WebClientAsyncResult)result;
            if (asyncResult.Request != null) {
                try {
                    object o = null;
                    Stream responseStream = null;
                    WebResponse response = EndSend(asyncResult, ref o, ref responseStream);
                    InvokeAsyncState invokeState = (InvokeAsyncState) o;
                    parameter = ReadResponse(invokeState.Method, response, responseStream);
                } 
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                        throw;
                    exception = e;
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsyncCallback", e);
                }
            }
            AsyncOperation asyncOp = (AsyncOperation)result.AsyncState;
            UserToken token = (UserToken)asyncOp.UserSuppliedState;
            OperationCompleted(token.UserState, new object[] { parameter }, exception, false);
        }
        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.InvokeAsync"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void InvokeAsync(string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback) {
            InvokeAsync(methodName, requestUrl, parameters, callback, null);
        }

        /// <include file='doc\HttpClientProtocol.uex' path='docs/doc[@for="HttpSimpleClientProtocol.InvokeAsync1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void InvokeAsync(string methodName, string requestUrl, object[] parameters, SendOrPostCallback callback, object userState) {
            if (userState == null)
                userState = NullToken;
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(new UserToken(callback, userState));
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, null, null, new AsyncCallback(InvokeAsyncCallback), asyncOp);
            try {
                AsyncInvokes.Add(userState, asyncResult);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsync", e);
                Exception exception = new ArgumentException(Res.GetString(Res.AsyncDuplicateUserState), e);
                InvokeCompletedEventArgs eventArgs = new InvokeCompletedEventArgs(new object[] { null }, exception, false, userState);
                asyncOp.PostOperationCompleted(callback, eventArgs);
                return;
            }
            try {
                HttpClientMethod method = GetClientMethod(methodName);
                MimeParameterWriter paramWriter = GetParameterWriter(method);
                Uri requestUri = new Uri(requestUrl);            
                if (paramWriter != null) {
                    paramWriter.RequestEncoding = RequestEncoding;
                    requestUrl = paramWriter.GetRequestUrl(requestUri.AbsoluteUri, parameters);
                    requestUri = new Uri(requestUrl, true);
                }
                asyncResult.InternalAsyncState = new InvokeAsyncState(method, paramWriter, parameters);
                BeginSend(requestUri, asyncResult, paramWriter.UsesWriteRequest);
            } 
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Error, this, "InvokeAsync", e);
                OperationCompleted(userState, new object[] { null }, e, false);
            }
        }

        MimeParameterWriter GetParameterWriter(HttpClientMethod method) {
            if (method.writerType == null)
                return null;
            return (MimeParameterWriter)MimeFormatter.CreateInstance(method.writerType, method.writerInitializer);                
        }

        HttpClientMethod GetClientMethod(string methodName) {
            HttpClientMethod method = clientType.GetMethod(methodName);
            if (method == null) throw new ArgumentException(Res.GetString(Res.WebInvalidMethodName, methodName), "methodName");
            return method;
        }

        object ReadResponse(HttpClientMethod method, WebResponse response, Stream responseStream) {
            HttpWebResponse httpResponse = response as HttpWebResponse;
            if (httpResponse != null && (int)httpResponse.StatusCode >= 300)
                throw new WebException(RequestResponseUtils.CreateResponseExceptionString(httpResponse, responseStream), null, 
                    WebExceptionStatus.ProtocolError, httpResponse);

            if (method.readerType == null)
                return null;

            // 


            if (responseStream != null) {
                MimeReturnReader reader = (MimeReturnReader)MimeFormatter.CreateInstance(method.readerType, method.readerInitializer);
                return reader.Read(response, responseStream);                
            }
            else
                return null;
        }
    }
}
