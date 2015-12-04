//------------------------------------------------------------------------------
// <copyright file="WhitespaceRuleReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using MS.Internal.Xml;

namespace System.Xml.Xsl.Runtime {

    /// <summary>
    /// </summary>
    internal class WhitespaceRuleReader : XmlWrappingReader {
        private WhitespaceRuleLookup wsRules;
        private BitStack stkStrip;
        private bool shouldStrip, preserveAdjacent;
        private string val;
        private XmlCharType xmlCharType = XmlCharType.Instance;

        static public XmlReader CreateReader(XmlReader baseReader, WhitespaceRuleLookup wsRules) {
            if (wsRules == null) {
                return baseReader;    // There is no rules to process
            }
            XmlReaderSettings readerSettings = baseReader.Settings;
            if (readerSettings != null) {
                if (readerSettings.IgnoreWhitespace) {
                    return baseReader;        // V2 XmlReader that strips all WS
                }
            } else {
                XmlTextReader txtReader = baseReader as XmlTextReader;
                if (txtReader != null && txtReader.WhitespaceHandling == WhitespaceHandling.None) {
                    return baseReader;        // V1 XmlTextReader that strips all WS
                }
                XmlTextReaderImpl txtReaderImpl = baseReader as XmlTextReaderImpl;
                if (txtReaderImpl != null && txtReaderImpl.WhitespaceHandling == WhitespaceHandling.None) {
                    return baseReader;        // XmlTextReaderImpl that strips all WS
                }
            }
            return new WhitespaceRuleReader(baseReader, wsRules);
        }

        private WhitespaceRuleReader(XmlReader baseReader, WhitespaceRuleLookup wsRules) : base(baseReader) {
            Debug.Assert(wsRules != null);

            this.val = null;
            this.stkStrip = new BitStack();
            this.shouldStrip = false;
            this.preserveAdjacent = false;

            this.wsRules = wsRules;
            this.wsRules.Atomize(baseReader.NameTable);
        }

        /// <summary>
        /// Override Value in order to possibly prepend extra whitespace.
        /// </summary>
        public override string Value {
            get { return (this.val == null) ? base.Value : this.val; }
        }

        /// <summary>
        /// Override Read in order to search for strippable whitespace, to concatenate adjacent text nodes, and to
        /// resolve entities.
        /// </summary>
        public override bool Read() {
            XmlCharType xmlCharType = XmlCharType.Instance;
            string ws = null;

            // Clear text value
            this.val = null;

            while (base.Read()) {
                switch (base.NodeType) {
                case XmlNodeType.Element:
                    // Push boolean indicating whether whitespace children of this element should be stripped
                    if (!base.IsEmptyElement) {
                        this.stkStrip.PushBit(this.shouldStrip);

                        // Strip if rules say we should and we're not within the scope of xml:space="preserve"
                        this.shouldStrip = wsRules.ShouldStripSpace(base.LocalName, base.NamespaceURI) && (base.XmlSpace != XmlSpace.Preserve);
                    }
                    break;

                case XmlNodeType.EndElement:
                    // Restore parent shouldStrip setting
                    this.shouldStrip = this.stkStrip.PopBit();
                    break;

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    // If preserving adjacent text, don't perform any further checks
                    if (this.preserveAdjacent)
                        return true;

                    if (this.shouldStrip) {
                        // Reader may report whitespace as Text or CDATA
                        if (xmlCharType.IsOnlyWhitespace(base.Value))
                            goto case XmlNodeType.Whitespace;

                        // If whitespace was cached, then prepend it to text or CDATA value
                        if (ws != null)
                            this.val = string.Concat(ws, base.Value);

                        // Preserve adjacent whitespace
                        this.preserveAdjacent = true;
                        return true;
                    }
                    break;

                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    // If preserving adjacent text, don't perform any further checks
                    if (this.preserveAdjacent)
                        return true;

                    if (this.shouldStrip) {
                        // Save whitespace until it can be determined whether it will be stripped
                        if (ws == null)
                            ws = base.Value;
                        else
                            ws = string.Concat(ws, base.Value);

                        // Read next event
                        continue;
                    }
                    break;
                case XmlNodeType.EndEntity:
                    // Read next event
                    continue;
                }

            // No longer preserve adjacent space
                this.preserveAdjacent = false;
                return true;
            }

            return false;
        }
    }
}
