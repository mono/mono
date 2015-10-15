//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// A navigator is a cursor over the nodes in a DOM, where each node is assigned a unique position. 
    /// A node is a (navigator, position) pair. 
    /// </summary>    
    internal struct QueryNode
    {
        SeekableXPathNavigator node;
        long nodePosition;

        /// <summary>
        /// Create a query node from the given navigator and its current position
        /// </summary>
        /// <param name="node"></param>
        internal QueryNode(SeekableXPathNavigator node)
        {
            this.node = node;
            this.nodePosition = node.CurrentPosition;
        }

        /// <summary>
        /// Initialize using the given (node, position) pair
        /// </summary>
#if NO
        internal QueryNode(SeekableXPathNavigator node, long nodePosition)
        {
            this.node = node;
            this.nodePosition = nodePosition;
        }
#endif
        internal string LocalName
        {
            get
            {
                return this.node.GetLocalName(this.nodePosition);
            }
        }

        /// <summary>
        /// Return the node's name
        /// </summary>
        internal string Name
        {
            get
            {
                return this.node.GetName(this.nodePosition);
            }
        }
        /// <summary>
        /// Return the node's namespace
        /// </summary>
        internal string Namespace
        {
            get
            {
                return this.node.GetNamespace(this.nodePosition);
            }
        }
        /// <summary>
        /// Return this query node's underlying Node
        /// </summary>
        internal SeekableXPathNavigator Node
        {
            get
            {
                return this.node;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal long Position
        {
            get
            {

                return this.nodePosition;
            }
        }
#if NO
        /// <summary>
        /// This node's type
        /// </summary>
        internal QueryNodeType Type
        {
            get
            {
                return QueryDataModel.GetNodeType(this.node.GetNodeType(this.nodePosition));
            }
        }
#endif
        /// <summary>
        /// This node's string value
        /// </summary>
        internal string Value
        {
            get
            {
                return this.node.GetValue(this.nodePosition);
            }
        }
#if NO
        /// <summary>
        /// Raw xpath node type
        /// </summary>
        internal XPathNodeType XPathNodeType
        {
            get
            {
                return this.node.GetNodeType(this.nodePosition);
            }
        }        
#endif
        /// <summary>
        /// Move this node's navigator to its position
        /// </summary>
        /// <returns></returns>
        internal SeekableXPathNavigator MoveTo()
        {
            this.node.CurrentPosition = this.nodePosition;
            return this.node;
        }
    }

    internal enum NodeSequenceItemFlags : byte
    {
        None = 0x00,
        NodesetLast = 0x01,
    }

    // PERF, Microsoft, Remove when generic sort works
    // Used to sort in document order
#if NO
    internal class NodeSequenceItemObjectComparer : IComparer
    {
        internal NodeSequenceItemObjectComparer()
        {
        }

        public int Compare(object obj1, object obj2)
        {
            NodeSequenceItem item1 = (NodeSequenceItem)obj1;
            NodeSequenceItem item2 = (NodeSequenceItem)obj2;
            
            XmlNodeOrder order = item1.Node.Node.ComparePosition(item1.Node.Position, item2.Node.Position);
            int ret;
            switch(order)
            {
                case XmlNodeOrder.Before:
                    ret = -1;
                    break;
                    
                case XmlNodeOrder.Same:
                    ret = 0;
                    break;
                    
                case XmlNodeOrder.After:
                    ret = 1;
                    break;
                    
                case XmlNodeOrder.Unknown:
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathException(SR.GetString(SR.QueryNotSortable)), TraceEventType.Critical);
            }

            return ret;
        }
    }

    // Used to sort in document order
    internal class NodeSequenceItemComparer : IComparer<NodeSequenceItem>
    {
        internal NodeSequenceItemComparer()
        {
        }
         
        public int Compare(NodeSequenceItem item1, NodeSequenceItem item2)
        {
            XmlNodeOrder order = item1.Node.Node.ComparePosition(item1.Node.Position, item2.Node.Position);
            int ret;
            switch(order)
            {
                case XmlNodeOrder.Before:
                    ret = -1;
                    break;
                    
                case XmlNodeOrder.Same:
                    ret = 0;
                    break;
                    
                case XmlNodeOrder.After:
                    ret = 1;
                    break;
                    
                case XmlNodeOrder.Unknown:
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathException(SR.GetString(SR.QueryNotSortable)), TraceEventType.Critical);
            }

            return ret;
        }
        
        public bool Equals(NodeSequenceItem item1, NodeSequenceItem item2)
        {
            return Compare(item1, item2) == 0;
        }

        public int GetHashCode(NodeSequenceItem item)
        {
            return item.GetHashCode();
        }
    }
#endif
    // Used to sort in document order
    internal class QueryNodeComparer : IComparer<QueryNode>
    {
        public QueryNodeComparer()
        {
        }

        public int Compare(QueryNode item1, QueryNode item2)
        {
            XmlNodeOrder order = item1.Node.ComparePosition(item1.Position, item2.Position);
            int ret;
            switch (order)
            {
                case XmlNodeOrder.Before:
                    ret = -1;
                    break;

                case XmlNodeOrder.Same:
                    ret = 0;
                    break;

                case XmlNodeOrder.After:
                    ret = 1;
                    break;

                case XmlNodeOrder.Unknown:
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new XPathException(SR.GetString(SR.QueryNotSortable)));
            }

            return ret;
        }

        public bool Equals(QueryNode item1, QueryNode item2)
        {
            return Compare(item1, item2) == 0;
        }

        public int GetHashCode(QueryNode item)
        {
            return item.GetHashCode();
        }
    }

    internal struct NodeSequenceItem
    {
        NodeSequenceItemFlags flags;
        QueryNode node;
        int position;
        int size;

        internal NodeSequenceItemFlags Flags
        {
            get
            {
                return this.flags;
            }
            set
            {
                this.flags = value;
            }
        }

        internal bool Last
        {
            get
            {
                return (0 != (NodeSequenceItemFlags.NodesetLast & this.flags));
            }
            set
            {
                if (value)
                {
                    this.flags |= NodeSequenceItemFlags.NodesetLast;
                }
                else
                {
                    this.flags &= ~(NodeSequenceItemFlags.NodesetLast);
                }
            }
        }

        internal string LocalName
        {
            get
            {
                return this.node.LocalName;
            }
        }

        internal string Name
        {
            get
            {
                return this.node.Name;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.node.Namespace;
            }
        }

        internal QueryNode Node
        {
            get
            {
                return this.node;
            }
#if NO
            set
            {
                this.node = value;
            }
#endif
        }

        internal int Position
        {
            get
            {
                return this.position;
            }
#if NO
            set
            {
                this.position = value;
            }
#endif
        }

        internal int Size
        {
            get
            {
                return this.size;
            }
            set
            {
                this.size = value;
            }
        }

        internal bool Compare(double dblVal, RelationOperator op)
        {
            return QueryValueModel.Compare(this.NumberValue(), dblVal, op);
        }

        internal bool Compare(string strVal, RelationOperator op)
        {
            return QueryValueModel.Compare(this.StringValue(), strVal, op);
        }

        internal bool Compare(ref NodeSequenceItem item, RelationOperator op)
        {
            return QueryValueModel.Compare(this.StringValue(), item.StringValue(), op);
        }

        internal bool Equals(string literal)
        {
            return QueryValueModel.Equals(this.StringValue(), literal);
        }

        internal bool Equals(double literal)
        {
            return (this.NumberValue() == literal);
        }

        internal SeekableXPathNavigator GetNavigator()
        {
            return this.node.MoveTo();
        }

        internal long GetNavigatorPosition()
        {
            return this.node.Position;
        }

        internal double NumberValue()
        {
            return QueryValueModel.Double(this.StringValue());
        }

        internal void Set(SeekableXPathNavigator node, int position, int size)
        {
            Fx.Assert(position > 0, "");
            Fx.Assert(null != node, "");

            this.node = new QueryNode(node);
            this.position = position;
            this.size = size;
            this.flags = NodeSequenceItemFlags.None;
        }

        internal void Set(QueryNode node, int position, int size)
        {
            Fx.Assert(position > 0, "");

            this.node = node;
            this.position = position;
            this.size = size;
            this.flags = NodeSequenceItemFlags.None;
        }

        internal void Set(ref NodeSequenceItem item, int position, int size)
        {
            Fx.Assert(position > 0, "");

            this.node = item.node;
            this.position = position;
            this.size = size;
            this.flags = item.flags;
        }

        internal void SetPositionAndSize(int position, int size)
        {
            this.position = position;
            this.size = size;
            this.flags &= ~NodeSequenceItemFlags.NodesetLast;
        }

        internal void SetSizeAndLast()
        {
            this.size = 1;
            this.flags |= NodeSequenceItemFlags.NodesetLast;
        }

        // This is not optimized right now
        // We may want to CACHE string values once they are computed
        internal string StringValue()
        {
            return this.node.Value;
        }
    }

    internal class NodeSequence
    {
#if DEBUG
        // debugging aid. Because C# references do not have displayble numeric values, hard to deduce the
        // graph structure to see what opcode is connected to what
        static long nextUniqueId = 0;
        internal long uniqueID;
#endif
        int count;
        internal static NodeSequence Empty = new NodeSequence(0);
        NodeSequenceItem[] items;
        NodeSequence next;
        ProcessingContext ownerContext;
        int position;
        internal int refCount;
        int sizePosition;

        static readonly QueryNodeComparer staticQueryNodeComparerInstance = new QueryNodeComparer();

        internal NodeSequence()
            : this(8, null)
        {
        }

        internal NodeSequence(int capacity)
            : this(capacity, null)
        {
        }

        internal NodeSequence(int capacity, ProcessingContext ownerContext)
        {
            this.items = new NodeSequenceItem[capacity];
            this.ownerContext = ownerContext;
#if DEBUG
            this.uniqueID = Interlocked.Increment(ref NodeSequence.nextUniqueId);
#endif
        }

#if NO
        internal NodeSequence(int capacity, ProcessingContext ownerContext, XPathNodeIterator iter)
            : this(capacity, ownerContext)
        {
            while(iter.MoveNext())
            {
                SeekableXPathNavigator nav = iter.Current as SeekableXPathNavigator;
                if(nav != null)
                {
                    Add(nav);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryMustBeSeekable)), TraceEventType.Critical);
                }
            }
        }
#endif
        internal int Count
        {
            get
            {
                return this.count;
            }
#if NO
            set
            {
                Fx.Assert(value >= 0 && value <= this.count, "");
                this.count = value;
            }
#endif
        }

        internal NodeSequenceItem this[int index]
        {
            get
            {
                return this.items[index];
            }
        }

        internal NodeSequenceItem[] Items
        {
            get
            {
                return this.items;
            }
        }

        internal bool IsNotEmpty
        {
            get
            {
                return (this.count > 0);
            }
        }

        internal string LocalName
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].LocalName;
                }

                return string.Empty;
            }
        }

        internal string Name
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].Name;
                }

                return string.Empty;
            }
        }

        internal string Namespace
        {
            get
            {
                if (this.count > 0)
                {
                    return this.items[0].Namespace;
                }

                return string.Empty;
            }
        }

        internal NodeSequence Next
        {
            get
            {
                return this.next;
            }
            set
            {
                this.next = value;
            }
        }

        internal ProcessingContext OwnerContext
        {
            get
            {
                return this.ownerContext;
            }
            set
            {
                this.ownerContext = value;
            }
        }
#if NO
        internal int NodesetStartAt
        {
            get
            {
                return -this.sizePosition;
            }
        }
#endif
        internal void Add(XPathNodeIterator iter)
        {
            while (iter.MoveNext())
            {
                SeekableXPathNavigator nav = iter.Current as SeekableXPathNavigator;
                if (nav != null)
                {
                    this.Add(nav);
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCritical(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryMustBeSeekable)));
                }
            }
        }

        internal void Add(SeekableXPathNavigator node)
        {
            Fx.Assert(this.items.Length > 0, "");
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
            this.position++;
            this.items[this.count++].Set(node, this.position, this.sizePosition);
        }

        internal void Add(QueryNode node)
        {
            Fx.Assert(this.items.Length > 0, "");
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }

            this.position++;
            this.items[this.count++].Set(node, this.position, this.sizePosition);
        }

        internal void Add(ref NodeSequenceItem item)
        {
            Fx.Assert(this.items.Length > 0, "");
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }

            this.position++;
            this.items[this.count++].Set(ref item, this.position, this.sizePosition);
        }

        internal void AddCopy(ref NodeSequenceItem item, int size)
        {
            Fx.Assert(this.items.Length > 0, "");
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }

            this.items[this.count] = item;
            this.items[this.count++].Size = size;
        }

        internal void AddCopy(ref NodeSequenceItem item)
        {
            Fx.Assert(this.items.Length > 0, "");
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }

            this.items[this.count++] = item;
        }
#if NO 
        internal void Add(NodeSequence seq)
        {
            int newCount = this.count + seq.count;
            if (newCount > this.items.Length)
            {
                // We are going to need room. Grow the array
                int growTo = this.items.Length * 2;
                this.Grow(newCount > growTo ? newCount : growTo);
            }
            Array.Copy(seq.items, 0, this.items, this.count, seq.count);
            this.count += seq.count;
        }
#endif
        internal bool CanReuse(ProcessingContext context)
        {
            return (this.count == 1 && this.ownerContext == context && this.refCount == 1);
        }

        internal void Clear()
        {
            this.count = 0;
        }

        internal void Reset(NodeSequence nextSeq)
        {
            this.count = 0;
            this.refCount = 0;
            this.next = nextSeq;
        }

        internal bool Compare(double val, RelationOperator op)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this.items[i].Compare(val, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(string val, RelationOperator op)
        {
            Fx.Assert(null != val, "");
            for (int i = 0; i < this.count; ++i)
            {
                if (this.items[i].Compare(val, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(ref NodeSequenceItem item, RelationOperator op)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this.items[i].Compare(ref item, op))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Compare(NodeSequence sequence, RelationOperator op)
        {
            Fx.Assert(null != sequence, "");
            for (int i = 0; i < sequence.count; ++i)
            {
                if (this.Compare(ref sequence.items[i], op))
                {
                    return true;
                }
            }
            return false;
        }
#if NO
        void EnsureCapacity()
        {
            if (this.count == this.items.Length)
            {
                this.Grow(this.items.Length * 2);
            }
        }

        void EnsureCapacity(int capacity)
        {
            if (capacity > this.items.Length)
            {
                int newSize = this.items.Length * 2;
                this.Grow(newSize > capacity ? newSize : capacity);
            }
        }
#endif
        internal bool Equals(string val)
        {
            Fx.Assert(null != val, "");
            for (int i = 0; i < this.count; ++i)
            {
                if (this.items[i].Equals(val))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool Equals(double val)
        {
            for (int i = 0; i < this.count; ++i)
            {
                if (this.items[i].Equals(val))
                {
                    return true;
                }
            }
            return false;
        }

        internal static int GetContextSize(NodeSequence sequence, int itemIndex)
        {
            Fx.Assert(null != sequence, "");
            int size = sequence.items[itemIndex].Size;
            if (size <= 0)
            {
                return sequence.items[-size].Size;
            }
            return size;
        }

        void Grow(int newSize)
        {
            NodeSequenceItem[] newItems = new NodeSequenceItem[newSize];
            if (this.items != null)
            {
                Array.Copy(this.items, newItems, this.items.Length);
            }
            this.items = newItems;
        }

        /// <summary>
        /// Merge all nodesets in this sequence... turning it into a sequence with a single nodeset
        /// This is done by simply renumbering all positions.. and clearing the nodeset flag
        /// </summary>
        internal void Merge()
        {
            Merge(true);
        }

        internal void Merge(bool renumber)
        {
            if (this.count == 0)
            {
                return;
            }

            if (renumber)
            {
                RenumberItems();
            }
        }
#if NO
        // Assumes list is flat and sorted
        internal void RemoveDuplicates()
        {
            if(this.count < 2)
            {
                return;
            }
            
            int last = 0;
            for(int next = 1; next < this.count; ++next)
            {
                if(Comparer.Compare(this.items[last], this.items[next]) != 0)
                {
                    ++last;
                    if(last != next)
                    {
                        this.items[last] = this.items[next];
                    }
                }
            }
            
            this.count = last + 1;

            RenumberItems();
        }
#endif
        void RenumberItems()
        {
            if (this.count > 0)
            {
                for (int i = 0; i < this.count; ++i)
                {
                    this.items[i].SetPositionAndSize(i + 1, this.count);
                }
                this.items[this.count - 1].Flags |= NodeSequenceItemFlags.NodesetLast;
            }
        }
#if NO
        internal void SortNodes()
        {
            this.Merge(false);

            // PERF, Microsoft, make this work
            //Array.Sort<NodeSequenceItem>(this.items, 0, this.count, NodeSequence.Comparer);
            Array.Sort(this.items, 0, this.count, NodeSequence.ObjectComparer);

            RenumberItems();
        }
#endif
        internal void StartNodeset()
        {
            this.position = 0;
            this.sizePosition = -this.count;
        }

        internal void StopNodeset()
        {
            switch (this.position)
            {
                default:
                    int sizePos = -this.sizePosition;
                    this.items[sizePos].Size = this.position;
                    this.items[sizePos + this.position - 1].Last = true;
                    break;

                case 0:
                    break;

                case 1:
                    this.items[-this.sizePosition].SetSizeAndLast();
                    break;
            }
        }

        internal string StringValue()
        {
            if (this.count > 0)
            {
                return this.items[0].StringValue();
            }

            return string.Empty;
        }

        /// <summary>
        /// Union algorithm:
        /// 1. Add both sequences of items to a newly created sequence
        /// 2. Sort the items based on document position
        /// 3. Renumber positions in this new unionized sequence
        /// </summary>
        internal NodeSequence Union(ProcessingContext context, NodeSequence otherSeq)
        {
            NodeSequence seq = context.CreateSequence();

            SortedBuffer<QueryNode, QueryNodeComparer> buff = new SortedBuffer<QueryNode, QueryNodeComparer>(staticQueryNodeComparerInstance);
            for (int i = 0; i < this.count; ++i)
                buff.Add(this.items[i].Node);

            for (int i = 0; i < otherSeq.count; ++i)
                buff.Add(otherSeq.items[i].Node);

            for (int i = 0; i < buff.Count; ++i)
                seq.Add(buff[i]);

            seq.RenumberItems();
            return seq;

            /*
            // PERF, Microsoft, I think we can do the merge ourselves and avoid the sort.
            //               Need to verify that the sequences are always in document order.
            for(int i = 0; i < this.count; ++i)
            {
                seq.AddCopy(ref this.items[i]);
            }
            
            for(int i = 0; i < otherSeq.count; ++i)
            {
                seq.AddCopy(ref otherSeq.items[i]);
            }
            
            seq.SortNodes();
            seq.RemoveDuplicates();
             
            return seq;
            */
        }

        #region IQueryBufferPool Members
#if NO
        public void Reset()
        {
            this.count = 0;
            this.Trim();
        }

        public void Trim()
        {
            if (this.count == 0)
            {
                this.items = null;
            }
            else if (this.count < this.items.Length)
            {
                NodeSequenceItem[] newItems = new NodeSequenceItem[this.count];
                Array.Copy(this.items, newItems, this.count);
                this.items = newItems;
            }
        }
#endif
        #endregion
    }

    internal class NodeSequenceIterator : XPathNodeIterator
    {
        // Shared
        NodeSequence seq;

        // Instance
        NodeSequenceIterator data;
        int index;
        SeekableXPathNavigator nav; // the navigator that will be used by this iterator

        internal NodeSequenceIterator(NodeSequence seq)
            : base()
        {
            this.data = this;
            this.seq = seq;
        }

        internal NodeSequenceIterator(NodeSequenceIterator iter)
        {
            this.data = iter.data;
            this.index = iter.index;
        }

        public override int Count
        {
            get
            {
                return this.data.seq.Count;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                if (this.index == 0)
                {
#pragma warning suppress 56503 // Microsoft, postponing the public change
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryProcessingException(QueryProcessingError.Unexpected, SR.GetString(SR.QueryContextNotSupportedInSequences)));
                }

                if (this.index > this.data.seq.Count)
                {
#pragma warning suppress 56503 // Microsoft, postponing the public change
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QueryAfterNodes)));
                }
                //
                // From MSDN - the public contract of .Current
                // You can use the properties of the XPathNavigator to return information on the current node. 
                // However, the XPathNavigator cannot be used to move away from the selected node set. 
                // Doing so could invalidate the state of the navigator. Alternatively, you can clone the XPathNavigator. 
                // The cloned XPathNavigator can then be moved away from the selected node set. This is an application level decision. 
                // Providing this functionality may effect the performance of the XPath query.
                //                
                // Return the navigator as is - where it is positioned. If the user moved the navigator, then the user is
                // hosed. We will make no guarantees - and are not required to. Doing so would force cloning, which is expensive.
                //
                // NOTE: .Current can get called repeatedly, so its activity should be relative CHEAP. 
                // No cloning, copying etc. All that work should be done in MoveNext()
                return this.nav;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.index;
            }
        }

        internal void Clear()
        {
            this.data.seq = null;
            this.nav = null;
        }

        public override XPathNodeIterator Clone()
        {
            return new NodeSequenceIterator(this);
        }

        public override IEnumerator GetEnumerator()
        {
            return new NodeSequenceEnumerator(this);
        }

        public override bool MoveNext()
        {
            if (null == this.data.seq)
            {
                // User is trying to use an iterator that is  out of scope.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QueryIteratorOutOfScope)));
            }

            if (this.index < this.data.seq.Count)
            {
                if (null == this.nav)
                {
                    // We haven't aquired the navigator we will use for this iterator yet. 
                    this.nav = (SeekableXPathNavigator)this.data.seq[this.index].GetNavigator().Clone();
                }
                else
                {
                    this.nav.CurrentPosition = this.data.seq[this.index].GetNavigatorPosition();
                }
                this.index++;
                return true;
            }

            this.index++;
            this.nav = null;
            return false;
        }

        public void Reset()
        {
            this.nav = null;
            this.index = 0;
        }
    }

    internal class NodeSequenceEnumerator : IEnumerator
    {
        NodeSequenceIterator iter;

        internal NodeSequenceEnumerator(NodeSequenceIterator iter)
        {
            this.iter = new NodeSequenceIterator(iter);
            Reset();
        }

        public object Current
        {
            get
            {
                if (this.iter.CurrentPosition == 0)
                {
#pragma warning suppress 56503 // Microsoft, postponing the public change
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QueryBeforeNodes)));
                }

                if (this.iter.CurrentPosition > this.iter.Count)
                {
#pragma warning suppress 56503 // Microsoft, postponing the public change
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.QueryAfterNodes)));
                }

                return this.iter.Current;
            }
        }

        public bool MoveNext()
        {
            return iter.MoveNext();
        }

        public void Reset()
        {
            this.iter.Reset();
        }
    }

    internal class SafeNodeSequenceIterator : NodeSequenceIterator, IDisposable
    {
        ProcessingContext context;
        int disposed;
        NodeSequence seq;

        public SafeNodeSequenceIterator(NodeSequence seq, ProcessingContext context)
            : base(seq)
        {
            this.context = context;
            this.seq = seq;
            Interlocked.Increment(ref this.seq.refCount);
            this.context.Processor.AddRef();
        }

        public override XPathNodeIterator Clone()
        {
            return new SafeNodeSequenceIterator(this.seq, this.context);
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
            {
                QueryProcessor processor = this.context.Processor;
                this.context.ReleaseSequence(this.seq);
                this.context.Processor.Matcher.ReleaseProcessor(processor);
            }
        }
    }

    internal struct NodesetIterator
    {
        int index;
        int indexStart;
        NodeSequence sequence;
        NodeSequenceItem[] items;

        internal NodesetIterator(NodeSequence sequence)
        {
            Fx.Assert(null != sequence, "");
            this.sequence = sequence;
            this.items = sequence.Items;
            this.index = -1;
            this.indexStart = -1;
        }

        internal int Index
        {
            get
            {
                return this.index;
            }
        }

        internal bool NextItem()
        {
            if (-1 == this.index)
            {
                this.index = this.indexStart;
                return true;
            }

            if (this.items[this.index].Last)
            {
                return false;
            }

            this.index++;
            return true;
        }

        internal bool NextNodeset()
        {
            this.indexStart = this.index + 1;
            this.index = -1;
            return (this.indexStart < this.sequence.Count);
        }
    }

    internal struct NodeSequenceBuilder
    {
        ProcessingContext context;
        NodeSequence sequence;

        internal NodeSequenceBuilder(ProcessingContext context, NodeSequence sequence)
        {
            this.context = context;
            this.sequence = sequence;
        }

        internal NodeSequenceBuilder(ProcessingContext context)
            : this(context, null)
        {
        }
#if NO
        internal NodeSequenceBuilder(NodeSequence sequence)
            : this(sequence.OwnerContext, sequence)
        {
        }
#endif
        internal NodeSequence Sequence
        {
            get
            {
                return (null != this.sequence) ? this.sequence : NodeSequence.Empty;
            }
            set
            {
                this.sequence = value;
            }
        }

        internal void Add(ref NodeSequenceItem item)
        {
            if (null == this.sequence)
            {
                this.sequence = this.context.CreateSequence();
                this.sequence.StartNodeset();
            }

            this.sequence.Add(ref item);
        }

        internal void EndNodeset()
        {
            if (null != this.sequence)
            {
                this.sequence.StopNodeset();
            }
        }

        internal void StartNodeset()
        {
            if (null != this.sequence)
            {
                this.sequence.StartNodeset();
            }
        }
    }
}
