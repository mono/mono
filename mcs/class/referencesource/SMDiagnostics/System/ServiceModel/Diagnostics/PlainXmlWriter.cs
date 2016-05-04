//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Xml;
    using System.Runtime;
    using System.Diagnostics;

    /// <summary>
    /// Very basic performance-oriented XmlWriter implementation. No validation/encoding is made.
    /// Namespaces are not supported
    /// Minimal formatting support
    /// </summary>
    internal class PlainXmlWriter : XmlWriter
    {
        internal class MaxSizeExceededException : Exception
        {
        }

        TraceXPathNavigator navigator;
        bool writingAttribute = false;
        string currentAttributeName;
        string currentAttributePrefix;
        string currentAttributeNs;
        string currentAttributeText = string.Empty;

        public PlainXmlWriter()
            : this(-1) //no quota
        {
        }

        public PlainXmlWriter(int maxSize)
        {
            this.navigator = new TraceXPathNavigator(maxSize);
        }

        public TraceXPathNavigator Navigator
        {
            get
            {
                return this.navigator;
            }
        }

        public override void WriteStartDocument() { }
        public override void WriteStartDocument(bool standalone) { }
        public override void WriteDocType(string name, string pubid, string sysid, string subset) { }
        public override void WriteEndDocument() { }

        public override string LookupPrefix(string ns)
        {
            return this.navigator.LookupPrefix(ns);
        }

        public override WriteState WriteState
        {
            get { return this.navigator.WriteState; }
        }

        public override XmlSpace XmlSpace
        {
            get { return XmlSpace.Default; }
        }

        public override string XmlLang
        {
            get { return string.Empty; }
        }

        public override void WriteValue(object value)
        {
            this.navigator.AddText(value.ToString());
        }

        public override void WriteValue(string value)
        {
            this.navigator.AddText(value);
        }

        public override void WriteBase64(byte[] buffer, int offset, int count) { }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
#pragma warning disable 618
            Fx.Assert(!String.IsNullOrEmpty(localName), "");
#pragma warning restore 618
            if (String.IsNullOrEmpty(localName))
            {
                throw new ArgumentNullException("localName");
            }

            this.navigator.AddElement(prefix, localName, ns);
        }

        public override void WriteFullEndElement()
        {
            WriteEndElement();
        }

        public override void WriteEndElement()
        {
            this.navigator.CloseElement();
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
#pragma warning disable 618
            Fx.Assert(!this.writingAttribute, "");
#pragma warning restore 618
            if (this.writingAttribute)
            {
                throw new InvalidOperationException();
            }

            this.currentAttributeName = localName;
            this.currentAttributePrefix = prefix;
            this.currentAttributeNs = ns;
            this.currentAttributeText = string.Empty;
            this.writingAttribute = true;
        }

        public override void WriteEndAttribute()
        {
#pragma warning disable 618
            Fx.Assert(this.writingAttribute, "");
#pragma warning restore 618
            if (!this.writingAttribute)
            {
                throw new InvalidOperationException();
            }
            this.navigator.AddAttribute(this.currentAttributeName, this.currentAttributeText, this.currentAttributeNs, this.currentAttributePrefix);
            this.writingAttribute = false;
        }

        public override void WriteCData(string text)
        {
            this.WriteRaw("<![CDATA[" + text + "]]>");
        }

        public override void WriteComment(string text)
        {
            this.navigator.AddComment(text);
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.navigator.AddProcessingInstruction(name, text);
        }

        public override void WriteEntityRef(string name)
        {
        }

        public override void WriteCharEntity(char ch)
        {
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
        }

        public override void WriteWhitespace(string ws)
        {
        }

        public override void WriteString(string text)
        {
            if (this.writingAttribute)
            {
                currentAttributeText += text;
            }
            else
            {
                this.WriteValue(text);
            }
        }

        public override void WriteChars(Char[] buffer, int index, int count)
        {
            // Exceptions being thrown as per data found at http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrfsystemxmlxmlwriterclasswritecharstopic.asp
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if ((buffer.Length - index) < count)
            {
                throw new ArgumentException(TraceSR.GetString(TraceSR.WriteCharsInvalidContent));
            }
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteRaw(String data)
        {
            this.WriteString(data);
        }

        public override void WriteRaw(Char[] buffer, int index, int count)
        {
            this.WriteChars(buffer, index, count);
        }

        public override void Close()
        {
        }

        public override void Flush()
        {
        }
    }
}
