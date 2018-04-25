
using System;
using System.IO;
using System.Text;
using System.Security;
using System.Threading;
using System.Xml.Schema;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

using System.Threading.Tasks;

#if SILVERLIGHT
using System.Reflection;
#endif

#if SILVERLIGHT
using BufferBuilder=System.Xml.BufferBuilder;
#else 
using BufferBuilder = System.Text.StringBuilder;
#endif

namespace System.Xml {

    internal partial class XmlTextReaderImpl : XmlReader, IXmlLineInfo, IXmlNamespaceResolver {

        private void CheckAsyncCall() {
            if (!useAsync) {
                throw new InvalidOperationException(Res.GetString(Res.Xml_ReaderAsyncNotSetException));
            }
        }

        public override Task<string> GetValueAsync() {
            CheckAsyncCall();
            if (parsingFunction >= ParsingFunction.PartialTextValue) {
                return _GetValueAsync();
            }
            return Task.FromResult(curNode.StringValue);
        }

        private async Task<string> _GetValueAsync() {

            if (parsingFunction >= ParsingFunction.PartialTextValue) {
                if (parsingFunction == ParsingFunction.PartialTextValue) {
                    await FinishPartialValueAsync().ConfigureAwait(false);
                    parsingFunction = nextParsingFunction;
                }
                else {
                    await FinishOtherValueIteratorAsync().ConfigureAwait(false);
                }
            }
            return curNode.StringValue;
        }

        private Task FinishInitAsync() {
            switch (laterInitParam.initType) {
                case InitInputType.UriString:
                    return FinishInitUriStringAsync();
                case InitInputType.Stream:
                    return FinishInitStreamAsync();
                case InitInputType.TextReader:
                    return FinishInitTextReaderAsync();
                default:
                    //should never hit here
                    Debug.Assert(false, "Invalid InitInputType");
                    return AsyncHelper.DoneTask;
            }
        }

        
        private async Task FinishInitUriStringAsync() {

            Stream stream = (Stream)(await laterInitParam.inputUriResolver.GetEntityAsync(laterInitParam.inputbaseUri, string.Empty, typeof(Stream)).ConfigureAwait(false));
            
            if (stream == null) {
                throw new XmlException(Res.Xml_CannotResolveUrl, laterInitParam.inputUriStr);
            }

            Encoding enc = null;
            // get Encoding from XmlParserContext
            if (laterInitParam.inputContext != null) {
                enc = laterInitParam.inputContext.Encoding;
            }

            try {
                // init ParsingState
                await InitStreamInputAsync(laterInitParam.inputbaseUri, reportedBaseUri, stream, null, 0, enc).ConfigureAwait(false);

                reportedEncoding = ps.encoding;

                // parse DTD
                if (laterInitParam.inputContext != null && laterInitParam.inputContext.HasDtdInfo) {
                    await ProcessDtdFromParserContextAsync(laterInitParam.inputContext).ConfigureAwait(false);
                }
            }
            catch {
                stream.Close();
                throw;
            }
            laterInitParam = null;
        }


        private async Task FinishInitStreamAsync() {

            Encoding enc = null;

            // get Encoding from XmlParserContext
            if (laterInitParam.inputContext != null) {
                enc = laterInitParam.inputContext.Encoding;
            }

            // init ParsingState
            await InitStreamInputAsync(laterInitParam.inputbaseUri, reportedBaseUri, laterInitParam.inputStream, laterInitParam.inputBytes, laterInitParam.inputByteCount, enc).ConfigureAwait(false);

            reportedEncoding = ps.encoding;

            // parse DTD
            if (laterInitParam.inputContext != null && laterInitParam.inputContext.HasDtdInfo) {
                await ProcessDtdFromParserContextAsync(laterInitParam.inputContext).ConfigureAwait(false);
            }
            laterInitParam = null;
        }

        private async Task FinishInitTextReaderAsync() {

             // init ParsingState
            await InitTextReaderInputAsync(reportedBaseUri, laterInitParam.inputTextReader).ConfigureAwait(false);

            reportedEncoding = ps.encoding;

            // parse DTD
            if (laterInitParam.inputContext != null && laterInitParam.inputContext.HasDtdInfo) {
                await ProcessDtdFromParserContextAsync(laterInitParam.inputContext).ConfigureAwait(false);
            }

            laterInitParam = null;
        }

        // Reads next node from the input data
        public override Task<bool> ReadAsync() {
            CheckAsyncCall();

            if (laterInitParam != null) {
                return FinishInitAsync().CallBoolTaskFuncWhenFinish(ReadAsync);
            }

            for (; ; ) {
                switch (parsingFunction) {
                    case ParsingFunction.ElementContent:
                        return ParseElementContentAsync();
                    case ParsingFunction.DocumentContent:
                        return ParseDocumentContentAsync();
#if !SILVERLIGHT // Needed only for XmlTextReader
                    //XmlTextReader can't execute Async method.
                    case ParsingFunction.OpenUrl:
                        Debug.Assert(false);
                        break;
#endif
                    case ParsingFunction.SwitchToInteractive:
                        Debug.Assert(!ps.appendMode);
                        readState = ReadState.Interactive;
                        parsingFunction = nextParsingFunction;
                        continue;
                    case ParsingFunction.SwitchToInteractiveXmlDecl:
                        return ReadAsync_SwitchToInteractiveXmlDecl();
                    case ParsingFunction.ResetAttributesRootLevel:
                        ResetAttributes();
                        curNode = nodes[index];
                        parsingFunction = (index == 0) ? ParsingFunction.DocumentContent : ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.MoveToElementContent:
                        ResetAttributes();
                        index++;
                        curNode = AddNode(index, index);
                        parsingFunction = ParsingFunction.ElementContent;
                        continue;
                    case ParsingFunction.PopElementContext:
                        PopElementContext();
                        parsingFunction = nextParsingFunction;
                        Debug.Assert(parsingFunction == ParsingFunction.ElementContent ||
                                      parsingFunction == ParsingFunction.DocumentContent);
                        continue;
                    case ParsingFunction.PopEmptyElementContext:
                        curNode = nodes[index];
                        Debug.Assert(curNode.type == XmlNodeType.Element);
                        curNode.IsEmptyElement = false;
                        ResetAttributes();
                        PopElementContext();
                        parsingFunction = nextParsingFunction;
                        continue;
#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                    case ParsingFunction.EntityReference:
                        parsingFunction = nextParsingFunction;
                        return ParseEntityReferenceAsync().ReturnTaskBoolWhenFinish(true);
                    case ParsingFunction.ReportEndEntity:
                        SetupEndEntityNodeInContent();
                        parsingFunction = nextParsingFunction;
                        return AsyncHelper.DoneTaskTrue;
                    case ParsingFunction.AfterResolveEntityInContent:
                        curNode = AddNode(index, index);
                        reportedEncoding = ps.encoding;
                        reportedBaseUri = ps.baseUriStr;
                        parsingFunction = nextParsingFunction;
                        continue;
                    case ParsingFunction.AfterResolveEmptyEntityInContent:
                        curNode = AddNode(index, index);
                        curNode.SetValueNode(XmlNodeType.Text, string.Empty);
                        curNode.SetLineInfo(ps.lineNo, ps.LinePos);
                        reportedEncoding = ps.encoding;
                        reportedBaseUri = ps.baseUriStr;
                        parsingFunction = nextParsingFunction;
                        return AsyncHelper.DoneTaskTrue;
#endif
                    case ParsingFunction.InReadAttributeValue:
                        FinishAttributeValueIterator();
                        curNode = nodes[index];
                        continue;
#if !SILVERLIGHT // Needed only for XmlTextReader (ReadChars, ReadBase64, ReadBinHex)
                    case ParsingFunction.InIncrementalRead:
                        FinishIncrementalRead();
                        return AsyncHelper.DoneTaskTrue;
                    case ParsingFunction.FragmentAttribute:
                        return Task.FromResult( ParseFragmentAttribute() );
                    case ParsingFunction.XmlDeclarationFragment:
                        ParseXmlDeclarationFragment();
                        parsingFunction = ParsingFunction.GoToEof;
                        return AsyncHelper.DoneTaskTrue;
#endif
                    case ParsingFunction.GoToEof:
                        OnEof();
                        return AsyncHelper.DoneTaskFalse;
                    case ParsingFunction.Error:
                    case ParsingFunction.Eof:
                    case ParsingFunction.ReaderClosed:
                        return AsyncHelper.DoneTaskFalse;
                    case ParsingFunction.NoData:
                        ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                        return AsyncHelper.DoneTaskFalse;
                    case ParsingFunction.PartialTextValue:
                        return SkipPartialTextValueAsync().CallBoolTaskFuncWhenFinish(ReadAsync);
                    case ParsingFunction.InReadValueChunk:
                        return FinishReadValueChunkAsync().CallBoolTaskFuncWhenFinish(ReadAsync);
                    case ParsingFunction.InReadContentAsBinary:
                        return FinishReadContentAsBinaryAsync().CallBoolTaskFuncWhenFinish(ReadAsync);
                    case ParsingFunction.InReadElementContentAsBinary:
                        return FinishReadElementContentAsBinaryAsync().CallBoolTaskFuncWhenFinish(ReadAsync);
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        private Task<bool> ReadAsync_SwitchToInteractiveXmlDecl() {
            readState = ReadState.Interactive;
            parsingFunction = nextParsingFunction;
            Task<bool> task = ParseXmlDeclarationAsync(false);
            if (task.IsSuccess()) {
                return ReadAsync_SwitchToInteractiveXmlDecl_Helper(task.Result);
            }
            else {
                return _ReadAsync_SwitchToInteractiveXmlDecl(task);
            }
        }

        private async Task<bool> _ReadAsync_SwitchToInteractiveXmlDecl(Task<bool> task) {
            bool result = await task.ConfigureAwait(false);
            return await ReadAsync_SwitchToInteractiveXmlDecl_Helper(result).ConfigureAwait(false);
        }

        private Task<bool> ReadAsync_SwitchToInteractiveXmlDecl_Helper(bool finish) {
            if (finish) {
                reportedEncoding = ps.encoding;
                return AsyncHelper.DoneTaskTrue;
            }
            else {
                reportedEncoding = ps.encoding;
                return ReadAsync();
            }
        }


        // Skips the current node. If on element, skips to the end tag of the element.
        public override async Task SkipAsync() {
            CheckAsyncCall();
            if ( readState != ReadState.Interactive )
                return;

            if ( InAttributeValueIterator ) {
                FinishAttributeValueIterator();
                curNode = nodes[index];
            }
            else {
                switch ( parsingFunction ) {
                    case ParsingFunction.InReadAttributeValue:
                        Debug.Assert( false );
                        break;
#if !SILVERLIGHT // Needed only for XmlTextReader (ReadChars, ReadBase64, ReadBinHex)
                    case ParsingFunction.InIncrementalRead:
                        FinishIncrementalRead();
                        break;
#endif
                    case ParsingFunction.PartialTextValue:
                        await SkipPartialTextValueAsync().ConfigureAwait(false);
                        break;
                    case ParsingFunction.InReadValueChunk:
                        await FinishReadValueChunkAsync().ConfigureAwait(false);
                        break;
                    case ParsingFunction.InReadContentAsBinary:
                        await FinishReadContentAsBinaryAsync().ConfigureAwait(false);
                        break;
                    case ParsingFunction.InReadElementContentAsBinary:
                        await FinishReadElementContentAsBinaryAsync().ConfigureAwait(false);
                        break;
                }
            }

            switch ( curNode.type ) {
                // skip subtree
                case XmlNodeType.Element:
                    if ( curNode.IsEmptyElement ) {
                        break;
                    }
                    int initialDepth = index;
                    parsingMode = ParsingMode.SkipContent;
                    // skip content
                    while ( await outerReader.ReadAsync().ConfigureAwait(false) && index > initialDepth ) ;
                    Debug.Assert( curNode.type == XmlNodeType.EndElement );
                    Debug.Assert( parsingFunction != ParsingFunction.Eof );
                    parsingMode = ParsingMode.Full;
                    break;
                case XmlNodeType.Attribute:
                    outerReader.MoveToElement();
                    goto case XmlNodeType.Element;
            }
            // move to following sibling node
            await outerReader.ReadAsync().ConfigureAwait(false);
            return;
        }

        private async Task<int> ReadContentAsBase64_AsyncHelper(Task<bool> task, byte[] buffer, int index, int count) {
            await task.ConfigureAwait(false);
            if (!task.Result) {
                return 0;
            }
            else {
                // setup base64 decoder
                InitBase64Decoder();

                // read binary data
                return await ReadContentAsBinaryAsync(buffer, index, count).ConfigureAwait(false);
            }
        }

        // Reads and concatenates content nodes, base64-decodes the results and copies the decoded bytes into the provided buffer
        public override Task< int > ReadContentAsBase64Async( byte[] buffer, int index, int count ) {
            CheckAsyncCall();
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

            // if not the first call to ReadContentAsBase64 
            if ( parsingFunction == ParsingFunction.InReadContentAsBinary ) {
                // and if we have a correct decoder
                if ( incReadDecoder == base64Decoder ) {
                    // read more binary data
                    return ReadContentAsBinaryAsync( buffer, index, count );
                }
            }
            // first call of ReadContentAsBase64 -> initialize (move to first text child (for elements) and initialize incremental read state)
            else {
                if ( readState != ReadState.Interactive ) {
                    return AsyncHelper.DoneTaskZero;
                }
                if ( parsingFunction == ParsingFunction.InReadElementContentAsBinary ) {
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                }
                if ( !XmlReader.CanReadContentAs( curNode.type ) ) {
                    throw CreateReadContentAsException( "ReadContentAsBase64" );
                }

                Task<bool> task = InitReadContentAsBinaryAsync();
                if (task.IsSuccess()) {
                    if (!task.Result) {
                        return AsyncHelper.DoneTaskZero;
                    }
                }
                else {
                    return ReadContentAsBase64_AsyncHelper(task, buffer, index, count);
                }
            }
    
            // setup base64 decoder
            InitBase64Decoder();

            // read binary data
            return ReadContentAsBinaryAsync( buffer, index, count );
        }

        // Reads and concatenates content nodes, binhex-decodes the results and copies the decoded bytes into the provided buffer
        public override async Task< int > ReadContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            CheckAsyncCall();
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

            // if not the first call to ReadContentAsBinHex 
            if ( parsingFunction == ParsingFunction.InReadContentAsBinary ) {
                // and if we have a correct decoder
                if ( incReadDecoder == binHexDecoder ) {
                    // read more binary data
                    return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
                }
            }
            // first call of ReadContentAsBinHex -> initialize (move to first text child (for elements) and initialize incremental read state)
            else {
                if ( readState != ReadState.Interactive ) {
                    return 0;
                }
                if ( parsingFunction == ParsingFunction.InReadElementContentAsBinary ) {
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                }
                if ( !XmlReader.CanReadContentAs( curNode.type ) ) {
                    throw CreateReadContentAsException( "ReadContentAsBinHex" );
                }

                if (!await InitReadContentAsBinaryAsync().ConfigureAwait(false)) {
                    return 0;
                }
            }
    
            // setup binhex decoder (when in first ReadContentAsBinHex call or when mixed with ReadContentAsBase64)
            InitBinHexDecoder();

            // read binary data
            return await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        private async Task<int> ReadElementContentAsBase64Async_Helper(Task<bool> task, byte[] buffer, int index, int count) {
            await task.ConfigureAwait(false);
            if (!task.Result) {
                return 0;
            }
            else {
                // setup base64 decoder
                InitBase64Decoder();

                // read binary data
                return await ReadElementContentAsBinaryAsync(buffer, index, count).ConfigureAwait(false);
            }
        }

        // Reads and concatenates content of an element, base64-decodes the results and copies the decoded bytes into the provided buffer
        public override Task< int > ReadElementContentAsBase64Async( byte[] buffer, int index, int count ) {
            CheckAsyncCall();
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

            // if not the first call to ReadContentAsBase64 
            if ( parsingFunction == ParsingFunction.InReadElementContentAsBinary ) {
                // and if we have a correct decoder
                if ( incReadDecoder == base64Decoder ) {
                    // read more binary data
                    return ReadElementContentAsBinaryAsync(buffer, index, count);
                }
            }
            // first call of ReadElementContentAsBase64 -> initialize 
            else {
                if ( readState != ReadState.Interactive ) {
                    return AsyncHelper.DoneTaskZero;
                }
                if ( parsingFunction == ParsingFunction.InReadContentAsBinary ) {
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                }
                if ( curNode.type != XmlNodeType.Element ) {
                    throw CreateReadElementContentAsException( "ReadElementContentAsBinHex" );
                }

                Task<bool> task = InitReadElementContentAsBinaryAsync();
                if (task.IsSuccess()) {
                    if (!task.Result) {
                        return AsyncHelper.DoneTaskZero;
                    }
                }
                else {
                    return ReadElementContentAsBase64Async_Helper(task, buffer, index, count);
                }
            }
    
            // setup base64 decoder
            InitBase64Decoder();

            // read binary data
            return ReadElementContentAsBinaryAsync(buffer, index, count);
        }

        // Reads and concatenates content of an element, binhex-decodes the results and copies the decoded bytes into the provided buffer
        public override async Task< int > ReadElementContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            CheckAsyncCall();
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

            // if not the first call to ReadContentAsBinHex 
            if ( parsingFunction == ParsingFunction.InReadElementContentAsBinary ) {
                // and if we have a correct decoder
                if ( incReadDecoder == binHexDecoder ) {
                    // read more binary data
                    return await ReadElementContentAsBinaryAsync(buffer, index, count).ConfigureAwait(false);
                }
            }
            // first call of ReadContentAsBinHex -> initialize
            else {
                if ( readState != ReadState.Interactive ) {
                    return 0;
                }
                if ( parsingFunction == ParsingFunction.InReadContentAsBinary ) {
                    throw new InvalidOperationException( Res.GetString( Res.Xml_MixingBinaryContentMethods ) );
                }
                if ( curNode.type != XmlNodeType.Element ) {
                    throw CreateReadElementContentAsException( "ReadElementContentAsBinHex" );
                }
                if (!await InitReadElementContentAsBinaryAsync().ConfigureAwait(false)) {
                    return 0;
                }
                
            }
    
            // setup binhex decoder (when in first ReadContentAsBinHex call or when mixed with ReadContentAsBase64)
            InitBinHexDecoder();

            // read binary data
            return await ReadElementContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
        }

        // Iterates over Value property and copies it into the provided buffer
        public override async Task< int > ReadValueChunkAsync( char[] buffer, int index, int count ) {
            CheckAsyncCall();
            // throw on elements
            if ( !XmlReader.HasValueInternal( curNode.type ) ) {
                throw new InvalidOperationException( Res.GetString( Res.Xml_InvalidReadValueChunk, curNode.type ) ) ;
            }
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

            // first call of ReadValueChunk -> initialize incremental read state
            if ( parsingFunction != ParsingFunction.InReadValueChunk ) {
                if ( readState != ReadState.Interactive ) {
                    return 0;
                }
                if ( parsingFunction == ParsingFunction.PartialTextValue ) {
                    incReadState = IncrementalReadState.ReadValueChunk_OnPartialValue;
                }
                else {
                    incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    nextNextParsingFunction = nextParsingFunction;
                    nextParsingFunction = parsingFunction;
                }
                parsingFunction = ParsingFunction.InReadValueChunk;
                readValueOffset = 0;
            }

            if ( count == 0 ) {
                return 0;
            }

            // read what is already cached in curNode
            int readCount = 0;
            int read = curNode.CopyTo( readValueOffset, buffer, index + readCount, count - readCount );
            readCount += read;
            readValueOffset += read;

            if ( readCount == count ) {
                // take care of surrogate pairs spanning between buffers
                char ch = buffer[index + count - 1];
                if ( XmlCharType.IsHighSurrogate(ch) ) {
                    readCount--;
                    readValueOffset--;
                    if ( readCount == 0 ) {
                        Throw( Res.Xml_NotEnoughSpaceForSurrogatePair );
                    }
                }
                return readCount;
            }

            // if on partial value, read the rest of it
            if ( incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue ) {
                curNode.SetValue( string.Empty );

                // read next chunk of text
                bool endOfValue = false;
                int startPos = 0;
                int endPos = 0;
                while ( readCount < count && !endOfValue ) {
                    int orChars = 0;

                    var tuple_0 = await ParseTextAsync(orChars).ConfigureAwait(false);
                    startPos = tuple_0.Item1;
                    endPos = tuple_0.Item2;
                    orChars = tuple_0.Item3;

                    endOfValue = tuple_0.Item4;

                    int copyCount = count - readCount;
                    if ( copyCount > endPos - startPos ) {
                        copyCount = endPos - startPos;
                    }
                    BlockCopyChars( ps.chars, startPos, buffer, ( index + readCount ), copyCount ); 

                    readCount += copyCount;
                    startPos += copyCount;
                }

                incReadState = endOfValue ? IncrementalReadState.ReadValueChunk_OnCachedValue : IncrementalReadState.ReadValueChunk_OnPartialValue;

                if ( readCount == count ) {
                    char ch = buffer[index + count - 1];
                    if ( XmlCharType.IsHighSurrogate(ch) ) {
                        readCount--;
                        startPos--;
                        if ( readCount == 0 ) {
                            Throw( Res.Xml_NotEnoughSpaceForSurrogatePair );
                        }
                    }
                }

                readValueOffset = 0;
                curNode.SetValue( ps.chars, startPos, endPos - startPos );
            }
            return readCount;
        }

        internal Task< int > DtdParserProxy_ReadDataAsync() {
            CheckAsyncCall();
            return this.ReadDataAsync();
        }

        internal async Task< int > DtdParserProxy_ParseNumericCharRefAsync( BufferBuilder internalSubsetBuilder ) {
            CheckAsyncCall();

            var tuple_1 = await this.ParseNumericCharRefAsync( true,  internalSubsetBuilder).ConfigureAwait(false);
            return tuple_1.Item2;

        }

        internal Task< int > DtdParserProxy_ParseNamedCharRefAsync( bool expand, BufferBuilder internalSubsetBuilder ) {
            CheckAsyncCall();
            return this.ParseNamedCharRefAsync( expand, internalSubsetBuilder );
        }

        internal async Task DtdParserProxy_ParsePIAsync( BufferBuilder sb ) {
            CheckAsyncCall();
            if ( sb == null ) {
                ParsingMode pm = parsingMode;
                parsingMode = ParsingMode.SkipNode;
                await ParsePIAsync( null ).ConfigureAwait(false);
                parsingMode = pm;
            }
            else {
                await ParsePIAsync( sb ).ConfigureAwait(false);
            }
        }
        
        internal async Task DtdParserProxy_ParseCommentAsync( BufferBuilder sb ) {
            CheckAsyncCall();
            Debug.Assert( parsingMode == ParsingMode.Full );

            try {
                if ( sb == null ) {
                    ParsingMode savedParsingMode = parsingMode;
                    parsingMode = ParsingMode.SkipNode;
                    await ParseCDataOrCommentAsync( XmlNodeType.Comment ).ConfigureAwait(false);
                    parsingMode = savedParsingMode;
                }
                else {
                    NodeData originalCurNode = curNode;

                    curNode = AddNode( index + attrCount + 1, index );
                    await ParseCDataOrCommentAsync( XmlNodeType.Comment ).ConfigureAwait(false);
                    curNode.CopyTo( 0, sb );

                    curNode = originalCurNode;
                }
            }
            catch ( XmlException e ) {
#if !SILVERLIGHT
                if ( e.ResString == Res.Xml_UnexpectedEOF && ps.entity != null ) {
                    SendValidationEvent( XmlSeverityType.Error, Res.Sch_ParEntityRefNesting, null, ps.LineNo, ps.LinePos );
                }   
                else {
                    throw;
                }
#else 
                throw e;
#endif
            }
        }

        internal async Task< Tuple<int, bool> > DtdParserProxy_PushEntityAsync(IDtdEntityInfo entity) {
            CheckAsyncCall();
            int entityId;

            bool retValue;
            if ( entity.IsExternal ) {
                if ( IsResolverNull ) {
                    entityId = -1;

                    return new Tuple<int, bool>(entityId, false);

                }
                retValue = await PushExternalEntityAsync( entity ).ConfigureAwait(false);
            }
            else {
                PushInternalEntity( entity );
                retValue = true;
            }
            entityId = ps.entityId;

            return new Tuple<int, bool>(entityId, retValue);

        }

        // SxS: The caller did not provide any SxS sensitive name or resource. No resource is being exposed either. 
        // It is OK to suppress SxS warning.
#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
#endif
        internal async Task< bool > DtdParserProxy_PushExternalSubsetAsync( string systemId, string publicId ) {
            CheckAsyncCall();
            Debug.Assert( parsingStatesStackTop == -1 );
            Debug.Assert( ( systemId != null && systemId.Length > 0 ) || ( publicId != null && publicId.Length > 0 ) );

            if (IsResolverNull) {
                return false;
            }

            // Resolve base URI
            if (ps.baseUri == null && !string.IsNullOrEmpty(ps.baseUriStr)) {
                ps.baseUri = xmlResolver.ResolveUri(null, ps.baseUriStr);
            }
            await PushExternalEntityOrSubsetAsync( publicId,  systemId, ps.baseUri,  null ).ConfigureAwait(false);

            ps.entity = null;
            ps.entityId = 0;

            Debug.Assert( ps.appendMode );
            int initialPos = ps.charPos;
            if ( v1Compat ) {
                await EatWhitespacesAsync( null ).ConfigureAwait(false);
            }
            if ( !await ParseXmlDeclarationAsync( true ).ConfigureAwait(false) ) {
                ps.charPos = initialPos;
            }

            return true;        
        }

        private Task InitStreamInputAsync( Uri baseUri, Stream stream, Encoding encoding ) {
            Debug.Assert( baseUri != null );
            return InitStreamInputAsync( baseUri, baseUri.ToString(), stream, null, 0, encoding );
        }

#if !SILVERLIGHT
        private Task InitStreamInputAsync( Uri baseUri, string baseUriStr, Stream stream, Encoding encoding ) {
            return InitStreamInputAsync( baseUri, baseUriStr, stream, null, 0, encoding );
        }
#endif

        private async Task InitStreamInputAsync( Uri baseUri, string baseUriStr, Stream stream, byte[] bytes, int byteCount, Encoding encoding ) {
            Debug.Assert( ps.charPos == 0 && ps.charsUsed == 0 && ps.textReader == null );
            Debug.Assert( baseUriStr != null );
            Debug.Assert( baseUri == null || ( baseUri.ToString().Equals( baseUriStr ) ) );

            ps.stream = stream;
            ps.baseUri = baseUri;
            ps.baseUriStr = baseUriStr;

            // take over the byte buffer allocated in XmlReader.Create, if available
            int bufferSize;
            if ( bytes != null ) {
                ps.bytes = bytes;
                ps.bytesUsed = byteCount;
                bufferSize = ps.bytes.Length;
            }
            else {
                // allocate the byte buffer 

                if (laterInitParam != null && laterInitParam.useAsync) {
                    bufferSize = AsyncBufferSize;
                }
                else {
                    bufferSize = XmlReader.CalcBufferSize(stream);
                }

                if ( ps.bytes == null || ps.bytes.Length < bufferSize ) {
                    ps.bytes = new byte[ bufferSize ];
                }
            }

            // allocate char buffer
            if ( ps.chars == null || ps.chars.Length < bufferSize + 1 ) {
                ps.chars = new char[ bufferSize + 1 ];
            }

            // make sure we have at least 4 bytes to detect the encoding (no preamble of System.Text supported encoding is longer than 4 bytes)
            ps.bytePos = 0;
            while ( ps.bytesUsed < 4 && ps.bytes.Length - ps.bytesUsed > 0 ) {
                int read = await stream.ReadAsync( ps.bytes, ps.bytesUsed, ps.bytes.Length - ps.bytesUsed ).ConfigureAwait(false);
                if ( read == 0 ) {
                    ps.isStreamEof = true;
                    break;
                }
                ps.bytesUsed += read;
            } 

            // detect & setup encoding
            if ( encoding == null ) {
                encoding = DetectEncoding();
            }
            SetupEncoding( encoding );

            // eat preamble 
            byte[] preamble = ps.encoding.GetPreamble();
            int preambleLen = preamble.Length;
            int i;
            for ( i = 0; i < preambleLen && i < ps.bytesUsed; i++ ) {
                if ( ps.bytes[i] != preamble[i] ) {
                    break;
                }
            }
            if ( i == preambleLen ) {
                ps.bytePos = preambleLen; 
            }

            documentStartBytePos = ps.bytePos;

            ps.eolNormalized = !normalize;

            // decode first characters
            ps.appendMode = true;
            await ReadDataAsync().ConfigureAwait(false);
        }
        private Task InitTextReaderInputAsync( string baseUriStr, TextReader input ) {
            return InitTextReaderInputAsync( baseUriStr, null, input );
        }

        private Task InitTextReaderInputAsync( string baseUriStr, Uri baseUri, TextReader input ) {
            Debug.Assert( ps.charPos == 0 && ps.charsUsed == 0 && ps.stream == null );
            Debug.Assert( baseUriStr != null );

            ps.textReader = input;
            ps.baseUriStr = baseUriStr;
            ps.baseUri = baseUri;
            
            if ( ps.chars == null ) {
                int bufferSize;
#if !ASYNC
                bufferSize = XmlReader.DefaultBufferSize;
#else
                if (laterInitParam != null && laterInitParam.useAsync) {
                    bufferSize = XmlReader.AsyncBufferSize;
                }
                else {
                    bufferSize = XmlReader.DefaultBufferSize;
                }
#endif
                ps.chars = new char[bufferSize + 1];
            }

            ps.encoding = Encoding.Unicode;
            ps.eolNormalized = !normalize;

            // read first characters
            ps.appendMode = true;
            return ReadDataAsync();
        }

        private Task ProcessDtdFromParserContextAsync(XmlParserContext context) {
            Debug.Assert( context != null && context.HasDtdInfo );

            switch ( dtdProcessing ) {
                case DtdProcessing.Prohibit:
                    ThrowWithoutLineInfo( Res.Xml_DtdIsProhibitedEx );
                    break;
                case DtdProcessing.Ignore:
                    // do nothing
                    break;
                case DtdProcessing.Parse:
                    return ParseDtdFromParserContextAsync();

                default:
                    Debug.Assert( false, "Unhandled DtdProcessing enumeration value." );
                    break;
            }

            return AsyncHelper.DoneTask;

        }

        // Switches the reader's encoding
        private Task SwitchEncodingAsync( Encoding newEncoding ) {
#if SILVERLIGHT 
            if ( ( newEncoding.WebName != ps.encoding.WebName || ps.decoder is SafeAsciiDecoder ) ) {
#else 
            if ( ( newEncoding.WebName != ps.encoding.WebName || ps.decoder is SafeAsciiDecoder ) && !afterResetState) {
#endif
                Debug.Assert( ps.stream != null );
                UnDecodeChars();
                ps.appendMode = false;
                SetupEncoding( newEncoding );
                return ReadDataAsync();
            }

            return AsyncHelper.DoneTask;

        }

        private Task SwitchEncodingToUTF8Async() {
            return SwitchEncodingAsync( new UTF8Encoding( true, true ) );
        }

        // Reads more data to the character buffer, discarding already parsed chars / decoded bytes.
        async Task< int > ReadDataAsync() {
            // Append Mode:  Append new bytes and characters to the buffers, do not rewrite them. Allocate new buffers
            //               if the current ones are full
            // Rewrite Mode: Reuse the buffers. If there is less than half of the char buffer left for new data, move 
            //               the characters that has not been parsed yet to the front of the buffer. Same for bytes.

            if ( ps.isEof ) {
                return 0;
            }

            int charsRead;
            if ( ps.appendMode ) {
                // the character buffer is full -> allocate a new one
                if ( ps.charsUsed == ps.chars.Length - 1 ) {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for ( int i = 0; i < attrCount; i++ ) {
                        nodes[index + i + 1].OnBufferInvalidated();
                    }

                    char[] newChars = new char[ ps.chars.Length * 2 ];
                    BlockCopyChars( ps.chars, 0, newChars, 0, ps.chars.Length );
                    ps.chars = newChars;
                }

                if ( ps.stream != null ) {
                    // the byte buffer is full -> allocate a new one
                    if ( ps.bytesUsed - ps.bytePos < MaxByteSequenceLen ) {
                        if ( ps.bytes.Length - ps.bytesUsed < MaxByteSequenceLen ) {
                            byte[] newBytes = new byte[ ps.bytes.Length * 2 ];
                            BlockCopy( ps.bytes, 0, newBytes, 0, ps.bytesUsed );
                            ps.bytes = newBytes;
                        }
                    }
                }

                charsRead = ps.chars.Length - ps.charsUsed - 1;
                if ( charsRead > ApproxXmlDeclLength ) {
                    charsRead = ApproxXmlDeclLength;
                }
            }
            else {
                int charsLen = ps.chars.Length;
                if ( charsLen - ps.charsUsed <= charsLen/2 ) {
                    // invalidate node values kept in buffer - applies to attribute values only
                    for ( int i = 0; i < attrCount; i++ ) {
                        nodes[index + i + 1].OnBufferInvalidated();
                    }

                    // move unparsed characters to front, unless the whole buffer contains unparsed characters
                    int copyCharsCount = ps.charsUsed - ps.charPos;
                    if ( copyCharsCount < charsLen - 1 ) {
                        ps.lineStartPos = ps.lineStartPos - ps.charPos;
                        if ( copyCharsCount > 0 ) {
                            BlockCopyChars( ps.chars, ps.charPos, ps.chars, 0, copyCharsCount );
                        }
                        ps.charPos = 0;
                        ps.charsUsed = copyCharsCount;
                    }
                    else {
                        char[] newChars = new char[ ps.chars.Length * 2 ];
                        BlockCopyChars( ps.chars, 0, newChars, 0, ps.chars.Length );
                        ps.chars = newChars;
                    }
                }

                if ( ps.stream != null ) {
                    // move undecoded bytes to the front to make some space in the byte buffer
                    int bytesLeft = ps.bytesUsed - ps.bytePos;
                    if ( bytesLeft <= MaxBytesToMove  ) {
                        if ( bytesLeft == 0 ) {
                            ps.bytesUsed = 0;
                        }
                        else {
                            BlockCopy( ps.bytes, ps.bytePos, ps.bytes, 0, bytesLeft );
                            ps.bytesUsed = bytesLeft;
                        }
                        ps.bytePos = 0;
                    }
                }
                charsRead = ps.chars.Length - ps.charsUsed - 1;
            }

            if ( ps.stream != null ) {
                if ( !ps.isStreamEof ) {
                    // read new bytes
                    if ( ps.bytePos == ps.bytesUsed && ps.bytes.Length - ps.bytesUsed > 0 ) {
                        int read = await ps.stream.ReadAsync( ps.bytes, ps.bytesUsed, ps.bytes.Length - ps.bytesUsed ).ConfigureAwait(false);
                        if ( read == 0 ) {
                            ps.isStreamEof = true;
                        }
                        ps.bytesUsed += read;
                    }
                }

                int originalBytePos = ps.bytePos;

                // decode chars
                charsRead = GetChars( charsRead );
                if ( charsRead == 0 && ps.bytePos != originalBytePos ) {
                    // GetChars consumed some bytes but it was not enough bytes to form a character -> try again
                    return await ReadDataAsync().ConfigureAwait(false);
                }
            }
            else if ( ps.textReader != null ) {
                // read chars
                charsRead = await ps.textReader.ReadAsync( ps.chars, ps.charsUsed, ps.chars.Length - ps.charsUsed - 1 ).ConfigureAwait(false);  
                ps.charsUsed += charsRead;
            }
            else {
                charsRead = 0;
            }

            RegisterConsumedCharacters(charsRead, InEntity);

            if ( charsRead == 0 ) {
                Debug.Assert ( ps.charsUsed < ps.chars.Length );
                ps.isEof = true;
            }
            ps.chars[ ps.charsUsed ] = (char)0;
            return charsRead;
        }

        // Parses the xml or text declaration and switched encoding if needed
        private async Task< bool > ParseXmlDeclarationAsync( bool isTextDecl ) {
            while ( ps.charsUsed - ps.charPos < 6 ) {  // minimum "<?xml "
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    goto NoXmlDecl;
                }
            }

            if ( !XmlConvert.StrEqual( ps.chars, ps.charPos, 5, XmlDeclarationBegining ) ||
                 xmlCharType.IsNameSingleChar( ps.chars[ps.charPos + 5] ) 
#if XML10_FIFTH_EDITION
                 || xmlCharType.IsNCNameHighSurrogateChar( ps.chars[ps.charPos + 5] ) 
#endif
                ) {
                goto NoXmlDecl;
            }

            if ( !isTextDecl ) {
                curNode.SetLineInfo( ps.LineNo, ps.LinePos + 2 );
                curNode.SetNamedNode( XmlNodeType.XmlDeclaration, Xml );
            }
            ps.charPos += 5;

            // parsing of text declarations cannot change global stringBuidler or curNode as we may be in the middle of a text node
            Debug.Assert( stringBuilder.Length == 0 || isTextDecl );
            BufferBuilder sb = isTextDecl ? new BufferBuilder() : stringBuilder;

            // parse version, encoding & standalone attributes
            int xmlDeclState = 0;   // <?xml (0) version='1.0' (1) encoding='__' (2) standalone='__' (3) ?>
            Encoding encoding = null;

            for (;;) {
                int originalSbLen = sb.Length;
                int wsCount = await EatWhitespacesAsync( xmlDeclState == 0 ? null : sb ).ConfigureAwait(false);

                // end of xml declaration
                if ( ps.chars[ps.charPos] == '?' ) {
                    sb.Length = originalSbLen;

                    if ( ps.chars[ps.charPos + 1] == '>' ) {
                        if ( xmlDeclState == 0 ) {
                            Throw( isTextDecl ? Res.Xml_InvalidTextDecl : Res.Xml_InvalidXmlDecl );
                        }

                        ps.charPos += 2;
                        if ( !isTextDecl ) {
                            curNode.SetValue( sb.ToString() );
                            sb.Length = 0;

                            nextParsingFunction = parsingFunction;
                            parsingFunction = ParsingFunction.ResetAttributesRootLevel;
                        }

                        // switch to encoding specified in xml declaration
                        if ( encoding == null ) {
                            if ( isTextDecl ) {
                                Throw( Res.Xml_InvalidTextDecl );
                            }
#if !SILVERLIGHT // Needed only for XmlTextReader
                            if ( afterResetState ) {
                                // check for invalid encoding switches to default encoding
                                string encodingName = ps.encoding.WebName;
                                if ( encodingName != "utf-8" && encodingName != "utf-16" &&
                                     encodingName != "utf-16BE" && !( ps.encoding is Ucs4Encoding ) ) {
                                    Throw( Res.Xml_EncodingSwitchAfterResetState, ( ps.encoding.GetByteCount( "A" ) == 1 ) ? "UTF-8" : "UTF-16" );
                                }
                            }
#endif
                            if ( ps.decoder is SafeAsciiDecoder ) {
                                await SwitchEncodingToUTF8Async().ConfigureAwait(false);
                            }
                        }
                        else {
                            await SwitchEncodingAsync( encoding ).ConfigureAwait(false);
                        }
                        ps.appendMode = false; 
                        return true;
                    }
                    else if ( ps.charPos + 1 == ps.charsUsed ) {
                        goto ReadData;
                    }
                    else {
                        ThrowUnexpectedToken( "'>'" );
                    }
                }

                if ( wsCount == 0 && xmlDeclState != 0 ) {
                    ThrowUnexpectedToken( "?>" );
                }
    
                // read attribute name            
                int nameEndPos = await ParseNameAsync().ConfigureAwait(false);

                NodeData attr = null;
                switch ( ps.chars[ps.charPos] ) {
                    case 'v':
                        if ( XmlConvert.StrEqual( ps.chars, ps.charPos, nameEndPos - ps.charPos, "version" ) && xmlDeclState == 0 ) {
                            if ( !isTextDecl ) {
                                attr = AddAttributeNoChecks( "version", 1 );
                            }
                            break;
                        }
                        goto default;
                    case 'e':
                        if ( XmlConvert.StrEqual( ps.chars, ps.charPos, nameEndPos - ps.charPos, "encoding" ) && 
                            ( xmlDeclState == 1 || ( isTextDecl && xmlDeclState == 0 ) ) ) {
                            if ( !isTextDecl ) {
                                attr = AddAttributeNoChecks( "encoding", 1 );
                            }
                            xmlDeclState = 1;
                            break;
                        }
                        goto default;
                    case 's':
                        if ( XmlConvert.StrEqual( ps.chars, ps.charPos, nameEndPos - ps.charPos, "standalone" ) &&
                             ( xmlDeclState == 1 || xmlDeclState == 2 ) && !isTextDecl ) {
                            if ( !isTextDecl ) {
                                attr = AddAttributeNoChecks( "standalone", 1 );
                            }
                            xmlDeclState = 2;
                            break;
                        }
                        goto default;
                    default:
                        Throw( isTextDecl ? Res.Xml_InvalidTextDecl : Res.Xml_InvalidXmlDecl );
                        break;
                }
                if ( !isTextDecl ) {
                    attr.SetLineInfo( ps.LineNo, ps.LinePos );
                }
                sb.Append( ps.chars, ps.charPos, nameEndPos - ps.charPos );
                ps.charPos = nameEndPos;

                // parse equals and quote char; 
                if ( ps.chars[ps.charPos] != '=' ) {
                    await EatWhitespacesAsync( sb ).ConfigureAwait(false);
                    if ( ps.chars[ps.charPos] != '=' ) {
                        ThrowUnexpectedToken( "=" );
                    }
                }
                sb.Append( '=' );
                ps.charPos++;

                char quoteChar = ps.chars[ps.charPos];
                if ( quoteChar != '"' && quoteChar != '\'' ) {
                    await EatWhitespacesAsync( sb ).ConfigureAwait(false);
                    quoteChar = ps.chars[ps.charPos];
                    if ( quoteChar != '"' && quoteChar != '\'' ) {
                        ThrowUnexpectedToken( "\"", "'" );
                    }
                }
                sb.Append( quoteChar );
                ps.charPos++;
                if ( !isTextDecl ) {
                    attr.quoteChar = quoteChar;
                    attr.SetLineInfo2( ps.LineNo, ps.LinePos );
                }

                // parse attribute value
                int pos = ps.charPos;
                char[] chars;
            Continue:
                chars = ps.chars;

#if SILVERLIGHT
                while (xmlCharType.IsAttributeValueChar(chars[pos])) {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
            unsafe {
                while (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)) {
                    pos++;
                }
            }
#endif

                if ( ps.chars[pos] == quoteChar ) {
                    switch ( xmlDeclState ) {
                        // version
                        case 0:
#if XML10_FIFTH_EDITION
                            //  VersionNum ::= '1.' [0-9]+   (starting with XML Fifth Edition)
                            if ( pos - ps.charPos >= 3 && 
                                 ps.chars[ps.charPos] == '1' && 
                                 ps.chars[ps.charPos + 1] == '.' && 
                                 XmlCharType.IsOnlyDigits( ps.chars, ps.charPos + 2, pos - ps.charPos - 2 ) ) {
#else 
                            // VersionNum  ::=  '1.0'        (XML Fourth Edition and earlier)
                            if ( XmlConvert.StrEqual( ps.chars, ps.charPos, pos - ps.charPos, "1.0" ) ) {
#endif
                                if ( !isTextDecl ) {
                                    attr.SetValue( ps.chars, ps.charPos, pos - ps.charPos );
                                }
                                xmlDeclState = 1;
                            }
                            else {
                                string badVersion = new string( ps.chars, ps.charPos, pos - ps.charPos );
                                Throw( Res.Xml_InvalidVersionNumber, badVersion );
                            }
                            break;
                        case 1:
                            string encName = new string( ps.chars, ps.charPos, pos - ps.charPos );
                            encoding = CheckEncoding( encName );
                            if ( !isTextDecl ) {
                                attr.SetValue( encName );
                            }
                            xmlDeclState = 2;
                            break;
                        case 2:
                            if ( XmlConvert.StrEqual( ps.chars, ps.charPos, pos - ps.charPos, "yes" ) ) {
                                this.standalone = true;
                            }
                            else if ( XmlConvert.StrEqual( ps.chars, ps.charPos, pos - ps.charPos, "no" ) ) {
                                this.standalone = false;
                            }
                            else {
                                Debug.Assert( !isTextDecl );
                                Throw( Res.Xml_InvalidXmlDecl, ps.LineNo, ps.LinePos - 1 );
                            }
                            if ( !isTextDecl ) {
                                attr.SetValue( ps.chars, ps.charPos, pos - ps.charPos );
                            }
                            xmlDeclState = 3;
                            break;
                        default:
                            Debug.Assert( false );
                            break;
                    }
                    sb.Append( chars, ps.charPos, pos - ps.charPos );
                    sb.Append( quoteChar );
                    ps.charPos = pos + 1;
                    continue;
                }
                else if ( pos == ps.charsUsed ) {
                    if ( await ReadDataAsync().ConfigureAwait(false) != 0 ) {
                        goto Continue;
                    }
                    else {
                        Throw( Res.Xml_UnclosedQuote );
                    }
                }
                else {
                    Throw( isTextDecl ? Res.Xml_InvalidTextDecl : Res.Xml_InvalidXmlDecl );
                }
                            
            ReadData:
                if ( ps.isEof || await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( Res.Xml_UnexpectedEOF1 );
                }
            }

        NoXmlDecl:
            // no xml declaration
            if ( !isTextDecl ) {
                parsingFunction = nextParsingFunction;
            }
#if !SILVERLIGHT // Needed only for XmlTextReader
            if ( afterResetState ) {
                // check for invalid encoding switches to default encoding
                string encodingName = ps.encoding.WebName;
                if ( encodingName != "utf-8" && encodingName != "utf-16" &&
                    encodingName != "utf-16BE" && !( ps.encoding is Ucs4Encoding ) ) {
                    Throw( Res.Xml_EncodingSwitchAfterResetState, ( ps.encoding.GetByteCount( "A" ) == 1 ) ? "UTF-8" : "UTF-16" );
                }
            }
#endif
            if ( ps.decoder is SafeAsciiDecoder ) {
                await SwitchEncodingToUTF8Async().ConfigureAwait(false);
            }
            ps.appendMode = false;
            return false;
        }

      

        // Parses the document content, no async keyword for perf optimize
        private Task<bool> ParseDocumentContentAsync() {
            for (; ; ) {
                bool needMoreChars = false;
                int pos = ps.charPos;
                char[] chars = ps.chars;

                // some tag
                if (chars[pos] == '<') {
                    needMoreChars = true;
                    if (ps.charsUsed - pos < 4) // minimum  "<a/>"
                        return ParseDocumentContentAsync_ReadData(needMoreChars);
                    pos++;
                    switch (chars[pos]) {
                        // processing instruction
                        case '?':
                            ps.charPos = pos + 1;
                            return ParsePIAsync().ContinueBoolTaskFuncWhenFalse(ParseDocumentContentAsync);
                        case '!':
                            pos++;
                            if (ps.charsUsed - pos < 2) // minimum characters expected "--"
                                return ParseDocumentContentAsync_ReadData(needMoreChars);
                            // comment
                            if (chars[pos] == '-') {
                                if (chars[pos + 1] == '-') {
                                    ps.charPos = pos + 2;
                                    return ParseCommentAsync().ContinueBoolTaskFuncWhenFalse(ParseDocumentContentAsync);
                                }
                                else {
                                    ThrowUnexpectedToken(pos + 1, "-");
                                }
                            }
                            // CDATA section
                            else if (chars[pos] == '[') {
                                if (fragmentType != XmlNodeType.Document) {
                                    pos++;
                                    if (ps.charsUsed - pos < 6) {
                                        return ParseDocumentContentAsync_ReadData(needMoreChars);
                                    }
                                    if (XmlConvert.StrEqual(chars, pos, 6, "CDATA[")) {
                                        ps.charPos = pos + 6;
                                        return ParseCDataAsync().CallBoolTaskFuncWhenFinish(ParseDocumentContentAsync_CData);
                                    }
                                    else {
                                        ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else {
                                    Throw(ps.charPos, Res.Xml_InvalidRootData);
                                }
                            }
                            // DOCTYPE declaration
                            else {
                                if (fragmentType == XmlNodeType.Document || fragmentType == XmlNodeType.None) {
                                    fragmentType = XmlNodeType.Document;
                                    ps.charPos = pos;
                                    return ParseDoctypeDeclAsync().ContinueBoolTaskFuncWhenFalse(ParseDocumentContentAsync);
                                }
                                else {
                                    if (ParseUnexpectedToken(pos) == "DOCTYPE") {
                                        Throw(Res.Xml_BadDTDLocation);
                                    }
                                    else {
                                        ThrowUnexpectedToken(pos, "<!--", "<[CDATA[");
                                    }
                                }
                            }
                            break;
                        case '/':
                            Throw(pos + 1, Res.Xml_UnexpectedEndTag);
                            break;
                        // document element start tag
                        default:
                            if (rootElementParsed) {
                                if (fragmentType == XmlNodeType.Document) {
                                    Throw(pos, Res.Xml_MultipleRoots);
                                }
                                if (fragmentType == XmlNodeType.None) {
                                    fragmentType = XmlNodeType.Element;
                                }
                            }
                            ps.charPos = pos;
                            rootElementParsed = true;
                            return ParseElementAsync().ReturnTaskBoolWhenFinish(true);
                    }
                }
                else if (chars[pos] == '&') {
                    return ParseDocumentContentAsync_ParseEntity();
                }
                // end of buffer
                else if (pos == ps.charsUsed || (v1Compat && chars[pos] == 0x0)) {
                    return ParseDocumentContentAsync_ReadData(needMoreChars);
                }
                // something else -> root level whitespaces
                else {
                    if (fragmentType == XmlNodeType.Document) {
                        return ParseRootLevelWhitespaceAsync().ContinueBoolTaskFuncWhenFalse(ParseDocumentContentAsync);
                    }
                    else {
                        return ParseDocumentContentAsync_WhiteSpace();
                    }
                }

                Debug.Assert(pos == ps.charsUsed && !ps.isEof);
            }
        }

        private Task<bool> ParseDocumentContentAsync_CData() {
            if (fragmentType == XmlNodeType.None) {
                fragmentType = XmlNodeType.Element;
            }
            return AsyncHelper.DoneTaskTrue;
        }

        private async Task<bool> ParseDocumentContentAsync_ParseEntity() {

            int pos = ps.charPos;

            if (fragmentType == XmlNodeType.Document) {
                Throw(pos, Res.Xml_InvalidRootData);
                return false;
            }
            else {
                if (fragmentType == XmlNodeType.None) {
                    fragmentType = XmlNodeType.Element;
                }
                
                var tuple_3 = await HandleEntityReferenceAsync(false, EntityExpandType.OnlyGeneral).ConfigureAwait(false);

                switch (tuple_3.Item2) {

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                    case EntityType.Unexpanded:
                        if (parsingFunction == ParsingFunction.EntityReference) {
                            parsingFunction = nextParsingFunction;
                        }
                        await ParseEntityReferenceAsync().ConfigureAwait(false);
                        return true;
#endif
                    case EntityType.CharacterDec:
                    case EntityType.CharacterHex:
                    case EntityType.CharacterNamed:
                        if (await ParseTextAsync().ConfigureAwait(false)) {
                            return true;
                        }
                        return await ParseDocumentContentAsync().ConfigureAwait(false);
                    default:
                        return await ParseDocumentContentAsync().ConfigureAwait(false);
                }
            }

        }

        private Task<bool> ParseDocumentContentAsync_WhiteSpace() {
            Task<bool> task = ParseTextAsync();
            if (task.IsSuccess()) {
                if (task.Result) {
                    if (fragmentType == XmlNodeType.None && curNode.type == XmlNodeType.Text) {
                        fragmentType = XmlNodeType.Element;
                    }
                    return AsyncHelper.DoneTaskTrue;
                }
                else {
                    return ParseDocumentContentAsync();
                }
            }
            else {
                return _ParseDocumentContentAsync_WhiteSpace(task);
            }
        }

        private async Task<bool> _ParseDocumentContentAsync_WhiteSpace(Task<bool> task) {
            if (await task.ConfigureAwait(false)) {
                if (fragmentType == XmlNodeType.None && curNode.type == XmlNodeType.Text) {
                    fragmentType = XmlNodeType.Element;
                }
                return true;
            }
            return await ParseDocumentContentAsync().ConfigureAwait(false);
        }

        private async Task<bool> ParseDocumentContentAsync_ReadData(bool needMoreChars) {
            // read new characters into the buffer
            if (await ReadDataAsync().ConfigureAwait(false) != 0) {
                return await ParseDocumentContentAsync().ConfigureAwait(false);
            }
            else {
                if (needMoreChars) {
                    Throw(Res.Xml_InvalidRootData);
                }

                if (InEntity) {
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                        HandleEntityEnd( true );
#else
                    if (HandleEntityEnd(true)) {
                        SetupEndEntityNodeInContent();
                        return true;
                    }
#endif
                    return await ParseDocumentContentAsync().ConfigureAwait(false);
                }
                Debug.Assert(index == 0);

                if (!rootElementParsed && fragmentType == XmlNodeType.Document) {
                    ThrowWithoutLineInfo(Res.Xml_MissingRoot);
                }
                if (fragmentType == XmlNodeType.None) {
                    fragmentType = rootElementParsed ? XmlNodeType.Document : XmlNodeType.Element;
                }
                OnEof();
                return false;
            }
        }

        
        // Parses element content
        private Task<bool> ParseElementContentAsync() {
            for (; ; ) {
                int pos = ps.charPos;
                char[] chars = ps.chars;

                switch (chars[pos]) {
                    // some tag
                    case '<':
                        switch (chars[pos + 1]) {
                            // processing instruction
                            case '?':
                                ps.charPos = pos + 2;
                                return ParsePIAsync().ContinueBoolTaskFuncWhenFalse(ParseElementContentAsync);
                            case '!':
                                pos += 2;
                                if (ps.charsUsed - pos < 2)
                                    return ParseElementContent_ReadData();
                                // comment
                                if (chars[pos] == '-') {
                                    if (chars[pos + 1] == '-') {
                                        ps.charPos = pos + 2;
                                        return ParseCommentAsync().ContinueBoolTaskFuncWhenFalse(ParseElementContentAsync);
                                    }
                                    else {
                                        ThrowUnexpectedToken(pos + 1, "-");
                                    }
                                }
                                // CDATA section
                                else if (chars[pos] == '[') {
                                    pos++;
                                    if (ps.charsUsed - pos < 6) {
                                        return ParseElementContent_ReadData();
                                    }
                                    if (XmlConvert.StrEqual(chars, pos, 6, "CDATA[")) {
                                        ps.charPos = pos + 6;
                                        return ParseCDataAsync().ReturnTaskBoolWhenFinish(true);
                                    }
                                    else {
                                        ThrowUnexpectedToken(pos, "CDATA[");
                                    }
                                }
                                else {

                                    if (ParseUnexpectedToken(pos) == "DOCTYPE") {
                                        Throw(Res.Xml_BadDTDLocation);
                                    }
                                    else {
                                        ThrowUnexpectedToken(pos, "<!--", "<[CDATA[");
                                    }
                                }
                                break;
                            // element end tag
                            case '/':
                                ps.charPos = pos + 2;
                                return ParseEndElementAsync().ReturnTaskBoolWhenFinish(true);
                            default:
                                // end of buffer
                                if (pos + 1 == ps.charsUsed) {
                                    return ParseElementContent_ReadData();
                                }
                                else {
                                    // element start tag
                                    ps.charPos = pos + 1;
                                    return ParseElementAsync().ReturnTaskBoolWhenFinish(true);
                                }
                        }
                        break;
                    case '&':
                        return ParseTextAsync().ContinueBoolTaskFuncWhenFalse(ParseElementContentAsync);
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed) {
                            return ParseElementContent_ReadData();
                        }
                        else {
                            // text node, whitespace or entity reference
                            return ParseTextAsync().ContinueBoolTaskFuncWhenFalse(ParseElementContentAsync);
                        }
                }
            }
        }

        private async Task<bool> ParseElementContent_ReadData() {
            // read new characters into the buffer
            if (await ReadDataAsync().ConfigureAwait(false) == 0) {
                if (ps.charsUsed - ps.charPos != 0) {
                    ThrowUnclosedElements();
                }
                if (!InEntity) {
                    if (index == 0 && fragmentType != XmlNodeType.Document) {
                        OnEof();
                        return false;
                    }
                    ThrowUnclosedElements();
                }
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                    HandleEntityEnd( true );
#else
                if (HandleEntityEnd(true)) {
                    SetupEndEntityNodeInContent();
                    return true;
                }
#endif
            }
            return await ParseElementContentAsync().ConfigureAwait(false);
        }

        // Parses the element start tag
        private Task ParseElementAsync() {
            int pos = ps.charPos;
            char[] chars = ps.chars;
            int colonPos = -1;

            curNode.SetLineInfo(ps.LineNo, ps.LinePos);

            // PERF: we intentionally don't call ParseQName here to parse the element name unless a special 
        // case occurs (like end of buffer, invalid name char)
        ContinueStartName:
            // check element name start char
            unsafe {
#if SILVERLIGHT
                if ( xmlCharType.IsStartNCNameSingleChar( chars[pos] ) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                if ((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartNameSC) != 0) {
#endif
                    pos++;
                }

#if XML10_FIFTH_EDITION
                else if ( pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar(chars[pos + 1], chars[pos])) {
                    pos += 2;
                }
#endif
                else {
                    goto ParseQNameSlow;
                }
            }

        ContinueName:
            unsafe {
                // parse element name
                for (; ; ) {
#if SILVERLIGHT
                    if ( xmlCharType.IsNCNameSingleChar( chars[pos] ) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                    if (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCNameSC) != 0)) {
#endif
                        pos++;
                    }

#if XML10_FIFTH_EDITION
                    else if ( pos < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar(chars[pos + 1], chars[pos])) {
                        pos += 2;
                    }
#endif
                    else {
                        break;
                    }
                }
            }

            // colon -> save prefix end position and check next char if it's name start char
            if (chars[pos] == ':') {
                if (colonPos != -1) {
                    if (supportNamespaces) {
                        Throw(pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(':', '\0'));
                    }
                    else {
                        pos++;
                        goto ContinueName;
                    }
                }
                else {
                    colonPos = pos;
                    pos++;
                    goto ContinueStartName;
                }
            }
            else if (pos + 1 < ps.charsUsed) {
                goto SetElement;
            }

        ParseQNameSlow:
            Task< Tuple<int,int> > parseQNameTask = ParseQNameAsync();
            return ParseElementAsync_ContinueWithSetElement(parseQNameTask);

        SetElement:
            return ParseElementAsync_SetElement(colonPos, pos);
        }

        private Task ParseElementAsync_ContinueWithSetElement(Task<Tuple<int,int>> task) {
            if (task.IsSuccess()) {
                var tuple_4 = task.Result;
                int colonPos = tuple_4.Item1;
                int pos = tuple_4.Item2;
                return ParseElementAsync_SetElement(colonPos, pos);
            }
            else { 
                return _ParseElementAsync_ContinueWithSetElement(task);
            }
        }

        private async Task _ParseElementAsync_ContinueWithSetElement(Task<Tuple<int, int>> task) {
            var tuple_4 = await task.ConfigureAwait(false);
            int colonPos = tuple_4.Item1;
            int pos = tuple_4.Item2;
            await ParseElementAsync_SetElement(colonPos, pos).ConfigureAwait(false); 
        }

        private Task ParseElementAsync_SetElement(int colonPos, int pos) {
            char[] chars = ps.chars;

            // push namespace context
            namespaceManager.PushScope();

            // init the NodeData class
            if (colonPos == -1 || !supportNamespaces) {
                curNode.SetNamedNode(XmlNodeType.Element,
                                      nameTable.Add(chars, ps.charPos, pos - ps.charPos));
            }
            else {
                int startPos = ps.charPos;
                int prefixLen = colonPos - startPos;
                if (prefixLen == lastPrefix.Length && XmlConvert.StrEqual(chars, startPos, prefixLen, lastPrefix)) {
                    curNode.SetNamedNode(XmlNodeType.Element,
                                          nameTable.Add(chars, colonPos + 1, pos - colonPos - 1),
                                          lastPrefix,
                                          null);
                }
                else {
                    curNode.SetNamedNode(XmlNodeType.Element,
                                          nameTable.Add(chars, colonPos + 1, pos - colonPos - 1),
                                          nameTable.Add(chars, ps.charPos, prefixLen),
                                          null);
                    lastPrefix = curNode.prefix;
                }
            }

            char ch = chars[pos];
            // white space after element name -> there are probably some attributes
            bool isWs;

#if SILVERLIGHT
            isWs = xmlCharType.IsWhiteSpace(ch);
#else // Optimization due to the lack of inlining when a method uses byte*
            unsafe {
                isWs = ((xmlCharType.charProperties[ch] & XmlCharType.fWhitespace) != 0);
            }
#endif

            ps.charPos = pos;
            if (isWs) {
                return ParseAttributesAsync();
            }
            // no attributes
            else {
                return ParseElementAsync_NoAttributes();
            }

        }

        private Task ParseElementAsync_NoAttributes() {
            int pos = ps.charPos;
            char[] chars = ps.chars;
            char ch = chars[pos];
             // non-empty element
            if (ch == '>') {
                ps.charPos = pos + 1;
                parsingFunction = ParsingFunction.MoveToElementContent;
            }
            // empty element
            else if (ch == '/') {
                if (pos + 1 == ps.charsUsed) {
                    ps.charPos = pos;
                    return ParseElementAsync_ReadData(pos);
                }
                if (chars[pos + 1] == '>') {
                    curNode.IsEmptyElement = true;
                    nextParsingFunction = parsingFunction;
                    parsingFunction = ParsingFunction.PopEmptyElementContext;
                    ps.charPos = pos + 2;
                }
                else {
                    ThrowUnexpectedToken(pos, ">");
                }
            }
            // something else after the element name
            else {
                Throw(pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(chars, ps.charsUsed, pos));
            }

            // add default attributes & strip spaces in attributes with type other than CDATA
            if (addDefaultAttributesAndNormalize) {
                AddDefaultAttributesAndNormalize();
            }

            // lookup element namespace
            ElementNamespaceLookup();

            return AsyncHelper.DoneTask;
        }

        private async Task ParseElementAsync_ReadData(int pos) {
            if (await ReadDataAsync().ConfigureAwait(false) == 0) {
                Throw(pos, Res.Xml_UnexpectedEOF, ">");
            }

            await ParseElementAsync_NoAttributes().ConfigureAwait(false);
        }

        private Task ParseEndElementAsync() { 
            NodeData startTagNode = nodes[index - 1];

            int prefLen = startTagNode.prefix.Length;
            int locLen = startTagNode.localName.Length;

            if (ps.charsUsed - ps.charPos < prefLen + locLen + 1) {
                return _ParseEndElmentAsync();
            }

            return ParseEndElementAsync_CheckNameAndParse();
        }

        private async Task _ParseEndElmentAsync() {
            await ParseEndElmentAsync_PrepareData().ConfigureAwait(false);
            await ParseEndElementAsync_CheckNameAndParse().ConfigureAwait(false);
        }

        private async Task ParseEndElmentAsync_PrepareData() {
            // check if the end tag name equals start tag name
            NodeData startTagNode = nodes[index - 1];

            int prefLen = startTagNode.prefix.Length;
            int locLen = startTagNode.localName.Length;

            while (ps.charsUsed - ps.charPos < prefLen + locLen + 1) {
                if (await ReadDataAsync().ConfigureAwait(false) == 0) {
                    break;
                }
            }

        }

        private Task ParseEndElementAsync_CheckNameAndParse() {
            NodeData startTagNode = nodes[index - 1];
            int prefLen = startTagNode.prefix.Length;
            int locLen = startTagNode.localName.Length;

            int nameLen;
            char[] chars = ps.chars;
            if (startTagNode.prefix.Length == 0) {
                if (!XmlConvert.StrEqual(chars, ps.charPos, locLen, startTagNode.localName)) {
                    return ThrowTagMismatchAsync(startTagNode);
                }
                nameLen = locLen;
            }
            else {
                int colonPos = ps.charPos + prefLen;
                if (!XmlConvert.StrEqual(chars, ps.charPos, prefLen, startTagNode.prefix) ||
                        chars[colonPos] != ':' ||
                        !XmlConvert.StrEqual(chars, colonPos + 1, locLen, startTagNode.localName)) {
                    return ThrowTagMismatchAsync(startTagNode);
                }
                nameLen = locLen + prefLen + 1;
            }
            LineInfo endTagLineInfo = new LineInfo(ps.lineNo, ps.LinePos);
            return ParseEndElementAsync_Finish(nameLen, startTagNode, endTagLineInfo);
        }

        private enum ParseEndElementParseFunction {
            CheckEndTag,
            ReadData,
            Done
        }

        private ParseEndElementParseFunction parseEndElement_NextFunc;

        private Task ParseEndElementAsync_Finish(int nameLen, NodeData startTagNode, LineInfo endTagLineInfo) {
            Task task = ParseEndElementAsync_CheckEndTag(nameLen, startTagNode, endTagLineInfo);
            while (true) {
                if (!task.IsSuccess()) {
                    return ParseEndElementAsync_Finish(task, nameLen, startTagNode, endTagLineInfo);
                }

                switch (parseEndElement_NextFunc)
                {
                    case ParseEndElementParseFunction.CheckEndTag:
                        task = ParseEndElementAsync_CheckEndTag(nameLen, startTagNode, endTagLineInfo);
                        break;
                    case ParseEndElementParseFunction.ReadData:
                        task = ParseEndElementAsync_ReadData();
                        break;
                    case ParseEndElementParseFunction.Done:
                        return task;
                }
            }
        }

        private async Task ParseEndElementAsync_Finish(Task task, int nameLen, NodeData startTagNode, LineInfo endTagLineInfo) {
            
            while (true) {
                await task.ConfigureAwait(false);
                switch (parseEndElement_NextFunc) {
                    case ParseEndElementParseFunction.CheckEndTag:
                        task = ParseEndElementAsync_CheckEndTag(nameLen, startTagNode, endTagLineInfo);
                        break;
                    case ParseEndElementParseFunction.ReadData:
                        task = ParseEndElementAsync_ReadData();
                        break;
                    case ParseEndElementParseFunction.Done:
                        return;
                }
            }
        }

        private Task ParseEndElementAsync_CheckEndTag(int nameLen, NodeData startTagNode, LineInfo endTagLineInfo) {

            int pos;
            char[] chars;
            for (; ; ) {
                pos = ps.charPos + nameLen;
                chars = ps.chars;

                if (pos == ps.charsUsed) {
                    parseEndElement_NextFunc = ParseEndElementParseFunction.ReadData;
                    return AsyncHelper.DoneTask;
                }

                bool tagMismatch = false;

                unsafe {
#if SILVERLIGHT
                    if ( xmlCharType.IsNCNameSingleChar( chars[pos] ) ||
#else // Optimization due to the lack of inlining when a method uses byte*
                    if (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCNameSC) != 0) ||
#endif
 (chars[pos] == ':')
#if XML10_FIFTH_EDITION
                         || xmlCharType.IsNCNameHighSurrogateChar( chars[pos] ) 
#endif
) {
                        tagMismatch = true;
                    }
                }
                                
                if (tagMismatch) {
                    return ThrowTagMismatchAsync(startTagNode);
                }
                
                // eat whitespaces
                if (chars[pos] != '>') {
                    char tmpCh;
                    while (xmlCharType.IsWhiteSpace(tmpCh = chars[pos])) {
                        pos++;
                        switch (tmpCh) {
                            case (char)0xA:
                                OnNewLine(pos);
                                continue;
                            case (char)0xD:
                                if (chars[pos] == (char)0xA) {
                                    pos++;
                                }
                                else if (pos == ps.charsUsed && !ps.isEof) {
                                    break;
                                }
                                OnNewLine(pos);
                                continue;
                        }
                    }
                }

                if (chars[pos] == '>') {
                    break;
                }
                else if (pos == ps.charsUsed) {
                    parseEndElement_NextFunc = ParseEndElementParseFunction.ReadData;
                    return AsyncHelper.DoneTask;
                }
                else {
                    ThrowUnexpectedToken(pos, ">");
                }

                Debug.Assert(false, "We should never get to this point.");
            }

            Debug.Assert(index > 0);
            index--;
            curNode = nodes[index];

            // set the element data
            Debug.Assert(curNode == startTagNode);
            startTagNode.lineInfo = endTagLineInfo;
            startTagNode.type = XmlNodeType.EndElement;
            ps.charPos = pos + 1;

            // set next parsing function
            nextParsingFunction = (index > 0) ? parsingFunction : ParsingFunction.DocumentContent;
            parsingFunction = ParsingFunction.PopElementContext;

            parseEndElement_NextFunc = ParseEndElementParseFunction.Done;
            return AsyncHelper.DoneTask;
        }

        private async Task ParseEndElementAsync_ReadData() {
            if (await ReadDataAsync().ConfigureAwait(false) == 0) {
                ThrowUnclosedElements();
            }
            parseEndElement_NextFunc = ParseEndElementParseFunction.CheckEndTag;
            return;
        }

        private async Task ThrowTagMismatchAsync( NodeData startTag ) {
            if ( startTag.type == XmlNodeType.Element ) { 
                // parse the bad name
                int colonPos;

                var tuple_5 = await ParseQNameAsync().ConfigureAwait(false);
                colonPos = tuple_5.Item1;

                int endPos = tuple_5.Item2;

                string[] args = new string[4];
                args[0] = startTag.GetNameWPrefix( nameTable );
                args[1] = startTag.lineInfo.lineNo.ToString(CultureInfo.InvariantCulture);
                args[2] = startTag.lineInfo.linePos.ToString(CultureInfo.InvariantCulture);
                args[3] = new string( ps.chars, ps.charPos, endPos - ps.charPos );
                Throw( Res.Xml_TagMismatchEx, args );
            }
            else {
                Debug.Assert( startTag.type == XmlNodeType.EntityReference );
                Throw( Res.Xml_UnexpectedEndTag );
            }
        }

        // Reads the attributes
        private async Task ParseAttributesAsync() {
            int pos = ps.charPos;
            char[] chars = ps.chars;
            NodeData attr = null;

            Debug.Assert( attrCount == 0 );

            for (;;) {
                // eat whitespaces
                int lineNoDelta = 0;
                char tmpch0;

#if SILVERLIGHT
                {
                    while (xmlCharType.IsWhiteSpace(tmpch0 = chars[pos])) {
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[tmpch0 = chars[pos]] & XmlCharType.fWhitespace) != 0)) {
#endif
                        if ( tmpch0 == (char)0xA ) {
                            OnNewLine( pos + 1 );
                            lineNoDelta++;
                        }
                        else if ( tmpch0 == (char)0xD ) {
                            if ( chars[pos+1] == (char)0xA ) {
                                OnNewLine( pos + 2 );
                                lineNoDelta++;
                                pos++;
                            }
                            else if ( pos+1 != ps.charsUsed ) {
                                OnNewLine( pos + 1 );
                                lineNoDelta++;
                            }
                            else {
                                ps.charPos = pos;
                                goto ReadData;
                            }
                        }
                        pos++;
                    }
                }

                char tmpch1;
                int startNameCharSize = 0;

                unsafe {
#if SILVERLIGHT
                    if ( xmlCharType.IsStartNCNameSingleChar( tmpch1 = chars[pos]) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                    if ((xmlCharType.charProperties[tmpch1 = chars[pos]] & XmlCharType.fNCStartNameSC) != 0) {
#endif
                        startNameCharSize = 1;
                    }
#if XML10_FIFTH_EDITION
                    else if ( pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar( chars[pos + 1], tmpch1 ) ) {
                        startNameCharSize = 2;
                    }
#endif
                }

                if ( startNameCharSize == 0 ) {
                    // element end
                    if ( tmpch1 == '>' ) {
                        Debug.Assert( curNode.type == XmlNodeType.Element );
                        ps.charPos = pos + 1;
                        parsingFunction = ParsingFunction.MoveToElementContent;
                        goto End;
                    }
                    // empty element end
                    else if ( tmpch1 == '/' ) {
                        Debug.Assert( curNode.type == XmlNodeType.Element );
                        if ( pos+1 == ps.charsUsed ) {
                            goto ReadData;
                        }
                        if ( chars[pos+1] == '>' ) {
                            ps.charPos = pos + 2;
                            curNode.IsEmptyElement = true;
                            nextParsingFunction = parsingFunction;
                            parsingFunction = ParsingFunction.PopEmptyElementContext;
                            goto End;
                        }
                        else {
                            ThrowUnexpectedToken( pos + 1, ">" );
                        }
                    }
                    else if ( pos == ps.charsUsed ) {
                        goto ReadData;
                    }
                    else if ( tmpch1 != ':' || supportNamespaces ) {
                        Throw( pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionArgs( chars, ps.charsUsed, pos ) );
                    }
                }

                if ( pos == ps.charPos ) {
                    ThrowExpectingWhitespace(pos);
                }
                ps.charPos = pos;

                // save attribute name line position
                int attrNameLinePos = ps.LinePos;

#if DEBUG
                int attrNameLineNo = ps.LineNo;
#endif

                // parse attribute name
                int colonPos = -1;
    
                // PERF: we intentionally don't call ParseQName here to parse the element name unless a special 
                // case occurs (like end of buffer, invalid name char)
                pos += startNameCharSize; // start name char has already been checked

                // parse attribute name
            ContinueParseName:
                char tmpch2;

                unsafe {
                    for (;;) {
#if SILVERLIGHT
                        if ( xmlCharType.IsNCNameSingleChar( tmpch2 = chars[pos] ) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                        if (((xmlCharType.charProperties[tmpch2 = chars[pos]] & XmlCharType.fNCNameSC) != 0)) {
#endif
                            pos++;
                        }
#if XML10_FIFTH_EDITION
                        else if (pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar(chars[pos + 1], tmpch2)) {
                            pos += 2;
                        }
#endif
                        else {
                            break;
                        }
                    }
                }

                // colon -> save prefix end position and check next char if it's name start char
                if ( tmpch2 == ':' ) {
                    if ( colonPos != -1 ) {
                        if ( supportNamespaces ) {
                            Throw( pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs( ':', '\0' ));
                        }
                        else {
                            pos++;
                            goto ContinueParseName;
                        }
                    }
                    else {
                        colonPos = pos;
                        pos++;

                        unsafe {
#if SILVERLIGHT
                            if ( xmlCharType.IsStartNCNameSingleChar( chars[pos] ) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                            if (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartNameSC) != 0)) {
#endif
                                pos++;
                                goto ContinueParseName;
                            }
#if XML10_FIFTH_EDITION
                            else if ( pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar( chars[pos + 1], chars[pos] ) ) {
                                pos += 2;
                                goto ContinueParseName;
                            }
#endif
                        }

                        // else fallback to full name parsing routine

                        var tuple_6 = await ParseQNameAsync().ConfigureAwait(false);
                        colonPos = tuple_6.Item1;

                        pos = tuple_6.Item2;

                        chars = ps.chars;
                    }
                }
                else if ( pos + 1 >= ps.charsUsed ) {

                    var tuple_7 = await ParseQNameAsync().ConfigureAwait(false);
                    colonPos = tuple_7.Item1;

                    pos = tuple_7.Item2;

                    chars = ps.chars;
                }

                attr = AddAttribute( pos, colonPos );
                attr.SetLineInfo( ps.LineNo, attrNameLinePos );

#if DEBUG
                Debug.Assert( attrNameLineNo == ps.LineNo );
#endif

                // parse equals and quote char; 
                if ( chars[pos] != '=' ) {
                    ps.charPos = pos;
                    await EatWhitespacesAsync( null ).ConfigureAwait(false);
                    pos = ps.charPos;
                    if ( chars[pos] != '=' ) {
                        ThrowUnexpectedToken( "=" );
                    }
                }
                pos++;

                char quoteChar = chars[pos];
                if ( quoteChar != '"' && quoteChar != '\'' ) {
                    ps.charPos = pos;
                    await EatWhitespacesAsync( null ).ConfigureAwait(false);
                    pos = ps.charPos;
                    quoteChar = chars[pos];
                    if ( quoteChar != '"' && quoteChar != '\'' ) {
                        ThrowUnexpectedToken( "\"", "'" );
                    }
                }
                pos++;
                ps.charPos = pos;

                attr.quoteChar = quoteChar;
                attr.SetLineInfo2( ps.LineNo, ps.LinePos );

                // parse attribute value
                char tmpch3;
#if SILVERLIGHT
                while (xmlCharType.IsAttributeValueChar(tmpch3 = chars[pos])) {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[tmpch3 = chars[pos]] & XmlCharType.fAttrValue) != 0)) {
                        pos++;
                    }
                }
#endif
                if ( tmpch3 == quoteChar ) {
#if DEBUG
#if !SILVERLIGHT
                    if ( normalize ) {
                        string val = new string( chars, ps.charPos, pos - ps.charPos );
                        Debug.Assert( val == XmlComplianceUtil.CDataNormalize( val ), "The attribute value is not CDATA normalized!" ); 
                    }
#endif
#endif
                    attr.SetValue( chars, ps.charPos, pos - ps.charPos );
                    pos++;
                    ps.charPos = pos;
                }
                else {
                    await ParseAttributeValueSlowAsync( pos, quoteChar, attr ).ConfigureAwait(false);
                    pos = ps.charPos;
                    chars = ps.chars;
                }

                // handle special attributes:
                if ( attr.prefix.Length == 0 ) {
                    // default namespace declaration
                    if ( Ref.Equal( attr.localName, XmlNs ) ) {
                        OnDefaultNamespaceDecl( attr );
                    }
                }
                else {
                    // prefixed namespace declaration
                    if ( Ref.Equal( attr.prefix, XmlNs ) ) {
                        OnNamespaceDecl( attr );
                    }
                    // xml: attribute
                    else if ( Ref.Equal( attr.prefix, Xml ) ) {
                        OnXmlReservedAttribute( attr );
                    }
                }
                continue;

            ReadData:
                ps.lineNo -= lineNoDelta;
                if ( await ReadDataAsync().ConfigureAwait(false) != 0 ) {
                    pos = ps.charPos;
                    chars = ps.chars;
                }
                else {
                    ThrowUnclosedElements();
                }
            }

        End:
            if ( addDefaultAttributesAndNormalize ) {
                AddDefaultAttributesAndNormalize();
            }
            // lookup namespaces: element
            ElementNamespaceLookup();

            // lookup namespaces: attributes
            if ( attrNeedNamespaceLookup ) {
                AttributeNamespaceLookup();
                attrNeedNamespaceLookup = false;
            }

            // check duplicate attributes
            if ( attrDuplWalkCount >= MaxAttrDuplWalkCount ) {
                AttributeDuplCheck();
            }
        }

        private async Task ParseAttributeValueSlowAsync( int curPos, char quoteChar, NodeData attr ) {
            int pos = curPos;
            char[] chars = ps.chars;
            int attributeBaseEntityId = ps.entityId;
#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
            int valueChunkStartPos = 0;
            LineInfo valueChunkLineInfo = new LineInfo(ps.lineNo, ps.LinePos);
            NodeData lastChunk = null;
#endif

            Debug.Assert( stringBuilder.Length == 0 );

            for (;;) {
                // parse the rest of the attribute value
#if SILVERLIGHT
                while (xmlCharType.IsAttributeValueChar(chars[pos])) {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fAttrValue) != 0)) {
                        pos++;
                    }
                }
#endif

                if ( pos - ps.charPos > 0 ) {
                    stringBuilder.Append( chars, ps.charPos, pos - ps.charPos );
                    ps.charPos = pos;
                }

                if ( chars[pos] == quoteChar && attributeBaseEntityId == ps.entityId ) {
                    break;
                }
                else {
                    switch ( chars[pos] ) {
                        // eol
                        case (char)0xA:
                            pos++;
                            OnNewLine( pos );
                            if ( normalize ) {
                                stringBuilder.Append( (char)0x20 );  // CDATA normalization of 0xA
                                ps.charPos++;
                            }
                            continue;
                        case (char)0xD:
                            if ( chars[pos+1] == (char)0xA ) {
                                pos += 2;
                                if ( normalize ) {
                                    stringBuilder.Append( ps.eolNormalized ? "\u0020\u0020" : "\u0020" ); // CDATA normalization of 0xD 0xA
                                    ps.charPos = pos;
                                }
                            }
                            else if ( pos+1 < ps.charsUsed || ps.isEof ) { 
                                pos++;
                                if ( normalize ) {
                                    stringBuilder.Append( (char)0x20 );  // CDATA normalization of 0xD and 0xD 0xA
                                    ps.charPos = pos;
                                }
                            } 
                            else {
                                goto ReadData;
                            }
                            OnNewLine( pos );
                            continue;
                        // tab
                        case (char)0x9:
                            pos++;
                            if ( normalize ) {
                                stringBuilder.Append( (char)0x20 );  // CDATA normalization of 0x9
                                ps.charPos++;
                            }
                            continue;
                        case '"':
                        case '\'':
                        case '>':
                            pos++;
                            continue;
                        // attribute values cannot contain '<'
                        case '<':
                            Throw( pos, Res.Xml_BadAttributeChar, XmlException.BuildCharExceptionArgs( '<', '\0' ) );
                            break;
                        // entity referece
                        case '&':
                            if ( pos - ps.charPos > 0 ) {
                                stringBuilder.Append( chars, ps.charPos, pos - ps.charPos );
                            }
                            ps.charPos = pos;

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                            int enclosingEntityId = ps.entityId;
                            LineInfo entityLineInfo = new LineInfo( ps.lineNo, ps.LinePos + 1 );
#endif

                            var tuple_8 = await HandleEntityReferenceAsync( true,  EntityExpandType.All).ConfigureAwait(false);
                            pos = tuple_8.Item1;

                            switch ( tuple_8.Item2 ) {

                                case EntityType.CharacterDec:
                                case EntityType.CharacterHex:
                                case EntityType.CharacterNamed:
                                    break;
#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                                case EntityType.Unexpanded:
                                    if ( parsingMode == ParsingMode.Full && ps.entityId == attributeBaseEntityId ) {
                                        // construct text value chunk
                                        int valueChunkLen = stringBuilder.Length - valueChunkStartPos;
                                        if ( valueChunkLen > 0 ) {
                                            NodeData textChunk = new NodeData();
                                            textChunk.lineInfo = valueChunkLineInfo;
                                            textChunk.depth = attr.depth + 1;
                                            textChunk.SetValueNode( XmlNodeType.Text, stringBuilder.ToString( valueChunkStartPos, valueChunkLen ) );
                                            AddAttributeChunkToList( attr, textChunk, ref lastChunk );
                                        }

                                        // parse entity name
                                        ps.charPos++;
                                        string entityName = await ParseEntityNameAsync().ConfigureAwait(false);
                                             
                                        // construct entity reference chunk
                                        NodeData entityChunk = new NodeData();
                                        entityChunk.lineInfo = entityLineInfo;
                                        entityChunk.depth = attr.depth + 1;
                                        entityChunk.SetNamedNode( XmlNodeType.EntityReference, entityName );
                                        AddAttributeChunkToList( attr, entityChunk, ref lastChunk );

                                        // append entity ref to the attribute value
                                        stringBuilder.Append( '&' );
                                        stringBuilder.Append( entityName );
                                        stringBuilder.Append( ';' );

                                        // update info for the next attribute value chunk
                                        valueChunkStartPos = stringBuilder.Length;
                                        valueChunkLineInfo.Set( ps.LineNo, ps.LinePos );

                                        fullAttrCleanup = true;
                                    }
                                    else {
                                        ps.charPos++;
                                        await ParseEntityNameAsync().ConfigureAwait(false);
                                    }
                                    pos = ps.charPos;
                                    break;

                                case EntityType.ExpandedInAttribute:
                                    if ( parsingMode == ParsingMode.Full && enclosingEntityId == attributeBaseEntityId  ) {
                                        
                                        // construct text value chunk
                                        int valueChunkLen = stringBuilder.Length - valueChunkStartPos;
                                        if ( valueChunkLen > 0 ) {
                                            NodeData textChunk = new NodeData();
                                            textChunk.lineInfo = valueChunkLineInfo;
                                            textChunk.depth = attr.depth + 1;
                                            textChunk.SetValueNode( XmlNodeType.Text, stringBuilder.ToString( valueChunkStartPos, valueChunkLen ) );
                                            AddAttributeChunkToList( attr, textChunk, ref lastChunk );
                                        }

                                        // construct entity reference chunk
                                        NodeData entityChunk = new NodeData();
                                        entityChunk.lineInfo = entityLineInfo;
                                        entityChunk.depth = attr.depth + 1;
                                        entityChunk.SetNamedNode( XmlNodeType.EntityReference, ps.entity.Name );
                                        AddAttributeChunkToList( attr, entityChunk, ref lastChunk );

                                        fullAttrCleanup = true;

                                        // Note: info for the next attribute value chunk will be updated once we
                                        // get out of the expanded entity
                                    }
                                    pos = ps.charPos;
                                    break;
#endif
                                default:
                                    pos = ps.charPos;
                                    break;
                            }
                            chars = ps.chars;
                            continue;
                        default:
                            // end of buffer
                            if ( pos == ps.charsUsed ) {
                                goto ReadData;
                            }
                            // surrogate chars
                            else { 
                                char ch = chars[pos];
                                if ( XmlCharType.IsHighSurrogate(ch) ) {
                                    if ( pos + 1 == ps.charsUsed ) {
                                        goto ReadData;
                                    }
                                    pos++;
                                    if ( XmlCharType.IsLowSurrogate( chars[pos] ) ) {
                                        pos++;
                                        continue;
                                    }
                                }
                                ThrowInvalidChar( chars, ps.charsUsed, pos );
                                break;
                            }
                    }
                }
            
            ReadData:
                // read new characters into the buffer
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( ps.charsUsed - ps.charPos > 0 ) {
                        if ( ps.chars[ps.charPos] != (char)0xD ) {
                            Debug.Assert( false, "We should never get to this point." );
                            Throw( Res.Xml_UnexpectedEOF1 );
                        }
                        Debug.Assert( ps.isEof );
                    }
                    else {
                        if ( !InEntity ) {
                            if ( fragmentType == XmlNodeType.Attribute ) {
                                if ( attributeBaseEntityId != ps.entityId ) {
                                    Throw( Res.Xml_EntityRefNesting );
                                }
                                break;
                            }
                            Throw( Res.Xml_UnclosedQuote );
                        }
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                        HandleEntityEnd( true );
#else 
                        if ( HandleEntityEnd( true ) ) { // no EndEntity reporting while parsing attributes
                            Debug.Assert( false );
                            Throw( Res.Xml_InternalError );
                        }
                        // update info for the next attribute value chunk
                        if ( attributeBaseEntityId == ps.entityId ) {
                            valueChunkStartPos = stringBuilder.Length;
                            valueChunkLineInfo.Set( ps.LineNo, ps.LinePos );
                        }
#endif
                    }
                }

                pos = ps.charPos;
                chars = ps.chars;
            }

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
            if ( attr.nextAttrValueChunk != null ) {
                // construct last text value chunk
                int valueChunkLen = stringBuilder.Length - valueChunkStartPos;
                if ( valueChunkLen > 0 ) {
                    NodeData textChunk = new NodeData();
                    textChunk.lineInfo = valueChunkLineInfo;
                    textChunk.depth = attr.depth + 1;
                    textChunk.SetValueNode( XmlNodeType.Text, stringBuilder.ToString( valueChunkStartPos, valueChunkLen ) );
                    AddAttributeChunkToList( attr, textChunk, ref lastChunk );
                }
            }
#endif

            ps.charPos = pos + 1;

            attr.SetValue( stringBuilder.ToString() );
            stringBuilder.Length = 0;
        }

        private Task<bool> ParseTextAsync() {
            int startPos;
            int endPos;
            int orChars = 0;

            // skip over the text if not in full parsing mode
            if (parsingMode != ParsingMode.Full) {
                return _ParseTextAsync(null);
            }

            curNode.SetLineInfo(ps.LineNo, ps.LinePos);
            Debug.Assert(stringBuilder.Length == 0);

            // the whole value is in buffer

            Task<Tuple<int,int,int,bool>> parseTextTask = ParseTextAsync(orChars);
            bool fullValue = false;
            if (!parseTextTask.IsSuccess()) {
                return _ParseTextAsync(parseTextTask);
            }
            else {
                var tuple_10 = parseTextTask.Result;
                startPos = tuple_10.Item1;
                endPos = tuple_10.Item2;
                orChars = tuple_10.Item3;
                fullValue = tuple_10.Item4;
            }

            if (fullValue) {
                if (endPos - startPos == 0) {
                    return ParseTextAsync_IgnoreNode();
                }
                XmlNodeType nodeType = GetTextNodeType(orChars);
                if (nodeType == XmlNodeType.None) {
                    return ParseTextAsync_IgnoreNode();
                }
                Debug.Assert(endPos - startPos > 0);
                curNode.SetValueNode(nodeType, ps.chars, startPos, endPos - startPos);
                return AsyncHelper.DoneTaskTrue;
            }
            // only piece of the value was returned
            else {
                return _ParseTextAsync(parseTextTask);
            }
 
        }

        // Parses text or white space node.
        // Returns true if a node has been parsed and its data set to curNode. 
        // Returns false when a white space has been parsed and ignored (according to current whitespace handling) or when parsing mode is not Full.
        // Also returns false if there is no text to be parsed.
        private async Task< bool > _ParseTextAsync(Task<Tuple<int,int,int,bool>> parseTask) {
            int startPos;
            int endPos;
            int orChars = 0;

            if (parseTask != null)
                goto Parse;

            // skip over the text if not in full parsing mode
            if ( parsingMode != ParsingMode.Full ) {

                Tuple<int,int,int,bool> tuple_9;
                do {
                    tuple_9 = await ParseTextAsync(orChars).ConfigureAwait(false);
                    startPos = tuple_9.Item1;
                    endPos = tuple_9.Item2;
                    orChars = tuple_9.Item3;
    
                } while ( !tuple_9.Item4 );

                goto IgnoredNode;
            }

            curNode.SetLineInfo( ps.LineNo, ps.LinePos ); 
            Debug.Assert( stringBuilder.Length == 0 );
            
            parseTask = ParseTextAsync(orChars);
 
        Parse:
            var tuple_10 = await parseTask.ConfigureAwait(false);
            startPos = tuple_10.Item1;
            endPos = tuple_10.Item2;
            orChars = tuple_10.Item3;

            if ( tuple_10.Item4 ) {

                if ( endPos - startPos == 0 ) {
                    goto IgnoredNode;
                }
                XmlNodeType nodeType = GetTextNodeType( orChars );
                if ( nodeType == XmlNodeType.None ) {
                    goto IgnoredNode;
                }
                Debug.Assert( endPos - startPos > 0 );
                curNode.SetValueNode( nodeType, ps.chars, startPos, endPos - startPos );
                return true;
            }
            // only piece of the value was returned
            else {
                // V1 compatibility mode -> cache the whole value
                if ( v1Compat ) {

                    Tuple<int,int,int,bool> tuple_11;

                    do {
                        if ( endPos - startPos > 0 ) {
                            stringBuilder.Append( ps.chars, startPos, endPos - startPos );
                        }

                        tuple_11 = await ParseTextAsync(orChars).ConfigureAwait(false);
                        startPos = tuple_11.Item1;
                        endPos = tuple_11.Item2;
                        orChars = tuple_11.Item3;
    
                    } while ( !tuple_11.Item4 );

                    if ( endPos - startPos > 0 ) {
                        stringBuilder.Append( ps.chars, startPos, endPos - startPos );
                    }

                    Debug.Assert( stringBuilder.Length > 0 );

                    XmlNodeType nodeType = GetTextNodeType( orChars );
                    if ( nodeType == XmlNodeType.None ) {
                        stringBuilder.Length = 0;
                        goto IgnoredNode;
                    }

                    curNode.SetValueNode( nodeType, stringBuilder.ToString() );
                    stringBuilder.Length = 0;
                    return true;
                }
                // V2 reader -> do not cache the whole value yet, read only up to 4kB to decide whether the value is a whitespace
                else {
                    bool fullValue = false;

                    // if it's a partial text value, not a whitespace -> return
                    if ( orChars > 0x20 ) {
                        Debug.Assert( endPos - startPos > 0 );
                        curNode.SetValueNode( XmlNodeType.Text, ps.chars, startPos, endPos - startPos );
                        nextParsingFunction = parsingFunction;
                        parsingFunction = ParsingFunction.PartialTextValue;
                        return true;
                    }

                    // partial whitespace -> read more data (up to 4kB) to decide if it is a whitespace or a text node
                    if ( endPos - startPos > 0 ) {
                        stringBuilder.Append( ps.chars, startPos, endPos - startPos );
                    }
                    do {

                        var tuple_12 = await ParseTextAsync(orChars).ConfigureAwait(false);
                        startPos = tuple_12.Item1;
                        endPos = tuple_12.Item2;
                        orChars = tuple_12.Item3;

                        fullValue = tuple_12.Item4;

                        if ( endPos - startPos > 0 ) {
                            stringBuilder.Append( ps.chars, startPos, endPos - startPos );
                        }
                    } while ( !fullValue && orChars <= 0x20 && stringBuilder.Length < MinWhitespaceLookahedCount );

                    // determine the value node type
                    XmlNodeType nodeType = ( stringBuilder.Length < MinWhitespaceLookahedCount ) ? GetTextNodeType( orChars ) : XmlNodeType.Text;
                    if ( nodeType == XmlNodeType.None ) {
                        // ignored whitespace -> skip over the rest of the value unless we already read it all
                        stringBuilder.Length = 0;
                        if ( !fullValue ) {

                            Tuple<int,int,int,bool> tuple_13;
                            do {
                                tuple_13 = await ParseTextAsync(orChars).ConfigureAwait(false);
                                startPos = tuple_13.Item1;
                                endPos = tuple_13.Item2;
                                orChars = tuple_13.Item3;
    
                            } while ( !tuple_13.Item4 );

                        }
                        goto IgnoredNode;
                    }
                    // set value to curNode
                    curNode.SetValueNode( nodeType, stringBuilder.ToString() );
                    stringBuilder.Length = 0;

                    // change parsing state if the full value was not parsed
                    if ( !fullValue ) {
                        nextParsingFunction = parsingFunction;
                        parsingFunction = ParsingFunction.PartialTextValue;
                    }
                    return true;
                }
            }

        IgnoredNode:
            return await ParseTextAsync_IgnoreNode().ConfigureAwait(false);
        }
        
        private Task<bool> ParseTextAsync_IgnoreNode() {

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)

            // ignored whitespace at the end of manually resolved entity
            if (parsingFunction == ParsingFunction.ReportEndEntity) {
                SetupEndEntityNodeInContent();
                parsingFunction = nextParsingFunction;
                return AsyncHelper.DoneTaskTrue;
            }
            else if (parsingFunction == ParsingFunction.EntityReference) {
                parsingFunction = nextNextParsingFunction;
                return ParseEntityReferenceAsync().ReturnTaskBoolWhenFinish(true);
            }
#endif
            return AsyncHelper.DoneTaskFalse;
        }

        // Parses a chunk of text starting at ps.charPos. 
        //   startPos .... start position of the text chunk that has been parsed (can differ from ps.charPos before the call)
        //   endPos ...... end position of the text chunk that has been parsed (can differ from ps.charPos after the call)
        //   ourOrChars .. all parsed character bigger or equal to 0x20 or-ed (|) into a single int. It can be used for whitespace detection 
        //                 (the text has a non-whitespace character if outOrChars > 0x20).
        // Returns true when the whole value has been parsed. Return false when it needs to be called again to get a next chunk of value.

        
        private class ParseTextState { 
            public int outOrChars;
            public char[] chars;
            public int pos;
            public int rcount;
            public int rpos;
            public int orChars;
            public char c;

            public ParseTextState(int outOrChars, char[] chars, int pos, int rcount, int rpos, int orChars, char c) {
                this.outOrChars = outOrChars;
                this.chars = chars;
                this.pos = pos;
                this.rcount = rcount;
                this.rpos = rpos;
                this.orChars = orChars;
                this.c = c;
            }
        }

        private enum ParseTextFunction {
            ParseText,
            Entity,
            Surrogate,
            ReadData,
            NoValue,
            PartialValue,
        }

        private ParseTextFunction parseText_NextFunction;

        private ParseTextState lastParseTextState;

        private Task<Tuple<int, int, int, bool>> parseText_dummyTask = Task.FromResult(new Tuple<int, int, int, bool>(0,0,0,false));

        //To avoid stackoverflow like ParseText->ParseEntity->ParText->..., use a loop and parsing function to implement such call.
        private Task<Tuple<int, int, int, bool>> ParseTextAsync(int outOrChars) {
            Task<Tuple<int, int, int, bool>> task = ParseTextAsync(outOrChars, ps.chars, ps.charPos, 0, -1, outOrChars, (char)0);
            while (true)
            {
                if (!task.IsSuccess()) {
                    return ParseTextAsync_AsyncFunc(task);
                }

                outOrChars = lastParseTextState.outOrChars;
                char[] chars = lastParseTextState.chars;
                int pos = lastParseTextState.pos;
                int rcount = lastParseTextState.rcount;
                int rpos = lastParseTextState.rpos;
                int orChars = lastParseTextState.orChars;
                char c = lastParseTextState.c;

                switch (parseText_NextFunction)
                {
                    case ParseTextFunction.ParseText:
                        task = ParseTextAsync(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.Entity:
                        task = ParseTextAsync_ParseEntity(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.ReadData:
                        task = ParseTextAsync_ReadData(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.Surrogate:
                        task = ParseTextAsync_Surrogate(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.NoValue:
                        return ParseTextAsync_NoValue(outOrChars, pos);
                    case ParseTextFunction.PartialValue:
                        return ParseTextAsync_PartialValue(pos, rcount, rpos, orChars, c);
                }
            }
        }

        private async Task<Tuple<int, int, int, bool>> ParseTextAsync_AsyncFunc(Task<Tuple<int, int, int, bool>> task) {
 
            while (true) {

                await task.ConfigureAwait(false);

                int outOrChars = lastParseTextState.outOrChars;
                char[] chars = lastParseTextState.chars;
                int pos = lastParseTextState.pos;
                int rcount = lastParseTextState.rcount;
                int rpos = lastParseTextState.rpos;
                int orChars = lastParseTextState.orChars;
                char c = lastParseTextState.c;

                switch (parseText_NextFunction) {
                    case ParseTextFunction.ParseText:
                        task = ParseTextAsync(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.Entity:
                        task = ParseTextAsync_ParseEntity(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.ReadData:
                        task = ParseTextAsync_ReadData(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.Surrogate:
                        task = ParseTextAsync_Surrogate(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        break;
                    case ParseTextFunction.NoValue:
                        return await ParseTextAsync_NoValue(outOrChars, pos).ConfigureAwait(false);
                    case ParseTextFunction.PartialValue:
                        return await ParseTextAsync_PartialValue(pos, rcount, rpos, orChars, c).ConfigureAwait(false);
                }
            }
        }

        private Task<Tuple<int, int, int, bool>> ParseTextAsync(int outOrChars, char[] chars, int pos, int rcount, int rpos, int orChars, char c) {
            
            for (; ; ) {
                // parse text content
#if SILVERLIGHT
                while (xmlCharType.IsTextChar(c = chars[pos])) {
                    orChars |= (int)c;
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[c = chars[pos]] & XmlCharType.fText) != 0)) {
                        orChars |= (int)c;
                        pos++;
                    }
                }
#endif
                switch (c) {
                    case (char)0x9:
                        pos++;
                        continue;
                    // eol
                    case (char)0xA:
                        pos++;
                        OnNewLine(pos);
                        continue;
                    case (char)0xD:
                        if (chars[pos + 1] == (char)0xA) {
                            if (!ps.eolNormalized && parsingMode == ParsingMode.Full) {
                                if (pos - ps.charPos > 0) {
                                    if (rcount == 0) {
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else {
                                        ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else {
                                    ps.charPos++;
                                }
                            }
                            pos += 2;
                        }
                        else if (pos + 1 < ps.charsUsed || ps.isEof) {
                            if (!ps.eolNormalized) {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }
                            pos++;
                        }
                        else {
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.ReadData;
                            return parseText_dummyTask;
                        }
                        OnNewLine(pos);
                        continue;
                    // some tag 
                    case '<':
                        lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        parseText_NextFunction = ParseTextFunction.PartialValue;
                        return parseText_dummyTask;
                    // entity reference
                    case '&':
                        lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        parseText_NextFunction = ParseTextFunction.Entity;
                        return parseText_dummyTask;
                    case ']':
                        if (ps.charsUsed - pos < 3 && !ps.isEof) {
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.ReadData;
                            return parseText_dummyTask;
                        }
                        if (chars[pos + 1] == ']' && chars[pos + 2] == '>') {
                            Throw(pos, Res.Xml_CDATAEndInText);
                        }
                        orChars |= ']';
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if (pos == ps.charsUsed) {
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.ReadData;
                            return parseText_dummyTask;
                        }
                        // surrogate chars
                        else {
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.Surrogate;
                            return parseText_dummyTask;
                        } 
                }
            }
        }

        private async Task<Tuple<int, int, int, bool>> ParseTextAsync_ParseEntity(int outOrChars, char[] chars, int pos, int rcount, int rpos, int orChars, char c) {
            // try to parse char entity inline
            int charRefEndPos, charCount;
            EntityType entityType;
            if ((charRefEndPos = ParseCharRefInline(pos, out charCount, out entityType)) > 0) {
                if (rcount > 0) {
                    ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
                }
                rpos = pos - rcount;
                rcount += (charRefEndPos - pos - charCount);
                pos = charRefEndPos;

                if (!xmlCharType.IsWhiteSpace(chars[charRefEndPos - charCount]) ||
                     (v1Compat && entityType == EntityType.CharacterDec)) {
                    orChars |= 0xFF;
                }
            }
            else {
                if (pos > ps.charPos) {
                    lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                    parseText_NextFunction = ParseTextFunction.PartialValue;
                    return parseText_dummyTask.Result;
                }

                var tuple_14 = await HandleEntityReferenceAsync(false, EntityExpandType.All).ConfigureAwait(false);
                pos = tuple_14.Item1;

                switch (tuple_14.Item2) {

#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                    case EntityType.Unexpanded:
                        // make sure we will report EntityReference after the text node
                        nextParsingFunction = parsingFunction;
                        parsingFunction = ParsingFunction.EntityReference;
                        // end the value (returns nothing)
                        lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                        parseText_NextFunction = ParseTextFunction.NoValue;
                        return parseText_dummyTask.Result;
#endif
                    case EntityType.CharacterDec:
                        if (!v1Compat) {
                            goto case EntityType.CharacterHex;
                        }
                        orChars |= 0xFF;
                        break;
                    case EntityType.CharacterHex:
                    case EntityType.CharacterNamed:
                        if (!xmlCharType.IsWhiteSpace(ps.chars[pos - 1])) {
                            orChars |= 0xFF;
                        }
                        break;
                    default:
                        pos = ps.charPos;
                        break;
                }
                chars = ps.chars;
            }

            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
            parseText_NextFunction = ParseTextFunction.ParseText;
            return parseText_dummyTask.Result;
        }
        
        private async Task<Tuple<int, int, int, bool>> ParseTextAsync_Surrogate(int outOrChars, char[] chars, int pos, int rcount, int rpos, int orChars, char c) {
            char ch = chars[pos];
            if (XmlCharType.IsHighSurrogate(ch)) {
                if (pos + 1 == ps.charsUsed) {
                    lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                    parseText_NextFunction = ParseTextFunction.ReadData;
                    return parseText_dummyTask.Result;
                }
                pos++;
                if (XmlCharType.IsLowSurrogate(chars[pos])) {
                    pos++;
                    orChars |= ch;
                    lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                    parseText_NextFunction = ParseTextFunction.ParseText;
                    return parseText_dummyTask.Result;
                }
            }
            int offset = pos - ps.charPos;
            if (await ZeroEndingStreamAsync(pos).ConfigureAwait(false)) {
                chars = ps.chars;
                pos = ps.charPos + offset;
                lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                parseText_NextFunction = ParseTextFunction.PartialValue;
                return parseText_dummyTask.Result;
            }
            else {
                ThrowInvalidChar(ps.chars, ps.charsUsed, ps.charPos + offset);
            }
            //should never hit here
            throw new Exception();
        }
        
        private async Task<Tuple<int, int, int, bool>> ParseTextAsync_ReadData(int outOrChars, char[] chars, int pos, int rcount, int rpos, int orChars, char c)
        {
            if (pos > ps.charPos) {
                    lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                    parseText_NextFunction = ParseTextFunction.PartialValue;
                    return parseText_dummyTask.Result;
                }
                // read new characters into the buffer 
                if (await ReadDataAsync().ConfigureAwait(false) == 0) {
                    if (ps.charsUsed - ps.charPos > 0) {
                        if (ps.chars[ps.charPos] != (char)0xD && ps.chars[ps.charPos] != ']') {
                            Throw(Res.Xml_UnexpectedEOF1);
                        }
                        Debug.Assert(ps.isEof);
                    }
                    else {
                        if (!InEntity) {
                            // end the value (returns nothing)
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.NoValue;
                            return parseText_dummyTask.Result;
                        }
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                        HandleEntityEnd( true );
#else
                        if (HandleEntityEnd(true)) {
                            // report EndEntity after the text node
                            nextParsingFunction = parsingFunction;
                            parsingFunction = ParsingFunction.ReportEndEntity;
                            // end the value (returns nothing)
                            lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                            parseText_NextFunction = ParseTextFunction.NoValue;
                            return parseText_dummyTask.Result;
                        }
#endif
                    }
                }
                pos = ps.charPos;
                chars = ps.chars;
                lastParseTextState = new ParseTextState(outOrChars, chars, pos, rcount, rpos, orChars, c);
                parseText_NextFunction = ParseTextFunction.ParseText;
                return parseText_dummyTask.Result;
        }

        private Task<Tuple<int, int, int, bool>> ParseTextAsync_NoValue(int outOrChars, int pos) {
            return Task.FromResult(new Tuple<int, int, int, bool>(pos, pos, outOrChars, true));
        }

        private Task<Tuple<int, int, int, bool>> ParseTextAsync_PartialValue(int pos, int rcount, int rpos, int orChars, char c) {
            if (parsingMode == ParsingMode.Full && rcount > 0) {
                ShiftBuffer(rpos + rcount, rpos, pos - rpos - rcount);
            }
            int startPos = ps.charPos;
            int endPos = pos - rcount;
            ps.charPos = pos;
            int outOrChars = orChars;

            return Task.FromResult(new Tuple<int, int, int, bool>(startPos, endPos, outOrChars, c == '<'));
        }

        
        // When in ParsingState.PartialTextValue, this method parses and caches the rest of the value and stores it in curNode.
        async Task FinishPartialValueAsync() {
            Debug.Assert( stringBuilder.Length == 0 );
            Debug.Assert( parsingFunction == ParsingFunction.PartialTextValue ||
                          ( parsingFunction == ParsingFunction.InReadValueChunk && incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue ) );

            curNode.CopyTo( readValueOffset, stringBuilder );

            int startPos;
            int endPos;
            int orChars = 0;

            var tuple_15 = await ParseTextAsync(orChars).ConfigureAwait(false);
            startPos = tuple_15.Item1;
            endPos = tuple_15.Item2;
            orChars = tuple_15.Item3;

            while ( !tuple_15.Item4 ) {

                stringBuilder.Append( ps.chars, startPos, endPos - startPos );

                tuple_15 = await ParseTextAsync(orChars).ConfigureAwait(false);
                startPos = tuple_15.Item1;
                endPos = tuple_15.Item2;
                orChars = tuple_15.Item3;
    
            }
            stringBuilder.Append( ps.chars, startPos, endPos - startPos );

            Debug.Assert( stringBuilder.Length > 0 );
            curNode.SetValue( stringBuilder.ToString() );
            stringBuilder.Length = 0;
        }

        async Task FinishOtherValueIteratorAsync() {
            switch ( parsingFunction ) {
                case ParsingFunction.InReadAttributeValue:
                    // do nothing, correct value is already in curNode
                    break;
                case ParsingFunction.InReadValueChunk:
                    if ( incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue ) {
                        await FinishPartialValueAsync().ConfigureAwait(false);
                        incReadState = IncrementalReadState.ReadValueChunk_OnCachedValue;
                    }
                    else {
                        if ( readValueOffset > 0 ) {
                            curNode.SetValue( curNode.StringValue.Substring( readValueOffset ) );
                            readValueOffset = 0;
                        }
                    }
                    break;
                case ParsingFunction.InReadContentAsBinary:
                case ParsingFunction.InReadElementContentAsBinary:
                    switch ( incReadState ) {
                        case IncrementalReadState.ReadContentAsBinary_OnPartialValue:
                            await FinishPartialValueAsync().ConfigureAwait(false);
                            incReadState = IncrementalReadState.ReadContentAsBinary_OnCachedValue;
                            break;
                        case IncrementalReadState.ReadContentAsBinary_OnCachedValue:
                            if ( readValueOffset > 0 ) {
                                curNode.SetValue( curNode.StringValue.Substring( readValueOffset ) );
                                readValueOffset = 0;
                            }
                            break;
                        case IncrementalReadState.ReadContentAsBinary_End:
                            curNode.SetValue( string.Empty );
                            break;
                    }
                    break;
            }
        }

        // When in ParsingState.PartialTextValue, this method skips over the rest of the partial value.
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        async Task SkipPartialTextValueAsync() {
            Debug.Assert( parsingFunction == ParsingFunction.PartialTextValue || parsingFunction == ParsingFunction.InReadValueChunk || 
                          parsingFunction == ParsingFunction.InReadContentAsBinary || parsingFunction == ParsingFunction.InReadElementContentAsBinary );
            int startPos;
            int endPos;
            int orChars = 0;

            parsingFunction = nextParsingFunction;

            Tuple<int,int,int,bool> tuple_16;
            do {
                tuple_16 = await ParseTextAsync(orChars).ConfigureAwait(false);
                startPos = tuple_16.Item1;
                endPos = tuple_16.Item2;
                orChars = tuple_16.Item3;
    
            } while ( !tuple_16.Item4 );

        }

        Task FinishReadValueChunkAsync() {
            Debug.Assert( parsingFunction == ParsingFunction.InReadValueChunk );

            readValueOffset = 0;
            if ( incReadState == IncrementalReadState.ReadValueChunk_OnPartialValue ) {
                Debug.Assert( ( index > 0 ) ? nextParsingFunction == ParsingFunction.ElementContent : nextParsingFunction == ParsingFunction.DocumentContent );
                return SkipPartialTextValueAsync();
            }
            else {
                parsingFunction = nextParsingFunction;
                nextParsingFunction = nextNextParsingFunction;

                return AsyncHelper.DoneTask;

            }
        }

        async Task FinishReadContentAsBinaryAsync() {
            Debug.Assert( parsingFunction == ParsingFunction.InReadContentAsBinary || parsingFunction == ParsingFunction.InReadElementContentAsBinary );

            readValueOffset = 0;
            if ( incReadState == IncrementalReadState.ReadContentAsBinary_OnPartialValue ) {
                Debug.Assert( ( index > 0 ) ? nextParsingFunction == ParsingFunction.ElementContent : nextParsingFunction == ParsingFunction.DocumentContent );
                await SkipPartialTextValueAsync().ConfigureAwait(false);
            }
            else {
                parsingFunction = nextParsingFunction;
                nextParsingFunction = nextNextParsingFunction;
            }
            if ( incReadState != IncrementalReadState.ReadContentAsBinary_End ) {
                while ( await MoveToNextContentNodeAsync( true ).ConfigureAwait(false) );
            }
        }

        async Task FinishReadElementContentAsBinaryAsync() {
            await FinishReadContentAsBinaryAsync().ConfigureAwait(false);

            if ( curNode.type != XmlNodeType.EndElement ) {
                Throw( Res.Xml_InvalidNodeType, curNode.type.ToString() );
            }
            // move off the end element
            await outerReader.ReadAsync().ConfigureAwait(false);
        }

        private async Task< bool > ParseRootLevelWhitespaceAsync() {
            Debug.Assert( stringBuilder.Length == 0 );

            XmlNodeType nodeType = GetWhitespaceType();

            if ( nodeType == XmlNodeType.None ) {
                await EatWhitespacesAsync( null ).ConfigureAwait(false);
                if ( ps.chars[ps.charPos] == '<' || ps.charsUsed - ps.charPos == 0 || await ZeroEndingStreamAsync( ps.charPos ).ConfigureAwait(false) ) {
                    return false;
                }
            }
            else {
                curNode.SetLineInfo( ps.LineNo, ps.LinePos ); 
                await EatWhitespacesAsync( stringBuilder ).ConfigureAwait(false);
                if ( ps.chars[ps.charPos] == '<' || ps.charsUsed - ps.charPos == 0 || await ZeroEndingStreamAsync( ps.charPos ).ConfigureAwait(false) ) {
                    if ( stringBuilder.Length > 0 ) {
                        curNode.SetValueNode( nodeType, stringBuilder.ToString() );
                        stringBuilder.Length = 0;
                        return true;
                    }
                    return false;
                }
            }

            if ( xmlCharType.IsCharData( ps.chars[ps.charPos] ) ) {
                Throw( Res.Xml_InvalidRootData );
            }
            else {
                ThrowInvalidChar( ps.chars, ps.charsUsed, ps.charPos );
            }
            return false;
        }

#if !SILVERLIGHT
        private async Task ParseEntityReferenceAsync() {
            Debug.Assert( ps.chars[ps.charPos] == '&' );
            ps.charPos++;

            curNode.SetLineInfo( ps.LineNo, ps.LinePos );
            curNode.SetNamedNode( XmlNodeType.EntityReference, await ParseEntityNameAsync().ConfigureAwait(false) );
        }
#endif
        
        private async Task< Tuple<int, EntityType> > HandleEntityReferenceAsync(bool isInAttributeValue, EntityExpandType expandType) {
            int charRefEndPos;

            Debug.Assert( ps.chars[ps.charPos] == '&' );

            if ( ps.charPos + 1 == ps.charsUsed ) {
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( Res.Xml_UnexpectedEOF1 );
                }
            }
            
            // numeric characters reference
            if ( ps.chars[ps.charPos+1] == '#' ) {
                EntityType entityType;

                var tuple_17 = await ParseNumericCharRefAsync( expandType != EntityExpandType.OnlyGeneral,  null).ConfigureAwait(false);
                entityType = tuple_17.Item1;

                charRefEndPos = tuple_17.Item2;

                Debug.Assert( entityType == EntityType.CharacterDec || entityType == EntityType.CharacterHex );

                return new Tuple<int, EntityType>(charRefEndPos, entityType);

            }
            // named reference
            else {
                // named character reference
                charRefEndPos = await ParseNamedCharRefAsync( expandType != EntityExpandType.OnlyGeneral, null ).ConfigureAwait(false);
                if ( charRefEndPos >= 0 ) {

                    return new Tuple<int, EntityType>(charRefEndPos, EntityType.CharacterNamed);

                }

                // general entity reference
#if !SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                // NOTE: XmlValidatingReader compatibility mode: expand all entities in attribute values
                // general entity reference
                if ( expandType == EntityExpandType.OnlyCharacter ||
                     ( entityHandling != EntityHandling.ExpandEntities &&
                       ( !isInAttributeValue || !validatingReaderCompatFlag ) ) ) {

                    return new Tuple<int, EntityType>(charRefEndPos, EntityType.Unexpanded);

                }
#endif
                int endPos;

                ps.charPos++;
                int savedLinePos = ps.LinePos;
                try {
                    endPos = await ParseNameAsync().ConfigureAwait(false);
                }
                catch ( XmlException ) {
                    Throw( Res.Xml_ErrorParsingEntityName, ps.LineNo, savedLinePos );

                    return new Tuple<int, EntityType>(charRefEndPos, EntityType.Skipped);

                }

                // check ';'
                if ( ps.chars[endPos] != ';' ) {
                    ThrowUnexpectedToken( endPos, ";" );
                }

                int entityLinePos = ps.LinePos;
                string entityName = nameTable.Add( ps.chars, ps.charPos, endPos - ps.charPos );
                ps.charPos = endPos + 1;
                charRefEndPos = -1;

                EntityType entType = await HandleGeneralEntityReferenceAsync( entityName, isInAttributeValue, false, entityLinePos ).ConfigureAwait(false);
                reportedBaseUri = ps.baseUriStr;
                reportedEncoding = ps.encoding;

                return new Tuple<int, EntityType>(charRefEndPos, entType);

            }
        }

        // returns true == continue parsing
        // return false == unexpanded external entity, stop parsing and return
        private async Task< EntityType > HandleGeneralEntityReferenceAsync( string name, bool isInAttributeValue, bool pushFakeEntityIfNullResolver, int entityStartLinePos ) {
            IDtdEntityInfo entity = null;

            if ( dtdInfo == null && fragmentParserContext != null && fragmentParserContext.HasDtdInfo && dtdProcessing == DtdProcessing.Parse ) {
                await ParseDtdFromParserContextAsync().ConfigureAwait(false);
            }

            if ( dtdInfo == null || 
                 ( ( entity = dtdInfo.LookupEntity( name) ) == null ) ) {
#if !SILVERLIGHT // Needed only for XmlTextReader (when used from XmlDocument)
                if ( disableUndeclaredEntityCheck ) {
                    SchemaEntity schemaEntity = new SchemaEntity( new XmlQualifiedName( name ), false );
                    schemaEntity.Text = string.Empty;
                    entity = schemaEntity;
                }
                else
#endif
                Throw( Res.Xml_UndeclaredEntity, name, ps.LineNo, entityStartLinePos );
            }

            if ( entity.IsUnparsedEntity ) {
#if !SILVERLIGHT // Needed only for XmlTextReader (when used from XmlDocument)
                if ( disableUndeclaredEntityCheck ) {
                    SchemaEntity schemaEntity = new SchemaEntity( new XmlQualifiedName( name ), false );
                    schemaEntity.Text = string.Empty;
                    entity = schemaEntity;
                }
                else
#endif
                Throw( Res.Xml_UnparsedEntityRef, name, ps.LineNo, entityStartLinePos ); 
            }

            if ( standalone && entity.IsDeclaredInExternal ) {
                Throw( Res.Xml_ExternalEntityInStandAloneDocument, entity.Name, ps.LineNo, entityStartLinePos );
            }

            if ( entity.IsExternal ) {
                if ( isInAttributeValue ) {
                    Throw( Res.Xml_ExternalEntityInAttValue, name, ps.LineNo, entityStartLinePos );
                    return EntityType.Skipped;
                }

                if ( parsingMode == ParsingMode.SkipContent ) {
                    return EntityType.Skipped;
                }

                if (IsResolverNull) {
                    if ( pushFakeEntityIfNullResolver ) {
                        await PushExternalEntityAsync( entity ).ConfigureAwait(false);
                        curNode.entityId = ps.entityId;
                        return EntityType.FakeExpanded;
                    }
                    return EntityType.Skipped;
                }
                else {
                    await PushExternalEntityAsync( entity ).ConfigureAwait(false);
                    curNode.entityId = ps.entityId;
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                    return EntityType.Expanded; 
#else 
                    return (isInAttributeValue && validatingReaderCompatFlag) ? EntityType.ExpandedInAttribute : EntityType.Expanded;
#endif
                }
            }
            else {
                if ( parsingMode == ParsingMode.SkipContent ) {
                    return EntityType.Skipped;
                }

                PushInternalEntity( entity );

                curNode.entityId = ps.entityId;
#if SILVERLIGHT // Needed only for XmlTextReader (reporting of entities)
                return EntityType.Expanded;
#else 
                return ( isInAttributeValue && validatingReaderCompatFlag ) ? EntityType.ExpandedInAttribute : EntityType.Expanded;
#endif
            }
        }

        private Task< bool > ParsePIAsync() {
            return ParsePIAsync( null );
        }

        // Parses processing instruction; if piInDtdStringBuilder != null, the processing instruction is in DTD and
        // it will be saved in the passed string builder (target, whitespace & value).
        private async Task< bool > ParsePIAsync( BufferBuilder piInDtdStringBuilder ) {
            if ( parsingMode == ParsingMode.Full ) {
                curNode.SetLineInfo( ps.LineNo, ps.LinePos );
            }

            Debug.Assert( stringBuilder.Length == 0 );

            // parse target name
            int nameEndPos = await ParseNameAsync().ConfigureAwait(false);
            string target = nameTable.Add( ps.chars, ps.charPos, nameEndPos - ps.charPos );

            if ( string.Compare( target, "xml", StringComparison.OrdinalIgnoreCase ) == 0 ) {
                Throw( target.Equals( "xml" ) ? Res.Xml_XmlDeclNotFirst : Res.Xml_InvalidPIName, target );
            }
            ps.charPos = nameEndPos;

            if ( piInDtdStringBuilder == null ) {
                if ( !ignorePIs && parsingMode == ParsingMode.Full ) {
                    curNode.SetNamedNode( XmlNodeType.ProcessingInstruction, target );
                }
            }
            else {
                piInDtdStringBuilder.Append( target );
            }

            // check mandatory whitespace
            char ch = ps.chars[ps.charPos];
            Debug.Assert( ps.charPos < ps.charsUsed );
            if ( await EatWhitespacesAsync( piInDtdStringBuilder ).ConfigureAwait(false) == 0 ) {
                if ( ps.charsUsed - ps.charPos < 2 ) {
                    await ReadDataAsync().ConfigureAwait(false);
                }
                if ( ch != '?' || ps.chars[ps.charPos+1] != '>' ) {
                    Throw( Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs( ps.chars, ps.charsUsed, ps.charPos ) );
                }
            }

            // scan processing instruction value
            int startPos, endPos;

            var tuple_18 = await ParsePIValueAsync().ConfigureAwait(false);
            startPos = tuple_18.Item1;
            endPos = tuple_18.Item2;

            if ( tuple_18.Item3 ) {

                if ( piInDtdStringBuilder == null ) {
                    if ( ignorePIs ) {
                        return false;
                    }
                    if ( parsingMode == ParsingMode.Full ) {
                        curNode.SetValue( ps.chars, startPos, endPos - startPos );
                    }
                }
                else {
                    piInDtdStringBuilder.Append( ps.chars, startPos, endPos - startPos );
                }
            }
            else {
                BufferBuilder sb;
                if ( piInDtdStringBuilder == null ) {
                    if ( ignorePIs || parsingMode != ParsingMode.Full ) {

                        Tuple<int,int,bool> tuple_19;
                        do {
                            tuple_19 = await ParsePIValueAsync().ConfigureAwait(false);
                            startPos = tuple_19.Item1;
                            endPos = tuple_19.Item2;
    
                        } while ( !tuple_19.Item3 );

                        return false;
                    }
                    sb = stringBuilder;
                    Debug.Assert( stringBuilder.Length == 0 );
                }
                else {
                    sb = piInDtdStringBuilder;
                }

                Tuple<int,int,bool> tuple_20;

                do {
                    sb.Append( ps.chars, startPos, endPos - startPos );

                    tuple_20 = await ParsePIValueAsync().ConfigureAwait(false);
                    startPos = tuple_20.Item1;
                    endPos = tuple_20.Item2;
    
                } while ( !tuple_20.Item3 );

                sb.Append( ps.chars, startPos, endPos - startPos );

                if ( piInDtdStringBuilder == null ) {
                    curNode.SetValue( stringBuilder.ToString() );
                    stringBuilder.Length = 0;
                }
            }
            return true;
        }

        private async Task< Tuple<int, int, bool> > ParsePIValueAsync() {
            int outStartPos;
            int outEndPos;

            // read new characters into the buffer
            if ( ps.charsUsed - ps.charPos < 2 ) {
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( ps.charsUsed, Res.Xml_UnexpectedEOF, "PI" );
                }
            }

            int pos = ps.charPos;
            char[] chars = ps.chars;
            int rcount = 0;
            int rpos = -1;
            
            for (;;) {

                char tmpch;

#if SILVERLIGHT
                while (xmlCharType.IsTextChar(tmpch = chars[pos]) &&
                    tmpch != '?') {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[tmpch = chars[pos]] & XmlCharType.fText) != 0) &&
                        tmpch != '?') {
                        pos++;
                    }
                }
#endif

                switch ( chars[pos] ) {
                    // possibly end of PI
                    case '?':
                        if ( chars[pos+1] == '>' ) {
                            if ( rcount > 0 ) {
                                Debug.Assert( !ps.eolNormalized );
                                ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                                outEndPos = pos - rcount;
                            }
                            else {
                                outEndPos = pos;
                            }
                            outStartPos = ps.charPos;
                            ps.charPos = pos + 2;

                            return new Tuple<int, int, bool>(outStartPos, outEndPos, true);

                        }
                        else if ( pos+1 == ps.charsUsed ) {
                            goto ReturnPartial;
                        }
                        else {
                            pos++;
                            continue;
                        }
                    // eol
                    case (char)0xA:
                        pos++;
                        OnNewLine( pos );
                        continue;
                    case (char)0xD:
                        if ( chars[pos+1] == (char)0xA ) {
                            if ( !ps.eolNormalized && parsingMode == ParsingMode.Full ) {
                                // EOL normalization of 0xD 0xA
                                if ( pos - ps.charPos > 0 ) {
                                    if ( rcount == 0 ) { 
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else {
                                        ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else {
                                    ps.charPos++;
                                }
                            }
                            pos += 2;
                        }
                        else if ( pos+1 < ps.charsUsed || ps.isEof ) {
                            if ( !ps.eolNormalized ) {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }
                            pos++;
                        }
                        else {
                            goto ReturnPartial;
                        }
                        OnNewLine( pos );
                        continue;
                    case '<':
                    case '&':
                    case ']':
                    case (char)0x9:
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if ( pos == ps.charsUsed ) {
                            goto ReturnPartial;
                        }
                        // surrogate characters
                        else {
                            char ch = chars[pos];
                            if ( XmlCharType.IsHighSurrogate(ch) ) {
                                if ( pos + 1 == ps.charsUsed ) {
                                    goto ReturnPartial;
                                }
                                pos++;
                                if ( XmlCharType.IsLowSurrogate( chars[pos] ) ) {
                                    pos++;
                                    continue;
                                }
                            }
                            ThrowInvalidChar( chars, ps.charsUsed, pos );
                            break;
                        }
                }
                
            }
            
        ReturnPartial:
            if ( rcount > 0 ) {
                ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                outEndPos = pos - rcount;
            }
            else {
                outEndPos = pos;
            }
            outStartPos = ps.charPos;
            ps.charPos = pos;

            return new Tuple<int, int, bool>(outStartPos, outEndPos, false);

        }

        private async Task< bool > ParseCommentAsync() {
            if ( ignoreComments ) {
                ParsingMode oldParsingMode = parsingMode;
                parsingMode = ParsingMode.SkipNode;
                await ParseCDataOrCommentAsync( XmlNodeType.Comment ).ConfigureAwait(false);
                parsingMode = oldParsingMode;
                return false;
            }
            else {
                await ParseCDataOrCommentAsync( XmlNodeType.Comment ).ConfigureAwait(false);
                return true;
            }
        }

        private Task ParseCDataAsync() {
            return ParseCDataOrCommentAsync( XmlNodeType.CDATA );
        }

        // Parses CDATA section or comment
        private async Task ParseCDataOrCommentAsync( XmlNodeType type ) {
            int startPos, endPos;

            if ( parsingMode == ParsingMode.Full ) {
                curNode.SetLineInfo( ps.LineNo, ps.LinePos );
                Debug.Assert( stringBuilder.Length == 0 );

                var tuple_21 = await ParseCDataOrCommentTupleAsync( type).ConfigureAwait(false);
                startPos = tuple_21.Item1;
                endPos = tuple_21.Item2;

                if ( tuple_21.Item3 ) {

                    curNode.SetValueNode( type, ps.chars, startPos, endPos - startPos );
                }
                else {

                    Tuple<int, int, bool> tuple_22;

                    do {
                        stringBuilder.Append( ps.chars, startPos, endPos - startPos );

                        tuple_22 = await ParseCDataOrCommentTupleAsync( type).ConfigureAwait(false);
                        startPos = tuple_22.Item1;
                        endPos = tuple_22.Item2;
    
                    } while ( !tuple_22.Item3 );

                    stringBuilder.Append( ps.chars, startPos, endPos - startPos );
                    curNode.SetValueNode( type, stringBuilder.ToString() );
                    stringBuilder.Length = 0;
                }
            }
            else {

                Tuple<int,int,bool> tuple_23;
                do {
                    tuple_23 = await ParseCDataOrCommentTupleAsync( type).ConfigureAwait(false);
                    startPos = tuple_23.Item1;
                    endPos = tuple_23.Item2;
    
                } while ( !tuple_23.Item3 ) ;

            }
        }

        // Parses a chunk of CDATA section or comment. Returns true when the end of CDATA or comment was reached.

        private async Task< Tuple<int, int, bool> > ParseCDataOrCommentTupleAsync(XmlNodeType type) {
            int outStartPos;
            int outEndPos;

            if ( ps.charsUsed - ps.charPos < 3 ) {
                // read new characters into the buffer
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( Res.Xml_UnexpectedEOF, ( type == XmlNodeType.Comment ) ? "Comment" : "CDATA" );
                }
            }

            int pos = ps.charPos;
            char[] chars = ps.chars;
            int rcount = 0;
            int rpos = -1;
            char stopChar = ( type == XmlNodeType.Comment ) ? '-' : ']';
            
            for (;;) {

                char tmpch;
#if SILVERLIGHT
                while (xmlCharType.IsTextChar(tmpch = chars[pos]) &&
                    tmpch != stopChar) {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[tmpch = chars[pos]] & XmlCharType.fText) != 0) &&
                        tmpch != stopChar) {
                        pos++;
                    }
                }
#endif

                // posibbly end of comment or cdata section
                if ( chars[pos] == stopChar ) {
                    if ( chars[pos+1] == stopChar ) {
                        if ( chars[pos+2] == '>' ) {
                            if ( rcount > 0 ) {
                                Debug.Assert( !ps.eolNormalized );
                                ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                                outEndPos = pos - rcount;
                            }
                            else {
                                outEndPos = pos;
                            }
                            outStartPos = ps.charPos;
                            ps.charPos = pos + 3;

                            return new Tuple<int, int, bool>(outStartPos, outEndPos, true);

                        }
                        else if ( pos+2 == ps.charsUsed ) {
                            goto ReturnPartial;
                        }
                        else if ( type == XmlNodeType.Comment ) {
                            Throw( pos, Res.Xml_InvalidCommentChars );
                        }
                    }
                    else if ( pos+1 == ps.charsUsed ) {
                        goto ReturnPartial;
                    }
                    pos++;
                    continue;
                }
                else {
                    switch ( chars[pos] ) {
                    // eol
                    case (char)0xA:
                        pos++;
                        OnNewLine( pos );
                        continue;
                    case (char)0xD:
                        if ( chars[pos+1] == (char)0xA ) {
                            // EOL normalization of 0xD 0xA - shift the buffer
                            if ( !ps.eolNormalized && parsingMode == ParsingMode.Full ) {
                                if ( pos - ps.charPos > 0 ) {
                                    if ( rcount == 0 ) { 
                                        rcount = 1;
                                        rpos = pos;
                                    }
                                    else {
                                        ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                                        rpos = pos - rcount;
                                        rcount++;
                                    }
                                }
                                else {
                                    ps.charPos++;
                                }
                            }
                            pos += 2;
                        }
                        else if ( pos+1 < ps.charsUsed || ps.isEof ) {
                            if ( !ps.eolNormalized ) {
                                chars[pos] = (char)0xA;             // EOL normalization of 0xD
                            }
                            pos++;
                        }
                        else {
                            goto ReturnPartial;
                        }
                        OnNewLine( pos );
                        continue;
                    case '<':
                    case '&':
                    case ']':
                    case (char)0x9:
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if ( pos == ps.charsUsed ) {
                            goto ReturnPartial;
                        }
                        // surrogate characters
                        char ch = chars[pos];
                        if ( XmlCharType.IsHighSurrogate(ch) ) {
                            if ( pos + 1 == ps.charsUsed ) {
                                goto ReturnPartial;
                            }
                            pos++;
                            if ( XmlCharType.IsLowSurrogate( chars[pos] ) ) {
                                pos++;
                                continue;
                            }
                        }
                        ThrowInvalidChar( chars, ps.charsUsed, pos );
                        break;
                    }
                }
            
            ReturnPartial:
                if ( rcount > 0 ) {
                    ShiftBuffer( rpos + rcount, rpos, pos - rpos - rcount );
                    outEndPos = pos - rcount;
                }
                else {
                    outEndPos = pos;
                }
                outStartPos = ps.charPos;

                ps.charPos = pos;

                return new Tuple<int, int, bool>(outStartPos, outEndPos, false);

            }
        }

        // Parses DOCTYPE declaration
        private async Task< bool > ParseDoctypeDeclAsync() {
            if ( dtdProcessing == DtdProcessing.Prohibit ) {
                ThrowWithoutLineInfo( v1Compat ? Res.Xml_DtdIsProhibited : Res.Xml_DtdIsProhibitedEx );
            }

            // parse 'DOCTYPE'
            while ( ps.charsUsed - ps.charPos < 8 ) {
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    Throw( Res.Xml_UnexpectedEOF, "DOCTYPE" );
                }
            }
            if ( !XmlConvert.StrEqual( ps.chars, ps.charPos, 7, "DOCTYPE" ) ) {
                ThrowUnexpectedToken( ( !rootElementParsed && dtdInfo == null ) ? "DOCTYPE" : "<!--" );
            }
            if ( !xmlCharType.IsWhiteSpace( ps.chars[ps.charPos + 7] ) ) {
                ThrowExpectingWhitespace( ps.charPos + 7 );
            }

            if ( dtdInfo != null ) {
                Throw( ps.charPos - 2, Res.Xml_MultipleDTDsProvided );  // position just before <!DOCTYPE
            }
            if ( rootElementParsed ) {
                Throw( ps.charPos - 2, Res.Xml_DtdAfterRootElement );
            }

            ps.charPos += 8;

            await EatWhitespacesAsync( null ).ConfigureAwait(false);

            // Parse DTD
            if (dtdProcessing == DtdProcessing.Parse) {
                curNode.SetLineInfo(ps.LineNo, ps.LinePos);

                await ParseDtdAsync().ConfigureAwait(false);

                nextParsingFunction = parsingFunction;
                parsingFunction = ParsingFunction.ResetAttributesRootLevel;
                return true;
            }
            // Skip DTD
            else {
                Debug.Assert(dtdProcessing == DtdProcessing.Ignore);

                await SkipDtdAsync().ConfigureAwait(false);
                return false;
            }
        }

        private async Task ParseDtdAsync() {
            IDtdParser dtdParser = DtdParser.Create();

            dtdInfo = await dtdParser.ParseInternalDtdAsync(new DtdParserProxy(this), true).ConfigureAwait(false);

#if SILVERLIGHT // Needed only for XmlTextReader and XmlValidatingReader
            if (dtdInfo.HasDefaultAttributes || dtdInfo.HasNonCDataAttributes) {
#else 
                if ( ( validatingReaderCompatFlag || !v1Compat ) && ( dtdInfo.HasDefaultAttributes || dtdInfo.HasNonCDataAttributes ) ) {
#endif
                addDefaultAttributesAndNormalize = true;
            }

            curNode.SetNamedNode(XmlNodeType.DocumentType, dtdInfo.Name.ToString(), string.Empty, null);
            curNode.SetValue(dtdInfo.InternalDtdSubset);
        }

        private async Task SkipDtdAsync() {
            int colonPos;

            // parse dtd name

            var tuple_24 = await ParseQNameAsync().ConfigureAwait(false);
            colonPos = tuple_24.Item1;

            int pos = tuple_24.Item2;

            ps.charPos = pos;

            // check whitespace
            await EatWhitespacesAsync( null ).ConfigureAwait(false);

            // PUBLIC Id
            if ( ps.chars[ps.charPos] == 'P' ) {
                // make sure we have enough characters
                while ( ps.charsUsed - ps.charPos < 6 ) {
                    if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                        Throw( Res.Xml_UnexpectedEOF1 );
                    }
                }
                // check 'PUBLIC'
                if ( !XmlConvert.StrEqual( ps.chars, ps.charPos, 6, "PUBLIC" ) ) {
                    ThrowUnexpectedToken( "PUBLIC" );
                }   
                ps.charPos += 6;

                // check whitespace
                if ( await EatWhitespacesAsync( null ).ConfigureAwait(false) == 0 ) {
                    ThrowExpectingWhitespace( ps.charPos );
                }

                // parse PUBLIC value
                await SkipPublicOrSystemIdLiteralAsync().ConfigureAwait(false);

                // check whitespace
                if ( await EatWhitespacesAsync( null ).ConfigureAwait(false) == 0 ) {
                    ThrowExpectingWhitespace( ps.charPos );
                }

                // parse SYSTEM value
                await SkipPublicOrSystemIdLiteralAsync().ConfigureAwait(false);

                await EatWhitespacesAsync( null ).ConfigureAwait(false);
            }
            else if ( ps.chars[ps.charPos] == 'S' ) {
                // make sure we have enough characters
                while ( ps.charsUsed - ps.charPos < 6 ) {
                    if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                        Throw( Res.Xml_UnexpectedEOF1 );
                    }
                }
                // check 'SYSTEM'
                if ( !XmlConvert.StrEqual( ps.chars, ps.charPos, 6, "SYSTEM" ) ) {
                    ThrowUnexpectedToken( "SYSTEM" );
                }   
                ps.charPos += 6;

                // check whitespace
                if ( await EatWhitespacesAsync( null ).ConfigureAwait(false) == 0 ) {
                    ThrowExpectingWhitespace( ps.charPos );
                }

                // parse SYSTEM value
                await SkipPublicOrSystemIdLiteralAsync().ConfigureAwait(false);

                await EatWhitespacesAsync( null ).ConfigureAwait(false);
            }
            else if ( ps.chars[ps.charPos] != '[' && ps.chars[ps.charPos] != '>' ) {
                Throw(Res.Xml_ExpectExternalOrClose);
            }

            // internal DTD
            if ( ps.chars[ps.charPos] == '[' ) {
                ps.charPos++;

                await SkipUntilAsync( ']', true ).ConfigureAwait(false);

                await EatWhitespacesAsync( null ).ConfigureAwait(false);
                if ( ps.chars[ps.charPos] != '>' ) {
                    ThrowUnexpectedToken( ">" );
                }
            }
            else if ( ps.chars[ps.charPos] == '>' ) {
                curNode.SetValue( string.Empty );
            }
            else {
                Throw( Res.Xml_ExpectSubOrClose );
            }
            ps.charPos++;
        }

        Task SkipPublicOrSystemIdLiteralAsync() {
            // check quote char
            char quoteChar = ps.chars[ps.charPos];
            if ( quoteChar != '"' && quoteChar != '\'' ) {
                ThrowUnexpectedToken( "\"", "'" );
            }

            ps.charPos++;
            return SkipUntilAsync( quoteChar, false );
        }

        async Task SkipUntilAsync( char stopChar, bool recognizeLiterals ) {
            bool inLiteral = false;
            bool inComment = false;
            bool inPI = false;
            char literalQuote = '"';

            char[] chars = ps.chars;
            int pos = ps.charPos;

            for (; ; ) {
                char ch;

#if SILVERLIGHT
                while ( xmlCharType.IsAttributeValueChar( ch = chars[pos] ) && ch != stopChar && ch != '-' && ch != '?') {
                    pos++;
                }
#else // Optimization due to the lack of inlining when a method uses byte*
                unsafe {
                    while (((xmlCharType.charProperties[ch = chars[pos]] & XmlCharType.fAttrValue) != 0) && chars[pos] != stopChar && ch != '-' && ch != '?') {
                        pos++;
                    }
                }
#endif

                // closing stopChar outside of literal and ignore/include sections -> save value & return
                if ( ch == stopChar && !inLiteral ) {
                    ps.charPos = pos + 1;
                    return;
                }

                // handle the special character
                ps.charPos = pos;
                switch ( ch ) {
                    // eol
                    case (char)0xA:
                        pos++;
                        OnNewLine( pos );
                        continue;
                    case (char)0xD:
                        if ( chars[pos+1] == (char)0xA ) {
                            pos += 2;
                        }
                        else if ( pos+1 < ps.charsUsed || ps.isEof ) { 
                            pos++;
                        } 
                        else {
                            goto ReadData;
                        }
                        OnNewLine( pos );
                        continue;

                    // comment, PI
                    case '<':
                        // processing instruction
                        if ( chars[pos + 1] == '?' ) {
                            if ( recognizeLiterals && !inLiteral && !inComment ) {
                                inPI = true;
                                pos += 2;
                                continue;
                            }
                        }
                        // comment
                        else if ( chars[pos + 1] == '!' ) {
                            if ( pos + 3 >= ps.charsUsed && !ps.isEof ) {
                                goto ReadData;
                            }
                            if ( chars[pos+2] == '-' && chars[pos+3] == '-' ) {
                                if ( recognizeLiterals && !inLiteral && !inPI ) {
                                    inComment = true;
                                    pos += 4;
                                    continue;
                                }
                            }
                        }
                        // need more data
                        else if ( pos + 1 >= ps.charsUsed && !ps.isEof ) {
                            goto ReadData;
                        }
                        pos++;
                        continue;
                    case '-':
                        // end of comment
                        if ( inComment ) {
                            if ( pos + 2 >= ps.charsUsed && !ps.isEof ) {
                                goto ReadData;
                            }
                            if ( chars[pos + 1] == '-' && chars[pos + 2] == '>' ) {
                                inComment = false;
                                pos += 2;
                                continue;
                            }
                        }
                        pos++;
                        continue;

                    case '?':
                        // end of processing instruction
                        if (inPI) {
                            if (pos + 1 >= ps.charsUsed && !ps.isEof) {
                                goto ReadData;
                            }
                            if (chars[pos + 1] == '>') {
                                inPI = false;
                                pos += 1;
                                continue;
                            }
                        }
                        pos++;
                        continue;

                    case (char)0x9:
                    case '>':
                    case ']':
                    case '&':
                        pos++;
                        continue;
                    case '"':
                    case '\'':
                        if ( inLiteral ) {
                            if ( literalQuote == ch ) {
                                inLiteral = false;
                            }
                        }
                        else {
                            if ( recognizeLiterals && !inComment && !inPI ) {
                                inLiteral = true;
                                literalQuote = ch;
                            }
                        }
                        pos++;
                        continue;
                    default:
                        // end of buffer
                        if ( pos == ps.charsUsed ) {
                            goto ReadData;
                        }
                        // surrogate chars
                        else { 
                            char tmpCh = chars[pos];
                            if ( XmlCharType.IsHighSurrogate( tmpCh ) ) {
                                if ( pos + 1 == ps.charsUsed ) {
                                    goto ReadData;
                                }
                                pos++;
                                if ( XmlCharType.IsLowSurrogate( chars[pos] ) ) {
                                    pos++;
                                    continue;
                                }
                            }
                            ThrowInvalidChar( chars, ps.charsUsed, pos );
                            break;
                        }
                }
            
            ReadData:
                // read new characters into the buffer
                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( ps.charsUsed - ps.charPos > 0 ) {
                        if ( ps.chars[ps.charPos] != (char)0xD ) {
                            Debug.Assert( false, "We should never get to this point." );
                            Throw( Res.Xml_UnexpectedEOF1 );
                        }
                        Debug.Assert( ps.isEof );
                    }
                    else {
                        Throw( Res.Xml_UnexpectedEOF1 );
                    }
                }
                chars = ps.chars;
                pos = ps.charPos;
            }
        }

        private async Task< int > EatWhitespacesAsync( BufferBuilder sb ) {
            int pos = ps.charPos;
            int wsCount = 0;
            char[] chars = ps.chars;

            for (;;) {
                for (;;) {
                    switch ( chars[pos] ) {
                        case (char)0xA:
                            pos++;
                            OnNewLine( pos );
                            continue;
                        case (char)0xD:
                            if ( chars[pos+1] == (char)0xA ) {
                                int tmp1 = pos - ps.charPos;
                                if ( sb != null && !ps.eolNormalized ) {
                                    if ( tmp1 > 0 ) {
                                        sb.Append( chars, ps.charPos, tmp1 );
                                        wsCount += tmp1;
                                    }
                                    ps.charPos = pos + 1;
                                }
                                pos += 2;
                            }
                            else if ( pos+1 < ps.charsUsed || ps.isEof ) {
                                if ( !ps.eolNormalized ) {
                                    chars[pos] = (char)0xA;             // EOL normalization of 0xD
                                }
                                pos++;
                            }
                            else {
                                goto ReadData;
                            }
                            OnNewLine( pos );
                            continue;
                        case (char)0x9:
                        case (char)0x20:
                            pos++;
                            continue;
                        default:
                            if ( pos == ps.charsUsed ) {
                                goto ReadData;
                            }
                            else {
                                int tmp2 = pos - ps.charPos;
                                if ( tmp2 > 0 ) {
                                    if ( sb != null  ) {
                                        sb.Append( ps.chars, ps.charPos, tmp2 );
                                    }
                                    ps.charPos = pos;
                                    wsCount += tmp2;
                                }
                                return wsCount;
                            }
                    }
                }

            ReadData:
                int tmp3 = pos - ps.charPos;
                if ( tmp3 > 0 ) {
                    if ( sb != null  ) {
                        sb.Append( ps.chars, ps.charPos, tmp3 );
                    }
                    ps.charPos = pos;
                    wsCount += tmp3;
                }

                if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                    if ( ps.charsUsed - ps.charPos == 0 ) {
                        return wsCount;
                    }
                    if ( ps.chars[ps.charPos] != (char)0xD ) {
                        Debug.Assert( false, "We should never get to this point." );
                        Throw( Res.Xml_UnexpectedEOF1 );
                    }
                    Debug.Assert( ps.isEof );
                }
                pos = ps.charPos;
                chars = ps.chars;
            }
        }

        // Parses numeric character entity reference (e.g. &#32; &#x20;).
        //      - replaces the last one or two character of the entity reference (';' and the character before) with the referenced 
        //        character or surrogates pair (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character

        private async Task< Tuple<EntityType, int> > ParseNumericCharRefAsync(bool expand, BufferBuilder internalSubsetBuilder) {
            EntityType entityType;

            for (;;) {
                int newPos;
                int charCount;
                switch ( newPos = ParseNumericCharRefInline( ps.charPos, expand, internalSubsetBuilder, out charCount, out entityType ) ) {
                    case -2:
                        // read new characters in the buffer
                        if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                            Throw( Res.Xml_UnexpectedEOF );
                        }
                        Debug.Assert( ps.chars[ps.charPos] == '&' );
                        continue;
                    default:
                        if ( expand ) {
                            ps.charPos = newPos - charCount;
                        }

                        return new Tuple<EntityType, int>(entityType, newPos);

                }
            }
        }

        // Parses named character entity reference (&amp; &apos; &lt; &gt; &quot;).
        // Returns -1 if the reference is not a character entity reference.
        // Otherwise 
        //      - replaces the last character of the entity reference (';') with the referenced character (if expand == true)
        //      - returns position of the end of the character reference, that is of the character next to the original ';'
        //      - if (expand == true) then ps.charPos is changed to point to the replaced character
        private async Task< int > ParseNamedCharRefAsync( bool expand, BufferBuilder internalSubsetBuilder ) {
            for (;;) {
                int newPos;
                switch ( newPos = ParseNamedCharRefInline( ps.charPos, expand, internalSubsetBuilder ) ) {
                    case -1:
                        return -1;
                    case -2:
                        // read new characters in the buffer
                        if ( await ReadDataAsync().ConfigureAwait(false) == 0 ) {
                            return -1;
                        }
                        Debug.Assert( ps.chars[ps.charPos] == '&' );
                        continue;
                    default:
                        if ( expand ) {
                            ps.charPos = newPos - 1;
                        }
                        return newPos;
                }
            }
        }

        private async Task< int > ParseNameAsync() {

            var tuple_25 = await ParseQNameAsync( false,  0).ConfigureAwait(false);
            return tuple_25.Item2;

        }

        private Task< Tuple<int, int> > ParseQNameAsync() {

            return ParseQNameAsync( true,  0);

        }

        private async Task< Tuple<int, int> > ParseQNameAsync(bool isQName, int startOffset) {
            int colonPos;

            int colonOffset = -1;
            int pos = ps.charPos + startOffset;

        ContinueStartName:
            char[] chars = ps.chars;

            //a tmp flag, used to avoid await keyword in unsafe context.
            bool awaitReadDataInNameAsync = false;
            // start name char
            unsafe {
#if SILVERLIGHT
                if ( xmlCharType.IsStartNCNameSingleChar( chars[pos] ) ) {
#else // Optimization due to the lack of inlining when a method uses byte*
                if ((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCStartNameSC) != 0) {
#endif
                    pos++;
                }

#if XML10_FIFTH_EDITION
                else if ( pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar( chars[pos + 1], chars[pos] ) ) {
                    pos += 2;
                }
#endif
                else {
                    if (pos + 1 >= ps.charsUsed) {
                        awaitReadDataInNameAsync = true;
                    }
                    else if (chars[pos] != ':' || supportNamespaces) {
                        Throw(pos, Res.Xml_BadStartNameChar, XmlException.BuildCharExceptionArgs(chars, ps.charsUsed, pos));
                    }
                }
            }

            if (awaitReadDataInNameAsync) {
                var tuple_27 = await ReadDataInNameAsync(pos).ConfigureAwait(false);
                pos = tuple_27.Item1;

                if (tuple_27.Item2) {
                    goto ContinueStartName;
                }
                Throw(pos, Res.Xml_UnexpectedEOF, "Name");
            }

        ContinueName:
            // parse name
            unsafe {
                for (;;) {
#if SILVERLIGHT
                    if ( xmlCharType.IsNCNameSingleChar( chars[pos] )) {
#else // Optimization due to the lack of inlining when a method uses byte*
                    if (((xmlCharType.charProperties[chars[pos]] & XmlCharType.fNCNameSC) != 0)) {
#endif
                        pos++;
                    }
#if XML10_FIFTH_EDITION
                    else if ( pos + 1 < ps.charsUsed && xmlCharType.IsNCNameSurrogateChar( chars[pos + 1], chars[pos] ) ) {
                        pos += 2;
                    }
#endif
                    else {
                        break;
                    }
                }
            }

            // colon
            if ( chars[pos] == ':' ) {
                if ( supportNamespaces ) {
                    if ( colonOffset != -1 || !isQName ) {
                        Throw(pos, Res.Xml_BadNameChar, XmlException.BuildCharExceptionArgs(':', '\0'));
                    }
                    colonOffset = pos - ps.charPos;
                    pos++;
                    goto ContinueStartName;
                }
                else {
                    colonOffset = pos - ps.charPos;
                    pos++;
                    goto ContinueName;
                }
            }
            // end of buffer
            else if ( pos == ps.charsUsed 
#if XML10_FIFTH_EDITION
                || ( pos + 1 == ps.charsUsed && xmlCharType.IsNCNameHighSurrogateChar( chars[pos] ) ) 
#endif
                ) {

                var tuple_28 = await ReadDataInNameAsync(pos).ConfigureAwait(false);
                pos = tuple_28.Item1;

                if ( tuple_28.Item2 ) {

                    chars = ps.chars;
                    goto ContinueName;
                }
                Throw( pos, Res.Xml_UnexpectedEOF, "Name" );
            }

            // end of name
            colonPos = ( colonOffset == -1 ) ? -1 : ps.charPos + colonOffset;

            return new Tuple<int, int>(colonPos, pos);

        }

        private async Task< Tuple<int, bool> > ReadDataInNameAsync(int pos) {

            int offset = pos - ps.charPos;
            bool newDataRead = ( await ReadDataAsync().ConfigureAwait(false) != 0 );
            pos = ps.charPos + offset;

            return new Tuple<int, bool>(pos, newDataRead);

        }

#if !SILVERLIGHT
        private async Task< string > ParseEntityNameAsync() {
            int endPos;
            try {
                endPos = await ParseNameAsync().ConfigureAwait(false);
            }
            catch ( XmlException ) {
                Throw( Res.Xml_ErrorParsingEntityName );
                return null;
            }

            // check ';'
            if ( ps.chars[endPos] != ';' ) {
                Throw( Res.Xml_ErrorParsingEntityName );
            }

            string entityName = nameTable.Add( ps.chars, ps.charPos, endPos - ps.charPos );
            ps.charPos = endPos + 1;
            return entityName;
        }
#endif

        // This method resolves and opens an external DTD subset or an external entity based on its SYSTEM or PUBLIC ID.
        // SxS: This method may expose a name if a resource in baseUri (ref) parameter. 
#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.Machine)]
#endif

        private async Task PushExternalEntityOrSubsetAsync(string publicId, string systemId, Uri baseUri, string entityName) {

            Uri uri;

            // First try opening the external reference by PUBLIC Id
            if ( !string.IsNullOrEmpty( publicId ) ) {
                try {
                    uri = xmlResolver.ResolveUri(baseUri, publicId);
                    if ( await OpenAndPushAsync( uri ).ConfigureAwait(false) ) {

                        return;

                    }
                }
                catch ( Exception ) {
                    // Intentionally empty - ---- all exception related to PUBLIC ID and try opening the entity via the SYSTEM ID
                }
            }

            // Then try SYSTEM Id
            uri = xmlResolver.ResolveUri( baseUri, systemId );
            try {
                if ( await OpenAndPushAsync( uri ).ConfigureAwait(false) ) {

                    return;

                }
                // resolver returned null, throw exception outside this try-catch
            }
            catch ( Exception e ) {
                if ( v1Compat ) {
                    throw;
                }
                string innerMessage;
#if SILVERLIGHT // This is to remove the second "An error occured" from "An error has occurred while opening external entity 'bla.ent': An error occurred."
                innerMessage = string.Empty;
#else 
                innerMessage = e.Message;
#endif
                Throw( new XmlException( entityName == null ? Res.Xml_ErrorOpeningExternalDtd : Res.Xml_ErrorOpeningExternalEntity, new string[] { uri.ToString(), innerMessage }, e, 0, 0 ) );
            }

            if ( entityName == null ) {
                ThrowWithoutLineInfo( Res.Xml_CannotResolveExternalSubset, new string[] { ( publicId != null ? publicId : string.Empty ), systemId }, null );
            }
            else {
                Throw( dtdProcessing == DtdProcessing.Ignore ? Res.Xml_CannotResolveEntityDtdIgnored : Res.Xml_CannotResolveEntity, entityName );
            }

            return;

        }

        // This method opens the URI as a TextReader or Stream, pushes new ParsingStateState on the stack and calls InitStreamInput or InitTextReaderInput.
        // Returns:
        //    - true when everything went ok.
        //    - false when XmlResolver.GetEntity returned null
        // Propagates any exceptions from the XmlResolver indicating when the URI cannot be opened.
        private async Task< bool > OpenAndPushAsync( Uri uri ) {
            Debug.Assert( xmlResolver != null );
            
            // First try to get the data as a TextReader
            if ( xmlResolver.SupportsType( uri, typeof( TextReader ) ) ) {
                TextReader textReader = (TextReader) await xmlResolver.GetEntityAsync( uri, null, typeof( TextReader ) ).ConfigureAwait(false);
                if ( textReader == null ) {
                    return false;
                }

                PushParsingState();
                await InitTextReaderInputAsync( uri.ToString(), uri, textReader ).ConfigureAwait(false);
            }
            else {
                // Then try get it as a Stream
                Debug.Assert( xmlResolver.SupportsType( uri, typeof( Stream ) ), "Stream must always be a supported type in XmlResolver" );

                Stream stream = (Stream)await xmlResolver.GetEntityAsync(uri, null, typeof(Stream)).ConfigureAwait(false);
                if ( stream == null ) {
                    return false;
                }

                PushParsingState();
                await InitStreamInputAsync( uri, stream, null ).ConfigureAwait(false);
            }
            return true;
        }

        // returns true if real entity has been pushed, false if fake entity (=empty content entity)
        // SxS: The method neither takes any name of resource directly nor it exposes any resource to the caller. 
        // Entity info was created based on source document. It's OK to suppress the SxS warning
#if !SILVERLIGHT
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
#endif
        private async Task< bool > PushExternalEntityAsync( IDtdEntityInfo entity ) {
            Debug.Assert( entity.IsExternal );

            if (!IsResolverNull) {

                Uri entityBaseUri = null;
                // Resolve base URI
                if (!string.IsNullOrEmpty(entity.BaseUriString)) {
                    entityBaseUri = xmlResolver.ResolveUri(null, entity.BaseUriString);
                }
                await PushExternalEntityOrSubsetAsync( entity.PublicId,  entity.SystemId, entityBaseUri,  entity.Name ).ConfigureAwait(false);

                RegisterEntity(entity);

                Debug.Assert( ps.appendMode );
                int initialPos = ps.charPos;
                if ( v1Compat ) {
                    await EatWhitespacesAsync( null ).ConfigureAwait(false);
                }
                if ( !await ParseXmlDeclarationAsync( true ).ConfigureAwait(false) ) {
                    ps.charPos = initialPos;
                }
                return true;
            }
            else {
                Encoding enc = ps.encoding;

                PushParsingState();
                InitStringInput( entity.SystemId, enc, string.Empty );

                RegisterEntity(entity);

                RegisterConsumedCharacters(0, true);

                return false;
            }
        }

        // This method is used to enable parsing of zero-terminated streams. The old XmlTextReader implementation used 
        // to parse such streams, we this one needs to do that as well. 
        // If the last characters decoded from the stream is 0 and the stream is in EOF state, this method will remove 
        // the character from the parsing buffer (decrements ps.charsUsed).
        // Note that this method calls ReadData() which may change the value of ps.chars and ps.charPos.
        private async Task< bool > ZeroEndingStreamAsync( int pos ) {
#if !SILVERLIGHT || FEATURE_NETCORE
            if ( v1Compat && pos == ps.charsUsed - 1 && ps.chars[pos] == (char)0 && await ReadDataAsync().ConfigureAwait(false) == 0 && ps.isStreamEof ) {
                ps.charsUsed--;
                return true;
            }
#endif
            return false;
        }

        private async Task ParseDtdFromParserContextAsync() {
            Debug.Assert( dtdInfo == null && fragmentParserContext != null && fragmentParserContext.HasDtdInfo );

            IDtdParser dtdParser = DtdParser.Create();

            // Parse DTD
            dtdInfo = await dtdParser.ParseFreeFloatingDtdAsync(fragmentParserContext.BaseURI, fragmentParserContext.DocTypeName, fragmentParserContext.PublicId,                                                     fragmentParserContext.SystemId, fragmentParserContext.InternalSubset, new DtdParserProxy( this ) ).ConfigureAwait(false);

#if SILVERLIGHT // Needed only for XmlTextReader or XmlValidatingReader
            if (dtdInfo.HasDefaultAttributes || dtdInfo.HasNonCDataAttributes) {
#else 
            if ( ( validatingReaderCompatFlag || !v1Compat ) && ( dtdInfo.HasDefaultAttributes || dtdInfo.HasNonCDataAttributes ) ) {
#endif
                addDefaultAttributesAndNormalize = true;
            }
        }

        async Task< bool > InitReadContentAsBinaryAsync() {
            Debug.Assert( parsingFunction != ParsingFunction.InReadContentAsBinary );

            if ( parsingFunction == ParsingFunction.InReadValueChunk ) {
                throw new InvalidOperationException( Res.GetString( Res.Xml_MixingReadValueChunkWithBinary ) );
            }
            if ( parsingFunction == ParsingFunction.InIncrementalRead ) {
                throw new InvalidOperationException( Res.GetString( Res.Xml_MixingV1StreamingWithV2Binary ) );
            }

            if ( !XmlReader.IsTextualNode( curNode.type ) ) {
                if ( !await MoveToNextContentNodeAsync( false ).ConfigureAwait(false) ) {
                    return false;
                }
            }

            SetupReadContentAsBinaryState( ParsingFunction.InReadContentAsBinary );
            incReadLineInfo.Set( curNode.LineNo, curNode.LinePos );
            return true;
        }

        async Task< bool > InitReadElementContentAsBinaryAsync() {
            Debug.Assert( parsingFunction != ParsingFunction.InReadElementContentAsBinary );
            Debug.Assert( curNode.type == XmlNodeType.Element );

            bool isEmpty = curNode.IsEmptyElement;

            // move to content or off the empty element
            await outerReader.ReadAsync().ConfigureAwait(false);
            if ( isEmpty ) {
                return false;
            }

            // make sure we are on a content node
            if ( !await MoveToNextContentNodeAsync( false ).ConfigureAwait(false) ) {
                if ( curNode.type != XmlNodeType.EndElement ) {
                    Throw( Res.Xml_InvalidNodeType, curNode.type.ToString() );
                }
                // move off end element
                await outerReader.ReadAsync().ConfigureAwait(false);
                return false;
            }
            SetupReadContentAsBinaryState( ParsingFunction.InReadElementContentAsBinary );
            incReadLineInfo.Set( curNode.LineNo, curNode.LinePos );
            return true;
        }

        async Task< bool > MoveToNextContentNodeAsync( bool moveIfOnContentNode ) {
            do {
                switch ( curNode.type ) {
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
                        outerReader.ResolveEntity();
                        break;
                    default:
                        return false;
                }
                moveIfOnContentNode = false;
            } while ( await outerReader.ReadAsync().ConfigureAwait(false) );
            return false;
        }

        async Task< int > ReadContentAsBinaryAsync( byte[] buffer, int index, int count ) { 
            Debug.Assert( incReadDecoder != null );

            if ( incReadState == IncrementalReadState.ReadContentAsBinary_End ) {
                return 0;
            }

            incReadDecoder.SetNextOutputBuffer( buffer, index, count );

            for (;;) {
                // read what is already cached in curNode
                int charsRead = 0;
                try {
                    charsRead = curNode.CopyToBinary( incReadDecoder, readValueOffset );
                }
                // add line info to the exception
                catch ( XmlException e ) {
                    curNode.AdjustLineInfo( readValueOffset, ps.eolNormalized, ref incReadLineInfo );
                    ReThrow( e, incReadLineInfo.lineNo, incReadLineInfo.linePos );
                }
                readValueOffset += charsRead;

                if ( incReadDecoder.IsFull ) {
                    return incReadDecoder.DecodedCount;
                }

                // if on partial value, read the rest of it
                if ( incReadState == IncrementalReadState.ReadContentAsBinary_OnPartialValue ) {
                    curNode.SetValue( string.Empty );

                    // read next chunk of text
                    bool endOfValue = false;
                    int startPos = 0;
                    int endPos = 0;
                    while ( !incReadDecoder.IsFull && !endOfValue ) {
                        int orChars = 0;

                        // store current line info and parse more text
                        incReadLineInfo.Set( ps.LineNo, ps.LinePos );

                        var tuple_36 = await ParseTextAsync(orChars).ConfigureAwait(false);
                        startPos = tuple_36.Item1;
                        endPos = tuple_36.Item2;
                        orChars = tuple_36.Item3;

                        endOfValue = tuple_36.Item4;

                        try {
                            charsRead = incReadDecoder.Decode( ps.chars, startPos, endPos - startPos ); 
                        }
                        // add line info to the exception
                        catch ( XmlException e ) {
                            ReThrow( e, incReadLineInfo.lineNo, incReadLineInfo.linePos);
                        }
                        startPos += charsRead;
                    }
                    incReadState = endOfValue ? IncrementalReadState.ReadContentAsBinary_OnCachedValue : IncrementalReadState.ReadContentAsBinary_OnPartialValue;
                    readValueOffset = 0;

                    if ( incReadDecoder.IsFull ) {
                        curNode.SetValue( ps.chars, startPos, endPos - startPos );
                        // adjust line info for the chunk that has been already decoded
                        AdjustLineInfo( ps.chars, startPos - charsRead, startPos, ps.eolNormalized, ref incReadLineInfo );
                        curNode.SetLineInfo( incReadLineInfo.lineNo, incReadLineInfo.linePos );
                        return incReadDecoder.DecodedCount;
                    }
                }

                // reset to normal state so we can call Read() to move forward
                ParsingFunction tmp = parsingFunction;
                parsingFunction = nextParsingFunction;
                nextParsingFunction = nextNextParsingFunction;

                // move to next textual node in the element content; throw on sub elements
                if ( !await MoveToNextContentNodeAsync( true ).ConfigureAwait(false) ) {
                    SetupReadContentAsBinaryState( tmp );
                    incReadState = IncrementalReadState.ReadContentAsBinary_End;
                    return incReadDecoder.DecodedCount;
                }
                SetupReadContentAsBinaryState( tmp );
                incReadLineInfo.Set( curNode.LineNo, curNode.LinePos );
            }
        }

        async Task< int > ReadElementContentAsBinaryAsync( byte[] buffer, int index, int count ) { 
            if ( count == 0 ) {
                return 0;
            }
            int decoded = await ReadContentAsBinaryAsync( buffer, index, count ).ConfigureAwait(false);
            if ( decoded > 0 ) {
                return decoded;
            }

            // if 0 bytes returned check if we are on a closing EndElement, throw exception if not
            if ( curNode.type != XmlNodeType.EndElement ) {
                throw new XmlException( Res.Xml_InvalidNodeType, curNode.type.ToString(), this as IXmlLineInfo );
            }

            // reset state
            parsingFunction = nextParsingFunction;
            nextParsingFunction = nextNextParsingFunction;
            Debug.Assert( parsingFunction != ParsingFunction.InReadElementContentAsBinary );

            // move off the EndElement
            await outerReader.ReadAsync().ConfigureAwait(false);
            return 0;
        }

    }
}

