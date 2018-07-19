//------------------------------------------------------------------------------
// <copyright file="XmlAutoDetectWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------


namespace System.Xml {
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;


    /// <summary>
    /// This writer implements XmlOutputMethod.AutoDetect.  If the first element is "html", then output will be
    /// directed to an Html writer.  Otherwise, output will be directed to an Xml writer.
    /// </summary>
    internal class XmlAutoDetectWriter : XmlRawWriter, IRemovableWriter {
        private XmlRawWriter wrapped;
        private OnRemoveWriter onRemove;
        private XmlWriterSettings writerSettings;
        private XmlEventCache eventCache;           // Cache up events until first StartElement is encountered
        private TextWriter textWriter;
        private Stream strm;

        //-----------------------------------------------
        // Constructors
        //-----------------------------------------------

        private XmlAutoDetectWriter(XmlWriterSettings writerSettings) {
            Debug.Assert(writerSettings.OutputMethod == XmlOutputMethod.AutoDetect);

            this.writerSettings = (XmlWriterSettings)writerSettings.Clone();
            this.writerSettings.ReadOnly = true;

            // Start caching all events
            this.eventCache = new XmlEventCache(string.Empty, true);
        }

        public XmlAutoDetectWriter(TextWriter textWriter, XmlWriterSettings writerSettings)
            : this(writerSettings) {
            this.textWriter = textWriter;
        }

        public XmlAutoDetectWriter(Stream strm, XmlWriterSettings writerSettings)
            : this(writerSettings) {
            this.strm = strm;
        }


        //-----------------------------------------------
        // IRemovableWriter interface
        //-----------------------------------------------

        /// <summary>
        /// This writer will raise this event once it has determined whether to replace itself with the Html or Xml writer.
        /// </summary>
        public OnRemoveWriter OnRemoveWriterEvent {
            get { return this.onRemove; }
            set { this.onRemove = value; }
        }


        //-----------------------------------------------
        // XmlWriter interface
        //-----------------------------------------------

        public override XmlWriterSettings Settings {
            get { return this.writerSettings; }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            if (this.wrapped == null) {
                // This is the first time WriteStartElement has been called, so create the Xml or Html writer
                if (ns.Length == 0 && IsHtmlTag(localName))
                    CreateWrappedWriter(XmlOutputMethod.Html);
                else
                    CreateWrappedWriter(XmlOutputMethod.Xml);
            }
            this.wrapped.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            Debug.Assert(this.wrapped != null);
            this.wrapped.WriteEndAttribute();
        }

        public override void WriteCData(string text) {
            if (TextBlockCreatesWriter(text))
                this.wrapped.WriteCData(text);
            else
                this.eventCache.WriteCData(text);
        }

        public override void WriteComment(string text) {
            if (this.wrapped == null)
                this.eventCache.WriteComment(text);
            else
                this.wrapped.WriteComment(text);
        }

        public override void WriteProcessingInstruction(string name, string text) {
            if (this.wrapped == null)
                this.eventCache.WriteProcessingInstruction(name, text);
            else
                this.wrapped.WriteProcessingInstruction(name, text);
        }

        public override void WriteWhitespace(string ws) {
            if (this.wrapped == null)
                this.eventCache.WriteWhitespace(ws);
            else
                this.wrapped.WriteWhitespace(ws);
        }

        public override void WriteString(string text) {
            if (TextBlockCreatesWriter(text))
                this.wrapped.WriteString(text);
            else
                this.eventCache.WriteString(text);
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            WriteString(new string(buffer, index, count));
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            WriteRaw(new string(buffer, index, count));
        }

        public override void WriteRaw(string data) {
            if (TextBlockCreatesWriter(data))
                this.wrapped.WriteRaw(data);
            else
                this.eventCache.WriteRaw(data);
        }

        public override void WriteEntityRef(string name) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteEntityRef(name);
        }

        public override void WriteCharEntity(char ch) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteCharEntity(ch);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteBase64(byte[] buffer, int index, int count) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteBinHex(buffer, index, count);
        }

        public override void Close() {
            // Flush any cached events to an Xml writer
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.Close();
        }

        public override void Flush() {
            // Flush any cached events to an Xml writer
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.Flush();
        }

        public override void WriteValue(object value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(string value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(bool value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(DateTime value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(DateTimeOffset value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(double value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(float value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(decimal value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(int value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(long value) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteValue(value);
        }

        //-----------------------------------------------
        // XmlRawWriter interface
        //-----------------------------------------------

        internal override IXmlNamespaceResolver NamespaceResolver {
            get {
                return this.resolver;
            }
            set {
                this.resolver = value;

                if (this.wrapped == null)
                    this.eventCache.NamespaceResolver = value;
                else
                    this.wrapped.NamespaceResolver = value;
            }
        }

        internal override void WriteXmlDeclaration(XmlStandalone standalone) {
            // Forces xml writer to be created
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteXmlDeclaration(standalone);
        }

        internal override void WriteXmlDeclaration(string xmldecl) {
            // Forces xml writer to be created
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteXmlDeclaration(xmldecl);
        }

        internal override void StartElementContent() {
            Debug.Assert(this.wrapped != null);
            this.wrapped.StartElementContent();
        }

        internal override void WriteEndElement(string prefix, string localName, string ns) {
            Debug.Assert(this.wrapped != null);
            this.wrapped.WriteEndElement(prefix, localName, ns);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns) {
            Debug.Assert(this.wrapped != null);
            this.wrapped.WriteFullEndElement(prefix, localName, ns);
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteNamespaceDeclaration(prefix, ns);
        }

        internal override bool SupportsNamespaceDeclarationInChunks {
            get {
                return this.wrapped.SupportsNamespaceDeclarationInChunks;
            }
        }

        internal override void WriteStartNamespaceDeclaration( string prefix ) {
            EnsureWrappedWriter(XmlOutputMethod.Xml);
            this.wrapped.WriteStartNamespaceDeclaration(prefix);
        }

        internal override void WriteEndNamespaceDeclaration() {
            this.wrapped.WriteEndNamespaceDeclaration();
        }

        //-----------------------------------------------
        // Helper methods
        //-----------------------------------------------

        /// <summary>
        /// Return true if "tagName" == "html" (case-insensitive).
        /// </summary>
        private static bool IsHtmlTag(string tagName) {
            if (tagName.Length != 4)
                return false;

            if (tagName[0] != 'H' && tagName[0] != 'h')
                return false;

            if (tagName[1] != 'T' && tagName[1] != 't')
                return false;

            if (tagName[2] != 'M' && tagName[2] != 'm')
                return false;

            if (tagName[3] != 'L' && tagName[3] != 'l')
                return false;

            return true;
        }

        /// <summary>
        /// If a wrapped writer has not yet been created, create one.
        /// </summary>
        private void EnsureWrappedWriter(XmlOutputMethod outMethod) {
            if (this.wrapped == null)
                CreateWrappedWriter(outMethod);
        }

        /// <summary>
        /// If the specified text consist only of whitespace, then cache the whitespace, as it is not enough to
        /// force the creation of a wrapped writer.  Otherwise, create a wrapped writer if one has not yet been
        /// created and return true.
        /// </summary>
        private bool TextBlockCreatesWriter(string textBlock) {
            if (this.wrapped == null) {
                // Whitespace-only text blocks aren't enough to determine Xml vs. Html
                if (XmlCharType.Instance.IsOnlyWhitespace(textBlock)) {
                    return false;
                }

                // Non-whitespace text block selects Xml method
                CreateWrappedWriter(XmlOutputMethod.Xml);
            }

            return true;
        }

        /// <summary>
        /// Create either the Html or Xml writer and send any cached events to it.
        /// </summary>
        private void CreateWrappedWriter(XmlOutputMethod outMethod) {
            Debug.Assert(this.wrapped == null);

            // Create either the Xml or Html writer
            this.writerSettings.ReadOnly = false;
            this.writerSettings.OutputMethod = outMethod;

            // If Indent was not set by the user, then default to True for Html
            if (outMethod == XmlOutputMethod.Html && this.writerSettings.IndentInternal == TriState.Unknown)
                this.writerSettings.Indent = true;

            this.writerSettings.ReadOnly = true;

            if (textWriter != null)
                this.wrapped = ((XmlWellFormedWriter)XmlWriter.Create(this.textWriter, this.writerSettings)).RawWriter;
            else
                this.wrapped = ((XmlWellFormedWriter)XmlWriter.Create(this.strm, this.writerSettings)).RawWriter;

            // Send cached events to the new writer
            this.eventCache.EndEvents();
            this.eventCache.EventsToWriter(this.wrapped);

            // Send OnRemoveWriter event
            if (this.onRemove != null)
                (this.onRemove)(this.wrapped);
        }
    }
}
