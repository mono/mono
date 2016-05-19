//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization.Json
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.ServiceModel;
    using System.Text;
    using System.Xml;

    class XmlJsonWriter : XmlDictionaryWriter, IXmlJsonWriterInitializer
    {
        const char BACK_SLASH = '\\';
        const char FORWARD_SLASH = '/';

        const char HIGH_SURROGATE_START = (char)0xd800;
        const char LOW_SURROGATE_END = (char)0xdfff;
        const char MAX_CHAR = (char)0xfffe;
        const char WHITESPACE = ' ';
        const char CARRIAGE_RETURN = '\r';
        const char NEWLINE = '\n';
        const string xmlNamespace = "http://www.w3.org/XML/1998/namespace";
        const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        [Fx.Tag.SecurityNote(Critical = "Static fields are marked SecurityCritical or readonly to prevent"
            + " data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        static BinHexEncoding binHexEncoding;

        string attributeText;
        JsonDataType dataType;
        int depth;
        bool endElementBuffer;
        bool isWritingDataTypeAttribute;
        bool isWritingServerTypeAttribute;
        bool isWritingXmlnsAttribute;
        bool isWritingXmlnsAttributeDefaultNs;
        NameState nameState;
        JsonNodeType nodeType;
        JsonNodeWriter nodeWriter;
        JsonNodeType[] scopes;
        string serverTypeValue;
        // Do not use this field's value anywhere other than the WriteState property.
        // It's OK to set this field's value anywhere and then change the WriteState property appropriately.
        // If it's necessary to check the WriteState outside WriteState, use the WriteState property.
        WriteState writeState;
        bool wroteServerTypeAttribute;
        bool indent;
        string indentChars;
        int indentLevel;

        public XmlJsonWriter() : this(false, null) { }

        public XmlJsonWriter(bool indent, string indentChars)
        {
            this.indent = indent;
            if (indent)
            {
                if (indentChars == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("indentChars");
                }
                this.indentChars = indentChars;
            }
            InitializeWriter();
        }

        enum JsonDataType
        {
            None,
            Null,
            Boolean,
            Number,
            String,
            Object,
            Array
        };

        [Flags]
        enum NameState
        {
            None = 0,
            IsWritingNameWithMapping = 1,
            IsWritingNameAttribute = 2,
            WrittenNameWithMapping = 4,
        }

        public override XmlWriterSettings Settings
        {
            // The XmlWriterSettings object used to create this writer instance.
            // If this writer was not created using the Create method, this property
            // returns a null reference. 
            get { return null; }
        }

        public override WriteState WriteState
        {
            get
            {
                if (writeState == WriteState.Closed)
                {
                    return WriteState.Closed;
                }
                if (HasOpenAttribute)
                {
                    return WriteState.Attribute;
                }
                switch (nodeType)
                {
                    case JsonNodeType.None:
                        return WriteState.Start;
                    case JsonNodeType.Element:
                        return WriteState.Element;
                    case JsonNodeType.QuotedText:
                    case JsonNodeType.StandaloneText:
                    case JsonNodeType.EndElement:
                        return WriteState.Content;
                    default:
                        return WriteState.Error;
                }
            }
        }

        public override string XmlLang
        {
            get { return null; }
        }

        public override XmlSpace XmlSpace
        {
            get { return XmlSpace.None; }
        }

        static BinHexEncoding BinHexEncoding
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical binHexEncoding field.",
                Safe = "Get-only properties only need to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (binHexEncoding == null)
                {
                    binHexEncoding = new BinHexEncoding();
                }
                return binHexEncoding;
            }
        }

        bool HasOpenAttribute
        {
            get
            {
                return (isWritingDataTypeAttribute || isWritingServerTypeAttribute || IsWritingNameAttribute || isWritingXmlnsAttribute);
            }
        }

        bool IsClosed
        {
            get { return (WriteState == WriteState.Closed); }
        }

        bool IsWritingCollection
        {
            get { return (depth > 0) && (scopes[depth] == JsonNodeType.Collection); }
        }

        bool IsWritingNameAttribute
        {
            get { return (nameState & NameState.IsWritingNameAttribute) == NameState.IsWritingNameAttribute; }
        }

        bool IsWritingNameWithMapping
        {
            get { return (nameState & NameState.IsWritingNameWithMapping) == NameState.IsWritingNameWithMapping; }
        }

        bool WrittenNameWithMapping
        {
            get { return (nameState & NameState.WrittenNameWithMapping) == NameState.WrittenNameWithMapping; }
        }

        public override void Close()
        {
            if (!IsClosed)
            {
                try
                {
                    WriteEndDocument();
                }
                finally
                {
                    try
                    {
                        nodeWriter.Flush();
                        nodeWriter.Close();
                    }
                    finally
                    {
                        writeState = WriteState.Closed;
                        if (depth != 0)
                        {
                            depth = 0;
                        }
                    }
                }
            }
        }

        public override void Flush()
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            nodeWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }
            if (ns == Globals.XmlnsNamespace)
            {
                return Globals.XmlnsPrefix;
            }
            if (ns == xmlNamespace)
            {
                return JsonGlobals.xmlPrefix;
            }
            if (ns == string.Empty)
            {
                return string.Empty;
            }
            return null;
        }

        public void SetOutput(Stream stream, Encoding encoding, bool ownsStream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            if (encoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("encoding");
            }
            if (encoding.WebName != Encoding.UTF8.WebName)
            {
                stream = new JsonEncodingStreamWrapper(stream, encoding, false);
            }
            else
            {
                encoding = null;
            }
            if (nodeWriter == null)
            {
                nodeWriter = new JsonNodeWriter();
            }

            nodeWriter.SetOutput(stream, ownsStream, encoding);
            InitializeWriter();
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Int16[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Int32[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Int64[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonWriteArrayNotSupported)));
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (count > buffer.Length - index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count",
                    SR.GetString(SR.JsonSizeExceedsRemainingBufferSpace,
                    buffer.Length - index)));
            }

            StartText();
            nodeWriter.WriteBase64Text(buffer, 0, buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (count > buffer.Length - index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count",
                    SR.GetString(SR.JsonSizeExceedsRemainingBufferSpace,
                    buffer.Length - index)));
            }

            StartText();
            WriteEscapedJsonString(BinHexEncoding.GetString(buffer, index, count));
        }

        public override void WriteCData(string text)
        {
            WriteString(text);
        }

        public override void WriteCharEntity(char ch)
        {
            WriteString(ch.ToString());
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (count > buffer.Length - index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count",
                    SR.GetString(SR.JsonSizeExceedsRemainingBufferSpace,
                    buffer.Length - index)));
            }

            WriteString(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteComment")));
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "2#sysid", Justification = "This method is derived from the base")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "1#pubid", Justification = "This method is derived from the base")]
        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteDocType")));
        }

        public override void WriteEndAttribute()
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (!HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonNoMatchingStartAttribute)));
            }

            Fx.Assert(!(isWritingDataTypeAttribute && isWritingServerTypeAttribute),
                "Can not write type attribute and __type attribute at the same time.");

            if (isWritingDataTypeAttribute)
            {
                switch (attributeText)
                {
                    case JsonGlobals.numberString:
                        {
                            ThrowIfServerTypeWritten(JsonGlobals.numberString);
                            dataType = JsonDataType.Number;
                            break;
                        }
                    case JsonGlobals.stringString:
                        {
                            ThrowIfServerTypeWritten(JsonGlobals.stringString);
                            dataType = JsonDataType.String;
                            break;
                        }
                    case JsonGlobals.arrayString:
                        {
                            ThrowIfServerTypeWritten(JsonGlobals.arrayString);
                            dataType = JsonDataType.Array;
                            break;
                        }
                    case JsonGlobals.objectString:
                        {
                            dataType = JsonDataType.Object;
                            break;
                        }
                    case JsonGlobals.nullString:
                        {
                            ThrowIfServerTypeWritten(JsonGlobals.nullString);
                            dataType = JsonDataType.Null;
                            break;
                        }
                    case JsonGlobals.booleanString:
                        {
                            ThrowIfServerTypeWritten(JsonGlobals.booleanString);
                            dataType = JsonDataType.Boolean;
                            break;
                        }
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new XmlException(SR.GetString(SR.JsonUnexpectedAttributeValue, attributeText)));
                }

                attributeText = null;
                isWritingDataTypeAttribute = false;

                if (!IsWritingNameWithMapping || WrittenNameWithMapping)
                {
                    WriteDataTypeServerType();
                }
            }
            else if (isWritingServerTypeAttribute)
            {
                serverTypeValue = attributeText;
                attributeText = null;
                isWritingServerTypeAttribute = false;

                // we are writing __type after type="object" (enforced by WSE)
                if ((!IsWritingNameWithMapping || WrittenNameWithMapping) && dataType == JsonDataType.Object)
                {
                    WriteServerTypeAttribute();
                }
            }
            else if (IsWritingNameAttribute)
            {
                WriteJsonElementName(attributeText);
                attributeText = null;
                nameState = NameState.IsWritingNameWithMapping | NameState.WrittenNameWithMapping;
                WriteDataTypeServerType();
            }
            else if (isWritingXmlnsAttribute)
            {
                if (!string.IsNullOrEmpty(attributeText) && isWritingXmlnsAttributeDefaultNs)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", SR.GetString(SR.JsonNamespaceMustBeEmpty, attributeText));
                }

                attributeText = null;
                isWritingXmlnsAttribute = false;
                isWritingXmlnsAttributeDefaultNs = false;
            }
        }

        public override void WriteEndDocument()
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (nodeType != JsonNodeType.None)
            {
                while (depth > 0)
                {
                    WriteEndElement();
                }
            }
        }

        public override void WriteEndElement()
        {
            if (IsClosed)
            {
                ThrowClosed();
            }

            if (depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonEndElementNoOpenNodes)));
            }
            if (HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonOpenAttributeMustBeClosedFirst, "WriteEndElement")));
            }

            endElementBuffer = false;

            JsonNodeType token = ExitScope();
            if (token == JsonNodeType.Collection)
            {
                indentLevel--;
                if (indent)
                {
                    if (nodeType == JsonNodeType.Element)
                    {
                        nodeWriter.WriteText(WHITESPACE);
                    }
                    else
                    {
                        WriteNewLine();
                        WriteIndent();
                    }
                }
                nodeWriter.WriteText(JsonGlobals.EndCollectionChar);
                token = ExitScope();
            }
            else if (nodeType == JsonNodeType.QuotedText)
            {
                // For writing "
                WriteJsonQuote();
            }
            else if (nodeType == JsonNodeType.Element)
            {
                if ((dataType == JsonDataType.None) && (serverTypeValue != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonMustSpecifyDataType,
                        JsonGlobals.typeString, JsonGlobals.objectString, JsonGlobals.serverTypeString)));
                }

                if (IsWritingNameWithMapping && !WrittenNameWithMapping)
                {
                    // Ending </item> without writing item attribute
                    // Not providing a better error message because localization deadline has passed.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonMustSpecifyDataType,
                        JsonGlobals.itemString, string.Empty, JsonGlobals.itemString)));
                }

                // the element is empty, it does not have any content, 
                if ((dataType == JsonDataType.None) ||
                    (dataType == JsonDataType.String))
                {
                    nodeWriter.WriteText(JsonGlobals.QuoteChar);
                    nodeWriter.WriteText(JsonGlobals.QuoteChar);
                }
            }
            else
            {
                // Assert on only StandaloneText and EndElement because preceding if
                //    conditions take care of checking for QuotedText and Element.
                Fx.Assert((nodeType == JsonNodeType.StandaloneText) || (nodeType == JsonNodeType.EndElement),
                    "nodeType has invalid value " + nodeType + ". Expected it to be QuotedText, Element, StandaloneText, or EndElement.");
            }
            if (depth != 0)
            {
                if (token == JsonNodeType.Element)
                {
                    endElementBuffer = true;
                }
                else if (token == JsonNodeType.Object)
                {
                    indentLevel--;
                    if (indent)
                    {
                        if (nodeType == JsonNodeType.Element)
                        {
                            nodeWriter.WriteText(WHITESPACE);
                        }
                        else
                        {
                            WriteNewLine();
                            WriteIndent();
                        }
                    }
                    nodeWriter.WriteText(JsonGlobals.EndObjectChar);
                    if ((depth > 0) && scopes[depth] == JsonNodeType.Element)
                    {
                        ExitScope();
                        endElementBuffer = true;
                    }
                }
            }

            dataType = JsonDataType.None;
            nodeType = JsonNodeType.EndElement;
            nameState = NameState.None;
            wroteServerTypeAttribute = false;
        }

        public override void WriteEntityRef(string name)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteEntityRef")));
        }

        public override void WriteFullEndElement()
        {
            WriteEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }

            if (!name.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.JsonXmlProcessingInstructionNotSupported), "name"));
            }

            if (WriteState != WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.JsonXmlInvalidDeclaration)));
            }
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName",
                    SR.GetString(SR.JsonInvalidLocalNameEmpty));
            }
            if (ns == null)
            {
                ns = string.Empty;
            }

            base.WriteQualifiedName(localName, ns);
        }

        public override void WriteRaw(string data)
        {
            WriteString(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("buffer");
            }

            // Not checking upper bound because it will be caught by "count".  This is what XmlTextWriter does.
            if (index < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("index", SR.GetString(SR.ValueMustBeNonNegative)));
            }

            if (count < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            }
            if (count > buffer.Length - index)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new ArgumentOutOfRangeException("count",
                    SR.GetString(SR.JsonSizeExceedsRemainingBufferSpace,
                    buffer.Length - index)));
            }

            WriteString(new string(buffer, index, count));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // [....], ToLowerInvariant is just used in Json error message
        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                if (IsWritingNameWithMapping && prefix == JsonGlobals.xmlnsPrefix)
                {
                    if (ns != null && ns != xmlnsNamespace)
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.XmlPrefixBoundToNamespace, "xmlns", xmlnsNamespace, ns), "ns"));
                    }
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("prefix", SR.GetString(SR.JsonPrefixMustBeNullOrEmpty, prefix));
                }
            }
            else
            {
                if (IsWritingNameWithMapping && ns == xmlnsNamespace && localName != JsonGlobals.xmlnsPrefix)
                {
                    prefix = JsonGlobals.xmlnsPrefix;
                }
            }
            if (!string.IsNullOrEmpty(ns))
            {
                if (IsWritingNameWithMapping && ns == xmlnsNamespace)
                {
                    prefix = JsonGlobals.xmlnsPrefix;
                }
                else if (string.IsNullOrEmpty(prefix) && localName == JsonGlobals.xmlnsPrefix && ns == xmlnsNamespace)
                {
                    prefix = JsonGlobals.xmlnsPrefix;
                    isWritingXmlnsAttributeDefaultNs = true;
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", SR.GetString(SR.JsonNamespaceMustBeEmpty, ns));
                }
            }
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", SR.GetString(SR.JsonInvalidLocalNameEmpty));
            }
            if ((nodeType != JsonNodeType.Element) && !wroteServerTypeAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(SR.GetString(SR.JsonAttributeMustHaveElement)));
            }
            if (HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonOpenAttributeMustBeClosedFirst, "WriteStartAttribute")));
            }
            if (prefix == JsonGlobals.xmlnsPrefix)
            {
                isWritingXmlnsAttribute = true;
            }
            else if (localName == JsonGlobals.typeString)
            {
                if (dataType != JsonDataType.None)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonAttributeAlreadyWritten, JsonGlobals.typeString)));
                }

                isWritingDataTypeAttribute = true;
            }
            else if (localName == JsonGlobals.serverTypeString)
            {
                if (serverTypeValue != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonAttributeAlreadyWritten, JsonGlobals.serverTypeString)));
                }

                if ((dataType != JsonDataType.None) && (dataType != JsonDataType.Object))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonServerTypeSpecifiedForInvalidDataType,
                        JsonGlobals.serverTypeString, JsonGlobals.typeString, dataType.ToString().ToLowerInvariant(), JsonGlobals.objectString)));
                }

                isWritingServerTypeAttribute = true;
            }
            else if (localName == JsonGlobals.itemString)
            {
                if (WrittenNameWithMapping)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonAttributeAlreadyWritten, JsonGlobals.itemString)));
                }

                if (!IsWritingNameWithMapping)
                {
                    // Don't write attribute with local name "item" if <item> element is not open.
                    // Not providing a better error message because localization deadline has passed.
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonEndElementNoOpenNodes)));
                }

                nameState |= NameState.IsWritingNameAttribute;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName", SR.GetString(SR.JsonUnexpectedAttributeLocalName, localName));
            }
        }

        public override void WriteStartDocument(bool standalone)
        {
            // In XML, writes the XML declaration with the version "1.0" and the standalone attribute. 
            WriteStartDocument();
        }

        public override void WriteStartDocument()
        {
            // In XML, writes the XML declaration with the version "1.0". 
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (WriteState != WriteState.Start)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                    SR.GetString(SR.JsonInvalidWriteState, "WriteStartDocument", WriteState.ToString())));
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            if (localName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            }
            if (localName.Length == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("localName",
                    SR.GetString(SR.JsonInvalidLocalNameEmpty));
            }
            if (!string.IsNullOrEmpty(prefix))
            {
                if (string.IsNullOrEmpty(ns) || !TrySetWritingNameWithMapping(localName, ns))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("prefix", SR.GetString(SR.JsonPrefixMustBeNullOrEmpty, prefix));
                }
            }
            if (!string.IsNullOrEmpty(ns))
            {
                if (!TrySetWritingNameWithMapping(localName, ns))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ns", SR.GetString(SR.JsonNamespaceMustBeEmpty, ns));
                }
            }
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonOpenAttributeMustBeClosedFirst, "WriteStartElement")));
            }
            if ((nodeType != JsonNodeType.None) && depth == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonMultipleRootElementsNotAllowedOnWriter)));
            }

            switch (nodeType)
            {
                case JsonNodeType.None:
                    {
                        if (!localName.Equals(JsonGlobals.rootString))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new XmlException(SR.GetString(SR.JsonInvalidRootElementName, localName, JsonGlobals.rootString)));
                        }
                        EnterScope(JsonNodeType.Element);
                        break;
                    }
                case JsonNodeType.Element:
                    {
                        if ((dataType != JsonDataType.Array) && (dataType != JsonDataType.Object))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new XmlException(SR.GetString(SR.JsonNodeTypeArrayOrObjectNotSpecified)));
                        }
                        if (indent)
                        {
                            WriteNewLine();
                            WriteIndent();
                        }
                        if (!IsWritingCollection)
                        {
                            if (nameState != NameState.IsWritingNameWithMapping)
                            {
                                WriteJsonElementName(localName);
                            }
                        }
                        else if (!localName.Equals(JsonGlobals.itemString))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new XmlException(SR.GetString(SR.JsonInvalidItemNameForArrayElement, localName, JsonGlobals.itemString)));
                        }
                        EnterScope(JsonNodeType.Element);
                        break;
                    }
                case JsonNodeType.EndElement:
                    {
                        if (endElementBuffer)
                        {
                            nodeWriter.WriteText(JsonGlobals.MemberSeparatorChar);
                        }
                        if (indent)
                        {
                            WriteNewLine();
                            WriteIndent();
                        }
                        if (!IsWritingCollection)
                        {
                            if (nameState != NameState.IsWritingNameWithMapping)
                            {
                                WriteJsonElementName(localName);
                            }
                        }
                        else if (!localName.Equals(JsonGlobals.itemString))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new XmlException(SR.GetString(SR.JsonInvalidItemNameForArrayElement, localName, JsonGlobals.itemString)));
                        }
                        EnterScope(JsonNodeType.Element);
                        break;
                    }
                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new XmlException(SR.GetString(SR.JsonInvalidStartElementCall)));
            }

            isWritingDataTypeAttribute = false;
            isWritingServerTypeAttribute = false;
            isWritingXmlnsAttribute = false;
            wroteServerTypeAttribute = false;
            serverTypeValue = null;
            dataType = JsonDataType.None;
            nodeType = JsonNodeType.Element;
        }

        public override void WriteString(string text)
        {
            if (HasOpenAttribute && (text != null))
            {
                attributeText += text;
            }
            else
            {
                if (text == null)
                {
                    text = string.Empty;
                }

                // do work only when not indenting whitespaces
                if (!((this.dataType == JsonDataType.Array || this.dataType == JsonDataType.Object || this.nodeType == JsonNodeType.EndElement) && XmlConverter.IsWhitespace(text)))
                {
                    StartText();
                    WriteEscapedJsonString(text);
                }
            }
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            WriteString(string.Concat(highChar, lowChar));
        }

        public override void WriteValue(bool value)
        {
            StartText();
            nodeWriter.WriteBoolText(value);
        }

        public override void WriteValue(decimal value)
        {
            StartText();
            nodeWriter.WriteDecimalText(value);
        }

        public override void WriteValue(double value)
        {
            StartText();
            nodeWriter.WriteDoubleText(value);
        }

        public override void WriteValue(float value)
        {
            StartText();
            nodeWriter.WriteFloatText(value);
        }

        public override void WriteValue(int value)
        {
            StartText();
            nodeWriter.WriteInt32Text(value);
        }

        public override void WriteValue(long value)
        {
            StartText();
            nodeWriter.WriteInt64Text(value);
        }

        public override void WriteValue(Guid value)
        {
            StartText();
            nodeWriter.WriteGuidText(value);
        }

        public override void WriteValue(DateTime value)
        {
            StartText();
            nodeWriter.WriteDateTimeText(value);
        }

        public override void WriteValue(string value)
        {
            WriteString(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            StartText();
            nodeWriter.WriteTimeSpanText(value);
        }

        public override void WriteValue(UniqueId value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            StartText();
            nodeWriter.WriteUniqueIdText(value);
        }

        public override void WriteValue(object value)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }

            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }

            if (value is Array)
            {
                WriteValue((Array)value);
            }
            else if (value is IStreamProvider)
            {
                WriteValue((IStreamProvider)value);
            }
            else
            {
                WritePrimitiveValue(value);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Whitespace", Justification = "This method is derived from the base")]
        public override void WriteWhitespace(string ws)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (ws == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ws");
            }

            for (int i = 0; i < ws.Length; ++i)
            {
                char c = ws[i];
                if (c != ' ' &&
                    c != '\t' &&
                    c != '\n' &&
                    c != '\r')
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ws",
                        SR.GetString(SR.JsonOnlyWhitespace, c.ToString(), "WriteWhitespace"));
                }
            }

            WriteString(ws);
        }

        public override void WriteXmlAttribute(string localName, string value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteXmlAttribute")));
        }

        public override void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteXmlAttribute")));
        }

        public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            if (!IsWritingNameWithMapping)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteXmlnsAttribute")));
            }
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
        {
            if (!IsWritingNameWithMapping)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.JsonMethodNotSupported, "WriteXmlnsAttribute")));
            }
        }

        internal static bool CharacterNeedsEscaping(char ch)
        {
            return (ch == FORWARD_SLASH || ch == JsonGlobals.QuoteChar || ch < WHITESPACE || ch == BACK_SLASH
                || (ch >= HIGH_SURROGATE_START && (ch <= LOW_SURROGATE_END || ch >= MAX_CHAR)));
        }


        static void ThrowClosed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                new InvalidOperationException(SR.GetString(SR.JsonWriterClosed)));
        }

        void CheckText(JsonNodeType nextNodeType)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }
            if (depth == 0)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    System.Runtime.Serialization.SR.GetString(System.Runtime.Serialization.SR.XmlIllegalOutsideRoot)));
            }

            if ((nextNodeType == JsonNodeType.StandaloneText) &&
                (nodeType == JsonNodeType.QuotedText))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(
                    SR.GetString(SR.JsonCannotWriteStandaloneTextAfterQuotedText)));
            }
        }

        void EnterScope(JsonNodeType currentNodeType)
        {
            depth++;
            if (scopes == null)
            {
                scopes = new JsonNodeType[4];
            }
            else if (scopes.Length == depth)
            {
                JsonNodeType[] newScopes = new JsonNodeType[depth * 2];
                Array.Copy(scopes, newScopes, depth);
                scopes = newScopes;
            }
            scopes[depth] = currentNodeType;
        }

        JsonNodeType ExitScope()
        {
            JsonNodeType nodeTypeToReturn = scopes[depth];
            scopes[depth] = JsonNodeType.None;
            depth--;
            return nodeTypeToReturn;
        }

        void InitializeWriter()
        {
            nodeType = JsonNodeType.None;
            dataType = JsonDataType.None;
            isWritingDataTypeAttribute = false;
            wroteServerTypeAttribute = false;
            isWritingServerTypeAttribute = false;
            serverTypeValue = null;
            attributeText = null;

            if (depth != 0)
            {
                depth = 0;
            }
            if ((scopes != null) && (scopes.Length > JsonGlobals.maxScopeSize))
            {
                scopes = null;
            }

            // Can't let writeState be at Closed if reinitializing.
            writeState = WriteState.Start;
            endElementBuffer = false;
            indentLevel = 0;
        }

        static bool IsUnicodeNewlineCharacter(char c)
        {
            // Newline characters in JSON strings need to be encoded on the way out (DevDiv #665974)
            // See Unicode 6.2, Table 5-1 (http://www.unicode.org/versions/Unicode6.2.0/ch05.pdf]) for the full list.

            // We only care about NEL, LS, and PS, since the other newline characters are all
            // control characters so are already encoded.
            return (c == '\u0085' || c == '\u2028' || c == '\u2029');
        }

        void StartText()
        {
            if (HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.JsonMustUseWriteStringForWritingAttributeValues)));
            }

            if ((dataType == JsonDataType.None) && (serverTypeValue != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonMustSpecifyDataType,
                    JsonGlobals.typeString, JsonGlobals.objectString, JsonGlobals.serverTypeString)));
            }

            if (IsWritingNameWithMapping && !WrittenNameWithMapping)
            {
                // Don't write out any text content unless the local name has been written.
                // Not providing a better error message because localization deadline has passed.
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonMustSpecifyDataType,
                    JsonGlobals.itemString, string.Empty, JsonGlobals.itemString)));
            }

            if ((dataType == JsonDataType.String) ||
                (dataType == JsonDataType.None))
            {
                CheckText(JsonNodeType.QuotedText);
                if (nodeType != JsonNodeType.QuotedText)
                {
                    WriteJsonQuote();
                }
                nodeType = JsonNodeType.QuotedText;
            }
            else if ((dataType == JsonDataType.Number) ||
                (dataType == JsonDataType.Boolean))
            {
                CheckText(JsonNodeType.StandaloneText);
                nodeType = JsonNodeType.StandaloneText;
            }
            else
            {
                ThrowInvalidAttributeContent();
            }
        }

        void ThrowIfServerTypeWritten(string dataTypeSpecified)
        {
            if (serverTypeValue != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonInvalidDataTypeSpecifiedForServerType,
                    JsonGlobals.typeString, dataTypeSpecified, JsonGlobals.serverTypeString, JsonGlobals.objectString)));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")] // [....], ToLowerInvariant is just used in Json error message
        void ThrowInvalidAttributeContent()
        {
            if (HasOpenAttribute)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonInvalidMethodBetweenStartEndAttribute)));
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new XmlException(SR.GetString(SR.JsonCannotWriteTextAfterNonTextAttribute,
                    dataType.ToString().ToLowerInvariant())));
            }
        }

        bool TrySetWritingNameWithMapping(string localName, string ns)
        {
            if (localName.Equals(JsonGlobals.itemString) && ns.Equals(JsonGlobals.itemString))
            {
                nameState = NameState.IsWritingNameWithMapping;
                return true;
            }
            return false;
        }

        void WriteDataTypeServerType()
        {
            if (dataType != JsonDataType.None)
            {
                switch (dataType)
                {
                    case JsonDataType.Array:
                        {
                            EnterScope(JsonNodeType.Collection);
                            nodeWriter.WriteText(JsonGlobals.CollectionChar);
                            indentLevel++;
                            break;
                        }
                    case JsonDataType.Object:
                        {
                            EnterScope(JsonNodeType.Object);
                            nodeWriter.WriteText(JsonGlobals.ObjectChar);
                            indentLevel++;
                            break;
                        }
                    case JsonDataType.Null:
                        {
                            nodeWriter.WriteText(JsonGlobals.nullString);
                            break;
                        }
                    default:
                        break;
                }

                if (serverTypeValue != null)
                {
                    // dataType must be object because we throw in all other case.
                    WriteServerTypeAttribute();
                }
            }
        }

        [SecuritySafeCritical]
        unsafe void WriteEscapedJsonString(string str)
        {
            fixed (char* chars = str)
            {
                int i = 0;
                int j;
                for (j = 0; j < str.Length; j++)
                {
                    char ch = chars[j];
                    if (ch <= FORWARD_SLASH)
                    {
                        if (ch == FORWARD_SLASH || ch == JsonGlobals.QuoteChar)
                        {
                            nodeWriter.WriteChars(chars + i, j - i);
                            nodeWriter.WriteText(BACK_SLASH);
                            nodeWriter.WriteText(ch);
                            i = j + 1;
                        }
                        else if (ch < WHITESPACE)
                        {
                            nodeWriter.WriteChars(chars + i, j - i);
                            nodeWriter.WriteText(BACK_SLASH);
                            nodeWriter.WriteText('u');
                            nodeWriter.WriteText(string.Format(CultureInfo.InvariantCulture, "{0:x4}", (int)ch));
                            i = j + 1;
                        }
                    }
                    else if (ch == BACK_SLASH)
                    {
                        nodeWriter.WriteChars(chars + i, j - i);
                        nodeWriter.WriteText(BACK_SLASH);
                        nodeWriter.WriteText(ch);
                        i = j + 1;
                    }
                    else if ((ch >= HIGH_SURROGATE_START && (ch <= LOW_SURROGATE_END || ch >= MAX_CHAR)) || IsUnicodeNewlineCharacter(ch))
                    {
                        nodeWriter.WriteChars(chars + i, j - i);
                        nodeWriter.WriteText(BACK_SLASH);
                        nodeWriter.WriteText('u');
                        nodeWriter.WriteText(string.Format(CultureInfo.InvariantCulture, "{0:x4}", (int)ch));
                        i = j + 1;
                    }
                }
                if (i < j)
                {
                    nodeWriter.WriteChars(chars + i, j - i);
                }
            }
        }

        void WriteIndent()
        {
            for (int i = 0; i < indentLevel; i++)
            {
                nodeWriter.WriteText(indentChars);
            }
        }

        void WriteNewLine()
        {
            nodeWriter.WriteText(CARRIAGE_RETURN);
            nodeWriter.WriteText(NEWLINE);
        }

        void WriteJsonElementName(string localName)
        {
            WriteJsonQuote();
            WriteEscapedJsonString(localName);
            WriteJsonQuote();
            nodeWriter.WriteText(JsonGlobals.NameValueSeparatorChar);
            if (indent)
            {
                nodeWriter.WriteText(WHITESPACE);
            }
        }

        void WriteJsonQuote()
        {
            nodeWriter.WriteText(JsonGlobals.QuoteChar);
        }

        void WritePrimitiveValue(object value)
        {
            if (IsClosed)
            {
                ThrowClosed();
            }

            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            }

            if (value is ulong)
            {
                WriteValue((ulong)value);
            }
            else if (value is string)
            {
                WriteValue((string)value);
            }
            else if (value is int)
            {
                WriteValue((int)value);
            }
            else if (value is long)
            {
                WriteValue((long)value);
            }
            else if (value is bool)
            {
                WriteValue((bool)value);
            }
            else if (value is double)
            {
                WriteValue((double)value);
            }
            else if (value is DateTime)
            {
                WriteValue((DateTime)value);
            }
            else if (value is float)
            {
                WriteValue((float)value);
            }
            else if (value is decimal)
            {
                WriteValue((decimal)value);
            }
            else if (value is XmlDictionaryString)
            {
                WriteValue((XmlDictionaryString)value);
            }
            else if (value is UniqueId)
            {
                WriteValue((UniqueId)value);
            }
            else if (value is Guid)
            {
                WriteValue((Guid)value);
            }
            else if (value is TimeSpan)
            {
                WriteValue((TimeSpan)value);
            }
            else if (value.GetType().IsArray)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString(SR.JsonNestedArraysNotSupported), "value"));
            }
            else
            {
                base.WriteValue(value);
            }
        }

        void WriteServerTypeAttribute()
        {
            string value = serverTypeValue;
            JsonDataType oldDataType = dataType;
            NameState oldNameState = nameState;
            WriteStartElement(JsonGlobals.serverTypeString);
            WriteValue(value);
            WriteEndElement();
            dataType = oldDataType;
            nameState = oldNameState;
            wroteServerTypeAttribute = true;
        }

        void WriteValue(ulong value)
        {
            StartText();
            nodeWriter.WriteUInt64Text(value);
        }

        void WriteValue(Array array)
        {
            // This method is called only if WriteValue(object) is called with an array
            // The contract for XmlWriter.WriteValue(object) requires that this object array be written out as a string.
            // E.g. WriteValue(new int[] { 1, 2, 3}) should be equivalent to WriteString("1 2 3").             
            JsonDataType oldDataType = dataType;
            // Set attribute mode to String because WritePrimitiveValue might write numerical text.
            //  Calls to methods that write numbers can't be mixed with calls that write quoted text unless the attribute mode is explictly string.            
            dataType = JsonDataType.String;
            StartText();
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    nodeWriter.WriteText(JsonGlobals.WhitespaceChar);
                }
                WritePrimitiveValue(array.GetValue(i));
            }
            dataType = oldDataType;
        }

        class JsonNodeWriter : XmlUTF8NodeWriter
        {
            [SecurityCritical]
            internal unsafe void WriteChars(char* chars, int charCount)
            {
                base.UnsafeWriteUTF8Chars(chars, charCount);
            }
        }
    }
}
