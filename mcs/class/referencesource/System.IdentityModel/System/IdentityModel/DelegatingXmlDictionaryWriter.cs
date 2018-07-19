//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Class wraps a given writer and delegates all XmlDictionaryWriter calls 
    /// to the inner wrapped writer.
    /// </summary>
    public class DelegatingXmlDictionaryWriter : XmlDictionaryWriter
    {
        XmlDictionaryWriter _innerWriter;

        // this writer is used to echo un-canonicalized bytes
        XmlWriter _tracingWriter;

        /// <summary>
        /// Initializes a new instance of <see cref="DelegatingXmlDictionaryWriter"/>
        /// </summary>
        protected DelegatingXmlDictionaryWriter()
        {
        }

        /// <summary>
        /// Initializes the inner writer that this instance wraps.
        /// </summary>
        /// <param name="innerWriter">XmlDictionaryWriter to wrap.</param>
        protected void InitializeInnerWriter(XmlDictionaryWriter innerWriter)
        {
            if (innerWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerWriter");
            }

            _innerWriter = innerWriter;
        }

        /// <summary>
        /// Initializes a writer that will write the un-canonicalize xml.
        /// If this agrument is not null, all calls will be echoed to this writer.
        /// </summary>
        /// <param name="tracingWriter">XmlTextWriter to echo .</param>
        protected void InitializeTracingWriter(XmlWriter tracingWriter)
        {
            _tracingWriter = tracingWriter;
        }

        /// <summary>
        /// Gets the wrapped writer.
        /// </summary>
        protected XmlDictionaryWriter InnerWriter
        {
            get
            {
                return _innerWriter;
            }
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        public override void Close()
        {
            _innerWriter.Close();
            if (_tracingWriter != null)
            {
                _tracingWriter.Close();
            }
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _innerWriter.Flush();
            if (_tracingWriter != null)
            {
                _tracingWriter.Flush();
            }
        }

        /// <summary>
        /// Encodes the specified binary bytes as Base64 and writes out the resulting text.
        /// </summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="index">The position in the buffer indicating the start of the bytes to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            _innerWriter.WriteBase64(buffer, index, count);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteBase64(buffer, index, count);
            }
        }

        /// <summary>
        /// Writes out a CDATA block containing the specified text.
        /// </summary>
        /// <param name="text">The text to place inside the CDATA block.</param>
        public override void WriteCData(string text)
        {
            _innerWriter.WriteCData(text);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteCData(text);
            }
        }

        /// <summary>
        /// Forces the generation of a character entity for the specified Unicode character value.
        /// </summary>
        /// <param name="ch">The Unicode character for which to generate a character entity.</param>
        public override void WriteCharEntity(char ch)
        {
            _innerWriter.WriteCharEntity(ch);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteCharEntity(ch);
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes text one buffer at a time.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position in the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteChars(char[] buffer, int index, int count)
        {
            _innerWriter.WriteChars(buffer, index, count);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteChars(buffer, index, count);
            }
        }

        /// <summary>
        /// Writes out a comment containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            _innerWriter.WriteComment(text);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteComment(text);
            }
        }

        /// <summary>
        /// Writes the DOCTYPE declaration with the specified name and optional attributes.
        /// </summary>
        /// <param name="name">The name of the DOCTYPE. This must be non-empty.</param>
        /// <param name="pubid">If non-null it also writes PUBLIC "pubid" "sysid" where pubid and sysid are
        /// replaced with the value of the given arguments.</param>
        /// <param name="sysid">If pubid is null and sysid is non-null it writes SYSTEM "sysid" where sysid
        /// is replaced with the value of this argument.</param>
        /// <param name="subset">If non-null it writes [subset] where subset is replaced with the value of
        /// this argument.</param>
        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            _innerWriter.WriteDocType(name, pubid, sysid, subset);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteDocType(name, pubid, sysid, subset);
            }
        }

        /// <summary>
        /// Closes the previous System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String) call.
        /// </summary>
        public override void WriteEndAttribute()
        {
            _innerWriter.WriteEndAttribute();
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteEndAttribute();
            }
        }

        /// <summary>
        /// Closes any open elements or attributes and puts the writer back in the Start state.
        /// </summary>
        public override void WriteEndDocument()
        {
            _innerWriter.WriteEndDocument();
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteEndDocument();
            }
        }

        /// <summary>
        /// Closes one element and pops the corresponding namespace scope.
        /// </summary>
        public override void WriteEndElement()
        {
            _innerWriter.WriteEndElement();
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes out an entity reference as name.
        /// </summary>
        /// <param name="name">The name of the entity reference.</param>
        public override void WriteEntityRef(string name)
        {
            _innerWriter.WriteEntityRef(name);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteEntityRef(name);
            }
        }

        /// <summary>
        /// Closes one element and pops the corresponding namespace scope.
        /// </summary>
        public override void WriteFullEndElement()
        {
            _innerWriter.WriteFullEndElement();
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteFullEndElement();
            }
        }

        /// <summary>
        /// Writes out a processing instruction with a space between the name and text as follows: &lt;?name text?>.
        /// </summary>
        /// <param name="name">The name of the processing instruction.</param>
        /// <param name="text">The text to include in the processing instruction.</param>
        public override void WriteProcessingInstruction(string name, string text)
        {
            _innerWriter.WriteProcessingInstruction(name, text);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteProcessingInstruction(name, text);
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes raw markup manually from a character buffer.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position within the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteRaw(char[] buffer, int index, int count)
        {
            _innerWriter.WriteRaw(buffer, index, count);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteRaw(buffer, index, count);
            }
        }

        /// <summary>
        /// Writes raw markup manually from a string.
        /// </summary>
        /// <param name="data">String containing the text to write.</param>
        public override void WriteRaw(string data)
        {
            _innerWriter.WriteRaw(data);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteRaw(data);
            }
        }

        /// <summary>
        /// Writes the start of an attribute with the specified local name and namespace URI.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the attribute.</param>
        /// <param name="localName">The local name of the attribute.</param>
        /// <param name="ns">The namespace URI for the attribute.</param>
        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            _innerWriter.WriteStartAttribute(prefix, localName, ns);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteStartAttribute(prefix, localName, ns);
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes the XML declaration with the version "1.0".
        /// </summary>
        public override void WriteStartDocument()
        {
            _innerWriter.WriteStartDocument();
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteStartDocument();
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes the XML declaration with the version
        /// "1.0" and the standalone attribute.
        /// </summary>
        /// <param name="standalone">If true, it writes "standalone=yes"; if false, it writes "standalone=no".</param>
        public override void WriteStartDocument(bool standalone)
        {
            _innerWriter.WriteStartDocument(standalone);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteStartDocument(standalone);
            }
        }

        /// <summary>
        /// When overridden in a derived class, writes the specified start tag and associates
        /// it with the given namespace and prefix.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the element.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="ns">The namespace URI to associate with the element.</param>
        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            _innerWriter.WriteStartElement(prefix, localName, ns);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteStartElement(prefix, localName, ns);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the state of the writer.
        /// </summary>
        public override WriteState WriteState
        {
            get { return _innerWriter.WriteState; }
        }

        /// <summary>
        /// Writes the given text content.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public override void WriteString(string text)
        {
            _innerWriter.WriteString(text);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteString(text);
            }
        }

        /// <summary>
        /// Generates and writes the surrogate character entity for the surrogate character pair.
        /// </summary>
        /// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF.</param>
        /// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF.</param>
        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            _innerWriter.WriteSurrogateCharEntity(lowChar, highChar);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteSurrogateCharEntity(lowChar, highChar);
            }
        }

        /// <summary>
        /// Writes out the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public override void WriteWhitespace(string ws)
        {
            _innerWriter.WriteWhitespace(ws);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteWhitespace(ws);
            }
        }

        /// <summary>
        /// Writes an attribute as a xml attribute with the prefix 'xml:'.
        /// </summary>
        /// <param name="localName">Localname of the attribute.</param>
        /// <param name="value">Attribute value.</param>
        public override void WriteXmlAttribute(string localName, string value)
        {
            _innerWriter.WriteXmlAttribute(localName, value);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteAttributeString(localName, value);
            }
        }

        /// <summary>
        /// Writes an xmlns namespace declaration. 
        /// </summary>
        /// <param name="prefix">The prefix of the namespace declaration.</param>
        /// <param name="namespaceUri">The namespace Uri itself.</param>
        public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            _innerWriter.WriteXmlnsAttribute(prefix, namespaceUri);
            if (_tracingWriter != null)
            {
                _tracingWriter.WriteAttributeString(prefix, String.Empty, namespaceUri, String.Empty);
            }
        }

        /// <summary>
        /// Returns the closest prefix defined in the current namespace scope for the namespace URI.
        /// </summary>
        /// <param name="ns">The namespace URI whose prefix you want to find.</param>
        /// <returns>The matching prefix or null if no matching namespace URI is found in the
        /// current scope.</returns>
        public override string LookupPrefix(string ns)
        {
            return _innerWriter.LookupPrefix(ns);
        }

        /// <summary>
        /// Returns a value indicating if the reader is capable of Canonicalization.
        /// </summary>
        public override bool CanCanonicalize
        {
            get
            {
                return _innerWriter.CanCanonicalize;
            }
        }

        /// <summary>
        /// Indicates the start of Canonicalization. Any write operatation following this will canonicalize the data 
        /// and will wirte it to the given stream.
        /// </summary>
        /// <param name="stream">Stream to which the canonical stream should be written.</param>
        /// <param name="includeComments">The value indicates if comments written should be canonicalized as well.</param>
        /// <param name="inclusivePrefixes">Set of prefixes that needs to be included into the canonical stream. The prefixes are defined at 
        /// the first element that is written to the canonical stream.</param>
        public override void StartCanonicalization(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            _innerWriter.StartCanonicalization(stream, includeComments, inclusivePrefixes);
        }

        /// <summary>
        /// Closes a previous Start canonicalization operation. The stream given to the StartCanonicalization is flushed 
        /// and any data written after this call will not be written to the canonical stream.
        /// </summary>
        public override void EndCanonicalization()
        {
            _innerWriter.EndCanonicalization();
        }
    }
}
