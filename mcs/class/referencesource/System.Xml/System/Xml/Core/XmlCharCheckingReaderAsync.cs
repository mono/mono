
using System;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace System.Xml {

    //
    // XmlCharCheckingReaderWithNS
    //
    internal partial class XmlCharCheckingReader : XmlWrappingReader {

        public override async Task< bool > ReadAsync() {
            switch ( state ) {
                case State.Initial:
                    state = State.Interactive;
                    if ( base.reader.ReadState == ReadState.Initial ) {
                        goto case State.Interactive;
                    }
                    break;

                case State.Error:
                    return false;

                case State.InReadBinary:
                    await FinishReadBinaryAsync().ConfigureAwait(false);
                    state = State.Interactive;
                    goto case State.Interactive;

                case State.Interactive:
                    if ( !await base.reader.ReadAsync().ConfigureAwait(false) ) {
                        return false;
                    }
                    break;

                default:
                    Debug.Assert( false );
                    return false;
            }

            XmlNodeType nodeType = base.reader.NodeType;

            if ( !checkCharacters ) {
                switch ( nodeType ) {
                    case XmlNodeType.Comment:
                        if ( ignoreComments ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    case XmlNodeType.Whitespace:
                        if ( ignoreWhitespace ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        if ( ignorePis ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        break;
                    case XmlNodeType.DocumentType:
                        if ( dtdProcessing == DtdProcessing.Prohibit ) {
                            Throw( Res.Xml_DtdIsProhibitedEx, string.Empty );
                        }
                        else if ( dtdProcessing == DtdProcessing.Ignore ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        break;
                }
                return true;
            }
            else {
                switch ( nodeType ) {
                    case XmlNodeType.Element:
                        if ( checkCharacters ) {
                            // check element name
                            ValidateQName( base.reader.Prefix, base.reader.LocalName );

                            // check values of attributes
                            if ( base.reader.MoveToFirstAttribute() ) {
                                do {
                                    ValidateQName( base.reader.Prefix, base.reader.LocalName );
                                    CheckCharacters( base.reader.Value );
                                } while ( base.reader.MoveToNextAttribute() );

                                base.reader.MoveToElement();
                            }
                        }
                        break;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        if ( checkCharacters ) {
                            CheckCharacters( await base.reader.GetValueAsync().ConfigureAwait(false) );
                        }
                        break;

                    case XmlNodeType.EntityReference:
                        if ( checkCharacters ) {
                            // check name
                            ValidateQName( base.reader.Name );
                        }
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        if ( ignorePis ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        if ( checkCharacters ) {
                            ValidateQName( base.reader.Name );
                            CheckCharacters( base.reader.Value );
                        }
                        break;

                    case XmlNodeType.Comment:
                        if ( ignoreComments ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        if ( checkCharacters ) {
                            CheckCharacters( base.reader.Value );
                        }
                        break;

                    case XmlNodeType.DocumentType:
                        if ( dtdProcessing == DtdProcessing.Prohibit ) {
                            Throw( Res.Xml_DtdIsProhibitedEx, string.Empty );
                        }
                        else if ( dtdProcessing == DtdProcessing.Ignore ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        if ( checkCharacters ) {
                            ValidateQName( base.reader.Name );
                            CheckCharacters( base.reader.Value );
                            
                            string str;
                            str = base.reader.GetAttribute( "SYSTEM" );
                            if ( str != null ) {
                                CheckCharacters( str );
                            }

                            str = base.reader.GetAttribute( "PUBLIC" );
                            if ( str != null ) {
                                int i;
                                if ( ( i = xmlCharType.IsPublicId( str ) ) >= 0 ) {
                                    Throw( Res.Xml_InvalidCharacter, XmlException.BuildCharExceptionArgs( str, i ) );
                                }
                            }
                        }
                        break;

                    case XmlNodeType.Whitespace:
                        if ( ignoreWhitespace ) {
                            return await ReadAsync().ConfigureAwait(false);
                        }
                        if ( checkCharacters ) {
                            CheckWhitespace( await base.reader.GetValueAsync().ConfigureAwait(false) );
                        }
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        if ( checkCharacters ) {
                            CheckWhitespace( await base.reader.GetValueAsync().ConfigureAwait(false) );
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if ( checkCharacters ) {
                            ValidateQName( base.reader.Prefix, base.reader.LocalName );
                        }
                        break;

                    default:
                        break;
                }
                lastNodeType = nodeType;
                return true;
            }
        }

        public override async Task< int > ReadContentAsBase64Async( byte[] buffer, int index, int count ) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            if ( state != State.InReadBinary ) {
                // forward ReadBase64Chunk calls into the base (wrapped) reader if possible, i.e. if it can read binary and we 
                // should not check characters
                if ( base.CanReadBinaryContent && ( !checkCharacters ) ) {
                    readBinaryHelper = null;
                    state = State.InReadBinary;
                    return await base.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
                }
                // the wrapped reader cannot read chunks or we are on an element where we should check characters or ignore white spaces
                else {
                    readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
                }
            }
            else { 
                // forward calls into wrapped reader 
                if ( readBinaryHelper == null ) {
                    return await base.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
                }
            }

            // turn off InReadBinary state in order to have a normal Read() behavior when called from readBinaryHelper
            state = State.Interactive;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBase64Async(buffer, index, count).ConfigureAwait(false);

            // turn on InReadBinary in again and return
            state = State.InReadBinary;
            return readCount;
        }

        public override async Task< int > ReadContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            if ( state != State.InReadBinary ) {
                // forward ReadBinHexChunk calls into the base (wrapped) reader if possible, i.e. if it can read chunks and we 
                // should not check characters
                if ( base.CanReadBinaryContent && ( !checkCharacters ) ) {
                    readBinaryHelper = null;
                    state = State.InReadBinary;
                    return await base.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
                }
                // the wrapped reader cannot read chunks or we are on an element where we should check characters or ignore white spaces
                else {
                    readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
                }
            }
            else { 
                // forward calls into wrapped reader 
                if ( readBinaryHelper == null ) {
                    return await base.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
                }
            }

            // turn off InReadBinary state in order to have a normal Read() behavior when called from readBinaryHelper
            state = State.Interactive;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBinHexAsync(buffer, index, count).ConfigureAwait(false);

            // turn on InReadBinary in again and return
            state = State.InReadBinary;
            return readCount;        
        }

        public override async Task< int > ReadElementContentAsBase64Async( byte[] buffer, int index, int count ) {
            // check arguments
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0) {
                throw new ArgumentOutOfRangeException("index");
            }
            if (buffer.Length - index < count) {
                throw new ArgumentOutOfRangeException("count");
            }

            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            if ( state != State.InReadBinary ) {
                // forward ReadBase64Chunk calls into the base (wrapped) reader if possible, i.e. if it can read binary and we 
                // should not check characters
                if ( base.CanReadBinaryContent && ( !checkCharacters ) ) {
                    readBinaryHelper = null;
                    state = State.InReadBinary;
                    return await base.ReadElementContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
                }
                // the wrapped reader cannot read chunks or we are on an element where we should check characters or ignore white spaces
                else {
                    readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
                }
            }
            else { 
                // forward calls into wrapped reader 
                if ( readBinaryHelper == null ) {
                    return await base.ReadElementContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);
                }
            }

            // turn off InReadBinary state in order to have a normal Read() behavior when called from readBinaryHelper
            state = State.Interactive;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBase64Async(buffer, index, count).ConfigureAwait(false);

            // turn on InReadBinary in again and return
            state = State.InReadBinary;
            return readCount;
        }

        public override async Task< int > ReadElementContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            // check arguments
            if (buffer == null) {
                throw new ArgumentNullException("buffer");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (index < 0) {
                throw new ArgumentOutOfRangeException("index");
            }
            if (buffer.Length - index < count) {
                throw new ArgumentOutOfRangeException("count");
            }
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            if ( state != State.InReadBinary ) {
                // forward ReadBinHexChunk calls into the base (wrapped) reader if possible, i.e. if it can read chunks and we 
                // should not check characters
                if ( base.CanReadBinaryContent && ( !checkCharacters ) ) {
                    readBinaryHelper = null;
                    state = State.InReadBinary;
                    return await base.ReadElementContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
                }
                // the wrapped reader cannot read chunks or we are on an element where we should check characters or ignore white spaces
                else {
                    readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, this );
                }
            }
            else { 
                // forward calls into wrapped reader 
                if ( readBinaryHelper == null ) {
                    return await base.ReadElementContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);
                }
            }

            // turn off InReadBinary state in order to have a normal Read() behavior when called from readBinaryHelper
            state = State.Interactive;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBinHexAsync(buffer, index, count).ConfigureAwait(false);

            // turn on InReadBinary in again and return
            state = State.InReadBinary;
            return readCount;        
        }

        private async Task FinishReadBinaryAsync() {
            state = State.Interactive;
            if ( readBinaryHelper != null ) {
                await readBinaryHelper.FinishAsync().ConfigureAwait(false);
            }
        }

    }
}
