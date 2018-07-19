//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Text;

    sealed class XmlCanonicalWriter
    {
        XmlUTF8NodeWriter writer;
        MemoryStream elementStream;
        byte[] elementBuffer;
        XmlUTF8NodeWriter elementWriter;
        bool inStartElement;
        int depth;
        Scope[] scopes;
        int xmlnsAttributeCount;
        XmlnsAttribute[] xmlnsAttributes;
        int attributeCount;
        Attribute[] attributes;
        Attribute attribute;
        Element element;
        byte[] xmlnsBuffer;
        int xmlnsOffset;
        const int maxBytesPerChar = 3;
        int xmlnsStartOffset;
        bool includeComments;
        string[] inclusivePrefixes;
        const string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";

        static readonly bool[] isEscapedAttributeChar = new bool[]
        {
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, // All
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, // '"', '&'
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false  // '<'
        };
        static readonly bool[] isEscapedElementChar = new bool[]
        {
            true, true, true, true, true, true, true, true, true, false, false, true, true, true, true, true, // All but 0x09, 0x0A
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true,
            false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, // '&'
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false  // '<', '>'
        };

        public XmlCanonicalWriter()
        {
        }

        public void SetOutput(Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            if (stream == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");

            if (writer == null)
            {
                writer = new XmlUTF8NodeWriter(isEscapedAttributeChar, isEscapedElementChar);
            }
            writer.SetOutput(stream, false, null);

            if (elementStream == null)
            {
                elementStream = new MemoryStream();
            }

            if (elementWriter == null)
            {
                elementWriter = new XmlUTF8NodeWriter(isEscapedAttributeChar, isEscapedElementChar);
            }
            elementWriter.SetOutput(elementStream, false, null);

            if (xmlnsAttributes == null)
            {
                xmlnsAttributeCount = 0;
                xmlnsOffset = 0;
                WriteXmlnsAttribute("xml", "http://www.w3.org/XML/1998/namespace");
                WriteXmlnsAttribute("xmlns", xmlnsNamespace);
                WriteXmlnsAttribute(string.Empty, string.Empty);
                xmlnsStartOffset = xmlnsOffset;
                for (int i = 0; i < 3; i++)
                {
                    xmlnsAttributes[i].referred = true;
                }
            }
            else
            {
                xmlnsAttributeCount = 3;
                xmlnsOffset = xmlnsStartOffset;
            }

            depth = 0;
            inStartElement = false;
            this.includeComments = includeComments;
            this.inclusivePrefixes = null;
            if (inclusivePrefixes != null)
            {
                this.inclusivePrefixes = new string[inclusivePrefixes.Length];
                for (int i = 0; i < inclusivePrefixes.Length; ++i)
                {
                    if (inclusivePrefixes[i] == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.InvalidInclusivePrefixListCollection));
                    }
                    this.inclusivePrefixes[i] = inclusivePrefixes[i];
                }
            }
        }

        public void Flush()
        {
            ThrowIfClosed();
            writer.Flush();
        }

        public void Close()
        {
            if (writer != null)
                writer.Close();
            if (elementWriter != null)
                elementWriter.Close();
            if (elementStream != null && elementStream.Length > 512)
                elementStream = null;
            elementBuffer = null;
            if (scopes != null && scopes.Length > 16)
                scopes = null;
            if (attributes != null && attributes.Length > 16)
                attributes = null;
            if (xmlnsBuffer != null && xmlnsBuffer.Length > 1024)
            {
                xmlnsAttributes = null;
                xmlnsBuffer = null;
            }
            inclusivePrefixes = null;
        }

        public void WriteDeclaration()
        {
        }

        public void WriteComment(string value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            ThrowIfClosed();
            if (includeComments)
            {
                writer.WriteComment(value);
            }
        }


        void StartElement()
        {
            if (scopes == null)
            {
                scopes = new Scope[4];
            }
            else if (depth == scopes.Length)
            {
                Scope[] newScopes = new Scope[depth * 2];
                Array.Copy(scopes, newScopes, depth);
                scopes = newScopes;
            }
            scopes[depth].xmlnsAttributeCount = xmlnsAttributeCount;
            scopes[depth].xmlnsOffset = xmlnsOffset;
            depth++;
            inStartElement = true;
            attributeCount = 0;
            elementStream.Position = 0;
        }

        void EndElement()
        {
            depth--;
            xmlnsAttributeCount = scopes[depth].xmlnsAttributeCount;
            xmlnsOffset = scopes[depth].xmlnsOffset;
        }

        public void WriteStartElement(string prefix, string localName)
        {
            if (prefix == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            ThrowIfClosed();
            bool isRootElement = (this.depth == 0);

            StartElement();
            element.prefixOffset = elementWriter.Position + 1;
            element.prefixLength = Encoding.UTF8.GetByteCount(prefix);
            element.localNameOffset = element.prefixOffset + element.prefixLength + (element.prefixLength != 0 ? 1 : 0);
            element.localNameLength = Encoding.UTF8.GetByteCount(localName);
            elementWriter.WriteStartElement(prefix, localName);

            // If we have a inclusivenamespace prefix list and the namespace declaration is in the 
            // outer context, then Add it to the root element.
            if (isRootElement && (this.inclusivePrefixes != null))
            {
                // Scan through all the namespace declarations in the outer scope.
                for (int i = 0; i < this.scopes[0].xmlnsAttributeCount; ++i)
                {
                    if (IsInclusivePrefix(ref xmlnsAttributes[i]))
                    {
                        XmlnsAttribute attribute = xmlnsAttributes[i];
                        AddXmlnsAttribute(ref attribute);
                    }
                }
            }
        }

        public void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            if (prefixBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            if (prefixOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixOffset > prefixBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.OffsetExceedsBufferSize, prefixBuffer.Length)));
            if (prefixLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixLength > prefixBuffer.Length - prefixOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, prefixBuffer.Length - prefixOffset)));

            if (localNameBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localNameBuffer"));
            if (localNameOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (localNameOffset > localNameBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", SR.GetString(SR.OffsetExceedsBufferSize, localNameBuffer.Length)));
            if (localNameLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (localNameLength > localNameBuffer.Length - localNameOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, localNameBuffer.Length - localNameOffset)));
            ThrowIfClosed();
            bool isRootElement = (this.depth == 0);

            StartElement();
            element.prefixOffset = elementWriter.Position + 1;
            element.prefixLength = prefixLength;
            element.localNameOffset = element.prefixOffset + prefixLength + (prefixLength != 0 ? 1 : 0);
            element.localNameLength = localNameLength;
            elementWriter.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);

            // If we have a inclusivenamespace prefix list and the namespace declaration is in the 
            // outer context, then Add it to the root element.
            if (isRootElement && (this.inclusivePrefixes != null))
            {
                // Scan through all the namespace declarations in the outer scope.
                for (int i = 0; i < this.scopes[0].xmlnsAttributeCount; ++i)
                {
                    if (IsInclusivePrefix(ref xmlnsAttributes[i]))
                    {
                        XmlnsAttribute attribute = xmlnsAttributes[i];
                        AddXmlnsAttribute(ref attribute);
                    }
                }
            }

        }

        bool IsInclusivePrefix(ref XmlnsAttribute xmlnsAttribute)
        {
            for (int i = 0; i < this.inclusivePrefixes.Length; ++i)
            {
                if (this.inclusivePrefixes[i].Length == xmlnsAttribute.prefixLength)
                {
                    if (String.Compare(Encoding.UTF8.GetString(xmlnsBuffer, xmlnsAttribute.prefixOffset, xmlnsAttribute.prefixLength), this.inclusivePrefixes[i], StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void WriteEndStartElement(bool isEmpty)
        {
            ThrowIfClosed();
            elementWriter.Flush();
            elementBuffer = elementStream.GetBuffer();
            inStartElement = false;
            ResolvePrefixes();
            writer.WriteStartElement(elementBuffer, element.prefixOffset, element.prefixLength, elementBuffer, element.localNameOffset, element.localNameLength);
            for (int i = scopes[depth - 1].xmlnsAttributeCount; i < xmlnsAttributeCount; i++)
            {
                // Check if this prefix with the same namespace has already been rendered.
                int j = i - 1;
                bool alreadyReferred = false;
                while (j >= 0)
                {
                    if (Equals(xmlnsBuffer, xmlnsAttributes[i].prefixOffset, xmlnsAttributes[i].prefixLength, xmlnsBuffer, xmlnsAttributes[j].prefixOffset, xmlnsAttributes[j].prefixLength))
                    {
                        // Check if the namespace is also equal.
                        if (Equals(xmlnsBuffer, xmlnsAttributes[i].nsOffset, xmlnsAttributes[i].nsLength, xmlnsBuffer, xmlnsAttributes[j].nsOffset, xmlnsAttributes[j].nsLength))
                        {
                            // We have found the prefix with the same namespace occur before. See if this has been
                            // referred.
                            if (xmlnsAttributes[j].referred)
                            {
                                // This has been referred previously. So we don't have 
                                // to output the namespace again.
                                alreadyReferred = true;
                                break;
                            }
                        }
                        else
                        {
                            // The prefix is the same, but the namespace value has changed. So we have to 
                            // output this namespace.
                            break;
                        }
                    }
                    --j;
                }

                if (!alreadyReferred)
                {
                    WriteXmlnsAttribute(ref xmlnsAttributes[i]);
                }
            }
            if (attributeCount > 0)
            {
                if (attributeCount > 1)
                {
                    SortAttributes();
                }

                for (int i = 0; i < attributeCount; i++)
                {
                    writer.WriteText(elementBuffer, attributes[i].offset, attributes[i].length);
                }
            }
            writer.WriteEndStartElement(false);
            if (isEmpty)
            {
                writer.WriteEndElement(elementBuffer, element.prefixOffset, element.prefixLength, elementBuffer, element.localNameOffset, element.localNameLength);
                EndElement();
            }
            elementBuffer = null;
        }

        public void WriteEndElement(string prefix, string localName)
        {
            if (prefix == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            ThrowIfClosed();
            writer.WriteEndElement(prefix, localName);
            EndElement();
        }

        void EnsureXmlnsBuffer(int byteCount)
        {
            if (xmlnsBuffer == null)
            {
                xmlnsBuffer = new byte[Math.Max(byteCount, 128)];
            }
            else if (xmlnsOffset + byteCount > xmlnsBuffer.Length)
            {
                byte[] newBuffer = new byte[Math.Max(xmlnsOffset + byteCount, xmlnsBuffer.Length * 2)];
                Buffer.BlockCopy(xmlnsBuffer, 0, newBuffer, 0, xmlnsOffset);
                xmlnsBuffer = newBuffer;
            }
        }

        public void WriteXmlnsAttribute(string prefix, string ns)
        {
            if (prefix == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            if (ns == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            ThrowIfClosed();
            if (prefix.Length > int.MaxValue - ns.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("ns", SR.GetString(SR.CombinedPrefixNSLength, int.MaxValue / maxBytesPerChar)));
            int totalLength = prefix.Length + ns.Length;
            if (totalLength > int.MaxValue / maxBytesPerChar)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("ns", SR.GetString(SR.CombinedPrefixNSLength, int.MaxValue / maxBytesPerChar)));
            EnsureXmlnsBuffer(totalLength * maxBytesPerChar);
            XmlnsAttribute xmlnsAttribute;
            xmlnsAttribute.prefixOffset = xmlnsOffset;
            xmlnsAttribute.prefixLength = Encoding.UTF8.GetBytes(prefix, 0, prefix.Length, xmlnsBuffer, xmlnsOffset);
            xmlnsOffset += xmlnsAttribute.prefixLength;
            xmlnsAttribute.nsOffset = xmlnsOffset;
            xmlnsAttribute.nsLength = Encoding.UTF8.GetBytes(ns, 0, ns.Length, xmlnsBuffer, xmlnsOffset);
            xmlnsOffset += xmlnsAttribute.nsLength;
            xmlnsAttribute.referred = false;
            AddXmlnsAttribute(ref xmlnsAttribute);
        }

        public void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            if (prefixBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            if (prefixOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixOffset > prefixBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.OffsetExceedsBufferSize, prefixBuffer.Length)));
            if (prefixLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixLength > prefixBuffer.Length - prefixOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, prefixBuffer.Length - prefixOffset)));

            if (nsBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("nsBuffer"));
            if (nsOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (nsOffset > nsBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsOffset", SR.GetString(SR.OffsetExceedsBufferSize, nsBuffer.Length)));
            if (nsLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (nsLength > nsBuffer.Length - nsOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, nsBuffer.Length - nsOffset)));
            ThrowIfClosed();
            if (prefixLength > int.MaxValue - nsLength)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("nsLength", SR.GetString(SR.CombinedPrefixNSLength, int.MaxValue)));
            EnsureXmlnsBuffer(prefixLength + nsLength);
            XmlnsAttribute xmlnsAttribute;
            xmlnsAttribute.prefixOffset = xmlnsOffset;
            xmlnsAttribute.prefixLength = prefixLength;
            Buffer.BlockCopy(prefixBuffer, prefixOffset, xmlnsBuffer, xmlnsOffset, prefixLength);
            xmlnsOffset += prefixLength;
            xmlnsAttribute.nsOffset = xmlnsOffset;
            xmlnsAttribute.nsLength = nsLength;
            Buffer.BlockCopy(nsBuffer, nsOffset, xmlnsBuffer, xmlnsOffset, nsLength);
            xmlnsOffset += nsLength;
            xmlnsAttribute.referred = false;
            AddXmlnsAttribute(ref xmlnsAttribute);
        }

        public void WriteStartAttribute(string prefix, string localName)
        {
            if (prefix == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("prefix");
            if (localName == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("localName");
            ThrowIfClosed();
            attribute.offset = elementWriter.Position;
            attribute.length = 0;
            attribute.prefixOffset = attribute.offset + 1; // WriteStartAttribute emits a space
            attribute.prefixLength = Encoding.UTF8.GetByteCount(prefix);
            attribute.localNameOffset = attribute.prefixOffset + attribute.prefixLength + (attribute.prefixLength != 0 ? 1 : 0);
            attribute.localNameLength = Encoding.UTF8.GetByteCount(localName);
            attribute.nsOffset = 0;
            attribute.nsLength = 0;
            elementWriter.WriteStartAttribute(prefix, localName);
        }

        public void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            if (prefixBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("prefixBuffer"));
            if (prefixOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixOffset > prefixBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixOffset", SR.GetString(SR.OffsetExceedsBufferSize, prefixBuffer.Length)));
            if (prefixLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (prefixLength > prefixBuffer.Length - prefixOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefixLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, prefixBuffer.Length - prefixOffset)));

            if (localNameBuffer == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("localNameBuffer"));
            if (localNameOffset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (localNameOffset > localNameBuffer.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameOffset", SR.GetString(SR.OffsetExceedsBufferSize, localNameBuffer.Length)));
            if (localNameLength < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", SR.GetString(SR.ValueMustBeNonNegative)));
            if (localNameLength > localNameBuffer.Length - localNameOffset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("localNameLength", SR.GetString(SR.SizeExceedsRemainingBufferSpace, localNameBuffer.Length - localNameOffset)));
            ThrowIfClosed();
            attribute.offset = elementWriter.Position;
            attribute.length = 0;
            attribute.prefixOffset = attribute.offset + 1; // WriteStartAttribute emits a space
            attribute.prefixLength = prefixLength;
            attribute.localNameOffset = attribute.prefixOffset + prefixLength + (prefixLength != 0 ? 1 : 0);
            attribute.localNameLength = localNameLength;
            attribute.nsOffset = 0;
            attribute.nsLength = 0;
            elementWriter.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        public void WriteEndAttribute()
        {
            ThrowIfClosed();
            elementWriter.WriteEndAttribute();
            attribute.length = elementWriter.Position - attribute.offset;
            AddAttribute(ref attribute);
        }

        public void WriteCharEntity(int ch)
        {
            ThrowIfClosed();
            if (ch <= char.MaxValue)
            {
                char[] chars = new char[1] { (char)ch };
                WriteEscapedText(chars, 0, 1);
            }
            else
            {
                WriteText(ch);
            }
        }

        public void WriteEscapedText(string value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            ThrowIfClosed();
            // Skip all white spaces before the start of root element.
            if (this.depth > 0)
            {
                if (inStartElement)
                {
                    elementWriter.WriteEscapedText(value);
                }
                else
                {
                    writer.WriteEscapedText(value);
                }
            }
        }

        public void WriteEscapedText(byte[] chars, int offset, int count)
        {
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));
            ThrowIfClosed();
            // Skip all white spaces before the start of root element.
            if (this.depth > 0)
            {
                if (inStartElement)
                {
                    elementWriter.WriteEscapedText(chars, offset, count);
                }
                else
                {
                    writer.WriteEscapedText(chars, offset, count);
                }
            }
        }

        public void WriteEscapedText(char[] chars, int offset, int count)
        {
            ThrowIfClosed();
            // Skip all white spaces before the start of root element.
            if (this.depth > 0)
            {
                if (inStartElement)
                {
                    elementWriter.WriteEscapedText(chars, offset, count);
                }
                else
                {
                    writer.WriteEscapedText(chars, offset, count);
                }
            }
        }

#if OLDWRITER
        unsafe internal void WriteText(char* chars, int charCount)
        {
            ThrowIfClosed();
            if (inStartElement)
            {
                elementWriter.WriteText(chars, charCount);
            }
            else
            {
                writer.WriteText(chars, charCount);
            }
        }
        unsafe internal void WriteEscapedText(char* chars, int count)
        {
            ThrowIfClosed();
            // Skip all white spaces before the start of root element.
            if (this.depth > 0)
            {
                if (inStartElement)
                {
                    elementWriter.WriteEscapedText(chars, count);
                }
                else
                {
                    writer.WriteEscapedText(chars, count);
                }
            }
        }
#endif

        public void WriteText(int ch)
        {
            ThrowIfClosed();
            if (inStartElement)
            {
                elementWriter.WriteText(ch);
            }
            else
            {
                writer.WriteText(ch);
            }
        }

        public void WriteText(byte[] chars, int offset, int count)
        {
            ThrowIfClosed();
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));
            if (inStartElement)
            {
                elementWriter.WriteText(chars, offset, count);
            }
            else
            {
                writer.WriteText(chars, offset, count);
            }
        }

        public void WriteText(string value)
        {
            if (value == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
            if (value.Length > 0)
            {
                if (inStartElement)
                {
                    elementWriter.WriteText(value);
                }
                else
                {
                    writer.WriteText(value);
                }
            }
        }

        public void WriteText(char[] chars, int offset, int count)
        {
            ThrowIfClosed();
            if (chars == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("chars"));
            if (offset < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.ValueMustBeNonNegative)));
            if (offset > chars.Length)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("offset", SR.GetString(SR.OffsetExceedsBufferSize, chars.Length)));
            if (count < 0)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.ValueMustBeNonNegative)));
            if (count > chars.Length - offset)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("count", SR.GetString(SR.SizeExceedsRemainingBufferSpace, chars.Length - offset)));
            if (inStartElement)
            {
                elementWriter.WriteText(chars, offset, count);
            }
            else
            {
                writer.WriteText(chars, offset, count);
            }
        }

        void ThrowIfClosed()
        {
            if (writer == null)
                ThrowClosed();
        }

        void ThrowClosed()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().ToString()));
        }

        void WriteXmlnsAttribute(ref XmlnsAttribute xmlnsAttribute)
        {
            if (xmlnsAttribute.referred)
            {
                writer.WriteXmlnsAttribute(xmlnsBuffer, xmlnsAttribute.prefixOffset, xmlnsAttribute.prefixLength, xmlnsBuffer, xmlnsAttribute.nsOffset, xmlnsAttribute.nsLength);
            }
        }

        void SortAttributes()
        {
            if (attributeCount < 16)
            {
                for (int i = 0; i < attributeCount - 1; i++)
                {
                    int attributeMin = i;
                    for (int j = i + 1; j < attributeCount; j++)
                    {
                        if (Compare(ref attributes[j], ref attributes[attributeMin]) < 0)
                        {
                            attributeMin = j;
                        }
                    }

                    if (attributeMin != i)
                    {
                        Attribute temp = attributes[i];
                        attributes[i] = attributes[attributeMin];
                        attributes[attributeMin] = temp;
                    }
                }
            }
            else
            {
                new AttributeSorter(this).Sort();
            }
        }

        void AddAttribute(ref Attribute attribute)
        {
            if (attributes == null)
            {
                attributes = new Attribute[4];
            }
            else if (attributeCount == attributes.Length)
            {
                Attribute[] newAttributes = new Attribute[attributeCount * 2];
                Array.Copy(attributes, newAttributes, attributeCount);
                attributes = newAttributes;
            }

            attributes[attributeCount] = attribute;
            attributeCount++;
        }

        void AddXmlnsAttribute(ref XmlnsAttribute xmlnsAttribute)
        {
            //            Console.WriteLine("{0}={1}", Encoding.UTF8.GetString(xmlnsBuffer, xmlnsAttribute.prefixOffset, xmlnsAttribute.prefixLength), 
            //                                Encoding.UTF8.GetString(xmlnsBuffer, xmlnsAttribute.nsOffset, xmlnsAttribute.nsLength));

            if (xmlnsAttributes == null)
            {
                xmlnsAttributes = new XmlnsAttribute[4];
            }
            else if (xmlnsAttributes.Length == xmlnsAttributeCount)
            {
                XmlnsAttribute[] newXmlnsAttributes = new XmlnsAttribute[xmlnsAttributeCount * 2];
                Array.Copy(xmlnsAttributes, newXmlnsAttributes, xmlnsAttributeCount);
                xmlnsAttributes = newXmlnsAttributes;
            }

            // If the prefix is in the inclusive prefix list, then mark it as 
            // to be rendered. Depth 0 is outer context and those can be ignored
            // for now.
            if ((depth > 0) && (this.inclusivePrefixes != null))
            {
                if (IsInclusivePrefix(ref xmlnsAttribute))
                {
                    xmlnsAttribute.referred = true;
                }
            }

            if (depth == 0)
            {
                // XmlnsAttributes at depth 0 are the outer context.  They don't need to be sorted.
                xmlnsAttributes[xmlnsAttributeCount++] = xmlnsAttribute;
            }
            else
            {
                // Sort the xmlns xmlnsAttribute
                int xmlnsAttributeIndex = scopes[depth - 1].xmlnsAttributeCount;
                bool isNewPrefix = true;
                while (xmlnsAttributeIndex < xmlnsAttributeCount)
                {
                    int result = Compare(ref xmlnsAttribute, ref xmlnsAttributes[xmlnsAttributeIndex]);
                    if (result > 0)
                    {
                        xmlnsAttributeIndex++;
                    }
                    else if (result == 0)
                    {
                        // We already have the same prefix at this scope. So let's
                        // just replace the old one with the new.
                        xmlnsAttributes[xmlnsAttributeIndex] = xmlnsAttribute;
                        isNewPrefix = false;
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                if (isNewPrefix)
                {
                    Array.Copy(xmlnsAttributes, xmlnsAttributeIndex, xmlnsAttributes, xmlnsAttributeIndex + 1, xmlnsAttributeCount - xmlnsAttributeIndex);
                    xmlnsAttributes[xmlnsAttributeIndex] = xmlnsAttribute;
                    xmlnsAttributeCount++;
                }
            }
        }

        void ResolvePrefix(int prefixOffset, int prefixLength, out int nsOffset, out int nsLength)
        {
            int xmlnsAttributeMin = scopes[depth - 1].xmlnsAttributeCount;

            // Lookup the attribute; it has to be there.  The decls are in sorted order
            // so we could do a binary search.
            int j = xmlnsAttributeCount - 1;
            while (!Equals(elementBuffer, prefixOffset, prefixLength,
                           xmlnsBuffer, xmlnsAttributes[j].prefixOffset, xmlnsAttributes[j].prefixLength))
            {
                j--;
            }

            nsOffset = xmlnsAttributes[j].nsOffset;
            nsLength = xmlnsAttributes[j].nsLength;

            if (j < xmlnsAttributeMin)
            {
                // If the xmlns decl isn't at this scope, see if we need to copy it down
                if (!xmlnsAttributes[j].referred)
                {
                    XmlnsAttribute xmlnsAttribute = xmlnsAttributes[j];
                    xmlnsAttribute.referred = true;

                    // This inserts the xmlns attribute in sorted order, so j is no longer valid
                    AddXmlnsAttribute(ref xmlnsAttribute);
                }
            }
            else
            {
                // Found at this scope, indicate we need to emit it
                xmlnsAttributes[j].referred = true;
            }
        }

        void ResolvePrefix(ref Attribute attribute)
        {
            if (attribute.prefixLength != 0)
            {
                ResolvePrefix(attribute.prefixOffset, attribute.prefixLength, out attribute.nsOffset, out attribute.nsLength);
            }
            else
            {
                // These should've been set when we added the prefix
                Fx.Assert(attribute.nsOffset == 0 && attribute.nsLength == 0, "");
            }
        }

        void ResolvePrefixes()
        {
            int nsOffset;
            int nsLength;
            ResolvePrefix(element.prefixOffset, element.prefixLength, out nsOffset, out nsLength);

            for (int i = 0; i < attributeCount; i++)
            {
                ResolvePrefix(ref attributes[i]);
            }
        }

        int Compare(ref XmlnsAttribute xmlnsAttribute1, ref XmlnsAttribute xmlnsAttribute2)
        {
            return Compare(xmlnsBuffer,
                           xmlnsAttribute1.prefixOffset, xmlnsAttribute1.prefixLength,
                           xmlnsAttribute2.prefixOffset, xmlnsAttribute2.prefixLength);
        }

        int Compare(ref Attribute attribute1, ref Attribute attribute2)
        {
            int s = Compare(xmlnsBuffer,
                            attribute1.nsOffset, attribute1.nsLength,
                            attribute2.nsOffset, attribute2.nsLength);

            if (s == 0)
            {
                s = Compare(elementBuffer,
                            attribute1.localNameOffset, attribute1.localNameLength,
                            attribute2.localNameOffset, attribute2.localNameLength);
            }

            return s;
        }

        int Compare(byte[] buffer, int offset1, int length1, int offset2, int length2)
        {
            if (offset1 == offset2)
            {
                return length1 - length2;
            }

            return Compare(buffer, offset1, length1, buffer, offset2, length2);
        }

        int Compare(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            //            Console.WriteLine("Compare: \"{0}\", \"{1}\"", Encoding.UTF8.GetString(sourceBuffer, offset1, length1), Encoding.UTF8.GetString(sourceBuffer, offset2, length2));

            int length = Math.Min(length1, length2);

            int s = 0;
            for (int i = 0; i < length && s == 0; i++)
            {
                s = buffer1[offset1 + i] - buffer2[offset2 + i];
            }

            if (s == 0)
            {
                s = length1 - length2;
            }

            return s;
        }

        bool Equals(byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            //            Console.WriteLine("Equals: \"{0}\", \"{1}\"", Encoding.UTF8.GetString(buffer1, offset1, length1), Encoding.UTF8.GetString(buffer2, offset2, length2));

            if (length1 != length2)
                return false;

            for (int i = 0; i < length1; i++)
            {
                if (buffer1[offset1 + i] != buffer2[offset2 + i])
                {
                    return false;
                }
            }

            return true;
        }

        class AttributeSorter : IComparer
        {
            XmlCanonicalWriter writer;

            public AttributeSorter(XmlCanonicalWriter writer)
            {
                this.writer = writer;
            }

            public void Sort()
            {
                object[] indeces = new object[writer.attributeCount];

                for (int i = 0; i < indeces.Length; i++)
                {
                    indeces[i] = i;
                }

                Array.Sort(indeces, this);

                Attribute[] attributes = new Attribute[writer.attributes.Length];
                for (int i = 0; i < indeces.Length; i++)
                {
                    attributes[i] = writer.attributes[(int)indeces[i]];
                }

                writer.attributes = attributes;
            }

            public int Compare(object obj1, object obj2)
            {
                int attributeIndex1 = (int)obj1;
                int attributeIndex2 = (int)obj2;
                return writer.Compare(ref writer.attributes[attributeIndex1], ref writer.attributes[attributeIndex2]);
            }
        }

        struct Scope
        {
            public int xmlnsAttributeCount;
            public int xmlnsOffset;
        }

        struct Element
        {
            public int prefixOffset;
            public int prefixLength;
            public int localNameOffset;
            public int localNameLength;
        }

        struct Attribute
        {
            public int prefixOffset;
            public int prefixLength;
            public int localNameOffset;
            public int localNameLength;
            public int nsOffset;
            public int nsLength;
            public int offset;
            public int length;
        }

        struct XmlnsAttribute
        {
            public int prefixOffset;
            public int prefixLength;
            public int nsOffset;
            public int nsLength;
            public bool referred;
        }
    }
}
