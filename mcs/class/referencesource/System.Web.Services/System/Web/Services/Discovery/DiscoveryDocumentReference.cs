//------------------------------------------------------------------------------
// <copyright file="DiscoveryDocumentReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Discovery {

    using System;
    using System.Net;
    using System.Xml;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Serialization;
    using System.Web.Services.Protocols;
    using System.Web.Services.Configuration;
    using System.Text;
    using System.Globalization;
    using System.Threading;
    using System.Collections;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlRoot("discoveryRef", Namespace = DiscoveryDocument.Namespace)]
    public sealed class DiscoveryDocumentReference : DiscoveryReference {

        private string reference;

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.DiscoveryDocumentReference"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryDocumentReference() {
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.DiscoveryDocumentReference1"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DiscoveryDocumentReference(string href) {
            Ref = href;
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.Ref"]/*' />
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

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.DefaultFilename"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override string DefaultFilename {
            get {
                string filename = FilenameFromUrl(Url);
                return Path.ChangeExtension(filename, ".disco");        // [[....]] change default extension
            }
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.Document"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public DiscoveryDocument Document {
            get {
                if (ClientProtocol == null)
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingClientProtocol));
                object document = ClientProtocol.Documents[Url];
                if (document == null) {
                    Resolve();
                    document = ClientProtocol.Documents[Url];
                }
                DiscoveryDocument discoDocument = document as DiscoveryDocument;
                if (discoDocument == null) {
                    throw new InvalidOperationException(Res.GetString(Res.WebInvalidDocType,
                                                      typeof(DiscoveryDocument).FullName,
                                                      document == null ? string.Empty : document.GetType().FullName,
                                                      Url));
                }
                return discoDocument;
            }
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.WriteDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void WriteDocument(object document, Stream stream) {
            WebServicesSection.Current.DiscoveryDocumentSerializer.Serialize(new StreamWriter(stream, new UTF8Encoding(false)), document);
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.ReadDocument"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object ReadDocument(Stream stream) {
            return WebServicesSection.Current.DiscoveryDocumentSerializer.Deserialize(stream);
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.Url"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore]
        public override string Url {
            get { return Ref; }
            set { Ref = value; }
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.GetDocumentNoParse"]/*' />
        /// <devdoc>
        /// Retrieves a discovery document from Url, either out of the ClientProtocol
        /// or from a stream. Does not
        /// </devdoc>
        private static DiscoveryDocument GetDocumentNoParse(ref string url, DiscoveryClientProtocol client) {
            DiscoveryDocument d = (DiscoveryDocument) client.Documents[url];
            if (d != null) {
                return d;
            }

            string contentType = null;

            Stream stream = client.Download(ref url, ref contentType);
            try {
                XmlTextReader reader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType)));
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.XmlResolver = null;
                reader.DtdProcessing = DtdProcessing.Prohibit;
                if (!DiscoveryDocument.CanRead(reader)) {
                    // there is no discovery document at this location
                    ArgumentException exception = new ArgumentException(Res.GetString(Res.WebInvalidFormat));
                    throw new InvalidOperationException(Res.GetString(Res.WebMissingDocument, url), exception);
                }
                return DiscoveryDocument.Read(reader);
            }
            finally {
                stream.Close();
            }
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.Resolve"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected internal override void Resolve(string contentType, Stream stream) {
            DiscoveryDocument document = null;

            if (ContentType.IsHtml(contentType)) {
                string newRef = LinkGrep.SearchForLink(stream);
                if (newRef != null) {
                    string newUrl = UriToString(Url, newRef);
                    document = GetDocumentNoParse(ref newUrl, ClientProtocol);
                    Url = newUrl;
                }
                else
                    throw new InvalidContentTypeException(Res.GetString(Res.WebInvalidContentType, contentType), contentType);
            }

            if (document == null) { // probably xml...
                XmlTextReader reader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType)));
                reader.XmlResolver = null;
                reader.WhitespaceHandling = WhitespaceHandling.Significant;
                reader.DtdProcessing = DtdProcessing.Prohibit;
                if (DiscoveryDocument.CanRead(reader)) {
                    // it's a discovery document, so just read it.
                    document = DiscoveryDocument.Read(reader);
                }
                else {
                    // check out the processing instructions before the first tag.  if any of them
                    // match the form specified in the DISCO spec, save the href.
                    stream.Position = 0;
                    XmlTextReader newReader = new XmlTextReader(new StreamReader(stream, RequestResponseUtils.GetEncoding(contentType)));
                    newReader.XmlResolver = null;
                    newReader.DtdProcessing = DtdProcessing.Prohibit;
                    while (newReader.NodeType != XmlNodeType.Element) {
                        if (newReader.NodeType == XmlNodeType.ProcessingInstruction) {
                            // manually parse the PI contents since XmlTextReader won't automatically do it
                            StringBuilder sb = new StringBuilder("<pi ");
                            sb.Append(newReader.Value);
                            sb.Append("/>");
                            XmlTextReader piReader = new XmlTextReader(new StringReader(sb.ToString()));
                            piReader.XmlResolver = null;
                            piReader.DtdProcessing = DtdProcessing.Prohibit;
                            piReader.Read();
                            string type = piReader["type"];
                            string alternate = piReader["alternate"];
                            string href = piReader["href"];
                            if (type != null && ContentType.MatchesBase(type, ContentType.TextXml)
                                && alternate != null && string.Compare(alternate, "yes", StringComparison.OrdinalIgnoreCase) == 0
                                && href != null) {
                                // we got a PI with the right attributes

                                // there is a link to a discovery document. follow it after fully qualifying it.
                                string newUrl = UriToString(Url, href);
                                document = GetDocumentNoParse(ref newUrl, ClientProtocol);
                                Url = newUrl;
                                break;
                            }
                        }
                        newReader.Read();
                    }
                }
            }

            if (document == null) {
                // there is no discovery document at this location
                Exception exception;
                if (ContentType.IsXml(contentType)) {
                    exception = new ArgumentException(Res.GetString(Res.WebInvalidFormat));
                }
                else {
                    exception = new InvalidContentTypeException(Res.GetString(Res.WebInvalidContentType, contentType), contentType);
                }
                throw new InvalidOperationException(Res.GetString(Res.WebMissingDocument, Url), exception);
            }

            ClientProtocol.References[Url] = this;
            ClientProtocol.Documents[Url] = document;

            foreach (object o in document.References) {
                if (o is DiscoveryReference) {
                    DiscoveryReference r = (DiscoveryReference) o;
                    if (r.Url.Length == 0) {
                        throw new InvalidOperationException(Res.GetString(Res.WebEmptyRef, r.GetType().FullName, Url));
                    }
                    r.Url = UriToString(Url, r.Url);
                    //All inheritors of DiscoveryReference that got URIs relative
                    //to Ref property should adjust them like ContractReference does here.
                    ContractReference cr = r as ContractReference;
                    if ( (cr != null) && (cr.DocRef != null) ) {
                        cr.DocRef = UriToString(Url, cr.DocRef);
                    }
                    r.ClientProtocol = ClientProtocol;
                    ClientProtocol.References[r.Url] = r;
                }
                else
                    ClientProtocol.AdditionalInformation.Add(o);
            }

            return;
        }

        /// <include file='doc\DiscoveryDocumentReference.uex' path='docs/doc[@for="DiscoveryDocumentReference.ResolveAll"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void ResolveAll() {
            ResolveAll(true);
        }

        internal void ResolveAll(bool throwOnError) {
            try {
                Resolve();
            }
            catch (Exception e) {
                if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                    throw;
                }
                if (throwOnError)
                    throw;

                if (Tracing.On) Tracing.ExceptionCatch(TraceEventType.Warning, this, "ResolveAll", e);

                // can't continue, because we couldn't find a document.
                return;
            }

            foreach (object o in Document.References) {
                DiscoveryDocumentReference r = o as DiscoveryDocumentReference;
                if (r == null)
                    continue;
                if (ClientProtocol.Documents[r.Url] != null) {
                    continue;
                }
                r.ClientProtocol = ClientProtocol;
                r.ResolveAll(throwOnError);
            }
        }
    }
}
