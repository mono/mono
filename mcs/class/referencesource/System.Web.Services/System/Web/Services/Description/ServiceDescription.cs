//------------------------------------------------------------------------------
// <copyright file="ServiceDescription.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System.Collections.Specialized;
    using System;
    using System.Xml;
    using System.IO;
    using System.Reflection;
    using System.ComponentModel;
    using System.CodeDom;
    using System.Text;
    using System.Web.Services.Configuration;
    using System.Diagnostics;
    using System.Threading;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web.Services.Protocols;

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription"]/*' />
    /// <devdoc>
    /// 
    /// </devdoc>
    [XmlRoot("definitions", Namespace = ServiceDescription.Namespace)]
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class ServiceDescription : NamedItem {
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = "http://schemas.xmlsoap.org/wsdl/";
        internal const string Prefix = "wsdl";
        Types types;
        ImportCollection imports;
        MessageCollection messages;
        PortTypeCollection portTypes;
        BindingCollection bindings;
        ServiceCollection services;
        string targetNamespace;
        ServiceDescriptionFormatExtensionCollection extensions;
        ServiceDescriptionCollection parent;
        string appSettingUrlKey;
        string appSettingBaseUrl;
        string retrievalUrl;
        static XmlSerializer serializer;
        static XmlSerializerNamespaces namespaces;
        const WsiProfiles SupportedClaims = WsiProfiles.BasicProfile1_1;
        static XmlSchema schema = null;
        static XmlSchema soapEncodingSchema = null;
        StringCollection validationWarnings;
        static StringCollection warnings = new StringCollection();
        ServiceDescription next;

        private static void InstanceValidation (object sender, ValidationEventArgs args) {
            warnings.Add(Res.GetString(Res.WsdlInstanceValidationDetails, args.Message, args.Exception.LineNumber.ToString(CultureInfo.InvariantCulture), args.Exception.LinePosition.ToString(CultureInfo.InvariantCulture)));
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.RetrievalUrl"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public string RetrievalUrl {
            get { return retrievalUrl == null ? string.Empty : retrievalUrl; }
            set { retrievalUrl = value; }
        }

        internal void SetParent(ServiceDescriptionCollection parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.ServiceDescriptions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public ServiceDescriptionCollection ServiceDescriptions {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Imports"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("import")]
        public ImportCollection Imports {
            get { if (imports == null) imports = new ImportCollection(this); return imports; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Types"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("types")]
        public Types Types {
            get { if (types == null) types = new Types(); return types; }
            set { types = value; }
        }

        private bool ShouldSerializeTypes() { return Types.HasItems(); }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Messages"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("message")]
        public MessageCollection Messages {
            get { if (messages == null) messages = new MessageCollection(this); return messages; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.PortTypes"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("portType")]
        public PortTypeCollection PortTypes {
            get { if (portTypes == null) portTypes = new PortTypeCollection(this); return portTypes; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Bindings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("binding")]
        public BindingCollection Bindings {
            get { if (bindings == null) bindings = new BindingCollection(this); return bindings; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Services"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("service")]
        public ServiceCollection Services {
            get { if (services == null) services = new ServiceCollection(this); return services; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.TargetNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("targetNamespace")]
        public string TargetNamespace {
            get { return targetNamespace; }
            set { targetNamespace = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Schema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static XmlSchema Schema {
            get {
                if (schema == null) {
                    using (XmlTextReader reader = new XmlTextReader(new StringReader(Schemas.Wsdl)))
                    {
                        reader.DtdProcessing = DtdProcessing.Ignore;
                        schema = XmlSchema.Read(reader, null);
                    }
                }
                return schema;
            }
        }

        internal static XmlSchema SoapEncodingSchema {
            get {
                if (soapEncodingSchema == null) {
                    using (XmlTextReader reader = new XmlTextReader(new StringReader(Schemas.SoapEncoding)))
                    {
                        reader.DtdProcessing = DtdProcessing.Ignore;
                        soapEncodingSchema = XmlSchema.Read(reader, null);
                    }
                }
                return soapEncodingSchema;
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.ValidationWarnings"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public StringCollection ValidationWarnings {
            get {
                if (validationWarnings == null) {
                    validationWarnings = new StringCollection();
                }
                return validationWarnings;
            }
        }

        internal void SetWarnings(StringCollection warnings) {
            this.validationWarnings = warnings;
        }

        // This is a special serializer that hardwires to the generated
        // ServiceDescriptionSerializer. To regenerate the serializer
        // Turn on KEEPTEMPFILES 
        // Restart server
        // Run wsdl as follows
        //   wsdl <URL_FOR_VALID_ASMX_FILE>?wsdl
        // Goto windows temp dir (usually \winnt\temp)
        // and get the latest generated .cs file
        // Change namespace to 'System.Web.Services.Description'
        // Change class names to ServiceDescriptionSerializationWriter
        // and ServiceDescriptionSerializationReader
        // Make the classes internal
        // Ensure the public Write method is Write125_definitions (If not
        // change Serialize to call the new one)
        // Ensure the public Read method is Read126_definitions (If not
        // change Deserialize to call the new one)
        internal class ServiceDescriptionSerializer : XmlSerializer {
            protected override XmlSerializationReader CreateReader() {
                return new ServiceDescriptionSerializationReader();
            }
            protected override XmlSerializationWriter CreateWriter() {
                return new ServiceDescriptionSerializationWriter();
            }
            public override bool CanDeserialize(System.Xml.XmlReader xmlReader) {
                return xmlReader.IsStartElement("definitions", ServiceDescription.Namespace);
            }
            protected override void Serialize(Object objectToSerialize, XmlSerializationWriter writer) {
                ((ServiceDescriptionSerializationWriter)writer).Write125_definitions(objectToSerialize);
            }
            protected override object Deserialize(XmlSerializationReader reader) {
                return ((ServiceDescriptionSerializationReader)reader).Read125_definitions();
            }
        }
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Serializer"]/*' />
        /// <devdoc>
        /// Returns the serializer for processing web service calls.  The serializer is customized according
        /// to settings in config.web.
        /// <internalonly/>
        /// <internalonly/>
        /// </devdoc>
        [XmlIgnore]
        public static XmlSerializer Serializer {
            get { 
                if (serializer == null) {
                    WebServicesSection config = WebServicesSection.Current;
                    XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("s", XmlSchema.Namespace);
                    WebServicesSection.LoadXmlFormatExtensions(config.GetAllFormatExtensionTypes(), overrides, ns);
                    namespaces = ns;
                    if (config.ServiceDescriptionExtended)
                        serializer = new XmlSerializer(typeof(ServiceDescription), overrides);
                    else
                        serializer = new ServiceDescriptionSerializer();
                    serializer.UnknownElement += new XmlElementEventHandler(RuntimeUtils.OnUnknownElement);
                }
                return serializer;
            }
        }

        internal string AppSettingBaseUrl {
            get { return appSettingBaseUrl; }
            set { appSettingBaseUrl = value; }
        }

        internal string AppSettingUrlKey {
            get { return appSettingUrlKey; }
            set { appSettingUrlKey = value; }
        }

        internal ServiceDescription Next {
            get { return next; }
            set { next = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(TextReader textReader) {
            return Read(textReader, false);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(Stream stream) {
            return Read(stream, false);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(XmlReader reader) {
            return Read(reader, false);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(string fileName) {
            return Read(fileName, false);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read4"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(TextReader textReader, bool validate) {
            XmlTextReader reader = new XmlTextReader(textReader);
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.XmlResolver = null;
            reader.DtdProcessing = DtdProcessing.Prohibit;
            return Read(reader, validate);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read5"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(Stream stream, bool validate) {
            XmlTextReader reader = new XmlTextReader(stream);
            reader.WhitespaceHandling = WhitespaceHandling.Significant;
            reader.XmlResolver = null;
            reader.DtdProcessing = DtdProcessing.Prohibit;
            return Read(reader, validate);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read6"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(string fileName, bool validate) {
            StreamReader reader = new StreamReader(fileName, Encoding.Default, true);
            try {
                return Read(reader, validate);
            }
            finally {
                reader.Close();
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Read7"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static ServiceDescription Read(XmlReader reader, bool validate) {
            if (validate) {
                XmlReaderSettings readerSettings = new XmlReaderSettings();

                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints;

                readerSettings.Schemas.Add(Schema);
                readerSettings.Schemas.Add(SoapBinding.Schema);
                readerSettings.ValidationEventHandler += new ValidationEventHandler(InstanceValidation);
                warnings.Clear();
                XmlReader validatingReader = XmlReader.Create(reader, readerSettings);
                if (reader.ReadState != ReadState.Initial) {
                    //underlying reader has moved, so move validatingreader as well
                    validatingReader.Read();
                }
                ServiceDescription sd = (ServiceDescription)Serializer.Deserialize(validatingReader);
                sd.SetWarnings(warnings);
                return sd;
            }
            else {
                return (ServiceDescription)Serializer.Deserialize(reader);
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.CanRead"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool CanRead(XmlReader reader) {
            return Serializer.CanDeserialize(reader);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Write"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(string fileName) {
            StreamWriter writer = new StreamWriter(fileName);
            try {
                Write(writer);
            }
            finally {
                writer.Close();
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Write1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(TextWriter writer) {
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = 2;
            Write(xmlWriter);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Write2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(Stream stream) {
            TextWriter writer = new StreamWriter(stream);
            Write(writer);
            writer.Flush();
        }


        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Write3"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(XmlWriter writer) {
            XmlSerializer serializer = Serializer;
            XmlSerializerNamespaces ns;
            if (Namespaces == null || Namespaces.Count == 0) {
                ns = new XmlSerializerNamespaces(namespaces);
                ns.Add(ServiceDescription.Prefix, ServiceDescription.Namespace);
                if (this.TargetNamespace != null && this.TargetNamespace.Length != 0) {
                    ns.Add("tns", this.TargetNamespace);
                }
                for (int i = 0; i < Types.Schemas.Count; i++) {
                    string tns = Types.Schemas[i].TargetNamespace;
                    if (tns != null && tns.Length > 0 && tns != this.TargetNamespace && tns != ServiceDescription.Namespace) {
                        ns.Add("s" + i.ToString(CultureInfo.InvariantCulture), tns);
                    }
                }
                for (int i = 0; i < Imports.Count; i++) {
                    Import import = Imports[i];
                    if (import.Namespace.Length > 0) {
                        ns.Add("i" + i.ToString(CultureInfo.InvariantCulture), import.Namespace);
                    }
                }
            }
            else {
                ns = Namespaces;
            }
            serializer.Serialize(writer, this, ns);
        }

        internal static WsiProfiles GetConformanceClaims(XmlElement documentation) {
            if (documentation == null)
                return WsiProfiles.None;

            WsiProfiles existingClaims = WsiProfiles.None;

            XmlNode child = documentation.FirstChild;
            while (child != null) {
                XmlNode sibling = child.NextSibling;
                if (child is XmlElement) {
                    XmlElement element = (XmlElement)child;
                    if (element.LocalName == Soap.Element.Claim && element.NamespaceURI == Soap.ConformanceClaim) {
                        if (Soap.BasicProfile1_1 == element.GetAttribute(Soap.Attribute.ConformsTo)) {
                            existingClaims |= WsiProfiles.BasicProfile1_1;
                        }
                    }
                }
                child = sibling;
            }
            return existingClaims;
        }

        internal static void AddConformanceClaims(XmlElement documentation, WsiProfiles claims) {
            //
            claims &= SupportedClaims;
            if (claims == WsiProfiles.None)
                return;

            // check already presend claims
            WsiProfiles existingClaims = GetConformanceClaims(documentation);
            claims &= ~existingClaims;
            if (claims == WsiProfiles.None)
                return;

            XmlDocument d = documentation.OwnerDocument;
            if ((claims & WsiProfiles.BasicProfile1_1) != 0) {
                XmlElement claim = d.CreateElement(Soap.ClaimPrefix, Soap.Element.Claim, Soap.ConformanceClaim);
                claim.SetAttribute(Soap.Attribute.ConformsTo, Soap.BasicProfile1_1);
                documentation.InsertBefore(claim, null);
            }
        }
    }


    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Import"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Import : DocumentableItem {
        string ns;
        string location;
        ServiceDescription parent;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal void SetParent(ServiceDescription parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Import.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Import.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("namespace")]
        public string Namespace {
            get { return ns == null ? string.Empty : ns; }
            set { ns = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Import.Location"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("location")]
        public string Location {
            get { return location == null ? string.Empty : location; }
            set { location = value; }
        }
    }


    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class DocumentableItem {
        XmlDocument parent;
        string documentation;
        XmlElement documentationElement;
        XmlAttribute[] anyAttribute;
        XmlSerializerNamespaces namespaces;
 
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem.Documentation"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public string Documentation {
            get {
                if (documentation != null)
                    return documentation;
                if (documentationElement == null)
                    return string.Empty;
                return documentationElement.InnerXml;
            }
            set {
                documentation = value;
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                XmlTextWriter xmlWriter = new XmlTextWriter(writer);
                xmlWriter.WriteElementString(ServiceDescription.Prefix, "documentation", ServiceDescription.Namespace, value);
                using (XmlTextReader reader = new XmlTextReader(new StringReader(writer.ToString())))
                {
                    reader.DtdProcessing = DtdProcessing.Ignore;
                    Parent.Load(reader);
                }
                documentationElement = parent.DocumentElement;
                xmlWriter.Close();
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem.DocumentationElement"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAnyElement("documentation", Namespace = ServiceDescription.Namespace)]
        [ComVisible(false)]
        public XmlElement DocumentationElement {
            get { return documentationElement; }
            set {
                documentationElement = value; 
                documentation = null;
            }
        }

        [XmlAnyAttribute]
        public System.Xml.XmlAttribute[] ExtensibleAttributes {
            get {
                return this.anyAttribute;
            }
            set {
                this.anyAttribute = value;
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem.Namespaces"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces {
            get {
                if (this.namespaces == null)
                    this.namespaces = new XmlSerializerNamespaces();
                return this.namespaces;
            }
            set { this.namespaces = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public abstract ServiceDescriptionFormatExtensionCollection Extensions { get; }

        internal XmlDocument Parent {
            get { 
                if (parent == null)
                    parent = new XmlDocument();
                return parent;
            }
        }

        internal XmlElement GetDocumentationElement() {
            if (documentationElement == null) {
                documentationElement = Parent.CreateElement(ServiceDescription.Prefix, "documentation", ServiceDescription.Namespace);
                Parent.InsertBefore(documentationElement, null);
            }
            return documentationElement;
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="DocumentableItem"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class NamedItem : DocumentableItem {
        string name;

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="NamedItem.Name"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("name")]
        public string Name {
            get { return name; }
            set { name = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Port"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Port : NamedItem {
        ServiceDescriptionFormatExtensionCollection extensions;
        XmlQualifiedName binding = XmlQualifiedName.Empty;
        Service parent;

        internal void SetParent(Service parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Port.Service"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Service Service {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Port.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Port.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("binding")]
        public XmlQualifiedName Binding {
            get { return binding; }
            set { binding = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Service"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Service : NamedItem {
        ServiceDescriptionFormatExtensionCollection extensions;
        PortCollection ports;
        ServiceDescription parent;

        internal void SetParent(ServiceDescription parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Service.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Service.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Service.Ports"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("port")]
        public PortCollection Ports {
            get { if (ports == null) ports = new PortCollection(this); return ports; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class FaultBinding : MessageBinding {
        ServiceDescriptionFormatExtensionCollection extensions;

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBinding.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class MessageBinding : NamedItem {
        OperationBinding parent;

        internal void SetParent(OperationBinding parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageBinding.OperationBinding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationBinding OperationBinding {
            get { return parent; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="InputBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class InputBinding : MessageBinding {
        ServiceDescriptionFormatExtensionCollection extensions; 

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="InputBinding.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OutputBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OutputBinding : MessageBinding {
        ServiceDescriptionFormatExtensionCollection extensions; 

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OutputBinding.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationBinding : NamedItem {
        ServiceDescriptionFormatExtensionCollection extensions; 
        FaultBindingCollection faults;
        InputBinding input;
        OutputBinding output;
        Binding parent;

        internal void SetParent(Binding parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding.Binding"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding Binding {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding.Input"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("input")]
        public InputBinding Input {
            get { return input; }
            set {
                if (input != null) {
                    input.SetParent(null);
                }
                input = value;
                if (input != null) {
                    input.SetParent(this);
                }
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding.Output"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("output")]
        public OutputBinding Output {
            get { return output; }
            set {
                if (output != null) {
                    output.SetParent(null);
                }
                output = value;
                if (output != null) {
                    output.SetParent(this);
                }
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBinding.Faults"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("fault")]
        public FaultBindingCollection Faults {
            get { if (faults == null) faults = new FaultBindingCollection(this); return faults; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Binding"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Binding : NamedItem {
        ServiceDescriptionFormatExtensionCollection extensions; 
        OperationBindingCollection operations; 
        XmlQualifiedName type = XmlQualifiedName.Empty;
        ServiceDescription parent;

        internal void SetParent(ServiceDescription parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Binding.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Binding.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Binding.Operations"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("operation")]
        public OperationBindingCollection Operations {
            get { if (operations == null) operations = new OperationBindingCollection(this); return operations; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Binding.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("type")]
        public XmlQualifiedName Type {
            get { 
                if ((object)type == null) return XmlQualifiedName.Empty;
                return type; 
            }
            set { type = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessage"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class OperationMessage : NamedItem {
        XmlQualifiedName message = XmlQualifiedName.Empty;
        Operation parent;

        internal void SetParent(Operation parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessage.Operation"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Operation Operation {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessage.Message"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("message")]
        public XmlQualifiedName Message {
            get { return message; }
            set { message = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFault"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationFault : OperationMessage {
        ServiceDescriptionFormatExtensionCollection extensions;

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationInput"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationInput : OperationMessage {
        ServiceDescriptionFormatExtensionCollection extensions;

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationOutput"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class OperationOutput : OperationMessage {
        ServiceDescriptionFormatExtensionCollection extensions;

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Operation : NamedItem {
        string[] parameters;
        OperationMessageCollection messages;
        OperationFaultCollection faults;
        PortType parent;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal void SetParent(PortType parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.PortType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType PortType {
            get { return parent; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.ParameterOrderString"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("parameterOrder"), DefaultValue("")]
        public string ParameterOrderString {
            get { 
                if (parameters == null) return string.Empty;
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < parameters.Length; i++) {
                    if (i > 0) builder.Append(' ');
                    builder.Append(parameters[i]);
                }
                return builder.ToString(); 
            }
            set {
                if (value == null)
                    parameters = null;
                else
                    parameters = value.Split(new char[] { ' ' });
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.ParameterOrder"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public string[] ParameterOrder {
            get { return parameters; }
            set { parameters = value; }
        }


        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.Messages"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("input", typeof(OperationInput)), 
        XmlElement("output", typeof(OperationOutput))]
        public OperationMessageCollection Messages {
            get { if (messages == null) messages = new OperationMessageCollection(this); return messages; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.Faults"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("fault")]
        public OperationFaultCollection Faults {
            get { if (faults == null) faults = new OperationFaultCollection(this); return faults; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Operation.IsBoundBy"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsBoundBy(OperationBinding operationBinding) {
            if (operationBinding.Name != Name) return false;
            OperationMessage input = Messages.Input;
            if (input != null) {
                if (operationBinding.Input == null) return false;

                string portTypeInputName = GetMessageName(Name, input.Name, true);
                string bindingInputName = GetMessageName(operationBinding.Name, operationBinding.Input.Name, true);
                if (bindingInputName != portTypeInputName) return false;
            }
            else if (operationBinding.Input != null)
                return false;
                
            OperationMessage output = Messages.Output;
            if (output != null) {
                if (operationBinding.Output == null) return false;

                string portTypeOutputName = GetMessageName(Name, output.Name, false);
                string bindingOutputName = GetMessageName(operationBinding.Name, operationBinding.Output.Name, false);
                if (bindingOutputName != portTypeOutputName) return false;
            }
            else if (operationBinding.Output != null)
                return false;
            return true;
        }

        private string GetMessageName(string operationName, string messageName, bool isInput) {
            if (messageName != null && messageName.Length > 0)
                return messageName;
            
            switch (Messages.Flow) {
            case OperationFlow.RequestResponse:
                if (isInput)
                    return operationName + "Request";
                return operationName + "Response";
            case OperationFlow.OneWay:
                if (isInput)
                    return operationName;
                Debug.Assert(isInput == true, "Oneway flow cannot have an output message");
                return null;
            // Cases not supported
            case OperationFlow.SolicitResponse:
                return null;
            case OperationFlow.Notification:
                return null;
            }
            Debug.Assert(false, "Unknown message flow");
            return null;
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortType"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class PortType : NamedItem {
        OperationCollection operations;
        ServiceDescription parent;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal void SetParent(ServiceDescription parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortType.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortType.Operations"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("operation")]
        public OperationCollection Operations {
            get { if (operations == null) operations = new OperationCollection(this); return operations; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Message"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Message : NamedItem {
        MessagePartCollection parts;
        ServiceDescription parent;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal void SetParent(ServiceDescription parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Message.ServiceDescription"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescription ServiceDescription {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Message.Parts"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("part")]
        public MessagePartCollection Parts {
            get { if (parts == null) parts = new MessagePartCollection(this); return parts; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Message.FindPartsByName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MessagePart[] FindPartsByName(string[] partNames) {
            MessagePart[] partArray = new MessagePart[partNames.Length];
            for (int i = 0; i < partNames.Length; i++) {
                partArray[i] = FindPartByName(partNames[i]);
            }
            return partArray;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Message.FindPartByName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MessagePart FindPartByName(string partName) {
            for (int i = 0; i < parts.Count; i++) {
                MessagePart part = parts[i];
                if (part.Name == partName) return part;
            }
            throw new ArgumentException(Res.GetString(Res.MissingMessagePartForMessageFromNamespace3, partName, Name, ServiceDescription.TargetNamespace), "partName");
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePart"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class MessagePart : NamedItem {
        XmlQualifiedName type = XmlQualifiedName.Empty;
        XmlQualifiedName element = XmlQualifiedName.Empty;
        Message parent;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal void SetParent(Message parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescription.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePart.Message"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message Message {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePart.Element"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("element")]
        public XmlQualifiedName Element {
            get { return element; }
            set { element = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePart.Type"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("type")]
        public XmlQualifiedName Type {
            get { 
                if ((object)type == null) return XmlQualifiedName.Empty;
                return type; 
            }
            set { type = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Types"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Types : DocumentableItem {
        XmlSchemas schemas;
        ServiceDescriptionFormatExtensionCollection extensions;

        internal bool HasItems() { 
            return (schemas != null && schemas.Count > 0) ||
                (extensions != null && extensions.Count > 0);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Types.Extensions"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions { 
            get { if (extensions == null) extensions = new ServiceDescriptionFormatExtensionCollection(this); return extensions; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="Types.Schemas"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlElement("schema", typeof(XmlSchema), Namespace = XmlSchema.Namespace)]
        public XmlSchemas Schemas {
            get { if (schemas == null) schemas = new XmlSchemas(); return schemas; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ServiceDescriptionFormatExtensionCollection : ServiceDescriptionBaseCollection {
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.ServiceDescriptionFormatExtensionCollection"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ServiceDescriptionFormatExtensionCollection(object parent) : base(parent) { }

        ArrayList handledElements;
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object this[int index] {
            get { return (object)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(object extension) {
            return List.Add(extension);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, object extension) {
            List.Insert(index, extension);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(object extension) {
            return List.IndexOf(extension);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(object extension) {
            return List.Contains(extension);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(object extension) {
            List.Remove(extension);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(object[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Find"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object Find(Type type) {
            for (int i = 0; i < List.Count; i++) {
                object item = List[i];
                if (type.IsAssignableFrom(item.GetType())) {
                    ((ServiceDescriptionFormatExtension)item).Handled = true;
                    return item;
                }
            }
            return null;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.FindAll"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object[] FindAll(Type type) {
            ArrayList list = new ArrayList();
            for (int i = 0; i < List.Count; i++) {
                object item = List[i];
                if (type.IsAssignableFrom(item.GetType())) {
                    ((ServiceDescriptionFormatExtension)item).Handled = true;
                    list.Add(item);
                }
            }
            return (object[])list.ToArray(type);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.Find1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElement Find(string name, string ns) {
            for (int i = 0; i < List.Count; i++) {
                XmlElement element = List[i] as XmlElement;
                if (element != null && element.LocalName == name && element.NamespaceURI == ns) {
                    SetHandled(element);
                    return element;
                }
            }
            return null;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.FindAll1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public XmlElement[] FindAll(string name, string ns) {
            ArrayList list = new ArrayList();
            for (int i = 0; i < List.Count; i++) {
                XmlElement element = List[i] as XmlElement;
                if (element != null && element.LocalName == name && element.NamespaceURI == ns) {
                    SetHandled(element);
                    list.Add(element);
                }
            }
            return (XmlElement[])list.ToArray(typeof(XmlElement));
        }

        void SetHandled(XmlElement element) {
            if (handledElements == null) 
                handledElements = new ArrayList();
            if (!handledElements.Contains(element))
                handledElements.Add(element);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.IsHandled"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsHandled(object item) {
            if (item is XmlElement)
                return IsHandled((XmlElement)item);
            else
                return ((ServiceDescriptionFormatExtension)item).Handled;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.IsRequired"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsRequired(object item) {
            if (item is XmlElement)
                return IsRequired((XmlElement)item);
            else
                return ((ServiceDescriptionFormatExtension)item).Required;
        }

        bool IsHandled(XmlElement element) {
            if (handledElements == null) return false;
            return handledElements.Contains(element);
        }

        bool IsRequired(XmlElement element) {
            XmlAttribute requiredAttr = element.Attributes["required", ServiceDescription.Namespace];
            if (requiredAttr == null || requiredAttr.Value == null) {
                requiredAttr = element.Attributes["required"];
                if (requiredAttr == null || requiredAttr.Value == null) return false; // not required, by default
            }
            return XmlConvert.ToBoolean(requiredAttr.Value);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            if (value is ServiceDescriptionFormatExtension) ((ServiceDescriptionFormatExtension)value).SetParent(parent);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtensionCollection.OnValidate"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnValidate(object value) {
            if (!(value is XmlElement || value is ServiceDescriptionFormatExtension)) 
                throw new ArgumentException(Res.GetString(Res.OnlyXmlElementsOrTypesDerivingFromServiceDescriptionFormatExtension0), "value");
            base.OnValidate(value);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtension"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class ServiceDescriptionFormatExtension {
        object parent; 
        bool required;
        bool handled;

        internal void SetParent(object parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtension.Parent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object Parent {
            get { return parent; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtension.Required"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("required", Namespace = ServiceDescription.Namespace), DefaultValue(false)]
        public bool Required {
            get { return required; }
            set { required = value; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionFormatExtension.Handled"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public bool Handled {
            get { return handled; }
            set { handled = value; }
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public enum OperationFlow {
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow.None"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        None,
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow.OneWay"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        OneWay,
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow.Notification"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        Notification,
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow.RequestResponse"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        RequestResponse,
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFlow.SolicitResponse"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        SolicitResponse,
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class OperationMessageCollection : ServiceDescriptionBaseCollection {
        internal OperationMessageCollection(Operation operation) : base(operation) { }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationMessage this[int index] {
            get { return (OperationMessage)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(OperationMessage operationMessage) {
            return List.Add(operationMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, OperationMessage operationMessage) {
            List.Insert(index, operationMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(OperationMessage operationMessage) {
            return List.IndexOf(operationMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(OperationMessage operationMessage) {
            return List.Contains(operationMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(OperationMessage operationMessage) {
            List.Remove(operationMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(OperationMessage[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Input"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationInput Input {
            get { 
                for (int i = 0; i < List.Count; i++) {
                    OperationInput input = List[i] as OperationInput;
                    if (input != null) {
                        return input;
                    }
                }
                return null;
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Output"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationOutput Output {
            get {
                for (int i = 0; i < List.Count; i++) {
                    OperationOutput output = List[i] as OperationOutput;
                    if (output != null) {
                        return output;
                    }
                }
                return null;
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.Flow"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationFlow Flow {
            get {
                if (List.Count == 0) {
                    return OperationFlow.None;
                }
                else if (List.Count == 1) {
                    if (List[0] is OperationInput) {
                        return OperationFlow.OneWay;
                    }
                    else {
                        return OperationFlow.Notification;
                    }
                }
                else {
                    if (List[0] is OperationInput) {
                        return OperationFlow.RequestResponse;
                    }
                    else {
                        return OperationFlow.SolicitResponse;
                    }
                }
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((OperationMessage)value).SetParent((Operation)parent);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.OnInsert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnInsert(int index, object value) {
            if (Count > 1 || (Count == 1 && value.GetType() == List[0].GetType()))
                throw new InvalidOperationException(Res.GetString(Res.WebDescriptionTooManyMessages));
            
            base.OnInsert(index, value);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.OnSet"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnSet(int index, object oldValue, object newValue) {
            if (oldValue.GetType() != newValue.GetType()) throw new InvalidOperationException(Res.GetString(Res.WebDescriptionTooManyMessages));
            base.OnSet(index, oldValue, newValue);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationMessageCollection.OnValidate"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnValidate(object value) {
            if (!(value is OperationInput || value is OperationOutput))
                throw new ArgumentException(Res.GetString(Res.OnlyOperationInputOrOperationOutputTypes), "value");
            base.OnValidate(value);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ImportCollection : ServiceDescriptionBaseCollection {
        internal ImportCollection(ServiceDescription serviceDescription) : base(serviceDescription) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Import this[int index] {
            get { return (Import)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Import import) {
            return List.Add(import);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Import import) {
            List.Insert(index, import);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Import import) {
            return List.IndexOf(import);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Import import) {
            return List.Contains(import);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Import import) {
            List.Remove(import);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Import[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ImportCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Import)value).SetParent((ServiceDescription)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class MessageCollection : ServiceDescriptionBaseCollection {
        internal MessageCollection(ServiceDescription serviceDescription) : base(serviceDescription) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message this[int index] {
            get { return (Message)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Message message) {
            return List.Add(message);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Message message) {
            List.Insert(index, message);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Message message) {
            return List.IndexOf(message);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Message message) {
            return List.Contains(message);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Message message) {
            List.Remove(message);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Message[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Message this[string name] {
            get { return (Message)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((Message)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessageCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Message)value).SetParent((ServiceDescription)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class PortCollection : ServiceDescriptionBaseCollection {
        internal PortCollection(Service service) : base(service) { }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Port this[int index] {
            get { return (Port)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Port port) {
            return List.Add(port);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Port port) {
            List.Insert(index, port);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Port port) {
            return List.IndexOf(port);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Port port) {
            return List.Contains(port);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Port port) {
            List.Remove(port);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Port[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Port this[string name] {
            get { return (Port)Table[name]; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((Port)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Port)value).SetParent((Service)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class PortTypeCollection : ServiceDescriptionBaseCollection {
        internal PortTypeCollection(ServiceDescription serviceDescription) : base(serviceDescription) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType this[int index] {
            get { return (PortType)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(PortType portType) {
            return List.Add(portType);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, PortType portType) {
            List.Insert(index, portType);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(PortType portType) {
            return List.IndexOf(portType);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(PortType portType) {
            return List.Contains(portType);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(PortType portType) {
            List.Remove(portType);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(PortType[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PortType this[string name] {
            get { return (PortType)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((PortType)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="PortTypeCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((PortType)value).SetParent((ServiceDescription)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class BindingCollection : ServiceDescriptionBaseCollection {
        internal BindingCollection(ServiceDescription serviceDescription) : base(serviceDescription) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding this[int index] {
            get { return (Binding)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Binding binding) {
            return List.Add(binding);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Binding binding) {
            List.Insert(index, binding);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Binding binding) {
            return List.IndexOf(binding);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Binding binding) {
            return List.Contains(binding);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Binding binding) {
            List.Remove(binding);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Binding[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Binding this[string name] {
            get { return (Binding)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((Binding)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="BindingCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Binding)value).SetParent((ServiceDescription)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ServiceCollection : ServiceDescriptionBaseCollection {
        internal ServiceCollection(ServiceDescription serviceDescription) : base(serviceDescription) { }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Service this[int index] {
            get { return (Service)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Service service) {
            return List.Add(service);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Service service) {
            List.Insert(index, service);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Service service) {
            return List.IndexOf(service);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Service service) {
            return List.Contains(service);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Service service) {
            List.Remove(service);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Service[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Service this[string name] {
            get { return (Service)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((Service)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Service)value).SetParent((ServiceDescription)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class MessagePartCollection : ServiceDescriptionBaseCollection {
        internal MessagePartCollection(Message message) : base(message) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MessagePart this[int index] {
            get { return (MessagePart)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(MessagePart messagePart) {
            return List.Add(messagePart);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, MessagePart messagePart) {
            List.Insert(index, messagePart);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(MessagePart messagePart) {
            return List.IndexOf(messagePart);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(MessagePart messagePart) {
            return List.Contains(messagePart);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(MessagePart messagePart) {
            List.Remove(messagePart);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(MessagePart[] array, int index) {
            List.CopyTo(array, index);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public MessagePart this[string name] {
            get { return (MessagePart)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((MessagePart)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="MessagePartCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((MessagePart)value).SetParent((Message)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class OperationBindingCollection : ServiceDescriptionBaseCollection {
        internal OperationBindingCollection(Binding binding) : base(binding) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationBinding this[int index] {
            get { return (OperationBinding)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(OperationBinding bindingOperation) {
            return List.Add(bindingOperation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, OperationBinding bindingOperation) {
            List.Insert(index, bindingOperation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(OperationBinding bindingOperation) {
            return List.IndexOf(bindingOperation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(OperationBinding bindingOperation) {
            return List.Contains(bindingOperation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(OperationBinding bindingOperation) {
            List.Remove(bindingOperation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(OperationBinding[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationBindingCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((OperationBinding)value).SetParent((Binding)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class FaultBindingCollection : ServiceDescriptionBaseCollection {
        internal FaultBindingCollection(OperationBinding operationBinding) : base(operationBinding) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public FaultBinding this[int index] {
            get { return (FaultBinding)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(FaultBinding bindingOperationFault) {
            return List.Add(bindingOperationFault);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, FaultBinding bindingOperationFault) {
            List.Insert(index, bindingOperationFault);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(FaultBinding bindingOperationFault) {
            return List.IndexOf(bindingOperationFault);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(FaultBinding bindingOperationFault) {
            return List.Contains(bindingOperationFault);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(FaultBinding bindingOperationFault) {
            List.Remove(bindingOperationFault);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(FaultBinding[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public FaultBinding this[string name] {
            get { return (FaultBinding)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((FaultBinding)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="FaultBindingCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((FaultBinding)value).SetParent((OperationBinding)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class OperationCollection : ServiceDescriptionBaseCollection {
        internal OperationCollection(PortType portType) : base(portType) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Operation this[int index] {
            get { return (Operation)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(Operation operation) {
            return List.Add(operation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, Operation operation) {
            List.Insert(index, operation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(Operation operation) {
            return List.IndexOf(operation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(Operation operation) {
            return List.Contains(operation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(Operation operation) {
            List.Remove(operation);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Operation[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((Operation)value).SetParent((PortType)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class OperationFaultCollection : ServiceDescriptionBaseCollection {
        internal OperationFaultCollection(Operation operation) : base(operation) { }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.this"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationFault this[int index] {
            get { return (OperationFault)List[index]; }
            set { List[index] = value; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.Add"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Add(OperationFault operationFaultMessage) {
            return List.Add(operationFaultMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.Insert"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Insert(int index, OperationFault operationFaultMessage) {
            List.Insert(index, operationFaultMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.IndexOf"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(OperationFault operationFaultMessage) {
            return List.IndexOf(operationFaultMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.Contains"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(OperationFault operationFaultMessage) {
            return List.Contains(operationFaultMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.Remove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Remove(OperationFault operationFaultMessage) {
            List.Remove(operationFaultMessage);
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.CopyTo"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(OperationFault[] array, int index) {
            List.CopyTo(array, index);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.this1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public OperationFault this[string name] {
            get { return (OperationFault)Table[name]; }
        }
        
        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override string GetKey(object value) {
            return ((OperationFault)value).Name;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="OperationFaultCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void SetParent(object value, object parent) {
            ((OperationFault)value).SetParent((Operation)parent);
        }
    }

    /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public abstract class ServiceDescriptionBaseCollection : CollectionBase {
        Hashtable table; // 
        object parent;

        internal ServiceDescriptionBaseCollection(object parent) {
            this.parent = parent;
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.Table"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual IDictionary Table { 
            get { if (table == null) table = new Hashtable(); return table; }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.GetKey"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual string GetKey(object value) {
            return null; // returning null means there is no key
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.SetParent"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected virtual void SetParent(object value, object parent) {
            // default is that the item has no parent
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.OnInsertComplete"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnInsertComplete(int index, object value) {
            AddValue(value);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.OnRemove"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnRemove(int index, object value) {
            RemoveValue(value);
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.OnClear"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnClear() {
            for (int i = 0; i < List.Count; i++) {
                RemoveValue(List[i]);
            }
        }

        /// <include file='doc\ServiceDescription.uex' path='docs/doc[@for="ServiceDescriptionBaseCollection.OnSet"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override void OnSet(int index, object oldValue, object newValue) {
            RemoveValue(oldValue);
            AddValue(newValue);
        }
       
        void AddValue(object value) {
            string key = GetKey(value);
            if (key != null) {
                try {
                    Table.Add(key, value);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    if (Table[key] != null) {
                        throw new ArgumentException(GetDuplicateMessage(value.GetType(), key), e.InnerException);
                    }
                    else {
                        throw e;
                    }
                }
            }
            SetParent(value, parent);
        }

        void RemoveValue(object value) {
            string key = GetKey(value);
            if (key != null) Table.Remove(key);
            SetParent(value, null);
        }

        static string GetDuplicateMessage(Type type, string elemName) {
            string message = null;
            if (type == typeof(ServiceDescriptionFormatExtension)) 
                message = Res.GetString(Res.WebDuplicateFormatExtension, elemName);
            else if (type == typeof(OperationMessage)) 
                message = Res.GetString(Res.WebDuplicateOperationMessage, elemName);
            else if (type == typeof(Import)) 
                message = Res.GetString(Res.WebDuplicateImport, elemName);
            else if (type == typeof(Message)) 
                message = Res.GetString(Res.WebDuplicateMessage, elemName);
            else if (type == typeof(Port)) 
                message = Res.GetString(Res.WebDuplicatePort, elemName);
            else if (type == typeof(PortType)) 
                message = Res.GetString(Res.WebDuplicatePortType, elemName);
            else if (type == typeof(Binding)) 
                message = Res.GetString(Res.WebDuplicateBinding, elemName);
            else if (type == typeof(Service)) 
                message = Res.GetString(Res.WebDuplicateService, elemName);
            else if (type == typeof(MessagePart)) 
                message = Res.GetString(Res.WebDuplicateMessagePart, elemName);
            else if (type == typeof(OperationBinding)) 
                message = Res.GetString(Res.WebDuplicateOperationBinding, elemName);
            else if (type == typeof(FaultBinding)) 
                message = Res.GetString(Res.WebDuplicateFaultBinding, elemName);
            else if (type == typeof(Operation)) 
                message = Res.GetString(Res.WebDuplicateOperation, elemName);
            else if (type == typeof(OperationFault)) 
                message = Res.GetString(Res.WebDuplicateOperationFault, elemName);
            else
                message = Res.GetString(Res.WebDuplicateUnknownElement, type, elemName);

            return message;
        }
    }

    internal class Schemas {
        Schemas() { }
        internal const string Wsdl = @"<?xml version='1.0' encoding='UTF-8' ?> 
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/'
           targetNamespace='http://schemas.xmlsoap.org/wsdl/'
           elementFormDefault='qualified' >
   
  <xs:complexType mixed='true' name='tDocumentation' >
    <xs:sequence>
      <xs:any minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:complexType>

  <xs:complexType name='tDocumented' >
    <xs:annotation>
      <xs:documentation>
      This type is extended by  component types to allow them to be documented
      </xs:documentation>
    </xs:annotation>
    <xs:sequence>
      <xs:element name='documentation' type='wsdl:tDocumentation' minOccurs='0' />
    </xs:sequence>
  </xs:complexType>
 <!-- allow extensibility via elements and attributes on all elements swa124 -->
 <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow attributes from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax' />   
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:complexType name='tExtensibleDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow elements from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
        <xs:anyAttribute namespace='##other' processContents='lax' />   
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <!-- original wsdl removed as part of swa124 resolution
  <xs:complexType name='tExtensibleAttributesDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow attributes from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:anyAttribute namespace='##other' processContents='lax' />    
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tExtensibleDocumented' abstract='true' >
    <xs:complexContent>
      <xs:extension base='wsdl:tDocumented' >
        <xs:annotation>
          <xs:documentation>
          This type is extended by component types to allow elements from other namespaces to be added.
          </xs:documentation>
        </xs:annotation>
        <xs:sequence>
          <xs:any namespace='##other' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
        </xs:sequence>
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
 -->
  <xs:element name='definitions' type='wsdl:tDefinitions' >
    <xs:key name='message' >
      <xs:selector xpath='wsdl:message' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='portType' >
      <xs:selector xpath='wsdl:portType' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='binding' >
      <xs:selector xpath='wsdl:binding' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='service' >
      <xs:selector xpath='wsdl:service' />
      <xs:field xpath='@name' />
    </xs:key>
    <xs:key name='import' >
      <xs:selector xpath='wsdl:import' />
      <xs:field xpath='@namespace' />
    </xs:key>
  </xs:element>

  <xs:group name='anyTopLevelOptionalElement' >
    <xs:annotation>
      <xs:documentation>
      Any top level optional element allowed to appear more then once - any child of definitions element except wsdl:types. Any extensibility element is allowed in any place.
      </xs:documentation>
    </xs:annotation>
    <xs:choice>
      <xs:element name='import' type='wsdl:tImport' />
      <xs:element name='types' type='wsdl:tTypes' />                     
      <xs:element name='message'  type='wsdl:tMessage' >
        <xs:unique name='part' >
          <xs:selector xpath='wsdl:part' />
          <xs:field xpath='@name' />
        </xs:unique>
      </xs:element>
      <xs:element name='portType' type='wsdl:tPortType' />
      <xs:element name='binding'  type='wsdl:tBinding' />
      <xs:element name='service'  type='wsdl:tService' >
        <xs:unique name='port' >
          <xs:selector xpath='wsdl:port' />
          <xs:field xpath='@name' />
        </xs:unique>
      </xs:element>
    </xs:choice>
  </xs:group>

  <xs:complexType name='tDefinitions' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:group ref='wsdl:anyTopLevelOptionalElement'  minOccurs='0'   maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='targetNamespace' type='xs:anyURI' use='optional' />
        <xs:attribute name='name' type='xs:NCName' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
   
  <xs:complexType name='tImport' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='namespace' type='xs:anyURI' use='required' />
        <xs:attribute name='location' type='xs:anyURI' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
   
  <xs:complexType name='tTypes' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' />
    </xs:complexContent>   
  </xs:complexType>
     
  <xs:complexType name='tMessage' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='part' type='wsdl:tPart' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>

  <xs:complexType name='tPart' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='element' type='xs:QName' use='optional' />
        <xs:attribute name='type' type='xs:QName' use='optional' />    
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>

  <xs:complexType name='tPortType' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:sequence>
          <xs:element name='operation' type='wsdl:tOperation' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>
   
  <xs:complexType name='tOperation' >
    <xs:complexContent>   
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:choice>
            <xs:group ref='wsdl:request-response-or-one-way-operation' />
            <xs:group ref='wsdl:solicit-response-or-notification-operation' />
          </xs:choice>
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='parameterOrder' type='xs:NMTOKENS' use='optional' />
      </xs:extension>
    </xs:complexContent>   
  </xs:complexType>
    
  <xs:group name='request-response-or-one-way-operation' >
    <xs:sequence>
      <xs:element name='input' type='wsdl:tParam' />
      <xs:sequence minOccurs='0' >
        <xs:element name='output' type='wsdl:tParam' />
        <xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />
      </xs:sequence>
    </xs:sequence>
  </xs:group>

  <xs:group name='solicit-response-or-notification-operation' >
    <xs:sequence>
      <xs:element name='output' type='wsdl:tParam' />
      <xs:sequence minOccurs='0' >
        <xs:element name='input' type='wsdl:tParam' />
        <xs:element name='fault' type='wsdl:tFault' minOccurs='0' maxOccurs='unbounded' />
      </xs:sequence>
    </xs:sequence>
  </xs:group>
        
  <xs:complexType name='tParam' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='optional' />
        <xs:attribute name='message' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tFault' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleAttributesDocumented' >
        <xs:attribute name='name' type='xs:NCName'  use='required' />
        <xs:attribute name='message' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tBinding' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='operation' type='wsdl:tBindingOperation' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='type' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
    
  <xs:complexType name='tBindingOperationMessage' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  
  <xs:complexType name='tBindingOperationFault' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:complexType name='tBindingOperation' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='input' type='wsdl:tBindingOperationMessage' minOccurs='0' />
          <xs:element name='output' type='wsdl:tBindingOperationMessage' minOccurs='0' />
          <xs:element name='fault' type='wsdl:tBindingOperationFault' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tService' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:sequence>
          <xs:element name='port' type='wsdl:tPort' minOccurs='0' maxOccurs='unbounded' />
        </xs:sequence>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
     
  <xs:complexType name='tPort' >
    <xs:complexContent>
      <xs:extension base='wsdl:tExtensibleDocumented' >
        <xs:attribute name='name' type='xs:NCName' use='required' />
        <xs:attribute name='binding' type='xs:QName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>

  <xs:attribute name='arrayType' type='xs:string' />
  <xs:attribute name='required' type='xs:boolean' />
  <xs:complexType name='tExtensibilityElement' abstract='true' >
    <xs:attribute ref='wsdl:required' use='optional' />
  </xs:complexType>

</xs:schema>";

        internal const string Soap = @"<?xml version='1.0' encoding='UTF-8' ?> 
<xs:schema xmlns:soap='http://schemas.xmlsoap.org/wsdl/soap/' xmlns:wsdl='http://schemas.xmlsoap.org/wsdl/' targetNamespace='http://schemas.xmlsoap.org/wsdl/soap/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:import namespace='http://schemas.xmlsoap.org/wsdl/' />
  <xs:simpleType name='encodingStyle'>
    <xs:annotation>
      <xs:documentation>
      'encodingStyle' indicates any canonicalization conventions followed in the contents of the containing element.  For example, the value 'http://schemas.xmlsoap.org/soap/encoding/' indicates the pattern described in SOAP specification
      </xs:documentation>
    </xs:annotation>
    <xs:list itemType='xs:anyURI' />
  </xs:simpleType>
  <xs:element name='binding' type='soap:tBinding' />
  <xs:complexType name='tBinding'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='transport' type='xs:anyURI' use='required' />
        <xs:attribute name='style' type='soap:tStyleChoice' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:simpleType name='tStyleChoice'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='rpc' />
      <xs:enumeration value='document' />
    </xs:restriction>
  </xs:simpleType>
  <xs:element name='operation' type='soap:tOperation' />
  <xs:complexType name='tOperation'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='soapAction' type='xs:anyURI' use='optional' />
        <xs:attribute name='style' type='soap:tStyleChoice' use='optional' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='body' type='soap:tBody' />
  <xs:attributeGroup name='tBodyAttributes'>
    <xs:attribute name='encodingStyle' type='soap:encodingStyle' use='optional' />
    <xs:attribute name='use' type='soap:useChoice' use='optional' />
    <xs:attribute name='namespace' type='xs:anyURI' use='optional' />
  </xs:attributeGroup>
  <xs:complexType name='tBody'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='parts' type='xs:NMTOKENS' use='optional' />
        <xs:attributeGroup ref='soap:tBodyAttributes' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:simpleType name='useChoice'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='literal' />
      <xs:enumeration value='encoded' />
    </xs:restriction>
  </xs:simpleType>
  <xs:element name='fault' type='soap:tFault' />
  <xs:complexType name='tFaultRes' abstract='true'>
    <xs:complexContent mixed='false'>
      <xs:restriction base='soap:tBody'>
        <xs:attribute ref='wsdl:required' use='optional' />
        <xs:attribute name='parts' type='xs:NMTOKENS' use='prohibited' />
        <xs:attributeGroup ref='soap:tBodyAttributes' />
      </xs:restriction>
    </xs:complexContent>
  </xs:complexType>
  <xs:complexType name='tFault'>
    <xs:complexContent mixed='false'>
      <xs:extension base='soap:tFaultRes'>
        <xs:attribute name='name' type='xs:NCName' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='header' type='soap:tHeader' />
  <xs:attributeGroup name='tHeaderAttributes'>
    <xs:attribute name='message' type='xs:QName' use='required' />
    <xs:attribute name='part' type='xs:NMTOKEN' use='required' />
    <xs:attribute name='use' type='soap:useChoice' use='required' />
    <xs:attribute name='encodingStyle' type='soap:encodingStyle' use='optional' />
    <xs:attribute name='namespace' type='xs:anyURI' use='optional' />
  </xs:attributeGroup>
  <xs:complexType name='tHeader'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:sequence>
          <xs:element minOccurs='0' maxOccurs='unbounded' ref='soap:headerfault' />
        </xs:sequence>
        <xs:attributeGroup ref='soap:tHeaderAttributes' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
  <xs:element name='headerfault' type='soap:tHeaderFault' />
  <xs:complexType name='tHeaderFault'>
    <xs:attributeGroup ref='soap:tHeaderAttributes' />
  </xs:complexType>
  <xs:element name='address' type='soap:tAddress' />
  <xs:complexType name='tAddress'>
    <xs:complexContent mixed='false'>
      <xs:extension base='wsdl:tExtensibilityElement'>
        <xs:attribute name='location' type='xs:anyURI' use='required' />
      </xs:extension>
    </xs:complexContent>
  </xs:complexType>
</xs:schema>";

        internal const string WebRef = @"<?xml version='1.0' encoding='UTF-8' ?>
<xs:schema xmlns:tns='http://microsoft.com/webReference/' elementFormDefault='qualified' targetNamespace='http://microsoft.com/webReference/' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:simpleType name='options'>
    <xs:list>
      <xs:simpleType>
        <xs:restriction base='xs:string'>
          <xs:enumeration value='properties' />
          <xs:enumeration value='newAsync' />
          <xs:enumeration value='oldAsync' />
          <xs:enumeration value='order' />
          <xs:enumeration value='enableDataBinding' />
        </xs:restriction>
      </xs:simpleType>
    </xs:list>
  </xs:simpleType>
  <xs:simpleType name='style'>
    <xs:restriction base='xs:string'>
      <xs:enumeration value='client' />
      <xs:enumeration value='server' />
      <xs:enumeration value='serverInterface' />
    </xs:restriction>
  </xs:simpleType>
  <xs:complexType name='webReferenceOptions'>
    <xs:all>
      <xs:element minOccurs='0' default='oldAsync' name='codeGenerationOptions' type='tns:options' />
      <xs:element minOccurs='0' default='client' name='style' type='tns:style' />
      <xs:element minOccurs='0' default='false' name='verbose' type='xs:boolean' />
      <xs:element minOccurs='0' name='schemaImporterExtensions'>
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs='0' maxOccurs='unbounded' name='type' type='xs:string' />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
  <xs:element name='webReferenceOptions' type='tns:webReferenceOptions' />
  <xs:complexType name='wsdlParameters'>
    <xs:all>
      <xs:element minOccurs='0' name='appSettingBaseUrl' type='xs:string' />
      <xs:element minOccurs='0' name='appSettingUrlKey' type='xs:string' />
      <xs:element minOccurs='0' name='domain' type='xs:string' />
      <xs:element minOccurs='0' name='out' type='xs:string' />
      <xs:element minOccurs='0' name='password' type='xs:string' />
      <xs:element minOccurs='0' name='proxy' type='xs:string' />
      <xs:element minOccurs='0' name='proxydomain' type='xs:string' />
      <xs:element minOccurs='0' name='proxypassword' type='xs:string' />
      <xs:element minOccurs='0' name='proxyusername' type='xs:string' />
      <xs:element minOccurs='0' name='username' type='xs:string' />
      <xs:element minOccurs='0' name='namespace' type='xs:string' />
      <xs:element minOccurs='0' name='language' type='xs:string' />
      <xs:element minOccurs='0' name='protocol' type='xs:string' />
      <xs:element minOccurs='0' name='nologo' type='xs:boolean' />
      <xs:element minOccurs='0' name='parsableerrors' type='xs:boolean' />
      <xs:element minOccurs='0' name='sharetypes' type='xs:boolean' />
      <xs:element minOccurs='0' name='webReferenceOptions' type='tns:webReferenceOptions' />
      <xs:element minOccurs='0' name='documents'>
        <xs:complexType>
          <xs:sequence>
            <xs:element minOccurs='0' maxOccurs='unbounded' name='document' type='xs:string' />
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
  <xs:element name='wsdlParameters' type='tns:wsdlParameters' />
</xs:schema>";

        internal const string SoapEncoding = @"<?xml version='1.0' encoding='UTF-8' ?>
<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'
           xmlns:tns='http://schemas.xmlsoap.org/soap/encoding/'
           targetNamespace='http://schemas.xmlsoap.org/soap/encoding/' >
        
 <xs:attribute name='root' >
   <xs:simpleType>
     <xs:restriction base='xs:boolean'>
       <xs:pattern value='0|1' />
     </xs:restriction>
   </xs:simpleType>
 </xs:attribute>

  <xs:attributeGroup name='commonAttributes' >
    <xs:attribute name='id' type='xs:ID' />
    <xs:attribute name='href' type='xs:anyURI' />
    <xs:anyAttribute namespace='##other' processContents='lax' />
  </xs:attributeGroup>
   
  <xs:simpleType name='arrayCoordinate' >
    <xs:restriction base='xs:string' />
  </xs:simpleType>
          
  <xs:attribute name='arrayType' type='xs:string' />
  <xs:attribute name='offset' type='tns:arrayCoordinate' />
  
  <xs:attributeGroup name='arrayAttributes' >
    <xs:attribute ref='tns:arrayType' />
    <xs:attribute ref='tns:offset' />
  </xs:attributeGroup>    
  
  <xs:attribute name='position' type='tns:arrayCoordinate' /> 
  
  <xs:attributeGroup name='arrayMemberAttributes' >
    <xs:attribute ref='tns:position' />
  </xs:attributeGroup>    

  <xs:group name='Array' >
    <xs:sequence>
      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:group>

  <xs:element name='Array' type='tns:Array' />
  <xs:complexType name='Array' >
    <xs:group ref='tns:Array' minOccurs='0' />
    <xs:attributeGroup ref='tns:arrayAttributes' />
    <xs:attributeGroup ref='tns:commonAttributes' />
  </xs:complexType> 
  <xs:element name='Struct' type='tns:Struct' />
  <xs:group name='Struct' >
    <xs:sequence>
      <xs:any namespace='##any' minOccurs='0' maxOccurs='unbounded' processContents='lax' />
    </xs:sequence>
  </xs:group>

  <xs:complexType name='Struct' >
    <xs:group ref='tns:Struct' minOccurs='0' />
    <xs:attributeGroup ref='tns:commonAttributes'/>
  </xs:complexType> 
  
  <xs:simpleType name='base64' >
    <xs:restriction base='xs:base64Binary' />
  </xs:simpleType>

  <xs:element name='duration' type='tns:duration' />
  <xs:complexType name='duration' >
    <xs:simpleContent>
      <xs:extension base='xs:duration' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='dateTime' type='tns:dateTime' />
  <xs:complexType name='dateTime' >
    <xs:simpleContent>
      <xs:extension base='xs:dateTime' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>



  <xs:element name='NOTATION' type='tns:NOTATION' />
  <xs:complexType name='NOTATION' >
    <xs:simpleContent>
      <xs:extension base='xs:QName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  

  <xs:element name='time' type='tns:time' />
  <xs:complexType name='time' >
    <xs:simpleContent>
      <xs:extension base='xs:time' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='date' type='tns:date' />
  <xs:complexType name='date' >
    <xs:simpleContent>
      <xs:extension base='xs:date' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gYearMonth' type='tns:gYearMonth' />
  <xs:complexType name='gYearMonth' >
    <xs:simpleContent>
      <xs:extension base='xs:gYearMonth' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gYear' type='tns:gYear' />
  <xs:complexType name='gYear' >
    <xs:simpleContent>
      <xs:extension base='xs:gYear' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gMonthDay' type='tns:gMonthDay' />
  <xs:complexType name='gMonthDay' >
    <xs:simpleContent>
      <xs:extension base='xs:gMonthDay' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gDay' type='tns:gDay' />
  <xs:complexType name='gDay' >
    <xs:simpleContent>
      <xs:extension base='xs:gDay' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='gMonth' type='tns:gMonth' />
  <xs:complexType name='gMonth' >
    <xs:simpleContent>
      <xs:extension base='xs:gMonth' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>
  
  <xs:element name='boolean' type='tns:boolean' />
  <xs:complexType name='boolean' >
    <xs:simpleContent>
      <xs:extension base='xs:boolean' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='base64Binary' type='tns:base64Binary' />
  <xs:complexType name='base64Binary' >
    <xs:simpleContent>
      <xs:extension base='xs:base64Binary' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='hexBinary' type='tns:hexBinary' />
  <xs:complexType name='hexBinary' >
    <xs:simpleContent>
     <xs:extension base='xs:hexBinary' >
       <xs:attributeGroup ref='tns:commonAttributes' />
     </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='float' type='tns:float' />
  <xs:complexType name='float' >
    <xs:simpleContent>
      <xs:extension base='xs:float' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='double' type='tns:double' />
  <xs:complexType name='double' >
    <xs:simpleContent>
      <xs:extension base='xs:double' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='anyURI' type='tns:anyURI' />
  <xs:complexType name='anyURI' >
    <xs:simpleContent>
      <xs:extension base='xs:anyURI' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='QName' type='tns:QName' />
  <xs:complexType name='QName' >
    <xs:simpleContent>
      <xs:extension base='xs:QName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  
  <xs:element name='string' type='tns:string' />
  <xs:complexType name='string' >
    <xs:simpleContent>
      <xs:extension base='xs:string' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='normalizedString' type='tns:normalizedString' />
  <xs:complexType name='normalizedString' >
    <xs:simpleContent>
      <xs:extension base='xs:normalizedString' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='token' type='tns:token' />
  <xs:complexType name='token' >
    <xs:simpleContent>
      <xs:extension base='xs:token' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='language' type='tns:language' />
  <xs:complexType name='language' >
    <xs:simpleContent>
      <xs:extension base='xs:language' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='Name' type='tns:Name' />
  <xs:complexType name='Name' >
    <xs:simpleContent>
      <xs:extension base='xs:Name' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NMTOKEN' type='tns:NMTOKEN' />
  <xs:complexType name='NMTOKEN' >
    <xs:simpleContent>
      <xs:extension base='xs:NMTOKEN' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NCName' type='tns:NCName' />
  <xs:complexType name='NCName' >
    <xs:simpleContent>
      <xs:extension base='xs:NCName' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='NMTOKENS' type='tns:NMTOKENS' />
  <xs:complexType name='NMTOKENS' >
    <xs:simpleContent>
      <xs:extension base='xs:NMTOKENS' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ID' type='tns:ID' />
  <xs:complexType name='ID' >
    <xs:simpleContent>
      <xs:extension base='xs:ID' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='IDREF' type='tns:IDREF' />
  <xs:complexType name='IDREF' >
    <xs:simpleContent>
      <xs:extension base='xs:IDREF' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ENTITY' type='tns:ENTITY' />
  <xs:complexType name='ENTITY' >
    <xs:simpleContent>
      <xs:extension base='xs:ENTITY' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='IDREFS' type='tns:IDREFS' />
  <xs:complexType name='IDREFS' >
    <xs:simpleContent>
      <xs:extension base='xs:IDREFS' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='ENTITIES' type='tns:ENTITIES' />
  <xs:complexType name='ENTITIES' >
    <xs:simpleContent>
      <xs:extension base='xs:ENTITIES' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='decimal' type='tns:decimal' />
  <xs:complexType name='decimal' >
    <xs:simpleContent>
      <xs:extension base='xs:decimal' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='integer' type='tns:integer' />
  <xs:complexType name='integer' >
    <xs:simpleContent>
      <xs:extension base='xs:integer' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='nonPositiveInteger' type='tns:nonPositiveInteger' />
  <xs:complexType name='nonPositiveInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:nonPositiveInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='negativeInteger' type='tns:negativeInteger' />
  <xs:complexType name='negativeInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:negativeInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='long' type='tns:long' />
  <xs:complexType name='long' >
    <xs:simpleContent>
      <xs:extension base='xs:long' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='int' type='tns:int' />
  <xs:complexType name='int' >
    <xs:simpleContent>
      <xs:extension base='xs:int' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='short' type='tns:short' />
  <xs:complexType name='short' >
    <xs:simpleContent>
      <xs:extension base='xs:short' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='byte' type='tns:byte' />
  <xs:complexType name='byte' >
    <xs:simpleContent>
      <xs:extension base='xs:byte' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='nonNegativeInteger' type='tns:nonNegativeInteger' />
  <xs:complexType name='nonNegativeInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:nonNegativeInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedLong' type='tns:unsignedLong' />
  <xs:complexType name='unsignedLong' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedLong' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedInt' type='tns:unsignedInt' />
  <xs:complexType name='unsignedInt' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedInt' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedShort' type='tns:unsignedShort' />
  <xs:complexType name='unsignedShort' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedShort' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='unsignedByte' type='tns:unsignedByte' />
  <xs:complexType name='unsignedByte' >
    <xs:simpleContent>
      <xs:extension base='xs:unsignedByte' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='positiveInteger' type='tns:positiveInteger' />
  <xs:complexType name='positiveInteger' >
    <xs:simpleContent>
      <xs:extension base='xs:positiveInteger' >
        <xs:attributeGroup ref='tns:commonAttributes' />
      </xs:extension>
    </xs:simpleContent>
  </xs:complexType>

  <xs:element name='anyType' />
</xs:schema>";
    }
}
