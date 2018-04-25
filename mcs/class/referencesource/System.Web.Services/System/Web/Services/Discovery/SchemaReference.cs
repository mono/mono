//------------------------------------------------------------------------------
// <copyright file="SchemaReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.IO;
    using System.Web.Services.Protocols;
    using System.ComponentModel;
    using System.Text;
    using System.Threading;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlRoot("schemaRef", Namespace = SchemaReference.Namespace)]
    public sealed class SchemaReference : DiscoveryReference {

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.Namespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public const string Namespace = "http://schemas.xmlsoap.org/disco/schema/";

        private string reference;
        private string targetNamespace;

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.SchemaReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SchemaReference() {
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.SchemaReference1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SchemaReference(string url) {
            Ref = url;
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.Ref"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("ref")]
        public string Ref {
            get {
                return reference == null ? "" : reference;
            }
            set {
                reference = value;
            }
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.TargetNamespace"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("targetNamespace"), DefaultValue(null)]
        public string TargetNamespace {
            get { return targetNamespace; }
            set { targetNamespace = value; }
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override string Url {
            get { return Ref; }
            set { Ref = value; }
        }

        internal XmlSchema GetSchema() {
            try {
                return Schema;
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                // don't let the exception out - keep going. Just add it to the list of errors.
                ClientProtocol.Errors[Url] = e;
                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "GetSchema", e);
            }
            return null;
        }

        internal override void LoadExternals(Hashtable loadedExternals) {
            LoadExternals(GetSchema(), Url, ClientProtocol, loadedExternals);
        }

        internal static void LoadExternals(XmlSchema schema, string url, DiscoveryClientProtocol client, Hashtable loadedExternals) {
            if (schema == null)
                return;
            foreach (XmlSchemaExternal external in schema.Includes) {
                if (external.SchemaLocation == null || external.SchemaLocation.Length == 0 || external.Schema != null)
                    continue;
                if (external is XmlSchemaInclude || external is XmlSchemaRedefine) {
                    string location = UriToString(url, external.SchemaLocation);
                    if (client.References[location] is SchemaReference) {
                        SchemaReference externalRef = (SchemaReference)client.References[location];
                        external.Schema = externalRef.GetSchema();
                        if (external.Schema != null)
                            loadedExternals[location] = external.Schema;
                        externalRef.LoadExternals(loadedExternals);
                    }
                }
            }
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.WriteDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteDocument(object document, Stream stream) {
            ((XmlSchema)document).Write(new StreamWriter(stream, new UTF8Encoding(false)));
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.ReadDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object ReadDocument(Stream stream) {
            XmlTextReader reader = new XmlTextReader(this.Url, stream);
            reader.XmlResolver = null;
            return XmlSchema.Read(reader, null);
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.DefaultFilename"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override string DefaultFilename {
            get {
                string fileName = MakeValidFilename(Schema.Id);
                if (fileName == null || fileName.Length == 0) {
                    fileName = FilenameFromUrl(Url);
                }
                return Path.ChangeExtension(fileName, ".xsd");
            }
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.Schema"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public XmlSchema Schema {
            get {
                if (ClientProtocol == null)
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingClientProtocol));

                object document = ClientProtocol.InlinedSchemas[Url];
                if (document == null) {
                    document = ClientProtocol.Documents[Url];
                }
                if (document == null) {
                    Resolve();
                    document = ClientProtocol.Documents[Url];
                }
                XmlSchema schema = document as XmlSchema;
                if (schema == null) {
                    throw new InvalidOperationException(Res.GetString(Res.WebInvalidDocType, 
                                                      typeof(XmlSchema).FullName,
                                                      document == null ? string.Empty : document.GetType().FullName,
                                                      Url));
                }
                return schema;
            }
        }

        /// <include file='doc\SchemaReference.uex' path='docs/doc[@for="SchemaReference.Resolve"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected internal override void Resolve(string contentType, Stream stream) {
            if (ContentType.IsHtml(contentType))
                ClientProtocol.Errors[Url] = new InvalidContentTypeException(Res.GetString(Res.WebInvalidContentType, contentType), contentType);
            XmlSchema schema = ClientProtocol.Documents[Url] as XmlSchema;
            if ( schema == null ) {
                if (ClientProtocol.Errors[Url] != null)
                    throw (Exception)ClientProtocol.Errors[Url];
                schema = (XmlSchema)ReadDocument(stream);
                ClientProtocol.Documents[Url] = schema;
            }
            if (ClientProtocol.References[Url] != this)
                ClientProtocol.References[Url] = this;
            // now resolve references in the schema.
            foreach (XmlSchemaExternal external in schema.Includes) {
                string location = null;
                try {
                    if (external.SchemaLocation != null && external.SchemaLocation.Length > 0) {
                        location = UriToString(Url, external.SchemaLocation);
                        SchemaReference externalRef = new SchemaReference(location);
                        externalRef.ClientProtocol = ClientProtocol;
                        ClientProtocol.References[location] = externalRef;
                        externalRef.Resolve();
                    }
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw new InvalidDocumentContentsException(Res.GetString(Res.TheSchemaDocumentContainsLinksThatCouldNotBeResolved, location), e);
                }
            }
            return;
        }
    }
}
