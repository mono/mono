//------------------------------------------------------------------------------
// <copyright file="WebServiceHandlerFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    //using System.Reflection;
    using System.Web.UI;
    using System.ComponentModel; // for CompModSwitches
    using System.IO;
    using System.Web.Services.Configuration;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\WebServiceHandlerFactory.uex' path='docs/doc[@for="WebServiceHandlerFactory"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class WebServiceHandlerFactory : IHttpHandlerFactory {
        /*
        static WebServiceHandlerFactory() {            
            Stream stream = new FileStream("c:\\out.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite); //(FileMode.OpenOrCreate);            
            TraceListener listener = new TextWriterTraceListener(stream);            
            Debug.AutoFlush = true;            
            Debug.Listeners.Add(listener);            
            Debug.WriteLine("--------------");            
        }
        */

#if DEBUG
        void DumpRequest(HttpContext context) {
            HttpRequest request = context.Request;
            Debug.WriteLine("Process Request called.");
            Debug.WriteLine("Path = " + request.Path);
            Debug.WriteLine("PhysicalPath = " + request.PhysicalPath);
            Debug.WriteLine("Query = " + request.Url.Query);
            Debug.WriteLine("HttpMethod = " + request.HttpMethod);
            Debug.WriteLine("ContentType = " + request.ContentType);
            Debug.WriteLine("PathInfo = " + request.PathInfo);
            Debug.WriteLine("----Http request headers: ----");
            System.Collections.Specialized.NameValueCollection headers = request.Headers;
            foreach (string name in headers) {
                string value = headers[name];
                if (value != null && value.Length > 0)
                    Debug.WriteLine(name + "=" + headers[name]);
            }                
        }
#endif


        /// <include file='doc\WebServiceHandlerFactory.uex' path='docs/doc[@for="WebServiceHandlerFactory.GetHandler"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public IHttpHandler GetHandler(HttpContext context, string verb, string url, string filePath) {
            TraceMethod method = Tracing.On ? new TraceMethod(this, "GetHandler") : null;
            if (Tracing.On) Tracing.Enter("IHttpHandlerFactory.GetHandler", method, Tracing.Details(context.Request));

            new AspNetHostingPermission(AspNetHostingPermissionLevel.Minimal).Demand();
            //if (CompModSwitches.Remote.TraceVerbose) DumpRequest(context);
            //System.Diagnostics.Debugger.Break();
#if DEBUG
            if (CompModSwitches.Remote.TraceVerbose) DumpRequest(context);
#endif

            Type type = GetCompiledType(url, context);
            IHttpHandler handler = CoreGetHandler(type, context, context.Request, context.Response);

            if (Tracing.On) Tracing.Exit("IHttpHandlerFactory.GetHandler", method);

            return handler;
        }

        // Asserts security permission. 
        // Reason: System.Web.UI.WebServiceParser.GetCompiledType() demands SecurityPermission.
        // Justification: The type returned is only used to get the IHttpHandler.
        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        private Type GetCompiledType(string url, HttpContext context)
        {
            return WebServiceParser.GetCompiledType(url, context);
        }

        internal IHttpHandler CoreGetHandler(Type type, HttpContext context, HttpRequest request, HttpResponse response) {
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "CoreGetHandler") : null;
            ServerProtocolFactory[] protocolFactories = GetServerProtocolFactories();
            ServerProtocol protocol = null;
            bool abort = false;
            for (int i = 0; i < protocolFactories.Length; i++) {
                try {
                    protocol = protocolFactories[i].Create(type, context, request, response, out abort);
                    if ((protocol != null && protocol.GetType() != typeof(UnsupportedRequestProtocol)) || abort)
                        break;
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw Tracing.ExceptionThrow(caller, new InvalidOperationException(Res.GetString(Res.FailedToHandleRequest0), e));
                }
            }

            if (abort)
                return new NopHandler();

            if (protocol == null) {
                if (request.PathInfo != null && request.PathInfo.Length != 0) {
                    throw Tracing.ExceptionThrow(caller, new InvalidOperationException(Res.GetString(Res.WebUnrecognizedRequestFormatUrl,
                        new object[] { request.PathInfo })));
                }
                else {
                    throw Tracing.ExceptionThrow(caller, new InvalidOperationException(Res.GetString(Res.WebUnrecognizedRequestFormat)));
                }
            }
            else if (protocol is UnsupportedRequestProtocol) {
                throw Tracing.ExceptionThrow(caller, new HttpException(((UnsupportedRequestProtocol)protocol).HttpCode, Res.GetString(Res.WebUnrecognizedRequestFormat)));
            }

            bool isAsync = protocol.MethodInfo.IsAsync;
            bool requiresSession = protocol.MethodAttribute.EnableSession;

            if (isAsync) {
                if (requiresSession) {
                    return new AsyncSessionHandler(protocol);
                }
                else {
                    return new AsyncSessionlessHandler(protocol);
                }
            }
            else {
                if (requiresSession) {
                    return new SyncSessionHandler(protocol);
                }
                else {
                    return new SyncSessionlessHandler(protocol);
                }
            }
        }

        // Asserts FullTrust permission.
        // Justification: FullTrust is used only to create the objects of type SoapServerProtocolFactory and for nothing else.
        [PermissionSet(SecurityAction.Assert, Name = "FullTrust")]
        private ServerProtocolFactory[] GetServerProtocolFactories()
        {
            return WebServicesSection.Current.ServerProtocolFactories;
        }

        /// <include file='doc\WebServiceHandlerFactory.uex' path='docs/doc[@for="WebServiceHandlerFactory.ReleaseHandler"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void ReleaseHandler(IHttpHandler handler) {
        }
    }

    internal class UnsupportedRequestProtocol : ServerProtocol {
        int httpCode;

        internal UnsupportedRequestProtocol(int httpCode) {
            this.httpCode = httpCode;
        }
        internal int HttpCode { get { return httpCode; } }

        internal override bool Initialize() { return true; }

        internal override bool IsOneWay {
            get { return false; }
        }

        internal override LogicalMethodInfo MethodInfo {
            get { return null; }
        }

        internal override ServerType ServerType {
            get { return null; }
        }

        internal override object[] ReadParameters() {
            return new object[0];
        }
        internal override void WriteReturns(object[] returnValues, Stream outputStream) { }
        internal override bool WriteException(Exception e, Stream outputStream) { return false; }
    }

    internal class NopHandler : IHttpHandler {

        /// <include file='doc\WebServiceHandlerFactory.uex' path='docs/doc[@for="NopHandler.IsReusable"]/*' />
        /// <devdoc>
        ///      IHttpHandler.IsReusable.
        /// </devdoc>
        public bool IsReusable {
            get { return false; }
        }

        /// <include file='doc\WebServiceHandlerFactory.uex' path='docs/doc[@for="NopHandler.ProcessRequest"]/*' />
        /// <devdoc>
        ///      IHttpHandler.ProcessRequest.
        /// </devdoc>
        public void ProcessRequest(HttpContext context) {
        }

    }
}

