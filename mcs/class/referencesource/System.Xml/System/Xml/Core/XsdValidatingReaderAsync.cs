
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Security.Policy;
using System.Collections.Generic;
using System.Runtime.Versioning;

using System.Threading.Tasks;

namespace System.Xml {

    internal partial class XsdValidatingReader : XmlReader, IXmlSchemaInfo, IXmlLineInfo, IXmlNamespaceResolver {

        // Gets the text value of the current node.
        public override Task<string> GetValueAsync() {
            if ((int)validationState < 0) {
                return Task.FromResult(cachedNode.RawValue);
            }
            return coreReader.GetValueAsync();
        }

        public override Task< object > ReadContentAsObjectAsync() {
            if (!CanReadContentAs(this.NodeType)) {
                throw CreateReadContentAsException("ReadContentAsObject");
            }

            return InternalReadContentAsObjectAsync(true);

        }

        public override async Task< string > ReadContentAsStringAsync() {
            if (!CanReadContentAs(this.NodeType)) {
                throw CreateReadContentAsException("ReadContentAsString");
            }
            object typedValue = await InternalReadContentAsObjectAsync().ConfigureAwait(false);
            XmlSchemaType xmlType = NodeType == XmlNodeType.Attribute ? AttributeXmlType : ElementXmlType;
            try {
                if (xmlType != null) {
                    return xmlType.ValueConverter.ToString(typedValue);
                }
                else {
                    return typedValue as string;
                }
            }
            catch (InvalidCastException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
            catch (FormatException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
            catch (OverflowException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
        }

        public override async Task< object > ReadContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            if (!CanReadContentAs(this.NodeType)) {
                throw CreateReadContentAsException("ReadContentAs");
            }
            string originalStringValue;

            var tuple_0 = await InternalReadContentAsObjectTupleAsync(false).ConfigureAwait(false);
            originalStringValue = tuple_0.Item1;

            object typedValue = tuple_0.Item2;

            XmlSchemaType xmlType = NodeType == XmlNodeType.Attribute ? AttributeXmlType : ElementXmlType; //
            try {
                if (xmlType != null) {
                    // special-case convertions to DateTimeOffset; typedValue is by default a DateTime 
                    // which cannot preserve time zone, so we need to convert from the original string
                    if (returnType == typeof(DateTimeOffset) && xmlType.Datatype is Datatype_dateTimeBase) {
                        typedValue = originalStringValue;
                    }
                    return xmlType.ValueConverter.ChangeType(typedValue, returnType);
                }
                else {
                    return XmlUntypedConverter.Untyped.ChangeType(typedValue, returnType, namespaceResolver);
                }
            }
            catch (FormatException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
            catch (InvalidCastException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
            catch (OverflowException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
        }

        public override async Task< object > ReadElementContentAsObjectAsync() {
            if (this.NodeType != XmlNodeType.Element) {
                throw CreateReadElementContentAsException("ReadElementContentAsObject");
            }

            var tuple_1 = await InternalReadElementContentAsObjectAsync( true).ConfigureAwait(false);

            return tuple_1.Item2;

        }

        public override async Task< string > ReadElementContentAsStringAsync() {
            if (this.NodeType != XmlNodeType.Element) {
                throw CreateReadElementContentAsException("ReadElementContentAsString");
            }
            XmlSchemaType xmlType;

            var tuple_9 = await InternalReadElementContentAsObjectAsync().ConfigureAwait(false);
            xmlType = tuple_9.Item1;

            object typedValue = tuple_9.Item2;

            try {
                if (xmlType != null) {
                    return xmlType.ValueConverter.ToString(typedValue);
                }
                else {
                    return typedValue as string;
                }
            }
            catch (InvalidCastException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
            catch (FormatException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
            catch (OverflowException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, "String", e, this as IXmlLineInfo);
            }
        }

        public override async Task< object > ReadElementContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            if (this.NodeType != XmlNodeType.Element) {
                throw CreateReadElementContentAsException("ReadElementContentAs");
            }
            XmlSchemaType xmlType;
            string originalStringValue;

            var tuple_10 = await InternalReadElementContentAsObjectTupleAsync( false).ConfigureAwait(false);
            xmlType = tuple_10.Item1;
            originalStringValue = tuple_10.Item2;

            object typedValue = tuple_10.Item3;

            try {
                if (xmlType != null) {
                    // special-case convertions to DateTimeOffset; typedValue is by default a DateTime 
                    // which cannot preserve time zone, so we need to convert from the original string
                    if (returnType == typeof(DateTimeOffset) && xmlType.Datatype is Datatype_dateTimeBase) { 
                        typedValue = originalStringValue;
                    }
                    return xmlType.ValueConverter.ChangeType(typedValue, returnType, namespaceResolver);
                }
                else {
                    return XmlUntypedConverter.Untyped.ChangeType(typedValue, returnType, namespaceResolver);
                }
            }
            catch (FormatException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
            catch (InvalidCastException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
            catch (OverflowException e) {
                throw new XmlException(Res.Xml_ReadContentAsFormatException, returnType.ToString(), e, this as IXmlLineInfo);
            }
        }

        private Task<bool> ReadAsync_Read(Task<bool> task) {
            if (task.IsSuccess()) {
                if (task.Result) {
                    return ProcessReaderEventAsync().ReturnTaskBoolWhenFinish(true);
                }
                else {
                    validator.EndValidation();
                    if (coreReader.EOF) {
                        validationState = ValidatingReaderState.EOF;
                    }
                    return AsyncHelper.DoneTaskFalse;
                }
            }
            else {
                return _ReadAsync_Read(task);
            }
        }

        private async Task<bool> _ReadAsync_Read(Task<bool> task) {

            if (await task.ConfigureAwait(false)) {
                await ProcessReaderEventAsync().ConfigureAwait(false);
                return true;
            }
            else {
                validator.EndValidation();
                if (coreReader.EOF) {
                    validationState = ValidatingReaderState.EOF;
                }
                return false;
            }
        }

        private Task<bool> ReadAsync_ReadAhead(Task task) {
            if (task.IsSuccess()) {
                validationState = ValidatingReaderState.Read;
                return AsyncHelper.DoneTaskTrue; ;
            }
            else {
                return _ReadAsync_ReadAhead(task);
            }
        }

        private async Task<bool> _ReadAsync_ReadAhead(Task task) {
            await task.ConfigureAwait(false);
            validationState = ValidatingReaderState.Read;
            return true;
        }

        // Reads the next node from the stream/TextReader.
        public override Task< bool > ReadAsync() {
            switch (validationState) {
                case ValidatingReaderState.Read:
                    Task<bool> readTask = coreReader.ReadAsync();
                    return ReadAsync_Read(readTask);

                case ValidatingReaderState.ParseInlineSchema:
                    return ProcessInlineSchemaAsync().ReturnTaskBoolWhenFinish(true);

                case ValidatingReaderState.OnAttribute:
                case ValidatingReaderState.OnDefaultAttribute:
                case ValidatingReaderState.ClearAttributes:
                case ValidatingReaderState.OnReadAttributeValue:
                    ClearAttributesInfo();
                    if (inlineSchemaParser != null) {
                        validationState = ValidatingReaderState.ParseInlineSchema;
                        goto case ValidatingReaderState.ParseInlineSchema;
                    }
                    else {
                        validationState = ValidatingReaderState.Read;
                        goto case ValidatingReaderState.Read;
                    }

                case ValidatingReaderState.ReadAhead: //Will enter here on calling Skip() 
                    ClearAttributesInfo();
                    Task task = ProcessReaderEventAsync();
                    return ReadAsync_ReadAhead(task);

                case ValidatingReaderState.OnReadBinaryContent:
                    validationState = savedState;
                    return readBinaryHelper.FinishAsync().CallBoolTaskFuncWhenFinish(ReadAsync);

                case ValidatingReaderState.Init:
                    validationState = ValidatingReaderState.Read;
                    if (coreReader.ReadState == ReadState.Interactive) { //If the underlying reader is already positioned on a ndoe, process it
                        return ProcessReaderEventAsync().ReturnTaskBoolWhenFinish(true);
                    }
                    else {
                        goto case ValidatingReaderState.Read;
                    }

                case ValidatingReaderState.ReaderClosed:
                case ValidatingReaderState.EOF:
                    return AsyncHelper.DoneTaskFalse;

                default:
                    return AsyncHelper.DoneTaskFalse;
            }
        }

        // Skips to the end tag of the current element.
        public override async Task SkipAsync() {
            int startDepth = Depth;
            switch (NodeType) {
                case XmlNodeType.Element:
                    if (coreReader.IsEmptyElement) {
                        break;
                    }
                    bool callSkipToEndElem = true;
                    //If union and unionValue has been parsed till EndElement, then validator.ValidateEndElement has been called
                    //Hence should not call SkipToEndElement as the current context has already been popped in the validator
                    if ((xmlSchemaInfo.IsUnionType || xmlSchemaInfo.IsDefault) && coreReader is XsdCachingReader) {
                        callSkipToEndElem = false;
                    }
                    await coreReader.SkipAsync().ConfigureAwait(false);
                    validationState = ValidatingReaderState.ReadAhead;
                    if (callSkipToEndElem) {
                        validator.SkipToEndElement(xmlSchemaInfo);
                    }
                    break;

                case XmlNodeType.Attribute:
                    MoveToElement();
                    goto case XmlNodeType.Element;
            }
            //For all other NodeTypes Skip() same as Read()
            await ReadAsync().ConfigureAwait(false);
            return;
        }

        public override async Task< int > ReadContentAsBase64Async(byte[] buffer, int index, int count) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if (validationState != ValidatingReaderState.OnReadBinaryContent) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
                savedState = validationState;
            }

            // restore original state in order to have a normal Read() behavior when called from readBinaryHelper
            validationState = savedState;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBase64Async(buffer, index, count).ConfigureAwait(false);

            // set OnReadBinaryContent state again and return
            savedState = validationState;
            validationState = ValidatingReaderState.OnReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadContentAsBinHexAsync(byte[] buffer, int index, int count) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if (validationState != ValidatingReaderState.OnReadBinaryContent) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
                savedState = validationState;
            }

            // restore original state in order to have a normal Read() behavior when called from readBinaryHelper
            validationState = savedState;

            // call to the helper
            int readCount = await readBinaryHelper.ReadContentAsBinHexAsync(buffer, index, count).ConfigureAwait(false);

            // set OnReadBinaryContent state again and return
            savedState = validationState;
            validationState = ValidatingReaderState.OnReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadElementContentAsBase64Async(byte[] buffer, int index, int count) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if (validationState != ValidatingReaderState.OnReadBinaryContent) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
                savedState = validationState;
            }

            // restore original state in order to have a normal Read() behavior when called from readBinaryHelper
            validationState = savedState;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBase64Async(buffer, index, count).ConfigureAwait(false);

            // set OnReadBinaryContent state again and return
            savedState = validationState;
            validationState = ValidatingReaderState.OnReadBinaryContent;
            return readCount;
        }

        public override async Task< int > ReadElementContentAsBinHexAsync(byte[] buffer, int index, int count) {
            if (ReadState != ReadState.Interactive) {
                return 0;
            }

            // init ReadContentAsBinaryHelper when called first time
            if (validationState != ValidatingReaderState.OnReadBinaryContent) {
                readBinaryHelper = ReadContentAsBinaryHelper.CreateOrReset(readBinaryHelper, this);
                savedState = validationState;
            }

            // restore original state in order to have a normal Read() behavior when called from readBinaryHelper
            validationState = savedState;

            // call to the helper
            int readCount = await readBinaryHelper.ReadElementContentAsBinHexAsync(buffer, index, count).ConfigureAwait(false);

            // set OnReadBinaryContent state again and return
            savedState = validationState;
            validationState = ValidatingReaderState.OnReadBinaryContent;
            return readCount;
        }

        private Task ProcessReaderEventAsync() {
            if (replayCache) { //if in replay mode, do nothing since nodes have been validated already
                //If NodeType == XmlNodeType.EndElement && if manageNamespaces, may need to pop namespace scope, since scope is not popped in ReadAheadForMemberType

                return AsyncHelper.DoneTask;

            }
            switch (coreReader.NodeType) {
                case XmlNodeType.Element:

                    return ProcessElementEventAsync();

                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    validator.ValidateWhitespace(GetStringValue);
                    break;

                case XmlNodeType.Text:          // text inside a node
                case XmlNodeType.CDATA:         // <![CDATA[...]]>
                    validator.ValidateText(GetStringValue);
                    break;

                case XmlNodeType.EndElement:

                    return ProcessEndElementEventAsync();

                case XmlNodeType.EntityReference:
                    throw new InvalidOperationException();

                case XmlNodeType.DocumentType:
#if TEMP_HACK_FOR_SCHEMA_INFO
                    validator.SetDtdSchemaInfo((SchemaInfo)coreReader.DtdInfo);
#else
                    validator.SetDtdSchemaInfo(coreReader.DtdInfo);
#endif
                    break;

                default:
                    break;
            }

            return AsyncHelper.DoneTask;

        }

        // SxS: This function calls ValidateElement on XmlSchemaValidator which is annotated with ResourceExposure attribute.
        // Since the resource names (namespace location) are not provided directly by the user (they are read from the source
        // document) and the function does not expose any resources it is fine to suppress the SxS warning. 
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        private async Task ProcessElementEventAsync() {
            if (this.processInlineSchema && IsXSDRoot(coreReader.LocalName, coreReader.NamespaceURI) && coreReader.Depth > 0) {
                xmlSchemaInfo.Clear();
                attributeCount = coreReaderAttributeCount = coreReader.AttributeCount;
                if (!coreReader.IsEmptyElement) { //If its not empty schema, then parse else ignore
                    inlineSchemaParser = new Parser(SchemaType.XSD, coreReaderNameTable, validator.SchemaSet.GetSchemaNames(coreReaderNameTable), validationEvent);
                    await inlineSchemaParser.StartParsingAsync(coreReader, null).ConfigureAwait(false);
                    inlineSchemaParser.ParseReaderNode();
                    validationState = ValidatingReaderState.ParseInlineSchema;
                }
                else {
                    validationState = ValidatingReaderState.ClearAttributes;
                }
            }
            else { //Validate element

                //Clear previous data
                atomicValue = null;
                originalAtomicValueString = null;
                xmlSchemaInfo.Clear();

                if (manageNamespaces) {
                    nsManager.PushScope();
                }
                //Find Xsi attributes that need to be processed before validating the element
                string xsiSchemaLocation = null;
                string xsiNoNamespaceSL = null;
                string xsiNil = null;
                string xsiType = null;
                if (coreReader.MoveToFirstAttribute()) {
                    do {
                        string objectNs = coreReader.NamespaceURI;
                        string objectName = coreReader.LocalName;
                        if (Ref.Equal(objectNs, NsXsi)) {
                            if (Ref.Equal(objectName, XsiSchemaLocation)) {
                                xsiSchemaLocation = coreReader.Value;
                            }
                            else if (Ref.Equal(objectName, XsiNoNamespaceSchemaLocation)) {
                                xsiNoNamespaceSL = coreReader.Value;
                            }
                            else if (Ref.Equal(objectName, XsiType)) {
                                xsiType = coreReader.Value;
                            }
                            else if (Ref.Equal(objectName, XsiNil)) {
                                xsiNil = coreReader.Value;
                            }
                        }
                        if (manageNamespaces && Ref.Equal(coreReader.NamespaceURI, NsXmlNs)) {
                            nsManager.AddNamespace(coreReader.Prefix.Length == 0 ? string.Empty : coreReader.LocalName, coreReader.Value);
                        }

                    } while (coreReader.MoveToNextAttribute());
                    coreReader.MoveToElement();
                }
                validator.ValidateElement(coreReader.LocalName, coreReader.NamespaceURI, xmlSchemaInfo, xsiType, xsiNil, xsiSchemaLocation, xsiNoNamespaceSL);
                ValidateAttributes();
                validator.ValidateEndOfAttributes(xmlSchemaInfo);
                if (coreReader.IsEmptyElement) {
                    await ProcessEndElementEventAsync().ConfigureAwait(false);
                }
                validationState = ValidatingReaderState.ClearAttributes;
            }
        }

        private async Task ProcessEndElementEventAsync() {
            atomicValue = validator.ValidateEndElement(xmlSchemaInfo);
            originalAtomicValueString = GetOriginalAtomicValueStringOfElement();
            if (xmlSchemaInfo.IsDefault) { //The atomicValue returned is a default value
                Debug.Assert(atomicValue != null);
                int depth = coreReader.Depth;
                coreReader = GetCachingReader();
                cachingReader.RecordTextNode( xmlSchemaInfo.XmlType.ValueConverter.ToString( atomicValue ), originalAtomicValueString, depth + 1, 0, 0 );
                cachingReader.RecordEndElementNode(); 
                await cachingReader.SetToReplayModeAsync().ConfigureAwait(false);
                replayCache = true;
            }
            else if (manageNamespaces) {
                nsManager.PopScope();
            }
        }

        private async Task ProcessInlineSchemaAsync() {
            Debug.Assert(inlineSchemaParser != null);
            if (await coreReader.ReadAsync().ConfigureAwait(false)) {
                if (coreReader.NodeType == XmlNodeType.Element) {
                    attributeCount = coreReaderAttributeCount = coreReader.AttributeCount;
                }
                else { //Clear attributes info if nodeType is not element
                    ClearAttributesInfo();
                }
                if (!inlineSchemaParser.ParseReaderNode()) {
                    inlineSchemaParser.FinishParsing();
                    XmlSchema schema = inlineSchemaParser.XmlSchema;
                    validator.AddSchema(schema);
                    inlineSchemaParser = null;
                    validationState = ValidatingReaderState.Read;
                }
            }
        }

        private Task< object > InternalReadContentAsObjectAsync() {
            return InternalReadContentAsObjectAsync(false);
        }

        private async Task< object > InternalReadContentAsObjectAsync(bool unwrapTypedValue) {

            var tuple_11 = await InternalReadContentAsObjectTupleAsync(unwrapTypedValue).ConfigureAwait(false);

            return tuple_11.Item2;

        }

        private async Task< Tuple<string, object> > InternalReadContentAsObjectTupleAsync(bool unwrapTypedValue) {
            Tuple<string, object> tuple;
            string originalStringValue;

            XmlNodeType nodeType = this.NodeType;
            if (nodeType == XmlNodeType.Attribute) {
                originalStringValue = this.Value;
                if ( attributePSVI != null && attributePSVI.typedAttributeValue != null ) {
                    if ( validationState == ValidatingReaderState.OnDefaultAttribute) {
                        XmlSchemaAttribute schemaAttr = attributePSVI.attributeSchemaInfo.SchemaAttribute;
                        originalStringValue = ( schemaAttr.DefaultValue != null ) ? schemaAttr.DefaultValue : schemaAttr.FixedValue;
                    }

                    tuple = new Tuple<string, object>(originalStringValue, ReturnBoxedValue( attributePSVI.typedAttributeValue, AttributeSchemaInfo.XmlType, unwrapTypedValue ));
                    return tuple;

                }
                else { //return string value

                    tuple = new Tuple<string, object>(originalStringValue, this.Value);
                    return tuple;

                }
            }
            else if (nodeType == XmlNodeType.EndElement) {
                if (atomicValue != null) {
                    originalStringValue = originalAtomicValueString;

                    tuple = new Tuple<string, object>(originalStringValue, atomicValue);
                    return tuple;

                }
                else {
                    originalStringValue = string.Empty;

                    tuple = new Tuple<string, object>(originalStringValue, string.Empty);
                    return tuple;

                }
            }
            else { //Positioned on text, CDATA, PI, Comment etc
                if (validator.CurrentContentType == XmlSchemaContentType.TextOnly) {  //if current element is of simple type
                    object value = ReturnBoxedValue(await ReadTillEndElementAsync().ConfigureAwait(false), xmlSchemaInfo.XmlType, unwrapTypedValue);
                    originalStringValue = originalAtomicValueString;

                    tuple = new Tuple<string, object>(originalStringValue, value);
                    return tuple;

                }
                else {
                    XsdCachingReader cachingReader = this.coreReader as XsdCachingReader;
                    if ( cachingReader != null ) {
                        originalStringValue = cachingReader.ReadOriginalContentAsString();
                    }
                    else {
                        originalStringValue = await InternalReadContentAsStringAsync().ConfigureAwait(false);
                    }

                    tuple = new Tuple<string, object>(originalStringValue, originalStringValue);
                    return tuple;

                }
            }
        }

        private Task< Tuple<XmlSchemaType, object> > InternalReadElementContentAsObjectAsync() {

            return InternalReadElementContentAsObjectAsync( false);

        }

        private async Task< Tuple<XmlSchemaType, object> > InternalReadElementContentAsObjectAsync(bool unwrapTypedValue) {

            var tuple_13 = await InternalReadElementContentAsObjectTupleAsync( unwrapTypedValue).ConfigureAwait(false);

            return new Tuple<XmlSchemaType, object>(tuple_13.Item1, tuple_13.Item3);

        }

        private async Task< Tuple<XmlSchemaType, string, object> > InternalReadElementContentAsObjectTupleAsync(bool unwrapTypedValue) {
            Tuple<XmlSchemaType, string, object> tuple;
            XmlSchemaType xmlType;
            string originalString;

            Debug.Assert(this.NodeType == XmlNodeType.Element);
            object typedValue = null;
            xmlType = null;
            //If its an empty element, can have default/fixed value
            if (this.IsEmptyElement) {
                if (xmlSchemaInfo.ContentType == XmlSchemaContentType.TextOnly) {
                    typedValue = ReturnBoxedValue(atomicValue, xmlSchemaInfo.XmlType, unwrapTypedValue);
                }
                else {
                    typedValue = atomicValue;
                }
                originalString = originalAtomicValueString;
                xmlType = ElementXmlType; //Set this for default values 
                await this.ReadAsync().ConfigureAwait(false);

                tuple = new Tuple<XmlSchemaType, string, object>(xmlType, originalString, typedValue);
                return tuple;

            }
            // move to content and read typed value
            await this.ReadAsync().ConfigureAwait(false);

            if (this.NodeType == XmlNodeType.EndElement) { //If IsDefault is true, the next node will be EndElement
                if (xmlSchemaInfo.IsDefault) {
                    if (xmlSchemaInfo.ContentType == XmlSchemaContentType.TextOnly) {
                        typedValue = ReturnBoxedValue(atomicValue, xmlSchemaInfo.XmlType, unwrapTypedValue);
                    }
                    else { //anyType has default value
                        typedValue = atomicValue;
                    }
                    originalString = originalAtomicValueString;
                }
                else { //Empty content
                    typedValue = string.Empty;
                    originalString = string.Empty;  
                }
            }
            else if (this.NodeType == XmlNodeType.Element) { //the first child is again element node
                throw new XmlException(Res.Xml_MixedReadElementContentAs, string.Empty, this as IXmlLineInfo);
            }
            else {

                var tuple_14 = await InternalReadContentAsObjectTupleAsync(unwrapTypedValue).ConfigureAwait(false);
                originalString = tuple_14.Item1;

                typedValue = tuple_14.Item2;

                // ReadElementContentAsXXX cannot be called on mixed content, if positioned on node other than EndElement, Error
                if (this.NodeType != XmlNodeType.EndElement) {
                    throw new XmlException(Res.Xml_MixedReadElementContentAs, string.Empty, this as IXmlLineInfo);
                }
            }
            xmlType = ElementXmlType; //Set this as we are moving ahead to the next node

            // move to next node
            await this.ReadAsync().ConfigureAwait(false);

            tuple = new Tuple<XmlSchemaType, string, object>(xmlType, originalString, typedValue);
            return tuple;

        }

        private async Task< object > ReadTillEndElementAsync() {
            if (atomicValue == null) {
                while (await coreReader.ReadAsync().ConfigureAwait(false)) {
                    if (replayCache) { //If replaying nodes in the cache, they have already been validated
                        continue;
                    }
                    switch (coreReader.NodeType) {
                        case XmlNodeType.Element:
                            await ProcessReaderEventAsync().ConfigureAwait(false);
                            goto breakWhile;

                        case XmlNodeType.Text:
                        case XmlNodeType.CDATA:
                            validator.ValidateText(GetStringValue);
                            break;

                        case XmlNodeType.Whitespace:
                        case XmlNodeType.SignificantWhitespace:
                            validator.ValidateWhitespace(GetStringValue);
                            break;

                        case XmlNodeType.Comment:
                        case XmlNodeType.ProcessingInstruction:
                            break;

                        case XmlNodeType.EndElement:
                            atomicValue = validator.ValidateEndElement(xmlSchemaInfo);
                            originalAtomicValueString = GetOriginalAtomicValueStringOfElement();
                            if (manageNamespaces) {
                                nsManager.PopScope();
                            }
                            goto breakWhile;
                    }
                    continue;
                breakWhile:
                    break;
                }
            }
            else { //atomicValue != null, meaning already read ahead - Switch reader
                if (atomicValue == this) { //switch back invalid marker; dont need it since coreReader moved to endElement
                    atomicValue = null;
                }
                SwitchReader();
            }
            return atomicValue;
        }

    }
}

