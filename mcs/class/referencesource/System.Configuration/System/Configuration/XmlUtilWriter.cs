//------------------------------------------------------------------------------
// <copyright file="XmlUtilWriter.cs" company="Microsoft">
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
    // A utility class for writing XML to a TextWriter.
    //
    // When this class is used to copy an XML document that may include a "<!DOCTYPE" directive,
    // we must track what is written until the "<!DOCTYPE" or first document element is found.
    // This is needed because the XML reader does not give us accurate spacing information
    // for the beginning of the "<!DOCTYPE" element. 
    //
    // Note that tracking this information is expensive, as it requires a scan of everything that is written
    // until "<!DOCTYPE" or the first element is found.
    //
    // Note also that this class is used at runtime to copy sections, so performance of all
    // writing functions directly affects application startup time.
    //
    internal class XmlUtilWriter {
        private const char    SPACE = ' ';
        private const string  NL    = "\r\n";

        private static string SPACES_8;
        private static string SPACES_4;
        private static string SPACES_2;

        private TextWriter              _writer;                // the wrapped text writer
        private Stream                  _baseStream;            // stream under TextWriter when tracking position
        private bool                    _trackPosition;         // should write position be tracked?
        private int                     _lineNumber;            // line number
        private int                     _linePosition;          // line position
        private bool                    _isLastLineBlank;       // is the last line blank?
        private object                  _lineStartCheckpoint;   // checkpoint taken at the start of each line

        static XmlUtilWriter() {
            SPACES_8 = new String(SPACE, 8);
            SPACES_4 = new String(SPACE, 4);
            SPACES_2 = new String(SPACE, 2);
        }

        internal XmlUtilWriter(TextWriter writer, bool trackPosition) {
            _writer = writer;
            _trackPosition = trackPosition;
            _lineNumber = 1;
            _linePosition = 1;
            _isLastLineBlank = true;

            if (_trackPosition) {
                _baseStream = ((StreamWriter)_writer).BaseStream;
                _lineStartCheckpoint = CreateStreamCheckpoint();
            }
        }
        
        internal TextWriter Writer {
            get {
                return _writer;
            }
        }

        internal bool TrackPosition {
            get {
                return _trackPosition;
            }
        }

        internal int LineNumber { 
            get { 
                return _lineNumber; 
            }
        }

        internal int LinePosition {
            get {
                return _linePosition;
            }
        }

        internal bool IsLastLineBlank {
            get {
                return _isLastLineBlank;
            }
        }

        //
        // Update the position after the character is written to the stream.
        //
        private void UpdatePosition(char ch) {
            switch (ch) {
                case '\r':
                    _lineNumber++;
                    _linePosition = 1;
                    _isLastLineBlank = true;
                    break;

                case '\n':
                    _lineStartCheckpoint = CreateStreamCheckpoint();
                    break;

                case SPACE:
                case '\t':
                    _linePosition++;
                    break;

                default:
                    _linePosition++;
                    _isLastLineBlank = false;
                    break;
            }
        }

        //
        // Write a string to _writer.
        // If we are tracking position, determine the line number and position
        //
        internal int Write(string s) {
            if (_trackPosition) {
                for (int i = 0; i < s.Length; i++) {
                    char ch = s[i];
                    _writer.Write(ch);
                    UpdatePosition(ch);
                }
            }
            else {
                _writer.Write(s);
            }

#if DEBUG_WRITE
            Flush();
#endif

            return s.Length;
        }

        //
        // Write a character to _writer.
        // If we are tracking position, determine the line number and position
        //
        internal int Write(char ch) {
            _writer.Write(ch);
            if (_trackPosition) {
                UpdatePosition(ch);
            }
#if DEBUG_WRITE
            Flush();
#endif

            return 1;
        }

        internal void Flush() {
            _writer.Flush();
        }

        // Escape a text string
        internal int AppendEscapeTextString(string s) {
            return AppendEscapeXmlString(s, false, 'A');
        }

        // Escape a XML string to preserve XML markup.
        internal int AppendEscapeXmlString(string s, bool inAttribute, char quoteChar) {
            int charactersWritten = 0;
            for (int i = 0; i < s.Length; i++) {
                char ch = s[i];

                bool appendCharEntity = false;
                string entityRef = null;
                if ((ch < 32 && ch != '\t' && ch != '\r' && ch != '\n') || (ch > 0xFFFD)) {
                    appendCharEntity = true;
                }
                else {
                    switch (ch)
                    {
                        case '<':
                            entityRef = "lt";
                            break;

                        case '>':
                            entityRef = "gt";
                            break;

                        case '&':
                            entityRef = "amp";
                            break;

                        case '\'':
                            if (inAttribute && quoteChar == ch) {
                                entityRef = "apos";
                            }
                            break;

                        case '"':
                            if (inAttribute && quoteChar == ch) {
                                entityRef = "quot";
                            }
                            break;

                        case '\n':
                        case '\r':
                            appendCharEntity = inAttribute;
                            break;

                        default:
                            break;
                    }
                }

                if (appendCharEntity) {
                    charactersWritten += AppendCharEntity(ch);
                }
                else if (entityRef != null) {
                    charactersWritten += AppendEntityRef(entityRef);
                }
                else {
                    charactersWritten += Write(ch);
                }
            }

            return charactersWritten;
        }

        internal int AppendEntityRef(string entityRef) {
            Write('&');
            Write(entityRef);
            Write(';');
            return entityRef.Length + 2;
        }

        internal int AppendCharEntity(char ch) {
            string numberToWrite = ((int)ch).ToString("X", CultureInfo.InvariantCulture);
            Write('&');
            Write('#');
            Write('x');
            Write(numberToWrite);
            Write(';');
            return numberToWrite.Length + 4;
        }

        internal int AppendCData(string cdata) {
            Write("<![CDATA[");
            Write(cdata);
            Write("]]>");
            return cdata.Length + 12;
        }

        internal int AppendProcessingInstruction(string name, string value) {
            Write("<?");
            Write(name);
            AppendSpace();
            Write(value);
            Write("?>");
            return name.Length + value.Length + 5;
        }

        internal int AppendComment(string comment) {
            Write("<!--");
            Write(comment);
            Write("-->");
            return comment.Length + 7;
        }

        internal int AppendAttributeValue(XmlTextReader reader) {
            int charactersWritten = 0;
            char quote = reader.QuoteChar;

            //
            // In !DOCTYPE, quote is '\0' for second public attribute. 
            // Protect ourselves from writing invalid XML by always
            // supplying a valid quote char.
            //
            if (quote != '"' && quote != '\'') {
                quote = '"';
            }

            charactersWritten += Write(quote);
            while (reader.ReadAttributeValue()) {
                if (reader.NodeType == XmlNodeType.Text) {
                    charactersWritten += AppendEscapeXmlString(reader.Value, true, quote);
                }
                else {
                    charactersWritten += AppendEntityRef(reader.Name);
                }
            }

            charactersWritten += Write(quote);
            return charactersWritten;
        }

        // Append whitespace, ensuring there is at least one space.
        internal int AppendRequiredWhiteSpace(int fromLineNumber, int fromLinePosition, int toLineNumber, int toLinePosition) {
            int charactersWritten = AppendWhiteSpace(fromLineNumber, fromLinePosition, toLineNumber, toLinePosition);
            if (charactersWritten == 0) {
                charactersWritten += AppendSpace();
            }

            return charactersWritten;
        }


        // Append whitespce
        internal int AppendWhiteSpace(int fromLineNumber, int fromLinePosition, int toLineNumber, int toLinePosition) {
            int charactersWritten = 0;
            while (fromLineNumber++ < toLineNumber) {
                charactersWritten += AppendNewLine();
                fromLinePosition = 1;
            }

            charactersWritten += AppendSpaces(toLinePosition - fromLinePosition);
            return charactersWritten;
        }

        //
        // Append indent
        //      linePosition - starting line position
        //      indent - number of spaces to indent each unit of depth
        //      depth - depth to indent
        //      newLine - insert new line before indent?
        //
        internal int AppendIndent(int linePosition, int indent, int depth, bool newLine) {
            int charactersWritten = 0;
            if (newLine) {
                charactersWritten += AppendNewLine();
            }

            int c = (linePosition - 1) + (indent * depth);
            charactersWritten += AppendSpaces(c);
            return charactersWritten;
        }

        //
        // AppendSpacesToLinePosition
        //
        // Write spaces up to the line position, taking into account the
        // current line position of the writer.
        //
        internal int AppendSpacesToLinePosition(int linePosition) {
            Debug.Assert(_trackPosition, "_trackPosition");

            if (linePosition <= 0) {
                return 0;
            }

            int delta = linePosition - _linePosition;
            if (delta < 0 && IsLastLineBlank) {
                SeekToLineStart();
            }

            return AppendSpaces(linePosition - _linePosition);
        }

        //
        // Append a new line
        //
        internal int AppendNewLine() {
            return Write(NL);
        }

        //
        // AppendSpaces
        //
        // Write spaces to the writer provided.  Since we do not want waste
        // memory by allocating do not use "new String(' ', count)".
        //
        internal int AppendSpaces(int count) {
            int c = count;
            while (c > 0) {
                if (c >= 8) {
                    Write(SPACES_8);
                    c -= 8;
                }
                else if (c >= 4) {
                    Write(SPACES_4);
                    c -= 4;
                }
                else if (c >= 2) {
                    Write(SPACES_2);
                    c -= 2;
                }
                else {
                    Write(SPACE);
                    break;
                }
            }

            return (count > 0) ? count : 0;
        }

        //
        // Append a single space character
        //
        internal int AppendSpace() {
            return Write(SPACE);
        }

        //
        // Reset the stream to the beginning of the current blank line.
        //
        internal void SeekToLineStart() {
            Debug.Assert(_isLastLineBlank, "_isLastLineBlank");
            RestoreStreamCheckpoint(_lineStartCheckpoint);
        }

        // Create a checkpoint that can be restored with RestoreStreamCheckpoint().
        internal object CreateStreamCheckpoint() {
            return new StreamWriterCheckpoint(this);
        }

        // Restore the writer state that was recorded with CreateStreamCheckpoint().
        internal void RestoreStreamCheckpoint(object o) {
            StreamWriterCheckpoint checkpoint = (StreamWriterCheckpoint) o;

            Flush();

            _lineNumber = checkpoint._lineNumber;
            _linePosition = checkpoint._linePosition;
            _isLastLineBlank = checkpoint._isLastLineBlank;

            _baseStream.Seek(checkpoint._streamPosition, SeekOrigin.Begin);
            _baseStream.SetLength(checkpoint._streamLength);
            _baseStream.Flush();
        }

        // Class that contains the state of the writer and its underlying stream.
        private class StreamWriterCheckpoint {
            internal int         _lineNumber;       // line number
            internal int         _linePosition;     // line position
            internal bool        _isLastLineBlank;  // is the last line blank?
            internal long        _streamLength;     // length of the stream
            internal long        _streamPosition;   // position of the stream pointer

            internal StreamWriterCheckpoint(XmlUtilWriter writer) {
                writer.Flush();
                _lineNumber = writer._lineNumber;
                _linePosition = writer._linePosition;
                _isLastLineBlank = writer._isLastLineBlank;

                writer._baseStream.Flush();
                _streamPosition = writer._baseStream.Position;
                _streamLength = writer._baseStream.Length;
            }
        }
    }
}
