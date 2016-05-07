//------------------------------------------------------------------------------
// <copyright file=QueryOutputWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Xml {
    using System;
    using System.Globalization;
    using System.IO;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using System.Diagnostics;


    /// <summary>
    /// This writer wraps an XmlRawWriter and inserts additional lexical information into the resulting
    /// Xml 1.0 document:
    ///   1. CData sections
    ///   2. DocType declaration
    ///
    /// It also performs well-formed document checks if standalone="yes" and/or a doc-type-decl is output.
    /// </summary>
    internal class QueryOutputWriter : XmlRawWriter {
        private XmlRawWriter wrapped;
        private bool inCDataSection;
        private Dictionary<XmlQualifiedName, int> lookupCDataElems;
        private BitStack bitsCData;
        private XmlQualifiedName qnameCData;
        private bool outputDocType, checkWellFormedDoc, hasDocElem, inAttr;
        private string systemId, publicId;
        private int depth;

        public QueryOutputWriter(XmlRawWriter writer, XmlWriterSettings settings) {
            this.wrapped = writer;

            this.systemId = settings.DocTypeSystem;
            this.publicId = settings.DocTypePublic;

            if (settings.OutputMethod == XmlOutputMethod.Xml) {
                // Xml output method shouldn't output doc-type-decl if system ID is not defined (even if public ID is)
                // Only check for well-formed document if output method is xml
                if (this.systemId != null) {
                    this.outputDocType = true;
                    this.checkWellFormedDoc = true;
                }

                // Check for well-formed document if standalone="yes" in an auto-generated xml declaration
                if (settings.AutoXmlDeclaration && settings.Standalone == XmlStandalone.Yes)
                    this.checkWellFormedDoc = true;

                if (settings.CDataSectionElements.Count > 0) {
                    this.bitsCData = new BitStack();
                    this.lookupCDataElems = new Dictionary<XmlQualifiedName, int>();
                    this.qnameCData = new XmlQualifiedName();

                    // Add each element name to the lookup table
                    foreach (XmlQualifiedName name in settings.CDataSectionElements) {
                        this.lookupCDataElems[name] = 0;
                    }

                    this.bitsCData.PushBit(false);
                }
            }
            else if (settings.OutputMethod == XmlOutputMethod.Html) {
                // Html output method should output doc-type-decl if system ID or public ID is defined
                if (this.systemId != null || this.publicId != null)
                    this.outputDocType = true;
            }
        }


        //-----------------------------------------------
        // XmlWriter interface
        //-----------------------------------------------

        /// <summary>
        /// Get and set the namespace resolver that's used by this RawWriter to resolve prefixes.
        /// </summary>
        internal override IXmlNamespaceResolver NamespaceResolver  {
            get {
                return this.resolver;
            }
            set {
                this.resolver = value;
                this.wrapped.NamespaceResolver = value;
            }
        }

        /// <summary>
        /// Write the xml declaration.  This must be the first call.
        /// </summary>
        internal override void WriteXmlDeclaration(XmlStandalone standalone) {
            this.wrapped.WriteXmlDeclaration(standalone);
        }

        internal override void WriteXmlDeclaration(string xmldecl) {
            this.wrapped.WriteXmlDeclaration(xmldecl);
        }

        /// <summary>
        /// Return settings provided to factory.
        /// </summary>
        public override XmlWriterSettings Settings {
            get {
                XmlWriterSettings settings = this.wrapped.Settings;

                settings.ReadOnly = false;
                settings.DocTypeSystem = this.systemId;
                settings.DocTypePublic = this.publicId;
                settings.ReadOnly = true;

                return settings;
            }
        }

        /// <summary>
        /// Suppress this explicit call to WriteDocType if information was provided by XmlWriterSettings.
        /// </summary>
        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            if (this.publicId == null && this.systemId == null) {
                Debug.Assert(!this.outputDocType);
                this.wrapped.WriteDocType(name, pubid, sysid, subset);
            }
        }

        /// <summary>
        /// Check well-formedness, possibly output doc-type-decl, and determine whether this element is a
        /// CData section element.
        /// </summary>
        public override void WriteStartElement(string prefix, string localName, string ns) {
            EndCDataSection();

            if (this.checkWellFormedDoc) {
                // Don't allow multiple document elements
                if (this.depth == 0 && this.hasDocElem)
                    throw new XmlException(Res.Xml_NoMultipleRoots, string.Empty);

                this.depth++;
                this.hasDocElem = true;
            }

            // Output doc-type declaration immediately before first element is output
            if (this.outputDocType) {
                this.wrapped.WriteDocType(
                        prefix.Length != 0 ? prefix + ":" + localName : localName,
                        this.publicId,
                        this.systemId,
                        null);

                this.outputDocType = false;
            }

            this.wrapped.WriteStartElement(prefix, localName, ns);

            if (this.lookupCDataElems != null) {
                // Determine whether this element is a CData section element
                this.qnameCData.Init(localName, ns);
                this.bitsCData.PushBit(this.lookupCDataElems.ContainsKey(this.qnameCData));
            }
        }

        internal override void WriteEndElement(string prefix, string localName, string ns) {
            EndCDataSection();

            this.wrapped.WriteEndElement(prefix, localName, ns);

            if (this.checkWellFormedDoc)
                this.depth--;

            if (this.lookupCDataElems != null)
                this.bitsCData.PopBit();
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns) {
            EndCDataSection();

            this.wrapped.WriteFullEndElement(prefix, localName, ns);

            if (this.checkWellFormedDoc)
                this.depth--;

            if (this.lookupCDataElems != null)
                this.bitsCData.PopBit();
        }

        internal override void StartElementContent() {
            this.wrapped.StartElementContent();
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            this.inAttr = true;
            this.wrapped.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            this.inAttr = false;
            this.wrapped.WriteEndAttribute();
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns) {
            this.wrapped.WriteNamespaceDeclaration(prefix, ns);
        }

        internal override bool SupportsNamespaceDeclarationInChunks {
            get {
                return this.wrapped.SupportsNamespaceDeclarationInChunks;
            }
        }

        internal override void WriteStartNamespaceDeclaration(string prefix) {
            this.wrapped.WriteStartNamespaceDeclaration(prefix);
        }

        internal override void WriteEndNamespaceDeclaration() {
            this.wrapped.WriteEndNamespaceDeclaration();
        }

        public override void WriteCData(string text) {
            this.wrapped.WriteCData(text);
        }

        public override void WriteComment(string text) {
            EndCDataSection();
            this.wrapped.WriteComment(text);
        }

        public override void WriteProcessingInstruction(string name, string text) {
            EndCDataSection();
            this.wrapped.WriteProcessingInstruction(name, text);
        }

        public override void WriteWhitespace(string ws) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteCData(ws);
            else
                this.wrapped.WriteWhitespace(ws);
        }

        public override void WriteString(string text) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteCData(text);
            else
                this.wrapped.WriteString(text);
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteCData(new string(buffer, index, count));
            else
                this.wrapped.WriteChars(buffer, index, count);
        }

        public override void WriteEntityRef(string name) {
            EndCDataSection();
            this.wrapped.WriteEntityRef(name);
        }

        public override void WriteCharEntity(char ch) {
            EndCDataSection();
            this.wrapped.WriteCharEntity(ch);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            EndCDataSection();
            this.wrapped.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteCData(new string(buffer, index, count));
            else
                this.wrapped.WriteRaw(buffer, index, count);
        }

        public override void WriteRaw(string data) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteCData(data);
            else
                this.wrapped.WriteRaw(data);
        }

        public override void Close() {
            this.wrapped.Close();

            if (this.checkWellFormedDoc && !this.hasDocElem) {
                // Need at least one document element
                throw new XmlException(Res.Xml_NoRoot, string.Empty);
            }
        }

        public override void Flush() {
            this.wrapped.Flush();
        }


        //-----------------------------------------------
        // Helper methods
        //-----------------------------------------------

        /// <summary>
        /// Write CData text if element is a CData element.  Return true if text should be written
        /// within a CData section.
        /// </summary>
        private bool StartCDataSection() {
            Debug.Assert(!this.inCDataSection);
            if (this.lookupCDataElems != null && this.bitsCData.PeekBit()) {
                this.inCDataSection = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// No longer write CData text.
        /// </summary>
        private void EndCDataSection() {
            this.inCDataSection = false;
        }
    }
}

