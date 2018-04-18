
//------------------------------------------------------------------------------
// <copyright file="XsdCachingReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Security.Policy;

namespace System.Xml {

    internal partial class XsdCachingReader : XmlReader, IXmlLineInfo {

        private enum CachingReaderState {
            None = 0,
            Init = 1,
            Record = 2,
            Replay = 3,
            ReaderClosed = 4,
            Error = 5,
        }
        
        private XmlReader coreReader;
        private XmlNameTable coreReaderNameTable;

        private ValidatingReaderNodeData[] contentEvents;
        private ValidatingReaderNodeData[] attributeEvents;
                    
        private ValidatingReaderNodeData cachedNode;
        
        private CachingReaderState cacheState;
        int contentIndex;
        int attributeCount;

        private bool returnOriginalStringValues;

        private CachingEventHandler cacheHandler;

        //current state
        int currentAttrIndex;
        int currentContentIndex;
        bool readAhead;
        
        //Lineinfo
        IXmlLineInfo lineInfo;

        //ReadAttributeValue TextNode
        private ValidatingReaderNodeData textNode;

        //Constants
        private const int InitialAttributeCount = 8;
        private const int InitialContentCount = 4;

//Constructor
        internal XsdCachingReader(XmlReader reader, IXmlLineInfo lineInfo, CachingEventHandler handlerMethod) {
            this.coreReader = reader;
            this.lineInfo = lineInfo;
            this.cacheHandler = handlerMethod;
            attributeEvents = new ValidatingReaderNodeData[InitialAttributeCount];
            contentEvents = new ValidatingReaderNodeData[InitialContentCount];
            Init();
        }
        
        private void Init() {
            coreReaderNameTable = coreReader.NameTable;
            cacheState = CachingReaderState.Init;
            contentIndex = 0;
            currentAttrIndex = -1;
            currentContentIndex = -1;
            attributeCount = 0;
            cachedNode = null;
            readAhead = false;
            //Initialize the cachingReader with start state
            if (coreReader.NodeType == XmlNodeType.Element) {
                ValidatingReaderNodeData element = AddContent(coreReader.NodeType);
                element.SetItemData(coreReader.LocalName, coreReader.Prefix, coreReader.NamespaceURI, coreReader.Depth);  //Only created for element node type
                element.SetLineInfo(lineInfo);
                RecordAttributes();
            }
        }

        internal void Reset(XmlReader reader) {
            this.coreReader = reader;
            Init();
        }

        // Settings
        public override XmlReaderSettings Settings { 
            get {                
                return coreReader.Settings;
            }
        }

    // Node Properties

        // Gets the type of the current node.
        public override XmlNodeType NodeType { 
            get {
                return cachedNode.NodeType;
            }
        }

        // Gets the name of the current node, including the namespace prefix.
        public override string Name { 
            get {
                return cachedNode.GetAtomizedNameWPrefix(coreReaderNameTable); 
            }
        }

        // Gets the name of the current node without the namespace prefix.
        public override string LocalName { 
            get {
                return cachedNode.LocalName;
            }
        }

        // Gets the namespace URN (as defined in the W3C Namespace Specification) of the current namespace scope.
        public override string NamespaceURI { 
            get {
                return cachedNode.Namespace;
            }
        }

        // Gets the namespace prefix associated with the current node.
        public override string Prefix { 
            get {
                return cachedNode.Prefix;
            }
        }

        // Gets a value indicating whether the current node can have a non-empty Value.
        public override bool HasValue { 
            get {
                return XmlReader.HasValueInternal(cachedNode.NodeType);
            }
        }

        // Gets the text value of the current node.
        public override string Value { 
            get {
                return returnOriginalStringValues ? cachedNode.OriginalStringValue : cachedNode.RawValue;
            }
        }

        // Gets the depth of the current node in the XML element stack.
        public override int Depth { 
            get {
                return cachedNode.Depth;
            }
        }

        // Gets the base URI of the current node.
        public override string BaseURI { 
            get {
                return coreReader.BaseURI;
            }
        }

        // Gets a value indicating whether the current node is an empty element (for example, <MyElement/>).
        public override bool IsEmptyElement { 
            get {
                return false;
            }
        }

        // Gets a value indicating whether the current node is an attribute that was generated from the default value defined
        // in the DTD or schema.
        public override bool IsDefault { 
            get {
                return false;
            }
        }

        // Gets the quotation mark character used to enclose the value of an attribute node.
        public override char QuoteChar { 
            get {
                return coreReader.QuoteChar;
            }
        }

        // Gets the current xml:space scope.
        public override XmlSpace XmlSpace { 
            get {
                return coreReader.XmlSpace;
            }
        }

        // Gets the current xml:lang scope.
        public override string XmlLang { 
            get {
                return coreReader.XmlLang;
            }
        }

    // Attribute Accessors

        // The number of attributes on the current node.
        public override int AttributeCount { 
            get {
                return attributeCount; 
            }
        }

        // Gets the value of the attribute with the specified Name.
        public override string GetAttribute( string name ) {
            int i;
            if (name.IndexOf( ':' ) == -1) {
                i = GetAttributeIndexWithoutPrefix(name);
            }
            else {
                i = GetAttributeIndexWithPrefix(name);
            }
            return (i >= 0) ? attributeEvents[i].RawValue : null;
        }

        // Gets the value of the attribute with the specified LocalName and NamespaceURI.
       public override string GetAttribute( string name, string namespaceURI ) {
            namespaceURI = ( namespaceURI == null ) ? string.Empty : coreReaderNameTable.Get( namespaceURI );
            name = coreReaderNameTable.Get(name);
            ValidatingReaderNodeData attribute;
            for ( int i = 0; i < attributeCount; i++ ) {
                attribute = attributeEvents[i];
                if ( Ref.Equal(attribute.LocalName, name) && Ref.Equal(attribute.Namespace, namespaceURI) ) {
                    return attribute.RawValue;
                }
            }
            return null;
        }

        // Gets the value of the attribute with the specified index.
        public override string GetAttribute( int i ) {
            if ( i < 0 || i >= attributeCount ) {
                throw new ArgumentOutOfRangeException("i");
            }
            return attributeEvents[i].RawValue;
        }

        // Gets the value of the attribute with the specified index.
        public override string this [ int i ] {
            get {
                return GetAttribute(i);
            }
        }

        // Gets the value of the attribute with the specified Name.
        public override string this [ string name ] { 
            get {
                return GetAttribute(name);
            }
        }

        // Gets the value of the attribute with the specified LocalName and NamespaceURI.
        public override string this [ string name, string namespaceURI ] { 
            get {
                return GetAttribute(name, namespaceURI);
            }
        }

        // Moves to the attribute with the specified Name.
        public override bool MoveToAttribute( string name ) {
            int i;
            if (name.IndexOf( ':' ) == -1) {
                i = GetAttributeIndexWithoutPrefix( name );
            }
            else {
                i = GetAttributeIndexWithPrefix( name );
            }

            if ( i >= 0 ) {
                currentAttrIndex = i;
                cachedNode = attributeEvents[i];
                return true;
            }
            else {
                return false;
            }
        }

        // Moves to the attribute with the specified LocalName and NamespaceURI
        public override bool MoveToAttribute( string name, string ns ) {
            ns = (ns == null) ? string.Empty : coreReaderNameTable.Get(ns);
            name = coreReaderNameTable.Get(name);
            ValidatingReaderNodeData attribute;
            for ( int i = 0; i < attributeCount; i++ ) {
                attribute = attributeEvents[i];
                if ( Ref.Equal(attribute.LocalName, name) &&
                     Ref.Equal(attribute.Namespace, ns) ) {
                         currentAttrIndex = i;
                         cachedNode = attributeEvents[i];
                    return true;
                }
            }
            return false;
        }
        
        // Moves to the attribute with the specified index.
        public override void MoveToAttribute(int i) {
            if ( i < 0 || i >= attributeCount ) {
                throw new ArgumentOutOfRangeException( "i" );
            }
            currentAttrIndex = i;
            cachedNode = attributeEvents[i];
        }

        // Moves to the first attribute.
        public override bool MoveToFirstAttribute() {
            if (attributeCount == 0) {
                return false;
            }
            currentAttrIndex = 0;
            cachedNode = attributeEvents[0];
            return true;
        }

        // Moves to the next attribute.
        public override bool MoveToNextAttribute() {
            if (currentAttrIndex + 1 < attributeCount) {
                cachedNode = attributeEvents[++currentAttrIndex];
                return true;
            }
            return false;
        }

        // Moves to the element that contains the current attribute node.
        public override bool MoveToElement() {
            if (cacheState != CachingReaderState.Replay || cachedNode.NodeType != XmlNodeType.Attribute) {
                return false;
            }
            currentContentIndex = 0;
            currentAttrIndex = -1;
            Read();
            return true;
        }

        // Reads the next node from the stream/TextReader.
        public override  bool  Read() {
            switch (cacheState) {
                case CachingReaderState.Init:
                    cacheState = CachingReaderState.Record;
                    goto case CachingReaderState.Record;

                case CachingReaderState.Record: 
                    ValidatingReaderNodeData recordedNode = null;
                    if (coreReader.Read()) {
                        switch(coreReader.NodeType) {
                            case XmlNodeType.Element:
                                //Dont record element within the content of a union type since the main reader will break on this and the underlying coreReader will be positioned on this node
                                cacheState = CachingReaderState.ReaderClosed;
                                return false;

                            case XmlNodeType.EndElement:
                                recordedNode = AddContent(coreReader.NodeType);
                                recordedNode.SetItemData(coreReader.LocalName, coreReader.Prefix, coreReader.NamespaceURI, coreReader.Depth);  //Only created for element node type
                                recordedNode.SetLineInfo(lineInfo);
                                break;

                            case XmlNodeType.Comment:
                            case XmlNodeType.ProcessingInstruction:
                            case XmlNodeType.Text:
                            case XmlNodeType.CDATA:
                            case XmlNodeType.Whitespace:
                            case XmlNodeType.SignificantWhitespace:
                                recordedNode = AddContent(coreReader.NodeType);
                                recordedNode.SetItemData(coreReader.Value);
                                recordedNode.SetLineInfo(lineInfo);
                                recordedNode.Depth = coreReader.Depth;
                                break;

                            default:
                                break;       
                        }
                        cachedNode = recordedNode;
                        return true;    
                    }
                    else {
                        cacheState = CachingReaderState.ReaderClosed;
                        return false;
                    }    

                case CachingReaderState.Replay:
                    if (currentContentIndex >= contentIndex) { //When positioned on the last cached node, switch back as the underlying coreReader is still positioned on this node
                        cacheState = CachingReaderState.ReaderClosed;
                        cacheHandler(this);
                        if (coreReader.NodeType != XmlNodeType.Element || readAhead) { //Only when coreReader not positioned on Element node, read ahead, otherwise it is on the next element node already, since this was not cached
                            return coreReader.Read();
                        }
                        return true;                        
                    }
                    cachedNode = contentEvents[currentContentIndex];
                    if (currentContentIndex > 0) {
                        ClearAttributesInfo();
                    }
                    currentContentIndex++;
                    return true;

                default:
                    return false;
            }
        }

        internal ValidatingReaderNodeData RecordTextNode(string textValue, string originalStringValue, int depth, int lineNo, int linePos) {
            ValidatingReaderNodeData textNode = AddContent(XmlNodeType.Text);
            textNode.SetItemData(textValue, originalStringValue);
            textNode.SetLineInfo(lineNo, linePos);
            textNode.Depth = depth;
            return textNode;
        }

        internal void SwitchTextNodeAndEndElement( string textValue, string originalStringValue ) {
            Debug.Assert(coreReader.NodeType == XmlNodeType.EndElement || (coreReader.NodeType == XmlNodeType.Element && coreReader.IsEmptyElement));

            ValidatingReaderNodeData textNode = RecordTextNode(textValue, originalStringValue, coreReader.Depth + 1, 0, 0);
            int endElementIndex = contentIndex - 2;
            ValidatingReaderNodeData endElementNode = contentEvents[endElementIndex];
            Debug.Assert(endElementNode.NodeType == XmlNodeType.EndElement);
            contentEvents[endElementIndex] = textNode;
            contentEvents[contentIndex - 1] = endElementNode;   
        }

        internal void RecordEndElementNode() {
            ValidatingReaderNodeData recordedNode = AddContent(XmlNodeType.EndElement);
            Debug.Assert(coreReader.NodeType == XmlNodeType.EndElement || (coreReader.NodeType == XmlNodeType.Element && coreReader.IsEmptyElement));
            recordedNode.SetItemData(coreReader.LocalName, coreReader.Prefix, coreReader.NamespaceURI, coreReader.Depth);  
            recordedNode.SetLineInfo(coreReader as IXmlLineInfo);
            if (coreReader.IsEmptyElement) { //Simulated endElement node for <e/>, the coreReader is on cached Element node itself.
                readAhead = true;
            }
        }

        internal string ReadOriginalContentAsString() {
            returnOriginalStringValues = true;
            string strValue = InternalReadContentAsString();
            returnOriginalStringValues = false;
            return strValue;
        }

        // Gets a value indicating whether XmlReader is positioned at the end of the stream.
        public override bool EOF { 
            get {
                return cacheState == CachingReaderState.ReaderClosed && coreReader.EOF;
            }
        }

        // Closes the stream, changes the ReadState to Closed, and sets all the properties back to zero.
        public override void Close() {
            coreReader.Close();
            cacheState = CachingReaderState.ReaderClosed;
        }

        // Returns the read state of the stream.
        public override ReadState ReadState { 
            get {
                return coreReader.ReadState;
            }
        }

        // Skips to the end tag of the current element.
        public override void Skip() {
            //Skip on caching reader should move to the end of the subtree, past all cached events
            switch (cachedNode.NodeType) {
                case XmlNodeType.Element:
                    if (coreReader.NodeType != XmlNodeType.EndElement && !readAhead) { //will be true for IsDefault cases where we peek only one node ahead
                        int startDepth = coreReader.Depth - 1;
                        while (coreReader.Read() && coreReader.Depth > startDepth) 
                        ;
                    }
                    coreReader.Read();
                    cacheState = CachingReaderState.ReaderClosed;
                    cacheHandler(this);
                    break;
    
                case XmlNodeType.Attribute:
                    MoveToElement();
                    goto case XmlNodeType.Element;

                default:
                    Debug.Assert(cacheState == CachingReaderState.Replay);
                    Read();
                    break;
            }
        }

        // Gets the XmlNameTable associated with this implementation.
        public override XmlNameTable NameTable { 
            get {
                return coreReaderNameTable;
            }
        }

        // Resolves a namespace prefix in the current element's scope.
        public override string LookupNamespace( string prefix) {
            return coreReader.LookupNamespace(prefix);
        }

        // Resolves the entity reference for nodes of NodeType EntityReference.
        public override void ResolveEntity() {
            throw new InvalidOperationException();
        }

        // Parses the attribute value into one or more Text and/or EntityReference node types.
        public override bool ReadAttributeValue() {
            Debug.Assert(cacheState == CachingReaderState.Replay);
            if (cachedNode.NodeType != XmlNodeType.Attribute) {
                return false;
            }
            cachedNode = CreateDummyTextNode(cachedNode.RawValue, cachedNode.Depth + 1);
            return true;
        }

        //
        // IXmlLineInfo members
        //

        bool IXmlLineInfo.HasLineInfo() {
            return true;
        }

        int IXmlLineInfo.LineNumber {
            get {
                return cachedNode.LineNumber;
            }
        }

        int IXmlLineInfo.LinePosition { 
            get {
                return cachedNode.LinePosition;
            }
        }

//Private methods
        internal void SetToReplayMode() {
            cacheState = CachingReaderState.Replay;
            currentContentIndex = 0;
            currentAttrIndex = -1;
            Read(); //Position on first node recorded to begin replaying
        }

        internal XmlReader GetCoreReader() {
            return coreReader;
        }

        internal IXmlLineInfo GetLineInfo() {
            return lineInfo;
        }

        private void ClearAttributesInfo() {
            attributeCount = 0;
            currentAttrIndex = -1;
        }

        private ValidatingReaderNodeData AddAttribute(int attIndex) {
            Debug.Assert(attIndex <= attributeEvents.Length);
            ValidatingReaderNodeData attInfo = attributeEvents[attIndex];
            if (attInfo != null) {
                attInfo.Clear(XmlNodeType.Attribute);
                return attInfo;
            }
            if (attIndex >= attributeEvents.Length -1 ) { //reached capacity of array, Need to increase capacity to twice the initial
                ValidatingReaderNodeData[] newAttributeEvents = new ValidatingReaderNodeData[attributeEvents.Length * 2];
                Array.Copy(attributeEvents, 0, newAttributeEvents, 0, attributeEvents.Length);
                attributeEvents = newAttributeEvents;
            }
            attInfo = attributeEvents[attIndex];
            if (attInfo == null) {
                attInfo = new ValidatingReaderNodeData(XmlNodeType.Attribute);
                attributeEvents[attIndex] = attInfo;
            }
            return attInfo;
        }

        private ValidatingReaderNodeData AddContent(XmlNodeType nodeType) {
            Debug.Assert(contentIndex <= contentEvents.Length);
            ValidatingReaderNodeData contentInfo = contentEvents[contentIndex];
            if (contentInfo != null) {
                contentInfo.Clear(nodeType);
                contentIndex++;
                return contentInfo;
            }
            if (contentIndex >= contentEvents.Length -1 ) { //reached capacity of array, Need to increase capacity to twice the initial
                ValidatingReaderNodeData[] newContentEvents = new ValidatingReaderNodeData[contentEvents.Length * 2];
                Array.Copy(contentEvents, 0, newContentEvents, 0, contentEvents.Length);
                contentEvents = newContentEvents;
            }
            contentInfo = contentEvents[contentIndex];
            if (contentInfo == null) {
                contentInfo = new ValidatingReaderNodeData(nodeType);
                contentEvents[contentIndex] = contentInfo;
            }
            contentIndex++;
            return contentInfo;
        }

        private void RecordAttributes() {
            Debug.Assert(coreReader.NodeType == XmlNodeType.Element);
            ValidatingReaderNodeData attInfo;
            attributeCount = coreReader.AttributeCount;
            if (coreReader.MoveToFirstAttribute()) {
                int attIndex = 0;
                do {
                    attInfo = AddAttribute(attIndex);
                    attInfo.SetItemData(coreReader.LocalName, coreReader.Prefix, coreReader.NamespaceURI, coreReader.Depth);
                    attInfo.SetLineInfo(lineInfo);
                    attInfo.RawValue = coreReader.Value;
                    attIndex++;
                } while (coreReader.MoveToNextAttribute());
                coreReader.MoveToElement();
            }
        }
    
        private int GetAttributeIndexWithoutPrefix(string name) {
            name = coreReaderNameTable.Get(name);
            if ( name == null ) {
                return -1;
            }
            ValidatingReaderNodeData attribute;
            for ( int i = 0; i < attributeCount; i++ ) {
                attribute = attributeEvents[i];
                if ( Ref.Equal(attribute.LocalName, name) && attribute.Prefix.Length == 0 ) {
                    return i;
                }
            }
            return -1;
        }

        private int GetAttributeIndexWithPrefix(string name) {
            name = coreReaderNameTable.Get(name);
            if ( name == null ) {
                return -1;
            }
            ValidatingReaderNodeData attribute;
            for ( int i = 0; i < attributeCount; i++ ) {
                attribute = attributeEvents[i];
                if ( Ref.Equal(attribute.GetAtomizedNameWPrefix(coreReaderNameTable), name) ) {
                    return i;
                }
            }
            return -1;
        }

        private ValidatingReaderNodeData CreateDummyTextNode(string attributeValue, int depth) {
            if (textNode == null) {
                textNode = new ValidatingReaderNodeData(XmlNodeType.Text);
            }
            textNode.Depth = depth;
            textNode.RawValue = attributeValue;
            return textNode;
        }

    }
}
