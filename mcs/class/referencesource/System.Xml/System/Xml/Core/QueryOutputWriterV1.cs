//------------------------------------------------------------------------------
// <copyright file=QueryOutputWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace System.Xml {

    /// <summary>
    /// This writer wraps an XmlWriter that was not build using the XmlRawWriter architecture (such as XmlTextWriter or a custom XmlWriter) 
    /// for use in the XslCompilerTransform. Depending on the Xsl stylesheet output settings (which gets transfered to this writer via the 
    /// internal properties of XmlWriterSettings) this writer will inserts additional lexical information into the resulting Xml 1.0 document:
    /// 
    ///   1. CData sections
    ///   2. DocType declaration
    ///   3. Standalone attribute
    ///
    /// It also calls WriteStateDocument if standalone="yes" and/or a DocType declaration is written out in order to enforce document conformance
    /// checking.
    /// </summary>
    internal class QueryOutputWriterV1 : XmlWriter {
        private XmlWriter wrapped;
        private bool inCDataSection;
        private Dictionary<XmlQualifiedName, XmlQualifiedName> lookupCDataElems;
        private BitStack bitsCData;
        private XmlQualifiedName qnameCData;
        private bool outputDocType, inAttr;
        private string systemId, publicId;
        private XmlStandalone standalone;

        public QueryOutputWriterV1(XmlWriter writer, XmlWriterSettings settings) {
            this.wrapped = writer;

            this.systemId = settings.DocTypeSystem;
            this.publicId = settings.DocTypePublic;

            if (settings.OutputMethod == XmlOutputMethod.Xml) {
                bool documentConformance = false;

                // Xml output method shouldn't output doc-type-decl if system ID is not defined (even if public ID is)
                // Only check for well-formed document if output method is xml
                if (this.systemId != null) {
                    documentConformance = true;
                    this.outputDocType = true;
                }

                // Check for well-formed document if standalone="yes" in an auto-generated xml declaration
                if (settings.Standalone == XmlStandalone.Yes) {
                    documentConformance = true;
                    this.standalone = settings.Standalone;
                }

                if (documentConformance) {
                    if (settings.Standalone == XmlStandalone.Yes) {
                        this.wrapped.WriteStartDocument(true);
                    }
                    else {
                        this.wrapped.WriteStartDocument();
                    }
                }

                if (settings.CDataSectionElements != null && settings.CDataSectionElements.Count > 0) {
                    this.bitsCData = new BitStack();
                    this.lookupCDataElems = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
                    this.qnameCData = new XmlQualifiedName();

                    // Add each element name to the lookup table
                    foreach (XmlQualifiedName name in settings.CDataSectionElements) {
                        this.lookupCDataElems[name] = null;
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

        public override WriteState WriteState {
            get {
                return this.wrapped.WriteState;
            }
        }

        public override void WriteStartDocument() {
            this.wrapped.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone) {
            this.wrapped.WriteStartDocument(standalone);
        }

        public override void WriteEndDocument() {
            this.wrapped.WriteEndDocument();
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
        /// Output doc-type-decl on the first element, and determine whether this element is a
        /// CData section element.
        /// </summary>
        public override void WriteStartElement(string prefix, string localName, string ns) {
            EndCDataSection();

            // Output doc-type declaration immediately before first element is output
            if (this.outputDocType) {
                WriteState ws = this.wrapped.WriteState;
                if (ws == WriteState.Start || ws == WriteState.Prolog) {
                    this.wrapped.WriteDocType(
                            prefix.Length != 0 ? prefix + ":" + localName : localName,
                            this.publicId,
                            this.systemId,
                            null );
                }
                this.outputDocType = false;
            }

            this.wrapped.WriteStartElement(prefix, localName, ns);

            if (this.lookupCDataElems != null) {
                // Determine whether this element is a CData section element
                this.qnameCData.Init(localName, ns);
                this.bitsCData.PushBit(this.lookupCDataElems.ContainsKey(this.qnameCData));
            }
        }

        public override void WriteEndElement() {
            EndCDataSection();

            this.wrapped.WriteEndElement();

            if (this.lookupCDataElems != null)
                this.bitsCData.PopBit();
        }

        public override void WriteFullEndElement() {
            EndCDataSection();

            this.wrapped.WriteFullEndElement();

            if (this.lookupCDataElems != null)
                this.bitsCData.PopBit();
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            this.inAttr = true;
            this.wrapped.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            this.inAttr = false;
            this.wrapped.WriteEndAttribute();
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

        public override void WriteBase64(byte[] buffer, int index, int count) {
            if (!this.inAttr && (this.inCDataSection || StartCDataSection()))
                this.wrapped.WriteBase64(buffer, index, count);
            else
                this.wrapped.WriteBase64(buffer, index, count);
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
        }

        public override void Flush() {
            this.wrapped.Flush();
        }

        public override string LookupPrefix(string ns) {
            return this.wrapped.LookupPrefix(ns);
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

