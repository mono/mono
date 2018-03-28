//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    class JavascriptXmlWriterWrapper : XmlDictionaryWriter
    {
        Encoding encoding;
        Stream stream;
        XmlDictionaryWriter xmlJsonWriter;
        byte[] encodedClosingFunctionCall;

        public JavascriptXmlWriterWrapper(Encoding encoding)
        {
            this.encoding = encoding;
            this.encodedClosingFunctionCall = this.encoding.GetBytes(");");
        }

        public JavascriptCallbackResponseMessageProperty JavascriptResponseMessageProperty
        {
            get;
            set;
        }

        public XmlDictionaryWriter XmlJsonWriter
        {
            get { return this.xmlJsonWriter; }
        }

        public override void Close()
        {
            this.xmlJsonWriter.Close();
        }

        public override void Flush()
        {
            this.xmlJsonWriter.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return this.xmlJsonWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.xmlJsonWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.xmlJsonWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.xmlJsonWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.xmlJsonWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.xmlJsonWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.xmlJsonWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.xmlJsonWriter.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.xmlJsonWriter.WriteEndDocument();

            if (this.JavascriptResponseMessageProperty != null &&
                !String.IsNullOrEmpty(this.JavascriptResponseMessageProperty.CallbackFunctionName))
            {
                this.xmlJsonWriter.Flush();
                if (this.JavascriptResponseMessageProperty.StatusCode != null && (int)this.JavascriptResponseMessageProperty.StatusCode != 200)
                {
                    byte[] buffer = this.encoding.GetBytes(String.Format(CultureInfo.InvariantCulture, ",{0}", (int)this.JavascriptResponseMessageProperty.StatusCode));
                    this.stream.Write(buffer, 0, buffer.Length);
                }
                this.stream.Write(this.encodedClosingFunctionCall, 0, this.encodedClosingFunctionCall.Length);
            }
        }

        public override void WriteEndElement()
        {
            this.xmlJsonWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this.xmlJsonWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.xmlJsonWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.xmlJsonWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this.xmlJsonWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.xmlJsonWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.xmlJsonWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone)
        {
            StartJsonMessage();
            this.xmlJsonWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument()
        {
            StartJsonMessage();
            this.xmlJsonWriter.WriteStartDocument();
        }

        void StartJsonMessage()
        {
            if (this.JavascriptResponseMessageProperty != null &&
                !String.IsNullOrEmpty(this.JavascriptResponseMessageProperty.CallbackFunctionName))
            {
                byte[] buffer = this.encoding.GetBytes(String.Format(CultureInfo.InvariantCulture, "{0}(", this.JavascriptResponseMessageProperty.CallbackFunctionName));
                this.stream.Write(buffer, 0, buffer.Length);
            }
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.xmlJsonWriter.WriteStartElement(prefix, localName, ns);
        }

        public override WriteState WriteState
        {
            get { return this.xmlJsonWriter.WriteState; }
        }

        public override void WriteString(string text)
        {
            this.xmlJsonWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.xmlJsonWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.xmlJsonWriter.WriteWhitespace(ws);
        }

        public void SetOutput(Stream stream, XmlDictionaryWriter writer)
        {
            this.stream = stream;
            this.xmlJsonWriter = writer;
        }
    }
}
