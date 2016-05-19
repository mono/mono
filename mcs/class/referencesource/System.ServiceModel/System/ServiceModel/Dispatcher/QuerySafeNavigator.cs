//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Runtime;
    using System.Xml;
    using System.Xml.XPath;

#if NO 
    //
    // A message is just one more source of Xml data. To filter a message, we create a navigator over it that 
    // surfaces its contained Xml to the filter engine. 
    // In M5.1, we navigate messages by first writing them into a message document. This turns the message into an
    // Xml DOM. we then get a navigator from the DOM. 
    // In M5.2, we'll navigate messages without requiring this step.
    //
    internal class MessageNavigator : GenericSeekableNavigator
    {
        StringBuilder builder;
        int headerCollectionVersion;
        bool loaded;
        bool navigatesBody;

        internal MessageNavigator(MessageNavigator nav)
            : base(nav)
        {
            this.headerCollectionVersion = nav.headerCollectionVersion;
            this.loaded = nav.loaded;
            this.navigatesBody = nav.navigatesBody;
        }
        
        internal MessageNavigator(Message message, bool navigateBody)
            : base()
        {
            this.navigatesBody = navigateBody;
            this.Load(message, this.navigatesBody);
        }

        internal bool IsLoaded
        {
            get
            {
                return this.loaded;
            }
        }

        internal bool NavigatesBody
        {
            get
            {
                return this.navigatesBody;
            }
        }

        internal override void Clear()
        {
            base.Clear();
            this.loaded = false;
            this.navigatesBody = false;
        }
                
        public override XPathNavigator Clone()
        {
            return new MessageNavigator(this);
        }
        
        internal MessageNavigator Ensure(Message message, bool navigateBody)
        {
            // Rebuild the navigator if:
            // If this navigator does not navigate on bodies and now we need to (or vice versa)
            // Or the header collection changed under us
            if (this.navigatesBody != navigateBody || message.Headers.CollectionVersion != this.headerCollectionVersion)
            {
                this.Load(message, navigateBody);
            }
            else
            {
                this.MoveToRoot();
            }

            return this;
        }

        // To load a message into a message document, we write the message into a buffer and then let XPathDocument
        // load that buffer
        internal void Load(Message message, bool navigatesBody)
        {
            if (null == this.builder)
            {
                this.builder = new StringBuilder(1024);
            }

            StringWriter stringWriter = new StringWriter(this.builder);
            XmlWriter writer = new XmlTextWriter(stringWriter);

            message.WriteMessage(writer, navigatesBody);
            writer.Close();
            
            StringReader reader = new StringReader(this.builder.ToString());
            XPathDocument messageDoc = new XPathDocument(reader);
            reader.Close();            
            this.builder.Length = 0;
            
            this.Init(messageDoc.CreateNavigator());
            this.loaded = true;
            this.navigatesBody = navigatesBody;
            this.headerCollectionVersion = message.Headers.CollectionVersion;
        }
    }
#endif

    // To prevent XPaths from running forever etc, users can specify limits on:
    //   the # of nodes a filter or filter table should inspect
    //
    // This file contains navigators that impose these limits

    internal interface INodeCounter
    {
        int CounterMarker { get; set; }
        int MaxCounter { set; }
        int ElapsedCount(int marker);
        void Increase();
        void IncreaseBy(int count);
    }

    internal class DummyNodeCounter : INodeCounter
    {
        internal static DummyNodeCounter Dummy = new DummyNodeCounter();
        public int CounterMarker
        {
            get { return 0; }
            set { }
        }

        public int MaxCounter
        {
            set { }
        }

        public int ElapsedCount(int marker) { return 0; }

        public void Increase() { }
        public void IncreaseBy(int count) { }
    }

    /// <summary>
    /// Seekable navigators that wrap other navigators and doesn't exceed node counting limits
    /// </summary>
    internal class SafeSeekableNavigator : SeekableXPathNavigator, INodeCounter
    {
        SeekableXPathNavigator navigator;
        SafeSeekableNavigator counter;
        int nodeCount;
        int nodeCountMax;

        internal SafeSeekableNavigator(SafeSeekableNavigator nav)
        {
            this.navigator = (SeekableXPathNavigator)nav.navigator.Clone();
            this.counter = nav.counter;
        }

        internal SafeSeekableNavigator(SeekableXPathNavigator navigator, int nodeCountMax)
        {
            this.navigator = navigator;
            this.counter = this;
            this.nodeCount = nodeCountMax;
            this.nodeCountMax = nodeCountMax;
        }

        public override string BaseURI
        {
            get
            {
                return this.navigator.BaseURI;
            }
        }

        public int CounterMarker
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

        public int MaxCounter
        {
            set
            {
                this.counter.nodeCountMax = value;
            }
        }

        /// <summary>
        /// Setting the current position moves this navigator to the location specified by the given position
        /// </summary>
        public override long CurrentPosition
        {
            get
            {
                return this.navigator.CurrentPosition;
            }
            set
            {
                this.navigator.CurrentPosition = value;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.navigator.HasAttributes;
            }
        }

        public override bool HasChildren
        {
            get
            {
                return this.navigator.HasChildren;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.navigator.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.navigator.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.navigator.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.navigator.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.navigator.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.navigator.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.navigator.Prefix;
            }
        }

        public override string Value
        {
            get
            {
                return this.navigator.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.navigator.XmlLang;
            }
        }

        public override XPathNavigator Clone()
        {
            return new SafeSeekableNavigator(this);
        }

#if NO
        internal SafeNavigator CreateSafeXPathNavigator()
        {
            return new SafeNavigator(this, this.navigator);
        }
#endif
        public override XmlNodeOrder ComparePosition(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return XmlNodeOrder.Unknown;
            }

            SafeSeekableNavigator nav = navigator as SafeSeekableNavigator;
            if (nav != null)
            {
                return this.navigator.ComparePosition(nav.navigator);
            }
            return XmlNodeOrder.Unknown;
        }

        public override XmlNodeOrder ComparePosition(long x, long y)
        {
            return this.navigator.ComparePosition(x, y);
        }

        public int ElapsedCount(int marker)
        {
            return marker - this.counter.nodeCount;
        }

        public override string GetLocalName(long nodePosition)
        {
            return this.navigator.GetLocalName(nodePosition);
        }

        public override string GetName(long nodePosition)
        {
            return this.navigator.GetName(nodePosition);
        }

        public override string GetNamespace(long nodePosition)
        {
            return this.navigator.GetNamespace(nodePosition);
        }

        public override XPathNodeType GetNodeType(long nodePosition)
        {
            return this.navigator.GetNodeType(nodePosition);
        }

        public override string GetValue(long nodePosition)
        {
            return this.navigator.GetValue(nodePosition);
        }

        public override string GetNamespace(string name)
        {
            this.IncrementNodeCount();
            return this.navigator.GetNamespace(name);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            this.IncrementNodeCount();
            return this.navigator.GetAttribute(localName, namespaceURI);
        }

        public void Increase()
        {
            this.IncrementNodeCount();
        }

        public void IncreaseBy(int count)
        {
            this.counter.nodeCount -= (count - 1);
            Increase();
        }

        internal void IncrementNodeCount()
        {
            if (this.counter.nodeCount > 0)
            {
                this.counter.nodeCount--;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XPathNavigatorException(SR.GetString(SR.FilterNodeQuotaExceeded, this.counter.nodeCountMax)));
            }
        }
#if NO
        internal virtual void Init(SeekableXPathNavigator navigator, int nodeCountMax)
        {
            this.navigator = navigator;
            this.nodeCount = nodeCountMax;
            this.counter = this;
        }
#endif
        public override bool IsDescendant(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return false;
            }

            SafeSeekableNavigator nav = navigator as SafeSeekableNavigator;
            if (nav != null)
            {
                return this.navigator.IsDescendant(nav.navigator);
            }
            return false;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            if (other == null)
            {
                return false;
            }

            SafeSeekableNavigator nav = other as SafeSeekableNavigator;
            if (nav != null)
            {
                return this.navigator.IsSamePosition(nav.navigator);
            }
            return false;
        }

        public override void MoveToRoot()
        {
            this.IncrementNodeCount();
            this.navigator.MoveToRoot();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToNextAttribute()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToNextAttribute();
        }

        public override bool MoveToPrevious()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToPrevious();
        }

        public override bool MoveToFirstAttribute()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToFirstAttribute();
        }

        public override bool MoveToNamespace(string name)
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToNamespace(name);
        }

        public override bool MoveToParent()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToParent();
        }

        public override bool MoveTo(XPathNavigator other)
        {
            if (other == null)
            {
                return false;
            }

            this.IncrementNodeCount();
            SafeSeekableNavigator nav = other as SafeSeekableNavigator;
            if (nav != null)
            {
                return this.navigator.MoveTo(nav.navigator);
            }
            return false;
        }

        public override bool MoveToId(string id)
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToId(id);
        }

        public override bool MoveToFirstChild()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToAttribute(localName, namespaceURI);
        }

        public override bool MoveToNext()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToNext();
        }

        public override bool MoveToFirst()
        {
            this.IncrementNodeCount();
            return this.navigator.MoveToFirst();
        }
    }

    /// <summary>
    /// The filter engine works with seekable navigators. This class takes a generic XPathNavigator implementation 
    /// and transforms it into a seekable navigator. Seekable navigators associate a 'position' to every node in a DOM.
    /// 
    /// This class maintains a (position, navigator) map. Cloning navigators is unavoidable - XPathNavigator offers
    /// no other way to snapshot its current position. However, caching allows memory allocations to be avoided - but
    /// only once the navigator is warmed up.
    /// </summary>
    internal class GenericSeekableNavigator : SeekableXPathNavigator
    {
        QueryBuffer<XPathNavigator> nodes;
        long currentPosition;
        XPathNavigator navigator;

        GenericSeekableNavigator dom;
#if NO
        internal GenericSeekableNavigator()
        {
            this.nodes = new QueryBuffer<XPathNavigator>(4);
            this.currentPosition = -1;
        }
#endif
        internal GenericSeekableNavigator(XPathNavigator navigator)
        {
            this.navigator = navigator;
            this.nodes = new QueryBuffer<XPathNavigator>(4);
            this.currentPosition = -1;
            this.dom = this;
        }

        internal GenericSeekableNavigator(GenericSeekableNavigator navigator)
        {
            this.navigator = navigator.navigator.Clone();
            this.nodes = default(QueryBuffer<XPathNavigator>);
            this.currentPosition = navigator.currentPosition;
            this.dom = navigator.dom;
        }

        public override string BaseURI
        {
            get
            {
                return this.navigator.BaseURI;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.navigator.HasAttributes;
            }
        }

        public override bool HasChildren
        {
            get
            {
                return this.navigator.HasChildren;
            }
        }

#if NO        
        internal XPathNavigator InternalNavigator
        {
            get
            {
                return this.navigator;
            }
        }
#endif
        public override bool IsEmptyElement
        {
            get
            {
                return this.navigator.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.navigator.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.navigator.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.navigator.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.navigator.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.navigator.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.navigator.Prefix;
            }
        }

        public override string Value
        {
            get
            {
                return this.navigator.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.navigator.XmlLang;
            }
        }

        /// <summary>
        /// Setting the current position moves this navigator to the location specified by the given position
        /// </summary>
        public override long CurrentPosition
        {
            get
            {
                if (-1 == this.currentPosition)
                {
                    this.SnapshotNavigator();
                }
                return this.currentPosition;
            }
            set
            {
                this.navigator.MoveTo(this[value]);
                this.currentPosition = value;
            }
        }

        /// <summary>
        /// Return the XPathNavigator that has the given position
        /// </summary>
        internal XPathNavigator this[long nodePosition]
        {
            get
            {
                int pos = (int)nodePosition;
                Fx.Assert(this.dom.nodes.IsValidIndex(pos) && null != this.dom.nodes[pos], "");
                return this.dom.nodes[pos];
            }
        }

#if NO
        internal virtual void Clear()
        {
            this.navigator = null;
            this.currentPosition = -1;
        }
#endif
        public override XPathNavigator Clone()
        {
            return new GenericSeekableNavigator(this);
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return XmlNodeOrder.Unknown;
            }

            GenericSeekableNavigator nav = navigator as GenericSeekableNavigator;
            if (nav != null)
            {
                return this.navigator.ComparePosition(nav.navigator);
            }
            return XmlNodeOrder.Unknown;
        }

        public override XmlNodeOrder ComparePosition(long x, long y)
        {
            XPathNavigator nodeX = this[x];
            XPathNavigator nodeY = this[y];

            return nodeX.ComparePosition(nodeY);
        }

        public override string GetLocalName(long nodePosition)
        {
            return this[nodePosition].LocalName;
        }

        public override string GetName(long nodePosition)
        {
            return this[nodePosition].Name;
        }

        public override string GetNamespace(long nodePosition)
        {
            return this[nodePosition].NamespaceURI;
        }

        public override XPathNodeType GetNodeType(long nodePosition)
        {
            return this[nodePosition].NodeType;
        }

        public override string GetValue(long nodePosition)
        {
            return this[nodePosition].Value;
        }

        public override string GetNamespace(string name)
        {
            return this.navigator.GetNamespace(name);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            return this.navigator.GetAttribute(localName, namespaceURI);
        }

#if NO
        internal void Init(XPathNavigator navigator)
        {
            Fx.Assert(null != navigator, "");
            this.navigator = navigator;
            this.currentPosition = -1;
        }
#endif
        public override bool IsDescendant(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return false;
            }

            GenericSeekableNavigator nav = navigator as GenericSeekableNavigator;
            if (null != nav)
            {
                return this.navigator.IsDescendant(nav.navigator);
            }
            return false;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            GenericSeekableNavigator nav = other as GenericSeekableNavigator;
            if (null != nav)
            {
                return this.navigator.IsSamePosition(nav.navigator);
            }
            return false;
        }

        public override void MoveToRoot()
        {
            this.currentPosition = -1;
            this.navigator.MoveToRoot();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            this.currentPosition = -1;
            return this.navigator.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToNextAttribute()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToNextAttribute();
        }

        public override bool MoveToPrevious()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToPrevious();
        }

        public override bool MoveToFirstAttribute()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToFirstAttribute();
        }

        public override bool MoveToNamespace(string name)
        {
            this.currentPosition = -1;
            return this.navigator.MoveToNamespace(name);
        }

        public override bool MoveToParent()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToParent();
        }

        public override bool MoveTo(XPathNavigator other)
        {
            GenericSeekableNavigator nav = other as GenericSeekableNavigator;
            if (null != nav)
            {
                if (this.navigator.MoveTo(nav.navigator))
                {
                    this.currentPosition = nav.currentPosition;
                    return true;
                }
            }

            return false;
        }

        public override bool MoveToId(string id)
        {
            this.currentPosition = -1;
            return this.navigator.MoveToId(id);
        }

        public override bool MoveToFirstChild()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            this.currentPosition = -1;
            return this.navigator.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            this.currentPosition = -1;
            return this.navigator.MoveToAttribute(localName, namespaceURI);
        }

        public override bool MoveToNext()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToNext();
        }

        public override bool MoveToFirst()
        {
            this.currentPosition = -1;
            return this.navigator.MoveToFirst();
        }

        internal void SnapshotNavigator()
        {
            this.currentPosition = this.dom.nodes.Count;
            this.dom.nodes.Add(this.navigator.Clone());
            /*
            if (this.currentPosition < this.nodes.Count)
            {
                // Use a cached navigator
                XPathNavigator clonedNavigator = this.nodes[(int)this.currentPosition];
                Fx.Assert(null != clonedNavigator, "");
                clonedNavigator.MoveTo(this);
            }
            else
            {
                this.nodes.Add(this.navigator.Clone());
            }
            */
        }

        #region IQueryBufferPool Members
#if NO 
        /// <summary>
        /// Reset the pool by deleting it entirely and starting it over
        /// </summary>
        public void Reset()
        {
            this.nodes.count = 0;
            this.nodes.TrimToCount();
        }

        public void Trim()
        {
            this.nodes.TrimToCount();
        }
#endif
        #endregion
    }
}
