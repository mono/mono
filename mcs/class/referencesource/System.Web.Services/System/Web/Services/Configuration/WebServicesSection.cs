//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Runtime.CompilerServices;

    public sealed class WebServicesSection : ConfigurationSection {
        public WebServicesSection() : base() {
            this.properties.Add(this.conformanceWarnings);
            this.properties.Add(this.protocols);
            this.properties.Add(this.serviceDescriptionFormatExtensionTypes);
            this.properties.Add(this.soapEnvelopeProcessing);
            this.properties.Add(this.soapExtensionImporterTypes);
            this.properties.Add(this.soapExtensionReflectorTypes);
            this.properties.Add(this.soapExtensionTypes);
            this.properties.Add(this.soapTransportImporterTypes);
            this.properties.Add(this.wsdlHelpGenerator);
            this.properties.Add(this.soapServerProtocolFactoryType);
            this.properties.Add(this.diagnostics);
        }

        static object ClassSyncObject {
            get {
                if (classSyncObject == null) {
                    object o = new object();
                    Interlocked.CompareExchange(ref classSyncObject, o, null);
                }
                return classSyncObject;
            }
        }

        [ConfigurationProperty("conformanceWarnings")]
        public WsiProfilesElementCollection ConformanceWarnings {
            get { return (WsiProfilesElementCollection)base[this.conformanceWarnings]; }
        }

        internal WsiProfiles EnabledConformanceWarnings {
            get {
                WsiProfiles retval = WsiProfiles.None;
                foreach (WsiProfilesElement element in this.ConformanceWarnings) {
                    retval |= element.Name;
                }

                return retval;
            }
        }

        public static WebServicesSection Current {
            get {
                WebServicesSection retval = null;

                // check to see if we are running on the server without loading system.web.dll
                if (Thread.GetDomain().GetData(".appDomain") != null) {
                    retval = GetConfigFromHttpContext();
                }
                if (retval == null) {
                    retval = (WebServicesSection)PrivilegedConfigurationManager.GetSection(WebServicesSection.SectionName);
                }
                return retval;
            }
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        static WebServicesSection GetConfigFromHttpContext() {
            PartialTrustHelpers.FailIfInPartialTrustOutsideAspNet();
            HttpContext context = HttpContext.Current;
            if (context != null) {
                return (WebServicesSection)context.GetSection(WebServicesSection.SectionName);
            }
            return null;
        }

        internal XmlSerializer DiscoveryDocumentSerializer {
            get {
                if (this.discoveryDocumentSerializer == null) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.discoveryDocumentSerializer == null) {
                            XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
                            XmlAttributes attrs = new XmlAttributes();
                            foreach (Type discoveryReferenceType in this.DiscoveryReferenceTypes) {
                                object[] xmlElementAttribs = discoveryReferenceType.GetCustomAttributes(typeof(XmlRootAttribute), false);
                                if (xmlElementAttribs.Length == 0) {
                                    throw new InvalidOperationException(Res.GetString(Res.WebMissingCustomAttribute, discoveryReferenceType.FullName, "XmlRoot"));
                                }
                                string name = ((XmlRootAttribute)xmlElementAttribs[0]).ElementName;
                                string ns = ((XmlRootAttribute)xmlElementAttribs[0]).Namespace;
                                XmlElementAttribute attr = new XmlElementAttribute(name, discoveryReferenceType);
                                attr.Namespace = ns;
                                attrs.XmlElements.Add(attr);
                            }
                            attrOverrides.Add(typeof(DiscoveryDocument), "References", attrs);
                            this.discoveryDocumentSerializer = new DiscoveryDocumentSerializer();
                        }
                    }
                }
                return discoveryDocumentSerializer;
            }
        }

        internal Type[] DiscoveryReferenceTypes {
            get { return this.discoveryReferenceTypes; }
        }

        public WebServiceProtocols EnabledProtocols {
            get {
                if (this.enabledProtocols == WebServiceProtocols.Unknown) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.enabledProtocols == WebServiceProtocols.Unknown) {
                            WebServiceProtocols temp = WebServiceProtocols.Unknown;
                            foreach (ProtocolElement element in this.Protocols) {
                                temp |= (WebServiceProtocols)element.Name;
                            }
                            this.enabledProtocols = temp;
                        }
                    }
                }
                return this.enabledProtocols;
            }
        }

        internal Type[] GetAllFormatExtensionTypes() {
            if (this.ServiceDescriptionFormatExtensionTypes.Count == 0) {
                return this.defaultFormatTypes;
            }
            else {
                Type[] formatTypes = new Type[defaultFormatTypes.Length + this.ServiceDescriptionFormatExtensionTypes.Count];
                Array.Copy(defaultFormatTypes, formatTypes, defaultFormatTypes.Length);

                for (int index = 0; index < this.ServiceDescriptionFormatExtensionTypes.Count; ++index) {
                    formatTypes[index + defaultFormatTypes.Length] = this.ServiceDescriptionFormatExtensionTypes[index].Type;
                }
                return formatTypes;
            }
        }

        static XmlFormatExtensionPointAttribute GetExtensionPointAttribute(Type type) {
            object[] attrs = type.GetCustomAttributes(typeof(XmlFormatExtensionPointAttribute), false);
            if (attrs.Length == 0)
                throw new ArgumentException(Res.GetString(Res.TheSyntaxOfTypeMayNotBeExtended1, type.FullName), "type");
            return (XmlFormatExtensionPointAttribute)attrs[0];
        }

        [ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
        static public WebServicesSection GetSection(Configuration config) {
            if (config == null) {
                throw new ArgumentNullException("config");
            }
            return (WebServicesSection)config.GetSection(WebServicesSection.SectionName);
        }

        protected override void InitializeDefault() {
            this.ConformanceWarnings.SetDefaults();
            this.Protocols.SetDefaults();
            // check to see if we are running on the server without loading system.web.dll
            if (Thread.GetDomain().GetData(".appDomain") != null) {
                this.WsdlHelpGenerator.SetDefaults();
            }
            this.SoapServerProtocolFactoryType.Type = typeof(SoapServerProtocolFactory);
        }

        internal static void LoadXmlFormatExtensions(Type[] extensionTypes, XmlAttributeOverrides overrides, XmlSerializerNamespaces namespaces) {
            Hashtable table = new Hashtable();
            table.Add(typeof(ServiceDescription), new XmlAttributes());
            table.Add(typeof(Import), new XmlAttributes());
            table.Add(typeof(Port), new XmlAttributes());
            table.Add(typeof(Service), new XmlAttributes());
            table.Add(typeof(FaultBinding), new XmlAttributes());
            table.Add(typeof(InputBinding), new XmlAttributes());
            table.Add(typeof(OutputBinding), new XmlAttributes());
            table.Add(typeof(OperationBinding), new XmlAttributes());
            table.Add(typeof(Binding), new XmlAttributes());
            table.Add(typeof(OperationFault), new XmlAttributes());
            table.Add(typeof(OperationInput), new XmlAttributes());
            table.Add(typeof(OperationOutput), new XmlAttributes());
            table.Add(typeof(Operation), new XmlAttributes());
            table.Add(typeof(PortType), new XmlAttributes());
            table.Add(typeof(Message), new XmlAttributes());
            table.Add(typeof(MessagePart), new XmlAttributes());
            table.Add(typeof(Types), new XmlAttributes());
            Hashtable extensions = new Hashtable();
            foreach (Type extensionType in extensionTypes) {
                if (extensions[extensionType] != null) {
                    continue;
                }
                extensions.Add(extensionType, extensionType);
                object[] attrs = extensionType.GetCustomAttributes(typeof(XmlFormatExtensionAttribute), false);
                if (attrs.Length == 0) {
                    throw new ArgumentException(Res.GetString(Res.RequiredXmlFormatExtensionAttributeIsMissing1, extensionType.FullName), "extensionTypes");
                }
                XmlFormatExtensionAttribute extensionAttr = (XmlFormatExtensionAttribute)attrs[0];
                foreach (Type extensionPointType in extensionAttr.ExtensionPoints) {
                    XmlAttributes xmlAttrs = (XmlAttributes)table[extensionPointType];
                    if (xmlAttrs == null) {
                        xmlAttrs = new XmlAttributes();
                        table.Add(extensionPointType, xmlAttrs);
                    }
                    XmlElementAttribute xmlAttr = new XmlElementAttribute(extensionAttr.ElementName, extensionType);
                    xmlAttr.Namespace = extensionAttr.Namespace;
                    xmlAttrs.XmlElements.Add(xmlAttr);
                }
                attrs = extensionType.GetCustomAttributes(typeof(XmlFormatExtensionPrefixAttribute), false);
                string[] prefixes = new string[attrs.Length];
                Hashtable nsDefs = new Hashtable();
                for (int i = 0; i < attrs.Length; i++) {
                    XmlFormatExtensionPrefixAttribute prefixAttr = (XmlFormatExtensionPrefixAttribute)attrs[i];
                    prefixes[i] = prefixAttr.Prefix;
                    nsDefs.Add(prefixAttr.Prefix, prefixAttr.Namespace);
                }
                Array.Sort(prefixes, InvariantComparer.Default);
                for (int i = 0; i < prefixes.Length; i++) {
                    namespaces.Add(prefixes[i], (string)nsDefs[prefixes[i]]);
                }
            }
            foreach (Type extensionPointType in table.Keys) {
                XmlFormatExtensionPointAttribute attr = GetExtensionPointAttribute(extensionPointType);
                XmlAttributes xmlAttrs = (XmlAttributes)table[extensionPointType];
                if (attr.AllowElements) {
                    xmlAttrs.XmlAnyElements.Add(new XmlAnyElementAttribute());
                }
                overrides.Add(extensionPointType, attr.MemberName, xmlAttrs);
            }
        }

        internal Type[] MimeImporterTypes {
            get { return this.mimeImporterTypes; }
        }

        internal Type[] MimeReflectorTypes {
            get { return this.mimeReflectorTypes; }
        }

        internal Type[] ParameterReaderTypes {
            get { return this.parameterReaderTypes; }
        }

        protected override ConfigurationPropertyCollection Properties {
            get { return this.properties; }
        }

        internal Type[] ProtocolImporterTypes {
            get {
                if (this.protocolImporterTypes.Length == 0) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.protocolImporterTypes.Length == 0) {
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<Type> protocolImporterList = new List<Type>();

                                // order is important for soap: 1.2 must come after 1.1
                                if ((enabledProtocols & WebServiceProtocols.HttpSoap) != 0) {
                                    protocolImporterList.Add(typeof(SoapProtocolImporter));
                                }
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap12) != 0) {
                                protocolImporterList.Add(typeof(Soap12ProtocolImporter));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != 0) {
                                protocolImporterList.Add(typeof(HttpGetProtocolImporter));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != 0) {
                                protocolImporterList.Add(typeof(HttpPostProtocolImporter));
                            }
                            this.protocolImporterTypes = protocolImporterList.ToArray();
                        }
                    }
                }
                return this.protocolImporterTypes;
            }

            set { this.protocolImporterTypes = value; }
        }

        internal Type[] ProtocolReflectorTypes {
            get {
                if (this.protocolReflectorTypes.Length == 0) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.protocolReflectorTypes.Length == 0) { 
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<Type> protocolReflectorList = new List<Type>();

                                // order is important for soap: 1.2 must come after 1.1
                                if ((enabledProtocols & WebServiceProtocols.HttpSoap) != 0) {
                                    protocolReflectorList.Add(typeof(SoapProtocolReflector));
                                }
                            if ((enabledProtocols & WebServiceProtocols.HttpSoap12) != 0) {
                                protocolReflectorList.Add(typeof(Soap12ProtocolReflector));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != 0) {
                                protocolReflectorList.Add(typeof(HttpGetProtocolReflector));
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != 0) {
                                protocolReflectorList.Add(typeof(HttpPostProtocolReflector));
                            }
                            this.protocolReflectorTypes = protocolReflectorList.ToArray();
                        }
                    }
                }
                return this.protocolReflectorTypes;
            }

            set { this.protocolReflectorTypes = value; }
        }

        [ConfigurationProperty("protocols")]
        public ProtocolElementCollection Protocols {
            get { return (ProtocolElementCollection)base[this.protocols]; }
        }

        [ConfigurationProperty("soapEnvelopeProcessing")]
        public SoapEnvelopeProcessingElement SoapEnvelopeProcessing {
            get { return (SoapEnvelopeProcessingElement)base[this.soapEnvelopeProcessing]; }
            set { base[this.soapEnvelopeProcessing] = value; }
        }

        public DiagnosticsElement Diagnostics {
            get { return (DiagnosticsElement)base[this.diagnostics]; }
            set { base[this.diagnostics] = value; }
        }

        protected override void Reset(ConfigurationElement parentElement) {

            // Fixes potential race condition where serverProtocolFactories != enabledProtocols settings
            this.serverProtocolFactories = null; 
            this.enabledProtocols = WebServiceProtocols.Unknown; 

            if (parentElement != null) {
                WebServicesSection parent = (WebServicesSection)parentElement;

                this.discoveryDocumentSerializer = parent.discoveryDocumentSerializer;
            }
            base.Reset(parentElement);
        }

        internal Type[] ReturnWriterTypes {
            get { return this.returnWriterTypes; }
        }

        internal ServerProtocolFactory[] ServerProtocolFactories {
            get {
                if (this.serverProtocolFactories == null) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.serverProtocolFactories == null) {
                            WebServiceProtocols enabledProtocols = this.EnabledProtocols;
                            List<ServerProtocolFactory> serverProtocolFactoryList = new List<ServerProtocolFactory>();
                                // These are order sensitive. We want SOAP to go first for perf
                                // and Discovery (?wsdl and ?disco) should go before Documentation
                                // both soap versions are handled by the same factory
                                if ((enabledProtocols & WebServiceProtocols.AnyHttpSoap) != 0) {
                                    serverProtocolFactoryList.Add((ServerProtocolFactory)Activator.CreateInstance(this.SoapServerProtocolFactory));
                                }
                            if ((enabledProtocols & WebServiceProtocols.HttpPost) != 0) {
                                serverProtocolFactoryList.Add(new HttpPostServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpPostLocalhost) != 0) {
                                serverProtocolFactoryList.Add(new HttpPostLocalhostServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.HttpGet) != 0) {
                                serverProtocolFactoryList.Add(new HttpGetServerProtocolFactory());
                            }
                            if ((enabledProtocols & WebServiceProtocols.Documentation) != 0) {
                                serverProtocolFactoryList.Add(new DiscoveryServerProtocolFactory());
                                serverProtocolFactoryList.Add(new DocumentationServerProtocolFactory());
                            }
                            this.serverProtocolFactories = serverProtocolFactoryList.ToArray();
                        }
                    }
                }

                return this.serverProtocolFactories;
            }
        }

        internal bool ServiceDescriptionExtended {
            get { return this.ServiceDescriptionFormatExtensionTypes.Count > 0; }
        }

        [ConfigurationProperty("serviceDescriptionFormatExtensionTypes")]
        public TypeElementCollection ServiceDescriptionFormatExtensionTypes {
            get { return (TypeElementCollection)base[this.serviceDescriptionFormatExtensionTypes]; }
        }

        [ConfigurationProperty("soapExtensionImporterTypes")]
        public TypeElementCollection SoapExtensionImporterTypes {
            get { return (TypeElementCollection)base[this.soapExtensionImporterTypes]; }
        }

        [ConfigurationProperty("soapExtensionReflectorTypes")]
        public TypeElementCollection SoapExtensionReflectorTypes {
            get { return (TypeElementCollection)base[this.soapExtensionReflectorTypes]; }
        }

        [ConfigurationProperty("soapExtensionTypes")]
        public SoapExtensionTypeElementCollection SoapExtensionTypes {
            get { return (SoapExtensionTypeElementCollection)base[this.soapExtensionTypes]; }
        }

        [ConfigurationProperty("soapServerProtocolFactory")]
        public TypeElement SoapServerProtocolFactoryType {
            get { return (TypeElement)base[this.soapServerProtocolFactoryType]; }
        }

        internal Type SoapServerProtocolFactory {
            get {
                if (this.soapServerProtocolFactory == null) {
                    lock (WebServicesSection.ClassSyncObject) {
                        if (this.soapServerProtocolFactory == null) {                            
                            this.soapServerProtocolFactory = this.SoapServerProtocolFactoryType.Type;
                        }
                    }
                }
                return this.soapServerProtocolFactory;
            }
        }

        [ConfigurationProperty("soapTransportImporterTypes")]
        public TypeElementCollection SoapTransportImporterTypes {
            get { return (TypeElementCollection)base[this.soapTransportImporterTypes]; }
        }

        internal Type[] SoapTransportImporters {
            get {
                Type[] retval = new Type[1 + this.SoapTransportImporterTypes.Count];
                retval[0] = typeof(SoapHttpTransportImporter);
                for (int i = 0; i < SoapTransportImporterTypes.Count; ++i) {
                    retval[i + 1] = SoapTransportImporterTypes[i].Type;
                }
                return retval;
            }
        }

        void TurnOnGetAndPost() {
            bool needPost = (this.EnabledProtocols & WebServiceProtocols.HttpPost) == 0;
            bool needGet = (this.EnabledProtocols & WebServiceProtocols.HttpGet) == 0;
            if (!needGet && !needPost)
                return;

            ArrayList importers = new ArrayList(ProtocolImporterTypes);
            ArrayList reflectors = new ArrayList(ProtocolReflectorTypes);
            if (needPost) {
                importers.Add(typeof(HttpPostProtocolImporter));
                reflectors.Add(typeof(HttpPostProtocolReflector));
            }
            if (needGet) {
                importers.Add(typeof(HttpGetProtocolImporter));
                reflectors.Add(typeof(HttpGetProtocolReflector));
            }
            ProtocolImporterTypes = (Type[])importers.ToArray(typeof(Type));
            ProtocolReflectorTypes = (Type[])reflectors.ToArray(typeof(Type));
            enabledProtocols |= WebServiceProtocols.HttpGet | WebServiceProtocols.HttpPost;
        }

        [ConfigurationProperty("wsdlHelpGenerator")]
        public WsdlHelpGeneratorElement WsdlHelpGenerator {
            get { return (WsdlHelpGeneratorElement)base[this.wsdlHelpGenerator]; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        // Object for synchronizing access to the entire class( avoiding lock( typeof( ... )) )
        static object classSyncObject = null;
        const string SectionName = @"system.web/webServices";
        readonly ConfigurationProperty conformanceWarnings = new ConfigurationProperty("conformanceWarnings", typeof(WsiProfilesElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty protocols = new ConfigurationProperty("protocols", typeof(ProtocolElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty serviceDescriptionFormatExtensionTypes = new ConfigurationProperty("serviceDescriptionFormatExtensionTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapEnvelopeProcessing = new ConfigurationProperty("soapEnvelopeProcessing", typeof(SoapEnvelopeProcessingElement), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapExtensionImporterTypes = new ConfigurationProperty("soapExtensionImporterTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapExtensionReflectorTypes = new ConfigurationProperty("soapExtensionReflectorTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapExtensionTypes = new ConfigurationProperty("soapExtensionTypes", typeof(SoapExtensionTypeElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapTransportImporterTypes = new ConfigurationProperty("soapTransportImporterTypes", typeof(TypeElementCollection), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty wsdlHelpGenerator = new ConfigurationProperty("wsdlHelpGenerator", typeof(WsdlHelpGeneratorElement), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty soapServerProtocolFactoryType = new ConfigurationProperty("soapServerProtocolFactory", typeof(TypeElement), null, ConfigurationPropertyOptions.None);
        readonly ConfigurationProperty diagnostics = new ConfigurationProperty("diagnostics", typeof(DiagnosticsElement), null, ConfigurationPropertyOptions.None);

        Type[] defaultFormatTypes = new Type[] {
                                                   typeof(HttpAddressBinding),
                                                   typeof(HttpBinding),
                                                   typeof(HttpOperationBinding),
                                                   typeof(HttpUrlEncodedBinding),
                                                   typeof(HttpUrlReplacementBinding),
                                                   typeof(MimeContentBinding),
                                                   typeof(MimeXmlBinding),
                                                   typeof(MimeMultipartRelatedBinding),
                                                   typeof(MimeTextBinding),
                                                   typeof(System.Web.Services.Description.SoapBinding),
                                                   typeof(SoapOperationBinding),
                                                   typeof(SoapBodyBinding),
                                                   typeof(SoapFaultBinding),
                                                   typeof(SoapHeaderBinding),
                                                   typeof(SoapAddressBinding),
                                                   typeof(Soap12Binding),
                                                   typeof(Soap12OperationBinding),
                                                   typeof(Soap12BodyBinding),
                                                   typeof(Soap12FaultBinding),
                                                   typeof(Soap12HeaderBinding),
                                                   typeof(Soap12AddressBinding) };
        Type[] discoveryReferenceTypes = new Type[] { typeof(DiscoveryDocumentReference), typeof(ContractReference), typeof(SchemaReference), typeof(System.Web.Services.Discovery.SoapBinding) };
        XmlSerializer discoveryDocumentSerializer = null;
        WebServiceProtocols enabledProtocols = WebServiceProtocols.Unknown;
        Type[] mimeImporterTypes = new Type[] { typeof(MimeXmlImporter), typeof(MimeFormImporter), typeof(MimeTextImporter) };
        Type[] mimeReflectorTypes = new Type[] { typeof(MimeXmlReflector), typeof(MimeFormReflector) };
        Type[] parameterReaderTypes = new Type[] { typeof(UrlParameterReader), typeof(HtmlFormParameterReader) };
        Type[] protocolImporterTypes = new Type[0];
        Type[] protocolReflectorTypes = new Type[0];
        Type[] returnWriterTypes = new Type[] { typeof(XmlReturnWriter) };
        ServerProtocolFactory[] serverProtocolFactories = null;
        Type soapServerProtocolFactory = null;
    }
}
