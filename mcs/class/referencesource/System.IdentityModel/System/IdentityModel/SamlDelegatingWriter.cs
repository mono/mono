//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    sealed class SamlDelegatingWriter : XmlDictionaryWriter
    {
        XmlDictionaryWriter innerWriter;
        Stream canonicalStream;
        ICanonicalWriterEndRootElementCallback callback;
        IXmlDictionary dictionary;
        int elementCount;
        MemoryStream startFragment;
        MemoryStream signatureFragment;
        MemoryStream endFragment;

        XmlDictionaryWriter effectiveWriter;
        MemoryStream writerStream;

        public SamlDelegatingWriter(XmlDictionaryWriter innerWriter, Stream canonicalStream, ICanonicalWriterEndRootElementCallback callback, IXmlDictionary dictionary)
        {
            if (innerWriter == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("innerWriter");

            if (canonicalStream == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("canonicalStream");

            if (callback == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callback");

            if (dictionary == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionary");

            this.innerWriter = innerWriter;
            this.canonicalStream = canonicalStream;
            this.callback = callback;
            this.dictionary = dictionary;
            this.elementCount = 0;

            this.startFragment = new MemoryStream();
            this.signatureFragment = new MemoryStream();
            this.endFragment = new MemoryStream();
            this.writerStream = new MemoryStream();

            this.effectiveWriter = XmlDictionaryWriter.CreateBinaryWriter(this.writerStream, this.dictionary);
            this.effectiveWriter.StartCanonicalization(this.canonicalStream, false, null);
            ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).StartFragment(this.startFragment, false);
        }

        private void OnEndOfRootElement()
        {
            this.elementCount--;
            if ((this.elementCount == 0) && (this.endFragment.Length == 0))
            {
                // We still have to compute the signature. Write end element as a different fragment
                // and end canonicalization. Call back SAML to compute the signature.
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).EndFragment();

                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).StartFragment(this.endFragment, false);
                this.effectiveWriter.WriteEndElement();
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).EndFragment();

                this.effectiveWriter.EndCanonicalization();

                // Start the signature fragment.
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).StartFragment(this.signatureFragment, false);

                this.callback.OnEndOfRootElement(this);
            }
            else if (this.elementCount == 0)
            {
                // Signature fragment is complete. End this fragment and write all fragments into the 
                // inner writer.
                this.effectiveWriter.WriteEndElement();
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).EndFragment();

                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).WriteFragment(this.startFragment.GetBuffer(), 0, (int)this.startFragment.Length);
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).WriteFragment(this.signatureFragment.GetBuffer(), 0, (int)this.signatureFragment.Length);
                ((IFragmentCapableXmlDictionaryWriter)this.effectiveWriter).WriteFragment(this.endFragment.GetBuffer(), 0, (int)this.endFragment.Length);

                this.startFragment.Close();
                this.signatureFragment.Close();
                this.endFragment.Close();

                this.writerStream.Position = 0;

                XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(this.writerStream, this.dictionary, XmlDictionaryReaderQuotas.Max);
                reader.MoveToContent();
                this.innerWriter.WriteNode(reader, false);
                this.innerWriter.Flush();
                reader.Close();

                this.writerStream.Close();
                this.effectiveWriter.Close();
            }
            else
            {
                this.effectiveWriter.WriteEndElement();
            }
        }

        public override void Close()
        {
            this.effectiveWriter.Close();
        }

        public override void Flush()
        {
            this.effectiveWriter.Flush();
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, bool[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, double[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, decimal[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, float[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, int[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, long[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, short[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, DateTime[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, Guid[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, string localName, string namespaceUri, TimeSpan[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, bool[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, decimal[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, double[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, float[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, int[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, long[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, short[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, DateTime[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, Guid[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteArray(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri, TimeSpan[] array, int offset, int count)
        {
            this.effectiveWriter.WriteArray(prefix, localName, namespaceUri, array, offset, count);
        }

        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            this.effectiveWriter.WriteAttributes(reader, defattr);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.effectiveWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            this.effectiveWriter.WriteBinHex(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.effectiveWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.effectiveWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.effectiveWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.effectiveWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.effectiveWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.effectiveWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.effectiveWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            OnEndOfRootElement();
        }

        public override void WriteEntityRef(string name)
        {
            this.effectiveWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.effectiveWriter.WriteFullEndElement();
        }

        public override void WriteName(string name)
        {
            this.effectiveWriter.WriteName(name);
        }

        public override void WriteNmToken(string name)
        {
            this.effectiveWriter.WriteNmToken(name);
        }

        public override void WriteNode(XmlDictionaryReader reader, bool defattr)
        {
            this.effectiveWriter.WriteNode(reader, defattr);
        }

        public override void WriteNode(XmlReader reader, bool defattr)
        {
            this.effectiveWriter.WriteNode(reader, defattr);
        }

        public override void WriteNode(System.Xml.XPath.XPathNavigator navigator, bool defattr)
        {
            this.effectiveWriter.WriteNode(navigator, defattr);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.effectiveWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            this.effectiveWriter.WriteQualifiedName(localName, ns);
        }

        public override void WriteQualifiedName(XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.effectiveWriter.WriteQualifiedName(localName, namespaceUri);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.effectiveWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteRaw(string data)
        {
            this.effectiveWriter.WriteRaw(data);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.effectiveWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.effectiveWriter.WriteStartAttribute(prefix, localName, namespaceUri);
        }

        public override void WriteStartDocument()
        {
            this.effectiveWriter.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.effectiveWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.elementCount++;
            this.effectiveWriter.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName, XmlDictionaryString namespaceUri)
        {
            this.elementCount++;
            this.effectiveWriter.WriteStartElement(prefix, localName, namespaceUri);
        }

        public override WriteState WriteState
        {
            get { return this.effectiveWriter.WriteState; }
        }

        public override void WriteString(string text)
        {
            this.effectiveWriter.WriteString(text);
        }

        public override void WriteString(XmlDictionaryString value)
        {
            this.effectiveWriter.WriteString(value);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.effectiveWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteValue(bool value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(decimal value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(double value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(float value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(int value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(long value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(object value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(string value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(DateTime value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(Guid value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(TimeSpan value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(IStreamProvider value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(System.Xml.UniqueId value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteValue(XmlDictionaryString value)
        {
            this.effectiveWriter.WriteValue(value);
        }

        public override void WriteWhitespace(string ws)
        {
            this.effectiveWriter.WriteWhitespace(ws);
        }

        public override void WriteXmlAttribute(string localName, string value)
        {
            this.effectiveWriter.WriteXmlAttribute(localName, value);
        }

        public override void WriteXmlAttribute(XmlDictionaryString localName, XmlDictionaryString value)
        {
            this.effectiveWriter.WriteXmlAttribute(localName, value);
        }

        public override void WriteXmlnsAttribute(string prefix, string namespaceUri)
        {
            this.effectiveWriter.WriteXmlnsAttribute(prefix, namespaceUri);
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString namespaceUri)
        {
            this.effectiveWriter.WriteXmlnsAttribute(prefix, namespaceUri);
        }

        public override string LookupPrefix(string ns)
        {
            return this.effectiveWriter.LookupPrefix(ns);
        }
    }

    internal interface ICanonicalWriterEndRootElementCallback
    {
        void OnEndOfRootElement(XmlDictionaryWriter writer);
    }
}
