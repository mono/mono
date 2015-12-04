//------------------------------------------------------------------------------
// <copyright file="DiscoveryDocument.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System.Xml.Serialization;
    using System.Xml;
    using System.IO;
    using System;
    using System.Text;
    using System.Collections;
    using System.Web.Services.Configuration;

    /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlRoot("discovery", Namespace = DiscoveryDocument.Namespace)]
    public sealed class DiscoveryDocument {

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = "http://schemas.xmlsoap.org/disco/";

        private ArrayList references = new ArrayList();

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.DiscoveryDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryDocument() {
        }

        // NOTE, [....]: This property is not really ignored by the xml serializer. Instead,
        // the attributes that would go here are configured in WebServicesConfiguration's
        // DiscoveryDocumentSerializer property.
        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.References"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public IList References {
            get {
                return references;
            }
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Read"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static DiscoveryDocument Read(Stream stream) {
            XmlTextReader r = new XmlTextReader(stream);
            r.WhitespaceHandling = WhitespaceHandling.Significant;
            r.XmlResolver = null;
            r.DtdProcessing = DtdProcessing.Prohibit;
            return Read(r);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Read1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static DiscoveryDocument Read(TextReader reader) {
            XmlTextReader r = new XmlTextReader(reader);
            r.WhitespaceHandling = WhitespaceHandling.Significant;
            r.XmlResolver = null;
            r.DtdProcessing = DtdProcessing.Prohibit;
            return Read(r);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Read2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static DiscoveryDocument Read(XmlReader xmlReader) {
            return (DiscoveryDocument) WebServicesSection.Current.DiscoveryDocumentSerializer.Deserialize(xmlReader);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.CanRead"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static bool CanRead(XmlReader xmlReader) {
            return WebServicesSection.Current.DiscoveryDocumentSerializer.CanDeserialize(xmlReader);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Write"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(TextWriter writer) {
            XmlTextWriter xmlWriter = new XmlTextWriter(writer);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = 2;
            Write(xmlWriter);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Write1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(Stream stream) {
            TextWriter writer = new StreamWriter(stream, new UTF8Encoding(false));
            Write(writer);
        }

        /// <include file='doc\DiscoveryDocument.uex' path='docs/doc[@for="DiscoveryDocument.Write2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Write(XmlWriter writer) {
            XmlSerializer serializer = WebServicesSection.Current.DiscoveryDocumentSerializer;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            serializer.Serialize(writer, this, ns);
        }


    }


    // This is a special serializer that hardwires to the generated
    // ServiceDescriptionSerializer. To regenerate the serializer
    // Turn on KEEPTEMPFILES 
    // Restart server
    // Run disco as follows
    //   disco <URL_FOR_VALID_ASMX_FILE>?disco
    // Goto windows temp dir (usually \winnt\temp)
    // and get the latest generated .cs file
    // Change namespace to 'System.Web.Services.Discovery'
    // Change class names to DiscoveryDocumentSerializationWriter
    // and DiscoveryDocumentSerializationReader
    // Make the classes internal
    // Ensure the public Write method is Write10_discovery (If not
    // change Serialize to call the new one)
    // Ensure the public Read method is Read11_discovery (If not
    // change Deserialize to call the new one)
    internal class DiscoveryDocumentSerializer : XmlSerializer {
        protected override XmlSerializationReader CreateReader() {
            return new DiscoveryDocumentSerializationReader();
        }
        protected override XmlSerializationWriter CreateWriter() {
            return new DiscoveryDocumentSerializationWriter();
        }
        public override bool CanDeserialize(System.Xml.XmlReader xmlReader) {
            return xmlReader.IsStartElement("discovery", "http://schemas.xmlsoap.org/disco/");
        }
        protected override void Serialize(Object objectToSerialize, XmlSerializationWriter writer) {
            ((DiscoveryDocumentSerializationWriter)writer).Write10_discovery(objectToSerialize);
        }
        protected override object Deserialize(XmlSerializationReader reader) {
            return ((DiscoveryDocumentSerializationReader)reader).Read10_discovery();
        }
    }
}
