//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.IdentityModel
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    sealed class CanonicalizationDriver
    {
        bool closeReadersAfterProcessing;
        XmlReader reader;
        string[] inclusivePrefixes;
        bool includeComments;

        public bool CloseReadersAfterProcessing
        {
            get { return this.closeReadersAfterProcessing; }
            set { this.closeReadersAfterProcessing = value; }
        }

        public bool IncludeComments
        {
            get { return this.includeComments; }
            set { this.includeComments = value; }
        }

        public string[] GetInclusivePrefixes()
        {
            return this.inclusivePrefixes;
        }

        public void Reset()
        {
            this.reader = null;
        }

        public void SetInclusivePrefixes(string[] inclusivePrefixes)
        {
            this.inclusivePrefixes = inclusivePrefixes;
        }

        public void SetInput(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.reader = XmlReader.Create(stream);
        }

        public void SetInput(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            this.reader = reader;
        }

        public byte[] GetBytes()
        {
            return GetMemoryStream().ToArray();
        }

        public MemoryStream GetMemoryStream()
        {
            MemoryStream stream = new MemoryStream();
            WriteTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public void WriteTo(HashAlgorithm hashAlgorithm)
        {
            WriteTo(new HashStream(hashAlgorithm));
        }

        public void WriteTo(Stream canonicalStream)
        {
            if (this.reader != null)
            {
                XmlDictionaryReader dicReader = this.reader as XmlDictionaryReader;
                if ((dicReader != null) && (dicReader.CanCanonicalize))
                {
                    dicReader.MoveToContent();
                    dicReader.StartCanonicalization(canonicalStream, this.includeComments, this.inclusivePrefixes);
                    dicReader.Skip();
                    dicReader.EndCanonicalization();
                }
                else
                {
                    XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(Stream.Null);
                    if (this.inclusivePrefixes != null)
                    {
                        // Add a dummy element at the top and populate the namespace 
                        // declaration of all the inclusive prefixes.
                        writer.WriteStartElement("a", reader.LookupNamespace(String.Empty));
                        for (int i = 0; i < this.inclusivePrefixes.Length; ++i)
                        {
                            string ns = reader.LookupNamespace(this.inclusivePrefixes[i]);
                            if (ns != null)
                            {
                                writer.WriteXmlnsAttribute(this.inclusivePrefixes[i], ns);
                            }
                        }
                    }
                    writer.StartCanonicalization(canonicalStream, this.includeComments, this.inclusivePrefixes);
                    if (reader is WrappedReader)
                    {
                        ((WrappedReader)reader).XmlTokens.GetWriter().WriteTo(writer, new DictionaryManager());
                    }
                    else
                    {

                        writer.WriteNode(reader, false);
                    }
                    writer.Flush();
                    writer.EndCanonicalization();

                    if (this.inclusivePrefixes != null)
                       writer.WriteEndElement();

                    writer.Close();
                }
                if (this.closeReadersAfterProcessing)
                {
                    this.reader.Close();
                }
                this.reader = null;
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.NoInputIsSetForCanonicalization)));
            }
        }
    }

}
