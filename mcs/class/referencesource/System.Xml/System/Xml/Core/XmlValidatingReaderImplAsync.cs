
using System;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Security.Policy;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Runtime.Versioning;

using System.Threading.Tasks;

namespace System.Xml
{
    internal sealed partial class XmlValidatingReaderImpl : XmlReader, IXmlLineInfo, IXmlNamespaceResolver {

        // Returns the text value of the current node.
        public override Task<string> GetValueAsync() {
            return coreReader.GetValueAsync();
        }

        // Reads and validated next node from the input data
        public override async Task< bool > ReadAsync() {
            switch ( parsingFunction ) {
                case ParsingFunction.Read:
                    if ( await coreReader.ReadAsync().ConfigureAwait(false) ) {
                        ProcessCoreReaderEvent();
                        return true;
                    }
                    else {
                        validator.CompleteValidation();
                        return false;
                    }
                case ParsingFunction.ParseDtdFromContext:
                    parsingFunction = ParsingFunction.Read;
                    await ParseDtdFromParserContextAsync().ConfigureAwait(false);
                    goto case ParsingFunction.Read;
                case ParsingFunction.Error:
                case ParsingFunction.ReaderClosed:
                    return false;
                case ParsingFunction.Init:
                    parsingFunction = ParsingFunction.Read; // this changes the value returned by ReadState
                    if ( coreReader.ReadState == ReadState.Interactive ) {
                        ProcessCoreReaderEvent();
                        return true;
                    }
                    else {
                        goto case ParsingFunction.Read;
                    }
                case ParsingFunction.ResolveEntityInternally:
                    parsingFunction = ParsingFunction.Read;
                    await ResolveEntityInternallyAsync().ConfigureAwait(false);
                    goto case ParsingFunction.Read;
                case ParsingFunction.InReadBinaryContent:
                    parsingFunction = ParsingFunction.Read;
                    await readBinaryHelper.FinishAsync().ConfigureAwait(false);
                    goto case ParsingFunction.Read;
                default:
                    Debug.Assert( false );
                    return false;
            }
        }

        public override async Task< int > ReadContentAsBase64Async( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper if called the first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper when called first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadElementContentAsBase64Async( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper if called the first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBase64Async( buffer, index, count ).ConfigureAwait(false);

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadElementContentAsBinHexAsync( byte[] buffer, int index, int count ) {
            if ( ReadState != ReadState.Interactive ) {
                return 0;
            }

            // init ReadChunkHelper when called first time
            if ( parsingFunction != ParsingFunction.InReadBinaryContent ) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset( readBinaryHelper, outerReader );
            }

            // set parsingFunction to Read state in order to have a normal Read() behavior when called from readBinaryHelper
            parsingFunction = ParsingFunction.Read;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBinHexAsync( buffer, index, count ).ConfigureAwait(false);

            // setup parsingFunction 
            parsingFunction = ParsingFunction.InReadBinaryContent;
            return readCount;
        }

        internal async Task MoveOffEntityReferenceAsync() {
            if ( outerReader.NodeType == XmlNodeType.EntityReference && parsingFunction != ParsingFunction.ResolveEntityInternally ) {
                if ( !await outerReader.ReadAsync().ConfigureAwait(false) ) {
                    throw new InvalidOperationException( Res.GetString(Res.Xml_InvalidOperation ) );
                }
            }
        }

        // Returns typed value of the current node (based on the type specified by schema)
        public async Task< object > ReadTypedValueAsync() {
            if ( validationType == ValidationType.None ) {
                return null;
            }

            switch ( outerReader.NodeType ) {
                case XmlNodeType.Attribute:
                    return coreReaderImpl.InternalTypedValue;
                case XmlNodeType.Element:
                    if ( SchemaType == null ) {
                        return null;
                    }
                    XmlSchemaDatatype dtype = ( SchemaType is XmlSchemaDatatype ) ? (XmlSchemaDatatype)SchemaType : ((XmlSchemaType)SchemaType).Datatype;
                    if ( dtype != null ) {
                        if ( !outerReader.IsEmptyElement ) {
                            for (;;) {
                                if ( !await outerReader.ReadAsync().ConfigureAwait(false) ) {
                                    throw new InvalidOperationException( Res.GetString( Res.Xml_InvalidOperation ) );
                                }
                                XmlNodeType type = outerReader.NodeType;
                                if ( type != XmlNodeType.CDATA && type != XmlNodeType.Text &&
                                    type != XmlNodeType.Whitespace && type != XmlNodeType.SignificantWhitespace &&
                                    type != XmlNodeType.Comment && type != XmlNodeType.ProcessingInstruction ) {
                                    break;
                                }
                            }
                            if ( outerReader.NodeType != XmlNodeType.EndElement ) {
                                throw new XmlException( Res.Xml_InvalidNodeType, outerReader.NodeType.ToString());
                            }
                        }
                        return coreReaderImpl.InternalTypedValue;
                    }
                    return null;

                case XmlNodeType.EndElement:
                    return null;

                default:
                    if ( coreReaderImpl.V1Compat ) { //If v1 XmlValidatingReader return null
                        return null;
                    }
                    else {  
                        return await GetValueAsync().ConfigureAwait(false);
                    }
            }
        }

//
// Private implementation methods
//

        private async Task ParseDtdFromParserContextAsync()
        {
            Debug.Assert( parserContext != null );
            Debug.Assert( coreReaderImpl.DtdInfo == null );

            if ( parserContext.DocTypeName == null || parserContext.DocTypeName.Length == 0 ) {
                return;
            }

            IDtdParser dtdParser = DtdParser.Create();
            XmlTextReaderImpl.DtdParserProxy proxy = new XmlTextReaderImpl.DtdParserProxy(coreReaderImpl);
            IDtdInfo dtdInfo = await dtdParser.ParseFreeFloatingDtdAsync( parserContext.BaseURI, parserContext.DocTypeName, parserContext.PublicId,                                                               parserContext.SystemId, parserContext.InternalSubset, proxy ).ConfigureAwait(false);
            coreReaderImpl.SetDtdInfo( dtdInfo);

            ValidateDtd();
        }

        private async Task ResolveEntityInternallyAsync() {
            Debug.Assert( coreReader.NodeType == XmlNodeType.EntityReference );
            int initialDepth = coreReader.Depth;
            outerReader.ResolveEntity();
            while ( await outerReader.ReadAsync().ConfigureAwait(false) && coreReader.Depth > initialDepth );
        }

    }
}

