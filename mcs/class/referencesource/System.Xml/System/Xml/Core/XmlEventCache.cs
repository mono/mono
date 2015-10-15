//------------------------------------------------------------------------------
// <copyright file="XmlEventCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Schema;
using System.Xml.Xsl.Runtime;

namespace System.Xml {

    /// <summary>
    /// Caches sequence of XmlEvents so that they can be replayed later.
    /// </summary>
    internal sealed class XmlEventCache : XmlRawWriter {
        private List<XmlEvent[]> pages;     // All event pages
        private XmlEvent[] pageCurr;        // Page that is currently being built
        private int pageSize;               // Number of events in pageCurr
        private bool hasRootNode;           // True if the cached document has a root node, false if it's a fragment
        private StringConcat singleText;    // If document consists of a single text node, cache it here rather than creating pages
        private string baseUri;             // Base Uri of document

        private enum XmlEventType {
            Unknown = 0,
            DocType,
            StartElem,
            StartAttr,
            EndAttr,
            CData,
            Comment,
            PI,
            Whitespace,
            String,
            Raw,
            EntRef,
            CharEnt,
            SurrCharEnt,
            Base64,
            BinHex,
            XmlDecl1,
            XmlDecl2,
            StartContent,
            EndElem,
            FullEndElem,
            Nmsp,
            EndBase64,
            Close,
            Flush,
            Dispose,
        }

    #if DEBUG
        private const int InitialPageSize = 4;
    #else
        private const int InitialPageSize = 32;
    #endif

        public XmlEventCache(string baseUri, bool hasRootNode) {
            this.baseUri = baseUri;
            this.hasRootNode = hasRootNode;
        }

        public void EndEvents() {
            if (this.singleText.Count == 0)
                AddEvent(XmlEventType.Unknown);
        }


        //-----------------------------------------------
        // XmlEventCache methods
        //-----------------------------------------------

        /// <summary>
        /// Return Base Uri of the document.
        /// </summary>
        public string BaseUri {
            get { return this.baseUri; }
        }

        /// <summary>
        /// Return true if the cached document has a root node, false if it's a fragment.
        /// </summary>
        public bool HasRootNode {
            get { return this.hasRootNode; }
        }

        /// <summary>
        /// Replay all cached events to an XmlWriter.
        /// </summary>
        public void EventsToWriter(XmlWriter writer) {
            XmlEvent[] page;
            int idxPage, idxEvent;
            byte[] bytes;
            char[] chars;
            XmlRawWriter rawWriter;

            // Special-case single text node at the top-level
            if (this.singleText.Count != 0) {
                writer.WriteString(this.singleText.GetResult());
                return;
            }

            rawWriter = writer as XmlRawWriter;

            // Loop over set of pages
            for (idxPage = 0; idxPage < this.pages.Count; idxPage++) {
                page = this.pages[idxPage];

                // Loop over events in each page
                for (idxEvent = 0; idxEvent < page.Length; idxEvent++) {
                    switch (page[idxEvent].EventType) {
                        case XmlEventType.Unknown:
                            // No more events
                            Debug.Assert(idxPage + 1 == this.pages.Count);
                            return;

                        case XmlEventType.DocType:
                            writer.WriteDocType(page[idxEvent].String1, page[idxEvent].String2, page[idxEvent].String3, (string) page[idxEvent].Object);
                            break;

                        case XmlEventType.StartElem:
                            writer.WriteStartElement(page[idxEvent].String1, page[idxEvent].String2, page[idxEvent].String3);
                            break;

                        case XmlEventType.StartAttr:
                            writer.WriteStartAttribute(page[idxEvent].String1, page[idxEvent].String2, page[idxEvent].String3);
                            break;

                        case XmlEventType.EndAttr:
                            writer.WriteEndAttribute();
                            break;

                        case XmlEventType.CData:
                            writer.WriteCData(page[idxEvent].String1);
                            break;

                        case XmlEventType.Comment:
                            writer.WriteComment(page[idxEvent].String1);
                            break;

                        case XmlEventType.PI:
                            writer.WriteProcessingInstruction(page[idxEvent].String1, page[idxEvent].String2);
                            break;

                        case XmlEventType.Whitespace:
                            writer.WriteWhitespace(page[idxEvent].String1);
                            break;

                        case XmlEventType.String:
                            writer.WriteString(page[idxEvent].String1);
                            break;

                        case XmlEventType.Raw:
                            writer.WriteRaw(page[idxEvent].String1);
                            break;

                        case XmlEventType.EntRef:
                            writer.WriteEntityRef(page[idxEvent].String1);
                            break;

                        case XmlEventType.CharEnt:
                            writer.WriteCharEntity((char) page[idxEvent].Object);
                            break;

                        case XmlEventType.SurrCharEnt:
                            chars = (char[]) page[idxEvent].Object;
                            writer.WriteSurrogateCharEntity(chars[0], chars[1]);
                            break;

                        case XmlEventType.Base64:
                            bytes = (byte[]) page[idxEvent].Object;
                            writer.WriteBase64(bytes, 0, bytes.Length);
                            break;

                        case XmlEventType.BinHex:
                            bytes = (byte[]) page[idxEvent].Object;
                            writer.WriteBinHex(bytes, 0, bytes.Length);
                            break;

                        case XmlEventType.XmlDecl1:
                            if (rawWriter != null)
                                rawWriter.WriteXmlDeclaration((XmlStandalone) page[idxEvent].Object);
                            break;

                        case XmlEventType.XmlDecl2:
                            if (rawWriter != null)
                                rawWriter.WriteXmlDeclaration(page[idxEvent].String1);
                            break;

                        case XmlEventType.StartContent:
                            if (rawWriter != null)
                                rawWriter.StartElementContent();
                            break;

                        case XmlEventType.EndElem:
                            if (rawWriter != null)
                                rawWriter.WriteEndElement(page[idxEvent].String1, page[idxEvent].String2, page[idxEvent].String3);
                            else
                                writer.WriteEndElement();
                            break;

                        case XmlEventType.FullEndElem:
                            if (rawWriter != null)
                                rawWriter.WriteFullEndElement(page[idxEvent].String1, page[idxEvent].String2, page[idxEvent].String3);
                            else
                                writer.WriteFullEndElement();
                            break;

                        case XmlEventType.Nmsp:
                            if (rawWriter != null)
                                rawWriter.WriteNamespaceDeclaration(page[idxEvent].String1, page[idxEvent].String2);
                            else
                                writer.WriteAttributeString("xmlns", page[idxEvent].String1, XmlReservedNs.NsXmlNs, page[idxEvent].String2);
                            break;

                        case XmlEventType.EndBase64:
                            if (rawWriter != null)
                                rawWriter.WriteEndBase64();
                            break;

                        case XmlEventType.Close:
                            writer.Close();
                            break;

                        case XmlEventType.Flush:
                            writer.Flush();
                            break;

                        case XmlEventType.Dispose:
                            ((IDisposable)writer).Dispose();
                            break;

                        default:
                            Debug.Assert(false, "Unknown event: " + page[idxEvent].EventType);
                            break;
                    }
                }
            }

            Debug.Assert(false, "Unknown event should be added to end of event sequence.");
        }

        /// <summary>
        /// Concatenate all element text and atomic value events and return the resulting string.
        /// </summary>
        public string EventsToString() {
            StringBuilder bldr;
            XmlEvent[] page;
            int idxPage, idxEvent;
            bool inAttr;

            // Special-case single text node at the top-level
            if (this.singleText.Count != 0)
                return this.singleText.GetResult();

            bldr = new StringBuilder();

            // Loop over set of pages
            inAttr = false;
            for (idxPage = 0; idxPage < this.pages.Count; idxPage++) {
                page = this.pages[idxPage];

                // Loop over events in each page
                for (idxEvent = 0; idxEvent < page.Length; idxEvent++) {
                    switch (page[idxEvent].EventType) {
                        case XmlEventType.Unknown:
                            // No more events
                            Debug.Assert(idxPage + 1 == this.pages.Count);
                            return bldr.ToString();

                        case XmlEventType.String:
                        case XmlEventType.Whitespace:
                        case XmlEventType.Raw:
                        case XmlEventType.CData:
                            // Append text
                            if (!inAttr)
                                bldr.Append(page[idxEvent].String1);
                            break;

                        case XmlEventType.StartAttr:
                            // Don't append text or atomic values if they appear within attributes
                            inAttr = true;
                            break;

                        case XmlEventType.EndAttr:
                            // No longer in an attribute
                            inAttr = false;
                            break;
                    }
                }
            }

            Debug.Assert(false, "Unknown event should be added to end of event sequence.");
            return string.Empty;
        }


        //-----------------------------------------------
        // XmlWriter interface
        //-----------------------------------------------

        public override XmlWriterSettings Settings {
            get { return null; }
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            AddEvent(XmlEventType.DocType, name, pubid, sysid, subset);
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            AddEvent(XmlEventType.StartElem, prefix, localName, ns);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            AddEvent(XmlEventType.StartAttr, prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            AddEvent(XmlEventType.EndAttr);
        }

        public override void WriteCData(string text) {
            AddEvent(XmlEventType.CData, text);
        }

        public override void WriteComment(string text) {
            AddEvent(XmlEventType.Comment, text);
        }

        public override void WriteProcessingInstruction(string name, string text) {
            AddEvent(XmlEventType.PI, name, text);
        }

        public override void WriteWhitespace(string ws) {
            AddEvent(XmlEventType.Whitespace, ws);
        }

        public override void WriteString(string text) {
            // Special-case single text node at the top level
            if (this.pages == null) {
                this.singleText.ConcatNoDelimiter(text);
            }
            else {
                AddEvent(XmlEventType.String, text);
            }
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            WriteString(new string(buffer, index, count));
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            WriteRaw(new string(buffer, index, count));
        }

        public override void WriteRaw(string data) {
            AddEvent(XmlEventType.Raw, data);
        }

        public override void WriteEntityRef(string name) {
            AddEvent(XmlEventType.EntRef, name);
        }

        public override void WriteCharEntity(char ch) {
            AddEvent(XmlEventType.CharEnt, (object) ch);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            // Save high and low characters
            char[] chars = {lowChar, highChar};
            AddEvent(XmlEventType.SurrCharEnt, (object) chars);
        }

        public override void WriteBase64(byte[] buffer, int index, int count) {
            AddEvent(XmlEventType.Base64, (object) ToBytes(buffer, index, count));
        }

        public override void WriteBinHex(byte[] buffer, int index, int count) {
            AddEvent(XmlEventType.BinHex, (object) ToBytes(buffer, index, count));
        }

        public override void Close() {
            AddEvent(XmlEventType.Close);
        }

        public override void Flush() {
            AddEvent(XmlEventType.Flush);
        }

        /// <summary>
        /// All other WriteValue methods are implemented by XmlWriter to delegate to WriteValue(object) or WriteValue(string), so
        /// only these two methods need to be implemented.
        /// </summary>
        public override void WriteValue(object value) {
            WriteString(XmlUntypedConverter.Untyped.ToString(value, this.resolver));
        }

        public override void WriteValue(string value) {
            WriteString(value);
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    AddEvent(XmlEventType.Dispose);
                }
            }
            finally {
                base.Dispose(disposing);
            }
        }


        //-----------------------------------------------
        // XmlRawWriter interface
        //-----------------------------------------------

        internal override void WriteXmlDeclaration(XmlStandalone standalone) {
            AddEvent(XmlEventType.XmlDecl1, (object) standalone);
        }

        internal override void WriteXmlDeclaration(string xmldecl) {
            AddEvent(XmlEventType.XmlDecl2, xmldecl);
        }

        internal override void StartElementContent() {
            AddEvent(XmlEventType.StartContent);
        }

        internal override void WriteEndElement(string prefix, string localName, string ns) {
            AddEvent(XmlEventType.EndElem, prefix, localName, ns);
        }

        internal override void WriteFullEndElement(string prefix, string localName, string ns) {
            AddEvent(XmlEventType.FullEndElem, prefix, localName, ns);
        }

        internal override void WriteNamespaceDeclaration(string prefix, string ns) {
            AddEvent(XmlEventType.Nmsp, prefix, ns);
        }

        internal override void WriteEndBase64() {
            AddEvent(XmlEventType.EndBase64);
        }


        //-----------------------------------------------
        // Helper methods
        //-----------------------------------------------

        private void AddEvent(XmlEventType eventType) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType);
        }

        private void AddEvent(XmlEventType eventType, string s1) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType, s1);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType, s1, s2);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2, string s3) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType, s1, s2, s3);
        }

        private void AddEvent(XmlEventType eventType, string s1, string s2, string s3, object o) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType, s1, s2, s3, o);
        }

        private void AddEvent(XmlEventType eventType, object o) {
            int idx = NewEvent();
            this.pageCurr[idx].InitEvent(eventType, o);
        }

        private int NewEvent() {
            if (this.pages == null) {
                this.pages = new List<XmlEvent[]>();
                this.pageCurr = new XmlEvent[InitialPageSize];
                this.pages.Add(this.pageCurr);

                if (this.singleText.Count != 0) {
                    // Review: There is no need to concatenate the strings here
                    this.pageCurr[0].InitEvent(XmlEventType.String, this.singleText.GetResult());
                    this.pageSize++;
                    this.singleText.Clear();
                }
            }
            else if (this.pageSize >= this.pageCurr.Length) {
                // Create new page
                this.pageCurr = new XmlEvent[this.pageSize * 2];
                this.pages.Add(this.pageCurr);
                this.pageSize = 0;
            }

            return this.pageSize++;
        }

        /// <summary>
        /// Create a standalone buffer that doesn't need an index or count passed along with it.
        /// </summary>
        private static byte[] ToBytes(byte[] buffer, int index, int count) {
            if (index != 0 || count != buffer.Length) {
                if (buffer.Length - index > count)
                    count = buffer.Length - index;

                byte[] bufferNew = new byte[count];
                Array.Copy(buffer, index, bufferNew, 0, count);

                return bufferNew;
            }

            return buffer;
        }


        /// <summary>
        /// Caches information for XML events like BeginElement, String, and EndAttribute so that they can be replayed later.
        /// </summary>
        private struct XmlEvent {
            private XmlEventType eventType;
            private string s1;
            private string s2;
            private string s3;
            private object o;

            public void InitEvent(XmlEventType eventType) {
                this.eventType = eventType;
            }

            public void InitEvent(XmlEventType eventType, string s1) {
                this.eventType = eventType;
                this.s1 = s1;
            }

            public void InitEvent(XmlEventType eventType, string s1, string s2) {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
            }

            public void InitEvent(XmlEventType eventType, string s1, string s2, string s3) {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
                this.s3 = s3;
            }

            public void InitEvent(XmlEventType eventType, string s1, string s2, string s3, object o) {
                this.eventType = eventType;
                this.s1 = s1;
                this.s2 = s2;
                this.s3 = s3;
                this.o = o;
            }

            public void InitEvent(XmlEventType eventType, object o) {
                this.eventType = eventType;
                this.o = o;
            }

            public XmlEventType EventType {
                get { return this.eventType; }
            }

            public string String1 {
                get { return this.s1; }
            }

            public string String2 {
                get { return this.s2; }
            }

            public string String3 {
                get { return this.s3; }
            }

            public object Object {
                get { return this.o; }
            }
        }
    }
}
