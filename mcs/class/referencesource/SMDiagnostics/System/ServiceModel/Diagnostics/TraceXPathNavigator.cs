//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.XPath;
    using System.Globalization;
    using System.Text;
    using System.IO;
    using System.Runtime;

    // We have to put something here so that when this item appears in the
    // debugger, ToString() isn't called. Calling ToString() can cause bad behavior.
    [DebuggerDisplay("")]
    class TraceXPathNavigator : XPathNavigator
    {
        const int UnlimitedSize = -1;
        ElementNode root = null;
        TraceNode current = null;
        bool closed = false;
        XPathNodeType state = XPathNodeType.Element;
        int maxSize;
        long currentSize;

        public TraceXPathNavigator(int maxSize)
        {
            this.maxSize = maxSize;
            this.currentSize = 0;
        }

        interface IMeasurable
        {
            int Size { get; }
        }

        class TraceNode
        {
            protected TraceNode(XPathNodeType nodeType, ElementNode parent)
            {
                this.nodeType = nodeType;
                this.parent = parent;
            }

            internal XPathNodeType NodeType
            {
                get { return this.nodeType; }
            }

            XPathNodeType nodeType;
            internal ElementNode parent;
        }

        class CommentNode : TraceNode, IMeasurable
        {
            internal CommentNode(string text, ElementNode parent)
                : base(XPathNodeType.Comment, parent)
            {
                this.nodeValue = text;
            }

            internal string nodeValue;

            public int Size
            {
                get
                {
                    return this.nodeValue.Length + 8; // <!--XXX-->
                }
            }
        }

        class ElementNode : TraceNode, IMeasurable
        {
            int attributeIndex = 0;
            int elementIndex = 0;

            internal string name;
            internal string prefix;
            internal string xmlns;
            internal List<TraceNode> childNodes = new List<TraceNode>();
            internal List<AttributeNode> attributes = new List<AttributeNode>();
            internal TextNode text;
            internal bool movedToText = false;

            internal ElementNode(string name, string prefix, ElementNode parent, string xmlns)
                : base(XPathNodeType.Element, parent)
            {
                this.name = name;
                this.prefix = prefix;
                this.xmlns = xmlns;
            }

            internal void Add(TraceNode node)
            {
                this.childNodes.Add(node);
            }

            //This method returns all subnodes with the given path of local names. Namespaces are ignored. 
            //For all path elements but the last one, the first match is taken. For the last path element, all matches are returned.
            internal IEnumerable<ElementNode> FindSubnodes(string[] headersPath)
            {
#pragma warning disable 618
                Fx.Assert(null != headersPath, "Headers path should not be null");
                Fx.Assert(headersPath.Length > 0, "There should be more than one item in the headersPath array.");
#pragma warning restore 618

                if (null == headersPath)
                {
                    throw new ArgumentNullException("headersPath");
                }
                ElementNode node = this;
                if (String.CompareOrdinal(node.name, headersPath[0]) != 0)
                {
                    node = null;
                }
                int i = 0;
                while (null != node && ++i < headersPath.Length)
                {
#pragma warning disable 618
                    Fx.Assert(null != headersPath[i], "None of the elements in headersPath should be null.");
#pragma warning restore 618
                    ElementNode subNode = null;
                    if (null != node.childNodes)
                    {
                        foreach (TraceNode child in node.childNodes)
                        {
                            if (child.NodeType == XPathNodeType.Element)
                            {
                                ElementNode childNode = child as ElementNode;
                                if (null != childNode && 0 == String.CompareOrdinal(childNode.name, headersPath[i]))
                                {
                                    if (headersPath.Length == i + 1)
                                    {
                                        yield return childNode;
                                    }
                                    else
                                    {
                                        subNode = childNode;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    node = subNode;
                }
            }

            internal TraceNode MoveToNext()
            {
                TraceNode retval = null;
                if ((this.elementIndex + 1) < this.childNodes.Count)
                {
                    ++this.elementIndex;
                    retval = this.childNodes[this.elementIndex];
                }
                return retval;
            }

            internal bool MoveToFirstAttribute()
            {
                this.attributeIndex = 0;
                return null != this.attributes && this.attributes.Count > 0;
            }

            internal bool MoveToNextAttribute()
            {
                bool retval = false;
                if ((this.attributeIndex + 1) < this.attributes.Count)
                {
                    ++this.attributeIndex;
                    retval = true;
                }
                return retval;
            }

            internal void Reset()
            {
                this.attributeIndex = 0;
                this.elementIndex = 0;
                this.movedToText = false;
                if (null != this.childNodes)
                {
                    foreach (TraceNode node in this.childNodes)
                    {
                        if (node.NodeType == XPathNodeType.Element)
                        {
                            ElementNode child = node as ElementNode;
                            if (child != null)
                            {
                                child.Reset();
                            }
                        }
                    }
                }
            }

            internal AttributeNode CurrentAttribute
            {
                get
                {
                    return this.attributes[this.attributeIndex];
                }
            }

            public int Size
            {
                get
                {
                    int size = 2 * this.name.Length + 6; //upper bound <NAME></NAME>
                    if (!string.IsNullOrEmpty(this.prefix))
                    {
                        size += this.prefix.Length + 1;
                    }
                    if (!string.IsNullOrEmpty(this.xmlns))
                    {
                        size += this.xmlns.Length + 9;  // xmlns="xmlns" 
                    }
                    return size;
                }
            }
        }

        class AttributeNode : IMeasurable
        {
            internal AttributeNode(string name, string prefix, string value, string xmlns)
            {
                this.name = name;
                this.prefix = prefix;
                this.nodeValue = value;
                this.xmlns = xmlns;
            }

            internal string name;
            internal string nodeValue;
            internal string prefix;
            internal string xmlns;

            public int Size
            {
                get
                {
                    int size = this.name.Length + this.nodeValue.Length + 5; 
                    
                    if (!string.IsNullOrEmpty(this.prefix))
                    {
                        size += this.prefix.Length + 1;
                    }
                    
                    if (!string.IsNullOrEmpty(this.xmlns))
                    {
                        size += this.xmlns.Length + 9; //upper bound
                    }

                    return size;
                }
            }
        }

        class ProcessingInstructionNode : TraceNode, IMeasurable
        {
            internal ProcessingInstructionNode(string name, string text, ElementNode parent)
                :
                base(XPathNodeType.ProcessingInstruction, parent)
            {
                this.name = name;
                this.text = text;
            }

            internal string name;
            internal string text;
            
            public int Size
            {
                get
                {
                    return this.name.Length + this.text.Length + 12; //<?xml NAME="TEXT"?>
                }
            }
        }

        class TextNode : IMeasurable
        {
            internal TextNode(string value)
            {
                this.nodeValue = value;
            }
            internal string nodeValue;

            public int Size
            {
                get
                {
                    return this.nodeValue.Length;
                }
            }
        }

        internal void AddElement(string prefix, string name, string xmlns)
        {
            if (this.closed)
            {
#pragma warning disable 618
                Fx.Assert("Cannot add data to a closed document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            else
            {
                ElementNode node = new ElementNode(name, prefix, this.CurrentElement, xmlns);
                if (this.current == null)
                {
                    this.VerifySize(node);
                    this.root = node;
                    this.current = this.root;
                }
                else if (!this.closed)
                {
                    this.VerifySize(node);
                    this.CurrentElement.Add(node);
                    this.current = node;
                }
            }
        }

        internal void AddProcessingInstruction(string name, string text)
        {
            if (this.current == null)
            {
                return;
            }
            else
            {
                ProcessingInstructionNode node = new ProcessingInstructionNode(name, text, this.CurrentElement);
                this.VerifySize(node);
                this.CurrentElement.Add(node);
            }
        }

        internal void AddText(string value)
        {
            if (this.closed)
            {
#pragma warning disable 618
                Fx.Assert("Cannot add data to a closed document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            if (this.current == null)
            {
                return;
            }
            else
            {
                if (this.CurrentElement.text == null)
                {
                    TextNode node = new TextNode(value);
                    this.VerifySize(node);
                    this.CurrentElement.text = node;
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    this.VerifySize(value);
                    this.CurrentElement.text.nodeValue += value;
                }
            }
        }

        internal void AddAttribute(string name, string value, string xmlns, string prefix)
        {
            if (this.closed)
            {
#pragma warning disable 618
                Fx.Assert("Cannot add data to a closed document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            AttributeNode node = new AttributeNode(name, prefix, value, xmlns);
            this.VerifySize(node);
            this.CurrentElement.attributes.Add(node);
        }

        internal void AddComment(string text)
        {
            if (this.closed)
            {
#pragma warning disable 618
                Fx.Assert("Cannot add data to a closed document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            CommentNode node = new CommentNode(text, this.CurrentElement);
            this.VerifySize(node);
            this.CurrentElement.Add(node);
        }

        internal void CloseElement()
        {
            if (this.closed)
            {
#pragma warning disable 618
                Fx.Assert("The document is already closed.");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            else
            {
                this.current = this.CurrentElement.parent;
                if (this.current == null)
                {
                    this.closed = true;
                }
            }
        }

        public override string BaseURI
        {
            get { return String.Empty; }
        }

        public override XPathNavigator Clone()
        {
            return this;
        }

        public override bool IsEmptyElement
        {
            get
            {
                bool retval = true;
                if (this.current != null)
                {
                    retval = this.CurrentElement.text != null || this.CurrentElement.childNodes.Count > 0;
                }
                return retval;
            }
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            return false;
        }

        [DebuggerDisplay("")]
        public override string LocalName
        {
            get { return this.Name; }
        }

        public override string LookupPrefix(string ns)
        {
            return this.LookupPrefix(ns, this.CurrentElement);
        }

        string LookupPrefix(string ns, ElementNode node)
        {
            string retval = null;
            if (string.Compare(ns, node.xmlns, StringComparison.Ordinal) == 0)
            {
                retval = node.prefix;
            }
            else
            {
                foreach (AttributeNode attributeNode in node.attributes)
                {
                    if (string.Compare("xmlns", attributeNode.prefix, StringComparison.Ordinal) == 0)
                    {
                        if (string.Compare(ns, attributeNode.nodeValue, StringComparison.Ordinal) == 0)
                        {
                            retval = attributeNode.name;
                            break;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(retval) && node.parent != null)
            {
                retval = LookupPrefix(ns, node.parent);
            }
            return retval;
        }

        public override bool MoveTo(XPathNavigator other)
        {
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            bool retval = this.CurrentElement.MoveToFirstAttribute();
            if (retval)
            {
                this.state = XPathNodeType.Attribute;
            }
            return retval;
        }

        public override bool MoveToFirstChild()
        {
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            bool retval = false;
            if (null != this.CurrentElement.childNodes && this.CurrentElement.childNodes.Count > 0)
            {
                this.current = this.CurrentElement.childNodes[0];
                this.state = this.current.NodeType;
                retval = true;
            }
            else if ((null == this.CurrentElement.childNodes || this.CurrentElement.childNodes.Count == 0) && this.CurrentElement.text != null)
            {
                this.state = XPathNodeType.Text;
                this.CurrentElement.movedToText = true;
                retval = true;
            }
            return retval;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNext()
        {
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            bool retval = false;
            if (this.state != XPathNodeType.Text)
            {
                ElementNode parent = this.current.parent;
                if (parent != null)
                {
                    TraceNode temp = parent.MoveToNext();
                    if (temp == null && parent.text != null && !parent.movedToText)
                    {
                        this.state = XPathNodeType.Text;
                        parent.movedToText = true;
                        this.current = parent;
                        retval = true;
                    }
                    else if (temp != null)
                    {
                        this.state = temp.NodeType;
                        retval = true;
                        this.current = temp;
                    }
                }
            }
            return retval;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            bool retval = this.CurrentElement.MoveToNextAttribute();
            if (retval)
            {
                this.state = XPathNodeType.Attribute;
            }
            return retval;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            if (this.current == null)
            {
#pragma warning disable 618
                Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                throw new InvalidOperationException();
            }
            bool retval = false;
            switch (this.state)
            {
                case XPathNodeType.Comment:
                case XPathNodeType.Element:
                case XPathNodeType.ProcessingInstruction:
                    if (this.current.parent != null)
                    {
                        this.current = this.current.parent;
                        this.state = this.current.NodeType;
                        retval = true;
                    }
                    break;
                case XPathNodeType.Attribute:
                    this.state = XPathNodeType.Element;
                    retval = true;
                    break;
                case XPathNodeType.Text:
                    this.state = XPathNodeType.Element;
                    retval = true;
                    break;
                case XPathNodeType.Namespace:
                    this.state = XPathNodeType.Element;
                    retval = true;
                    break;
            }
            return retval;
        }

        public override bool MoveToPrevious()
        {
            return false;
        }

        public override void MoveToRoot()
        {
            this.current = this.root;
            this.state = XPathNodeType.Element;
            this.root.Reset();
        }

        [DebuggerDisplay("")]
        public override string Name
        {
            get
            {
                string retval = String.Empty;
                if (this.current == null)
                {
#pragma warning disable 618
                    Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                }
                else
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Attribute:
                            retval = this.CurrentElement.CurrentAttribute.name;
                            break;
                        case XPathNodeType.Element:
                            retval = this.CurrentElement.name;
                            break;
                        case XPathNodeType.ProcessingInstruction:
                            retval = this.CurrentProcessingInstruction.name;
                            break;
                    }
                }
                return retval;
            }
        }

        public override System.Xml.XmlNameTable NameTable
        {
            get { return null; }
        }

        [DebuggerDisplay("")]
        public override string NamespaceURI
        {
            get
            {
                string retval = String.Empty;
                if (this.current == null)
                {
#pragma warning disable 618
                    Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                }
                else
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Element:
                            retval = this.CurrentElement.xmlns;
                            break;
                        case XPathNodeType.Attribute:
                            retval = this.CurrentElement.CurrentAttribute.xmlns;
                            break;
                        case XPathNodeType.Namespace:
                            retval = null;
                            break;
                    }
                }
                return retval;
            }
        }

        [DebuggerDisplay("")]
        public override XPathNodeType NodeType
        {
            get { return this.state; }
        }

        [DebuggerDisplay("")]
        public override string Prefix
        {
            get
            {
                string retval = String.Empty;
                if (this.current == null)
                {
#pragma warning disable 618
                    Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                }
                else
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Element:
                            retval = this.CurrentElement.prefix;
                            break;
                        case XPathNodeType.Attribute:
                            retval = this.CurrentElement.CurrentAttribute.prefix;
                            break;
                        case XPathNodeType.Namespace:
                            retval = null;
                            break;
                    }
                }
                return retval;
            }
        }

        CommentNode CurrentComment
        {
            get { return this.current as CommentNode; }
        }

        ElementNode CurrentElement
        {
            get { return this.current as ElementNode; }
        }

        ProcessingInstructionNode CurrentProcessingInstruction
        {
            get { return this.current as ProcessingInstructionNode; }
        }

        [DebuggerDisplay("")]
        public override string Value
        {
            get
            {
                string retval = String.Empty;
                if (this.current == null)
                {
#pragma warning disable 618
                    Fx.Assert("Operation is invalid on an empty document");
#pragma warning restore 618
                }
                else
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Text:
                            retval = this.CurrentElement.text.nodeValue;
                            break;
                        case XPathNodeType.Attribute:
                            retval = this.CurrentElement.CurrentAttribute.nodeValue;
                            break;
                        case XPathNodeType.Comment:
                            retval = this.CurrentComment.nodeValue;
                            break;
                        case XPathNodeType.ProcessingInstruction:
                            retval = this.CurrentProcessingInstruction.text;
                            break;
                    }
                }
                return retval;
            }
        }

        internal WriteState WriteState
        {
            get
            {
                WriteState retval = WriteState.Error;
                if (this.current == null)
                {
                    retval = WriteState.Start;
                }
                else if (this.closed)
                {
                    retval = WriteState.Closed;
                }
                else
                {
                    switch (this.state)
                    {
                        case XPathNodeType.Attribute:
                            retval = WriteState.Attribute;
                            break;
                        case XPathNodeType.Element:
                            retval = WriteState.Element;
                            break;
                        case XPathNodeType.Text:
                            retval = WriteState.Content;
                            break;
                        case XPathNodeType.Comment:
                            retval = WriteState.Content;
                            break;
                    }
                }
                return retval;
            }
        }

        public override string ToString()
        {
            this.MoveToRoot();
            StringBuilder sb = new StringBuilder();
            EncodingFallbackAwareXmlTextWriter writer = new EncodingFallbackAwareXmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture));
            writer.WriteNode(this, false);
            return sb.ToString();
        }

        void VerifySize(IMeasurable node)
        {
            this.VerifySize(node.Size);
        }

        void VerifySize(string node)
        {
            this.VerifySize(node.Length);
        }

        void VerifySize(int nodeSize)
        {
            if (this.maxSize != TraceXPathNavigator.UnlimitedSize)
            {
                if (this.currentSize + nodeSize > this.maxSize)
                {
                    throw new PlainXmlWriter.MaxSizeExceededException();
                }
            }
            this.currentSize += nodeSize;
        }

        public void RemovePii(string[][] paths)
        {
#pragma warning disable 618
            Fx.Assert(null != paths, "");
#pragma warning restore 618
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            foreach (string[] path in paths)
            {
                RemovePii(path);
            }
        }

        public void RemovePii(string[] path)
        {
            RemovePii(path, DiagnosticStrings.PiiList);
        }

        public void RemovePii(string[] headersPath, string[] piiList)
        {
#pragma warning disable 618
            Fx.Assert(null != this.root, "");
            if (this.root == null)
            {
                throw new InvalidOperationException();
            }
            foreach (ElementNode node in this.root.FindSubnodes(headersPath))
            {
                Fx.Assert(null != node, "");
                MaskSubnodes(node, piiList);
            }
        }
#pragma warning restore 618

        static void MaskElement(ElementNode element)
        {
            if (null != element)
            {
                element.childNodes.Clear();
                element.Add(new CommentNode("Removed", element));
                element.text = null;
                element.attributes = null;
            }
        }

        static void MaskSubnodes(ElementNode element, string[] elementNames)
        {
            MaskSubnodes(element, elementNames, false);
        }

        static void MaskSubnodes(ElementNode element, string[] elementNames, bool processNodeItself)
        {
#pragma warning disable 618
            Fx.Assert(null != elementNames, "");
#pragma warning restore 618
            if (elementNames == null)
            {
                throw new ArgumentNullException("elementNames");
            }

            if (null != element)
            {
                bool recurse = true;
                if (processNodeItself)
                {
                    foreach (string elementName in elementNames)
                    {
#pragma warning disable 618
                        Fx.Assert(!String.IsNullOrEmpty(elementName), "");
#pragma warning restore 618
                        if (0 == String.CompareOrdinal(elementName, element.name))
                        {
                            MaskElement(element);
                            recurse = false;
                            break;
                        }
                    }
                }
                if (recurse)
                {
                    if (null != element.childNodes)
                    {
                        foreach (ElementNode subNode in element.childNodes)
                        {
#pragma warning disable 618
                            Fx.Assert(null != subNode, "");
#pragma warning restore 618
                            MaskSubnodes(subNode, elementNames, true);
                        }
                    }
                }
            }
        }
    }
}
