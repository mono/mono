//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml.XPath;

    /// <summary>
    /// This structure contains the criteria used to select a set of nodes from a context node
    ///
    /// Selectors select nodes with particular a relationship to a context node. Candidate nodes are first identified by 
    /// traversing away from the context node along an axis of traversal. The attribute axis, for example, identifies all 
    /// attributes of a given context node as candidates for selection. 
    ///
    /// The candidate nodeset identified by axis traversal is then refined by applying node tests. 
    /// A nodeType test constructs a new nodeset by selecting only those nodes of a given type from source candidate set
    /// A qname test further refines this nodeset by selecting only those that match a given qname
    /// </summary>
    class NodeSelectCriteria
    {
        protected QueryAxis axis;
        protected NodeQName qname;
        protected NodeQNameType qnameType;
        protected QueryNodeType type;

        internal NodeSelectCriteria(QueryAxisType axis, NodeQName qname, QueryNodeType nodeType)
        {
            this.axis = QueryDataModel.GetAxis(axis);
            this.qname = qname;
            this.qnameType = qname.GetQNameType();
            this.type = nodeType;
        }

        internal QueryAxis Axis
        {
            get
            {
                return this.axis;
            }
        }

        internal bool IsCompressable
        {
            get
            {
                // PERF, [....], weaken guard?
                return QueryAxisType.Self == this.axis.Type || QueryAxisType.Child == this.axis.Type;
                //return ((QueryAxisType.Self == this.axis.Type) || ((this.axis.Type != QueryAxisType.DescendantOrSelf || this.axis.Type != QueryAxisType.Descendant)&& 0 != ((QueryNodeType.Element | QueryNodeType.Root) & this.type)));
            }
        }

        internal NodeQName QName
        {
            get
            {
                return this.qname;
            }
        }

        internal QueryNodeType Type
        {
            get
            {
                return this.type;
            }
        }

#if NO                        
        internal static NodeSelectCriteria Create(QueryAxisType axis, NodeQName qname, QueryNodeType nodeType)
        {
            return new NodeSelectCriteria(axis, qname, nodeType);
        }
#endif
        public bool Equals(NodeSelectCriteria criteria)
        {
            return (this.axis.Type == criteria.axis.Type && this.type == criteria.type && this.qname.Equals(criteria.qname));
        }
#if NO
        internal bool Match(SeekableXPathNavigator node)
        {
            return (this.MatchType(node) && this.MatchQName(node));
        }
#endif
        internal bool MatchType(SeekableXPathNavigator node)
        {
            QueryNodeType nodeType;
            switch (node.NodeType)
            {
                default:
                    return false;

                case XPathNodeType.Root:
                    nodeType = QueryNodeType.Root;
                    break;

                case XPathNodeType.Attribute:
                    nodeType = QueryNodeType.Attribute;
                    break;

                case XPathNodeType.Element:
                    nodeType = QueryNodeType.Element;
                    break;

                case XPathNodeType.Comment:
                    nodeType = QueryNodeType.Comment;
                    break;

                case XPathNodeType.Text:
                case XPathNodeType.Whitespace:
                case XPathNodeType.SignificantWhitespace:
                    nodeType = QueryNodeType.Text;
                    break;

                case XPathNodeType.ProcessingInstruction:
                    nodeType = QueryNodeType.Processing;
                    break;
            }

            return (nodeType == (this.type & nodeType));
        }

        internal bool MatchQName(SeekableXPathNavigator node)
        {
            // Is this a standard qname test.. with known names and namespaces
            switch (this.qnameType & NodeQNameType.Standard)
            {
                default:
                    break;

                case NodeQNameType.Name:
                    // Selection criteria did not specify a namespace. Then, if the node supplies a namespace, we know
                    // that the criteria cannot possibly match
                    return (0 == node.NamespaceURI.Length && this.qname.EqualsName(node.LocalName));

                case NodeQNameType.Standard:
                    string str = node.LocalName;
                    if (this.qname.name.Length == str.Length && this.qname.name == str)
                    {
                        str = node.NamespaceURI;
                        return (this.qname.ns.Length == str.Length && this.qname.ns == str);
                    }
                    return false;
            }

            if (NodeQNameType.Empty == this.qnameType)
            {
                return true;
            }

            // Maybe a wildcard
            switch (this.qnameType & NodeQNameType.Wildcard)
            {
                default:
                    break;

                case NodeQNameType.NameWildcard:
                    return this.qname.EqualsNamespace(node.NamespaceURI);

                case NodeQNameType.Wildcard:
                    return true;
            }

            return false;
        }

        internal void Select(SeekableXPathNavigator contextNode, NodeSequence destSequence)
        {
            switch (this.type)
            {
                default:
                    if (QueryAxisType.Self == this.axis.Type)
                    {
                        if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                        {
                            destSequence.Add(contextNode);
                        }
                    }
                    else if (QueryAxisType.Descendant == this.axis.Type)
                    {
                        SelectDescendants(contextNode, destSequence);
                    }
                    else if (QueryAxisType.DescendantOrSelf == this.axis.Type)
                    {
                        destSequence.Add(contextNode);
                        SelectDescendants(contextNode, destSequence);
                    }
                    else if (QueryAxisType.Child == this.axis.Type)
                    {
                        // Select children of arbitrary type off the context node
                        if (contextNode.MoveToFirstChild())
                        {
                            do
                            {
                                // Select the node if its type and qname matches
                                if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                                {
                                    destSequence.Add(contextNode);
                                }
                            }
                            while (contextNode.MoveToNext());
                        }

                    }
                    else if (QueryAxisType.Attribute == this.axis.Type)
                    {
                        if (contextNode.MoveToFirstAttribute())
                        {
                            do
                            {
                                // Select the node if its type and qname matches
                                if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                                {
                                    destSequence.Add(contextNode);
                                    // you can't have multiple instances of an attibute with the same qname
                                    // Stop once one was found
                                    // UNLESS WE HAVE A WILDCARD OFCOURSE!
                                    if (0 == (this.qnameType & NodeQNameType.Wildcard))
                                    {
                                        break;
                                    }
                                }
                            }
                            while (contextNode.MoveToNextAttribute());
                        }

                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
                    }
                    break;

                case QueryNodeType.Attribute:
                    // Select attributes off the context Node
                    if (contextNode.MoveToFirstAttribute())
                    {
                        do
                        {
                            if (this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                                // you can't have multiple instances of an attibute with the same qname
                                // Stop once one was found
                                // UNLESS WE HAVE A WILDCARD OFCOURSE!
                                if (0 == (this.qnameType & NodeQNameType.Wildcard))
                                {
                                    break;
                                }
                            }
                        }
                        while (contextNode.MoveToNextAttribute());
                    }
                    break;

                case QueryNodeType.ChildNodes:
                    if (QueryAxisType.Descendant == this.axis.Type)
                    {
                        // Select descendants of arbitrary type off the context node
                        SelectDescendants(contextNode, destSequence);
                    }
                    else
                    {
                        // Select children of arbitrary type off the context node
                        if (contextNode.MoveToFirstChild())
                        {
                            do
                            {
                                // Select the node if its type and qname matches
                                if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                                {
                                    destSequence.Add(contextNode);
                                }
                            }
                            while (contextNode.MoveToNext());
                        }
                    }
                    break;

                case QueryNodeType.Element:
                    if (QueryAxisType.Descendant == this.axis.Type)
                    {
                        // Select descendants of arbitrary type off the context node
                        SelectDescendants(contextNode, destSequence);
                    }
                    else if (QueryAxisType.DescendantOrSelf == this.axis.Type)
                    {
                        destSequence.Add(contextNode);
                        SelectDescendants(contextNode, destSequence);
                    }
                    else if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            // Children could have non element nodes in line
                            // Select the node if it is an element and the qname matches
                            if (XPathNodeType.Element == contextNode.NodeType && this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    break;

                case QueryNodeType.Root:
                    contextNode.MoveToRoot();
                    destSequence.Add(contextNode);
                    break;

                case QueryNodeType.Text:
                    // Select child text nodes
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            // Select the node if its type matches
                            // Can't just do a comparison to XPathNodeType.Text since whitespace nodes
                            // count as text
                            if (this.MatchType(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    break;
            }
        }

        internal Opcode Select(SeekableXPathNavigator contextNode, NodeSequence destSequence, SelectOpcode next)
        {
            Opcode returnOpcode = next.Next;

            switch (this.type)
            {
                default:
                    if (QueryAxisType.Self == this.axis.Type)
                    {
                        if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                        {
                            long position = contextNode.CurrentPosition;
                            returnOpcode = next.Eval(destSequence, contextNode);
                            contextNode.CurrentPosition = position;
                        }
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected));
                    }

                    break;

                case QueryNodeType.ChildNodes:
                    // Select children of arbitrary type off the context node
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            // Select the node if its type and qname matches
                            if (this.MatchType(contextNode) && this.MatchQName(contextNode))
                            {
                                destSequence.Add(contextNode);
                            }
                        }
                        while (contextNode.MoveToNext());
                    }
                    break;

                case QueryNodeType.Element:
                    // Select child elements
                    if (contextNode.MoveToFirstChild())
                    {
                        do
                        {
                            // Children could have non element nodes in line
                            // Select the node if it is an element and the qname matches
                            if (XPathNodeType.Element == contextNode.NodeType && this.MatchQName(contextNode))
                            {
                                long position = contextNode.CurrentPosition;
                                returnOpcode = next.Eval(destSequence, contextNode);
                                contextNode.CurrentPosition = position;
                            }
                        } while (contextNode.MoveToNext());
                    }

                    break;

                case QueryNodeType.Root:
                    contextNode.MoveToRoot();
                    returnOpcode = next.Eval(destSequence, contextNode);
                    break;
            }

            return returnOpcode;
        }

        void SelectDescendants(SeekableXPathNavigator contextNode, NodeSequence destSequence)
        {
            int level = 1;
            if (!contextNode.MoveToFirstChild())
            {
                return;
            }
            while (level > 0)
            {
                // Don't need type check.  All child nodes allowed.
                if (this.MatchQName(contextNode))
                {
                    destSequence.Add(contextNode);
                }

                if (contextNode.MoveToFirstChild())
                {
                    ++level;
                }
                else if (contextNode.MoveToNext())
                {

                }
                else
                {
                    while (level > 0)
                    {
                        contextNode.MoveToParent();
                        --level;

                        if (contextNode.MoveToNext())
                        {
                            break;
                        }
                    }
                }
            }
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0}, {1}:{2}", this.type, this.qname.ns, this.qname.name);
        }
#endif

    }

    // General purpose selector
    // Pops all parameters from the value stack
    internal class SelectOpcode : Opcode
    {
        protected NodeSelectCriteria criteria;

        internal SelectOpcode(NodeSelectCriteria criteria)
            : this(OpcodeID.Select, criteria)
        {
        }

        internal SelectOpcode(OpcodeID id, NodeSelectCriteria criteria)
            : this(id, criteria, OpcodeFlags.None)
        {
        }

        internal SelectOpcode(OpcodeID id, NodeSelectCriteria criteria, OpcodeFlags flags)
            : base(id)
        {
            this.criteria = criteria;
            this.flags |= (flags | OpcodeFlags.Select);
            if (criteria.IsCompressable && (0 == (this.flags & OpcodeFlags.InitialSelect)))
            {
                this.flags |= OpcodeFlags.CompressableSelect;
            }
        }

        internal NodeSelectCriteria Criteria
        {
            get
            {
                return this.criteria;
            }
        }

        internal override bool Equals(Opcode op)
        {
            if (base.Equals(op))
            {
                return this.criteria.Equals(((SelectOpcode)op).criteria);
            }

            return false;
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topFrame = context.TopSequenceArg;
            SeekableXPathNavigator node = null;
            Value[] sequences = context.Sequences;

            for (int i = topFrame.basePtr; i <= topFrame.endPtr; ++i)
            {
                // Each NodeSequence will generate a new one, but only if the source FilterSequence isn't empty
                // If the source FilterSequence is empty, release it and replace it with an empty sequence
                NodeSequence sourceSeq = sequences[i].Sequence;
                int sourceSeqCount = sourceSeq.Count;
                if (sourceSeqCount == 0)
                {
                    context.ReplaceSequenceAt(i, NodeSequence.Empty);
                    context.ReleaseSequence(sourceSeq);
                }
                else
                {
                    NodeSequenceItem[] items = sourceSeq.Items;
                    if (sourceSeq.CanReuse(context))
                    {
                        node = items[0].GetNavigator();
                        sourceSeq.Clear();
                        sourceSeq.StartNodeset();

                        this.criteria.Select(node, sourceSeq);

                        sourceSeq.StopNodeset();
                    }
                    else
                    {
                        NodeSequence newSeq = null;
                        for (int item = 0; item < sourceSeqCount; ++item)
                        {
                            node = items[item].GetNavigator();
                            Fx.Assert(null != node, "");
                            if (null == newSeq)
                            {
                                newSeq = context.CreateSequence();
                            }
                            newSeq.StartNodeset();
                            this.criteria.Select(node, newSeq);
                            newSeq.StopNodeset();
                        }
                        context.ReplaceSequenceAt(i, (null != newSeq) ? newSeq : NodeSequence.Empty);
                        context.ReleaseSequence(sourceSeq);
                    }
                }
            }

            return this.next;
        }

        internal override Opcode Eval(NodeSequence sequence, SeekableXPathNavigator node)
        {
            if (this.next == null || 0 == (this.next.Flags & OpcodeFlags.CompressableSelect))
            {
                // The next opcode is not a compressible select. Complete the select operation and return the next opcode
                sequence.StartNodeset();
                this.criteria.Select(node, sequence);
                sequence.StopNodeset();
                return this.next;
            }

            return this.criteria.Select(node, sequence, (SelectOpcode)this.next);
        }

#if DEBUG_FILTER
        public override string ToString()
        {
            return string.Format("{0} {1}", base.ToString(), this.criteria.ToString());
        }
#endif
    }

    internal class InitialSelectOpcode : SelectOpcode
    {
        internal InitialSelectOpcode(NodeSelectCriteria criteria)
            : base(OpcodeID.InitialSelect, criteria, OpcodeFlags.InitialSelect)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            StackFrame topFrame = context.TopSequenceArg;
            Value[] sequences = context.Sequences;

            bool wasInUse = context.SequenceStackInUse;
            context.PushSequenceFrame();
            for (int i = topFrame.basePtr; i <= topFrame.endPtr; ++i)
            {
                NodeSequence sourceSeq = sequences[i].Sequence;
                int count = sourceSeq.Count;
                if (count == 0)
                {
                    // Empty sequence. 
                    // Since there are no nodes in the sequence, we will track this sequence also 
                    // using an empty sequence 
                    if (!wasInUse)
                        context.PushSequence(NodeSequence.Empty);
                }
                else
                {
                    NodeSequenceItem[] items = sourceSeq.Items;
                    for (int item = 0; item < sourceSeq.Count; ++item)
                    {
                        SeekableXPathNavigator node = items[item].GetNavigator();
                        Fx.Assert(null != node, "");

                        NodeSequence newSeq = context.CreateSequence();
                        newSeq.StartNodeset();

                        this.criteria.Select(node, newSeq);

                        newSeq.StopNodeset();

                        context.PushSequence(newSeq);
                    }
                }
            }
            return this.next;
        }
    }

    internal class SelectRootOpcode : Opcode
    {
        internal SelectRootOpcode()
            : base(OpcodeID.SelectRoot)
        {
        }

        internal override Opcode Eval(ProcessingContext context)
        {
            // The query processor object also serves as the query document root
            int iterationCount = context.IterationCount;
            Opcode returnOpcode = this.next;

            // A root is always an initial step
            context.PushSequenceFrame();
            NodeSequence seq = context.CreateSequence();
            if (this.next != null && 0 != (this.next.Flags & OpcodeFlags.CompressableSelect))
            {
                SeekableXPathNavigator node = context.Processor.ContextNode;
                node.MoveToRoot();
                returnOpcode = this.next.Eval(seq, node);
                while (returnOpcode != null && 0 != (returnOpcode.Flags & OpcodeFlags.CompressableSelect))
                {
                    returnOpcode = returnOpcode.Next;
                }
            }
            else
            {
                // Roots do not have any qnames..
                seq.StartNodeset();
                SeekableXPathNavigator node = context.Processor.ContextNode;
                node.MoveToRoot();
                seq.Add(node);
                seq.StopNodeset();
            }

            if (seq.Count == 0)
            {
                context.ReleaseSequence(seq);
                seq = NodeSequence.Empty;
            }

            for (int i = 0; i < iterationCount; ++i)
            {
                context.PushSequence(seq);
            }
            if (iterationCount > 1)
                seq.refCount += iterationCount - 1;

            return returnOpcode;
        }
    }
}
