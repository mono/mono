
using System.Diagnostics;

using System.Threading.Tasks;

namespace System.Xml
{
    internal partial class ReadContentAsBinaryHelper {

// Internal methods 

        internal async Task< int > ReadContentAsBase64Async( byte[] buffer, int index, int count ) {
            // check arguments
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

            switch ( state ) {
                case State.None:
                    if ( !reader.CanReadContentAs() ) {
                        throw reader.CreateReadContentAsException( "ReadContentAsBase64" );
                    }
                    if ( !await InitAsync().ConfigureAwait(false) ) {
                        return 0;
                    }
                    break;
                case State.InReadContent:
                    // if we have a correct decoder, go read
                    if ( decoder == base64Decoder ) {
                        // read more binary data
                        return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
                    }
                    break;
                case State.InReadElementContent:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                default:
                    Debug.Assert( false );
                    return 0;
            }

            Debug.Assert( state == State.InReadContent );

            // setup base64 decoder
            InitBase64Decoder();

            // read more binary data
            return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        internal async Task< int > ReadContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            // check arguments
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

            switch ( state ) {
                case State.None:
                    if ( !reader.CanReadContentAs() ) {
                        throw reader.CreateReadContentAsException( "ReadContentAsBinHex" );
                    }
                    if ( !await InitAsync().ConfigureAwait(false) ) {
                        return 0;
                    }
                    break;
                case State.InReadContent:
                    // if we have a correct decoder, go read
                    if ( decoder == binHexDecoder ) {
                        // read more binary data
                        return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
                    }
                    break;
                case State.InReadElementContent:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                default:
                    Debug.Assert( false );
                    return 0;
            }    

            Debug.Assert( state == State.InReadContent );

            // setup binhex decoder
            InitBinHexDecoder();

            // read more binary data
            return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        internal async Task< int > ReadElementContentAsBase64Async( byte[] buffer, int index, int count ) {
            // check arguments
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

            switch ( state ) {
                case State.None:
                    if ( reader.NodeType != XmlNodeType.Element ) {
                        throw reader.CreateReadElementContentAsException( "ReadElementContentAsBase64" );
                    }
                    if ( !await InitOnElementAsync().ConfigureAwait(false) ) {
                        return 0;
                    }
                    break;
                case State.InReadContent:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                case State.InReadElementContent:
                    // if we have a correct decoder, go read
                    if ( decoder == base64Decoder ) {
                        // read more binary data
                        return await ReadElementContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
                    }
                    break;
                default:
                    Debug.Assert( false );
                    return 0;
            }    

            Debug.Assert( state == State.InReadElementContent );

            // setup base64 decoder
            InitBase64Decoder();

            // read more binary data
            return await ReadElementContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        internal async Task< int > ReadElementContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            // check arguments
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

            switch ( state ) {
                case State.None:
                    if ( reader.NodeType != XmlNodeType.Element ) {
                        throw reader.CreateReadElementContentAsException( "ReadElementContentAsBinHex" );
                    }
                    if ( !await InitOnElementAsync().ConfigureAwait(false) ) {
                        return 0;
                    }
                    break;
                case State.InReadContent:
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                case State.InReadElementContent:
                    // if we have a correct decoder, go read
                    if ( decoder == binHexDecoder ) {
                        // read more binary data
                        return await ReadElementContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
                    }
                    break;
                default:
                    Debug.Assert( false );
                    return 0;
            }    

            Debug.Assert( state == State.InReadElementContent );

            // setup binhex decoder
            InitBinHexDecoder();

            // read more binary data
            return await ReadElementContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        internal async Task FinishAsync() {
            if ( state != State.None ) {
                while ( await MoveToNextContentNodeAsync( true ).ConfigureAwait(false) )
                    ;
                if ( state == State.InReadElementContent ) {
                    if ( reader.NodeType != XmlNodeType.EndElement ) {
                        throw new XmlException( Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo );
                    }
                    // move off the EndElement
                    await reader.ReadAsync().ConfigureAwait(false);
                }
            }
            Reset();
        }

// Private methods
        private async Task< bool > InitAsync() {
            // make sure we are on a content node
            if ( !await MoveToNextContentNodeAsync( false ).ConfigureAwait(false) ) {
                return false;
            }

            state = State.InReadContent;
            isEnd = false;
            return true;
        }

        private async Task< bool > InitOnElementAsync() {
            Debug.Assert( reader.NodeType == XmlNodeType.Element );
            bool isEmpty = reader.IsEmptyElement;

            // move to content or off the empty element
            await reader.ReadAsync().ConfigureAwait(false);
            if ( isEmpty ) {
                return false;
            }

            // make sure we are on a content node
            if ( !await MoveToNextContentNodeAsync( false ).ConfigureAwait(false) ) {
                if ( reader.NodeType != XmlNodeType.EndElement ) {
                    throw new XmlException( Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo );
                }
                // move off end element
                await reader.ReadAsync().ConfigureAwait(false);
                return false;
            }
            state = State.InReadElementContent;
            isEnd = false;
            return true;
        }

        private async Task< int > ReadContentAsBinaryAsync( byte[] buffer, int index, int count ) {
            Debug.Assert( decoder != null );

            if ( isEnd ) {
                Reset();
                return 0;
            }
            decoder.SetNextOutputBuffer( buffer, index, count );

            for (;;) {
                // use streaming ReadValueChunk if the reader supports it
                if ( canReadValueChunk ) {
                    for (;;) {
                        if ( valueOffset < valueChunkLength ) {
                            int decodedCharsCount = decoder.Decode( valueChunk, valueOffset, valueChunkLength - valueOffset );
                            valueOffset += decodedCharsCount;
                        }
                        if ( decoder.IsFull ) {
                            return decoder.DecodedCount;
                        }
                        Debug.Assert( valueOffset == valueChunkLength );
                        if ( ( valueChunkLength = await reader.ReadValueChunkAsync( valueChunk, 0, ChunkSize ).ConfigureAwait(false) ) == 0 ) {
                            break;
                        }
                        valueOffset = 0;
                    }
                }
                else {
                    // read what is reader.Value
                    string value = await reader.GetValueAsync().ConfigureAwait(false);
                    int decodedCharsCount = decoder.Decode( value, valueOffset, value.Length - valueOffset );
                    valueOffset += decodedCharsCount;

                    if ( decoder.IsFull ) {
                        return decoder.DecodedCount;
                    }
                }

                valueOffset = 0;

                // move to next textual node in the element content; throw on sub elements
                if ( !await MoveToNextContentNodeAsync( true ).ConfigureAwait(false) ) {
                    isEnd = true;
                    return decoder.DecodedCount;
                }
            }
        }

        private async Task< int > ReadElementContentAsBinaryAsync( byte[] buffer, int index, int count ) {
            if ( count == 0 ) {
                return 0;
            }
            // read binary
            int decoded = await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
            if ( decoded > 0 ) {
                return decoded;
            }

            // if 0 bytes returned check if we are on a closing EndElement, throw exception if not
            if ( reader.NodeType != XmlNodeType.EndElement ) {
                throw new XmlException( Res.Xml_InvalidNodeType, reader.NodeType.ToString(), reader as IXmlLineInfo );
            }

            // move off the EndElement
            await reader.ReadAsync().ConfigureAwait(false);
            state = State.None;
            return 0;
        }

        async Task< bool > MoveToNextContentNodeAsync( bool moveIfOnContentNode ) {
            do {
                switch ( reader.NodeType ) {
                    case XmlNodeType.Attribute:
                        return !moveIfOnContentNode;
                    case XmlNodeType.Text:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.CDATA:
                        if ( !moveIfOnContentNode ) {
                            return true;
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        // skip comments, pis and end entity nodes
                        break;
                    case XmlNodeType.EntityReference:
                        if ( reader.CanResolveEntity ) {
                            reader.ResolveEntity();
                            break;
                        }
                        goto default;
                    default:
                        return false;
                }
                moveIfOnContentNode = false;
            } while ( await reader.ReadAsync().ConfigureAwait(false) );
            return false;
        }
    }
}
