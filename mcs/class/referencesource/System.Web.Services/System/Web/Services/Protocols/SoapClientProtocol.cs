//------------------------------------------------------------------------------
// <copyright file="SoapClientProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Text;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Diagnostics;
    using System.Xml.Schema;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.Services.Configuration;
    using System.Net;
    using System.Security.Permissions;
    using System.Threading;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Web.Services.Diagnostics;

    internal class SoapClientType {
        Hashtable methods = new Hashtable();
        WebServiceBindingAttribute binding;

        internal SoapReflectedExtension[] HighPriExtensions;
        internal SoapReflectedExtension[] LowPriExtensions;
        internal object[] HighPriExtensionInitializers;
        internal object[] LowPriExtensionInitializers;

        internal string serviceNamespace;
        internal bool serviceDefaultIsEncoded;

        internal SoapClientType(Type type) {
            this.binding = WebServiceBindingReflector.GetAttribute(type);
            if (this.binding == null) throw new InvalidOperationException(Res.GetString(Res.WebClientBindingAttributeRequired));
            // Note: Service namespace is taken from WebserviceBindingAttribute and not WebserviceAttribute because
            // the generated proxy does not have a WebServiceAttribute; however all have a WebServiceBindingAttribute. 
            serviceNamespace = binding.Namespace;
            serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
            ArrayList soapMethodList = new ArrayList();
            ArrayList mappings = new ArrayList();
            GenerateXmlMappings(type, soapMethodList, serviceNamespace, serviceDefaultIsEncoded, mappings);
            XmlMapping[] xmlMappings = (XmlMapping[])mappings.ToArray(typeof(XmlMapping));

            TraceMethod caller = Tracing.On ? new TraceMethod(this, ".ctor", type) : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceCreateSerializer), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", xmlMappings, type));
            XmlSerializer[] serializers = XmlSerializer.FromMappings(xmlMappings, type);
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceCreateSerializer), caller);

            SoapExtensionTypeElementCollection extensionTypes = WebServicesSection.Current.SoapExtensionTypes;
            ArrayList highPri = new ArrayList();
            ArrayList lowPri = new ArrayList();
            for (int i = 0; i < extensionTypes.Count; i++) {
                SoapExtensionTypeElement element = extensionTypes[i];
                SoapReflectedExtension extension = new SoapReflectedExtension(extensionTypes[i].Type, null, extensionTypes[i].Priority);
                if (extensionTypes[i].Group == PriorityGroup.High)
                    highPri.Add(extension);
                else
                    lowPri.Add(extension);
            }

            HighPriExtensions = (SoapReflectedExtension[]) highPri.ToArray(typeof(SoapReflectedExtension));
            LowPriExtensions = (SoapReflectedExtension[]) lowPri.ToArray(typeof(SoapReflectedExtension));
            Array.Sort(HighPriExtensions);
            Array.Sort(LowPriExtensions);
            HighPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, HighPriExtensions);
            LowPriExtensionInitializers = SoapReflectedExtension.GetInitializers(type, LowPriExtensions);

            int count = 0;
            for (int i = 0; i < soapMethodList.Count; i++) {
                SoapReflectedMethod soapMethod = (SoapReflectedMethod)soapMethodList[i];
                SoapClientMethod clientMethod = new SoapClientMethod();
                clientMethod.parameterSerializer = serializers[count++]; 
                if (soapMethod.responseMappings != null) clientMethod.returnSerializer = serializers[count++];
                clientMethod.inHeaderSerializer = serializers[count++];
                if (soapMethod.outHeaderMappings != null) clientMethod.outHeaderSerializer = serializers[count++];
                clientMethod.action = soapMethod.action;
                clientMethod.oneWay = soapMethod.oneWay;
                clientMethod.rpc = soapMethod.rpc;
                clientMethod.use = soapMethod.use;
                clientMethod.paramStyle = soapMethod.paramStyle;
                clientMethod.methodInfo = soapMethod.methodInfo;
                clientMethod.extensions = soapMethod.extensions;
                clientMethod.extensionInitializers = SoapReflectedExtension.GetInitializers(clientMethod.methodInfo, soapMethod.extensions);
                ArrayList inHeaders = new ArrayList();
                ArrayList outHeaders = new ArrayList();
                for (int j = 0; j < soapMethod.headers.Length; j++) {
                    SoapHeaderMapping mapping = new SoapHeaderMapping();
                    SoapReflectedHeader soapHeader = soapMethod.headers[j];
                    mapping.memberInfo = soapHeader.memberInfo;
                    mapping.repeats = soapHeader.repeats;
                    mapping.custom = soapHeader.custom;
                    mapping.direction = soapHeader.direction;
                    mapping.headerType = soapHeader.headerType;
                    if ((mapping.direction & SoapHeaderDirection.In) != 0)
                        inHeaders.Add(mapping);
                    if ((mapping.direction & (SoapHeaderDirection.Out | SoapHeaderDirection.Fault)) != 0)
                        outHeaders.Add(mapping);
                }
                clientMethod.inHeaderMappings = (SoapHeaderMapping[])inHeaders.ToArray(typeof(SoapHeaderMapping));
                if (clientMethod.outHeaderSerializer != null)
                    clientMethod.outHeaderMappings = (SoapHeaderMapping[])outHeaders.ToArray(typeof(SoapHeaderMapping));
                methods.Add(soapMethod.name, clientMethod);
            }
        }

        internal static void GenerateXmlMappings(Type type, ArrayList soapMethodList, string serviceNamespace, bool serviceDefaultIsEncoded, ArrayList mappings) {
            LogicalMethodInfo[] methodInfos = LogicalMethodInfo.Create(type.GetMethods(BindingFlags.Public | BindingFlags.Instance), LogicalMethodTypes.Sync);
           
            SoapReflectionImporter soapImporter = SoapReflector.CreateSoapImporter(serviceNamespace, serviceDefaultIsEncoded);
            XmlReflectionImporter xmlImporter = SoapReflector.CreateXmlImporter(serviceNamespace, serviceDefaultIsEncoded);
            WebMethodReflector.IncludeTypes(methodInfos, xmlImporter);
            SoapReflector.IncludeTypes(methodInfos, soapImporter);
 
 
            for (int i = 0; i < methodInfos.Length; i++) {
                LogicalMethodInfo methodInfo = methodInfos[i];
                SoapReflectedMethod soapMethod = SoapReflector.ReflectMethod(methodInfo, true, xmlImporter, soapImporter, serviceNamespace);
                if (soapMethod == null) continue;
                soapMethodList.Add(soapMethod);
                mappings.Add(soapMethod.requestMappings);
                if (soapMethod.responseMappings != null) mappings.Add(soapMethod.responseMappings);
                mappings.Add(soapMethod.inHeaderMappings);
                if (soapMethod.outHeaderMappings != null) mappings.Add(soapMethod.outHeaderMappings);
            }
        }

        internal SoapClientMethod GetMethod(string name) {
            return (SoapClientMethod)methods[name];
        }

        internal WebServiceBindingAttribute Binding {
            get { return binding; }
        }
    }

    internal class SoapClientMethod {
        internal XmlSerializer returnSerializer;
        internal XmlSerializer parameterSerializer;
        internal XmlSerializer inHeaderSerializer;
        internal XmlSerializer outHeaderSerializer;
        internal string action;
        internal LogicalMethodInfo methodInfo;
        internal SoapHeaderMapping[] inHeaderMappings;
        internal SoapHeaderMapping[] outHeaderMappings;
        internal SoapReflectedExtension[] extensions;
        internal object[] extensionInitializers;
        internal bool oneWay;
        internal bool rpc;
        internal SoapBindingUse use;
        internal SoapParameterStyle paramStyle;
    }

    /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol"]/*' />
    /// <devdoc>
    ///    <para>
    ///       Specifies most of the implementation for communicating with a SOAP web service over HTTP.
    ///    </para>
    /// </devdoc>
    [ComVisible(true)]
    public class SoapHttpClientProtocol : HttpWebClientProtocol {
        SoapClientType clientType;
        SoapProtocolVersion version = SoapProtocolVersion.Default;

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.SoapHttpClientProtocol"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Services.Protocols.SoapHttpClientProtocol'/> class.
        ///    </para>
        /// </devdoc>
        public SoapHttpClientProtocol() 
            : base() {
            Type type = this.GetType();
            clientType = (SoapClientType)GetFromCache(type);
            if (clientType == null) {
                lock (InternalSyncObject) {
                    clientType = (SoapClientType)GetFromCache(type);
                    if (clientType == null) {
                        clientType = new SoapClientType(type);
                        AddToCache(type, clientType);
                    }
                }
            }
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.Discover"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Discover() {
            if (clientType.Binding == null)
                throw new InvalidOperationException(Res.GetString(Res.DiscoveryIsNotPossibleBecauseTypeIsMissing1, this.GetType().FullName));
            DiscoveryClientProtocol disco = new DiscoveryClientProtocol(this);            
            DiscoveryDocument doc = disco.Discover(Url);
            foreach (object item in doc.References) {
                System.Web.Services.Discovery.SoapBinding soapBinding = item as System.Web.Services.Discovery.SoapBinding;
                if (soapBinding != null) {
                    if (clientType.Binding.Name == soapBinding.Binding.Name &&
                        clientType.Binding.Namespace == soapBinding.Binding.Namespace) {
                        Url = soapBinding.Address;
                        return;
                    }
                }
            }
            throw new InvalidOperationException(Res.GetString(Res.TheBindingNamedFromNamespaceWasNotFoundIn3, clientType.Binding.Name, clientType.Binding.Namespace, Url));
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.GetWebRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override WebRequest GetWebRequest(Uri uri) {            
            WebRequest request = base.GetWebRequest(uri);            
            return request;
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.SoapVersion"]/*' />
        [DefaultValue(SoapProtocolVersion.Default), WebServicesDescription(Res.ClientProtocolSoapVersion), ComVisible(false)]
        public SoapProtocolVersion SoapVersion {
            get { return version; }
            set { version = value; }
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.GetWriterForMessage"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Allows to intercept XmlWriter creation.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand | SecurityAction.InheritanceDemand, Name = "FullTrust")]
        protected virtual XmlWriter GetWriterForMessage(SoapClientMessage message, int bufferSize) {
            if (bufferSize < 512)
                bufferSize = 512;
            XmlTextWriter writer = new XmlTextWriter(new StreamWriter(message.Stream, RequestEncoding != null ? RequestEncoding : new UTF8Encoding(false), bufferSize));
            /*
            if (RequestEncoding != null && RequestEncoding.GetType() != typeof(UTF8Encoding)) {
                writer = new XmlTextWriter(new StreamWriter(message.Stream, RequestEncoding, bufferSize));
            }
            else {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Encoding = new UTF8Encoding(false);
                ws.Indent = false;
                ws.NewLineHandling = NewLineHandling.None;
                writer = XmlWriter.Create(message.Stream, ws);
            }
            */
            return writer;
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.GetReaderForMessage"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Allows to intercept XmlReader creation.
        ///    </para>
        /// </devdoc>
        
        [PermissionSet(SecurityAction.LinkDemand | SecurityAction.InheritanceDemand, Name = "FullTrust")]
        protected virtual XmlReader GetReaderForMessage(SoapClientMessage message, int bufferSize) {
            Encoding enc = message.SoapVersion == SoapProtocolVersion.Soap12 ? RequestResponseUtils.GetEncoding2(message.ContentType) : RequestResponseUtils.GetEncoding(message.ContentType);
            if (bufferSize < 512)
                bufferSize = 512;
            XmlTextReader reader;
            if (enc != null)
                reader = new XmlTextReader(new StreamReader(message.Stream, enc, true, bufferSize));
            else
                // 
                reader = new XmlTextReader(message.Stream);

            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.Normalization = true;
            reader.XmlResolver = null;

            return reader;
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.Invoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Invokes a method of a SOAP web service.
        ///    </para>
        /// </devdoc>
        protected object[] Invoke(string methodName, object[] parameters) {
            WebResponse response = null;                      
            WebRequest request = null;
            try {
                request = GetWebRequest(Uri);
                NotifyClientCallOut(request);
                // 
                PendingSyncRequest = request;
                SoapClientMessage message = BeforeSerialize(request, methodName, parameters);            
                Stream requestStream = request.GetRequestStream();            
                try {                                
                    message.SetStream(requestStream);
                    Serialize(message);           
                }                        
                finally {
                    requestStream.Close();
                }

                response = GetWebResponse(request);
                Stream responseStream = null;
                try {
                    responseStream = response.GetResponseStream();
                    return ReadResponse(message, response, responseStream, false);
                }
                catch (XmlException e) {
                    throw new InvalidOperationException(Res.GetString(Res.WebResponseBadXml), e);
                }
                finally {
                    if (responseStream != null)
                        responseStream.Close();
                }
            }
            finally {
                if (request == PendingSyncRequest)
                    PendingSyncRequest = null;
            }
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.BeginInvoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Starts an asynchronous invocation of a method of a SOAP web
        ///       service.
        ///    </para>
        /// </devdoc>
        protected IAsyncResult BeginInvoke(string methodName, object[] parameters, AsyncCallback callback, object asyncState) {
            InvokeAsyncState invokeState = new InvokeAsyncState(methodName, parameters);
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, invokeState, null, callback, asyncState);
            return BeginSend(Uri, asyncResult, true);
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.InitializeAsyncRequest"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal override void InitializeAsyncRequest(WebRequest request, object internalAsyncState) {
            InvokeAsyncState invokeState = (InvokeAsyncState)internalAsyncState;
            invokeState.Message = BeforeSerialize(request, invokeState.MethodName, invokeState.Parameters);            
        }

        internal override void AsyncBufferedSerialize(WebRequest request, Stream requestStream, object internalAsyncState) {
            InvokeAsyncState invokeState = (InvokeAsyncState)internalAsyncState;
            SoapClientMessage message = invokeState.Message;
            message.SetStream(requestStream);
            Serialize(invokeState.Message);
        }

        class InvokeAsyncState {
            public string MethodName;
            public object[] Parameters;
            public SoapClientMessage Message;

            public InvokeAsyncState(string methodName, object[] parameters) {
                this.MethodName = methodName;
                this.Parameters = parameters;
            }
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapHttpClientProtocol.EndInvoke"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Ends an asynchronous invocation of a method of a remote SOAP web service.
        ///    </para>
        /// </devdoc>
        protected object[] EndInvoke(IAsyncResult asyncResult) {
            object o = null;
            Stream responseStream = null;
            try {
                WebResponse response = EndSend(asyncResult, ref o, ref responseStream);
                InvokeAsyncState invokeState = (InvokeAsyncState)o;
                return ReadResponse(invokeState.Message, response, responseStream, true);
            }
            catch (XmlException e) {
                throw new InvalidOperationException(Res.GetString(Res.WebResponseBadXml), e);
            }
            finally {
                if (responseStream != null)
                    responseStream.Close();
            }
        }

        private void InvokeAsyncCallback(IAsyncResult result) {
            object[] parameters = null;
            Exception exception = null;
    
            WebClientAsyncResult asyncResult = (WebClientAsyncResult)result;
            if (asyncResult.Request != null) {
                object o = null;
                Stream responseStream = null;
                try {
                    WebResponse response = EndSend(asyncResult, ref o, ref responseStream);
                    InvokeAsyncState invokeState = (InvokeAsyncState)o;
                    parameters = ReadResponse(invokeState.Message, response, responseStream, true);
                } 
                catch (XmlException e) {
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsyncCallback", e);
                    exception = new InvalidOperationException(Res.GetString(Res.WebResponseBadXml), e);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                        throw;
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsyncCallback", e);
                    exception = e;
                }
                finally {
                    if (responseStream != null)
                        responseStream.Close();
                }
            }
            AsyncOperation asyncOp = (AsyncOperation)result.AsyncState;
            UserToken token = (UserToken)asyncOp.UserSuppliedState;
            OperationCompleted(token.UserState, parameters, exception, false);
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapClientProtocol.InvokeAsync"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void InvokeAsync(string methodName, object[] parameters, SendOrPostCallback callback) {
            InvokeAsync(methodName, parameters, callback, null);
        }

        /// <include file='doc\SoapClientProtocol.uex' path='docs/doc[@for="SoapClientProtocol.InvokeAsync1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void InvokeAsync(string methodName, object[] parameters, SendOrPostCallback callback, object userState) {
            if (userState == null)
                userState = NullToken;
            InvokeAsyncState invokeState = new InvokeAsyncState(methodName, parameters);
            AsyncOperation asyncOp = AsyncOperationManager.CreateOperation(new UserToken(callback, userState));
            WebClientAsyncResult asyncResult = new WebClientAsyncResult(this, invokeState, null, new AsyncCallback(InvokeAsyncCallback), asyncOp);
            try {
                AsyncInvokes.Add(userState, asyncResult);
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsync", e);
                Exception exception = new ArgumentException(Res.GetString(Res.AsyncDuplicateUserState), e);
                InvokeCompletedEventArgs eventArgs = new InvokeCompletedEventArgs(new object[] { null }, exception, false, userState);
                asyncOp.PostOperationCompleted(callback, eventArgs);
                return;
            }
            try {
                BeginSend(Uri, asyncResult, true);
            } 
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException)
                    throw;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "InvokeAsync", e);
                OperationCompleted(userState, new object[] { null }, e, false);
            }
        }

        private static Array CombineExtensionsHelper(Array array1, Array array2, Array array3, Type elementType) {
            int length = array1.Length + array2.Length + array3.Length;
            if (length == 0)
                return null;
            Array result = null;
            if (elementType == typeof(SoapReflectedExtension))
                result = new SoapReflectedExtension[length];
            else if (elementType == typeof(object))
                result  = new object[length];
            else
                throw new ArgumentException(Res.GetString(Res.ElementTypeMustBeObjectOrSoapReflectedException), "elementType");
            
            int pos = 0;
            Array.Copy(array1, 0, result, pos, array1.Length);
            pos += array1.Length;
            Array.Copy(array2, 0, result, pos, array2.Length);
            pos += array2.Length;
            Array.Copy(array3, 0, result, pos, array3.Length);
            return result;
        }

        private string EnvelopeNs {
            get { 
                return this.version == SoapProtocolVersion.Soap12 ? Soap12.Namespace : Soap.Namespace; 
            }
        }

        private string EncodingNs {
            get { 
                return this.version == SoapProtocolVersion.Soap12 ? Soap12.Encoding : Soap.Encoding; 
            }
        }

        private string HttpContentType {
            get { 
                return this.version == SoapProtocolVersion.Soap12 ? ContentType.ApplicationSoap : ContentType.TextXml; 
            }
        }

        SoapClientMessage BeforeSerialize(WebRequest request, string methodName, object[] parameters) {
            if (parameters == null) throw new ArgumentNullException("parameters");
            SoapClientMethod method = clientType.GetMethod(methodName);
            if (method == null) throw new ArgumentException(Res.GetString(Res.WebInvalidMethodName, methodName));            

            // Run BeforeSerialize extension pass. Extensions are not allowed
            // to write into the stream during this pass.
            SoapReflectedExtension[] allExtensions = (SoapReflectedExtension[])CombineExtensionsHelper(clientType.HighPriExtensions, method.extensions, clientType.LowPriExtensions, typeof(SoapReflectedExtension));
            object[] allExtensionInitializers = (object[])CombineExtensionsHelper(clientType.HighPriExtensionInitializers, method.extensionInitializers, clientType.LowPriExtensionInitializers, typeof(object));
            SoapExtension[] initializedExtensions = SoapMessage.InitializeExtensions(allExtensions, allExtensionInitializers);
            SoapClientMessage message = new SoapClientMessage(this, method, Url);
            message.initializedExtensions = initializedExtensions;
            if (initializedExtensions != null)
                message.SetExtensionStream(new SoapExtensionStream());
            message.InitExtensionStreamChain(message.initializedExtensions);            

            string soapAction = UrlEncoder.EscapeString(method.action, Encoding.UTF8);
            message.SetStage(SoapMessageStage.BeforeSerialize);
            if (this.version == SoapProtocolVersion.Soap12)
                message.ContentType = ContentType.Compose(ContentType.ApplicationSoap, RequestEncoding != null ? RequestEncoding : Encoding.UTF8, soapAction);
            else
                message.ContentType = ContentType.Compose(ContentType.TextXml, RequestEncoding != null ? RequestEncoding : Encoding.UTF8);
            message.SetParameterValues(parameters);
            SoapHeaderHandling.GetHeaderMembers(message.Headers, this, method.inHeaderMappings, SoapHeaderDirection.In, true);
            message.RunExtensions(message.initializedExtensions, true);

            // Last chance to set request headers            
            request.ContentType = message.ContentType;
            if (message.ContentEncoding != null && message.ContentEncoding.Length > 0)
                request.Headers[ContentType.ContentEncoding] = message.ContentEncoding;

            request.Method = "POST";
            if (this.version != SoapProtocolVersion.Soap12 && request.Headers[Soap.Action] == null) {
                StringBuilder actionStringBuilder = new StringBuilder(soapAction.Length + 2);            
                actionStringBuilder.Append('"');
                actionStringBuilder.Append(soapAction);
                actionStringBuilder.Append('"');
                request.Headers.Add(Soap.Action, actionStringBuilder.ToString());                
            }

            return message;
        }

        void Serialize(SoapClientMessage message) {
            Stream stream = message.Stream;            
            SoapClientMethod method = message.Method;
            bool isEncoded = method.use == SoapBindingUse.Encoded;

            // Serialize the message.  
            string envelopeNs = EnvelopeNs;
            string encodingNs = EncodingNs;

            XmlWriter writer = GetWriterForMessage(message, 1024);
            if (writer == null)
                throw new InvalidOperationException(Res.GetString(Res.WebNullWriterForMessage));

            writer.WriteStartDocument();
            writer.WriteStartElement(Soap.Prefix, Soap.Element.Envelope, envelopeNs);
            writer.WriteAttributeString("xmlns", Soap.Prefix, null, envelopeNs);
            if (isEncoded) {
                writer.WriteAttributeString("xmlns", "soapenc", null, encodingNs);
                writer.WriteAttributeString("xmlns", "tns", null, clientType.serviceNamespace);
                writer.WriteAttributeString("xmlns", "types", null, SoapReflector.GetEncodedNamespace(clientType.serviceNamespace, clientType.serviceDefaultIsEncoded));
            }
            writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
            writer.WriteAttributeString("xmlns", "xsd", null, XmlSchema.Namespace);
            SoapHeaderHandling.WriteHeaders(writer, method.inHeaderSerializer, message.Headers, method.inHeaderMappings, SoapHeaderDirection.In, isEncoded, clientType.serviceNamespace, clientType.serviceDefaultIsEncoded, envelopeNs);
            writer.WriteStartElement(Soap.Element.Body, envelopeNs);
            if (isEncoded && version != SoapProtocolVersion.Soap12) // don't write encodingStyle on soap:Body for soap 1.2
                writer.WriteAttributeString("soap", Soap.Attribute.EncodingStyle, null, encodingNs);

            object[] parameters = message.GetParameterValues();
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "Serialize") : null;

            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceWriteRequest), caller, new TraceMethod(method.parameterSerializer, "Serialize", writer, parameters, null, isEncoded ? encodingNs : null));
            method.parameterSerializer.Serialize(writer, parameters, null, isEncoded ? encodingNs : null);
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceWriteRequest), caller);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();

            // run the after serialize extension pass. 
            message.SetStage(SoapMessageStage.AfterSerialize);
            message.RunExtensions(message.initializedExtensions, true);
        }

        object[] ReadResponse(SoapClientMessage message, WebResponse response, Stream responseStream, bool asyncCall) {
            SoapClientMethod method = message.Method;

            // 


            HttpWebResponse httpResponse = response as HttpWebResponse;
            int statusCode = httpResponse != null ? (int)httpResponse.StatusCode : -1;
            if (statusCode >= 300 && statusCode != 500 && statusCode != 400)
                throw new WebException(RequestResponseUtils.CreateResponseExceptionString(httpResponse, responseStream), null, 
                    WebExceptionStatus.ProtocolError, httpResponse);

            message.Headers.Clear();
            message.SetStream(responseStream);
            message.InitExtensionStreamChain(message.initializedExtensions);

            message.SetStage(SoapMessageStage.BeforeDeserialize);
            message.ContentType = response.ContentType;
            message.ContentEncoding = response.Headers[ContentType.ContentEncoding];
            message.RunExtensions(message.initializedExtensions, false);

            if (method.oneWay && (httpResponse == null || (int)httpResponse.StatusCode != 500)) {
                return new object[0];
            }

            // this statusCode check is just so we don't repeat the contentType check we did above
            bool isSoap = ContentType.IsSoap(message.ContentType);
            if (!isSoap || (isSoap && (httpResponse != null) && (httpResponse.ContentLength == 0))) {
                // special-case 400 since we exempted it above on the off-chance it might be a soap 1.2 sender fault. 
                // based on the content-type, it looks like it's probably just a regular old 400
                if (statusCode == 400) 
                    throw new WebException(RequestResponseUtils.CreateResponseExceptionString(httpResponse, responseStream), null, 
                        WebExceptionStatus.ProtocolError, httpResponse);
                else
                    throw new InvalidOperationException(Res.GetString(Res.WebResponseContent, message.ContentType, HttpContentType) +
                                    Environment.NewLine +
                                    RequestResponseUtils.CreateResponseExceptionString(response, responseStream));
            }
            if (message.Exception != null) {
                throw message.Exception;
            }

            // perf fix: changed buffer size passed to StreamReader
            int bufferSize;
            if (asyncCall || httpResponse == null)
                bufferSize = 512;
            else {
                bufferSize = RequestResponseUtils.GetBufferSize((int)httpResponse.ContentLength);
            }
            XmlReader reader = GetReaderForMessage(message, bufferSize);
            if (reader == null)
                throw new InvalidOperationException(Res.GetString(Res.WebNullReaderForMessage));

            reader.MoveToContent();
            int depth = reader.Depth;

            // should be able to handle no ns, soap 1.1 ns, or soap 1.2 ns
            string encodingNs = EncodingNs;
            string envelopeNs = reader.NamespaceURI;

            if (envelopeNs == null || envelopeNs.Length == 0)
                // ok to omit namespace -- assume correct version
                reader.ReadStartElement(Soap.Element.Envelope);
            else if (reader.NamespaceURI == Soap.Namespace)
                reader.ReadStartElement(Soap.Element.Envelope, Soap.Namespace);
            else if (reader.NamespaceURI == Soap12.Namespace)
                reader.ReadStartElement(Soap.Element.Envelope, Soap12.Namespace);
            else
                throw new SoapException(Res.GetString(Res.WebInvalidEnvelopeNamespace, envelopeNs, EnvelopeNs), SoapException.VersionMismatchFaultCode);

            reader.MoveToContent();
            SoapHeaderHandling headerHandler = new SoapHeaderHandling();
            headerHandler.ReadHeaders(reader, method.outHeaderSerializer, message.Headers, method.outHeaderMappings, SoapHeaderDirection.Out | SoapHeaderDirection.Fault, envelopeNs, method.use == SoapBindingUse.Encoded ? encodingNs : null, false);
            reader.MoveToContent();
            reader.ReadStartElement(Soap.Element.Body, envelopeNs);
            reader.MoveToContent();
            if (reader.IsStartElement(Soap.Element.Fault, envelopeNs)) {
                message.Exception = ReadSoapException(reader);
            } 
            else {
                if (method.oneWay) {
                    reader.Skip();
                    message.SetParameterValues(new object[0]);
                }
                else {
                    TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadResponse") : null;
                    bool isEncodedSoap = method.use == SoapBindingUse.Encoded;
                    if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceReadResponse), caller, new TraceMethod(method.returnSerializer, "Deserialize", reader, isEncodedSoap ? encodingNs : null));

                    bool useDeserializationEvents = !isEncodedSoap && (WebServicesSection.Current.SoapEnvelopeProcessing.IsStrict || Tracing.On);
                    if (useDeserializationEvents) {
                        XmlDeserializationEvents events = Tracing.On ? Tracing.GetDeserializationEvents() : RuntimeUtils.GetDeserializationEvents();
                        message.SetParameterValues((object[])method.returnSerializer.Deserialize(reader, null, events));
                    }
                    else {
                        message.SetParameterValues((object[])method.returnSerializer.Deserialize(reader, isEncodedSoap ? encodingNs : null));
                    }

                    if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceReadResponse), caller);
                }
            }

            // Consume soap:Body and soap:Envelope closing tags
            while (depth < reader.Depth && reader.Read()) {
                // Nothing, just read on
            }
            // consume end tag
            if (reader.NodeType == XmlNodeType.EndElement) {
                reader.Read();
            }

            message.SetStage(SoapMessageStage.AfterDeserialize);
            message.RunExtensions(message.initializedExtensions, false);
            SoapHeaderHandling.SetHeaderMembers(message.Headers, this, method.outHeaderMappings, SoapHeaderDirection.Out | SoapHeaderDirection.Fault, true);

            if (message.Exception != null) throw message.Exception;
            return message.GetParameterValues();        
        }

        SoapException ReadSoapException(XmlReader reader) {
            XmlQualifiedName faultCode = XmlQualifiedName.Empty;
            string faultString = null;
            string faultActor = null;
            string faultRole = null;
            XmlNode detail = null;
            SoapFaultSubCode subcode = null;
            string lang = null;
            bool soap12 = (reader.NamespaceURI == Soap12.Namespace);
            if (reader.IsEmptyElement) {
                reader.Skip();
            }
            else {
                reader.ReadStartElement();
                reader.MoveToContent();
                int depth = reader.Depth;
                while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.None) {
                    if (reader.NamespaceURI == Soap.Namespace || reader.NamespaceURI == Soap12.Namespace || reader.NamespaceURI == null || reader.NamespaceURI.Length == 0) {
                        if (reader.LocalName == Soap.Element.FaultCode || reader.LocalName == Soap12.Element.FaultCode) {
                            if (soap12)
                                faultCode = ReadSoap12FaultCode(reader, out subcode);
                            else
                                faultCode = ReadFaultCode(reader);
                        }
                        else if (reader.LocalName == Soap.Element.FaultString) {
                            lang = reader.GetAttribute(Soap.Attribute.Lang, Soap.XmlNamespace);
                            reader.MoveToElement();
                            faultString = reader.ReadElementString();
                        }
                        else if (reader.LocalName == Soap12.Element.FaultReason) {
                            if (reader.IsEmptyElement)
                                reader.Skip();
                            else {
                                reader.ReadStartElement(); // consume Reason element to get to Text child
                                reader.MoveToContent();
                                while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.None) {
                                    if (reader.LocalName == Soap12.Element.FaultReasonText && reader.NamespaceURI == Soap12.Namespace) {
                                        faultString = reader.ReadElementString();
                                    }
                                    else {
                                        reader.Skip();
                                    }
                                    reader.MoveToContent();
                                }
                                while (reader.NodeType == XmlNodeType.Whitespace) reader.Skip();
                                if (reader.NodeType == XmlNodeType.None) reader.Skip();
                                else reader.ReadEndElement();
                            }
                        }
                        else if (reader.LocalName == Soap.Element.FaultActor || reader.LocalName == Soap12.Element.FaultNode) {
                            faultActor = reader.ReadElementString();
                        }
                        else if (reader.LocalName == Soap.Element.FaultDetail || reader.LocalName == Soap12.Element.FaultDetail) {
                            detail = new XmlDocument().ReadNode(reader);
                        }
                        else if (reader.LocalName == Soap12.Element.FaultRole) {
                            faultRole = reader.ReadElementString();
                        }
                        else {
                            reader.Skip();
                        }
                    }
                    else {
                        reader.Skip();
                    }
                    reader.MoveToContent();
                }
                // Consume soap:Body and soap:Envelope closing tags
                while (reader.Read() && depth < reader.Depth) {
                    // Nothing, just read on
                }
                // consume end tag
                if (reader.NodeType == XmlNodeType.EndElement) {
                    reader.Read();
                }
            }
            if (detail != null || soap12) // with soap 1.2, can't tell if fault is for header
                return new SoapException(faultString, faultCode, faultActor, faultRole, lang, detail, subcode, null);
            else
                return new SoapHeaderException(faultString, faultCode, faultActor, faultRole, lang, subcode, null);
        }

        private XmlQualifiedName ReadSoap12FaultCode(XmlReader reader, out SoapFaultSubCode subcode) {
            SoapFaultSubCode code = ReadSoap12FaultCodesRecursive(reader, 0);
            if (code == null) {
                subcode = null;
                return null;
            }
            else {
                subcode = code.SubCode;
                return code.Code;
            }
        }
         
        private SoapFaultSubCode ReadSoap12FaultCodesRecursive(XmlReader reader, int depth) {
            if (depth > 100) return null;
            if (reader.IsEmptyElement) {
                reader.Skip();
                return null;
            }
            XmlQualifiedName code = null;
            SoapFaultSubCode subcode = null;
            int faultDepth = reader.Depth;
            reader.ReadStartElement();
            reader.MoveToContent();
            while (reader.NodeType != XmlNodeType.EndElement && reader.NodeType != XmlNodeType.None) {
                if (reader.NamespaceURI == Soap12.Namespace || reader.NamespaceURI == null || reader.NamespaceURI.Length == 0) {
                    if (reader.LocalName == Soap12.Element.FaultCodeValue) {
                        code = ReadFaultCode(reader);
                    }
                    else if (reader.LocalName == Soap12.Element.FaultSubcode) {
                        subcode = ReadSoap12FaultCodesRecursive(reader, depth + 1);
                    }
                    else {
                        reader.Skip();
                    }
                }
                else {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
            // Consume closing tag
            while (faultDepth < reader.Depth && reader.Read()) {
                // Nothing, just read on
            }
            // consume end tag
            if (reader.NodeType == XmlNodeType.EndElement) {
                reader.Read();
            }

            return new SoapFaultSubCode(code, subcode);
        }

        private XmlQualifiedName ReadFaultCode(XmlReader reader) {
            if (reader.IsEmptyElement) {
                reader.Skip();
                return null;
            }
            reader.ReadStartElement();
            string qnameValue = reader.ReadString();
            int colon = qnameValue.IndexOf(":", StringComparison.Ordinal);
            string ns = reader.NamespaceURI;
            if (colon >= 0) {
                string prefix = qnameValue.Substring(0, colon);
                ns = reader.LookupNamespace(prefix);
                if (ns == null)
                    throw new InvalidOperationException(Res.GetString(Res.WebQNamePrefixUndefined, prefix));
            }
            reader.ReadEndElement();
            
            return new XmlQualifiedName(qnameValue.Substring(colon + 1), ns);
        }
    }
}


