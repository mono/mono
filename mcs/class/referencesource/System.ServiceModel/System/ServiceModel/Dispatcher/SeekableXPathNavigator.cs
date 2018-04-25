//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    
    /// <summary>
    /// SeekableXPathNavigator extends XPathNavigator
    /// 
    /// The FilterEngine can work with SeekingXPathNavigator more efficiently than it can with
    /// the raw XPathNavigator.
    ///
    /// A navigator is a cursor over an Xml document. While executing queries, the FE must take 
    /// frequent snapshots of the Navigator/cursor's current position. The only way for it to do so 
    /// is to Clone the navigator, which requires a memory allocation - something we wish to avoid. 
    /// Therefore, we introduce a CurrentPosition property that gives us a very fast way to save 
    /// and set where the navigator is placed in a document - without cloning. 
    /// 
    /// </summary>
    public abstract class SeekableXPathNavigator : XPathNavigator
    {
        // An opaque position reference
        // An integer for efficiency 
        public abstract long CurrentPosition
        {
            get;
            set;
        }        
        /// <summary>
        /// Compare the two given navigator positions
        /// </summary>
        public abstract XmlNodeOrder ComparePosition(long firstPosition, long secondPosition);
        /// <summary>
        /// Return the local name of the node at the given position
        /// The localName is typically a node's tag - without the prefix
        /// </summary>
        public abstract string GetLocalName(long nodePosition);
        /// <summary>
        /// Return the Name of the node at the given position
        /// The Name is a node's tag and typically includes the prefix
        /// </summary>
        public abstract string GetName(long nodePosition); 
        /// <summary>
        /// Return the namespace URI of the node at the given positon
        /// </summary>
        public abstract string GetNamespace(long nodePosition);
        /// <summary>
        /// What kind of node is this? - Element, Attribute, Processing Instruction etc
        /// </summary>
        public abstract XPathNodeType GetNodeType(long nodePosition);
        /// <summary>
        /// Return the string value of the node at the given position. 
        /// For elements, this is InnerText - the concatenation of all Text nodes below the node at the specified position
        /// </summary>
        public abstract string GetValue(long nodePosition);
    }
}
