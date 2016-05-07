//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    // Anything marked 




    class SeekableMessageNavigator : SeekableXPathNavigator, INodeCounter
    {
        // Use a single template for constructing the common elements.
        static Node[] BlankDom;

        // Constants
        const string XmlP = "xml";
        const string XmlnsP = "xmlns";
        const string SoapP = "s";
        const string EnvelopeTag = MessageStrings.Envelope;
        const string HeaderTag = MessageStrings.Header;
        const string BodyTag = MessageStrings.Body;

        // These constants are constructed from each other so new ones can be added without too much hastle.
        const int NullIndex = 0;
        const int RootIndex = NullIndex + 1;
        const int EnvelopeIndex = RootIndex + 1;
        const int SoapNSIndex = EnvelopeIndex + 1;
        const int XmlNSIndex = SoapNSIndex + 1;
        const int HeaderIndex = XmlNSIndex + 1;
        const int FirstHeaderIndex = HeaderIndex + 1;

        const int StartSize = 50; // Blank nodes to start with
        const int GrowFactor = 2; // Multiplier to grow the node array by
        const int StretchMax = 1000; // Use the multiplier until the node array gets this big
        const int GrowInc = 1000; // Grow by this many nodes when StretchMax is exceeded

        // Shared as dom
        Message message;
        MessageHeaders headers;
        XmlSpace space;
        StringBuilder stringBuilder;

        Node[] nodes;
        int bodyIndex;
        int nextFreeIndex;

        NameTable nameTable;
        bool includeBody;
        bool atomize;

        // Shared as counter
        int nodeCount;
        int nodeCountMax;

        // Instance
        SeekableMessageNavigator dom;
        SeekableMessageNavigator counter;
        Stack<string> nsStack;
        int location;
        int specialParent; // Always null if we're not at a namespace node

        static SeekableMessageNavigator()
        {
            BlankDom = new Node[HeaderIndex + 1];

            // Root
            BlankDom[RootIndex].type = XPathNodeType.Root;
            BlankDom[RootIndex].firstChild = EnvelopeIndex;
            BlankDom[RootIndex].prefix = string.Empty;
            BlankDom[RootIndex].name = string.Empty;
            BlankDom[RootIndex].val = string.Empty;

            // Envelope
            BlankDom[EnvelopeIndex].type = XPathNodeType.Element;
            BlankDom[EnvelopeIndex].prefix = SoapP;
            //BlankDom[EnvelopeIndex].ns = soapNS;
            BlankDom[EnvelopeIndex].name = EnvelopeTag;
            BlankDom[EnvelopeIndex].parent = RootIndex;
            BlankDom[EnvelopeIndex].firstChild = HeaderIndex;
            BlankDom[EnvelopeIndex].firstNamespace = SoapNSIndex;

            // SOAP Namespace
            BlankDom[SoapNSIndex].type = XPathNodeType.Namespace;
            BlankDom[SoapNSIndex].name = SoapP;
            //BlankDom[SoapNSIndex].val = soapNS;
            BlankDom[SoapNSIndex].nextSibling = XmlNSIndex;
            BlankDom[SoapNSIndex].parent = EnvelopeIndex;

            // xmlns:xml Namespace
            BlankDom[XmlNSIndex].type = XPathNodeType.Namespace;
            BlankDom[XmlNSIndex].name = "xml";
            BlankDom[XmlNSIndex].val = XmlUtil.XmlNs;
            BlankDom[XmlNSIndex].prevSibling = SoapNSIndex;
            BlankDom[XmlNSIndex].parent = RootIndex; // This one needs to be connected a little different

            // Header
            BlankDom[HeaderIndex].type = XPathNodeType.Element;
            BlankDom[HeaderIndex].prefix = SoapP;
            //BlankDom[HeaderIndex].ns = soapNS;
            BlankDom[HeaderIndex].name = HeaderTag;
            BlankDom[HeaderIndex].parent = EnvelopeIndex;
            //BlankDom[HeaderIndex].nextSibling = bodyIndex;
            //BlankDom[HeaderIndex].firstChild = this.bodyIndex != FirstHeaderIndex ? FirstHeaderIndex : NullIndex;
            BlankDom[HeaderIndex].firstNamespace = SoapNSIndex;

        }

        internal SeekableMessageNavigator(SeekableMessageNavigator nav)
        {
            Fx.Assert(nav != null, "Navigator may not be null");

            this.counter = nav.counter;
            this.dom = nav.dom;
            this.location = nav.location;
            this.specialParent = nav.specialParent;
            if (this.specialParent != NullIndex)
            {
                this.nsStack = nav.CloneNSStack();
            }
        }

        internal SeekableMessageNavigator(Message msg, int countMax, XmlSpace space, bool includeBody, bool atomize)
        {
            Init(msg, countMax, space, includeBody, atomize);
        }

        // The base uri of the element
        // This is usually associated with the URI of the original data's location
        // WS, [....], look into what readers from messages surface.  If it's always null, we can save
        // some memory
        public override string BaseURI
        {
            get
            {
                LoadOnDemand();
                string s = this.dom.nodes[this.location].baseUri;
                return s == null ? string.Empty : s;
            }
        }

        // Get/Set an opaque position value.
        // This property will save and restore a navigator's position without cloning
        // The two integers needed to uniquely identify this navigators location within the DOM are encoded in 
        // a long.  The high 32 bits is the parent index, and the low 32 bits is the current node index.
        public override long CurrentPosition
        {
            get
            {
                long p = this.specialParent;
                p <<= 32;
                p += this.location;
                return p;
            }
            set
            {
                Position p = this.dom.DecodePosition(value);

                // If we are at a namespace, collect the namespaces that have already been seen
                if (p.parent != NullIndex)
                {
                    if (this.nsStack == null)
                    {
                        this.nsStack = new Stack<string>();
                    }
                    else
                    {
                        this.nsStack.Clear();
                    }

                    int n = this.dom.nodes[p.parent].firstNamespace;

                    while (n != p.elem)
                    {
                        // PERF, [....], we might be able to get rid of this check by tweaking the position
                        // validator
                        if (n == NullIndex)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNavigatorPosition, SR.GetString(SR.SeekableMessageNavInvalidPosition)));
                        }

                        this.nsStack.Push(this.dom.nodes[n].name);
                        n = this.dom.nodes[n].nextSibling;
                    }
                }

                this.location = p.elem;
                this.specialParent = p.parent;
            }
        }

        // Returns whether the current node has any defined attributes.
        public override bool HasAttributes
        {
            get
            {
                LoadOnDemand();
                return this.dom.nodes[this.location].firstAttribute != NullIndex;
            }
        }

        // Returns whether the current node has any children.
        public override bool HasChildren
        {
            get
            {
                LoadOnDemand();
                return this.dom.nodes[this.location].firstChild != NullIndex;
            }
        }

        // Returns whether the current node was defined as an empty element.
        // This is not relevant to xpath, so we're saving memory by not recording the true value.
        // If a reader does ever get wrapped around it, we'll have to fix this.
        public override bool IsEmptyElement
        {
            get
            {
                // <a/> is an empty element.  <a></a> is not.
                return this.dom.nodes[this.location].empty;
            }
        }

        // Return the local name of the current node.
        public override string LocalName
        {
            get
            {
                string s = this.dom.nodes[this.location].name;
                return s == null ? string.Empty : s;
            }
        }

        // Returns the message that was used to construct this navigator's DOM
        internal Message Message
        {
            get
            {
                return this.dom.message;
            }
        }

        // Returns the qualified name of the current node
        public override string Name
        {
            get
            {
                return GetName(this.location);
            }
        }

        // Returns the namespace URI of the current node
        public override string NamespaceURI
        {
            get
            {
                string s = this.dom.nodes[this.location].ns;
                return s == null ? string.Empty : s;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                // Delay atomizing for the cases where we typically don't
                // need to do it (e.g. creating a navigator over a Message).
                if (!this.dom.atomize)
                {
                    this.dom.Atomize();
                }
                return this.dom.nameTable;
            }
        }

        // Returns the type of the current node
        public override XPathNodeType NodeType
        {
            get
            {
                return this.dom.nodes[this.location].type;
            }
        }

        // Returns the prefix of the current node
        public override string Prefix
        {
            get
            {
                LoadOnDemand();
                string s = this.dom.nodes[this.location].prefix;
                return s == null ? string.Empty : s;
            }
        }

        // Returns the text value of the current node
        public override string Value
        {
            get
            {
                return this.dom.GetValue(this.location);
            }
        }

        // Returns the value of xml:lang that is currently in scope.
        public override string XmlLang
        {
            get
            {
                LoadOnDemand();
                string s = this.dom.nodes[this.location].xmlLang;
                return s == null ? string.Empty : s;
            }
        }

#if NO
        // The default value of xml:space for the DOM
        internal XmlSpace DefaultXmlSpace
        {
            get
            {
                return this.dom.space;
            }
        }

        internal SeekableMessageNavigator DomNavigator
        {
            get
            {
                return this.dom;
            }
        }
#endif

        // Create a clone of the current navigator that shares its DOM and node counter
        public override XPathNavigator Clone()
        {
            return new SeekableMessageNavigator(this);
        }

        // Compare the current position to that of another navigator.
        public override XmlNodeOrder ComparePosition(XPathNavigator nav)
        {
            if (nav == null)
            {
                return XmlNodeOrder.Unknown;
            }

            // We can only compare the positions of navigators of the same type.
            SeekableMessageNavigator smnav = nav as SeekableMessageNavigator;
            if (smnav != null)
            {
                return ComparePosition(smnav);
            }
            return XmlNodeOrder.Unknown;
        }

        // Compare the current position to that of another navigator.
        internal XmlNodeOrder ComparePosition(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return XmlNodeOrder.Unknown;
            }

            if (this.dom != nav.dom)
            {
                return XmlNodeOrder.Unknown;
            }

            return this.dom.ComparePosition(this.specialParent, this.location, nav.specialParent, nav.location);
        }

        // Compare two position values that are valid for this navigator's DOM
        public override XmlNodeOrder ComparePosition(long pos1, long pos2)
        {
            Position p1 = this.dom.DecodePosition(pos1);
            Position p2 = this.dom.DecodePosition(pos2);
            return this.dom.ComparePosition(p1.parent, p1.elem, p2.parent, p2.elem);
        }

        // Compile: We don't need to override this.  Seems like it could have even been static.

        // Evaluate an xpath expression against the current navigator
        public override object Evaluate(string xpath)
        {
            // We can only evaluate atomized navigators        
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Evaluate")));
            }
            return base.Evaluate(xpath);
        }

        // Evaluate an xpath expression against the current navigator
        public override object Evaluate(XPathExpression expr)
        {
            // We can only evaluate atomized navigators        
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Evaluate")));
            }
            return base.Evaluate(expr);
        }

        // Evaluate an xpath expression against the current navigator
        public override object Evaluate(XPathExpression expr, XPathNodeIterator context)
        {
            // We can only evaluate atomized navigators        
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Evaluate")));
            }
            return base.Evaluate(expr, context);
        }

        // Get the value of an attribute with a particular name and namespace
        public override string GetAttribute(string name, string ns)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }

            // We must be at an element node
            if (this.NodeType != XPathNodeType.Element)
            {
                return string.Empty;
            }

            int a;
            string ret = string.Empty;
            Increase();
            LoadOnDemand();

            // Walk through the attributes looking for the one we want.
            a = this.dom.nodes[this.location].firstAttribute;
            while (a != NullIndex)
            {
                if (String.CompareOrdinal(this.dom.nodes[a].name, name) == 0 && String.CompareOrdinal(this.dom.nodes[a].ns, ns) == 0)
                {
                    ret = this.dom.nodes[a].val;
                    break;
                }

                Increase();
                a = this.dom.nodes[a].nextSibling;
            }

            return ret;
        }

        // Returns the local name of the node defined by the given navigator position
        public override string GetLocalName(long pos)
        {
            string s = this.dom.nodes[this.dom.DecodePosition(pos).elem].name;
            return s == null ? string.Empty : s;
        }

        // Returns the qualified name of the node defined by the given navigator position
        public override string GetName(long pos)
        {
            return GetName(this.dom.DecodePosition(pos).elem);
        }

        // Returns the namespace uri of the node defined by the given navigator position
        public override string GetNamespace(long pos)
        {
            string s = this.dom.nodes[this.dom.DecodePosition(pos).elem].ns;
            return s == null ? string.Empty : s;
        }

        // Returns the namespace uri that is bound to the given prefix in the current scope, or the empty
        // string if the prefix is not defined
        public override string GetNamespace(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            if (this.NodeType != XPathNodeType.Element)
            {
                return string.Empty;
            }

            int ns;
            Increase();
            LoadOnDemand();
            ns = this.dom.nodes[this.location].firstNamespace;
            string ret = string.Empty;

            while (ns != NullIndex)
            {
                Increase();

                if (String.CompareOrdinal(this.dom.nodes[ns].name, name) == 0)
                {
                    ret = this.dom.nodes[ns].val;
                    break;
                }

                ns = this.dom.nodes[ns].nextSibling;
            }

            return ret;
        }

        // Returns the node type of the node defined by the given navigator position
        public override XPathNodeType GetNodeType(long pos)
        {
            return this.dom.nodes[this.dom.DecodePosition(pos).elem].type;
        }

        // Returns the string value of the node defined by the given navigator position
        public override string GetValue(long pos)
        {
            string s = this.dom.GetValue(this.dom.DecodePosition(pos).elem);
            return s == null ? string.Empty : s;
        }

        // Test whether the given navigator is positioned on a descendant of this navigator
        public override bool IsDescendant(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            // Navigator positions can only be compared if they are of the same type
            SeekableMessageNavigator smnav = nav as SeekableMessageNavigator;
            if (smnav != null)
            {
                return IsDescendant(smnav);
            }
            return false;
        }

        // Test whether the given navigator is positioned on a descendant of this navigator
        internal bool IsDescendant(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            if (this.dom != nav.dom)
            {
                return false;
            }

            // Namespaces and attributes are not considered descendants
            XPathNodeType type = this.dom.nodes[nav.location].type;
            if (type == XPathNodeType.Namespace || type == XPathNodeType.Attribute)
            {
                return false;
            }

            // Namespaces and attributes are not parents
            type = this.dom.nodes[this.location].type;
            if (type == XPathNodeType.Namespace || type == XPathNodeType.Attribute)
            {
                return false;
            }

            // Climb up the tree looking for the current navigator's position
            int n = nav.location;
            while (n != NullIndex)
            {
                Increase();
                n = this.dom.nodes[n].parent;

                if (n == this.location)
                {
                    return true;
                }
            }
            return false;
        }

        // Tests whether the given navigator is positioned on the same node as this navigator
        public override bool IsSamePosition(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            // Navigator positions can only be compared if they are of the same type
            SeekableMessageNavigator smnav = nav as SeekableMessageNavigator;
            if (smnav != null)
            {
                return IsSamePosition(smnav);
            }
            return false;
        }

        // Tests whether the given navigator is positioned on the same node as this navigator
        internal bool IsSamePosition(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            return this.dom == nav.dom && this.location == nav.location && this.specialParent == nav.specialParent;
        }

        // Test if the given xpath matches this navigator
        public override bool Matches(string xpath)
        {
            // We can only match atomized navigators        
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Matches")));
            }
            return base.Matches(xpath);
        }

        // Test if the given xpath matches this navigator
        public override bool Matches(XPathExpression expr)
        {
            // We can only match atomized navigators        
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Matches")));
            }
            return base.Matches(expr);
        }

        // Move this navigator to the same position as the given one
        public override bool MoveTo(XPathNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            // Can only move to the position of a navigator of the same type
            SeekableMessageNavigator smnav = nav as SeekableMessageNavigator;
            if (smnav != null)
            {
                return MoveTo(smnav);
            }
            return false;
        }

        // Move this navigator to the same position as the given one
        internal bool MoveTo(SeekableMessageNavigator nav)
        {
            if (nav == null)
            {
                return false;
            }

            this.dom = nav.dom;
            this.counter = nav.counter;
            this.location = nav.location;
            this.specialParent = nav.specialParent;
            if (this.specialParent != NullIndex)
            {
                this.nsStack = nav.CloneNSStack();
            }

            return true;
        }

        // Move this navigator to an attribute with the given name and namespace
        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }

            if (namespaceURI == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("namespaceURI");
            }

            LoadOnDemand();

            // We must be positioned on an element
            if (this.dom.nodes[this.location].type != XPathNodeType.Element)
            {
                return false;
            }

            // Move through the attributes looking for one that matches
            Increase();
            int n = this.dom.nodes[this.location].firstAttribute;
            while (n != NullIndex)
            {
                if (String.CompareOrdinal(this.dom.nodes[n].name, localName) == 0 &&
                    String.CompareOrdinal(this.dom.nodes[n].ns, namespaceURI) == 0)
                {
                    // If we find it, we're done
                    this.location = n;
                    return true;
                }
                else
                {
                    // Try the next one
                    Increase();
                    n = this.dom.nodes[n].nextSibling;
                }
            }

            // Didn't find it
            return false;
        }

        // Move the navigator to the first sibling of the current node
        public override bool MoveToFirst()
        {
            // Not valid for attributes and namespaces
            XPathNodeType t = this.dom.nodes[this.location].type;
            if (t != XPathNodeType.Attribute && t != XPathNodeType.Namespace)
            {
                // Get the parent of the current node
                Increase();
                int p = this.dom.nodes[this.location].parent;

                // If the parent was null, we're at the root (which has no siblings)
                if (p != NullIndex)
                {
                    // Move to the first child of our node's parent (ie, first sibling)
                    // Cool, huh? :-)
                    Increase();
                    this.location = this.dom.nodes[p].firstChild;
                }

                // The move is still considered successful if we were already there.
                return true;
            }
            return false;
        }

        // Move the navigator to the first attribute of it's current element
        public override bool MoveToFirstAttribute()
        {
            // Only valid for element nodes
            if (this.dom.nodes[this.location].type != XPathNodeType.Element)
            {
                return false;
            }

            // Get the first attribute
            LoadOnDemand();
            int n = this.dom.nodes[this.location].firstAttribute;

            // The move was only successful if there is at least one attribute
            if (n != NullIndex)
            {
                Increase();
                this.location = n;
                return true;
            }

            return false;
        }

        // Move the navigator to the first child of the current node.
        public override bool MoveToFirstChild()
        {
            // PERF, [....], do we need this check?  The null check may be enough
            // Only valid for the root or an element node
            if (this.location == RootIndex || this.dom.nodes[this.location].type == XPathNodeType.Element)
            {
                // Get the first child
                LoadOnDemand();
                int n = this.dom.nodes[this.location].firstChild;

                // Only successful if there was at least one child
                if (n != NullIndex)
                {
                    Increase();
                    this.location = n;
                    return true;
                }
            }
            return false;
        }

        // Move the navigator to the first namespace that fits the scope
        public override bool MoveToFirstNamespace(XPathNamespaceScope scope)
        {
            // Only valid on element nodes
            if (this.dom.nodes[this.location].type != XPathNodeType.Element)
            {
                return false;
            }

            // Start with a clean namespace stack
            if (this.nsStack == null)
            {
                this.nsStack = new Stack<string>();
            }
            else
            {
                this.nsStack.Clear();
            }

            // Find the namespace
            LoadOnDemand();
            int n = FindNamespace(this.location, this.dom.nodes[this.location].firstNamespace, scope);

            // If one was found, move there
            if (n != NullIndex)
            {
                // Record the parent
                this.specialParent = this.location;
                Increase();
                this.location = n;
                return true;
            }
            return false;
        }

        // Move the navigator to the node with the given unique ID
        public override bool MoveToId(string id)
        {
            // SOAP prohibits the inclusion of a DTD, so unique IDs cannot be defined.
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.NotSupported, SR.GetString(SR.SeekableMessageNavIDNotSupported)));
        }

        // Move the navigator to the namespace manager the given prefix
        public override bool MoveToNamespace(string name)
        {
            if (name == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("name");
            }

            // Only valid from an element node
            if (this.dom.nodes[this.location].type != XPathNodeType.Element)
            {
                return false;
            }

            // Start with a clean namespace stack
            if (this.nsStack == null)
            {
                this.nsStack = new Stack<string>();
            }
            else
            {
                this.nsStack.Clear();
            }

            Increase();
            LoadOnDemand();

            // Look through the namespaces for the specified prefix
            int n = this.dom.nodes[this.location].firstNamespace;
            string nodeName;
            string nodeVal;
            int nsCount = 0;
            while (n != NullIndex)
            {
                // Skip any already defined prefixes
                nodeName = this.dom.nodes[n].name;
                if (!this.nsStack.Contains(nodeName))
                {
                    this.nsStack.Push(nodeName);
                    ++nsCount;
                    nodeVal = this.dom.nodes[n].val;
                    if ((nodeName.Length > 0 || nodeVal.Length > 0) && String.CompareOrdinal(nodeName, name) == 0)
                    {
                        this.specialParent = this.location;
                        this.location = n;
                        return true;
                    }
                }

                // Try the next one
                Increase();
                n = this.dom.nodes[n].nextSibling;
            }

            // PERF, [....], can we just clear?
            // We didn't find it, so restore the namespace stack
            for (int i = 0; i < nsCount; ++i)
            {
                this.nsStack.Pop();
            }

            return false;
        }

        // Move the navigator to the next sibling
        public override bool MoveToNext()
        {
            // Not valid for attribute/namespace nodes.  They have different 'next' functions.
            XPathNodeType type = this.dom.nodes[this.location].type;
            if (type == XPathNodeType.Attribute || type == XPathNodeType.Namespace)
            {
                return false;
            }

            // Successful if there is a next sibling.
            int n = this.dom.nodes[this.location].nextSibling;
            if (n != NullIndex)
            {
                Increase();
                this.location = n;
                return true;
            }

            return false;
        }

        // Move the navigator to the next attribute node
        public override bool MoveToNextAttribute()
        {
            // Only valid on an attribute node
            if (this.dom.nodes[this.location].type != XPathNodeType.Attribute)
            {
                return false;
            }

            // Successful if there is a next sibling
            int n = this.dom.nodes[this.location].nextSibling;
            if (n != NullIndex)
            {
                Increase();
                this.location = n;
                return true;
            }

            return false;
        }

        // Move the navigator to the next namespace node in the specified scope
        public override bool MoveToNextNamespace(XPathNamespaceScope scope)
        {
            // Only valid from a namespace node
            if (this.dom.nodes[this.location].type != XPathNodeType.Namespace)
            {
                return false;
            }

            // Successful if the a namespace was found in the given scope
            int n = FindNamespace(this.specialParent, this.dom.nodes[this.location].nextSibling, scope);
            if (n != NullIndex)
            {
                // Move to the namespace
                Increase();
                this.location = n;
                return true;
            }
            return false;
        }

        // Move the navigator to the parent of the current node
        public override bool MoveToParent()
        {
            // The root node doesn't have a parent
            if (this.location == RootIndex)
            {
                return false;
            }

            // Use the special parent if it's not null
            Increase();
            if (this.specialParent != NullIndex)
            {
                Increase();
                this.location = this.specialParent;
                this.specialParent = NullIndex;
            }
            else
            {
                this.location = this.dom.nodes[this.location].parent;
            }

            return true;
        }

        // Move the navigator to the previous sibling of the current node
        public override bool MoveToPrevious()
        {
            // Not valid for attribute/namespace nodes
            int n = NullIndex;
            XPathNodeType t = this.dom.nodes[this.location].type;
            if (t != XPathNodeType.Attribute && t != XPathNodeType.Namespace)
            {
                n = this.dom.nodes[this.location].prevSibling;
            }

            // Successful if there was a previous sibling
            if (n != NullIndex)
            {
                Increase();
                this.location = n;
                return true;
            }
            return false;
        }

        // Move the navigator to the root node
        public override void MoveToRoot()
        {
            Increase();
            this.location = RootIndex;
            this.specialParent = NullIndex;
        }

        // Select nodes from the navigator using the given xpath
        public override XPathNodeIterator Select(string xpath)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Select")));
            }
            return base.Select(xpath);
        }

        // Select nodes from the navigator using the given xpath
        public override XPathNodeIterator Select(XPathExpression xpath)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "Select")));
            }
            return base.Select(xpath);
        }

        // Select ancestor nodes from the navigator of a given type
        public override XPathNodeIterator SelectAncestors(XPathNodeType type, bool matchSelf)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectAncestors")));
            }
            return base.SelectAncestors(type, matchSelf);
        }

        // Select ancestor nodes from the navigator with a particular name and namespace
        public override XPathNodeIterator SelectAncestors(string name, string namespaceURI, bool matchSelf)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectAncestors")));
            }
            return base.SelectAncestors(name, namespaceURI, matchSelf);
        }

        // Select child nodes of a certain type from the navigator
        public override XPathNodeIterator SelectChildren(XPathNodeType type)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectChildren")));
            }
            return base.SelectChildren(type);
        }

        // Select child nodes from the navigator with a certain name and namespace
        public override XPathNodeIterator SelectChildren(string name, string namespaceURI)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectChildren")));
            }
            return base.SelectChildren(name, namespaceURI);
        }

        // Select descendant nodes of a certain type from the navigator
        public override XPathNodeIterator SelectDescendants(XPathNodeType type, bool matchSelf)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectDescendants")));
            }
            return base.SelectDescendants(type, matchSelf);
        }

        // Selectg descendant nodes from the navigator with a certain name and namespace
        public override XPathNodeIterator SelectDescendants(string name, string namespaceURI, bool matchSelf)
        {
            // Cannot select from an unatomized navigator
            if (!this.dom.atomize)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.NotAtomized, SR.GetString(SR.SeekableMessageNavNonAtomized, "SelectDescendants")));
            }
            return base.SelectDescendants(name, namespaceURI, matchSelf);
        }

        // ToString: Don't need to override this

        // Atomize the required strings if atomization was not enabled for this DOM
        // Calling this function permanently enables atomization for the DOM
        internal void Atomize()
        {
            if (!this.dom.atomize)
            {
                this.dom.atomize = true;
                this.dom.nameTable = new NameTable();
                this.dom.nameTable.Add(string.Empty);
                this.dom.Atomize(RootIndex, this.nextFreeIndex);
            }
        }

        // Set a new count for this navigator
        // Set it to use itself for the counter so all clones of it will share the new counter
        internal void ForkNodeCount(int count)
        {
            Fx.Assert(count > 0, "Maximum node count must be greater than zero");

            this.nodeCount = count;
            this.nodeCountMax = count;
            this.counter = this;
        }

        // THREAD: Synchronize this function if multiple threads can try to re-initialize an instance at the same time.  Also, if you reinitialize the navigator that holds information referenced by clones, the clones will be affected as well.
        internal void Init(Message msg, int countMax, XmlSpace space, bool includeBody, bool atomize)
        {
            Fx.Assert(countMax > 0, "Maximum node count must be greater than zero");
            this.counter = this;
            this.nodeCount = countMax;
            this.nodeCountMax = countMax;

            Fx.Assert(msg != null, "Message may not be null");
            this.dom = this;
            this.location = RootIndex;
            this.specialParent = NullIndex;
            this.includeBody = includeBody;
            this.message = msg;
            this.headers = msg.Headers;
            this.space = space;
            this.atomize = false; // Will get fixed at the end of this function

            int minSize = msg.Headers.Count + FirstHeaderIndex + 1;

            if (this.nodes == null || this.nodes.Length < minSize)
            {
                this.nodes = new Node[minSize + StartSize];
            }
            else
            {
                Array.Clear(this.nodes, 1, this.nextFreeIndex - 1);
            }

            this.bodyIndex = minSize - 1;
            this.nextFreeIndex = minSize;

            // Use the static blank DOM to create the first few nodes
            Array.Copy(BlankDom, this.nodes, HeaderIndex + 1);

            string soapNS = msg.Version.Envelope.Namespace;
            this.nodes[EnvelopeIndex].ns = soapNS;
            this.nodes[SoapNSIndex].val = soapNS;
            this.nodes[HeaderIndex].ns = soapNS;

            this.nodes[HeaderIndex].nextSibling = bodyIndex;
            this.nodes[HeaderIndex].firstChild = this.bodyIndex != FirstHeaderIndex ? FirstHeaderIndex : NullIndex;

            // Headers
            if (msg.Headers.Count > 0)
            {
                for (int i = FirstHeaderIndex, h = 0; h < msg.Headers.Count; ++i, ++h)
                {
                    this.nodes[i].type = XPathNodeType.Element;
                    this.nodes[i].parent = HeaderIndex;
                    this.nodes[i].nextSibling = i + 1;
                    this.nodes[i].prevSibling = i - 1;

                    // Extract the header block stub data
                    MessageHeaderInfo header = msg.Headers[h];
                    this.nodes[i].ns = header.Namespace;
                    this.nodes[i].name = header.Name;
                    this.nodes[i].firstChild = -1;
                }
                this.nodes[FirstHeaderIndex].prevSibling = NullIndex;
                this.nodes[this.bodyIndex - 1].nextSibling = NullIndex;
            }

            // Body
            this.nodes[bodyIndex].type = XPathNodeType.Element;
            this.nodes[bodyIndex].prefix = SoapP;
            this.nodes[bodyIndex].ns = soapNS;
            this.nodes[bodyIndex].name = BodyTag;
            this.nodes[bodyIndex].parent = EnvelopeIndex;
            this.nodes[bodyIndex].prevSibling = HeaderIndex;
            this.nodes[bodyIndex].firstNamespace = SoapNSIndex;
            this.nodes[bodyIndex].firstChild = -1; // Need to load

            // Atomize
            if (atomize)
            {
                Atomize();
            }
        }

        // Add an attribute node
        // Must be called on the DOM object
        void AddAttribute(int node, int attr)
        {
            // Since order just needs to be consistant, we can just push it on the front of the list
            this.nodes[attr].parent = node;
            this.nodes[attr].nextSibling = this.nodes[node].firstAttribute;
            this.nodes[node].firstAttribute = attr;
        }

        // Append a node to another's set of children
        // Must be called on the DOM object
        void AddChild(int parent, int child)
        {
            // What we do depends on whether there are alredy children
            if (this.nodes[parent].firstChild == NullIndex)
            {
                // Make the node the only child
                this.nodes[parent].firstChild = child;
                this.nodes[child].parent = parent;
            }
            else
            {
                // Make the new child the last sibling of the first child
                AddSibling(this.nodes[parent].firstChild, child);
            }
        }

        // Add a namespace node
        // Must be called on the DOM object
        void AddNamespace(int node, int ns)
        {
            // Since order just needs to be consistant, we can just push it on the front of the list
            this.nodes[ns].parent = node;
            this.nodes[ns].nextSibling = this.nodes[node].firstNamespace;
            this.nodes[node].firstNamespace = ns;
        }

        // Make a node the last sibling of another node
        // Must be called on the DOM object
        void AddSibling(int node1, int node2)
        {
            // Get the current last sibling
            int i = LastSibling(node1);

            // Link the node in after the last sibling
            this.nodes[i].nextSibling = node2;
            this.nodes[node2].prevSibling = i;
            this.nodes[node2].parent = this.nodes[i].parent;
        }

        // Atomize the necessary strings within a range of nodes.
        // Must be called on the DOM object
        void Atomize(int first, int bound)
        {
            string s;
            for (; first < bound; ++first)
            {
                s = this.nodes[first].prefix;
                if (s != null)
                {
                    this.nodes[first].prefix = this.nameTable.Add(s);
                }

                s = this.nodes[first].name;
                if (s != null)
                {
                    this.nodes[first].name = this.nameTable.Add(s);
                }

                s = this.nodes[first].ns;
                if (s != null)
                {
                    this.nodes[first].ns = this.nameTable.Add(s);
                }
            }
        }
#if NO
        // Verify that a given position is valid for the document, and throw if it is not.
        // The node referenced must already have been constructed
        // Must be called on the DOM object
        void CheckValidPosition(int elem, int parent)
        {
            if (!IsValidPosition(elem, parent))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNavigatorPosition, SR.GetString(SR.SeekableMessageNavInvalidPosition)));
            }
        }
#endif
        // Create a copy of the current namespace stack
        Stack<string> CloneNSStack()
        {
            Stack<string> newStack = new Stack<string>();

            foreach (string s in this.nsStack)
            {
                newStack.Push(s);
            }

            return newStack;
        }

        // Compare the locations of two nodes in the DOM
        // This function assumes the nodes are both in the body or in the same header block
        XmlNodeOrder CompareLocation(int loc1, int loc2)
        {
            if (loc1 == loc2)
            {
                return XmlNodeOrder.Same;
            }
            else if (loc1 < loc2)
            {
                return XmlNodeOrder.Before;
            }
            else
            {
                return XmlNodeOrder.After;
            }
        }

        // Performs the actual position comparison.
        XmlNodeOrder ComparePosition(int p1, int loc1, int p2, int loc2)
        {
            // Both namespaces of the same node
            if (p1 == p2 && p1 != NullIndex)
            {
                return CompareLocation(loc1, loc2);
            }

            // Get the element of navigator 1
            int thisNode;
            if (p1 == NullIndex)
            {
                if (this.nodes[loc1].type == XPathNodeType.Attribute)
                {
                    thisNode = this.nodes[loc1].parent;
                }
                else
                {
                    thisNode = loc1;
                }
            }
            else
            {
                thisNode = p1;
            }

            // Get the element of navigator 2
            int thatNode;
            if (p2 == NullIndex)
            {
                if (this.nodes[loc2].type == XPathNodeType.Attribute)
                {
                    thatNode = this.nodes[loc2].parent;
                }
                else
                {
                    thatNode = loc2;
                }
            }
            else
            {
                thatNode = p2;
            }

            // If the elements are the same
            if (thisNode == thatNode)
            {
                XPathNodeType type1 = this.nodes[loc1].type;
                XPathNodeType type2 = this.nodes[loc2].type;

                // We already know they are not both namespaces.

                if (type1 == XPathNodeType.Namespace)
                {
                    if (type2 == XPathNodeType.Attribute)
                    {
                        // Namespaces come before attributes
                        return XmlNodeOrder.Before;
                    }
                    else
                    {
                        // loc2 is the element itself
                        return XmlNodeOrder.After;
                    }
                }

                if (type2 == XPathNodeType.Namespace)
                {
                    if (type1 == XPathNodeType.Attribute)
                    {
                        // Namespaces come before attributes
                        return XmlNodeOrder.After;
                    }
                    else
                    {
                        // loc1 is the element itself
                        return XmlNodeOrder.Before;
                    }
                }

            }

            // Need to find out which upper level element is the parent of each node
            // Upper level elements are in document order

            int thisP = thisNode;
            while (thisP > this.bodyIndex)
            {
                thisP = this.nodes[thisP].parent;
            }

            int thatP = thatNode;
            while (thatP > this.bodyIndex)
            {
                thatP = this.nodes[thatP].parent;
            }

            if (thisP == thatP)
            {
                return CompareLocation(loc1, loc2);
            }
            else
            {
                return CompareLocation(thisP, thatP);
            }
        }

        // Decompose the 'long' position into it's element and parent components 
        // Must be called on the DOM node
        Position DecodePosition(long pos)
        {
            Position p = new Position((int)pos, (int)(pos >> 32));

            // Make sure both nodes are valid
            if (p.elem > NullIndex && p.elem < this.nextFreeIndex)
            {
                if (p.parent == NullIndex)
                {
                    return p;
                }

                // If the parent is not null, make sure it is a valid parent and that the element is a namespace node
                if (p.parent > NullIndex && p.parent < this.nextFreeIndex && this.nodes[p.parent].type == XPathNodeType.Element && this.nodes[p.elem].type == XPathNodeType.Namespace)
                {
                    // Check that 'parent' is a descendant-or-self of elem->parent
                    int n = this.nodes[p.elem].parent;
                    int par = p.parent;

                    do
                    {
                        if (par == n)
                        {
                            return p;
                        }

                        par = this.nodes[par].parent;

                    } while (par != NullIndex);
                }
            }

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNavigatorPosition, SR.GetString(SR.SeekableMessageNavInvalidPosition)));
        }

        // Get the index of the next namespace that matches the scope
        // This function populates the namespace stack too
        // PERF, [....], see if we can have this function set the current location too
        int FindNamespace(int parent, int ns, XPathNamespaceScope scope)
        {
            bool done = false;
            int nsCount = 0;

            // ns is the first one we try
            while (ns != NullIndex && !done)
            {
                Increase();

                string nodeName, nodeVal;

                // Skip any prefixes that are already defined
                nodeName = this.dom.nodes[ns].name;
                if (this.nsStack.Contains(nodeName))
                {
                    ns = this.dom.nodes[ns].nextSibling;
                    continue;
                }
                this.nsStack.Push(nodeName);
                ++nsCount;

                // Skip the node undefining the default namespace
                nodeVal = this.dom.nodes[ns].val;
                if (nodeName.Length == 0 && nodeVal.Length == 0)
                {
                    continue;
                }

                // See if the current namespace is in our scope
                switch (scope)
                {
                    case XPathNamespaceScope.All:
                        done = true;
                        break;

                    case XPathNamespaceScope.ExcludeXml:
                        // If it's 'xml', keep looking
                        if (String.CompareOrdinal(nodeName, XmlP) == 0)
                        {
                            Increase();
                            ns = this.dom.nodes[ns].nextSibling;
                        }
                        else
                        {
                            done = true;
                        }
                        break;

                    case XPathNamespaceScope.Local:
                        // If the node's parent is not the element we're searching from, we've exhausted
                        // the locally defined namespaces
                        if (this.dom.nodes[ns].parent != parent)
                        {
                            ns = NullIndex;
                        }
                        else
                        {
                            done = true;
                        }

                        break;
                }
            }

            // If we didn't find one, restore the namespace stack
            if (ns == NullIndex)
            {
                for (int i = 0; i < nsCount; ++i)
                {
                    this.nsStack.Pop();
                }
            }

            return ns;
        }

        // Returns the qualified name of the given node
        string GetName(int elem)
        {
            string p, n;
            LoadOnDemand(elem);
            p = this.dom.nodes[elem].prefix;
            n = this.dom.nodes[elem].name;

            if (p != null && p.Length > 0)
            {
                return p + ":" + n;
            }
            return n;
        }

        // Returns the string value of given node
        // If the value is null, it attempts to acquire it
        // Must only be called on the DOM object
        string GetValue(int elem)
        {
            string val = this.nodes[elem].val;
            if (val == null)
            {
                if (this.stringBuilder == null)
                {
                    this.stringBuilder = new StringBuilder();
                }
                else
                {
                    this.stringBuilder.Length = 0;
                }

                GetValueDriver(elem);

                string s = this.stringBuilder.ToString();
                this.nodes[elem].val = s;
                return s;
            }
            else
            {
                return val;
            }
        }

        // Recursive function that collects up the string value of element nodes
        // Must only be called on the DOM object
        void GetValueDriver(int elem)
        {
            string val;
            this.dom.LoadOnDemand(elem);
            switch (this.nodes[elem].type)
            {
                // The string value of Element/Root nodes is the concatination of the values of their children
                case XPathNodeType.Element:
                case XPathNodeType.Root:
                    val = this.nodes[elem].val;
                    if (val == null)
                    {
                        int n = this.nodes[elem].firstChild;
                        while (n != NullIndex)
                        {
                            Increase();
                            GetValueDriver(n);
                            n = this.nodes[n].nextSibling;
                        }
                    }
                    else
                    {
                        this.stringBuilder.Append(val);
                    }
                    break;

                // String value cannot be 'computed' for other node types.
                default:
                    this.stringBuilder.Append(this.nodes[elem].val);
                    break;
            }
        }
#if NO
        // Checks if a node index is defined in the current DOM
        // Must be called on the DOM object
        bool IsValidNode(int n, bool allowNull)
        {
            if (allowNull)
            {
                return n >= NullIndex && n < this.dom.nextFreeIndex;
            }
            else
            {
                return n > NullIndex && n < this.dom.nextFreeIndex;
            }
        }

        // Verify that a given position is valid for the document.
        // The node referenced must already have been constructed
        // Must be called on the DOM object
        bool IsValidPosition(int elem, int parent)
        {
            // Make sure both nodes are valid
            if ( elem > NullIndex && elem < this.nextFreeIndex)
            {
                if (parent == NullIndex)
                {
                    return true;
                }
                
                // If the parent is not null, make sure it is a valid parent
                if (parent > NullIndex && parent < this.nextFreeIndex)
                {
                    // The parent must be an element node
                    if (this.nodes[parent].type != XPathNodeType.Element)
                    {
                        return false;
                    }

                    // If the parent is not null, the element must be a namespace node
                    if (this.nodes[elem].type != XPathNodeType.Namespace)
                    {
                        return false;
                    }

                    // Check that 'parent' is a descendant-or-self of elem->parent
                    int n = this.nodes[elem].parent;

                    while (parent != n)
                    {
                        parent = this.nodes[parent].parent;

                        if (parent == NullIndex)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }
#endif
        // Find the last child of a node
        // Must be called on the DOM object
        int LastChild(int n)
        {
            // See if there are any children
            n = this.nodes[n].firstChild;
            if (n == NullIndex)
            {
                return NullIndex;
            }

            // Find the last sibling of the first child
            return LastSibling(n);
        }

        // Find the last sibling of a node
        // Must be called on the DOM object
        int LastSibling(int n)
        {
            // Walk down the list unitl we get to the end.
            while (this.nodes[n].nextSibling != NullIndex)
            {
                n = this.nodes[n].nextSibling;
            }
            return n;
        }

        // Load the children of the body element
        // Must be called on the DOM object
        void LoadBody()
        {
            if (!this.message.IsEmpty)
            {
                // Get the body reader
                XmlReader reader = this.message.GetReaderAtBodyContents();
                if (reader.ReadState == ReadState.Initial)
                {
                    reader.Read();
                }

                // Record where we start so we can atomize later
                int lower = this.nextFreeIndex;

                // Load the children of the body element
                ReadChildNodes(reader, this.bodyIndex, SoapNSIndex);

                // Record where we finished
                int upper = this.nextFreeIndex;

                // Atomize if necessary
                if (this.atomize)
                {
                    Atomize(lower, upper);
                }
            }
        }

        // Load the given header node
        // Must be called on the DOM object
        void LoadHeader(int self)
        {
            // Get the header reader
            XmlReader reader = this.headers.GetReaderAtHeader(self - FirstHeaderIndex);
            if (reader.ReadState == ReadState.Initial)
            {
                reader.Read();
            }

            // Record where we started so we can atomize later
            int lower = this.nextFreeIndex;

            // Finish populating the header element
            this.nodes[self].firstNamespace = SoapNSIndex;
            this.nodes[self].prefix = this.atomize ? this.nameTable.Add(reader.Prefix) : reader.Prefix;
            this.nodes[self].baseUri = reader.BaseURI;
            this.nodes[self].xmlLang = reader.XmlLang;

            // We are ignoring any siblings the reader may try to surface.
            // NullIndex will be returned when then next element is the closing tag
            if (!reader.IsEmptyElement)
            {
                ReadAttributes(self, reader);
                reader.Read();
                ReadChildNodes(reader, self, this.nodes[self].firstNamespace);
            }
            else
            {
                ReadAttributes(self, reader);
            }

            // Record where we finished
            int upper = this.nextFreeIndex;

            // Atomize if necessary
            if (this.atomize)
            {
                Atomize(lower, upper);
            }
        }

        // Make sure the current node is fully loaded into the DOM
        // This function does not need to be called from the DOM object
        void LoadOnDemand()
        {
            this.dom.LoadOnDemand(this.location);
        }

        // Make sure the given node is fully loaded into the DOM
        // Must be called on the DOM object
        // THREAD: Synchronize this function to prevent multiple concurrent loads of the same element.
        //         They will cause errors in the DOM structure.
        void LoadOnDemand(int elem)
        {
            // Bail if we're not at a loadable item
            if (elem > this.bodyIndex || elem < FirstHeaderIndex)
            {
                return;
            }

            // Check for the 'unloaded' marker
            if (this.nodes[elem].firstChild == -1)
            {
                // Clear the 'unloaded' marker
                this.nodes[elem].firstChild = 0;

                // Load as appropriate
                if (elem == this.bodyIndex)
                {
                    if (this.includeBody)
                    {
                        LoadBody();
                    }
                    else
                    {
                        // Throw an exception if we try to navigate into the body.
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NavigatorInvalidBodyAccessException(SR.GetString(SR.SeekableMessageNavBodyForbidden)));
                    }
                }
                else
                {
                    LoadHeader(elem);
                }
            }
            else if (elem == this.bodyIndex && !this.includeBody)
            {
                // Throw an exception if we try to navigate into the body.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NavigatorInvalidBodyAccessException(SR.GetString(SR.SeekableMessageNavBodyForbidden)));
            }
        }

        // Get the index of a new unused node
        // Perform an necessary resizing
        // Must be called on the DOM object
        // THREAD: Synchronize this function.  If LoadOnDemand is synchronized, it may be possible to avoid synchronizing this one.
        int NewNode()
        {
            // Resize if necessary
            if (this.nextFreeIndex == this.nodes.Length)
            {
                // Compute the new size for the node array
                int size;
                if (this.nodes.Length <= StretchMax)
                {
                    size = this.nodes.Length * GrowFactor;
                }
                else
                {
                    size = this.nodes.Length + GrowInc;
                }

                // Resize the node array                
                Node[] tmp = new Node[size];
                this.nodes.CopyTo(tmp, 0);
                this.nodes = tmp;
            }

            // Return the index of the next free node
            return this.nextFreeIndex++;
        }

        // Read in the attributes/namespaces and attach them to their element
        // Must be called on the DOM object
        void ReadAttributes(int elem, XmlReader reader)
        {
            int a, n;
            while (reader.MoveToNextAttribute())
            {
                if (QueryDataModel.IsAttribute(reader.NamespaceURI))
                {
                    a = NewNode();
                    this.nodes[a].type = XPathNodeType.Attribute;
                    this.nodes[a].prefix = reader.Prefix;
                    this.nodes[a].name = reader.LocalName;
                    this.nodes[a].ns = reader.NamespaceURI;
                    this.nodes[a].val = reader.Value;
                    this.nodes[a].baseUri = reader.BaseURI;
                    this.nodes[a].xmlLang = reader.XmlLang;
                    AddAttribute(elem, a);
                }
                else
                {
                    string name = reader.Prefix.Length == 0 ? string.Empty : reader.LocalName;

                    // It is illegal to override the 'xml' and 'xmlns' prefixes
                    if (String.CompareOrdinal(name, XmlP) == 0 || String.CompareOrdinal(name, XmlnsP) == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.InvalidNamespacePrefix, SR.GetString(SR.SeekableMessageNavOverrideForbidden, reader.Name)));
                    }

                    n = NewNode();
                    this.nodes[n].type = XPathNodeType.Namespace;
                    this.nodes[n].name = name;
                    this.nodes[n].val = reader.Value;
                    this.nodes[n].baseUri = reader.BaseURI;
                    this.nodes[n].xmlLang = reader.XmlLang;
                    AddNamespace(elem, n);
                }
            }
        }

        // Load elements from a reader
        // Must be called on the DOM object
        int ReadChildNodes(XmlReader reader, int parent, int parentNS)
        {
            Fx.Assert(reader != null, "Reader cannot be null");

            // Loop over all nodes being surfaced
            int n = NullIndex;
            do
            {
                // PERF, [....], reorder cases so more common ones are earlier
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        n = NewNode();
                        this.nodes[n].type = XPathNodeType.Element;
                        this.nodes[n].prefix = reader.Prefix;
                        this.nodes[n].name = reader.LocalName;
                        this.nodes[n].ns = reader.NamespaceURI;
                        this.nodes[n].firstNamespace = parentNS;
                        this.nodes[n].baseUri = reader.BaseURI;
                        this.nodes[n].xmlLang = reader.XmlLang;

                        // Empty elements don't surface closing tags so they need to be handled differently
                        if (!reader.IsEmptyElement)
                        {
                            ReadAttributes(n, reader);
                            reader.Read();
                            ReadChildNodes(reader, n, this.nodes[n].firstNamespace);
                        }
                        else
                        {
                            ReadAttributes(n, reader);
                            this.nodes[n].empty = true;
                        }

                        AddChild(parent, n);
                        break;

                    case XmlNodeType.Comment:
                        n = NewNode();
                        this.nodes[n].type = XPathNodeType.Comment;
                        this.nodes[n].val = reader.Value;
                        this.nodes[n].baseUri = reader.BaseURI;
                        this.nodes[n].xmlLang = reader.XmlLang;
                        AddChild(parent, n);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        n = NewNode();
                        this.nodes[n].type = XPathNodeType.ProcessingInstruction;
                        this.nodes[n].name = reader.LocalName;
                        this.nodes[n].val = reader.Value;
                        this.nodes[n].baseUri = reader.BaseURI;
                        this.nodes[n].xmlLang = reader.XmlLang;
                        AddChild(parent, n);
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        if (reader.XmlSpace == XmlSpace.Preserve)
                        {
                            // If we're preserving whitespace, try to append it to a text node instead
                            // of creating a new node
                            n = LastChild(parent);
                            if (n != NullIndex && (this.nodes[n].type == XPathNodeType.Text ||
                                this.nodes[n].type == XPathNodeType.Whitespace ||
                                this.nodes[n].type == XPathNodeType.SignificantWhitespace))
                            {
                                this.nodes[n].val = this.nodes[n].val + reader.Value;
                            }
                            else
                            {
                                n = NewNode();
                                this.nodes[n].type = XPathNodeType.SignificantWhitespace;
                                this.nodes[n].val = reader.Value;
                                this.nodes[n].baseUri = reader.BaseURI;
                                this.nodes[n].xmlLang = reader.XmlLang;
                                AddChild(parent, n);
                            }
                        }
                        else
                        {
                            goto case XmlNodeType.Whitespace;
                        }
                        break;

                    case XmlNodeType.Whitespace:
                        if (this.space == XmlSpace.Preserve)
                        {
                            // If we're preserving whitespace, try to append it to a text node instead
                            // of creating a new node
                            n = LastChild(parent);
                            if (n != NullIndex && (this.nodes[n].type == XPathNodeType.Text ||
                                this.nodes[n].type == XPathNodeType.Whitespace ||
                                this.nodes[n].type == XPathNodeType.SignificantWhitespace))
                            {
                                this.nodes[n].val = this.nodes[n].val + reader.Value;
                            }
                            else
                            {
                                n = NewNode();
                                this.nodes[n].type = XPathNodeType.Whitespace;
                                this.nodes[n].val = reader.Value;
                                this.nodes[n].baseUri = reader.BaseURI;
                                this.nodes[n].xmlLang = reader.XmlLang;
                                AddChild(parent, n);
                            }
                        }
                        break;

                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                        // Try to append it to a text node instead of creating a new node
                        n = LastChild(parent);
                        if (n == NullIndex || (this.nodes[n].type != XPathNodeType.Text &&
                            this.nodes[n].type != XPathNodeType.Whitespace &&
                            this.nodes[n].type != XPathNodeType.SignificantWhitespace))
                        {
                            n = NewNode();




                            this.nodes[n].baseUri = reader.BaseURI;
                            this.nodes[n].xmlLang = reader.XmlLang;
                            AddChild(parent, n);
                        }
                        this.nodes[n].type = XPathNodeType.Text;
                        this.nodes[n].val = reader.Value;
                        break;

                    case XmlNodeType.EntityReference:
                        reader.ResolveEntity();
                        reader.Read();
                        ReadChildNodes(reader, parent, parentNS);




                        break;

                    case XmlNodeType.EndEntity:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.None:
                        return n;

                    case XmlNodeType.DocumentType:
                        break;

                    case XmlNodeType.XmlDeclaration:
                    default:
                        break;
                }
            } while (reader.Read());

            return n;
        }

        int INodeCounter.CounterMarker
        {
            get
            {
                return this.counter.nodeCount;
            }
            set
            {
                this.counter.nodeCount = value;
            }
        }

        int INodeCounter.MaxCounter
        {
            set
            {
                this.counter.nodeCountMax = value;
            }
        }

        int INodeCounter.ElapsedCount(int marker)
        {
            return marker - this.counter.nodeCount;
        }

        void Increase()
        {
            Fx.Assert(this.counter != null, "Counter reference is null");
            if (this.counter.nodeCount > 0)
            {
                --this.counter.nodeCount;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathNavigatorException(SR.GetString(SR.FilterNodeQuotaExceeded, this.counter.nodeCountMax)));
            }
        }

        // PERF, [....], find a better way to implement and have internal
        void INodeCounter.Increase()
        {
            Increase();
        }

        void INodeCounter.IncreaseBy(int count)
        {
            this.counter.nodeCount -= (count - 1);
            Increase();
        }

        struct Node
        {
            // Connectivity
            internal int parent;
            internal int firstAttribute;
            internal int firstChild;
            internal int firstNamespace;
            internal int nextSibling;
            internal int prevSibling;

            // Data
            internal string baseUri;
            internal bool empty;
            internal string name;
            internal string ns;
            internal string prefix;
            internal string val;
            internal string xmlLang;

            // Type
            internal XPathNodeType type;
        }

        struct Position
        {
            internal int elem;
            internal int parent;

            internal Position(int e, int p)
            {
                this.elem = e;
                this.parent = p;
            }
        }
    }
}
