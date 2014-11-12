
//------------------------------------------------------------------------------
// <copyright file="XmlTextReaderHelpers.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Security;
#if !SILVERLIGHT
using System.Xml.Schema;
#endif
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.Versioning;

#if SILVERLIGHT
using BufferBuilder=System.Xml.BufferBuilder;
#else
using BufferBuilder = System.Text.StringBuilder;
#endif

namespace System.Xml {

    internal partial class XmlTextReaderImpl  {

//
// ParsingState
//
        // Parsing state (aka. scanner data) - holds parsing buffer and entity input data information
        private struct ParsingState {
            // character buffer
            internal char[] chars;
            internal int    charPos;
            internal int    charsUsed;
            internal Encoding encoding;
            internal bool   appendMode;
            
            // input stream & byte buffer
            internal Stream  stream;
            internal Decoder decoder;
            internal byte[]  bytes;
            internal int     bytePos;
            internal int     bytesUsed;

            // input text reader
            internal TextReader textReader;

            // current line number & position
            internal int   lineNo;
            internal int   lineStartPos;

            // base uri of the current entity
            internal string baseUriStr;
            internal Uri    baseUri;

            // eof flag of the entity
            internal bool   isEof;
            internal bool   isStreamEof;

            // entity type & id
            internal IDtdEntityInfo entity;
            internal int            entityId;

            // normalization
            internal bool   eolNormalized;

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
            // EndEntity reporting
            internal bool   entityResolvedManually;
#endif

            internal void Clear() {
                chars = null;
                charPos = 0;
                charsUsed = 0;
                encoding = null;
                stream = null;
                decoder = null;
                bytes = null;
                bytePos = 0;
                bytesUsed = 0;
                textReader = null;
                lineNo = 1;
                lineStartPos = -1;
                baseUriStr = string.Empty;
                baseUri = null;
                isEof = false;
                isStreamEof = false;
                eolNormalized = true;
#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                entityResolvedManually = false;
#endif
            }

            internal void Close( bool closeInput ) {
                if ( closeInput ) {
                    if ( stream != null ) {
                        stream.Close();
                    }
                    else if ( textReader != null ) {
                        textReader.Close();
                    }
                }
            }

            internal int LineNo {
                get {
                    return lineNo;
                }
            }

            internal int LinePos {
                get {
                    return charPos - lineStartPos;
                }
            }
        }

//
// XmlContext
//
        private class XmlContext  {
            internal XmlSpace xmlSpace;
            internal string xmlLang;
            internal string defaultNamespace;
            internal XmlContext previousContext;

            internal XmlContext() {
                xmlSpace = XmlSpace.None;
                xmlLang = string.Empty;
                defaultNamespace = string.Empty;
                previousContext = null;
            }

            internal XmlContext( XmlContext previousContext ) {
                this.xmlSpace = previousContext.xmlSpace;
                this.xmlLang = previousContext.xmlLang;
                this.defaultNamespace = previousContext.defaultNamespace;
                this.previousContext = previousContext;
            }
        }

#if !SILVERLIGHT // Needed only for XmlTextReader
//
// NoNamespaceManager
//
        private class NoNamespaceManager : XmlNamespaceManager {    
            public NoNamespaceManager() : base () {}
            public override string DefaultNamespace { get { return string.Empty; } }
            public override void PushScope() {}
            public override bool PopScope() { return false; }
            public override void AddNamespace( string prefix, string uri ) {}
            public override void RemoveNamespace( string prefix, string uri ) {}
            public override IEnumerator GetEnumerator() { return null; }
            public override IDictionary<string,string> GetNamespacesInScope( XmlNamespaceScope scope ) { return null; }
            public override string LookupNamespace( string prefix ) { return string.Empty; }
            public override string LookupPrefix( string uri ) { return null; }
            public override bool HasNamespace( string prefix ) { return false; }
        }
#endif

//
// DtdParserProxy: IDtdParserAdapter proxy for XmlTextReaderImpl
//
#if SILVERLIGHT
        internal partial class DtdParserProxy : IDtdParserAdapter {
#else
        internal partial class DtdParserProxy : IDtdParserAdapterV1 {
#endif

            // Fields
            private XmlTextReaderImpl reader;

        // Constructors
            internal DtdParserProxy( XmlTextReaderImpl reader ) {
                this.reader = reader;
            }

        // IDtdParserAdapter proxies
            XmlNameTable IDtdParserAdapter.NameTable { 
                get { return reader.DtdParserProxy_NameTable; } 
            }

            IXmlNamespaceResolver IDtdParserAdapter.NamespaceResolver {
                get { return reader.DtdParserProxy_NamespaceResolver; }
            }
            
            Uri IDtdParserAdapter.BaseUri {
                // SxS: DtdParserProxy_BaseUri property on the reader may expose machine scope resources. This property
                // is just returning the value of the other property, so it may expose machine scope resource as well.
#if !SILVERLIGHT
                [ResourceConsumption(ResourceScope.Machine)]
                [ResourceExposure(ResourceScope.Machine)]
#endif
                get { return reader.DtdParserProxy_BaseUri; } 
            }
            
            bool IDtdParserAdapter.IsEof { 
                get { return reader.DtdParserProxy_IsEof; } 
            }
            
            char[] IDtdParserAdapter.ParsingBuffer { 
                get { return reader.DtdParserProxy_ParsingBuffer; } 
            }
            
            int IDtdParserAdapter.ParsingBufferLength { 
                get { return reader.DtdParserProxy_ParsingBufferLength; } 
            }
            
            int IDtdParserAdapter.CurrentPosition { 
                get { return reader.DtdParserProxy_CurrentPosition; } 
                set { reader.DtdParserProxy_CurrentPosition = value; } 
            }

            int IDtdParserAdapter.EntityStackLength { 
                get { return reader.DtdParserProxy_EntityStackLength; } 
            }
            
            bool IDtdParserAdapter.IsEntityEolNormalized { 
                get { return reader.DtdParserProxy_IsEntityEolNormalized; } 
            }
            
            void IDtdParserAdapter.OnNewLine( int pos ) { 
                reader.DtdParserProxy_OnNewLine( pos ); 
            }
            
            int IDtdParserAdapter.LineNo { 
                get { return reader.DtdParserProxy_LineNo; } 
            }
            
            int IDtdParserAdapter.LineStartPosition { 
                get { return reader.DtdParserProxy_LineStartPosition; } 
            }

			 int  IDtdParserAdapter.ReadData() { 
                return reader.DtdParserProxy_ReadData(); 
            }

             int  IDtdParserAdapter.ParseNumericCharRef( BufferBuilder internalSubsetBuilder ) { 
                return reader.DtdParserProxy_ParseNumericCharRef( internalSubsetBuilder ); 
            }
            
             int  IDtdParserAdapter.ParseNamedCharRef( bool expand, BufferBuilder internalSubsetBuilder ) { 
                return reader.DtdParserProxy_ParseNamedCharRef( expand, internalSubsetBuilder ); 
            }
            
            void IDtdParserAdapter.ParsePI( BufferBuilder sb ) { 
                reader.DtdParserProxy_ParsePI( sb ); 
            }
            
            void IDtdParserAdapter.ParseComment( BufferBuilder sb ) { 
                reader.DtdParserProxy_ParseComment( sb ); 
            }
            
             bool  IDtdParserAdapter.PushEntity( IDtdEntityInfo entity, out int entityId ) { 

                return reader.DtdParserProxy_PushEntity( entity, out entityId ); 

            }

			bool IDtdParserAdapter.PopEntity( out IDtdEntityInfo oldEntity, out int newEntityId ) { 
                return reader.DtdParserProxy_PopEntity( out oldEntity, out newEntityId ); 
            }

             bool  IDtdParserAdapter.PushExternalSubset( string systemId, string publicId ) { 
                return reader.DtdParserProxy_PushExternalSubset( systemId, publicId ); 
            }

			void IDtdParserAdapter.PushInternalDtd( string baseUri, string internalDtd ) {
                Debug.Assert(internalDtd != null);
                reader.DtdParserProxy_PushInternalDtd( baseUri, internalDtd ); 
            }

            void IDtdParserAdapter.Throw( Exception e ) { 
                reader.DtdParserProxy_Throw( e ); 
            }
            
            void IDtdParserAdapter.OnSystemId( string systemId, LineInfo keywordLineInfo, LineInfo systemLiteralLineInfo ) { 
                reader.DtdParserProxy_OnSystemId( systemId, keywordLineInfo, systemLiteralLineInfo ); 
            }

            void IDtdParserAdapter.OnPublicId( string publicId, LineInfo keywordLineInfo, LineInfo publicLiteralLineInfo ) { 
                reader.DtdParserProxy_OnPublicId( publicId, keywordLineInfo, publicLiteralLineInfo ); 
            }

#if !SILVERLIGHT
            bool IDtdParserAdapterWithValidation.DtdValidation { 
                get { return reader.DtdParserProxy_DtdValidation; } 
            }

            IValidationEventHandling IDtdParserAdapterWithValidation.ValidationEventHandling {
                get { return reader.DtdParserProxy_ValidationEventHandling; }
            }

            bool IDtdParserAdapterV1.Normalization { 
                get { return reader.DtdParserProxy_Normalization; } 
            }

            bool IDtdParserAdapterV1.Namespaces { 
                get { return reader.DtdParserProxy_Namespaces; } 
            }

            bool IDtdParserAdapterV1.V1CompatibilityMode { 
                get { return reader.DtdParserProxy_V1CompatibilityMode; } 
            }
#endif

        }

//
// NodeData
//
        private class NodeData : IComparable  {
            // static instance with no data - is used when XmlTextReader is closed
            static volatile NodeData s_None;

            // NOTE: Do not use this property for reference comparison. It may not be unique.
            internal static NodeData None {
                get {
                    if ( s_None == null ) {
                        // no locking; s_None is immutable so it's not a problem that it may get initialized more than once
                        s_None = new NodeData();
                    }
                    return s_None;
                }
            }

            // type
            internal XmlNodeType type;

            // name
            internal string localName;
            internal string prefix;
            internal string ns;
            internal string nameWPrefix;

            // value:
            // value == null -> the value is kept in the 'chars' buffer starting at valueStartPos and valueLength long
            string value;          
            char[] chars;
            int    valueStartPos;   
            int    valueLength;
            
            // main line info
            internal LineInfo   lineInfo;

            // second line info
            internal LineInfo   lineInfo2;

            // quote char for attributes
            internal char   quoteChar;

            // depth
            internal int    depth;

            // empty element / default attribute
            bool   isEmptyOrDefault;

            // entity id
            internal int    entityId;

            // helper members
            internal bool   xmlContextPushed;

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
            // attribute value chunks
            internal NodeData nextAttrValueChunk;
#endif

            // type info
            internal object schemaType;
            internal object typedValue;

            internal NodeData() {
                Clear( XmlNodeType.None );
                xmlContextPushed = false;
            }

            internal int LineNo { 
                get { 
                    return lineInfo.lineNo;
                }
            }

            internal int LinePos { 
                get { 
                    return lineInfo.linePos;
                }
            }

            internal bool IsEmptyElement {
                get {
                    return type == XmlNodeType.Element && isEmptyOrDefault;
                }
                set {
                    Debug.Assert( type == XmlNodeType.Element );
                    isEmptyOrDefault = value;
                }
            }

            internal bool IsDefaultAttribute {
                get {
                    return type == XmlNodeType.Attribute && isEmptyOrDefault;
                }
                set {
                    Debug.Assert( type == XmlNodeType.Attribute );
                    isEmptyOrDefault = value;
                }
            }

            internal bool ValueBuffered {
                get { 
                    return value == null;
                }
            }

            internal string StringValue {
                get {
                    Debug.Assert( valueStartPos >= 0 || this.value != null, "Value not ready." );

                    if ( this.value == null ) {
                        this.value = new string( chars, valueStartPos, valueLength );
                    }
                    return this.value;
                }
            }

            internal void TrimSpacesInValue() {
                if ( ValueBuffered ) {
                    XmlTextReaderImpl.StripSpaces( chars, valueStartPos, ref valueLength );
                }
                else {
                    value = XmlTextReaderImpl.StripSpaces(value);
                }
            }

            internal void Clear( XmlNodeType type ) {
                this.type = type;
                ClearName();
                value = string.Empty;  
                valueStartPos = -1;
                nameWPrefix = string.Empty;
                schemaType = null;
                typedValue = null;
            }

            internal void ClearName() {
                localName = string.Empty;
                prefix = string.Empty;
                ns = string.Empty;
                nameWPrefix = string.Empty;
            }

            internal void SetLineInfo( int lineNo, int linePos ) {
                lineInfo.Set( lineNo, linePos );
            }

            internal void SetLineInfo2( int lineNo, int linePos ) {
                lineInfo2.Set( lineNo, linePos );
            }

            internal void SetValueNode( XmlNodeType type, string value ) {
                Debug.Assert( value != null );

                this.type = type;
                ClearName();
                this.value = value;
                this.valueStartPos = -1;
            }

            internal void SetValueNode( XmlNodeType type, char[] chars, int startPos, int len ) {
                this.type = type;
                ClearName();
                
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void SetNamedNode( XmlNodeType type, string localName ) {
                SetNamedNode( type, localName, string.Empty, localName );
            }

            internal void SetNamedNode( XmlNodeType type, string localName, string prefix, string nameWPrefix ) {
                Debug.Assert( localName != null );
                Debug.Assert( localName.Length > 0 );

                this.type = type;
                this.localName = localName;
                this.prefix = prefix;
                this.nameWPrefix = nameWPrefix;
                this.ns = string.Empty;
                this.value = string.Empty;
                this.valueStartPos = -1;
            }

            internal void SetValue( string value ) {
                this.valueStartPos = -1;
                this.value = value;
            }

            internal void SetValue( char[] chars, int startPos, int len ) {
                this.value = null;
                this.chars = chars;
                this.valueStartPos = startPos;
                this.valueLength = len;
            }

            internal void OnBufferInvalidated() {
                if ( this.value == null ) {
                    Debug.Assert( valueStartPos != -1 );
                    Debug.Assert( chars != null );
                    this.value = new string( chars, valueStartPos, valueLength );
                }
                valueStartPos = -1;
            }

            internal void CopyTo( int valueOffset, BufferBuilder sb ) {
                if ( value == null ) {
                    Debug.Assert( valueStartPos != -1 );
                    Debug.Assert( chars != null );
                    sb.Append( chars, valueStartPos + valueOffset, valueLength - valueOffset );
                }
                else {
                    if ( valueOffset <= 0 ) {
                        sb.Append( value );
                    }
                    else {
                        sb.Append( value, valueOffset, value.Length - valueOffset );
                    }
                }
            }

            internal int CopyTo( int valueOffset, char[] buffer, int offset, int length ) {
                if ( value == null ) {
                    Debug.Assert( valueStartPos != -1 );
                    Debug.Assert( chars != null );
                    int copyCount = valueLength - valueOffset;
                    if ( copyCount > length ) {
                        copyCount = length;
                    }
                    XmlTextReaderImpl.BlockCopyChars( chars, valueStartPos + valueOffset, buffer, offset, copyCount );
                    return copyCount;
                }
                else {
                    int copyCount = value.Length - valueOffset;
                    if ( copyCount > length ) {
                        copyCount = length;
                    }
                    value.CopyTo( valueOffset, buffer, offset, copyCount );
                    return copyCount;
                }
            }

            internal int CopyToBinary( IncrementalReadDecoder decoder, int valueOffset ) {
                if ( value == null ) {
                    Debug.Assert( valueStartPos != -1 );
                    Debug.Assert( chars != null );
                    return decoder.Decode( chars, valueStartPos + valueOffset, valueLength - valueOffset );
                }
                else {
                    return decoder.Decode( value, valueOffset, value.Length - valueOffset );
                }
            }

            internal void AdjustLineInfo( int valueOffset, bool isNormalized, ref LineInfo lineInfo ) {
                if ( valueOffset == 0 ) {
                    return;
                }
                if ( valueStartPos != -1 ) {
                    XmlTextReaderImpl.AdjustLineInfo( chars, valueStartPos, valueStartPos + valueOffset, isNormalized, ref lineInfo );
                }
                else {
                    XmlTextReaderImpl.AdjustLineInfo( value, 0, valueOffset, isNormalized, ref lineInfo );
                }
            }

            // This should be inlined by JIT compiler
            internal string GetNameWPrefix( XmlNameTable nt ) { 
                if ( nameWPrefix != null ) {
                    return nameWPrefix;
                }
                else {
                    return CreateNameWPrefix( nt );
                }
            }

            internal string CreateNameWPrefix( XmlNameTable nt ) { 
                Debug.Assert( nameWPrefix == null );
                if ( prefix.Length == 0 ) {
                    nameWPrefix = localName;
                }
                else {
                    nameWPrefix = nt.Add( string.Concat( prefix, ":", localName ) );
                }
                return nameWPrefix;
            }

            int IComparable.CompareTo( object obj ) {
                NodeData other = obj as NodeData;
                if ( other != null ) {
                    if ( Ref.Equal( localName, other.localName ) ) {
                        if ( Ref.Equal( ns, other.ns ) ) {
                            return 0;
                        }
                        else {
                            return string.CompareOrdinal( ns, other.ns );
                        }
                    }
                    else {
                        return string.CompareOrdinal( localName, other.localName );
                    }
                }
                else {
                    Debug.Assert( false, "We should never get to this point." );
                    // 'other' is null, 'this' is not null. Always return 1, like "".CompareTo(null).
                    return 1;
                }
            }
        }

        // 
        // DtdDefaultAttributeInfoToNodeDataComparer
        // 
        // Compares IDtdDefaultAttributeInfo to NodeData
        private class DtdDefaultAttributeInfoToNodeDataComparer : IComparer<object> {
            private static IComparer<object> s_instance = new DtdDefaultAttributeInfoToNodeDataComparer();

            internal static IComparer<object> Instance {
                get { return s_instance; }
            }

            public int Compare(object x, object y) {
                Debug.Assert(x == null || x is NodeData || x is IDtdDefaultAttributeInfo);
                Debug.Assert(y == null || y is NodeData || y is IDtdDefaultAttributeInfo);

                string localName, localName2;
                string prefix, prefix2;

                if (x == null) {
                    return y == null ? 0 : -1;
                }
                else if (y == null) {
                    return 1;
                }

                NodeData nodeData = x as NodeData;
                if (nodeData != null) {
                    localName = nodeData.localName;
                    prefix = nodeData.prefix;
                }
                else {
                    IDtdDefaultAttributeInfo attrDef = x as IDtdDefaultAttributeInfo;
                    if (attrDef != null) {
                        localName = attrDef.LocalName;
                        prefix = attrDef.Prefix;
                    }
                    else {
                        throw new XmlException(Res.Xml_DefaultException, string.Empty);
                    }
                }

                nodeData = y as NodeData;
                if (nodeData != null) {
                    localName2 = nodeData.localName;
                    prefix2 = nodeData.prefix;
                }
                else {
                    IDtdDefaultAttributeInfo attrDef = y as IDtdDefaultAttributeInfo;
                    if (attrDef != null) {
                        localName2 = attrDef.LocalName;
                        prefix2 = attrDef.Prefix;
                    }
                    else {
                        throw new XmlException(Res.Xml_DefaultException, string.Empty);
                    }
                }

                // string.Compare does reference euqality first for us, so we don't have to do it here
                int result = string.Compare(localName, localName2, StringComparison.Ordinal);
                if (result != 0) {
                    return result;
                }

                return string.Compare(prefix, prefix2, StringComparison.Ordinal);
            }
        }

#if !SILVERLIGHT
//
// OnDefaultAttributeUse delegate
//
        internal delegate void OnDefaultAttributeUseDelegate(IDtdDefaultAttributeInfo defaultAttribute, XmlTextReaderImpl coreReader);
#endif

    }
}
