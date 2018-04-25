//------------------------------------------------------------------------------
// <copyright file="SoapServerMethod.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.Services;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Web.Services.Diagnostics;

    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class SoapServerMethod {
        //
        // Internal field visibility is maintained for
        // compatibility with existing code.
        //
        internal LogicalMethodInfo methodInfo;
        internal XmlSerializer returnSerializer;
        internal XmlSerializer parameterSerializer;
        internal XmlSerializer inHeaderSerializer;
        internal XmlSerializer outHeaderSerializer;
        internal SoapHeaderMapping[] inHeaderMappings;
        internal SoapHeaderMapping[] outHeaderMappings;
        internal SoapReflectedExtension[] extensions;
        internal object[] extensionInitializers;
        internal string action;
        internal bool oneWay;
        internal bool rpc;
        internal SoapBindingUse use;
        internal SoapParameterStyle paramStyle;
        internal WsiProfiles wsiClaims;

        public SoapServerMethod() {
        }

        public SoapServerMethod(Type serverType, LogicalMethodInfo methodInfo) {
            this.methodInfo = methodInfo;

            //
            // Set up the XmlImporter, the SoapImporter, and acquire
            // the ServiceAttribute on the serverType for use in
            // creating a SoapReflectedMethod.
            //
            WebServiceAttribute serviceAttribute = WebServiceReflector.GetAttribute(serverType);
            string serviceNamespace = serviceAttribute.Namespace;
            bool serviceDefaultIsEncoded = SoapReflector.ServiceDefaultIsEncoded(serverType);

            SoapReflectionImporter soapImporter = SoapReflector.CreateSoapImporter(serviceNamespace, serviceDefaultIsEncoded);
            XmlReflectionImporter xmlImporter = SoapReflector.CreateXmlImporter(serviceNamespace, serviceDefaultIsEncoded);

            //
            // Add some types relating to the methodInfo into the two importers
            //
            SoapReflector.IncludeTypes(methodInfo, soapImporter);
            WebMethodReflector.IncludeTypes(methodInfo, xmlImporter);

            //
            // Create a SoapReflectedMethod by reflecting on the
            // LogicalMethodInfo passed to us.
            //
            SoapReflectedMethod soapMethod = SoapReflector.ReflectMethod(methodInfo, false, xmlImporter, soapImporter, serviceNamespace);

            //
            // Most of the fields in this class are ----ed in from the reflected information
            //
            ImportReflectedMethod(soapMethod);
            ImportSerializers(soapMethod, GetServerTypeEvidence(serverType));
            ImportHeaderSerializers(soapMethod);
        }

        public LogicalMethodInfo MethodInfo {
            get {
                return methodInfo;
            }
        }

        public XmlSerializer ReturnSerializer {
            get {
                return returnSerializer;
            }
        }

        public XmlSerializer ParameterSerializer {
            get {
                return parameterSerializer;
            }
        }

        public XmlSerializer InHeaderSerializer {
            get {
                return inHeaderSerializer;
            }
        }

        public XmlSerializer OutHeaderSerializer {
            get {
                return outHeaderSerializer;
            }
        }

        //
        // 

        public SoapHeaderMapping[] InHeaderMappings {
            get {
                return inHeaderMappings;
            }
        }

        //
        // 

        public SoapHeaderMapping[] OutHeaderMappings {
            get {
                return outHeaderMappings;
            }
        }

        /*
         * WSE3 does not require access to Extension data
         *
        public SoapReflectedExtension[] Extensions
        {
            get
            {
                return extensions;
            }
        }

        public object[] ExtensionInitializers
        {
            get
            {
                return extensionInitializers;
            }
        }
        */

        public string Action {
            get {
                return action;
            }
        }

        public bool OneWay {
            get {
                return oneWay;
            }
        }

        public bool Rpc {
            get {
                return rpc;
            }
        }

        public SoapBindingUse BindingUse {
            get {
                return use;
            }
        }

        public SoapParameterStyle ParameterStyle {
            get {
                return paramStyle;
            }
        }

        public WsiProfiles WsiClaims {
            get {
                return wsiClaims;
            }
        }

        [SecurityPermission(SecurityAction.Assert, ControlEvidence = true)]
        private Evidence GetServerTypeEvidence(Type type) {
            return type.Assembly.Evidence;
        }

        private List<XmlMapping> GetXmlMappingsForMethod(SoapReflectedMethod soapMethod) {
            List<XmlMapping> mappings = new List<XmlMapping>();
            mappings.Add(soapMethod.requestMappings);
            if (soapMethod.responseMappings != null) {
                mappings.Add(soapMethod.responseMappings);
            }
            mappings.Add(soapMethod.inHeaderMappings);
            if (soapMethod.outHeaderMappings != null) {
                mappings.Add(soapMethod.outHeaderMappings);
            }

            return mappings;
        }

        private void ImportReflectedMethod(SoapReflectedMethod soapMethod) {
            this.action = soapMethod.action;
            this.extensions = soapMethod.extensions;
            this.extensionInitializers = SoapReflectedExtension.GetInitializers(this.methodInfo, soapMethod.extensions);
            this.oneWay = soapMethod.oneWay;
            this.rpc = soapMethod.rpc;
            this.use = soapMethod.use;
            this.paramStyle = soapMethod.paramStyle;
            this.wsiClaims = soapMethod.binding == null ? WsiProfiles.None : soapMethod.binding.ConformsTo;
        }

        private void ImportHeaderSerializers(SoapReflectedMethod soapMethod) {
            List<SoapHeaderMapping> inHeaders = new List<SoapHeaderMapping>();
            List<SoapHeaderMapping> outHeaders = new List<SoapHeaderMapping>();

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

            this.inHeaderMappings = inHeaders.ToArray();
            if (this.outHeaderSerializer != null)
                this.outHeaderMappings = outHeaders.ToArray();
        }

        private void ImportSerializers(SoapReflectedMethod soapMethod, Evidence serverEvidence) {
            //
            // Keep track of all XmlMapping instances we need for this method.
            //
            List<XmlMapping> mappings = GetXmlMappingsForMethod(soapMethod);

            //
            // Generate serializers from those XmlMappings
            //

            XmlMapping[] xmlMappings = mappings.ToArray();
            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ImportSerializers") : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceCreateSerializer), caller, new TraceMethod(typeof(XmlSerializer), "FromMappings", xmlMappings, serverEvidence));
            XmlSerializer[] serializers = null;
            if (AppDomain.CurrentDomain.IsHomogenous) {
                serializers = XmlSerializer.FromMappings(xmlMappings);
            }
            else {
#pragma warning disable 618 // If we're in a non-homogenous domain, legacy CAS mode is enabled, so passing through evidence will not fail
                serializers = XmlSerializer.FromMappings(xmlMappings, serverEvidence);
#pragma warning restore 618
            }
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceCreateSerializer), caller);

            int i = 0;
            this.parameterSerializer = serializers[i++];
            if (soapMethod.responseMappings != null) {
                this.returnSerializer = serializers[i++];
            }
            this.inHeaderSerializer = serializers[i++];
            if (soapMethod.outHeaderMappings != null) {
                this.outHeaderSerializer = serializers[i++];
            }
        }
    }
}
