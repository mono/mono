//------------------------------------------------------------------------------
// <copyright file="XmlRawWriterWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// This internal class implements the XmlRawWriter interface by passing all calls to a wrapped XmlWriter implementation.
    /// </summary>
    sealed internal class XmlRawWriterWrapper : XmlRawWriter {
        private XmlWriter wrapped;

        public XmlRawWriterWrapper(XmlWriter writer) {
            this.wrapped = writer;
        }


        //-----------------------------------------------
        // XmlWriter interface
        //-----------------------------------------------

        public override XmlWriterSettings Settings { 
            get { return this.wrapped.Settings; }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            this.wrapped.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            this.wrapped.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            this.wrapped.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            this.wrapped.WriteEndAttribute();
        }

        public override void WriteCData(string text) {
            this.wrapped.WriteCData(text);
        }

        public override void WriteComment(string text) {
            this.wrapped.WriteComment(text);
        }

        public override void WriteProcessingInstruction(string name, string text) {
            this.wrapped.WriteProcessingInstruction(name, text);
        }

        public override void WriteWhitespace(string ws) {
            this.wrapped.WriteWhitespace(ws);
        }

        public override void WriteString(string text) {
            this.wrapped.WriteString(text);
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            this.wrapped.WriteChars(buffer, index, count);
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            this.wrapped.WriteRaw(buffer, index, count);
        }

        public override void WriteRaw(string data) {
            this.wrapped.WriteRaw(data);
        }

        public override void WriteEntityRef(string name) {
            this.wrapped.WriteEntityRef(name);
        }

        public override void WriteCharEntity(char ch) {
            this.wrapped.WriteCharEntity(ch);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            this.wrapped.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void Close() {
            this.wrapped.Close();
        }

        public override void Flush() {
            this.wrapped.Flush();
        }

        public override void WriteValue(object value) {
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(string value) {
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(bool value) {
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(DateTime value) {
            this.wrapped.WriteValue(value);
        }
        
        public override void WriteValue(float value) {
            this.wrapped.WriteValue(value);
        }
        
        public override void WriteValue(decimal value) {
            this.wrapped.WriteValue(value);
        }
        
        public override void WriteValue(double value) {
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(int value) {
            this.wrapped.WriteValue(value);
        }

        public override void WriteValue(long value) {
            this.wrapped.WriteValue(value);
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    ((IDisposable)this.wrapped).Dispose();
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }


        //-----------------------------------------------
        // XmlRawWriter interface
        //-----------------------------------------------

        /// <summary>
        /// No-op.
        /// </summary>
        internal override void WriteXmlDeclaration(XmlStandalone standalone) {
        }

        /// <summary>
        /// No-op.
        /// </summary>
        internal override void WriteXmlDeclaration(string xmldecl) {
        }

        /// <summary>
        /// No-op.
        /// </summary>
        internal override void StartElementContent() {
        }

        /// <summary>
        /// Forward to WriteEndElement().
        /// </summary>
        internal override void WriteEndElement(string prefix, string localName, string ns) {
            this.wrapped.WriteEndElement();
        }

        /// <summary>
        /// Forward to WriteFullEndElement().
        /// </summary>
        internal override void WriteFullEndElement(string prefix, string localName, string ns) {
            this.wrapped.WriteFullEndElement();
        }

        /// <summary>
        /// Forward to WriteAttribute();
        /// </summary>
        internal override void WriteNamespaceDeclaration(string prefix, string ns) {
            if (prefix.Length == 0)
                this.wrapped.WriteAttributeString(string.Empty, "xmlns", XmlReservedNs.NsXmlNs, ns);
            else
                this.wrapped.WriteAttributeString("xmlns", prefix, XmlReservedNs.NsXmlNs, ns);
        }
    }
}
