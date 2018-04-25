//------------------------------------------------------------------------------
// <copyright file="SoapServerProtocol.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Xml.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Web.Services.Description;
    using System.Text;
    using System.Net;
    using System.Web.Services.Configuration;
    using System.Threading;
    using System.Security.Policy;
    using System.Security.Permissions;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class SoapServerType : ServerType {
        Hashtable methods = new Hashtable();
        Hashtable duplicateMethods = new Hashtable();

        internal SoapReflectedExtension[] HighPriExtensions;
        internal SoapReflectedExtension[] LowPriExtensions;
        internal object[] HighPriExtensionInitializers;
        internal object[] LowPriExtensionInitializers;

        internal string serviceNamespace;
        internal bool serviceDefaultIsEncoded;
        internal bool routingOnSoapAction;
        internal WebServiceProtocols protocolsSupported;

        public string ServiceNamespace
        {
            get
            {
                return serviceNamespace;
            }
        }

        public bool ServiceDefaultIsEncoded
        {
            get
            {
                return serviceDefaultIsEncoded;
            }
        }

        public bool ServiceRoutingOnSoapAction
        {
            get
            {
                return routingOnSoapAction;
            }
        }

        public SoapServerType(Type type, WebServiceProtocols protocolsSupported) : base(type) {
            this.protocolsSupported = protocolsSupported;
            bool soap11 = (protocolsSupported & WebServiceProtocols.HttpSoap) != 0;
            bool soap12 = (protocolsSupported & WebServiceProtocols.HttpSoap12) != 0;
            LogicalMethodInfo[] methodInfos = WebMethodReflector.GetMethods(type);
            ArrayList mappings = new ArrayList();
            WebServiceAttribute serviceAttribute = WebServiceReflector.GetAttribute(type);
            object soapServiceAttribute = SoapReflector.GetSoapServiceAttribute(type);
            routingOnSoapAction = SoapReflector.GetSoapServiceRoutingStyle(soapServiceAttribute) == SoapServiceRoutingStyle.SoapAction;
            serviceNamespace = serviceAttribute.Namespace;
            serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(type);
            SoapReflectionImporter soapImporter = SoapReflector.CreateSoapImporter(serviceNamespace, serviceDefaultIsEncoded);
            XmlReflectionImporter xmlImporter = SoapReflector.CreateXmlImporter(serviceNamespace, serviceDefaultIsEncoded);
            SoapReflector.IncludeTypes(methodInfos, soapImporter);
            WebMethodReflector.IncludeTypes(methodInfos, xmlImporter);
            SoapReflectedMethod[] soapMethods = new SoapReflectedMethod[methodInfos.Length];

            SoapExtensionTypeElementCollection extensionTypes = WebServicesSection.Current.SoapExtensionTypes;
            ArrayList highPri = new ArrayList();
            ArrayList lowPri = new ArrayList();
            for (int i = 0; i < extensionTypes.Count; i++) {
                SoapExtensionTypeElement element = extensionTypes[i];
                if (element == null)
                    continue;
                SoapReflectedExtension extension = new SoapReflectedExtension(element.Type, null, element.Priority);
                if (element.Group == PriorityGroup.High)
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
 
            for (int i = 0; i < methodInfos.Length; i++) {
                LogicalMethodInfo methodInfo = methodInfos[i];
                SoapReflectedMethod soapMethod = SoapReflector.ReflectMethod(methodInfo, false, xmlImporter, soapImporter, serviceAttribute.Namespace);
                mappings.Add(soapMethod.requestMappings);
                if (soapMethod.responseMappings != null) mappings.Add(soapMethod.responseMappings);
                mappings.Add(soapMethod.inHeaderMappings);
                if (soapMethod.outHeaderMappings != null) mappings.Add(soapMethod.outHeaderMappings);
                soapMethods[i] = soapMethod;
            }
            
            XmlMapping[] xmlMappings = (XmlMapping[])mappings.ToArray(typeof(XmlMapping));
            TraceMethod caller = Tracing.On ? new TraceMethod(this, ".ctor", type, protocolsSupported) : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceCreateSerializer), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", xmlMappings, this.Evidence));
            XmlSerializer[] serializers = null;
            if (AppDomain.CurrentDomain.IsHomogenous) {
                serializers = XmlSerializer.FromMappings(xmlMappings);
            }
            else {
#pragma warning disable 618 // If we're in a non-homogenous domain, legacy CAS mode is enabled, so passing through evidence will not fail
                serializers = XmlSerializer.FromMappings((xmlMappings), this.Evidence);
#pragma warning restore 618
            }
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceCreateSerializer), caller);
            
            int count = 0;
            for (int i = 0; i < soapMethods.Length; i++) {
                SoapServerMethod serverMethod = new SoapServerMethod();
                SoapReflectedMethod soapMethod = soapMethods[i];
                serverMethod.parameterSerializer = serializers[count++]; 
                if (soapMethod.responseMappings != null) serverMethod.returnSerializer = serializers[count++];
                serverMethod.inHeaderSerializer = serializers[count++];
                if (soapMethod.outHeaderMappings != null) serverMethod.outHeaderSerializer = serializers[count++];
                serverMethod.methodInfo = soapMethod.methodInfo;
                serverMethod.action = soapMethod.action;
                serverMethod.extensions = soapMethod.extensions;
                serverMethod.extensionInitializers = SoapReflectedExtension.GetInitializers(serverMethod.methodInfo, soapMethod.extensions);
                serverMethod.oneWay = soapMethod.oneWay;
                serverMethod.rpc = soapMethod.rpc;
                serverMethod.use = soapMethod.use;
                serverMethod.paramStyle = soapMethod.paramStyle;
                serverMethod.wsiClaims = soapMethod.binding == null ? WsiProfiles.None : soapMethod.binding.ConformsTo;
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
                    if (mapping.direction == SoapHeaderDirection.In)
                        inHeaders.Add(mapping);
                    else if (mapping.direction == SoapHeaderDirection.Out)
                        outHeaders.Add(mapping);
                    else {
                        inHeaders.Add(mapping);
                        outHeaders.Add(mapping);
                    }
                }
                serverMethod.inHeaderMappings = (SoapHeaderMapping[])inHeaders.ToArray(typeof(SoapHeaderMapping));
                if (serverMethod.outHeaderSerializer != null)
                    serverMethod.outHeaderMappings = (SoapHeaderMapping[])outHeaders.ToArray(typeof(SoapHeaderMapping));
            
                // check feasibility of routing on request element for soap 1.1
                if (soap11 && !routingOnSoapAction && soapMethod.requestElementName.IsEmpty)
                    throw new SoapException(Res.GetString(Res.TheMethodDoesNotHaveARequestElementEither1, serverMethod.methodInfo.Name), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));

                // we can lookup methods by action or request element
                if (methods[soapMethod.action] == null)
                    methods[soapMethod.action] = serverMethod;
                else {
                    // duplicate soap actions not allowed in soap 1.1 if we're routing on soap action
                    if (soap11 && routingOnSoapAction) {
                        SoapServerMethod duplicateMethod = (SoapServerMethod)methods[soapMethod.action];
                        throw new SoapException(Res.GetString(Res.TheMethodsAndUseTheSameSoapActionWhenTheService3, serverMethod.methodInfo.Name, duplicateMethod.methodInfo.Name, soapMethod.action), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));
                    }
                    duplicateMethods[soapMethod.action] = serverMethod;
                }

                if (methods[soapMethod.requestElementName] == null)
                    methods[soapMethod.requestElementName] = serverMethod;
                else {
                    // duplicate request elements not allowed in soap 1.1 if we're routing on request element
                    if (soap11 && !routingOnSoapAction) {
                        SoapServerMethod duplicateMethod = (SoapServerMethod)methods[soapMethod.requestElementName];
                        throw new SoapException(Res.GetString(Res.TheMethodsAndUseTheSameRequestElementXmlns4, serverMethod.methodInfo.Name, duplicateMethod.methodInfo.Name, soapMethod.requestElementName.Name, soapMethod.requestElementName.Namespace), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace));
                    }
                    duplicateMethods[soapMethod.requestElementName] = serverMethod;
                }
            }
        }

        public SoapServerMethod GetMethod(object key) {
            return (SoapServerMethod)methods[key];
        }

        public SoapServerMethod GetDuplicateMethod(object key) {
            return (SoapServerMethod)duplicateMethods[key];
        }
    }

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public class SoapServerProtocolFactory : ServerProtocolFactory {
        // POST requests without pathinfo (the "/Foo" in "foo.asmx/Foo") 
        // are treated as soap. if the server supports both versions we route requests
        // with soapaction to 1.1 and other requests to 1.2
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request) {
            if (request.PathInfo.Length > 0)
                return null;
            
            if (request.HttpMethod != "POST")
                // MethodNotAllowed = 405,
                return new UnsupportedRequestProtocol(405);

            // at this point we know it's probably soap. we're still not sure of the version
            // but we leave that to the SoapServerProtocol to figure out
            return new SoapServerProtocol();
        }
    }

    [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public class SoapServerProtocol : ServerProtocol {
        SoapServerType serverType;
        SoapServerMethod serverMethod;
        SoapServerMessage message;
        bool isOneWay;
        Exception onewayInitException;
        SoapProtocolVersion version;
        WebServiceProtocols protocolsSupported;
        SoapServerProtocolHelper helper;

        //
        // Default constructor is provided since WebServicesConfiguration is inaccessible
        // 
        protected internal SoapServerProtocol()
        {
            this.protocolsSupported = WebServicesSection.Current.EnabledProtocols;
        }

        /// <include file='doc\SoapServerProtocol.uex' path='docs/doc[@for="SoapServerProtocol.GetWriterForMessage"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Allows to intercept XmlWriter creation.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand | SecurityAction.InheritanceDemand, Name = "FullTrust")]
        protected virtual XmlWriter GetWriterForMessage(SoapServerMessage message, int bufferSize) {
            if (bufferSize < 512)
                bufferSize = 512;
            return new XmlTextWriter(new StreamWriter(message.Stream, new UTF8Encoding(false), bufferSize));
            /*
            XmlWriterSettings ws = new XmlWriterSettings();
            ws.Encoding = new UTF8Encoding(false);
            ws.Indent = false;
            ws.NewLineHandling = NewLineHandling.None;
            return XmlWriter.Create(message.Stream, ws);
            */
        }

        /// <include file='doc\SoapServerProtocol.uex' path='docs/doc[@for="SoapServerProtocol.GetReaderForMessage"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Allows to intercept XmlReader creation.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand | SecurityAction.InheritanceDemand, Name = "FullTrust")]
        protected virtual XmlReader GetReaderForMessage(SoapServerMessage message, int bufferSize) {
            Encoding enc = RequestResponseUtils.GetEncoding2(message.ContentType);
            if (bufferSize < 512)
                bufferSize = 512;
            int readTimeout = WebServicesSection.Current.SoapEnvelopeProcessing.ReadTimeout;
            Int64 timeout = readTimeout < 0 ? 0L : (Int64)readTimeout * 10000000;
            Int64 nowTicks = DateTime.UtcNow.Ticks;
            Int64 timeoutTicks = Int64.MaxValue - timeout <= nowTicks ? Int64.MaxValue : nowTicks + timeout;
            XmlTextReader reader;
            if (enc != null) {
                if (timeoutTicks == Int64.MaxValue) {
                    reader = new XmlTextReader(new StreamReader(message.Stream, enc, true, bufferSize));
                }
                else {
                    reader = new SoapEnvelopeReader(new StreamReader(message.Stream, enc, true, bufferSize), timeoutTicks);
                }
            }
            else {
                if (timeoutTicks == Int64.MaxValue) {
                    reader = new XmlTextReader(message.Stream);
                }
                else {
                    reader = new SoapEnvelopeReader(message.Stream, timeoutTicks);
                }
            }
            reader.DtdProcessing = DtdProcessing.Prohibit;
            reader.Normalization = true;
            reader.XmlResolver = null;
            return reader;
        }

        internal override bool Initialize() {
            // try to guess the request version so we can handle any exceptions that might come up
            GuessVersion();

            message = new SoapServerMessage(this);
            onewayInitException = null;

            if (null == (serverType = (SoapServerType)GetFromCache(typeof(SoapServerProtocol), Type))
                && null == (serverType = (SoapServerType)GetFromCache(typeof(SoapServerProtocol), Type, true)))
            {
                lock (InternalSyncObject)
                {
                    if (null == (serverType = (SoapServerType)GetFromCache(typeof(SoapServerProtocol), Type))
                        && null == (serverType = (SoapServerType)GetFromCache(typeof(SoapServerProtocol), Type, true)))
                    {
                        bool excludeSchemeHostPortFromCachingKey = this.IsCacheUnderPressure(typeof(SoapServerProtocol), Type);
                        serverType = new SoapServerType(Type, protocolsSupported);
                        AddToCache(typeof(SoapServerProtocol), Type, serverType, excludeSchemeHostPortFromCachingKey);
                    }
                }
            }

            // We delay throwing any exceptions out of the extension until we determine if the method is one-way or not.
            Exception extensionException = null;
            try {
                message.highPriConfigExtensions = SoapMessage.InitializeExtensions(serverType.HighPriExtensions, serverType.HighPriExtensionInitializers);
                //
                // Allow derived classes to modify the high priority extensions list.
                //
                message.highPriConfigExtensions = ModifyInitializedExtensions(PriorityGroup.High, message.highPriConfigExtensions);

                // For one-way methods we rely on Request.InputStream guaranteeing that the entire request body has arrived
                message.SetStream(Request.InputStream);
        
                #if DEBUG
                    //Debug.Assert(message.Stream.CanSeek, "Web services SOAP handler assumes a seekable stream.");
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (!message.Stream.CanSeek) throw new InvalidOperationException("Non-Seekable stream " + message.Stream.GetType().FullName + " Web services SOAP handler assumes a seekable stream.");

                #endif

                message.InitExtensionStreamChain(message.highPriConfigExtensions);
                message.SetStage(SoapMessageStage.BeforeDeserialize);
                message.ContentType = Request.ContentType;
                message.ContentEncoding = Request.Headers[ContentType.ContentEncoding];
                message.RunExtensions(message.highPriConfigExtensions, false);
                extensionException = message.Exception;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "Initialize", e);
                extensionException = e;
            }

            // set this here since we might throw before we init the other extensions
            message.allExtensions = message.highPriConfigExtensions;
                                
            // maybe the extensions that just ran changed some of the request data so we can make a better version guess
            GuessVersion();
            try {
                this.serverMethod = RouteRequest(message);

                // the RouteRequest impl should throw an exception if it can't route the request but just in case...
                if (this.serverMethod == null)
                    throw new SoapException(Res.GetString(Res.UnableToHandleRequest0), new XmlQualifiedName(Soap.Code.Server, Soap.Namespace));
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (helper.RequestNamespace != null)
                    SetHelper(SoapServerProtocolHelper.GetHelper(this, helper.RequestNamespace));

                // version mismatches override other errors
                CheckHelperVersion();

                throw;
            }
            this.isOneWay = serverMethod.oneWay;
            if (extensionException == null) {
                try {
                    SoapReflectedExtension[] otherReflectedExtensions = (SoapReflectedExtension[]) CombineExtensionsHelper(serverMethod.extensions, serverType.LowPriExtensions, typeof(SoapReflectedExtension));
                    object[] otherInitializers = (object[]) CombineExtensionsHelper(serverMethod.extensionInitializers, serverType.LowPriExtensionInitializers, typeof(object));
                    message.otherExtensions = SoapMessage.InitializeExtensions(otherReflectedExtensions, otherInitializers);
                    //
                    // Allow derived classes to modify the other extensions list.
                    //
                    message.otherExtensions = ModifyInitializedExtensions(PriorityGroup.Low, message.otherExtensions);
                    message.allExtensions = (SoapExtension[]) CombineExtensionsHelper(message.highPriConfigExtensions, message.otherExtensions, typeof(SoapExtension));
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "Initialize", e);
                    extensionException = e;
                }
            }

            if (extensionException != null) {
                if (isOneWay)
                    onewayInitException = extensionException;
                else if (extensionException is SoapException)
                    throw extensionException;
                else
                    throw SoapException.Create(Version, Res.GetString(Res.WebConfigExtensionError), new XmlQualifiedName(Soap.Code.Server, Soap.Namespace), extensionException);
            }

            return true;
        }

        /// <devdoc>
        /// Allows derived classes to reorder or to replace intialized soap extensions prior to
        /// the system calling ChainStream or executing them in any stage.        
        /// </devdoc>
        protected virtual SoapExtension[] ModifyInitializedExtensions(PriorityGroup group, SoapExtension[] extensions)
        {
            return extensions;
        }

        /// <devdoc>
        /// Determines which SoapMethod should be invoked for a particular
        /// message.
        /// </devdoc>
        protected virtual SoapServerMethod RouteRequest(SoapServerMessage message)
        {
            return helper.RouteRequest();
        }

        private void GuessVersion() {
            // make a best guess as to the version. we'll get more info when we ---- the envelope
            if (IsSupported(WebServiceProtocols.AnyHttpSoap)) {
                // both versions supported, we need to pick one
                if (Request.Headers[Soap.Action] == null || ContentType.MatchesBase(Request.ContentType, ContentType.ApplicationSoap))
                    SetHelper(new Soap12ServerProtocolHelper(this));
                else
                    SetHelper(new Soap11ServerProtocolHelper(this));
            }
            else if (IsSupported(WebServiceProtocols.HttpSoap)) {
                SetHelper(new Soap11ServerProtocolHelper(this));
            }
            else if (IsSupported(WebServiceProtocols.HttpSoap12)) {
                SetHelper(new Soap12ServerProtocolHelper(this));
            }
        }

        internal bool IsSupported(WebServiceProtocols protocol) {
            return ((protocolsSupported & protocol) == protocol);
        }

        internal override ServerType ServerType {
            get { return serverType; }
        }

        internal override LogicalMethodInfo MethodInfo {
            get { return serverMethod.methodInfo; }
        }

        internal SoapServerMethod ServerMethod {
            get { return serverMethod; }
        }

        internal SoapServerMessage Message {
            get { return message; }
        }

        internal override bool IsOneWay {
            get { return this.isOneWay; }            
        }            
        
        internal override Exception OnewayInitException {
            get {
                Debug.Assert(isOneWay || (onewayInitException == null), "initException is meant to be used for oneWay methods only.");
                return this.onewayInitException;
            }
        }            
            
        internal SoapProtocolVersion Version {
            get { return version; }
        }

        /*
        internal bool IsInitialized {
            get { return serverMethod != null; }
        }
        */

        internal override void CreateServerInstance() {
            base.CreateServerInstance();
            message.SetStage(SoapMessageStage.AfterDeserialize);

            message.RunExtensions(message.allExtensions, true);
            SoapHeaderHandling.SetHeaderMembers(message.Headers, this.Target, serverMethod.inHeaderMappings, SoapHeaderDirection.In, false);
        }

        /*
        #if DEBUG
        private static void CopyStream(Stream source, Stream dest) {
            byte[] bytes = new byte[1024];
            int numRead = 0;
            while ((numRead = source.Read(bytes, 0, 1024)) > 0)
                dest.Write(bytes, 0, numRead);
        }
        #endif
        */

        private void SetHelper(SoapServerProtocolHelper helper) {
            this.helper = helper;
            this.version = helper.Version;
            Context.Items[WebService.SoapVersionContextSlot] = helper.Version;
        }

        private static Array CombineExtensionsHelper(Array array1, Array array2, Type elementType) {
            if (array1 == null) return array2;
            if (array2 == null) return array1;
            int length = array1.Length + array2.Length;
            if (length == 0)
                return null;
            Array result = null;
            if (elementType == typeof(SoapReflectedExtension))
                result = new SoapReflectedExtension[length];
            else if (elementType == typeof(SoapExtension))
                result = new SoapExtension[length];
            else if (elementType == typeof(object))
                result  = new object[length];
            else 
                throw new ArgumentException(Res.GetString(Res.ElementTypeMustBeObjectOrSoapExtensionOrSoapReflectedException), "elementType");
            
            Array.Copy(array1, 0, result, 0, array1.Length);
            Array.Copy(array2, 0, result, array1.Length, array2.Length);
            return result;
        }

        private void CheckHelperVersion() {
            if (helper.RequestNamespace == null) return;

            // looks at the helper request namespace and version information to see if we need to return a 
            // version mismatch fault (and if so, what version fault). there are two conditions to check:
            // unknown envelope ns and known but unsupported envelope ns. there are a few rules this code must follow:
            // * a 1.1 node responds with a 1.1 fault. 
            // * a 1.2 node responds to a 1.1 request with a 1.1 fault but responds to an unknown request with a 1.2 fault.
            // * a both node can respond with either but we prefer 1.1.

            // GetHelper returns an arbitrary helper when the envelope ns is unknown, so we can check the helper's
            // expected envelope against the actual request ns to see if the request ns is unknown

            if (helper.RequestNamespace != helper.EnvelopeNs) { // unknown envelope ns -- version mismatch
                // respond with the version we support or 1.1 if we support both
                string requestNamespace = helper.RequestNamespace;
                if (IsSupported(WebServiceProtocols.HttpSoap))
                    SetHelper(new Soap11ServerProtocolHelper(this));
                else
                    SetHelper(new Soap12ServerProtocolHelper(this));
                throw new SoapException(Res.GetString(Res.WebInvalidEnvelopeNamespace, requestNamespace, helper.EnvelopeNs), SoapException.VersionMismatchFaultCode);
            }
            else if (!IsSupported(helper.Protocol)) { // known envelope ns but we don't support this version -- version mismatch
                // always respond with 1.1
                string requestNamespace = helper.RequestNamespace;
                string expectedNamespace = IsSupported(WebServiceProtocols.HttpSoap) ? Soap.Namespace : Soap12.Namespace;
                SetHelper(new Soap11ServerProtocolHelper(this));
                throw new SoapException(Res.GetString(Res.WebInvalidEnvelopeNamespace, requestNamespace, expectedNamespace), SoapException.VersionMismatchFaultCode);
            }
        }

        internal override object[] ReadParameters() {
            message.InitExtensionStreamChain(message.otherExtensions);
            message.RunExtensions(message.otherExtensions, true);
 
            // do a sanity check on the content-type before we check the version since otherwise the error might be really nasty
            if (!ContentType.IsSoap(message.ContentType))
                throw new SoapException(Res.GetString(Res.WebRequestContent, message.ContentType, helper.HttpContentType), 
                    new XmlQualifiedName(Soap.Code.Client, Soap.Namespace), new SoapFaultSubCode(Soap12FaultCodes.UnsupportedMediaTypeFaultCode));

            // now that all the extensions have run, establish the real version of the request
            XmlReader reader = null;
            try {
                reader = GetXmlReader();
                reader.MoveToContent();
                SetHelper(SoapServerProtocolHelper.GetHelper(this, reader.NamespaceURI));
            }
            catch (XmlException e) {
                throw new SoapException(Res.GetString(Res.WebRequestUnableToRead), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace), e);
            }
            CheckHelperVersion();

            // now do a more specific content-type check for soap 1.1 only (soap 1.2 allows various xml content types)
            if (version == SoapProtocolVersion.Soap11 && !ContentType.MatchesBase(message.ContentType, helper.HttpContentType))
                throw new SoapException(Res.GetString(Res.WebRequestContent, message.ContentType, helper.HttpContentType), 
                    new XmlQualifiedName(Soap.Code.Client, Soap.Namespace), new SoapFaultSubCode(Soap12FaultCodes.UnsupportedMediaTypeFaultCode));

            if (message.Exception != null) {
                throw message.Exception;
            }
            try {
                if (!reader.IsStartElement(Soap.Element.Envelope, helper.EnvelopeNs))
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingEnvelopeElement));

                if (reader.IsEmptyElement)
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingBodyElement));

                int depth = reader.Depth;

                reader.ReadStartElement(Soap.Element.Envelope, helper.EnvelopeNs);
                reader.MoveToContent();

                // run time check for R2738 A MESSAGE MUST include all soapbind:headers specified on a wsdl:input or wsdl:output of a wsdl:operationwsdl:binding that describes it. 
                bool checkRequiredHeaders = (this.serverMethod.wsiClaims & WsiProfiles.BasicProfile1_1) != 0 && version != SoapProtocolVersion.Soap12;
                string missingHeader = new SoapHeaderHandling().ReadHeaders(reader, serverMethod.inHeaderSerializer, message.Headers, serverMethod.inHeaderMappings, SoapHeaderDirection.In, helper.EnvelopeNs, serverMethod.use == SoapBindingUse.Encoded ? helper.EncodingNs : null, checkRequiredHeaders);
                        
                if (missingHeader != null) {
                    throw new SoapHeaderException(Res.GetString(Res.WebMissingHeader, missingHeader), 
                        new XmlQualifiedName(Soap.Code.MustUnderstand, Soap.Namespace));
                }

                if (!reader.IsStartElement(Soap.Element.Body, helper.EnvelopeNs))
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingBodyElement));

                reader.ReadStartElement(Soap.Element.Body, helper.EnvelopeNs);
                reader.MoveToContent();

                object[] values;
                bool isEncodedSoap = serverMethod.use == SoapBindingUse.Encoded;
                TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadParameters") : null;
                if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceReadRequest), caller, new TraceMethod(serverMethod.parameterSerializer, "Deserialize", reader, serverMethod.use == SoapBindingUse.Encoded ? helper.EncodingNs : null));

                bool useDeserializationEvents = !isEncodedSoap && (WebServicesSection.Current.SoapEnvelopeProcessing.IsStrict || Tracing.On);
                if (useDeserializationEvents) {
                    XmlDeserializationEvents events = Tracing.On ? Tracing.GetDeserializationEvents() : RuntimeUtils.GetDeserializationEvents();
                    values = (object[])serverMethod.parameterSerializer.Deserialize(reader, null, events);
                }
                else {
                    values = (object[])serverMethod.parameterSerializer.Deserialize(reader, isEncodedSoap ? helper.EncodingNs : null);
                }
                if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceReadRequest), caller);

                // Consume soap:Body and soap:Envelope closing tags
                while (depth < reader.Depth && reader.Read()) {
                    // Nothing, just read on
                }
                // consume end tag
                if (reader.NodeType == XmlNodeType.EndElement) {
                    reader.Read();
                }

                message.SetParameterValues(values);

                return values;
            }
            catch (SoapException) {
                throw;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                throw new SoapException(Res.GetString(Res.WebRequestUnableToRead), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace), e);
            }
        }

        internal override void WriteReturns(object[] returnValues, Stream outputStream) {

            if (serverMethod.oneWay) return;
            bool isEncoded = serverMethod.use == SoapBindingUse.Encoded;
            SoapHeaderHandling.EnsureHeadersUnderstood(message.Headers);
            message.Headers.Clear();
            SoapHeaderHandling.GetHeaderMembers(message.Headers, this.Target, serverMethod.outHeaderMappings, SoapHeaderDirection.Out, false);

            if (message.allExtensions != null)
                message.SetExtensionStream(new SoapExtensionStream());
            
            message.InitExtensionStreamChain(message.allExtensions);
            
            message.SetStage(SoapMessageStage.BeforeSerialize);
            message.ContentType = ContentType.Compose(helper.HttpContentType, Encoding.UTF8);
            message.SetParameterValues(returnValues);
            message.RunExtensions(message.allExtensions, true);
            message.SetStream(outputStream);
            Response.ContentType = message.ContentType;
            if (message.ContentEncoding != null && message.ContentEncoding.Length > 0)
                Response.AppendHeader(ContentType.ContentEncoding, message.ContentEncoding);

            XmlWriter writer = GetWriterForMessage(message, 1024);
            if (writer == null)
                throw new InvalidOperationException(Res.GetString(Res.WebNullWriterForMessage));

            writer.WriteStartDocument();
            writer.WriteStartElement("soap", Soap.Element.Envelope, helper.EnvelopeNs);
            writer.WriteAttributeString("xmlns", "soap", null, helper.EnvelopeNs);
            if (isEncoded) {
                writer.WriteAttributeString("xmlns", "soapenc", null, helper.EncodingNs);
                writer.WriteAttributeString("xmlns", "tns", null, serverType.serviceNamespace);
                writer.WriteAttributeString("xmlns", "types", null, SoapReflector.GetEncodedNamespace(serverType.serviceNamespace, serverType.serviceDefaultIsEncoded));
            }
            if (serverMethod.rpc && version == SoapProtocolVersion.Soap12) {
                writer.WriteAttributeString("xmlns", "rpc", null, Soap12.RpcNamespace);
            }
            writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);
            writer.WriteAttributeString("xmlns", "xsd", null, XmlSchema.Namespace);
            SoapHeaderHandling.WriteHeaders(writer, serverMethod.outHeaderSerializer, message.Headers, serverMethod.outHeaderMappings, SoapHeaderDirection.Out, isEncoded, serverType.serviceNamespace, serverType.serviceDefaultIsEncoded, helper.EnvelopeNs);
            writer.WriteStartElement(Soap.Element.Body, helper.EnvelopeNs);
            if (isEncoded && version != SoapProtocolVersion.Soap12) // don't write encodingStyle on soap:Body for soap 1.2
                writer.WriteAttributeString("soap", Soap.Attribute.EncodingStyle, null, helper.EncodingNs);

            TraceMethod caller = Tracing.On ? new TraceMethod(this, "WriteReturns") : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceWriteResponse), caller, new TraceMethod(serverMethod.returnSerializer, "Serialize", writer, returnValues, null, isEncoded ? helper.EncodingNs : null));
            serverMethod.returnSerializer.Serialize(writer, returnValues, null, isEncoded ? helper.EncodingNs : null);
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceWriteResponse), caller);

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();

            message.SetStage(SoapMessageStage.AfterSerialize);
            message.RunExtensions(message.allExtensions, true);
        }

        internal override bool WriteException(Exception e, Stream outputStream) {
            if (message == null) return false;

            message.Headers.Clear();
            if (serverMethod != null && this.Target != null)
                SoapHeaderHandling.GetHeaderMembers(message.Headers, this.Target, serverMethod.outHeaderMappings, SoapHeaderDirection.Fault, false);

            SoapException soapException;
            if (e is SoapException)
                soapException = (SoapException)e;
            else if (serverMethod != null && serverMethod.rpc && helper.Version == SoapProtocolVersion.Soap12 && e is ArgumentException)
                // special case to handle soap 1.2 rpc "BadArguments" fault
                soapException = SoapException.Create(Version, Res.GetString(Res.WebRequestUnableToProcess), new XmlQualifiedName(Soap.Code.Client, Soap.Namespace), null, null, null, new SoapFaultSubCode(Soap12FaultCodes.RpcBadArgumentsFaultCode), e);
            else 
                soapException = SoapException.Create(Version, Res.GetString(Res.WebRequestUnableToProcess), new XmlQualifiedName(Soap.Code.Server, Soap.Namespace), e);

            if (SoapException.IsVersionMismatchFaultCode(soapException.Code)) {
                if (IsSupported(WebServiceProtocols.HttpSoap12)) {
                    SoapUnknownHeader unknownHeader = CreateUpgradeHeader();
                    if (unknownHeader != null)
                        Message.Headers.Add(unknownHeader);
                }
            }
            Response.ClearHeaders();
            Response.Clear();
            HttpStatusCode statusCode = helper.SetResponseErrorCode(Response, soapException);

            bool disableExtensions = false;
            SoapExtensionStream extensionStream = new SoapExtensionStream();

            if (message.allExtensions != null)
                message.SetExtensionStream(extensionStream);

            try {
                message.InitExtensionStreamChain(message.allExtensions);
            }
            catch (Exception ex) {
                if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException) {
                    throw;
                }
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", ex);
                disableExtensions = true;
            }

            message.SetStage(SoapMessageStage.BeforeSerialize);
            message.ContentType = ContentType.Compose(helper.HttpContentType, Encoding.UTF8);
            message.Exception = soapException;

            if (!disableExtensions) {
                try {
                    message.RunExtensions(message.allExtensions, false);
                }
                catch (Exception ex) {
                    if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException) {
                        throw;
                    }
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", ex);
                    disableExtensions = true;
                }
            }

            message.SetStream(outputStream);

            Response.ContentType = message.ContentType;
            if (message.ContentEncoding != null && message.ContentEncoding.Length > 0) {
                Response.AppendHeader(ContentType.ContentEncoding, message.ContentEncoding);
            }

            XmlWriter writer = GetWriterForMessage(message, 512);
            if (writer == null)
                throw new InvalidOperationException(Res.GetString(Res.WebNullWriterForMessage));

            helper.WriteFault(writer, message.Exception, statusCode);

            if (!disableExtensions) {
                SoapException extensionException = null;
                try {
                    message.SetStage(SoapMessageStage.AfterSerialize);
                    message.RunExtensions(message.allExtensions, false);
                }
                catch (Exception ex) 
                {
                    if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException) 
                    {
                        throw;
                    }
                    if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "WriteException", ex);
                    if (!extensionStream.HasWritten) {
                        // if we haven't already written to the stream, we may be able to send an error
                        extensionException = SoapException.Create(Version, Res.GetString(Res.WebExtensionError), new XmlQualifiedName(Soap.Code.Server, Soap.Namespace), ex);
                    }
                }

                if (extensionException != null) {
                    Response.ContentType = ContentType.Compose(ContentType.TextPlain, Encoding.UTF8);
                    StreamWriter sw = new StreamWriter(outputStream, new UTF8Encoding(false));
                    sw.WriteLine(GenerateFaultString(message.Exception));

                    sw.Flush();
                }
            }


            return true;
        }

        bool WriteException_TryWriteFault(SoapServerMessage message, Stream outputStream, HttpStatusCode statusCode, bool disableExtensions)
        {

            return true;
        }
        
        internal SoapUnknownHeader CreateUpgradeHeader() {
            XmlDocument doc = new XmlDocument();
            XmlElement upgradeElement = doc.CreateElement(Soap12.Prefix, Soap12.Element.Upgrade, Soap12.Namespace);
            if (IsSupported(WebServiceProtocols.HttpSoap))
                upgradeElement.AppendChild(CreateUpgradeEnvelope(doc, Soap.Prefix, Soap.Namespace));
            if (IsSupported(WebServiceProtocols.HttpSoap12))
                upgradeElement.AppendChild(CreateUpgradeEnvelope(doc, Soap12.Prefix, Soap12.Namespace));

            SoapUnknownHeader upgradeHeader = new SoapUnknownHeader();
            upgradeHeader.Element = upgradeElement;
            return upgradeHeader;
        }

        private static XmlElement CreateUpgradeEnvelope(XmlDocument doc, string prefix, string envelopeNs) {
            XmlElement envelopeElement = doc.CreateElement(Soap12.Prefix, Soap12.Element.UpgradeEnvelope, Soap12.Namespace);
            XmlAttribute xmlnsAttr = doc.CreateAttribute("xmlns", prefix, "http://www.w3.org/2000/xmlns/");
            xmlnsAttr.Value = envelopeNs;
            XmlAttribute qnameAttr = doc.CreateAttribute(Soap12.Attribute.UpgradeEnvelopeQname);
            qnameAttr.Value = prefix + ":" + Soap.Element.Envelope;
            envelopeElement.Attributes.Append(qnameAttr);
            envelopeElement.Attributes.Append(xmlnsAttr);
            return envelopeElement;
        }

        internal XmlReader GetXmlReader() {
            Encoding enc = RequestResponseUtils.GetEncoding2(Message.ContentType);
            // in the case of conformant service check that input encodig is utg8 or urf16
            //
            bool checkEncoding = serverMethod != null && (serverMethod.wsiClaims & WsiProfiles.BasicProfile1_1) != 0 && Version != SoapProtocolVersion.Soap12;
            if (checkEncoding && enc != null) {
                // WS-I BP 1.1: R1012: A MESSAGE MUST be serialized as either UTF-8 or UTF-16.
                if (!(enc is UTF8Encoding) && !(enc is UnicodeEncoding)) {
                    throw new InvalidOperationException(Res.GetString(Res.WebWsiContentTypeEncoding));
                }
            }
            XmlReader reader = GetReaderForMessage(Message, RequestResponseUtils.GetBufferSize(Request.ContentLength));
            if (reader == null)
                throw new InvalidOperationException(Res.GetString(Res.WebNullReaderForMessage));
            return reader;
        }

        internal class SoapEnvelopeReader : XmlTextReader {
            Int64 readerTimedout;

            internal SoapEnvelopeReader(TextReader input, Int64 timeout) : base(input) {
                this.readerTimedout = timeout;
            }
  
            internal SoapEnvelopeReader(Stream input, Int64 timeout) : base(input) {
                this.readerTimedout = timeout;
            }

            public override bool Read() {
                CheckTimeout();
                return base.Read(); 
            }

            public override bool MoveToNextAttribute() {
                CheckTimeout();
                return base.MoveToNextAttribute(); 
            }

            public override XmlNodeType MoveToContent() {
                CheckTimeout();
                return base.MoveToContent(); 
            }

            private void CheckTimeout() {
                if (DateTime.UtcNow.Ticks > readerTimedout) {
                    throw new InvalidOperationException(Res.GetString(Res.WebTimeout));
                }
            }
        }
    }

    internal abstract class SoapServerProtocolHelper {
        SoapServerProtocol protocol;
        string requestNamespace;

        protected SoapServerProtocolHelper(SoapServerProtocol protocol) {
            this.protocol = protocol;
        }

        protected SoapServerProtocolHelper(SoapServerProtocol protocol, string requestNamespace) {
            this.protocol = protocol;
            this.requestNamespace = requestNamespace;
        }

        internal static SoapServerProtocolHelper GetHelper(SoapServerProtocol protocol, string envelopeNs) {
            SoapServerProtocolHelper helper;
            if (envelopeNs == Soap.Namespace)
                helper = new Soap11ServerProtocolHelper(protocol, envelopeNs);
            else if (envelopeNs == Soap12.Namespace)
                helper = new Soap12ServerProtocolHelper(protocol, envelopeNs);
            else
                // just return a soap 1.1 helper -- the fact that the requestNs doesn't match will signal a version mismatch
                helper = new Soap11ServerProtocolHelper(protocol, envelopeNs);
            return helper;
        }

        internal HttpStatusCode SetResponseErrorCode(HttpResponse response, SoapException soapException) {
            if (soapException.SubCode != null && soapException.SubCode.Code == Soap12FaultCodes.UnsupportedMediaTypeFaultCode) {
                response.StatusCode = (int) HttpStatusCode.UnsupportedMediaType;
                soapException.ClearSubCode();
            }
            else if (SoapException.IsClientFaultCode(soapException.Code)) {
                System.Web.Services.Protocols.ServerProtocol.SetHttpResponseStatusCode(response,
                    (int)HttpStatusCode.InternalServerError);

                for (Exception inner = soapException; inner != null; inner = inner.InnerException) {
                    if (inner is XmlException) {
                        response.StatusCode = (int) HttpStatusCode.BadRequest;
                    }
                }
            }
            else {
                System.Web.Services.Protocols.ServerProtocol.SetHttpResponseStatusCode(response,
                    (int)HttpStatusCode.InternalServerError);
            }
            response.StatusDescription = HttpWorkerRequest.GetStatusDescription(response.StatusCode);
            return (HttpStatusCode)response.StatusCode;
        }

        internal abstract void WriteFault(XmlWriter writer, SoapException soapException, HttpStatusCode statusCode);
        internal abstract SoapServerMethod RouteRequest();
        internal abstract SoapProtocolVersion Version { get; }
        internal abstract WebServiceProtocols Protocol { get; }
        internal abstract string EnvelopeNs { get; }
        internal abstract string EncodingNs { get; }
        internal abstract string HttpContentType { get; }

        internal string RequestNamespace {
            get { return requestNamespace; }
        }

        protected SoapServerProtocol ServerProtocol {
            get { return protocol; }
        }

        protected SoapServerType ServerType {
            get { return (SoapServerType)protocol.ServerType; }
        }
        
        // tries to get to the first child element of body, ignoring details
        // such as the namespace of Envelope and Body (a version mismatch check will come later)
        protected XmlQualifiedName GetRequestElement() {
            SoapServerMessage message = ServerProtocol.Message;
            long savedPosition = message.Stream.Position;
            XmlReader reader = protocol.GetXmlReader();
            reader.MoveToContent();
            
            requestNamespace = reader.NamespaceURI;
            if (!reader.IsStartElement(Soap.Element.Envelope, requestNamespace))
                throw new InvalidOperationException(Res.GetString(Res.WebMissingEnvelopeElement));

            if (reader.IsEmptyElement)
                throw new InvalidOperationException(Res.GetString(Res.WebMissingBodyElement));

            reader.ReadStartElement(Soap.Element.Envelope, requestNamespace);
            reader.MoveToContent();

            while (!reader.EOF && !reader.IsStartElement(Soap.Element.Body, requestNamespace))
                reader.Skip();

            if (reader.EOF) {
                throw new InvalidOperationException(Res.GetString(Res.WebMissingBodyElement));
            }

            XmlQualifiedName element;
            if (reader.IsEmptyElement) {
                element = XmlQualifiedName.Empty;
            }
            else {
                reader.ReadStartElement(Soap.Element.Body, requestNamespace);
                reader.MoveToContent();
                element = new XmlQualifiedName(reader.LocalName, reader.NamespaceURI);
            }
            message.Stream.Position = savedPosition;
            return element;
        }
    }
}
