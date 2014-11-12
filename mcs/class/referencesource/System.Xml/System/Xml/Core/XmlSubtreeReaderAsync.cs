
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace System.Xml {

    internal sealed partial class XmlSubtreeReader : XmlWrappingReader, IXmlLineInfo, IXmlNamespaceResolver {

        public override Task<string> GetValueAsync() {
            if (useCurNode) {
                return Task.FromResult(curNode.value);
            }
            else {
                return reader.GetValueAsync();
            }
        }

        public override async Task< bool > ReadAsync() {
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
                    if ( await reader.ReadAsync().ConfigureAwait(false) ) {
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
                    if ( !await FinishReadElementContentAsBinaryAsync().ConfigureAwait(false) ) {
                        return false;
                    }
                    return await ReadAsync().ConfigureAwait(false);

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    if ( !await FinishReadContentAsBinaryAsync().ConfigureAwait(false) ) {
                        return false;
                    }
                    return await ReadAsync().ConfigureAwait(false);

                default:
                    Debug.Assert( false );
                    return false;
            }
        }

        public override async Task SkipAsync() {
            switch ( state ) {
                case State.Initial:
                    await ReadAsync().ConfigureAwait(false);
                    return;

                case State.Interactive:
                    curNsAttr = -1;
                    useCurNode = false;
                    reader.MoveToElement();
                    Debug.Assert( reader.Depth >= initialDepth );
                    if ( reader.Depth == initialDepth ) {
                        if ( reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement ) {
                            // we are on root of the subtree -> skip to the end element and set to Eof state
                            if ( await reader.ReadAsync().ConfigureAwait(false) ) {
                                while ( reader.NodeType != XmlNodeType.EndElement && reader.Depth > initialDepth ) {
                                    await reader.SkipAsync().ConfigureAwait(false);
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
                    await reader.SkipAsync().ConfigureAwait(false);
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
                    if ( await FinishReadElementContentAsBinaryAsync().ConfigureAwait(false) ) {
                        await SkipAsync().ConfigureAwait(false);
                    }
                    break;

                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    if ( await FinishReadContentAsBinaryAsync().ConfigureAwait(false) ) {
                        await SkipAsync().ConfigureAwait(false);
                    }
                    break;

                case State.Error:
                    return;

                default:
                    Debug.Assert( false );
                    return;
            }
        }

        public override async Task< object > ReadContentAsObjectAsync() {
            try {
                InitReadContentAsType( "ReadContentAsObject" );
                object value = await reader.ReadContentAsObjectAsync().ConfigureAwait(false);
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override async Task< string > ReadContentAsStringAsync() {
            try {
                InitReadContentAsType( "ReadContentAsString" );
                string value = await reader.ReadContentAsStringAsync().ConfigureAwait(false);
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override async Task< object > ReadContentAsAsync( Type returnType, IXmlNamespaceResolver namespaceResolver ) {
            try {
                InitReadContentAsType( "ReadContentAs" );
                object value = await reader.ReadContentAsAsync( returnType, namespaceResolver ).ConfigureAwait(false);
                FinishReadContentAsType();
                return value;
            }
            catch {
                state = State.Error;
                throw;
            }
        }

        public override async Task< int > ReadContentAsBase64Async( byte[] buffer, int index, int count ) {
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
                            return await reader.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
                        default:
                            Debug.Assert( false );
                            return 0;
                    }

                case State.Interactive:
                    state = State.ReadContentAsBase64;
                    goto case State.ReadContentAsBase64;

                case State.ReadContentAsBase64:
                    int read = await reader.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
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

        public override async Task< int > ReadElementContentAsBase64Async( byte[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if ( !await InitReadElementContentAsBinaryAsync( State.ReadElementContentAsBase64 ).ConfigureAwait(false) ) {
                        return 0;
                    }
                    goto case State.ReadElementContentAsBase64;

                case State.ReadElementContentAsBase64:
                    int read = await reader.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
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
                        await ReadAsync().ConfigureAwait(false);
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

        public override async Task< int > ReadContentAsBinHexAsync( byte[] buffer, int index, int count ) {
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
                            return await reader.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
                        default:
                            Debug.Assert( false );
                            return 0;
                    }

                case State.Interactive:
                    state = State.ReadContentAsBinHex;
                    goto case State.ReadContentAsBinHex;

                case State.ReadContentAsBinHex:
                    int read = await reader.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
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

        public override async Task< int > ReadElementContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return 0;

                case State.Interactive:
                case State.PopNamespaceScope:
                case State.ClearNsAttributes:
                    if ( !await InitReadElementContentAsBinaryAsync( State.ReadElementContentAsBinHex ).ConfigureAwait(false) ) {
                        return 0;
                    }
                    goto case State.ReadElementContentAsBinHex;
                case State.ReadElementContentAsBinHex:
                    int read = await reader.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
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
                        await ReadAsync().ConfigureAwait(false);
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

        public override Task< int > ReadValueChunkAsync( char[] buffer, int index, int count ) {
            switch (state) {
                case State.Initial:
                case State.EndOfFile:
                case State.Closed:
                case State.Error:
                    return Task.FromResult(0);

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
                        return Task.FromResult(copyCount);
                    }
                    // Otherwise fall back to the case State.Interactive.
                    // No need to clean ns attributes or pop scope because the reader when ReadValueChunk is called
                    // - on Element errors
                    // - on EndElement errors
                    // - on Attribute does not move
                    // and that's all where State.ClearNsAttributes or State.PopnamespaceScope can be set
                    goto case State.Interactive;

                case State.Interactive:
                    return reader.ReadValueChunkAsync(buffer, index, count);

                case State.ReadElementContentAsBase64:
                case State.ReadElementContentAsBinHex:
                case State.ReadContentAsBase64:
                case State.ReadContentAsBinHex:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingReadValueChunkWithBinary ) );

                default:
                    Debug.Assert( false );
                    return Task.FromResult(0);
            }
        }

        private async Task< bool > InitReadElementContentAsBinaryAsync( State binaryState ) {
            if ( NodeType != XmlNodeType.Element ) {
                throw reader.CreateReadElementContentAsException( "ReadElementContentAsBase64" );
            }

            bool isEmpty = IsEmptyElement;

            // move to content or off the empty element
            if ( !await ReadAsync().ConfigureAwait(false) || isEmpty ) {
                return false;
            }
            // special-case child element and end element
            switch ( NodeType ) {
                case XmlNodeType.Element:
                    throw new XmlException(Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo);
                case XmlNodeType.EndElement:
                    // pop scope & move off end element
                    ProcessNamespaces();
                    await ReadAsync().ConfigureAwait(false);
                    return false;
            }
 
            Debug.Assert( state == State.Interactive );
            state = binaryState;
            return true;
        }

        private async Task< bool > FinishReadElementContentAsBinaryAsync() {
            Debug.Assert( state == State.ReadElementContentAsBase64 || state == State.ReadElementContentAsBinHex );

            byte[] bytes = new byte[256];
            if ( state == State.ReadElementContentAsBase64 ) {
                while ( await reader.ReadContentAsBase64Async( bytes, 0, 256 ).ConfigureAwait(false) > 0 ) ;
            }
            else {
                while ( await reader.ReadContentAsBinHexAsync( bytes, 0, 256 ).ConfigureAwait(false) > 0 ) ;
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
            return await ReadAsync().ConfigureAwait(false);
        }

        private async Task< bool > FinishReadContentAsBinaryAsync() {
            Debug.Assert( state == State.ReadContentAsBase64 || state == State.ReadContentAsBinHex );

            byte[] bytes = new byte[256];
            if ( state == State.ReadContentAsBase64 ) {
                while ( await reader.ReadContentAsBase64Async( bytes, 0, 256 ).ConfigureAwait(false) > 0 ) ;
            }
            else {
                while ( await reader.ReadContentAsBinHexAsync( bytes, 0, 256 ).ConfigureAwait(false) > 0 ) ;
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

    }
}

