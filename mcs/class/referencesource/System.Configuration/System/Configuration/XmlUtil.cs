//------------------------------------------------------------------------------
// <copyright file="XmlUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Configuration.Internal;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Net;

    //
    // XmlTextReader Helper class.
    // 
    // Provides the following services:
    //
    //      * Reader methods that verify restrictions on the XML that can be contained in a config file.
    //      * Methods to copy the reader stream to a writer stream.
    //      * Method to copy a configuration section to a string.
    //      * Methods to format a string of XML.
    //
    // Errors found during read are accumlated in a ConfigurationSchemaErrors object.
    //
    internal sealed class XmlUtil : IDisposable, IConfigErrorInfo
    {
        private const int   MAX_LINE_WIDTH=60;

        // Offset from where the reader reports the LinePosition of an Xml Node to
        // the start of that representation in text.
        static readonly int[] s_positionOffset = {
            0,      // None,                  
            1,      // Element,                 <elem
            -1,     // Attribute,               N/A
            0,      // Text,                    
            9,      // CDATA,                   <![CDATA[
            1,      // EntityReference,         &lt
            -1,     // Entity,                  N/A
            2,      // ProcessingInstruction,   <?pi
            4,      // Comment,                 <!--
            -1,     // Document,                N/A
            10,     // DocumentType,            <!DOCTYPE
            -1,     // DocumentFragment,        N/A
            -1,     // Notation,                N/A
            0,      // Whitespace,              
            0,      // SignificantWhitespace,   
            2,      // EndElement,              />
            -1,     // EndEntity,               N/A
            2,      // XmlDeclaration           <?xml
        };

        private static int GetPositionOffset(XmlNodeType nodeType) {
            return s_positionOffset[(int)nodeType];
        }

        private Stream                      _stream;                    // the stream to read
        private string                      _streamName;                // name of the stream, typically a file name
        private XmlTextReader               _reader;                    // the XmlTextReader over the stream
        private StringWriter                _cachedStringWriter;        // cached string writer used by CopySection()
        private ConfigurationSchemaErrors   _schemaErrors;              // accumulated errors
        private int                         _lastLineNumber;            // last line number after a call to CopyXmlNode()
        private int                         _lastLinePosition;          // last line position after a call to CopyXmlNode()

        internal XmlUtil(Stream stream, string name, bool readToFirstElement) :
            this(stream, name, readToFirstElement, new ConfigurationSchemaErrors()) {}

        internal XmlUtil(Stream stream, string name, bool readToFirstElement, ConfigurationSchemaErrors schemaErrors) {
            try {
                _streamName = name;
                _stream = stream;
                _reader = new XmlTextReader(_stream);

                // config reads never require a resolver
                _reader.XmlResolver = null;

                _schemaErrors = schemaErrors;
                _lastLineNumber = 1;
                _lastLinePosition = 1;

                //
                // When parsing config that we don't intend to copy, skip all content
                // before the first element.
                //
                if (readToFirstElement) {
                    _reader.WhitespaceHandling = WhitespaceHandling.None;

                    bool done = false;
                    while (!done && _reader.Read()) {
                        switch (_reader.NodeType) {
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.Comment:
                            case XmlNodeType.DocumentType:
                                break;

                            case XmlNodeType.Element:
                                done = true;
                                break;

                            default:
                                throw new ConfigurationErrorsException(SR.GetString(SR.Config_base_unrecognized_element), this);
                        }
                    }
                }
            }
            catch {
                ReleaseResources();
                throw;
            }
        }

        private void ReleaseResources() {
            if (_reader != null) {
                // closing _reader will also close underlying _stream
                _reader.Close();
                _reader = null;
            }
            else if (_stream != null) {
                _stream.Close();
            }

            _stream = null;

            if (_cachedStringWriter != null) {
                _cachedStringWriter.Close();
                _cachedStringWriter = null;
            }
        }

        public void Dispose() {
            ReleaseResources();
        }

        public string Filename {
            get { return _streamName; } 
        }

        public int LineNumber {
            get { return Reader.LineNumber; }
        }

        //
        // Return the line position of the reader, compensating for the reader's offset
        // for nodes such as an XmlElement.
        //
        internal int TrueLinePosition {
            get { 
                int trueLinePosition = Reader.LinePosition - GetPositionOffset(Reader.NodeType); 
                Debug.Assert(trueLinePosition > 0, "trueLinePosition > 0");
                return trueLinePosition;
            }
        }

        internal XmlTextReader Reader {
            get { return _reader; }
        }

        internal ConfigurationSchemaErrors SchemaErrors {
            get { return _schemaErrors; }
        }

        //
        // Read until the Next Element element, or we hit
        // the end of the file.
        //
        internal void ReadToNextElement() {
            while (_reader.Read()) {
                if (_reader.MoveToContent() == XmlNodeType.Element) {
                    // We found an element, so return
                    return;
                }
            }

            // We must of hit end of file
        }

        //
        // Skip this element and its children, then read to next start element,
        // or until we hit end of file.
        //
        internal void SkipToNextElement() {
            _reader.Skip();
            _reader.MoveToContent();

            while (!_reader.EOF && _reader.NodeType != XmlNodeType.Element) {
                _reader.Read();
                _reader.MoveToContent();
            }
        }

        //
        // Read to the next start element, and verify that all XML nodes read are permissible.
        //
        internal void StrictReadToNextElement(ExceptionAction action) {
            while (_reader.Read()) {
                // optimize for the common case
                if (_reader.NodeType == XmlNodeType.Element) {
                    return;
                }

                VerifyIgnorableNodeType(action);
            }
        }

        //
        // Skip this element and its children, then read to next start element,
        // or until we hit end of file. Verify that nodes that are read after the
        // skipped element are permissible.
        //
        internal void StrictSkipToNextElement(ExceptionAction action) {
            _reader.Skip();

            while (!_reader.EOF && _reader.NodeType != XmlNodeType.Element) {
                VerifyIgnorableNodeType(action);
                _reader.Read();
            }
        }

        //
        // StrictSkipToOurParentsEndElement
        //
        // Skip until we hit the end element for our parent, and verify
        // that nodes at the parent level are permissible.
        //
        internal void StrictSkipToOurParentsEndElement(ExceptionAction action) {
            int currentDepth = _reader.Depth;

            // Skip everything at out current level
            while (_reader.Depth >= currentDepth) {
                _reader.Skip();
            }

            while (!_reader.EOF && _reader.NodeType != XmlNodeType.EndElement) {
                VerifyIgnorableNodeType(action);
                _reader.Read();
            }
        }

        //
        // Add an error if the node type is not permitted by the configuration schema.
        //
        internal void VerifyIgnorableNodeType(ExceptionAction action) {
            XmlNodeType nodeType = _reader.NodeType;
            
            if (nodeType != XmlNodeType.Comment && nodeType != XmlNodeType.EndElement) {
                ConfigurationException ex = new ConfigurationErrorsException(
                    SR.GetString(SR.Config_base_unrecognized_element), 
                    this);
                                
                SchemaErrors.AddError(ex, action);
            }
        }

        //
        // Add an error if there are attributes that have not been examined,
        // and are therefore unrecognized.
        //
        internal void VerifyNoUnrecognizedAttributes(ExceptionAction action) {
            if (_reader.MoveToNextAttribute()) {
                AddErrorUnrecognizedAttribute(action);
            }
        }

        //
        // Add an error if the retrieved attribute is null, 
        // and therefore not present.
        //
        internal bool VerifyRequiredAttribute(
                object o, string attrName, ExceptionAction action) {

            if (o == null) {
                AddErrorRequiredAttribute(attrName, action);
                return false;
            }
            else {
                return true;
            }
        }

        // 
        // Functions to handle parsing errors
        //

        internal void AddErrorUnrecognizedAttribute(ExceptionAction action) {
            ConfigurationErrorsException ex = new ConfigurationErrorsException(
                SR.GetString(SR.Config_base_unrecognized_attribute, _reader.Name), 
                this);

            SchemaErrors.AddError(ex, action);
        }

        internal void AddErrorRequiredAttribute(string attrib, ExceptionAction action) {
            ConfigurationErrorsException ex = new ConfigurationErrorsException(
                SR.GetString(SR.Config_missing_required_attribute, attrib, _reader.Name),
                this);

            SchemaErrors.AddError(ex, action);
        }

        internal void AddErrorReservedAttribute(ExceptionAction action) {
            ConfigurationErrorsException ex = new ConfigurationErrorsException(
                SR.GetString(SR.Config_reserved_attribute, _reader.Name),
                this);

            SchemaErrors.AddError(ex, action);
        }

        internal void AddErrorUnrecognizedElement(ExceptionAction action) {
            ConfigurationErrorsException ex = new ConfigurationErrorsException(
                SR.GetString(SR.Config_base_unrecognized_element),
                this);

            SchemaErrors.AddError(ex, action);
        }

        internal void VerifyAndGetNonEmptyStringAttribute(ExceptionAction action, out string newValue) {
            if (!String.IsNullOrEmpty(_reader.Value)) {
                newValue = _reader.Value;
            }
            else {
                newValue = null;

                ConfigurationException ex = new ConfigurationErrorsException(
                    SR.GetString(SR.Empty_attribute, _reader.Name),
                    this);

                SchemaErrors.AddError(ex, action);
            }
        }

        // VerifyAndGetBooleanAttribute
        //
        // Verify and Retrieve the Boolean Attribute.  If it is not
        // a valid value then log an error and set the value to a given default.
        //
        internal void VerifyAndGetBooleanAttribute(
                ExceptionAction action, bool defaultValue, out bool newValue) {

            if (_reader.Value == "true") {
                newValue = true;
            }
            else if (_reader.Value == "false") {
                newValue = false;
            }
            else {
                // Unrecognized value
                newValue = defaultValue;

                ConfigurationErrorsException ex = new ConfigurationErrorsException(
                    SR.GetString(SR.Config_invalid_boolean_attribute, _reader.Name), 
                    this);

                SchemaErrors.AddError(ex, action);
            }
        }

        //
        // Copy an XML element, then continue copying until we've hit the next element
        // or exited this depth.
        //
        internal bool CopyOuterXmlToNextElement(XmlUtilWriter utilWriter, bool limitDepth) {
            CopyElement(utilWriter);

            // Copy until reaching the next element, or if limitDepth == true until we've exited this depth.
            return CopyReaderToNextElement(utilWriter, limitDepth);
        }

        //
        // Copy an XML element but skip all its child elements, then continue copying until we've hit the next element.
        //
        internal bool SkipChildElementsAndCopyOuterXmlToNextElement(XmlUtilWriter utilWriter) {
            bool    isEmptyElement = _reader.IsEmptyElement;
            int     startingLine = _reader.LineNumber;
#if DBG
            int     depth = _reader.Depth;
#endif

            Debug.Assert(_reader.NodeType == XmlNodeType.Element, "_reader.NodeType == XmlNodeType.Element");

            CopyXmlNode(utilWriter);

            // See if we need to skip any child element
            if (!isEmptyElement) {
                while (_reader.NodeType != XmlNodeType.EndElement) {
                    
                    // Skip all the inner child elements
                    if (_reader.NodeType == XmlNodeType.Element) {
                        _reader.Skip();

                        // We need to skip all the whitespaces following a skipped element.
                        // - If the whitespaces don't contain /r/n, then it's okay to skip them 
                        //   as part of the element.
                        // - If the whitespaces contain /r/n, not skipping them will result
                        //   in a redundant emtpy line being copied.
                        if (_reader.NodeType == XmlNodeType.Whitespace) {
                            _reader.Skip();
                        }
                    }
                    else {
                        // We want to preserve other content, e.g. comments.
                        CopyXmlNode(utilWriter);
                    }
                }

                if (_reader.LineNumber != startingLine) {
                    // The whitespace in front of the EndElement was skipped above.
                    // We need to append spaces to compensate for that.
                    utilWriter.AppendSpacesToLinePosition(TrueLinePosition);
                }


#if DBG                
                Debug.Assert(_reader.Depth == depth, "We should be at the same depth as the opening Element");
#endif
                    
                // Copy the end element.
                CopyXmlNode(utilWriter);
            }                

            return CopyReaderToNextElement(utilWriter, true);
        }

        //
        // Copy the reader until we hit an element, or we've exited the current depth.
        //
        internal bool CopyReaderToNextElement(XmlUtilWriter utilWriter, bool limitDepth) {
            bool moreToRead = true;

            // Set the depth if we limit copying to this depth
            int depth;
            if (limitDepth) {
                // there is nothing in the element
                if (_reader.NodeType == XmlNodeType.EndElement)
                    return true;

                depth = _reader.Depth;
            }
            else {
                depth = 0;
            }

            // Copy nodes until we've reached the desired depth, or until we hit an element.
            do {
                if (_reader.NodeType == XmlNodeType.Element)
                    break;

                if (_reader.Depth < depth) {
                    break;
                }

                moreToRead = CopyXmlNode(utilWriter);
            } while (moreToRead);

            return moreToRead;
        }

        //
        // Skip over the current element and copy until the next element.
        // This function removes the one blank line that would otherwise
        // be inserted by simply skipping and copying to the next element
        // in a situation like this:
        //
        //      <!-- end of previous configSection -->
        //      <configSectionToDelete>
        //          <content />
        //          <moreContent />
        //      </configSectionToDelete>
        //      <!-- end of configSectionToDelete -->
        //      <nextConfigSection />
        //
        internal bool SkipAndCopyReaderToNextElement(XmlUtilWriter utilWriter, bool limitDepth) {
            Debug.Assert(_reader.NodeType == XmlNodeType.Element, "_reader.NodeType == XmlNodeType.Element");

            // If the last line before the element is not blank, then we do not have to
            // remove the blank line.
            if (!utilWriter.IsLastLineBlank) {
                _reader.Skip();
                return CopyReaderToNextElement(utilWriter, limitDepth);
            }

            // Set the depth if we limit copying to this depth
            int depth;
            if (limitDepth) {
                depth = _reader.Depth;
            }
            else {
                depth = 0;
            }

            // Skip over the element
            _reader.Skip();

            int lineNumberOfEndElement = _reader.LineNumber;

            // Read until we hit a a non-whitespace node or reach the end
            while (!_reader.EOF) {
                if (_reader.NodeType != XmlNodeType.Whitespace) {
                    //
                    // If the next non-whitepace node is on another line,
                    // seek back to the beginning of the current blank line,
                    // skip a blank line of whitespace, and copy the remaining whitespace.
                    //
                    if (_reader.LineNumber > lineNumberOfEndElement) {
                        utilWriter.SeekToLineStart();
                        utilWriter.AppendWhiteSpace(lineNumberOfEndElement + 1, 1, LineNumber, TrueLinePosition);
                    }

                    break;
                }

                _reader.Read();
            }

            // Copy nodes until we've reached the desired depth, or until we hit an element.
            while (!_reader.EOF) {
                if (_reader.NodeType == XmlNodeType.Element)
                    break;

                if (_reader.Depth < depth) {
                    break;
                }

                CopyXmlNode(utilWriter);
            };

            return !_reader.EOF;
        }

        //
        // Copy an XML element and its children, up to and including the end element.
        //
        private void CopyElement(XmlUtilWriter utilWriter) {
            Debug.Assert(_reader.NodeType== XmlNodeType.Element, "_reader.NodeType== XmlNodeType.Element");

            int depth = _reader.Depth;
            bool isEmptyElement = _reader.IsEmptyElement;

            // Copy current node
            CopyXmlNode(utilWriter);

            // Copy nodes while the depth is greater than the current depth.
            while (_reader.Depth > depth) {
                CopyXmlNode(utilWriter);
            }

            // Copy the end element.
            if (!isEmptyElement) {
                CopyXmlNode(utilWriter);
            }
        }

        //
        // Copy a single XML node, attempting to preserve whitespace.
        // A side effect of this method is to advance the reader to the next node.
        //
        // PERFORMANCE NOTE: this function is used at runtime to copy a configuration section,
        // and at designtime to copy an entire XML document.
        //
        // At designtime, this function needs to be able to copy a <!DOCTYPE declaration.
        // Copying a <!DOCTYPE declaration is expensive, because due to limitations of the 
        // XmlReader API, we must track the position of the writer to accurately format it. 
        // Tracking the position of the writer is expensive, as it requires examining every
        // character that is written for newline characters, and maintaining the seek position
        // of the underlying stream at each new line, which in turn requires a stream flush.
        //
        // This function must NEVER require tracking the writer position to copy the Xml nodes
        // that are used in a configuration section.
        // 
        internal bool CopyXmlNode(XmlUtilWriter utilWriter) {
            //
            // For nodes that have a closing string, such as "<element  >"
            // the XmlReader API does not give us the location of the closing string, e.g. ">".
            // To correctly determine the location of the closing part, we advance the reader,
            // determine the position of the next node, then work backwards to add whitespace
            // and add the closing string.
            //
            string close = null;
            int lineNumber = -1;
            int linePosition = -1;

            int readerLineNumber = 0;
            int readerLinePosition = 0;
            int writerLineNumber = 0;
            int writerLinePosition = 0;
            if (utilWriter.TrackPosition) {
                readerLineNumber = _reader.LineNumber;
                readerLinePosition = _reader.LinePosition;
                writerLineNumber = utilWriter.LineNumber;
                writerLinePosition = utilWriter.LinePosition;
            }

            // We test the node type in the likely order of decreasing occurrence.
            XmlNodeType nodeType = _reader.NodeType;
            if (nodeType == XmlNodeType.Whitespace) {
                utilWriter.Write(_reader.Value);
            }
            else if (nodeType == XmlNodeType.Element) {
                close = (_reader.IsEmptyElement) ? "/>" : ">";

                // get the line position after the element declaration:
                //      <element    attr="value"
                //              ^
                //              linePosition
                //
                lineNumber = _reader.LineNumber;
                linePosition = _reader.LinePosition + _reader.Name.Length;

                utilWriter.Write('<');
                utilWriter.Write(_reader.Name);

                //
                // Note that there is no way to get spacing between attribute name and value
                // For example:
                //
                //          <elem attr="value" />
                //
                // is reported with the same position as
                //
                //          <elem attr = "value" />
                //
                // The first example has no spaces around '=', the second example does.
                //
                while (_reader.MoveToNextAttribute()) {
                    // get line position of the attribute declaration
                    //      <element attr="value"
                    //               ^
                    //               attrLinePosition
                    //
                    int attrLineNumber = _reader.LineNumber;
                    int attrLinePosition = _reader.LinePosition;

                    // Write the whitespace before the attribute
                    utilWriter.AppendRequiredWhiteSpace(lineNumber, linePosition, attrLineNumber, attrLinePosition);

                    // Write the attribute and value
                    int charactersWritten = utilWriter.Write(_reader.Name);
                    charactersWritten += utilWriter.Write('=');
                    charactersWritten += utilWriter.AppendAttributeValue(_reader);

                    // Update position. Note that the attribute value is escaped to always be on a single line.
                    lineNumber = attrLineNumber;
                    linePosition = attrLinePosition + charactersWritten;
                }
            }
            else if (nodeType == XmlNodeType.EndElement) {
                close = ">";

                // get line position after the end element declaration:
                //      </element    >
                //               ^
                //               linePosition
                //
                lineNumber = _reader.LineNumber;
                linePosition = _reader.LinePosition + _reader.Name.Length;

                utilWriter.Write("</");
                utilWriter.Write(_reader.Name);
            }
            else if (nodeType == XmlNodeType.Comment) {
                utilWriter.AppendComment(_reader.Value);
            }
            else if (nodeType == XmlNodeType.Text) {
                utilWriter.AppendEscapeTextString(_reader.Value);
            }
            else if (nodeType == XmlNodeType.XmlDeclaration) {
                close = "?>";

                // get line position after the xml declaration:
                //      <?xml    version="1.0"
                //           ^
                //           linePosition
                //
                lineNumber = _reader.LineNumber;
                linePosition = _reader.LinePosition + 3;

                utilWriter.Write("<?xml");

                //
                // Note that there is no way to get spacing between attribute name and value
                // For example:
                //
                //          <?xml attr="value" ?>
                //
                // is reported with the same position as
                //
                //          <?xml attr = "value" ?>
                //
                // The first example has no spaces around '=', the second example does.
                //
                while (_reader.MoveToNextAttribute()) {
                    // get line position of the attribute declaration
                    //      <?xml    version="1.0"
                    //               ^
                    //               attrLinePosition
                    //
                    int attrLineNumber = _reader.LineNumber;
                    int attrLinePosition = _reader.LinePosition;

                    // Write the whitespace before the attribute
                    utilWriter.AppendRequiredWhiteSpace(lineNumber, linePosition, attrLineNumber, attrLinePosition);

                    // Write the attribute and value
                    int charactersWritten = utilWriter.Write(_reader.Name);
                    charactersWritten += utilWriter.Write('=');
                    charactersWritten += utilWriter.AppendAttributeValue(_reader);

                    // Update position. Note that the attribute value is escaped to always be on a single line.
                    lineNumber = attrLineNumber;
                    linePosition = attrLinePosition + charactersWritten;
                }

                // Position reader at beginning of node
                _reader.MoveToElement();
            }
            else if (nodeType == XmlNodeType.SignificantWhitespace) {
                utilWriter.Write(_reader.Value);
            }
            else if (nodeType == XmlNodeType.ProcessingInstruction) {
                //
                // Note that there is no way to get spacing between attribute name and value
                // For example:
                //
                //          <?pi "value" ?>
                //
                // is reported with the same position as
                //
                //          <?pi    "value" ?>
                //
                // The first example has one space between 'pi' and "value", the second has multiple spaces.
                //
                utilWriter.AppendProcessingInstruction(_reader.Name, _reader.Value);
            }
            else if (nodeType == XmlNodeType.EntityReference) {
                utilWriter.AppendEntityRef(_reader.Name);
            }
            else if (nodeType == XmlNodeType.CDATA) {
                utilWriter.AppendCData(_reader.Value);
            }
            else if (nodeType == XmlNodeType.DocumentType) {
                // 
                // XmlNodeType.DocumentType has the following format:
                //
                //      <!DOCTYPE rootElementName {(SYSTEM uriRef)|(PUBLIC id uriRef)} {[ dtdDecls ]} >
                //
                // The reader only gives us the position of 'rootElementName', so we must track what was
                // written before "<!DOCTYPE" in order to correctly determine the position of the
                // <!DOCTYPE tag
                //
                Debug.Assert(utilWriter.TrackPosition, "utilWriter.TrackPosition");
                int c = utilWriter.Write("<!DOCTYPE");

                // Write the space between <!DOCTYPE and the rootElementName
                utilWriter.AppendRequiredWhiteSpace(_lastLineNumber, _lastLinePosition + c, _reader.LineNumber, _reader.LinePosition);

                // Write the rootElementName
                utilWriter.Write(_reader.Name);

                // Get the dtd declarations, if any
                string dtdValue = null;
                if (_reader.HasValue) {
                    dtdValue = _reader.Value;
                }

                // get line position after the !DOCTYPE declaration:
                //      <!DOCTYPE  rootElement     SYSTEM rootElementDtdUri >
                //                            ^
                //                            linePosition
                lineNumber = _reader.LineNumber;
                linePosition = _reader.LinePosition + _reader.Name.Length;

                // Note that there is no way to get the spacing after PUBLIC or SYSTEM attributes and their values
                if (_reader.MoveToFirstAttribute()) {
                    // Write the space before SYSTEM or PUBLIC
                    utilWriter.AppendRequiredWhiteSpace(lineNumber, linePosition, _reader.LineNumber, _reader.LinePosition); 

                    // Write SYSTEM or PUBLIC and the 1st value of the attribute
                    string attrName = _reader.Name;
                    utilWriter.Write(attrName);
                    utilWriter.AppendSpace();
                    utilWriter.AppendAttributeValue(_reader);
                    _reader.MoveToAttribute(0);
                    
                    // If PUBLIC, write the second value of the attribute
                    if (attrName == "PUBLIC") {
                        _reader.MoveToAttribute(1);
                        utilWriter.AppendSpace();
                        utilWriter.AppendAttributeValue(_reader);
                        _reader.MoveToAttribute(1);
                    }
                }

                // If there is a dtd, write it
                if (dtdValue != null && dtdValue.Length > 0) {
                    utilWriter.Write(" [");
                    utilWriter.Write(dtdValue);
                    utilWriter.Write(']');
                }

                utilWriter.Write('>');
            }

            // Advance the _reader so we can get the position of the next node.
            bool moreToRead = _reader.Read();
            nodeType = _reader.NodeType;

            // Close the node we are copying.
            if (close != null) {
                //
                // Find the position of the close string, for example:
                //
                //          <element      >  <subElement />
                //                        ^
                //                        closeLinePosition
                //
                int startOffset = GetPositionOffset(nodeType);
                int closeLineNumber = _reader.LineNumber;
                int closeLinePosition = _reader.LinePosition - startOffset - close.Length;

                // Add whitespace up to the position of the close string
                utilWriter.AppendWhiteSpace(lineNumber, linePosition, closeLineNumber, closeLinePosition);

                // Write the close string
                utilWriter.Write(close);
            }

            //
            // Track the position of the reader based on the position of the reader
            // before we copied this node and what we have written in copying the node.
            // This allows us to determine the position of the <!DOCTYPE tag.
            //
            if (utilWriter.TrackPosition) {
                _lastLineNumber = (readerLineNumber - writerLineNumber) + utilWriter.LineNumber;

                if (writerLineNumber == utilWriter.LineNumber) {
                    _lastLinePosition = (readerLinePosition - writerLinePosition) + utilWriter.LinePosition;
                }
                else {
                    _lastLinePosition = utilWriter.LinePosition;
                }
            }

            return moreToRead;
        }

        // RetrieveFullOpenElementTag
        //
        // Asuming that we are at an element, retrieve the text for that element
        // and attributes that can be serialized to an xml file.
        //
        private string RetrieveFullOpenElementTag() {
            StringBuilder element;

            Debug.Assert(_reader.NodeType == XmlNodeType.Element, 
                         "_reader.NodeType == NodeType.Element");

            // Start with element tag name
            element = new StringBuilder(64);
            element.Append("<");
            element.Append(_reader.Name);

            // Add attributes
            while (_reader.MoveToNextAttribute()) {

                element.Append(" ");
                element.Append(_reader.Name);
                element.Append("=");
                element.Append('\"');
                element.Append(_reader.Value);
                element.Append('\"');
            }

            // Now close the element tag
            element.Append(">");

            return element.ToString();
        }
        
        //
        // Copy or replace an element node.
        // If the element is an empty element, replace it with a formatted start element if either:
        //   * The contents of the start element string need updating.
        //   * The element needs to contain child elements.
        //
        // If the element is empty and is replaced with a start/end element pair, return a 
        // end element string with whitespace formatting; otherwise return null.
        //
        internal string UpdateStartElement(XmlUtilWriter utilWriter, string updatedStartElement, bool needsChildren, int linePosition, int indent) {
            Debug.Assert(_reader.NodeType == XmlNodeType.Element, "_reader.NodeType == NodeType.Element");

            string  endElement = null;
            bool    needsEndElement = false;
            string  elementName;

            elementName = _reader.Name;

            // If the element is empty, determine if a new end element is needed.
            if (_reader.IsEmptyElement) {
                if (updatedStartElement == null && needsChildren) {
                    updatedStartElement = RetrieveFullOpenElementTag();
                }

                needsEndElement = (updatedStartElement != null);
            }

            if (updatedStartElement == null) {
                //
                // If no changes to the start element are required, just copy it.
                //
                CopyXmlNode(utilWriter);
            }
            else {
                //
                // Format a new start element/end element pair
                //
                string updatedEndElement = "</" + elementName + ">";
                string updatedElement = updatedStartElement + updatedEndElement;
                string formattedElement = FormatXmlElement(updatedElement, linePosition, indent, true);

                //
                // Get the start and end element strings from the formatted element.
                //
                int iEndElement = formattedElement.LastIndexOf('\n') + 1;
                string startElement;
                if (needsEndElement) {
                    endElement = formattedElement.Substring(iEndElement);

                    // Include a newline in the start element as we are expanding an empty element.
                    startElement = formattedElement.Substring(0, iEndElement);
                }
                else {
                    // Omit the newline from the start element.
                    startElement = formattedElement.Substring(0, iEndElement - 2);
                }

                // Write the new start element.
                utilWriter.Write(startElement);

                // Skip over the existing start element.
                _reader.Read();
            }

            return endElement;
        }

        //
        // Create the cached string writer if it does not exist, 
        // otherwise reuse the existing buffer.
        // 
        private void ResetCachedStringWriter() {
            if (_cachedStringWriter == null) {
                _cachedStringWriter = new StringWriter(new StringBuilder(64), CultureInfo.InvariantCulture);
            }
            else {
                _cachedStringWriter.GetStringBuilder().Length = 0;
            }
        }

        //
        // Copy a configuration section to a string, and advance the reader.
        //
        internal string CopySection() {
            ResetCachedStringWriter();

            // Preserve whitespace for sections for backcompat
            WhitespaceHandling originalHandling = _reader.WhitespaceHandling;
            _reader.WhitespaceHandling = WhitespaceHandling.All;

            // Create string writer to write to
            XmlUtilWriter utilWriter = new XmlUtilWriter(_cachedStringWriter, false);

            // Copy the element
            CopyElement(utilWriter);

            // Reset whitespace handling
            _reader.WhitespaceHandling = originalHandling;

            if ((originalHandling == WhitespaceHandling.None) &&
                 (Reader.NodeType  == XmlNodeType.Whitespace))  {
                // If we were previously suppose to skip whitespace, and now we
                // are at it, then lets jump to the next item
                _reader.Read();
            }

            utilWriter.Flush();
            string s = ((StringWriter)utilWriter.Writer).ToString();
            return s;
        }

        // Format an Xml element to be written to the config file.
        // Params:
        //   xmlElement      - the element
        //   linePosition    - start position of the element
        //   indent          - indent for each depth
        //   skipFirstIndent - skip indent for the first element?
        //
        static internal string FormatXmlElement(string xmlElement, int linePosition, int indent, bool skipFirstIndent) {

            XmlParserContext context = new XmlParserContext(null, null, null, XmlSpace.Default, Encoding.Unicode);
            XmlTextReader reader = new XmlTextReader(xmlElement, XmlNodeType.Element, context);

            StringWriter stringWriter = new StringWriter(new StringBuilder(64), CultureInfo.InvariantCulture);
            XmlUtilWriter utilWriter = new XmlUtilWriter(stringWriter, false);

            // append newline before indent?
            bool newLine = false;

            // last node visited was text?
            bool lastWasText = false;

            // width of line from end of indentation
            int lineWidth;

            // length of the stringbuilder after last indent with newline
            int sbLengthLastNewLine = 0;

            while (reader.Read()) {
                XmlNodeType nodeType = reader.NodeType;

                if (lastWasText) {
                    utilWriter.Flush();
                    lineWidth = sbLengthLastNewLine - ((StringWriter)utilWriter.Writer).GetStringBuilder().Length;
                }
                else {
                    lineWidth = 0;
                }

                switch (nodeType) {
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Element:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.Comment:
                        // Do not indent if the last node was text - doing so would add whitespace
                        // that is included as part of the text.
                        if (!skipFirstIndent && !lastWasText) {
                            utilWriter.AppendIndent(linePosition, indent, reader.Depth, newLine);

                            if (newLine) {
                                utilWriter.Flush();
                                sbLengthLastNewLine = ((StringWriter)utilWriter.Writer).GetStringBuilder().Length;
                            }
                        }
                        break;

                    default:
                        break;
                }

                lastWasText = false;
                switch (nodeType) {
                    case XmlNodeType.Whitespace:
                        break;

                    case XmlNodeType.SignificantWhitespace:
                        utilWriter.Write(reader.Value);
                        break;

                    case XmlNodeType.CDATA:
                        utilWriter.AppendCData(reader.Value);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                        utilWriter.AppendProcessingInstruction(reader.Name, reader.Value);
                        break;

                    case XmlNodeType.Comment:
                        utilWriter.AppendComment(reader.Value);
                        break;

                    case XmlNodeType.Text:
                        utilWriter.AppendEscapeTextString(reader.Value);
                        lastWasText = true;
                        break;

                    case XmlNodeType.Element:
                        {
                            // Write "<elem"
                            utilWriter.Write('<');
                            utilWriter.Write(reader.Name);

                            lineWidth += reader.Name.Length + 2;

                            int c = reader.AttributeCount;
                            for (int i = 0; i < c; i++) {
                                // Add new line if we've exceeded the line width
                                bool writeSpace;
                                if (lineWidth > MAX_LINE_WIDTH) {
                                    utilWriter.AppendIndent(linePosition, indent, reader.Depth - 1, true);
                                    lineWidth = indent;
                                    writeSpace = false;
                                    utilWriter.Flush();
                                    sbLengthLastNewLine = ((StringWriter)utilWriter.Writer).GetStringBuilder().Length;
                                }
                                else {
                                    writeSpace = true;
                                }

                                // Write the attribute
                                reader.MoveToNextAttribute();
                                utilWriter.Flush();
                                int startLength = ((StringWriter)utilWriter.Writer).GetStringBuilder().Length;
                                if (writeSpace) {
                                    utilWriter.AppendSpace();
                                }

                                utilWriter.Write(reader.Name);
                                utilWriter.Write('=');
                                utilWriter.AppendAttributeValue(reader);
                                utilWriter.Flush();
                                lineWidth += ((StringWriter)utilWriter.Writer).GetStringBuilder().Length - startLength;
                            }
                        }

                        // position reader back on element
                        reader.MoveToElement();

                        // write closing tag
                        if (reader.IsEmptyElement) {
                            utilWriter.Write(" />");
                        }
                        else {
                            utilWriter.Write('>');
                        }

                        break;

                    case XmlNodeType.EndElement:
                        utilWriter.Write("</");
                        utilWriter.Write(reader.Name);
                        utilWriter.Write('>');
                        break;

                    case XmlNodeType.EntityReference:
                        utilWriter.AppendEntityRef(reader.Name);
                        break;

                    // Ignore <?xml and <!DOCTYPE nodes
                    case XmlNodeType.XmlDeclaration:
                    case XmlNodeType.DocumentType:
                    default:
                        break;
                }

                // put each new element on a new line
                newLine = true;

                // do not skip any more indents
                skipFirstIndent = false;
            }

            utilWriter.Flush();
            string s = ((StringWriter)utilWriter.Writer).ToString();
            return s;
        }
    }
}
