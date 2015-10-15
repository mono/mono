//------------------------------------------------------------------------------
// <copyright file="XmlSequenceWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Schema;

namespace System.Xml.Xsl.Runtime {
    using Res           = System.Xml.Utils.Res;

    /// <summary>
    ///                         External XmlWriter      Cached Sequence
    /// ===================================================================================================
    /// Multiple Trees          Merged into Entity      Multiple Trees
    ///
    /// Attributes              Error                   Floating
    /// at top-level                                    Attribute
    ///
    /// Namespace               Error                   Floating
    /// at top-level                                    Namespace
    ///
    /// Elements, Text, PI      Implicit Root           Floating            
    /// Comments at top-level                           Nodes
    ///
    /// Root at top-level       Ignored                 Root
    ///
    /// Atomic Values           Whitespace-Separated    Atomic Values
    /// at top-level            Text Node
    ///
    /// Nodes By Reference      Copied                  Preserve Identity
    /// </summary>
    internal abstract class XmlSequenceWriter {
        /// <summary>
        /// Start construction of a new Xml tree (document or fragment).
        /// </summary>
        public abstract XmlRawWriter StartTree(XPathNodeType rootType, IXmlNamespaceResolver nsResolver, XmlNameTable nameTable);

        /// <summary>
        /// End construction of a new Xml tree (document or fragment).
        /// </summary>
        public abstract void EndTree();

        /// <summary>
        /// Write a top-level item by reference.
        /// </summary>
        public abstract void WriteItem(XPathItem item);
    }


    /// <summary>
    /// An implementation of XmlSequenceWriter that builds a cached XPath/XQuery sequence.
    /// </summary>
    internal class XmlCachedSequenceWriter : XmlSequenceWriter {
        private XmlQueryItemSequence seqTyped;
        private XPathDocument doc;
        private XmlRawWriter writer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlCachedSequenceWriter() {
            this.seqTyped = new XmlQueryItemSequence();
        }

        /// <summary>
        /// Return the sequence after it has been fully constructed.
        /// </summary>
        public XmlQueryItemSequence ResultSequence {
            get { return this.seqTyped; }
        }

        /// <summary>
        /// Start construction of a new Xml tree (document or fragment).
        /// </summary>
        public override XmlRawWriter StartTree(XPathNodeType rootType, IXmlNamespaceResolver nsResolver, XmlNameTable nameTable) {
            // Build XPathDocument
            // If rootType != XPathNodeType.Root, then build an XQuery fragment
            this.doc = new XPathDocument(nameTable);
            this.writer = doc.LoadFromWriter(XPathDocument.LoadFlags.AtomizeNames | (rootType == XPathNodeType.Root ? XPathDocument.LoadFlags.None : XPathDocument.LoadFlags.Fragment), string.Empty);
            this.writer.NamespaceResolver = nsResolver;
            return this.writer;
        }

        /// <summary>
        /// End construction of a new Xml tree (document or fragment).
        /// </summary>
        public override void EndTree() {
            // Add newly constructed document to sequence
            this.writer.Close();
            this.seqTyped.Add(this.doc.CreateNavigator());
        }

        /// <summary>
        /// Write a top-level item by reference.
        /// </summary>
        public override void WriteItem(XPathItem item) {
            // Preserve identity
            this.seqTyped.AddClone(item);
        }
    }


    /// <summary>
    /// An implementation of XmlSequenceWriter that converts an instance of the XQuery data model into a series
    /// of calls to XmlRawWriter.  The algorithm to do this is designed to be compatible with the rules in the
    /// "XSLT 2.0 and XQuery 1.0 Serialization" spec.  Here are the rules we use:
    ///   1. An exception is thrown if the top-level sequence contains attribute or namespace nodes
    ///   2. Each atomic value in the top-level sequence is converted to text, and XmlWriter.WriteString is called
    ///   3. A call to XmlRawWriter.WriteWhitespace(" ") is made between adjacent atomic values at the top-level
    ///   4. All items in the top-level sequence are merged together into a single result document.
    /// </summary>
    internal class XmlMergeSequenceWriter : XmlSequenceWriter {
        private XmlRawWriter xwrt;
        private bool lastItemWasAtomic;

        /// <summary>
        /// Constructor.
        /// </summary>
        public XmlMergeSequenceWriter(XmlRawWriter xwrt) {
            this.xwrt = xwrt;
            this.lastItemWasAtomic = false;
        }

        /// <summary>
        /// Start construction of a new Xml tree (document or fragment).
        /// </summary>
        public override XmlRawWriter StartTree(XPathNodeType rootType, IXmlNamespaceResolver nsResolver, XmlNameTable nameTable) {
            if (rootType == XPathNodeType.Attribute || rootType == XPathNodeType.Namespace)
                throw new XslTransformException(Res.XmlIl_TopLevelAttrNmsp, string.Empty);

            // Provide a namespace resolver to the writer
            this.xwrt.NamespaceResolver = nsResolver;

            return this.xwrt;
        }

        /// <summary>
        /// End construction of a new Xml tree (document or fragment).
        /// </summary>
        public override void EndTree() {
            this.lastItemWasAtomic = false;
        }

        /// <summary>
        /// Write a top-level item by reference.
        /// </summary>
        public override void WriteItem(XPathItem item) {
            if (item.IsNode) {
                XPathNavigator nav = item as XPathNavigator;

                if (nav.NodeType == XPathNodeType.Attribute || nav.NodeType == XPathNodeType.Namespace)
                    throw new XslTransformException(Res.XmlIl_TopLevelAttrNmsp, string.Empty);

                // Copy navigator to raw writer
                CopyNode(nav);
                this.lastItemWasAtomic = false;
            }
            else {
                WriteString(item.Value);
            }
        }

        /// <summary>
        /// Write the string value of a top-level atomic value.
        /// </summary>
        private void WriteString(string value) {
            if (this.lastItemWasAtomic) {
                // Insert space character between adjacent atomic values
                this.xwrt.WriteWhitespace(" ");
            }
            else {
                this.lastItemWasAtomic = true;
            }
            this.xwrt.WriteString(value);
        }

        /// <summary>
        /// Copy the navigator subtree to the raw writer.
        /// </summary>
        private void CopyNode(XPathNavigator nav) {
            XPathNodeType nodeType;
            int iLevel = 0;

            while (true) {
                if (CopyShallowNode(nav)) {
                    nodeType = nav.NodeType;
                    if (nodeType == XPathNodeType.Element) {
                        // Copy attributes
                        if (nav.MoveToFirstAttribute()) {
                            do {
                                CopyShallowNode(nav);
                            }
                            while (nav.MoveToNextAttribute());
                            nav.MoveToParent();
                        }

                        // Copy namespaces in document order (navigator returns them in reverse document order)
                        XPathNamespaceScope nsScope = (iLevel == 0) ? XPathNamespaceScope.ExcludeXml : XPathNamespaceScope.Local;
                        if (nav.MoveToFirstNamespace(nsScope)) {
                            CopyNamespaces(nav, nsScope);
                            nav.MoveToParent();
                        }

                        this.xwrt.StartElementContent();
                    }

                    // If children exist, move down to next level
                    if (nav.MoveToFirstChild()) {
                        iLevel++;
                        continue;
                    }
                    else {
                        // EndElement
                        if (nav.NodeType == XPathNodeType.Element)
                            this.xwrt.WriteEndElement(nav.Prefix, nav.LocalName, nav.NamespaceURI);
                    }
                }

                // No children
                while (true) {
                    if (iLevel == 0) {
                        // The entire subtree has been copied
                        return;
                    }

                    if (nav.MoveToNext()) {
                        // Found a sibling, so break to outer loop
                        break;
                    }

                    // No siblings, so move up to previous level
                    iLevel--;
                    nav.MoveToParent();

                    // EndElement
                    if (nav.NodeType == XPathNodeType.Element)
                        this.xwrt.WriteFullEndElement(nav.Prefix, nav.LocalName, nav.NamespaceURI);
                }
            }
        }

        /// <summary>
        /// Begin shallow copy of the specified node to the writer.  Returns true if the node might have content.
        /// </summary>
        private bool CopyShallowNode(XPathNavigator nav) {
            bool mayHaveChildren = false;

            switch (nav.NodeType) {
                case XPathNodeType.Element:
                    this.xwrt.WriteStartElement(nav.Prefix, nav.LocalName, nav.NamespaceURI);
                    mayHaveChildren = true;
                    break;

                case XPathNodeType.Attribute:
                    this.xwrt.WriteStartAttribute(nav.Prefix, nav.LocalName, nav.NamespaceURI);
                    this.xwrt.WriteString(nav.Value);
                    this.xwrt.WriteEndAttribute();
                    break;

                case XPathNodeType.Text:
                    this.xwrt.WriteString(nav.Value);
                    break;

                case XPathNodeType.SignificantWhitespace:
                case XPathNodeType.Whitespace:
                    this.xwrt.WriteWhitespace(nav.Value);
                    break;

                case XPathNodeType.Root:
                    mayHaveChildren = true;
                    break;

                case XPathNodeType.Comment:
                    this.xwrt.WriteComment(nav.Value);
                    break;

                case XPathNodeType.ProcessingInstruction:
                    this.xwrt.WriteProcessingInstruction(nav.LocalName, nav.Value);
                    break;

                case XPathNodeType.Namespace:
                    this.xwrt.WriteNamespaceDeclaration(nav.LocalName, nav.Value);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            return mayHaveChildren;
        }

        /// <summary>
        /// Copy all or some (which depends on nsScope) of the namespaces on the navigator's current node to the
        /// raw writer.
        /// </summary>
        private void CopyNamespaces(XPathNavigator nav, XPathNamespaceScope nsScope) {
            string prefix = nav.LocalName;
            string ns = nav.Value;

            if (nav.MoveToNextNamespace(nsScope)) {
                CopyNamespaces(nav, nsScope);
            }

            this.xwrt.WriteNamespaceDeclaration(prefix, ns);
        }
    }
}
