
//------------------------------------------------------------------------------
// <copyright file="XmlSubtreeReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

namespace System.Xml {

    internal sealed partial class XmlSubtreeReader : XmlWrappingReader, IXmlLineInfo, IXmlNamespaceResolver {

//
// Private types
//
        class NodeData {
            internal XmlNodeType type;
            internal string localName;
            internal string prefix;
            internal string name;
            internal string namespaceUri;
            internal string value;

            internal NodeData() {
            }

            internal void Set( XmlNodeType nodeType, string localName, string prefix, string name, string namespaceUri, string value ) {
                this.type      = nodeType;
                this.localName = localName;
                this.prefix    = prefix;
                this.name      = name;
                this.namespaceUri = namespaceUri;
                this.value     = value;
            }
        }

        enum State {
            Initial      = ReadState.Initial,
            Interactive  = ReadState.Interactive,
            Error        = ReadState.Error,
            EndOfFile    = ReadState.EndOfFile,
            Closed       = ReadState.Closed,
            PopNamespaceScope,
            ClearNsAttributes,
            ReadElementContentAsBase64,
            ReadElementContentAsBinHex,
            ReadContentAsBase64,
            ReadContentAsBinHex,
        }

        const int AttributeActiveStates = 0x62; // 00001100010 bin
        const int NamespaceActiveStates = 0x7E2; // 11111100010 bin

//
// Fields
//
        int              initialDepth;
        State            state;

        // namespace management
        XmlNamespaceManager  nsManager;
        NodeData[]           nsAttributes;
        int                  nsAttrCount;
        int                  curNsAttr = -1;
        
        string               xmlns;
        string               xmlnsUri;

        // incremental reading of added xmlns nodes (ReadValueChunk, ReadContentAsBase64, ReadContentAsBinHex)
        int                  nsIncReadOffset;
        IncrementalReadDecoder binDecoder;

        // cached nodes
        bool                 useCurNode;
        NodeData             curNode;
        // node used for a text node of ReadAttributeValue or as Initial or EOF node
        NodeData             tmpNode;

// 
// Constants
//
        internal int InitialNamespaceAttributeCount = 4;

// 
// Constructor
//
        internal XmlSubtreeReader( XmlReader reader ) : base( reader ) {
            initialDepth = reader.Depth;
            state  = State.Initial;
            nsManager = new XmlNamespaceManager( reader.NameTable );
            xmlns = reader.NameTable.Add( "xmlns" );
            xmlnsUri = reader.NameTable.Add(XmlReservedNs.NsXmlNs);

            tmpNode = new NodeData();
            tmpNode.Set( XmlNodeType.None, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty );

            SetCurrentNode( tmpNode );
        }

//
// XmlReader implementation
//
        public override XmlNodeType NodeType { 
            get { 
                return ( useCurNode ) ? curNode.type : reader.NodeType;
            }
        }

        public override string Name { 
            get { 
                return ( useCurNode ) ? curNode.name : reader.Name;
            }
        }

        public override string LocalName { 
            get { 
                return ( useCurNode ) ? curNode.localName : reader.LocalName;
            } 
        }

        public override string NamespaceURI { 
            get { 
                return ( useCurNode ) ? curNode.namespaceUri : reader.NamespaceURI;
            }
        }

        public override string Prefix { 
            get { 
                return ( useCurNode ) ? curNode.prefix : reader.Prefix;
            } 
        }

        public override string Value { 
            get { 
                return ( useCurNode ) ? curNode.value : reader.Value;
            } 
        }

        public override int Depth { 
            get {
                int depth = reader.Depth - initialDepth;
                if ( curNsAttr != -1 ) {
                    if ( curNode.type == XmlNodeType.Text ) { // we are on namespace attribute value
                        depth += 2;
                    }
                    else {
                        depth++;
                    }
                }
                return depth;
            }
        }

        public override string BaseURI {
            get {
                return reader.BaseURI;
            }
        }

        public override bool IsEmptyElement { 
            get { 
                return reader.IsEmptyElement; 
            }
        }

        public override bool EOF { 
            get {
                return state == State.EndOfFile || state == State.Closed;
            } 
        }

        public override ReadState ReadState { 
            get { 
                if ( reader.ReadState == ReadState.Error ) {
                    return ReadState.Error;
                }
                else {
                    if ( (int)state <= (int)State.Closed ) {
                        return (ReadState)(int)state;
                    }
                    else {
                        return ReadState.Interactive;
                    }
                }
            } 
        }

        public override XmlNameTable NameTable { 
            get { 
                return reader.NameTable;
            } 
        }

        public override int AttributeCount { 
            get {
                return InAttributeActiveState ? reader.AttributeCount + nsAttrCount : 0;
            } 
        }

        public override string GetAttribute( string name ) {
            if (!InAttributeActiveState) {
                return null;
            }
            string attr = reader.GetAttribute( name );
            if ( attr != null ) {
                return attr;
            }
            for ( int i = 0; i < nsAttrCount; i++ ) {
                if ( name == nsAttributes[i].name ) {
                    return nsAttributes[i].value;
                }
            }
            return null;
        }

        public override string GetAttribute( string name, string namespaceURI ) {
            if (!InAttributeActiveState) {
                return null;
            }
            string attr = reader.GetAttribute( name, namespaceURI );
            if ( attr != null ) {
                return attr;
            }
            for ( int i = 0; i < nsAttrCount; i++ ) {
                if ( name == nsAttributes[i].localName && namespaceURI == xmlnsUri ) {
                    return nsAttributes[i].value;
                }
            }
            return null;
        }

        public override string GetAttribute( int i ) {
            if ( !InAttributeActiveState ) {
                throw new ArgumentOutOfRangeException("i");
            }
            int n = reader.AttributeCount;
            if ( i < n ) {
                return reader.GetAttribute( i );
            }
            else if ( i - n < nsAttrCount ) {
                return nsAttributes[i-n].value;
            }
            else {
                throw new ArgumentOutOfRangeException( "i" );
            }
        }

        public override bool MoveToAttribute( string name ) {
            if ( !InAttributeActiveState ) {
                return false;
            }
            if ( reader.MoveToAttribute( name ) ) {
                curNsAttr = -1;
                useCurNode = false;
                return true;
            }
            for ( int i = 0; i < nsAttrCount; i++ ) {
                if ( name == nsAttributes[i].name ) {
                    MoveToNsAttribute( i );
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToAttribute( string name, string ns ) {
            if ( !InAttributeActiveState ) {
                return false;
            }
            if ( reader.MoveToAttribute( name, ns ) ) {
                curNsAttr = -1;
                useCurNode = false;
                return true;
            }
            for ( int i = 0; i < nsAttrCount; i++ ) {
                if ( name == nsAttributes[i].localName && ns == xmlnsUri ) {
                    MoveToNsAttribute( i );
                    return true;
                }
            }
            return false;
        }

        public override void MoveToAttribute( int i ) {
            if ( !InAttributeActiveState ) {
                throw new ArgumentOutOfRangeException("i");
            }
            int n = reader.AttributeCount;
            if ( i < n ) {
                reader.MoveToAttribute( i );
                curNsAttr = -1;
                useCurNode = false;
            }
            else if ( i - n < nsAttrCount ) {
                MoveToNsAttribute( i - n );
            }
            else {
                throw new ArgumentOutOfRangeException( "i" );
            }
        }

        public override bool MoveToFirstAttribute() {
            if ( !InAttributeActiveState ) {
                return false;
            }
            if ( reader.MoveToFirstAttribute() ) {
                useCurNode = false;
                return true;
            }
            if ( nsAttrCount > 0 ) {
                MoveToNsAttribute( 0 );
                return true;
            }
            return false;
        }

        public override bool MoveToNextAttribute() {
            if ( !InAttributeActiveState ) {
                return false;
            }
            if ( curNsAttr == -1 && reader.MoveToNextAttribute() ) {
                return true;
            }
            if ( curNsAttr + 1 < nsAttrCount ) {
                MoveToNsAttribute( curNsAttr + 1 );
                return true;
            }
            return false;
        }

        public override bool MoveToElement() {
            if ( !InAttributeActiveState ) {
                return false;
            }

            useCurNode = false;
            //If on Namespace attribute, the base reader is already on Element node.
            if (curNsAttr >= 0) {
                curNsAttr = -1;
                Debug.Assert(reader.NodeType == XmlNodeType.Element);
                return true;
            }
            else {
                return reader.MoveToElement();
            }
        }

        public override bool ReadAttributeValue() {
            if ( !InAttributeActiveState ) {
                return false;
            }
            if ( curNsAttr == -1 ) {
                return reader.ReadAttributeValue();
            }
            else if ( curNode.type == XmlNodeType.Text ) { // we are on namespace attribute value
                return false;
            }
            else {
                Debug.Assert( curNode.type == XmlNodeType.Attribute );
                tmpNode.type = XmlNodeType.Text;
                tmpNode.value = curNode.value;
                SetCurrentNode( tmpNode );
                return true;
            }
        }

        public override  bool  Read() {
            switch ( state ) {
                case State.Initial:
                    useCurNode = false;
                    state = State.Interactive;
                    ProcessNamespaces();
                    return true;

                case State.Interactive:
                    curNsAttr = -1;
                    useCurNode = false;
                    reader.MoveToElement();
                    Debug.Assert( reader.Depth >= initialDepth );
                    if ( reader.Depth == initialDepth ) {
                        if ( reader.NodeType == XmlNodeType.EndElement || 
                            ( reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement ) ) {
                            state = State.EndOfFile;
                            SetEmptyNode();
                            return false;
                        }
                        Debug.Assert( reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement );
                    }
                    if ( reader.Read() ) {
                        ProcessNamespaces();
                        return true;
                    }
                    else {
                        SetEmptyNode();
                        return false;
                    }

                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return false;

                case State.PopNamespaceScope:
                    nsManager.PopScope();
                    goto case State.ClearNsAttributes;

                case State.ClearNsAttributes:
                    nsAttrCount = 0;
                    state = State.Interactive;
                    goto case State.Interactive;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    if ( !FinishReadElementContentAsBinary() ) {
                        return false;
                    }
                    return Read();

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    if ( !FinishReadContentAsBinary() ) {
                        return false;
                    }
                    return Read();

                default:
                    Debug.Assert( false );
                    return false;
            }
        }

        public override void Close() {
            if ( state == State.Closed) {
                return;
            }
            try {
                // move the underlying reader to the next sibling
                if (state != State.EndOfFile) {
                    reader.MoveToElement();
                    Debug.Assert( reader.Depth >= initialDepth );
                    // move off the root of the subtree
                    if (reader.Depth == initialDepth && reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement) {
                        reader.Read();
                    }
                    // move to the end of the subtree, do nothing if on empty root element
                    while (reader.Depth > initialDepth && reader.Read()) {
                        /* intentionally empty */
                    }
                }
            }
            catch { // never fail...
            }
            finally {
                curNsAttr = -1;
                useCurNode = false;
                state = State.Closed;
                SetEmptyNode();
            }
        }

        public override void Skip() {
            switch ( state ) {
                case State.Initial:
                    Read();
                    return;

                case State.Interactive:
                    curNsAttr = -1;
                    useCurNode = false;
                    reader.MoveToElement();
                    Debug.Assert( reader.Depth >= initialDepth );
                    if ( reader.Depth == initialDepth ) {
                        if ( reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement ) {
                            // we are on root of the subtree -> skip to the end element and set to Eof state
                            if ( reader.Read() ) {
                                while ( reader.NodeType != XmlNodeType.EndElement && reader.Depth > initialDepth ) {
                                    reader.Skip();
                                }
                            }
                        }
                        Debug.Assert( reader.NodeType == XmlNodeType.EndElement || 
                                      reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement ||
                                      reader.ReadState != ReadState.Interactive );
                        state = State.EndOfFile;
                        SetEmptyNode();
                        return;
                    }

                    if ( reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement ) {
                        nsManager.PopScope();
                    }
                    reader.Skip();
                    ProcessNamespaces();

                    Debug.Assert( reader.Depth >= initialDepth );
                    return;

                case State.Closed:
                case State.EndOfFile:
                    return;

                case State.PopNamespaceScope:
                    nsManager.PopScope();
                    goto case State.ClearNsAttributes;

                case State.ClearNsAttributes:
                    nsAttrCount = 0;
                    state = State.Interactive;
                    goto case State.Interactive;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    if ( FinishReadElementContentAsBinary() ) {
                        Skip();
                    }
                    break;

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    if ( FinishReadContentAsBinary() ) {
                        Skip();
                    }
                    break;

                case State.Error:
                    return;

                default:
                    Debug.Assert( false );
                    return;
            }
        }

        public override  object  ReadContentAsObject() {
            try {
                InitReadContentAsType( "ReadContentAsObject" );
                object value = reader.ReadContentAsObject();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  bool  ReadContentAsBoolean() {
            try {
                InitReadContentAsType( "ReadContentAsBoolean" );
                bool value = reader.ReadContentAsBoolean();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  DateTime  ReadContentAsDateTime() {
            try {
                InitReadContentAsType( "ReadContentAsDateTime" );
                DateTime value = reader.ReadContentAsDateTime();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  double  ReadContentAsDouble() {
            try {
                InitReadContentAsType( "ReadContentAsDouble" );
                double value = reader.ReadContentAsDouble();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  float  ReadContentAsFloat() {
            try {
                InitReadContentAsType( "ReadContentAsFloat" );
                float value = reader.ReadContentAsFloat();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  decimal  ReadContentAsDecimal() {
            try {
                InitReadContentAsType( "ReadContentAsDecimal" );
                decimal value = reader.ReadContentAsDecimal();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  int  ReadContentAsInt() {
            try {
                InitReadContentAsType( "ReadContentAsInt" );
                int value = reader.ReadContentAsInt();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  long  ReadContentAsLong() {
            try {
                InitReadContentAsType( "ReadContentAsLong" );
                long value = reader.ReadContentAsLong();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  string  ReadContentAsString() {
            try {
                InitReadContentAsType( "ReadContentAsString" );
                string value = reader.ReadContentAsString();
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override  object  ReadContentAs( Type returnType, IXmlNamespaceResolver namespaceResolver ) {
            try {
                InitReadContentAsType( "ReadContentAs" );
                object value = reader.ReadContentAs( returnType, namespaceResolver );
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override bool CanReadBinaryContent {
            get {
                return reader.CanReadBinaryContent;
            }
        }

        public override  int  ReadContentAsBase64( byte[] buffer, int index, int count ) {
            switch ( state ) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.ClearNsAttributes:
                case State.PopNamespaceScope:
                    switch ( NodeType ) {
                        case XmlNodeType.Element:
                            throw CreateReadContentAsException( "ReadContentAsBase64" );
                        case XmlNodeType.EndElement:
                            return 0;
                        case XmlNodeType.Attribute:
                            if ( curNsAttr != -1 && reader.CanReadBinaryContent ) {
                                CheckBuffer( buffer, index, count );
                                if ( count == 0 ) {
                                    return 0;
                                }
                                if ( nsIncReadOffset == 0 ) {
                                    // called first time on this ns attribute
                                    if ( binDecoder != null && binDecoder is Base64Decoder ) {
                                        binDecoder.Reset();
                                    }
                                    else {
                                        binDecoder = new Base64Decoder();
                                    }
                                }
                                if ( nsIncReadOffset == curNode.value.Length ) {
                                    return 0;
                                }
                                binDecoder.SetNextOutputBuffer( buffer, index, count );
                                nsIncReadOffset += binDecoder.Decode( curNode.value, nsIncReadOffset, curNode.value.Length - nsIncReadOffset );
                                return binDecoder.DecodedCount;
                            }
                            goto case XmlNodeType.Text;
                        case XmlNodeType.Text:
                            Debug.Assert( AttributeCount > 0 );
                            return reader.ReadContentAsBase64( buffer, index, count );
                        default:
                            Debug.Assert( false );
                            return 0;
                    }

                case State.Interactive:
                    state = State.ReadContentAsBase64;
                    goto case State.ReadContentAsBase64;

                case State.ReadContentAsBase64:
                    int read = reader.ReadContentAsBase64( buffer, index, count );
                    if ( read == 0 ) {
                        state = State.Interactive;
                        ProcessNamespaces();
                    }
                    return read;

                case State.ReadContentAsBinHex:
                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );

                default:
                    Debug.Assert( false );
                    return 0;
            }
        }

        public override  int  ReadElementContentAsBase64( byte[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if ( !InitReadElementContentAsBinary( State.ReadElementContentAsBase64 ) ) {
                        return 0;
                    }
                    goto case State.ReadElementContentAsBase64;

                case State.ReadElementContentAsBase64:
                    int read = reader.ReadContentAsBase64( buffer, index, count );
                    if ( read > 0 || count == 0 ) {
                        return read;
                    }
                    if ( NodeType != XmlNodeType.EndElement ) {
                        throw new XmlException(Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo);
                    }

                    // pop namespace scope
                    state = State.Interactive;
                    ProcessNamespaces();

                    // set eof state or move off the end element
                    if ( reader.Depth == initialDepth ) {
                        state = State.EndOfFile;
                        SetEmptyNode();
                    }
                    else {
                        Read();
                    }
                    return 0;

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                case State.ReadElementContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );

                default:
                    Debug.Assert( false );
                    return 0;
            }
        }

        public override  int  ReadContentAsBinHex( byte[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.ClearNsAttributes:
                case State.PopNamespaceScope:
                    switch ( NodeType ) {
                        case XmlNodeType.Element:
                            throw CreateReadContentAsException( "ReadContentAsBinHex" );
                        case XmlNodeType.EndElement:
                            return 0;
                        case XmlNodeType.Attribute:
                            if (curNsAttr != -1 && reader.CanReadBinaryContent) {
                                CheckBuffer( buffer, index, count );
                                if ( count == 0 ) {
                                    return 0;
                                }
                                if ( nsIncReadOffset == 0 ) {
                                    // called first time on this ns attribute
                                    if ( binDecoder != null && binDecoder is BinHexDecoder ) {
                                        binDecoder.Reset();
                                    }
                                    else {
                                        binDecoder = new BinHexDecoder();
                                    }
                                }
                                if ( nsIncReadOffset == curNode.value.Length ) {
                                    return 0;
                                }
                                binDecoder.SetNextOutputBuffer( buffer, index, count );
                                nsIncReadOffset += binDecoder.Decode( curNode.value, nsIncReadOffset, curNode.value.Length - nsIncReadOffset );
                                return binDecoder.DecodedCount;                            }
                            goto case XmlNodeType.Text;
                        case XmlNodeType.Text:
                            Debug.Assert( AttributeCount > 0 );
                            return reader.ReadContentAsBinHex( buffer, index, count );
                        default:
                            Debug.Assert( false );
                            return 0;
                    }

                case State.Interactive:
                    state = State.ReadContentAsBinHex;
                    goto case State.ReadContentAsBinHex;

                case State.ReadContentAsBinHex:
                    int read = reader.ReadContentAsBinHex( buffer, index, count );
                    if ( read == 0 ) {
                        state = State.Interactive;
                        ProcessNamespaces();
                    }
                    return read;

                case State.ReadContentAsBase64:
                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );

                default:
                    Debug.Assert( false );
                    return 0;
            }
        }

        public override  int  ReadElementContentAsBinHex( byte[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if ( !InitReadElementContentAsBinary( State.ReadElementContentAsBinHex ) ) {
                        return 0;
                    }
                    goto case State.ReadElementContentAsBinHex;
                case State.ReadElementContentAsBinHex:
                    int read = reader.ReadContentAsBinHex( buffer, index, count );
                    if ( read > 0  || count == 0 ) {
                        return read;
                    }
                    if ( NodeType != XmlNodeType.EndElement ) {
                        throw new XmlException(Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo);
                    }

                    // pop namespace scope
                    state = State.Interactive;
                    ProcessNamespaces();

                    // set eof state or move off the end element
                    if ( reader.Depth == initialDepth ) {
                        state = State.EndOfFile;
                        SetEmptyNode();
                    }
                    else {
                        Read();
                    }
                    return 0;

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                case State.ReadElementContentAsBase64:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );

                default:
                    Debug.Assert( false );
                    return 0;
            }
        }

        public override bool CanReadValueChunk {
            get {
                return reader.CanReadValueChunk;
            }
        }

        public override  int  ReadValueChunk( char[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.ClearNsAttributes:
                case State.PopNamespaceScope:
                    // ReadValueChunk implementation on added xmlns attributes
                    if (curNsAttr != -1 && reader.CanReadValueChunk) {
                        CheckBuffer( buffer, index, count );
                        int copyCount = curNode.value.Length - nsIncReadOffset;
                        if ( copyCount > count ) {
                            copyCount = count;
                        }
                        if ( copyCount > 0 ) {
                            curNode.value.CopyTo( nsIncReadOffset, buffer, index, copyCount );
                        }
                        nsIncReadOffset += copyCount;
                        return copyCount;
                    }
                    // Otherwise fall back to the case State.Interactive.
                    // No need to clean ns attributes or pop scope because the reader when ReadValueChunk is called
                    // - on Element errors
                    // - on EndElement errors
                    // - on Attribute does not move
                    // and that's all where State.ClearNsAttributes or State.PopnamespaceScope can be set
                    goto case State.Interactive;

                case State.Interactive:
                    return reader.ReadValueChunk( buffer, index, count );

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingReadValueChunkWithBinary ) );

                default:
                    Debug.Assert( false );
                    return 0;
            }
        }

        public override string LookupNamespace(string prefix) {
            return ((IXmlNamespaceResolver)this).LookupNamespace(prefix);
        }

//
// IDisposable interface
//
        protected override void Dispose( bool disposing ) {
            // note: we do not want to dispose the underlying reader
            this.Close();
        }

//
// IXmlLineInfo implementation
//
        int IXmlLineInfo.LineNumber {
            get {
                if ( !useCurNode ) {
                    IXmlLineInfo lineInfo = reader as IXmlLineInfo;
                    if ( lineInfo != null ) {
                        return lineInfo.LineNumber;
                    }
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition { 
            get {
                if ( !useCurNode ) {
                    IXmlLineInfo lineInfo = reader as IXmlLineInfo;
                    if ( lineInfo != null ) {
                        return lineInfo.LinePosition;
                    }
                }
                return 0;
            }
        }

        bool IXmlLineInfo.HasLineInfo() {
            return reader is IXmlLineInfo;
        }

//
// IXmlNamespaceResolver implementation
//
        IDictionary<string,string> IXmlNamespaceResolver.GetNamespacesInScope( XmlNamespaceScope scope ) {
            if (!InNamespaceActiveState) {
                return new Dictionary<string, string>();
            }
            return nsManager.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace( string prefix ) {
            if (!InNamespaceActiveState) {
                return null;
            }
            return nsManager.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix( string namespaceName ) {
            if (!InNamespaceActiveState) {
                return null;
            }
            return nsManager.LookupPrefix(namespaceName);
        }

// 
// Private methods
//
        private void ProcessNamespaces() {
            switch ( reader.NodeType ) {
                case XmlNodeType.Element:
                    nsManager.PushScope();

                    string prefix = reader.Prefix;
                    string ns = reader.NamespaceURI;
                    if ( nsManager.LookupNamespace( prefix ) != ns ) {
                        AddNamespace( prefix, ns );
                    }

                    if ( reader.MoveToFirstAttribute() ) {
                        do {
                            prefix = reader.Prefix;
                            ns = reader.NamespaceURI;

                            if ( Ref.Equal( ns, xmlnsUri ) ) {
                                if ( prefix.Length == 0 ) {
                                    nsManager.AddNamespace( string.Empty, reader.Value );
                                    RemoveNamespace( string.Empty, xmlns );
                                }
                                else {
                                    prefix = reader.LocalName;
                                    nsManager.AddNamespace( prefix, reader.Value );
                                    RemoveNamespace( xmlns, prefix );
                                }
                            }
                            else if ( prefix.Length != 0 && nsManager.LookupNamespace( prefix ) != ns ) {
                                AddNamespace( prefix, ns );
                            }
                        } while ( reader.MoveToNextAttribute() );
                        reader.MoveToElement();
                    }

                    if ( reader.IsEmptyElement ) {
                        state = State.PopNamespaceScope;
                    }
                    break;
                case XmlNodeType.EndElement:
                    state = State.PopNamespaceScope;
                    break;
            }
        }

        private void AddNamespace( string prefix, string ns ) {
            nsManager.AddNamespace( prefix, ns );

            int index = nsAttrCount++;
            if ( nsAttributes == null ) {
                nsAttributes = new NodeData[InitialNamespaceAttributeCount];
            }
            if ( index == nsAttributes.Length ) {
                NodeData[] newNsAttrs = new NodeData[nsAttributes.Length * 2];
                Array.Copy( nsAttributes, 0, newNsAttrs, 0, index );
                nsAttributes = newNsAttrs;
            }

            if ( nsAttributes[index] == null ) {
                nsAttributes[index] = new NodeData();
            }
            if ( prefix.Length == 0 ) {
                nsAttributes[index].Set( XmlNodeType.Attribute, xmlns, string.Empty, xmlns, xmlnsUri, ns );
            }
            else {
                nsAttributes[index].Set( XmlNodeType.Attribute, prefix, xmlns, reader.NameTable.Add( string.Concat( xmlns, ":", prefix ) ), xmlnsUri, ns );
            }

            Debug.Assert( state == State.ClearNsAttributes || state == State.Interactive || state == State.PopNamespaceScope );
            state = State.ClearNsAttributes;

            curNsAttr = -1;
        }

        private void RemoveNamespace( string prefix, string localName ) {
            for ( int i = 0; i < nsAttrCount; i++ ) {
                if ( Ref.Equal( prefix, nsAttributes[i].prefix ) &&
                     Ref.Equal( localName, nsAttributes[i].localName ) ) {
                         if ( i < nsAttrCount - 1 ) {
                             // swap
                             NodeData tmpNodeData = nsAttributes[i];
                             nsAttributes[i] = nsAttributes[nsAttrCount - 1];
                             nsAttributes[nsAttrCount - 1] = tmpNodeData;
                         }
                         nsAttrCount--;
                         break;
                 }
            }
        }

        private void MoveToNsAttribute( int index ) {
            Debug.Assert( index >= 0 && index <= nsAttrCount );
            reader.MoveToElement();
            curNsAttr = index;
            nsIncReadOffset = 0;
            SetCurrentNode( nsAttributes[index] );
        }

        private  bool  InitReadElementContentAsBinary( State binaryState ) {
            if ( NodeType != XmlNodeType.Element ) {
                throw reader.CreateReadElementContentAsException( "ReadElementContentAsBase64" );
            }

            bool isEmpty = IsEmptyElement;

            // move to content or off the empty element
            if ( !Read() || isEmpty ) {
                return false;
            }
            // special-case child element and end element
            switch ( NodeType ) {
                case XmlNodeType.Element:
                    throw new XmlException(Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo);
                case XmlNodeType.EndElement:
                    // pop scope & move off end element
                    ProcessNamespaces();
                    Read();
                    return false;
            }
 
            Debug.Assert( state == State.Interactive );
            state = binaryState;
            return true;
        }

        private  bool  FinishReadElementContentAsBinary() {
            Debug.Assert( state == State.ReadElementContentAsBase64 || state == State.ReadElementContentAsBinHex );

            byte[] bytes = new byte[256];
            if ( state == State.ReadElementContentAsBase64 ) {
                while ( reader.ReadContentAsBase64( bytes, 0, 256 ) > 0 ) ;
            }
            else {
                while ( reader.ReadContentAsBinHex( bytes, 0, 256 ) > 0 ) ;
            }

            if ( NodeType != XmlNodeType.EndElement ) {
                throw new XmlException(Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo);
            }

            // pop namespace scope
            state = State.Interactive;
            ProcessNamespaces();

            // check eof
            if ( reader.Depth == initialDepth ) {
                 state = State.EndOfFile;
                 SetEmptyNode();
                 return false;
            }
            // move off end element
            return Read();
        }

        private  bool  FinishReadContentAsBinary() {
            Debug.Assert( state == State.ReadContentAsBase64 || state == State.ReadContentAsBinHex );

            byte[] bytes = new byte[256];
            if ( state == State.ReadContentAsBase64 ) {
                while ( reader.ReadContentAsBase64( bytes, 0, 256 ) > 0 ) ;
            }
            else {
                while ( reader.ReadContentAsBinHex( bytes, 0, 256 ) > 0 ) ;
            }

            state = State.Interactive;
            ProcessNamespaces();

            // check eof
            if ( reader.Depth == initialDepth ) {
                 state = State.EndOfFile;
                 SetEmptyNode();
                 return false;
            }
            return true;
        }

        private bool InAttributeActiveState {
            get {
#if DEBUG
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.Initial ) ) );
                Debug.Assert( 0 != ( AttributeActiveStates & ( 1 << (int)State.Interactive ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.Error ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.EndOfFile ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.Closed ) ) );
                Debug.Assert( 0 != ( AttributeActiveStates & ( 1 << (int)State.PopNamespaceScope ) ) );
                Debug.Assert( 0 != ( AttributeActiveStates & ( 1 << (int)State.ClearNsAttributes ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.ReadElementContentAsBase64 ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.ReadElementContentAsBinHex ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.ReadContentAsBase64 ) ) );
                Debug.Assert( 0 == ( AttributeActiveStates & ( 1 << (int)State.ReadContentAsBinHex ) ) );
#endif
                return 0 != ( AttributeActiveStates & ( 1 << (int)state ) );
            }
        }

        private bool InNamespaceActiveState {
            get {
#if DEBUG
                Debug.Assert( 0 == ( NamespaceActiveStates & ( 1 << (int)State.Initial ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.Interactive ) ) );
                Debug.Assert( 0 == ( NamespaceActiveStates & ( 1 << (int)State.Error ) ) );
                Debug.Assert( 0 == ( NamespaceActiveStates & ( 1 << (int)State.EndOfFile ) ) );
                Debug.Assert( 0 == ( NamespaceActiveStates & ( 1 << (int)State.Closed ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.PopNamespaceScope ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.ClearNsAttributes ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.ReadElementContentAsBase64 ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.ReadElementContentAsBinHex ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.ReadContentAsBase64 ) ) );
                Debug.Assert( 0 != ( NamespaceActiveStates & ( 1 << (int)State.ReadContentAsBinHex ) ) );
#endif
                return 0 != ( NamespaceActiveStates & ( 1 << (int)state ) );
            }
        }

        void SetEmptyNode() {
            Debug.Assert( tmpNode.localName == string.Empty && tmpNode.prefix == string.Empty && tmpNode.name == string.Empty && tmpNode.namespaceUri == string.Empty );
            tmpNode.type = XmlNodeType.None;
            tmpNode.value = string.Empty;

            curNode = tmpNode;
            useCurNode = true;
        }

        void SetCurrentNode( NodeData node ) {
            curNode = node;
            useCurNode = true;
        }

        void InitReadContentAsType( string methodName ) {
            switch ( state ) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_ClosedOrErrorReader ) );

                case State.Interactive:
                    return;

                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    // no need to clean ns attributes or pop scope because the reader when ReadContentAs is called
                    // - on Element errors
                    // - on Attribute does not move
                    // - on EndElement does not move
                    // and that's all where State.ClearNsAttributes or State.PopNamespacScope can be set
                    return;

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingReadValueChunkWithBinary ) );

                default:
                    Debug.Assert( false );
                    break;
            }
            throw CreateReadContentAsException( methodName );
        }

        void FinishReadContentAsType() {
            Debug.Assert( state == State.Interactive ||
                          state == State.PopNamespaceScope ||
                          state == State.ClearNsAttributes );

            switch ( NodeType ) {
                case XmlNodeType.Element:
                    // new element we moved to - process namespaces
                    ProcessNamespaces();
                    break;
                case XmlNodeType.EndElement:
                    // end element we've stayed on or have been moved to
                    state = State.PopNamespaceScope;
                    break;
                case XmlNodeType.Attribute:
                    // stayed on attribute, do nothing
                    break;
            }
        }

        void CheckBuffer( Array buffer, int index, int count ) {
            if ( buffer == null ) {
                throw new ArgumentNullException( "buffer" );
            }
            if ( count < 0 ) {
                throw new ArgumentOutOfRangeException( "count" );
            }
            if ( index < 0 ) {
                throw new ArgumentOutOfRangeException( "index" );
            }
            if ( buffer.Length - index < count ) {
                throw new ArgumentOutOfRangeException( "count" );
            }
        }

    }
}

