//------------------------------------------------------------------------------
// <copyright file="QueryReaderSettings.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace System.Xml.Xsl {
    internal class QueryReaderSettings {
        private bool               validatingReader;
        private XmlReaderSettings  xmlReaderSettings;
        private XmlNameTable       xmlNameTable;
        private EntityHandling     entityHandling;
        private bool               namespaces;
        private bool               normalization;
        private bool               prohibitDtd;
        private WhitespaceHandling whitespaceHandling;
        private XmlResolver        xmlResolver;

        public QueryReaderSettings(XmlNameTable xmlNameTable) {
            Debug.Assert(xmlNameTable != null);
            xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.NameTable = xmlNameTable;
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Document;
            xmlReaderSettings.XmlResolver = null;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
            xmlReaderSettings.CloseInput = true;
        }

        public QueryReaderSettings(XmlReader reader) {
#pragma warning disable 618
            XmlValidatingReader valReader = reader as XmlValidatingReader;
#pragma warning restore 618
            if (valReader != null) {
                // Unwrap validation reader
                validatingReader = true;
                reader = valReader.Impl.Reader;
            }
            xmlReaderSettings = reader.Settings;
            if (xmlReaderSettings != null) {
                xmlReaderSettings = xmlReaderSettings.Clone();
                xmlReaderSettings.NameTable = reader.NameTable;
                xmlReaderSettings.CloseInput = true;
                xmlReaderSettings.LineNumberOffset = 0;
                xmlReaderSettings.LinePositionOffset = 0;
                XmlTextReaderImpl impl = reader as XmlTextReaderImpl;
                if (impl != null) {
                    xmlReaderSettings.XmlResolver = impl.GetResolver();
                }
            } else {
                xmlNameTable = reader.NameTable;
                XmlTextReader xmlTextReader = reader as XmlTextReader;
                if (xmlTextReader != null) {
                    XmlTextReaderImpl impl = xmlTextReader.Impl;
                    entityHandling     = impl.EntityHandling;
                    namespaces         = impl.Namespaces;
                    normalization      = impl.Normalization;
                    prohibitDtd        = ( impl.DtdProcessing == DtdProcessing.Prohibit );
                    whitespaceHandling = impl.WhitespaceHandling;
                    xmlResolver        = impl.GetResolver();
                } else {
                    entityHandling     = EntityHandling.ExpandEntities;
                    namespaces         = true;
                    normalization      = true;
                    prohibitDtd        = true;
                    whitespaceHandling = WhitespaceHandling.All;
                    xmlResolver        = null;
                }
            }
        }

        public XmlReader CreateReader(Stream stream, string baseUri) {
            XmlReader reader;
            if (xmlReaderSettings != null) {
                reader = XmlTextReader.Create(stream, xmlReaderSettings, baseUri);
            } else {
                XmlTextReaderImpl readerImpl = new XmlTextReaderImpl(baseUri, stream, xmlNameTable);
                readerImpl.EntityHandling = entityHandling;
                readerImpl.Namespaces = namespaces;
                readerImpl.Normalization = normalization;
                readerImpl.DtdProcessing = prohibitDtd ? DtdProcessing.Prohibit : DtdProcessing.Parse;
                readerImpl.WhitespaceHandling = whitespaceHandling;
                readerImpl.XmlResolver = xmlResolver;
                reader = readerImpl;
            }
            if (validatingReader) {
#pragma warning disable 618
                reader = new XmlValidatingReader(reader);
#pragma warning restore 618
            }
            return reader;
        }

        public XmlNameTable NameTable {
            get { return xmlReaderSettings != null ? xmlReaderSettings.NameTable : xmlNameTable; }
        }
    }
}
